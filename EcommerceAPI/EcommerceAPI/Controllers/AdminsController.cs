using EcommerceAPI.Authorization;
using EcommerceAPI.Constants;
using EcommerceAPI.DTOs;
using EcommerceAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminsController : ControllerBase
    {
        private readonly IAdminService _adminService;
        private readonly ILogger<AdminsController> _logger;

        public AdminsController(IAdminService adminService, ILogger<AdminsController> logger)
        {
            _adminService = adminService;
            _logger = logger;
        }

        /// <summary>
        /// Create a new Admin account (SuperAdmin only)
        /// </summary>
        [HttpPost]
        [HasPermission(Permissions.AdminManagement.Create)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<AdminDetailDto>> CreateAdmin([FromBody] CreateAdminDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var admin = await _adminService.CreateAdminAsync(dto);
                return CreatedAtAction(nameof(GetAdminById), new { id = admin.Id }, admin);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating admin");
                return StatusCode(500, new { message = "An error occurred while creating the admin" });
            }
        }

        /// <summary>
        /// Get all Admin accounts (SuperAdmin only)
        /// </summary>
        [HttpGet]
        [HasPermission(Permissions.AdminManagement.Read)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<List<AdminDetailDto>>> GetAllAdmins()
        {
            try
            {
                var admins = await _adminService.GetAllAdminsAsync();
                return Ok(admins);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving admins");
                return StatusCode(500, new { message = "An error occurred while retrieving admins" });
            }
        }

        /// <summary>
        /// Get a specific Admin account by ID (SuperAdmin only)
        /// </summary>
        [HttpGet("{id}")]
        [HasPermission(Permissions.AdminManagement.Read)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<AdminDetailDto>> GetAdminById(int id)
        {
            try
            {
                var admin = await _adminService.GetAdminByIdAsync(id);
                return Ok(admin);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving admin");
                return StatusCode(500, new { message = "An error occurred while retrieving the admin" });
            }
        }

        /// <summary>
        /// Assign permissions to an Admin (SuperAdmin only)
        /// </summary>
        [HttpPost("{id}/permissions")]
        [HasPermission(Permissions.AdminManagement.ManagePermissions)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> AssignPermissions(int id, [FromBody] AssignPermissionsDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                await _adminService.AssignPermissionsAsync(id, dto.PermissionSlugs);
                return Ok(new { message = "Permissions assigned successfully" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning permissions");
                return StatusCode(500, new { message = "An error occurred while assigning permissions" });
            }
        }

        /// <summary>
        /// Revoke permissions from an Admin (SuperAdmin only)
        /// </summary>
        [HttpDelete("{id}/permissions")]
        [HasPermission(Permissions.AdminManagement.ManagePermissions)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> RevokePermissions(int id, [FromBody] AssignPermissionsDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                await _adminService.RevokePermissionsAsync(id, dto.PermissionSlugs);
                return Ok(new { message = "Permissions revoked successfully" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking permissions");
                return StatusCode(500, new { message = "An error occurred while revoking permissions" });
            }
        }

        /// <summary>
        /// Update Admin status (activate/deactivate) - SuperAdmin only
        /// </summary>
        [HttpPatch("{id}/status")]
        [HasPermission(Permissions.AdminManagement.Update)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> UpdateAdminStatus(int id, [FromBody] UpdateAdminStatusDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                await _adminService.UpdateAdminStatusAsync(id, dto.IsActive);
                return Ok(new { message = "Admin status updated successfully" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating admin status");
                return StatusCode(500, new { message = "An error occurred while updating admin status" });
            }
        }

        /// <summary>
        /// Delete an Admin account (SuperAdmin only)
        /// </summary>
        [HttpDelete("{id}")]
        [HasPermission(Permissions.AdminManagement.Delete)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> DeleteAdmin(int id)
        {
            try
            {
                await _adminService.DeleteAdminAsync(id);
                return Ok(new { message = "Admin deleted successfully" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting admin");
                return StatusCode(500, new { message = "An error occurred while deleting the admin" });
            }
        }
    }
}
