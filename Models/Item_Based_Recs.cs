using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace INTEX_II_413.Models
{
    public class Item_Based_Recs
    {
        [Key]
        [Required]
        public int ProductId { get; set; }

        // Top 10 Recommendations
        [Required]
        public int Recommendation1 { get; set; }
        [Required]
        public int Recommendation2 { get; set; }
        [Required]
        public int Recommendation3 { get; set; }
        [Required]
        public int Recommendation4 { get; set; }
        [Required]
        public int Recommendation5 { get; set; }
        [Required]
        public int Recommendation6 { get; set; }
        [Required]
        public int Recommendation7 { get; set; }
        [Required]
        public int Recommendation8 { get; set; }
        [Required]
        public int Recommendation9 { get; set; }
        [Required]
        public int Recommendation10 { get; set; }

        
    }
}

