using Bmd.GuildManager.Core.Abstractions;
using Bmd.GuildManager.Core.Models;
using Microsoft.Azure.Cosmos;

namespace Bmd.GuildManager.Functions.Repositories;

public class CosmosCharacterRepository(CosmosClient cosmosClient) : ICharacterRepository
{
	private const string DatabaseName = "guildmanager";
	private const string ContainerName = "Characters";

	private Container Container =>
		cosmosClient.GetContainer(DatabaseName, ContainerName);

	public async Task CreateAsync(Character character)
	{
		try
		{
			await Container.CreateItemAsync(
				character,
				new PartitionKey(character.PlayerId.ToString()));
		}
		catch (CosmosException ex)
			when (ex.StatusCode == System.Net.HttpStatusCode.Conflict)
		{
			// Document already exists — idempotent, safe to ignore
		}
	}

	public async Task<Character?> FindByCharacterIdAsync(Guid characterId, Guid playerId)
	{
		try
		{
			var response = await Container.ReadItemAsync<Character>(
				characterId.ToString(),
				new PartitionKey(playerId.ToString()));
			return response.Resource;
		}
		catch (CosmosException ex)
			when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
		{
			return null;
		}
	}

	public async Task<IReadOnlyList<Character>> GetByPlayerIdAsync(Guid playerId)
	{
		var query = new QueryDefinition(
			"SELECT * FROM c WHERE c.playerId = @playerId")
			.WithParameter("@playerId", playerId.ToString());

		var iterator = Container.GetItemQueryIterator<Character>(query);
		var results = new List<Character>();

		while (iterator.HasMoreResults)
		{
			var page = await iterator.ReadNextAsync();
			results.AddRange(page);
		}

		return results.AsReadOnly();
	}

	public async Task UpdateAsync(Character character, string etag)
	{
		var options = new ItemRequestOptions
		{
			IfMatchEtag = etag
		};

		await Container.ReplaceItemAsync(
			character,
			character.Id,
			new PartitionKey(character.PlayerId.ToString()),
			options);
	}
}
