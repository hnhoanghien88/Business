using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace Business.Api.Authorization;

public sealed class PermissionPolicyProvider(IOptions<AuthorizationOptions> options)
    : DefaultAuthorizationPolicyProvider(options)
{
    public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        var configuredPolicy = await base.GetPolicyAsync(policyName);
        if (configuredPolicy is not null)
            return configuredPolicy;

        if (string.IsNullOrWhiteSpace(policyName) || !policyName.Contains('.'))
            return null;

        return new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .AddRequirements(new PermissionRequirement(policyName))
            .Build();
    }
}
