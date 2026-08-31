using Business.Application.Restaurant.Categories.Dtos;
using Business.Domain.Entities.Restaurant;

namespace Business.Application.Restaurant.Categories;

public static class CategoryRules
{
    public static string Clean(string value) => value.Trim();
    public static string? CleanOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    public static CategoryDto ToDto(Category category) => new(
        category.Id,
        category.ParentId,
        category.Code,
        category.Name,
        category.Description,
        category.DisplayOrder,
        category.IsActive,
        category.IsActive,
        0,
        [],
        category.Children.Count > 0,
        0,
        category.Foods.Count,
        category.UpdatedDate);
}
