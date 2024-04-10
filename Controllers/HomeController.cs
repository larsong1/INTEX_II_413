using Azure;
using System;
using INTEX_II_413.Models;
using INTEX_II_413.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Formats.Tar;
using System.Collections.Generic;

//For Implementing the pipeline
using Microsoft.ML;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace INTEX_II_413.Controllers
{
    public class HomeController : Controller
    {
        private IIntexRepository _repo;

        //This is the pipeline that will be used to make predictions
        private readonly InferenceSession _sessionFraud;

        public HomeController(IIntexRepository temp)
        {
            _repo = temp;

            _sessionFraud = new InferenceSession("C:/Users/Hammo/source/repos/INTEX_II_413/Fraud_Identification_model.onnx");

        }



        public IActionResult Index()
        {
            return View("Index");
        }


        [HttpPost]
        public IActionResult PlaceOrder(OrderSubmissionViewModel submissionModel)
        {
            var customer = _repo.Customers.FirstOrDefault(c => c.CustomerId == submissionModel.CustomerId);
            if (customer == null)
            {
                // Handle the case where the customer is not found
                return NotFound();
            }

            // Calculate the customer's age based on their birthdate
            int age = DateTime.Now.Year - customer.BirthDate.Year;
            if (DateTime.Now < customer.BirthDate.AddYears(age)) age--;

            // Prepare the fraud prediction data
            FraudPredictionViewModel fraudPredictionData = new FraudPredictionViewModel
            {
                Time = submissionModel.Order.Time,
                Amount = submissionModel.Order.Amount,
                Age = age,
                CountryOfTransaction = submissionModel.Order.Country,
                ShippingAddress = submissionModel.Order.Address,
                Bank = submissionModel.Order.Bank,
                TypeOfCard = submissionModel.Order.CardType,
                CountryOfResidence = customer.Country, // Assuming this is where you store the customer's residence country
                Gender = customer.Gender
            };

            // Assuming PredictFraud now properly expects a FraudPredictionViewModel and returns a boolean
            bool isFraudulent = PredictFraud(fraudPredictionData);

            if (isFraudulent)
            {
                // Redirect to a fraud confirmation page or handle accordingly
                return RedirectToAction("FraudConfirmation");
            }
            else
            {
                // Here, you would save the order to your database. Since you don't have _context,
                // you should use whatever mechanism you have in place, such as a repository method.

                // _repo.Orders.Add(submissionModel.Order); // Example repository call to save the order
                // _repo.SaveChanges(); // Save changes to the database

                // Process the order normally
                return RedirectToAction("Confirmation");
            }
        }






        [HttpPost]
        public IActionResult PredictFraud(OrderViewModel orderData)
        {

            // A Tensor is a multi-dimensional array. In this case, we are creating a 1D tensor with 30 elements
            var inputList = new List<float>();

            // Add logic to fill inputList based on orderData

            // Example dummy coding - adjust according to actual orderData structure
            inputList.Add(orderData.TransactionId);
            // Add more inputs based on the expected columns

            // Considering 'entry_mode_CVC' as not present, hence 'entry_mode_PIN' and 'entry_mode_Tap' will be 0
            // Since the type of transaction is always 'Online', set 'type_of_transaction_Online' to 1 and 'type_of_transaction_POS' to 0
            inputList.AddRange(new float[] { 0, 0, 1, 0 }); // Adjust indices based on actual input requirements



            var inputTensor = new DenseTensor<float>(inputList.ToArray(), new[] { 1, inputList.Count });

            var inputs = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor("input", inputTensor)
            };

            using (var result = _sessionFraud.Run(inputs)) // This is where the prediction is made
            {
                var output = result.First().AsTensor<bool>().ToArray();
                return output[0]; // Return the prediction result
            }
        }




        public IActionResult Products(int pageNum = 1, string? productCategory = null)
        {
            int pageSize = 4;

            var productList = _repo.Products
                .Where(x => x.Category == productCategory || productCategory == null)
                .OrderBy(x => x.Category);


            ProductsListViewModel plvm = new ProductsListViewModel
            {
                Products = productList.Skip((pageNum - 1) * pageSize).Take(pageSize),
                PaginationInfo = new PaginationInfo
                {
                    CurrentPage = pageNum,
                    ItemsPerPage = pageSize,
                    TotalItems = productList.Count()
                }
            };

            return View(plvm);
        }
        public IActionResult AboutUs()
        {
            return View("AboutUs");
        }

        public IActionResult Privacy()
        {
            return View("Privacy");
        }
        [HttpGet]
        public IActionResult Login()
        {
            return View("Login");
        }

        public IActionResult Confirmation()
        {
            Random rand = new Random();
            int orderNumber = rand.Next(1000000, 10000000);

            // Pass the order number to the view
            ViewBag.OrderNumber = orderNumber;

            return View();
        }

        public IActionResult FraudConfirmation()
        {
            return View("FraudConfirmation");
        }

        public IActionResult Checkout()
        {
            return View("Checkout");
        }

        public IActionResult SingleProduct(int id, string returnUrl)
        {
            // This is where the admin will see all the orders that have been placed
            // Specifically they will see orders that have been flagged as fraudulent

            // I will need to get the orders from the database
            // var orders = _repo.Orders.Where(o => o.FraudulentFlag == true).ToList();

            //foreach (var order in orders)
            {
                // I will need to display the order number, the customer's name, the date the order was placed, and the total amount of the order
                // I will also need to display the products that were ordered
            }

            return View();
        }

        //var product = _repo.Products.Where(p => p.ProductId == id).FirstOrDefault();

           // return View(product);
    
    public IActionResult NewUser()
    {
        return View();
    }

        [HttpPost]
        public IActionResult CreateAccount()
        {
            return View("NewUser");
        }

        [HttpPost]
        public IActionResult Cart() 
        {
            return View("Checkout");
        }

        [HttpPost]
        public IActionResult AddProduct(Product response)
        {
            _repo.AddProduct(response);
            _repo.SaveChanges();
            return RedirectToAction("AdminProducts"); 
        }

        [HttpGet]
        public IActionResult DeleteProduct(int id)
        {
            var recordToDelete = _repo.Products
                .Single(x => x.ProductId == id);

            return View("DeleteProduct",recordToDelete);
        }
        [HttpPost]

        public IActionResult DeleteProduct(Product record)
        {
            _repo.DeleteProduct(record);

            return RedirectToAction("AdminProduct");
        }

        public IActionResult EditProduct(int id)
        {
            var recordToEdit = _repo.Products
                .Single(x => x.ProductId == id);

            return View("EditProduct", recordToEdit);

        }
        [HttpPost]
        public IActionResult EditProduct(Product updatedInfo)
        {
            _repo.EditProduct(updatedInfo);
            _repo.SaveChanges();

            return RedirectToAction("AdminProduct");
        }

        public IActionResult AdminProducts()
        {
            var products = _repo.Products.ToList();
            return View(products);
        }
        public IActionResult AdminCustomers()
        {
            var customers = _repo.Customers.ToList();
            return View(customers);
        }

        public IActionResult AdminOrders()
        {
            var orders = _repo.Orders.ToList();
            return View(orders);
        }

        public IActionResult AdminHomepage()
        {
            return View();
        }

        [HttpPost]
        public IActionResult AdminAddProduct()
        {
            return View("AddProduct");
        }
    }
}
