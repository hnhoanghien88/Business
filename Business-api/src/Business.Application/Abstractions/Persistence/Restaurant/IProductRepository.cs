using Business.Domain.Entities.Restaurant;

namespace Business.Application.Abstractions.Persistence.Restaurant;

public interface IProductRepository
{
    Task<Product?> GetByCodeAsync(string code, CancellationToken cancellationToken);
    Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken);
    Task AddAsync(Product product, CancellationToken cancellationToken);
    Task SaveAsync(CancellationToken cancellationToken);
    Task DeleteAsync(Product product, CancellationToken cancellationToken);
}
