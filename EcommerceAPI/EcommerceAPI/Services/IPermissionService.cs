using EcommerceAPI.Models;

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
}
