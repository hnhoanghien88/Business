using Business.Application.Restaurant.Layouts.Dtos;

namespace Business.Application.Abstractions.Persistence.Restaurant;

public interface ILayoutReadRepository
{
    Task<PagedLayoutDto<AreaDto>> GetAreasAsync(string? search, bool? isActive, string sort, int page, int pageSize, CancellationToken cancellationToken);
    Task<AreaDto?> GetAreaAsync(string code, CancellationToken cancellationToken);
    Task<PagedLayoutDto<TableDto>> GetTablesAsync(string? search, ulong? areaId, string? status, bool? isActive, string sort, int page, int pageSize, CancellationToken cancellationToken);
    Task<TableDto?> GetTableAsync(string code, CancellationToken cancellationToken);
}
