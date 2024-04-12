using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Transactions;

namespace INTEX_II_413.Models
{
    public class Order
    {
        [Key]
        public int TransactionId { get; set; }
        [ForeignKey("Customer")]
        public int CustomerId { get; set; }
        public DateTime Date {  get; set; }
        public string DayOfWeek { get; set; }
        public DateTime Time { get; set; }
        public string CardType { get; set; }
        public string EntryMode { get; set; }
        [Column(TypeName = "decimal(8, 2)")]
        public decimal Amount { get; set; }
        public string Type { get; set; }
        public string Country { get; set; }
        public string? Address { get; set; }
        public string Bank { get; set; }
        public bool Fraud { get; set; }
        public bool? FraudPredicted { get; set; }

        public virtual ICollection<LineItem> LineItems { get; set; } = new List<LineItem>();

    }
}
