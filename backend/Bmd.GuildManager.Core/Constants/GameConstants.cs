namespace Bmd.GuildManager.Core.Constants;

public static class GameConstants
{
    // Stat generation — used to derive quest difficulty ranges (GDD §5)
    public const int MinStatValue = 3;
    public const int MaxStatValue = 10;
    public const int StatCount = 3;

    public const int MaxLevel = 20;

    // XP required to advance from Level N to Level N+1.
    // Index 0 = Level 1 → 2. Index 18 = Level 19 → 20.
    public static readonly int[] XpThresholds =
    [
        100,        // 1 → 2
        250,        // 2 → 3
        500,        // 3 → 4
        1_000,      // 4 → 5
        2_000,      // 5 → 6
        4_000,      // 6 → 7
        8_000,      // 7 → 8
        16_000,     // 8 → 9
        32_000,     // 9 → 10
        64_000,     // 10 → 11
        128_000,    // 11 → 12
        256_000,    // 12 → 13
        512_000,    // 13 → 14
        1_024_000,  // 14 → 15
        2_048_000,  // 15 → 16
        4_096_000,  // 16 → 17
        8_192_000,  // 17 → 18
        16_384_000, // 18 → 19
        32_768_000  // 19 → 20
    ];

    // Outcome thresholds — teamPowerRatio = effectiveTeamPower / difficultyRating (GDD §6)
    public const double CriticalSuccessThreshold = 1.50;
    public const double SuccessThreshold = 1.00;
    public const double FailureThreshold = 0.60;

    // Death probability per outcome per character (GDD §6)
    public const double DeathProbabilityCriticalSuccess = 0.01;
    public const double DeathProbabilitySuccess = 0.02;
    public const double DeathProbabilityFailure = 0.20;
    public const double DeathProbabilityCatastrophicFailure = 0.60;

    // CriticalSuccess XP/gold overage multiplier cap (GDD §6)
    public const double OverageMultiplierCap = 2.0;

    // CriticalSuccess XP jitter range
    public const double XpJitterMin = 0.9;
    public const double XpJitterMax = 1.1;
}
