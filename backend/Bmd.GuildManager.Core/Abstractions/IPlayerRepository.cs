using Bmd.GuildManager.Core.Models;

namespace Bmd.GuildManager.Core.Abstractions;

public interface IPlayerRepository
{
	Task CreateAsync(Player player);

	Task UpdateAsync(Player player, string etag);

	Task<Player?> FindByIdempotencyKeyAsync(string idempotencyKey);

	Task<CosmosDocument<Player>?> FindByPlayerIdAsync(Guid playerId);
}
