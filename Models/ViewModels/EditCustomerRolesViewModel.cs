namespace INTEX_II_413.Models.ViewModels
{
    public class EditCustomerRolesViewModel
    {
        public string UserId { get; set; }
        public string Email { get; set; }
        public List<string> Roles { get; set; }
        public List<string> AvailableRoles { get; set; }
    }
}
