using Business.Application.Abstractions.Persistence.Restaurant;
using Business.Application.Restaurant.Categories.Dtos;
using MediatR;

namespace Business.Application.Restaurant.Categories.GetCategories;

public sealed record GetCategoriesQuery(
    string? Search = null,
    CategoryStatusFilter Status = CategoryStatusFilter.All,
    int Page = 1,
    int PageSize = 20) : IRequest<PagedCategoriesDto>;

public sealed class GetCategoriesQueryHandler(ICategoryReadRepository repository)
    : IRequestHandler<GetCategoriesQuery, PagedCategoriesDto>
{
    public Task<PagedCategoriesDto> Handle(
        GetCategoriesQuery request,
        CancellationToken cancellationToken)
    {
        if (request.Page < 1) throw new ArgumentOutOfRangeException(nameof(request.Page));
        if (request.PageSize is < 1 or > 100)
            throw new ArgumentOutOfRangeException(nameof(request.PageSize));
        return repository.GetAsync(
            request.Search?.Trim(),
            request.Status,
            request.Page,
            request.PageSize,
            cancellationToken);
    }
}
