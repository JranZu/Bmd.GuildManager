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
	[FromKeyedServices(ServiceBusConstants.PlayerEventsTopic)] IEventPublisher eventPublisher,
	ILogger<OnboardPlayerFunction> logger)
{
	[Function("OnboardPlayer")]
	public async Task RunAsync(
		[ServiceBusTrigger(ServiceBusConstants.PlayerEventsTopic, ServiceBusConstants.OnboardingSubscription, Connection = "ServiceBusConnectionString")]
		string message,
		CancellationToken cancellationToken = default)
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

		var playerDoc = await playerRepository.FindByPlayerIdAsync(playerId, cancellationToken);

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

		// --- Publish all events BEFORE marking the player as onboarded ---
		// If any publish fails, Service Bus retries the message. Because
		// OnboardedAt is still null the idempotency guard above will let
		// the retry through and re-publish all events from scratch.

		var guildCreated = new GuildCreated(playerId, guildName, GameConstants.StartingGold);
		var guildCreatedEnvelope = EventEnvelope<GuildCreated>.Create(
			source: "OnboardPlayerFunction",
			correlationId: playerId,
			data: guildCreated);

		await eventPublisher.PublishAsync(guildCreatedEnvelope, cancellationToken);

		logger.LogInformation("GuildCreated event published for player {PlayerId}", playerId);

		var characterIds = Enumerable.Range(0, GameConstants.StarterCharacterCount)
			.Select(_ => Guid.NewGuid())
			.ToList();
		var starterCharactersGranted = new StarterCharactersGranted(playerId, characterIds);
		var starterCharactersEnvelope = EventEnvelope<StarterCharactersGranted>.Create(
			source: "OnboardPlayerFunction",
			correlationId: playerId,
			data: starterCharactersGranted);

		await eventPublisher.PublishAsync(starterCharactersEnvelope, cancellationToken);

		logger.LogInformation("StarterCharactersGranted event published for player {PlayerId}", playerId);

		var itemIds = Enumerable.Range(0, GameConstants.StarterItemCount)
			.Select(_ => Guid.NewGuid())
			.ToList();
		var starterItemsGranted = new StarterItemsGranted(playerId, itemIds);
		var starterItemsEnvelope = EventEnvelope<StarterItemsGranted>.Create(
			source: "OnboardPlayerFunction",
			correlationId: playerId,
			data: starterItemsGranted);

		await eventPublisher.PublishAsync(starterItemsEnvelope, cancellationToken);

		logger.LogInformation("StarterItemsGranted event published for player {PlayerId}", playerId);

		// --- Mark onboarded LAST — only after all events are confirmed ---
		var updatedPlayer = player with { Gold = GameConstants.StartingGold, OnboardedAt = DateTimeOffset.UtcNow };

		try
		{
			await playerRepository.UpdateAsync(updatedPlayer, playerDoc.ETag, cancellationToken);
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

		logger.LogInformation("Guild provisioned with {StartingGold} gold for player {PlayerId}", GameConstants.StartingGold, playerId);
	}
}