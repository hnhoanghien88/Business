using Business.Application.Abstractions.Persistence.Restaurant;
using Business.Application.Restaurant.Products.Dtos;
using MediatR;

namespace Business.Application.Restaurant.Products.GetProducts;

public sealed record GetProductsQuery(string? Search = null, int Page = 1, int PageSize = 20) : IRequest<PagedProductsDto>;

public sealed class GetProductsQueryHandler(IProductReadRepository repository) : IRequestHandler<GetProductsQuery, PagedProductsDto>
{
    public Task<PagedProductsDto> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        if (request.Page < 1) throw new ArgumentOutOfRangeException(nameof(request.Page));
        if (request.PageSize is < 1 or > 100) throw new ArgumentOutOfRangeException(nameof(request.PageSize));
        return repository.GetAsync(request.Search?.Trim(), request.Page, request.PageSize, cancellationToken);
    }
}
