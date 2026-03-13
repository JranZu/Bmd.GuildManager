using Bmd.GuildManager.Core.Models;

namespace Bmd.GuildManager.Core.Services;

public static class QuestFactory
{
    private static readonly DifficultyTier[] Tiers =
        [DifficultyTier.Novice, DifficultyTier.Apprentice, DifficultyTier.Veteran, DifficultyTier.Elite, DifficultyTier.Legendary];

    public static Quest Generate(DifficultyTier tier)
    {
        var (minDifficulty, maxDifficulty,
             minAdventurers, maxAdventurers,
             minDuration, maxDuration) = GetTierParameters(tier);

        var questId = Guid.NewGuid();
        var questType = PickQuestType();
        var riskLevel = PickRiskLevel();
        var difficulty = Random.Shared.Next(minDifficulty, maxDifficulty + 1);
        var adventurers = Random.Shared.Next(minAdventurers, maxAdventurers + 1);
        var duration = Random.Shared.Next(minDuration, maxDuration + 1);

        return new Quest(
            Id:                     questId.ToString(),
            QuestId:                questId,
            Name:                   QuestNameBuilder.BuildName(tier, riskLevel, questType),
            Description:            QuestNameBuilder.BuildDescription(tier, riskLevel, questType),
            QuestType:              questType,
            Tier:                   tier,
            RiskLevel:              riskLevel,
            DifficultyRating:       difficulty,
            RequiredAdventurers:    adventurers,
            DurationSeconds:        duration,
            Status:                 QuestStatus.Available,
            PlayerId:               null,
            AssignedCharacterIds:   [],
            CreatedAt:              DateTimeOffset.UtcNow,
            StartedAt:              null,
            EstimatedCompletionAt:  null);
    }

    public static IReadOnlyList<DifficultyTier> AllTiers() => Tiers;

    private static string PickQuestType() =>
        Random.Shared.Next(5) switch
        {
            0 => "Kill",
            1 => "Gather",
            2 => "Rescue",
            3 => "Delivery",
            _ => "Escort"
        };

    private static string PickRiskLevel() =>
        Random.Shared.Next(10) switch
        {
            < 4 => "Low",
            < 8 => "Medium",
            _   => "High"
        };

    private static (int minDiff, int maxDiff,
                    int minAdv, int maxAdv,
                    int minDur, int maxDur)
        GetTierParameters(DifficultyTier tier) => tier switch
    {
        DifficultyTier.Novice      => (9,   60,  1, 1, 60,   180),
        DifficultyTier.Apprentice  => (60,  120, 1, 2, 120,  300),
        DifficultyTier.Veteran     => (120, 240, 2, 3, 240,  600),
        DifficultyTier.Elite       => (240, 480, 3, 4, 480,  900),
        DifficultyTier.Legendary   => (480, 960, 4, 5, 600,  1800),
        _                => throw new ArgumentOutOfRangeException(
                                nameof(tier), $"Unknown tier: {tier}")
    };
}
