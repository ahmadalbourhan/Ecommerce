using EcommerceAPI.Authorization;
using EcommerceAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace EcommerceAPI.Authorization
{
    public class PermissionAuthorizationFilter : IAsyncAuthorizationFilter
    {
        private readonly IPermissionService _permissionService;
        private readonly ILogger<PermissionAuthorizationFilter> _logger;

        public PermissionAuthorizationFilter(IPermissionService permissionService, ILogger<PermissionAuthorizationFilter> logger)
        {
            _permissionService = permissionService;
            _logger = logger;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var endpoint = context.HttpContext.GetEndpoint();

            // Skip permission checks for endpoints marked AllowAnonymous
            if (endpoint?.Metadata?.GetMetadata<AllowAnonymousAttribute>() != null)
            {
                return;
            }

            // Get the HasPermission attribute from the endpoint (action or controller)
            var hasPermissionAttribute = endpoint?.Metadata?.OfType<HasPermissionAttribute>().FirstOrDefault();

            // If no HasPermission attribute, do not enforce permission checks here
            // (endpoint can still be protected by standard [Authorize] attributes/policies).
            if (hasPermissionAttribute == null)
            {
                return;
            }

            // If endpoint requires permission, ensure the user is authenticated
            var user = context.HttpContext.User;
            if (user?.Identity == null || !user.Identity.IsAuthenticated)
            {
                _logger.LogWarning("Unauthenticated request to a permission-protected endpoint");
                // Challenge will prompt authentication handlers (returns 401)
                context.Result = new ChallengeResult();
                return;
            }

            // Extract user id and role from claims
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
            var userRole = user.FindFirst("role")?.Value;

            // SuperAdmin bypasses permission checks
            if (userRole == "SuperAdmin")
            {
                return;
            }

            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                _logger.LogWarning("User authenticated but userId claim missing or invalid");
                context.Result = new ForbidResult();
                return;
            }

            // Check permission
            var hasPermission = await _permissionService.HasPermissionAsync(userId, hasPermissionAttribute.Permission);
            if (!hasPermission)
            {
                _logger.LogWarning($"User {userId} does not have permission '{hasPermissionAttribute.Permission}'");
                context.Result = new ForbidResult();
                return;
            }

            _logger.LogInformation($"User {userId} authorized for permission '{hasPermissionAttribute.Permission}'");
        }
    }
}
