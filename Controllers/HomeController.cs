using Azure;
using INTEX_II_413.Models;
using INTEX_II_413.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace INTEX_II_413.Controllers
{
    public class HomeController : Controller
    {
        private IIntexRepository _repo;

        public HomeController(IIntexRepository temp)
        {
            _repo = temp;
        }

        public IActionResult Index()
        {
            return View("Index");
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
            ViewBag.returnUrl = returnUrl;

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
    }
}
