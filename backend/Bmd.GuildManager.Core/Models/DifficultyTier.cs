using System.Text.Json.Serialization;

namespace Bmd.GuildManager.Core.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum DifficultyTier
{
	Novice     = 1,
	Apprentice = 2,
	Veteran    = 3,
	Elite      = 4,
	Legendary  = 5
}
