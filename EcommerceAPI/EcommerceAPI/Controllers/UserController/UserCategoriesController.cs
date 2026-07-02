using EcommerceAPI.DTOs;
using EcommerceAPI.Models;
using EcommerceAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceAPI.Controllers.UserController
{
    [ApiController]
    [Route("api/user/categories")]
    public class UserCategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        private readonly ILogger<UserCategoriesController> _logger;

        public UserCategoriesController(ICategoryService categoryService, ILogger<UserCategoriesController> logger)
        {
            _categoryService = categoryService;
            _logger = logger;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<ResponseDto<IEnumerable<CategoryWithProductsDto>>>> GetAll()
        {
            try
            {
                var categories = await _categoryService.GetAllCategoriesAsync();
                return Ok(new ResponseDto<IEnumerable<CategoryWithProductsDto>>(
                    200,
                    "Categories retrieved successfully",
                    categories.Select(MapToCategoryWithProductsDto)));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user categories");
                return StatusCode(500, new ResponseDto(500, "Internal server error", false));
            }
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<ResponseDto<CategoryWithProductsDto>>> GetById(int id)
        {
            try
            {
                var category = await _categoryService.GetCategoryByIdAsync(id);
                if (category == null)
                {
                    return NotFound(new ResponseDto(404, $"Category with ID {id} not found", false));
                }

                return Ok(new ResponseDto<CategoryWithProductsDto>(200, "Category retrieved successfully", MapToCategoryWithProductsDto(category)));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ResponseDto(400, ex.Message, false));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user category");
                return StatusCode(500, new ResponseDto(500, "Internal server error", false));
            }
        }

        private static CategoryWithProductsDto MapToCategoryWithProductsDto(Category category)
        {
            return new CategoryWithProductsDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                Products = category.Products?.Select(product => new ProductBasicDto
                {
                    Id = product.Id,
                    Name = product.Name,
                    Description = product.Description,
                    Price = product.Price,
                    Stock = product.Stock,
                    Image = product.Image
                }).ToList() ?? new List<ProductBasicDto>()
            };
        }
    }
}
