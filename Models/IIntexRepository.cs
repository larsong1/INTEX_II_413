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

        // get item based recommendations
        IQueryable<Item_Based_Recs> ItemBasedRecs { get; }

        // get user based recommendations
        IQueryable<User_Based_Recs> UserBasedRecs { get; }

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

        // add order
        void AddOrder(Order o);

        // save changes
        void SaveChanges();
    }
}
