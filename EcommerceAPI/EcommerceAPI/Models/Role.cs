using Microsoft.AspNetCore.Identity;

namespace EcommerceAPI.Models
{
    // Role extends IdentityRole<int> so it can be used with ASP.NET Identity
    public class Role : IdentityRole<int>
    {
        public Role() : base() { }
        public Role(string roleName) : base(roleName) { }

        // Navigation properties for the permission system
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
        public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}
