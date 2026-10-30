using Microsoft.EntityFrameworkCore;
using Products.API.Data.Models;
using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddLogging(configure =>
            {
                configure.AddConsole();
                configure.SetMinimumLevel(LogLevel.Information);
            });

var postgreConnectionString = GetEnvironmentVariableOrThrow("POSTGRESQL_CONNECTION_STRING");
builder.Services.AddDbContext<ProductsDbContext>(options => options.UseNpgsql(postgreConnectionString));
builder.Services.AddScoped<IProductRepository, ProductRepository>();



var elasticSearchSettings = new ElasticsearchClientSettings(new Uri(GetEnvironmentVariableOrThrow("ELASTICSEARCH_ENDPOINT")))
    .Authentication(new ApiKey("ELASTICSEARCH_APIKEY"));

builder.Services.AddSingleton(serviceProvider => new ElasticsearchClient(elasticSearchSettings));
builder.Services.AddScoped<IProductIndexer, ElasticSearchProductIndexer>();

builder.Services.AddSingleton<EventChannel<ProductCreatedEvent>>();
builder.Services.AddSingleton<IEventBus, InMemoryEventBus>();
builder.Services.AddHostedService<ProductCreatedEventConsumer>();

builder.Services
    .AddHealthChecks()
    .AddCheck(
        "self",
        () => HealthCheckResult.Healthy(),
        tags: ["live"])
    .AddNpgSql(
        postgreConnectionString,
        name: "postgres",
        tags: ["ready"])
    .AddCheck<ElasticsearchHealthCheck>(
         "elasticsearch",
        tags: ["ready"]);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("live")
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});

app.MapGet("/api", () => $"Products Api - Version: {Environment.GetEnvironmentVariable("APP_VERSION")}");

var productsGroup = app.MapGroup("/api/products");

productsGroup.MapPost("/indices", async (IProductIndexer indexer) =>
{
    await indexer.CreateIndexIdepotentAsync();

});

productsGroup.MapPost("/", async (IProductRepository repository,IEventBus eventBus, [FromBody] CreateProductRequestDto request) =>
{
    var product = await repository.CreateProduct(request);
    await eventBus.Publish(new ProductCreatedEvent(product.Id));
});

productsGroup.MapGet("/search",
                     async ([AsParameters] SearchProductQueryDto query, IProductIndexer indexer) =>
{
    var response = await indexer.SearchAsync(query);

    return Results.Ok(response);
});

productsGroup.MapGet("/{id}", async (int id, IProductRepository repository) =>
{
    var result = await repository.GetById(id);
    if (result == null) return Results.NotFound();

    var response = new GetProductResponseDto(result.Id, result.Name, result.Description, result.Price, result.Stock, result.Createdat, new GetProductCategoryResponseDto(result.Category.Id, result.Category.Name));
    return Results.Ok(response);
});

app.Run();

static string GetEnvironmentVariableOrThrow(string name)
{
    var value = Environment.GetEnvironmentVariable(name);

    if (string.IsNullOrWhiteSpace(value)) throw new InvalidDataException($"The environment variable {name} must be defined");

    return value;

}