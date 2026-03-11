using System.Text.Json;
using Bmd.GuildManager.Core.Abstractions;
using Bmd.GuildManager.Core.Events;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Bmd.GuildManager.Functions.Functions;

public class OnboardPlayerFunction(
	IPlayerRepository playerRepository,
	IEventPublisher eventPublisher,
	ILogger<OnboardPlayerFunction> logger)
{
	const int STARTING_GOLD = 500;

	private static readonly JsonSerializerOptions JsonOptions = new()
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase
	};

	[Function("OnboardPlayer")]
	public async Task RunAsync(
		[ServiceBusTrigger("player-events", "onboarding-sub", Connection = "ServiceBusConnectionString")]
		string message)
	{
		var envelope = JsonSerializer.Deserialize<EventEnvelope<PlayerCreated>>(message, JsonOptions);

		try
		{
			envelope = JsonSerializer.Deserialize<EventEnvelope<PlayerCreated>>(
				message, JsonOptions);
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

		var player = await playerRepository.FindByPlayerIdAsync(playerId);

		if (player is null)
		{
			logger.LogWarning("Player {PlayerId} not found during onboarding", playerId);
			return;
		}

		if (player.Gold == STARTING_GOLD)
		{
			logger.LogWarning("Player {PlayerId} already onboarded, skipping", playerId);
			return;
		}

		var updatedPlayer = player with { Gold = STARTING_GOLD };
		await playerRepository.UpdateAsync(updatedPlayer);

		logger.LogInformation("Guild provisioned with {startingGold} gold for player {PlayerId}", STARTING_GOLD, playerId);

		var guildCreated = new GuildCreated(playerId, guildName, STARTING_GOLD);
		var guildCreatedEnvelope = EventEnvelope<GuildCreated>.Create(
			source: "OnboardPlayerFunction",
			correlationId: playerId,
			data: guildCreated);

		await eventPublisher.PublishAsync(guildCreatedEnvelope);

		logger.LogInformation("GuildCreated event published for player {PlayerId}", playerId);

		var starterCharactersGranted = new StarterCharactersGranted(playerId, [Guid.NewGuid(), Guid.NewGuid()]);
		var starterCharactersEnvelope = EventEnvelope<StarterCharactersGranted>.Create(
			source: "OnboardPlayerFunction",
			correlationId: playerId,
			data: starterCharactersGranted);

		await eventPublisher.PublishAsync(starterCharactersEnvelope);

		logger.LogInformation("StarterCharactersGranted event published for player {PlayerId}", playerId);

		var starterItemsGranted = new StarterItemsGranted(playerId, [Guid.NewGuid(), Guid.NewGuid()]);
		var starterItemsEnvelope = EventEnvelope<StarterItemsGranted>.Create(
			source: "OnboardPlayerFunction",
			correlationId: playerId,
			data: starterItemsGranted);

		await eventPublisher.PublishAsync(starterItemsEnvelope);

		logger.LogInformation("StarterItemsGranted event published for player {PlayerId}", playerId);
	}
}