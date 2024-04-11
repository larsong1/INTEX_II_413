namespace INTEX_II_413.Models.ViewModels
{
    public class FraudPredictionViewModel
    {
        public DateTime Time { get; set; }
        public decimal Amount { get; set; }
        public int Age { get; set; }
        public int Year => Time.Year;
        public int Month => Time.Month;
        public int Day => Time.Day;
        public int DayOfWeekNumeric => (int)Time.DayOfWeek;
        public string CountryOfTransaction { get; set; }
        public string ShippingAddress { get; set; }
        public string Bank { get; set; }
        public string TypeOfCard { get; set; }
        public string CountryOfResidence { get; set; }
        public char Gender { get; set; }
    }
}
