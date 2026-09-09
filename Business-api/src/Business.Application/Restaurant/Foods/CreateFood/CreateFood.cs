using Business.Application.Abstractions.Persistence.Restaurant;
using Business.Application.Common.Exceptions;
using Business.Application.Restaurant.Foods.Dtos;
using Business.Domain.Entities.Restaurant;
using FluentValidation;
using MediatR;

namespace Business.Application.Restaurant.Foods.CreateFood;

public sealed record CreateFoodCommand(
    ulong CategoryId,
    string Code,
    string Name,
    string? Description,
    string? ImageUrl,
    int DisplayOrder,
    bool IsActive,
    ulong? ActorId) : IRequest<FoodDto>;

public sealed class CreateFoodValidator : AbstractValidator<CreateFoodCommand>
{
    public CreateFoodValidator()
    {
        RuleFor(x => x.CategoryId).GreaterThan(0UL);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ImageUrl).MaximumLength(1000);
    }
}

public sealed class CreateFoodCommandHandler(
    IFoodRepository repository,
    IFoodReadRepository reads) : IRequestHandler<CreateFoodCommand, FoodDto>
{
    public async Task<FoodDto> Handle(
        CreateFoodCommand request,
        CancellationToken cancellationToken)
    {
        var code = FoodRules.CleanCode(request.Code);
        if (await repository.CodeExistsAsync(code, cancellationToken))
            throw new ConflictException("Food code is already in use.", "code");
        if (!await repository.CategoryIsActiveAsync(request.CategoryId, cancellationToken))
            throw new ConflictException("Category does not exist or is inactive.", "categoryId");

        var now = DateTime.UtcNow;
        var food = new Food
        {
            CategoryId = request.CategoryId,
            Code = code,
            Name = FoodRules.Clean(request.Name),
            Description = FoodRules.CleanOptional(request.Description),
            ImageUrl = FoodRules.CleanOptional(request.ImageUrl),
            DisplayOrder = request.DisplayOrder,
            IsActive = request.IsActive,
            CreatedBy = request.ActorId,
            UpdatedBy = request.ActorId,
            CreatedDate = now,
            UpdatedDate = now,
            Category = null!
        };
        await repository.AddAsync(food, cancellationToken);
        return await reads.GetByCodeAsync(code, cancellationToken)
            ?? throw new InvalidOperationException("Created food could not be read.");
    }
}
