using UserManagement.Domain.Enums;

namespace UserManagement.API.Extensions;

public static class AuthorizationExtensions
{
    public static IServiceCollection AddRoleAuthorization(
        this IServiceCollection services)
    {
        services.AddAuthorization(opts =>
        {
            opts.AddPolicy("AdminOnly", policy =>
                policy.RequireRole(UserRole.Administrator.ToString()));
        });

        return services;
    }
}