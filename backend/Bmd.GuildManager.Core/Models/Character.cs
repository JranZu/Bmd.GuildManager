using System.Text.Json.Serialization;
using Bmd.GuildManager.Core.Constants;

namespace Bmd.GuildManager.Core.Models;

public record Character(
	[property: JsonPropertyName("id")]                    string Id,
	[property: JsonPropertyName("characterId")]           Guid CharacterId,
	[property: JsonPropertyName("playerId")]              Guid PlayerId,
	[property: JsonPropertyName("name")]                  string Name,
	[property: JsonPropertyName("level")]                 int Level,
	[property: JsonPropertyName("strength")]              int Strength,
	[property: JsonPropertyName("luck")]                  int Luck,
	[property: JsonPropertyName("endurance")]             int Endurance,
	[property: JsonPropertyName("status")]                CharacterStatus Status,
	[property: JsonPropertyName("equipment")]             IReadOnlyList<Item> Equipment,
	[property: JsonPropertyName("xp")]                   int Xp,
	[property: JsonPropertyName("activeQuestSnapshot")]  ActiveQuestSnapshot? ActiveQuestSnapshot)
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
			Id:                   characterId.ToString(),
			CharacterId:          characterId,
			PlayerId:             playerId,
			Name:                 name,
			Level:                level,
			Strength:             strength,
			Luck:                 luck,
			Endurance:            endurance,
			Status:               CharacterStatus.Idle,
			Equipment:            [],
			Xp:                   0,
			ActiveQuestSnapshot:  null);
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
			Id:                   characterId.ToString(),
			CharacterId:          characterId,
			PlayerId:             playerId,
			Name:                 name,
			Level:                level,
			Strength:             strength,
			Luck:                 luck,
			Endurance:            endurance,
			Status:               CharacterStatus.Idle,
			Equipment:            [],
			Xp:                   0,
			ActiveQuestSnapshot:  null);
	}

	public Character WithXpApplied(int xpToAdd)
	{
		var newXp = Xp + xpToAdd;
		var newLevel = Level;

		while (newLevel < GameConstants.MaxLevel &&
		       newLevel - 1 < GameConstants.XpThresholds.Length &&
		       newXp >= GameConstants.XpThresholds[newLevel - 1])
		{
			newLevel++;
		}

		return this with { Xp = newXp, Level = newLevel };
	}
}

public record ActiveQuestSnapshot(
	[property: JsonPropertyName("questId")]               Guid QuestId,
	[property: JsonPropertyName("name")]                  string Name,
	[property: JsonPropertyName("description")]           string Description,
	[property: JsonPropertyName("tier")]                  string Tier,
	[property: JsonPropertyName("estimatedCompletionAt")] DateTimeOffset EstimatedCompletionAt);
