namespace EcommerceAPI.Models
{
    public class Permission
    {
        public int Id { get; set; }
        public string Slug { get; set; } = string.Empty;

        // Navigation properties
        public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}
