using System.ComponentModel.DataAnnotations;

namespace PROEL4W_MVC_Kaijenson_Motor_Shop.Models
{
    public class Supplier
    {
        [Key]
        public int SupplierId { get; set; }

        [Required(ErrorMessage = "Supplier name is required")]
        [StringLength(100)]
        [Display(Name = "Supplier Name")]
        public string Name { get; set; } = string.Empty;

        [StringLength(100)]
        [Display(Name = "Contact Person")]
        public string? Contact { get; set; }

        [StringLength(20)]
        [Display(Name = "Phone Number")]
        public string? Phone { get; set; }

        [EmailAddress]
        [StringLength(100)]
        [Display(Name = "Email Address")]
        public string? Email { get; set; }

        [StringLength(200)]
        [Display(Name = "Categories")]
        public string? Categories { get; set; }

        [Display(Name = "Last Order")]
        [DataType(DataType.Date)]
        public DateTime? LastOrder { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Active";

        [Display(Name = "Date Added")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
