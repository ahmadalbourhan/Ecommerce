using EcommerceAPI.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace EcommerceAPI.Authorization
{
    public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly IPermissionService _permissionService;
        private readonly ILogger<PermissionAuthorizationHandler> _logger;

        public PermissionAuthorizationHandler(
            IPermissionService permissionService,
            ILogger<PermissionAuthorizationHandler> logger)
        {
            _permissionService = permissionService;
            _logger = logger;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PermissionRequirement requirement)
        {
            if (context.User.Identity?.IsAuthenticated != true)
            {
                return;
            }

            var userRole = context.User.FindFirst("role")?.Value;
            if (string.Equals(userRole, "SuperAdmin", StringComparison.OrdinalIgnoreCase))
            {
                context.Succeed(requirement);
                return;
            }

            var hasPermissionClaim = context.User.FindAll("permission")
                .Any(c => c.Value == requirement.Permission);

            if (hasPermissionClaim)
            {
                context.Succeed(requirement);
                return;
            }

            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId))
            {
                if (await _permissionService.HasPermissionAsync(userId, requirement.Permission))
                {
                    context.Succeed(requirement);
                    return;
                }
            }

            _logger.LogWarning(
                "User {UserId} denied access to permission {Permission}",
                userIdClaim?.Value ?? "unknown",
                requirement.Permission);
        }
    }
}
