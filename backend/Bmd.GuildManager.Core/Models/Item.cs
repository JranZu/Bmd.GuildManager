using System.Text.Json.Serialization;

namespace Bmd.GuildManager.Core.Models;

public record Item(
	[property: JsonPropertyName("itemId")]            Guid ItemId,
	[property: JsonPropertyName("name")]              string Name,
	[property: JsonPropertyName("tier")]              string Tier,
	[property: JsonPropertyName("rarity")]            string Rarity,
	[property: JsonPropertyName("strengthBonus")]     int StrengthBonus,
	[property: JsonPropertyName("luckBonus")]         int LuckBonus,
	[property: JsonPropertyName("enduranceBonus")]    int EnduranceBonus,
	[property: JsonPropertyName("basePrice")]         int BasePrice,
	[property: JsonPropertyName("status")]            ItemStatus Status,
	[property: JsonPropertyName("transferTargetId")]  Guid? TransferTargetId,
	[property: JsonPropertyName("transferStartedAt")] DateTimeOffset? TransferStartedAt);
