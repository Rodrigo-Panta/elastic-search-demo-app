using MediatR;

namespace Products.Application;

public sealed record CreateProductCommand(
    string Name,
    string Description,
    int CategoryId,
    decimal Price,
    int Stock
): IRequest<CreateProductResult>;