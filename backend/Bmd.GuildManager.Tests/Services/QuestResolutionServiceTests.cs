using Bmd.GuildManager.Core.Constants;
using Bmd.GuildManager.Core.Models;
using Bmd.GuildManager.Core.Services;
using Bmd.GuildManager.Tests.Functions;

namespace Bmd.GuildManager.Tests.Services;

public class QuestResolutionServiceTests
{
    // --- DetermineOutcomeFromRatio boundary tests ---

    [Theory]
    [InlineData(1.50, QuestStatus.CriticalSuccess)]  // exact boundary
    [InlineData(2.00, QuestStatus.CriticalSuccess)]  // well above
    [InlineData(1.49, QuestStatus.Success)]          // just below CriticalSuccess
    [InlineData(1.00, QuestStatus.Success)]          // exact boundary
    [InlineData(0.99, QuestStatus.Failure)]          // just below Success
    [InlineData(0.60, QuestStatus.Failure)]          // exact boundary
    [InlineData(0.59, QuestStatus.CatastrophicFailure)] // just below Failure
    [InlineData(0.00, QuestStatus.CatastrophicFailure)] // minimum
    public void DetermineOutcomeFromRatio_ReturnsCorrectOutcome(
        double ratio, QuestStatus expectedOutcome)
    {
        Assert.Equal(expectedOutcome, QuestResolutionService.DetermineOutcomeFromRatio(ratio));
    }

    // --- DetermineOutcome applies jitter ---

    [Fact]
    public void DetermineOutcome_AppliesJitter_CanLowerOutcome()
    {
        // teamPower=100, difficulty=100 ? base ratio=1.0 (Success boundary)
        // jitter=0.75 ? effectivePower=75 ? ratio=0.75 ? Failure
        var service = new QuestResolutionService(new FakeRandomProvider(0.0)); // 0.0 maps to 0.75 min
        var outcome = service.DetermineOutcome(teamPower: 100, difficultyRating: 100);
        Assert.Equal(QuestStatus.Failure, outcome);
    }

    [Fact]
    public void DetermineOutcome_AppliesJitter_CanRaiseOutcome()
    {
        // teamPower=100, difficulty=100 ? base ratio=1.0
        // jitter=1.25 ? effectivePower=125 ? ratio=1.25 ? Success (not CriticalSuccess)
        var service = new QuestResolutionService(new FakeRandomProvider(1.0)); // 1.0 maps to 1.25 max
        var outcome = service.DetermineOutcome(teamPower: 100, difficultyRating: 100);
        Assert.Equal(QuestStatus.Success, outcome);
    }

    // --- CalculateTeamPower ---

    [Fact]
    public void CalculateTeamPower_SumsStatsAndLevelBonus()
    {
        var characters = new List<Character>
        {
            Character.Create(Guid.NewGuid(), "Alpha", level: 1,
                strength: 5, luck: 5, endurance: 5),
            Character.Create(Guid.NewGuid(), "Beta", level: 3,
                strength: 4, luck: 4, endurance: 4)
        };

        // Alpha: TotalPower = 5+5+5+(1x2)+0 = 17
        // Beta:  TotalPower = 4+4+4+(3x2)+0 = 18
        // Total: 35
        Assert.Equal(35, QuestResolutionService.CalculateTeamPower(characters));
    }

    [Fact]
    public void CalculateTeamPower_IncludesEquipmentStatBonuses()
    {
        static Item MakeItem(int str, int lck, int end) =>
            new(Guid.NewGuid(), "Item", DifficultyTier.Novice, "Common",
                StrengthBonus: str, LuckBonus: lck, EnduranceBonus: end,
                BasePrice: 10, Status: ItemStatus.Equipped,
                TransferTargetId: null, TransferStartedAt: null);

        var characters = new List<Character>
        {
            Character.Create(Guid.NewGuid(), "Alpha", level: 1,
                strength: 5, luck: 5, endurance: 5)
                with { Equipment = [MakeItem(2, 1, 0)] },  // +3 bonus
            Character.Create(Guid.NewGuid(), "Beta", level: 3,
                strength: 4, luck: 4, endurance: 4)
                with { Equipment = [MakeItem(0, 0, 4)] }   // +4 bonus
        };

        // Alpha: BasePower=17, equipment bonus=3 → TotalPower=20
        // Beta:  BasePower=18, equipment bonus=4 → TotalPower=22
        // Total: 42
        Assert.Equal(42, QuestResolutionService.CalculateTeamPower(characters));
    }

