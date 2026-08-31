using Business.Application.Abstractions.Persistence.Restaurant;
using Business.Application.Common.Exceptions;
using Business.Application.Restaurant.Categories.Dtos;
using FluentValidation;
using MediatR;

namespace Business.Application.Restaurant.Categories.UpdateCategory;

public sealed record UpdateCategoryCommand(
    string Code,
    ulong? ParentId,
    string Name,
    string? Description,
    int DisplayOrder,
    bool IsActive,
    DateTime Version) : IRequest<CategoryDto>;

public sealed class UpdateCategoryValidator : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Description).MaximumLength(500);
        RuleFor(x => x.Version).NotEmpty();
    }
}

public sealed class UpdateCategoryCommandHandler(ICategoryRepository repository)
    : IRequestHandler<UpdateCategoryCommand, CategoryDto>
{
    public async Task<CategoryDto> Handle(
        UpdateCategoryCommand request,
        CancellationToken cancellationToken)
    {
        var code = CategoryRules.Clean(request.Code);
        var category = await repository.GetByCodeAsync(code, cancellationToken)
            ?? throw new NotFoundException($"Category '{code}' was not found.");

        if (request.ParentId == category.Id)
            throw new ConflictException("A category cannot be its own parent.", "parentId");
        if (request.ParentId is ulong parentId)
        {
            if (!await repository.IsValidActiveParentAsync(parentId, cancellationToken))
                throw new ConflictException("The selected parent does not exist or is inactive.", "parentId");
            if (await repository.IsDescendantAsync(category.Id, parentId, cancellationToken))
                throw new ConflictException("A descendant cannot be selected as parent.", "parentId");
        }

        category.ParentId = request.ParentId;
        category.Name = CategoryRules.Clean(request.Name);
        category.Description = CategoryRules.CleanOptional(request.Description);
        category.DisplayOrder = request.DisplayOrder;
        category.IsActive = request.IsActive;
        await repository.SaveAsync(category, request.Version, cancellationToken);
        return CategoryRules.ToDto(category);
    }
}
