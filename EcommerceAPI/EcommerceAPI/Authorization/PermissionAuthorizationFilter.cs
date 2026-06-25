using EcommerceAPI.Authorization;
using EcommerceAPI.Services;
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
            // Get the HasPermission attribute from the controller or action
            var hasPermissionAttribute = context.ActionDescriptor.EndpointMetadata
                .OfType<HasPermissionAttribute>()
                .FirstOrDefault();

            // If no permission attribute, allow access
            if (hasPermissionAttribute == null)
            {
                return;
            }

            // Check if user is authenticated
            var userIdClaim = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                _logger.LogWarning("User not authenticated or userId not found in claims");
                context.Result = new UnauthorizedResult();
                return;
            }

            // Check if user has the required permission
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
