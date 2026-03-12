using Bmd.GuildManager.Core.Abstractions;
using Bmd.GuildManager.Core.Models.Requests;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Bmd.GuildManager.Functions.Functions;

public class GetAvailableQuestsFunction(
    IQuestRepository questRepository,
    ILogger<GetAvailableQuestsFunction> logger)
{
    [Function("GetAvailableQuests")]
    public async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Function, "get",
            Route = "quests/available")] HttpRequest req)
    {
        logger.LogInformation("GetAvailableQuests called");

        var quests = await questRepository.GetAvailableQuestsAsync();

        var response = quests
            .Select(q => new QuestSummary(
                QuestId: q.QuestId,
                Name: q.Name,
                Description: q.Description,
                QuestType: q.QuestType,
                Tier: q.Tier,
                RiskLevel: q.RiskLevel,
                DifficultyRating: q.DifficultyRating,
                RequiredAdventurers: q.RequiredAdventurers,
                DurationSeconds: q.DurationSeconds))
            .ToList();

        logger.LogInformation(
            "Returning {Count} available quests", response.Count);

        return new OkObjectResult(response);
    }
}
