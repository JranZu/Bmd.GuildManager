using System.Text.Json;
using Bmd.GuildManager.Core.Events;

namespace Bmd.GuildManager.Tests.Events;

public class PlayerEventsTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    [Fact]
    public void PlayerCreated_RoundTrip()
    {
        var payload = new PlayerCreated(Guid.NewGuid(), "TestGuild");
        var envelope = EventEnvelope<PlayerCreated>.Create("player-service", Guid.NewGuid(), payload);

        var json = JsonSerializer.Serialize(envelope, JsonOptions);
        var result = JsonSerializer.Deserialize<EventEnvelope<PlayerCreated>>(json, JsonOptions);

        Assert.NotNull(result);
        Assert.Equal("PlayerCreated", result.EventType);
        Assert.Equal(payload, result.Data);
    }

    [Fact]
    public void GuildCreated_RoundTrip()
    {
        var payload = new GuildCreated(Guid.NewGuid(), "TestGuild", 500);
        var envelope = EventEnvelope<GuildCreated>.Create("player-service", Guid.NewGuid(), payload);

        var json = JsonSerializer.Serialize(envelope, JsonOptions);
        var result = JsonSerializer.Deserialize<EventEnvelope<GuildCreated>>(json, JsonOptions);

        Assert.NotNull(result);
        Assert.Equal("GuildCreated", result.EventType);
        Assert.Equal(payload, result.Data);
    }

    [Fact]
    public void StarterCharactersGranted_RoundTrip()
    {
        var payload = new StarterCharactersGranted(Guid.NewGuid(), [Guid.NewGuid(), Guid.NewGuid()]);
        var envelope = EventEnvelope<StarterCharactersGranted>.Create("player-service", Guid.NewGuid(), payload);

        var json = JsonSerializer.Serialize(envelope, JsonOptions);
        var result = JsonSerializer.Deserialize<EventEnvelope<StarterCharactersGranted>>(json, JsonOptions);

        Assert.NotNull(result);
        Assert.Equal("StarterCharactersGranted", result.EventType);
        Assert.Equal(payload.PlayerId, result.Data.PlayerId);
        Assert.Equal(payload.CharacterIds, result.Data.CharacterIds);
    }

    [Fact]
    public void StarterItemsGranted_RoundTrip()
    {
        var payload = new StarterItemsGranted(Guid.NewGuid(), [Guid.NewGuid(), Guid.NewGuid()]);
        var envelope = EventEnvelope<StarterItemsGranted>.Create("player-service", Guid.NewGuid(), payload);

        var json = JsonSerializer.Serialize(envelope, JsonOptions);
        var result = JsonSerializer.Deserialize<EventEnvelope<StarterItemsGranted>>(json, JsonOptions);

        Assert.NotNull(result);
        Assert.Equal("StarterItemsGranted", result.EventType);
        Assert.Equal(payload.PlayerId, result.Data.PlayerId);
        Assert.Equal(payload.ItemIds, result.Data.ItemIds);
    }
}
