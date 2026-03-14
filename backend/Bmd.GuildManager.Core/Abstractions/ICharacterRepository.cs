using Bmd.GuildManager.Core.Models;

namespace Bmd.GuildManager.Core.Abstractions;

public interface ICharacterRepository
{
	Task CreateAsync(Character character, CancellationToken cancellationToken = default);
	Task UpdateAsync(Character character, string etag, CancellationToken cancellationToken = default);
	Task<CosmosDocument<Character>?> FindByCharacterIdAsync(Guid characterId, Guid playerId, CancellationToken cancellationToken = default);
	Task<IReadOnlyList<Character>> GetByPlayerIdAsync(Guid playerId, CancellationToken cancellationToken = default);
}
