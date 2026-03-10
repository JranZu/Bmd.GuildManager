using System.Text.Json;
using Bmd.GuildManager.Core.Events;

namespace Bmd.GuildManager.Tests.Events;

public class MarketEventsTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    [Fact]
    public void ItemListed_RoundTrip()
    {
        var payload = new ItemListed(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Elite", 250);
        var envelope = EventEnvelope<ItemListed>.Create("market-service", Guid.NewGuid(), payload);

        var json = JsonSerializer.Serialize(envelope, JsonOptions);
        var result = JsonSerializer.Deserialize<EventEnvelope<ItemListed>>(json, JsonOptions);

        Assert.NotNull(result);
        Assert.Equal("ItemListed", result.EventType);
        Assert.Equal(payload, result.Data);
    }

    [Fact]
    public void ItemSold_RoundTrip()
    {
        var payload = new ItemSold(Guid.NewGuid(), "Veteran", 300);
        var envelope = EventEnvelope<ItemSold>.Create("market-service", Guid.NewGuid(), payload);

        var json = JsonSerializer.Serialize(envelope, JsonOptions);
        var result = JsonSerializer.Deserialize<EventEnvelope<ItemSold>>(json, JsonOptions);

        Assert.NotNull(result);
        Assert.Equal("ItemSold", result.EventType);
        Assert.Equal(payload, result.Data);
    }

    [Fact]
    public void ItemListingCanceled_RoundTrip()
    {
        var payload = new ItemListingCanceled(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        var envelope = EventEnvelope<ItemListingCanceled>.Create("market-service", Guid.NewGuid(), payload);

        var json = JsonSerializer.Serialize(envelope, JsonOptions);
        var result = JsonSerializer.Deserialize<EventEnvelope<ItemListingCanceled>>(json, JsonOptions);

        Assert.NotNull(result);
        Assert.Equal("ItemListingCanceled", result.EventType);
        Assert.Equal(payload, result.Data);
    }
}
