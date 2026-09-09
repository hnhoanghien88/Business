using Business.Application.Abstractions.Persistence.Restaurant;
using Business.Application.Common.Exceptions;
using MediatR;

namespace Business.Application.Restaurant.Foods.DeactivateFood;

public sealed record DeactivateFoodCommand(
    string Code,
    DateTime? Version,
    ulong? ActorId) : IRequest;

public sealed class DeactivateFoodCommandHandler(
    IFoodRepository repository) : IRequestHandler<DeactivateFoodCommand>
{
    public async Task Handle(
        DeactivateFoodCommand request,
        CancellationToken cancellationToken)
    {
        var code = FoodRules.CleanCode(request.Code);
        var food = await repository.GetByCodeAsync(code, true, cancellationToken)
            ?? throw new NotFoundException($"Food '{code}' was not found.");
        if (request.Version.HasValue && food.UpdatedDate != request.Version.Value)
            throw new ConflictException("Food was changed by another user.");

        food.IsActive = false;
        food.UpdatedBy = request.ActorId;
        foreach (var variant in food.Variants)
            variant.IsActive = false;
        await repository.SaveAsync(cancellationToken);
    }
}
