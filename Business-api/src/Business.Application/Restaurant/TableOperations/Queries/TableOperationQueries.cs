using Business.Application.Abstractions.Persistence.Restaurant;
using Business.Application.Common.Exceptions;
using Business.Application.Restaurant.TableOperations.Dtos;
using MediatR;

namespace Business.Application.Restaurant.TableOperations.Queries;

public sealed record GetOperationalTablesQuery(
    string? Search,
    ulong? AreaId,
    string? Status) : IRequest<IReadOnlyList<OperationalTableDto>>;

public sealed record GetTableSessionQuery(ulong SessionId) : IRequest<TableSessionDto>;

public sealed class GetOperationalTablesHandler(ITableOperationsReadRepository repository)
    : IRequestHandler<GetOperationalTablesQuery, IReadOnlyList<OperationalTableDto>>
{
    public Task<IReadOnlyList<OperationalTableDto>> Handle(
        GetOperationalTablesQuery request,
        CancellationToken cancellationToken) =>
        repository.GetTablesAsync(
            request.Search,
            request.AreaId,
            request.Status,
            cancellationToken);
}

public sealed class GetTableSessionHandler(ITableOperationsReadRepository repository)
    : IRequestHandler<GetTableSessionQuery, TableSessionDto>
{
    public async Task<TableSessionDto> Handle(
        GetTableSessionQuery request,
        CancellationToken cancellationToken) =>
        await repository.GetSessionAsync(request.SessionId, cancellationToken)
        ?? throw new NotFoundException($"Table session '{request.SessionId}' was not found.");
}
