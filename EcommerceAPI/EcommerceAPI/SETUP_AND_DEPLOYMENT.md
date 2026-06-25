# Permission System - Setup & Deployment Guide

## Prerequisites

- .NET 8
- SQL Server
- Entity Framework Core

## Initial Setup Steps

### 1. Verify Files Are In Place

After the implementation, verify these key files exist:

```
EcommerceAPI/
??? Constants/
?   ??? Permissions.cs ?
??? Models/
?   ??? User.cs (modified) ?
?   ??? Permission.cs (modified) ?
?   ??? UserPermission.cs ?
??? Services/
?   ??? IPermissionService.cs (modified) ?
?   ??? PermissionService.cs (modified) ?
??? Authorization/
?   ??? HasPermissionAttribute.cs ?
?   ??? PermissionAuthorizationFilter.cs ?
??? Seeders/
?   ??? DataSeeder.cs ?
??? Data/
?   ??? EcommerceDbContext.cs (modified) ?
??? Controllers/
?   ??? PermissionsController.cs (modified) ?
?   ??? Examples/
?       ??? PermissionExampleController.cs ?
??? Program.cs (modified) ?
??? Documentation/
    ??? PERMISSION_SYSTEM_GUIDE.md ?
    ??? QUICK_REFERENCE.md ?
    ??? PERMISSION_IMPLEMENTATION_SUMMARY.md ?
```

### 2. Restore Dependencies

```bash
dotnet restore
```

### 3. Update Database with Migration

```bash
# Option 1: Using Package Manager Console (Visual Studio)
Add-Migration AddUserPermissionTable
Update-Database

# Option 2: Using dotnet CLI
dotnet ef migrations add AddUserPermissionTable
dotnet ef database update
```

If the migration already exists (because EF Core detected changes), just run:
```bash
Update-Database
```

### 4. Build the Solution

```bash
dotnet build
```

Should build without errors. (Note: Hot reload may show Edit & Continue warnings which are harmless)

### 5. Run the Application

```bash
dotnet run
```

Or press F5 in Visual Studio.

**First Run Behavior:**
- DataSeeder runs automatically
- Creates SuperAdmin and Admin roles
- Seeds all permissions from `Permissions` class
- Creates default SuperAdmin user
- Logs all actions to console

### 6. Verify Setup

Check the console output for messages like:
```
Database seeding completed successfully
Roles seeded successfully
Seeded 12 permissions
Assigned 12 permissions to SuperAdmin role
Default SuperAdmin user created - Username: superadmin, Email: superadmin@ecommerce.com
```

## Testing the System

### Via Swagger UI

1. Navigate to `https://localhost:5001` (or your Swagger root)
2. Try the new permission endpoints:
   - `GET /api/permissions/available/all`
   - `GET /api/permissions/users/1`
   - etc.

### Creating Test Users

You'll need to integrate this with your authentication system. Example flow:

1. **Create Admin User**
   ```sql
   INSERT INTO Users (Name, Username, Email, Password) 
   VALUES ('Test Admin', 'testadmin', 'testadmin@ecommerce.com', 'hashedpassword');

   INSERT INTO UserRoles (UserId, RoleId) 
   VALUES ((SELECT Id FROM Users WHERE Username = 'testadmin'), 
           (SELECT Id FROM Roles WHERE Name = 'Admin'));
   ```

2. **Assign Permissions via API**
   ```
   POST /api/permissions/users/{adminUserId}/assign
   {
     "permissionSlug": "Product.Create"
   }
   ```

3. **Test Protected Endpoints**
   - SuperAdmin should access all endpoints
   - Admin should access only assigned permissions
   - Others should get 403 Forbidden

## Applying to Your Controllers

### Before

```csharp
[HttpPost("products")]
public async Task<IActionResult> CreateProduct(CreateProductDto dto)
{
    return Ok(await _productService.CreateAsync(dto));
}
```

### After

```csharp
using EcommerceAPI.Constants;      // Add this
using EcommerceAPI.Authorization;  // Add this

[HttpPost("products")]
[HasPermission(Permissions.Product.Create)]  // Add this line
public async Task<IActionResult> CreateProduct(CreateProductDto dto)
{
    return Ok(await _productService.CreateAsync(dto));
}
```

## Extending with New Entities

### Add Invoice Entity (Example)

1. **Create/Update your Invoice model**
   ```csharp
   public class Invoice
   {
       public int Id { get; set; }
       public int UserId { get; set; }
       // ... other properties
   }
   ```

2. **Add to Permissions.cs**
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

