using EcommerceAPI.Data;
using EcommerceAPI.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcommerceAPI.Controllers.UserController
{
    [ApiController]
    [Route("api/user/cart")]
    public class UserCartController : ControllerBase
    {
        private readonly EcommerceDbContext _context;
        private readonly ILogger<UserCartController> _logger;

        public UserCartController(EcommerceDbContext context, ILogger<UserCartController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpPost("calculate")]
        [AllowAnonymous]
        public async Task<ActionResult<ResponseDto<CartTotalsDto>>> Calculate([FromBody] CalculateCartDto dto)
        {
            try
            {
                if (dto.Items == null || dto.Items.Count == 0)
                {
                    return Ok(new ResponseDto<CartTotalsDto>(200, "Cart totals calculated successfully", new CartTotalsDto()));
                }

                var productIds = dto.Items.Select(i => i.ProductId).Distinct().ToList();
                var products = await _context.Products
                    .Where(p => productIds.Contains(p.Id))
                    .ToDictionaryAsync(p => p.Id);

                var totals = new CartTotalsDto();

                foreach (var item in dto.Items)
                {
                    if (item.Quantity <= 0)
                    {
                        return BadRequest(new ResponseDto(400, "Cart item quantity must be greater than 0", false));
                    }

                    if (!products.TryGetValue(item.ProductId, out var product))
                    {
                        return NotFound(new ResponseDto(404, $"Product with ID {item.ProductId} not found", false));
                    }

                    var totalPrice = product.Price * item.Quantity;
                    totals.Items.Add(new CartTotalItemDto
                    {
                        ProductId = product.Id,
                        Quantity = item.Quantity,
                        UnitPrice = product.Price,
                        TotalPrice = totalPrice
                    });
                }

                totals.Subtotal = totals.Items.Sum(i => i.TotalPrice);
                return Ok(new ResponseDto<CartTotalsDto>(200, "Cart totals calculated successfully", totals));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating cart totals");
                return StatusCode(500, new ResponseDto(500, "Internal server error", false));
            }
        }
    }
}
