namespace EcommerceAPI.Authorization
{
    public static class PermissionPolicies
    {
        public const string Prefix = "Permission:";

        public static string Name(string permission) => $"{Prefix}{permission}";
    }
}
