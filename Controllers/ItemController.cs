using ApnaRent.Data;
using ApnaRent.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

[Authorize]
public class ItemController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public ItemController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public IActionResult Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var items = User.IsInRole("Admin")
            ? _context.Items.Include(i => i.Owner).ToList()
            : _context.Items.Where(i => i.OwnerId == userId).ToList();

        var rentedItemIds = _context.Bookings
            .Where(b => b.Status == "Approved" || b.Status == "Active")
            .Select(b => b.ItemId)
            .ToHashSet();

        ViewBag.RentedItemIds = rentedItemIds;

        return View(items);
    }

    [AllowAnonymous]
    public IActionResult AllItems(string search, int? categoryId)
    {
        var itemsQuery = _context.Items.Include(i => i.Owner)
            .Where(i => i.ApprovalStatus == "Approved");

        if (!string.IsNullOrEmpty(search))
        {
            itemsQuery = itemsQuery.Where(i => i.Name.Contains(search) || i.Description.Contains(search));
            ViewBag.SearchTerm = search;
        }

        if (categoryId.HasValue)
        {
            itemsQuery = itemsQuery.Where(i => i.CategoryId == categoryId);
            ViewBag.SelectedCategoryId = categoryId;
        }

        ViewBag.Categories = _context.Categories.ToList();

        ViewBag.NextAvailableDates = _context.Bookings
            .Where(b => b.Status == "Approved" || b.Status == "Active")
            .GroupBy(b => b.ItemId)
            .ToDictionary(g => g.Key, g => (DateTime?)g.Min(b => b.ToDate));

        if (User.Identity.IsAuthenticated)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            ViewBag.CurrentUserId = userId;
            ViewBag.SavedItemIds = _context.SavedItems.Where(s => s.UserId == userId).Select(s => s.ItemId).ToHashSet();
        }

        return View(itemsQuery.OrderByDescending(i => i.Id).ToList());
    }

    public IActionResult Create()
    {
        ViewBag.Categories = _context.Categories.ToList();
        return View(new Item());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Item item)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Categories = _context.Categories.ToList();
            return View(item);
        }

        var newItem = new Item
        {
            Name = item.Name,
            Description = item.Description,
            PricePerDay = item.PricePerDay,
            Location = item.Location,
            AvailabilityCount = item.AvailabilityCount,
            CategoryId = item.CategoryId,
            OwnerId = User.FindFirstValue(ClaimTypes.NameIdentifier),
            IsAvailable = item.AvailabilityCount > 0,
            ApprovalStatus = User.IsInRole("Admin") ? "Approved" : "Pending"
        };

        if (item.ImageFile != null)
        {
            var fileName = Guid.NewGuid() + Path.GetExtension(item.ImageFile.FileName);
            var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/items");
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            var path = Path.Combine(folder, fileName);
            using var stream = new FileStream(path, FileMode.Create);
            item.ImageFile.CopyTo(stream);
            newItem.ImagePath = "/images/items/" + fileName;
        }

        _context.Items.Add(newItem);
        _context.SaveChanges();

        if (!User.IsInRole("Admin"))
        {
            var admins = await _userManager.GetUsersInRoleAsync("Admin");
            foreach (var admin in admins)
            {
                _context.Notifications.Add(new Notification
                {
                    UserId = admin.Id,
                    Message = $"New item listing '{newItem.Name}' is awaiting your approval",
                    Type = "ItemListingRequest",
                    RelatedItemId = newItem.Id
                });
            }
            _context.SaveChanges();

            TempData["Info"] = "Your listing has been submitted and is awaiting admin approval.";
        }

        return RedirectToAction(nameof(Index));
    }

    public IActionResult Edit(int id)
    {
        var item = _context.Items.Find(id);
        if (item == null) return NotFound();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (item.OwnerId != userId)
            return Forbid();

        ViewBag.Categories = _context.Categories.ToList();
        return View(item);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(Item item, IFormFile ImageFile)
    {
        var existingItem = _context.Items.Find(item.Id);
        if (existingItem == null) return NotFound();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (existingItem.OwnerId != userId)
            return Forbid();

        if (!ModelState.IsValid)
        {
            ViewBag.Categories = _context.Categories.ToList();
            return View(item);
        }

        existingItem.Name = item.Name;
        existingItem.Description = item.Description;
        existingItem.PricePerDay = item.PricePerDay;
        existingItem.Location = item.Location;
        existingItem.AvailabilityCount = item.AvailabilityCount;
        existingItem.CategoryId = item.CategoryId;

        if (ImageFile != null && ImageFile.Length > 0)
        {
            var fileName = Guid.NewGuid() + Path.GetExtension(ImageFile.FileName);
            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);
            var filePath = Path.Combine(folderPath, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
                ImageFile.CopyTo(stream);
            existingItem.ImagePath = "/images/" + fileName;
        }

        _context.SaveChanges();
        return RedirectToAction("Index");
    }
    public IActionResult Delete(int id)
    {
        var item = _context.Items.Find(id);
        if (item == null) return NotFound();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        bool isAdmin = User.IsInRole("Admin");

        if (!isAdmin && item.OwnerId != userId)
            return Forbid();

        bool isCurrentlyRented = _context.Bookings.Any(b => b.ItemId == id && (b.Status == "Approved" || b.Status == "Active"));
        if (isCurrentlyRented)
        {
            TempData["Error"] = "This item is currently rented and cannot be deleted.";
            return RedirectToAction("Index");
        }

        _context.Items.Remove(item);
        _context.SaveChanges();
        return RedirectToAction("Index");
    }

    [AllowAnonymous]
    public IActionResult Details(int id)
    {
        var item = _context.Items.Include(i => i.Owner).FirstOrDefault(i => i.Id == id);
        if (item == null) return NotFound();

        var currentUserId = User.Identity.IsAuthenticated ? User.FindFirstValue(ClaimTypes.NameIdentifier) : null;
        bool isOwner = currentUserId != null && item.OwnerId == currentUserId;
        bool isAdmin = User.IsInRole("Admin");

        if (item.ApprovalStatus != "Approved" && !isOwner && !isAdmin)
            return NotFound();

        ViewBag.NextAvailableDate = _context.Bookings
            .Where(b => b.ItemId == id && (b.Status == "Approved" || b.Status == "Active"))
            .OrderBy(b => b.ToDate)
            .Select(b => (DateTime?)b.ToDate)
            .FirstOrDefault();

        if (User.Identity.IsAuthenticated)
        {
            ViewBag.IsOwnItem = isOwner;
            ViewBag.IsSaved = _context.SavedItems.Any(s => s.UserId == currentUserId && s.ItemId == id);
        }

        return View(item);
    }
}