using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace EcommerceAPI.Authorization
{
    public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly ILogger<PermissionAuthorizationHandler> _logger;

        public PermissionAuthorizationHandler(ILogger<PermissionAuthorizationHandler> logger)
        {
            _logger = logger;
        }

        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PermissionRequirement requirement)
        {
            // SuperAdmin always has access
            var userRole = context.User.FindFirst("role")?.Value;
            if (userRole == "SuperAdmin")
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            // Check for permission claim
            var hasClaim = context.User.FindAll(ClaimTypes.Role)
                .Union(context.User.FindAll("role"))
                .Any(c => c.Value == requirement.Permission);

            if (!hasClaim)
            {
                hasClaim = context.User.FindAll("permission")
                    .Any(c => c.Value == requirement.Permission);
            }

            if (hasClaim)
            {
                context.Succeed(requirement);
            }
            else
            {
                _logger.LogWarning($"User {context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value} denied access to {requirement.Permission}");
            }

            return Task.CompletedTask;
        }
    }
}
