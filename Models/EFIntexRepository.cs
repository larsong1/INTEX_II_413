namespace INTEX_II_413.Models
{
    public class EFIntexRepository : IIntexRepository
    {
        private IntexContext _context;
        public EFIntexRepository(IntexContext context)
        {
            _context = context;
        }
        public IQueryable<Product> Products => _context.Products;
    }
}
