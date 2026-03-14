using Bmd.GuildManager.Core.Events;
using Bmd.GuildManager.Core.Models;
using Bmd.GuildManager.Functions.Functions;
using Microsoft.Extensions.Logging.Abstractions;
using System.Text.Json;
using Bmd.GuildManager.Functions.Serialization;

namespace Bmd.GuildManager.Tests.Functions;

public class HandleCharacterCreatedFunctionTests
{
    private static string BuildMessage(Guid playerId, Guid characterId, string name = "Aldric")
    {
        var payload = new CharacterCreated(playerId, characterId, name, 1, 8, 6, 7);
        var envelope = EventEnvelope<CharacterCreated>.Create(
            "test", playerId, payload);
        return JsonSerializer.Serialize(envelope, FunctionJsonOptions.Default);
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
        await function.RunAsync(BuildMessage(playerId, characterId), TestContext.Current.CancellationToken);

        Assert.Single(repository.Characters);
        var character = repository.Characters[0];
        Assert.Equal(characterId, character.CharacterId);
        Assert.Equal(playerId, character.PlayerId);
        Assert.Equal("Aldric", character.Name);
        Assert.Equal(1, character.Level);
        Assert.Equal(CharacterStatus.Idle, character.Status);
        Assert.Empty(character.Equipment);
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

        await function.RunAsync(message, TestContext.Current.CancellationToken);
        await function.RunAsync(message, TestContext.Current.CancellationToken);

        Assert.Single(repository.Characters);
    }

    [Fact]
    public async Task RunAsync_InvalidJson_Throws()
    {
        var repository = new FakeCharacterRepository();
        var function = new HandleCharacterCreatedFunction(
            repository,
            NullLogger<HandleCharacterCreatedFunction>.Instance);

        await Assert.ThrowsAsync<JsonException>(() =>
            function.RunAsync("this is not json", TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task RunAsync_ValidEvent_SetsStatusToIdle()
    {
        var repository = new FakeCharacterRepository();
        var function = new HandleCharacterCreatedFunction(
            repository,
            NullLogger<HandleCharacterCreatedFunction>.Instance);

        await function.RunAsync(BuildMessage(Guid.NewGuid(), Guid.NewGuid()), TestContext.Current.CancellationToken);

        Assert.Equal(CharacterStatus.Idle, repository.Characters[0].Status);
    }
}
