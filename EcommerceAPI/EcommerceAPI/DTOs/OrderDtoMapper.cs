using EcommerceAPI.Models;

namespace EcommerceAPI.DTOs
{
    public static class OrderDtoMapper
    {
        public static OrderDto ToDto(Order order)
        {
            return new OrderDto
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                UserId = order.UserId,
                Status = order.Status,
                Total = order.Total,
                OrderedAt = order.OrderedAt,
                AcceptedAt = order.AcceptedAt,
                DeliveredAt = order.DeliveredAt,
                PaymentMethod = order.PaymentMethod,
                User = order.User == null
                    ? null
                    : new UserBasicDto
                    {
                        Id = order.User.Id,
                        Username = order.User.Username,
                        PhoneNumber = order.User.PhoneNumber
                    },
                Items = order.OrderItems?.Select(item => new OrderItemDto
                {
                    Id = item.Id,
                    OrderId = item.OrderId,
                    ProductId = item.ProductId,
                    ProductName = item.Product?.Name ?? string.Empty,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    TotalPrice = item.TotalPrice,
                    CreatedAt = item.CreatedAt,
                    UpdatedAt = item.UpdatedAt
                }).ToList() ?? new List<OrderItemDto>()
            };
        }
    }
}
