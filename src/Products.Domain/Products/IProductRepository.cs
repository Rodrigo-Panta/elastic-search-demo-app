using Products.Domain;

public interface IProductRepository
{
    Task<int> GetNextId();
    Task<Product> CreateProduct(Product product);
    Task<Product?> GetById(int productId);
    Task<PagedResult<Product>> GetPaged(PagedRequest request);
}


