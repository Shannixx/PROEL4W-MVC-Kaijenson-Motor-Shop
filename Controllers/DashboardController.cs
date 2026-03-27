using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PROEL4W_MVC_Kaijenson_Motor_Shop.Data;
using PROEL4W_MVC_Kaijenson_Motor_Shop.Models;
using System.Text.Json;

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
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Login", "Account");

            var products = await _context.Products.ToListAsync();

            var viewModel = new DashboardViewModel
            {
                TotalProducts = products.Count,
                InStockCount = products.Count(p => p.Status == "In Stock"),
                LowStockCount = products.Count(p => p.Status == "Low Stock"),
                OutOfStockCount = products.Count(p => p.Status == "Out of Stock"),
                TotalUsers = await _context.Users.CountAsync(),
                TotalUnits = products.Sum(p => p.StockQuantity),
                StockValue = products.Sum(p => p.Price * p.StockQuantity),
                TodaysSales = await _context.Sales
                    .Where(s => s.Status == "Completed" && s.Date.Date == DateTime.Today)
                    .SumAsync(s => s.Total),
                TotalOrders = await _context.Sales.CountAsync(),
                RecentProducts = products
                    .OrderByDescending(p => p.CreatedAt)
                    .Take(5)
                    .ToList(),
                RecentLogs = await _context.ActivityLogs
                    .Include(l => l.User)
                    .OrderByDescending(l => l.Timestamp)
                    .Take(10)
                    .ToListAsync(),
                LowStockAlerts = products
                    .Where(p => p.StockQuantity <= p.MinStock && p.StockQuantity > 0)
                    .OrderBy(p => p.StockQuantity)
                    .Take(5)
                    .ToList(),
                RecentTransactions = await _context.Sales
                    .Include(s => s.Items)
                    .OrderByDescending(s => s.Date)
                    .Take(5)
                    .ToListAsync()
            };

            // Stock movement chart data (last 7 days)
            var movements = await _context.StockTransactions
                .Where(t => t.Date >= DateTime.Now.AddDays(-7))
                .GroupBy(t => t.Date.Date)
                .Select(g => new
                {
                    date = g.Key,
                    stockIn = g.Where(t => t.Type == "stock-in").Sum(t => t.Quantity),
                    stockOut = g.Where(t => t.Type == "stock-out").Sum(t => t.Quantity)
                })
                .OrderBy(g => g.date)
                .ToListAsync();

            viewModel.StockMovementJson = JsonSerializer.Serialize(movements.Select(m => new
            {
                date = m.date.ToString("MMM dd"),
                stockIn = m.stockIn,
                stockOut = m.stockOut
            }));

            // Stock by category chart data
            var stockByCategory = products
                .GroupBy(p => p.Category)
                .Select(g => new { category = g.Key, total = g.Sum(p => p.StockQuantity) })
                .OrderByDescending(x => x.total)
                .ToList();

            viewModel.StockByCategoryJson = JsonSerializer.Serialize(stockByCategory);

            return View(viewModel);
        }
    }
}
