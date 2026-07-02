using System.ComponentModel.DataAnnotations;

namespace EcommerceAPI.DTOs
{
    public class OrderDto
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public DateTime OrderedAt { get; set; }
        public DateTime? AcceptedAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public UserBasicDto? User { get; set; }
        public List<OrderItemDto> Items { get; set; } = new();
    }

    public class OrderItemDto
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateOrderDto
    {
        public int? UserId { get; set; }

        [Required, MinLength(1)]
        public List<CreateOrderItemDto> Items { get; set; } = new();

        public string PaymentMethod { get; set; } = "CashOnDelivery";
    }

    public class CreateOrderItemDto
    {
        [Required]
        public int ProductId { get; set; }

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }
    }

    public class UpdateOrderStatusDto
    {
        [Required, StringLength(50)]
        public string Status { get; set; } = string.Empty;
    }
}
