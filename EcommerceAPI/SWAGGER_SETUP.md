# Swagger UI Configuration Guide

## Overview
Swagger UI has been successfully integrated into the E-Commerce API project. This provides an interactive API documentation interface that makes it easy to explore and test all API endpoints.

## Setup Details

### 1. NuGet Package Added
- **Package**: `Swashbuckle.AspNetCore` v6.4.0
- **Added to**: `EcommerceAPI.csproj`

### 2. Configuration in Program.cs

#### Services Registration
```csharp
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "E-Commerce API",
        Version = "v1",
        Description = "A comprehensive REST API for e-commerce operations including products, categories, users, roles, and permissions management.",
        Contact = new OpenApiContact
        {
            Name = "E-Commerce API Support",
            Email = "support@ecommerce.com"
        }
    });
});
```

#### Middleware Configuration
```csharp
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "E-Commerce API v1");
        options.RoutePrefix = string.Empty;
    });
}
```

## How to Access Swagger UI

### Running the Application
1. Run the application: `dotnet run`
2. Navigate to: `https://localhost:5001/` (or the configured HTTPS port)
3. Swagger UI will be displayed as the home page

### Swagger JSON Endpoint
- The raw Swagger/OpenAPI specification is available at: `/swagger/v1/swagger.json`

## API Documentation Included

All controllers have been documented with:
- **Summary**: Description of what each endpoint does
- **Parameters**: Details about request parameters
- **Response Types**: HTTP status codes and response types
- **XML Comments**: Comprehensive documentation in Swagger UI

### Documented Controllers

#### 1. **Users Controller** (`api/users`)
- GET `/api/users` - Retrieve all users
- GET `/api/users/{id}` - Retrieve a specific user
- POST `/api/users` - Create a new user
- PUT `/api/users/{id}` - Update an existing user
- DELETE `/api/users/{id}` - Delete a user
- POST `/api/users/{id}/roles` - Assign a role to a user

#### 2. **Roles Controller** (`api/roles`)
- GET `/api/roles` - Retrieve all roles
- GET `/api/roles/{id}` - Retrieve a specific role
- POST `/api/roles` - Create a new role
- PUT `/api/roles/{id}` - Update an existing role
- DELETE `/api/roles/{id}` - Delete a role
- POST `/api/roles/{id}/permissions` - Assign a permission to a role

#### 3. **Permissions Controller** (`api/permissions`)
- GET `/api/permissions` - Retrieve all permissions
- GET `/api/permissions/{id}` - Retrieve a specific permission
- POST `/api/permissions` - Create a new permission
- PUT `/api/permissions/{id}` - Update an existing permission
- DELETE `/api/permissions/{id}` - Delete a permission

#### 4. **Products Controller** (`product`)
- GET `/product` - Retrieve all products
- GET `/product/{id}` - Retrieve a specific product
- GET `/product/user/{userId}` - Retrieve products by user
- POST `/product` - Create a new product
- PUT `/product/{id}` - Update an existing product
- DELETE `/product/{id}` - Delete a product

#### 5. **Categories Controller** (`category`)
- GET `/category` - Retrieve all categories with products
- GET `/category/{id}` - Retrieve a specific category with products
- POST `/category` - Create a new category
- PUT `/category/{id}` - Update an existing category
- DELETE `/category/{id}` - Delete a category

## Features

### Interactive Testing
- **Try It Out**: Each endpoint has a "Try it out" button to test directly from the UI
- **Request/Response Examples**: View sample requests and responses
- **Schema Display**: See the structure of request and response bodies

### Response Codes
Each endpoint documents its possible response codes:
- `200 OK` - Successful GET, PUT, DELETE
- `201 Created` - Successful POST
- `204 No Content` - Successful DELETE or update
- `400 Bad Request` - Invalid input
- `404 Not Found` - Resource not found
- `500 Internal Server Error` - Server error

### Model Schemas
Swagger automatically generates schemas for all DTOs and models:
- Request body schemas
- Response schemas
- Error response schemas

## Development Environment Only

Swagger UI is configured to only run in the **Development** environment:
```csharp
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(...);
}
```

This means:
- ? Available during local development and testing
- ? Disabled in Production environments for security

## Customization

### To Modify Swagger Documentation

1. **Change API Title or Version**:
   Edit the `OpenApiInfo` in `Program.cs`

2. **Add More Details**:
   Add more properties to `OpenApiInfo`:
   ```csharp
   options.SwaggerDoc("v1", new OpenApiInfo
   {
       Title = "E-Commerce API",
       Version = "v1",
       Description = "...",
       TermsOfService = new Uri("https://..."),
       License = new OpenApiLicense { Name = "MIT" },
       Contact = new OpenApiContact { ... }
   });
   ```

3. **Add to Endpoints**:
   Use `[Tags("TagName")]` attribute on controllers:
   ```csharp
   [Tags("User Management")]
   public class UsersController : ControllerBase { }
   ```

## Security Considerations

1. **Disable in Production**: Never expose Swagger UI in production
2. **API Key/Authentication**: Can add authorization headers to Swagger UI
3. **Sensitive Data**: Be careful what information is exposed in documentation

## Testing API Endpoints

### Example: Create a User
1. Open Swagger UI
2. Navigate to POST `/api/users`
3. Click "Try it out"
4. Enter sample JSON:
   ```json
   {
     "name": "John Doe",
     "username": "johndoe",
     "email": "john@example.com",
     "password": "SecurePassword123"
   }
   ```
5. Click "Execute"
6. View the response

## Build Status
? **Build Successful** - All code compiles without errors

## Next Steps
1. Run the application
2. Navigate to the root URL to access Swagger UI
3. Test endpoints using the interactive interface
4. Refer to response documentation for error handling

---

**Note**: The Swagger UI automatically updates whenever you restart the application with code changes.
