using Bmd.GuildManager.Core.Models;

namespace Bmd.GuildManager.Core.Services;

internal static class QuestNameBuilder
{
    private static readonly Random Random = new();

    internal static string BuildName(DifficultyTier tier, string riskLevel, string questType)
    {
        var location = Pick(QuestWordPools.LocationsForTier(tier));
        var adjective = Pick(QuestWordPools.AdjectivesForRisk(riskLevel));
        var creature = Pick(QuestWordPools.CreaturesForTier(tier));
        var material = Pick(QuestWordPools.MaterialsForTier(tier));

        return questType switch
        {
            "Kill" => Pick<Func<string>>(
            [
                () => $"Hunt of the {Capitalize(adjective)} {Capitalize(creature)}",
                () => $"Slay the {Capitalize(creature)} of {Capitalize(location)}",
                () => $"The {Capitalize(adjective)} Bounty"
            ])(),

            "Gather" => Pick<Func<string>>(
            [
                () => $"Harvest of {Capitalize(location)}",
                () => $"The {Capitalize(adjective)} Collection",
                () => $"Gather {Capitalize(material)} from {Capitalize(location)}"
            ])(),

            "Rescue" => Pick<Func<string>>(
            [
                () => $"The {Capitalize(adjective)} Extraction",
                () => $"Rescue at {Capitalize(location)}",
                () => $"The {Capitalize(adjective)} Retrieval"
            ])(),

            "Delivery" => Pick<Func<string>>(
            [
                () => $"Delivery to {Capitalize(location)}",
                () => $"The {Capitalize(adjective)} Courier",
                () => $"Safe Passage to {Capitalize(location)}"
            ])(),

            "Escort" => Pick<Func<string>>(
            [
                () => $"Escort through {Capitalize(location)}",
                () => $"The {Capitalize(adjective)} Guard",
                () => $"Safe Passage through {Capitalize(location)}"
            ])(),

            _ => $"A {Capitalize(tier.ToString())} Contract"
        };
    }

    internal static string BuildDescription(DifficultyTier tier, string riskLevel, string questType)
    {
        var location = Pick(QuestWordPools.LocationsForTier(tier));
        var adjective = Pick(QuestWordPools.AdjectivesForRisk(riskLevel));
        var creature = Pick(QuestWordPools.CreaturesForTier(tier));
        var material = Pick(QuestWordPools.MaterialsForTier(tier));

        return questType switch
        {
            "Kill" => Pick<Func<string>>(
            [
                () => $"A {adjective} {creature} has been spotted near {location}. Bring them down.",
                () => $"The guild has a contract on {creature} operating out of {location}. No survivors.",
                () => $"Reports of {creature} near {location} have reached the guild. Handle it."
            ])(),

            "Gather" => Pick<Func<string>>(
            [
                () => $"Valuable {material} has been located at {location}. Extract what you can before others get there.",
                () => $"The guild needs {material} from {location}. Retrieve it quickly.",
                () => $"A {adjective} supply of {material} was spotted near {location}. Don't leave empty handed."
            ])(),

            "Rescue" => Pick<Func<string>>(
            [
                () => $"Someone needs to be pulled out of {location} before it's too late.",
                () => $"A {adjective} extraction job near {location}. Get in, get them out.",
                () => $"Guild contacts in {location} need immediate assistance. Time is short."
            ])(),

            "Delivery" => Pick<Func<string>>(
            [
                () => $"Transport a {adjective} package to {location}. Do not ask what's inside.",
                () => $"A delivery must reach {location} by any means necessary.",
                () => $"The cargo is {adjective}. The route through {location} is worse. Good luck."
            ])(),

            "Escort" => Pick<Func<string>>(
            [
                () => $"A valuable escort job through {location}. Keep them alive.",
                () => $"The roads through {location} are {adjective}. Someone needs safe passage.",
                () => $"Get them through {location} in one piece. Payment on arrival."
            ])(),

            _ => "A contract has been posted. Details available at the guild hall."
        };
    }

    private static T Pick<T>(T[] options) =>
        options[Random.Next(options.Length)];

    private static string Pick(string[] options) =>
        options[Random.Next(options.Length)];

    private static string Capitalize(string input) =>
        string.IsNullOrEmpty(input)
            ? input
            : char.ToUpper(input[0]) + input[1..];
}
