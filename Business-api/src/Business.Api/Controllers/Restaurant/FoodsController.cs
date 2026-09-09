using System.Security.Claims;
using Business.Api.Models;
using Business.Application.Common.Authorization;
using Business.Application.Restaurant.Foods.Availability;
using Business.Application.Restaurant.Foods.CreateFood;
using Business.Application.Restaurant.Foods.DeactivateFood;
using Business.Application.Restaurant.Foods.Dtos;
using Business.Application.Restaurant.Foods.GetFoodByCode;
using Business.Application.Restaurant.Foods.GetFoods;
using Business.Application.Restaurant.Foods.Prices;
using Business.Application.Restaurant.Foods.UpdateFood;
using Business.Application.Restaurant.Foods.Variants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Business.Api.Controllers.Restaurant;

[ApiController]
[Authorize]
[Route("api/restaurant/foods")]
public sealed class FoodsController(ISender sender) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = FoodsPermissions.Read)]
    public async Task<ActionResult<ApiResponse<PagedFoodsDto>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] ulong? categoryId,
        [FromQuery] string status = "all",
        [FromQuery] string availability = "all",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var foods = await sender.Send(
            new GetFoodsQuery(
                search,
                categoryId,
                ParseStatus(status),
                ParseAvailability(availability),
                page,
                pageSize),
            cancellationToken);
        return Ok(new ApiResponse<PagedFoodsDto>(
            true,
            foods,
            "Foods retrieved successfully."));
    }

    [HttpGet("{code}")]
    [Authorize(Policy = FoodsPermissions.Read)]
    public async Task<ActionResult<ApiResponse<FoodDto>>> GetByCode(
        string code,
        CancellationToken cancellationToken)
    {
        var food = await sender.Send(
            new GetFoodByCodeQuery(code),
            cancellationToken);
        return Ok(new ApiResponse<FoodDto>(
            true,
            food,
            "Food retrieved successfully."));
    }

    [HttpPost]
    [Authorize(Policy = FoodsPermissions.Create)]
    public async Task<ActionResult<ApiResponse<FoodDto>>> Create(
        CreateRequest request,
        CancellationToken cancellationToken)
    {
        var food = await sender.Send(
            new CreateFoodCommand(
                request.CategoryId,
                request.Code,
                request.Name,
                request.Description,
                request.ImageUrl,
                request.DisplayOrder,
                request.IsActive,
                ActorId()),
            cancellationToken);
        return CreatedAtAction(
            nameof(GetByCode),
            new { code = food.Code },
            new ApiResponse<FoodDto>(true, food, "Food created successfully."));
    }

    [HttpPut("{code}")]
    [Authorize(Policy = FoodsPermissions.Update)]
    public async Task<ActionResult<ApiResponse<FoodDto>>> Update(
        string code,
        UpdateRequest request,
        CancellationToken cancellationToken)
    {
        var food = await sender.Send(
            new UpdateFoodCommand(
                code,
                request.CategoryId,
                request.Name,
                request.Description,
                request.ImageUrl,
                request.DisplayOrder,
                request.IsActive,
                request.Version,
                ActorId()),
            cancellationToken);
        return Ok(new ApiResponse<FoodDto>(
            true,
            food,
            "Food updated successfully."));
    }

    [HttpDelete("{code}")]
    [Authorize(Policy = FoodsPermissions.Delete)]
    public async Task<IActionResult> Deactivate(
        string code,
        [FromQuery] DateTime? version,
        CancellationToken cancellationToken)
    {
        await sender.Send(
            new DeactivateFoodCommand(code, version, ActorId()),
            cancellationToken);
        return NoContent();
    }

    [HttpPost("{code}/variants")]
    [Authorize(Policy = FoodsPermissions.Update)]
    public async Task<ActionResult<ApiResponse<FoodVariantDto>>> CreateVariant(
        string code,
        CreateVariantRequest request,
        CancellationToken cancellationToken)
    {
        var variant = await sender.Send(
            new CreateFoodVariantCommand(
                code,
                request.Code,
                request.Name,
                request.CurrentPrice,
                request.IsDefault,
                request.DisplayOrder,
                request.IsActive,
                ActorId()),
            cancellationToken);
        return CreatedAtAction(
            nameof(GetByCode),
            new { code },
            new ApiResponse<FoodVariantDto>(
                true,
                variant,
                "Food variant created successfully."));
    }

    [HttpPut("{code}/variants/{variantCode}")]
    [Authorize(Policy = FoodsPermissions.Update)]
    public async Task<ActionResult<ApiResponse<FoodVariantDto>>> UpdateVariant(
        string code,
        string variantCode,
        UpdateVariantRequest request,
        CancellationToken cancellationToken)
    {
        var variant = await sender.Send(
            new UpdateFoodVariantCommand(
                code,
                variantCode,
                request.Name,
                request.IsDefault,
                request.DisplayOrder,
                request.IsActive,
                request.Version,
                ActorId()),
            cancellationToken);
        return Ok(new ApiResponse<FoodVariantDto>(
            true,
            variant,
            "Food variant updated successfully."));
    }

    [HttpPut("{code}/variants/{variantCode}/price")]
    [Authorize(Policy = FoodsPermissions.Update)]
    public async Task<ActionResult<ApiResponse<FoodVariantDto>>> ChangePrice(
        string code,
        string variantCode,
        ChangePriceRequest request,
        CancellationToken cancellationToken)
    {
        var variant = await sender.Send(
            new ChangeFoodPriceCommand(
                code,
                variantCode,
                request.Price,
                request.Version,
                ActorId()),
            cancellationToken);
        return Ok(new ApiResponse<FoodVariantDto>(
            true,
            variant,
            "Food price updated successfully."));
    }

    [HttpGet("{code}/variants/{variantCode}/prices")]
    [Authorize(Policy = FoodsPermissions.Read)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<FoodPriceHistoryDto>>>> GetPrices(
        string code,
        string variantCode,
        CancellationToken cancellationToken)
    {
        var history = await sender.Send(
            new GetFoodPriceHistoryQuery(code, variantCode),
            cancellationToken);
        return Ok(new ApiResponse<IReadOnlyList<FoodPriceHistoryDto>>(
            true,
            history,
            "Food price history retrieved successfully."));
    }

    [HttpPut("{code}/variants/{variantCode}/availability")]
    [Authorize(Policy = FoodsPermissions.Update)]
    public async Task<ActionResult<ApiResponse<FoodVariantDto>>> ChangeAvailability(
        string code,
        string variantCode,
        ChangeAvailabilityRequest request,
        CancellationToken cancellationToken)
    {
        var variant = await sender.Send(
            new ChangeFoodAvailabilityCommand(
                code,
                variantCode,
                request.IsAvailable,
                request.Reason,
                request.Version,
                ActorId()),
            cancellationToken);
        return Ok(new ApiResponse<FoodVariantDto>(
            true,
            variant,
            "Food availability updated successfully."));
    }

    private ulong? ActorId() =>
        ulong.TryParse(User.FindFirstValue("sub"), out var id) ? id : null;

    private static FoodStatusFilter ParseStatus(string value) =>
        value.ToLowerInvariant() switch
        {
            "active" => FoodStatusFilter.Active,
            "inactive" => FoodStatusFilter.Inactive,
            _ => FoodStatusFilter.All
        };

    private static FoodAvailabilityFilter ParseAvailability(string value) =>
        value.ToLowerInvariant() switch
        {
            "available" => FoodAvailabilityFilter.Available,
            "unavailable" => FoodAvailabilityFilter.Unavailable,
            _ => FoodAvailabilityFilter.All
        };

    public sealed record CreateRequest(
        ulong CategoryId,
        string Code,
        string Name,
        string? Description,
        string? ImageUrl,
        int DisplayOrder,
        bool IsActive = true);

    public sealed record UpdateRequest(
        ulong CategoryId,
        string Name,
        string? Description,
        string? ImageUrl,
        int DisplayOrder,
        bool IsActive,
        DateTime Version);

    public sealed record CreateVariantRequest(
        string Code,
        string Name,
        decimal CurrentPrice,
        bool IsDefault,
        int DisplayOrder,
        bool IsActive = true);

    public sealed record UpdateVariantRequest(
        string Name,
        bool IsDefault,
        int DisplayOrder,
        bool IsActive,
        DateTime Version);

    public sealed record ChangePriceRequest(decimal Price, DateTime Version);
    public sealed record ChangeAvailabilityRequest(
        bool IsAvailable,
        string? Reason,
        DateTime Version);
}
