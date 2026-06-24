using EcommerceAPI.Data;
using EcommerceAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace EcommerceAPI.Repositories
{
    public class PermissionRepository : IPermissionRepository
    {
        private readonly EcommerceDbContext _context;

        public PermissionRepository(EcommerceDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Permission>> GetAllAsync()
        {
            return await _context.Permissions.ToListAsync();
        }

        public async Task<Permission?> GetByIdAsync(int id)
        {
            return await _context.Permissions.FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task AddAsync(Permission entity)
        {
            await _context.Permissions.AddAsync(entity);
        }

        public async Task UpdateAsync(Permission entity)
        {
            _context.Permissions.Update(entity);
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(int id)
        {
            var permission = await GetByIdAsync(id);
            if (permission != null)
            {
                _context.Permissions.Remove(permission);
            }
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
