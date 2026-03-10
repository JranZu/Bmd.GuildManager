using System.Text.Json.Serialization;

namespace Bmd.GuildManager.Core.Events;

public record QuestStarted(
    [property: JsonPropertyName("questId")] Guid QuestId,
    [property: JsonPropertyName("playerId")] Guid PlayerId,
    [property: JsonPropertyName("questType")] string QuestType,
    [property: JsonPropertyName("characters")] IReadOnlyList<Guid> Characters,
    [property: JsonPropertyName("durationSeconds")] int DurationSeconds);

public record QuestCompleted(
    [property: JsonPropertyName("questId")] Guid QuestId,
    [property: JsonPropertyName("playerId")] Guid PlayerId,
    [property: JsonPropertyName("success")] bool Success);

public record QuestResolved(
    [property: JsonPropertyName("questId")] Guid QuestId,
    [property: JsonPropertyName("playerId")] Guid PlayerId,
    [property: JsonPropertyName("outcome")] string Outcome,
    [property: JsonPropertyName("characters")] IReadOnlyList<QuestResolvedCharacter> Characters,
    [property: JsonPropertyName("lootGenerated")] bool LootGenerated,
    [property: JsonPropertyName("goldAwarded")] int GoldAwarded);

public record QuestResolvedCharacter(
    [property: JsonPropertyName("characterId")] Guid CharacterId,
    [property: JsonPropertyName("survived")] bool Survived);
