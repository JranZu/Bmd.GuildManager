using System.Text.Json;
using Bmd.GuildManager.Core.Abstractions;
using Bmd.GuildManager.Core.Events;
using Bmd.GuildManager.Core.Models;
using Bmd.GuildManager.Core.Models.Requests;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Bmd.GuildManager.Functions.Functions;

public class CreatePlayerFunction(
    IPlayerRepository playerRepository,
	[FromKeyedServices("player-events")] IEventPublisher eventPublisher,
    ILogger<CreatePlayerFunction> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    [Function("CreatePlayer")]
    public async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "players")] HttpRequest req)
    {
        CreatePlayerRequest? request;
        try
        {
            request = await JsonSerializer.DeserializeAsync<CreatePlayerRequest>(req.Body, JsonOptions);
        }
        catch (JsonException)
        {
            return new BadRequestObjectResult("Request body is invalid JSON.");
        }

        if (string.IsNullOrWhiteSpace(request?.GuildName))
        {
            return new BadRequestObjectResult("guildName is required.");
        }

        var idempotencyKey = req.Headers.TryGetValue("Idempotency-Key", out var keyValues)
            ? keyValues.ToString()
            : null;

        if (!string.IsNullOrWhiteSpace(idempotencyKey))
        {
            var existing = await playerRepository.FindByIdempotencyKeyAsync(idempotencyKey);
            if (existing is not null)
            {
                logger.LogInformation(
                    "Duplicate request detected for idempotency key {Key}, returning existing player {PlayerId}",
                    idempotencyKey,
                    existing.PlayerId);

                return new ObjectResult(new { playerId = existing.PlayerId })
                {
                    StatusCode = StatusCodes.Status201Created
                };
            }
        }

        var player = Player.Create(request.GuildName, idempotencyKey);
        await playerRepository.CreateAsync(player);

        logger.LogInformation(
            "Player {PlayerId} created with guild {GuildName}",
            player.PlayerId,
            player.GuildName);

        var eventPayload = new PlayerCreated(player.PlayerId, player.GuildName);
        var envelope = EventEnvelope<PlayerCreated>.Create(
            source: "CreatePlayerFunction",
            correlationId: player.PlayerId,
            data: eventPayload);

        await eventPublisher.PublishAsync(envelope);

        logger.LogInformation("PlayerCreated event published for {PlayerId}", player.PlayerId);

        return new ObjectResult(new { playerId = player.PlayerId })
        {
            StatusCode = StatusCodes.Status201Created
        };
    }
}
