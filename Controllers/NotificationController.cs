using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PROEL4W_MVC_Kaijenson_Motor_Shop.Data;
using PROEL4W_MVC_Kaijenson_Motor_Shop.Models;

namespace PROEL4W_MVC_Kaijenson_Motor_Shop.Controllers
{
    public class NotificationController : Controller
    {
        private readonly ApplicationDbContext _context;

        public NotificationController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Notification/GetNotifications
        [HttpGet]
        public async Task<IActionResult> GetNotifications()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized();

            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId || n.UserId == null)
                .OrderByDescending(n => n.CreatedAt)
                .Take(20)
                .Select(n => new
                {
                    n.NotificationId,
                    n.Type,
                    n.Title,
                    n.Message,
                    n.IsRead,
                    TimeAgo = GetTimeAgo(n.CreatedAt),
                    CreatedAt = n.CreatedAt.ToString("MMM dd, yyyy HH:mm")
                })
                .ToListAsync();

            return Json(notifications);
        }

        // GET: /Notification/GetUnreadCount
        [HttpGet]
        public async Task<IActionResult> GetUnreadCount()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Json(new { count = 0 });

            var count = await _context.Notifications
                .Where(n => (n.UserId == userId || n.UserId == null) && !n.IsRead)
                .CountAsync();

            return Json(new { count });
        }

        // POST: /Notification/MarkAsRead/5
        [HttpPost]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized();

            var notification = await _context.Notifications.FindAsync(id);
            if (notification != null)
            {
                notification.IsRead = true;
                await _context.SaveChangesAsync();
            }

            return Json(new { success = true });
        }

        // POST: /Notification/MarkAllRead
        [HttpPost]
        public async Task<IActionResult> MarkAllRead()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized();

            var unread = await _context.Notifications
                .Where(n => (n.UserId == userId || n.UserId == null) && !n.IsRead)
                .ToListAsync();

            foreach (var n in unread)
                n.IsRead = true;

            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        // GET: /Notification/GetPreferences
        [HttpGet]
        public async Task<IActionResult> GetPreferences()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized();

            var pref = await _context.NotificationPreferences
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (pref == null)
            {
                // Return defaults
                return Json(new
                {
                    lowStockAlerts = true,
                    newOrderNotifications = true,
                    dailySalesSummary = false,
                    systemUpdates = true,
                    customerActivity = false
                });
            }

            return Json(new
            {
                lowStockAlerts = pref.LowStockAlerts,
                newOrderNotifications = pref.NewOrderNotifications,
                dailySalesSummary = pref.DailySalesSummary,
                systemUpdates = pref.SystemUpdates,
                customerActivity = pref.CustomerActivity
            });
        }

        // POST: /Notification/SavePreferences
        [HttpPost]
        public async Task<IActionResult> SavePreferences([FromBody] NotificationPreferenceDto dto)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized();

            var pref = await _context.NotificationPreferences
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (pref == null)
            {
                pref = new NotificationPreference
                {
                    UserId = userId.Value
                };
                _context.NotificationPreferences.Add(pref);
            }

            pref.LowStockAlerts = dto.LowStockAlerts;
            pref.NewOrderNotifications = dto.NewOrderNotifications;
            pref.DailySalesSummary = dto.DailySalesSummary;
            pref.SystemUpdates = dto.SystemUpdates;
            pref.CustomerActivity = dto.CustomerActivity;

            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        // Helper: Create a notification for specific user
        public static async Task CreateNotification(ApplicationDbContext context, int? userId, string type, string title, string message)
        {
            // Check user preferences before creating
            if (userId != null)
            {
                var pref = await context.NotificationPreferences
                    .FirstOrDefaultAsync(p => p.UserId == userId);

                if (pref != null)
                {
                    bool shouldCreate = type switch
                    {
                        "low-stock" => pref.LowStockAlerts,
                        "new-order" => pref.NewOrderNotifications,
                        "daily-sales" => pref.DailySalesSummary,
                        "system" => pref.SystemUpdates,
                        "customer" => pref.CustomerActivity,
                        _ => true
                    };

                    if (!shouldCreate) return;
                }
            }

            context.Notifications.Add(new Notification
            {
                UserId = userId,
                Type = type,
                Title = title,
                Message = message,
                IsRead = false,
                CreatedAt = DateTime.Now
            });

            await context.SaveChangesAsync();
        }

        // Helper: broadcast to all admins
        public static async Task NotifyAdmins(ApplicationDbContext context, string type, string title, string message)
        {
            var adminIds = await context.Users
                .Where(u => u.Role == "Admin")
                .Select(u => u.UserId)
                .ToListAsync();

            foreach (var adminId in adminIds)
            {
                await CreateNotification(context, adminId, type, title, message);
            }
        }

        private static string GetTimeAgo(DateTime dateTime)
        {
            var span = DateTime.Now - dateTime;
            if (span.TotalMinutes < 1) return "Just now";
            if (span.TotalMinutes < 60) return $"{(int)span.TotalMinutes}m ago";
            if (span.TotalHours < 24) return $"{(int)span.TotalHours}h ago";
            if (span.TotalDays < 7) return $"{(int)span.TotalDays}d ago";
            return dateTime.ToString("MMM dd");
        }
    }

    public class NotificationPreferenceDto
    {
        public bool LowStockAlerts { get; set; }
        public bool NewOrderNotifications { get; set; }
        public bool DailySalesSummary { get; set; }
        public bool SystemUpdates { get; set; }
        public bool CustomerActivity { get; set; }
    }
}
