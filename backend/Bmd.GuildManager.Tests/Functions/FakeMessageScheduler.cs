using Bmd.GuildManager.Core.Abstractions;

namespace Bmd.GuildManager.Tests.Functions;

public class FakeMessageScheduler : IMessageScheduler
{
    public record ScheduledMessage(
        string QueueOrTopicName,
        string MessageBody,
        DateTimeOffset ScheduledEnqueueTime);

    public List<ScheduledMessage> Scheduled { get; } = [];

    public Task ScheduleMessageAsync(
        string queueOrTopicName,
        string messageBody,
        DateTimeOffset scheduledEnqueueTime)
    {
        Scheduled.Add(new ScheduledMessage(
            queueOrTopicName,
            messageBody,
            scheduledEnqueueTime));
        return Task.CompletedTask;
    }
}
