using Business.Application.Abstractions.Persistence.Restaurant;
using Business.Application.Common.Exceptions;
using Business.Domain.Entities.Restaurant;
using Microsoft.EntityFrameworkCore;

namespace Business.Infrastructure.Persistence;

public sealed class MySqlProductsRepository(BusinessDbContext db) : IProductRepository
{
    public Task<Food?> GetByCodeAsync(string code, CancellationToken cancellationToken) =>
        db.Foods.SingleOrDefaultAsync(food => food.Code == code && food.IsActive, cancellationToken);

    public Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken) =>
        db.Foods.AnyAsync(food => food.Code == code, cancellationToken);

    public async Task AddAsync(Food food, CancellationToken cancellationToken)
    {
        if (food.CategoryId == 0)
        {
            var category = await db.Categories.SingleOrDefaultAsync(
                item => item.Code == "LEGACY",
                cancellationToken);
            if (category is null)
            {
                category = new Category
                {
                    Code = "LEGACY",
                    Name = "Chưa phân loại"
                };
                db.Categories.Add(category);
            }

            food.Category = category;
        }
        else if (!await db.Categories.AnyAsync(
                     category => category.Id == food.CategoryId && category.IsActive,
                     cancellationToken))
        {
            throw new ConflictException("Category does not exist or is inactive.", "categoryId");
        }

        db.Foods.Add(food);
        await SaveAsync(cancellationToken);
    }

    public async Task SaveAsync(CancellationToken cancellationToken)
    {
        try { await db.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateException) { throw new ConflictException("Food data conflicts with an existing record."); }
    }

    public async Task DeactivateAsync(Food food, CancellationToken cancellationToken)
    {
        food.IsActive = false;
        await SaveAsync(cancellationToken);
    }
}
