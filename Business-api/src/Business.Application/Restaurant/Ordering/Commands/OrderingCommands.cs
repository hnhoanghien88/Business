using Business.Application.Abstractions.Persistence.Restaurant;
using Business.Application.Restaurant.Ordering.Dtos;
using FluentValidation;
using MediatR;

namespace Business.Application.Restaurant.Ordering.Commands;

public sealed record PreviewOrderCommand(
    ulong SessionId, IReadOnlyList<OrderLineInput> Items, string? PromotionCode)
    : IRequest<OrderPreviewDto>;
public sealed record ConfirmOrderCommand(
    ulong SessionId, Guid RequestId, decimal ObservedTotal, IReadOnlyList<OrderLineInput> Items,
    string? PromotionCode, ulong? ActorId) : IRequest<OrderConfirmationDto>;

public sealed class PreviewOrderValidator : AbstractValidator<PreviewOrderCommand>
{
    public PreviewOrderValidator()
    {
        RuleFor(value => value.SessionId).GreaterThan(0UL);
        RuleFor(value => value.Items).NotEmpty().Must(value => value.Count <= 100);
        RuleForEach(value => value.Items).ChildRules(item =>
        {
            item.RuleFor(value => value.FoodVariantId).GreaterThan(0UL);
            item.RuleFor(value => value.Quantity).InclusiveBetween(1, 1000);
            item.RuleFor(value => value.Note).MaximumLength(500);
        });
        RuleFor(value => value.PromotionCode).MaximumLength(50);
    }
}
public sealed class ConfirmOrderValidator : AbstractValidator<ConfirmOrderCommand>
{
    public ConfirmOrderValidator()
    {
        RuleFor(value => value.SessionId).GreaterThan(0UL);
        RuleFor(value => value.Items).NotEmpty().Must(value => value.Count <= 100);
        RuleForEach(value => value.Items).ChildRules(item =>
        {
            item.RuleFor(value => value.FoodVariantId).GreaterThan(0UL);
            item.RuleFor(value => value.Quantity).InclusiveBetween(1, 1000);
            item.RuleFor(value => value.Note).MaximumLength(500);
        });
        RuleFor(value => value.PromotionCode).MaximumLength(50);
        RuleFor(value => value.RequestId).NotEmpty();
        RuleFor(value => value.ObservedTotal).GreaterThanOrEqualTo(0m);
    }
}
public sealed class PreviewOrderHandler(IOrderingRepository repository)
    : IRequestHandler<PreviewOrderCommand, OrderPreviewDto>
{
    public Task<OrderPreviewDto> Handle(PreviewOrderCommand request, CancellationToken cancellationToken) =>
        repository.PreviewAsync(request.SessionId, request.Items, request.PromotionCode, cancellationToken);
}
public sealed class ConfirmOrderHandler(IOrderingRepository repository)
    : IRequestHandler<ConfirmOrderCommand, OrderConfirmationDto>
{
    public Task<OrderConfirmationDto> Handle(ConfirmOrderCommand request, CancellationToken cancellationToken) =>
        repository.ConfirmAsync(request.SessionId, request.RequestId, request.ObservedTotal,
            request.Items, request.PromotionCode, request.ActorId, cancellationToken);
}
