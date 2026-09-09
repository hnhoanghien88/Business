using Business.Application.Abstractions.Persistence.Restaurant;
using Business.Application.Restaurant.Foods.Dtos;
using MediatR;

namespace Business.Application.Restaurant.Foods.GetFoods;

public sealed record GetFoodsQuery(
    string? Search = null,
    ulong? CategoryId = null,
    FoodStatusFilter Status = FoodStatusFilter.All,
    FoodAvailabilityFilter Availability = FoodAvailabilityFilter.All,
    int Page = 1,
    int PageSize = 20) : IRequest<PagedFoodsDto>;

public sealed class GetFoodsQueryHandler(IFoodReadRepository repository) : IRequestHandler<GetFoodsQuery, PagedFoodsDto>
{
    public Task<PagedFoodsDto> Handle(GetFoodsQuery request, CancellationToken cancellationToken)
    {
        if (request.Page < 1) throw new ArgumentOutOfRangeException(nameof(request.Page));
        if (request.PageSize is < 1 or > 100) throw new ArgumentOutOfRangeException(nameof(request.PageSize));
        return repository.GetAsync(
            request.Search?.Trim(),
            request.CategoryId,
            request.Status,
            request.Availability,
            request.Page,
            request.PageSize,
            cancellationToken);
    }
}
