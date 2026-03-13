using System.Text.Json;
using Bmd.GuildManager.Core.Abstractions;
using Bmd.GuildManager.Core.Constants;
using Bmd.GuildManager.Core.Events;
using Bmd.GuildManager.Functions.Serialization;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Bmd.GuildManager.Functions.Functions;

public class OnboardPlayerFunction(
	IPlayerRepository playerRepository,
	[FromKeyedServices("player-events")] IEventPublisher eventPublisher,
	ILogger<OnboardPlayerFunction> logger)
{
	const int StartingGold = 500;

	[Function("OnboardPlayer")]
	public async Task RunAsync(
		[ServiceBusTrigger("player-events", "onboarding-sub", Connection = "ServiceBusConnectionString")]
		string message)
	{
		EventEnvelope<PlayerCreated>? envelope;

		try
		{
			envelope = JsonSerializer.Deserialize<EventEnvelope<PlayerCreated>>(
				message, FunctionJsonOptions.Default);
		}
		catch (JsonException ex)
		{
			logger.LogWarning(ex, "Received invalid PlayerCreated JSON payload");
			return;
		}

		if (envelope is null)
		{
			logger.LogWarning("Received null PlayerCreated message");
			return;
		}


		var playerId = envelope.Data.PlayerId;
		var guildName = envelope.Data.GuildName;

		logger.LogInformation(
			"OnboardPlayer received PlayerCreated for player {PlayerId} with guild {GuildName}",
			playerId,
			guildName);

		var playerDoc = await playerRepository.FindByPlayerIdAsync(playerId);

		if (playerDoc is null)
		{
			logger.LogWarning("Player {PlayerId} not found during onboarding", playerId);
			return;
		}

		var player = playerDoc.Document;

		if (player.OnboardedAt is not null)
		{
			logger.LogWarning("Player {PlayerId} already onboarded, skipping", playerId);
			return;
		}

		var updatedPlayer = player with { Gold = StartingGold, OnboardedAt = DateTimeOffset.UtcNow };

		try
		{
			await playerRepository.UpdateAsync(updatedPlayer, playerDoc.ETag);
		}
		catch (CosmosException ex)
			when (ex.StatusCode == System.Net.HttpStatusCode.PreconditionFailed)
		{
			// ETag conflict — another process updated this player concurrently.
			// Re-throwing lets Service Bus retry the message.
			logger.LogWarning(
				"ETag conflict updating player {PlayerId} during onboarding — will retry via Service Bus",
				playerId);
			throw;
		}

		logger.LogInformation("Guild provisioned with {startingGold} gold for player {PlayerId}", StartingGold, playerId);

		var guildCreated = new GuildCreated(playerId, guildName, StartingGold);
		var guildCreatedEnvelope = EventEnvelope<GuildCreated>.Create(
			source: "OnboardPlayerFunction",
			correlationId: playerId,
			data: guildCreated);

		await eventPublisher.PublishAsync(guildCreatedEnvelope);

		logger.LogInformation("GuildCreated event published for player {PlayerId}", playerId);

		var characterIds = Enumerable.Range(0, GameConstants.StarterCharacterCount)
			.Select(_ => Guid.NewGuid())
			.ToList();
		var starterCharactersGranted = new StarterCharactersGranted(playerId, characterIds);
		var starterCharactersEnvelope = EventEnvelope<StarterCharactersGranted>.Create(
			source: "OnboardPlayerFunction",
			correlationId: playerId,
			data: starterCharactersGranted);

		await eventPublisher.PublishAsync(starterCharactersEnvelope);

		logger.LogInformation("StarterCharactersGranted event published for player {PlayerId}", playerId);

		var itemIds = Enumerable.Range(0, GameConstants.StarterItemCount)
			.Select(_ => Guid.NewGuid())
			.ToList();
		var starterItemsGranted = new StarterItemsGranted(playerId, itemIds);
		var starterItemsEnvelope = EventEnvelope<StarterItemsGranted>.Create(
			source: "OnboardPlayerFunction",
			correlationId: playerId,
			data: starterItemsGranted);

		await eventPublisher.PublishAsync(starterItemsEnvelope);

		logger.LogInformation("StarterItemsGranted event published for player {PlayerId}", playerId);
	}
}