public class ProductCreatedEventConsumer(
    EventChannel<ProductCreatedEvent> channel,
    IProductIndexer indexer,
    ILogger<ProductCreatedEventConsumer> logger)
    : BackgroundService
{

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await foreach(var @event in
            channel.CustomChannel.Reader.ReadAllAsync(cancellationToken)
        )
        {
            try
            {
                await indexer.AddDocumetentAsync(@event.ProductId);
            }
            catch (Exception e)
            {
                logger.LogError(e, $"Error processing ProductCreatedEvent for {@event.ProductId}");
            }
        }
    }
    
}