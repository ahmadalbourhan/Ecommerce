using EcommerceAPI.Constants;
using Microsoft.AspNetCore.Authorization;

namespace EcommerceAPI.Authorization
{
    public static class PermissionPolicyExtensions
    {
        public static IServiceCollection AddPermissionAuthorization(this IServiceCollection services)
        {
            services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

            services.AddAuthorization(options =>
            {
                foreach (var permission in Permissions.GetAllPermissions())
                {
                    options.AddPolicy(
                        PermissionPolicies.Name(permission),
                        policy => policy.Requirements.Add(new PermissionRequirement(permission)));
                }
            });

            return services;
        }
    }
}
