namespace ApnaRent.Models
{
    public class UserManagementVM
    {
        public string UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public int BookingCount { get; set; }
        public int ItemsListedCount { get; set; }
    }
}