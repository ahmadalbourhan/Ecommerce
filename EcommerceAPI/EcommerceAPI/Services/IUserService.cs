using EcommerceAPI.Models;
using EcommerceAPI.DTOs;

namespace EcommerceAPI.Services
{
    public interface IUserService
    {
        Task<IEnumerable<UserDetailDto>> GetAllAsync();
        Task<UserDetailDto?> GetByIdAsync(int id);
        Task<UserDetailDto> CreateAsync(User user, int? roleId = null);
        Task<UserDetailDto> UpdateAsync(User user, int? roleId = null);
        Task<bool> DeleteAsync(int id);
        Task AssignRoleAsync(int userId, int roleId);
    }
}
