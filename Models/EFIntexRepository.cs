using INTEX_II_413.Models;
using System.Linq;

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

    public IQueryable<Item_Based_Recs> ItemBasedRecs => _context.ItemBasedRecs;

    public IQueryable<User_Based_Recs> UserBasedRecs => _context.UserBasedRecs;


    public void AddCustomer(Customer customer)
    {
        _context.Customers.Add(customer);
        _context.SaveChanges();

    }

    public void AddProduct(Product p)
    {
        _context.Products.Add(p);
        _context.SaveChanges();

    }

    public void EditProduct(Product updatedProduct)
    {
        _context.Products.Update(updatedProduct);
        _context.SaveChanges();

    }

    public void DeleteProduct(Product product)
    {
        _context.Products.Remove(product);
        _context.SaveChanges();

    }

    public void EditCustomer(Customer updatedCustomer)
    {
        _context.Customers.Update(updatedCustomer);
        _context.SaveChanges();

    }

    public void DeleteCustomer(Customer customer)
    {
        _context.Customers.Remove(customer);
        _context.SaveChanges();

    }

    public void SaveChanges()
    {
        _context.SaveChanges();
    }
}
