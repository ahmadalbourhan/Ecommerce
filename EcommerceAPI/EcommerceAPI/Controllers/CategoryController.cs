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
        public async Task<ActionResult<ResponseDto<IEnumerable<Category>>>> GetAll()
        {
            try
            {
                var categories = await _categoryService.GetAllCategoriesAsync();
                return Ok(new ResponseDto<IEnumerable<Category>>(200, "Categories retrieved successfully", categories));
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
        public async Task<ActionResult<ResponseDto<Category>>> GetById(int id)
        {
            try
            {
                var category = await _categoryService.GetCategoryByIdAsync(id);
                if (category == null)
                {
                    return NotFound(new ResponseDto(404, $"Category with ID {id} not found", false));
                }
                return Ok(new ResponseDto<Category>(200, "Category retrieved successfully", category));
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
        public async Task<ActionResult<ResponseDto<Category>>> Create([FromBody] Category category)
        {
            try
            {
                if (category == null)
                {
                    return BadRequest(new ResponseDto(400, "Category cannot be null", false));
                }

                var createdCategory = await _categoryService.CreateCategoryAsync(category);
                return CreatedAtAction(nameof(GetById), new { id = createdCategory.Id }, 
                    new ResponseDto<Category>(201, "Category created successfully", createdCategory));
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
        public async Task<ActionResult<ResponseDto<Category>>> Update(int id, [FromBody] Category category)
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
                return Ok(new ResponseDto<Category>(200, "Category updated successfully", updatedCategory));
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
    }
}
