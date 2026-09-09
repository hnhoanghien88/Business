using Business.Application.Abstractions.Persistence.Restaurant;
using Business.Domain.Entities.Restaurant;
using FluentValidation;
using MediatR;

namespace Business.Application.Restaurant.TableOperations.Commands;

public sealed record OpenTableCommand(
    string TableCode,
    int GuestCount,
    string? Note,
    bool OverrideCapacity,
    string? OverrideReason,
    ulong? ActorId) : IRequest<TableSession>;

public sealed record CloseTableSessionCommand(
    ulong SessionId,
    bool OverrideObligations,
    string? OverrideReason,
    ulong? ActorId) : IRequest<TableSession>;

public sealed record MarkTableCleanCommand(string TableCode, ulong? ActorId) : IRequest<RestaurantTable>;

public sealed class OpenTableValidator : AbstractValidator<OpenTableCommand>
{
    public OpenTableValidator()
    {
        RuleFor(x => x.TableCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.GuestCount).GreaterThan(0);
        RuleFor(x => x.Note).MaximumLength(500);
        RuleFor(x => x.OverrideReason)
            .NotEmpty()
            .MaximumLength(500)
            .When(x => x.OverrideCapacity);
    }
}

public sealed class CloseTableSessionValidator : AbstractValidator<CloseTableSessionCommand>
{
    public CloseTableSessionValidator()
    {
        RuleFor(x => x.SessionId).GreaterThan(0UL);
        RuleFor(x => x.OverrideReason)
            .NotEmpty()
            .MaximumLength(500)
            .When(x => x.OverrideObligations);
    }
}

public sealed class MarkTableCleanValidator : AbstractValidator<MarkTableCleanCommand>
{
    public MarkTableCleanValidator()
    {
        RuleFor(x => x.TableCode).NotEmpty().MaximumLength(50);
    }
}

public sealed class OpenTableHandler(ITableOperationsRepository repository)
    : IRequestHandler<OpenTableCommand, TableSession>
{
    public Task<TableSession> Handle(OpenTableCommand request, CancellationToken cancellationToken) =>
        repository.OpenAsync(
            request.TableCode.Trim().ToUpperInvariant(),
            request.GuestCount,
            request.Note,
            request.OverrideCapacity,
            request.OverrideReason,
            request.ActorId,
            cancellationToken);
}

public sealed class CloseTableSessionHandler(ITableOperationsRepository repository)
    : IRequestHandler<CloseTableSessionCommand, TableSession>
{
    public Task<TableSession> Handle(
        CloseTableSessionCommand request,
        CancellationToken cancellationToken) =>
        repository.CloseAsync(
            request.SessionId,
            request.OverrideObligations,
            request.OverrideReason,
            request.ActorId,
            cancellationToken);
}

public sealed class MarkTableCleanHandler(ITableOperationsRepository repository)
    : IRequestHandler<MarkTableCleanCommand, RestaurantTable>
{
    public Task<RestaurantTable> Handle(
        MarkTableCleanCommand request,
        CancellationToken cancellationToken) =>
        repository.MarkCleanAsync(
            request.TableCode.Trim().ToUpperInvariant(),
            request.ActorId,
            cancellationToken);
}
