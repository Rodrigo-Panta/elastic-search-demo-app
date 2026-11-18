using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Prodcuts.Infra;
using Products.Application;
using Products.Domain;

namespace Products.Infra;

public static class InfraDependencyInjection
{
    public static IServiceCollection AddInfra(this IServiceCollection services)
    {
        var postgreConnectionString = GetEnvironmentVariableOrThrow("POSTGRESQL_CONNECTION_STRING");
        services.AddDbContext<ProductsDbContext>(options => options.UseNpgsql(postgreConnectionString));
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();

        var elasticSearchSettings = new ElasticsearchClientSettings(new Uri(GetEnvironmentVariableOrThrow("ELASTICSEARCH_ENDPOINT")))
    .Authentication(new ApiKey("ELASTICSEARCH_APIKEY"));

        services.AddSingleton(serviceProvider => new ElasticsearchClient(elasticSearchSettings));
        services.AddScoped<IProductIndexer, ElasticSearchProductIndexer>();

        services.AddSingleton<EventChannel<ProductCreatedEvent>>();
        services.AddSingleton<IEventBus, InMemoryEventBus>();
        services.AddHostedService<ProductCreatedEventConsumer>();
        return services;
    }


    public static IHealthChecksBuilder AddInfraHealthChecks(this IHealthChecksBuilder healthChecksBuilder)
    {
        var postgreConnectionString = GetEnvironmentVariableOrThrow("POSTGRESQL_CONNECTION_STRING");
        return healthChecksBuilder
            .AddNpgSql(
                postgreConnectionString,
                name: "postgres",
                tags: ["ready"])
            .AddCheck<ElasticsearchHealthCheck>(
                "elasticsearch",
                tags: ["ready"]);
    }

    private static string GetEnvironmentVariableOrThrow(string name)
    {
        var value = Environment.GetEnvironmentVariable(name);

        if (string.IsNullOrWhiteSpace(value)) throw new InvalidDataException($"The environment variable {name} must be defined");

        return value;

    }
}