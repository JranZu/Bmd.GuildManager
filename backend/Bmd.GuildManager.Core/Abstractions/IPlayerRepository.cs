using Bmd.GuildManager.Core.Models;

namespace Bmd.GuildManager.Core.Abstractions;

public interface IPlayerRepository
{
    Task<Player?> FindByIdempotencyKeyAsync(string idempotencyKey);

    Task CreateAsync(Player player);
}
