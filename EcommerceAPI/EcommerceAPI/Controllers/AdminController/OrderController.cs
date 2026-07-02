using EcommerceAPI.Data;
using EcommerceAPI.DTOs;
using EcommerceAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcommerceAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly EcommerceDbContext _context;
        private readonly ILogger<OrderController> _logger;

        public OrderController(IOrderService orderService, EcommerceDbContext context, ILogger<OrderController> logger)
        {
            _orderService = orderService;
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ResponseDto<IEnumerable<OrderDto>>>> GetAll()
        {
            try
            {
                var orders = await _orderService.GetAllOrdersAsync();
                var dtos = orders.Select(OrderDtoMapper.ToDto);
                return Ok(new ResponseDto<IEnumerable<OrderDto>>(200, "Orders retrieved successfully", dtos));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving orders");
                return StatusCode(500, new ResponseDto(500, "Internal server error", false));
            }
        }

        [HttpGet("paged")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ResponseDto<PagedResultDto<OrderDto>>>> GetPaged(
            [FromQuery] string? search,
            [FromQuery] string? status,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                page = Math.Max(1, page);
                pageSize = Math.Clamp(pageSize, 1, 50);

                var query = _context.Orders
                    .AsNoTracking()
                    .Include(o => o.User)
                    .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(status) && !status.Equals("all", StringComparison.OrdinalIgnoreCase))
                {
                    var normalizedStatus = status.Trim();
                    query = query.Where(o => o.Status == normalizedStatus);
                }

                if (!string.IsNullOrWhiteSpace(search))
                {
                    var normalizedSearch = search.Trim();
                    query = query.Where(o =>
                        o.OrderNumber.Contains(normalizedSearch) ||
                        o.Status.Contains(normalizedSearch) ||
                        o.PaymentMethod.Contains(normalizedSearch) ||
                        o.UserId.ToString().Contains(normalizedSearch) ||
                        (o.User != null && (
                            o.User.Username.Contains(normalizedSearch) ||
                            o.User.Email.Contains(normalizedSearch) ||
                            o.User.Name.Contains(normalizedSearch))) ||
                        o.OrderItems.Any(i => i.Product != null && i.Product.Name.Contains(normalizedSearch)));
                }

                query = query.OrderByDescending(o => o.OrderedAt).ThenByDescending(o => o.Id);

                var totalItems = await query.CountAsync();
                var orders = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var result = new PagedResultDto<OrderDto>
                {
                    Items = orders.Select(OrderDtoMapper.ToDto).ToList(),
                    Page = page,
                    PageSize = pageSize,
                    TotalItems = totalItems,
                    TotalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)pageSize)
                };

                return Ok(new ResponseDto<PagedResultDto<OrderDto>>(200, "Orders retrieved successfully", result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving paged orders");
                return StatusCode(500, new ResponseDto(500, "Internal server error", false));
            }
        }

        [HttpGet("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ResponseDto<OrderDto>>> GetById(int id)
        {
            try
            {
                var order = await _orderService.GetOrderByIdAsync(id);
                if (order == null)
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
                _logger.LogError(ex, "Error retrieving order");
                return StatusCode(500, new ResponseDto(500, "Internal server error", false));
            }
        }

        [HttpGet("user/{userId}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ResponseDto<IEnumerable<OrderDto>>>> GetByUserId(int userId)
        {
            try
            {
                var orders = await _orderService.GetOrdersByUserIdAsync(userId);
                var dtos = orders.Select(OrderDtoMapper.ToDto);
                return Ok(new ResponseDto<IEnumerable<OrderDto>>(200, "Orders retrieved successfully", dtos));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ResponseDto(400, ex.Message, false));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving orders by user");
                return StatusCode(500, new ResponseDto(500, "Internal server error", false));
            }
        }

        [HttpPost]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ResponseDto<OrderDto>>> Create([FromBody] CreateOrderDto dto)
        {
            try
            {
                if (dto == null)
                {
                    return BadRequest(new ResponseDto(400, "Order cannot be null", false));
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(new ResponseDto(400, "Invalid order data", false));
                }

                var order = await _orderService.CreateOrderAsync(dto, GetCurrentUserId());
                var orderDto = OrderDtoMapper.ToDto(order);
                return CreatedAtAction(nameof(GetById), new { id = orderDto.Id },
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
                _logger.LogError(ex, "Error creating order");
                return StatusCode(500, new ResponseDto(500, "Internal server error", false));
            }
        }

        [HttpPatch("{id}/status")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ResponseDto<OrderDto>>> UpdateStatus(int id, [FromBody] UpdateOrderStatusDto dto)
        {
            try
            {
                if (dto == null)
                {
                    return BadRequest(new ResponseDto(400, "Status cannot be null", false));
                }

                var order = await _orderService.UpdateOrderStatusAsync(id, dto.Status);
                return Ok(new ResponseDto<OrderDto>(200, "Order status updated successfully", OrderDtoMapper.ToDto(order)));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ResponseDto(400, ex.Message, false));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ResponseDto(404, ex.Message, false));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order status");
                return StatusCode(500, new ResponseDto(500, "Internal server error", false));
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ResponseDto>> Delete(int id)
        {
            try
            {
                var success = await _orderService.DeleteOrderAsync(id);
                if (!success)
                {
                    return NotFound(new ResponseDto(404, $"Order with ID {id} not found", false));
                }

                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ResponseDto(400, ex.Message, false));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting order");
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
