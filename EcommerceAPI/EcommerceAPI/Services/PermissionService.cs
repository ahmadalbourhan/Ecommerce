using EcommerceAPI.Constants;
using EcommerceAPI.Data;
using EcommerceAPI.Models;
using EcommerceAPI.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EcommerceAPI.Services
{
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
        /// SuperAdmin users automatically get all permissions.
        /// </summary>
        public async Task<List<string>> GetUserPermissionsAsync(int userId)
        {
            var permissions = new HashSet<string>();

            // IMPORTANT:
            // Roles are assigned via ASP.NET Identity (UserManager.AddToRoleAsync), which writes to the Identity join table.
            // Do not rely on custom navigation properties here; query Identity tables directly for correctness.
            var roleIds = await _context.Set<IdentityUserRole<int>>()
                .Where(ur => ur.UserId == userId)
                .Select(ur => ur.RoleId)
                .ToListAsync();

            var roleNames = await _context.Roles
                .Where(r => roleIds.Contains(r.Id))
                .Select(r => r.Name)
                .ToListAsync();

            _logger.LogInformation($"User {userId} has {roleNames.Count} role(s): {string.Join(", ", roleNames.Where(r => !string.IsNullOrWhiteSpace(r)))}");

            var isSuperAdmin = roleNames.Any(r => string.Equals(r, "SuperAdmin", StringComparison.OrdinalIgnoreCase));

            if (isSuperAdmin)
            {
                _logger.LogInformation($"User {userId} is SuperAdmin - granting all permissions");
                // SuperAdmin gets all available permissions
                var allPerms = Permissions.GetAllPermissions().ToList();
                _logger.LogInformation($"Returning {allPerms.Count} permissions for SuperAdmin");
                return allPerms;
            }

            _logger.LogInformation($"User {userId} is not SuperAdmin - checking other permission sources");

            var userClaims = await _context.UserClaims
                .Where(uc => uc.UserId == userId && uc.ClaimType == "permission")
                .Select(uc => uc.ClaimValue)
                .ToListAsync();

            foreach (var permission in userClaims)
            {
                if (!string.IsNullOrEmpty(permission))
                    permissions.Add(permission);
            }

            // Check role-based permissions
            if (roleIds.Count > 0)
            {
                // role claims (IdentityRoleClaim entries)
                var roleClaims = await _context.RoleClaims
                    .Where(rc => roleIds.Contains(rc.RoleId) && rc.ClaimType == "permission")
                    .Select(rc => rc.ClaimValue)
                    .ToListAsync();

                foreach (var roleClaim in roleClaims)
                {
                    if (!string.IsNullOrEmpty(roleClaim))
                        permissions.Add(roleClaim);
                }

                // role permissions (custom RolePermission pivot with Permission.Slug)
                var rolePermissions = await _context.RolePermissions
                    .Where(rp => roleIds.Contains(rp.RoleId))
                    .Include(rp => rp.Permission)
                    .Select(rp => rp.Permission.Slug)
                    .ToListAsync();

                foreach (var rpSlug in rolePermissions)
                {
                    if (!string.IsNullOrEmpty(rpSlug))
                        permissions.Add(rpSlug);
                }
            }

            // Also check UserPermission table for explicitly assigned permissions
            var userPermissions = await _context.UserPermissions
                .Where(up => up.UserId == userId)
                .Include(up => up.Permission)
                .ToListAsync();

            foreach (var userPermission in userPermissions)
            {
                if (userPermission?.Permission != null && !string.IsNullOrEmpty(userPermission.Permission.Slug))
                    permissions.Add(userPermission.Permission.Slug);
            }

            _logger.LogInformation($"User {userId} final permissions count: {permissions.Count}");
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
                .Where(v => !string.IsNullOrEmpty(v))
                .Select(v => v!)
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
                var userClaim = new Microsoft.AspNetCore.Identity.IdentityUserClaim<int>
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
