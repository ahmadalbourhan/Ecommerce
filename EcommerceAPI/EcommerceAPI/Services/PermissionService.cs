using EcommerceAPI.Repositories;
using EcommerceAPI.Data;
using EcommerceAPI.Models;
using Microsoft.EntityFrameworkCore;
using EcommerceAPI.Constants;

namespace EcommerceAPI.Services
{
    public class PermissionService : IPermissionService
    {
        private readonly IPermissionRepository _permissionRepository;
        private readonly EcommerceDbContext _context;
        private readonly ILogger<PermissionService> _logger;

        public PermissionService(IPermissionRepository permissionRepository, EcommerceDbContext context, ILogger<PermissionService> logger)
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
        /// Gets all permissions for a user (from role or user-specific assignments).
        /// </summary>
        public async Task<List<string>> GetUserPermissionsAsync(int userId)
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .ThenInclude(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
                .Include(u => u.UserPermissions)
                .ThenInclude(up => up.Permission)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return new List<string>();
            }

            var permissions = new HashSet<string>();

            // Get permissions from roles
            foreach (var userRole in user.UserRoles)
            {
                foreach (var rolePermission in userRole.Role.RolePermissions)
                {
                    permissions.Add(rolePermission.Permission.Slug);
                }
            }

            // Get user-specific permissions (for Admin users with assigned permissions)
            foreach (var userPermission in user.UserPermissions)
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
            var role = await _context.Roles
                .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(r => r.Id == roleId);

            if (role == null)
            {
                return new List<string>();
            }

            return role.RolePermissions
                .Select(rp => rp.Permission.Slug)
                .ToList();
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
            // Verify user exists and is an Admin
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                _logger.LogWarning($"User {userId} not found");
                return false;
            }

            var isAdmin = user.UserRoles.Any(ur => ur.Role.Name == "Admin");
            if (!isAdmin)
            {
                _logger.LogWarning($"User {userId} is not an Admin");
                return false;
            }

            // Find permission
            var permission = await _context.Permissions
                .FirstOrDefaultAsync(p => p.Slug == permissionSlug);

            if (permission == null)
            {
                _logger.LogWarning($"Permission '{permissionSlug}' not found");
                return false;
            }

            // Check if already assigned
            var alreadyAssigned = await _context.UserPermissions
                .AnyAsync(up => up.UserId == userId && up.PermissionId == permission.Id);

            if (alreadyAssigned)
            {
                _logger.LogInformation($"Permission '{permissionSlug}' already assigned to user {userId}");
                return true;
            }

            // Assign permission
            var userPermission = new UserPermission
            {
                UserId = userId,
                PermissionId = permission.Id
            };

            await _context.UserPermissions.AddAsync(userPermission);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Permission '{permissionSlug}' assigned to user {userId}");
            return true;
        }

        /// <summary>
        /// Revokes a permission from a user.
        /// </summary>
        public async Task<bool> RevokePermissionFromUserAsync(int userId, string permissionSlug)
        {
            var permission = await _context.Permissions
                .FirstOrDefaultAsync(p => p.Slug == permissionSlug);

            if (permission == null)
            {
                _logger.LogWarning($"Permission '{permissionSlug}' not found");
                return false;
            }

            var userPermission = await _context.UserPermissions
                .FirstOrDefaultAsync(up => up.UserId == userId && up.PermissionId == permission.Id);

            if (userPermission == null)
            {
                _logger.LogWarning($"User {userId} does not have permission '{permissionSlug}'");
                return false;
            }

            _context.UserPermissions.Remove(userPermission);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Permission '{permissionSlug}' revoked from user {userId}");
            return true;
        }

        /// <summary>
        /// Gets all available permissions in the system.
        /// </summary>
        public async Task<List<string>> GetAvailablePermissionsAsync()
        {
            return await _context.Permissions
                .Select(p => p.Slug)
                .ToListAsync();
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
