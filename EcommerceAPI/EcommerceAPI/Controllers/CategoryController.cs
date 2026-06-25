using Microsoft.AspNetCore.Mvc;
using EcommerceAPI.Models;
using EcommerceAPI.Services;
using EcommerceAPI.DTOs;
using EcommerceAPI.Authorization;
using EcommerceAPI.Constants;

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
        /// Retrieve all categories with their associated products
        /// </summary>
        /// <returns>List of all categories</returns>
        /// <response code="200">Returns the list of categories</response>
        /// <response code="500">Internal server error</response>
        [HttpGet]
        [HasPermission(Permissions.Categories.Read)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
        /// Retrieve a specific category by ID with its products
        /// </summary>
        /// <param name="id">The category ID</param>
        /// <returns>The category with the specified ID and its products</returns>
        /// <response code="200">Returns the category</response>
        /// <response code="400">Invalid category ID</response>
        /// <response code="404">Category not found</response>
        /// <response code="500">Internal server error</response>
        [HttpGet("{id}")]
        [HasPermission(Permissions.Categories.Read)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
        /// <param name="createCategoryDto">Category data to create</param>
        /// <returns>The created category</returns>
        /// <response code="201">Category created successfully</response>
        /// <response code="400">Invalid category data</response>
        /// <response code="500">Internal server error</response>
        [HttpPost]
        [HasPermission(Permissions.Categories.Create)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ResponseDto<CategoryWithProductsDto>>> Create([FromBody] CreateCategoryDto createCategoryDto)
        {
            try
            {
                if (createCategoryDto == null)
                {
                    return BadRequest(new ResponseDto(400, "Category data cannot be null", false));
                }

                var category = new Category
                {
                    Name = createCategoryDto.Name,
                    Description = createCategoryDto.Description
                };

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
        /// <param name="id">The category ID to update</param>
        /// <param name="updateCategoryDto">Updated category data</param>
        /// <response code="200">Category updated successfully</response>
        /// <response code="400">Invalid category data</response>
        /// <response code="404">Category not found</response>
        /// <response code="500">Internal server error</response>
        [HttpPut("{id}")]
        [HasPermission(Permissions.Categories.Update)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ResponseDto<CategoryWithProductsDto>>> Update(int id, [FromBody] UpdateCategoryDto updateCategoryDto)
        {
            try
            {
                if (updateCategoryDto == null)
                {
                    return BadRequest(new ResponseDto(400, "Category data cannot be null", false));
                }

                var category = new Category
                {
                    Id = id,
                    Name = updateCategoryDto.Name,
                    Description = updateCategoryDto.Description
                };

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
        /// <param name="id">The category ID to delete</param>
        /// <response code="200">Category deleted successfully</response>
        /// <response code="400">Invalid category ID</response>
        /// <response code="404">Category not found</response>
        /// <response code="500">Internal server error</response>
        [HttpDelete("{id}")]
        [HasPermission(Permissions.Categories.Delete)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
