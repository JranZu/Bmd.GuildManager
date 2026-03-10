using System.Text.Json;
using Bmd.GuildManager.Core.Events;

namespace Bmd.GuildManager.Tests.Events;

public class EconomyEventsTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    [Fact]
    public void GoldCredited_RoundTrip()
    {
        var payload = new GoldCredited(Guid.NewGuid(), 100, "QuestReward", Guid.NewGuid());
        var envelope = EventEnvelope<GoldCredited>.Create("economy-service", Guid.NewGuid(), payload);

        var json = JsonSerializer.Serialize(envelope, JsonOptions);
        var result = JsonSerializer.Deserialize<EventEnvelope<GoldCredited>>(json, JsonOptions);

        Assert.NotNull(result);
        Assert.Equal("GoldCredited", result.EventType);
        Assert.Equal(payload, result.Data);
    }

    [Fact]
    public void GoldDebited_RoundTrip()
    {
        var payload = new GoldDebited(Guid.NewGuid(), 50, "MarketPurchase", Guid.NewGuid());
        var envelope = EventEnvelope<GoldDebited>.Create("economy-service", Guid.NewGuid(), payload);

        var json = JsonSerializer.Serialize(envelope, JsonOptions);
        var result = JsonSerializer.Deserialize<EventEnvelope<GoldDebited>>(json, JsonOptions);

        Assert.NotNull(result);
        Assert.Equal("GoldDebited", result.EventType);
        Assert.Equal(payload, result.Data);
    }

    [Fact]
    public void AdventurerRecruited_RoundTrip()
    {
        var payload = new AdventurerRecruited(Guid.NewGuid(), Guid.NewGuid(), 200);
        var envelope = EventEnvelope<AdventurerRecruited>.Create("economy-service", Guid.NewGuid(), payload);

        var json = JsonSerializer.Serialize(envelope, JsonOptions);
        var result = JsonSerializer.Deserialize<EventEnvelope<AdventurerRecruited>>(json, JsonOptions);

        Assert.NotNull(result);
        Assert.Equal("AdventurerRecruited", result.EventType);
        Assert.Equal(payload, result.Data);
    }
}
