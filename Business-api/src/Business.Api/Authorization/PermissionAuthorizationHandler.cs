using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace Business.Api.Authorization;

public sealed class PermissionAuthorizationHandler(
    IIdentityPermissionService permissions,
    IHttpContextAccessor httpContextAccessor)
    : AuthorizationHandler<PermissionRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var httpContext = httpContextAccessor.HttpContext;
        var userId = context.User.FindFirstValue("sub");
        var permissionVersion = context.User.FindFirstValue("permissionversion");
        var authorization = httpContext?.Request.Headers.Authorization.ToString();
        var accessToken = authorization?.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) == true
            ? authorization["Bearer ".Length..].Trim()
            : httpContext?.Request.Path.StartsWithSegments("/hubs") == true
                ? httpContext.Request.Query["access_token"].ToString()
                : null;

        if (userId is null
            || permissionVersion is null
            || string.IsNullOrWhiteSpace(accessToken))
            return;

        if (await permissions.HasPermissionAsync(
                userId,
                permissionVersion,
                accessToken,
                requirement.Permission,
                httpContext!.RequestAborted))
            context.Succeed(requirement);
    }
}
