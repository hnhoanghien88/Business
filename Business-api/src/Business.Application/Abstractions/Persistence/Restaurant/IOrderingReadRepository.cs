using Business.Application.Restaurant.Ordering.Dtos;

namespace Business.Application.Abstractions.Persistence.Restaurant;

public interface IOrderingReadRepository
{
    Task<OrderingMenuDto?> GetMenuAsync(ulong sessionId, string? search, ulong? categoryId, CancellationToken cancellationToken);
    Task<SessionOrderingDto?> GetSessionOrdersAsync(ulong sessionId, CancellationToken cancellationToken);
}
