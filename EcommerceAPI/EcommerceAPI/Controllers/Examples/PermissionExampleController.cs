using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceAPI.Controllers.Examples
{
    /// <summary>
    /// This is an EXAMPLE controller showing how to use authorization.
    /// You should apply the [Authorize] attribute to protected controller actions.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class PermissionExampleController : ControllerBase
    {
        /// <summary>
        /// Example: Create a product
        /// </summary>
        [HttpPost("products")]
        [Authorize]
        public IActionResult CreateProduct()
        {
            return Ok(new { message = "Product created successfully" });
        }

        /// <summary>
        /// Example: Read all products
        /// </summary>
        [HttpGet("products")]
        [Authorize]
        public IActionResult GetAllProducts()
        {
            return Ok(new { message = "Products retrieved successfully" });
        }

        /// <summary>
        /// Example: Update a product
        /// </summary>
        [HttpPut("products/{id}")]
        [Authorize]
        public IActionResult UpdateProduct(int id)
        {
            return Ok(new { message = "Product updated successfully" });
        }

        /// <summary>
        /// Example: Delete a product
        /// </summary>
        [HttpDelete("products/{id}")]
        [Authorize]
        public IActionResult DeleteProduct(int id)
        {
            return Ok(new { message = "Product deleted successfully" });
        }

        /// <summary>
        /// Example: Create a category
        /// </summary>
        [HttpPost("categories")]
        [Authorize]
        public IActionResult CreateCategory()
        {
            return Ok(new { message = "Category created successfully" });
        }

        /// <summary>
        /// Example: Assign permissions to user
        /// </summary>
        [HttpPost("admin/assign-permissions")]
        [Authorize]
        public IActionResult AssignPermissionsToUser()
        {
            return Ok(new { message = "Permissions assigned successfully" });
        }

        /// <summary>
        /// Example: Get reports
        /// </summary>
        [HttpGet("reports")]
        [Authorize]
        public IActionResult GetReports()
        {
            return Ok(new { message = "Reports retrieved successfully" });
        }
    }

    /// <summary>
    /// Usage Instructions:
    ///
    /// Add [Authorize] to controller methods that require a valid JWT.
    /// </summary>
}
