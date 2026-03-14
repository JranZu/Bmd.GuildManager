using System.Text.Json;
using Bmd.GuildManager.Core.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Bmd.GuildManager.Functions.Functions;

public class GetCharactersFunction(
	ICharacterRepository characterRepository,
	ILogger<GetCharactersFunction> logger)
{
	[Function("GetCharacters")]
	public async Task<IActionResult> RunAsync(
		[HttpTrigger(AuthorizationLevel.Function, "get",
			Route = "players/{playerId}/characters")] HttpRequest req,
		Guid playerId)
	{
		var ct = req.HttpContext.RequestAborted;

		logger.LogInformation(
			"GetCharacters called for player {PlayerId}", playerId);

		var characters = await characterRepository.GetByPlayerIdAsync(playerId, ct);

		logger.LogInformation(
			"Returning {Count} characters for player {PlayerId}",
			characters.Count,
			playerId);

		return new OkObjectResult(characters);
	}
}