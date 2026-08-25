namespace Business.Application.Restaurant.Products.Dtos;

public sealed record ProductDto(string Code, string Name);
public sealed record PagedProductsDto(IReadOnlyList<ProductDto> Items, int TotalCount, int Page, int PageSize);
