using System.Text.Json;
using Bmd.GuildManager.Core.Abstractions;
using Bmd.GuildManager.Core.Events;
using Bmd.GuildManager.Core.Models;
using Bmd.GuildManager.Core.Models.Requests;
using Bmd.GuildManager.Functions.Infrastructure;
using Bmd.GuildManager.Functions.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Bmd.GuildManager.Functions.Functions;

public class StartQuestFunction(
	IQuestRepository questRepository,
	ICharacterRepository characterRepository,
	[FromKeyedServices(ServiceBusConstants.QuestEventsTopic)] IEventPublisher eventPublisher,
	IMessageScheduler messageScheduler,
	ILogger<StartQuestFunction> logger)
{
    [Function("StartQuest")]
    public async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Function, "post",
            Route = "quests")] HttpRequest req)
    {
        var ct = req.HttpContext.RequestAborted;

        // --- 1. Parse and validate request ---
        StartQuestRequest? request;
        try
        {
            request = await JsonSerializer
                .DeserializeAsync<StartQuestRequest>(req.Body, FunctionJsonOptions.Default);
        }
        catch (JsonException)
        {
            return new BadRequestObjectResult("Request body is invalid JSON.");
        }

        if (request is null
            || request.PlayerId == Guid.Empty
            || request.QuestId == Guid.Empty
            || request.CharacterIds is null
            || request.CharacterIds.Count == 0)
        {
            return new BadRequestObjectResult(
                "playerId, questId, and at least one characterId are required.");
        }

        logger.LogInformation(
            "StartQuest called by player {PlayerId} for quest {QuestId} " +
            "with {CharacterCount} character(s)",
            request.PlayerId, request.QuestId, request.CharacterIds.Count);

        // --- 2. Load and validate the quest ---
        var questDoc = await questRepository.FindByQuestIdAsync(request.QuestId, ct);

        if (questDoc is null || questDoc.Document.Status != QuestStatus.Available)
        {
            logger.LogWarning(
                "Quest {QuestId} not found or not available", request.QuestId);
            return new ConflictObjectResult("Quest is not available.");
        }

        var quest = questDoc.Document;

        // --- 3. Load all characters in parallel ---
        var charDocTasks = request.CharacterIds
            .Select(id => characterRepository.FindByCharacterIdAsync(id, request.PlayerId, ct))
            .ToList();

        var charDocs = await Task.WhenAll(charDocTasks);

        var characters = new List<(Character Character, string ETag)>();

        for (var i = 0; i < request.CharacterIds.Count; i++)
        {
            var characterId = request.CharacterIds[i];
            var charDoc = charDocs[i];

            if (charDoc is null)
            {
                logger.LogWarning(
                    "Character {CharacterId} not found for player {PlayerId}",
                    characterId, request.PlayerId);
                return new ConflictObjectResult(
                    $"Character {characterId} not found.");
            }

            if (charDoc.Document.Status != CharacterStatus.Idle)
            {
                logger.LogWarning(
                    "Character {CharacterId} is not Idle (status: {Status})",
                    characterId, charDoc.Document.Status);
                return new ConflictObjectResult(
                    $"Character {characterId} is not available for a quest.");
            }

            characters.Add((charDoc.Document, charDoc.ETag));
        }

        // --- 4. Validate character count ---
        if (characters.Count != quest.RequiredAdventurers)
        {
            return new BadRequestObjectResult(
                $"This quest requires exactly {quest.RequiredAdventurers} adventurer(s).");
        }

        // --- 5. Compute timing ---
        var now = DateTimeOffset.UtcNow;
        var estimatedCompletionAt = now.AddSeconds(quest.DurationSeconds);

        // --- 6. Claim the quest FIRST ---
        // This is the true concurrency guard. If two players race for the
        // same quest, only one ETag can win. The loser gets a clean 409 here
        // before any character writes have occurred — guaranteed clean state.
        var claimedQuest = quest with
        {
            Status = QuestStatus.InProgress,
            PlayerId = request.PlayerId,
            AssignedCharacterIds = request.CharacterIds,
            StartedAt = now,
            EstimatedCompletionAt = estimatedCompletionAt
        };

        try
        {
            await questRepository.UpdateAsync(claimedQuest, questDoc.ETag, ct);
        }
        catch (CosmosException ex)
            when (ex.StatusCode == System.Net.HttpStatusCode.PreconditionFailed)
        {
            logger.LogWarning(
                "ETag conflict claiming quest {QuestId} — already taken by another player",
                request.QuestId);
            return new ConflictObjectResult(
                "Quest was claimed by another player. Please choose a different quest.");
        }

        logger.LogInformation(
            "Quest {QuestId} claimed by player {PlayerId}",
            quest.QuestId, request.PlayerId);

        // --- 7. Update characters to OnQuest ---
        // Quest is now claimed. If any character update fails we log it as a
        // critical error and return 500.
        //
        // KNOWN LIMITATION: The quest has been claimed in Cosmos but this character's
        // status was not updated to OnQuest. When the QuestCompleted message fires,
        // ResolveQuestFunction will attempt to award XP/gold to all AssignedCharacterIds
        // including this one — even though their status is still Idle.
        // A compensating check in ResolveQuestFunction guards against this (see that function).
        // True atomicity would require Cosmos transactions (single-partition only) and is
        // deferred to a future reliability hardening phase.
        var snapshot = new ActiveQuestSnapshot(
            QuestId: quest.QuestId,
            Name: quest.Name,
            Description: quest.Description,
            Tier: quest.Tier,
            EstimatedCompletionAt: estimatedCompletionAt);

        foreach (var (character, etag) in characters)
        {
            var updated = character with
            {
                Status = CharacterStatus.OnQuest,
                ActiveQuestSnapshot = snapshot
            };

            try
            {
                await characterRepository.UpdateAsync(updated, etag, ct);
            }
            catch (CosmosException ex)
                when (ex.StatusCode == System.Net.HttpStatusCode.PreconditionFailed)
            {
                logger.LogCritical(
                        "ETag conflict updating character {CharacterId} after quest {QuestId} " +
                        "was already claimed. Character will remain Idle; " +
                        "ResolveQuestFunction compensating check will skip this character.",
                        character.CharacterId, quest.QuestId);
                return new ObjectResult(
                    "Quest claimed but character update failed due to concurrent modification. " +
                    "The quest will expire automatically.")
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }
        }

        // --- 8. Publish QuestStarted event ---
        var correlationId = Guid.NewGuid();

        var questStarted = new QuestStarted(
            QuestId: quest.QuestId,
            PlayerId: request.PlayerId,
            QuestType: quest.QuestType,
            QuestTier: quest.Tier,
            CharacterIds: request.CharacterIds,
            DurationSeconds: quest.DurationSeconds,
            EstimatedCompletionAt: estimatedCompletionAt);

        var envelope = EventEnvelope<QuestStarted>.Create(
            source: "StartQuestFunction",
            correlationId: correlationId,
            data: questStarted);

        await eventPublisher.PublishAsync(envelope, ct);

        logger.LogInformation(
            "QuestStarted event published for quest {QuestId}", quest.QuestId);

        // --- 9. Schedule QuestCompleted Service Bus message ---
        // The correlationId is shared with QuestStarted so the entire chain
        // QuestStarted → QuestCompleted → QuestResolved → LootGenerated
        // is traceable end-to-end in Application Insights.
        var questCompleted = new QuestCompleted(
            QuestId: quest.QuestId,
            PlayerId: request.PlayerId);

        var completedEnvelope = EventEnvelope<QuestCompleted>.Create(
            source: "StartQuestFunction",
            correlationId: correlationId,
            data: questCompleted);

        var messageBody = JsonSerializer.Serialize(completedEnvelope, FunctionJsonOptions.Default);

        await messageScheduler.ScheduleMessageAsync(
            ServiceBusConstants.QuestCompletedQueue,
            messageBody,
            estimatedCompletionAt,
            ct);

        logger.LogInformation(
            "QuestCompleted message scheduled for {Time} on queue {Queue}",
            estimatedCompletionAt, ServiceBusConstants.QuestCompletedQueue);

        // --- 10. Return 202 ---
        return new ObjectResult(new { questId = quest.QuestId })
        {
            StatusCode = StatusCodes.Status202Accepted
        };
    }
}
