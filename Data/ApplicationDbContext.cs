using Microsoft.EntityFrameworkCore;
using PROEL4W_MVC_Kaijenson_Motor_Shop.Models;
using PROEL4W_MVC_Kaijenson_Motor_Shop.Services;

namespace PROEL4W_MVC_Kaijenson_Motor_Shop.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ActivityLog> ActivityLogs { get; set; }
        public DbSet<Sale> Sales { get; set; }
        public DbSet<SaleItem> SaleItems { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<StockTransaction> StockTransactions { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();

            modelBuilder.Entity<User>().HasData(new User
            {
                UserId = 1,
                FirstName = "Admin",
                LastName = "Kaijenson",
                Email = "admin123",
                Password = HashingService.HashData("admin123"),
                Role = "Admin",
                CreatedAt = new DateTime(2026, 1, 1)
            });
        }
    }
}
