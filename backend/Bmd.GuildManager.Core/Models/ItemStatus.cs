using System.Text.Json.Serialization;

namespace Bmd.GuildManager.Core.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ItemStatus
{
	// Stable states
	Stashed,
	Equipped,
	ForSale,

	// Transitional states — set at transfer initiation, cleared on completion
	Equipping,
	Unequipping,
	Selling,
	Returning,

	// Terminal states — item is removed from operational store when these are reached
	Sold,
	Discarded,
	Lost
}
