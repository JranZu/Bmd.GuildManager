using Bmd.GuildManager.Core.Models;

namespace Bmd.GuildManager.Core.Abstractions;

public interface ICharacterRepository
{
	Task CreateAsync(Character character);
	Task<Character?> FindByCharacterIdAsync(Guid characterId, Guid playerId);
	Task<IReadOnlyList<Character>> GetByPlayerIdAsync(Guid playerId);
}
