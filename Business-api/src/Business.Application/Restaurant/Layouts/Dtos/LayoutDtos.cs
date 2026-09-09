namespace Business.Application.Restaurant.Layouts.Dtos;

public sealed record AreaDto(
    ulong Id,
    string Code,
    string Name,
    string? Description,
    int DisplayOrder,
    bool IsActive,
    int TableCount,
    int OpenSessionCount,
    DateTime Version);

public sealed record TableDto(
    ulong Id,
    ulong AreaId,
    string AreaCode,
    string AreaName,
    bool AreaIsActive,
    string Code,
    string Name,
    int Capacity,
    string Status,
    bool IsActive,
    bool HasOpenSession,
    bool CanMove,
    bool CanDisable,
    DateTime Version);

public sealed record PagedLayoutDto<T>(IReadOnlyList<T> Items, int TotalCount, int Page, int PageSize);
