using Bmd.GuildManager.Core.Events;
using Bmd.GuildManager.Core.Models;
using Bmd.GuildManager.Functions.Functions;
using Microsoft.Extensions.Logging.Abstractions;
using System.Text.Json;

namespace Bmd.GuildManager.Tests.Functions;

public class HandleCharacterCreatedFunctionTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private static string BuildMessage(Guid playerId, Guid characterId, string name = "Aldric")
    {
        var payload = new CharacterCreated(playerId, characterId, name, 1, 8, 6, 7);
        var envelope = EventEnvelope<CharacterCreated>.Create(
            "test", playerId, payload);
        return JsonSerializer.Serialize(envelope, JsonOptions);
    }

    [Fact]
    public async Task RunAsync_ValidEvent_PersistsCharacter()
    {
        var repository = new FakeCharacterRepository();
        var function = new HandleCharacterCreatedFunction(
            repository,
            NullLogger<HandleCharacterCreatedFunction>.Instance);

        var playerId = Guid.NewGuid();
        var characterId = Guid.NewGuid();
        await function.RunAsync(BuildMessage(playerId, characterId));

        Assert.Single(repository.Characters);
        var character = repository.Characters[0];
        Assert.Equal(characterId, character.CharacterId);
        Assert.Equal(playerId, character.PlayerId);
        Assert.Equal("Aldric", character.Name);
        Assert.Equal(1, character.Level);
        Assert.Equal(CharacterStatus.Idle, character.Status);
        Assert.Empty(character.EquipmentIds);
    }

    [Fact]
    public async Task RunAsync_DuplicateEvent_DoesNotCreateDuplicate()
    {
        var repository = new FakeCharacterRepository();
        var function = new HandleCharacterCreatedFunction(
            repository,
            NullLogger<HandleCharacterCreatedFunction>.Instance);

        var playerId = Guid.NewGuid();
        var characterId = Guid.NewGuid();
        var message = BuildMessage(playerId, characterId);

        await function.RunAsync(message);
        await function.RunAsync(message);

        Assert.Single(repository.Characters);
    }

    [Fact]
    public async Task RunAsync_InvalidJson_DoesNotThrow()
    {
        var repository = new FakeCharacterRepository();
        var function = new HandleCharacterCreatedFunction(
            repository,
            NullLogger<HandleCharacterCreatedFunction>.Instance);

        await function.RunAsync("this is not json");

        Assert.Empty(repository.Characters);
    }

    [Fact]
    public async Task RunAsync_ValidEvent_SetsStatusToIdle()
    {
        var repository = new FakeCharacterRepository();
        var function = new HandleCharacterCreatedFunction(
            repository,
            NullLogger<HandleCharacterCreatedFunction>.Instance);

        await function.RunAsync(BuildMessage(Guid.NewGuid(), Guid.NewGuid()));

        Assert.Equal(CharacterStatus.Idle, repository.Characters[0].Status);
    }
}
