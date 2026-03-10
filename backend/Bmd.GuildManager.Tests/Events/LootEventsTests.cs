using System.Text.Json;
using Bmd.GuildManager.Core.Events;

namespace Bmd.GuildManager.Tests.Events;

public class LootEventsTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    [Fact]
    public void LootGenerated_RoundTrip()
    {
        var payload = new LootGenerated(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Veteran", "Rare");
        var envelope = EventEnvelope<LootGenerated>.Create("loot-service", Guid.NewGuid(), payload);

        var json = JsonSerializer.Serialize(envelope, JsonOptions);
        var result = JsonSerializer.Deserialize<EventEnvelope<LootGenerated>>(json, JsonOptions);

        Assert.NotNull(result);
        Assert.Equal("LootGenerated", result.EventType);
        Assert.Equal(payload, result.Data);
    }

    [Fact]
    public void ItemAddedToInventory_RoundTrip()
    {
        var payload = new ItemAddedToInventory(Guid.NewGuid(), Guid.NewGuid());
        var envelope = EventEnvelope<ItemAddedToInventory>.Create("loot-service", Guid.NewGuid(), payload);

        var json = JsonSerializer.Serialize(envelope, JsonOptions);
        var result = JsonSerializer.Deserialize<EventEnvelope<ItemAddedToInventory>>(json, JsonOptions);

        Assert.NotNull(result);
        Assert.Equal("ItemAddedToInventory", result.EventType);
        Assert.Equal(payload, result.Data);
    }

    [Fact]
    public void ItemEquipped_RoundTrip()
    {
        var payload = new ItemEquipped(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        var envelope = EventEnvelope<ItemEquipped>.Create("loot-service", Guid.NewGuid(), payload);

        var json = JsonSerializer.Serialize(envelope, JsonOptions);
        var result = JsonSerializer.Deserialize<EventEnvelope<ItemEquipped>>(json, JsonOptions);

        Assert.NotNull(result);
        Assert.Equal("ItemEquipped", result.EventType);
        Assert.Equal(payload, result.Data);
    }

    [Fact]
    public void ItemUnequipped_RoundTrip()
    {
        var payload = new ItemUnequipped(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        var envelope = EventEnvelope<ItemUnequipped>.Create("loot-service", Guid.NewGuid(), payload);

        var json = JsonSerializer.Serialize(envelope, JsonOptions);
        var result = JsonSerializer.Deserialize<EventEnvelope<ItemUnequipped>>(json, JsonOptions);

        Assert.NotNull(result);
        Assert.Equal("ItemUnequipped", result.EventType);
        Assert.Equal(payload, result.Data);
    }

    [Fact]
    public void ItemDiscarded_RoundTrip()
    {
        var payload = new ItemDiscarded(Guid.NewGuid(), Guid.NewGuid());
        var envelope = EventEnvelope<ItemDiscarded>.Create("loot-service", Guid.NewGuid(), payload);

        var json = JsonSerializer.Serialize(envelope, JsonOptions);
        var result = JsonSerializer.Deserialize<EventEnvelope<ItemDiscarded>>(json, JsonOptions);

        Assert.NotNull(result);
        Assert.Equal("ItemDiscarded", result.EventType);
        Assert.Equal(payload, result.Data);
    }
}
