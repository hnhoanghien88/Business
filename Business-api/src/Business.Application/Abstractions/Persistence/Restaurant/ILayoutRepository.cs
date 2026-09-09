using Business.Domain.Entities.Restaurant;

namespace Business.Application.Abstractions.Persistence.Restaurant;

public interface ILayoutRepository
{
    Task<RestaurantArea?> GetAreaByCodeAsync(string code, CancellationToken cancellationToken);
    Task<RestaurantArea?> GetAreaByIdAsync(ulong id, CancellationToken cancellationToken);
    Task<RestaurantTable?> GetTableByCodeAsync(string code, CancellationToken cancellationToken);
    Task<bool> AreaCodeExistsAsync(string code, CancellationToken cancellationToken);
    Task<bool> TableCodeExistsAsync(string code, CancellationToken cancellationToken);
    Task<bool> IsActiveAreaAsync(ulong id, CancellationToken cancellationToken);
    Task<int> GetAreaTableCountAsync(ulong id, CancellationToken cancellationToken);
    Task<bool> HasOpenSessionAsync(ulong tableId, CancellationToken cancellationToken);
    Task AddAreaAsync(RestaurantArea area, CancellationToken cancellationToken);
    Task SaveAreaAsync(RestaurantArea area, DateTime version, CancellationToken cancellationToken);
    Task AddTableAsync(RestaurantTable table, CancellationToken cancellationToken);
    Task SaveTableAsync(RestaurantTable table, DateTime version, CancellationToken cancellationToken);
}
