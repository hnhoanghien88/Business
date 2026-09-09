using Business.Application.Abstractions.Persistence.Restaurant;
using Business.Application.Restaurant.Foods.Dtos;
using MediatR;

namespace Business.Application.Restaurant.Foods.Prices;

public sealed record GetFoodPriceHistoryQuery(
    string FoodCode,
    string VariantCode) : IRequest<IReadOnlyList<FoodPriceHistoryDto>>;

public sealed class GetFoodPriceHistoryQueryHandler(
    IFoodReadRepository repository)
    : IRequestHandler<GetFoodPriceHistoryQuery, IReadOnlyList<FoodPriceHistoryDto>>
{
    public Task<IReadOnlyList<FoodPriceHistoryDto>> Handle(
        GetFoodPriceHistoryQuery request,
        CancellationToken cancellationToken) => repository.GetPriceHistoryAsync(
            FoodRules.CleanCode(request.FoodCode),
            FoodRules.CleanCode(request.VariantCode),
            cancellationToken);
}
