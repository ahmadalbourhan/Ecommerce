using EcommerceAPI.Models;
using EcommerceAPI.Repositories;
using EcommerceAPI.DTOs;
using Microsoft.EntityFrameworkCore;

namespace EcommerceAPI.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly EcommerceAPI.Data.EcommerceDbContext _context;
        private readonly IPermissionService _permissionService;

        public UserService(
            IUserRepository userRepository,
            EcommerceAPI.Data.EcommerceDbContext context,
            IPermissionService permissionService)
        {
            _userRepository = userRepository;
            _context = context;
            _permissionService = permissionService;
        }

        public async Task<IEnumerable<UserDetailDto>> GetAllAsync()
        {
            var users = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .ToListAsync();

            var result = new List<UserDetailDto>();
            foreach (var user in users)
            {
                result.Add(await MapToDetailDtoAsync(user));
            }

            return result;
        }

        public async Task<UserDetailDto?> GetByIdAsync(int id)
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == id);

            return user == null ? null : await MapToDetailDtoAsync(user);
        }

        public async Task<UserDetailDto> CreateAsync(User user, int? roleId = null)
        {
            user.Password = HashPasswordIfNeeded(user.Password);
            await _userRepository.AddAsync(user);
            await _userRepository.SaveAsync();

            await ReplaceRoleAsync(user.Id, roleId);
            var created = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstAsync(u => u.Id == user.Id);

            return await MapToDetailDtoAsync(created);
        }

        public async Task<UserDetailDto> UpdateAsync(User user, int? roleId = null)
        {
            var existing = await _userRepository.GetByIdAsync(user.Id);
            if (existing == null)
                throw new KeyNotFoundException($"User with ID {user.Id} not found.");

            existing.Name = user.Name;
            existing.Email = user.Email;
            existing.PhoneNumber = user.PhoneNumber;
            existing.Username = user.Username;
            existing.IsActive = user.IsActive;

            if (!string.IsNullOrWhiteSpace(user.Password))
            {
                existing.Password = HashPasswordIfNeeded(user.Password);
            }

            await _userRepository.UpdateAsync(existing);
            await _userRepository.SaveAsync();

            await ReplaceRoleAsync(existing.Id, roleId);
            var updated = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstAsync(u => u.Id == existing.Id);

            return await MapToDetailDtoAsync(updated);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                return false;

            await _userRepository.DeleteAsync(id);
            await _userRepository.SaveAsync();
            return true;
        }

        public async Task AssignRoleAsync(int userId, int roleId)
        {
            await ReplaceRoleAsync(userId, roleId);
        }

        private async Task ReplaceRoleAsync(int userId, int? roleId)
        {
            var existingRoles = await _context.UserRoles.Where(ur => ur.UserId == userId).ToListAsync();
            _context.UserRoles.RemoveRange(existingRoles);

            if (roleId.HasValue && roleId.Value > 0)
            {
                var roleExists = await _context.Roles.AnyAsync(r => r.Id == roleId.Value);
                if (!roleExists)
                    throw new KeyNotFoundException($"Role with ID {roleId.Value} not found.");

                await _context.UserRoles.AddAsync(new UserRole { UserId = userId, RoleId = roleId.Value });
            }

            await _context.SaveChangesAsync();
        }

        private async Task<UserDetailDto> MapToDetailDtoAsync(User user)
        {
            var roles = user.UserRoles
                .Where(ur => ur.Role != null)
                .Select(ur => ur.Role!)
                .ToList();
            var primaryRole = roles.FirstOrDefault();
            var permissions = await _permissionService.GetUserPermissionsAsync(user.Id);

            return new UserDetailDto
            {
                Id = user.Id,
                Name = user.Name,
                FullName = user.Name,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Username = user.Username,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                RoleId = primaryRole?.Id,
                RoleName = primaryRole?.Name,
                Roles = roles.Select(r => r.Name).ToList(),
                Permissions = permissions
            };
        }

        private static string HashPasswordIfNeeded(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return password;

            if (password.StartsWith("$2a$") || password.StartsWith("$2b$") || password.StartsWith("$2y$"))
                return password;

            return BCrypt.Net.BCrypt.HashPassword(password);
        }
    }
}
