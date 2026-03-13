using System.Text.Json.Serialization;

namespace Bmd.GuildManager.Core.Events;

public record LootGenerated(
    [property: JsonPropertyName("itemId")]         Guid ItemId,
    [property: JsonPropertyName("playerId")]       Guid PlayerId,
    [property: JsonPropertyName("questId")]        Guid QuestId,
    [property: JsonPropertyName("name")]           string Name,
    [property: JsonPropertyName("tier")]           string Tier,
    [property: JsonPropertyName("rarity")]         string Rarity,
    [property: JsonPropertyName("strengthBonus")]  int StrengthBonus,
    [property: JsonPropertyName("luckBonus")]      int LuckBonus,
    [property: JsonPropertyName("enduranceBonus")] int EnduranceBonus,
    [property: JsonPropertyName("basePrice")]      int BasePrice);

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
