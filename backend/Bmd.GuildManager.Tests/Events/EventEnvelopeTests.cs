using System.Text.Json;
using Bmd.GuildManager.Core.Events;

namespace Bmd.GuildManager.Tests.Events;

public class EventEnvelopeTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private record SamplePayload(string QuestName, int Difficulty);

    [Fact]
    public void RoundTrip_SerializesAndDeserializesCorrectly()
    {
        var payload = new SamplePayload("Dragon Hunt", 5);
        var correlationId = Guid.NewGuid();

        var envelope = EventEnvelope<SamplePayload>.Create("quest-service", correlationId, payload);

        var json = JsonSerializer.Serialize(envelope, JsonOptions);
        var deserialized = JsonSerializer.Deserialize<EventEnvelope<SamplePayload>>(json, JsonOptions);

        Assert.NotNull(deserialized);
        Assert.Equal(envelope.EventId, deserialized.EventId);
        Assert.Equal(envelope.EventType, deserialized.EventType);
        Assert.Equal(envelope.Timestamp, deserialized.Timestamp);
        Assert.Equal(envelope.CorrelationId, deserialized.CorrelationId);
        Assert.Equal(envelope.Source, deserialized.Source);
        Assert.Equal(envelope.Version, deserialized.Version);
        Assert.Equal(envelope.Data, deserialized.Data);
    }

    [Fact]
    public void Serialize_UsesCamelCaseFieldNames()
    {
        var envelope = EventEnvelope<SamplePayload>.Create(
            "quest-service",
            Guid.NewGuid(),
            new SamplePayload("Dragon Hunt", 5));

        var json = JsonSerializer.Serialize(envelope, JsonOptions);

        Assert.Contains("\"eventId\"", json);
        Assert.Contains("\"eventType\"", json);
        Assert.Contains("\"timestamp\"", json);
        Assert.Contains("\"correlationId\"", json);
        Assert.Contains("\"source\"", json);
        Assert.Contains("\"version\"", json);
        Assert.Contains("\"data\"", json);
    }

    [Fact]
    public void Create_SetsEventTypeToPayloadTypeName()
    {
        var envelope = EventEnvelope<SamplePayload>.Create(
            "quest-service",
            Guid.NewGuid(),
            new SamplePayload("Dragon Hunt", 5));

        Assert.Equal(nameof(SamplePayload), envelope.EventType);
    }

    [Fact]
    public void Create_SetsVersionToOne()
    {
        var envelope = EventEnvelope<SamplePayload>.Create(
            "quest-service",
            Guid.NewGuid(),
            new SamplePayload("Dragon Hunt", 5));

        Assert.Equal(1, envelope.Version);
    }

    [Fact]
    public void Create_SetsTimestampToUtc()
    {
        var before = DateTime.UtcNow;

        var envelope = EventEnvelope<SamplePayload>.Create(
            "quest-service",
            Guid.NewGuid(),
            new SamplePayload("Dragon Hunt", 5));

        var after = DateTime.UtcNow;

        Assert.InRange(envelope.Timestamp, before, after);
        Assert.Equal(DateTimeKind.Utc, envelope.Timestamp.Kind);
    }

    [Fact]
    public void Create_PreservesCorrelationId()
    {
        var correlationId = Guid.NewGuid();

        var envelope = EventEnvelope<SamplePayload>.Create(
            "quest-service",
            correlationId,
            new SamplePayload("Dragon Hunt", 5));

        Assert.Equal(correlationId, envelope.CorrelationId);
    }

    [Fact]
    public void ValueEquality_TwoIdenticalEnvelopes_AreEqual()
    {
        var eventId = Guid.NewGuid();
        var correlationId = Guid.NewGuid();
        var timestamp = DateTime.UtcNow;
        var payload = new SamplePayload("Dragon Hunt", 5);

        var a = new EventEnvelope<SamplePayload>(eventId, "SamplePayload", timestamp, correlationId, "quest-service", 1, payload);
        var b = new EventEnvelope<SamplePayload>(eventId, "SamplePayload", timestamp, correlationId, "quest-service", 1, payload);

        Assert.Equal(a, b);
    }
}
