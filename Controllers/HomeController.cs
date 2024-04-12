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
using System.Drawing;

namespace INTEX_II_413.Controllers
{
    public class HomeController : Controller
    {
        private IIntexRepository _repo;

        
        private readonly UserManager<IdentityUser> _userManager; 

        private readonly RoleManager<IdentityRole> _roleManager;


        //This is the pipeline that will be used to make predictions
        private readonly InferenceSession _sessionFraud;


        public HomeController(IIntexRepository temp, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _repo = temp;

            _sessionFraud = new InferenceSession("Fraud_Identification_model_3.onnx");

            _userManager = userManager;

            _roleManager = roleManager;
        }

        public IActionResult Index()
        {
            // This will display the public index page to everyone
            var userId = _userManager.GetUserId(User); // This will be null if the user is not logged in

            List<int> recommendedProductIds = new List<int>();

            if (userId != null)
            {
                // Retrieve CustomerId for the logged-in user from the Customer table
                var customer = _repo.Customers.FirstOrDefault(c => c.AspNetUserId == userId);
                if (customer != null)
                {
                    // Fetch high-rated products by joining LineItems and Orders where CustomerId matches
                    var highRatedProducts = _repo.LineItems
                                                 .Join(_repo.Orders,
                                                       li => li.TransactionId,
                                                       o => o.TransactionId,
                                                       (li, o) => new { LineItem = li, Order = o })
                                                 .Where(x => x.Order.CustomerId == customer.CustomerId && x.LineItem.Rating == 5)
                                                 .Select(x => x.LineItem)
                                                 .ToList();

                    if (highRatedProducts.Any())
                    {
                        var random = new Random();
                        // Pick a random high-rated product
                        var highRatedProduct = highRatedProducts[random.Next(highRatedProducts.Count)];

                        // Fetch recommendations for this product
                        var recommendations = _repo.UserBasedRecs
                                                   .FirstOrDefault(r => r.ProductId == highRatedProduct.ProductId);

                        if (recommendations != null)
                        {
                            recommendedProductIds.Add(recommendations.Recommendation1);
                            recommendedProductIds.Add(recommendations.Recommendation2);
                            recommendedProductIds.Add(recommendations.Recommendation3);
                        }
                    }
                }
            }

            // Default product if no user is logged in or no ratings found
            if (!recommendedProductIds.Any())
            {
                recommendedProductIds.Add(24);
                recommendedProductIds.Add(28); // Assuming product 23 is your default
                recommendedProductIds.Add(31);
            }

            var recommendedProducts = _repo.Products
                                           .Where(p => recommendedProductIds.Contains(p.ProductId))
                                           .ToList();

            var viewModel = new IndexViewModel
            {
                RecommendedProducts = recommendedProducts
            };

            return View("Index", viewModel); // Pass the view model to the view
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

            // Clear the cart
            var cart = SessionCart.GetCart(HttpContext.RequestServices);
            cart.Clear();

            return RedirectToAction("Confirmation");
        }

        [HttpPost]
        public bool PredictFraud(FraudPredictionViewModel fraudPredictionData)
        {
            // Load the ONNX model
            InferenceSession session = new InferenceSession("Fraud_Identification_model_3.onnx");

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
                    returnValue = false;
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
            //if (user == null)
            //{
            //    // Handle the case where the user is not found
            //    return View("Login"); // Or redirect to a login page
            //}

            var userId = _userManager.GetUserId(User); // This gets the user's identity ID, not the CustomerId

            var customer = _repo.Customers.FirstOrDefault(c => c.AspNetUserId == userId);
            //if (customer == null)
            //{
            //    // Handle the case where no customer is found for the user
            //    return View("Login"); // Or an appropriate error handling
            //}

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
        [HttpGet]
        public IActionResult DeleteProduct(int id)
        {
            var recordToDelete = _repo.Products
                .Single(x => x.ProductId == id);

            return View("DeleteProduct", recordToDelete);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult DeleteProduct(Product record)
        {
            _repo.DeleteProduct(record);

            return View("AdminHomepage");
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

            return View("AdminHomepage");
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
        public IActionResult AdminCustomers(int pageNum = 1)
        {
            int pgSize = 100;

            var customers = _repo.Customers.OrderByDescending(x => x.CustomerId);

            CustomerListViewModel clvm = new CustomerListViewModel
            {
                Customers = customers.Skip((pageNum - 1) * pgSize).Take(pgSize),
                PaginationInfo = new PaginationInfo
                {
                    CurrentPage = pageNum,
                    ItemsPerPage = pgSize,
                    TotalItems = customers.Count()
                }
            };

            return View(clvm);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult AdminOrders(int pageNum = 1, string fraudFilter = "true", int pageSize = 100)
        {
            var query = _repo.Orders.AsQueryable();

            // Apply fraud filter if provided
            if (!string.IsNullOrEmpty(fraudFilter))
            {
                bool isFraud = bool.Parse(fraudFilter); // Assuming fraudFilter is a string representation of a boolean
                query = query.Where(x => x.FraudPredicted == isFraud);
            }
            else
            {
                // If fraudFilter is null or empty, include all orders (both fraud and non-fraud)
            }

            // Order by Date
            query = query.OrderBy(x => x.Date);

            // Count total items
            var totalItems = query.Count();

            // Paginate the query
            var orders = new OrdersListViewModel
            {
                Orders = query.Skip((pageNum - 1) * pageSize)
                              .Take(pageSize),

                PaginationInfo = new PaginationInfo
                {
                    CurrentPage = pageNum,
                    ItemsPerPage = pageSize,
                    TotalItems = totalItems
                },

                CurrentOrderFilter = fraudFilter
            };

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

        

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EditCustomerRoles(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound();

            var userRoles = await _userManager.GetRolesAsync(user);
            var availableRoles = _roleManager.Roles.Select(r => r.Name).ToList();

            var model = new EditCustomerRolesViewModel
            {
                UserId = userId,
                Email = user.Email,
                Roles = userRoles.ToList(),
                AvailableRoles = availableRoles
            };

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EditCustomerRoles(EditCustomerRolesViewModel model, List<string> selectedRoles)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
                return NotFound();

            var currentRoles = await _userManager.GetRolesAsync(user);
            var result = await _userManager.RemoveFromRolesAsync(user, currentRoles);

            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Failed to remove user roles");
                return View(model);
            }

            result = await _userManager.AddToRolesAsync(user, selectedRoles);
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Failed to add user roles");
                return View(model);
            }

            return RedirectToAction("AdminCustomers");
        }



        [Authorize(Roles = "Admin")]
        public IActionResult EditCustomer(int id)
        {
            var customer = _repo.Customers
                .FirstOrDefault(c => c.CustomerId == id);
            if (customer == null)
            {
                return NotFound();
            }

            return View("EditCustomer", customer);
        }


        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult EditCustomer(Customer updatedInfo)
        {
            _repo.EditCustomer(updatedInfo);
            _repo.SaveChanges();

            return View("AdminHomepage");
        }

        [Authorize(Roles = "Admin")]
        public IActionResult DeleteCustomer(int id)
        {
            var recordToDelete = _repo.Customers
                .Single(x => x.CustomerId == id);

            return View("DeleteCustomer", recordToDelete);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]

        public IActionResult DeleteCustomer(Customer customer)
        {
            _repo.DeleteCustomer(customer);

            return View("AdminHomepage");
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult AddProduct(Product response)
        {
            _repo.AddProduct(response);
            _repo.SaveChanges();
            return View("AdminHomepage");
        }
    }
}
