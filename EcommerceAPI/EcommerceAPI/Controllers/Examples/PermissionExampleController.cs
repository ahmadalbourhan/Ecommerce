using EcommerceAPI.Authorization;
using EcommerceAPI.Constants;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceAPI.Controllers.Examples
{
    /// <summary>
    /// This is an EXAMPLE controller showing how to use the permission system.
    /// You should apply the [HasPermission] attributes to your actual controllers.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class PermissionExampleController : ControllerBase
    {
        /// <summary>
        /// Example: Create a product (requires Product.Create permission)
        /// </summary>
        [HttpPost("products")]
        [HasPermission(Permissions.Products.Create)]
        public IActionResult CreateProduct()
        {
            return Ok(new { message = "Product created successfully" });
        }

        /// <summary>
        /// Example: Read all products (requires Product.Read permission)
        /// </summary>
        [HttpGet("products")]
        [HasPermission(Permissions.Products.Read)]
        public IActionResult GetAllProducts()
        {
            return Ok(new { message = "Products retrieved successfully" });
        }

        /// <summary>
        /// Example: Update a product (requires Product.Update permission)
        /// </summary>
        [HttpPut("products/{id}")]
        [HasPermission(Permissions.Products.Update)]
        public IActionResult UpdateProduct(int id)
        {
            return Ok(new { message = "Product updated successfully" });
        }

        /// <summary>
        /// Example: Delete a product (requires Product.Delete permission)
        /// </summary>
        [HttpDelete("products/{id}")]
        [HasPermission(Permissions.Products.Delete)]
        public IActionResult DeleteProduct(int id)
        {
            return Ok(new { message = "Product deleted successfully" });
        }

        /// <summary>
        /// Example: Create a category (requires Category.Create permission)
        /// </summary>
        [HttpPost("categories")]
        [HasPermission(Permissions.Categories.Create)]
        public IActionResult CreateCategory()
        {
            return Ok(new { message = "Category created successfully" });
        }

        /// <summary>
        /// Example: Assign permissions to user (requires Permission.Assign)
        /// This demonstrates how to use multiple permissions or protect sensitive operations
        /// </summary>
        [HttpPost("admin/assign-permissions")]
        [HasPermission(Permissions.AdminManagement.ManagePermissions)]
        public IActionResult AssignPermissionsToUser()
        {
            return Ok(new { message = "Permissions assigned successfully" });
        }

        /// <summary>
        /// You can also use multiple [HasPermission] attributes for OR logic.
        /// This endpoint would require EITHER permission to be granted.
        /// </summary>
        [HttpGet("reports")]
        [HasPermission(Permissions.Products.Read)]
        [HasPermission(Permissions.Categories.Read)]
        public IActionResult GetReports()
        {
            return Ok(new { message = "Reports retrieved successfully" });
        }
    }

    /// <summary>
    /// Usage Instructions:
    /// 
    /// 1. Import the Constants:
    ///    using EcommerceAPI.Constants;
    ///    using EcommerceAPI.Authorization;
    /// 
    /// 2. Add the [HasPermission] attribute to your controller methods:
    ///    [HasPermission(Permissions.Product.Create)]
    ///    [HasPermission(Permissions.Category.Update)]
    ///    etc.
    /// 
    /// 3. The PermissionAuthorizationFilter will:
    ///    - Extract the user ID from the authenticated user's claims
    ///    - Check if the user has the required permission
    ///    - Allow or deny access based on the permission check
    /// 
    /// 4. Permission Levels:
    ///    - SuperAdmin: Has ALL permissions automatically
    ///    - Admin: Only has permissions explicitly assigned by SuperAdmin
    ///    - Other roles: Would need to be implemented separately
    /// 
    /// 5. Testing with Swagger:
    ///    - Authenticate as SuperAdmin (username: superadmin)
    ///    - SuperAdmin can access all endpoints
    ///    - Create an Admin user and assign permissions via:
    ///      POST /api/permissions/users/{userId}/assign
    ///    - Login as that Admin user (requires auth integration)
    ///    - Test that they can only access endpoints for assigned permissions
    /// </summary>
}
