using Bmd.GuildManager.Core.Abstractions;
using Bmd.GuildManager.Core.Constants;
using Bmd.GuildManager.Core.Events;
using Bmd.GuildManager.Core.Models;
using Bmd.GuildManager.Functions.Functions;
using Microsoft.Extensions.Logging.Abstractions;
using System.Text.Json;

namespace Bmd.GuildManager.Tests.Functions;

public class OnboardPlayerFunctionTests
{
	private static readonly JsonSerializerOptions JsonOptions = new()
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase
	};

	private static string BuildMessage(Guid playerId, string guildName)
	{
		var payload = new PlayerCreated(playerId, guildName);
		var envelope = EventEnvelope<PlayerCreated>.Create("player-service", playerId, payload);
		return JsonSerializer.Serialize(envelope, JsonOptions);
	}

	[Fact]
	public async Task RunAsync_ValidEvent_UpdatesGoldAndPublishesAllThreeEvents()
	{
		var player = Player.Create("Test Guild");
		var repository = new FakePlayerRepository();
		repository.Players.Add(player);

		var publisher = new FakeEventPublisher();
		var function = new OnboardPlayerFunction(repository, publisher, NullLogger<OnboardPlayerFunction>.Instance);

		var message = BuildMessage(player.PlayerId, player.GuildName);
		await function.RunAsync(message, TestContext.Current.CancellationToken);

		Assert.Equal(500, repository.Players[0].Gold);
		Assert.NotNull(repository.Players[0].OnboardedAt);
		Assert.Equal(3, publisher.Published.Count);
		Assert.Equal("GuildCreated", publisher.Published[0].EventType);
		Assert.Equal("StarterCharactersGranted", publisher.Published[1].EventType);
		Assert.Equal("StarterItemsGranted", publisher.Published[2].EventType);

		var charactersEvent = (StarterCharactersGranted)publisher.Published[1].Data;
		Assert.Equal(GameConstants.StarterCharacterCount, charactersEvent.CharacterIds.Count);
		Assert.All(charactersEvent.CharacterIds, id => Assert.NotEqual(Guid.Empty, id));

		var itemsEvent = (StarterItemsGranted)publisher.Published[2].Data;
		Assert.Equal(GameConstants.StarterItemCount, itemsEvent.ItemIds.Count);
		Assert.All(itemsEvent.ItemIds, id => Assert.NotEqual(Guid.Empty, id));
	}

	[Fact]
	public async Task RunAsync_AlreadyOnboarded_PublishesNoEvents()
	{
		var player = Player.Create("Test Guild") with { OnboardedAt = DateTime.UtcNow };
		var repository = new FakePlayerRepository();
		repository.Players.Add(player);

		var publisher = new FakeEventPublisher();
		var function = new OnboardPlayerFunction(repository, publisher, NullLogger<OnboardPlayerFunction>.Instance);

		var message = BuildMessage(player.PlayerId, player.GuildName);
		await function.RunAsync(message, TestContext.Current.CancellationToken);

		Assert.Empty(publisher.Published);
	}

	[Fact]
	public async Task RunAsync_PlayerNotFound_PublishesNoEvents()
	{
		var repository = new FakePlayerRepository();
		var publisher = new FakeEventPublisher();
		var function = new OnboardPlayerFunction(repository, publisher, NullLogger<OnboardPlayerFunction>.Instance);

		var message = BuildMessage(Guid.NewGuid(), "Ghost Guild");
		await function.RunAsync(message, TestContext.Current.CancellationToken);

		Assert.Empty(publisher.Published);
	}
}