using Business.Application.Abstractions.Persistence.Restaurant;
using Business.Application.Common.Exceptions;
using Business.Application.Restaurant.Foods.Dtos;
using FluentValidation;
using MediatR;

namespace Business.Application.Restaurant.Foods.UpdateFood;

public sealed record UpdateFoodCommand(
    string Code,
    ulong CategoryId,
    string Name,
    string? Description,
    string? ImageUrl,
    int DisplayOrder,
    bool IsActive,
    DateTime Version,
    ulong? ActorId) : IRequest<FoodDto>;

public sealed class UpdateFoodValidator : AbstractValidator<UpdateFoodCommand>
{
    public UpdateFoodValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.CategoryId).GreaterThan(0UL);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ImageUrl).MaximumLength(1000);
        RuleFor(x => x.Version).NotEmpty();
    }
}

public sealed class UpdateFoodCommandHandler(
    IFoodRepository repository,
    IFoodReadRepository reads) : IRequestHandler<UpdateFoodCommand, FoodDto>
{
    public async Task<FoodDto> Handle(
        UpdateFoodCommand request,
        CancellationToken cancellationToken)
    {
        var code = FoodRules.CleanCode(request.Code);
        var food = await repository.GetByCodeAsync(code, true, cancellationToken)
            ?? throw new NotFoundException($"Food '{code}' was not found.");
        if (food.UpdatedDate != request.Version)
            throw new ConflictException("Food was changed by another user.");
        if (food.CategoryId != request.CategoryId
            && !await repository.CategoryIsActiveAsync(request.CategoryId, cancellationToken))
            throw new ConflictException("Category does not exist or is inactive.", "categoryId");

        food.CategoryId = request.CategoryId;
        food.Name = FoodRules.Clean(request.Name);
        food.Description = FoodRules.CleanOptional(request.Description);
        food.ImageUrl = FoodRules.CleanOptional(request.ImageUrl);
        food.DisplayOrder = request.DisplayOrder;
        food.IsActive = request.IsActive;
        food.UpdatedBy = request.ActorId;
        await repository.SaveAsync(cancellationToken);
        return await reads.GetByCodeAsync(code, cancellationToken)
            ?? throw new InvalidOperationException("Updated food could not be read.");
    }
}
