using Business.Application.Abstractions.Persistence.Restaurant;
using Business.Application.Restaurant.TableOperations.Dtos;
using Dapper;

namespace Business.Infrastructure.Persistence;

public sealed class DapperTableOperationsReadRepository(MySqlConnectionFactory connectionFactory)
    : ITableOperationsReadRepository
{
    public async Task<IReadOnlyList<OperationalTableDto>> GetTablesAsync(
        string? search,
        ulong? areaId,
        string? status,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT t.Id, t.AreaId, a.Code AreaCode, a.Name AreaName,
                   a.DisplayOrder AreaDisplayOrder, t.Code, t.Name, t.Capacity,
                   t.Status, t.IsActive, s.Id SessionId, s.GuestCount,
                   s.OpenedDate, t.UpdatedDate Version
            FROM restaurant_tables t
            JOIN restaurant_areas a ON a.Id = t.AreaId
            LEFT JOIN restaurant_table_sessions s
              ON s.TableId = t.Id AND s.Status = 'Open'
            WHERE a.IsActive = 1
              AND (@AreaId IS NULL OR t.AreaId = @AreaId)
              AND (@Status IS NULL OR @Status = '' OR @Status = 'all' OR t.Status = @Status)
              AND (@Search IS NULL OR @Search = '' OR t.Code LIKE CONCAT('%', @Search, '%')
                   OR t.Name LIKE CONCAT('%', @Search, '%'))
            ORDER BY a.DisplayOrder, a.Name, t.Name, t.Code, t.Id
            """;
        await using var connection = connectionFactory.CreateConnection();
        var rows = await connection.QueryAsync<OperationalTableDto>(
            new CommandDefinition(
                sql,
                new
                {
                    Search = search?.Trim(),
                    AreaId = areaId,
                    Status = status?.Trim()
                },
                cancellationToken: cancellationToken));
        return rows.AsList();
    }

    public async Task<TableSessionDto?> GetSessionAsync(
        ulong sessionId,
        CancellationToken cancellationToken)
    {
        const string sessionSql = """
            SELECT s.Id, s.TableId, t.Code TableCode, t.Name TableName, s.GuestCount,
                   s.Status, s.OpenedDate, s.ClosedDate, s.Note, s.UpdatedDate Version
            FROM restaurant_table_sessions s
            JOIN restaurant_tables t ON t.Id = s.TableId
            WHERE s.Id = @SessionId
            """;
        const string ordersSql = """
            SELECT Id, OrderNo, Status, TotalAmount
            FROM restaurant_orders
            WHERE TableSessionId = @SessionId
            ORDER BY OrderedDate, Id
            """;
        const string paidSql = """
            SELECT COALESCE(SUM(Amount), 0)
            FROM restaurant_payments
            WHERE TableSessionId = @SessionId AND Status = 'Paid'
            """;

        await using var connection = connectionFactory.CreateConnection();
        var session = await connection.QuerySingleOrDefaultAsync<SessionRow>(
            new CommandDefinition(sessionSql, new { SessionId = sessionId }, cancellationToken: cancellationToken));
        if (session is null)
        {
            return null;
        }

        var orders = (await connection.QueryAsync<SessionOrderDto>(
            new CommandDefinition(ordersSql, new { SessionId = sessionId }, cancellationToken: cancellationToken))).AsList();
        var paid = await connection.ExecuteScalarAsync<decimal>(
            new CommandDefinition(paidSql, new { SessionId = sessionId }, cancellationToken: cancellationToken));
        var total = orders.Where(order => order.Status != "Cancelled").Sum(order => order.TotalAmount);
        var blockers = new List<string>();
        if (orders.Any(order => order.Status is not ("Completed" or "Cancelled")))
        {
            blockers.Add("Orders are still in progress.");
        }
        if (total - paid > 0)
        {
            blockers.Add("The session has an unpaid balance.");
        }

        return new TableSessionDto(
            session.Id,
            session.TableId,
            session.TableCode,
            session.TableName,
            session.GuestCount,
            session.Status,
            session.OpenedDate,
            session.ClosedDate,
            session.Note,
            total,
            paid,
            Math.Max(total - paid, 0),
            blockers,
            orders,
            session.Version);
    }

    private sealed class SessionRow
    {
        public ulong Id { get; init; }
        public ulong TableId { get; init; }
        public required string TableCode { get; init; }
        public required string TableName { get; init; }
        public int GuestCount { get; init; }
        public required string Status { get; init; }
        public DateTime OpenedDate { get; init; }
        public DateTime? ClosedDate { get; init; }
        public string? Note { get; init; }
        public DateTime Version { get; init; }
    }
}
