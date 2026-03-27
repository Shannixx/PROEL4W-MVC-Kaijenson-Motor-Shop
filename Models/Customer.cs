using System.ComponentModel.DataAnnotations;

namespace PROEL4W_MVC_Kaijenson_Motor_Shop.Models
{
    public class Customer
    {
        [Key]
        public int CustomerId { get; set; }

        [Required(ErrorMessage = "Customer name is required")]
        [StringLength(100)]
        [Display(Name = "Full Name")]
        public string Name { get; set; } = string.Empty;

        [StringLength(20)]
        [Display(Name = "Phone Number")]
        public string? Phone { get; set; }

        [EmailAddress]
        [StringLength(100)]
        [Display(Name = "Email Address")]
        public string? Email { get; set; }

        [Display(Name = "Total Purchases")]
        [DataType(DataType.Currency)]
        public decimal TotalPurchases { get; set; }

        [Display(Name = "Last Visit")]
        [DataType(DataType.Date)]
        public DateTime? LastVisit { get; set; }

        [Display(Name = "Date Added")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
