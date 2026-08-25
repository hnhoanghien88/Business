using Business.Application.Restaurant.Products.Dtos;

namespace Business.Application.Abstractions.Persistence.Restaurant;

public interface IProductReadRepository
{
    Task<ProductDto?> GetByCodeAsync(string code, CancellationToken cancellationToken);
    Task<PagedProductsDto> GetAsync(string? search, int page, int pageSize, CancellationToken cancellationToken);
}
