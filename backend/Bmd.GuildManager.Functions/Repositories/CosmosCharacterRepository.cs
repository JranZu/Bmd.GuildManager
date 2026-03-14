using Bmd.GuildManager.Core.Abstractions;
using Bmd.GuildManager.Core.Models;
using Bmd.GuildManager.Functions.Serialization;
using Microsoft.Azure.Cosmos;

namespace Bmd.GuildManager.Functions.Repositories;

public class CosmosCharacterRepository(CosmosClient cosmosClient) : ICharacterRepository
{
	private const string ContainerName = "Characters";

	private Container Container =>
		cosmosClient.GetContainer(CosmosConstants.DatabaseName, ContainerName);

	public async Task CreateAsync(Character character, CancellationToken cancellationToken = default)
	{
		try
		{
			await Container.CreateItemAsync(
				character,
				new PartitionKey(character.PlayerId.ToString()),
				cancellationToken: cancellationToken);
		}
		catch (CosmosException ex)
			when (ex.StatusCode == System.Net.HttpStatusCode.Conflict)
		{
			// Document already exists — idempotent, safe to ignore
		}
	}

	public async Task<CosmosDocument<Character>?> FindByCharacterIdAsync(Guid characterId, Guid playerId, CancellationToken cancellationToken = default)
	{
		try
		{
			var response = await Container.ReadItemAsync<Character>(
				characterId.ToString(),
				new PartitionKey(playerId.ToString()),
				cancellationToken: cancellationToken);
			return new CosmosDocument<Character>(response.Resource, response.ETag);
		}
		catch (CosmosException ex)
			when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
		{
			return null;
		}
	}

	public async Task<IReadOnlyList<Character>> GetByPlayerIdAsync(Guid playerId, CancellationToken cancellationToken = default)
	{
		var query = new QueryDefinition(
			"SELECT * FROM c WHERE c.playerId = @playerId")
			.WithParameter("@playerId", playerId.ToString());

		var iterator = Container.GetItemQueryIterator<Character>(query);
		var results = new List<Character>();

		while (iterator.HasMoreResults)
		{
			var page = await iterator.ReadNextAsync(cancellationToken);
			results.AddRange(page);
		}

		return results.AsReadOnly();
	}

	public async Task UpdateAsync(Character character, string etag, CancellationToken cancellationToken = default)
	{
		var options = new ItemRequestOptions
		{
			IfMatchEtag = etag
		};

		await Container.ReplaceItemAsync(
			character,
			character.Id,
			new PartitionKey(character.PlayerId.ToString()),
			options,
			cancellationToken);
	}
}
