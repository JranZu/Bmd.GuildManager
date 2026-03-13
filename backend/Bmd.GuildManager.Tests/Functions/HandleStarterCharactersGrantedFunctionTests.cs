using Bmd.GuildManager.Core.Events;
using Bmd.GuildManager.Functions.Functions;
using Microsoft.Extensions.Logging.Abstractions;
using System.Text.Json;

namespace Bmd.GuildManager.Tests.Functions;

public class HandleStarterCharactersGrantedFunctionTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private static string BuildMessage(Guid playerId, IReadOnlyList<Guid> characterIds)
    {
        var payload = new StarterCharactersGranted(playerId, characterIds);
        var envelope = EventEnvelope<StarterCharactersGranted>.Create(
            "test", playerId, payload);
        return JsonSerializer.Serialize(envelope, JsonOptions);
    }

    [Fact]
    public async Task RunAsync_ValidEvent_PublishesCharacterCreatedForEach()
    {
        var publisher = new FakeEventPublisher();
        var function = new HandleStarterCharactersGrantedFunction(
            publisher,
            NullLogger<HandleStarterCharactersGrantedFunction>.Instance);

        var playerId = Guid.NewGuid();
        var characterIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        await function.RunAsync(BuildMessage(playerId, characterIds));

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
            NullLogger<HandleStarterCharactersGrantedFunction>.Instance);

        var playerId = Guid.NewGuid();
        var characterId1 = Guid.NewGuid();
        var characterId2 = Guid.NewGuid();
        var characterIds = new List<Guid> { characterId1, characterId2 };

        await function.RunAsync(BuildMessage(playerId, characterIds));

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
            NullLogger<HandleStarterCharactersGrantedFunction>.Instance);

        var playerId = Guid.NewGuid();
        var characterIds = new List<Guid> { Guid.NewGuid() };
        var message = BuildMessage(playerId, characterIds);

        await function.RunAsync(message);

        Assert.All(publisher.Published, e =>
            Assert.Equal(playerId, e.CorrelationId));
    }

    [Fact]
    public async Task RunAsync_ValidEvent_StarterCharactersHaveBeginnerStats()
    {
        var publisher = new FakeEventPublisher();
        var function = new HandleStarterCharactersGrantedFunction(
            publisher,
            NullLogger<HandleStarterCharactersGrantedFunction>.Instance);

        var playerId = Guid.NewGuid();
        var characterIds = Enumerable.Range(0, 10)
            .Select(_ => Guid.NewGuid())
            .ToList();

        await function.RunAsync(BuildMessage(playerId, characterIds));

        var characters = publisher.Published
            .Select(e => (CharacterCreated)e.Data)
            .ToList();

        Assert.All(characters, c =>
        {
            Assert.Equal(1, c.Level);
            Assert.InRange(c.Strength, 3, 10);
            Assert.InRange(c.Luck, 3, 10);
            Assert.InRange(c.Endurance, 3, 10);
        });
    }

    [Fact]
    public async Task RunAsync_InvalidJson_DoesNotThrow()
    {
        var publisher = new FakeEventPublisher();
        var function = new HandleStarterCharactersGrantedFunction(
            publisher,
            NullLogger<HandleStarterCharactersGrantedFunction>.Instance);

        await function.RunAsync("this is not json");

        Assert.Empty(publisher.Published);
    }

    [Fact]
    public async Task RunAsync_EmptyCharacterList_PublishesNoEvents()
    {
        var publisher = new FakeEventPublisher();
        var function = new HandleStarterCharactersGrantedFunction(
            publisher,
            NullLogger<HandleStarterCharactersGrantedFunction>.Instance);

        await function.RunAsync(BuildMessage(Guid.NewGuid(), []));

        Assert.Empty(publisher.Published);
    }
}
