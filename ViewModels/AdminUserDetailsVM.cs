using ApnaRent.Models;
using System.Collections.Generic;

namespace ApnaRent.ViewModels
{
    public class AdminUserDetailsVM
    {
        public string UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public bool IsBlocked { get; set; }
        public bool IsOtpVerified { get; set; }
        public List<Booking> BookingsMade { get; set; } = new();
        public List<Item> ItemsListed { get; set; } = new();
    }
}