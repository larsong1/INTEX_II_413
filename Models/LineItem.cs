using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace INTEX_II_413.Models
{
    [PrimaryKey(nameof(TransactionId), nameof(ProductId))]
    public class LineItem
    {
        [Key, Column(Order = 1)]
        [ForeignKey("Order")]
        public int TransactionId { get; set; }
        [Key, Column(Order = 2)]
        [ForeignKey("Product")]
        public int ProductId { get; set; }
        public int Qty { get; set; }
        public int Rating { get; set; }
    }
}
