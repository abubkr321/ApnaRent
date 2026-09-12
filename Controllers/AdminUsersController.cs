using ApnaRent.Data;
using ApnaRent.Models;
using ApnaRent.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize(Roles = "Admin")]
public class AdminUsersController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;

    public AdminUsersController(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var users = await _userManager.Users.ToListAsync();
        var model = new List<UserManagementVM>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains("Admin")) continue;

            model.Add(new UserManagementVM
            {
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = roles.FirstOrDefault(),
                BookingCount = _context.Bookings.Count(b => b.UserId == user.Id),
                ItemsListedCount = _context.Items.Count(i => i.OwnerId == user.Id)
            });
        }

        return View(model);
    }

    public async Task<IActionResult> Details(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        var roles = await _userManager.GetRolesAsync(user);

        var vm = new AdminUserDetailsVM
        {
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = roles.FirstOrDefault(),
            IsBlocked = user.LockoutEnd != null && user.LockoutEnd > DateTimeOffset.Now,
            IsOtpVerified = user.IsOtpVerified,
            BookingsMade = _context.Bookings.Include(b => b.Item).Where(b => b.UserId == id).OrderByDescending(b => b.Id).ToList(),
            ItemsListed = _context.Items.Where(i => i.OwnerId == id).OrderByDescending(i => i.Id).ToList()
        };

        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> Block(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user != null)
        {
            user.LockoutEnd = DateTimeOffset.MaxValue;
            await _userManager.UpdateAsync(user);
        }
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    public async Task<IActionResult> Unblock(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user != null)
        {
            user.LockoutEnd = null;
            await _userManager.UpdateAsync(user);
        }
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    public async Task<IActionResult> ChangeRole(string id, string newRole)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        var currentRoles = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, currentRoles);
        await _userManager.AddToRoleAsync(user, newRole);

        return RedirectToAction(nameof(Details), new { id });
    }
}