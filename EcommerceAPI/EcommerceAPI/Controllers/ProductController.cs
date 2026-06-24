using Microsoft.AspNetCore.Mvc;
using EcommerceAPI.Models;
using EcommerceAPI.Services;
using EcommerceAPI.DTOs;

namespace EcommerceAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ILogger<ProductController> _logger;

        public ProductController(IProductService productService, ILogger<ProductController> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        /// <summary>
        /// Get all products
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ResponseDto<IEnumerable<Product>>>> GetAll()
        {
            try
            {
                var products = await _productService.GetAllProductsAsync();
                return Ok(new ResponseDto<IEnumerable<Product>>(200, "Products retrieved successfully", products));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all products");
                return StatusCode(500, new ResponseDto(500, "Internal server error", false));
            }
        }

        /// <summary>
        /// Get product by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ResponseDto<Product>>> GetById(int id)
        {
            try
            {
                var product = await _productService.GetProductByIdAsync(id);
                if (product == null)
                {
                    return NotFound(new ResponseDto(404, $"Product with ID {id} not found", false));
                }
                return Ok(new ResponseDto<Product>(200, "Product retrieved successfully", product));
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
        [HttpPost]
        public async Task<ActionResult<ResponseDto<Product>>> Create([FromBody] Product product)
        {
            try
            {
                if (product == null)
                {
                    return BadRequest(new ResponseDto(400, "Product cannot be null", false));
                }

                var createdProduct = await _productService.CreateProductAsync(product);
                return CreatedAtAction(nameof(GetById), new { id = createdProduct.Id }, 
                    new ResponseDto<Product>(201, "Product created successfully", createdProduct));
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

        /// <summary>
        /// Update an existing product
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<ResponseDto<Product>>> Update(int id, [FromBody] Product product)
        {
            try
            {
                if (product == null)
                {
                    return BadRequest(new ResponseDto(400, "Product cannot be null", false));
                }

                if (id != product.Id)
                {
                    return BadRequest(new ResponseDto(400, "Product ID in URL does not match Product ID in body", false));
                }

                var updatedProduct = await _productService.UpdateProductAsync(product);
                return Ok(new ResponseDto<Product>(200, "Product updated successfully", updatedProduct));
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
        /// Delete a product
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult<ResponseDto>> Delete(int id)
        {
            try
            {
                var success = await _productService.DeleteProductAsync(id);
                if (!success)
                {
                    return NotFound(new ResponseDto(404, $"Product with ID {id} not found", false));
                }
                return Ok(new ResponseDto(200, $"Product with ID {id} deleted successfully"));
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
    }
}
