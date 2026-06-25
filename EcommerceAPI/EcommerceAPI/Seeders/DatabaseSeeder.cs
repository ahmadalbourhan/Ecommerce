using EcommerceAPI.Constants;
using EcommerceAPI.Data;
using EcommerceAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace EcommerceAPI.Seeders
{
    public class DatabaseSeeder
    {
        private readonly EcommerceDbContext _context;
        private readonly ILogger<DatabaseSeeder> _logger;

        public DatabaseSeeder(
            EcommerceDbContext context,
            ILogger<DatabaseSeeder> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task SeedAsync()
        {
            try
            {
                // Apply migrations if any
                await _context.Database.MigrateAsync();

                // Seed roles (custom Roles table)
                await SeedRolesAsync();

                // Seed permissions (custom Permissions table)
                await SeedPermissionsAsync();

                // Assign all permissions to SuperAdmin role using RolePermissions pivot
                await AssignPermissionsToSuperAdminAsync();

                // Ensure a default SuperAdmin user exists and is assigned the SuperAdmin role
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
            var roles = new[] { "SuperAdmin", "Admin" };

            foreach (var roleName in roles)
            {
                var exists = await _context.Roles.AnyAsync(r => r.Name == roleName);
                if (!exists)
                {
                    var role = new Role { Name = roleName };
                    await _context.Roles.AddAsync(role);
                }
            }

            await _context.SaveChangesAsync();
        }

        private async Task SeedDefaultSuperAdminAsync()
        {
            // Default credentials (change them after first run)
            const string adminEmail = "superadmin@example.com";
            const string adminUserName = "superadmin";
            const string adminFullName = "Super Administrator";
            const string adminPassword = "ChangeMe123!";

            // Ensure role exists
            var superAdminRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "SuperAdmin");
            if (superAdminRole == null)
            {
                _logger.LogWarning("SuperAdmin role does not exist when trying to create default user. Creating role.");
                superAdminRole = new Role { Name = "SuperAdmin" };
                await _context.Roles.AddAsync(superAdminRole);
                await _context.SaveChangesAsync();
            }

            var existing = await _context.Users.FirstOrDefaultAsync(u => u.Email == adminEmail);
            if (existing != null)
            {
                // Ensure the user is in the SuperAdmin role
                var hasRole = await _context.UserRoles.AnyAsync(ur => ur.UserId == existing.Id && ur.RoleId == superAdminRole.Id);
                if (!hasRole)
                {
                    var userRole = new UserRole { UserId = existing.Id, RoleId = superAdminRole.Id };
                    await _context.UserRoles.AddAsync(userRole);
                    await _context.SaveChangesAsync();
                }

                _logger.LogInformation("SuperAdmin user already exists, skipping creation.");
                return;
            }

            var user = new User
            {
                Username = adminUserName,
                Email = adminEmail,
                Name = adminFullName,
                Password = BCrypt.Net.BCrypt.HashPassword(adminPassword),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            // Assign SuperAdmin role
            var userRoleAssignment = new UserRole
            {
                UserId = user.Id,
                RoleId = superAdminRole.Id
            };
            await _context.UserRoles.AddAsync(userRoleAssignment);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created default SuperAdmin user: {email}", adminEmail);
        }

        private async Task SeedPermissionsAsync()
        {
            // Seed permission slugs into the custom Permissions table if they don't exist
            var allPermissions = Permissions.GetAllPermissions();

            var existingSlugs = await _context.Permissions
                .Select(p => p.Slug)
                .ToListAsync();

            var toAdd = allPermissions.Except(existingSlugs).ToList();
            if (!toAdd.Any())
            {
                _logger.LogInformation("Permissions already seeded, skipping");
                return;
            }

            foreach (var slug in toAdd)
            {
                _context.Permissions.Add(new Permission { Slug = slug });
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation($"Seeded {toAdd.Count} permissions");
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

            // Get existing permission IDs assigned to role via RolePermissions
            var existingPermissionIds = superAdminRole.RolePermissions.Select(rp => rp.PermissionId).ToHashSet();

            var allPermissions = await _context.Permissions.ToListAsync();

            var added = 0;
            foreach (var permission in allPermissions)
            {
                if (!existingPermissionIds.Contains(permission.Id))
                {
                    _context.RolePermissions.Add(new RolePermission
                    {
                        RoleId = superAdminRole.Id,
                        PermissionId = permission.Id
                    });
                    added++;
                }
            }

            if (added > 0)
            {
                await _context.SaveChangesAsync();
            }

            _logger.LogInformation($"Assigned {added} permissions to SuperAdmin role");
        }
    }
}
