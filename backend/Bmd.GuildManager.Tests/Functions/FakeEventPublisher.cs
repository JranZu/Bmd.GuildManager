using Bmd.GuildManager.Core.Abstractions;
using Bmd.GuildManager.Core.Events;

namespace Bmd.GuildManager.Tests.Functions;

public class FakeEventPublisher : IEventPublisher
{
	public List<EventEnvelope<object>> Published { get; } = [];

	public bool FailOnPublish { get; set; }

	public Task PublishAsync<T>(EventEnvelope<T> envelope, CancellationToken cancellationToken = default)
	{
		if (FailOnPublish)
		{
			throw new InvalidOperationException("Simulated publish failure.");
		}

		Published.Add(new EventEnvelope<object>(
			envelope.EventId,
			envelope.EventType,
			envelope.Timestamp,
			envelope.CorrelationId,
			envelope.Source,
			envelope.Version,
			envelope.Data!));

		return Task.CompletedTask;
	}
}
