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
                await _context.Database.MigrateAsync();

                // Seed roles
                await SeedRolesAsync();

                // Seed default SuperAdmin user
                await SeedDefaultSuperAdminAsync();

                // Assign all permissions to SuperAdmin role
                await AssignPermissionsToSuperAdminAsync();

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
                var roleExists = await _roleManager.RoleExistsAsync(roleName);
                if (!roleExists)
                {
                    var role = new Role { Name = roleName };
                    var result = await _roleManager.CreateAsync(role);
                    if (result.Succeeded)
                    {
                        _logger.LogInformation($"Role '{roleName}' created successfully");
                    }
                    else
                    {
                        _logger.LogError($"Failed to create role '{roleName}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    }
                }
            }
        }

        private async Task SeedDefaultSuperAdminAsync()
        {
            var superAdminEmail = "superadmin@app.com";
            var existingUser = await _userManager.FindByEmailAsync(superAdminEmail);

            if (existingUser != null)
            {
                _logger.LogInformation("Default SuperAdmin user already exists");
                return;
            }

            var superAdminUser = new User
            {
                UserName = superAdminEmail,
                Email = superAdminEmail,
                FullName = "Super Administrator",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(superAdminUser, "SuperAdmin@123");
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(superAdminUser, "SuperAdmin");
                _logger.LogInformation("Default SuperAdmin user created successfully");
            }
            else
            {
                _logger.LogError($"Failed to create SuperAdmin user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }

        private async Task AssignPermissionsToSuperAdminAsync()
        {
            var superAdminRole = await _roleManager.FindByNameAsync("SuperAdmin");
            if (superAdminRole == null)
            {
                _logger.LogWarning("SuperAdmin role not found");
                return;
            }

            var existingClaims = await _context.RoleClaims
                .Where(rc => rc.RoleId == superAdminRole.Id && rc.ClaimType == "permission")
                .ToListAsync();

            if (existingClaims.Any())
            {
                _logger.LogInformation("SuperAdmin role already has permissions assigned");
                return;
            }

            var allPermissions = Permissions.GetAllPermissions();

            foreach (var permission in allPermissions)
            {
                var claim = new IdentityRoleClaim<int>
                {
                    RoleId = superAdminRole.Id,
                    ClaimType = "permission",
                    ClaimValue = permission
                };

                _context.RoleClaims.Add(claim);
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation($"Assigned {allPermissions.Length} permissions to SuperAdmin role");
        }
    }
}
