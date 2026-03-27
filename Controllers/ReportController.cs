using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PROEL4W_MVC_Kaijenson_Motor_Shop.Data;
using System.Text.Json;

namespace PROEL4W_MVC_Kaijenson_Motor_Shop.Controllers
{
    public class ReportController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Report — Reports Overview
        public async Task<IActionResult> Index()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Login", "Account");

            var totalSales = await _context.Sales.Where(s => s.Status == "Completed").SumAsync(s => s.Total);
            var totalOrders = await _context.Sales.CountAsync();
            var totalCustomers = await _context.Customers.CountAsync();
            var totalProducts = await _context.Products.CountAsync();

            ViewData["TotalSales"] = totalSales;
            ViewData["TotalOrders"] = totalOrders;
            ViewData["TotalCustomers"] = totalCustomers;
            ViewData["TotalProducts"] = totalProducts;

            // Monthly sales data for chart
            var monthlySales = await _context.Sales
                .Where(s => s.Status == "Completed" && s.Date >= DateTime.Now.AddMonths(-6))
                .GroupBy(s => new { s.Date.Year, s.Date.Month })
                .Select(g => new { Month = g.Key.Month, Year = g.Key.Year, Total = g.Sum(s => s.Total), Count = g.Count() })
                .OrderBy(g => g.Year).ThenBy(g => g.Month)
                .ToListAsync();

            ViewData["MonthlySalesJson"] = JsonSerializer.Serialize(monthlySales.Select(m => new
            {
                month = new DateTime(m.Year, m.Month, 1).ToString("MMM"),
                sales = m.Total,
                orders = m.Count
            }));

            // Category sales
            var categorySales = await _context.SaleItems
                .Join(_context.Products, si => si.ProductName, p => p.ProductName, (si, p) => new { p.Category, si.Quantity, si.Price })
                .GroupBy(x => x.Category)
                .Select(g => new { name = g.Key, value = g.Sum(x => x.Quantity * x.Price) })
                .OrderByDescending(x => x.value)
                .ToListAsync();

            ViewData["CategorySalesJson"] = JsonSerializer.Serialize(categorySales);

            return View();
        }

        // GET: /Report/Inventory — Inventory Reports
        public async Task<IActionResult> Inventory()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Login", "Account");

            var products = await _context.Products.ToListAsync();

            var stockByCategory = products
                .GroupBy(p => p.Category)
                .Select(g => new { category = g.Key, total = g.Sum(p => p.StockQuantity) })
                .OrderByDescending(x => x.total)
                .ToList();

            ViewData["StockByCategoryJson"] = JsonSerializer.Serialize(stockByCategory);
            ViewData["TotalProducts"] = products.Count;
            ViewData["TotalUnits"] = products.Sum(p => p.StockQuantity);
            ViewData["StockValue"] = products.Sum(p => p.Price * p.StockQuantity);
            ViewData["LowStockCount"] = products.Count(p => p.Status == "Low Stock");
            ViewData["OutOfStockCount"] = products.Count(p => p.Status == "Out of Stock");

            // Stock movement over last 7 days
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

            ViewData["StockMovementJson"] = JsonSerializer.Serialize(movements.Select(m => new
            {
                date = m.date.ToString("MMM dd"),
                stockIn = m.stockIn,
                stockOut = m.stockOut
            }));

            return View(products);
        }

        // GET: /Report/Sales — Sales Reports
        public async Task<IActionResult> Sales()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Login", "Account");

            var sales = await _context.Sales
                .Include(s => s.Items)
                .OrderByDescending(s => s.Date)
                .ToListAsync();

            var totalRevenue = sales.Where(s => s.Status == "Completed").Sum(s => s.Total);
            var avgOrderValue = sales.Any() ? totalRevenue / Math.Max(1, sales.Count(s => s.Status == "Completed")) : 0;

            ViewData["TotalRevenue"] = totalRevenue;
            ViewData["AvgOrderValue"] = avgOrderValue;
            ViewData["TotalOrders"] = sales.Count;
            ViewData["CompletedOrders"] = sales.Count(s => s.Status == "Completed");

            // Top selling products
            var topProducts = await _context.SaleItems
                .GroupBy(si => si.ProductName)
                .Select(g => new { name = g.Key, sold = g.Sum(si => si.Quantity), revenue = g.Sum(si => si.Price * si.Quantity) })
                .OrderByDescending(x => x.revenue)
                .Take(5)
                .ToListAsync();

            ViewData["TopProductsJson"] = JsonSerializer.Serialize(topProducts);

            // Daily sales for last 30 days
            var dailySales = await _context.Sales
                .Where(s => s.Status == "Completed" && s.Date >= DateTime.Now.AddDays(-30))
                .GroupBy(s => s.Date.Date)
                .Select(g => new { date = g.Key, total = g.Sum(s => s.Total), count = g.Count() })
                .OrderBy(g => g.date)
                .ToListAsync();

            ViewData["DailySalesJson"] = JsonSerializer.Serialize(dailySales.Select(d => new
            {
                date = d.date.ToString("MMM dd"),
                total = d.total,
                count = d.count
            }));

            return View(sales);
        }

        // GET: /Report/Restocking — Restocking recommendations
        public async Task<IActionResult> Restocking()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Login", "Account");

            var lowStockProducts = await _context.Products
                .Where(p => p.StockQuantity <= p.MinStock)
                .OrderBy(p => p.StockQuantity)
                .ToListAsync();

            var suppliers = await _context.Suppliers
                .Where(s => s.Status == "Active")
                .ToListAsync();

            ViewData["Suppliers"] = suppliers;

            return View(lowStockProducts);
        }
    }
}
