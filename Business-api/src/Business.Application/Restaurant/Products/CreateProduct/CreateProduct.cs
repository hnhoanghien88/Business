using Business.Application.Abstractions.Persistence.Restaurant;
using Business.Application.Common.Exceptions;
using Business.Application.Restaurant.Products.Dtos;
using Business.Domain.Entities.Restaurant;
using FluentValidation;
using MediatR;

namespace Business.Application.Restaurant.Products.CreateProduct;

public sealed record CreateProductCommand(string Code, string Name) : IRequest<ProductDto>;

public sealed class CreateProductValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(255);
    }
}

public sealed class CreateProductCommandHandler(IProductRepository repository) : IRequestHandler<CreateProductCommand, ProductDto>
{
    public async Task<ProductDto> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var code = ProductRules.CleanCode(request.Code);
        if (await repository.CodeExistsAsync(code, cancellationToken))
            throw new ConflictException("Product code is already in use.", "code");
        var product = new Product { Code = code, Name = ProductRules.Clean(request.Name) };
        await repository.AddAsync(product, cancellationToken);
        return ProductRules.ToDto(product);
    }
}
