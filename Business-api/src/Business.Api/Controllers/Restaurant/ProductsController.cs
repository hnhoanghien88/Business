using Business.Api.Models;
using Business.Application.Common.Authorization;
using Business.Application.Restaurant.Products.CreateProduct;
using Business.Application.Restaurant.Products.DeleteProduct;
using Business.Application.Restaurant.Products.Dtos;
using Business.Application.Restaurant.Products.GetProductByCode;
using Business.Application.Restaurant.Products.GetProducts;
using Business.Application.Restaurant.Products.UpdateProduct;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Business.Api.Controllers.Restaurant;

[ApiController]
[Authorize]
[Route("api/restaurant/products")]
public sealed class ProductsController(ISender sender) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = ProductPermissions.Read)]
    public async Task<ActionResult<ApiResponse<PagedProductsDto>>> GetAll(
        [FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var products = await sender.Send(new GetProductsQuery(search, page, pageSize), cancellationToken);
        return Ok(new ApiResponse<PagedProductsDto>(true, products, "Products retrieved successfully."));
    }

    [HttpGet("{code}")]
    [Authorize(Policy = ProductPermissions.Read)]
    public async Task<ActionResult<ApiResponse<ProductDto>>> GetByCode(string code, CancellationToken cancellationToken)
    {
        var product = await sender.Send(new GetProductByCodeQuery(code), cancellationToken);
        return Ok(new ApiResponse<ProductDto>(true, product, "Product retrieved successfully."));
    }

    [HttpPost]
    [Authorize(Policy = ProductPermissions.Create)]
    public async Task<ActionResult<ApiResponse<ProductDto>>> Create(CreateRequest request, CancellationToken cancellationToken)
    {
        var product = await sender.Send(new CreateProductCommand(request.Code, request.Name), cancellationToken);
        return CreatedAtAction(nameof(GetByCode), new { code = product.Code },
            new ApiResponse<ProductDto>(true, product, "Product created successfully."));
    }

    [HttpPut("{code}")]
    [Authorize(Policy = ProductPermissions.Update)]
    public async Task<ActionResult<ApiResponse<ProductDto>>> Update(string code, UpdateRequest request, CancellationToken cancellationToken)
    {
        var product = await sender.Send(new UpdateProductCommand(code, request.Name), cancellationToken);
        return Ok(new ApiResponse<ProductDto>(true, product, "Product updated successfully."));
    }

    [HttpDelete("{code}")]
    [Authorize(Policy = ProductPermissions.Delete)]
    public async Task<ActionResult<ApiResponse<object>>> Delete(string code, CancellationToken cancellationToken)
    {
        await sender.Send(new DeleteProductCommand(code), cancellationToken);
        return Ok(new ApiResponse<object>(true, null, "Product deleted successfully."));
    }

    public sealed record CreateRequest(string Code, string Name);
    public sealed record UpdateRequest(string Name);
}
