using Business.Application.Abstractions.Persistence.Restaurant;
using Business.Application.Common.Exceptions;
using Business.Domain.Entities.Restaurant;
using Microsoft.EntityFrameworkCore;

namespace Business.Infrastructure.Persistence;

public sealed class MySqlProductsRepository(BusinessDbContext db) : IProductRepository
{
    public Task<Product?> GetByCodeAsync(string code, CancellationToken cancellationToken) =>
        db.Products.SingleOrDefaultAsync(product => product.Code == code, cancellationToken);

    public Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken) =>
        db.Products.AnyAsync(product => product.Code == code, cancellationToken);

    public async Task AddAsync(Product product, CancellationToken cancellationToken)
    {
        db.Products.Add(product);
        await SaveAsync(cancellationToken);
    }

    public async Task SaveAsync(CancellationToken cancellationToken)
    {
        try { await db.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateException) { throw new ConflictException("Product data conflicts with an existing record."); }
    }

    public async Task DeleteAsync(Product product, CancellationToken cancellationToken)
    {
        db.Products.Remove(product);
        await SaveAsync(cancellationToken);
    }
}
