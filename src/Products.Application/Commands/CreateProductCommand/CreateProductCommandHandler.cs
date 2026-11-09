using MediatR;
using Products.Domain;

namespace Products.Application;

public class CreateProductCommandHandler(
    IProductRepository repository,
    ICategoryRepository categoryRepository,
    IEventBus eventBus
) : IRequestHandler<CreateProductCommand, CreateProductResult>
{
    public async Task<CreateProductResult> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var category = await categoryRepository.GetById(request.CategoryId);

        if (category == null) throw new Exception("CategoryNotFound");

        var product = new Product(
                        await repository.GetNextId(),
                        request.Name,
                        request.Description,
                        category,
                        new Price(request.Price),
                        new Stock(request.Stock));

        await eventBus.Publish(new ProductCreatedEvent(product.Id, product.Name));

        return new CreateProductResult(product.Id);
    }
}