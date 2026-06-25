using Microsoft.AspNetCore.Identity;

namespace EcommerceAPI.Models
{
    // Inherit from IdentityUserRole<int> so Identity and the custom model share the same join type
    public class UserRole : IdentityUserRole<int>
    {
        // Navigation properties
        public User User { get; set; } = null!;
        public Role Role { get; set; } = null!;
    }
}
