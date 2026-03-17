using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PROEL4W_MVC_Kaijenson_Motor_Shop.Data;
using PROEL4W_MVC_Kaijenson_Motor_Shop.Models;

namespace PROEL4W_MVC_Kaijenson_Motor_Shop.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Check if user is logged in
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Login", "Account");

            var viewModel = new DashboardViewModel
            {
                TotalProducts = await _context.Products.CountAsync(),
                InStockCount = await _context.Products.CountAsync(p => p.Status == "In Stock"),
                LowStockCount = await _context.Products.CountAsync(p => p.Status == "Low Stock"),
                OutOfStockCount = await _context.Products.CountAsync(p => p.Status == "Out of Stock"),
                TotalUsers = await _context.Users.CountAsync(),
                RecentProducts = await _context.Products
                    .OrderByDescending(p => p.CreatedAt)
                    .Take(5)
                    .ToListAsync(),
                RecentLogs = await _context.ActivityLogs
                    .Include(l => l.User)
                    .OrderByDescending(l => l.Timestamp)
                    .Take(10)
                    .ToListAsync()
            };

            return View(viewModel);
        }
    }
}
