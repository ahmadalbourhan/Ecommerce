using EcommerceAPI.Models;
using EcommerceAPI.Repositories;

namespace EcommerceAPI.Services
{
    public class RoleService : IRoleService
    {
        private readonly IRoleRepository _roleRepository;
        private readonly EcommerceAPI.Data.EcommerceDbContext _context;

        public RoleService(IRoleRepository roleRepository, EcommerceAPI.Data.EcommerceDbContext context)
        {
            _roleRepository = roleRepository;
            _context = context;
        }

        public async Task<IEnumerable<Role>> GetAllAsync()
        {
            return await _roleRepository.GetAllAsync();
        }

        public async Task<Role?> GetByIdAsync(int id)
        {
            return await _roleRepository.GetByIdAsync(id);
        }

        public async Task<Role> CreateAsync(Role role)
        {
            await _roleRepository.AddAsync(role);
            await _roleRepository.SaveAsync();
            return role;
        }

        public async Task<Role> UpdateAsync(Role role)
        {
            await _roleRepository.UpdateAsync(role);
            await _roleRepository.SaveAsync();
            return role;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var role = await _roleRepository.GetByIdAsync(id);
            if (role == null)
                return false;

            await _roleRepository.DeleteAsync(id);
            await _roleRepository.SaveAsync();
            return true;
        }

        public async Task AssignPermissionAsync(int roleId, int permissionId)
        {
            var rolePermission = new RolePermission { RoleId = roleId, PermissionId = permissionId };
            await _context.RolePermissions.AddAsync(rolePermission);
            await _context.SaveChangesAsync();
        }
    }
}
