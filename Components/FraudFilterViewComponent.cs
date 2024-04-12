using Microsoft.AspNetCore.Mvc;
using INTEX_II_413.Models;

namespace INTEX_II_413.Components
{
    public class FraudFilterViewComponent : ViewComponent
    {
        private IIntexRepository _repo;
        public FraudFilterViewComponent(IIntexRepository temp) {
            _repo = temp;
        }

        public IViewComponentResult Invoke()
        {
            ViewBag.SelectedFraudFilter = RouteData?.Values["fraudFilter"];
            var fraudFilter= _repo.Orders
                .Select(x => x.FraudPredicted)
                .Distinct()
                .OrderBy(x => x);

            return View(fraudFilter);
        }
    }
}
