namespace Bmd.GuildManager.Core.Abstractions;

public interface IQuestGeneratorService
{
    Task EnsureMinimumQuestsAsync(int minimumPerTier, CancellationToken cancellationToken = default);
}
