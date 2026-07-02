using EcommerceAPI.Data;
using EcommerceAPI.DTOs;
using EcommerceAPI.Models;
using EcommerceAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcommerceAPI.Controllers.UserController
{
    [ApiController]
    [Route("api/user/products")]
    public class UserProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly EcommerceDbContext _context;
        private readonly ILogger<UserProductsController> _logger;

        public UserProductsController(IProductService productService, EcommerceDbContext context, ILogger<UserProductsController> logger)
        {
            _productService = productService;
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<ResponseDto<IEnumerable<ProductWithCategoryDto>>>> GetAll([FromQuery] string? search)
        {
            try
            {
                var products = string.IsNullOrWhiteSpace(search)
                    ? await _productService.GetAllProductsAsync()
                    : await _productService.SearchProductsAsync(search);

                return Ok(new ResponseDto<IEnumerable<ProductWithCategoryDto>>(
                    200,
                    "Products retrieved successfully",
                    products.Select(MapToProductWithCategoryDto)));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user products");
                return StatusCode(500, new ResponseDto(500, "Internal server error", false));
            }
        }

        [HttpGet("paged")]
        [AllowAnonymous]
        public async Task<ActionResult<ResponseDto<PagedResultDto<ProductWithCategoryDto>>>> GetPaged(
            [FromQuery] string? search,
            [FromQuery] int? categoryId,
            [FromQuery] string? sort,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 1)
        {
            try
            {
                page = Math.Max(1, page);
                pageSize = Math.Clamp(pageSize, 1, 48);

                var query = _context.Products
                    .AsNoTracking()
                    .Include(p => p.Category)
                    .Include(p => p.User)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(search))
                {
                    var normalizedSearch = search.Trim();
                    query = query.Where(p =>
                        p.Name.Contains(normalizedSearch) ||
                        (p.Category != null && p.Category.Name.Contains(normalizedSearch)));
                }

                var selectedCategoryId = categoryId.GetValueOrDefault();
                if (selectedCategoryId > 0)
                {
                    query = query.Where(p => p.CategoryId == selectedCategoryId);
                }

                query = (sort ?? string.Empty).Trim().ToLowerInvariant() switch
                {
                    "price_asc" => query.OrderBy(p => p.Price).ThenBy(p => p.Name),
                    "price_desc" => query.OrderByDescending(p => p.Price).ThenBy(p => p.Name),
                    "recent" => query.OrderByDescending(p => p.CreatedAt).ThenByDescending(p => p.Id),
                    _ => query.OrderBy(p => p.Name)
                };

                var totalItems = await query.CountAsync();
                var products = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var result = new PagedResultDto<ProductWithCategoryDto>
                {
                    Items = products.Select(MapToProductWithCategoryDto).ToList(),
                    Page = page,
                    PageSize = pageSize,
                    TotalItems = totalItems,
                    TotalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)pageSize)
                };

                return Ok(new ResponseDto<PagedResultDto<ProductWithCategoryDto>>(
                    200,
                    "Products retrieved successfully",
                    result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving paged user products");
                return StatusCode(500, new ResponseDto(500, "Internal server error", false));
            }
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<ResponseDto<ProductWithCategoryDto>>> GetById(int id)
        {
            try
            {
                var product = await _productService.GetProductByIdAsync(id);
                if (product == null)
                {
                    return NotFound(new ResponseDto(404, $"Product with ID {id} not found", false));
                }

                return Ok(new ResponseDto<ProductWithCategoryDto>(200, "Product retrieved successfully", MapToProductWithCategoryDto(product)));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ResponseDto(400, ex.Message, false));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user product");
                return StatusCode(500, new ResponseDto(500, "Internal server error", false));
            }
        }

        private static ProductWithCategoryDto MapToProductWithCategoryDto(Product product)
        {
            return new ProductWithCategoryDto
            {
                Id = product.Id,
                CategoryId = product.CategoryId,
                UserId = product.UserId,
                Name = product.Name,
                Cost = product.Cost,
                Price = product.Price,
                Stock = product.Stock,
                Image = product.Image,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt,
                Category = product.Category == null ? null : new CategoryBasicDto { Id = product.Category.Id, Name = product.Category.Name },
                User = product.User == null ? null : new UserBasicDto { Id = product.User.Id, Username = product.User.Username }
            };
        }
    }
}
