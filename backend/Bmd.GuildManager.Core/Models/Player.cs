using System.Text.Json.Serialization;

namespace Bmd.GuildManager.Core.Models;

public record Player(
	[property: JsonPropertyName("id")]             string Id,
	[property: JsonPropertyName("playerId")]       Guid PlayerId,
	[property: JsonPropertyName("guildName")]      string GuildName,
	[property: JsonPropertyName("gold")]           int Gold,
	[property: JsonPropertyName("createdDate")]    DateTimeOffset CreatedDate,
	[property: JsonPropertyName("onboardedAt")]    DateTimeOffset? OnboardedAt,
	[property: JsonPropertyName("idempotencyKey")] string? IdempotencyKey,
	[property: JsonPropertyName("stash")]          IReadOnlyList<Item> Stash)
{
	public static Player Create(string guildName, string? idempotencyKey = null, DateTimeOffset? createdDate = null)
	{
		var playerId = Guid.NewGuid();
		return new Player(
			Id:             playerId.ToString(),
			PlayerId:       playerId,
			GuildName:      guildName,
			Gold:           0,
			CreatedDate:    createdDate ?? DateTimeOffset.UtcNow,
			OnboardedAt:    null,
			IdempotencyKey: idempotencyKey,
			Stash:          []);
	}
}