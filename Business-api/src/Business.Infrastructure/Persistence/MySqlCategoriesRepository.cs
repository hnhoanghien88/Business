using Business.Application.Abstractions.Persistence.Restaurant;
using Business.Application.Common.Exceptions;
using Business.Domain.Entities.Restaurant;
using Microsoft.EntityFrameworkCore;

namespace Business.Infrastructure.Persistence;

public sealed class MySqlCategoriesRepository(BusinessDbContext dbContext) : ICategoryRepository
{
    public Task<Category?> GetByCodeAsync(string code, CancellationToken cancellationToken) =>
        dbContext.Categories.SingleOrDefaultAsync(
            category => category.Code == code,
            cancellationToken);

    public Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken) =>
        dbContext.Categories.AnyAsync(category => category.Code == code, cancellationToken);

    public Task<bool> IsValidActiveParentAsync(ulong parentId, CancellationToken cancellationToken) =>
        dbContext.Categories.AnyAsync(
            category => category.Id == parentId && category.IsActive,
            cancellationToken);

    public async Task<bool> IsDescendantAsync(
        ulong categoryId,
        ulong candidateId,
        CancellationToken cancellationToken)
    {
        var currentId = (ulong?)candidateId;
        var visited = new HashSet<ulong>();
        while (currentId is ulong id && visited.Add(id))
        {
            if (id == categoryId) return true;
            currentId = await dbContext.Categories
                .Where(category => category.Id == id)
                .Select(category => category.ParentId)
                .SingleOrDefaultAsync(cancellationToken);
        }
        return false;
    }

    public async Task AddAsync(Category category, CancellationToken cancellationToken)
    {
        dbContext.Categories.Add(category);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            throw new ConflictException("Category code is already in use.", "code");
        }
    }

    public async Task SaveAsync(
        Category category,
        DateTime version,
        CancellationToken cancellationToken)
    {
        dbContext.Entry(category).Property(item => item.UpdatedDate).OriginalValue = version;
        category.UpdatedDate = DateTime.UtcNow;
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictException(
                "Category changed since it was loaded. Reload and try again.",
                "version");
        }
    }
}
