using EcommerceAPI.Models;
using EcommerceAPI.Services;
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
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Permission>>> GetAll()
        {
            var permissions = await _permissionService.GetAllAsync();
            return Ok(permissions);
        }

        /// <summary>
        /// Retrieve a specific permission by ID
        /// </summary>
        /// <param name="id">The permission ID</param>
        /// <returns>The permission with the specified ID</returns>
        /// <response code="200">Returns the permission</response>
        /// <response code="404">Permission not found</response>
        [HttpGet("{id}")]
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
        /// <param name="permission">Permission data to create</param>
        /// <returns>The created permission</returns>
        /// <response code="201">Permission created successfully</response>
        /// <response code="400">Invalid permission data</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Permission>> Create(Permission permission)
        {
            var createdPermission = await _permissionService.CreateAsync(permission);
            return CreatedAtAction(nameof(GetById), new { id = createdPermission.Id }, createdPermission);
        }

        /// <summary>
        /// Update an existing permission
        /// </summary>
        /// <param name="id">The permission ID to update</param>
        /// <param name="permission">Updated permission data</param>
        /// <response code="204">Permission updated successfully</response>
        /// <response code="400">Invalid request</response>
        /// <response code="404">Permission not found</response>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, Permission permission)
        {
            if (id != permission.Id)
            {
                return BadRequest();
            }
            await _permissionService.UpdateAsync(permission);
            return NoContent();
        }

        /// <summary>
        /// Delete a permission
        /// </summary>
        /// <param name="id">The permission ID to delete</param>
        /// <response code="204">Permission deleted successfully</response>
        /// <response code="404">Permission not found</response>
        [HttpDelete("{id}")]
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
    }
}
