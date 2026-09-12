using ApnaRent.Data;
using ApnaRent.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

[Authorize]
public class SavedController : Controller
{
    private readonly ApplicationDbContext _context;
    public SavedController(ApplicationDbContext context) => _context = context;

    public IActionResult Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var saved = _context.SavedItems
            .Include(s => s.Item).ThenInclude(i => i.Owner)
            .Where(s => s.UserId == userId)
            .ToList();
        return View(saved);
    }

    [HttpPost]
    public IActionResult Toggle(int itemId, string returnUrl)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var existing = _context.SavedItems.FirstOrDefault(s => s.UserId == userId && s.ItemId == itemId);

        if (existing != null)
            _context.SavedItems.Remove(existing);
        else
            _context.SavedItems.Add(new SavedItem { UserId = userId, ItemId = itemId });

        _context.SaveChanges();

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction("Index", "Home");
    }
}