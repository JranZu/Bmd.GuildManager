using Bmd.GuildManager.Core.Abstractions;
using Bmd.GuildManager.Core.Constants;
using Bmd.GuildManager.Core.Events;
using Bmd.GuildManager.Core.Models;
using Bmd.GuildManager.Functions.Functions;
using Microsoft.Extensions.Logging.Abstractions;
using System.Text.Json;
using Bmd.GuildManager.Functions.Serialization;

namespace Bmd.GuildManager.Tests.Functions;

public class OnboardPlayerFunctionTests
{
	private static string BuildMessage(Guid playerId, string guildName)
	{
		var payload = new PlayerCreated(playerId, guildName);
		var envelope = EventEnvelope<PlayerCreated>.Create("player-service", playerId, payload);
		return JsonSerializer.Serialize(envelope, FunctionJsonOptions.Default);
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

		Assert.Equal(GameConstants.StartingGold, repository.Players[0].Gold);
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

	[Fact]
	public async Task RunAsync_PublishFails_OnboardedAtRemainsNull()
	{
		var player = Player.Create("Test Guild");
		var repository = new FakePlayerRepository();
		repository.Players.Add(player);

		var publisher = new FakeEventPublisher { FailOnPublish = true };
		var function = new OnboardPlayerFunction(repository, publisher, NullLogger<OnboardPlayerFunction>.Instance);

		var message = BuildMessage(player.PlayerId, player.GuildName);
		await Assert.ThrowsAsync<InvalidOperationException>(
			() => function.RunAsync(message, TestContext.Current.CancellationToken));

		Assert.Null(repository.Players[0].OnboardedAt);
	}

	[Fact]
	public async Task RunAsync_EventsPublishBeforeOnboardedAtIsSet()
	{
		var player = Player.Create("Test Guild");
		var repository = new OrderTrackingPlayerRepository();
		repository.Players.Add(player);

		var publisher = new OrderTrackingEventPublisher(repository);
		var function = new OnboardPlayerFunction(repository, publisher, NullLogger<OnboardPlayerFunction>.Instance);

		var message = BuildMessage(player.PlayerId, player.GuildName);
		await function.RunAsync(message, TestContext.Current.CancellationToken);

		// All three events should have been published while OnboardedAt was still null
		Assert.Equal(3, publisher.OnboardedAtDuringPublish.Count);
		Assert.All(publisher.OnboardedAtDuringPublish, snapshot => Assert.Null(snapshot));
	}

	[Fact]
	public async Task RunAsync_InvalidJson_Throws()
	{
		var repository = new FakePlayerRepository();
		var publisher = new FakeEventPublisher();
		var function = new OnboardPlayerFunction(repository, publisher, NullLogger<OnboardPlayerFunction>.Instance);

		await Assert.ThrowsAsync<JsonException>(() =>
			function.RunAsync("this is not json", TestContext.Current.CancellationToken));
	}

	/// <summary>
	/// Fake that records whether OnboardedAt was set at each publish call.
	/// </summary>
	private sealed class OrderTrackingEventPublisher(OrderTrackingPlayerRepository repository) : IEventPublisher
	{
		public List<DateTimeOffset?> OnboardedAtDuringPublish { get; } = [];

		public Task PublishAsync<T>(EventEnvelope<T> envelope, CancellationToken cancellationToken = default)
		{
			// Snapshot the player's OnboardedAt at the moment of each publish
			var player = repository.Players.First();
			OnboardedAtDuringPublish.Add(player.OnboardedAt);
			return Task.CompletedTask;
		}
	}

	/// <summary>
	/// A FakePlayerRepository subclass that supports order tracking.
	/// </summary>
	private sealed class OrderTrackingPlayerRepository : FakePlayerRepository;
}