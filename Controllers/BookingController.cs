using ApnaRent.Data;
using ApnaRent.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

public class BookingController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public BookingController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    private void RefreshBookingStatuses()
    {
        var now = DateTime.Now;

        var toActivate = _context.Bookings.Include(b => b.Item)
            .Where(b => b.Status == "Approved" && b.FromDate <= now && b.ToDate >= now).ToList();
        foreach (var b in toActivate) b.Status = "Active";

        var toComplete = _context.Bookings.Include(b => b.Item)
            .Where(b => (b.Status == "Approved" || b.Status == "Active") && b.ToDate < now).ToList();
        foreach (var b in toComplete)
        {
            b.Status = "Completed";
            b.Item.AvailabilityCount += b.Quantity;
        }

        _context.SaveChanges();
    }

    [Authorize(Roles = "User")]
    public IActionResult Create(int itemId)
    {
        var item = _context.Items.Find(itemId);
        if (item == null) return RedirectToAction("Index", "Home");

        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (item.OwnerId == currentUserId)
        {
            TempData["Error"] = "You cannot rent your own item.";
            return RedirectToAction("Index", "Home");
        }

        var booking = new Booking { ItemId = item.Id, Quantity = 1 };
        ViewBag.ItemName = item.Name;
        ViewBag.PricePerDay = item.PricePerDay;
        ViewBag.MaxAvailable = item.AvailabilityCount;
        ViewBag.OwnerId = item.OwnerId;

        return View(booking);
    }

    [HttpPost]
    [Authorize(Roles = "User")]
    public IActionResult Create(Booking booking)
    {
        var item = _context.Items.FirstOrDefault(i => i.Id == booking.ItemId);
        if (item == null) return RedirectToAction("Index", "Home");

        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (item.OwnerId == currentUserId)
        {
            TempData["Error"] = "You cannot rent your own item.";
            return RedirectToAction("Index", "Home");
        }

        int days = (booking.ToDate - booking.FromDate).Days;
        if (days <= 0) return RedirectToAction("Index", "Home");

        if (booking.Quantity <= 0) booking.Quantity = 1;

        if (item.AvailabilityCount < booking.Quantity)
        {
            TempData["Error"] = $"Only {item.AvailabilityCount} left in stock.";
            return RedirectToAction("Create", new { itemId = item.Id });
        }

        booking.UserId = currentUserId;
        booking.Days = days;
        booking.TotalPrice = days * item.PricePerDay * booking.Quantity;
        booking.Status = "Pending";
        booking.PaymentMethod = "Cash on Delivery";
        booking.Item = item;

        item.AvailabilityCount -= booking.Quantity;

        _context.Bookings.Add(booking);
        _context.SaveChanges();

        _context.Notifications.Add(new Notification
        {
            UserId = item.OwnerId,
            Message = $"New rental request for {item.Name}",
            Type = "BookingRequest",
            RelatedBookingId = booking.Id
        });
        _context.SaveChanges();

        return RedirectToAction("MyBookings", "Booking");
    }

    [Authorize(Roles = "User")]
    public IActionResult MyBookings()
    {
        RefreshBookingStatuses();
        var userId = _userManager.GetUserId(User);

        var bookings = _context.Bookings
            .Include(b => b.Item)
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.Id)
            .ToList();

        return View(bookings);
    }

    // Owner manages requests for their own listed items
    [Authorize]
    public IActionResult MyItemRequests(string status)
    {
        RefreshBookingStatuses();
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var bookingsQuery = _context.Bookings
            .Include(b => b.User)
            .Include(b => b.Item)
            .Where(b => b.Item.OwnerId == userId);

        ViewBag.PendingCount = bookingsQuery.Count(b => b.Status == "Pending");

        if (!string.IsNullOrEmpty(status))
            bookingsQuery = bookingsQuery.Where(b => b.Status == status);

        return View(bookingsQuery.OrderByDescending(b => b.Id).ToList());
    }

    [Authorize]
    public IActionResult Complete(int id)
    {
        var booking = _context.Bookings.Include(b => b.Item).FirstOrDefault(b => b.Id == id);
        if (booking == null) return NotFound();
        if (booking.Status != "Approved" && booking.Status != "Active") return RedirectToAction("MyBookings");

        booking.Status = "Completed";
        booking.Item.AvailabilityCount += booking.Quantity;

        _context.SaveChanges();
        return RedirectToAction("MyBookings");
    }

    [Authorize]
    public IActionResult Cancel(int id)
    {
        var booking = _context.Bookings.Include(b => b.Item).FirstOrDefault(b => b.Id == id);
        if (booking == null) return NotFound();
        if (booking.Status != "Pending") return RedirectToAction("MyBookings");

        booking.Status = "Cancelled";
        booking.Item.AvailabilityCount += booking.Quantity;

        _context.SaveChanges();
        return RedirectToAction("MyBookings");
    }

    // ✅ Admin's global oversight page: Pending shows only Admin-owned items; other tabs show platform-wide
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AdminIndex(string status = "Approved")
    {
        RefreshBookingStatuses();

        var adminUsers = await _userManager.GetUsersInRoleAsync("Admin");
        var adminIds = adminUsers.Select(a => a.Id).ToHashSet();

        var bookingsQuery = _context.Bookings
            .Include(b => b.User)
            .Include(b => b.Item).ThenInclude(i => i.Owner)
            .AsQueryable();

        if (status == "Pending")
            bookingsQuery = bookingsQuery.Where(b => b.Status == "Pending" && adminIds.Contains(b.Item.OwnerId));
        else
            bookingsQuery = bookingsQuery.Where(b => b.Status == status);

        ViewBag.PendingCount = _context.Bookings.Count(b => b.Status == "Pending" && adminIds.Contains(b.Item.OwnerId));
        ViewBag.SelectedStatus = status;

        return View(bookingsQuery.OrderByDescending(b => b.Id).ToList());
    }

    [Authorize]
    public IActionResult Approve(int id)
    {
        var booking = _context.Bookings.Include(b => b.Item).FirstOrDefault(b => b.Id == id);
        if (booking == null) return NotFound();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!User.IsInRole("Admin") && booking.Item.OwnerId != userId)
            return Forbid();

        booking.Status = "Approved";

        _context.Notifications.Add(new Notification
        {
            UserId = booking.UserId,
            Message = $"Your booking for {booking.Item.Name} was approved!",
            Type = "BookingApproved",
            RelatedBookingId = booking.Id
        });

        _context.SaveChanges();

        return User.IsInRole("Admin")
            ? RedirectToAction(nameof(AdminIndex), new { status = "Pending" })
            : RedirectToAction(nameof(MyItemRequests));
    }

    [HttpPost]
    [Authorize]
    public IActionResult Reject(int id, string reason)
    {
        var booking = _context.Bookings.Include(b => b.Item).FirstOrDefault(b => b.Id == id);
        if (booking == null) return NotFound();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!User.IsInRole("Admin") && booking.Item.OwnerId != userId)
            return Forbid();

        booking.Status = "Rejected";
        booking.RejectionReason = reason;
        booking.Item.AvailabilityCount += booking.Quantity;

        _context.Notifications.Add(new Notification
        {
            UserId = booking.UserId,
            Message = $"Your booking for {booking.Item.Name} was rejected: {reason}",
            Type = "BookingRejected",
            RelatedBookingId = booking.Id
        });

        _context.SaveChanges();

        return User.IsInRole("Admin")
            ? RedirectToAction(nameof(AdminIndex), new { status = "Pending" })
            : RedirectToAction(nameof(MyItemRequests));
    }
    [Authorize]
    public IActionResult Details(int id)
    {
        var booking = _context.Bookings
            .Include(b => b.Item).ThenInclude(i => i.Owner)
            .Include(b => b.User)
            .FirstOrDefault(b => b.Id == id);

        if (booking == null) return NotFound();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        bool isAdmin = User.IsInRole("Admin");
        bool isRenter = booking.UserId == userId;
        bool isOwner = booking.Item.OwnerId == userId;

        if (!isAdmin && !isRenter && !isOwner)
            return Forbid();

        return View(booking);
    }
}