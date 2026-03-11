using Bmd.GuildManager.Core.Abstractions;
using Bmd.GuildManager.Core.Models;

namespace Bmd.GuildManager.Tests.Functions;

public class FakeCharacterRepository : ICharacterRepository
{
    public List<Character> Characters { get; } = [];

    public Task CreateAsync(Character character)
    {
        Characters.Add(character);
        return Task.CompletedTask;
    }

    public Task<Character?> FindByCharacterIdAsync(Guid characterId, Guid playerId)
    {
        var match = Characters.FirstOrDefault(c =>
            c.CharacterId == characterId && c.PlayerId == playerId);
        return Task.FromResult(match);
    }

    public Task<IReadOnlyList<Character>> GetByPlayerIdAsync(Guid playerId)
    {
        var results = Characters
            .Where(c => c.PlayerId == playerId)
            .ToList()
            .AsReadOnly();
        return Task.FromResult<IReadOnlyList<Character>>(results);
    }
}
