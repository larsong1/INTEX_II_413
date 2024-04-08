using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace INTEX_II_413.Models
{
    public class Customer
    {
        [Key]
        public int CustomerId { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime BirthDate { get; set; }
        public string Country {  get; set; }
        public char Gender { get; set; }
        public int Age { get; set; }
        public string? Email {  get; set; }
        public string? UserName { get; set; }
        public string? Password { get; set; }
        public bool? IsAdmin { get; set; }
    }
}
