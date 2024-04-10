using Microsoft.EntityFrameworkCore;
using System.Formats.Tar;

namespace INTEX_II_413.Models
{
    public interface IIntexRepository
    {
        public IQueryable<Product> Products { get; }
        public IQueryable<Customer> Customers { get; }
        public IQueryable<Order> Orders { get; }
        public IQueryable<LineItem> LineItems { get; }
        public void AddProduct(Product p);
    }
}
