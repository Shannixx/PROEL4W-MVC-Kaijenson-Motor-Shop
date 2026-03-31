using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PROEL4W_MVC_Kaijenson_Motor_Shop.Data;

namespace PROEL4W_MVC_Kaijenson_Motor_Shop.Controllers
{
    public class ActivityLogController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ActivityLogController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /ActivityLog
        public async Task<IActionResult> Index(string searchString, string actionFilter, string dateFrom, string dateTo, int page = 1)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Login", "Account");

            if (HttpContext.Session.GetString("UserRole") != "Admin")
            {
                TempData["ErrorMessage"] = "Access denied. Admin privileges required.";
                return RedirectToAction("Index", "Dashboard");
            }

            int pageSize = 20;
            var logs = _context.ActivityLogs.Include(l => l.User).AsQueryable();

            // Search
            if (!string.IsNullOrEmpty(searchString))
            {
                logs = logs.Where(l =>
                    l.Action.Contains(searchString) ||
                    (l.Details != null && l.Details.Contains(searchString)) ||
                    (l.User != null && (l.User.FirstName.Contains(searchString) || l.User.LastName.Contains(searchString))));
                ViewData["SearchString"] = searchString;
            }

            // Action type filter
            if (!string.IsNullOrEmpty(actionFilter))
            {
                logs = logs.Where(l => l.Action == actionFilter);
                ViewData["ActionFilter"] = actionFilter;
            }

            // Date range filter
            if (!string.IsNullOrEmpty(dateFrom) && DateTime.TryParse(dateFrom, out var fromDate))
            {
                logs = logs.Where(l => l.Timestamp >= fromDate);
                ViewData["DateFrom"] = dateFrom;
            }
            if (!string.IsNullOrEmpty(dateTo) && DateTime.TryParse(dateTo, out var toDate))
            {
                logs = logs.Where(l => l.Timestamp <= toDate.AddDays(1));
                ViewData["DateTo"] = dateTo;
            }

            // Get distinct action types for filter dropdown
            ViewData["ActionTypes"] = await _context.ActivityLogs
                .Select(l => l.Action)
                .Distinct()
                .OrderBy(a => a)
                .ToListAsync();

            // Pagination
            int totalItems = await logs.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            page = Math.Max(1, Math.Min(page, Math.Max(1, totalPages)));

            var pagedLogs = await logs
                .OrderByDescending(l => l.Timestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"] = totalPages;
            ViewData["TotalItems"] = totalItems;

            return View(pagedLogs);
        }

        // POST: /ActivityLog/ClearAll
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ClearAll()
        {
            if (HttpContext.Session.GetString("UserRole") != "Admin")
            {
                TempData["ErrorMessage"] = "Access denied.";
                return RedirectToAction("Index", "Dashboard");
            }

            var allLogs = await _context.ActivityLogs.ToListAsync();
            _context.ActivityLogs.RemoveRange(allLogs);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Cleared {allLogs.Count} activity log entries.";
            return RedirectToAction(nameof(Index));
        }
    }
}
