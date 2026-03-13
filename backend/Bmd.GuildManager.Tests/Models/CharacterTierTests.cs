using Bmd.GuildManager.Core.Constants;
using Bmd.GuildManager.Core.Models;

namespace Bmd.GuildManager.Tests.Models;

public class CharacterTierTests
{
    // Builds a character with a precise TotalPower value.
    // Uses min BasePower (3+3+3+(1×2) = 11) and assigns the remainder as StrengthBonus on one item.
    private static Character CharacterWithTotalPower(int targetTotalPower)
    {
        var character = Character.Create(Guid.NewGuid(), "Test", level: 1,
            strength: GameConstants.MinStatValue,
            luck: GameConstants.MinStatValue,
            endurance: GameConstants.MinStatValue); // BasePower = 11

        var bonus = targetTotalPower - character.BasePower;
        if (bonus <= 0)
            return character;

        var item = new Item(Guid.NewGuid(), "Test Item", DifficultyTier.Novice, "Common",
            StrengthBonus: bonus, LuckBonus: 0, EnduranceBonus: 0,
            BasePrice: 10, Status: ItemStatus.Equipped,
            TransferTargetId: null, TransferStartedAt: null);

        return character with { Equipment = [item] };
    }

    // Covers every tier boundary using exact TotalPower values.
    // Thresholds: Apprentice ≥ 20, Veteran ≥ 40, Elite ≥ 80, Legendary ≥ 160.
    [Theory]
    [InlineData(11,  DifficultyTier.Novice)]      // min BasePower, no equipment
    [InlineData(19,  DifficultyTier.Novice)]      // just below Apprentice threshold
    [InlineData(20,  DifficultyTier.Apprentice)]  // at Apprentice threshold
    [InlineData(39,  DifficultyTier.Apprentice)]  // just below Veteran threshold
    [InlineData(40,  DifficultyTier.Veteran)]     // at Veteran threshold
    [InlineData(79,  DifficultyTier.Veteran)]     // just below Elite threshold
    [InlineData(80,  DifficultyTier.Elite)]       // at Elite threshold
    [InlineData(159, DifficultyTier.Elite)]       // just below Legendary threshold
    [InlineData(160, DifficultyTier.Legendary)]   // at Legendary threshold
    [InlineData(320, DifficultyTier.Legendary)]   // fully-maxed Legendary reference (2× entry threshold)
    public void CalculateTier_MapsToCorrectTierByTotalPower(int totalPower, DifficultyTier expected)
    {
        var character = CharacterWithTotalPower(totalPower);
        Assert.Equal(expected, character.CalculateTier());
    }

    [Fact]
    public void CalculateTier_StarterCharacter_ReturnsNovice()
    {
        // Default starter: 5+5+5+(1×2) = 17 TotalPower — below the Apprentice threshold of 20.
        var character = Character.Create(Guid.NewGuid(), "Test",
            level: 1, strength: 5, luck: 5, endurance: 5);
        Assert.Equal(DifficultyTier.Novice, character.CalculateTier());
    }

    [Fact]
    public void CalculateTier_StarterCharacterWithMinEquipmentBonus_ReturnsApprentice()
    {
        // BasePower 17 + StrengthBonus 3 = TotalPower 20 — exactly at the Apprentice threshold.
        var character = Character.Create(Guid.NewGuid(), "Test",
            level: 1, strength: 5, luck: 5, endurance: 5);
        var item = new Item(Guid.NewGuid(), "Test Item", DifficultyTier.Novice, "Common",
            StrengthBonus: 3, LuckBonus: 0, EnduranceBonus: 0,
            BasePrice: 10, Status: ItemStatus.Equipped,
            TransferTargetId: null, TransferStartedAt: null);
        var equipped = character with { Equipment = [item] };
        Assert.Equal(DifficultyTier.Apprentice, equipped.CalculateTier());
    }

    [Fact]
    public void CalculateTier_MaxStatsMaxLevelNoEquipment_ReturnsVeteran()
    {
        // Level 20, max stats: BasePower = 10+10+10+(20×2) = 70 TotalPower.
        // 40 ≤ 70 < 80 — Veteran. Reaching Elite or Legendary requires equipment stat bonuses.
        var character = Character.Create(Guid.NewGuid(), "Test",
            level: 20, strength: 10, luck: 10, endurance: 10);
        Assert.Equal(DifficultyTier.Veteran, character.CalculateTier());
    }
}

