using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PROEL4W_MVC_Kaijenson_Motor_Shop.Data;

namespace PROEL4W_MVC_Kaijenson_Motor_Shop.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            if (HttpContext.Session.GetInt32("UserId") != null)
                return RedirectToAction("Index", "Dashboard");

            return RedirectToAction("Login", "Account");
        }

        // GET: /Home/GlobalSearch?q=...
        [HttpGet]
        public async Task<IActionResult> GlobalSearch(string q)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return Json(new { products = Array.Empty<object>(), orders = Array.Empty<object>(), customers = Array.Empty<object>() });

            if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
                return Json(new { products = Array.Empty<object>(), orders = Array.Empty<object>(), customers = Array.Empty<object>() });

            var isAdmin = HttpContext.Session.GetString("UserRole") == "Admin";

            // Search products
            var products = await _context.Products
                .Where(p => p.ProductName.Contains(q) || p.Category.Contains(q))
                .Take(5)
                .Select(p => new
                {
                    p.ProductId,
                    p.ProductName,
                    p.Category,
                    Price = p.Price.ToString("N2"),
                    p.Status,
                    p.StockQuantity
                })
                .ToListAsync();

            // Search orders
            var orders = await _context.Sales
                .Where(s => s.CustomerName.Contains(q) || s.SaleId.ToString().Contains(q))
                .Take(5)
                .Select(s => new
                {
                    s.SaleId,
                    s.CustomerName,
                    Total = s.Total.ToString("N2"),
                    s.Status,
                    Date = s.Date.ToString("MMM dd, yyyy")
                })
                .ToListAsync();

            // Search customers (admin only)
            object[] customers = Array.Empty<object>();
            if (isAdmin)
            {
                customers = (await _context.Customers
                    .Where(c => c.Name.Contains(q) || (c.Email != null && c.Email.Contains(q)) || (c.Phone != null && c.Phone.Contains(q)))
                    .Take(5)
                    .Select(c => new
                    {
                        c.CustomerId,
                        c.Name,
                        c.Email,
                        c.Phone,
                        TotalPurchases = c.TotalPurchases.ToString("N2")
                    })
                    .ToListAsync()).ToArray<object>();
            }

            return Json(new { products, orders, customers });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}
