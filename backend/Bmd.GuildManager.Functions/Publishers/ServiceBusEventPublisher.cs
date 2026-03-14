using Azure.Messaging.ServiceBus;
using Bmd.GuildManager.Core.Abstractions;
using Bmd.GuildManager.Core.Events;
using Bmd.GuildManager.Functions.Serialization;
using System.Text.Json;

namespace Bmd.GuildManager.Functions.Publishers;

public class ServiceBusEventPublisher(ServiceBusClient serviceBusClient, string topicName)
	: IEventPublisher, IAsyncDisposable
{
	private readonly ServiceBusSender _sender = serviceBusClient.CreateSender(topicName);

	public async Task PublishAsync<T>(EventEnvelope<T> envelope, CancellationToken cancellationToken = default)
	{
		var messageBody = JsonSerializer.Serialize(envelope, FunctionJsonOptions.Default);
		var message = new ServiceBusMessage(messageBody)
		{
			ContentType = "application/json",
			MessageId = envelope.EventId.ToString(),
			Subject = envelope.EventType
		};

		await _sender.SendMessageAsync(message, cancellationToken);
	}

	public ValueTask DisposeAsync() => _sender.DisposeAsync();
}
