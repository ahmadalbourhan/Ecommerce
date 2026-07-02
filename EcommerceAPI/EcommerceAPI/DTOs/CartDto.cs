using System.ComponentModel.DataAnnotations;

namespace EcommerceAPI.DTOs
{
    public class CalculateCartDto
    {
        [Required, MinLength(1)]
        public List<CalculateCartItemDto> Items { get; set; } = new();
    }

    public class CalculateCartItemDto
    {
        [Required]
        public int ProductId { get; set; }

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }
    }

    public class CartTotalsDto
    {
        public decimal Subtotal { get; set; }
        public List<CartTotalItemDto> Items { get; set; } = new();
    }

    public class CartTotalItemDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
