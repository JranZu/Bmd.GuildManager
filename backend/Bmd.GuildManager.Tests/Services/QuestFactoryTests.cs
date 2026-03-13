using Bmd.GuildManager.Core.Models;
using Bmd.GuildManager.Core.Services;

namespace Bmd.GuildManager.Tests.Services;

public class QuestFactoryTests
{
    private static readonly DifficultyTier[] AllTiers =
        [DifficultyTier.Novice, DifficultyTier.Apprentice, DifficultyTier.Veteran, DifficultyTier.Elite, DifficultyTier.Legendary];

    // --- Structure tests ---

    [Theory]
    [InlineData(DifficultyTier.Novice)]
    [InlineData(DifficultyTier.Apprentice)]
    [InlineData(DifficultyTier.Veteran)]
    [InlineData(DifficultyTier.Elite)]
    [InlineData(DifficultyTier.Legendary)]
    public void Generate_QuestId_IsNotEmpty(DifficultyTier tier)
    {
        var quest = QuestFactory.Generate(tier);
        Assert.NotEqual(Guid.Empty, quest.QuestId);
    }

    [Theory]
    [InlineData(DifficultyTier.Novice)]
    [InlineData(DifficultyTier.Apprentice)]
    [InlineData(DifficultyTier.Veteran)]
    [InlineData(DifficultyTier.Elite)]
    [InlineData(DifficultyTier.Legendary)]
    public void Generate_Id_MatchesQuestId(DifficultyTier tier)
    {
        var quest = QuestFactory.Generate(tier);
        Assert.Equal(quest.QuestId.ToString(), quest.Id);
    }

    [Theory]
    [InlineData(DifficultyTier.Novice)]
    [InlineData(DifficultyTier.Apprentice)]
    [InlineData(DifficultyTier.Veteran)]
    [InlineData(DifficultyTier.Elite)]
    [InlineData(DifficultyTier.Legendary)]
    public void Generate_Status_IsAvailable(DifficultyTier tier)
    {
        var quest = QuestFactory.Generate(tier);
        Assert.Equal(QuestStatus.Available, quest.Status);
    }

    [Theory]
    [InlineData(DifficultyTier.Novice)]
    [InlineData(DifficultyTier.Apprentice)]
    [InlineData(DifficultyTier.Veteran)]
    [InlineData(DifficultyTier.Elite)]
    [InlineData(DifficultyTier.Legendary)]
    public void Generate_Tier_MatchesRequested(DifficultyTier tier)
    {
        var quest = QuestFactory.Generate(tier);
        Assert.Equal(tier, quest.Tier);
    }

    [Theory]
    [InlineData(DifficultyTier.Novice)]
    [InlineData(DifficultyTier.Apprentice)]
    [InlineData(DifficultyTier.Veteran)]
    [InlineData(DifficultyTier.Elite)]
    [InlineData(DifficultyTier.Legendary)]
    public void Generate_PlayerId_IsNull(DifficultyTier tier)
    {
        var quest = QuestFactory.Generate(tier);
        Assert.Null(quest.PlayerId);
    }

    [Theory]
    [InlineData(DifficultyTier.Novice)]
    [InlineData(DifficultyTier.Apprentice)]
    [InlineData(DifficultyTier.Veteran)]
    [InlineData(DifficultyTier.Elite)]
    [InlineData(DifficultyTier.Legendary)]
    public void Generate_AssignedCharacterIds_IsEmpty(DifficultyTier tier)
    {
        var quest = QuestFactory.Generate(tier);
        Assert.Empty(quest.AssignedCharacterIds);
    }

    [Theory]
    [InlineData(DifficultyTier.Novice)]
    [InlineData(DifficultyTier.Apprentice)]
    [InlineData(DifficultyTier.Veteran)]
    [InlineData(DifficultyTier.Elite)]
    [InlineData(DifficultyTier.Legendary)]
    public void Generate_StartedAt_IsNull(DifficultyTier tier)
    {
        var quest = QuestFactory.Generate(tier);
        Assert.Null(quest.StartedAt);
    }

