using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PROEL4W_MVC_Kaijenson_Motor_Shop.Data;
using PROEL4W_MVC_Kaijenson_Motor_Shop.Models;
using PROEL4W_MVC_Kaijenson_Motor_Shop.Services;

namespace PROEL4W_MVC_Kaijenson_Motor_Shop.Controllers
{
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public UserController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // GET: /User
        public async Task<IActionResult> Index(string searchString, int page = 1)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Login", "Account");

            // Admin only
            if (HttpContext.Session.GetString("UserRole") != "Admin")
            {
                TempData["ErrorMessage"] = "Access denied. Admin privileges required.";
                return RedirectToAction("Index", "Dashboard");
            }

            int pageSize = 10;
            var users = _context.Users.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                users = users.Where(u =>
                    u.FirstName.Contains(searchString) ||
                    u.LastName.Contains(searchString) ||
                    u.Email.Contains(searchString));
                ViewData["SearchString"] = searchString;
            }

            int totalItems = await users.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            page = Math.Max(1, Math.Min(page, Math.Max(1, totalPages)));

            var pagedUsers = await users
                .OrderByDescending(u => u.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"] = totalPages;
            ViewData["TotalItems"] = totalItems;

            return View(pagedUsers);
        }

        // GET: /User/Create
        public IActionResult Create()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Login", "Account");

            if (HttpContext.Session.GetString("UserRole") != "Admin")
            {
                TempData["ErrorMessage"] = "Access denied. Admin privileges required.";
                return RedirectToAction("Index", "Dashboard");
            }

            return View();
        }

        // POST: /User/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RegisterViewModel model)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Login", "Account");

            if (HttpContext.Session.GetString("UserRole") != "Admin")
            {
                TempData["ErrorMessage"] = "Access denied.";
                return RedirectToAction("Index", "Dashboard");
            }

            if (!ModelState.IsValid)
                return View(model);

            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Username);
            if (existingUser != null)
            {
                ModelState.AddModelError("Username", "This username is already taken.");
                return View(model);
            }

            var user = new User
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Username,
                Password = HashingService.HashData(model.Password),
                Role = "Manager",
                CreatedAt = DateTime.Now
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var adminId = HttpContext.Session.GetInt32("UserId");
            _context.ActivityLogs.Add(new ActivityLog
            {
                UserId = adminId,
                Action = "Create User",
                Details = $"Created new user: {user.FullName}",
                Timestamp = DateTime.Now
            });
            await _context.SaveChangesAsync();

            // Notify about new user
            await NotificationController.NotifyAdmins(_context,
                "system", "New User Created",
                $"A new {user.Role} account was created for {user.FullName}");

            TempData["SuccessMessage"] = "User created successfully!";
            return RedirectToAction(nameof(Index));
        }

        // GET: /User/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Login", "Account");

            if (HttpContext.Session.GetString("UserRole") != "Admin")
            {
                TempData["ErrorMessage"] = "Access denied.";
                return RedirectToAction("Index", "Dashboard");
            }

            if (id == null) return NotFound();

            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            return View(user);
        }

        // POST: /User/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, User user)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Login", "Account");

            if (HttpContext.Session.GetString("UserRole") != "Admin")
            {
                TempData["ErrorMessage"] = "Access denied.";
                return RedirectToAction("Index", "Dashboard");
            }

            if (id != user.UserId) return NotFound();

            // Remove Password from validation as we don't update it here
            ModelState.Remove("Password");

            if (ModelState.IsValid)
            {
                try
                {
                    var existingUser = await _context.Users.AsNoTracking()
                        .FirstOrDefaultAsync(u => u.UserId == id);
                    if (existingUser == null) return NotFound();

                    user.Password = existingUser.Password;
                    user.ProfileImage = existingUser.ProfileImage;
                    user.CreatedAt = existingUser.CreatedAt;

                    _context.Update(user);
                    await _context.SaveChangesAsync();

                    var adminId = HttpContext.Session.GetInt32("UserId");
                    _context.ActivityLogs.Add(new ActivityLog
                    {
                        UserId = adminId,
                        Action = "Edit User",
                        Details = $"Updated user: {user.FullName}",
                        Timestamp = DateTime.Now
                    });
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "User updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Users.Any(u => u.UserId == user.UserId))
                        return NotFound();
                    else
                        throw;
                }

                return RedirectToAction(nameof(Index));
            }

            return View(user);
        }

        // GET: /User/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Login", "Account");

            if (HttpContext.Session.GetString("UserRole") != "Admin")
            {
                TempData["ErrorMessage"] = "Access denied.";
                return RedirectToAction("Index", "Dashboard");
            }

            if (id == null) return NotFound();

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == id);
            if (user == null) return NotFound();

            return View(user);
        }

        // POST: /User/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Login", "Account");

            if (HttpContext.Session.GetString("UserRole") != "Admin")
            {
                TempData["ErrorMessage"] = "Access denied.";
                return RedirectToAction("Index", "Dashboard");
            }

            var currentUserId = HttpContext.Session.GetInt32("UserId");
            if (id == currentUserId)
            {
                TempData["ErrorMessage"] = "You cannot delete your own account!";
                return RedirectToAction(nameof(Index));
            }

            // Protect original admin
            if (id == 1)
            {
                TempData["ErrorMessage"] = "The original admin account cannot be deleted.";
                return RedirectToAction(nameof(Index));
            }

            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                string userName = user.FullName;
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                _context.ActivityLogs.Add(new ActivityLog
                {
                    UserId = currentUserId,
                    Action = "Delete User",
                    Details = $"Deleted user: {userName}",
                    Timestamp = DateTime.Now
                });
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "User deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: /User/Profile
        public async Task<IActionResult> Profile()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            return View(user);
        }

        // POST: /User/Profile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(User user, IFormFile? imageFile)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            if (user.UserId != userId) return Forbid();

            ModelState.Remove("Password");

            if (ModelState.IsValid)
            {
                var existingUser = await _context.Users.AsNoTracking()
                    .FirstOrDefaultAsync(u => u.UserId == userId);
                if (existingUser == null) return NotFound();

                user.Password = existingUser.Password;
                user.CreatedAt = existingUser.CreatedAt;
                user.Role = existingUser.Role;

                // Handle profile image upload
                if (imageFile != null && imageFile.Length > 0)
                {
                    var uploadsDir = Path.Combine(_environment.WebRootPath, "uploads", "profiles");
                    if (!Directory.Exists(uploadsDir))
                        Directory.CreateDirectory(uploadsDir);

                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                    var filePath = Path.Combine(uploadsDir, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }

                    user.ProfileImage = "/uploads/profiles/" + fileName;
                }
                else
                {
                    user.ProfileImage = existingUser.ProfileImage;
                }

                _context.Update(user);
                await _context.SaveChangesAsync();

                // Update session
                HttpContext.Session.SetString("UserName", user.FullName);
                HttpContext.Session.SetString("UserEmail", user.Email);
                HttpContext.Session.SetString("UserImage", user.ProfileImage ?? "");

                _context.ActivityLogs.Add(new ActivityLog
                {
                    UserId = userId,
                    Action = "Update Profile",
                    Details = $"{user.FullName} updated their profile",
                    Timestamp = DateTime.Now
                });
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Profile updated successfully!";
                return RedirectToAction("Profile");
            }

            return View(user);
        }

        // GET: /User/ChangePassword
        public IActionResult ChangePassword()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Login", "Account");

            return View();
        }

        // POST: /User/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
                return View(model);

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            string hashedCurrent = HashingService.HashData(model.CurrentPassword);
            if (user.Password != hashedCurrent)
            {
                ModelState.AddModelError("CurrentPassword", "Current password is incorrect.");
                return View(model);
            }

            user.Password = HashingService.HashData(model.NewPassword);
            _context.Update(user);
            await _context.SaveChangesAsync();

            _context.ActivityLogs.Add(new ActivityLog
            {
                UserId = userId,
                Action = "Change Password",
                Details = $"{user.FullName} changed their password",
                Timestamp = DateTime.Now
            });
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Password changed successfully!";
            return RedirectToAction("Profile");
        }

        // POST: /User/ChangeRole/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeRole(int id, string newRole)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Login", "Account");

            if (HttpContext.Session.GetString("UserRole") != "Admin")
            {
                TempData["ErrorMessage"] = "Access denied. Admin privileges required.";
                return RedirectToAction("Index", "Dashboard");
            }

            // Validate role
            if (newRole != "Admin" && newRole != "Manager")
            {
                TempData["ErrorMessage"] = "Invalid role specified.";
                return RedirectToAction(nameof(Index));
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            // Protect original admin from demotion
            if (id == 1 && newRole != "Admin")
            {
                TempData["ErrorMessage"] = "The original admin account cannot be demoted.";
                return RedirectToAction(nameof(Index));
            }

            // Prevent changing own role
            var currentUserId = HttpContext.Session.GetInt32("UserId");
            if (id == currentUserId)
            {
                TempData["ErrorMessage"] = "You cannot change your own role!";
                return RedirectToAction(nameof(Index));
            }

            string oldRole = user.Role;
            user.Role = newRole;
            _context.Update(user);
            await _context.SaveChangesAsync();

            _context.ActivityLogs.Add(new ActivityLog
            {
                UserId = currentUserId,
                Action = "Change Role",
                Details = $"Changed {user.FullName}'s role from {oldRole} to {newRole}",
                Timestamp = DateTime.Now
            });
            await _context.SaveChangesAsync();

            // Notify about role change
            await NotificationController.CreateNotification(_context, user.UserId,
                "system", "Role Updated",
                $"Your role has been changed from {oldRole} to {newRole}");

            TempData["SuccessMessage"] = $"{user.FullName}'s role changed to {newRole}!";
            return RedirectToAction(nameof(Index));
        }
    }
}
