using Business.Application.Abstractions.Persistence.Restaurant;
using Business.Application.Common.Exceptions;
using Business.Application.Restaurant.Layouts.Dtos;
using Business.Domain.Entities.Restaurant;
using FluentValidation;
using MediatR;

namespace Business.Application.Restaurant.Layouts.Areas;

public sealed record GetAreasQuery(string? Search, bool? IsActive, string Sort, int Page, int PageSize) : IRequest<PagedLayoutDto<AreaDto>>;
public sealed record GetAreaQuery(string Code) : IRequest<AreaDto>;
public sealed record CreateAreaCommand(string Code, string Name, string? Description, int DisplayOrder) : IRequest<AreaDto>;
public sealed record UpdateAreaCommand(string Code, string Name, string? Description, int DisplayOrder, bool IsActive, bool ConfirmImpact, DateTime Version) : IRequest<AreaDto>;

public sealed class CreateAreaValidator : AbstractValidator<CreateAreaCommand>
{
    public CreateAreaValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Description).MaximumLength(500);
    }
}

public sealed class UpdateAreaValidator : AbstractValidator<UpdateAreaCommand>
{
    public UpdateAreaValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Description).MaximumLength(500);
        RuleFor(x => x.Version).NotEmpty();
    }
}

public sealed class GetAreasHandler(ILayoutReadRepository repository) : IRequestHandler<GetAreasQuery, PagedLayoutDto<AreaDto>>
{
    public Task<PagedLayoutDto<AreaDto>> Handle(GetAreasQuery request, CancellationToken token) =>
        repository.GetAreasAsync(request.Search, request.IsActive, request.Sort, request.Page, request.PageSize, token);
}

public sealed class GetAreaHandler(ILayoutReadRepository repository) : IRequestHandler<GetAreaQuery, AreaDto>
{
    public async Task<AreaDto> Handle(GetAreaQuery request, CancellationToken token) =>
        await repository.GetAreaAsync(LayoutRules.Code(request.Code), token)
        ?? throw new NotFoundException($"Area '{request.Code}' was not found.");
}

public sealed class CreateAreaHandler(ILayoutRepository repository) : IRequestHandler<CreateAreaCommand, AreaDto>
{
    public async Task<AreaDto> Handle(CreateAreaCommand request, CancellationToken token)
    {
        var code = LayoutRules.Code(request.Code);
        if (await repository.AreaCodeExistsAsync(code, token)) throw new ConflictException("Area code is already in use.", "code");
        var now = DateTime.UtcNow;
        var area = new RestaurantArea { Code = code, Name = LayoutRules.Text(request.Name), Description = LayoutRules.Optional(request.Description), DisplayOrder = request.DisplayOrder, CreatedDate = now, UpdatedDate = now };
        await repository.AddAreaAsync(area, token);
        return LayoutRules.Area(area);
    }
}

public sealed class UpdateAreaHandler(ILayoutRepository repository) : IRequestHandler<UpdateAreaCommand, AreaDto>
{
    public async Task<AreaDto> Handle(UpdateAreaCommand request, CancellationToken token)
    {
        var area = await repository.GetAreaByCodeAsync(LayoutRules.Code(request.Code), token)
            ?? throw new NotFoundException($"Area '{request.Code}' was not found.");
        var count = await repository.GetAreaTableCountAsync(area.Id, token);
        if (area.IsActive && !request.IsActive && count > 0 && !request.ConfirmImpact)
            throw new ConflictException("Confirm deactivation impact before continuing.", "confirmImpact");
        area.Name = LayoutRules.Text(request.Name);
        area.Description = LayoutRules.Optional(request.Description);
        area.DisplayOrder = request.DisplayOrder;
        area.IsActive = request.IsActive;
        await repository.SaveAreaAsync(area, request.Version, token);
        return LayoutRules.Area(area, count);
    }
}
