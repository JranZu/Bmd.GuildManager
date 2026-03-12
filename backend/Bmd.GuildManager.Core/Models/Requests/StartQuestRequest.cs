using System.Text.Json.Serialization;

namespace Bmd.GuildManager.Core.Models.Requests;

public record StartQuestRequest(
    [property: JsonPropertyName("playerId")] Guid PlayerId,
    [property: JsonPropertyName("questId")] Guid QuestId,
    [property: JsonPropertyName("characterIds")] IReadOnlyList<Guid> CharacterIds);
