    using ApnaRent.Data;
    using ApnaRent.Models;
    using ApnaRent.ViewModels;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using System.Security.Claims;

    [Authorize]
    public class ChatController : Controller
    {
        private readonly ApplicationDbContext _context;
        public ChatController(ApplicationDbContext context) => _context = context;

        public IActionResult Index(int itemId, string withUserId = null)
        {
            var item = _context.Items.FirstOrDefault(i => i.Id == itemId);
            if (item == null) return NotFound();

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            string otherUserId;

            if (currentUserId == item.OwnerId)
            {
                if (string.IsNullOrEmpty(withUserId)) return BadRequest();
                otherUserId = withUserId;
            }
            else
            {
                otherUserId = item.OwnerId;
            }

            var messages = _context.Messages
                .Where(m => m.ItemId == itemId &&
                    ((m.SenderId == currentUserId && m.ReceiverId == otherUserId) ||
                     (m.SenderId == otherUserId && m.ReceiverId == currentUserId)))
                .OrderBy(m => m.SentOn)
                .ToList();

            var unreadNotifs = _context.Notifications
                .Where(n => n.UserId == currentUserId && n.Type == "NewMessage"
                    && n.RelatedItemId == itemId && !n.IsRead)
                .ToList();
            foreach (var n in unreadNotifs) n.IsRead = true;
            _context.SaveChanges();

            ViewBag.ItemId = itemId;
            ViewBag.ItemName = item.Name;
            ViewBag.CurrentUserId = currentUserId;
            ViewBag.OtherUserId = otherUserId;

            return View(messages);
        }

        [HttpPost]
        public IActionResult Send(int itemId, string otherUserId, string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return RedirectToAction("Index", new { itemId, withUserId = otherUserId });

            var senderId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            _context.Messages.Add(new Message
            {
                ItemId = itemId,
                SenderId = senderId,
                ReceiverId = otherUserId,
                Text = text
            });

            var item = _context.Items.First(i => i.Id == itemId);

            _context.Notifications.Add(new Notification
            {
                UserId = otherUserId,
                Message = $"New message about {item.Name}",
                Type = "NewMessage",
                RelatedItemId = itemId,
                RelatedOtherUserId = senderId
            });

            _context.SaveChanges();

            return RedirectToAction("Index", new { itemId, withUserId = otherUserId });
        }

        public IActionResult Inbox()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var myMessages = _context.Messages
                .Include(m => m.Item)
                .Where(m => m.SenderId == userId || m.ReceiverId == userId)
                .ToList();

            var conversations = myMessages
                .GroupBy(m => new { m.ItemId, OtherUserId = m.SenderId == userId ? m.ReceiverId : m.SenderId })
                .Select(g =>
                {
                    var last = g.OrderByDescending(m => m.SentOn).First();
                    var otherUser = _context.Users.FirstOrDefault(u => u.Id == g.Key.OtherUserId);
                    return new ChatInboxItemVM
                    {
                        ItemId = g.Key.ItemId,
                        OtherUserId = g.Key.OtherUserId,
                        OtherUserName = otherUser?.FullName ?? otherUser?.Email ?? "User",
                        ItemName = g.First().Item.Name,
                        LastMessage = last.Text,
                        LastMessageTime = last.SentOn
                    };
                })
                .OrderByDescending(c => c.LastMessageTime)
                .ToList();

            return View(conversations);
        }
    }