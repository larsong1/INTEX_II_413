using Microsoft.AspNetCore.Mvc;
using INTEX_II_413.Models;

namespace INTEX_II_413.Components
{
    public class CartSummaryViewComponent : ViewComponent
    {
        private Cart cart;

        public CartSummaryViewComponent(Cart cartService)
        {
            cart = cartService;
        }

        public IViewComponentResult Invoke() { return View(cart); }
    }
}