    // --- RollDeath ---

    [Theory]
    [InlineData(QuestStatus.CriticalSuccess,     0.009, true)]   // just below 1% threshold ? dies
    [InlineData(QuestStatus.CriticalSuccess,     0.011, false)]  // just above ? survives
    [InlineData(QuestStatus.Success,             0.019, true)]
    [InlineData(QuestStatus.Success,             0.021, false)]
    [InlineData(QuestStatus.Failure,             0.199, true)]
    [InlineData(QuestStatus.Failure,             0.201, false)]
    [InlineData(QuestStatus.CatastrophicFailure, 0.599, true)]
    [InlineData(QuestStatus.CatastrophicFailure, 0.601, false)]
    public void RollDeath_ReturnsCorrectResult(
        QuestStatus outcome, double roll, bool expectedDied)
    {
        var service = new QuestResolutionService(new FakeRandomProvider(roll));
        Assert.Equal(expectedDied, service.RollDeath(outcome));
    }

    // --- CalculateXpAwarded ---

    [Theory]
    [InlineData(DifficultyTier.Novice,     QuestStatus.Success,            25)]
    [InlineData(DifficultyTier.Novice,     QuestStatus.Failure,            10)]
    [InlineData(DifficultyTier.Novice,     QuestStatus.CatastrophicFailure, 5)]
    [InlineData(DifficultyTier.Apprentice, QuestStatus.Success,            60)]
    [InlineData(DifficultyTier.Apprentice, QuestStatus.Failure,            20)]
    [InlineData(DifficultyTier.Veteran,    QuestStatus.Success,           120)]
    [InlineData(DifficultyTier.Veteran,    QuestStatus.Failure,            40)]
    [InlineData(DifficultyTier.Elite,      QuestStatus.Success,           250)]
    [InlineData(DifficultyTier.Elite,      QuestStatus.Failure,            80)]
    [InlineData(DifficultyTier.Legendary,  QuestStatus.Success,           500)]
    [InlineData(DifficultyTier.Legendary,  QuestStatus.Failure,           150)]
    public void CalculateXpAwarded_NonCritical_ReturnsFlat(
        DifficultyTier tier, QuestStatus outcome, int expectedXp)
    {
        var service = new QuestResolutionService(new FakeRandomProvider(1.0));
        Assert.Equal(expectedXp, service.CalculateXpAwarded(outcome, tier, 1.0));
    }

    [Fact]
    public void CalculateXpAwarded_CriticalSuccess_ScalesByRatio()
    {
        // Novice baseXp=25, ratio=1.60, jitter midpoint=1.0 (FakeRandomProvider returns 1.0 ? maps to 1.0 in 0.9–1.1 range)
        // overageMultiplier = min(1.60, 2.0) = 1.60
        // jitter: NextDouble(0.9, 1.1) with t=1.0 ? 0.9 + (1.0 × 0.2) = 1.1
        // xp = round(25 × 1.60 × 1.1) = round(44.0) = 44
        var service = new QuestResolutionService(new FakeRandomProvider(1.0));
        var xp = service.CalculateXpAwarded(QuestStatus.CriticalSuccess, DifficultyTier.Novice, teamPowerRatio: 1.60);
        Assert.Equal(44, xp);
    }

    [Fact]
    public void CalculateXpAwarded_CriticalSuccess_CapsMultiplierAtTwo()
    {
        // ratio=3.0 ? capped at 2.0
        // jitter t=1.0 ? 1.1
        // xp = round(25 × 2.0 × 1.1) = round(55.0) = 55
        var service = new QuestResolutionService(new FakeRandomProvider(1.0));
        var xp = service.CalculateXpAwarded(QuestStatus.CriticalSuccess, DifficultyTier.Novice, teamPowerRatio: 3.0);
        Assert.Equal(55, xp);
    }

