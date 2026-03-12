using Bmd.GuildManager.Core.Models;
using Bmd.GuildManager.Core.Services;

namespace Bmd.GuildManager.Tests.Services;

public class QuestFactoryTests
{
    private static readonly string[] AllTiers =
        ["Novice", "Apprentice", "Veteran", "Elite", "Legendary"];

    // --- Structure tests ---

    [Theory]
    [InlineData("Novice")]
    [InlineData("Apprentice")]
    [InlineData("Veteran")]
    [InlineData("Elite")]
    [InlineData("Legendary")]
    public void Generate_QuestId_IsNotEmpty(string tier)
    {
        var quest = QuestFactory.Generate(tier);
        Assert.NotEqual(Guid.Empty, quest.QuestId);
    }

    [Theory]
    [InlineData("Novice")]
    [InlineData("Apprentice")]
    [InlineData("Veteran")]
    [InlineData("Elite")]
    [InlineData("Legendary")]
    public void Generate_Id_MatchesQuestId(string tier)
    {
        var quest = QuestFactory.Generate(tier);
        Assert.Equal(quest.QuestId.ToString(), quest.Id);
    }

    [Theory]
    [InlineData("Novice")]
    [InlineData("Apprentice")]
    [InlineData("Veteran")]
    [InlineData("Elite")]
    [InlineData("Legendary")]
    public void Generate_Status_IsAvailable(string tier)
    {
        var quest = QuestFactory.Generate(tier);
        Assert.Equal(QuestStatus.Available, quest.Status);
    }

    [Theory]
    [InlineData("Novice")]
    [InlineData("Apprentice")]
    [InlineData("Veteran")]
    [InlineData("Elite")]
    [InlineData("Legendary")]
    public void Generate_Tier_MatchesRequested(string tier)
    {
        var quest = QuestFactory.Generate(tier);
        Assert.Equal(tier, quest.Tier);
    }

    [Theory]
    [InlineData("Novice")]
    [InlineData("Apprentice")]
    [InlineData("Veteran")]
    [InlineData("Elite")]
    [InlineData("Legendary")]
    public void Generate_PlayerId_IsNull(string tier)
    {
        var quest = QuestFactory.Generate(tier);
        Assert.Null(quest.PlayerId);
    }

    [Theory]
    [InlineData("Novice")]
    [InlineData("Apprentice")]
    [InlineData("Veteran")]
    [InlineData("Elite")]
    [InlineData("Legendary")]
    public void Generate_AssignedCharacterIds_IsEmpty(string tier)
    {
        var quest = QuestFactory.Generate(tier);
        Assert.Empty(quest.AssignedCharacterIds);
    }

    [Theory]
    [InlineData("Novice")]
    [InlineData("Apprentice")]
    [InlineData("Veteran")]
    [InlineData("Elite")]
    [InlineData("Legendary")]
    public void Generate_StartedAt_IsNull(string tier)
    {
        var quest = QuestFactory.Generate(tier);
        Assert.Null(quest.StartedAt);
    }

    [Theory]
    [InlineData("Novice")]
    [InlineData("Apprentice")]
    [InlineData("Veteran")]
    [InlineData("Elite")]
    [InlineData("Legendary")]
    public void Generate_EstimatedCompletionAt_IsNull(string tier)
    {
        var quest = QuestFactory.Generate(tier);
        Assert.Null(quest.EstimatedCompletionAt);
    }

    [Theory]
    [InlineData("Novice")]
    [InlineData("Apprentice")]
    [InlineData("Veteran")]
    [InlineData("Elite")]
    [InlineData("Legendary")]
    public void Generate_CreatedAt_IsRecentUtc(string tier)
    {
        var before = DateTimeOffset.UtcNow;
        var quest = QuestFactory.Generate(tier);
        var after = DateTimeOffset.UtcNow;

        Assert.InRange(quest.CreatedAt, before, after);
    }

    // --- Tier range tests ---

    [Theory]
    [InlineData("Novice",     10,  30,  1, 1,  60,   180)]
    [InlineData("Apprentice", 25,  60,  1, 2,  120,  300)]
    [InlineData("Veteran",    50,  100, 2, 3,  240,  600)]
    [InlineData("Elite",      90,  160, 3, 4,  480,  900)]
    [InlineData("Legendary",  150, 250, 4, 5,  600,  1800)]
    public void Generate_TierParameters_AreWithinRange(
        string tier,
        int minDiff, int maxDiff,
        int minAdv, int maxAdv,
        int minDur, int maxDur)
    {
        // Run multiple times to exercise randomness
        for (var i = 0; i < 20; i++)
        {
            var quest = QuestFactory.Generate(tier);

            Assert.InRange(quest.DifficultyRating, minDiff, maxDiff);
            Assert.InRange(quest.RequiredAdventurers, minAdv, maxAdv);
            Assert.InRange(quest.DurationSeconds, minDur, maxDur);
        }
    }

    // --- String content tests ---

    [Theory]
    [InlineData("Novice")]
    [InlineData("Apprentice")]
    [InlineData("Veteran")]
    [InlineData("Elite")]
    [InlineData("Legendary")]
    public void Generate_Name_IsNotNullOrWhiteSpace(string tier)
    {
        for (var i = 0; i < 20; i++)
        {
            var quest = QuestFactory.Generate(tier);
            Assert.False(string.IsNullOrWhiteSpace(quest.Name));
        }
    }

    [Theory]
    [InlineData("Novice")]
    [InlineData("Apprentice")]
    [InlineData("Veteran")]
    [InlineData("Elite")]
    [InlineData("Legendary")]
    public void Generate_Description_IsNotNullOrWhiteSpace(string tier)
    {
        for (var i = 0; i < 20; i++)
        {
            var quest = QuestFactory.Generate(tier);
            Assert.False(string.IsNullOrWhiteSpace(quest.Description));
        }
    }

    [Theory]
    [InlineData("Novice")]
    [InlineData("Apprentice")]
    [InlineData("Veteran")]
    [InlineData("Elite")]
    [InlineData("Legendary")]
    public void Generate_QuestType_IsNotNullOrWhiteSpace(string tier)
    {
        for (var i = 0; i < 20; i++)
        {
            var quest = QuestFactory.Generate(tier);
            Assert.False(string.IsNullOrWhiteSpace(quest.QuestType));
        }
    }

    [Theory]
    [InlineData("Novice")]
    [InlineData("Apprentice")]
    [InlineData("Veteran")]
    [InlineData("Elite")]
    [InlineData("Legendary")]
    public void Generate_RiskLevel_IsValidValue(string tier)
    {
        string[] validRiskLevels = ["Low", "Medium", "High"];

        for (var i = 0; i < 20; i++)
        {
            var quest = QuestFactory.Generate(tier);
            Assert.Contains(quest.RiskLevel, validRiskLevels);
        }
    }

    // --- AllTiers helper ---

    [Fact]
    public void AllTiers_ReturnsFiveTiers()
    {
        Assert.Equal(5, QuestFactory.AllTiers().Count);
    }

    [Fact]
    public void AllTiers_ContainsAllExpectedTiers()
    {
        var tiers = QuestFactory.AllTiers();
        Assert.Contains("Novice", tiers);
        Assert.Contains("Apprentice", tiers);
        Assert.Contains("Veteran", tiers);
        Assert.Contains("Elite", tiers);
        Assert.Contains("Legendary", tiers);
    }

    // --- Unknown tier guard ---

    [Fact]
    public void Generate_UnknownTier_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => QuestFactory.Generate("Unknown"));
    }
}
