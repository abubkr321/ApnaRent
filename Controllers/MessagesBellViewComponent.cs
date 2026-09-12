using ApnaRent.Data;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

public class MessagesBellViewComponent : ViewComponent
{
    private readonly ApplicationDbContext _context;
    public MessagesBellViewComponent(ApplicationDbContext context) => _context = context;

    public IViewComponentResult Invoke()
    {
        if (!HttpContext.User.Identity.IsAuthenticated)
            return Content("");

        var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

        int unread = _context.Notifications
            .Count(n => n.UserId == userId && n.Type == "NewMessage" && !n.IsRead);

        return View(unread);
    }
}