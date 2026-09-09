namespace Business.Application.Restaurant.Foods.Dtos;

public sealed record FoodSummaryDto(
    ulong Id,
    ulong CategoryId,
    string CategoryCode,
    string CategoryName,
    string Code,
    string Name,
    string? Description,
    string? ImageUrl,
    int DisplayOrder,
    bool IsActive,
    int VariantCount,
    decimal? MinimumPrice,
    decimal? MaximumPrice,
    bool IsEffectivelySellable,
    DateTime Version);

public sealed record FoodVariantDto(
    ulong Id,
    string Code,
    string Name,
    decimal CurrentPrice,
    bool IsDefault,
    bool IsAvailable,
    string? SoldOutReason,
    int DisplayOrder,
    bool IsActive,
    DateTime Version);

public sealed record FoodPriceHistoryDto(
    ulong Id,
    decimal Price,
    DateTime EffectiveFrom,
    DateTime? EffectiveTo,
    ulong? CreatedBy);

public sealed record FoodDto(
    ulong Id,
    ulong CategoryId,
    string CategoryCode,
    string CategoryName,
    string Code,
    string Name,
    string? Description,
    string? ImageUrl,
    int DisplayOrder,
    bool IsActive,
    DateTime Version,
    IReadOnlyList<FoodVariantDto> Variants);

public sealed record PagedFoodsDto(
    IReadOnlyList<FoodSummaryDto> Items,
    int TotalCount,
    int Page,
    int PageSize);

public enum FoodStatusFilter { All, Active, Inactive }
public enum FoodAvailabilityFilter { All, Available, Unavailable }
