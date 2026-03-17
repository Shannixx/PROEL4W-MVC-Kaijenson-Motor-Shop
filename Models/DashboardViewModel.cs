namespace PROEL4W_MVC_Kaijenson_Motor_Shop.Models
{
    public class DashboardViewModel
    {
        public int TotalProducts { get; set; }
        public int InStockCount { get; set; }
        public int LowStockCount { get; set; }
        public int OutOfStockCount { get; set; }
        public int TotalUsers { get; set; }
        public List<Product> RecentProducts { get; set; } = new();
        public List<ActivityLog> RecentLogs { get; set; } = new();
    }
}
