using System.Text.Json.Serialization;

namespace Bmd.GuildManager.Core.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RiskLevel
{
    Low,
    Medium,
    High
}
