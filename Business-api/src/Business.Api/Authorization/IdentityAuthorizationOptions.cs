namespace Business.Api.Authorization;

public sealed class IdentityAuthorizationOptions
{
    public const string SectionName = "IdentityAuthorization";
    public string BaseUrl { get; init; } = string.Empty;
    public string ApplicationCode { get; init; } = string.Empty;
    public int CacheMinutes { get; init; } = 5;
}
