using Microsoft.EntityFrameworkCore;
using Products.API.Data.Models;



public class ProductRepository(ProductsDbContext dbContext) : IProductRepository
{

    public Task<Product?> GetById(int productId) =>
        dbContext.Products.Where(p => p.Id == productId).Include(p => p.Category).FirstOrDefaultAsync();

    public async Task<PagedResult<Product>> GetPaged(PagedRequest request)
    {
        var result = await dbContext.Products.OrderBy(p=>p.Id). Where(p=> p.Id > request.lastId).Take(request.pageSize).ToListAsync();
        var count = await dbContext.Products.CountAsync();
        return new PagedResult<Product>(result, result.LastOrDefault()?.Id, count);
    }
}