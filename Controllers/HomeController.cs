using INTEX_II_413.Models;
using INTEX_II_413.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

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
        public IActionResult PredictFraud() //This is the method that will be called when the user clicks the "Predict Fraud" button
            // I will receive the input data from the user and use it to make a prediction. 
            // I will receive the input not dummy coded so I will have to do the logic to dummy code it here

            // Exaple: I will receive "country" as a string, but the model expects it to be a number.
            // I will have to write the logic to recognize if the country is "USA", "Canada", "Mexico", etc. and convert it to a number
        {
            //This is the input data that will be used to make the prediction

            // A Tensor is a multi-dimensional array. In this case, we are creating a 1D tensor with 30 elements
            var input = new List<float> { /* input variables*/};
            var inputTensor = new DenseTensor<float>(input.ToArray(), new[] {1, input.Count });

            var inputs = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor("input", inputTensor)
            };

            using (var result = _sessionFraud.Run(inputs)) // This is where the prediction is made
            // The output will be true or false, depending on whether the model thinks the transaction is fraudulent
            {
                var output = result.First().AsTensor<bool>().ToArray();
                if (output[0])
                {
                    //return RedirectToAction("FraudConfirmation");
                }
                else
                {
                    //return RedirectToAction("Confirmation");
                }


                return View(); //This is just a placeholder. I will change it to a redirect to the appropriate view
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

        public IActionResult AdminProducts()
        {
            return View("AdminProducts");
        }

        public IActionResult Orders()
        {
            // This is where the admin will see all the orders that have been placed
            // Specifically they will see orders that have been flagged as fraudulent
            return View();
        }

        public IActionResult SingleProduct(int id)
        {
            var product = _repo.Products.Where(p => p.ProductId == id).FirstOrDefault();

            return View(product);
        }
        public IActionResult NewUser()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Products(int id)
        {
            return RedirectToAction("SingleProduct", new { id = id });
        }

        [HttpPost]
        public IActionResult CreateAccount()
        {
            return View("NewUser");
        }

        
    }
}
