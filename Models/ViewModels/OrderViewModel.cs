namespace INTEX_II_413.Models.ViewModels
{
    public class OrderViewModel
    {
        public int CustomerId { get; set; }
        // Include other fields from the form

        
        public string Bank { get; set; }
        public string CardType { get; set; }

        public string EntryMode { get; set; }
        public string TransactionType { get; set; }

        public string Address { get; set; }
        public string Country { get; set; }
        // You can add more fields as needed
    }

}
