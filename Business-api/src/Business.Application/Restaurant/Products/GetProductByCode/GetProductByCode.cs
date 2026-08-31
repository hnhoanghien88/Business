using Business.Application.Abstractions.Persistence.Restaurant;
using Business.Application.Common.Exceptions;
using Business.Application.Restaurant.Products.Dtos;
using MediatR;

namespace Business.Application.Restaurant.Products.GetProductByCode;

public sealed record GetProductByCodeQuery(string Code) : IRequest<ProductDto>;

public sealed class GetProductByCodeQueryHandler(IProductReadRepository repository) : IRequestHandler<GetProductByCodeQuery, ProductDto>
{
    public async Task<ProductDto> Handle(GetProductByCodeQuery request, CancellationToken cancellationToken)
    {
        var code = ProductRules.CleanCode(request.Code);
        return await repository.GetByCodeAsync(code, cancellationToken)
            ?? throw new NotFoundException($"Food '{code}' was not found.");
    }
}
