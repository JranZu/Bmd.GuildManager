using Bmd.GuildManager.Core.Abstractions;
using Bmd.GuildManager.Core.Models;

namespace Bmd.GuildManager.Core.Services;

/// <summary>
/// Pre-selected word choices shared between name and description generation
/// to ensure internal consistency within a single quest.
/// </summary>
public record QuestWordSelection(
    string Location,
    string Adjective,
    string Creature,
    string Material);

public class QuestNameBuilder(IRandomProvider random)
{
    private readonly IRandomProvider _random = random;

    /// <summary>
    /// Picks one word from each pool for the given tier/risk so that
    /// <see cref="BuildName"/> and <see cref="BuildDescription"/> reference
    /// the same location, adjective, creature, and material.
    /// </summary>
    internal QuestWordSelection SelectWords(DifficultyTier tier, RiskLevel riskLevel) =>
        new(
            Location:  Pick(QuestWordPools.LocationsForTier(tier)),
            Adjective: Pick(QuestWordPools.AdjectivesForRisk(riskLevel)),
            Creature:  Pick(QuestWordPools.CreaturesForTier(tier)),
            Material:  Pick(QuestWordPools.MaterialsForTier(tier)));

    internal string BuildName(QuestWordSelection words, QuestType questType)
    {
        var (location, adjective, creature, material) = words;

        return questType switch
        {
            QuestType.Kill => Pick<Func<string>>(
            [
                () => $"Hunt of the {Capitalize(adjective)} {Capitalize(creature)}",
                () => $"Slay the {Capitalize(creature)} of {Capitalize(location)}",
                () => $"The {Capitalize(adjective)} Bounty"
            ])(),

            QuestType.Gather => Pick<Func<string>>(
            [
                () => $"Harvest of {Capitalize(location)}",
                () => $"The {Capitalize(adjective)} Collection",
                () => $"Gather {Capitalize(material)} from {Capitalize(location)}"
            ])(),

            QuestType.Rescue => Pick<Func<string>>(
            [
                () => $"The {Capitalize(adjective)} Extraction",
                () => $"Rescue at {Capitalize(location)}",
                () => $"The {Capitalize(adjective)} Retrieval"
            ])(),

            QuestType.Delivery => Pick<Func<string>>(
            [
                () => $"Delivery to {Capitalize(location)}",
                () => $"The {Capitalize(adjective)} Courier",
                () => $"Safe Passage to {Capitalize(location)}"
            ])(),

            QuestType.Escort => Pick<Func<string>>(
            [
                () => $"Escort through {Capitalize(location)}",
                () => $"The {Capitalize(adjective)} Guard",
                () => $"Safe Passage through {Capitalize(location)}"
            ])(),

            _ => $"A {Capitalize(questType.ToString())} Contract"
        };
    }

    internal string BuildDescription(QuestWordSelection words, QuestType questType)
    {
        var (location, adjective, creature, material) = words;

        return questType switch
        {
            QuestType.Kill => Pick<Func<string>>(
            [
                () => $"A {adjective} {creature} has been spotted near {location}. Bring them down.",
                () => $"The guild has a contract on {creature} operating out of {location}. No survivors.",
                () => $"Reports of {creature} near {location} have reached the guild. Handle it."
            ])(),

            QuestType.Gather => Pick<Func<string>>(
            [
                () => $"Valuable {material} has been located at {location}. Extract what you can before others get there.",
                () => $"The guild needs {material} from {location}. Retrieve it quickly.",
                () => $"A {adjective} supply of {material} was spotted near {location}. Don't leave empty handed."
            ])(),

            QuestType.Rescue => Pick<Func<string>>(
            [
                () => $"Someone needs to be pulled out of {location} before it's too late.",
                () => $"A {adjective} extraction job near {location}. Get in, get them out.",
                () => $"Guild contacts in {location} need immediate assistance. Time is short."
            ])(),

            QuestType.Delivery => Pick<Func<string>>(
            [
                () => $"Transport a {adjective} package to {location}. Do not ask what's inside.",
                () => $"A delivery must reach {location} by any means necessary.",
                () => $"The cargo is {adjective}. The route through {location} is worse. Good luck."
            ])(),

            QuestType.Escort => Pick<Func<string>>(
            [
                () => $"A valuable escort job through {location}. Keep them alive.",
                () => $"The roads through {location} are {adjective}. Someone needs safe passage.",
                () => $"Get them through {location} in one piece. Payment on arrival."
            ])(),

            _ => "A contract has been posted. Details available at the guild hall."
        };
    }

    private T Pick<T>(T[] options) =>
        options[_random.NextInt(0, options.Length)];

    private string Pick(string[] options) =>
        options[_random.NextInt(0, options.Length)];

    private static string Capitalize(string input) =>
        string.IsNullOrEmpty(input)
            ? input
            : char.ToUpper(input[0]) + input[1..];
}
