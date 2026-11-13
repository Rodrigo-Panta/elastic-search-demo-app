namespace Products.Application;

public interface IProductIndexer
{
    Task CreateAsync();
    Task DeleteAsync();
    Task BulkAddDocumentsAsync(IEnumerable<ProductDocumentDto> dtos);
    Task AddDocumentAsync(int productId);
    Task<IEnumerable<ProductDocumentDto>> SearchAsync(SearchProductQueryDto dto);
}

