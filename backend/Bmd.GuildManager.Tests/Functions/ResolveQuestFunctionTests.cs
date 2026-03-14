using Bmd.GuildManager.Core.Events;
using Bmd.GuildManager.Core.Models;
using Bmd.GuildManager.Core.Services;
using Bmd.GuildManager.Functions.Functions;
using Bmd.GuildManager.Functions.Serialization;
using Microsoft.Extensions.Logging.Abstractions;
using System.Text.Json;

namespace Bmd.GuildManager.Tests.Functions;

public class ResolveQuestFunctionTests
{
	private static Quest BuildInProgressQuest(Guid playerId, IReadOnlyList<Guid> characterIds)
	{
		var questId = Guid.NewGuid();
		return new Quest(
			Id: questId.ToString(),
			QuestId: questId,
			Name: "Test Quest",
			Description: "A test quest.",
			QuestType: QuestType.Kill,
			Tier: DifficultyTier.Novice,
			RiskLevel: RiskLevel.Low,
			DifficultyRating: 20,
			RequiredAdventurers: characterIds.Count,
			DurationSeconds: 120,
			Status: QuestStatus.InProgress,
			PlayerId: playerId,
			AssignedCharacterIds: characterIds,
			CreatedAt: DateTimeOffset.UtcNow.AddMinutes(-5),
			StartedAt: DateTimeOffset.UtcNow.AddMinutes(-2),
			EstimatedCompletionAt: DateTimeOffset.UtcNow);
	}

	private static string BuildMessage(Guid questId, Guid playerId)
	{
		var payload = new QuestCompleted(questId, playerId);
		var envelope = EventEnvelope<QuestCompleted>.Create(
			"test", Guid.NewGuid(), payload);
		return JsonSerializer.Serialize(envelope, FunctionJsonOptions.Default);
	}

	private static ResolveQuestFunction BuildFunction(
		FakeQuestRepository questRepo,
		FakeCharacterRepository characterRepo,
		FakeEventPublisher publisher)
	{
		// FakeRandomProvider with deterministic values:
		// NextDouble(0.75, 1.25) for jitter → needs NextDouble() values
		// NextDouble() for death roll
		// Values chosen to produce a Success outcome with no deaths
		var random = new FakeRandomProvider(0.5, 0.99, 0.5, 0.5);
		var resolutionService = new QuestResolutionService(random);

		return new ResolveQuestFunction(
			questRepo,
			characterRepo,
			publisher,
			new FakeBlobServiceClient(),
			resolutionService,
			NullLogger<ResolveQuestFunction>.Instance);
	}

	[Fact]
	public async Task RunAsync_IdleCharacterAtResolution_IsSkippedFromRewards()
	{
		var playerId = Guid.NewGuid();
		var onQuestCharacter = Character.Create(
			playerId, "Warrior", level: 3, strength: 10, luck: 5, endurance: 8) with
		{
			Status = CharacterStatus.OnQuest,
			ActiveQuestSnapshot = new ActiveQuestSnapshot(
				Guid.Empty, "Test Quest", "A quest.", DifficultyTier.Novice,
				DateTimeOffset.UtcNow)
		};
		var idleCharacter = Character.Create(
			playerId, "Rogue", level: 2, strength: 6, luck: 8, endurance: 4) with
		{
			Status = CharacterStatus.Idle
		};

		var quest = BuildInProgressQuest(playerId,
			[onQuestCharacter.CharacterId, idleCharacter.CharacterId]);

		var questRepo = new FakeQuestRepository();
		questRepo.Quests.Add(quest);

		var characterRepo = new FakeCharacterRepository();
		characterRepo.Characters.Add(onQuestCharacter);
		characterRepo.Characters.Add(idleCharacter);

		var publisher = new FakeEventPublisher();
		var function = BuildFunction(questRepo, characterRepo, publisher);
		var message = BuildMessage(quest.QuestId, playerId);

		await function.RunAsync(message, TestContext.Current.CancellationToken);

		// QuestResolved should have been published
		Assert.Single(publisher.Published);

		var resolved = (QuestResolved)publisher.Published[0].Data;

		// Only the OnQuest character should be included; the Idle one should be skipped
		Assert.Single(resolved.Characters);
		Assert.Equal(onQuestCharacter.CharacterId, resolved.Characters[0].CharacterId);
	}

	[Fact]
	public async Task RunAsync_AllCharactersIdle_ArchivesWithoutResolution()
	{
		var playerId = Guid.NewGuid();
		var idleCharacter = Character.Create(
			playerId, "Rogue", level: 2, strength: 6, luck: 8, endurance: 4) with
		{
			Status = CharacterStatus.Idle
		};

		var quest = BuildInProgressQuest(playerId, [idleCharacter.CharacterId]);

		var questRepo = new FakeQuestRepository();
		questRepo.Quests.Add(quest);

		var characterRepo = new FakeCharacterRepository();
		characterRepo.Characters.Add(idleCharacter);

		var publisher = new FakeEventPublisher();
		var function = BuildFunction(questRepo, characterRepo, publisher);
		var message = BuildMessage(quest.QuestId, playerId);

		await function.RunAsync(message, TestContext.Current.CancellationToken);

		// No QuestResolved event should be published — all characters were skipped
		Assert.Empty(publisher.Published);

		// Quest should be archived (deleted from Cosmos)
		Assert.Empty(questRepo.Quests);
	}

	[Fact]
	public async Task RunAsync_OnQuestCharacter_IsIncludedInResolution()
	{
		var playerId = Guid.NewGuid();
		var onQuestCharacter = Character.Create(
			playerId, "Warrior", level: 3, strength: 10, luck: 5, endurance: 8) with
		{
			Status = CharacterStatus.OnQuest,
			ActiveQuestSnapshot = new ActiveQuestSnapshot(
				Guid.Empty, "Test Quest", "A quest.", DifficultyTier.Novice,
				DateTimeOffset.UtcNow)
		};

		var quest = BuildInProgressQuest(playerId, [onQuestCharacter.CharacterId]);

		var questRepo = new FakeQuestRepository();
		questRepo.Quests.Add(quest);

		var characterRepo = new FakeCharacterRepository();
		characterRepo.Characters.Add(onQuestCharacter);

		var publisher = new FakeEventPublisher();
		var function = BuildFunction(questRepo, characterRepo, publisher);
		var message = BuildMessage(quest.QuestId, playerId);

		await function.RunAsync(message, TestContext.Current.CancellationToken);

		Assert.Single(publisher.Published);
		var resolved = (QuestResolved)publisher.Published[0].Data;
		Assert.Single(resolved.Characters);
		Assert.Equal(onQuestCharacter.CharacterId, resolved.Characters[0].CharacterId);
	}

	[Fact]
	public async Task RunAsync_InvalidJson_Throws()
	{
		var questRepo = new FakeQuestRepository();
		var characterRepo = new FakeCharacterRepository();
		var publisher = new FakeEventPublisher();
		var function = BuildFunction(questRepo, characterRepo, publisher);

		await Assert.ThrowsAsync<JsonException>(() =>
			function.RunAsync("this is not json", TestContext.Current.CancellationToken));
	}
}
