using Business.Application.Abstractions.Persistence.Restaurant;
using Business.Application.Common.Exceptions;
using Business.Application.Restaurant.Foods.Dtos;
using FluentValidation;
using MediatR;

namespace Business.Application.Restaurant.Foods.Availability;

public sealed record ChangeFoodAvailabilityCommand(
    string FoodCode,
    string VariantCode,
    bool IsAvailable,
    string? Reason,
    DateTime Version,
    ulong? ActorId) : IRequest<FoodVariantDto>;

public sealed class ChangeFoodAvailabilityValidator
    : AbstractValidator<ChangeFoodAvailabilityCommand>
{
    public ChangeFoodAvailabilityValidator()
    {
        RuleFor(x => x.Reason).MaximumLength(500);
        RuleFor(x => x.Version).NotEmpty();
    }
}

public sealed class ChangeFoodAvailabilityCommandHandler(
    IFoodRepository repository) : IRequestHandler<ChangeFoodAvailabilityCommand, FoodVariantDto>
{
    public async Task<FoodVariantDto> Handle(
        ChangeFoodAvailabilityCommand request,
        CancellationToken cancellationToken)
    {
        var food = await repository.GetByCodeAsync(
            FoodRules.CleanCode(request.FoodCode),
            true,
            cancellationToken)
            ?? throw new NotFoundException($"Food '{request.FoodCode}' was not found.");
        var variant = food.Variants.SingleOrDefault(
            x => x.Code == FoodRules.CleanCode(request.VariantCode))
            ?? throw new NotFoundException($"Variant '{request.VariantCode}' was not found.");
        if (variant.UpdatedDate != request.Version)
            throw new ConflictException("Variant was changed by another user.");
        variant.IsAvailable = request.IsAvailable;
        variant.SoldOutReason = request.IsAvailable
            ? null
            : FoodRules.CleanOptional(request.Reason);
        variant.UpdatedBy = request.ActorId;
        await repository.SaveAsync(cancellationToken);
        return FoodRules.ToDto(variant);
    }
}
