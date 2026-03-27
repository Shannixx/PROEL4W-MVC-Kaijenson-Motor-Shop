using System.ComponentModel.DataAnnotations;

namespace PROEL4W_MVC_Kaijenson_Motor_Shop.Models
{
    public class DashboardViewModel
    {
        // Product Stats
        public int TotalProducts { get; set; }
        public int InStockCount { get; set; }
        public int LowStockCount { get; set; }
        public int OutOfStockCount { get; set; }
        public int TotalUsers { get; set; }

        // Sales Stats
        public decimal TodaysSales { get; set; }
        public int TotalOrders { get; set; }
        public int TotalUnits { get; set; }
        public decimal StockValue { get; set; }

        // Lists
        public List<Product> RecentProducts { get; set; } = new();
        public List<ActivityLog> RecentLogs { get; set; } = new();
        public List<Product> LowStockAlerts { get; set; } = new();
        public List<Sale> RecentTransactions { get; set; } = new();

        // Chart Data (serialized as JSON)
        public string? MonthlySalesJson { get; set; }
        public string? CategorySalesJson { get; set; }
        public string? StockMovementJson { get; set; }
        public string? StockByCategoryJson { get; set; }
    }
}
