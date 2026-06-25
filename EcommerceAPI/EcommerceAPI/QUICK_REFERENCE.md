# Permission System - Quick Reference

## Adding a New Entity (2 Steps)

### Step 1: Edit `Constants/Permissions.cs`
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

### Step 2: Update `GetAllPermissions()`
```csharp
Invoice.Create,
Invoice.Read,
Invoice.Update,
Invoice.Delete,
```

**That's it!** Restart the app and permissions are auto-seeded.

---

## Protecting Endpoints (1 Line)

```csharp
[HttpPost("products")]
[HasPermission("Product.Create")]  // ? Add this
public IActionResult CreateProduct() { ... }
```

---

## Existing Permissions

### Product
- `Product.Create` - Create products
- `Product.Read` - View products
- `Product.Update` - Edit products
- `Product.Delete` - Delete products

### Category
- `Category.Create` - Create categories
- `Category.Read` - View categories
- `Category.Update` - Edit categories
- `Category.Delete` - Delete categories

### Permission Management
- `Permission.Assign` - Assign permissions to users
- `Permission.Revoke` - Revoke permissions from users

---

## API Endpoints

### Get User's Permissions
```
GET /api/permissions/users/{userId}
```

### Get All Available Permissions
```
GET /api/permissions/available/all
```

### Assign Permission to Admin User
```
POST /api/permissions/users/{userId}/assign
{
  "permissionSlug": "Product.Create"
}
```

### Revoke Permission from User
```
POST /api/permissions/users/{userId}/revoke
{
  "permissionSlug": "Product.Create"
}
```

### Get Unassigned Permissions
```
GET /api/permissions/users/{userId}/unassigned
```

---

## Default SuperAdmin

| Field | Value |
|-------|-------|
| Username | `superadmin` |
| Email | `superadmin@ecommerce.com` |
| Password | `SuperAdmin@123` |
| Permissions | ALL |

?? **Change password in production!**

---

## Checking Permissions in Code

```csharp
// Check if user has permission
bool hasPermission = await _permissionService.HasPermissionAsync(userId, "Product.Create");

if (!hasPermission)
    throw new UnauthorizedAccessException("No permission");

// Get all user permissions
var permissions = await _permissionService.GetUserPermissionsAsync(userId);

// Assign permission to Admin user
await _permissionService.AssignPermissionToUserAsync(userId, "Product.Create");

// Revoke permission
await _permissionService.RevokePermissionFromUserAsync(userId, "Product.Create");
```

---

## Permission Levels

### SuperAdmin
- Has ALL permissions automatically
- Permissions stored in `RolePermission` table
- Can assign/revoke permissions

### Admin
- Only has explicitly assigned permissions
- Permissions stored in `UserPermission` table
- Cannot manage other users' permissions

---

## Authorization Flow

```
User requests endpoint
    ?
System checks [HasPermission] attribute
    ?
Extracts user ID from authentication claims
    ?
Queries PermissionService.HasPermissionAsync()
    ?
Checks both role and user permissions
    ?
? Access granted OR ? 403 Forbidden
```

---

## File Locations

**Core Files**
- `Constants/Permissions.cs` - Permission definitions
- `Services/PermissionService.cs` - Permission logic
- `Authorization/HasPermissionAttribute.cs` - Attribute
- `Authorization/PermissionAuthorizationFilter.cs` - Filter
- `Seeders/DataSeeder.cs` - Database seeding

**Models**
- `Models/User.cs` - Enhanced with UserPermissions
- `Models/Permission.cs` - Enhanced with UserPermissions
- `Models/UserPermission.cs` - New model for user-level permissions

**API**
- `Controllers/PermissionsController.cs` - Permission endpoints
- `Controllers/Examples/PermissionExampleController.cs` - Usage examples

**Configuration**
- `Program.cs` - Service registration and seeding setup
- `Data/EcommerceDbContext.cs` - Database configuration

---

## Common Tasks

### Task: Add Product.Archive permission
1. Edit `Constants/Permissions.cs`:
   ```csharp
   public const string Archive = "Product.Archive";
   ```
2. Add to `All` array:
   ```csharp
   public static readonly string[] All = { Create, Read, Update, Delete, Archive };
   ```
3. Add to `GetAllPermissions()`:
   ```csharp
   Product.Archive,
   ```
4. Restart app ?

### Task: Protect a DELETE endpoint
```csharp
[HttpDelete("{id}")]
[HasPermission(Permissions.Product.Delete)]
public async Task<IActionResult> DeleteProduct(int id)
{
    return Ok();
}
```

### Task: Give Admin all Product permissions
Call API 4 times:
```
POST /api/permissions/users/{userId}/assign
{ "permissionSlug": "Product.Create" }
{ "permissionSlug": "Product.Read" }
{ "permissionSlug": "Product.Update" }
{ "permissionSlug": "Product.Delete" }
```

---

## Troubleshooting

| Problem | Solution |
|---------|----------|
| Permissions not showing | Delete database, restart app |
| [HasPermission] not working | Full restart needed (not hot reload) |
| Getting 403 on all endpoints | Check user is authenticated with ID claim |
| Can't assign permission | Ensure user is Admin role, not SuperAdmin |

---

## Architecture

```
Permissions.cs (defines what exists)
    ?
DataSeeder (creates on startup)
    ?
PermissionService (query/modify logic)
    ?
IPermissionService (interface)
    ?
[HasPermission] attribute + Filter (protect endpoints)
    ?
API endpoints (manage permissions)
```

---

## Database Tables

**RolePermission** (for SuperAdmin)
- RoleId (FK)
- PermissionId (FK)

**UserPermission** (for Admin users)
- UserId (FK)
- PermissionId (FK)

**Permission**
- Id
- Slug (e.g., "Product.Create")

**User**
- Id
- Name
- Username
- Email
- Password
- UserPermissions (collection)

**Role**
- Id
- Name
- RolePermissions (collection)

---

For detailed information, see:
- `PERMISSION_SYSTEM_GUIDE.md` - Full documentation
- `PERMISSION_IMPLEMENTATION_SUMMARY.md` - Implementation details
- `Controllers/Examples/PermissionExampleController.cs` - Code examples
