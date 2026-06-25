namespace EcommerceAPI.Authorization
{
    /// <summary>
    /// Attribute to protect endpoints with permission-based authorization.
    /// Usage: [HasPermission("Products.Create")]
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class HasPermissionAttribute : Attribute
    {
        public string Permission { get; }

        public HasPermissionAttribute(string permission)
        {
            Permission = permission;
        }
    }
}
