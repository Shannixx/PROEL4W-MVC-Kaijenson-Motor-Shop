using System.ComponentModel.DataAnnotations;

namespace PROEL4W_MVC_Kaijenson_Motor_Shop.Models
{
    public class Product
    {
        [Key]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Product name is required")]
        [StringLength(100, ErrorMessage = "Product name cannot exceed 100 characters")]
        [Display(Name = "Product Name")]
        public string ProductName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Category is required")]
        [StringLength(50, ErrorMessage = "Category cannot exceed 50 characters")]
        public string Category { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }

        [StringLength(30)]
        [Display(Name = "SKU")]
        public string? SKU { get; set; }

        [Range(0, 999999.99)]
        [DataType(DataType.Currency)]
        [Display(Name = "Cost (₱)")]
        public decimal Cost { get; set; }

        [Range(0, 999999)]
        [Display(Name = "Min Stock Level")]
        public int MinStock { get; set; } = 5;

        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, 999999.99, ErrorMessage = "Price must be between 0.01 and 999,999.99")]
        [DataType(DataType.Currency)]
        [Display(Name = "Price (₱)")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Stock quantity is required")]
        [Range(0, 999999, ErrorMessage = "Stock must be between 0 and 999,999")]
        [Display(Name = "Stock Quantity")]
        public int StockQuantity { get; set; }

        [StringLength(20)]
        public string Status { get; set; } = "In Stock";

        [StringLength(256)]
        [Display(Name = "Product Image")]
        public string? ImagePath { get; set; }

        [Display(Name = "Date Added")]
        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "Last Updated")]
        [DataType(DataType.DateTime)]
        public DateTime? UpdatedAt { get; set; }

        public void UpdateStatus()
        {
            if (StockQuantity <= 0)
                Status = "Out of Stock";
            else if (StockQuantity <= MinStock)
                Status = "Low Stock";
            else
                Status = "In Stock";
        }
    }
}
