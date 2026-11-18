using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Products.Application;

namespace Products.Infra;

public class ProductCreatedEventConsumer(
    EventChannel<ProductCreatedEvent> channel,
    IServiceScopeFactory scopeFactory,
    ILogger<ProductCreatedEventConsumer> logger)
    : BackgroundService
{

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await foreach (var @event in
            channel.CustomChannel.Reader.ReadAllAsync(cancellationToken)
        )
        {
            try
            {
                await using var scope =
               scopeFactory.CreateAsyncScope();

                var indexer = scope.ServiceProvider
                    .GetRequiredService<IProductIndexer>();

                await indexer.AddDocumentAsync(@event.ProductId);
            }
            catch (Exception e)
            {
                logger.LogError(e, $"Error processing ProductCreatedEvent for {@event.ProductId}");
            }
        }
    }

}