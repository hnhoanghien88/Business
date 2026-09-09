using System.Security.Claims;
using Business.Api.Models;
using Business.Application.Common.Authorization;
using Business.Application.Restaurant.TableOperations.Commands;
using Business.Application.Restaurant.TableOperations.Dtos;
using Business.Application.Restaurant.TableOperations.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Business.Api.Controllers.Restaurant;

[ApiController]
[Authorize]
[Route("api/restaurant/table-operations")]
public sealed class TableOperationsController(
    ISender sender,
    IAuthorizationService authorizationService) : ControllerBase
{
    [HttpGet("tables")]
    [Authorize(Policy = TableOperationsPermissions.Read)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<OperationalTableDto>>>> GetTables(
        [FromQuery] string? search,
        [FromQuery] ulong? areaId,
        [FromQuery] string? status,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetOperationalTablesQuery(search, areaId, status),
            cancellationToken);
        return Ok(new ApiResponse<IReadOnlyList<OperationalTableDto>>(
            true,
            result,
            "Table snapshot retrieved."));
    }

    [HttpGet("sessions/{sessionId:long}")]
    [Authorize(Policy = TableOperationsPermissions.Read)]
    public async Task<ActionResult<ApiResponse<TableSessionDto>>> GetSession(
        ulong sessionId,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetTableSessionQuery(sessionId),
            cancellationToken);
        return Ok(new ApiResponse<TableSessionDto>(true, result, "Session retrieved."));
    }

    [HttpPost("tables/{code}/open")]
    [Authorize(Policy = TableOperationsPermissions.Open)]
    public async Task<ActionResult<ApiResponse<TableSessionDto>>> Open(
        string code,
        OpenSessionRequest request,
        CancellationToken cancellationToken)
    {
        await RequireOverrideAsync(request.OverrideCapacity);
        var session = await sender.Send(
            new OpenTableCommand(
                code,
                request.GuestCount,
                request.Note,
                request.OverrideCapacity,
                request.OverrideReason,
                ActorId()),
            cancellationToken);
        var result = await sender.Send(
            new GetTableSessionQuery(session.Id),
            cancellationToken);
        return Ok(new ApiResponse<TableSessionDto>(true, result, "Table opened."));
    }

    [HttpPost("sessions/{sessionId:long}/close")]
    [Authorize(Policy = TableOperationsPermissions.Close)]
    public async Task<ActionResult<ApiResponse<TableSessionDto>>> Close(
        ulong sessionId,
        CloseSessionRequest request,
        CancellationToken cancellationToken)
    {
        await RequireOverrideAsync(request.OverrideObligations);
        await sender.Send(
            new CloseTableSessionCommand(
                sessionId,
                request.OverrideObligations,
                request.OverrideReason,
                ActorId()),
            cancellationToken);
        var result = await sender.Send(
            new GetTableSessionQuery(sessionId),
            cancellationToken);
        return Ok(new ApiResponse<TableSessionDto>(true, result, "Session closed."));
    }

    [HttpPost("tables/{code}/mark-clean")]
    [Authorize(Policy = TableOperationsPermissions.Clean)]
    public async Task<ActionResult<ApiResponse<bool>>> MarkClean(
        string code,
        CancellationToken cancellationToken)
    {
        await sender.Send(
            new MarkTableCleanCommand(code, ActorId()),
            cancellationToken);
        return Ok(new ApiResponse<bool>(true, true, "Table marked Available."));
    }

    private async Task RequireOverrideAsync(bool requested)
    {
        if (!requested)
        {
            return;
        }
        var authorized = await authorizationService.AuthorizeAsync(
            User,
            null,
            TableOperationsPermissions.Override);
        if (!authorized.Succeeded)
        {
            throw new UnauthorizedAccessException(
                "Table operation override permission is required.");
        }
    }

    private ulong? ActorId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");
        return ulong.TryParse(value, out var actorId) ? actorId : null;
    }

    public sealed record OpenSessionRequest(
        int GuestCount,
        string? Note,
        bool OverrideCapacity = false,
        string? OverrideReason = null);

    public sealed record CloseSessionRequest(
        bool OverrideObligations = false,
        string? OverrideReason = null);
}
