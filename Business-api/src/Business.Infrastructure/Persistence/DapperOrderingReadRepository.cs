using Business.Application.Abstractions.Persistence.Restaurant;
using Business.Application.Restaurant.Ordering.Dtos;
using Dapper;

namespace Business.Infrastructure.Persistence;

public sealed class DapperOrderingReadRepository(MySqlConnectionFactory connectionFactory)
    : IOrderingReadRepository
{
    public async Task<OrderingMenuDto?> GetMenuAsync(
        ulong sessionId, string? search, ulong? categoryId, CancellationToken cancellationToken)
    {
        const string sessionSql = """
            SELECT s.Id, t.Code
            FROM restaurant_table_sessions s
            JOIN restaurant_tables t ON t.Id = s.TableId
            WHERE s.Id = @SessionId AND s.Status = 'Open' AND t.Status = 'Occupied'
            """;
        const string categorySql = """
            SELECT DISTINCT c.Id, c.Code, c.Name
            FROM restaurant_categories c
            JOIN restaurant_foods f ON f.CategoryId = c.Id AND f.IsActive = 1
            WHERE c.IsActive = 1
            ORDER BY c.DisplayOrder, c.Name
            """;
        const string foodSql = """
            SELECT f.Id, f.CategoryId, f.Code, f.Name, f.Description, f.ImageUrl,
                   v.Id VariantId, v.Code VariantCode, v.Name VariantName,
                   v.CurrentPrice, v.IsDefault, v.IsAvailable, v.SoldOutReason
            FROM restaurant_foods f
            JOIN restaurant_categories c ON c.Id = f.CategoryId AND c.IsActive = 1
            JOIN restaurant_food_variants v ON v.FoodId = f.Id AND v.IsActive = 1
            WHERE f.IsActive = 1
              AND (@CategoryId IS NULL OR f.CategoryId = @CategoryId)
              AND (@Search IS NULL OR @Search = '' OR f.Name LIKE CONCAT('%', @Search, '%')
                   OR f.Code LIKE CONCAT('%', @Search, '%'))
            ORDER BY f.DisplayOrder, f.Name, v.DisplayOrder, v.Name
            """;
        await using var connection = connectionFactory.CreateConnection();
        var session = await connection.QuerySingleOrDefaultAsync<SessionRow>(new CommandDefinition(
            sessionSql, new { SessionId = sessionId }, cancellationToken: cancellationToken));
        if (session is null) return null;
        var categories = (await connection.QueryAsync<MenuCategoryDto>(new CommandDefinition(
            categorySql, cancellationToken: cancellationToken))).AsList();
        var rows = await connection.QueryAsync<FoodRow>(new CommandDefinition(
            foodSql, new { Search = search?.Trim(), CategoryId = categoryId }, cancellationToken: cancellationToken));
        var foods = rows.GroupBy(row => new
            { row.Id, row.CategoryId, row.Code, row.Name, row.Description, row.ImageUrl })
            .Select(group => new MenuFoodDto(
                group.Key.Id, group.Key.CategoryId, group.Key.Code, group.Key.Name,
                group.Key.Description, group.Key.ImageUrl,
                group.Select(row => new MenuVariantDto(
                    row.VariantId, row.VariantCode, row.VariantName, row.CurrentPrice,
                    row.IsDefault, row.IsAvailable, row.SoldOutReason)).ToList()))
            .ToList();
        return new OrderingMenuDto(session.Id, session.Code, categories, foods);
    }

    public async Task<SessionOrderingDto?> GetSessionOrdersAsync(
        ulong sessionId, CancellationToken cancellationToken)
    {
        const string existsSql = "SELECT COUNT(*) FROM restaurant_table_sessions WHERE Id = @SessionId";
        const string sql = """
            SELECT Id, OrderNo, Status, TotalAmount, OrderedDate
            FROM restaurant_orders WHERE TableSessionId = @SessionId
            ORDER BY OrderedDate DESC, Id DESC
            """;
        await using var connection = connectionFactory.CreateConnection();
        if (await connection.ExecuteScalarAsync<int>(new CommandDefinition(
            existsSql, new { SessionId = sessionId }, cancellationToken: cancellationToken)) == 0) return null;
        var orders = (await connection.QueryAsync<SessionOrderDto>(new CommandDefinition(
            sql, new { SessionId = sessionId }, cancellationToken: cancellationToken))).AsList();
        return new SessionOrderingDto(
            sessionId, orders.Where(order => order.Status != "Cancelled").Sum(order => order.TotalAmount), orders);
    }

    private sealed record SessionRow(ulong Id, string Code);
    private sealed record FoodRow(
        ulong Id, ulong CategoryId, string Code, string Name, string? Description, string? ImageUrl,
        ulong VariantId, string VariantCode, string VariantName, decimal CurrentPrice,
        bool IsDefault, bool IsAvailable, string? SoldOutReason);
}
