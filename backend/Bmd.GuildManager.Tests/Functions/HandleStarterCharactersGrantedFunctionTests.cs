using Bmd.GuildManager.Core.Events;
using Bmd.GuildManager.Functions.Functions;
using Microsoft.Extensions.Logging.Abstractions;
using System.Text.Json;
using Bmd.GuildManager.Functions.Serialization;

namespace Bmd.GuildManager.Tests.Functions;

public class HandleStarterCharactersGrantedFunctionTests
{
    private static string BuildMessage(Guid playerId, IReadOnlyList<Guid> characterIds)
    {
        var payload = new StarterCharactersGranted(playerId, characterIds);
        var envelope = EventEnvelope<StarterCharactersGranted>.Create(
            "test", playerId, payload);
        return JsonSerializer.Serialize(envelope, FunctionJsonOptions.Default);
    }

    [Fact]
    public async Task RunAsync_ValidEvent_PublishesCharacterCreatedForEach()
    {
        var publisher = new FakeEventPublisher();
        var function = new HandleStarterCharactersGrantedFunction(
            publisher,
            NullLogger<HandleStarterCharactersGrantedFunction>.Instance,
            new FakeRandomProvider(0.5));

        var playerId = Guid.NewGuid();
        var characterIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        await function.RunAsync(BuildMessage(playerId, characterIds), TestContext.Current.CancellationToken);

        Assert.Equal(2, publisher.Published.Count);
        Assert.All(publisher.Published, e =>
            Assert.Equal("CharacterCreated", e.EventType));
    }

    [Fact]
    public async Task RunAsync_ValidEvent_UsesPreAssignedCharacterIds()
    {
        var publisher = new FakeEventPublisher();
        var function = new HandleStarterCharactersGrantedFunction(
            publisher,
            NullLogger<HandleStarterCharactersGrantedFunction>.Instance,
            new FakeRandomProvider(0.5));

        var playerId = Guid.NewGuid();
        var characterId1 = Guid.NewGuid();
        var characterId2 = Guid.NewGuid();
        var characterIds = new List<Guid> { characterId1, characterId2 };

        await function.RunAsync(BuildMessage(playerId, characterIds), TestContext.Current.CancellationToken);

        var publishedIds = publisher.Published
            .Select(e => (CharacterCreated)e.Data)
            .Select(d => d.CharacterId)
            .ToList();

        Assert.Contains(characterId1, publishedIds);
        Assert.Contains(characterId2, publishedIds);
    }

    [Fact]
    public async Task RunAsync_ValidEvent_ForwardsCorrelationId()
    {
        var publisher = new FakeEventPublisher();
        var function = new HandleStarterCharactersGrantedFunction(
            publisher,
            NullLogger<HandleStarterCharactersGrantedFunction>.Instance,
            new FakeRandomProvider(0.5));

        var playerId = Guid.NewGuid();
        var characterIds = new List<Guid> { Guid.NewGuid() };
        var message = BuildMessage(playerId, characterIds);

        await function.RunAsync(message, TestContext.Current.CancellationToken);

        Assert.All(publisher.Published, e =>
            Assert.Equal(playerId, e.CorrelationId));
    }

    [Fact]
    public async Task RunAsync_ValidEvent_StarterCharactersHaveBeginnerStats()
    {
        var publisher = new FakeEventPublisher();
        var function = new HandleStarterCharactersGrantedFunction(
            publisher,
            NullLogger<HandleStarterCharactersGrantedFunction>.Instance,
            new FakeRandomProvider(0.5));

        var playerId = Guid.NewGuid();
        var characterIds = Enumerable.Range(0, 10)
            .Select(_ => Guid.NewGuid())
            .ToList();

        await function.RunAsync(BuildMessage(playerId, characterIds), TestContext.Current.CancellationToken);

        var characters = publisher.Published
            .Select(e => (CharacterCreated)e.Data)
            .ToList();

        Assert.All(characters, c =>
        {
            Assert.Equal(1, c.Level);
            Assert.Equal(7, c.Strength);
            Assert.Equal(7, c.Luck);
            Assert.Equal(7, c.Endurance);
        });
    }

    [Fact]
    public async Task RunAsync_InvalidJson_Throws()
    {
        var publisher = new FakeEventPublisher();
        var function = new HandleStarterCharactersGrantedFunction(
            publisher,
            NullLogger<HandleStarterCharactersGrantedFunction>.Instance,
            new FakeRandomProvider(0.5));

        await Assert.ThrowsAsync<JsonException>(() =>
            function.RunAsync("this is not json", TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task RunAsync_EmptyCharacterList_PublishesNoEvents()
    {
        var publisher = new FakeEventPublisher();
        var function = new HandleStarterCharactersGrantedFunction(
            publisher,
            NullLogger<HandleStarterCharactersGrantedFunction>.Instance,
            new FakeRandomProvider(0.5));

        await function.RunAsync(BuildMessage(Guid.NewGuid(), []), TestContext.Current.CancellationToken);

        Assert.Empty(publisher.Published);
    }
}
