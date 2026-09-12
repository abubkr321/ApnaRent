using ApnaRent.Models;
using System.Collections.Generic;

namespace ApnaRent.ViewModels
{
    public class AdminHomeVM
    {
        public int TotalItems { get; set; }
        public int TotalBookings { get; set; }
        public int PendingBookings { get; set; }
        public int PendingItemApprovals { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalUsers { get; set; }

        public List<Booking> RecentBookings { get; set; }
        public List<Item> LowStockItems { get; set; }

        public List<string> RevenueMonths { get; set; } = new();
        public List<decimal> RevenueValues { get; set; } = new();
        public Dictionary<string, int> BookingStatusCounts { get; set; } = new();
        public Dictionary<string, int> ItemsByCategory { get; set; } = new();
    }
}