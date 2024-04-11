namespace INTEX_II_413.Models.ViewModels
{
    public class ProductsListViewModel
    {
        public IQueryable<Product> Products { get; set;}
        public PaginationInfo PaginationInfo { get; set;} = new PaginationInfo();

        // string for current product category
        public string? CurrentProductCategory { get; set; }

        // string for current product color
        public string? CurrentProductColor { get; set; }

    }
}
