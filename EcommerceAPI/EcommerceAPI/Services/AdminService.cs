using EcommerceAPI.Constants;
using EcommerceAPI.DTOs;
using EcommerceAPI.Models;
using EcommerceAPI.Data;
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
        private readonly EcommerceDbContext _context;
        private readonly IPermissionService _permissionService;
        private readonly ILogger<AdminService> _logger;

        public AdminService(
            EcommerceDbContext context,
            IPermissionService permissionService,
            ILogger<AdminService> logger)
        {
            _context = context;
            _permissionService = permissionService;
            _logger = logger;
        }

        public async Task<AdminDetailDto> CreateAdminAsync(CreateAdminDto dto)
        {
            try
            {
                // Check if user already exists
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
                if (existingUser != null)
                {
                    throw new InvalidOperationException($"User with email {dto.Email} already exists.");
                }

                var adminRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
                if (adminRole == null)
                {
                    throw new InvalidOperationException("Admin role not found.");
                }

                var user = new User
                {
                    Username = dto.Email,
                    Email = dto.Email,
                    Name = dto.FullName,
                    Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                await _context.Users.AddAsync(user);
                await _context.SaveChangesAsync();

                // Assign Admin role
                var userRole = new UserRole
                {
                    UserId = user.Id,
                    RoleId = adminRole.Id
                };
                await _context.UserRoles.AddAsync(userRole);
                await _context.SaveChangesAsync();

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
                var adminRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
                if (adminRole == null)
                    return new List<AdminDetailDto>();

                var admins = await _context.UserRoles
                    .Where(ur => ur.RoleId == adminRole.Id)
                    .Include(ur => ur.User)
                    .Select(ur => ur.User)
                    .ToListAsync();

                var result = new List<AdminDetailDto>();

                foreach (var admin in admins)
                {
                    var permissions = await _permissionService.GetUserPermissionsAsync(admin.Id);
                    result.Add(new AdminDetailDto
                    {
                        Id = admin.Id,
                        Email = admin.Email ?? string.Empty,
                        FullName = admin.Name,
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
                var user = await _context.Users
                    .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                    .FirstOrDefaultAsync(u => u.Id == id);

                if (user == null)
                    throw new KeyNotFoundException($"Admin user with ID {id} not found.");

                var isAdmin = user.UserRoles.Any(ur => ur.Role.Name == "Admin");
                if (!isAdmin)
                    throw new InvalidOperationException("User is not an admin.");

                var permissions = await _permissionService.GetUserPermissionsAsync(id);

                return new AdminDetailDto
                {
                    Id = user.Id,
                    Email = user.Email ?? string.Empty,
                    FullName = user.Name,
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
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == adminId);
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
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == adminId);
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
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == adminId);
                if (user == null)
                    throw new KeyNotFoundException($"Admin user with ID {adminId} not found.");

                user.IsActive = isActive;
                _context.Users.Update(user);
                await _context.SaveChangesAsync();

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
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
                if (user == null)
                    throw new KeyNotFoundException($"Admin user with ID {id} not found.");

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

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
