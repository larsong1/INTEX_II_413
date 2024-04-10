using INTEX_II_413.Models;

public class EFIntexRepository : IIntexRepository
{
    private IntexContext _context;

    public EFIntexRepository(IntexContext context)
    {
        _context = context;
    }

    public IQueryable<Product> Products => _context.Products;
    public IQueryable<Customer> Customers => _context.Customers;
    public IQueryable<Order> Orders => _context.Orders;
    public IQueryable<LineItem> LineItems => _context.LineItems;

    public void AddProduct(Product p)
    {
        _context.Products.Add(p);
        _context.SaveChanges();
    }
}
