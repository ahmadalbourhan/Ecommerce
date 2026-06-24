using Microsoft.AspNetCore.Mvc;
using EcommerceAPI.Models;
using EcommerceAPI.Services;
using EcommerceAPI.DTOs;

namespace EcommerceAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        private readonly ILogger<CategoryController> _logger;

        public CategoryController(ICategoryService categoryService, ILogger<CategoryController> logger)
        {
            _categoryService = categoryService;
            _logger = logger;
        }

        /// <summary>
        /// Get all categories
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ResponseDto<IEnumerable<CategoryWithProductsDto>>>> GetAll()
        {
            try
            {
                var categories = await _categoryService.GetAllCategoriesAsync();
                var categoryDtos = categories.Select(MapToCategoryWithProductsDto);
                return Ok(new ResponseDto<IEnumerable<CategoryWithProductsDto>>(200, "Categories retrieved successfully", categoryDtos));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all categories");
                return StatusCode(500, new ResponseDto(500, "Internal server error", false));
            }
        }

        /// <summary>
        /// Get category by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ResponseDto<CategoryWithProductsDto>>> GetById(int id)
        {
            try
            {
                var category = await _categoryService.GetCategoryByIdAsync(id);
                if (category == null)
                {
                    return NotFound(new ResponseDto(404, $"Category with ID {id} not found", false));
                }

                var dto = MapToCategoryWithProductsDto(category);
                return Ok(new ResponseDto<CategoryWithProductsDto>(200, "Category retrieved successfully", dto));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument when retrieving category");
                return BadRequest(new ResponseDto(400, ex.Message, false));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving category");
                return StatusCode(500, new ResponseDto(500, "Internal server error", false));
            }
        }

        /// <summary>
        /// Create a new category
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ResponseDto<CategoryWithProductsDto>>> Create([FromBody] Category category)
        {
            try
            {
                if (category == null)
                {
                    return BadRequest(new ResponseDto(400, "Category cannot be null", false));
                }

                var createdCategory = await _categoryService.CreateCategoryAsync(category);
                var createdDto = MapToCategoryWithProductsDto(createdCategory);

                return CreatedAtAction(nameof(GetById), new { id = createdDto.Id }, 
                    new ResponseDto<CategoryWithProductsDto>(201, "Category created successfully", createdDto));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument when creating category");
                return BadRequest(new ResponseDto(400, ex.Message, false));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating category");
                return StatusCode(500, new ResponseDto(500, "Internal server error", false));
            }
        }

        /// <summary>
        /// Update an existing category
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<ResponseDto<CategoryWithProductsDto>>> Update(int id, [FromBody] Category category)
        {
            try
            {
                if (category == null)
                {
                    return BadRequest(new ResponseDto(400, "Category cannot be null", false));
                }

                if (id != category.Id)
                {
                    return BadRequest(new ResponseDto(400, "Category ID in URL does not match Category ID in body", false));
                }

                var updatedCategory = await _categoryService.UpdateCategoryAsync(category);
                var updatedDto = MapToCategoryWithProductsDto(updatedCategory);
                return Ok(new ResponseDto<CategoryWithProductsDto>(200, "Category updated successfully", updatedDto));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument when updating category");
                return BadRequest(new ResponseDto(400, ex.Message, false));
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Category not found when updating");
                return NotFound(new ResponseDto(404, ex.Message, false));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating category");
                return StatusCode(500, new ResponseDto(500, "Internal server error", false));
            }
        }

        /// <summary>
        /// Delete a category
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult<ResponseDto>> Delete(int id)
        {
            try
            {
                var success = await _categoryService.DeleteCategoryAsync(id);
                if (!success)
                {
                    return NotFound(new ResponseDto(404, $"Category with ID {id} not found", false));
                }
                return Ok(new ResponseDto(200, $"Category with ID {id} deleted successfully"));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument when deleting category");
                return BadRequest(new ResponseDto(400, ex.Message, false));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting category");
                return StatusCode(500, new ResponseDto(500, "Internal server error", false));
            }
        }

        private static CategoryWithProductsDto MapToCategoryWithProductsDto(Category category)
        {
            if (category == null) return new CategoryWithProductsDto();

            var dto = new CategoryWithProductsDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                Products = category.Products?.Select(p => new ProductBasicDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    Image = p.Image
                }).ToList() ?? new List<ProductBasicDto>()
            };

            return dto;
        }
    }
}
