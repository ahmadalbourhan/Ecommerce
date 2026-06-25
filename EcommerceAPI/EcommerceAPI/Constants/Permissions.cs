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
        public static class Products
        {
            public const string Create = "Products.Create";
            public const string Read = "Products.Read";
            public const string Update = "Products.Update";
            public const string Delete = "Products.Delete";

            public static readonly string[] All = { Create, Read, Update, Delete };
        }

        // Category Permissions
        public static class Categories
        {
            public const string Create = "Categories.Create";
            public const string Read = "Categories.Read";
            public const string Update = "Categories.Update";
            public const string Delete = "Categories.Delete";

            public static readonly string[] All = { Create, Read, Update, Delete };
        }

        // Admin Management Permissions
        public static class AdminManagement
        {
            public const string Create = "AdminManagement.Create";
            public const string Read = "AdminManagement.Read";
            public const string Update = "AdminManagement.Update";
            public const string Delete = "AdminManagement.Delete";
            public const string ManagePermissions = "AdminManagement.ManagePermissions";

            public static readonly string[] All = { Create, Read, Update, Delete, ManagePermissions };
        }

        /// <summary>
        /// Get all permissions from all entities.
        /// </summary>
        public static string[] GetAllPermissions()
        {
            return new[]
            {
                // Product permissions
                Products.Create,
                Products.Read,
                Products.Update,
                Products.Delete,
                // Category permissions
                Categories.Create,
                Categories.Read,
                Categories.Update,
                Categories.Delete,
                // Admin management permissions
                AdminManagement.Create,
                AdminManagement.Read,
                AdminManagement.Update,
                AdminManagement.Delete,
                AdminManagement.ManagePermissions,
            };
        }
    }
}
