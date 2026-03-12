using Bmd.GuildManager.Core.Abstractions;
using Bmd.GuildManager.Core.Services;
using Microsoft.Extensions.Logging;

namespace Bmd.GuildManager.Functions.Services;

public class QuestGeneratorService(
    IQuestRepository questRepository,
    ILogger<QuestGeneratorService> logger) : IQuestGeneratorService
{
    public async Task EnsureMinimumQuestsAsync(int minimumPerTier)
    {
        foreach (var tier in QuestFactory.AllTiers())
        {
            var current = await questRepository.CountAvailableByTierAsync(tier);
            var needed = minimumPerTier - current;

            if (needed <= 0)
            {
                logger.LogInformation(
                    "Tier {Tier} has {Count} available quests — no generation needed",
                    tier, current);
                continue;
            }

            logger.LogInformation(
                "Tier {Tier} has {Count} available quests — generating {Needed}",
                tier, current, needed);

            for (var i = 0; i < needed; i++)
            {
                var quest = QuestFactory.Generate(tier);
                await questRepository.CreateAsync(quest);
            }
        }
    }
}
