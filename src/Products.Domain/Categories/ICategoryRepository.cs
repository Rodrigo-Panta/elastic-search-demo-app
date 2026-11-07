namespace Products.Domain;

public interface ICategoryRepository
{
    Task<Category?> GetById(int productId);
    
}