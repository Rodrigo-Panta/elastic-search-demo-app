using Microsoft.EntityFrameworkCore;
using Products.API.Data.Models;
using Products.Domain;



public class CategoryRepository(ProductsDbContext dbContext) : ICategoryRepository
{
    public async Task<Category?> GetById(int categoryId) =>
        (await dbContext
               .Categories
               .Where(c => c.Id == categoryId)
               .FirstOrDefaultAsync())?.ToEntity();
}