using EcommerceAPI.Constants;
using EcommerceAPI.Data;
using EcommerceAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EcommerceAPI.Seeders
{
    public class DatabaseSeeder
    {
        private readonly EcommerceDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly ILogger<DatabaseSeeder> _logger;

        public DatabaseSeeder(
            EcommerceDbContext context,
            UserManager<User> userManager,
            RoleManager<Role> roleManager,
            ILogger<DatabaseSeeder> logger)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
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
                    // Use RoleManager to ensure Identity tables are populated correctly
                    var role = new Role { Name = roleName };
                    await _roleManager.CreateAsync(role);
                }
            }

            // Save if any new roles were added to the custom Role table
            await _context.SaveChangesAsync();
        }

        private async Task SeedDefaultSuperAdminAsync()
        {
            // Default credentials (change them after first run)
            const string adminEmail = "superadmin@example.com";
            const string adminUserName = "superadmin";
            const string adminFullName = "Super Administrator";
            const string adminPassword = "ChangeMe123!"; // meets current password policy

            // Ensure role exists
            var roleExists = await _roleManager.RoleExistsAsync("SuperAdmin");
            if (!roleExists)
            {
                _logger.LogWarning("SuperAdmin role does not exist when trying to create default user. Creating role.");
                await _roleManager.CreateAsync(new Role { Name = "SuperAdmin" });
            }

            var existing = await _userManager.FindByEmailAsync(adminEmail);
            if (existing != null)
            {
                // Ensure the user is in the SuperAdmin role
                if (!await _userManager.IsInRoleAsync(existing, "SuperAdmin"))
                {
                    await _userManager.AddToRoleAsync(existing, "SuperAdmin");
                }

                _logger.LogInformation("SuperAdmin user already exists, skipping creation.");
                return;
            }

            var user = new User
            {
                UserName = adminUserName,
                Email = adminEmail,
                FullName = adminFullName,
                EmailConfirmed = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var createResult = await _userManager.CreateAsync(user, adminPassword);
            if (!createResult.Succeeded)
            {
                _logger.LogError("Failed to create SuperAdmin user: {errors}", string.Join(';', createResult.Errors.Select(e => e.Description)));
                return;
            }

            var addRoleResult = await _userManager.AddToRoleAsync(user, "SuperAdmin");
            if (!addRoleResult.Succeeded)
            {
                _logger.LogError("Failed to assign SuperAdmin role to user: {errors}", string.Join(';', addRoleResult.Errors.Select(e => e.Description)));
                return;
            }

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

            // Get existing permission slugs assigned to role via RolePermissions
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
