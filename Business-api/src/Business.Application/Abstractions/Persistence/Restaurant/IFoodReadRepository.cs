using Business.Application.Restaurant.Foods.Dtos;

namespace Business.Application.Abstractions.Persistence.Restaurant;

public interface IFoodReadRepository
{
    Task<FoodDto?> GetByCodeAsync(string code, CancellationToken cancellationToken);
    Task<IReadOnlyList<FoodPriceHistoryDto>> GetPriceHistoryAsync(
        string foodCode,
        string variantCode,
        CancellationToken cancellationToken);
    Task<PagedFoodsDto> GetAsync(
        string? search,
        ulong? categoryId,
        FoodStatusFilter status,
        FoodAvailabilityFilter availability,
        int page,
        int pageSize,
        CancellationToken cancellationToken);
}
