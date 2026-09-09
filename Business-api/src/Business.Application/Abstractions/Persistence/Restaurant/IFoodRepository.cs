using Business.Domain.Entities.Restaurant;

namespace Business.Application.Abstractions.Persistence.Restaurant;

public interface IFoodRepository
{
    Task<Food?> GetByCodeAsync(
        string code,
        bool includeVariants,
        CancellationToken cancellationToken);
    Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken);
    Task<bool> CategoryIsActiveAsync(ulong categoryId, CancellationToken cancellationToken);
    Task AddAsync(Food food, CancellationToken cancellationToken);
    Task AddVariantAsync(
        Food food,
        FoodVariant variant,
        FoodPriceHistory initialPrice,
        CancellationToken cancellationToken);
    Task SaveAsync(CancellationToken cancellationToken);
    Task ChangePriceAsync(
        FoodVariant variant,
        decimal price,
        ulong? actorId,
        CancellationToken cancellationToken);
}
