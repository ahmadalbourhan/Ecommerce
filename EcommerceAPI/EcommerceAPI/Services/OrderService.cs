using EcommerceAPI.DTOs;
using EcommerceAPI.Models;
using EcommerceAPI.Repositories;
using EcommerceAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace EcommerceAPI.Services
{
    public class OrderService : IOrderService
    {
        public const string CashOnDelivery = "CashOnDelivery";

        private static readonly HashSet<string> AllowedStatuses = new(StringComparer.OrdinalIgnoreCase)
        {
            "Pending",
            "Accepted",
            "Delivered",
            "Cancelled"
        };

        private readonly IOrderRepository _orderRepository;
        private readonly EcommerceDbContext _context;

        public OrderService(IOrderRepository orderRepository, EcommerceDbContext context)
        {
            _orderRepository = orderRepository;
            _context = context;
        }

        public async Task<IEnumerable<Order>> GetAllOrdersAsync()
        {
            return await _orderRepository.GetAllAsync();
        }

        public async Task<Order?> GetOrderByIdAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Order ID must be greater than 0", nameof(id));

            return await _orderRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Order>> GetOrdersByUserIdAsync(int userId)
        {
            if (userId <= 0)
                throw new ArgumentException("User ID must be greater than 0", nameof(userId));

            return await _orderRepository.GetByUserIdAsync(userId);
        }

        public async Task<Order> CreateOrderAsync(CreateOrderDto dto, int fallbackUserId)
        {
            if (dto == null)
                throw new ArgumentException("Order cannot be null", nameof(dto));

            if (NormalizePaymentMethod(dto.PaymentMethod) != CashOnDelivery)
                throw new ArgumentException("Payment method must be CashOnDelivery", nameof(dto.PaymentMethod));

            if (dto.Items == null || dto.Items.Count == 0)
                throw new ArgumentException("Order must contain at least one item", nameof(dto.Items));

            var userId = dto.UserId.GetValueOrDefault(fallbackUserId);
            if (userId <= 0)
                throw new ArgumentException("Valid user ID is required", nameof(dto.UserId));

            var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
            if (!userExists)
                throw new KeyNotFoundException($"User with ID {userId} not found");

            var productIds = dto.Items.Select(i => i.ProductId).Distinct().ToList();
            var products = await _context.Products
                .Where(p => productIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id);

            var requestedQuantities = dto.Items
                .GroupBy(i => i.ProductId)
                .ToDictionary(g => g.Key, g => g.Sum(i => i.Quantity));

            foreach (var requestedItem in requestedQuantities)
            {
                if (!products.TryGetValue(requestedItem.Key, out var product))
                    throw new KeyNotFoundException($"Product with ID {requestedItem.Key} not found");

                if (requestedItem.Value <= 0)
                    throw new ArgumentException("Order item quantity must be greater than 0", nameof(dto.Items));

                if (product.Stock <= 0)
                    throw new InvalidOperationException($"Product '{product.Name}' is out of stock");

                if (requestedItem.Value > product.Stock)
                    throw new InvalidOperationException($"Only {product.Stock} item(s) left in stock for '{product.Name}'");
            }

            var now = DateTime.UtcNow;
            var order = new Order
            {
                OrderNumber = GenerateOrderNumber(),
                UserId = userId,
                Status = "Pending",
                OrderedAt = now,
                PaymentMethod = CashOnDelivery
            };

            foreach (var item in dto.Items)
            {
                if (item.Quantity <= 0)
                    throw new ArgumentException("Order item quantity must be greater than 0", nameof(item.Quantity));

                if (!products.TryGetValue(item.ProductId, out var product))
                    throw new KeyNotFoundException($"Product with ID {item.ProductId} not found");

                var totalPrice = product.Price * item.Quantity;
                product.Stock -= item.Quantity;
                product.UpdatedAt = now;
                _context.Entry(product).Property(p => p.Stock).IsModified = true;
                _context.Entry(product).Property(p => p.UpdatedAt).IsModified = true;

                order.OrderItems.Add(new OrderItem
                {
                    ProductId = product.Id,
                    Quantity = item.Quantity,
                    UnitPrice = product.Price,
                    TotalPrice = totalPrice,
                    CreatedAt = now,
                    UpdatedAt = now
                });
            }

            order.Total = order.OrderItems.Sum(i => i.TotalPrice);
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            return await _orderRepository.GetByIdAsync(order.Id) ?? order;
        }

        public async Task<Order> UpdateOrderStatusAsync(int id, string status)
        {
            if (id <= 0)
                throw new ArgumentException("Order ID must be greater than 0", nameof(id));

            if (string.IsNullOrWhiteSpace(status) || !AllowedStatuses.Contains(status))
                throw new ArgumentException("Status must be Pending, Accepted, Delivered, or Cancelled", nameof(status));

            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null)
                throw new KeyNotFoundException($"Order with ID {id} not found");

            var normalizedStatus = AllowedStatuses.First(s => string.Equals(s, status, StringComparison.OrdinalIgnoreCase));
            order.Status = normalizedStatus;

            if (normalizedStatus == "Accepted" && order.AcceptedAt == null)
                order.AcceptedAt = DateTime.UtcNow;

            if (normalizedStatus == "Delivered")
            {
                order.AcceptedAt ??= DateTime.UtcNow;
                order.DeliveredAt ??= DateTime.UtcNow;
            }

            return await _orderRepository.UpdateAsync(order);
        }

        public async Task<bool> DeleteOrderAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Order ID must be greater than 0", nameof(id));

            return await _orderRepository.DeleteAsync(id);
        }

        private static string GenerateOrderNumber()
        {
            return $"ORD-{DateTime.UtcNow:yyyyMMddHHmmss}-{Random.Shared.Next(1000, 9999)}";
        }

        private static string NormalizePaymentMethod(string? paymentMethod)
        {
            var value = (paymentMethod ?? string.Empty).Trim().Replace(" ", string.Empty).Replace("_", string.Empty).Replace("-", string.Empty);
            return value.Equals("CashOnDelivery", StringComparison.OrdinalIgnoreCase) ||
                value.Equals("COD", StringComparison.OrdinalIgnoreCase)
                ? CashOnDelivery
                : value;
        }
    }
}
