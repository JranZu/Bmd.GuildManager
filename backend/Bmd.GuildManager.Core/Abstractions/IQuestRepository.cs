using Bmd.GuildManager.Core.Models;

namespace Bmd.GuildManager.Core.Abstractions;

public interface IQuestRepository
{
    Task CreateAsync(Quest quest);
    Task<CosmosDocument<Quest>?> FindByQuestIdAsync(Guid questId);
    Task<IReadOnlyList<Quest>> GetAvailableQuestsAsync();
    Task<int> CountAvailableByTierAsync(string tier);
    Task UpdateAsync(Quest quest, string etag);
}
