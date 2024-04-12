using Azure;
using System;
using INTEX_II_413.Models;
using INTEX_II_413.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.ProjectModel;
using System.Diagnostics;
using System.Drawing.Printing;
using System.Formats.Tar;
using System.Collections.Generic;

//For Implementing the pipeline
using Microsoft.ML;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using INTEX_II_413.Pages;
using Microsoft.AspNetCore.Identity;

namespace INTEX_II_413.Controllers
{
    public class HomeController : Controller
    {
        private IIntexRepository _repo;

        private readonly UserManager<IdentityUser> _userManager;

        //This is the pipeline that will be used to make predictions
        private readonly InferenceSession _sessionFraud;

        public HomeController(IIntexRepository temp, UserManager<IdentityUser> userManager)
        {
            _repo = temp;

            _sessionFraud = new InferenceSession("Fraud_Identification_model_3.onnx");

            _userManager = userManager;
        }



        public IActionResult Index()
        {
            return View("Index");
        }


        [Authorize(Roles = "Admin,Customer")]
        [HttpPost]
        public IActionResult PlaceOrder(OrderSubmissionViewModel submissionModel)
        {
            var customer = _repo.Customers.FirstOrDefault(c => c.CustomerId == submissionModel.CustomerId);
            if (customer == null)
            {
                return NotFound();
            }

            int age = DateTime.Now.Year - customer.BirthDate.Year;
            if (DateTime.Now < customer.BirthDate.AddYears(age)) age--;

            FraudPredictionViewModel fraudPredictionData = new FraudPredictionViewModel
            {
                Time = submissionModel.Order.Time,
                Amount = submissionModel.Order.Amount,
                Age = age,
                CountryOfTransaction = submissionModel.Order.Country,
                ShippingAddress = submissionModel.Order.Address,
                Bank = submissionModel.Order.Bank,
                TypeOfCard = submissionModel.Order.CardType,
                CountryOfResidence = customer.Country,
                Gender = customer.Gender
            };

            bool isFraudulent = PredictFraud(fraudPredictionData);

            if (isFraudulent)
            {
                submissionModel.Order.FraudPredicted = true;
                FinalOrderSubmission(submissionModel);
                return RedirectToAction("FraudConfirmation");
            }
            else
            {
                submissionModel.Order.FraudPredicted = false;
                FinalOrderSubmission(submissionModel);
                return RedirectToAction("Confirmation");
            }
        
               
        }


        [Authorize(Roles = "Admin,Customer")]
        [HttpPost]
        public IActionResult FinalOrderSubmission(OrderSubmissionViewModel model)
        {
            model.Order.CustomerId = model.CustomerId;

            _repo.AddOrder(model.Order);
            _repo.SaveChanges();
            return RedirectToAction("Confirmation");
        }


