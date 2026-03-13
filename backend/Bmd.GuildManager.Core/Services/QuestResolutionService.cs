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
    /// Calculates aggregate team power by summing each character's TotalPower
	/// /// </summary>
    public static int CalculateTeamPower(IReadOnlyList<Character> characters)
    {
        return characters.Sum(c => c.TotalPower);
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
    public int CalculateXpAwarded(QuestStatus outcome, DifficultyTier questTier, double teamPowerRatio)
    {
        var baseXp = GetBaseXp(questTier, outcome);

        if (outcome != QuestStatus.CriticalSuccess)
            return baseXp;

        var overageMultiplier = Math.Min(teamPowerRatio, GameConstants.OverageMultiplierCap);
        var jitter = random.NextDouble(GameConstants.XpJitterMin, GameConstants.XpJitterMax);
        return (int)Math.Round(baseXp * overageMultiplier * jitter);
    }

    private static int GetBaseXp(DifficultyTier questTier, QuestStatus outcome) =>
        (questTier, outcome) switch
        {
            (DifficultyTier.Novice,      QuestStatus.CriticalSuccess) => 25,
            (DifficultyTier.Novice,      QuestStatus.Success)         => 25,
            (DifficultyTier.Novice,      QuestStatus.Failure)         => 10,
            (DifficultyTier.Novice,      _)                           => 5,
            (DifficultyTier.Apprentice,  QuestStatus.CriticalSuccess) => 60,
            (DifficultyTier.Apprentice,  QuestStatus.Success)         => 60,
            (DifficultyTier.Apprentice,  QuestStatus.Failure)         => 20,
            (DifficultyTier.Apprentice,  _)                           => 5,
            (DifficultyTier.Veteran,     QuestStatus.CriticalSuccess) => 120,
            (DifficultyTier.Veteran,     QuestStatus.Success)         => 120,
            (DifficultyTier.Veteran,     QuestStatus.Failure)         => 40,
            (DifficultyTier.Veteran,     _)                           => 5,
            (DifficultyTier.Elite,       QuestStatus.CriticalSuccess) => 250,
            (DifficultyTier.Elite,       QuestStatus.Success)         => 250,
            (DifficultyTier.Elite,       QuestStatus.Failure)         => 80,
            (DifficultyTier.Elite,       _)                           => 5,
            (DifficultyTier.Legendary,   QuestStatus.CriticalSuccess) => 500,
            (DifficultyTier.Legendary,   QuestStatus.Success)         => 500,
            (DifficultyTier.Legendary,   QuestStatus.Failure)         => 150,
            (DifficultyTier.Legendary,   _)                           => 5,
            _ => throw new ArgumentOutOfRangeException(nameof(questTier), questTier, null)
        };

    // --- Gold calculation ---

    /// <summary>
    /// Calculates gold awarded. Failure/CatastrophicFailure yield zero.
    /// Success rolls within a tier-based range.
    /// CriticalSuccess applies the overage multiplier (capped at 2×) to the
    /// midpoint of the Success range, then jitters within the tier's CriticalSuccess range.
    /// </summary>
    public int CalculateGoldAwarded(QuestStatus outcome, DifficultyTier questTier, double teamPowerRatio)
    {
        if (outcome is QuestStatus.Failure or QuestStatus.CatastrophicFailure)
            return 0;

        if (outcome == QuestStatus.CriticalSuccess)
        {
            var (successMin, successMax) = GetGoldRange(questTier, QuestStatus.Success);
            var baseGold = (successMin + successMax) / 2.0;
            var overageMultiplier = Math.Min(teamPowerRatio, GameConstants.OverageMultiplierCap);
            var scaled = (int)Math.Round(baseGold * overageMultiplier);

            var (critMin, critMax) = GetGoldRange(questTier, QuestStatus.CriticalSuccess);
            return Math.Clamp(scaled, critMin, critMax);
        }

        var (minGold, maxGold) = GetGoldRange(questTier, outcome);
        return random.NextInt(minGold, maxGold + 1);
    }

    private static (int Min, int Max) GetGoldRange(DifficultyTier questTier, QuestStatus outcome) =>
        (questTier, outcome) switch
        {
            (DifficultyTier.Novice,      QuestStatus.CriticalSuccess) => (30,  60),
            (DifficultyTier.Novice,      QuestStatus.Success)         => (15,  30),
            (DifficultyTier.Apprentice,  QuestStatus.CriticalSuccess) => (80,  140),
            (DifficultyTier.Apprentice,  QuestStatus.Success)         => (40,  70),
            (DifficultyTier.Veteran,     QuestStatus.CriticalSuccess) => (160, 280),
            (DifficultyTier.Veteran,     QuestStatus.Success)         => (80,  140),
            (DifficultyTier.Elite,       QuestStatus.CriticalSuccess) => (350, 600),
            (DifficultyTier.Elite,       QuestStatus.Success)         => (175, 300),
            (DifficultyTier.Legendary,   QuestStatus.CriticalSuccess) => (700, 1_200),
            (DifficultyTier.Legendary,   QuestStatus.Success)         => (350, 600),
            _ => throw new ArgumentOutOfRangeException(nameof(questTier), questTier, null)
        };

    // --- Loot eligibility ---

    /// <summary>
    /// Returns true if the outcome qualifies for loot drops.
    /// </summary>
    public static bool IsLootEligible(QuestStatus outcome) =>
        outcome is QuestStatus.CriticalSuccess or QuestStatus.Success;
}
