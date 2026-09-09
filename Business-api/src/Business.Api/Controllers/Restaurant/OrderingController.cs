using System.Security.Claims;
using Business.Api.Models;
using Business.Application.Common.Authorization;
using Business.Application.Restaurant.Ordering.Commands;
using Business.Application.Restaurant.Ordering.Dtos;
using Business.Application.Restaurant.Ordering.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Business.Api.Controllers.Restaurant;

[ApiController, Authorize, Route("api/restaurant/ordering/sessions/{sessionId:long}")]
public sealed class OrderingController(ISender sender) : ControllerBase
{
    [HttpGet("menu"), Authorize(Policy = OrderingPermissions.Read)]
    public async Task<ActionResult<ApiResponse<OrderingMenuDto>>> Menu(
        ulong sessionId, [FromQuery] string? search, [FromQuery] ulong? categoryId,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetOrderingMenuQuery(sessionId, search, categoryId), cancellationToken);
        return Ok(new ApiResponse<OrderingMenuDto>(true, result, "Menu retrieved successfully."));
    }

    [HttpGet("orders"), Authorize(Policy = OrderingPermissions.Read)]
    public async Task<ActionResult<ApiResponse<SessionOrderingDto>>> Orders(
        ulong sessionId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetSessionOrdersQuery(sessionId), cancellationToken);
        return Ok(new ApiResponse<SessionOrderingDto>(true, result, "Orders retrieved successfully."));
    }

    [HttpPost("promotion-preview"), Authorize(Policy = OrderingPermissions.Create)]
    public async Task<ActionResult<ApiResponse<OrderPreviewDto>>> Preview(
        ulong sessionId, PreviewRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new PreviewOrderCommand(sessionId, request.Items, request.PromotionCode), cancellationToken);
        return Ok(new ApiResponse<OrderPreviewDto>(true, result, "Current total calculated."));
    }

    [HttpPost("confirm"), Authorize(Policy = OrderingPermissions.Create)]
    public async Task<ActionResult<ApiResponse<OrderConfirmationDto>>> Confirm(
        ulong sessionId, ConfirmRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ConfirmOrderCommand(
            sessionId, request.RequestId, request.ObservedTotal, request.Items,
            request.PromotionCode, ActorId()), cancellationToken);
        var response = new ApiResponse<OrderConfirmationDto>(true, result, "Order sent to kitchen.");
        return result.IsReplay ? Ok(response) : StatusCode(StatusCodes.Status201Created, response);
    }

    private ulong? ActorId() => ulong.TryParse(User.FindFirstValue("sub"), out var id) ? id : null;
    public sealed record PreviewRequest(IReadOnlyList<OrderLineInput> Items, string? PromotionCode);
    public sealed record ConfirmRequest(
        Guid RequestId, decimal ObservedTotal, IReadOnlyList<OrderLineInput> Items, string? PromotionCode);
}
