using Products.API.Data.Models;

public interface IProductRepository
{
    Task<Product?> GetById(int productId);
    Task<PagedResult<Product>> GetPaged(PagedRequest request);
}


