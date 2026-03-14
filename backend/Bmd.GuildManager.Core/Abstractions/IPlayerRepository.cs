using Bmd.GuildManager.Core.Models;

namespace Bmd.GuildManager.Core.Abstractions;

public interface IPlayerRepository
{
	Task CreateAsync(Player player, CancellationToken cancellationToken = default);

	Task UpdateAsync(Player player, string etag, CancellationToken cancellationToken = default);

	Task<Player?> FindByIdempotencyKeyAsync(string idempotencyKey, CancellationToken cancellationToken = default);

	Task<CosmosDocument<Player>?> FindByPlayerIdAsync(Guid playerId, CancellationToken cancellationToken = default);
}
