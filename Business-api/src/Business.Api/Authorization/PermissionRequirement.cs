using Microsoft.AspNetCore.Authorization;

namespace Business.Api.Authorization;

public sealed record PermissionRequirement(string Permission)
    : IAuthorizationRequirement;
