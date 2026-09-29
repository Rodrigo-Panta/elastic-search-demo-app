using Microsoft.EntityFrameworkCore;
using Products.API.Data.Models;



var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();


var postgreConnectionString = GetEnvironmentVariableOrThrow("POSTGRESQL_CONNECTION_STRING");

builder.Services.AddDbContext<ProductsDbContext>(options => options.UseNpgsql(postgreConnectionString));
builder.Services.AddScoped<IProductRepository, ProductRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();


app.MapPost("/products/index", () =>
{


});

app.MapGet("/products/{id}", async (int id, IProductRepository repository) =>
{
    var result = await repository.GetById(id);
    if(result == null) return Results.NotFound();

    var response = new GetProductResponseDto(result.Id, result.Name, result.Description, result.Price, result.Stock, result.Createdat, new GetProductCategoryResponseDto(result.Category.Id, result.Category.Name) );
    return Results.Ok(response);
});

app.Run();

static string GetEnvironmentVariableOrThrow(string name)
{
    var value = Environment.GetEnvironmentVariable(name);

    if(string.IsNullOrWhiteSpace(value)) throw new InvalidDataException($"The environment variable {name} must be defined");

    return value;
    
}