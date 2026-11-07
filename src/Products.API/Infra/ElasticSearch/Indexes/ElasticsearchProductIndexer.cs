using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Nodes;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Products.API.Data.Models;
using Products.Domain;

public class ElasticSearchProductIndexer(
    ElasticsearchClient client,
    ILogger<ElasticSearchProductIndexer> logger,
    IProductRepository repository) : IProductIndexer
{

    private const int INDEX_CREATION_PAGE_SIZE = 500;
    private const string INDEX_NAME = "products";
    private const int QUERY_DEFAULT_MAX_RESULTS = 15;

    public async Task AddDocumetentAsync(int productId)
    {
        var product = await repository.GetById(productId);

        if (product == null)
        {
            logger.LogError($"Failed to index Product {productId} on Elastic Search");
            return;
        }

        var dto = new ProductDocumentDto(
            product.Id,
            product.Name,
            product.Description,
            product.Category.Id,
            product.Category.Name,
            product.Price.Value,
            product.Stock.Value,
            product.CreatedAt
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
                    p.Category.Id,
                    p.Category.Name,
                    p.Price.Value,
                    p.Stock.Value,
                    p.CreatedAt
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
            await client.Indices.DeleteAsync(INDEX_NAME);
            throw;
        }
    }

    public async Task<IEnumerable<ProductDocumentDto>> SearchAsync(SearchProductQueryDto dto)
    {
        var response = await client.SearchAsync<ProductDocumentDto>(s => s
           .Indices(INDEX_NAME)
           .From(0)
           .Size(dto.maxResults ?? QUERY_DEFAULT_MAX_RESULTS)
           .Query(q => q
                .Bool(b => b
                    .Should(
                        s => s.MultiMatch(mm => mm
                            .Query(dto.queryTerm ?? "")
                            .Fields([x => $"{x.name}^3", x => x.description])
                            .Operator(Operator.And).Boost(2)),
                        s => s
                            .MultiMatch(mm => mm
                            .Query(dto.queryTerm ?? "")
                            .Fields(x => $"{x.name}^3", x => x.description)))
                    .MinimumShouldMatch(1)
                    .Filter(f => f
                            .Match(m => m
                            .Field(p => p.category)
                            .Query(dto.category ?? "")))
                    .Filter(f => f
                            .Range(r => r
                            .Number(nr => nr
                            .Field(p => p.price)
                            .Gte((double?)dto.minPrice)
                            .Lte((double?)dto.maxPrice)))
                    )
                )
           )
       );

        return response.Documents;
    }
}

