using Microsoft.EntityFrameworkCore;
using Products.API.Data.Models;
using Products.Domain;



public class ProductRepository(ProductsDbContext dbContext) : IProductRepository
{
    public async Task<Product> CreateProduct(Product product)
    {
        var model = new ProductModel
        {
            Id = product.Id,
            Categoryid = product.Category.Id,
            Name = product.Name,
            Stock = product.Stock.Value,
            Price = product.Price.Value,
            Description = product.Description,
            Createdat = product.CreatedAt
        };

        await dbContext.Products.AddAsync(model);
        await dbContext.SaveChangesAsync();

        return product;
    }

    public async Task<Product?> GetById(int productId) => (await dbContext
                                                            .Products
                                                            .Where(p => p.Id == productId)
                                                            .Include(p => p.Category).FirstOrDefaultAsync())?
                                                            .ToEntity();

    public async Task<int> GetNextId() => await dbContext.Products.CountAsync() > 0 
                                            ? await dbContext.Products.MaxAsync(p => p.Id) + 1 
                                            : 1;

    public async Task<PagedResult<Product>> GetPaged(PagedRequest request)
    {
        var result = await dbContext
                            .Products
                            .OrderBy(p => p.Id)
                            .Where(p => p.Id > request.lastId)
                            .Take(request.pageSize)
                            .Include(p => p.Category)
                            .ToListAsync();
        var entities = result.Select(model => model.ToEntity());
        var count = await dbContext.Products.CountAsync();

        return new PagedResult<Product>(entities, result.LastOrDefault()?.Id, count);
    }


}