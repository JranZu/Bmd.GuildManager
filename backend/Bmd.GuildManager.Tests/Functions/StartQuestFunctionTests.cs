using Bmd.GuildManager.Core.Events;
using Bmd.GuildManager.Core.Models;
using Bmd.GuildManager.Functions.Functions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using System.Text;
using System.Text.Json;

namespace Bmd.GuildManager.Tests.Functions;

public class StartQuestFunctionTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    // --- Builder helpers ---

    private static Quest BuildAvailableQuest(
        int requiredAdventurers = 1,
        string tier = "Novice")
    {
        var questId = Guid.NewGuid();
        return new Quest(
            Id: questId.ToString(),
            QuestId: questId,
            Name: "Test Quest",
            Description: "A test quest.",
            QuestType: "Kill",
            Tier: tier,
            RiskLevel: "Low",
            DifficultyRating: 20,
            RequiredAdventurers: requiredAdventurers,
            DurationSeconds: 120,
            Status: QuestStatus.Available,
            PlayerId: null,
            AssignedCharacterIds: [],
            CreatedAt: DateTimeOffset.UtcNow,
            StartedAt: null,
            EstimatedCompletionAt: null);
    }

    private static Character BuildIdleCharacter(Guid playerId) =>
        Character.CreateWithId(
            characterId: Guid.NewGuid(),
            playerId: playerId,
            name: "Aldric",
            level: 1,
            strength: 8,
            luck: 6,
            endurance: 7);

    private static HttpRequest BuildRequest(object body)
    {
        var json = JsonSerializer.Serialize(body, JsonOptions);
        var context = new DefaultHttpContext();
        context.Request.ContentType = "application/json";
        context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(json));
        return context.Request;
    }

    private static StartQuestFunction BuildFunction(
        FakeQuestRepository questRepo,
        FakeCharacterRepository characterRepo,
        FakeEventPublisher publisher,
        FakeMessageScheduler scheduler) =>
        new(questRepo, characterRepo, publisher, scheduler,
            NullLogger<StartQuestFunction>.Instance);

    // --- Happy path ---

    [Fact]
    public async Task RunAsync_ValidRequest_Returns202WithQuestId()
    {
        var playerId = Guid.NewGuid();
        var quest = BuildAvailableQuest(requiredAdventurers: 1);
        var character = BuildIdleCharacter(playerId);

        var questRepo = new FakeQuestRepository();
        var characterRepo = new FakeCharacterRepository();
        var publisher = new FakeEventPublisher();
        var scheduler = new FakeMessageScheduler();

        questRepo.Quests.Add(quest);
        characterRepo.Characters.Add(character);

        var function = BuildFunction(questRepo, characterRepo, publisher, scheduler);
        var request = BuildRequest(new
        {
            playerId = playerId,
            questId = quest.QuestId,
            characterIds = new[] { character.CharacterId }
        });

        var result = await function.RunAsync(request) as ObjectResult;

        Assert.NotNull(result);
        Assert.Equal(StatusCodes.Status202Accepted, result.StatusCode);
    }

    [Fact]
    public async Task RunAsync_ValidRequest_QuestIsMarkedInProgress()
    {
        var playerId = Guid.NewGuid();
        var quest = BuildAvailableQuest(requiredAdventurers: 1);
        var character = BuildIdleCharacter(playerId);

        var questRepo = new FakeQuestRepository();
        var characterRepo = new FakeCharacterRepository();

        questRepo.Quests.Add(quest);
        characterRepo.Characters.Add(character);

        var function = BuildFunction(questRepo, characterRepo,
            new FakeEventPublisher(), new FakeMessageScheduler());

        await function.RunAsync(BuildRequest(new
        {
            playerId = playerId,
            questId = quest.QuestId,
            characterIds = new[] { character.CharacterId }
        }));

        Assert.Equal(QuestStatus.InProgress, questRepo.Quests[0].Status);
        Assert.Equal(playerId, questRepo.Quests[0].PlayerId);
    }

    [Fact]
    public async Task RunAsync_ValidRequest_CharactersAreSetToOnQuest()
    {
        var playerId = Guid.NewGuid();
        var quest = BuildAvailableQuest(requiredAdventurers: 1);
        var character = BuildIdleCharacter(playerId);

        var questRepo = new FakeQuestRepository();
        var characterRepo = new FakeCharacterRepository();

        questRepo.Quests.Add(quest);
        characterRepo.Characters.Add(character);

        var function = BuildFunction(questRepo, characterRepo,
            new FakeEventPublisher(), new FakeMessageScheduler());

        await function.RunAsync(BuildRequest(new
        {
            playerId = playerId,
            questId = quest.QuestId,
            characterIds = new[] { character.CharacterId }
        }));

        Assert.Equal(CharacterStatus.OnQuest, characterRepo.Characters[0].Status);
    }

    [Fact]
    public async Task RunAsync_ValidRequest_ActiveQuestSnapshotIsSetOnCharacter()
    {
        var playerId = Guid.NewGuid();
        var quest = BuildAvailableQuest(requiredAdventurers: 1);
        var character = BuildIdleCharacter(playerId);

        var questRepo = new FakeQuestRepository();
        var characterRepo = new FakeCharacterRepository();

        questRepo.Quests.Add(quest);
        characterRepo.Characters.Add(character);

        var function = BuildFunction(questRepo, characterRepo,
            new FakeEventPublisher(), new FakeMessageScheduler());

        await function.RunAsync(BuildRequest(new
        {
            playerId = playerId,
            questId = quest.QuestId,
            characterIds = new[] { character.CharacterId }
        }));

        var snapshot = characterRepo.Characters[0].ActiveQuestSnapshot;
        Assert.NotNull(snapshot);
        Assert.Equal(quest.QuestId, snapshot.QuestId);
        Assert.Equal(quest.Name, snapshot.Name);
        Assert.Equal(quest.Description, snapshot.Description);
        Assert.Equal(quest.Tier, snapshot.Tier);
    }

    [Fact]
    public async Task RunAsync_ValidRequest_QuestStartedEventIsPublished()
    {
        var playerId = Guid.NewGuid();
        var quest = BuildAvailableQuest(requiredAdventurers: 1);
        var character = BuildIdleCharacter(playerId);

        var questRepo = new FakeQuestRepository();
        var characterRepo = new FakeCharacterRepository();
        var publisher = new FakeEventPublisher();

        questRepo.Quests.Add(quest);
        characterRepo.Characters.Add(character);

        var function = BuildFunction(questRepo, characterRepo,
            publisher, new FakeMessageScheduler());

        await function.RunAsync(BuildRequest(new
        {
            playerId = playerId,
            questId = quest.QuestId,
            characterIds = new[] { character.CharacterId }
        }));

        Assert.Single(publisher.Published);
        Assert.Equal("QuestStarted", publisher.Published[0].EventType);

        var data = (QuestStarted)publisher.Published[0].Data;
        Assert.Equal(quest.QuestId, data.QuestId);
        Assert.Equal(playerId, data.PlayerId);
        Assert.Equal(quest.QuestType, data.QuestType);
        Assert.Equal(quest.Tier, data.QuestTier);
    }

    [Fact]
    public async Task RunAsync_ValidRequest_QuestCompletedMessageIsScheduled()
    {
        var playerId = Guid.NewGuid();
        var quest = BuildAvailableQuest(requiredAdventurers: 1);
        var character = BuildIdleCharacter(playerId);

        var questRepo = new FakeQuestRepository();
        var characterRepo = new FakeCharacterRepository();
        var scheduler = new FakeMessageScheduler();

        questRepo.Quests.Add(quest);
        characterRepo.Characters.Add(character);

        var before = DateTimeOffset.UtcNow;

        var function = BuildFunction(questRepo, characterRepo,
            new FakeEventPublisher(), scheduler);

        await function.RunAsync(BuildRequest(new
        {
            playerId = playerId,
            questId = quest.QuestId,
            characterIds = new[] { character.CharacterId }
        }));

        Assert.Single(scheduler.Scheduled);
        var scheduled = scheduler.Scheduled[0];
        Assert.Equal("quest-completed", scheduled.QueueOrTopicName);
        Assert.True(scheduled.ScheduledEnqueueTime >
                    before.AddSeconds(quest.DurationSeconds - 1));
    }

    [Fact]
    public async Task RunAsync_ValidRequest_SharedCorrelationIdAcrossEventAndScheduledMessage()
    {
        var playerId = Guid.NewGuid();
        var quest = BuildAvailableQuest(requiredAdventurers: 1);
        var character = BuildIdleCharacter(playerId);

        var questRepo = new FakeQuestRepository();
        var characterRepo = new FakeCharacterRepository();
        var publisher = new FakeEventPublisher();
        var scheduler = new FakeMessageScheduler();

        questRepo.Quests.Add(quest);
        characterRepo.Characters.Add(character);

        var function = BuildFunction(questRepo, characterRepo, publisher, scheduler);

        await function.RunAsync(BuildRequest(new
        {
            playerId = playerId,
            questId = quest.QuestId,
            characterIds = new[] { character.CharacterId }
        }));

        // Extract correlationId from the published QuestStarted event
        var publishedCorrelationId = publisher.Published[0].CorrelationId;

        // Extract correlationId from the scheduled QuestCompleted message body
        var scheduledBody = scheduler.Scheduled[0].MessageBody;
        var scheduledEnvelope = JsonSerializer
            .Deserialize<EventEnvelope<QuestCompleted>>(scheduledBody, JsonOptions);

        Assert.NotNull(scheduledEnvelope);
        Assert.Equal(publishedCorrelationId, scheduledEnvelope.CorrelationId);
    }

    // --- Validation failures ---

    [Fact]
    public async Task RunAsync_QuestNotFound_Returns409()
    {
        var playerId = Guid.NewGuid();
        var character = BuildIdleCharacter(playerId);

        var characterRepo = new FakeCharacterRepository();
        characterRepo.Characters.Add(character);

        var function = BuildFunction(
            new FakeQuestRepository(), characterRepo,
            new FakeEventPublisher(), new FakeMessageScheduler());

        var result = await function.RunAsync(BuildRequest(new
        {
            playerId = playerId,
            questId = Guid.NewGuid(),
            characterIds = new[] { character.CharacterId }
        }));

        Assert.IsType<ConflictObjectResult>(result);
    }

    [Fact]
    public async Task RunAsync_QuestAlreadyInProgress_Returns409()
    {
        var playerId = Guid.NewGuid();
        var quest = BuildAvailableQuest(requiredAdventurers: 1) with
        { Status = QuestStatus.InProgress };
        var character = BuildIdleCharacter(playerId);

        var questRepo = new FakeQuestRepository();
        var characterRepo = new FakeCharacterRepository();

        questRepo.Quests.Add(quest);
        characterRepo.Characters.Add(character);

        var function = BuildFunction(questRepo, characterRepo,
            new FakeEventPublisher(), new FakeMessageScheduler());

        var result = await function.RunAsync(BuildRequest(new
        {
            playerId = playerId,
            questId = quest.QuestId,
            characterIds = new[] { character.CharacterId }
        }));

        Assert.IsType<ConflictObjectResult>(result);
    }

    [Fact]
    public async Task RunAsync_CharacterNotFound_Returns409()
    {
        var playerId = Guid.NewGuid();
        var quest = BuildAvailableQuest(requiredAdventurers: 1);

        var questRepo = new FakeQuestRepository();
        questRepo.Quests.Add(quest);

        var function = BuildFunction(
            questRepo, new FakeCharacterRepository(),
            new FakeEventPublisher(), new FakeMessageScheduler());

        var result = await function.RunAsync(BuildRequest(new
        {
            playerId = playerId,
            questId = quest.QuestId,
            characterIds = new[] { Guid.NewGuid() }
        }));

        Assert.IsType<ConflictObjectResult>(result);
    }

    [Fact]
    public async Task RunAsync_CharacterIsDead_Returns409()
    {
        var playerId = Guid.NewGuid();
        var quest = BuildAvailableQuest(requiredAdventurers: 1);
        var character = BuildIdleCharacter(playerId) with
        { Status = CharacterStatus.Dead };

        var questRepo = new FakeQuestRepository();
        var characterRepo = new FakeCharacterRepository();

        questRepo.Quests.Add(quest);
        characterRepo.Characters.Add(character);

        var function = BuildFunction(questRepo, characterRepo,
            new FakeEventPublisher(), new FakeMessageScheduler());

        var result = await function.RunAsync(BuildRequest(new
        {
            playerId = playerId,
            questId = quest.QuestId,
            characterIds = new[] { character.CharacterId }
        }));

        Assert.IsType<ConflictObjectResult>(result);
    }

    [Fact]
    public async Task RunAsync_CharacterIsOnQuest_Returns409()
    {
        var playerId = Guid.NewGuid();
        var quest = BuildAvailableQuest(requiredAdventurers: 1);
        var character = BuildIdleCharacter(playerId) with
        { Status = CharacterStatus.OnQuest };

        var questRepo = new FakeQuestRepository();
        var characterRepo = new FakeCharacterRepository();

        questRepo.Quests.Add(quest);
        characterRepo.Characters.Add(character);

        var function = BuildFunction(questRepo, characterRepo,
            new FakeEventPublisher(), new FakeMessageScheduler());

        var result = await function.RunAsync(BuildRequest(new
        {
            playerId = playerId,
            questId = quest.QuestId,
            characterIds = new[] { character.CharacterId }
        }));

        Assert.IsType<ConflictObjectResult>(result);
    }

    [Fact]
    public async Task RunAsync_WrongCharacterCount_Returns400()
    {
        var playerId = Guid.NewGuid();
        var quest = BuildAvailableQuest(requiredAdventurers: 2);
        var character = BuildIdleCharacter(playerId);

        var questRepo = new FakeQuestRepository();
        var characterRepo = new FakeCharacterRepository();

        questRepo.Quests.Add(quest);
        characterRepo.Characters.Add(character);

        var function = BuildFunction(questRepo, characterRepo,
            new FakeEventPublisher(), new FakeMessageScheduler());

        var result = await function.RunAsync(BuildRequest(new
        {
            playerId = playerId,
            questId = quest.QuestId,
            characterIds = new[] { character.CharacterId } // only 1, quest needs 2
        }));

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task RunAsync_InvalidJson_Returns400()
    {
        var context = new DefaultHttpContext();
        context.Request.ContentType = "application/json";
        context.Request.Body = new MemoryStream("not json"u8.ToArray());

        var function = BuildFunction(
            new FakeQuestRepository(), new FakeCharacterRepository(),
            new FakeEventPublisher(), new FakeMessageScheduler());

        var result = await function.RunAsync(context.Request);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task RunAsync_MultipleCharacters_AllSetToOnQuest()
    {
        var playerId = Guid.NewGuid();
        var quest = BuildAvailableQuest(requiredAdventurers: 3);
        var char1 = BuildIdleCharacter(playerId);
        var char2 = BuildIdleCharacter(playerId);
        var char3 = BuildIdleCharacter(playerId);

        var questRepo = new FakeQuestRepository();
        var characterRepo = new FakeCharacterRepository();

        questRepo.Quests.Add(quest);
        characterRepo.Characters.AddRange([char1, char2, char3]);

        var function = BuildFunction(questRepo, characterRepo,
            new FakeEventPublisher(), new FakeMessageScheduler());

        await function.RunAsync(BuildRequest(new
        {
            playerId = playerId,
            questId = quest.QuestId,
            characterIds = new[] { char1.CharacterId,
                                   char2.CharacterId,
                                   char3.CharacterId }
        }));

        Assert.All(characterRepo.Characters,
            c => Assert.Equal(CharacterStatus.OnQuest, c.Status));
    }
}
