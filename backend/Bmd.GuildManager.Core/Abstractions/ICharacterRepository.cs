using Bmd.GuildManager.Core.Models;

namespace Bmd.GuildManager.Core.Abstractions;

public interface ICharacterRepository
{
	Task CreateAsync(Character character);
	Task UpdateAsync(Character character, string etag);
	Task<CosmosDocument<Character>?> FindByCharacterIdAsync(Guid characterId, Guid playerId);
	Task<IReadOnlyList<Character>> GetByPlayerIdAsync(Guid playerId);
}
