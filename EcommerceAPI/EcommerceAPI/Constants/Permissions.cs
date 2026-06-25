namespace EcommerceAPI.Constants
{
    /// <summary>
    /// Defines all application permissions grouped by entity.
    /// Format: {Entity}.{Action}
    /// Adding a new entity: Add a nested class with CRUD constants.
    /// </summary>
    public static class Permissions
    {
        // Product Permissions
        public static class Product
        {
            public const string Create = "Product.Create";
            public const string Read = "Product.Read";
            public const string Update = "Product.Update";
            public const string Delete = "Product.Delete";

            public static readonly string[] All = { Create, Read, Update, Delete };
        }

        // Category Permissions
        public static class Category
        {
            public const string Create = "Category.Create";
            public const string Read = "Category.Read";
            public const string Update = "Category.Update";
            public const string Delete = "Category.Delete";

            public static readonly string[] All = { Create, Read, Update, Delete };
        }

        // Permission Management Permissions
        public static class Permission
        {
            public const string Assign = "Permission.Assign";
            public const string Revoke = "Permission.Revoke";

            public static readonly string[] All = { Assign, Revoke };
        }

        /// <summary>
        /// Get all permissions from all entities.
        /// </summary>
        public static string[] GetAllPermissions()
        {
            return new[]
            {
                // Product permissions
                Product.Create,
                Product.Read,
                Product.Update,
                Product.Delete,
                // Category permissions
                Category.Create,
                Category.Read,
                Category.Update,
                Category.Delete,
                // Permission management (SuperAdmin only typically)
                Permission.Assign,
                Permission.Revoke,
            };
        }
    }
}
