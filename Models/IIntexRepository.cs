using Microsoft.EntityFrameworkCore;

namespace INTEX_II_413.Models
{
    public interface IIntexRepository
    {
        public IQueryable<Product> Products { get; }
        public IQueryable<Customer> Customers { get; }
        public IQueryable<Order> Orders { get; }
        public IQueryable<LineItem> LineItems { get; }
    }
}
