using ApnaRent.Data;
using ApnaRent.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize(Roles = "Admin")]
public class AdminHomeController : Controller
{
    private readonly ApplicationDbContext _context;

    public AdminHomeController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        var months = new List<string>();
        var revenue = new List<decimal>();

        for (int i = 5; i >= 0; i--)
        {
            var month = DateTime.Now.AddMonths(-i);
            months.Add(month.ToString("MMM"));

            var monthTotal = _context.Bookings
                .Where(b => b.Status != "Cancelled" && b.Status != "Rejected"
                    && b.FromDate.Month == month.Month && b.FromDate.Year == month.Year)
                .Sum(b => (decimal?)b.TotalPrice) ?? 0;

            revenue.Add(monthTotal);
        }

        var statusCounts = _context.Bookings
            .GroupBy(b => b.Status)
            .ToDictionary(g => g.Key, g => g.Count());

        var categoryCounts = _context.Items
            .Include(i => i.Category)
            .Where(i => i.Category != null)
            .GroupBy(i => i.Category.Name)
            .ToDictionary(g => g.Key, g => g.Count());

        var vm = new AdminHomeVM
        {
            TotalItems = _context.Items.Count(),
            TotalBookings = _context.Bookings.Count(),
            PendingBookings = _context.Bookings.Count(b => b.Status == "Pending"),
            PendingItemApprovals = _context.Items.Count(i => i.ApprovalStatus == "Pending"),
            TotalRevenue = _context.Bookings.Where(o => o.Status != "Cancelled" && o.Status != "Rejected").Sum(o => (decimal?)o.TotalPrice) ?? 0,
            TotalUsers = _context.Users.Count(),

            RecentBookings = _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Item)
                .OrderByDescending(b => b.Id)
                .Take(5)
                .ToList(),

            LowStockItems = _context.Items.Where(i => i.AvailabilityCount <= 1).ToList(),

            RevenueMonths = months,
            RevenueValues = revenue,
            BookingStatusCounts = statusCounts,
            ItemsByCategory = categoryCounts
        };

        return View(vm);
    }
}