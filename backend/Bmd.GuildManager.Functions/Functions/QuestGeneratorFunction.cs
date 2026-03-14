using Bmd.GuildManager.Core.Abstractions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Bmd.GuildManager.Functions.Functions;

public class QuestGeneratorFunction(
    IQuestGeneratorService questGeneratorService,
    ILogger<QuestGeneratorFunction> logger)
{
    private const int MinimumQuestsPerTier = 3;

    [Function("QuestGenerator")]
    public async Task RunAsync(
        [TimerTrigger("0 */5 * * * *")] TimerInfo timerInfo,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "QuestGenerator triggered at {Time} — ensuring minimum {Min} quests per tier",
            DateTimeOffset.UtcNow, MinimumQuestsPerTier);

        await questGeneratorService.EnsureMinimumQuestsAsync(MinimumQuestsPerTier, cancellationToken);

        logger.LogInformation("QuestGenerator completed");
    }
}
