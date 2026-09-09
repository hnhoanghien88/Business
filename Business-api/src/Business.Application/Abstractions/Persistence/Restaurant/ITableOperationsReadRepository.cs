using Business.Application.Restaurant.TableOperations.Dtos;

namespace Business.Application.Abstractions.Persistence.Restaurant;

public interface ITableOperationsReadRepository
{
    Task<IReadOnlyList<OperationalTableDto>> GetTablesAsync(
        string? search,
        ulong? areaId,
        string? status,
        CancellationToken cancellationToken);

    Task<TableSessionDto?> GetSessionAsync(ulong sessionId, CancellationToken cancellationToken);
}
