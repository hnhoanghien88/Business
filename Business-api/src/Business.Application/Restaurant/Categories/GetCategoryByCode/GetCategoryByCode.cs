using Business.Application.Abstractions.Persistence.Restaurant;
using Business.Application.Common.Exceptions;
using Business.Application.Restaurant.Categories.Dtos;
using MediatR;

namespace Business.Application.Restaurant.Categories.GetCategoryByCode;

public sealed record GetCategoryByCodeQuery(string Code) : IRequest<CategoryDto>;

public sealed class GetCategoryByCodeQueryHandler(ICategoryReadRepository repository)
    : IRequestHandler<GetCategoryByCodeQuery, CategoryDto>
{
    public async Task<CategoryDto> Handle(
        GetCategoryByCodeQuery request,
        CancellationToken cancellationToken) =>
        await repository.GetByCodeAsync(request.Code.Trim(), cancellationToken)
        ?? throw new NotFoundException($"Category '{request.Code}' was not found.");
}
