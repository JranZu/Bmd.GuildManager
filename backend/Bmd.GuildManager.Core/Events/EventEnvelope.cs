using System.Text.Json.Serialization;

namespace Bmd.GuildManager.Core.Events;

/// <summary>
/// Generic envelope that wraps every domain event with consistent metadata
/// for observability, distributed tracing, and debugging across services.
/// </summary>
public record EventEnvelope<T>(
    [property: JsonPropertyName("eventId")] Guid EventId,
    [property: JsonPropertyName("eventType")] string EventType,
    [property: JsonPropertyName("timestamp")] DateTimeOffset Timestamp,
    [property: JsonPropertyName("correlationId")] Guid CorrelationId,
    [property: JsonPropertyName("source")] string Source,
    [property: JsonPropertyName("version")] int Version,
    [property: JsonPropertyName("data")] T Data)
{
    /// <summary>
    /// Creates a new <see cref="EventEnvelope{T}"/> with auto-populated
    /// <see cref="EventId"/>, <see cref="Timestamp"/>, and <see cref="Version"/>.
    /// </summary>
    public static EventEnvelope<T> Create(string source, Guid correlationId, T data, Guid? eventId = null, DateTimeOffset? timestamp = null) =>
        new(
            EventId: eventId ?? Guid.NewGuid(),
            EventType: typeof(T).Name,
            Timestamp: timestamp ?? DateTimeOffset.UtcNow,
            CorrelationId: correlationId,
            Source: source,
            Version: 1,
            Data: data);
}
