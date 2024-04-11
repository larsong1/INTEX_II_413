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

    }
}
