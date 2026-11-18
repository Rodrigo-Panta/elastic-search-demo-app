using Microsoft.EntityFrameworkCore;
using Products.Domain;

namespace Products.Infra;

public class CategoryRepository(ProductsDbContext dbContext) : ICategoryRepository
{
    public async Task<Category?> GetById(int categoryId) =>
        (await dbContext
               .Categories
               .Where(c => c.Id == categoryId)
               .FirstOrDefaultAsync())?.ToEntity();
}