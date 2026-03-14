using Bmd.GuildManager.Core.Events;

namespace Bmd.GuildManager.Core.Abstractions;

public interface IEventPublisher
{
    Task PublishAsync<T>(EventEnvelope<T> envelope, CancellationToken cancellationToken = default);
}
