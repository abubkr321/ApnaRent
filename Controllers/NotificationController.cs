using ApnaRent.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
public class NotificationController : Controller
{
    private readonly ApplicationDbContext _context;
    public NotificationController(ApplicationDbContext context) => _context = context;

    public IActionResult GoTo(int id)
    {
        var notif = _context.Notifications.Find(id);
        if (notif == null) return RedirectToAction("Index", "Home");

        notif.IsRead = true;
        _context.SaveChanges();

        return notif.Type switch
        {
            "BookingRequest" => RedirectToAction("AdminIndex", "Booking", new { status = "Pending" }),
            "BookingApproved" => RedirectToAction("MyBookings", "Booking"),
            "BookingRejected" => RedirectToAction("MyBookings", "Booking"),
            "NewMessage" => RedirectToAction("Index", "Chat", new { itemId = notif.RelatedItemId, withUserId = notif.RelatedOtherUserId }),
            "ItemApproved" => RedirectToAction("Details", "Item", new { id = notif.RelatedItemId }),
            "ItemRejected" => RedirectToAction("Index", "Item"),
            "ItemListingRequest" => RedirectToAction("Index", "AdminItemApproval", new { status = "Pending" }),
            _ => RedirectToAction("Index", "Home")
        };
    }
}