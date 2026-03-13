using System.Text.Json;
using Bmd.GuildManager.Core.Abstractions;
using Bmd.GuildManager.Core.Constants;
using Bmd.GuildManager.Core.Data;
using Bmd.GuildManager.Core.Events;
using Bmd.GuildManager.Core.Models;
using Bmd.GuildManager.Functions.Serialization;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Bmd.GuildManager.Functions.Functions;

public class HandleStarterCharactersGrantedFunction(
	[FromKeyedServices("player-events")] IEventPublisher eventPublisher,
	ILogger<HandleStarterCharactersGrantedFunction> logger,
	IRandomProvider random)
{
	[Function("HandleStarterCharactersGranted")]
	public async Task RunAsync(
		[ServiceBusTrigger("player-events", "starter-characters-sub",
			Connection = "ServiceBusConnectionString")]
		string message)
	{
		EventEnvelope<StarterCharactersGranted>? envelope;
		try
		{
			envelope = JsonSerializer.Deserialize<EventEnvelope<StarterCharactersGranted>>(
				message, FunctionJsonOptions.Default);
		}
		catch (JsonException ex)
		{
			logger.LogWarning(ex,
				"Received invalid StarterCharactersGranted JSON payload");
			return;
		}

		if (envelope is null)
		{
			logger.LogWarning(
				"Received null or undeserializable StarterCharactersGranted message");
			return;
		}

		if (envelope.Data.CharacterIds is null)
		{
			logger.LogWarning(
				"Received invalid StarterCharactersGranted message characterIds was null");
			return;
		}

		var playerId = envelope.Data.PlayerId;
		var characterIds = envelope.Data.CharacterIds;

		logger.LogInformation(
			"HandleStarterCharactersGranted received event for player {PlayerId} " +
			"with {Count} characters",
			playerId,
			characterIds.Count);

		var usedNames = new HashSet<string>();

		foreach (var characterId in characterIds)
		{
			var name = PickUniqueName(usedNames);
			var character = Character.CreateWithId(
				characterId: characterId,
				playerId: playerId,
				name: name,
				level: 1,
				strength: random.NextInt(GameConstants.MinStatValue, GameConstants.MaxStatValue + 1),
				luck: random.NextInt(GameConstants.MinStatValue, GameConstants.MaxStatValue + 1),
				endurance: random.NextInt(GameConstants.MinStatValue, GameConstants.MaxStatValue + 1));

			var eventData = new CharacterCreated(
				PlayerId: playerId,
				CharacterId: character.CharacterId,
				Name: character.Name,
				Level: character.Level,
				Strength: character.Strength,
				Luck: character.Luck,
				Endurance: character.Endurance);

			var characterEnvelope = EventEnvelope<CharacterCreated>.Create(
				source: "HandleStarterCharactersGrantedFunction",
				correlationId: envelope.CorrelationId,
				data: eventData);

			await eventPublisher.PublishAsync(characterEnvelope);

			logger.LogInformation(
				"CharacterCreated event published for character {CharacterId} " +
				"({Name}) for player {PlayerId}",
				character.CharacterId,
				character.Name,
				playerId);
		}
	}

	private string PickUniqueName(HashSet<string> usedNames)
	{
		var available = StarterCharacterNames.Pool
			.Where(n => !usedNames.Contains(n))
			.ToList();

		var pool = available.Count > 0 ? available : [.. StarterCharacterNames.Pool];
		var name = pool[random.NextInt(0, pool.Count)];
		usedNames.Add(name);
		return name;
	}
}