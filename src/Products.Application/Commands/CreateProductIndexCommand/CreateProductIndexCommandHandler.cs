using System;
using MediatR;
using Microsoft.Extensions.Logging;
using Products.Domain;

namespace Products.Application;

public class CreateProductIndexCommandHandler(
    IProductIndexer indexer,
    IProductRepository repository,
    ILogger<CreateProductIndexCommandHandler> logger
) : IRequestHandler<CreateProductIndexCommand>
{
    private const int INDEX_CREATION_PAGE_SIZE = 500;

    public async Task Handle(CreateProductIndexCommand request, CancellationToken cancellationToken)
    {
        try
        {
            await indexer.DeleteAsync();
            await indexer.CreateAsync();

            PagedResult<Product> result = await repository.GetPaged(new PagedRequest(INDEX_CREATION_PAGE_SIZE, 0));

            long processed = 0;

            while (processed < result.totalCount)
            {

                List<ProductDocumentDto> dtos = result.Values.Select(p => new ProductDocumentDto(
                    p.Id,
                    p.Name,
                    p.Description,
                    p.Category.Id,
                    p.Category.Name,
                    p.Price.Value,
                    p.Stock.Value,
                    p.CreatedAt
                )).ToList();

                await indexer.BulkAddDocumentsAsync(dtos);

                processed += result.Values.Count();
                if (result.lastId == null) break; //check

                result = await repository.GetPaged(new PagedRequest(INDEX_CREATION_PAGE_SIZE, result.lastId ?? 0));
            }


        }
        catch (Exception e)
        {
            logger.LogError($"Error while creating product index; {e}", e);
            await indexer.DeleteAsync();
            throw;
        }
    }
}
