using EcommerceAPI.Constants;
using EcommerceAPI.DTOs;
using EcommerceAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EcommerceAPI.Services
{
    public interface IAdminService
    {
        Task<AdminDetailDto> CreateAdminAsync(CreateAdminDto dto);
        Task<List<AdminDetailDto>> GetAllAdminsAsync();
        Task<AdminDetailDto> GetAdminByIdAsync(int id);
        Task<bool> AssignPermissionsAsync(int adminId, List<string> permissionSlugs);
        Task<bool> RevokePermissionsAsync(int adminId, List<string> permissionSlugs);
        Task<bool> UpdateAdminStatusAsync(int adminId, bool isActive);
        Task<bool> DeleteAdminAsync(int id);
    }

    public class AdminService : IAdminService
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly IPermissionService _permissionService;
        private readonly ILogger<AdminService> _logger;

        public AdminService(
            UserManager<User> userManager,
            RoleManager<Role> roleManager,
            IPermissionService permissionService,
            ILogger<AdminService> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _permissionService = permissionService;
            _logger = logger;
        }

        public async Task<AdminDetailDto> CreateAdminAsync(CreateAdminDto dto)
        {
            try
            {
                // Check if user already exists
                var existingUser = await _userManager.FindByEmailAsync(dto.Email);
                if (existingUser != null)
                {
                    throw new InvalidOperationException($"User with email {dto.Email} already exists.");
                }

                var user = new User
                {
                    UserName = dto.Email,
                    Email = dto.Email,
                    FullName = dto.FullName,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(user, dto.Password);
                if (!result.Succeeded)
                {
                    throw new InvalidOperationException($"Failed to create user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }

                // Assign Admin role
                await _userManager.AddToRoleAsync(user, "Admin");

                _logger.LogInformation($"Admin user created: {dto.Email}");

                return await GetAdminByIdAsync(user.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating admin user");
                throw;
            }
        }

        public async Task<List<AdminDetailDto>> GetAllAdminsAsync()
        {
            try
            {
                var adminRole = await _roleManager.FindByNameAsync("Admin");
                if (adminRole == null)
                    return new List<AdminDetailDto>();

                var admins = await _userManager.GetUsersInRoleAsync("Admin");
                var result = new List<AdminDetailDto>();

                foreach (var admin in admins)
                {
                    var permissions = await _permissionService.GetUserPermissionsAsync(admin.Id);
                    result.Add(new AdminDetailDto
                    {
                        Id = admin.Id,
                        Email = admin.Email ?? string.Empty,
                        FullName = admin.FullName,
                        IsActive = admin.IsActive,
                        CreatedAt = admin.CreatedAt,
                        Permissions = permissions
                    });
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all admins");
                throw;
            }
        }

        public async Task<AdminDetailDto> GetAdminByIdAsync(int id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id.ToString());
                if (user == null)
                    throw new KeyNotFoundException($"Admin user with ID {id} not found.");

                var roles = await _userManager.GetRolesAsync(user);
                if (!roles.Contains("Admin"))
                    throw new InvalidOperationException("User is not an admin.");

                var permissions = await _permissionService.GetUserPermissionsAsync(id);

                return new AdminDetailDto
                {
                    Id = user.Id,
                    Email = user.Email ?? string.Empty,
                    FullName = user.FullName,
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt,
                    Permissions = permissions
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting admin by ID");
                throw;
            }
        }

        public async Task<bool> AssignPermissionsAsync(int adminId, List<string> permissionSlugs)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(adminId.ToString());
                if (user == null)
                    throw new KeyNotFoundException($"Admin user with ID {adminId} not found.");

                foreach (var permissionSlug in permissionSlugs)
                {
                    await _permissionService.AssignPermissionToUserAsync(adminId, permissionSlug);
                }

                _logger.LogInformation($"Permissions assigned to admin {adminId}: {string.Join(", ", permissionSlugs)}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning permissions");
                throw;
            }
        }

        public async Task<bool> RevokePermissionsAsync(int adminId, List<string> permissionSlugs)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(adminId.ToString());
                if (user == null)
                    throw new KeyNotFoundException($"Admin user with ID {adminId} not found.");

                foreach (var permissionSlug in permissionSlugs)
                {
                    await _permissionService.RevokePermissionFromUserAsync(adminId, permissionSlug);
                }

                _logger.LogInformation($"Permissions revoked from admin {adminId}: {string.Join(", ", permissionSlugs)}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking permissions");
                throw;
            }
        }

        public async Task<bool> UpdateAdminStatusAsync(int adminId, bool isActive)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(adminId.ToString());
                if (user == null)
                    throw new KeyNotFoundException($"Admin user with ID {adminId} not found.");

                user.IsActive = isActive;
                var result = await _userManager.UpdateAsync(user);

                if (!result.Succeeded)
                    throw new InvalidOperationException($"Failed to update admin status");

                _logger.LogInformation($"Admin {adminId} status updated to {isActive}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating admin status");
                throw;
            }
        }

        public async Task<bool> DeleteAdminAsync(int id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id.ToString());
                if (user == null)
                    throw new KeyNotFoundException($"Admin user with ID {id} not found.");

                var result = await _userManager.DeleteAsync(user);
                if (!result.Succeeded)
                    throw new InvalidOperationException($"Failed to delete admin user");

                _logger.LogInformation($"Admin user {id} deleted");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting admin");
                throw;
            }
        }
    }
}
