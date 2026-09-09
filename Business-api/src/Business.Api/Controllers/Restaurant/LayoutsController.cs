using Business.Api.Models;
using Business.Application.Common.Authorization;
using Business.Application.Restaurant.Layouts.Areas;
using Business.Application.Restaurant.Layouts.Dtos;
using Business.Application.Restaurant.Layouts.Tables;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Business.Api.Controllers.Restaurant;

[ApiController]
[Authorize]
[Route("api/restaurant/layouts")]
public sealed class LayoutsController(ISender sender) : ControllerBase
{
    [HttpGet("areas")]
    [Authorize(Policy = LayoutsPermissions.Read)]
    public async Task<ActionResult<ApiResponse<PagedLayoutDto<AreaDto>>>> GetAreas(
        [FromQuery] string? search,
        [FromQuery] bool? isActive,
        [FromQuery] string sort = "displayOrder",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken token = default)
    {
        var value = await sender.Send(new GetAreasQuery(search, isActive, sort, page, pageSize), token);
        return Ok(new ApiResponse<PagedLayoutDto<AreaDto>>(true, value, "Areas retrieved."));
    }

    [HttpGet("areas/{code}")]
    [Authorize(Policy = LayoutsPermissions.Read)]
    public async Task<ActionResult<ApiResponse<AreaDto>>> GetArea(string code, CancellationToken token)
    {
        var value = await sender.Send(new GetAreaQuery(code), token);
        return Ok(new ApiResponse<AreaDto>(true, value, "Area retrieved."));
    }

    [HttpPost("areas")]
    [Authorize(Policy = LayoutsPermissions.Create)]
    public async Task<ActionResult<ApiResponse<AreaDto>>> CreateArea(CreateAreaRequest request, CancellationToken token)
    {
        var value = await sender.Send(new CreateAreaCommand(request.Code, request.Name, request.Description, request.DisplayOrder), token);
        return CreatedAtAction(nameof(GetArea), new { code = value.Code }, new ApiResponse<AreaDto>(true, value, "Area created."));
    }

    [HttpPut("areas/{code}")]
    [Authorize(Policy = LayoutsPermissions.Update)]
    public async Task<ActionResult<ApiResponse<AreaDto>>> UpdateArea(string code, UpdateAreaRequest request, CancellationToken token)
    {
        var value = await sender.Send(new UpdateAreaCommand(code, request.Name, request.Description, request.DisplayOrder, request.IsActive, request.ConfirmImpact, request.Version), token);
        return Ok(new ApiResponse<AreaDto>(true, value, "Area updated."));
    }

    [HttpGet("tables")]
    [Authorize(Policy = LayoutsPermissions.Read)]
    public async Task<ActionResult<ApiResponse<PagedLayoutDto<TableDto>>>> GetTables(
        [FromQuery] string? search,
        [FromQuery] ulong? areaId,
        [FromQuery] string? status,
        [FromQuery] bool? isActive,
        [FromQuery] string sort = "area",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken token = default)
    {
        var value = await sender.Send(new GetTablesQuery(search, areaId, status, isActive, sort, page, pageSize), token);
        return Ok(new ApiResponse<PagedLayoutDto<TableDto>>(true, value, "Tables retrieved."));
    }

    [HttpGet("tables/{code}")]
    [Authorize(Policy = LayoutsPermissions.Read)]
    public async Task<ActionResult<ApiResponse<TableDto>>> GetTable(string code, CancellationToken token)
    {
        var value = await sender.Send(new GetTableQuery(code), token);
        return Ok(new ApiResponse<TableDto>(true, value, "Table retrieved."));
    }

    [HttpPost("tables")]
    [Authorize(Policy = LayoutsPermissions.Create)]
    public async Task<ActionResult<ApiResponse<TableDto>>> CreateTable(CreateTableRequest request, CancellationToken token)
    {
        var value = await sender.Send(new CreateTableCommand(request.AreaId, request.Code, request.Name, request.Capacity), token);
        return CreatedAtAction(nameof(GetTable), new { code = value.Code }, new ApiResponse<TableDto>(true, value, "Table created."));
    }

    [HttpPut("tables/{code}")]
    [Authorize(Policy = LayoutsPermissions.Update)]
    public async Task<ActionResult<ApiResponse<TableDto>>> UpdateTable(string code, UpdateTableRequest request, CancellationToken token)
    {
        var value = await sender.Send(new UpdateTableCommand(code, request.AreaId, request.Name, request.Capacity, request.Version), token);
        return Ok(new ApiResponse<TableDto>(true, value, "Table updated."));
    }

    [HttpPut("tables/{code}/activation")]
    [Authorize(Policy = LayoutsPermissions.Update)]
    public async Task<ActionResult<ApiResponse<TableDto>>> SetActivation(string code, ActivationRequest request, CancellationToken token)
    {
        var value = await sender.Send(new SetTableActivationCommand(code, request.IsActive, request.Version), token);
        return Ok(new ApiResponse<TableDto>(true, value, "Table activation updated."));
    }

    public sealed record CreateAreaRequest(string Code, string Name, string? Description, int DisplayOrder);
    public sealed record UpdateAreaRequest(string Name, string? Description, int DisplayOrder, bool IsActive, bool ConfirmImpact, DateTime Version);
    public sealed record CreateTableRequest(ulong AreaId, string Code, string Name, int Capacity);
    public sealed record UpdateTableRequest(ulong AreaId, string Name, int Capacity, DateTime Version);
    public sealed record ActivationRequest(bool IsActive, DateTime Version);
}
