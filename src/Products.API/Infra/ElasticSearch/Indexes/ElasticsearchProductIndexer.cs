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
        await client.IndexAsync(product, x => x.Index(INDEX_NAME));
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
                await client.BulkAsync(b => b
                    .Index(INDEX_NAME)
                    .IndexMany(result.Values));

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

