namespace Business.Application.Restaurant.Ordering.Dtos;

public sealed record MenuVariantDto(
    ulong Id, string Code, string Name, decimal CurrentPrice, bool IsDefault,
    bool IsAvailable, string? SoldOutReason);
public sealed record MenuFoodDto(
    ulong Id, ulong CategoryId, string Code, string Name, string? Description,
    string? ImageUrl, IReadOnlyList<MenuVariantDto> Variants);
public sealed record MenuCategoryDto(ulong Id, string Code, string Name);
public sealed record OrderingMenuDto(
    ulong SessionId, string TableCode, IReadOnlyList<MenuCategoryDto> Categories,
    IReadOnlyList<MenuFoodDto> Foods);
public sealed record OrderLineInput(ulong FoodVariantId, int Quantity, string? Note);
public sealed record OrderPreviewDto(
    decimal SubtotalAmount, decimal DiscountAmount, decimal TotalAmount,
    string? PromotionCode, string? PromotionMessage);
public sealed record OrderConfirmationDto(
    ulong OrderId, string OrderNo, string KitchenNo, decimal TotalAmount, bool IsReplay);
public sealed record SessionOrderDto(
    ulong Id, string OrderNo, string Status, decimal TotalAmount, DateTime OrderedDate);
public sealed record SessionOrderingDto(
    ulong SessionId, decimal TotalAmount, IReadOnlyList<SessionOrderDto> Orders);
