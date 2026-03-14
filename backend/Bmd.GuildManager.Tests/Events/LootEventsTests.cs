using System.Text.Json;
using Bmd.GuildManager.Core.Events;
using Bmd.GuildManager.Functions.Serialization;

namespace Bmd.GuildManager.Tests.Events;

public class LootEventsTests
{
    [Fact]
    public void LootGenerated_RoundTrip()
    {
        var payload = new LootGenerated(
            ItemId:         Guid.NewGuid(),
            PlayerId:       Guid.NewGuid(),
            QuestId:        Guid.NewGuid(),
            Name:           "Shadow Blade",
            Tier:           "Veteran",
            Rarity:         "Rare",
            StrengthBonus:  12,
            LuckBonus:      0,
            EnduranceBonus: 5,
            BasePrice:      300);

        var envelope = EventEnvelope<LootGenerated>.Create("loot-service", Guid.NewGuid(), payload);

        var json = JsonSerializer.Serialize(envelope, FunctionJsonOptions.Default);
        var result = JsonSerializer.Deserialize<EventEnvelope<LootGenerated>>(json, FunctionJsonOptions.Default);

        Assert.NotNull(result);
        Assert.Equal("LootGenerated", result.EventType);
        Assert.Equal(payload, result.Data);
    }

    [Fact]
    public void ItemAddedToInventory_RoundTrip()
    {
        var payload = new ItemAddedToInventory(Guid.NewGuid(), Guid.NewGuid());
        var envelope = EventEnvelope<ItemAddedToInventory>.Create("loot-service", Guid.NewGuid(), payload);

        var json = JsonSerializer.Serialize(envelope, FunctionJsonOptions.Default);
        var result = JsonSerializer.Deserialize<EventEnvelope<ItemAddedToInventory>>(json, FunctionJsonOptions.Default);

        Assert.NotNull(result);
        Assert.Equal("ItemAddedToInventory", result.EventType);
        Assert.Equal(payload, result.Data);
    }

    [Fact]
    public void ItemEquipped_RoundTrip()
    {
        var payload = new ItemEquipped(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        var envelope = EventEnvelope<ItemEquipped>.Create("loot-service", Guid.NewGuid(), payload);

        var json = JsonSerializer.Serialize(envelope, FunctionJsonOptions.Default);
        var result = JsonSerializer.Deserialize<EventEnvelope<ItemEquipped>>(json, FunctionJsonOptions.Default);

        Assert.NotNull(result);
        Assert.Equal("ItemEquipped", result.EventType);
        Assert.Equal(payload, result.Data);
    }

    [Fact]
    public void ItemUnequipped_RoundTrip()
    {
        var payload = new ItemUnequipped(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        var envelope = EventEnvelope<ItemUnequipped>.Create("loot-service", Guid.NewGuid(), payload);

        var json = JsonSerializer.Serialize(envelope, FunctionJsonOptions.Default);
        var result = JsonSerializer.Deserialize<EventEnvelope<ItemUnequipped>>(json, FunctionJsonOptions.Default);

        Assert.NotNull(result);
        Assert.Equal("ItemUnequipped", result.EventType);
        Assert.Equal(payload, result.Data);
    }

    [Fact]
    public void ItemDiscarded_RoundTrip()
    {
        var payload = new ItemDiscarded(Guid.NewGuid(), Guid.NewGuid());
        var envelope = EventEnvelope<ItemDiscarded>.Create("loot-service", Guid.NewGuid(), payload);

        var json = JsonSerializer.Serialize(envelope, FunctionJsonOptions.Default);
        var result = JsonSerializer.Deserialize<EventEnvelope<ItemDiscarded>>(json, FunctionJsonOptions.Default);

        Assert.NotNull(result);
        Assert.Equal("ItemDiscarded", result.EventType);
        Assert.Equal(payload, result.Data);
    }
}
