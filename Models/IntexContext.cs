﻿using Microsoft.EntityFrameworkCore;

namespace INTEX_II_413.Models
{
    public class IntexContext : DbContext
    {
        public IntexContext(DbContextOptions<IntexContext> options) : base(options) { }

        public DbSet<Product> Products { get; set; }
    }
}
