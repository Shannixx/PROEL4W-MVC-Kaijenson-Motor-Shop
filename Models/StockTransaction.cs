using System.ComponentModel.DataAnnotations;

namespace PROEL4W_MVC_Kaijenson_Motor_Shop.Models
{
    public class StockTransaction
    {
        [Key]
        public int StockTransactionId { get; set; }

        [Required]
        [Display(Name = "Date")]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; } = DateTime.Now;

        [StringLength(10)]
        [Display(Name = "Time")]
        public string Time { get; set; } = DateTime.Now.ToString("HH:mm");

        [Required]
        [StringLength(100)]
        [Display(Name = "Product Name")]
        public string ProductName { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        [Display(Name = "Type")]
        public string Type { get; set; } = "stock-in"; // stock-in or stock-out

        [Required]
        [Range(1, 999999)]
        public int Quantity { get; set; }

        [StringLength(50)]
        [Display(Name = "Reference")]
        public string? Reference { get; set; }

        [StringLength(100)]
        [Display(Name = "Performed By")]
        public string? PerformedBy { get; set; }
    }
}
