using System.Threading.Channels;

public class EventChannel<TEvent> : IEventChannel
{
    public Channel<TEvent> CustomChannel { get; } =
        Channel.CreateUnbounded<TEvent>();
}