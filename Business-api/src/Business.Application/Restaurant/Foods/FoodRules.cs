using Business.Application.Restaurant.Foods.Dtos;
using Business.Domain.Entities.Restaurant;

namespace Business.Application.Restaurant.Foods;

public static class FoodRules
{
    public static string Clean(string value) => value.Trim();
    public static string CleanCode(string value) => value.Trim().ToUpperInvariant();
    public static string? CleanOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    public static FoodVariantDto ToDto(FoodVariant variant) => new(
        variant.Id,
        variant.Code,
        variant.Name,
        variant.CurrentPrice,
        variant.IsDefault,
        variant.IsAvailable,
        variant.SoldOutReason,
        variant.DisplayOrder,
        variant.IsActive,
        variant.UpdatedDate);
}
