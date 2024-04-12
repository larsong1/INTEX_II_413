namespace INTEX_II_413.Models.ViewModels
{
    public class OrdersListViewModel
    {
        public IQueryable<Order> Orders { get; set; }
        public PaginationInfo PaginationInfo { get; set; } = new PaginationInfo();

        // string for current product category
        public string? CurrentOrderFilter { get; set; }



    }
}
