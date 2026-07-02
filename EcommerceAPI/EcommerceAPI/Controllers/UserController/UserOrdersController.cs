using EcommerceAPI.DTOs;
using EcommerceAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceAPI.Controllers.UserController
{
    [ApiController]
    [Route("api/user/orders")]
    [Authorize]
    public class UserOrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<UserOrdersController> _logger;

        public UserOrdersController(IOrderService orderService, ILogger<UserOrdersController> logger)
        {
            _orderService = orderService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<ResponseDto<IEnumerable<OrderDto>>>> GetMyOrders()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId <= 0)
                {
                    return Unauthorized(new ResponseDto(401, "Please sign in again.", false));
                }

                var orders = await _orderService.GetOrdersByUserIdAsync(userId);
                return Ok(new ResponseDto<IEnumerable<OrderDto>>(200, "Orders retrieved successfully", orders.Select(OrderDtoMapper.ToDto)));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user orders");
                return StatusCode(500, new ResponseDto(500, "Internal server error", false));
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ResponseDto<OrderDto>>> TrackOrder(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId <= 0)
                {
                    return Unauthorized(new ResponseDto(401, "Please sign in again.", false));
                }

                var order = await _orderService.GetOrderByIdAsync(id);
                if (order == null || order.UserId != userId)
                {
                    return NotFound(new ResponseDto(404, $"Order with ID {id} not found", false));
                }

                return Ok(new ResponseDto<OrderDto>(200, "Order retrieved successfully", OrderDtoMapper.ToDto(order)));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ResponseDto(400, ex.Message, false));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error tracking user order");
                return StatusCode(500, new ResponseDto(500, "Internal server error", false));
            }
        }

        [HttpPost]
        public async Task<ActionResult<ResponseDto<OrderDto>>> Checkout([FromBody] CreateOrderDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId <= 0)
                {
                    return Unauthorized(new ResponseDto(401, "Please sign in to checkout.", false));
                }

                var checkoutDto = new CreateOrderDto
                {
                    UserId = userId,
                    Items = dto.Items,
                    PaymentMethod = "CashOnDelivery"
                };

                var order = await _orderService.CreateOrderAsync(checkoutDto, userId);
                var orderDto = OrderDtoMapper.ToDto(order);
                return CreatedAtAction(nameof(TrackOrder), new { id = orderDto.Id },
                    new ResponseDto<OrderDto>(201, "Order created successfully", orderDto));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ResponseDto(400, ex.Message, false));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ResponseDto(400, ex.Message, false));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ResponseDto(404, ex.Message, false));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user order");
                return StatusCode(500, new ResponseDto(500, "Internal server error", false));
            }
        }

        private int GetCurrentUserId()
        {
            var userIdClaim =
                User.FindFirst("id")?.Value ??
                User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ??
                User.FindFirst("nameid")?.Value ??
                User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value ??
                User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }
    }
}
