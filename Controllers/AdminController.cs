using INTEX_II_413.Models;
using Microsoft.AspNetCore.Mvc;

namespace INTEX_II_413.Controllers
{
    public class AdminController : Controller
    {
        private IIntexRepository _repo;

        public AdminController(IIntexRepository temp)
        {
            _repo = temp;
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
