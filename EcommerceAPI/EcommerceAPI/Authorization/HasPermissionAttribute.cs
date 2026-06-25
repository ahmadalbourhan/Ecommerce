using Microsoft.AspNetCore.Authorization;

namespace EcommerceAPI.Authorization
{
    /// <summary>
    /// Requires JWT authentication and a specific permission policy.
    /// Usage: [HasPermission(Permissions.Products.Create)]
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class HasPermissionAttribute : AuthorizeAttribute
    {
        public string Permission { get; }

        public HasPermissionAttribute(string permission)
        {
            Permission = permission;
            Policy = PermissionPolicies.Name(permission);
        }
    }
}
