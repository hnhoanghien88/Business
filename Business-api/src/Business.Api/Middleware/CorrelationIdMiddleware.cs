using System.Diagnostics;
using Serilog.Context;

namespace Business.Api.Middleware;

public sealed class CorrelationIdMiddleware(RequestDelegate next)
{
    public const string HeaderName = "X-Correlation-ID";
    public async Task InvokeAsync(HttpContext context)
    {
        var candidate = context.Request.Headers[HeaderName].FirstOrDefault();
        var id = !string.IsNullOrWhiteSpace(candidate) && candidate.Length <= 128 && candidate.All(c => char.IsAsciiLetterOrDigit(c) || c is '-' or '_' or '.' or ':') ? candidate : Activity.Current?.TraceId.ToString() ?? Guid.NewGuid().ToString("N");
        var traceId = Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier;
        context.TraceIdentifier = id; context.Response.Headers[HeaderName] = id;
        using (LogContext.PushProperty("CorrelationId", id)) using (LogContext.PushProperty("TraceId", traceId)) await next(context);
    }
}
