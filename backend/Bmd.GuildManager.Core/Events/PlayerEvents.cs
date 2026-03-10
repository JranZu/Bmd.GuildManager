using System.Text.Json.Serialization;

namespace Bmd.GuildManager.Core.Events;

public record PlayerCreated(
    [property: JsonPropertyName("playerId")] Guid PlayerId,
    [property: JsonPropertyName("guildName")] string GuildName);

public record GuildCreated(
    [property: JsonPropertyName("playerId")] Guid PlayerId,
    [property: JsonPropertyName("guildName")] string GuildName,
    [property: JsonPropertyName("startingGold")] int StartingGold);

public record StarterCharactersGranted(
    [property: JsonPropertyName("playerId")] Guid PlayerId,
    [property: JsonPropertyName("characterIds")] IReadOnlyList<Guid> CharacterIds);

public record StarterItemsGranted(
    [property: JsonPropertyName("playerId")] Guid PlayerId,
    [property: JsonPropertyName("itemIds")] IReadOnlyList<Guid> ItemIds);