        [HttpPost]
        public bool PredictFraud(FraudPredictionViewModel fraudPredictionData)
        {
            // Load the ONNX model
            InferenceSession session = new InferenceSession("C:\\Users\\Hammo\\source\\repos\\INTEX_II_413\\Fraud_Identification_model_3.onnx");

            // Extract necessary values from the fraudPredictionData
            int hourOfDay = fraudPredictionData.Time.Hour;
            int dayOfWeekNumeric = (int)fraudPredictionData.Time.DayOfWeek; // Assuming this maps directly to a numeric day of the week

            // Create a dictionary that matches the exact input names expected by the ONNX model
            var inputFeatures = new Dictionary<string, float>
    {
        {"float_input", hourOfDay},
        {"amount", (float)fraudPredictionData.Amount},
        {"age", fraudPredictionData.Age},
        {"year", fraudPredictionData.Time.Year},
        {"month", fraudPredictionData.Time.Month},
        {"day", fraudPredictionData.Time.Day},
        {"day_of_week_numeric", dayOfWeekNumeric},
        {"country_of_transaction_India", fraudPredictionData.CountryOfTransaction == "India" ? 1f : 0f},
        {"country_of_transaction_Russia", fraudPredictionData.CountryOfTransaction == "Russia" ? 1f : 0f},
        {"country_of_transaction_USA", fraudPredictionData.CountryOfTransaction == "USA" ? 1f : 0f},
        {"country_of_transaction_United Kingdom", fraudPredictionData.CountryOfTransaction == "United Kingdom" ? 1f : 0f},
        {"shipping_address_India", fraudPredictionData.ShippingAddress == "India" ? 1f : 0f},
        {"shipping_address_Russia", fraudPredictionData.ShippingAddress == "Russia" ? 1f : 0f},
        {"shipping_address_USA", fraudPredictionData.ShippingAddress == "USA" ? 1f : 0f},
        {"shipping_address_United Kingdom", fraudPredictionData.ShippingAddress == "United Kingdom" ? 1f : 0f},
        {"bank_HSBC", fraudPredictionData.Bank == "HSBC" ? 1f : 0f},
        {"bank_Halifax", fraudPredictionData.Bank == "Halifax" ? 1f : 0f},
        {"bank_Lloyds", fraudPredictionData.Bank == "Lloyds" ? 1f : 0f},
        {"bank_Metro", fraudPredictionData.Bank == "Metro" ? 1f : 0f},
        {"bank_Monzo", fraudPredictionData.Bank == "Monzo" ? 1f : 0f},
        {"bank_RBS", fraudPredictionData.Bank == "RBS" ? 1f : 0f},
        {"type_of_card_Visa", fraudPredictionData.TypeOfCard == "Visa" ? 1f : 0f},
        {"country_of_residence_India", fraudPredictionData.CountryOfResidence == "India" ? 1f : 0f},
        {"country_of_residence_Russia", fraudPredictionData.CountryOfResidence == "Russia" ? 1f : 0f},
        {"country_of_residence_USA", fraudPredictionData.CountryOfResidence == "USA" ? 1f : 0f},
        {"country_of_residence_United Kingdom", fraudPredictionData.CountryOfResidence == "United Kingdom" ? 1f : 0f},
        {"gender_M", fraudPredictionData.Gender == 'M' ? 1f : 0f}
    };

            // Convert the dictionary into a tensor for the model input
            var inputTensor = new DenseTensor<float>(inputFeatures.Values.ToArray(), new[] { 1, inputFeatures.Count });

            // Prepare input for the ONNX model
            var inputs = new List<NamedOnnxValue>
            {
        NamedOnnxValue.CreateFromTensor("float_input", inputTensor)
            };

            // Run the model
            using (var results = session.Run(inputs))
            {
                var prediction = results.FirstOrDefault(item => item.Name == "output_label")?.AsTensor<long>().ToArray();
                
                var predictionValue = prediction[0];
               
                var returnValue = false;

                if (predictionValue == 1)
                {
                    returnValue = true;
                }
                else
                {
                    returnValue =  false;
                }

                return returnValue;
            }

        }


        public IActionResult Products(int pageNum = 1, string productCategory = null, string productColor = null, int pageSize = 5)
        
        {
            var query = _repo.Products
                .Where(x => productCategory == null || x.CatalogCategory == productCategory)
                .Where(x => productColor == null || x.PrimaryColor == productColor)
                .OrderBy(x => x.Name);

            var totalItems = query.Count();

            var products = new ProductsListViewModel
            {
                Products = query.Skip((pageNum - 1) * pageSize)
                               .Take(pageSize),

                PaginationInfo = new PaginationInfo
                {
                    CurrentPage = pageNum,
                    ItemsPerPage = pageSize,
                    TotalItems = totalItems
                },

                CurrentProductCategory = productCategory,
                CurrentProductColor = productColor
            };

            return View(products);
        }


        public IActionResult AboutUs()
        {
            return View("AboutUs");
        }

        public IActionResult Privacy()
        {
            return View("Privacy");
        }

        public IActionResult Help()
        {
            return View("Help");
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View("Login");
        }

        [Authorize(Roles = "Admin,Customer")]
        public IActionResult Confirmation()
        {
            Random rand = new Random();
            int orderNumber = rand.Next(1000000, 10000000);

            // Pass the order number to the view
            ViewBag.OrderNumber = orderNumber;

            return View();
        }

        [Authorize(Roles = "Admin,Customer")]
        public IActionResult FraudConfirmation()
        {
            return View("FraudConfirmation");
        }


        public IActionResult SingleProduct(int id, string returnUrl)
        {
            ViewBag.returnUrl = returnUrl;

            var viewModel = new SimilarProductViewModel();

            // Set ProductId for Item_Based_Recs
            viewModel.Item_Based_Recs = new Item_Based_Recs
            {
                ProductId = id
            };

            // Retrieve product based on id
            viewModel.Product = _repo.Products.FirstOrDefault(p => p.ProductId == id);

            // Retrieve recommendations based on ProductId
            viewModel.Item_Based_Recs.Recommendation1 = (_repo.ItemBasedRecs.FirstOrDefault(p => p.ProductId == id)?.Recommendation1 ?? 0) == 0 ? 1 : (int)(_repo.ItemBasedRecs.FirstOrDefault(p => p.ProductId == id)?.Recommendation1);
            viewModel.Item_Based_Recs.Recommendation2 = (_repo.ItemBasedRecs.FirstOrDefault(p => p.ProductId == id)?.Recommendation2 ?? 0) == 0 ? 1 : (int)(_repo.ItemBasedRecs.FirstOrDefault(p => p.ProductId == id)?.Recommendation2);
            viewModel.Item_Based_Recs.Recommendation3 = (_repo.ItemBasedRecs.FirstOrDefault(p => p.ProductId == id)?.Recommendation3 ?? 0) == 0 ? 1 : (int)(_repo.ItemBasedRecs.FirstOrDefault(p => p.ProductId == id)?.Recommendation3);


            // Fetch details of recommended products
            viewModel.Recommendation1Product = _repo.Products.FirstOrDefault(p => p.ProductId == viewModel.Item_Based_Recs.Recommendation1);
            viewModel.Recommendation2Product = _repo.Products.FirstOrDefault(p => p.ProductId == viewModel.Item_Based_Recs.Recommendation2);
            viewModel.Recommendation3Product = _repo.Products.FirstOrDefault(p => p.ProductId == viewModel.Item_Based_Recs.Recommendation3);

            return View(viewModel); // Pass the view model to the view
        }



