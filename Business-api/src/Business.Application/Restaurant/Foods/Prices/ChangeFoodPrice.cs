using Business.Application.Abstractions.Persistence.Restaurant;
using Business.Application.Common.Exceptions;
using Business.Application.Restaurant.Foods.Dtos;
using FluentValidation;
using MediatR;

namespace Business.Application.Restaurant.Foods.Prices;

public sealed record ChangeFoodPriceCommand(
    string FoodCode,
    string VariantCode,
    decimal Price,
    DateTime Version,
    ulong? ActorId) : IRequest<FoodVariantDto>;

public sealed class ChangeFoodPriceValidator : AbstractValidator<ChangeFoodPriceCommand>
{
    public ChangeFoodPriceValidator()
    {
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Version).NotEmpty();
    }
}

public sealed class ChangeFoodPriceCommandHandler(
    IFoodRepository repository) : IRequestHandler<ChangeFoodPriceCommand, FoodVariantDto>
{
    public async Task<FoodVariantDto> Handle(
        ChangeFoodPriceCommand request,
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
        await repository.ChangePriceAsync(
            variant,
            request.Price,
            request.ActorId,
            cancellationToken);
        return FoodRules.ToDto(variant);
    }
}
