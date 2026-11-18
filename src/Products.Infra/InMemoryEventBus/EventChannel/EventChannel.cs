using System.Threading.Channels;
namespace Products.Infra;

public class EventChannel<TEvent> : IEventChannel
{
    public Channel<TEvent> CustomChannel { get; } =
        Channel.CreateUnbounded<TEvent>();
}