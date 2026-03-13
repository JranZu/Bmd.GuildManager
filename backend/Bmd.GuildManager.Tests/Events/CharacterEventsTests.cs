using System.Text.Json;
using Bmd.GuildManager.Core.Events;
using Bmd.GuildManager.Core.Models;

namespace Bmd.GuildManager.Tests.Events;

public class CharacterEventsTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    [Fact]
    public void CharacterCreated_RoundTrip()
    {
        var payload = new CharacterCreated(Guid.NewGuid(), Guid.NewGuid(), "Theron", 1, 10, 5, 8);
        var envelope = EventEnvelope<CharacterCreated>.Create("character-service", Guid.NewGuid(), payload);

        var json = JsonSerializer.Serialize(envelope, JsonOptions);
        var result = JsonSerializer.Deserialize<EventEnvelope<CharacterCreated>>(json, JsonOptions);

        Assert.NotNull(result);
        Assert.Equal("CharacterCreated", result.EventType);
        Assert.Equal(payload, result.Data);
    }

    [Fact]
    public void CharacterDied_RoundTrip()
    {
        var payload = new CharacterDied(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DifficultyTier.Veteran);
        var envelope = EventEnvelope<CharacterDied>.Create("character-service", Guid.NewGuid(), payload);

        var json = JsonSerializer.Serialize(envelope, JsonOptions);
        var result = JsonSerializer.Deserialize<EventEnvelope<CharacterDied>>(json, JsonOptions);

        Assert.NotNull(result);
        Assert.Equal("CharacterDied", result.EventType);
        Assert.Equal(payload.CharacterId,   result.Data.CharacterId);
        Assert.Equal(payload.PlayerId,      result.Data.PlayerId);
        Assert.Equal(payload.QuestId,       result.Data.QuestId);
        Assert.Equal(payload.CharacterTier, result.Data.CharacterTier);
    }
}
