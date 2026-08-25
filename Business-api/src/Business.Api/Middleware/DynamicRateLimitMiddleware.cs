using System.Security.Claims;
using Business.Domain.Entities;
using Business.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Business.Api.Middleware;

public sealed class RateLimitPolicyProvider(BusinessDbContext db, IMemoryCache cache, IOptions<RateLimitingOptions> options)
{
    private const string CacheKey = "rate-limit-policies";
    public Task<List<RateLimitPolicy>> GetAsync(CancellationToken token) => cache.GetOrCreateAsync(CacheKey, entry =>
    {
        entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(options.Value.PolicyCacheSeconds);
        return db.RateLimitPolicies.AsNoTracking().Where(x => x.IsActive && !x.IsDeleted).OrderByDescending(x => x.Priority).ToListAsync(token);
    })!;
    public void Invalidate() => cache.Remove(CacheKey);
}

public sealed class DynamicRateLimitMiddleware(RequestDelegate next, ILogger<DynamicRateLimitMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context, RateLimitPolicyProvider provider, IRateLimitStore store, IOptions<RateLimitingOptions> options)
    {
        if (!options.Value.Enabled) { await next(context); return; }
        var route = (context.GetEndpoint() as RouteEndpoint)?.RoutePattern.RawText?.TrimStart('/');
        if (string.IsNullOrWhiteSpace(route)) { await next(context); return; }
        var policies = await provider.GetAsync(context.RequestAborted); var leases = new List<RateLimitLease>();
        try
        {
            foreach (var policy in policies.Where(x => Matches(x, route, context.Request.Method)))
            {
                RateLimitLease lease;
                try { lease = await store.AcquireAsync(BuildKey(options.Value, policy, context, route), policy.Algorithm, checked((int)policy.PermitLimit), checked((int)policy.WindowSeconds), checked((int)(policy.BurstLimit ?? 0)), context.RequestAborted); }
                catch (Exception exception) when (options.Value.FailOpen && exception is not OperationCanceledException) { logger.LogError(exception, "Rate limit store unavailable; allowing request because FailureMode is Open."); await next(context); return; }
                context.Response.Headers["RateLimit-Limit"] = lease.Limit.ToString(); context.Response.Headers["RateLimit-Remaining"] = lease.Remaining.ToString(); context.Response.Headers["RateLimit-Reset"] = lease.ResetAt.ToString();
                if (!lease.Acquired) { context.Response.StatusCode = 429; context.Response.Headers.RetryAfter = Math.Max(1, lease.ResetAt - DateTimeOffset.UtcNow.ToUnixTimeSeconds()).ToString(); await context.Response.WriteAsJsonAsync(new { title = "Too many requests", status = 429, detail = $"Rate limit policy '{policy.Name}' was exceeded.", correlationId = context.TraceIdentifier }); return; }
                if (lease.RequiresRelease) leases.Add(lease);
            }
            await next(context);
        }
        finally { foreach (var lease in leases) try { await store.ReleaseAsync(lease, CancellationToken.None); } catch (Exception exception) { logger.LogWarning(exception, "Failed to release rate limit lease {LeaseId}.", lease.LeaseId); } }
    }
    private static bool Matches(RateLimitPolicy policy, string route, string method)
    {
        var methods = policy.HttpMethods?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries); if (methods is { Length: > 0 } && !methods.Contains(method, StringComparer.OrdinalIgnoreCase)) return false;
        var pattern = policy.RoutePattern.TrimStart('/'); return pattern == "*" || pattern.EndsWith("/**") && route.StartsWith(pattern[..^3], StringComparison.OrdinalIgnoreCase) || string.Equals(pattern, route, StringComparison.OrdinalIgnoreCase);
    }
    private static string BuildKey(RateLimitingOptions options, RateLimitPolicy policy, HttpContext context, string route)
    {
        var parts = new List<string> { options.KeyPrefix, policy.Id.ToString(), policy.Version.ToString() };
        foreach (var dimension in policy.PartitionBy.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)) parts.Add(dimension switch { "User" => context.User.FindFirstValue("uid") ?? "anonymous", "IpAddress" => context.Connection.RemoteIpAddress?.ToString() ?? "unknown", "Application" => options.ApplicationCode, "Endpoint" => $"{context.Request.Method}:{route}", _ => dimension });
        return string.Join(':', parts);
    }
}
