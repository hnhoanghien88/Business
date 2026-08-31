using Business.Application.Abstractions.Persistence.Restaurant;
using Business.Application.Common.Exceptions;
using Business.Application.Restaurant.Categories.Dtos;
using Business.Domain.Entities.Restaurant;
using FluentValidation;
using MediatR;

namespace Business.Application.Restaurant.Categories.CreateCategory;

public sealed record CreateCategoryCommand(
    ulong? ParentId,
    string Code,
    string Name,
    string? Description,
    int DisplayOrder,
    bool IsActive) : IRequest<CategoryDto>;

public sealed class CreateCategoryValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Description).MaximumLength(500);
    }
}

public sealed class CreateCategoryCommandHandler(ICategoryRepository repository)
    : IRequestHandler<CreateCategoryCommand, CategoryDto>
{
    public async Task<CategoryDto> Handle(
        CreateCategoryCommand request,
        CancellationToken cancellationToken)
    {
        var code = CategoryRules.Clean(request.Code);
        if (await repository.CodeExistsAsync(code, cancellationToken))
            throw new ConflictException("Category code is already in use.", "code");
        if (request.ParentId is ulong parentId
            && !await repository.IsValidActiveParentAsync(parentId, cancellationToken))
            throw new ConflictException("The selected parent does not exist or is inactive.", "parentId");

        var now = DateTime.UtcNow;
        var category = new Category
        {
            ParentId = request.ParentId,
            Code = code,
            Name = CategoryRules.Clean(request.Name),
            Description = CategoryRules.CleanOptional(request.Description),
            DisplayOrder = request.DisplayOrder,
            IsActive = request.IsActive,
            CreatedDate = now,
            UpdatedDate = now
        };
        await repository.AddAsync(category, cancellationToken);
        return CategoryRules.ToDto(category);
    }
}
