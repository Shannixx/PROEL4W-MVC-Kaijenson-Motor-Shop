using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PROEL4W_MVC_Kaijenson_Motor_Shop.Data;
using PROEL4W_MVC_Kaijenson_Motor_Shop.Models;

namespace PROEL4W_MVC_Kaijenson_Motor_Shop.Controllers
{
    public class StockTransactionController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StockTransactionController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string searchString, string type, int page = 1)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Login", "Account");

            int pageSize = 15;
            var transactions = _context.StockTransactions.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                transactions = transactions.Where(t =>
                    t.ProductName.Contains(searchString) ||
                    (t.Reference != null && t.Reference.Contains(searchString)));
                ViewData["SearchString"] = searchString;
            }

            if (!string.IsNullOrEmpty(type))
            {
                transactions = transactions.Where(t => t.Type == type);
                ViewData["SelectedType"] = type;
            }

            int totalItems = await transactions.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            page = Math.Max(1, Math.Min(page, Math.Max(1, totalPages)));

            var pagedTransactions = await transactions
                .OrderByDescending(t => t.Date).ThenByDescending(t => t.Time)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"] = totalPages;
            ViewData["TotalItems"] = totalItems;
            ViewData["StockInCount"] = await _context.StockTransactions.CountAsync(t => t.Type == "stock-in");
            ViewData["StockOutCount"] = await _context.StockTransactions.CountAsync(t => t.Type == "stock-out");

            return View(pagedTransactions);
        }

        public IActionResult Create()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Login", "Account");

            ViewData["Products"] = _context.Products.OrderBy(p => p.ProductName).ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(StockTransaction transaction)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Login", "Account");

            if (ModelState.IsValid)
            {
                var userName = HttpContext.Session.GetString("UserName") ?? "System";
                transaction.PerformedBy = userName;
                transaction.Time = DateTime.Now.ToString("HH:mm");

                _context.StockTransactions.Add(transaction);

                // Update product stock
                var product = await _context.Products
                    .FirstOrDefaultAsync(p => p.ProductName == transaction.ProductName);
                if (product != null)
                {
                    if (transaction.Type == "stock-in")
                        product.StockQuantity += transaction.Quantity;
                    else
                    {
                        product.StockQuantity -= transaction.Quantity;
                        if (product.StockQuantity < 0) product.StockQuantity = 0;
                    }
                    product.UpdateStatus();
                    product.UpdatedAt = DateTime.Now;
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Stock transaction recorded successfully!";
                return RedirectToAction(nameof(Index));
            }

            ViewData["Products"] = _context.Products.OrderBy(p => p.ProductName).ToList();
            return View(transaction);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Login", "Account");

            var transaction = await _context.StockTransactions.FindAsync(id);
            if (transaction != null)
            {
                _context.StockTransactions.Remove(transaction);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Transaction deleted!";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
