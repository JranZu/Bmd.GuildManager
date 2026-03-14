using System.Text.Json.Serialization;
using Bmd.GuildManager.Core.Models;

namespace Bmd.GuildManager.Core.Events;

public record QuestStarted(
    [property: JsonPropertyName("questId")] Guid QuestId,
    [property: JsonPropertyName("playerId")] Guid PlayerId,
    [property: JsonPropertyName("questType")] QuestType QuestType,
    [property: JsonPropertyName("questTier")] DifficultyTier QuestTier,
    [property: JsonPropertyName("characterIds")] IReadOnlyList<Guid> CharacterIds,
    [property: JsonPropertyName("durationSeconds")] int DurationSeconds,
    [property: JsonPropertyName("estimatedCompletionAt")] DateTimeOffset EstimatedCompletionAt);

public record QuestCompleted(
    [property: JsonPropertyName("questId")] Guid QuestId,
    [property: JsonPropertyName("playerId")] Guid PlayerId,
    [property: JsonPropertyName("success")] bool Success);

public record QuestResolved(
    [property: JsonPropertyName("questId")] Guid QuestId,
    [property: JsonPropertyName("playerId")] Guid PlayerId,
    [property: JsonPropertyName("questTier")] DifficultyTier QuestTier,
    [property: JsonPropertyName("outcome")] string Outcome,
    [property: JsonPropertyName("xpAwarded")] int XpAwarded,
    [property: JsonPropertyName("characters")] IReadOnlyList<QuestResolvedCharacter> Characters,
    [property: JsonPropertyName("lootEligible")] bool LootEligible,
    [property: JsonPropertyName("goldAwarded")] int GoldAwarded);

public record QuestResolvedCharacter(
    [property: JsonPropertyName("characterId")] Guid CharacterId,
    [property: JsonPropertyName("survived")] bool Survived);
