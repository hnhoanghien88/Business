using Business.Application.Abstractions.Persistence.Restaurant;
using Business.Application.Common.Exceptions;
using Business.Application.Restaurant.Foods.Dtos;
using MediatR;

namespace Business.Application.Restaurant.Foods.GetFoodByCode;

public sealed record GetFoodByCodeQuery(string Code) : IRequest<FoodDto>;

public sealed class GetFoodByCodeQueryHandler(IFoodReadRepository repository) : IRequestHandler<GetFoodByCodeQuery, FoodDto>
{
    public async Task<FoodDto> Handle(GetFoodByCodeQuery request, CancellationToken cancellationToken)
    {
        var code = FoodRules.CleanCode(request.Code);
        return await repository.GetByCodeAsync(code, cancellationToken)
            ?? throw new NotFoundException($"Food '{code}' was not found.");
    }
}
