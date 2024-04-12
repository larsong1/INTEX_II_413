using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace INTEX_II_413.Models
{
    public class Product
    {
        [Key]
        public int ProductId { get; set; }

        public string Name { get; set; }

        public string? Description { get; set; }

        [Column(TypeName = "decimal(8, 2)")]
        public decimal Price { get; set; }

        public string Category { get; set; } = String.Empty;

        public int Year { get; set; }
        public int NumParts { get; set; }
        public string ImgLink { get; set; }
        public string PrimaryColor { get; set; }
        public string SecondaryColor { get; set; }

        public string? CatalogCategory { get; set; }
    }
}
