using System.Text.Json;
using Bmd.GuildManager.Core.Abstractions;
using Bmd.GuildManager.Core.Events;
using Bmd.GuildManager.Core.Models;
using Bmd.GuildManager.Functions.Infrastructure;
using Bmd.GuildManager.Functions.Serialization;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Bmd.GuildManager.Functions.Functions;

public class HandleCharacterCreatedFunction(
	ICharacterRepository characterRepository,
	ILogger<HandleCharacterCreatedFunction> logger)
{
	[Function("HandleCharacterCreated")]
	public async Task RunAsync(
		[ServiceBusTrigger(ServiceBusConstants.PlayerEventsTopic, ServiceBusConstants.CharacterCreatedSubscription,
			Connection = "ServiceBusConnectionString")]
		string message,
		CancellationToken cancellationToken = default)
	{
		EventEnvelope<CharacterCreated>? envelope;
		try
		{
			envelope = JsonSerializer.Deserialize<EventEnvelope<CharacterCreated>>(
				message, FunctionJsonOptions.Default);
		}
		catch (JsonException ex)
		{
			logger.LogError(ex, "Received invalid CharacterCreated JSON — message will be dead-lettered");
			throw;
		}

		if (envelope is null)
		{
			throw new InvalidOperationException(
				"CharacterCreated message deserialized to null — message will be dead-lettered");
		}

		var data = envelope.Data;

		logger.LogInformation(
			"HandleCharacterCreated received event for character {CharacterId} " +
			"belonging to player {PlayerId}",
			data.CharacterId,
			data.PlayerId);

		var existing = await characterRepository
			.FindByCharacterIdAsync(data.CharacterId, data.PlayerId, cancellationToken);

		if (existing is not null)
		{
			logger.LogWarning(
				"Character {CharacterId} already exists, skipping duplicate",
				data.CharacterId);
			return;
		}

		var character = Character.CreateWithId(
			data.CharacterId,
			data.PlayerId,
			data.Name,
			data.Level,
			data.Strength,
			data.Luck,
			data.Endurance);

		await characterRepository.CreateAsync(character, cancellationToken);

		logger.LogInformation(
			"Character {CharacterId} persisted for player {PlayerId}",
			data.CharacterId,
			data.PlayerId);
	}
}