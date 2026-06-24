# Quick Start Guide - E-Commerce API with Swagger

## Prerequisites
- .NET 8.0 SDK or later
- SQL Server (local or remote)
- Visual Studio, VS Code, or another code editor

## Setup Instructions

### 1. Configure Database Connection
Edit `appsettings.json` and set your SQL Server connection string:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=EcommerceDB;Trusted_Connection=true;"
  }
}
```

### 2. Apply Database Migrations
```bash
dotnet ef database update
```

This will:
- Create all database tables
- Set up relationships and constraints
- Initialize the database schema

### 3. Run the Application
```bash
dotnet run
```

Or press `F5` in Visual Studio.

### 4. Access Swagger UI
Open your browser and navigate to:
- **Development**: `https://localhost:5001/` (or your configured port)
- Swagger UI will be displayed as the home page

## API Endpoints Overview

### Base URL
```
https://localhost:5001
```

### Available Controllers

| Controller | Base Route | Purpose |
|-----------|-----------|---------|
| Users | `/api/users` | User management |
| Roles | `/api/roles` | Role management |
| Permissions | `/api/permissions` | Permission management |
| Products | `/product` | Product management |
| Categories | `/category` | Category management |

## Testing with Swagger UI

### Example 1: Create a User
1. Open Swagger UI
2. Find "POST /api/users" under Users
3. Click "Try it out"
4. Paste this JSON:
```json
{
  "name": "John Doe",
  "username": "johndoe",
  "email": "john@example.com",
  "password": "SecurePassword123"
}
```
5. Click "Execute"
6. Check the response

### Example 2: Create a Product
1. Find "POST /product" under Products
2. Click "Try it out"
3. Paste this JSON:
```json
{
  "categoryId": 1,
  "userId": 1,
  "name": "Laptop",
  "cost": 500.00,
  "price": 899.99,
  "image": "laptop.jpg"
}
```
4. Click "Execute"

### Example 3: Get Products by User
1. Find "GET /product/user/{userId}" under Products
2. Click "Try it out"
3. Enter userId: `1`
4. Click "Execute"

## Project Structure

```
EcommerceAPI/
??? Controllers/           # API endpoints
??? Models/               # Database models
??? Services/             # Business logic
??? Repositories/         # Data access layer
??? Data/                 # DbContext and migrations
??? DTOs/                 # Data transfer objects
??? Migrations/           # Database migrations
??? Program.cs            # Application configuration
??? EcommerceAPI.csproj   # Project file
```

## Key Features

? **Swagger UI** - Interactive API documentation  
? **RESTful API** - Standard REST conventions  
? **Entity Framework Core** - ORM for database access  
? **Dependency Injection** - Loose coupling and testability  
? **Async/Await** - Non-blocking operations  
? **Error Handling** - Comprehensive error responses  
? **Response DTOs** - Structured API responses  

## Troubleshooting

### Database Connection Error
- Verify SQL Server is running
- Check connection string in `appsettings.json`
- Ensure database user has appropriate permissions

### Swagger UI Not Showing
- Check if running in Development environment
- Verify `UseSwagger()` and `UseSwaggerUI()` are called
- Clear browser cache and reload

### Port Already in Use
- Change port in `appsettings.json` or `launchSettings.json`
- Or kill the process using the port

### Build Fails
```bash
dotnet clean
dotnet build
```

## Common API Responses

### Success Response (200 OK)
```json
{
  "statusCode": 200,
  "message": "Operation successful",
  "data": { ... },
  "isSuccess": true
}
```

### Error Response (400/404/500)
```json
{
  "statusCode": 400,
  "message": "Error description",
  "isSuccess": false
}
```

## Environment Configuration

### Development
- Swagger UI enabled
- Detailed error messages
- Hot reload enabled

### Production
- Swagger UI disabled
- Generic error messages
- Optimizations enabled

## Performance Tips

1. **Use Indexes**: Database queries use indexed foreign keys
2. **Async Operations**: All DB calls are async for better scalability
3. **Pagination**: Consider adding pagination for large datasets
4. **Caching**: Can be added to frequently accessed data

## Security Notes

1. **Never commit** database connection strings with real credentials
2. **Use Environment Variables** for sensitive configuration
3. **Hash passwords** before storing (implement in production)
4. **Disable Swagger** in production environments
5. **Add Authentication** for production use (JWT, OAuth, etc.)

## Documentation Files

- `IMPLEMENTATION_SUMMARY.md` - Complete implementation details
- `SWAGGER_SETUP.md` - Swagger configuration guide
- This file - Quick start guide

## Support

For issues or questions:
1. Check the documentation files
2. Review error messages in the output
3. Check SQL Server logs
4. Review application logs in console

---

**Happy API Development! ??**
