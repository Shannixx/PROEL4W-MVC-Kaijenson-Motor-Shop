using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PROEL4W_MVC_Kaijenson_Motor_Shop.Data;
using PROEL4W_MVC_Kaijenson_Motor_Shop.Models;

namespace PROEL4W_MVC_Kaijenson_Motor_Shop.Controllers
{
    public class SupplierController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SupplierController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string searchString, int page = 1)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Login", "Account");

            int pageSize = 10;
            var suppliers = _context.Suppliers.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                suppliers = suppliers.Where(s =>
                    s.Name.Contains(searchString) ||
                    (s.Contact != null && s.Contact.Contains(searchString)) ||
                    (s.Categories != null && s.Categories.Contains(searchString)));
                ViewData["SearchString"] = searchString;
            }

            int totalItems = await suppliers.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            page = Math.Max(1, Math.Min(page, Math.Max(1, totalPages)));

            var pagedSuppliers = await suppliers
                .OrderBy(s => s.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"] = totalPages;
            ViewData["TotalItems"] = totalItems;

            return View(pagedSuppliers);
        }

        public IActionResult Create()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Login", "Account");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Supplier supplier)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Login", "Account");

            if (ModelState.IsValid)
            {
                supplier.CreatedAt = DateTime.Now;
                _context.Suppliers.Add(supplier);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Supplier \"{supplier.Name}\" added successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(supplier);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Login", "Account");

            if (id == null) return NotFound();
            var supplier = await _context.Suppliers.FindAsync(id);
            if (supplier == null) return NotFound();
            return View(supplier);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Supplier supplier)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Login", "Account");

            if (id != supplier.SupplierId) return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(supplier);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Supplier updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(supplier);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Login", "Account");

            var supplier = await _context.Suppliers.FindAsync(id);
            if (supplier != null)
            {
                _context.Suppliers.Remove(supplier);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Supplier \"{supplier.Name}\" deleted!";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
