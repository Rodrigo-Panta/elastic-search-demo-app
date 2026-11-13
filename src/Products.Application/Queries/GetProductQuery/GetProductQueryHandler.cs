using System;
using MediatR;

namespace Products.Application;

public class GetProductQueryHandler(IProductRepository repository) : IRequestHandler<GetProductQuery, GetProductQueryResult?>
{
    public async Task<GetProductQueryResult?> Handle(GetProductQuery request, CancellationToken cancellationToken)
    {
        var entity = await repository.GetById(request.id);
        if (entity == null) return null;

        var response = new GetProductQueryResult(
                            entity.Id,
                            entity.Name,
                            entity.Description,
                            entity.Price.Value,
                            entity.Stock.Value,
                            entity.CreatedAt,
                            new GetProductCategoryResult(
                                entity.Category.Id,
                                entity.Category.Name));

        return response;
    }
}
