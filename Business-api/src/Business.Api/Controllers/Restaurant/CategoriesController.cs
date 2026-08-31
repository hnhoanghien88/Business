using Business.Api.Models;
using Business.Application.Common.Authorization;
using Business.Application.Restaurant.Categories.CreateCategory;
using Business.Application.Restaurant.Categories.Dtos;
using Business.Application.Restaurant.Categories.GetCategories;
using Business.Application.Restaurant.Categories.GetCategoryByCode;
using Business.Application.Restaurant.Categories.UpdateCategory;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Business.Api.Controllers.Restaurant;

[ApiController]
[Authorize]
[Route("api/restaurant/categories")]
public sealed class CategoriesController(ISender sender) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = CategoriesPermissions.Read)]
    public async Task<ActionResult<ApiResponse<PagedCategoriesDto>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] string status = "all",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(
            new GetCategoriesQuery(search, ParseStatus(status), page, pageSize),
            cancellationToken);
        return Ok(new ApiResponse<PagedCategoriesDto>(true, result, "Categories retrieved."));
    }

    [HttpGet("{code}")]
    [Authorize(Policy = CategoriesPermissions.Read)]
    public async Task<ActionResult<ApiResponse<CategoryDto>>> GetByCode(
        string code,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetCategoryByCodeQuery(code), cancellationToken);
        return Ok(new ApiResponse<CategoryDto>(true, result, "Category retrieved."));
    }

    [HttpPost]
    [Authorize(Policy = CategoriesPermissions.Create)]
    public async Task<ActionResult<ApiResponse<CategoryDto>>> Create(
        CreateCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new CreateCategoryCommand(
                request.ParentId,
                request.Code,
                request.Name,
                request.Description,
                request.DisplayOrder,
                request.IsActive),
            cancellationToken);
        return CreatedAtAction(
            nameof(GetByCode),
            new { code = result.Code },
            new ApiResponse<CategoryDto>(true, result, "Category created."));
    }

    [HttpPut("{code}")]
    [Authorize(Policy = CategoriesPermissions.Update)]
    public async Task<ActionResult<ApiResponse<CategoryDto>>> Update(
        string code,
        UpdateCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new UpdateCategoryCommand(
                code,
                request.ParentId,
                request.Name,
                request.Description,
                request.DisplayOrder,
                request.IsActive,
                request.Version),
            cancellationToken);
        return Ok(new ApiResponse<CategoryDto>(true, result, "Category updated."));
    }

    public sealed record CreateCategoryRequest(
        ulong? ParentId,
        string Code,
        string Name,
        string? Description,
        int DisplayOrder,
        bool IsActive = true);

    public sealed record UpdateCategoryRequest(
        ulong? ParentId,
        string Name,
        string? Description,
        int DisplayOrder,
        bool IsActive,
        DateTime Version);

    private static CategoryStatusFilter ParseStatus(string status) => status.ToLowerInvariant() switch
    {
        "active" => CategoryStatusFilter.Active,
        "inactive" => CategoryStatusFilter.Inactive,
        "effective-active" => CategoryStatusFilter.EffectiveActive,
        "effective-inactive" => CategoryStatusFilter.EffectiveInactive,
        _ => CategoryStatusFilter.All
    };
}
