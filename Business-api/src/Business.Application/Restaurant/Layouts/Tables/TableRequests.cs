using Business.Application.Abstractions.Persistence.Restaurant;
using Business.Application.Common.Exceptions;
using Business.Application.Restaurant.Layouts.Dtos;
using Business.Domain.Entities.Restaurant;
using FluentValidation;
using MediatR;

namespace Business.Application.Restaurant.Layouts.Tables;

public sealed record GetTablesQuery(string? Search, ulong? AreaId, string? Status, bool? IsActive, string Sort, int Page, int PageSize) : IRequest<PagedLayoutDto<TableDto>>;
public sealed record GetTableQuery(string Code) : IRequest<TableDto>;
public sealed record CreateTableCommand(ulong AreaId, string Code, string Name, int Capacity) : IRequest<TableDto>;
public sealed record UpdateTableCommand(string Code, ulong AreaId, string Name, int Capacity, DateTime Version) : IRequest<TableDto>;
public sealed record SetTableActivationCommand(string Code, bool IsActive, DateTime Version) : IRequest<TableDto>;

public sealed class CreateTableValidator : AbstractValidator<CreateTableCommand>
{
    public CreateTableValidator() { RuleFor(x => x.Code).NotEmpty().MaximumLength(50); RuleFor(x => x.Name).NotEmpty().MaximumLength(150); RuleFor(x => x.Capacity).InclusiveBetween(1, 1000); }
}
public sealed class UpdateTableValidator : AbstractValidator<UpdateTableCommand>
{
    public UpdateTableValidator() { RuleFor(x => x.Name).NotEmpty().MaximumLength(150); RuleFor(x => x.Capacity).InclusiveBetween(1, 1000); RuleFor(x => x.Version).NotEmpty(); }
}
public sealed class SetTableActivationValidator : AbstractValidator<SetTableActivationCommand>
{
    public SetTableActivationValidator() { RuleFor(x => x.Version).NotEmpty(); }
}

public sealed class GetTablesHandler(ILayoutReadRepository repository) : IRequestHandler<GetTablesQuery, PagedLayoutDto<TableDto>>
{
    public Task<PagedLayoutDto<TableDto>> Handle(GetTablesQuery request, CancellationToken token) => repository.GetTablesAsync(request.Search, request.AreaId, request.Status, request.IsActive, request.Sort, request.Page, request.PageSize, token);
}
public sealed class GetTableHandler(ILayoutReadRepository repository) : IRequestHandler<GetTableQuery, TableDto>
{
    public async Task<TableDto> Handle(GetTableQuery request, CancellationToken token) => await repository.GetTableAsync(LayoutRules.Code(request.Code), token) ?? throw new NotFoundException($"Table '{request.Code}' was not found.");
}
public sealed class CreateTableHandler(ILayoutRepository repository) : IRequestHandler<CreateTableCommand, TableDto>
{
    public async Task<TableDto> Handle(CreateTableCommand request, CancellationToken token)
    {
        var code = LayoutRules.Code(request.Code);
        if (await repository.TableCodeExistsAsync(code, token)) throw new ConflictException("Table code is already in use.", "code");
        if (!await repository.IsActiveAreaAsync(request.AreaId, token)) throw new ConflictException("Selected area does not exist or is inactive.", "areaId");
        var area = await repository.GetAreaByIdAsync(request.AreaId, token)
            ?? throw new ConflictException("Selected area does not exist.", "areaId");
        var now = DateTime.UtcNow;
        var table = new RestaurantTable { AreaId = request.AreaId, Code = code, Name = LayoutRules.Text(request.Name), Capacity = request.Capacity, Status = "Available", IsActive = true, CreatedDate = now, UpdatedDate = now, Area = area! };
        await repository.AddTableAsync(table, token);
        return LayoutRules.Table(table);
    }
}

public sealed class UpdateTableHandler(ILayoutRepository repository) : IRequestHandler<UpdateTableCommand, TableDto>
{
    public async Task<TableDto> Handle(UpdateTableCommand request, CancellationToken token)
    {
        var table = await repository.GetTableByCodeAsync(LayoutRules.Code(request.Code), token) ?? throw new NotFoundException($"Table '{request.Code}' was not found.");
        if (request.AreaId != table.AreaId && await repository.HasOpenSessionAsync(table.Id, token)) throw new ConflictException("A table with an open session cannot be moved.", "areaId");
        if (!await repository.IsActiveAreaAsync(request.AreaId, token)) throw new ConflictException("Selected area does not exist or is inactive.", "areaId");
        table.AreaId = request.AreaId;
        table.Area = await repository.GetAreaByIdAsync(request.AreaId, token)
            ?? throw new ConflictException("Selected area does not exist.", "areaId");
        table.Name = LayoutRules.Text(request.Name);
        table.Capacity = request.Capacity;
        await repository.SaveTableAsync(table, request.Version, token);
        return LayoutRules.Table(table, await repository.HasOpenSessionAsync(table.Id, token));
    }
}

public sealed class SetTableActivationHandler(ILayoutRepository repository) : IRequestHandler<SetTableActivationCommand, TableDto>
{
    public async Task<TableDto> Handle(SetTableActivationCommand request, CancellationToken token)
    {
        var table = await repository.GetTableByCodeAsync(LayoutRules.Code(request.Code), token) ?? throw new NotFoundException($"Table '{request.Code}' was not found.");
        var open = await repository.HasOpenSessionAsync(table.Id, token);
        if (!request.IsActive && (open || table.Status is "Occupied" or "Cleaning")) throw new ConflictException("Live or cleaning tables cannot be disabled.", "isActive");
        if (request.IsActive && !await repository.IsActiveAreaAsync(table.AreaId, token)) throw new ConflictException("A table cannot be activated in an inactive area.", "areaId");
        table.IsActive = request.IsActive; table.Status = request.IsActive ? "Available" : "Disabled";
        await repository.SaveTableAsync(table, request.Version, token);
        return LayoutRules.Table(table, false);
    }
}
