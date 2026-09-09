using Business.Application.Abstractions.Persistence.Restaurant;
using Business.Application.Common.Exceptions;
using Business.Application.Restaurant.Ordering.Dtos;
using MediatR;

namespace Business.Application.Restaurant.Ordering.Queries;

public sealed record GetOrderingMenuQuery(ulong SessionId, string? Search, ulong? CategoryId)
    : IRequest<OrderingMenuDto>;
public sealed record GetSessionOrdersQuery(ulong SessionId) : IRequest<SessionOrderingDto>;

public sealed class GetOrderingMenuHandler(IOrderingReadRepository repository)
    : IRequestHandler<GetOrderingMenuQuery, OrderingMenuDto>
{
    public async Task<OrderingMenuDto> Handle(GetOrderingMenuQuery request, CancellationToken cancellationToken) =>
        await repository.GetMenuAsync(request.SessionId, request.Search, request.CategoryId, cancellationToken)
        ?? throw new NotFoundException($"Open table session '{request.SessionId}' was not found.");
}

public sealed class GetSessionOrdersHandler(IOrderingReadRepository repository)
    : IRequestHandler<GetSessionOrdersQuery, SessionOrderingDto>
{
    public async Task<SessionOrderingDto> Handle(GetSessionOrdersQuery request, CancellationToken cancellationToken) =>
        await repository.GetSessionOrdersAsync(request.SessionId, cancellationToken)
        ?? throw new NotFoundException($"Table session '{request.SessionId}' was not found.");
}
