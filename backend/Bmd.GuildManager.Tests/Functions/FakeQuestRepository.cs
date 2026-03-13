using Bmd.GuildManager.Core.Abstractions;
using Bmd.GuildManager.Core.Models;

namespace Bmd.GuildManager.Tests.Functions;

public class FakeQuestRepository : IQuestRepository
{
    public List<Quest> Quests { get; } = [];

    public Task CreateAsync(Quest quest)
    {
        Quests.Add(quest);
        return Task.CompletedTask;
    }

    public Task<CosmosDocument<Quest>?> FindByQuestIdAsync(Guid questId)
    {
        var match = Quests.FirstOrDefault(q => q.QuestId == questId);
        return Task.FromResult(
            match is null ? null : new CosmosDocument<Quest>(match, "fake-etag"));
    }

    public Task<IReadOnlyList<Quest>> GetAvailableQuestsAsync()
    {
        var results = Quests
            .Where(q => q.Status == QuestStatus.Available)
            .ToList()
            .AsReadOnly();
        return Task.FromResult<IReadOnlyList<Quest>>(results);
    }

    public Task<int> CountAvailableByTierAsync(DifficultyTier tier)
    {
        var count = Quests.Count(q =>
            q.Status == QuestStatus.Available &&
            q.Tier == tier);
        return Task.FromResult(count);
    }

    public Task UpdateAsync(Quest quest, string etag)
    {
        var index = Quests.FindIndex(q => q.QuestId == quest.QuestId);
        if (index >= 0)
            Quests[index] = quest;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid questId)
    {
        Quests.RemoveAll(q => q.QuestId == questId);
        return Task.CompletedTask;
    }
}
