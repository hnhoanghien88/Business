using Business.Application.Restaurant.Categories.Dtos;

namespace Business.Application.Abstractions.Persistence.Restaurant;

public interface ICategoryReadRepository
{
    Task<PagedCategoriesDto> GetAsync(
        string? search,
        CategoryStatusFilter status,
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    Task<CategoryDto?> GetByCodeAsync(string code, CancellationToken cancellationToken);
}
