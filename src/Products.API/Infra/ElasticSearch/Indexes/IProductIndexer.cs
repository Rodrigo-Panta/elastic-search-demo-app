using Products.API.Data.Models;

public interface IProductIndexer 
{
    Task CreateIndexIdepotentAsync();
    Task AddDocumetentAsync(int productId);

}

