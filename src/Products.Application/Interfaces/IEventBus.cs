namespace Products.Application;

public interface IEventBus
{
    Task Publish<T>(
        T @event, 
        CancellationToken cancellationToken = default);
}