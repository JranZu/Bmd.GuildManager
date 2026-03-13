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
        // teamPower=100, difficulty=100 → base ratio=1.0 (Success boundary)
        // jitter=0.75 → effectivePower=75 → ratio=0.75 → Failure
        var service = new QuestResolutionService(new FakeRandomProvider(0.0)); // 0.0 maps to 0.75 min
        var outcome = service.DetermineOutcome(teamPower: 100, difficultyRating: 100);
        Assert.Equal(QuestStatus.Failure, outcome);
    }

    [Fact]
    public void DetermineOutcome_AppliesJitter_CanRaiseOutcome()
    {
        // teamPower=100, difficulty=100 → base ratio=1.0
        // jitter=1.25 → effectivePower=125 → ratio=1.25 → Success (not CriticalSuccess)
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

        // Alpha: 5+5+5+(1×2) = 17
        // Beta:  4+4+4+(3×2) = 18
        // Total: 35
        Assert.Equal(35, QuestResolutionService.CalculateTeamPower(characters));
    }

    // --- RollDeath ---

    [Theory]
    [InlineData(QuestStatus.CriticalSuccess,     0.009, true)]   // just below 1% threshold → dies
    [InlineData(QuestStatus.CriticalSuccess,     0.011, false)]  // just above → survives
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
    [InlineData("Novice",     QuestStatus.Success,            25)]
    [InlineData("Novice",     QuestStatus.Failure,            10)]
    [InlineData("Novice",     QuestStatus.CatastrophicFailure, 5)]
    [InlineData("Apprentice", QuestStatus.Success,            60)]
    [InlineData("Apprentice", QuestStatus.Failure,            20)]
    [InlineData("Veteran",    QuestStatus.Success,           120)]
    [InlineData("Veteran",    QuestStatus.Failure,            40)]
    [InlineData("Elite",      QuestStatus.Success,           250)]
    [InlineData("Elite",      QuestStatus.Failure,            80)]
    [InlineData("Legendary",  QuestStatus.Success,           500)]
    [InlineData("Legendary",  QuestStatus.Failure,           150)]
    public void CalculateXpAwarded_NonCritical_ReturnsFlat(
        string tier, QuestStatus outcome, int expectedXp)
    {
        var service = new QuestResolutionService(new FakeRandomProvider(1.0));
        Assert.Equal(expectedXp, service.CalculateXpAwarded(outcome, tier, 1.0));
    }

    [Fact]
    public void CalculateXpAwarded_CriticalSuccess_ScalesByRatio()
    {
        // Novice baseXp=25, ratio=1.60, jitter midpoint=1.0 (FakeRandomProvider returns 1.0 → maps to 1.0 in 0.9–1.1 range)
        // overageMultiplier = min(1.60, 2.0) = 1.60
        // jitter: NextDouble(0.9, 1.1) with t=1.0 → 0.9 + (1.0 × 0.2) = 1.1
        // xp = round(25 × 1.60 × 1.1) = round(44.0) = 44
        var service = new QuestResolutionService(new FakeRandomProvider(1.0));
        var xp = service.CalculateXpAwarded(QuestStatus.CriticalSuccess, "Novice", teamPowerRatio: 1.60);
        Assert.Equal(44, xp);
    }

    [Fact]
    public void CalculateXpAwarded_CriticalSuccess_CapsMultiplierAtTwo()
    {
        // ratio=3.0 → capped at 2.0
        // jitter t=1.0 → 1.1
        // xp = round(25 × 2.0 × 1.1) = round(55.0) = 55
        var service = new QuestResolutionService(new FakeRandomProvider(1.0));
        var xp = service.CalculateXpAwarded(QuestStatus.CriticalSuccess, "Novice", teamPowerRatio: 3.0);
        Assert.Equal(55, xp);
    }

    // --- CalculateGoldAwarded ---

    [Theory]
    [InlineData(QuestStatus.Failure,             "Novice", 0)]
    [InlineData(QuestStatus.CatastrophicFailure, "Novice", 0)]
    public void CalculateGoldAwarded_NoRewardOutcomes_ReturnsZero(
        QuestStatus outcome, string tier, int expected)
    {
        var service = new QuestResolutionService(new FakeRandomProvider(0.5));
        Assert.Equal(expected, service.CalculateGoldAwarded(outcome, tier, 1.0));
    }

    [Theory]
    [InlineData(QuestStatus.Success,         "Novice",      15, 30)]
    [InlineData(QuestStatus.CriticalSuccess, "Novice",      30, 60)]
    [InlineData(QuestStatus.Success,         "Apprentice",  40, 70)]
    [InlineData(QuestStatus.CriticalSuccess, "Apprentice",  80, 140)]
    [InlineData(QuestStatus.Success,         "Veteran",     80, 140)]
    [InlineData(QuestStatus.CriticalSuccess, "Veteran",    160, 280)]
    [InlineData(QuestStatus.Success,         "Elite",      175, 300)]
    [InlineData(QuestStatus.CriticalSuccess, "Elite",      350, 600)]
    [InlineData(QuestStatus.Success,         "Legendary",  350, 600)]
    [InlineData(QuestStatus.CriticalSuccess, "Legendary",  700, 1200)]
    public void CalculateGoldAwarded_RewardOutcomes_FallsWithinRange(
        QuestStatus outcome, string tier, int expectedMin, int expectedMax)
    {
        // FakeRandomProvider.NextInt returns midpoint, which is always within range
        var service = new QuestResolutionService(new FakeRandomProvider(0.5));
        var gold = service.CalculateGoldAwarded(outcome, tier, 1.0);
        Assert.InRange(gold, expectedMin, expectedMax);
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
