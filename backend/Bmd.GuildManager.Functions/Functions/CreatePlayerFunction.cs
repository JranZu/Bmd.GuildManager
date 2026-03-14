using System.Text.Json;
using Bmd.GuildManager.Core.Abstractions;
using Bmd.GuildManager.Core.Events;
using Bmd.GuildManager.Core.Models;
using Bmd.GuildManager.Core.Models.Requests;
using Bmd.GuildManager.Functions.Infrastructure;
using Bmd.GuildManager.Functions.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Bmd.GuildManager.Functions.Functions;

public class CreatePlayerFunction(
    IPlayerRepository playerRepository,
	[FromKeyedServices(ServiceBusConstants.PlayerEventsTopic)] IEventPublisher eventPublisher,
    ILogger<CreatePlayerFunction> logger)
{
    [Function("CreatePlayer")]
    public async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "players")] HttpRequest req)
    {
        var ct = req.HttpContext.RequestAborted;

        CreatePlayerRequest? request;
        try
        {
            request = await JsonSerializer.DeserializeAsync<CreatePlayerRequest>(req.Body, FunctionJsonOptions.Default);
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

        // Always ensure an idempotency key exists so the re-publish recovery
        // path works even when the caller doesn't provide one.
        idempotencyKey = string.IsNullOrWhiteSpace(idempotencyKey)
            ? Guid.NewGuid().ToString()
            : idempotencyKey;

        if (!string.IsNullOrWhiteSpace(idempotencyKey))
        {
            var existing = await playerRepository.FindByIdempotencyKeyAsync(idempotencyKey, ct);
            if (existing is not null)
            {
                logger.LogInformation(
                    "Duplicate request detected for idempotency key {Key}, returning existing player {PlayerId}",
                    idempotencyKey,
                    existing.PlayerId);

                // If the player was never onboarded the original PlayerCreated
                // event may have been lost (CreateAsync succeeded but PublishAsync
                // threw). Re-publish so onboarding can proceed.
                if (existing.OnboardedAt is null)
                {
                    var retryPayload = new PlayerCreated(existing.PlayerId, existing.GuildName);
                    var retryEnvelope = EventEnvelope<PlayerCreated>.Create(
                        source: "CreatePlayerFunction",
                        correlationId: existing.PlayerId,
                        data: retryPayload);

                    await eventPublisher.PublishAsync(retryEnvelope, ct);

                    logger.LogInformation(
                        "Re-published PlayerCreated event for un-onboarded player {PlayerId}",
                        existing.PlayerId);
                }

                return new ObjectResult(new { playerId = existing.PlayerId })
                {
                    StatusCode = StatusCodes.Status201Created
                };
            }
        }

        var player = Player.Create(request.GuildName, idempotencyKey);
        await playerRepository.CreateAsync(player, ct);

        logger.LogInformation(
            "Player {PlayerId} created with guild {GuildName}",
            player.PlayerId,
            player.GuildName);

        var eventPayload = new PlayerCreated(player.PlayerId, player.GuildName);
        var envelope = EventEnvelope<PlayerCreated>.Create(
            source: "CreatePlayerFunction",
            correlationId: player.PlayerId,
            data: eventPayload);

        await eventPublisher.PublishAsync(envelope, ct);

        logger.LogInformation("PlayerCreated event published for {PlayerId}", player.PlayerId);

        return new ObjectResult(new { playerId = player.PlayerId })
        {
            StatusCode = StatusCodes.Status201Created
        };
    }
}
