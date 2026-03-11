using System.Text.Json;
using Bmd.GuildManager.Core.Abstractions;
using Bmd.GuildManager.Core.Events;
using Bmd.GuildManager.Core.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Bmd.GuildManager.Functions.Functions;

public class HandleCharacterCreatedFunction(
	ICharacterRepository characterRepository,
	ILogger<HandleCharacterCreatedFunction> logger)
{
	private static readonly JsonSerializerOptions JsonOptions = new()
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase
	};

	[Function("HandleCharacterCreated")]
	public async Task RunAsync(
		[ServiceBusTrigger("player-events", "character-created-sub",
			Connection = "ServiceBusConnectionString")]
		string message)
	{
		EventEnvelope<CharacterCreated>? envelope;
		try
		{
			envelope = JsonSerializer.Deserialize<EventEnvelope<CharacterCreated>>(
				message, JsonOptions);
		}
		catch (JsonException ex)
		{
			logger.LogWarning(ex, "Received invalid CharacterCreated JSON payload");
			return;
		}

		if (envelope is null)
		{
			logger.LogWarning("Received null or undeserializable CharacterCreated message");
			return;
		}

		var data = envelope.Data;

		logger.LogInformation(
			"HandleCharacterCreated received event for character {CharacterId} " +
			"belonging to player {PlayerId}",
			data.CharacterId,
			data.PlayerId);

		var existing = await characterRepository
			.FindByCharacterIdAsync(data.CharacterId, data.PlayerId);

		if (existing is not null)
		{
			logger.LogWarning(
				"Character {CharacterId} already exists, skipping duplicate",
				data.CharacterId);
			return;
		}

		var character = new Character(
			Id:                  data.CharacterId.ToString(),
			CharacterId:         data.CharacterId,
			PlayerId:            data.PlayerId,
			Name:                data.Name,
			Level:               data.Level,
			Strength:            data.Strength,
			Luck:                data.Luck,
			Endurance:           data.Endurance,
			Status:              CharacterStatus.Idle,
			EquipmentIds:        [],
			Xp:                  0,
			ActiveQuestSnapshot: null);

		await characterRepository.CreateAsync(character);

		logger.LogInformation(
			"Character {CharacterId} persisted for player {PlayerId}",
			data.CharacterId,
			data.PlayerId);
	}
}