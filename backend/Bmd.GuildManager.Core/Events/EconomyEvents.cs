using System.Text.Json.Serialization;

namespace Bmd.GuildManager.Core.Events;

public record GoldCredited(
    [property: JsonPropertyName("playerId")] Guid PlayerId,
    [property: JsonPropertyName("amount")] int Amount,
    [property: JsonPropertyName("reason")] string Reason,
    [property: JsonPropertyName("referenceId")] Guid ReferenceId);

public record GoldDebited(
    [property: JsonPropertyName("playerId")] Guid PlayerId,
    [property: JsonPropertyName("amount")] int Amount,
    [property: JsonPropertyName("reason")] string Reason,
    [property: JsonPropertyName("referenceId")] Guid ReferenceId);

public record AdventurerRecruited(
    [property: JsonPropertyName("playerId")] Guid PlayerId,
    [property: JsonPropertyName("characterId")] Guid CharacterId,
    [property: JsonPropertyName("recruitmentCost")] int RecruitmentCost);
