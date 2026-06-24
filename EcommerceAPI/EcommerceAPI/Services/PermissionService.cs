using EcommerceAPI.Models;
using EcommerceAPI.Repositories;

namespace EcommerceAPI.Services
{
    public class PermissionService : IPermissionService
    {
        private readonly IPermissionRepository _permissionRepository;

        public PermissionService(IPermissionRepository permissionRepository)
        {
            _permissionRepository = permissionRepository;
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
    }
}
