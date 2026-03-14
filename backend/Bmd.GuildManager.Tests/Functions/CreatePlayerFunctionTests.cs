using Bmd.GuildManager.Core.Abstractions;
using Bmd.GuildManager.Core.Events;
using Bmd.GuildManager.Core.Models;
using Bmd.GuildManager.Functions.Functions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using System.Text;
using System.Text.Json;
using Bmd.GuildManager.Functions.Serialization;

namespace Bmd.GuildManager.Tests.Functions;

public class CreatePlayerFunctionTests
{
    private static HttpRequest BuildRequest(object body, string? idempotencyKey = null)
    {
        var json = JsonSerializer.Serialize(body, FunctionJsonOptions.Default);
        var context = new DefaultHttpContext();
        context.Request.ContentType = "application/json";
        context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(json));

        if (idempotencyKey is not null)
        {
            context.Request.Headers["Idempotency-Key"] = idempotencyKey;
        }

        return context.Request;
    }

    [Fact]
    public async Task RunAsync_ValidRequest_Returns201WithPlayerId()
    {
        var repository = new FakePlayerRepository();
        var publisher = new FakeEventPublisher();
        var function = new CreatePlayerFunction(repository, publisher, NullLogger<CreatePlayerFunction>.Instance);

        var request = BuildRequest(new { guildName = "Test Guild" });
        var result = await function.RunAsync(request) as ObjectResult;

        Assert.NotNull(result);
        Assert.Equal(StatusCodes.Status201Created, result.StatusCode);
        Assert.Single(repository.Players);
        Assert.Single(publisher.Published);
        Assert.Equal("PlayerCreated", publisher.Published[0].EventType);
    }

    [Fact]
    public async Task RunAsync_MissingGuildName_Returns400()
    {
        var function = new CreatePlayerFunction(
            new FakePlayerRepository(),
            new FakeEventPublisher(),
            NullLogger<CreatePlayerFunction>.Instance);

        var request = BuildRequest(new { guildName = "" });
        var result = await function.RunAsync(request) as BadRequestObjectResult;

        Assert.NotNull(result);
        Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
    }

    [Fact]
    public async Task RunAsync_InvalidJson_Returns400()
    {
        var context = new DefaultHttpContext();
        context.Request.ContentType = "application/json";
        context.Request.Body = new MemoryStream("not json"u8.ToArray());

        var function = new CreatePlayerFunction(
            new FakePlayerRepository(),
            new FakeEventPublisher(),
            NullLogger<CreatePlayerFunction>.Instance);

        var result = await function.RunAsync(context.Request) as BadRequestObjectResult;

        Assert.NotNull(result);
        Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
    }

    [Fact]
    public async Task RunAsync_DuplicateIdempotencyKey_UnOnboarded_ReturnsExistingAndRepublishesEvent()
    {
        var existingPlayer = Player.Create("Test Guild", "test-key-123");
        var repository = new FakePlayerRepository();
        repository.Players.Add(existingPlayer);

        var publisher = new FakeEventPublisher();
        var function = new CreatePlayerFunction(repository, publisher, NullLogger<CreatePlayerFunction>.Instance);

        var request = BuildRequest(new { guildName = "Test Guild" }, "test-key-123");
        var result = await function.RunAsync(request) as ObjectResult;

        Assert.NotNull(result);
        Assert.Equal(StatusCodes.Status201Created, result.StatusCode);
        Assert.Single(repository.Players);
        Assert.Single(publisher.Published);
        Assert.Equal("PlayerCreated", publisher.Published[0].EventType);

        var data = (PlayerCreated)publisher.Published[0].Data;
        Assert.Equal(existingPlayer.PlayerId, data.PlayerId);
        Assert.Equal(existingPlayer.GuildName, data.GuildName);
    }

    [Fact]
    public async Task RunAsync_DuplicateIdempotencyKey_AlreadyOnboarded_ReturnsExistingWithoutRepublish()
    {
        var existingPlayer = Player.Create("Test Guild", "test-key-123")
            with { OnboardedAt = DateTimeOffset.UtcNow };
        var repository = new FakePlayerRepository();
        repository.Players.Add(existingPlayer);

        var publisher = new FakeEventPublisher();
        var function = new CreatePlayerFunction(repository, publisher, NullLogger<CreatePlayerFunction>.Instance);

        var request = BuildRequest(new { guildName = "Test Guild" }, "test-key-123");
        var result = await function.RunAsync(request) as ObjectResult;

        Assert.NotNull(result);
        Assert.Equal(StatusCodes.Status201Created, result.StatusCode);
        Assert.Single(repository.Players);
        Assert.Empty(publisher.Published);
    }
}


