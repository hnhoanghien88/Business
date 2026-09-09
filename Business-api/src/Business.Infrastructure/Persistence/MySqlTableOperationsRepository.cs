using System.Data;
using Business.Application.Abstractions.Persistence.Restaurant;
using Business.Application.Common.Exceptions;
using Business.Domain.Entities.Restaurant;
using Microsoft.EntityFrameworkCore;

namespace Business.Infrastructure.Persistence;

public sealed class MySqlTableOperationsRepository(BusinessDbContext dbContext)
    : ITableOperationsRepository
{
    public async Task<TableSession> OpenAsync(
        string tableCode,
        int guestCount,
        string? note,
        bool overrideCapacity,
        string? overrideReason,
        ulong? actorId,
        CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);
        var table = await dbContext.RestaurantTables
            .Include(value => value.Area)
            .SingleOrDefaultAsync(value => value.Code == tableCode, cancellationToken)
            ?? throw new NotFoundException($"Table '{tableCode}' was not found.");
        if (!table.IsActive || !table.Area.IsActive || table.Status != "Available")
        {
            throw new ConflictException("Only an active Available table can be opened.", "status");
        }
        if (await dbContext.TableSessions.AnyAsync(
            value => value.TableId == table.Id && value.Status == "Open",
            cancellationToken))
        {
            throw new ConflictException("The table already has an open session.", "tableCode");
        }
        if (guestCount > table.Capacity && !overrideCapacity)
        {
            throw new ConflictException("Guest count exceeds capacity and requires override.", "guestCount");
        }

        var now = DateTime.UtcNow;
        var auditNote = overrideCapacity
            ? JoinNote(note, $"Capacity override: {overrideReason}")
            : Clean(note);
        var session = new TableSession
        {
            TableId = table.Id,
            GuestCount = guestCount,
            Status = "Open",
            OpenedDate = now,
            OpenedBy = actorId,
            Note = auditNote,
            CreatedDate = now,
            UpdatedDate = now,
            Table = table
        };
        table.Status = "Occupied";
        table.UpdatedBy = actorId;
        table.UpdatedDate = now;
        dbContext.TableSessions.Add(session);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return session;
        }
        catch (DbUpdateException)
        {
            throw new ConflictException("The table was opened by another request.", "tableCode");
        }
    }

    public async Task<TableSession> CloseAsync(
        ulong sessionId,
        bool overrideObligations,
        string? overrideReason,
        ulong? actorId,
        CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);
        var session = await dbContext.TableSessions
            .Include(value => value.Table)
            .Include(value => value.Orders)
            .Include(value => value.Payments)
            .SingleOrDefaultAsync(value => value.Id == sessionId, cancellationToken)
            ?? throw new NotFoundException($"Table session '{sessionId}' was not found.");
        if (session.Status != "Open" || session.Table.Status != "Occupied")
        {
            throw new ConflictException("Only the current open session can be closed.", "status");
        }

        var total = session.Orders
            .Where(order => order.Status != "Cancelled")
            .Sum(order => order.TotalAmount);
        var paid = session.Payments
            .Where(payment => payment.Status == "Paid")
            .Sum(payment => payment.Amount);
        var hasOpenOrders = session.Orders.Any(
            order => order.Status is not ("Completed" or "Cancelled"));
        if ((hasOpenOrders || total - paid > 0) && !overrideObligations)
        {
            throw new ConflictException("Orders or payments are not complete.", "obligations");
        }

        var now = DateTime.UtcNow;
        session.Status = "Closed";
        session.ClosedDate = now;
        session.ClosedBy = actorId;
        session.UpdatedDate = now;
        if (overrideObligations)
        {
            session.Note = JoinNote(session.Note, $"Close override: {overrideReason}");
        }
        session.Table.Status = "Cleaning";
        session.Table.UpdatedBy = actorId;
        session.Table.UpdatedDate = now;
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return session;
    }

    public async Task<RestaurantTable> MarkCleanAsync(
        string tableCode,
        ulong? actorId,
        CancellationToken cancellationToken)
    {
        var table = await dbContext.RestaurantTables
            .SingleOrDefaultAsync(value => value.Code == tableCode, cancellationToken)
            ?? throw new NotFoundException($"Table '{tableCode}' was not found.");
        if (table.Status != "Cleaning")
        {
            throw new ConflictException("Only a Cleaning table can be marked clean.", "status");
        }
        table.Status = "Available";
        table.UpdatedBy = actorId;
        table.UpdatedDate = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        return table;
    }

    private static string? Clean(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string JoinNote(string? note, string audit) =>
        string.IsNullOrWhiteSpace(note) ? audit : $"{note.Trim()} | {audit}";
}
