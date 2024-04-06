namespace INTEX_II_413.Models
{
    public interface IIntexRepository
    {
        public IQueryable<Product> Products { get; }
    }
}
