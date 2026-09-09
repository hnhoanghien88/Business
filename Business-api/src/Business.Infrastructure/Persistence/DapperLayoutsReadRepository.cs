using Business.Application.Abstractions.Persistence.Restaurant;
using Business.Application.Restaurant.Layouts.Dtos;
using Dapper;

namespace Business.Infrastructure.Persistence;

public sealed class DapperLayoutsReadRepository(MySqlConnectionFactory connectionFactory) : ILayoutReadRepository
{
    public async Task<PagedLayoutDto<AreaDto>> GetAreasAsync(string? search, bool? isActive, string sort, int page, int pageSize, CancellationToken token)
    {
        await using var connection = connectionFactory.CreateConnection();
        const string sql = """
            SELECT a.Id, a.Code, a.Name, a.Description, a.DisplayOrder, a.IsActive,
              COUNT(DISTINCT t.Id) TableCount,
              COUNT(DISTINCT CASE WHEN s.Status = 'Open' THEN s.Id END) OpenSessionCount,
              a.UpdatedDate Version
            FROM restaurant_areas a
            LEFT JOIN restaurant_tables t ON t.AreaId = a.Id
            LEFT JOIN restaurant_table_sessions s ON s.TableId = t.Id
            GROUP BY a.Id, a.Code, a.Name, a.Description, a.DisplayOrder, a.IsActive, a.UpdatedDate
            """;
        var rows = (await connection.QueryAsync<AreaDto>(new CommandDefinition(sql, cancellationToken: token))).AsList();
        var query = rows.Where(x => (!isActive.HasValue || x.IsActive == isActive) && (string.IsNullOrWhiteSpace(search) || x.Code.Contains(search.Trim(), StringComparison.OrdinalIgnoreCase) || x.Name.Contains(search.Trim(), StringComparison.OrdinalIgnoreCase)));
        query = sort.ToLowerInvariant() switch { "name" => query.OrderBy(x => x.Name).ThenBy(x => x.Code), "code" => query.OrderBy(x => x.Code), _ => query.OrderBy(x => x.DisplayOrder).ThenBy(x => x.Name).ThenBy(x => x.Code) };
        var values = query.ToList();
        return new(values.Skip((page - 1) * pageSize).Take(pageSize).ToList(), values.Count, page, pageSize);
    }

    public async Task<AreaDto?> GetAreaAsync(string code, CancellationToken token) =>
        (await GetAreasAsync(code, null, "code", 1, 100, token)).Items.FirstOrDefault(x => string.Equals(x.Code, code, StringComparison.OrdinalIgnoreCase));

    public async Task<PagedLayoutDto<TableDto>> GetTablesAsync(string? search, ulong? areaId, string? status, bool? isActive, string sort, int page, int pageSize, CancellationToken token)
    {
        await using var connection = connectionFactory.CreateConnection();
        const string sql = """
            SELECT t.Id, t.AreaId, a.Code AreaCode, a.Name AreaName, a.IsActive AreaIsActive,
              t.Code, t.Name, t.Capacity, t.Status, t.IsActive,
              EXISTS(SELECT 1 FROM restaurant_table_sessions s WHERE s.TableId=t.Id AND s.Status='Open') HasOpenSession,
              NOT EXISTS(SELECT 1 FROM restaurant_table_sessions s WHERE s.TableId=t.Id AND s.Status='Open') CanMove,
              (t.IsActive=1 AND t.Status='Available' AND NOT EXISTS(SELECT 1 FROM restaurant_table_sessions s WHERE s.TableId=t.Id AND s.Status='Open')) CanDisable,
              t.UpdatedDate Version
            FROM restaurant_tables t JOIN restaurant_areas a ON a.Id=t.AreaId
            """;
        var rows = (await connection.QueryAsync<TableDto>(new CommandDefinition(sql, cancellationToken: token))).AsList();
        var query = rows.Where(x => (!areaId.HasValue || x.AreaId == areaId) && (!isActive.HasValue || x.IsActive == isActive) && (string.IsNullOrWhiteSpace(status) || status == "all" || string.Equals(x.Status, status, StringComparison.OrdinalIgnoreCase)) && (string.IsNullOrWhiteSpace(search) || x.Code.Contains(search.Trim(), StringComparison.OrdinalIgnoreCase) || x.Name.Contains(search.Trim(), StringComparison.OrdinalIgnoreCase)));
        query = sort.ToLowerInvariant() switch { "name" => query.OrderBy(x => x.Name), "code" => query.OrderBy(x => x.Code), "capacity" => query.OrderBy(x => x.Capacity).ThenBy(x => x.Code), "status" => query.OrderBy(x => x.Status).ThenBy(x => x.Code), _ => query.OrderBy(x => x.AreaName).ThenBy(x => x.Name).ThenBy(x => x.Code) };
        var values = query.ToList();
        return new(values.Skip((page - 1) * pageSize).Take(pageSize).ToList(), values.Count, page, pageSize);
    }

    public async Task<TableDto?> GetTableAsync(string code, CancellationToken token) =>
        (await GetTablesAsync(code, null, null, null, "code", 1, 100, token)).Items.FirstOrDefault(x => string.Equals(x.Code, code, StringComparison.OrdinalIgnoreCase));
}
