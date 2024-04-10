using Azure;
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
        public IActionResult PlaceOrder(OrderViewModel orderData) //Fix this to take in the correct data
        {
            FraudPredictionViewModel fraudPredictionData = new FraudPredictionViewModel
            {
                // Map orderData and possibly query the database for additional data needed
                Time = orderData.Time,
                Amount = orderData.Amount,
                Age = // Calculate based on customer's birthdate,
                CountryOfTransaction = orderData.Country,
                ShippingAddress = orderData.Address,
                Bank = orderData.Bank,
                TypeOfCard = orderData.CardType,
                CountryOfResidence = // This might be the same as CountryOfTransaction or different based on logic,
                Gender = // Get from Customer data,


                };

            bool isFraudulent = PredictFraud(orderData);

            if (isFraudulent)
            {
                // Redirect to a fraud confirmation page or handle accordingly
                return RedirectToAction("FraudConfirmation");
            }
            else
            {
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

        var product = _repo.Products.Where(p => p.ProductId == id).FirstOrDefault();

            return View(product);
    }
    public IActionResult NewUser()
    {
        return View();
    }

    [HttpPost]
    public IActionResult CreateAccount()
    {
        return View("NewUser");
    }

}

        //[HttpPost]
        //public IActionResult AddProduct(Product response)
        //{
        //    _repo.Product.Add(response);
        //    _repo.SaveChanges();
        //    return View("AdminProducts");
        //}