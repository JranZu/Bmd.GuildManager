using Azure.Messaging.ServiceBus;
using Bmd.GuildManager.Core.Abstractions;
using System.Collections.Concurrent;

namespace Bmd.GuildManager.Functions.Publishers;

public class ServiceBusMessageScheduler(ServiceBusClient serviceBusClient) : IMessageScheduler, IAsyncDisposable
{
    private readonly ConcurrentDictionary<string, ServiceBusSender> _senders = new();

    public async Task ScheduleMessageAsync(
        string queueOrTopicName,
        string messageBody,
        DateTimeOffset scheduledEnqueueTime,
        CancellationToken cancellationToken = default)
    {
        var message = new ServiceBusMessage(messageBody)
        {
            ScheduledEnqueueTime = scheduledEnqueueTime,
            ContentType = "application/json"
        };

        var sender = _senders.GetOrAdd(queueOrTopicName, serviceBusClient.CreateSender);
        await sender.SendMessageAsync(message, cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var sender in _senders.Values)
        {
            await sender.DisposeAsync();
        }

        _senders.Clear();
        GC.SuppressFinalize(this);
    }
}
