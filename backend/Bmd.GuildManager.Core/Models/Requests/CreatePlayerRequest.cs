using System.Text.Json.Serialization;

namespace Bmd.GuildManager.Core.Models.Requests;

public record CreatePlayerRequest(
    [property: JsonPropertyName("guildName")] string GuildName);
