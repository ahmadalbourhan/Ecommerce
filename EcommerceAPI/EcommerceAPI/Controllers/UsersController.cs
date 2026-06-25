using EcommerceAPI.Models;
using EcommerceAPI.Services;
using EcommerceAPI.Authorization;
using EcommerceAPI.Constants;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Retrieve all users
        /// </summary>
        /// <returns>List of all users</returns>
        /// <response code="200">Returns the list of users</response>
        [HttpGet]
        [HasPermission(Permissions.Users.Read)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<User>>> GetAll()
        {
            var users = await _userService.GetAllAsync();
            return Ok(users);
        }

        /// <summary>
        /// Retrieve a specific user by ID
        /// </summary>
        /// <param name="id">The user ID</param>
        /// <returns>The user with the specified ID</returns>
        /// <response code="200">Returns the user</response>
        /// <response code="404">User not found</response>
        [HttpGet("{id}")]
        [HasPermission(Permissions.Users.Read)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<User>> GetById(int id)
        {
            var user = await _userService.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }

        /// <summary>
        /// Create a new user
        /// </summary>
        /// <param name="user">User data to create</param>
        /// <returns>The created user</returns>
        /// <response code="201">User created successfully</response>
        /// <response code="400">Invalid user data</response>
        [HttpPost]
        [HasPermission(Permissions.Users.Create)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<User>> Create(User user)
        {
            var createdUser = await _userService.CreateAsync(user);
            return CreatedAtAction(nameof(GetById), new { id = createdUser.Id }, createdUser);
        }

        /// <summary>
        /// Update an existing user
        /// </summary>
        /// <param name="id">The user ID to update</param>
        /// <param name="user">Updated user data</param>
        /// <response code="204">User updated successfully</response>
        /// <response code="400">Invalid request</response>
        /// <response code="404">User not found</response>
        [HttpPut("{id}")]
        [HasPermission(Permissions.Users.Update)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, User user)
        {
            if (id != user.Id)
            {
                return BadRequest();
            }
            await _userService.UpdateAsync(user);
            return NoContent();
        }

        /// <summary>
        /// Delete a user
        /// </summary>
        /// <param name="id">The user ID to delete</param>
        /// <response code="204">User deleted successfully</response>
        /// <response code="404">User not found</response>
        [HttpDelete("{id}")]
        [HasPermission(Permissions.Users.Delete)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _userService.DeleteAsync(id);
            if (!result)
            {
                return NotFound();
            }
            return NoContent();
        }

        /// <summary>
        /// Assign a role to a user
        /// </summary>
        /// <param name="id">The user ID</param>
        /// <param name="dto">Role assignment data containing the role ID</param>
        /// <response code="204">Role assigned successfully</response>
        /// <response code="400">Invalid request</response>
        [HttpPost("{id}/roles")]
        [HasPermission(Permissions.Users.Update)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AssignRole(int id, [FromBody] RoleAssignmentDto dto)
        {
            await _userService.AssignRoleAsync(id, dto.RoleId);
            return NoContent();
        }
    }

    /// <summary>
    /// DTO for assigning roles to users
    /// </summary>
    public class RoleAssignmentDto
    {
        /// <summary>
        /// The role ID to assign
        /// </summary>
        public int RoleId { get; set; }
    }
}
