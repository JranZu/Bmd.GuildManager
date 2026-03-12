using Bmd.GuildManager.Core.Models;

namespace Bmd.GuildManager.Core.Services;

public static class QuestFactory
{
    private static readonly Random Random = new();

    private static readonly string[] Tiers =
        ["Novice", "Apprentice", "Veteran", "Elite", "Legendary"];

    public static Quest Generate(string tier)
    {
        var (minDifficulty, maxDifficulty,
             minAdventurers, maxAdventurers,
             minDuration, maxDuration) = GetTierParameters(tier);

        var questId = Guid.NewGuid();
        var questType = PickQuestType();
        var riskLevel = PickRiskLevel();
        var difficulty = Random.Next(minDifficulty, maxDifficulty + 1);
        var adventurers = Random.Next(minAdventurers, maxAdventurers + 1);
        var duration = Random.Next(minDuration, maxDuration + 1);

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

    public static IReadOnlyList<string> AllTiers() => Tiers;

    private static string PickQuestType() =>
        Random.Next(5) switch
        {
            0 => "Kill",
            1 => "Gather",
            2 => "Rescue",
            3 => "Delivery",
            _ => "Escort"
        };

    private static string PickRiskLevel() =>
        Random.Next(10) switch
        {
            < 4 => "Low",
            < 8 => "Medium",
            _   => "High"
        };

    private static (int minDiff, int maxDiff,
                    int minAdv, int maxAdv,
                    int minDur, int maxDur)
        GetTierParameters(string tier) => tier switch
    {
        "Novice"      => (10,  30,  1, 1, 60,   180),
        "Apprentice"  => (25,  60,  1, 2, 120,  300),
        "Veteran"     => (50,  100, 2, 3, 240,  600),
        "Elite"       => (90,  160, 3, 4, 480,  900),
        "Legendary"   => (150, 250, 4, 5, 600,  1800),
        _             => throw new ArgumentOutOfRangeException(
                             nameof(tier), $"Unknown tier: {tier}")
    };
}
