# EcommerceAPI – Frontend Endpoint Reference

This file lists the HTTP endpoints currently implemented in this backend, so a frontend can integrate without reading controller code.

## Base URLs

- **Swagger UI**: `/`
- **Swagger JSON**: `/swagger/v1/swagger.json`
- **Typical dev HTTPS**: `https://localhost:7108`

## Auth

This API uses JWT Bearer authentication.

- **Send token like this**:
  - `Authorization: Bearer <access_token>`

## Response envelopes

Some endpoints return a `ResponseDto<T>` envelope (status/message/data). Others return raw entities (e.g. `User`, `Role`, `Permission`) or simple `{ message }` objects.

---

## Authentication (`AuthController`)

Base route: `/api/Auth`

- **POST** `/api/Auth/login`
  - **Body**: `LoginRequest` (email + password)
  - **Returns**: `LoginResponse` `{ accessToken, refreshToken, user { id, email, fullName, role, permissions[] } }`
  - **Auth**: AllowAnonymous

- **POST** `/api/Auth/refresh-token`
  - **Body**: `RefreshTokenRequest` `{ refreshToken }`
  - **Returns**: `RefreshTokenResponse` `{ accessToken, refreshToken }`
  - **Auth**: AllowAnonymous

- **POST** `/api/Auth/logout`
  - **Returns**: `{ message }`
  - **Auth**: Bearer token required

---

## Products (`ProductController`)

Base route: `/Product`

- **GET** `/Product`
  - **Returns**: `ResponseDto<IEnumerable<Product>>`

- **GET** `/Product/{id}`
  - **Returns**: `ResponseDto<Product>`

- **GET** `/Product/user/{userId}`
  - **Returns**: `ResponseDto<IEnumerable<Product>>`

- **POST** `/Product`
  - **Body**: `Product`
  - **Returns**: `ResponseDto<Product>` (201)

- **PUT** `/Product/{id}`
  - **Body**: `Product` (must match `{id}`)
  - **Returns**: `ResponseDto<Product>`

- **DELETE** `/Product/{id}`
  - **Returns**: `ResponseDto`

---

## Categories (`CategoryController`)

Base route: `/Category`

- **GET** `/Category`
  - **Returns**: `ResponseDto<IEnumerable<CategoryWithProductsDto>>`

- **GET** `/Category/{id}`
  - **Returns**: `ResponseDto<CategoryWithProductsDto>`

- **POST** `/Category`
  - **Body**: `CreateCategoryDto`
  - **Returns**: `ResponseDto<CategoryWithProductsDto>` (201)

- **PUT** `/Category/{id}`
  - **Body**: `UpdateCategoryDto`
  - **Returns**: `ResponseDto<CategoryWithProductsDto>`

- **DELETE** `/Category/{id}`
  - **Returns**: `ResponseDto`

---

## Users (`UsersController`)

Base route: `/api/Users`

- **GET** `/api/Users`
  - **Returns**: `IEnumerable<User>` (raw)

- **GET** `/api/Users/{id}`
  - **Returns**: `User` (raw)

- **POST** `/api/Users`
  - **Body**: `User` (raw entity)
  - **Returns**: `User` (201, raw)

- **PUT** `/api/Users/{id}`
  - **Body**: `User` (raw entity)
  - **Returns**: 204 NoContent

- **DELETE** `/api/Users/{id}`
  - **Returns**: 204 NoContent

- **POST** `/api/Users/{id}/roles`
  - **Body**: `{ "roleId": number }`
  - **Returns**: 204 NoContent

---

## Roles (`RolesController`)

Base route: `/api/Roles`

- **GET** `/api/Roles`
  - **Returns**: `IEnumerable<Role>` (raw)

- **GET** `/api/Roles/{id}`
  - **Returns**: `Role` (raw)

- **POST** `/api/Roles`
  - **Body**: `Role` (raw entity)
  - **Returns**: `Role` (201, raw)

- **PUT** `/api/Roles/{id}`
  - **Body**: `Role` (raw entity)
  - **Returns**: 204 NoContent

- **DELETE** `/api/Roles/{id}`
  - **Returns**: 204 NoContent

- **POST** `/api/Roles/{id}/permissions`
  - **Body**: `{ "permissionId": number }`
  - **Returns**: 204 NoContent

---

## Permissions (`PermissionsController`)

Base route: `/api/Permissions`

- **GET** `/api/Permissions`
  - **Returns**: `IEnumerable<Permission>` (raw)

- **GET** `/api/Permissions/{id}`
  - **Returns**: `Permission` (raw)

- **POST** `/api/Permissions`
  - **Body**: `Permission` (raw entity)
  - **Returns**: `Permission` (201, raw)

- **PUT** `/api/Permissions/{id}`
  - **Body**: `Permission` (raw entity)
  - **Returns**: 204 NoContent

- **DELETE** `/api/Permissions/{id}`
  - **Returns**: 204 NoContent

- **GET** `/api/Permissions/available/all`
  - **Returns**: `List<string>` of permission slugs

- **GET** `/api/Permissions/users/{userId}`
  - **Returns**: `List<string>` permission slugs for a user

- **GET** `/api/Permissions/users/{userId}/unassigned`
  - **Returns**: `List<string>` permission slugs not assigned to that user

- **POST** `/api/Permissions/users/{userId}/assign`
  - **Body**: `{ "permissionSlug": "Permission.Assign" }`
  - **Auth**: Requires `[HasPermission("Permission.Assign")]`
  - **Returns**: `{ message }`

- **POST** `/api/Permissions/users/{userId}/revoke`
  - **Body**: `{ "permissionSlug": "Permission.Revoke" }`
  - **Auth**: Requires `[HasPermission("Permission.Revoke")]`
  - **Returns**: `{ message }`

---

## Admin management (`AdminsController`)

Base route: `/api/Admins`

All endpoints require Bearer token + permission checks via `[HasPermission(...)]`.

- **POST** `/api/Admins`
  - **Body**: `CreateAdminDto`
  - **Permission**: `AdminManagement.Create`
  - **Returns**: `AdminDetailDto` (201)

- **GET** `/api/Admins`
  - **Permission**: `AdminManagement.Read`
  - **Returns**: `List<AdminDetailDto>`

- **GET** `/api/Admins/{id}`
  - **Permission**: `AdminManagement.Read`
  - **Returns**: `AdminDetailDto`

- **POST** `/api/Admins/{id}/permissions`
  - **Body**: `AssignPermissionsDto` (list of permission slugs)
  - **Permission**: `AdminManagement.ManagePermissions`
  - **Returns**: `{ message }`

- **DELETE** `/api/Admins/{id}/permissions`
  - **Body**: `AssignPermissionsDto` (list of permission slugs)
  - **Permission**: `AdminManagement.ManagePermissions`
  - **Returns**: `{ message }`

- **PATCH** `/api/Admins/{id}/status`
  - **Body**: `UpdateAdminStatusDto` `{ isActive: boolean }`
  - **Permission**: `AdminManagement.Update`
  - **Returns**: `{ message }`

- **DELETE** `/api/Admins/{id}`
  - **Permission**: `AdminManagement.Delete`
  - **Returns**: `{ message }`

---

## Examples (do not integrate) (`PermissionExampleController`)

Base route: `/api/PermissionExample`

This controller is documentation-only and exists to demonstrate the permission attribute usage.

