using Azure.Messaging.ServiceBus;
using Bmd.GuildManager.Core.Abstractions;

namespace Bmd.GuildManager.Functions.Publishers;

public class ServiceBusMessageScheduler(ServiceBusClient serviceBusClient) : IMessageScheduler
{
    public async Task ScheduleMessageAsync(
        string queueOrTopicName,
        string messageBody,
        DateTimeOffset scheduledEnqueueTime)
    {
        var message = new ServiceBusMessage(messageBody)
        {
            ScheduledEnqueueTime = scheduledEnqueueTime,
            ContentType = "application/json"
        };

        var sender = serviceBusClient.CreateSender(queueOrTopicName);
        await sender.SendMessageAsync(message);
    }
}
