using System.Text.Json;
using Bmd.GuildManager.Core.Events;

namespace Bmd.GuildManager.Tests.Events;

public class QuestEventsTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    [Fact]
    public void QuestStarted_RoundTrip()
    {
        var estimatedCompletionAt = DateTimeOffset.UtcNow.AddSeconds(300);
        var payload = new QuestStarted(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Combat",
            "Rare",
            [Guid.NewGuid(), Guid.NewGuid()],
            300,
            estimatedCompletionAt);
        var envelope = EventEnvelope<QuestStarted>.Create("quest-service", Guid.NewGuid(), payload);

        var json = JsonSerializer.Serialize(envelope, JsonOptions);
        var result = JsonSerializer.Deserialize<EventEnvelope<QuestStarted>>(json, JsonOptions);

        Assert.NotNull(result);
        Assert.Equal("QuestStarted", result.EventType);
        Assert.Equal(payload.QuestId, result.Data.QuestId);
        Assert.Equal(payload.QuestTier, result.Data.QuestTier);
        Assert.Equal(payload.CharacterIds, result.Data.CharacterIds);
        Assert.Equal(payload.DurationSeconds, result.Data.DurationSeconds);
        Assert.Equal(payload.EstimatedCompletionAt, result.Data.EstimatedCompletionAt);
    }

    [Fact]
    public void QuestCompleted_RoundTrip()
    {
        var payload = new QuestCompleted(Guid.NewGuid(), Guid.NewGuid(), true);
        var envelope = EventEnvelope<QuestCompleted>.Create("quest-service", Guid.NewGuid(), payload);

        var json = JsonSerializer.Serialize(envelope, JsonOptions);
        var result = JsonSerializer.Deserialize<EventEnvelope<QuestCompleted>>(json, JsonOptions);

        Assert.NotNull(result);
        Assert.Equal("QuestCompleted", result.EventType);
        Assert.Equal(payload, result.Data);
    }

    [Fact]
    public void QuestResolved_RoundTrip()
    {
        var characters = new List<QuestResolvedCharacter>
        {
            new(Guid.NewGuid(), true),
            new(Guid.NewGuid(), false)
        };
        var payload = new QuestResolved(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Novice",
            "Success",
            25,
            characters,
            true,
            22);
        var envelope = EventEnvelope<QuestResolved>.Create("quest-service", Guid.NewGuid(), payload);

        var json = JsonSerializer.Serialize(envelope, JsonOptions);
        var result = JsonSerializer.Deserialize<EventEnvelope<QuestResolved>>(json, JsonOptions);

        Assert.NotNull(result);
        Assert.Equal("QuestResolved", result.EventType);
        Assert.Equal(payload.QuestId, result.Data.QuestId);
        Assert.Equal(payload.QuestTier, result.Data.QuestTier);
        Assert.Equal(payload.Outcome, result.Data.Outcome);
        Assert.Equal(payload.XpAwarded, result.Data.XpAwarded);
        Assert.Equal(payload.GoldAwarded, result.Data.GoldAwarded);
        Assert.Equal(payload.LootEligible, result.Data.LootEligible);
        Assert.Equal(2, result.Data.Characters.Count);
        Assert.Equal(characters[0].CharacterId, result.Data.Characters[0].CharacterId);
        Assert.Equal(characters[0].Survived, result.Data.Characters[0].Survived);
        Assert.Equal(characters[1].CharacterId, result.Data.Characters[1].CharacterId);
        Assert.False(result.Data.Characters[1].Survived);
    }
}
