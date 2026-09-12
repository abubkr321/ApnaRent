using ApnaRent.Data;
using ApnaRent.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize(Roles = "Admin")]
public class AdminItemApprovalController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminItemApprovalController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index(string status = "Pending")
    {
        var adminUsers = await _userManager.GetUsersInRoleAsync("Admin");
        var adminIds = adminUsers.Select(a => a.Id).ToHashSet();

        var items = _context.Items
            .Include(i => i.Owner)
            .Include(i => i.Category)
            .Where(i => i.OwnerId != null && !adminIds.Contains(i.OwnerId) && i.ApprovalStatus == status)
            .OrderByDescending(i => i.Id)
            .ToList();

        ViewBag.PendingCount = _context.Items.Count(i => i.OwnerId != null && !adminIds.Contains(i.OwnerId) && i.ApprovalStatus == "Pending");
        ViewBag.SelectedStatus = status;

        return View(items);
    }

    [HttpPost]
    public IActionResult Approve(int id)
    {
        var item = _context.Items.Find(id);
        if (item == null) return NotFound();

        item.ApprovalStatus = "Approved";
        item.ItemRejectionReason = null;

        _context.Notifications.Add(new Notification
        {
            UserId = item.OwnerId,
            Message = $"Your listing '{item.Name}' has been approved and is now live.",
            Type = "ItemApproved",
            RelatedItemId = item.Id
        });

        _context.SaveChanges();
        return RedirectToAction(nameof(Index), new { status = "Pending" });
    }

    [HttpPost]
    public IActionResult Reject(int id, string reason)
    {
        var item = _context.Items.Find(id);
        if (item == null) return NotFound();

        item.ApprovalStatus = "Rejected";
        item.ItemRejectionReason = reason;

        _context.Notifications.Add(new Notification
        {
            UserId = item.OwnerId,
            Message = $"Your listing '{item.Name}' was rejected: {reason}",
            Type = "ItemRejected",
            RelatedItemId = item.Id
        });

        _context.SaveChanges();
        return RedirectToAction(nameof(Index), new { status = "Pending" });
    }

    [HttpPost]
    public IActionResult Delete(int id)
    {
        var item = _context.Items.Find(id);
        if (item == null) return NotFound();

        _context.Items.Remove(item);
        _context.SaveChanges();
        return RedirectToAction(nameof(Index), new { status = "Approved" });
    }
}