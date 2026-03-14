using Bmd.GuildManager.Core.Abstractions;
using Bmd.GuildManager.Core.Models;
using Microsoft.Azure.Cosmos;

namespace Bmd.GuildManager.Functions.Repositories;

public class CosmosPlayerRepository(CosmosClient cosmosClient) : IPlayerRepository
{
    private const string DatabaseName = "guildmanager";
    private const string ContainerName = "Players";

    private Container Container => cosmosClient.GetContainer(DatabaseName, ContainerName);

	public async Task CreateAsync(Player player, CancellationToken cancellationToken = default)
	{
		await Container.CreateItemAsync(player, new PartitionKey(player.PlayerId.ToString()), cancellationToken: cancellationToken);
	}

	public async Task UpdateAsync(Player player, string etag, CancellationToken cancellationToken = default)
	{
		var options = new ItemRequestOptions
		{
			IfMatchEtag = etag
		};

		await Container.ReplaceItemAsync(
			player,
			player.Id,
			new PartitionKey(player.PlayerId.ToString()),
			options,
			cancellationToken);
	}

	public async Task<CosmosDocument<Player>?> FindByPlayerIdAsync(Guid playerId, CancellationToken cancellationToken = default)
	{
		try
		{
			var response = await Container.ReadItemAsync<Player>(
				playerId.ToString(),
				new PartitionKey(playerId.ToString()),
				cancellationToken: cancellationToken);
			return new CosmosDocument<Player>(response.Resource, response.ETag);
		}
		catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
		{
			return null;
		}
	}

	public async Task<Player?> FindByIdempotencyKeyAsync(string idempotencyKey, CancellationToken cancellationToken = default)
	{
		var query = new QueryDefinition("SELECT * FROM c WHERE c.idempotencyKey = @key")
			.WithParameter("@key", idempotencyKey);

		var iterator = Container.GetItemQueryIterator<Player>(query);

		while (iterator.HasMoreResults)
		{
			var page = await iterator.ReadNextAsync(cancellationToken);
			var existing = page.FirstOrDefault();
			if (existing is not null)
			{
				return existing;
			}
		}

		return null;
	}
}