    [Theory]
    [InlineData(DifficultyTier.Novice)]
    [InlineData(DifficultyTier.Apprentice)]
    [InlineData(DifficultyTier.Veteran)]
    [InlineData(DifficultyTier.Elite)]
    [InlineData(DifficultyTier.Legendary)]
    public void Generate_EstimatedCompletionAt_IsNull(DifficultyTier tier)
    {
        var quest = QuestFactory.Generate(tier);
        Assert.Null(quest.EstimatedCompletionAt);
    }

    [Theory]
    [InlineData(DifficultyTier.Novice)]
    [InlineData(DifficultyTier.Apprentice)]
    [InlineData(DifficultyTier.Veteran)]
    [InlineData(DifficultyTier.Elite)]
    [InlineData(DifficultyTier.Legendary)]
    public void Generate_CreatedAt_IsRecentUtc(DifficultyTier tier)
    {
        var before = DateTimeOffset.UtcNow;
        var quest = QuestFactory.Generate(tier);
        var after = DateTimeOffset.UtcNow;

        Assert.InRange(quest.CreatedAt, before, after);
    }

    // --- Tier range tests ---

    [Theory]
    [InlineData(DifficultyTier.Novice,      9,   60,  1, 1,  60,   180)]
    [InlineData(DifficultyTier.Apprentice,  60,  120, 1, 2,  120,  300)]
    [InlineData(DifficultyTier.Veteran,     120, 240, 2, 3,  240,  600)]
    [InlineData(DifficultyTier.Elite,       240, 480, 3, 4,  480,  900)]
    [InlineData(DifficultyTier.Legendary,   480, 960, 4, 5,  600,  1800)]
    public void Generate_TierParameters_AreWithinRange(
        DifficultyTier tier,
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
    [InlineData(DifficultyTier.Novice)]
    [InlineData(DifficultyTier.Apprentice)]
    [InlineData(DifficultyTier.Veteran)]
    [InlineData(DifficultyTier.Elite)]
    [InlineData(DifficultyTier.Legendary)]
    public void Generate_Name_IsNotNullOrWhiteSpace(DifficultyTier tier)
    {
        for (var i = 0; i < 20; i++)
        {
            var quest = QuestFactory.Generate(tier);
            Assert.False(string.IsNullOrWhiteSpace(quest.Name));
        }
    }

    [Theory]
    [InlineData(DifficultyTier.Novice)]
    [InlineData(DifficultyTier.Apprentice)]
    [InlineData(DifficultyTier.Veteran)]
    [InlineData(DifficultyTier.Elite)]
    [InlineData(DifficultyTier.Legendary)]
    public void Generate_Description_IsNotNullOrWhiteSpace(DifficultyTier tier)
    {
        for (var i = 0; i < 20; i++)
        {
            var quest = QuestFactory.Generate(tier);
            Assert.False(string.IsNullOrWhiteSpace(quest.Description));
        }
    }

    [Theory]
    [InlineData(DifficultyTier.Novice)]
    [InlineData(DifficultyTier.Apprentice)]
    [InlineData(DifficultyTier.Veteran)]
    [InlineData(DifficultyTier.Elite)]
    [InlineData(DifficultyTier.Legendary)]
    public void Generate_QuestType_IsNotNullOrWhiteSpace(DifficultyTier tier)
    {
        for (var i = 0; i < 20; i++)
        {
            var quest = QuestFactory.Generate(tier);
            Assert.False(string.IsNullOrWhiteSpace(quest.QuestType));
        }
    }

    [Theory]
    [InlineData(DifficultyTier.Novice)]
    [InlineData(DifficultyTier.Apprentice)]
    [InlineData(DifficultyTier.Veteran)]
    [InlineData(DifficultyTier.Elite)]
    [InlineData(DifficultyTier.Legendary)]
    public void Generate_RiskLevel_IsValidValue(DifficultyTier tier)
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
        Assert.Contains(DifficultyTier.Novice,      tiers);
        Assert.Contains(DifficultyTier.Apprentice,  tiers);
        Assert.Contains(DifficultyTier.Veteran,     tiers);
        Assert.Contains(DifficultyTier.Elite,       tiers);
        Assert.Contains(DifficultyTier.Legendary,   tiers);
    }

    }
