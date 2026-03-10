using Bmd.GuildManager.Core.Abstractions;
using Bmd.GuildManager.Core.Models;
using Microsoft.Azure.Cosmos;

namespace Bmd.GuildManager.Functions.Repositories;

public class CosmosPlayerRepository(CosmosClient cosmosClient) : IPlayerRepository
{
    private const string DatabaseName = "guildmanager";
    private const string ContainerName = "Players";

    private Container Container => cosmosClient.GetContainer(DatabaseName, ContainerName);

	public async Task CreateAsync(Player player)
	{
		await Container.CreateItemAsync(player, new PartitionKey(player.PlayerId.ToString()));
	}

	public async Task UpdateAsync(Player player)
	{
		await Container.ReplaceItemAsync(
			player,
			player.Id,
			new PartitionKey(player.PlayerId.ToString()));
	}

	public async Task<Player?> FindByPlayerIdAsync(Guid playerId)
	{
		try
		{
			var response = await Container.ReadItemAsync<Player>(
				playerId.ToString(),
				new PartitionKey(playerId.ToString()));
			return response.Resource;
		}
		catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
		{
			return null;
		}
	}

	public async Task<Player?> FindByIdempotencyKeyAsync(string idempotencyKey)
	{
		var query = new QueryDefinition("SELECT * FROM c WHERE c.idempotencyKey = @key")
			.WithParameter("@key", idempotencyKey);

		var iterator = Container.GetItemQueryIterator<Player>(query);

		while (iterator.HasMoreResults)
		{
			var page = await iterator.ReadNextAsync();
			var existing = page.FirstOrDefault();
			if (existing is not null)
			{
				return existing;
			}
		}

		return null;
	}
}
