using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;

namespace Business.Api.Authorization;

public interface IIdentityPermissionService
{
    Task<bool> HasPermissionAsync(
        string userId,
        string permissionVersion,
        string accessToken,
        string permission,
        CancellationToken cancellationToken);
}

public sealed class IdentityPermissionService(
    HttpClient httpClient,
    IOptions<IdentityAuthorizationOptions> options) : IIdentityPermissionService
{
    private readonly IdentityAuthorizationOptions _options = options.Value;

    public async Task<bool> HasPermissionAsync(
        string userId,
        string permissionVersion,
        string accessToken,
        string permission,
        CancellationToken cancellationToken)
    {
        var query = new URLSearchParams(_options.ApplicationCode);
        using var request = new HttpRequestMessage(HttpMethod.Get, $"authorization?{query}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (response.StatusCode is System.Net.HttpStatusCode.Unauthorized
            or System.Net.HttpStatusCode.Forbidden)
            return false;

        response.EnsureSuccessStatusCode();
        var authorization = await response.Content.ReadFromJsonAsync<AuthorizationResponse>(
            cancellationToken: cancellationToken);
        return authorization?.Permissions.Contains(permission, StringComparer.Ordinal) == true;
    }

    private sealed record AuthorizationResponse(IReadOnlyList<string> Permissions);

    private sealed class URLSearchParams(string applicationCode)
    {
        public override string ToString() =>
            $"applicationCode={Uri.EscapeDataString(applicationCode)}";
    }
}
