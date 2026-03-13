using System.Text.Json;
using Bmd.GuildManager.Core.Events;

namespace Bmd.GuildManager.Tests.Events;

public class PopulationEventsTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    [Fact]
    public void PopulationUpdateScheduled_RoundTrip()
    {
        var payload = new PopulationUpdateScheduled(DateTime.UtcNow);
        var envelope = EventEnvelope<PopulationUpdateScheduled>.Create("population-service", Guid.NewGuid(), payload);

        var json = JsonSerializer.Serialize(envelope, JsonOptions);
        var result = JsonSerializer.Deserialize<EventEnvelope<PopulationUpdateScheduled>>(json, JsonOptions);

        Assert.NotNull(result);
        Assert.Equal("PopulationUpdateScheduled", result.EventType);
        Assert.Equal(payload, result.Data);
    }

    [Fact]
    public void PopulationUpdated_RoundTrip()
    {
        var payload = new PopulationUpdated(1000, 500, 200, 50, 10);
        var envelope = EventEnvelope<PopulationUpdated>.Create("population-service", Guid.NewGuid(), payload);

        var json = JsonSerializer.Serialize(envelope, JsonOptions);
        var result = JsonSerializer.Deserialize<EventEnvelope<PopulationUpdated>>(json, JsonOptions);

        Assert.NotNull(result);
        Assert.Equal("PopulationUpdated", result.EventType);
        Assert.Equal(payload.Novice,      result.Data.Novice);
        Assert.Equal(payload.Apprentice,  result.Data.Apprentice);
        Assert.Equal(payload.Veteran,     result.Data.Veteran);
        Assert.Equal(payload.Elite,       result.Data.Elite);
        Assert.Equal(payload.Legendary,   result.Data.Legendary);
    }

    [Fact]
    public void PlayerEventOccurred_RoundTrip()
    {
        var payload = new PlayerEventOccurred(Guid.NewGuid(), "QuestCompleted");
        var envelope = EventEnvelope<PlayerEventOccurred>.Create("population-service", Guid.NewGuid(), payload);

        var json = JsonSerializer.Serialize(envelope, JsonOptions);
        var result = JsonSerializer.Deserialize<EventEnvelope<PlayerEventOccurred>>(json, JsonOptions);

        Assert.NotNull(result);
        Assert.Equal("PlayerEventOccurred", result.EventType);
        Assert.Equal(payload, result.Data);
    }
}
