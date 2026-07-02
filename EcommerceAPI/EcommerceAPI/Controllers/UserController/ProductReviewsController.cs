using EcommerceAPI.Data;
using EcommerceAPI.DTOs;
using EcommerceAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcommerceAPI.Controllers.UserController
{
    [ApiController]
    [Route("api/user/products/{productId:int}/reviews")]
    public class ProductReviewsController : ControllerBase
    {
        private readonly EcommerceDbContext _context;
        private readonly ILogger<ProductReviewsController> _logger;
        private readonly IWebHostEnvironment _environment;

        public ProductReviewsController(
            EcommerceDbContext context,
            ILogger<ProductReviewsController> logger,
            IWebHostEnvironment environment)
        {
            _context = context;
            _logger = logger;
            _environment = environment;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<ResponseDto<PagedResultDto<ProductReviewDto>>>> GetPaged(
            int productId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 5)
        {
            try
            {
                page = Math.Max(1, page);
                pageSize = Math.Clamp(pageSize, 1, 20);

                var productExists = await _context.Products.AnyAsync(p => p.Id == productId);
                if (!productExists)
                {
                    return NotFound(new ResponseDto(404, $"Product with ID {productId} not found", false));
                }

                var query = _context.ProductReviews
                    .AsNoTracking()
                    .Include(r => r.User)
                    .Where(r => r.ProductId == productId)
                    .OrderByDescending(r => r.CreatedAt)
                    .ThenByDescending(r => r.Id);

                var totalItems = await query.CountAsync();
                var reviews = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var result = new PagedResultDto<ProductReviewDto>
                {
                    Items = reviews.Select(ToDto).ToList(),
                    Page = page,
                    PageSize = pageSize,
                    TotalItems = totalItems,
                    TotalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)pageSize)
                };

                return Ok(new ResponseDto<PagedResultDto<ProductReviewDto>>(200, "Reviews retrieved successfully", result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving product reviews");
                return StatusCode(500, new ResponseDto(500, "Internal server error", false));
            }
        }

        [HttpGet("stats")]
        [AllowAnonymous]
        public async Task<ActionResult<ResponseDto<ProductReviewStatsDto>>> GetStats(int productId)
        {
            var productExists = await _context.Products.AnyAsync(p => p.Id == productId);
            if (!productExists)
            {
                return NotFound(new ResponseDto(404, $"Product with ID {productId} not found", false));
            }

            var grouped = await _context.ProductReviews
                .AsNoTracking()
                .Where(r => r.ProductId == productId)
                .GroupBy(r => r.Rating)
                .Select(g => new { Rating = g.Key, Count = g.Count() })
                .ToListAsync();

            var total = grouped.Sum(g => g.Count);
            var average = total == 0 ? 0 : grouped.Sum(g => g.Rating * g.Count) / (double)total;

            var stats = new ProductReviewStatsDto
            {
                ProductId = productId,
                TotalReviews = total,
                AverageRating = Math.Round(average, 1),
                RatingCounts = Enumerable.Range(1, 5).ToDictionary(rating => rating, rating => grouped.FirstOrDefault(g => g.Rating == rating)?.Count ?? 0)
            };

            return Ok(new ResponseDto<ProductReviewStatsDto>(200, "Review stats retrieved successfully", stats));
        }

        [HttpGet("eligibility")]
        [Authorize]
        public async Task<ActionResult<ResponseDto<ProductReviewEligibilityDto>>> GetEligibility(int productId)
        {
            var userId = GetCurrentUserId();
            if (userId <= 0)
            {
                return Unauthorized(new ResponseDto(401, "Please sign in again.", false));
            }

            var eligibility = await BuildEligibilityAsync(productId, userId);
            return Ok(new ResponseDto<ProductReviewEligibilityDto>(200, "Review eligibility retrieved successfully", eligibility));
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<ResponseDto<ProductReviewDto>>> Create(int productId, [FromBody] CreateProductReviewDto dto)
        {
            try
            {
                if (dto == null)
                {
                    return BadRequest(new ResponseDto(400, "Review cannot be null", false));
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(new ResponseDto(400, "Rating must be between 1 and 5.", false));
                }

                var commentValidation = ValidateComment(dto.Comment);
                if (commentValidation != null)
                {
                    return BadRequest(new ResponseDto(400, commentValidation, false));
                }

                if (!string.IsNullOrWhiteSpace(dto.ImageUrl) && !dto.ImageUrl.StartsWith("/uploads/review/", StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest(new ResponseDto(400, "Please upload the review image from your device.", false));
                }

                var userId = GetCurrentUserId();
                if (userId <= 0)
                {
                    return Unauthorized(new ResponseDto(401, "Please sign in again.", false));
                }

                var productExists = await _context.Products.AnyAsync(p => p.Id == productId);
                if (!productExists)
                {
                    return NotFound(new ResponseDto(404, $"Product with ID {productId} not found", false));
                }

                var canReview = await _context.Orders
                    .AnyAsync(o =>
                        o.Id == dto.OrderId &&
                        o.UserId == userId &&
                        o.Status == "Delivered" &&
                        o.OrderItems.Any(i => i.ProductId == productId));

                if (!canReview)
                {
                    return BadRequest(new ResponseDto(400, "You can review this product only after a delivered order that includes it.", false));
                }

                var alreadyReviewed = await _context.ProductReviews
                    .AnyAsync(r => r.ProductId == productId && r.UserId == userId && r.OrderId == dto.OrderId);

                if (alreadyReviewed)
                {
                    return BadRequest(new ResponseDto(400, "You already reviewed this product for that order.", false));
                }

                var review = new ProductReview
                {
                    ProductId = productId,
                    UserId = userId,
                    OrderId = dto.OrderId,
                    Rating = dto.Rating,
                    Comment = string.IsNullOrWhiteSpace(dto.Comment) ? null : dto.Comment.Trim(),
                    ImageUrl = string.IsNullOrWhiteSpace(dto.ImageUrl) ? null : dto.ImageUrl.Trim(),
                    CreatedAt = DateTime.UtcNow
                };

                _context.ProductReviews.Add(review);
                await _context.SaveChangesAsync();

                var saved = await _context.ProductReviews
                    .AsNoTracking()
                    .Include(r => r.User)
                    .FirstAsync(r => r.Id == review.Id);

                return CreatedAtAction(nameof(GetPaged), new { productId }, new ResponseDto<ProductReviewDto>(201, "Review added successfully", ToDto(saved)));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product review");
                return StatusCode(500, new ResponseDto(500, "Internal server error", false));
            }
        }

        [HttpPost("upload-image")]
        [Authorize]
        public async Task<ActionResult<ResponseDto<ProductReviewImageUploadDto>>> UploadImage(int productId, IFormFile file)
        {
            var productExists = await _context.Products.AnyAsync(p => p.Id == productId);
            if (!productExists)
            {
                return NotFound(new ResponseDto(404, $"Product with ID {productId} not found", false));
            }

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

            var uploadsPath = Path.Combine(_environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot"), "uploads", "review");
            Directory.CreateDirectory(uploadsPath);

            var fileName = $"{Guid.NewGuid():N}{extension}";
            var filePath = Path.Combine(uploadsPath, fileName);

            await using (var stream = System.IO.File.Create(filePath))
            {
                await file.CopyToAsync(stream);
            }

            var publicPath = $"/uploads/review/{fileName}";
            return Ok(new ResponseDto<ProductReviewImageUploadDto>(200, "Image uploaded successfully", new ProductReviewImageUploadDto
            {
                ImageUrl = publicPath
            }));
        }

        private async Task<ProductReviewEligibilityDto> BuildEligibilityAsync(int productId, int userId)
        {
            var orders = await _context.Orders
                .AsNoTracking()
                .Where(o =>
                    o.UserId == userId &&
                    o.Status == "Delivered" &&
                    o.OrderItems.Any(i => i.ProductId == productId) &&
                    !_context.ProductReviews.Any(r => r.ProductId == productId && r.UserId == userId && r.OrderId == o.Id))
                .OrderByDescending(o => o.DeliveredAt ?? o.OrderedAt)
                .Select(o => new ReviewableOrderDto
                {
                    OrderId = o.Id,
                    OrderNumber = o.OrderNumber,
                    OrderedAt = o.OrderedAt,
                    DeliveredAt = o.DeliveredAt
                })
                .ToListAsync();

            return new ProductReviewEligibilityDto
            {
                CanReview = orders.Count > 0,
                Message = orders.Count > 0
                    ? null
                    : "You can review this product after it is delivered in one of your orders.",
                Orders = orders
            };
        }

        private static ProductReviewDto ToDto(ProductReview review)
        {
            return new ProductReviewDto
            {
                Id = review.Id,
                ProductId = review.ProductId,
                UserId = review.UserId,
                OrderId = review.OrderId,
                Rating = review.Rating,
                Comment = review.Comment,
                ImageUrl = review.ImageUrl,
                CreatedAt = review.CreatedAt,
                User = review.User == null ? null : new UserBasicDto
                {
                    Id = review.User.Id,
                    Username = review.User.Name,
                    PhoneNumber = review.User.PhoneNumber
                }
            };
        }

        private static string? ValidateComment(string? comment)
        {
            if (string.IsNullOrWhiteSpace(comment)) return null;

            var value = comment.Trim();
            var lowered = value.ToLowerInvariant();
            var blockedPatterns = new[]
            {
                "<script",
                "</script",
                "javascript:",
                "onerror=",
                "onload=",
                "onclick=",
                "--",
                ";--",
                "/*",
                "*/",
                "xp_",
                "drop table",
                "drop database",
                "delete from",
                "insert into",
                "update ",
                "select ",
                "union select",
                "alter table",
                "truncate table",
                "exec ",
                "execute "
            };

            return blockedPatterns.Any(pattern => lowered.Contains(pattern))
                ? "Comment cannot contain JavaScript or SQL commands."
                : null;
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
