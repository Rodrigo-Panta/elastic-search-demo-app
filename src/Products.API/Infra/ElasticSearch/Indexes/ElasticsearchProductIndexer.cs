using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Products.Application;

public class ElasticSearchProductIndexer(
    ElasticsearchClient client,
    ILogger<ElasticSearchProductIndexer> logger,
    IProductRepository repository) : IProductIndexer
{

    private const int INDEX_CREATION_PAGE_SIZE = 500;
    private const string INDEX_NAME = "products";
    private const int QUERY_DEFAULT_MAX_RESULTS = 15;

    public async Task AddDocumentAsync(int productId)
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

    public async Task BulkAddDocumentsAsync(IEnumerable<ProductDocumentDto> dtos)
    {
        await client.BulkAsync(b => b
            .Index(INDEX_NAME)
            .IndexMany(dtos));
    }

    public Task CreateAsync()
    {
        return client.Indices.CreateAsync(INDEX_NAME);
    }

    public Task DeleteAsync()
    {
        return client.Indices.DeleteAsync(INDEX_NAME);
    }


    public async Task<IEnumerable<ProductDocumentDto>> SearchAsync(SearchProductQueryDto dto)
    {
        var response = await client.SearchAsync<ProductDocumentDto>(s => s
           .Indices(INDEX_NAME)
           .From(0)
           .Size(dto.maxResults ?? QUERY_DEFAULT_MAX_RESULTS)
           .Query(q => q
                .Bool(b =>
                {
                    AddTermSearch(dto.queryTerm, b);
                    AddCategoryFilter(dto.category, b);
                    AddPriceFilter(dto.minPrice, dto.maxPrice, b);
                }))
       );

        return response.Documents;
    }

    private void AddTermSearch(string? term, BoolQueryDescriptor<ProductDocumentDto> query)
    {
        if (string.IsNullOrWhiteSpace(term))
            return;
        query.Should(
            s => s.MultiMatch(mm => mm
                .Query(term)
                .Fields([x => $"{x.name}^3", x => x.description])
                .Operator(Operator.And).Boost(2)),
            s => s
                .MultiMatch(mm => mm
                .Query(term)
                .Fields(x => $"{x.name}^3", x => x.description)))
        .MinimumShouldMatch(1);
    }

    private void AddCategoryFilter(string? category, BoolQueryDescriptor<ProductDocumentDto> query)
    {
        if (string.IsNullOrWhiteSpace(category))
            return;
        query.Filter(f => f
                .Match(m => m
                .Field(p => p.category)
                .Query(category)));

    }

    private void AddPriceFilter(decimal? minPrice, decimal? maxPrice, BoolQueryDescriptor<ProductDocumentDto> query)
    {
        if (minPrice == null && maxPrice == null)
            return;
        query.Filter(f => f
                .Range(r => r
                .Number(nr => nr
                .Field(p => p.price)
                .Gte((double?)minPrice)
                .Lte((double?)maxPrice))));
    }
}

