using Bmd.GuildManager.Core.Models;

namespace Bmd.GuildManager.Tests.Models;

public class CharacterPowerTests
{
    private static Character BuildCharacter(int level = 1, int strength = 5, int luck = 5, int endurance = 5) =>
        Character.Create(Guid.NewGuid(), "Test", level, strength, luck, endurance);

    private static Item BuildItem(int strengthBonus = 0, int luckBonus = 0, int enduranceBonus = 0) =>
        new(Guid.NewGuid(), "Item", DifficultyTier.Novice, "Common",
            StrengthBonus: strengthBonus, LuckBonus: luckBonus, EnduranceBonus: enduranceBonus,
            BasePrice: 10, Status: ItemStatus.Equipped,
            TransferTargetId: null, TransferStartedAt: null);

    // --- BasePower ---

    [Fact]
    public void BasePower_ReflectsStatsAndLevelBonus()
    {
        // 5 + 5 + 5 + (1 × 2) = 17
        var character = BuildCharacter(level: 1, strength: 5, luck: 5, endurance: 5);
        Assert.Equal(17, character.BasePower);
    }

    [Fact]
    public void BasePower_IgnoresEquipment()
    {
        var character = BuildCharacter(level: 1, strength: 5, luck: 5, endurance: 5)
            with { Equipment = [BuildItem(strengthBonus: 10, luckBonus: 10, enduranceBonus: 10)] };
        Assert.Equal(17, character.BasePower);
    }

    // --- TotalPower ---

    [Fact]
    public void TotalPower_NoEquipment_EqualToBasePower()
    {
        var character = BuildCharacter(level: 1, strength: 5, luck: 5, endurance: 5);
        Assert.Equal(character.BasePower, character.TotalPower);
    }

    [Fact]
    public void TotalPower_WithEquipment_AddsBonuses()
    {
        // BasePower = 17; one item: +2 strength, +1 luck, +3 endurance = +6
        var character = BuildCharacter(level: 1, strength: 5, luck: 5, endurance: 5)
            with { Equipment = [BuildItem(strengthBonus: 2, luckBonus: 1, enduranceBonus: 3)] };
        Assert.Equal(23, character.TotalPower);
    }

    [Fact]
    public void TotalPower_MultipleItems_SumsAllBonuses()
    {
        // BasePower = 17; two items: +1+0+0 and +0+2+1 = +4 total bonus
        var character = BuildCharacter(level: 1, strength: 5, luck: 5, endurance: 5)
            with
            {
                Equipment =
                [
                    BuildItem(strengthBonus: 1),
                    BuildItem(luckBonus: 2, enduranceBonus: 1)
                ]
            };
        Assert.Equal(21, character.TotalPower);
    }
}
