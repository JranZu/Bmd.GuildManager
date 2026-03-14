using Bmd.GuildManager.Core.Abstractions;
using Bmd.GuildManager.Core.Models;
using Bmd.GuildManager.Functions.Infrastructure;
using Microsoft.Azure.Cosmos;

namespace Bmd.GuildManager.Functions.Repositories;

public class CosmosQuestRepository(CosmosClient cosmosClient) : IQuestRepository
{
    private const string ContainerName = "Quests";

    private readonly Container _container =
        cosmosClient.GetContainer(CosmosConstants.DatabaseName, ContainerName);

    public async Task CreateAsync(Quest quest, CancellationToken cancellationToken = default)
    {
        await _container.CreateItemAsync(
            quest,
            new PartitionKey(quest.QuestId.ToString()),
            cancellationToken: cancellationToken);
    }

    public async Task<CosmosDocument<Quest>?> FindByQuestIdAsync(Guid questId, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _container.ReadItemAsync<Quest>(
                questId.ToString(),
                new PartitionKey(questId.ToString()),
                cancellationToken: cancellationToken);
            return new CosmosDocument<Quest>(response.Resource, response.ETag);
        }
        catch (CosmosException ex)
            when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<IReadOnlyList<Quest>> GetAvailableQuestsAsync(CancellationToken cancellationToken = default)
    {
        var query = new QueryDefinition(
            "SELECT * FROM c WHERE c.status = @status")
            .WithParameter("@status", nameof(QuestStatus.Available));

        var iterator = _container.GetItemQueryIterator<Quest>(query);
        var results = new List<Quest>();

        while (iterator.HasMoreResults)
        {
            var page = await iterator.ReadNextAsync(cancellationToken);
            results.AddRange(page);
        }

        return results.AsReadOnly();
    }

    public async Task<int> CountAvailableByTierAsync(DifficultyTier tier, CancellationToken cancellationToken = default)
    {
        var query = new QueryDefinition(
            "SELECT VALUE COUNT(1) FROM c WHERE c.status = @status AND c.tier = @tier")
            .WithParameter("@status", nameof(QuestStatus.Available))
            .WithParameter("@tier", tier.ToString());

        var iterator = _container.GetItemQueryIterator<int>(query);
        var result = await iterator.ReadNextAsync(cancellationToken);
        return result.FirstOrDefault();
    }

    public async Task UpdateAsync(Quest quest, string etag, CancellationToken cancellationToken = default)
    {
        var options = new ItemRequestOptions
        {
            IfMatchEtag = etag
        };

        await _container.ReplaceItemAsync(
            quest,
            quest.Id,
            new PartitionKey(quest.QuestId.ToString()),
            options,
            cancellationToken);
    }

    public async Task DeleteAsync(Guid questId, CancellationToken cancellationToken = default)
    {
        try
        {
            await _container.DeleteItemAsync<Quest>(
                questId.ToString(),
                new PartitionKey(questId.ToString()),
                cancellationToken: cancellationToken);
        }
        catch (CosmosException ex)
            when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            // Already deleted — idempotent, safe to ignore
        }
    }
}
