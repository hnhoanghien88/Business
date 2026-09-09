using Business.Application.Abstractions.Persistence.Restaurant;
using Business.Application.Common.Exceptions;
using Business.Application.Restaurant.Foods.Dtos;
using Business.Domain.Entities.Restaurant;
using FluentValidation;
using MediatR;

namespace Business.Application.Restaurant.Foods.Variants;

public sealed record CreateFoodVariantCommand(
    string FoodCode,
    string Code,
    string Name,
    decimal CurrentPrice,
    bool IsDefault,
    int DisplayOrder,
    bool IsActive,
    ulong? ActorId) : IRequest<FoodVariantDto>;

public sealed class CreateFoodVariantValidator : AbstractValidator<CreateFoodVariantCommand>
{
    public CreateFoodVariantValidator()
    {
        RuleFor(x => x.FoodCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.CurrentPrice).GreaterThanOrEqualTo(0);
    }
}

public sealed class CreateFoodVariantCommandHandler(
    IFoodRepository repository) : IRequestHandler<CreateFoodVariantCommand, FoodVariantDto>
{
    public async Task<FoodVariantDto> Handle(
        CreateFoodVariantCommand request,
        CancellationToken cancellationToken)
    {
        var food = await repository.GetByCodeAsync(
            FoodRules.CleanCode(request.FoodCode),
            true,
            cancellationToken)
            ?? throw new NotFoundException($"Food '{request.FoodCode}' was not found.");
        var code = FoodRules.CleanCode(request.Code);
        if (food.Variants.Any(x => x.Code == code))
            throw new ConflictException("Variant code is already in use.", "code");

        var now = DateTime.UtcNow;
        var makeDefault = request.IsActive
            && (request.IsDefault || !food.Variants.Any(x => x.IsActive));
        if (makeDefault)
            foreach (var current in food.Variants)
                current.IsDefault = false;
        var variant = new FoodVariant
        {
            FoodId = food.Id,
            Food = food,
            Code = code,
            Name = FoodRules.Clean(request.Name),
            CurrentPrice = request.CurrentPrice,
            IsDefault = makeDefault,
            IsAvailable = true,
            DisplayOrder = request.DisplayOrder,
            IsActive = request.IsActive,
            CreatedBy = request.ActorId,
            UpdatedBy = request.ActorId,
            CreatedDate = now,
            UpdatedDate = now
        };
        var history = new FoodPriceHistory
        {
            FoodVariant = variant,
            Price = request.CurrentPrice,
            EffectiveFrom = now,
            CreatedBy = request.ActorId,
            CreatedDate = now
        };
        await repository.AddVariantAsync(food, variant, history, cancellationToken);
        return FoodRules.ToDto(variant);
    }
}

public sealed record UpdateFoodVariantCommand(
    string FoodCode,
    string VariantCode,
    string Name,
    bool IsDefault,
    int DisplayOrder,
    bool IsActive,
    DateTime Version,
    ulong? ActorId) : IRequest<FoodVariantDto>;

public sealed class UpdateFoodVariantValidator : AbstractValidator<UpdateFoodVariantCommand>
{
    public UpdateFoodVariantValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Version).NotEmpty();
    }
}

public sealed class UpdateFoodVariantCommandHandler(
    IFoodRepository repository) : IRequestHandler<UpdateFoodVariantCommand, FoodVariantDto>
{
    public async Task<FoodVariantDto> Handle(
        UpdateFoodVariantCommand request,
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
        if (!request.IsActive && variant.IsDefault
            && food.Variants.Any(x => x.Id != variant.Id && x.IsActive))
            throw new ConflictException("Select another default before deactivating this variant.");
        if (variant.IsDefault && !request.IsDefault && request.IsActive
            && food.Variants.Any(x => x.Id != variant.Id && x.IsActive))
            throw new ConflictException("Select another default before clearing this variant.");

        if (request.IsDefault)
            foreach (var current in food.Variants)
                current.IsDefault = current.Id == variant.Id;
        variant.Name = FoodRules.Clean(request.Name);
        variant.DisplayOrder = request.DisplayOrder;
        variant.IsActive = request.IsActive;
        if (!request.IsActive)
            variant.IsDefault = false;
        variant.UpdatedBy = request.ActorId;
        await repository.SaveAsync(cancellationToken);
        return FoodRules.ToDto(variant);
    }
}
