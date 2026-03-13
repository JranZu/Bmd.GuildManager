using System.Text.Json.Serialization;

namespace Bmd.GuildManager.Core.Events;

public record PopulationUpdateScheduled(
    [property: JsonPropertyName("scheduledTime")] DateTimeOffset ScheduledTime);

public record PopulationUpdated(
    [property: JsonPropertyName("novice")]      int Novice,
    [property: JsonPropertyName("apprentice")]  int Apprentice,
    [property: JsonPropertyName("veteran")]     int Veteran,
    [property: JsonPropertyName("elite")]       int Elite,
    [property: JsonPropertyName("legendary")]   int Legendary);

public record PlayerEventOccurred(
    [property: JsonPropertyName("playerId")] Guid PlayerId,
    [property: JsonPropertyName("eventName")] string EventName);
