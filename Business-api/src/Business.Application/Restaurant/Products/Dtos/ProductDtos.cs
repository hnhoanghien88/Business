namespace Business.Application.Restaurant.Products.Dtos;

public sealed record ProductDto(ulong Id, ulong CategoryId, string Code, string Name, bool IsActive);
public sealed record PagedProductsDto(IReadOnlyList<ProductDto> Items, int TotalCount, int Page, int PageSize);
