using System.Text.Json.Serialization;

namespace Bmd.GuildManager.Core.Events;

public record ItemListed(
    [property: JsonPropertyName("listingId")] Guid ListingId,
    [property: JsonPropertyName("itemId")] Guid ItemId,
    [property: JsonPropertyName("playerId")] Guid PlayerId,
    [property: JsonPropertyName("itemTier")] string ItemTier,
    [property: JsonPropertyName("basePrice")] int BasePrice);

public record ItemSold(
    [property: JsonPropertyName("listingId")] Guid ListingId,
    [property: JsonPropertyName("buyerTier")] string BuyerTier,
    [property: JsonPropertyName("finalPrice")] int FinalPrice);

public record ItemListingCanceled(
    [property: JsonPropertyName("listingId")] Guid ListingId,
    [property: JsonPropertyName("itemId")] Guid ItemId,
    [property: JsonPropertyName("playerId")] Guid PlayerId);
