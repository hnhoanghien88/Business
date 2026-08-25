using Business.Application.Abstractions.Persistence.Restaurant;
using Business.Application.Common.Exceptions;
using Business.Application.Restaurant.Products.Dtos;
using FluentValidation;
using MediatR;

namespace Business.Application.Restaurant.Products.UpdateProduct;

public sealed record UpdateProductCommand(string Code, string Name) : IRequest<ProductDto>;

public sealed class UpdateProductValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(255);
    }
}

public sealed class UpdateProductCommandHandler(IProductRepository repository) : IRequestHandler<UpdateProductCommand, ProductDto>
{
    public async Task<ProductDto> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var code = ProductRules.CleanCode(request.Code);
        var product = await repository.GetByCodeAsync(code, cancellationToken)
            ?? throw new NotFoundException($"Product '{code}' was not found.");
        product.Name = ProductRules.Clean(request.Name);
        await repository.SaveAsync(cancellationToken);
        return ProductRules.ToDto(product);
    }
}
