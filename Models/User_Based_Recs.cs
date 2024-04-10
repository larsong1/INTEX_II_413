using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace INTEX_II_413.Models
{
    public class User_Based_Recs
    {
        [Key]
        //[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        public int Recommendation1 { get; set; }

        [Required]
        public int Recommendation2 { get; set; }

        [Required]
        public int Recommendation3 { get; set; }

        [Required]
        public int Recommendation4 { get; set; }

        // Navigation properties for the product and its recommendations
        // These establish relationships to the Product model

        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }

        [ForeignKey("Recommendation1")]
        public virtual Product RecommendedProduct1 { get; set; }

        [ForeignKey("Recommendation2")]
        public virtual Product RecommendedProduct2 { get; set; }

        [ForeignKey("Recommendation3")]
        public virtual Product RecommendedProduct3 { get; set; }

        [ForeignKey("Recommendation4")]
        public virtual Product RecommendedProduct4 { get; set; }
    }
}
