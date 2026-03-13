using Bmd.GuildManager.Core.Abstractions;
using Bmd.GuildManager.Core.Models;
using Microsoft.Azure.Cosmos;

namespace Bmd.GuildManager.Functions.Repositories;

public class CosmosQuestRepository(CosmosClient cosmosClient) : IQuestRepository
{
    private const string DatabaseName = "guildmanager";
    private const string ContainerName = "Quests";

    private Container Container =>
        cosmosClient.GetContainer(DatabaseName, ContainerName);

    public async Task CreateAsync(Quest quest)
    {
        await Container.CreateItemAsync(
            quest,
            new PartitionKey(quest.QuestId.ToString()));
    }

    public async Task<CosmosDocument<Quest>?> FindByQuestIdAsync(Guid questId)
    {
        try
        {
            var response = await Container.ReadItemAsync<Quest>(
                questId.ToString(),
                new PartitionKey(questId.ToString()));
            return new CosmosDocument<Quest>(response.Resource, response.ETag);
        }
        catch (CosmosException ex)
            when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<IReadOnlyList<Quest>> GetAvailableQuestsAsync()
    {
        var query = new QueryDefinition(
            "SELECT * FROM c WHERE c.status = @status")
            .WithParameter("@status", nameof(QuestStatus.Available));

        var iterator = Container.GetItemQueryIterator<Quest>(query);
        var results = new List<Quest>();

        while (iterator.HasMoreResults)
        {
            var page = await iterator.ReadNextAsync();
            results.AddRange(page);
        }

        return results.AsReadOnly();
    }

    public async Task<int> CountAvailableByTierAsync(DifficultyTier tier)
    {
        var query = new QueryDefinition(
            "SELECT VALUE COUNT(1) FROM c WHERE c.status = @status AND c.tier = @tier")
            .WithParameter("@status", nameof(QuestStatus.Available))
            .WithParameter("@tier", tier.ToString());

        var iterator = Container.GetItemQueryIterator<int>(query);
        var result = await iterator.ReadNextAsync();
        return result.FirstOrDefault();
    }

    public async Task UpdateAsync(Quest quest, string etag)
    {
        var options = new ItemRequestOptions
        {
            IfMatchEtag = etag
        };

        await Container.ReplaceItemAsync(
            quest,
            quest.Id,
            new PartitionKey(quest.QuestId.ToString()),
            options);
    }

    public async Task DeleteAsync(Guid questId)
    {
        try
        {
            await Container.DeleteItemAsync<Quest>(
                questId.ToString(),
                new PartitionKey(questId.ToString()));
        }
        catch (CosmosException ex)
            when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            // Already deleted — idempotent, safe to ignore
        }
    }
}
