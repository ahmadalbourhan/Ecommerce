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

        // User Management Permissions
        public static class Users
        {
            public const string Create = "Users.Create";
            public const string Read = "Users.Read";
            public const string Update = "Users.Update";
            public const string Delete = "Users.Delete";

            public static readonly string[] All = { Create, Read, Update, Delete };
        }

        // Role Management Permissions
        public static class Roles
        {
            public const string Create = "Roles.Create";
            public const string Read = "Roles.Read";
            public const string Update = "Roles.Update";
            public const string Delete = "Roles.Delete";
            public const string ManagePermissions = "Roles.ManagePermissions";

            public static readonly string[] All = { Create, Read, Update, Delete, ManagePermissions };
        }

        /// <summary>
        /// Get all permissions from all entities.
        /// </summary>
        public static string[] GetAllPermissions()
        {
            return new[]
            {
                Products.Create,
                Products.Read,
                Products.Update,
                Products.Delete,
                Categories.Create,
                Categories.Read,
                Categories.Update,
                Categories.Delete,
                AdminManagement.Create,
                AdminManagement.Read,
                AdminManagement.Update,
                AdminManagement.Delete,
                AdminManagement.ManagePermissions,
                Users.Create,
                Users.Read,
                Users.Update,
                Users.Delete,
                Roles.Create,
                Roles.Read,
                Roles.Update,
                Roles.Delete,
                Roles.ManagePermissions,
            };
        }
    }
}
