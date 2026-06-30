using EcommerceAPI.Models;
using EcommerceAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PermissionsController : ControllerBase
    {
        private readonly IPermissionService _permissionService;

        public PermissionsController(IPermissionService permissionService)
        {
            _permissionService = permissionService;
        }

        /// <summary>
        /// Retrieve all permissions
        /// </summary>
        /// <returns>List of all permissions</returns>
        /// <response code="200">Returns the list of permissions</response>
        [HttpGet]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Permission>>> GetAll()
        {
            var permissions = await _permissionService.GetAllAsync();
            return Ok(permissions);
        }

        /// <summary>
        /// Retrieve a specific permission by ID
        /// </summary>
        /// <param name="id">The unique identifier of the permission to retrieve</param>
        /// <returns>The permission with the specified ID</returns>
        /// <response code="200">Returns the permission</response>
        /// <response code="404">Permission not found</response>
        [HttpGet("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Permission>> GetById(int id)
        {
            var permission = await _permissionService.GetByIdAsync(id);
            if (permission == null)
            {
                return NotFound();
            }
            return Ok(permission);
        }

        /// <summary>
        /// Create a new permission
        /// </summary>
        /// <param name="permissionDto">Permission data to create</param>
        /// <returns>The created permission</returns>
        /// <response code="201">Permission created successfully</response>
        /// <response code="400">Invalid permission data</response>
        [HttpPost]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Permission>> Create([FromBody] DTOs.CreatePermissionDto permissionDto)
        {
            if (permissionDto == null)
            {
                return BadRequest("Permission cannot be null");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var permission = new Permission
            {
                Slug = permissionDto.Slug
            };

            var createdPermission = await _permissionService.CreateAsync(permission);
            return CreatedAtAction(nameof(GetById), new { id = createdPermission.Id }, createdPermission);
        }

        /// <summary>
        /// Update an existing permission
        /// </summary>
        /// <param name="id">The unique identifier of the permission to update</param>
        /// <param name="permissionDto">Updated permission data</param>
        /// <response code="204">Permission updated successfully</response>
        /// <response code="400">Invalid request</response>
        /// <response code="404">Permission not found</response>
        [HttpPut("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] DTOs.UpdatePermissionDto permissionDto)
        {
            if (permissionDto == null)
            {
                return BadRequest("Permission cannot be null");
            }

            if (id != permissionDto.Id)
            {
                return BadRequest("ID mismatch");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var permission = new Permission
            {
                Id = permissionDto.Id,
                Slug = permissionDto.Slug
            };

            await _permissionService.UpdateAsync(permission);
            return NoContent();
        }

        /// <summary>
        /// Delete a permission
        /// </summary>
        /// <param name="id">The unique identifier of the permission to delete</param>
        /// <response code="204">Permission deleted successfully</response>
        /// <response code="404">Permission not found</response>
        [HttpDelete("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _permissionService.DeleteAsync(id);
            if (!result)
            {
                return NotFound();
            }
            return NoContent();
        }

        /// <summary>
        /// Get all available permissions in the system
        /// </summary>
        /// <response code="200">Returns list of all available permission slugs</response>
        [HttpGet("available/all")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<string>>> GetAvailablePermissions()
        {
            var permissions = await _permissionService.GetAvailablePermissionsAsync();
            return Ok(permissions);
        }

        /// <summary>
        /// Get permissions for a specific user
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <response code="200">Returns list of user's permissions</response>
        [HttpGet("users/{userId}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<string>>> GetUserPermissions(int userId)
        {
            var permissions = await _permissionService.GetUserPermissionsAsync(userId);
            return Ok(permissions);
        }

        /// <summary>
        /// Get unassigned permissions for a specific user
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <response code="200">Returns list of unassigned permissions</response>
        [HttpGet("users/{userId}/unassigned")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<string>>> GetUnassignedPermissions(int userId)
        {
            var permissions = await _permissionService.GetUnassignedPermissionsAsync(userId);
            return Ok(permissions);
        }

        /// <summary>
        /// Assign a permission to a user (SuperAdmin only)
        /// </summary>
        /// <param name="userId">The user ID (must be Admin role)</param>
        /// <param name="request">Permission slug to assign</param>
        /// <response code="200">Permission assigned successfully</response>
        /// <response code="400">Invalid request or user is not Admin</response>
        /// <response code="404">User or permission not found</response>
        [HttpPost("users/{userId}/assign")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AssignPermissionToUser(int userId, [FromBody] AssignPermissionRequest request)
        {
            var result = await _permissionService.AssignPermissionToUserAsync(userId, request.PermissionSlug);
            if (!result)
            {
                return BadRequest("Failed to assign permission. Ensure the user is an Admin and the permission exists.");
            }
            return Ok(new { message = $"Permission '{request.PermissionSlug}' assigned successfully." });
        }

        /// <summary>
        /// Revoke a permission from a user (SuperAdmin only)
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="request">Permission slug to revoke</param>
        /// <response code="200">Permission revoked successfully</response>
        /// <response code="400">Invalid request</response>
        /// <response code="404">User or permission not found</response>
        [HttpPost("users/{userId}/revoke")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RevokePermissionFromUser(int userId, [FromBody] AssignPermissionRequest request)
        {
            var result = await _permissionService.RevokePermissionFromUserAsync(userId, request.PermissionSlug);
            if (!result)
            {
                return BadRequest("Failed to revoke permission.");
            }
            return Ok(new { message = $"Permission '{request.PermissionSlug}' revoked successfully." });
        }
    }

    public class AssignPermissionRequest
    {
        public string PermissionSlug { get; set; } = string.Empty;
    }
}
