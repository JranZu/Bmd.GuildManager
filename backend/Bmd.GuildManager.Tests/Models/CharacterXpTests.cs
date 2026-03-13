using Bmd.GuildManager.Core.Constants;
using Bmd.GuildManager.Core.Models;

namespace Bmd.GuildManager.Tests.Models;

public class CharacterXpTests
{
    private static Character BuildCharacter(int level, int xp) =>
        Character.Create(Guid.NewGuid(), "Test", level, 5, 5, 5) with { Xp = xp };

    [Fact]
    public void WithXpApplied_BelowThreshold_DoesNotLevelUp()
    {
        var character = BuildCharacter(level: 1, xp: 0);
        var result = character.WithXpApplied(99); // threshold is 100
        Assert.Equal(1, result.Level);
        Assert.Equal(99, result.Xp);
    }

    [Fact]
    public void WithXpApplied_ExactlyAtThreshold_LevelsUp()
    {
        var character = BuildCharacter(level: 1, xp: 0);
        var result = character.WithXpApplied(100);
        Assert.Equal(2, result.Level);
        Assert.Equal(100, result.Xp);
    }

    [Fact]
    public void WithXpApplied_CrossesMultipleThresholds_LevelsUpMultipleTimes()
    {
        // Level 1, 0 XP — award 350 XP
        // Threshold 1→2 = 100, 2→3 = 250
        // 350 >= 100 → Level 2; 350 >= 250 → Level 3; 350 < 500 → stop
        var character = BuildCharacter(level: 1, xp: 0);
        var result = character.WithXpApplied(350);
        Assert.Equal(3, result.Level);
        Assert.Equal(350, result.Xp);
    }

    [Fact]
    public void WithXpApplied_AtMaxLevel_DoesNotExceedCap()
    {
        var character = BuildCharacter(level: GameConstants.MaxLevel, xp: 0);
        var result = character.WithXpApplied(1_000_000);
        Assert.Equal(GameConstants.MaxLevel, result.Level);
    }

    [Fact]
    public void WithXpApplied_DoesNotMutateOriginal()
    {
        var character = BuildCharacter(level: 1, xp: 0);
        var result = character.WithXpApplied(100);
        Assert.Equal(0, character.Xp);
        Assert.Equal(1, character.Level);
        Assert.Equal(100, result.Xp);
        Assert.Equal(2, result.Level);
    }
}