3. **Update GetAllPermissions()**
   ```csharp
   public static string[] GetAllPermissions()
   {
       return new[]
       {
           // ... existing ...
           Invoice.Create,
           Invoice.Read,
           Invoice.Update,
           Invoice.Delete,
       };
   }
   ```

4. **Protect your endpoints**
   ```csharp
   [HasPermission(Permissions.Invoice.Create)]
   public async Task<IActionResult> CreateInvoice(CreateInvoiceDto dto) { ... }
   ```

5. **Restart the app** - Permissions are auto-seeded!

## Deployment Checklist

- [ ] Database migrations applied (`Update-Database`)
- [ ] Application builds successfully (`dotnet build`)
- [ ] DataSeeder runs on startup (check logs)
- [ ] Default SuperAdmin created with correct credentials
- [ ] All permission constants defined in `Permissions.cs`
- [ ] Protected endpoints have `[HasPermission]` attributes
- [ ] Authentication system sets `ClaimTypes.NameIdentifier` claim
- [ ] SuperAdmin role can assign permissions to Admin users
- [ ] Admin users can only access permitted endpoints
- [ ] Documentation reviewed and updated for your team

## Production Considerations

### Security

1. **Change Default Password**
   ```csharp
   // In DataSeeder.cs, update the default password
   Password = "YourSecurePasswordHere"
   ```

2. **Use Proper Password Hashing**
   - Consider using ASP.NET Core Identity instead of plain text
   - Or implement bcrypt/Argon2 hashing

3. **Enable HTTPS**
   - All authentication-related traffic must use HTTPS
   - Disable HTTP in production

4. **API Key/JWT Integration**
   - Consider requiring API keys for permission assignment endpoints
   - Add rate limiting to prevent brute force

### Monitoring

1. **Log Permission Denials**
   - Monitor 403 Forbidden responses
   - Alert on repeated failed permission checks

2. **Audit Trail**
   - Log all permission assignments/revocations
   - Consider adding timestamp to UserPermission table

3. **Performance**
   - Monitor `GetUserPermissionsAsync()` performance
   - Consider caching permission checks
   - Add indexes on UserId, PermissionId

### Backup

1. **Database Backups**
   - Include the new `UserPermission` table in backups
   - Test restore procedures

2. **Permission Configuration Backup**
   - Keep `Permissions.cs` under version control
   - Document all available permissions

## Troubleshooting

### Issue: "No suitable constructor found"

**Cause**: Dependencies not injected correctly

**Solution**: 
```csharp
// Make sure EcommerceDbContext is registered in Program.cs
builder.Services.AddDbContext<EcommerceDbContext>(options => ...);
builder.Services.AddScoped<IPermissionService, PermissionService>();
```

### Issue: Permissions not persisting

**Cause**: Database context changes not saved

**Solution**: Ensure `SaveChangesAsync()` is called:
```csharp
await _context.UserPermissions.AddAsync(userPermission);
await _context.SaveChangesAsync();  // Don't forget this!
```

### Issue: Authorization filter not triggering

**Cause**: Filter not registered globally

**Solution**: Verify in Program.cs:
```csharp
builder.Services.AddControllers(options =>
{
    options.Filters.Add<PermissionAuthorizationFilter>();
});
```

### Issue: Foreign key constraint violations

**Cause**: Trying to assign non-existent permission or user

**Solution**: 
```csharp
// Verify both user and permission exist first
var user = await _context.Users.FindAsync(userId);
var permission = await _context.Permissions.FirstOrDefaultAsync(p => p.Slug == permissionSlug);

if (user == null || permission == null)
    return BadRequest("Invalid user or permission");
```

## Rollback Instructions

If you need to remove the permission system:

1. **Delete Migration**
   ```bash
   Remove-Migration AddUserPermissionTable
   ```

2. **Revert Changes**
   - Restore previous versions of modified files from version control
   - Remove new files

3. **Update Database**
   ```bash
   Update-Database
   ```

## Version Updates

When updating the permission system:

1. Always back up your database first
2. New permissions are auto-added to SuperAdmin on restart
3. Existing user permissions are preserved
4. Old permissions are not removed automatically

## Getting Help

If issues arise:

1. Check `PERMISSION_SYSTEM_GUIDE.md` for detailed documentation
2. Review `QUICK_REFERENCE.md` for common tasks
3. Look at `Controllers/Examples/PermissionExampleController.cs` for usage patterns
4. Check application logs for seeding errors
5. Verify database schema matches expected structure

## Next Steps

1. ? Complete initial setup (this guide)
2. ? Apply permission attributes to all endpoints
3. ? Create authentication integration
4. ? Set up admin UI for permission management
5. ? Add audit logging
6. ? Configure monitoring and alerting

Congratulations! Your permission system is ready for production use.
