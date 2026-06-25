## Permission System Implementation Guide

This document explains how to use the complete permission system implemented in the E-Commerce API.

### Overview

The permission system uses a hierarchical approach:
- **SuperAdmin Role**: Gets ALL permissions automatically on seeding
- **Admin Role**: Can have individual permissions assigned by SuperAdmin
- **Permission Format**: `{Entity}.{Action}` (e.g., `Product.Create`, `Category.Read`)

### 1. Adding New Entities to the Permission System

The system is designed to be easily extensible. To add a new entity:

#### Step 1: Add to the Permissions static class

Edit `Constants/Permissions.cs`:

```csharp
public static class YourNewEntity
{
    public const string Create = "YourNewEntity.Create";
    public const string Read = "YourNewEntity.Read";
    public const string Update = "YourNewEntity.Update";
    public const string Delete = "YourNewEntity.Delete";

    public static readonly string[] All = { Create, Read, Update, Delete };
}
```

#### Step 2: Update the GetAllPermissions method

Add your entity's permissions to the `GetAllPermissions()` method in `Permissions.cs`:

```csharp
public static string[] GetAllPermissions()
{
    return new[]
    {
        // ... existing permissions ...
        YourNewEntity.Create,
        YourNewEntity.Read,
        YourNewEntity.Update,
        YourNewEntity.Delete,
    };
}
```

That's it! The seeder will automatically create these permissions on the next startup.

### 2. Protecting Endpoints with Permissions

Use the `[HasPermission]` attribute on controller actions:

```csharp
[HttpPost]
[HasPermission("Product.Create")]
public async Task<IActionResult> Create(CreateProductDto dto)
{
    // Only users with Product.Create permission can access this
    return Ok(await _productService.CreateAsync(dto));
}

[HttpGet("{id}")]
[HasPermission("Product.Read")]
public async Task<IActionResult> GetById(int id)
{
    // Only users with Product.Read permission can access this
    return Ok(await _productService.GetByIdAsync(id));
}
```

### 3. Assigning Permissions to Admin Users

A SuperAdmin can assign permissions to an Admin user via the API:

**Endpoint**: `POST /api/permissions/users/{userId}/assign`
**Required Permission**: `Permission.Assign`
**Headers**: Must be authenticated as SuperAdmin

**Request Body**:
```json
{
  "permissionSlug": "Product.Create"
}
```

**Response**:
```json
{
  "message": "Permission 'Product.Create' assigned successfully."
}
```

### 4. Revoking Permissions from Admin Users

**Endpoint**: `POST /api/permissions/users/{userId}/revoke`
**Required Permission**: `Permission.Revoke`

**Request Body**:
```json
{
  "permissionSlug": "Product.Create"
}
```

### 5. Getting User Permissions

**Endpoint**: `GET /api/permissions/users/{userId}`

Returns a list of all permissions for a user:
```json
[
  "Product.Create",
  "Product.Read",
  "Product.Update",
  "Category.Read"
]
```

### 6. Getting Available Permissions

**Endpoint**: `GET /api/permissions/available/all`

Returns all available permissions in the system:
```json
[
  "Product.Create",
  "Product.Read",
  "Product.Update",
  "Product.Delete",
  "Category.Create",
  "Category.Read",
  "Category.Update",
  "Category.Delete"
]
```

### 7. Getting Unassigned Permissions for a User

**Endpoint**: `GET /api/permissions/users/{userId}/unassigned`

Returns permissions not yet assigned to a specific user.

### 8. Database Migration

After adding the new `UserPermission` table model, you need to create and apply a migration:

```bash
# In the Package Manager Console or terminal from the project root
Add-Migration AddUserPermissionTable
Update-Database
```

### 9. Default SuperAdmin Credentials

The seeder creates a default SuperAdmin user on the first run:

- **Username**: superadmin
- **Email**: superadmin@ecommerce.com
- **Password**: SuperAdmin@123 (?? Change this in production!)

### 10. Architecture Overview

```
Permissions.cs (Constants)
    ?
DataSeeder (Creates roles, permissions, and SuperAdmin on startup)
    ?
IPermissionService / PermissionService (Business logic)
    ?
PermissionsController (API endpoints)
    ?
[HasPermission] Attribute + PermissionAuthorizationFilter (Endpoint protection)
```

### 11. Permission Storage

- **SuperAdmin Permissions**: Stored in `RolePermission` table (role-level)
- **Admin Permissions**: Stored in `UserPermission` table (user-level)
- **Role-based permissions**: Loaded from role associations
- **User-level permissions**: Only for Admin role users

### 12. Example: Creating a New Entity System

Let's say you want to add an "Invoice" entity:

1. Add to `Constants/Permissions.cs`:
```csharp
public static class Invoice
{
    public const string Create = "Invoice.Create";
    public const string Read = "Invoice.Read";
    public const string Update = "Invoice.Update";
    public const string Delete = "Invoice.Delete";

    public static readonly string[] All = { Create, Read, Update, Delete };
}
```

2. Update `Permissions.GetAllPermissions()`:
```csharp
Invoice.Create,
Invoice.Read,
Invoice.Update,
Invoice.Delete,
```

3. Protect your controller methods:
```csharp
[HttpPost]
[HasPermission("Invoice.Create")]
public async Task<IActionResult> CreateInvoice(CreateInvoiceDto dto) { ... }
```

4. Restart the application - the seeder will automatically:
   - Create the 4 new permissions
   - Assign them to the SuperAdmin role
   - Make them available for Admin assignment

### 13. Authentication Integration

The current implementation assumes you have user authentication in place with claims containing the user ID. The `PermissionAuthorizationFilter` looks for `ClaimTypes.NameIdentifier` to get the user ID.

Make sure your authentication system sets this claim:
```csharp
var claims = new List<Claim>
{
    new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
    // ... other claims ...
};
```

### 14. Checking Permissions Programmatically

In a service or controller, you can check permissions programmatically:

```csharp
public class ProductService
{
    private readonly IPermissionService _permissionService;

    public ProductService(IPermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    public async Task<Product> CreateProductAsync(int userId, CreateProductDto dto)
    {
        if (!await _permissionService.HasPermissionAsync(userId, "Product.Create"))
        {
            throw new UnauthorizedAccessException("User does not have permission to create products.");
        }

        // Create product logic here
    }
}
```

### 15. Troubleshooting

**Issue**: Permissions not being assigned to SuperAdmin on startup
- **Solution**: Delete the database or the relevant tables and restart. The seeder will recreate everything.

**Issue**: Permission attribute not working
- **Solution**: Ensure `PermissionAuthorizationFilter` is registered in `Program.cs` and the user is authenticated.

**Issue**: Getting "User does not have permission" errors
- **Solution**: Check that the user's role or the user themselves have the required permission assigned via the API or database.
