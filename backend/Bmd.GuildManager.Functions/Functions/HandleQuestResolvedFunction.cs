using System.Text.Json;
using Bmd.GuildManager.Core.Abstractions;
using Bmd.GuildManager.Core.Events;
using Bmd.GuildManager.Core.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Bmd.GuildManager.Functions.Functions;

public class HandleQuestResolvedFunction(
    ICharacterRepository characterRepository,
    ILogger<HandleQuestResolvedFunction> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    [Function("HandleQuestResolved")]
    public async Task RunAsync(
        [ServiceBusTrigger("quest-events", "character-quest-resolved-sub",
            Connection = "ServiceBusConnectionString")]
        string messageBody)
    {
        // --- 1. Deserialize ---
        EventEnvelope<QuestResolved>? envelope;
        try
        {
            envelope = JsonSerializer.Deserialize<EventEnvelope<QuestResolved>>(
                messageBody, JsonOptions);
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "Failed to deserialize QuestResolved message");
            return;
        }

        if (envelope is null)
        {
            logger.LogError("QuestResolved message deserialized to null");
            return;
        }

        var data = envelope.Data;

        logger.LogInformation(
            "HandleQuestResolved processing quest {QuestId}, outcome {Outcome}, " +
            "xpAwarded {XpAwarded}",
            data.QuestId, data.Outcome, data.XpAwarded);

        // Characters where survived = false are handled by Phase 10 (HandleCharacterDeathFunction)
        var survivors = data.Characters.Where(c => c.Survived).ToList();

        foreach (var resolvedChar in survivors)
        {
            await ProcessSurvivorAsync(
                resolvedChar.CharacterId,
                data.PlayerId,
                data.XpAwarded,
                data.QuestId);
        }

        logger.LogInformation(
            "HandleQuestResolved complete for quest {QuestId}: " +
            "{SurvivorCount} survivor(s) updated",
            data.QuestId, survivors.Count);
    }

    private async Task ProcessSurvivorAsync(
        Guid characterId,
        Guid playerId,
        int xpAwarded,
        Guid questId)
    {
        var charDoc = await characterRepository
            .FindByCharacterIdAsync(characterId, playerId);

        if (charDoc is null)
        {
            logger.LogWarning(
                "Character {CharacterId} not found for quest {QuestId} — skipping",
                characterId, questId);
            return;
        }

        var character = charDoc.Document;
        var etag = charDoc.ETag;

        // Idempotency guard: if this character's snapshot no longer references
        // this quest, it has already been processed or belongs to a different quest
        if (character.ActiveQuestSnapshot?.QuestId != questId)
        {
            logger.LogInformation(
                "Character {CharacterId} snapshot does not reference quest {QuestId} — skipping",
                characterId, questId);
            return;
        }

        var levelBefore = character.Level;

        var updated = character.WithXpApplied(xpAwarded) with
        {
            Status = CharacterStatus.Idle,
            ActiveQuestSnapshot = null
        };

        if (updated.Level > levelBefore)
        {
            logger.LogInformation(
                "Character {CharacterId} leveled up: {OldLevel} → {NewLevel}",
                characterId, levelBefore, updated.Level);
        }

        try
        {
            await characterRepository.UpdateAsync(updated, etag);

            logger.LogInformation(
                "Character {CharacterId} updated: xp {OldXp} → {NewXp}, " +
                "level {Level}, status Idle",
                characterId, character.Xp, updated.Xp, updated.Level);
        }
        catch (CosmosException ex)
            when (ex.StatusCode == System.Net.HttpStatusCode.PreconditionFailed)
        {
            // ETag conflict — another process updated this character concurrently.
            // Re-throwing lets Service Bus retry the message.
            logger.LogWarning(
                "ETag conflict updating character {CharacterId} for quest {QuestId} — " +
                "will retry via Service Bus",
                characterId, questId);
            throw;
        }
    }
}
