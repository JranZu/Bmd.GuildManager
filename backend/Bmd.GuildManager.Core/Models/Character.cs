using System.Text.Json.Serialization;

namespace Bmd.GuildManager.Core.Models;

public record Character(
	[property: JsonPropertyName("id")]             string Id,
	[property: JsonPropertyName("characterId")]    Guid CharacterId,
	[property: JsonPropertyName("playerId")]       Guid PlayerId,
	[property: JsonPropertyName("name")]           string Name,
	[property: JsonPropertyName("level")]          int Level,
	[property: JsonPropertyName("strength")]       int Strength,
	[property: JsonPropertyName("luck")]           int Luck,
	[property: JsonPropertyName("endurance")]      int Endurance,
	[property: JsonPropertyName("status")]         CharacterStatus Status,
	[property: JsonPropertyName("equipmentIds")]   IReadOnlyList<Guid> EquipmentIds)
{
	public static Character Create(
		Guid playerId,
		string name,
		int level,
		int strength,
		int luck,
		int endurance)
	{
		var characterId = Guid.NewGuid();
		return new Character(
			Id:           characterId.ToString(),
			CharacterId:  characterId,
			PlayerId:     playerId,
			Name:         name,
			Level:        level,
			Strength:     strength,
			Luck:         luck,
			Endurance:    endurance,
			Status:       CharacterStatus.Idle,
			EquipmentIds: []);
	}

	public static Character CreateWithId(
	Guid characterId,
	Guid playerId,
	string name,
	int level,
	int strength,
	int luck,
	int endurance)
	{
		return new Character(
			Id: characterId.ToString(),
			CharacterId: characterId,
			PlayerId: playerId,
			Name: name,
			Level: level,
			Strength: strength,
			Luck: luck,
			Endurance: endurance,
			Status: CharacterStatus.Idle,
			EquipmentIds: []);
	}
}
