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
		int endurance) =>
		CreateWithId(Guid.NewGuid(), playerId, name, level, strength, luck, endurance);

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
			   newXp >= GameConstants.XpThresholds[newLevel - 1])
		{
			newLevel++;
		}

		return this with { Xp = newXp, Level = newLevel };
	}

	/// <summary>
	/// Base combat contribution of this character from stats and level alone, before equipment bonuses (GDD §6).
	/// </summary>
	public int BasePower => Strength + Luck + Endurance + (Level * 2);

	/// <summary>
	/// Total combat power of this character, including all equipped item stat bonuses (GDD §6).
	/// </summary>
	public int TotalPower => BasePower + Equipment.Sum(item => item.StrengthBonus + item.LuckBonus + item.EnduranceBonus);

	/// <summary>
	/// Derives the character's tier by comparing TotalPower against exponential thresholds (GDD §4, §6).
	/// Each threshold doubles from the previous, mirroring the quest difficulty doubling pattern.
	/// </summary>
	public DifficultyTier CalculateTier()
	{
		var thresholds = GameConstants.TierTotalPowerThresholds;
		for (var i = thresholds.Length - 1; i >= 0; i--)
		{
			if (TotalPower >= thresholds[i])
				return (DifficultyTier)(i + 2);
		}
		return DifficultyTier.Novice;
	}
}

public record ActiveQuestSnapshot(
	[property: JsonPropertyName("questId")]               Guid QuestId,
	[property: JsonPropertyName("name")]                  string Name,
	[property: JsonPropertyName("description")]           string Description,
	[property: JsonPropertyName("tier")]                  DifficultyTier Tier,
	[property: JsonPropertyName("estimatedCompletionAt")] DateTimeOffset EstimatedCompletionAt);
