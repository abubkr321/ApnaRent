using ApnaRent.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

[AllowAnonymous]
public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult Index(string search, int? categoryId)
    {
        if (User.Identity.IsAuthenticated && User.IsInRole("Admin"))
        {
            return RedirectToAction("Index", "AdminHome");
        }

        var expiredBookings = _context.Bookings
            .Include(b => b.Item)
            .Where(b => b.Status == "Approved" && b.ToDate < DateTime.Now)
            .ToList();

        foreach (var booking in expiredBookings)
        {
            booking.Status = "Completed";
            booking.Item.AvailabilityCount += booking.Quantity;
        }

        _context.SaveChanges();

        var itemsQuery = _context.Items.Include(i => i.Owner).Where(i => i.ApprovalStatus == "Approved");

        if (!string.IsNullOrEmpty(search))
        {
            itemsQuery = itemsQuery.Where(i =>
                i.Name.Contains(search) || i.Description.Contains(search));
            ViewBag.IsSearching = true;
            ViewBag.SearchTerm = search;
        }
        else if (categoryId.HasValue)
        {
            itemsQuery = itemsQuery.Where(i => i.CategoryId == categoryId);
            ViewBag.IsSearching = true;
            ViewBag.SelectedCategoryId = categoryId;
        }
        else
        {
            itemsQuery = itemsQuery.OrderByDescending(i => i.Id).Take(6);
            ViewBag.IsSearching = false;
        }

        ViewBag.Categories = _context.Categories.ToList();

        ViewBag.NextAvailableDates = _context.Bookings
            .Where(b => b.Status == "Approved")
            .GroupBy(b => b.ItemId)
            .ToDictionary(g => g.Key, g => (DateTime?)g.Min(b => b.ToDate));

        if (User.Identity.IsAuthenticated)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            ViewBag.CurrentUserId = userId;
            ViewBag.SavedItemIds = _context.SavedItems.Where(s => s.UserId == userId).Select(s => s.ItemId).ToHashSet();
        }

        return View(itemsQuery.ToList());
    }

    public IActionResult About() => View();
    public IActionResult FAQ() => View();
    public IActionResult Guarantee() => View();

    public IActionResult RentalPolicy()
    {
        return View();
    }
}