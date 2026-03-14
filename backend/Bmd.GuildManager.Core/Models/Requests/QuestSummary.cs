using System.Text.Json.Serialization;

namespace Bmd.GuildManager.Core.Models.Requests;

public record QuestSummary(
    [property: JsonPropertyName("questId")] Guid QuestId,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("questType")] QuestType QuestType,
    [property: JsonPropertyName("tier")] DifficultyTier Tier,
    [property: JsonPropertyName("riskLevel")] RiskLevel RiskLevel,
    [property: JsonPropertyName("difficultyRating")] int DifficultyRating,
    [property: JsonPropertyName("requiredAdventurers")] int RequiredAdventurers,
    [property: JsonPropertyName("durationSeconds")] int DurationSeconds);
