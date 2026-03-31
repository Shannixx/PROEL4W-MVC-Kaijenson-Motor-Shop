using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PROEL4W_MVC_Kaijenson_Motor_Shop.Models
{
    public class NotificationPreference
    {
        [Key]
        public int PreferenceId { get; set; }

        [Required]
        public int UserId { get; set; }

        public bool LowStockAlerts { get; set; } = true;

        public bool NewOrderNotifications { get; set; } = true;

        public bool DailySalesSummary { get; set; } = false;

        public bool SystemUpdates { get; set; } = true;

        public bool CustomerActivity { get; set; } = false;

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
    }
}
