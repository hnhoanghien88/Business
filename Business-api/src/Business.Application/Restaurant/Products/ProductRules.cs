using Business.Application.Restaurant.Products.Dtos;
using Business.Domain.Entities.Restaurant;

namespace Business.Application.Restaurant.Products;

internal static class ProductRules
{
    public static string Clean(string value) => value.Trim();
    public static string CleanCode(string value) => value.Trim().ToUpperInvariant();
    public static ProductDto ToDto(Food food) =>
        new(food.Id, food.CategoryId, food.Code, food.Name, food.IsActive);
}