    [Authorize(Roles = "Admin,Customer")]
[HttpPost]
public async Task<IActionResult> Cart(decimal total)
{
    var user = await _userManager.GetUserAsync(User);
    if (user == null)
    {
        // Handle the case where the user is not found
        return View("Error"); // Or redirect to a login page
    }

    var userId = _userManager.GetUserId(User); // This gets the user's identity ID, not the CustomerId

    var customer = _repo.Customers.FirstOrDefault(c => c.AspNetUserId == userId);
    if (customer == null)
    {
        // Handle the case where no customer is found for the user
        return View("Error"); // Or an appropriate error handling
    }

    TempData["CustomerId"] = customer.CustomerId.ToString(); // Make sure you are accessing the CustomerId property
    TempData["OrderAmount"] = total.ToString();

    return RedirectToAction("Checkout");
}





public IActionResult Checkout()
        {
            OrderSubmissionViewModel model = new OrderSubmissionViewModel();

            if (TempData["CustomerId"] is string customerIdString && int.TryParse(customerIdString, out int customerId))
            {
                model.CustomerId = customerId;
            }
            else
            {
                // Handle the case where CustomerId is not available
                return RedirectToAction("Error"); // Redirect to an error page or handle appropriately
            }

            // Optionally set other order details if needed
            if (TempData["OrderAmount"] is string orderAmountString && decimal.TryParse(orderAmountString, out decimal orderAmount))
            {
                model.Order = new Order { Amount = orderAmount };
            }

            return View("Checkout", model);
        }




        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult AddProduct(Product response)
        {
            _repo.AddProduct(response);
            _repo.SaveChanges();
            return RedirectToAction("AdminProducts"); 
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult DeleteProduct(int id)
        {
            var recordToDelete = _repo.Products
                .Single(x => x.ProductId == id);

            return View("DeleteProduct",recordToDelete);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult DeleteProduct(Product record)
        {
            _repo.DeleteProduct(record);

            return RedirectToAction("AdminProduct");
        }

        [Authorize(Roles = "Admin")]
        public IActionResult EditProduct(int id)
        {
            var recordToEdit = _repo.Products
                .Single(x => x.ProductId == id);

            return View("EditProduct", recordToEdit);

        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult EditProduct(Product updatedInfo)
        {
            _repo.EditProduct(updatedInfo);
            _repo.SaveChanges();

            return RedirectToAction("AdminProduct");
        }

        [Authorize(Roles = "Admin")]
        public IActionResult AdminProducts(int pageNum = 1, string? productCategory = null, int pageSize = 1)
        {
            //var products = _repo.Products.ToList();
            //return View(products);
            int pgSize = pageSize;
            int defaultPageSize = 6;
            if (pgSize == 1)
            {
                pgSize = defaultPageSize;
                if (HttpContext.Session.GetInt32("pageSize") != null)
                {
                    pgSize = (int)HttpContext.Session.GetInt32("pageSize");
                }
            }
            if (HttpContext.Session.GetInt32("pageSize") != pgSize)
            {
                HttpContext.Session.SetInt32("pageSize", pgSize);
            }
            var productList = _repo.Products
                .Where(x => x.Category == productCategory || productCategory == null)
                .OrderBy(x => x.Category);
            ProductsListViewModel plvm = new ProductsListViewModel
            {
                Products = productList.Skip((pageNum - 1) * pgSize).Take(pgSize),
                PaginationInfo = new PaginationInfo
                {
                    CurrentPage = pageNum,
                    ItemsPerPage = pgSize,
                    TotalItems = productList.Count()
                }
            };
            return View(plvm);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult AdminCustomers()
        {
            var customers = _repo.Customers.ToList();
            return View(customers);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult AdminOrders()
        {
            var orders = _repo.Orders
                .OrderByDescending(x => x.Date) // Order by date descending
                .ToList();

            return View(orders);

        }


        [Authorize(Roles = "Admin")]
        public IActionResult AdminHomepage()
        {
            return View();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult AdminAddProduct()
        {
            return View("AddProduct");
        }
    }
}
