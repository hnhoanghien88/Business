using Business.Application.Restaurant.Ordering.Dtos;

namespace Business.Application.Abstractions.Persistence.Restaurant;

public interface IOrderingRepository
{
    Task<OrderPreviewDto> PreviewAsync(ulong sessionId, IReadOnlyList<OrderLineInput> items, string? promotionCode, CancellationToken cancellationToken);
    Task<OrderConfirmationDto> ConfirmAsync(ulong sessionId, Guid requestId, decimal observedTotal, IReadOnlyList<OrderLineInput> items, string? promotionCode, ulong? actorId, CancellationToken cancellationToken);
}
