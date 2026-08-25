using System.Diagnostics;

namespace Business.Api.Middleware;

public sealed class StructuredRequestLoggingMiddleware(RequestDelegate next, ILogger<StructuredRequestLoggingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var started = Stopwatch.GetTimestamp(); await next(context); var elapsed = Stopwatch.GetElapsedTime(started).TotalMilliseconds;
        var args = new object?[] { context.Request.Method, context.Request.Path, context.Response.StatusCode, elapsed, context.User.FindFirst("uid")?.Value };
        const string template = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {ElapsedMs:F3} ms for user {UserId}";
        if (context.Response.StatusCode >= 500) logger.LogError(template, args); else if (elapsed > 3000) logger.LogWarning("Slow " + template, args); else logger.LogInformation(template, args);
    }
}
