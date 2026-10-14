using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Nodes;
using Products.API.Data.Models;

public class ElasticSearchProductIndexer(
    ElasticsearchClient client,
    ILogger<ElasticSearchProductIndexer> logger,
    IProductRepository repository) : IProductIndexer
{

    private const int INDEX_CREATION_PAGE_SIZE = 500;
    private const string INDEX_NAME = "products";

    public async Task AddDocumetentAsync(int productId)
    {
        var product = await repository.GetById(productId);

        if(product == null)
        {
            logger.LogError($"Failed to index Product {productId} on Elastic Search");
            return;
        }

        var dto = new ProductDocumentDto(
            product.Id,
            product.Name,
            product.Description,
            product.Categoryid,
            product.Category.Name,
            product.Price,
            product.Stock,
            product.Createdat
        );

        await client.IndexAsync(dto, x => x.Index(INDEX_NAME));
    }

    public async Task CreateIndexIdepotentAsync()
    {
        try
        {
            await client.Indices.DeleteAsync(INDEX_NAME);
            await client.Indices.CreateAsync(INDEX_NAME);

            PagedResult<Product> result = await repository.GetPaged(new PagedRequest(INDEX_CREATION_PAGE_SIZE, 0));
            long processed = 0;

            while (processed < result.totalCount)
            {

                List<ProductDocumentDto> dtos = result.Values.Select(p => new ProductDocumentDto(
                    p.Id,
                    p.Name,
                    p.Description,
                    p.Categoryid,
                    p.Category.Name,
                    p.Price,
                    p.Stock,
                    p.Createdat
                )).ToList();

                await client.BulkAsync(b => b
                    .Index(INDEX_NAME)
                    .IndexMany(dtos));

                processed += result.Values.Count();
                if (result.lastId == null) break; //check

                result = await repository.GetPaged(new PagedRequest(INDEX_CREATION_PAGE_SIZE, result.lastId ?? 0));
            }


        }
        catch (Exception e)
        {
            logger.LogError($"Error while creating product index; {e}", e);
            await client.Indices.DeleteAsync("products");
            throw;
        }
    }
}

