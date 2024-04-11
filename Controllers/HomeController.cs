using Azure;
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

        public IActionResult Products(int pageNum = 1, string productCategory = null, string primaryColor = null, int pageSize = 5)
        {
            var query = _repo.Products
                .Where(x => (productCategory == null || x.Category == productCategory) &&
                            (primaryColor == null || x.PrimaryColor == primaryColor))
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
                CurrentProductColor = primaryColor
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

        [Authorize(Roles = "Admin,Customer")]
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

        [Authorize(Roles = "Admin,Customer")]
        public IActionResult NewUser()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CreateAccount()
        {
            return View("NewUser");
        }

        [Authorize(Roles = "Admin,Customer")]
        [HttpPost]
        public IActionResult Cart() 
        {
            return View("Checkout");
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
            var orders = _repo.Orders.ToList();
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
