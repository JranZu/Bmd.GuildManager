using Azure.Messaging.ServiceBus;
using Bmd.GuildManager.Core.Abstractions;
using Bmd.GuildManager.Core.Events;
using System.Text.Json;

namespace Bmd.GuildManager.Functions.Publishers;

public class ServiceBusEventPublisher(ServiceBusClient serviceBusClient, string topicName) : IEventPublisher
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task PublishAsync<T>(EventEnvelope<T> envelope)
    {
        var messageBody = JsonSerializer.Serialize(envelope, JsonOptions);
        var message = new ServiceBusMessage(messageBody)
        {
            ContentType = "application/json",
            MessageId = envelope.EventId.ToString()
        };

        await using var sender = serviceBusClient.CreateSender(topicName);
        await sender.SendMessageAsync(message);
    }
}
