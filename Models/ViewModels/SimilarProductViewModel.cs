namespace INTEX_II_413.Models.ViewModels
{
    public class SimilarProductViewModel
    {
        public Product Product { get; set; }
        public Item_Based_Recs Item_Based_Recs { get; set; }

        public Product Recommendation1Product { get; set; }
        public Product Recommendation2Product { get; set; }
        public Product Recommendation3Product { get; set; }
    }
}
