using Microsoft.AspNetCore.Mvc;
using INTEX_II_413.Models;

namespace INTEX_II_413.Components
{
    public class ProductCategoryViewComponent : ViewComponent
    {
        private IIntexRepository _repo;
        public ProductCategoryViewComponent(IIntexRepository temp) {
            _repo = temp;
        }

        public IViewComponentResult Invoke()
        {
            ViewBag.SelectedProductCategory = RouteData?.Values["productCategory"];
            var productCategorys = _repo.Products
                .Select(x => x.Category)
                .Distinct()
                .OrderBy(x => x);

            return View(productCategorys);
        }
    }
}
