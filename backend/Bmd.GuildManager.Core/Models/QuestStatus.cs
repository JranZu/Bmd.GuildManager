using System.Text.Json.Serialization;

namespace Bmd.GuildManager.Core.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum QuestStatus
{
    Available,
    InProgress,

    // Terminal states — only appear in Blob archive, never in Cosmos DB
    CriticalSuccess,
    Success,
    Failure,
    CatastrophicFailure
}
