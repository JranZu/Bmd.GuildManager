using System.Text.Json.Serialization;

namespace Bmd.GuildManager.Core.Models;

public record Quest(
    [property: JsonPropertyName("id")]                      string Id,
    [property: JsonPropertyName("questId")]                 Guid QuestId,
    [property: JsonPropertyName("name")]                    string Name,
    [property: JsonPropertyName("description")]             string Description,
    [property: JsonPropertyName("questType")]               QuestType QuestType,
    [property: JsonPropertyName("tier")]                    DifficultyTier Tier,
    [property: JsonPropertyName("riskLevel")]               RiskLevel RiskLevel,
    [property: JsonPropertyName("difficultyRating")]        int DifficultyRating,
    [property: JsonPropertyName("requiredAdventurers")]     int RequiredAdventurers,
    [property: JsonPropertyName("durationSeconds")]         int DurationSeconds,
    [property: JsonPropertyName("status")]                  QuestStatus Status,
    [property: JsonPropertyName("playerId")]                Guid? PlayerId,
    [property: JsonPropertyName("assignedCharacterIds")]    IReadOnlyList<Guid> AssignedCharacterIds,
    [property: JsonPropertyName("createdAt")]               DateTimeOffset CreatedAt,
    [property: JsonPropertyName("startedAt")]               DateTimeOffset? StartedAt,
    [property: JsonPropertyName("estimatedCompletionAt")]   DateTimeOffset? EstimatedCompletionAt);
