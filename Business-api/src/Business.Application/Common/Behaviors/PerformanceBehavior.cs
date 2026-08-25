using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Business.Application.Common.Behaviors;

public sealed class PerformanceBehavior<TRequest, TResponse>(
    PerformanceOptions options,
    ILogger<PerformanceBehavior<TRequest, TResponse>> logger) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var startedAt = Stopwatch.GetTimestamp();
        try { return await next(cancellationToken); }
        finally
        {
            var elapsed = Stopwatch.GetElapsedTime(startedAt);
            if (elapsed.TotalMilliseconds >= options.SlowRequestThresholdMilliseconds)
                logger.LogWarning(
                    "Slow MediatR request {RequestName} took {ElapsedMilliseconds:F0} ms (threshold: {ThresholdMilliseconds} ms)",
                    typeof(TRequest).Name, elapsed.TotalMilliseconds, options.SlowRequestThresholdMilliseconds);
        }
    }
}
