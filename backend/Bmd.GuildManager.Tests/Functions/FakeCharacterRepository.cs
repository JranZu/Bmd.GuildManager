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

    public Task UpdateAsync(Character character, string etag)
    {
        var index = Characters.FindIndex(c => c.CharacterId == character.CharacterId);
        if (index >= 0)
            Characters[index] = character;
        return Task.CompletedTask;
    }

    public Task<CosmosDocument<Character>?> FindByCharacterIdAsync(Guid characterId, Guid playerId)
    {
        var match = Characters.FirstOrDefault(c =>
            c.CharacterId == characterId && c.PlayerId == playerId);
        return Task.FromResult(
            match is null ? null : new CosmosDocument<Character>(match, "fake-etag"));
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
