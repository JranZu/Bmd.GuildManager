namespace Bmd.GuildManager.Core.Abstractions;

public interface IMessageScheduler
{
    Task ScheduleMessageAsync(string queueOrTopicName, string messageBody, DateTimeOffset scheduledEnqueueTime, CancellationToken cancellationToken = default);
}
