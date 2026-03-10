using System.Text.Json.Serialization;

namespace Bmd.GuildManager.Core.Events;

public record LootGenerated(
    [property: JsonPropertyName("itemId")] Guid ItemId,
    [property: JsonPropertyName("playerId")] Guid PlayerId,
    [property: JsonPropertyName("questId")] Guid QuestId,
    [property: JsonPropertyName("itemTier")] string ItemTier,
    [property: JsonPropertyName("rarity")] string Rarity);

public record ItemAddedToInventory(
    [property: JsonPropertyName("itemId")] Guid ItemId,
    [property: JsonPropertyName("playerId")] Guid PlayerId);

public record ItemEquipped(
    [property: JsonPropertyName("itemId")] Guid ItemId,
    [property: JsonPropertyName("characterId")] Guid CharacterId,
    [property: JsonPropertyName("playerId")] Guid PlayerId);

public record ItemUnequipped(
    [property: JsonPropertyName("itemId")] Guid ItemId,
    [property: JsonPropertyName("characterId")] Guid CharacterId,
    [property: JsonPropertyName("playerId")] Guid PlayerId);

public record ItemDiscarded(
    [property: JsonPropertyName("itemId")] Guid ItemId,
    [property: JsonPropertyName("playerId")] Guid PlayerId);
