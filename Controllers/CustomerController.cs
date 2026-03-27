using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PROEL4W_MVC_Kaijenson_Motor_Shop.Data;
using PROEL4W_MVC_Kaijenson_Motor_Shop.Models;

namespace PROEL4W_MVC_Kaijenson_Motor_Shop.Controllers
{
    public class CustomerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CustomerController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Customer
        public async Task<IActionResult> Index(string searchString, int page = 1)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Login", "Account");

            int pageSize = 10;
            var customers = _context.Customers.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                customers = customers.Where(c =>
                    c.Name.Contains(searchString) ||
                    (c.Email != null && c.Email.Contains(searchString)) ||
                    (c.Phone != null && c.Phone.Contains(searchString)));
                ViewData["SearchString"] = searchString;
            }

            int totalItems = await customers.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            page = Math.Max(1, Math.Min(page, Math.Max(1, totalPages)));

            var pagedCustomers = await customers
                .OrderByDescending(c => c.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"] = totalPages;
            ViewData["TotalItems"] = totalItems;

            return View(pagedCustomers);
        }

        // GET: /Customer/Create
        public IActionResult Create()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Login", "Account");
            return View();
        }

        // POST: /Customer/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Customer customer)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Login", "Account");

            if (ModelState.IsValid)
            {
                customer.CreatedAt = DateTime.Now;
                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Customer \"{customer.Name}\" added successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(customer);
        }

        // GET: /Customer/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Login", "Account");

            if (id == null) return NotFound();
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null) return NotFound();
            return View(customer);
        }

        // POST: /Customer/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Customer customer)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Login", "Account");

            if (id != customer.CustomerId) return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(customer);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Customer updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(customer);
        }

        // POST: /Customer/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Login", "Account");

            var customer = await _context.Customers.FindAsync(id);
            if (customer != null)
            {
                _context.Customers.Remove(customer);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Customer \"{customer.Name}\" deleted!";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
