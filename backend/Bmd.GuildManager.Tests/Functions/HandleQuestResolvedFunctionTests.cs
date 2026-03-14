using Bmd.GuildManager.Core.Events;
using Bmd.GuildManager.Core.Models;
using Bmd.GuildManager.Functions.Functions;
using Microsoft.Extensions.Logging.Abstractions;
using System.Text.Json;
using Bmd.GuildManager.Functions.Serialization;

namespace Bmd.GuildManager.Tests.Functions;

public class HandleQuestResolvedFunctionTests
{
    private static Character BuildOnQuestCharacter(Guid playerId, Guid questId) =>
        Character.Create(playerId, "Hero", level: 1, strength: 5, luck: 5, endurance: 5) with
        {
            Status = CharacterStatus.OnQuest,
            ActiveQuestSnapshot = new ActiveQuestSnapshot(
                questId, "Test Quest", "A quest.", DifficultyTier.Novice,
                DateTimeOffset.UtcNow.AddMinutes(5))
        };

    private static string BuildMessage(
        Guid questId,
        Guid playerId,
        IReadOnlyList<QuestResolvedCharacter> characters,
        int xpAwarded = 25)
    {
        var payload = new QuestResolved(
            questId, playerId, DifficultyTier.Novice, "Success",
            xpAwarded, characters, true, 20);
        var envelope = EventEnvelope<QuestResolved>.Create(
            "test", Guid.NewGuid(), payload);
        return JsonSerializer.Serialize(envelope, FunctionJsonOptions.Default);
    }

    [Fact]
    public async Task RunAsync_Survivor_SetsStatusIdleAndClearsSnapshot()
    {
        var playerId = Guid.NewGuid();
        var questId = Guid.NewGuid();
        var character = BuildOnQuestCharacter(playerId, questId);

        var repo = new FakeCharacterRepository();
        repo.Characters.Add(character);

        var function = new HandleQuestResolvedFunction(
            repo, NullLogger<HandleQuestResolvedFunction>.Instance);

        var message = BuildMessage(questId, playerId,
            [new QuestResolvedCharacter(character.CharacterId, Survived: true)]);

        await function.RunAsync(message, TestContext.Current.CancellationToken);

        var updated = repo.Characters[0];
        Assert.Equal(CharacterStatus.Idle, updated.Status);
        Assert.Null(updated.ActiveQuestSnapshot);
    }

    [Fact]
    public async Task RunAsync_Survivor_AppliesXp()
    {
        var playerId = Guid.NewGuid();
        var questId = Guid.NewGuid();
        var character = BuildOnQuestCharacter(playerId, questId);

        var repo = new FakeCharacterRepository();
        repo.Characters.Add(character);

        var function = new HandleQuestResolvedFunction(
            repo, NullLogger<HandleQuestResolvedFunction>.Instance);

        await function.RunAsync(BuildMessage(questId, playerId,
            [new QuestResolvedCharacter(character.CharacterId, Survived: true)],
            xpAwarded: 25), TestContext.Current.CancellationToken);

        Assert.Equal(25, repo.Characters[0].Xp);
    }

    [Fact]
    public async Task RunAsync_XpCrossesThreshold_LevelsUp()
    {
        var playerId = Guid.NewGuid();
        var questId = Guid.NewGuid();
        var character = BuildOnQuestCharacter(playerId, questId) with { Xp = 90 };

        var repo = new FakeCharacterRepository();
        repo.Characters.Add(character);

        var function = new HandleQuestResolvedFunction(
            repo, NullLogger<HandleQuestResolvedFunction>.Instance);

        // 90 + 25 = 115, crosses the 100 threshold ? Level 2
        await function.RunAsync(BuildMessage(questId, playerId,
            [new QuestResolvedCharacter(character.CharacterId, Survived: true)],
            xpAwarded: 25), TestContext.Current.CancellationToken);

        Assert.Equal(2, repo.Characters[0].Level);
        Assert.Equal(115, repo.Characters[0].Xp);
    }

    [Fact]
    public async Task RunAsync_DeadCharacter_IsNotUpdated()
    {
        var playerId = Guid.NewGuid();
        var questId = Guid.NewGuid();
        var character = BuildOnQuestCharacter(playerId, questId);

        var repo = new FakeCharacterRepository();
        repo.Characters.Add(character);

        var function = new HandleQuestResolvedFunction(
            repo, NullLogger<HandleQuestResolvedFunction>.Instance);

        // Character is marked survived=false
        await function.RunAsync(BuildMessage(questId, playerId,
            [new QuestResolvedCharacter(character.CharacterId, Survived: false)]), TestContext.Current.CancellationToken);

        // Character should be completely untouched
        var unchanged = repo.Characters[0];
        Assert.Equal(CharacterStatus.OnQuest, unchanged.Status);
        Assert.NotNull(unchanged.ActiveQuestSnapshot);
        Assert.Equal(0, unchanged.Xp);
    }

    [Fact]
    public async Task RunAsync_Idempotency_AlreadyProcessed_SkipsUpdate()
    {
        var playerId = Guid.NewGuid();
        var questId = Guid.NewGuid();

        // Simulate a character that was already processed —
        // snapshot cleared, status Idle
        var character = Character.Create(
            playerId, "Hero", level: 1, strength: 5, luck: 5, endurance: 5) with
        {
            Status = CharacterStatus.Idle,
            ActiveQuestSnapshot = null,
            Xp = 25
        };

        var repo = new FakeCharacterRepository();
        repo.Characters.Add(character);

        var function = new HandleQuestResolvedFunction(
            repo, NullLogger<HandleQuestResolvedFunction>.Instance);

        await function.RunAsync(BuildMessage(questId, playerId,
            [new QuestResolvedCharacter(character.CharacterId, Survived: true)],
            xpAwarded: 25), TestContext.Current.CancellationToken);

        // XP must not be applied a second time
        Assert.Equal(25, repo.Characters[0].Xp);
    }

    [Fact]
    public async Task RunAsync_InvalidJson_Throws()
    {
        var function = new HandleQuestResolvedFunction(
            new FakeCharacterRepository(),
            NullLogger<HandleQuestResolvedFunction>.Instance);

        await Assert.ThrowsAsync<JsonException>(() =>
            function.RunAsync("this is not json", TestContext.Current.CancellationToken));
    }

	[Fact]
	public async Task RunAsync_NullCharactersList_Throws()
	{
		// Simulates a message from a wrong event type leaking through
		// (e.g. QuestStarted deserialized as QuestResolved)
		var payload = new QuestResolved(
			Guid.NewGuid(), Guid.NewGuid(), DifficultyTier.Novice,
			null!, 0, null!, false, 0);
		var envelope = EventEnvelope<QuestResolved>.Create("test", Guid.NewGuid(), payload);
		var message = JsonSerializer.Serialize(envelope, FunctionJsonOptions.Default);

		var function = new HandleQuestResolvedFunction(
			new FakeCharacterRepository(),
			NullLogger<HandleQuestResolvedFunction>.Instance);

		await Assert.ThrowsAsync<InvalidOperationException>(() =>
			function.RunAsync(message, TestContext.Current.CancellationToken));
	}
}
