using System.ComponentModel.DataAnnotations;

namespace PROEL4W_MVC_Kaijenson_Motor_Shop.Models
{
    public class Sale
    {
        [Key]
        public int SaleId { get; set; }

        [Required]
        [Display(Name = "Sale Date")]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; } = DateTime.Now;

        [Required]
        [StringLength(100)]
        [Display(Name = "Customer Name")]
        public string CustomerName { get; set; } = string.Empty;

        [Required]
        [Range(0, 9999999.99)]
        [DataType(DataType.Currency)]
        public decimal Total { get; set; }

        [Required]
        [StringLength(30)]
        [Display(Name = "Payment Method")]
        public string PaymentMethod { get; set; } = "Cash";

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Completed";

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation
        public ICollection<SaleItem> Items { get; set; } = new List<SaleItem>();
    }
}
