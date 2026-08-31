using Business.Domain.Entities.Restaurant;

namespace Business.Application.Abstractions.Persistence.Restaurant;

public interface IProductRepository
{
    Task<Food?> GetByCodeAsync(string code, CancellationToken cancellationToken);
    Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken);
    Task AddAsync(Food food, CancellationToken cancellationToken);
    Task SaveAsync(CancellationToken cancellationToken);
    Task DeactivateAsync(Food food, CancellationToken cancellationToken);
}
