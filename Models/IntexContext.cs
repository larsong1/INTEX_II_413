using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace INTEX_II_413.Models
{
    public class IntexContext : IdentityDbContext<IdentityUser>
    {
        public IntexContext(DbContextOptions<IntexContext> options) : base(options) { }

        public DbSet<Product> Products { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<LineItem> LineItems { get; set; }

        public DbSet<Item_Based_Recs> ItemBasedRecs { get; set; }
        public DbSet<User_Based_Recs> UserBasedRecs { get; set; }
    
    }
}