    // --- CalculateGoldAwarded ---

    [Theory]
    [InlineData(QuestStatus.Failure,             DifficultyTier.Novice, 0)]
    [InlineData(QuestStatus.CatastrophicFailure, DifficultyTier.Novice, 0)]
    public void CalculateGoldAwarded_NoRewardOutcomes_ReturnsZero(
        QuestStatus outcome, DifficultyTier tier, int expected)
    {
        var service = new QuestResolutionService(new FakeRandomProvider(0.5));
        Assert.Equal(expected, service.CalculateGoldAwarded(outcome, tier, 1.0));
    }

    [Theory]
    [InlineData(QuestStatus.Success,         DifficultyTier.Novice,      15, 30)]
    [InlineData(QuestStatus.Success,         DifficultyTier.Apprentice,  40, 70)]
    [InlineData(QuestStatus.Success,         DifficultyTier.Veteran,     80, 140)]
    [InlineData(QuestStatus.Success,         DifficultyTier.Elite,      175, 300)]
    [InlineData(QuestStatus.Success,         DifficultyTier.Legendary,  350, 600)]
    public void CalculateGoldAwarded_Success_FallsWithinRange(
        QuestStatus outcome, DifficultyTier tier, int expectedMin, int expectedMax)
    {
        // FakeRandomProvider.NextInt returns midpoint, which is always within range
        var service = new QuestResolutionService(new FakeRandomProvider(0.5));
        var gold = service.CalculateGoldAwarded(outcome, tier, 1.0);
        Assert.InRange(gold, expectedMin, expectedMax);
    }

    [Fact]
    public void CalculateGoldAwarded_CriticalSuccess_AppliesOverageMultiplier()
    {
        // Novice Success range: 15–30, midpoint (baseGold) = 22.5
        // ratio=1.60, overageMultiplier = min(1.60, 2.0) = 1.60
        // scaled = round(22.5 × 1.60) = round(36.0) = 36
        // CriticalSuccess range: 30–60, clamp(36, 30, 60) = 36
        var service = new QuestResolutionService(new FakeRandomProvider(0.5));
        var gold = service.CalculateGoldAwarded(QuestStatus.CriticalSuccess, DifficultyTier.Novice, teamPowerRatio: 1.60);
        Assert.Equal(36, gold);
    }

    [Fact]
    public void CalculateGoldAwarded_CriticalSuccess_CapsMultiplierAtTwo()
    {
        // Novice Success range: 15–30, midpoint = 22.5
        // ratio=3.0 → capped at 2.0
        // scaled = round(22.5 × 2.0) = round(45.0) = 45
        // CriticalSuccess range: 30–60, clamp(45, 30, 60) = 45
        var service = new QuestResolutionService(new FakeRandomProvider(0.5));
        var gold = service.CalculateGoldAwarded(QuestStatus.CriticalSuccess, DifficultyTier.Novice, teamPowerRatio: 3.0);
        Assert.Equal(45, gold);
    }

    [Fact]
    public void CalculateGoldAwarded_CriticalSuccess_ClampsToMinRange()
    {
        // Novice Success range: 15–30, midpoint = 22.5
        // ratio=1.0 → overageMultiplier = 1.0
        // scaled = round(22.5 × 1.0) = round(22.5) = 22
        // CriticalSuccess range: 30–60, clamp(22, 30, 60) = 30 (clamped to min)
        var service = new QuestResolutionService(new FakeRandomProvider(0.5));
        var gold = service.CalculateGoldAwarded(QuestStatus.CriticalSuccess, DifficultyTier.Novice, teamPowerRatio: 1.0);
        Assert.Equal(30, gold);
    }

    // --- IsLootEligible ---

    [Theory]
    [InlineData(QuestStatus.CriticalSuccess,     true)]
    [InlineData(QuestStatus.Success,             true)]
    [InlineData(QuestStatus.Failure,             false)]
    [InlineData(QuestStatus.CatastrophicFailure, false)]
    public void IsLootEligible_ReturnsCorrectValue(QuestStatus outcome, bool expected)
    {
        Assert.Equal(expected, QuestResolutionService.IsLootEligible(outcome));
    }
}
