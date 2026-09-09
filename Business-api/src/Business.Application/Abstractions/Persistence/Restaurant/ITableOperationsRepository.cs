using Business.Domain.Entities.Restaurant;

namespace Business.Application.Abstractions.Persistence.Restaurant;

public interface ITableOperationsRepository
{
    Task<TableSession> OpenAsync(
        string tableCode,
        int guestCount,
        string? note,
        bool overrideCapacity,
        string? overrideReason,
        ulong? actorId,
        CancellationToken cancellationToken);

    Task<TableSession> CloseAsync(
        ulong sessionId,
        bool overrideObligations,
        string? overrideReason,
        ulong? actorId,
        CancellationToken cancellationToken);

    Task<RestaurantTable> MarkCleanAsync(
        string tableCode,
        ulong? actorId,
        CancellationToken cancellationToken);
}
