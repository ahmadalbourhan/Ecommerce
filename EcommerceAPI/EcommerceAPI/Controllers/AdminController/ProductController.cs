using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcommerceAPI.Models;
using EcommerceAPI.Services;
using EcommerceAPI.DTOs;
using EcommerceAPI.Data;
using Microsoft.AspNetCore.Authorization;

namespace EcommerceAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly EcommerceDbContext _context;
        private readonly ILogger<ProductController> _logger;
        private readonly IWebHostEnvironment _environment;

        public ProductController(
            IProductService productService,
            EcommerceDbContext context,
            ILogger<ProductController> logger,
            IWebHostEnvironment environment)
        {
            _productService = productService;
            _context = context;
            _logger = logger;
            _environment = environment;
        }

        /// <summary>
        /// Retrieve all products
        /// </summary>
        /// <returns>List of all products</returns>
        /// <response code="200">Returns the list of products</response>
        /// <response code="500">Internal server error</response>
        [HttpGet]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ResponseDto<IEnumerable<ProductWithCategoryDto>>>> GetAll([FromQuery] string? search)
        {
            try
            {
                var products = string.IsNullOrWhiteSpace(search)
                    ? await _productService.GetAllProductsAsync()
                    : await _productService.SearchProductsAsync(search);
                var dtos = products.Select(MapToProductWithCategoryDto);
                return Ok(new ResponseDto<IEnumerable<ProductWithCategoryDto>>(200, "Products retrieved successfully", dtos));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all products");
                return StatusCode(500, new ResponseDto(500, "Internal server error", false));
            }
        }

        /// <summary>
        /// Retrieve products by user ID
        /// </summary>
        /// <param name="userId">The unique identifier of the user to retrieve products for</param>
        /// <returns>List of products for the specified user</returns>
        /// <response code="200">Returns the list of user's products</response>
        /// <response code="400">Invalid user ID</response>
        /// <response code="500">Internal server error</response>
        [HttpGet("user/{userId}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ResponseDto<IEnumerable<ProductWithCategoryDto>>>> GetByUserId(int userId)
        {
            try
            {
                var products = await _productService.GetProductsByUserIdAsync(userId);
                var dtos = products.Select(MapToProductWithCategoryDto);
                return Ok(new ResponseDto<IEnumerable<ProductWithCategoryDto>>(200, "Products retrieved successfully", dtos));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument when retrieving products by user");
                return BadRequest(new ResponseDto(400, ex.Message, false));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving products by user");
                return StatusCode(500, new ResponseDto(500, "Internal server error", false));
            }
        }

        /// <summary>
        /// Retrieve a specific product by ID
        /// </summary>
        /// <param name="id">The unique identifier of the product to retrieve</param>
        /// <returns>The product with the specified ID</returns>
        /// <response code="200">Returns the product</response>
        /// <response code="400">Invalid product ID</response>
        /// <response code="404">Product not found</response>
        /// <response code="500">Internal server error</response>
        [HttpGet("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ResponseDto<ProductWithCategoryDto>>> GetById(int id)
        {
            try
            {
                var product = await _productService.GetProductByIdAsync(id);
                if (product == null)
                {
                    return NotFound(new ResponseDto(404, $"Product with ID {id} not found", false));
                }
                var dto = MapToProductWithCategoryDto(product);
                return Ok(new ResponseDto<ProductWithCategoryDto>(200, "Product retrieved successfully", dto));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument when retrieving product");
                return BadRequest(new ResponseDto(400, ex.Message, false));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving product");
                return StatusCode(500, new ResponseDto(500, "Internal server error", false));
            }
        }

        /// <summary>
        /// Create a new product
        /// </summary>
        /// <param name="productDto">Product data to create</param>
        /// <returns>The created product</returns>
        /// <response code="201">Product created successfully</response>
        /// <response code="400">Invalid product data or category not found</response>
        /// <response code="500">Internal server error</response>
        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ResponseDto<ProductWithCategoryDto>>> Create([FromBody] DTOs.CreateProductDto productDto)
        {
            try
            {
                if (productDto == null)
                {
                    return BadRequest(new ResponseDto(400, "Product cannot be null", false));
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(new ResponseDto(400, "Invalid product data", false));
                }

                var product = new Product
                {
                    Name = productDto.Name,
                    Description = string.IsNullOrWhiteSpace(productDto.Description) ? null : productDto.Description.Trim(),
                    Cost = productDto.Cost,
                    Price = productDto.Price,
                    Stock = productDto.Stock,
                    CategoryId = productDto.CategoryId,
                    Image = productDto.Image ?? string.Empty,
                };

                var resolvedUserId = GetCurrentUserId();

                if (resolvedUserId <= 0)
                {
                    return BadRequest(new ResponseDto(400, "Could not identify the logged-in user. Please sign in again.", false));
                }

                product.UserId = resolvedUserId;

                var createdProduct = await _productService.CreateProductAsync(product);
                var createdDto = MapToProductWithCategoryDto(createdProduct);
                return CreatedAtAction(nameof(GetById), new { id = createdDto.Id },
                    new ResponseDto<ProductWithCategoryDto>(201, "Product created successfully", createdDto));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument when creating product");
                return BadRequest(new ResponseDto(400, ex.Message, false));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation when creating product - category not found");
                return BadRequest(new ResponseDto(400, ex.Message, false));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");
                return StatusCode(500, new ResponseDto(500, "Internal server error", false));
            }
        }

        [HttpPost("upload-image")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ResponseDto<ProductImageUploadDto>>> UploadImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new ResponseDto(400, "Please choose an image to upload.", false));
            }

            const long maxFileSize = 5 * 1024 * 1024;
            if (file.Length > maxFileSize)
            {
                return BadRequest(new ResponseDto(400, "Image size must be 5 MB or less.", false));
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var allowedExtensions = new HashSet<string> { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
            if (!allowedExtensions.Contains(extension))
            {
                return BadRequest(new ResponseDto(400, "Only JPG, PNG, WebP, and GIF images are allowed.", false));
            }

            var uploadsPath = Path.Combine(_environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot"), "uploads", "products");
            Directory.CreateDirectory(uploadsPath);

            var fileName = $"{Guid.NewGuid():N}{extension}";
            var filePath = Path.Combine(uploadsPath, fileName);

            await using (var stream = System.IO.File.Create(filePath))
            {
                await file.CopyToAsync(stream);
            }

            var publicPath = $"/uploads/products/{fileName}";
            return Ok(new ResponseDto<ProductImageUploadDto>(200, "Image uploaded successfully", new ProductImageUploadDto
            {
                ImagePath = publicPath
            }));
        }

        [HttpGet("paged")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ResponseDto<PagedResultDto<ProductWithCategoryDto>>>> GetPaged(
            [FromQuery] string? search,
            [FromQuery] int? categoryId,
            [FromQuery] string? sort,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                page = Math.Max(1, page);
                pageSize = Math.Clamp(pageSize, 1, 50);

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
                        (p.Description != null && p.Description.Contains(normalizedSearch)) ||
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
                    "stock_asc" => query.OrderBy(p => p.Stock).ThenBy(p => p.Name),
                    "stock_desc" => query.OrderByDescending(p => p.Stock).ThenBy(p => p.Name),
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

                return Ok(new ResponseDto<PagedResultDto<ProductWithCategoryDto>>(200, "Products retrieved successfully", result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving paged products");
                return StatusCode(500, new ResponseDto(500, "Internal server error", false));
            }
        }

        /// <summary>
        /// Update an existing product
        /// </summary>
        /// <param name="id">The unique identifier of the product to update</param>
        /// <param name="productDto">Updated product data</param>
        /// <response code="200">Product updated successfully</response>
        /// <response code="400">Invalid request or category not found</response>
        /// <response code="404">Product not found</response>
        /// <response code="500">Internal server error</response>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ResponseDto<ProductWithCategoryDto>>> Update(int id, [FromBody] DTOs.UpdateProductDto productDto)
        {
            try
            {
                if (productDto == null)
                {
                    return BadRequest(new ResponseDto(400, "Product cannot be null", false));
                }

                if (id != productDto.Id)
                {
                    return BadRequest(new ResponseDto(400, "Product ID in URL does not match Product ID in body", false));
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(new ResponseDto(400, "Invalid product data", false));
                }

                var product = new Product
                {
                    Id = productDto.Id,
                    Name = productDto.Name,
                    Description = string.IsNullOrWhiteSpace(productDto.Description) ? null : productDto.Description.Trim(),
                    Cost = productDto.Cost,
                    Price = productDto.Price,
                    Stock = productDto.Stock,
                    CategoryId = productDto.CategoryId,
                    Image = productDto.Image ?? string.Empty
                };

                var resolvedUserId = GetCurrentUserId();

                if (resolvedUserId <= 0)
                {
                    return BadRequest(new ResponseDto(400, "Could not identify the logged-in user. Please sign in again.", false));
                }

                product.UserId = resolvedUserId;

                var updatedProduct = await _productService.UpdateProductAsync(product);
                var updatedDto = MapToProductWithCategoryDto(updatedProduct);
                return Ok(new ResponseDto<ProductWithCategoryDto>(200, "Product updated successfully", updatedDto));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument when updating product");
                return BadRequest(new ResponseDto(400, ex.Message, false));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation when updating product - category not found");
                return BadRequest(new ResponseDto(400, ex.Message, false));
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Product not found when updating");
                return NotFound(new ResponseDto(404, ex.Message, false));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product");
                return StatusCode(500, new ResponseDto(500, "Internal server error", false));
            }
        }

        /// <summary>
        /// Partially update an existing product
        /// </summary>
        /// <param name="id">The unique identifier of the product to update</param>
        /// <param name="patch">The JSON object containing the partial updates</param>
        /// <response code="200">Product updated successfully</response>
        /// <response code="400">Invalid request or category not found</response>
        /// <response code="404">Product not found</response>
        [HttpPatch("{id}")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ResponseDto<ProductWithCategoryDto>>> Patch(int id, [FromBody] System.Text.Json.JsonElement patch)
        {
            if (patch.ValueKind != System.Text.Json.JsonValueKind.Object)
            {
                return BadRequest(new ResponseDto(400, "Patch payload must be a JSON object", false));
            }

            var existing = await _productService.GetProductByIdAsync(id);
            if (existing == null)
            {
                return NotFound(new ResponseDto(404, $"Product with ID {id} not found", false));
            }

            try
            {
                if (patch.TryGetProperty("name", out var nameProp) && nameProp.ValueKind == System.Text.Json.JsonValueKind.String)
                {
                    var name = nameProp.GetString();
                    if (!string.IsNullOrWhiteSpace(name)) existing.Name = name!;
                }

                if (patch.TryGetProperty("description", out var descriptionProp))
                {
                    var description = descriptionProp.ValueKind == System.Text.Json.JsonValueKind.String
                        ? descriptionProp.GetString()
                        : null;
                    existing.Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
                }

                if (patch.TryGetProperty("cost", out var costProp) && (costProp.ValueKind == System.Text.Json.JsonValueKind.Number))
                {
                    if (costProp.TryGetDecimal(out var cost)) existing.Cost = cost;
                }

                if (patch.TryGetProperty("price", out var priceProp) && (priceProp.ValueKind == System.Text.Json.JsonValueKind.Number))
                {
                    if (priceProp.TryGetDecimal(out var price)) existing.Price = price;
                }

                if (patch.TryGetProperty("stock", out var stockProp) && stockProp.ValueKind == System.Text.Json.JsonValueKind.Number)
                {
                    if (stockProp.TryGetInt32(out var stock)) existing.Stock = stock;
                }

                if (patch.TryGetProperty("categoryId", out var catProp) && catProp.ValueKind == System.Text.Json.JsonValueKind.Number)
                {
                    if (catProp.TryGetInt32(out var cid)) existing.CategoryId = cid;
                }

                if (patch.TryGetProperty("image", out var imgProp) && imgProp.ValueKind == System.Text.Json.JsonValueKind.String)
                {
                    existing.Image = imgProp.GetString() ?? string.Empty;
                }

                if (patch.TryGetProperty("userId", out var userProp) && userProp.ValueKind == System.Text.Json.JsonValueKind.Number)
                {
                    if (userProp.TryGetInt32(out var uid)) existing.UserId = uid;
                }

                existing.UpdatedAt = DateTime.UtcNow;

                var updated = await _productService.UpdateProductAsync(existing);
                var updatedDto = MapToProductWithCategoryDto(updated);
                return Ok(new ResponseDto<ProductWithCategoryDto>(200, "Product updated successfully", updatedDto));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument when patching product");
                return BadRequest(new ResponseDto(400, ex.Message, false));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation when patching product - category not found");
                return BadRequest(new ResponseDto(400, ex.Message, false));
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Product not found when patching");
                return NotFound(new ResponseDto(404, ex.Message, false));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error patching product");
                return StatusCode(500, new ResponseDto(500, "Internal server error", false));
            }
        }

        /// <summary>
        /// Delete a product
        /// </summary>
        /// <param name="id">The unique identifier of the product to delete</param>
        /// <response code="204">Product deleted successfully</response>
        /// <response code="400">Invalid product ID</response>
        /// <response code="404">Product not found</response>
        /// <response code="500">Internal server error</response>
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
                var success = await _productService.DeleteProductAsync(id);
                if (!success)
                {
                    return NotFound(new ResponseDto(404, $"Product with ID {id} not found", false));
                }
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument when deleting product");
                return BadRequest(new ResponseDto(400, ex.Message, false));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product");
                return StatusCode(500, new ResponseDto(500, "Internal server error", false));
            }
        }

        private static ProductWithCategoryDto MapToProductWithCategoryDto(Product p)
        {
            if (p == null) return new ProductWithCategoryDto();

            return new ProductWithCategoryDto
            {
                Id = p.Id,
                CategoryId = p.CategoryId,
                UserId = p.UserId,
                Name = p.Name,
                Description = p.Description,
                Cost = p.Cost,
                Price = p.Price,
                Stock = p.Stock,
                Image = p.Image,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt,
                Category = p.Category == null ? null : new CategoryBasicDto { Id = p.Category.Id, Name = p.Category.Name },
                User = p.User == null ? null : new UserBasicDto { Id = p.User.Id, Username = p.User.Username }
            };
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
