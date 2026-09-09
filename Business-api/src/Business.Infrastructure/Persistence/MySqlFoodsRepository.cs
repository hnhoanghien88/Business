using Business.Application.Abstractions.Persistence.Restaurant;
using Business.Application.Common.Exceptions;
using Business.Domain.Entities.Restaurant;
using Microsoft.EntityFrameworkCore;

namespace Business.Infrastructure.Persistence;

public sealed class MySqlFoodsRepository(BusinessDbContext db) : IFoodRepository
{
    public Task<Food?> GetByCodeAsync(
        string code,
        bool includeVariants,
        CancellationToken cancellationToken)
    {
        IQueryable<Food> query = db.Foods;
        if (includeVariants)
            query = query.Include(food => food.Variants);
        return query.SingleOrDefaultAsync(food => food.Code == code, cancellationToken);
    }

    public Task<bool> CodeExistsAsync(
        string code,
        CancellationToken cancellationToken) =>
        db.Foods.AnyAsync(food => food.Code == code, cancellationToken);

    public Task<bool> CategoryIsActiveAsync(
        ulong categoryId,
        CancellationToken cancellationToken) =>
        db.Categories.AnyAsync(
            category => category.Id == categoryId && category.IsActive,
            cancellationToken);

    public async Task AddAsync(Food food, CancellationToken cancellationToken)
    {
        db.Foods.Add(food);
        await SaveAsync(cancellationToken);
    }

    public async Task AddVariantAsync(
        Food food,
        FoodVariant variant,
        FoodPriceHistory initialPrice,
        CancellationToken cancellationToken)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        db.FoodVariants.Add(variant);
        db.FoodPriceHistories.Add(initialPrice);
        await SaveAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    public async Task ChangePriceAsync(
        FoodVariant variant,
        decimal price,
        ulong? actorId,
        CancellationToken cancellationToken)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        var now = DateTime.UtcNow;
        var current = await db.FoodPriceHistories.SingleOrDefaultAsync(
            history => history.FoodVariantId == variant.Id && history.EffectiveTo == null,
            cancellationToken);
        if (current is not null)
            current.EffectiveTo = now;
        db.FoodPriceHistories.Add(new FoodPriceHistory
        {
            FoodVariantId = variant.Id,
            FoodVariant = variant,
            Price = price,
            EffectiveFrom = now,
            CreatedBy = actorId,
            CreatedDate = now
        });
        variant.CurrentPrice = price;
        variant.UpdatedBy = actorId;
        await SaveAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    public async Task SaveAsync(CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        foreach (var entry in db.ChangeTracker.Entries<Food>()
                     .Where(entry => entry.State == EntityState.Modified))
            entry.Entity.UpdatedDate = now;
        foreach (var entry in db.ChangeTracker.Entries<FoodVariant>()
                     .Where(entry => entry.State == EntityState.Modified))
            entry.Entity.UpdatedDate = now;

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictException("Food data was changed by another user.");
        }
        catch (DbUpdateException)
        {
            throw new ConflictException("Food data conflicts with an existing record.");
        }
    }
}
