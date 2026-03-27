using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PROEL4W_MVC_Kaijenson_Motor_Shop.Data;
using PROEL4W_MVC_Kaijenson_Motor_Shop.Models;
using System.Text.Json;

namespace PROEL4W_MVC_Kaijenson_Motor_Shop.Controllers
{
    public class SaleController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SaleController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Sale — POS Page
        public async Task<IActionResult> Index()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Login", "Account");

            var products = await _context.Products
                .Where(p => p.StockQuantity > 0)
                .OrderBy(p => p.ProductName)
                .ToListAsync();

            ViewData["Categories"] = await _context.Products
                .Select(p => p.Category)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            ViewData["Customers"] = await _context.Customers
                .OrderBy(c => c.Name)
                .ToListAsync();

            return View(products);
        }

        // POST: /Sale/Checkout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(string customerName, string paymentMethod, string cartItems)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Login", "Account");

            try
            {
                var items = JsonSerializer.Deserialize<List<CartItem>>(cartItems);
                if (items == null || !items.Any())
                {
                    TempData["ErrorMessage"] = "Cart is empty!";
                    return RedirectToAction(nameof(Index));
                }

                var sale = new Sale
                {
                    Date = DateTime.Now,
                    CustomerName = string.IsNullOrEmpty(customerName) ? "Walk-in Customer" : customerName,
                    PaymentMethod = paymentMethod ?? "Cash",
                    Status = "Completed",
                    Total = items.Sum(i => i.Price * i.Quantity)
                };

                _context.Sales.Add(sale);
                await _context.SaveChangesAsync();

                var userName = HttpContext.Session.GetString("UserName") ?? "System";

                foreach (var item in items)
                {
                    _context.SaleItems.Add(new SaleItem
                    {
                        SaleId = sale.SaleId,
                        ProductName = item.ProductName,
                        Quantity = item.Quantity,
                        Price = item.Price
                    });

                    // Update product stock
                    var product = await _context.Products
                        .FirstOrDefaultAsync(p => p.ProductName == item.ProductName);
                    if (product != null)
                    {
                        product.StockQuantity -= item.Quantity;
                        if (product.StockQuantity < 0) product.StockQuantity = 0;
                        product.UpdateStatus();
                        product.UpdatedAt = DateTime.Now;
                    }

                    // Add stock transaction
                    _context.StockTransactions.Add(new StockTransaction
                    {
                        Date = DateTime.Now,
                        Time = DateTime.Now.ToString("HH:mm"),
                        ProductName = item.ProductName,
                        Type = "stock-out",
                        Quantity = item.Quantity,
                        Reference = $"S{sale.SaleId:D3}",
                        PerformedBy = "POS System"
                    });
                }

                // Update customer total purchases
                if (!string.IsNullOrEmpty(customerName))
                {
                    var customer = await _context.Customers
                        .FirstOrDefaultAsync(c => c.Name == customerName);
                    if (customer != null)
                    {
                        customer.TotalPurchases += sale.Total;
                        customer.LastVisit = DateTime.Now;
                    }
                }

                // Log activity
                var userId = HttpContext.Session.GetInt32("UserId");
                _context.ActivityLogs.Add(new ActivityLog
                {
                    UserId = userId,
                    Action = "New Sale",
                    Details = $"Sale #{sale.SaleId} — ₱{sale.Total:N2} from {sale.CustomerName}",
                    Timestamp = DateTime.Now
                });

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Sale #{sale.SaleId} completed! Total: ₱{sale.Total:N2}";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error processing sale: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: /Sale/Orders
        public async Task<IActionResult> Orders(string searchString, string status, int page = 1)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Login", "Account");

            int pageSize = 10;
            var sales = _context.Sales.Include(s => s.Items).AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                sales = sales.Where(s =>
                    s.CustomerName.Contains(searchString) ||
                    s.SaleId.ToString().Contains(searchString));
                ViewData["SearchString"] = searchString;
            }

            if (!string.IsNullOrEmpty(status))
            {
                sales = sales.Where(s => s.Status == status);
                ViewData["SelectedStatus"] = status;
            }

            int totalItems = await sales.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            page = Math.Max(1, Math.Min(page, Math.Max(1, totalPages)));

            var pagedSales = await sales
                .OrderByDescending(s => s.Date)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"] = totalPages;
            ViewData["TotalItems"] = totalItems;
            ViewData["CompletedCount"] = await _context.Sales.CountAsync(s => s.Status == "Completed");
            ViewData["PendingCount"] = await _context.Sales.CountAsync(s => s.Status == "Pending");
            ViewData["RefundedCount"] = await _context.Sales.CountAsync(s => s.Status == "Refunded");

            return View(pagedSales);
        }

        // GET: /Sale/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var sale = await _context.Sales
                .Include(s => s.Items)
                .FirstOrDefaultAsync(s => s.SaleId == id);

            if (sale == null) return NotFound();

            return Json(new
            {
                sale.SaleId,
                Date = sale.Date.ToString("MMM dd, yyyy"),
                sale.CustomerName,
                sale.PaymentMethod,
                sale.Status,
                Total = sale.Total.ToString("N2"),
                Items = sale.Items.Select(i => new
                {
                    i.ProductName,
                    i.Quantity,
                    Price = i.Price.ToString("N2"),
                    Subtotal = (i.Price * i.Quantity).ToString("N2")
                })
            });
        }

        // POST: /Sale/UpdateStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var sale = await _context.Sales.FindAsync(id);
            if (sale != null)
            {
                sale.Status = status;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Order #{sale.SaleId} marked as {status}";
            }
            return RedirectToAction(nameof(Orders));
        }

        // POST: /Sale/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var sale = await _context.Sales.Include(s => s.Items).FirstOrDefaultAsync(s => s.SaleId == id);
            if (sale != null)
            {
                _context.SaleItems.RemoveRange(sale.Items);
                _context.Sales.Remove(sale);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Order #{id} deleted";
            }
            return RedirectToAction(nameof(Orders));
        }
    }

    public class CartItem
    {
        public string ProductName { get; set; } = "";
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
