using Bmd.GuildManager.Core.Models;

namespace Bmd.GuildManager.Core.Abstractions;

public interface IQuestRepository
{
    Task CreateAsync(Quest quest, CancellationToken cancellationToken = default);
    Task<CosmosDocument<Quest>?> FindByQuestIdAsync(Guid questId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Quest>> GetAvailableQuestsAsync(CancellationToken cancellationToken = default);
    Task<int> CountAvailableByTierAsync(DifficultyTier tier, CancellationToken cancellationToken = default);
    Task UpdateAsync(Quest quest, string etag, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid questId, CancellationToken cancellationToken = default);
}
