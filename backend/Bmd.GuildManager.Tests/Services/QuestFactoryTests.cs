using Bmd.GuildManager.Core.Models;
using Bmd.GuildManager.Core.Services;
using Bmd.GuildManager.Tests.Functions;

namespace Bmd.GuildManager.Tests.Services;

public class QuestFactoryTests
{
    private readonly QuestFactory _factory = new(new FakeRandomProvider());

    // --- Structure tests ---

    [Theory]
    [InlineData(DifficultyTier.Novice)]
    [InlineData(DifficultyTier.Apprentice)]
    [InlineData(DifficultyTier.Veteran)]
    [InlineData(DifficultyTier.Elite)]
    [InlineData(DifficultyTier.Legendary)]
    public void Generate_QuestId_IsNotEmpty(DifficultyTier tier)
    {
        var quest = _factory.Generate(tier);
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
        var quest = _factory.Generate(tier);
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
        var quest = _factory.Generate(tier);
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
        var quest = _factory.Generate(tier);
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
        var quest = _factory.Generate(tier);
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
        var quest = _factory.Generate(tier);
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
        var quest = _factory.Generate(tier);
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
        var quest = _factory.Generate(tier);
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
        var quest = _factory.Generate(tier);
        var after = DateTimeOffset.UtcNow;

        Assert.InRange(quest.CreatedAt, before, after);
    }

    // --- Tier parameter tests ---

    // FakeRandomProvider.NextInt(min, maxExclusive) returns (min + maxExclusive) / 2
    [Theory]
    [InlineData(DifficultyTier.Novice,      35,  1,  120)]
    [InlineData(DifficultyTier.Apprentice,  90,  2,  210)]
    [InlineData(DifficultyTier.Veteran,     180, 3,  420)]
    [InlineData(DifficultyTier.Elite,       360, 4,  690)]
    [InlineData(DifficultyTier.Legendary,   720, 5,  1200)]
    public void Generate_TierParameters_AreExactForFakeRandom(
        DifficultyTier tier,
        int expectedDifficulty,
        int expectedAdventurers,
        int expectedDuration)
    {
        var quest = _factory.Generate(tier);

        Assert.Equal(expectedDifficulty,  quest.DifficultyRating);
        Assert.Equal(expectedAdventurers, quest.RequiredAdventurers);
        Assert.Equal(expectedDuration,    quest.DurationSeconds);
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
        var quest = _factory.Generate(tier);
        Assert.False(string.IsNullOrWhiteSpace(quest.Name));
    }

    [Theory]
    [InlineData(DifficultyTier.Novice)]
    [InlineData(DifficultyTier.Apprentice)]
    [InlineData(DifficultyTier.Veteran)]
    [InlineData(DifficultyTier.Elite)]
    [InlineData(DifficultyTier.Legendary)]
    public void Generate_Description_IsNotNullOrWhiteSpace(DifficultyTier tier)
    {
        var quest = _factory.Generate(tier);
        Assert.False(string.IsNullOrWhiteSpace(quest.Description));
    }

    // FakeRandomProvider.NextInt(0, 5) = 2 → Rescue
    [Theory]
    [InlineData(DifficultyTier.Novice)]
    [InlineData(DifficultyTier.Apprentice)]
    [InlineData(DifficultyTier.Veteran)]
    [InlineData(DifficultyTier.Elite)]
    [InlineData(DifficultyTier.Legendary)]
    public void Generate_QuestType_IsRescueForFakeRandom(DifficultyTier tier)
    {
        var quest = _factory.Generate(tier);
        Assert.Equal(QuestType.Rescue, quest.QuestType);
    }

    // FakeRandomProvider.NextInt(0, 10) = 5, which is < 8 → Medium
    [Theory]
    [InlineData(DifficultyTier.Novice)]
    [InlineData(DifficultyTier.Apprentice)]
    [InlineData(DifficultyTier.Veteran)]
    [InlineData(DifficultyTier.Elite)]
    [InlineData(DifficultyTier.Legendary)]
    public void Generate_RiskLevel_IsMediumForFakeRandom(DifficultyTier tier)
    {
        var quest = _factory.Generate(tier);
        Assert.Equal(RiskLevel.Medium, quest.RiskLevel);
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
