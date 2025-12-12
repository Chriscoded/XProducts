using System.ComponentModel.DataAnnotations;

namespace XProducts.API.DTOs
{
    public class ProductUpdateDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = default!;
        public string? Description { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        [Range(0, int.MaxValue)]
        public int StockQuantity { get; set; }
    }

}
