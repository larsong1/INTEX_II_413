using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace INTEX_II_413.Models
{
    public class Product
    {
        [Key]
        public int ProductId { get; set; }

        public string ProductName { get; set; }

        public string ProductDescription { get; set; }

        [Column(TypeName = "decimal(8, 2)")]
        public decimal ProductPrice { get; set; }

        public string ProductCategory { get; set; } = String.Empty;
    }
}
