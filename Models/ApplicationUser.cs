using Microsoft.AspNetCore.Identity;

namespace ApnaRent.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }
        public bool IsOtpVerified { get; set; } = false;
    }
}