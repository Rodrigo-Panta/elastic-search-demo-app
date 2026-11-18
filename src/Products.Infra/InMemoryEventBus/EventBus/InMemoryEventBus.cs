using System.Collections.Concurrent;
using Products.Application;

namespace Products.Infra;
public class InMemoryEventBus : IEventBus
{

    private readonly ConcurrentDictionary<Type, IEventChannel> _channels = new();

    public async Task Publish<TEvent>(
        TEvent @event,
        CancellationToken cancellationToken = default)
    {

        var channel = GetChannel<TEvent>();
        
        await channel.CustomChannel.Writer.WriteAsync(
                    @event,
                    cancellationToken);
    }

    private EventChannel<TEvent> GetChannel<TEvent>()
    {
        var channel = _channels.GetOrAdd(typeof(TEvent),
         _ => new EventChannel<TEvent>());

        return (EventChannel<TEvent>)channel;
    }
}