using EcommerceAPI.Constants;
using EcommerceAPI.Data;
using EcommerceAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace EcommerceAPI.Seeders
{
    public class DataSeeder
    {
        private readonly EcommerceDbContext _context;
        private readonly ILogger<DataSeeder> _logger;

        public DataSeeder(EcommerceDbContext context, ILogger<DataSeeder> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task SeedAsync()
        {
            try
            {
                await _context.Database.EnsureCreatedAsync();

                // Seed roles if they don't exist
                await SeedRolesAsync();

                // Seed permissions if they don't exist
                await SeedPermissionsAsync();

                // Assign all permissions to SuperAdmin role
                await AssignPermissionsToSuperAdminAsync();

                // Create default SuperAdmin user if it doesn't exist
                await SeedDefaultSuperAdminAsync();

                _logger.LogInformation("Database seeding completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while seeding the database");
                throw;
            }
        }

        private async Task SeedRolesAsync()
        {
            // Check if roles already exist
            if (await _context.Roles.AnyAsync())
            {
                _logger.LogInformation("Roles already exist, skipping seeding");
                return;
            }

            var roles = new List<Role>
            {
                new Role { Name = "SuperAdmin" },
                new Role { Name = "Admin" }
            };

            await _context.Roles.AddRangeAsync(roles);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Roles seeded successfully");
        }

        private async Task SeedPermissionsAsync()
        {
            // Check if permissions already exist
            if (await _context.Permissions.AnyAsync())
            {
                _logger.LogInformation("Permissions already exist, skipping seeding");
                return;
            }

            var permissions = new List<Permission>();

            // Get all permissions from the Permissions class
            var allPermissions = Permissions.GetAllPermissions();

            foreach (var permissionSlug in allPermissions)
            {
                permissions.Add(new Permission { Slug = permissionSlug });
            }

            await _context.Permissions.AddRangeAsync(permissions);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Seeded {permissions.Count} permissions");
        }

        private async Task AssignPermissionsToSuperAdminAsync()
        {
            var superAdminRole = await _context.Roles
                .Include(r => r.RolePermissions)
                .FirstOrDefaultAsync(r => r.Name == "SuperAdmin");

            if (superAdminRole == null)
            {
                _logger.LogWarning("SuperAdmin role not found");
                return;
            }

            // Check if SuperAdmin already has permissions
            if (superAdminRole.RolePermissions.Any())
            {
                _logger.LogInformation("SuperAdmin role already has permissions, skipping assignment");
                return;
            }

            // Get all permissions
            var allPermissions = await _context.Permissions.ToListAsync();

            foreach (var permission in allPermissions)
            {
                superAdminRole.RolePermissions.Add(new RolePermission
                {
                    RoleId = superAdminRole.Id,
                    PermissionId = permission.Id
                });
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation($"Assigned {allPermissions.Count} permissions to SuperAdmin role");
        }

        private async Task SeedDefaultSuperAdminAsync()
        {
            // Check if a SuperAdmin user already exists
            var superAdminRole = await _context.Roles
                .FirstOrDefaultAsync(r => r.Name == "SuperAdmin");

            if (superAdminRole == null)
            {
                _logger.LogWarning("SuperAdmin role not found");
                return;
            }

            var existingSuperAdmin = await _context.UserRoles
                .Include(ur => ur.User)
                .Where(ur => ur.RoleId == superAdminRole.Id)
                .AnyAsync();

            if (existingSuperAdmin)
            {
                _logger.LogInformation("A SuperAdmin user already exists, skipping creation");
                return;
            }

            // Create default SuperAdmin user
            var superAdminUser = new User
            {
                Name = "Super Administrator",
                Username = "superadmin",
                Email = "superadmin@ecommerce.com",
                Password = "SuperAdmin@123" // Note: In production, use proper password hashing
            };

            await _context.Users.AddAsync(superAdminUser);
            await _context.SaveChangesAsync();

            // Assign SuperAdmin role to the user
            var userRole = new UserRole
            {
                UserId = superAdminUser.Id,
                RoleId = superAdminRole.Id
            };

            await _context.UserRoles.AddAsync(userRole);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Default SuperAdmin user created - Username: superadmin, Email: superadmin@ecommerce.com");
        }
    }
}
