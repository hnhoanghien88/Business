namespace Business.Application.Restaurant.TableOperations.Dtos;

public sealed record OperationalTableDto(
    ulong Id,
    ulong AreaId,
    string AreaCode,
    string AreaName,
    int AreaDisplayOrder,
    string Code,
    string Name,
    int Capacity,
    string Status,
    bool IsActive,
    ulong? SessionId,
    int? GuestCount,
    DateTime? OpenedDate,
    DateTime Version);

public sealed record SessionOrderDto(ulong Id, string OrderNo, string Status, decimal TotalAmount);

public sealed record TableSessionDto(
    ulong Id,
    ulong TableId,
    string TableCode,
    string TableName,
    int GuestCount,
    string Status,
    DateTime OpenedDate,
    DateTime? ClosedDate,
    string? Note,
    decimal TotalAmount,
    decimal PaidAmount,
    decimal RemainingAmount,
    IReadOnlyList<string> CloseBlockers,
    IReadOnlyList<SessionOrderDto> Orders,
    DateTime Version);
