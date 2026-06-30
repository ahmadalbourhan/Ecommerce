using EcommerceAPI.Models;
using EcommerceAPI.Services;
using EcommerceAPI.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RolesController : ControllerBase
    {
        private readonly IRoleService _roleService;

        public RolesController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        /// <summary>
        /// Retrieve all roles
        /// </summary>
        /// <returns>List of all roles</returns>
        /// <response code="200">Returns the list of roles</response>
        [HttpGet]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Role>>> GetAll()
        {
            var roles = await _roleService.GetAllAsync();
            return Ok(roles);
        }

        /// <summary>
        /// Retrieve a specific role by ID
        /// </summary>
        /// <param name="id">The unique identifier of the role to retrieve</param>
        /// <returns>The role with the specified ID</returns>
        /// <response code="200">Returns the role</response>
        /// <response code="404">Role not found</response>
        [HttpGet("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Role>> GetById(int id)
        {
            var role = await _roleService.GetByIdAsync(id);
            if (role == null)
            {
                return NotFound();
            }
            return Ok(role);
        }

        /// <summary>
        /// Create a new role
        /// </summary>
        /// <param name="dto">Role data to create</param>
        /// <returns>The created role</returns>
        /// <response code="201">Role created successfully</response>
        /// <response code="400">Invalid role data</response>
        [HttpPost]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Role>> Create([FromBody] CreateRoleDto dto)
        {
            var role = new Role { Name = dto.Name };
            var createdRole = await _roleService.CreateAsync(role);
            return CreatedAtAction(nameof(GetById), new { id = createdRole.Id }, createdRole);
        }

        /// <summary>
        /// Update an existing role
        /// </summary>
        /// <param name="id">The unique identifier of the role to update</param>
        /// <param name="dto">Updated role data</param>
        /// <response code="204">Role updated successfully</response>
        /// <response code="400">Invalid request</response>
        /// <response code="404">Role not found</response>
        [HttpPut("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateRoleDto dto)
        {
            var role = new Role { Id = id, Name = dto.Name };
            await _roleService.UpdateAsync(role);
            return NoContent();
        }

        /// <summary>
        /// Delete a role
        /// </summary>
        /// <param name="id">The unique identifier of the role to delete</param>
        /// <response code="204">Role deleted successfully</response>
        /// <response code="404">Role not found</response>
        [HttpDelete("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _roleService.DeleteAsync(id);
            if (!result)
            {
                return NotFound();
            }
            return NoContent();
        }

        /// <summary>
        /// Assign a permission to a role
        /// </summary>
        /// <param name="id">The role ID</param>
        /// <param name="dto">Permission assignment data containing the permission ID</param>
        /// <response code="204">Permission assigned successfully</response>
        /// <response code="400">Invalid request</response>
        [HttpPost("{id}/permissions")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AssignPermission(int id, [FromBody] PermissionAssignmentDto dto)
        {
            await _roleService.AssignPermissionAsync(id, dto.PermissionId);
            return NoContent();
        }
    }

    /// <summary>
    /// DTO for assigning permissions to roles
    /// </summary>
    public class PermissionAssignmentDto
    {
        /// <summary>
        /// The permission ID to assign
        /// </summary>
        public int PermissionId { get; set; }
    }
}
