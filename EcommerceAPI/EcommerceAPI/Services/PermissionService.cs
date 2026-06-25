using EcommerceAPI.Constants;
using EcommerceAPI.Data;
using EcommerceAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace EcommerceAPI.Services
{
    public interface IPermissionService
    {
        // Existing methods
        Task<IEnumerable<Permission>> GetAllAsync();
        Task<Permission?> GetByIdAsync(int id);
        Task<Permission> CreateAsync(Permission permission);
        Task<Permission> UpdateAsync(Permission permission);
        Task<bool> DeleteAsync(int id);

        // New permission management methods
        Task<List<string>> GetUserPermissionsAsync(int userId);
        Task<List<string>> GetRolePermissionsAsync(int roleId);
        Task<bool> HasPermissionAsync(int userId, string permission);
        Task<bool> AssignPermissionToUserAsync(int userId, string permissionSlug);
        Task<bool> RevokePermissionFromUserAsync(int userId, string permissionSlug);
        Task<List<string>> GetAvailablePermissionsAsync();
        Task<List<string>> GetUnassignedPermissionsAsync(int userId);
    }

    public class PermissionService : IPermissionService
    {
        private readonly IPermissionRepository _permissionRepository;
        private readonly EcommerceDbContext _context;
        private readonly ILogger<PermissionService> _logger;

        public PermissionService(
            IPermissionRepository permissionRepository,
            EcommerceDbContext context,
            ILogger<PermissionService> logger)
        {
            _permissionRepository = permissionRepository;
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<Permission>> GetAllAsync()
        {
            return await _permissionRepository.GetAllAsync();
        }

        public async Task<Permission?> GetByIdAsync(int id)
        {
            return await _permissionRepository.GetByIdAsync(id);
        }

        public async Task<Permission> CreateAsync(Permission permission)
        {
            await _permissionRepository.AddAsync(permission);
            await _permissionRepository.SaveAsync();
            return permission;
        }

        public async Task<Permission> UpdateAsync(Permission permission)
        {
            await _permissionRepository.UpdateAsync(permission);
            await _permissionRepository.SaveAsync();
            return permission;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var permission = await _permissionRepository.GetByIdAsync(id);
            if (permission == null)
                return false;

            await _permissionRepository.DeleteAsync(id);
            await _permissionRepository.SaveAsync();
            return true;
        }

        /// <summary>
        /// Gets all permissions for a user (from role claims and user claims).
        /// </summary>
        public async Task<List<string>> GetUserPermissionsAsync(int userId)
        {
            var permissions = new HashSet<string>();

            // For now, get permissions from IdentityUserClaims when role is SuperAdmin
            // This will be populated by the seeder
            var userClaims = await _context.UserClaims
                .Where(uc => uc.UserId == userId && uc.ClaimType == "permission")
                .Select(uc => uc.ClaimValue)
                .ToListAsync();

            foreach (var permission in userClaims)
            {
                if (!string.IsNullOrEmpty(permission))
                    permissions.Add(permission);
            }

            // Also check role-based permissions (SuperAdmin gets all)
            var userRoles = await _context.UserRoles
                .Where(ur => ur.UserId == userId)
                .ToListAsync();

            foreach (var userRole in userRoles)
            {
                var roleClaims = await _context.RoleClaims
                    .Where(rc => rc.RoleId == userRole.RoleId && rc.ClaimType == "permission")
                    .Select(rc => rc.ClaimValue)
                    .ToListAsync();

                foreach (var roleClaim in roleClaims)
                {
                    if (!string.IsNullOrEmpty(roleClaim))
                        permissions.Add(roleClaim);
                }
            }

            // Also check UserPermission table for explicitly assigned permissions
            var userPermissions = await _context.UserPermissions
                .Where(up => up.UserId == userId)
                .Include(up => up.Permission)
                .ToListAsync();

            foreach (var userPermission in userPermissions)
            {
                permissions.Add(userPermission.Permission.Slug);
            }

            return permissions.ToList();
        }

        /// <summary>
        /// Gets all permissions assigned to a role.
        /// </summary>
        public async Task<List<string>> GetRolePermissionsAsync(int roleId)
        {
            return await _context.RoleClaims
                .Where(rc => rc.RoleId == roleId && rc.ClaimType == "permission")
                .Select(rc => rc.ClaimValue)
                .ToListAsync();
        }

        /// <summary>
        /// Checks if a user has a specific permission.
        /// </summary>
        public async Task<bool> HasPermissionAsync(int userId, string permission)
        {
            var permissions = await GetUserPermissionsAsync(userId);
            return permissions.Contains(permission);
        }

        /// <summary>
        /// Assigns a permission to a user (for Admin users).
        /// </summary>
        public async Task<bool> AssignPermissionToUserAsync(int userId, string permissionSlug)
        {
            try
            {
                // Check if already assigned
                var alreadyAssigned = await _context.UserClaims
                    .AnyAsync(uc => uc.UserId == userId && uc.ClaimType == "permission" && uc.ClaimValue == permissionSlug);

                if (alreadyAssigned)
                {
                    _logger.LogInformation($"Permission '{permissionSlug}' already assigned to user {userId}");
                    return true;
                }

                // Add permission claim
                var userClaim = new IdentityUserClaim<int>
                {
                    UserId = userId,
                    ClaimType = "permission",
                    ClaimValue = permissionSlug
                };

                _context.UserClaims.Add(userClaim);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Permission '{permissionSlug}' assigned to user {userId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error assigning permission {permissionSlug} to user {userId}");
                return false;
            }
        }

        /// <summary>
        /// Revokes a permission from a user.
        /// </summary>
        public async Task<bool> RevokePermissionFromUserAsync(int userId, string permissionSlug)
        {
            try
            {
                var userClaim = await _context.UserClaims
                    .FirstOrDefaultAsync(uc => uc.UserId == userId && uc.ClaimType == "permission" && uc.ClaimValue == permissionSlug);

                if (userClaim == null)
                {
                    _logger.LogWarning($"User {userId} does not have permission '{permissionSlug}'");
                    return false;
                }

                _context.UserClaims.Remove(userClaim);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Permission '{permissionSlug}' revoked from user {userId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error revoking permission {permissionSlug} from user {userId}");
                return false;
            }
        }

        /// <summary>
        /// Gets all available permissions in the system.
        /// </summary>
        public async Task<List<string>> GetAvailablePermissionsAsync()
        {
            return Permissions.GetAllPermissions().ToList();
        }

        /// <summary>
        /// Gets permissions not yet assigned to a user.
        /// </summary>
        public async Task<List<string>> GetUnassignedPermissionsAsync(int userId)
        {
            var assignedPermissions = await GetUserPermissionsAsync(userId);
            var availablePermissions = await GetAvailablePermissionsAsync();

            return availablePermissions
                .Where(p => !assignedPermissions.Contains(p))
                .ToList();
        }
    }
}
