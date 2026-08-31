using Business.Domain.Entities.Restaurant;

namespace Business.Application.Abstractions.Persistence.Restaurant;

public interface ICategoryRepository
{
    Task<Category?> GetByCodeAsync(string code, CancellationToken cancellationToken);
    Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken);
    Task<bool> IsValidActiveParentAsync(ulong parentId, CancellationToken cancellationToken);
    Task<bool> IsDescendantAsync(ulong categoryId, ulong candidateId, CancellationToken cancellationToken);
    Task AddAsync(Category category, CancellationToken cancellationToken);
    Task SaveAsync(Category category, DateTime version, CancellationToken cancellationToken);
}
