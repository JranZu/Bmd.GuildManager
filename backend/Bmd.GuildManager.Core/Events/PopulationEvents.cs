using System.Text.Json.Serialization;

namespace Bmd.GuildManager.Core.Events;

public record PopulationUpdateScheduled(
    [property: JsonPropertyName("scheduledTime")] DateTime ScheduledTime);

public record PopulationUpdated(
    [property: JsonPropertyName("beginner")] int Beginner,
    [property: JsonPropertyName("veteran")] int Veteran,
    [property: JsonPropertyName("elite")] int Elite,
    [property: JsonPropertyName("epic")] int Epic);

public record PlayerEventOccurred(
    [property: JsonPropertyName("playerId")] Guid PlayerId,
    [property: JsonPropertyName("eventName")] string EventName);
