using System.Linq;

namespace INTEX_II_413.Models
{
    public interface IIntexRepository
    {
        // get products
        IQueryable<Product> Products { get; }

        // get customers
        IQueryable<Customer> Customers { get; }

        // get orders
        IQueryable<Order> Orders { get; }

        // get line items
        IQueryable<LineItem> LineItems { get; }

        // add product
        void AddProduct(Product p);

        // edit product
        void EditProduct(Product updatedProduct);

        // delete product
        void DeleteProduct(Product product);

        // edit customer
        void EditCustomer(Customer updatedCustomer);

        // delete customer
        void DeleteCustomer(Customer customer);

        // save changes
        void SaveChanges();
    }
}
