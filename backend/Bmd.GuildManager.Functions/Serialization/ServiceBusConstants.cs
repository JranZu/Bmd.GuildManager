namespace Bmd.GuildManager.Functions.Serialization;

internal static class ServiceBusConstants
{
    // Topics
    internal const string PlayerEventsTopic = "player-events";
    internal const string QuestEventsTopic = "quest-events";

    // Queues
    internal const string QuestCompletedQueue = "quest-completed";

    // Subscriptions
    internal const string OnboardingSubscription = "onboarding-sub";
    internal const string StarterCharactersSubscription = "starter-characters-sub";
    internal const string CharacterCreatedSubscription = "character-created-sub";
    internal const string CharacterQuestResolvedSubscription = "character-quest-resolved-sub";
}
