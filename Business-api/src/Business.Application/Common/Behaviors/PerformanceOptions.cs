namespace Business.Application.Common.Behaviors;

public sealed class PerformanceOptions
{
    public const string SectionName = "Observability";
    public int SlowRequestThresholdMilliseconds { get; init; } = 500;
}
