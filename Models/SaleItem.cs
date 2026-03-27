using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PROEL4W_MVC_Kaijenson_Motor_Shop.Models
{
    public class SaleItem
    {
        [Key]
        public int SaleItemId { get; set; }

        [Required]
        public int SaleId { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Product Name")]
        public string ProductName { get; set; } = string.Empty;

        [Required]
        [Range(1, 999999)]
        public int Quantity { get; set; }

        [Required]
        [Range(0.01, 999999.99)]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        // Navigation
        [ForeignKey("SaleId")]
        public Sale? Sale { get; set; }
    }
}
