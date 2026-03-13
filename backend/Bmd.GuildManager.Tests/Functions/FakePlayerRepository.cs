using Bmd.GuildManager.Core.Abstractions;
using Bmd.GuildManager.Core.Models;

namespace Bmd.GuildManager.Tests.Functions;

public class FakePlayerRepository : IPlayerRepository
{
	public List<Player> Players { get; } = [];

	public Task<Player?> FindByIdempotencyKeyAsync(string idempotencyKey)
	{
		var match = Players.FirstOrDefault(p => p.IdempotencyKey == idempotencyKey);
		return Task.FromResult(match);
	}

	public Task<CosmosDocument<Player>?> FindByPlayerIdAsync(Guid playerId)
	{
		var match = Players.FirstOrDefault(p => p.PlayerId == playerId);
		if (match is null)
			return Task.FromResult<CosmosDocument<Player>?>(null);
		return Task.FromResult<CosmosDocument<Player>?>(
			new CosmosDocument<Player>(match, "fake-etag"));
	}

	public Task CreateAsync(Player player)
	{
		Players.Add(player);
		return Task.CompletedTask;
	}

	public Task UpdateAsync(Player player, string etag)
	{
		var index = Players.FindIndex(p => p.PlayerId == player.PlayerId);
		if (index >= 0)
			Players[index] = player;
		return Task.CompletedTask;
	}
}