namespace EcommerceAPI.Models
{
    /// <summary>
    /// Stores user-specific permissions for Admin users.
    /// SuperAdmin permissions are stored as RolePermissions instead.
    /// </summary>
    public class UserPermission
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int PermissionId { get; set; }

        // Navigation properties
        public User User { get; set; } = null!;
        public Permission Permission { get; set; } = null!;
    }
}
