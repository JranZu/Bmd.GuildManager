using Bmd.GuildManager.Core.Abstractions;
using Bmd.GuildManager.Core.Constants;
using Bmd.GuildManager.Core.Models;

namespace Bmd.GuildManager.Core.Services;

public class QuestResolutionService(IRandomProvider random)
{
    // --- Outcome determination ---

    /// <summary>
    /// Determines the quest outcome by applying ±25% jitter to team power,
    /// then comparing the effective ratio against threshold constants.
    /// </summary>
    public QuestStatus DetermineOutcome(int teamPower, int difficultyRating)
    {
        var jitter = random.NextDouble(0.75, 1.25);
        var effectivePower = teamPower * jitter;
        var ratio = effectivePower / difficultyRating;

        return DetermineOutcomeFromRatio(ratio);
    }

    /// <summary>
    /// Determines outcome from a pre-computed ratio. Useful for tests
    /// that need to assert exact boundary behavior without fighting randomness.
    /// </summary>
    public static QuestStatus DetermineOutcomeFromRatio(double ratio) => ratio switch
    {
        >= GameConstants.CriticalSuccessThreshold => QuestStatus.CriticalSuccess,
        >= GameConstants.SuccessThreshold          => QuestStatus.Success,
        >= GameConstants.FailureThreshold          => QuestStatus.Failure,
        _                                          => QuestStatus.CatastrophicFailure
    };

    // --- Team power ---

    /// <summary>
    /// Calculates aggregate team power from base stats plus a level bonus.
    /// Equipment bonus is deferred to Phase 13.
    /// </summary>
    public static int CalculateTeamPower(IReadOnlyList<Character> characters)
    {
        return characters.Sum(c =>
            c.Strength + c.Luck + c.Endurance + (c.Level * 2));
    }

    // --- Death rolls ---

    /// <summary>
    /// Rolls whether a character dies based on the outcome's death probability.
    /// </summary>
    public bool RollDeath(QuestStatus outcome)
    {
        var probability = outcome switch
        {
            QuestStatus.CriticalSuccess     => GameConstants.DeathProbabilityCriticalSuccess,
            QuestStatus.Success             => GameConstants.DeathProbabilitySuccess,
            QuestStatus.Failure             => GameConstants.DeathProbabilityFailure,
            QuestStatus.CatastrophicFailure => GameConstants.DeathProbabilityCatastrophicFailure,
            _ => throw new ArgumentOutOfRangeException(nameof(outcome), outcome, null)
        };

        return random.NextDouble() < probability;
    }

    // --- XP calculation ---

    /// <summary>
    /// Calculates XP awarded per character. CriticalSuccess scales by overage
    /// (capped at 2×) and applies a ±10% jitter.
    /// </summary>
    public int CalculateXpAwarded(QuestStatus outcome, string questTier, double teamPowerRatio)
    {
        var baseXp = GetBaseXp(questTier, outcome);

        if (outcome != QuestStatus.CriticalSuccess)
            return baseXp;

        var overageMultiplier = Math.Min(teamPowerRatio, GameConstants.OverageMultiplierCap);
        var jitter = random.NextDouble(GameConstants.XpJitterMin, GameConstants.XpJitterMax);
        return (int)Math.Round(baseXp * overageMultiplier * jitter);
    }

    private static int GetBaseXp(string questTier, QuestStatus outcome) =>
        (questTier, outcome) switch
        {
            ("Novice",     QuestStatus.CriticalSuccess) => 25,
            ("Novice",     QuestStatus.Success)         => 25,
            ("Novice",     QuestStatus.Failure)         => 10,
            ("Novice",     _)                           => 5,
            ("Apprentice", QuestStatus.CriticalSuccess) => 60,
            ("Apprentice", QuestStatus.Success)         => 60,
            ("Apprentice", QuestStatus.Failure)         => 20,
            ("Apprentice", _)                           => 5,
            ("Veteran",    QuestStatus.CriticalSuccess) => 120,
            ("Veteran",    QuestStatus.Success)         => 120,
            ("Veteran",    QuestStatus.Failure)         => 40,
            ("Veteran",    _)                           => 5,
            ("Elite",      QuestStatus.CriticalSuccess) => 250,
            ("Elite",      QuestStatus.Success)         => 250,
            ("Elite",      QuestStatus.Failure)         => 80,
            ("Elite",      _)                           => 5,
            ("Legendary",  QuestStatus.CriticalSuccess) => 500,
            ("Legendary",  QuestStatus.Success)         => 500,
            ("Legendary",  QuestStatus.Failure)         => 150,
            ("Legendary",  _)                           => 5,
            _ => throw new ArgumentOutOfRangeException(nameof(questTier), questTier, null)
        };

    // --- Gold calculation ---

    /// <summary>
    /// Calculates gold awarded. Failure/CatastrophicFailure yield zero.
    /// Success/CriticalSuccess roll within a tier-based range.
    /// </summary>
    public int CalculateGoldAwarded(QuestStatus outcome, string questTier, double teamPowerRatio)
    {
        if (outcome is QuestStatus.Failure or QuestStatus.CatastrophicFailure)
            return 0;

        var (minGold, maxGold) = GetGoldRange(questTier, outcome);
        return random.NextInt(minGold, maxGold + 1);
    }

    private static (int Min, int Max) GetGoldRange(string questTier, QuestStatus outcome) =>
        (questTier, outcome) switch
        {
            ("Novice",     QuestStatus.CriticalSuccess) => (30,  60),
            ("Novice",     QuestStatus.Success)         => (15,  30),
            ("Apprentice", QuestStatus.CriticalSuccess) => (80,  140),
            ("Apprentice", QuestStatus.Success)         => (40,  70),
            ("Veteran",    QuestStatus.CriticalSuccess) => (160, 280),
            ("Veteran",    QuestStatus.Success)         => (80,  140),
            ("Elite",      QuestStatus.CriticalSuccess) => (350, 600),
            ("Elite",      QuestStatus.Success)         => (175, 300),
            ("Legendary",  QuestStatus.CriticalSuccess) => (700, 1_200),
            ("Legendary",  QuestStatus.Success)         => (350, 600),
            _ => throw new ArgumentOutOfRangeException(nameof(questTier), questTier, null)
        };

    // --- Loot eligibility ---

    /// <summary>
    /// Returns true if the outcome qualifies for loot drops.
    /// </summary>
    public static bool IsLootEligible(QuestStatus outcome) =>
        outcome is QuestStatus.CriticalSuccess or QuestStatus.Success;
}
