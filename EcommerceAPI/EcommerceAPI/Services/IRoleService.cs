using EcommerceAPI.Models;

namespace EcommerceAPI.Services
{
    public interface IRoleService
    {
        Task<IEnumerable<Role>> GetAllAsync();
        Task<Role?> GetByIdAsync(int id);
        Task<Role> CreateAsync(Role role);
        Task<Role> UpdateAsync(Role role);
        Task<bool> DeleteAsync(int id);
        Task AssignPermissionAsync(int roleId, int permissionId);
    }
}
