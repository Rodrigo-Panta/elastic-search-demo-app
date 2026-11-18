using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Products.Application;
using Products.Infra;
using MediatR;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

builder.Services.AddLogging(configure =>
            {
                configure.AddConsole();
                configure.SetMinimumLevel(LogLevel.Information);
            });


builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
});

builder.Services.AddInfra();

builder.Services
    .AddHealthChecks()
    .AddCheck(
        "self",
        () => HealthCheckResult.Healthy(),
        tags: ["live"])
    .AddInfraHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // 2. Map the OpenAPI JSON endpoint (defaults to /openapi/v1.json)
    app.MapOpenApi();

    // 3. Enable Swagger UI middleware pointing to the OpenAPI endpoint
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "My API v1");
    });
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

productsGroup.MapPost("/indices", async ([FromServices] IMediator mediator) =>
{
    await mediator.Send(new CreateProductIndexCommand());
});

productsGroup.MapPost("/", async ([FromServices] IMediator mediator, [FromBody]
                                    CreateProductRequestDto request) =>
{
    var result = await mediator.Send(
        new CreateProductCommand(
            request.Name,
            request.Description,
            request.Categoryid,
            request.Price,
            request.Stock));

    return Results.Ok(result);
});

productsGroup.MapGet("/search",
                     async ([AsParameters] SearchProductQueryDto query, [FromServices] IMediator mediator) =>
{
    var response = await mediator.Send(new SearchProductsQuery(query));
    return Results.Ok(response);
});

productsGroup.MapGet("/{id}", async (int id, [FromServices] IMediator mediator) =>
{
    var result = await mediator.Send(new GetProductQuery(id));
    if (result == null) return Results.NotFound();

    var response = new GetProductResponseDto(
                        result.Id,
                        result.Name,
                        result.Description,
                        result.Price,
                        result.Stock,
                        result.CreatedAt,
                        new GetProductCategoryResponseDto(
                            result.Category.Id,
                            result.Category.Name));

    return Results.Ok(response);
});

app.Run();
