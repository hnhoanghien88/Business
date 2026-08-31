using Business.Application.Abstractions.Persistence.Restaurant;
using Business.Application.Common.Exceptions;
using MediatR;

namespace Business.Application.Restaurant.Products.DeleteProduct;

public sealed record DeleteProductCommand(string Code) : IRequest;

public sealed class DeleteProductCommandHandler(IProductRepository repository) : IRequestHandler<DeleteProductCommand>
{
    public async Task Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var code = ProductRules.CleanCode(request.Code);
        var food = await repository.GetByCodeAsync(code, cancellationToken)
            ?? throw new NotFoundException($"Food '{code}' was not found.");
        await repository.DeactivateAsync(food, cancellationToken);
    }
}
