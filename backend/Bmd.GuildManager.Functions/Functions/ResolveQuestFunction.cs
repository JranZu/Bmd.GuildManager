using System.Text;
using System.Text.Json;
using Azure.Storage.Blobs;
using Bmd.GuildManager.Core.Abstractions;
using Bmd.GuildManager.Core.Events;
using Bmd.GuildManager.Core.Models;
using Bmd.GuildManager.Core.Services;
using Bmd.GuildManager.Functions.Infrastructure;
using Bmd.GuildManager.Functions.Serialization;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Bmd.GuildManager.Functions.Functions;

public class ResolveQuestFunction(
    IQuestRepository questRepository,
    ICharacterRepository characterRepository,
    [FromKeyedServices(ServiceBusConstants.QuestEventsTopic)] IEventPublisher eventPublisher,
    BlobServiceClient blobServiceClient,
    QuestResolutionService resolutionService,
    ILogger<ResolveQuestFunction> logger)
{
    private const string ArchiveContainer = "quest-archive";

    [Function("ResolveQuest")]
    public async Task RunAsync(
        [ServiceBusTrigger(ServiceBusConstants.QuestCompletedQueue, Connection = "ServiceBusConnectionString")]
        string messageBody,
        CancellationToken cancellationToken = default)
    {
        // --- 1. Deserialize the QuestCompleted envelope ---
        EventEnvelope<QuestCompleted>? envelope;
        try
        {
            envelope = JsonSerializer.Deserialize<EventEnvelope<QuestCompleted>>(
                messageBody, FunctionJsonOptions.Default);
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "Failed to deserialize QuestCompleted message");
            return;
        }

        if (envelope is null)
        {
            logger.LogError("QuestCompleted message deserialized to null");
            return;
        }

        var questId = envelope.Data.QuestId;
        var playerId = envelope.Data.PlayerId;

        logger.LogInformation(
            "ResolveQuest triggered for quest {QuestId}", questId);

        // --- 2. Load quest (idempotency guard) ---
        var questDoc = await questRepository.FindByQuestIdAsync(questId, cancellationToken);
        if (questDoc is null)
        {
            logger.LogWarning(
                "Quest {QuestId} not found — already resolved or never existed. Skipping.",
                questId);
            return;
        }

        var quest = questDoc.Document;

        // --- 3. Load assigned characters in parallel ---
        // Compensating check: only include characters whose status is OnQuest or Dead.
        // If StartQuestFunction partially failed (ETag conflict after quest claim),
        // a character may still be in Idle status — skip those to avoid awarding
        // XP/gold for a quest they never actually participated in.
        var charDocTasks = quest.AssignedCharacterIds
            .Select(id => characterRepository.FindByCharacterIdAsync(id, playerId, cancellationToken))
            .ToList();

        var charDocs = await Task.WhenAll(charDocTasks);

        var characters = new List<Character>();
        for (var i = 0; i < quest.AssignedCharacterIds.Count; i++)
        {
            var characterId = quest.AssignedCharacterIds[i];
            var charDoc = charDocs[i];

            if (charDoc is null)
                continue;

            if (charDoc.Document.Status is not (CharacterStatus.OnQuest or CharacterStatus.Dead))
            {
                logger.LogWarning(
                    "Character {CharacterId} for quest {QuestId} has status {Status} " +
                    "instead of OnQuest/Dead — skipping rewards (partial StartQuest failure suspected)",
                    characterId, questId, charDoc.Document.Status);
                continue;
            }

            characters.Add(charDoc.Document);
        }

        if (characters.Count == 0)
        {
            logger.LogError(
                "Quest {QuestId} has no resolvable characters. Archiving without resolution.",
                questId);
            await ArchiveAndDeleteQuestAsync(quest, cancellationToken);
            return;
        }

        // --- 4. Calculate outcome ---
        var teamPower = QuestResolutionService.CalculateTeamPower(characters);
        var outcome = resolutionService.DetermineOutcome(teamPower, quest.DifficultyRating);

        // Ratio without jitter for the CriticalSuccess XP scaling multiplier
        var teamPowerRatio = (double)teamPower / quest.DifficultyRating;

        logger.LogInformation(
            "Quest {QuestId}: teamPower={TeamPower}, difficulty={Difficulty}, outcome={Outcome}",
            questId, teamPower, quest.DifficultyRating, outcome);

        // --- 5. Roll death per character ---
        var resolvedCharacters = characters.Select(c =>
        {
            var died = resolutionService.RollDeath(outcome);
            return new QuestResolvedCharacter(c.CharacterId, Survived: !died);
        }).ToList();

        // --- 6. Calculate rewards ---
        var xpAwarded = resolutionService.CalculateXpAwarded(outcome, quest.Tier, teamPowerRatio);
        var goldAwarded = resolutionService.CalculateGoldAwarded(outcome, quest.Tier, teamPowerRatio);
        var lootEligible = QuestResolutionService.IsLootEligible(outcome);

        // --- 7. Publish QuestResolved ---
        var questResolved = new QuestResolved(
            QuestId: questId,
            PlayerId: playerId,
            QuestTier: quest.Tier,
            Outcome: outcome.ToString(),
            XpAwarded: xpAwarded,
            Characters: resolvedCharacters,
            LootEligible: lootEligible,
            GoldAwarded: goldAwarded);

        var resolvedEnvelope = EventEnvelope<QuestResolved>.Create(
            source: "ResolveQuestFunction",
            correlationId: envelope.CorrelationId,
            data: questResolved);

        await eventPublisher.PublishAsync(resolvedEnvelope, cancellationToken);

        logger.LogInformation(
            "QuestResolved published for quest {QuestId}: outcome={Outcome}, " +
            "survivors={Survivors}/{Total}, xp={Xp}, gold={Gold}",
            questId, outcome,
            resolvedCharacters.Count(c => c.Survived), resolvedCharacters.Count,
            xpAwarded, goldAwarded);

        // --- 8. Archive quest to Blob Storage and delete from Cosmos ---
        await ArchiveAndDeleteQuestAsync(quest with { Status = outcome }, cancellationToken);
    }

	private async Task ArchiveAndDeleteQuestAsync(Quest quest, CancellationToken cancellationToken)
	{
		var containerClient = blobServiceClient.GetBlobContainerClient(ArchiveContainer);
		await containerClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

		var now = DateTimeOffset.UtcNow;
		var blobName = $"{now.Year}/{now.Month:D2}/{quest.QuestId}.json";
		var blobClient = containerClient.GetBlobClient(blobName);

		var json = JsonSerializer.Serialize(quest, FunctionJsonOptions.Default);
		var bytes = Encoding.UTF8.GetBytes(json);

		await blobClient.UploadAsync(
			new BinaryData(bytes),
			overwrite: true,
			cancellationToken: cancellationToken);

		logger.LogInformation(
			"Quest {QuestId} archived to blob {BlobName}", quest.QuestId, blobName);

		await questRepository.DeleteAsync(quest.QuestId, cancellationToken);

		logger.LogInformation(
			"Quest {QuestId} deleted from Cosmos DB", quest.QuestId);
	}
}
