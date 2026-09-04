using System.Threading.Channels;

namespace RouteFlow.Deliveries.Infrastructure.Messaging;

internal sealed class InProcessIntegrationEventQueue
{
    private readonly Channel<QueuedIntegrationEvent> _channel = Channel.CreateUnbounded<QueuedIntegrationEvent>(
        new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });

    public ChannelReader<QueuedIntegrationEvent> Reader => _channel.Reader;
    public ChannelWriter<QueuedIntegrationEvent> Writer => _channel.Writer;
}
