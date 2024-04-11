using Microsoft.AspNetCore.Mvc;
using INTEX_II_413.Models;

namespace INTEX_II_413.Components
{
    public class ProductColorViewComponent : ViewComponent
    {
        private IIntexRepository _repo;
        public ProductColorViewComponent(IIntexRepository temp) {
            _repo = temp;
        }

        public IViewComponentResult Invoke()
        {
            ViewBag.SelectedProductColor = RouteData?.Values["productColor"];
            var productColors = _repo.Products
                .Select(x => x.PrimaryColor)
                .Distinct()
                .OrderBy(x => x);

            return View(productColors);
        }
    }
}
