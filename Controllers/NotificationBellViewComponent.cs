using ApnaRent.Data;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

public class NotificationBellViewComponent : ViewComponent
{
    private readonly ApplicationDbContext _context;
    public NotificationBellViewComponent(ApplicationDbContext context) => _context = context;

    public IViewComponentResult Invoke()
    {
        if (!HttpContext.User.Identity.IsAuthenticated)
            return Content("");

        var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

        var notifications = _context.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedOn)
            .Take(10)
            .ToList();

        return View(notifications);
    }
}