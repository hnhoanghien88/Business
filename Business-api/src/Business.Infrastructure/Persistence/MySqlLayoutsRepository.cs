using Business.Application.Abstractions.Persistence.Restaurant;
using Business.Application.Common.Exceptions;
using Business.Domain.Entities.Restaurant;
using Microsoft.EntityFrameworkCore;

namespace Business.Infrastructure.Persistence;

public sealed class MySqlLayoutsRepository(BusinessDbContext dbContext) : ILayoutRepository
{
    public Task<RestaurantArea?> GetAreaByCodeAsync(string code, CancellationToken token) =>
        dbContext.RestaurantAreas.SingleOrDefaultAsync(x => x.Code == code, token);
    public Task<RestaurantArea?> GetAreaByIdAsync(ulong id, CancellationToken token) =>
        dbContext.RestaurantAreas.SingleOrDefaultAsync(x => x.Id == id, token);
    public Task<RestaurantTable?> GetTableByCodeAsync(string code, CancellationToken token) =>
        dbContext.RestaurantTables.Include(x => x.Area).SingleOrDefaultAsync(x => x.Code == code, token);
    public Task<bool> AreaCodeExistsAsync(string code, CancellationToken token) =>
        dbContext.RestaurantAreas.AnyAsync(x => x.Code == code, token);
    public Task<bool> TableCodeExistsAsync(string code, CancellationToken token) =>
        dbContext.RestaurantTables.AnyAsync(x => x.Code == code, token);
    public Task<bool> IsActiveAreaAsync(ulong id, CancellationToken token) =>
        dbContext.RestaurantAreas.AnyAsync(x => x.Id == id && x.IsActive, token);
    public Task<int> GetAreaTableCountAsync(ulong id, CancellationToken token) =>
        dbContext.RestaurantTables.CountAsync(x => x.AreaId == id, token);
    public Task<bool> HasOpenSessionAsync(ulong tableId, CancellationToken token) =>
        dbContext.TableSessions.AnyAsync(x => x.TableId == tableId && x.Status == "Open", token);

    public async Task AddAreaAsync(RestaurantArea area, CancellationToken token)
    {
        dbContext.RestaurantAreas.Add(area);
        await SaveUniqueAsync("Area code is already in use.", token);
    }
    public Task SaveAreaAsync(RestaurantArea area, DateTime version, CancellationToken token) => SaveAsync(area, version, token);
    public async Task AddTableAsync(RestaurantTable table, CancellationToken token)
    {
        dbContext.RestaurantTables.Add(table);
        await SaveUniqueAsync("Table code is already in use.", token);
    }
    public Task SaveTableAsync(RestaurantTable table, DateTime version, CancellationToken token) => SaveAsync(table, version, token);

    private async Task SaveUniqueAsync(string message, CancellationToken token)
    {
        try { await dbContext.SaveChangesAsync(token); }
        catch (DbUpdateException) { throw new ConflictException(message, "code"); }
    }

    private async Task SaveAsync<TEntity>(TEntity entity, DateTime version, CancellationToken token) where TEntity : class
    {
        dbContext.Entry(entity).Property("UpdatedDate").OriginalValue = version;
        dbContext.Entry(entity).Property("UpdatedDate").CurrentValue = DateTime.UtcNow;
        try { await dbContext.SaveChangesAsync(token); }
        catch (DbUpdateConcurrencyException) { throw new ConflictException("The layout changed since it was loaded. Reload and try again.", "version"); }
    }
}
