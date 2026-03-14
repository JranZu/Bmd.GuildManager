using Bmd.GuildManager.Core.Abstractions;
using Bmd.GuildManager.Core.Models;

namespace Bmd.GuildManager.Core.Services;

public class QuestFactory
{
    private static readonly DifficultyTier[] Tiers =
        [DifficultyTier.Novice, DifficultyTier.Apprentice, DifficultyTier.Veteran, DifficultyTier.Elite, DifficultyTier.Legendary];

    private readonly IRandomProvider _random;
    private readonly QuestNameBuilder _nameBuilder;

    public QuestFactory(IRandomProvider random, QuestNameBuilder nameBuilder)
    {
        _random = random;
        _nameBuilder = nameBuilder;
    }

    public Quest Generate(DifficultyTier tier)
    {
        var (minDifficulty, maxDifficulty,
             minAdventurers, maxAdventurers,
             minDuration, maxDuration) = GetTierParameters(tier);

        var questId = Guid.NewGuid();
        var questType = PickQuestType();
        var riskLevel = PickRiskLevel();
        var difficulty = _random.NextInt(minDifficulty, maxDifficulty + 1);
        var adventurers = _random.NextInt(minAdventurers, maxAdventurers + 1);
        var duration = _random.NextInt(minDuration, maxDuration + 1);

        var words = _nameBuilder.SelectWords(tier, riskLevel);

        return new Quest(
            Id:                     questId.ToString(),
            QuestId:                questId,
            Name:                   _nameBuilder.BuildName(words, questType),
            Description:            _nameBuilder.BuildDescription(words, questType),
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

    private QuestType PickQuestType() =>
        _random.NextInt(0, 5) switch
        {
            0 => QuestType.Kill,
            1 => QuestType.Gather,
            2 => QuestType.Rescue,
            3 => QuestType.Delivery,
            _ => QuestType.Escort
        };

    private RiskLevel PickRiskLevel() =>
        _random.NextInt(0, 10) switch
        {
            < 4 => RiskLevel.Low,
            < 8 => RiskLevel.Medium,
            _   => RiskLevel.High
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
