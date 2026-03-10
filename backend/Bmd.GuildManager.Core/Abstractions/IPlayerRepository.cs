using Bmd.GuildManager.Core.Models;

namespace Bmd.GuildManager.Core.Abstractions;

public interface IPlayerRepository
{
	Task CreateAsync(Player player);

	Task UpdateAsync(Player player);

	Task<Player?> FindByIdempotencyKeyAsync(string idempotencyKey);

	Task<Player?> FindByPlayerIdAsync(Guid playerId);
}
