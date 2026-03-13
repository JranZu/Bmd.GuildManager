using Bmd.GuildManager.Core.Models;

namespace Bmd.GuildManager.Core.Services;

internal static class QuestWordPools
{
    // --- Locations by tier ---

    internal static readonly string[] LocationsNovice =
    [
        "the old mill", "the crossroads", "Millhaven village",
        "the riverside camp", "the farmer's fields", "Ashbury town",
        "the forest trail", "the broken bridge", "the grain stores",
        "the village outskirts"
    ];

    internal static readonly string[] LocationsApprentice =
    [
        "the Ashwood forest", "the mountain pass", "the old watchtower",
        "the flooded mines", "Crestfall outpost", "the goblin warrens",
        "the haunted manor", "the bandit stockade", "the crumbling aqueduct",
        "the fog-covered hills"
    ];

    internal static readonly string[] LocationsVeteran =
    [
        "the Sunken Fortress", "the Witchfen Marshes", "the Ironveil Mines",
        "the Ashen Plains", "the Warlord's Keep", "the Cursed Cathedral",
        "the Bleakstone Pass", "the Plagued Harbor", "the Thornwall Ruins",
        "the Sorcerer's Tower"
    ];

    internal static readonly string[] LocationsElite =
    [
        "the Obsidian Spire", "the Shattered Keep", "the Void Reaches",
        "the Necropolis", "the Titan's Graveyard", "the Wraithwood",
        "the Forsaken Citadel", "the Sea of Ash", "the Hollow Peaks",
        "the Demon's Causeway"
    ];

    internal static readonly string[] LocationsLegendary =
    [
        "the Void Gate", "the Throne of Ash", "the World's End",
        "the Realm Between", "the Shattered Firmament", "the God's Graveyard",
        "the Eternal Abyss", "the Last Bastion", "the Edge of the Known World",
        "the Chamber of Unmaking"
    ];

    // --- Adjectives by risk level ---

    internal static readonly string[] AdjectivesLow =
    [
        "quiet", "routine", "minor", "simple", "modest",
        "unremarkable", "straightforward", "common", "small", "brief"
    ];

    internal static readonly string[] AdjectivesMedium =
    [
        "dangerous", "treacherous", "grim", "cursed", "troubled",
        "volatile", "desperate", "bitter", "dark", "grim"
    ];

    internal static readonly string[] AdjectivesHigh =
    [
        "catastrophic", "doomed", "forsaken", "infernal", "wretched",
        "accursed", "nightmarish", "ruinous", "blighted", "dire"
    ];

    // --- Creatures/Targets by tier ---

    internal static readonly string[] CreaturesNovice =
    [
        "goblin", "bandit", "wolf pack", "poacher", "river troll",
        "petty thief", "rabid hound", "cave spider", "Grik the Coward",
        "a deserter captain"
    ];

    internal static readonly string[] CreaturesApprentice =
    [
        "orc warband", "witch", "undead knight", "wyvern", "mercenary captain",
        "plague cultist", "river serpent", "Mordak the Hungry",
        "a disgraced warlord", "bone golem"
    ];

    internal static readonly string[] CreaturesVeteran =
    [
        "stone giant", "necromancer", "vampire lord", "Serath the Pale",
        "iron golem", "war hydra", "the Ashen Witch", "demon scout",
        "a corrupted paladin", "plague bearer"
    ];

    internal static readonly string[] CreaturesElite =
    [
        "elder dragon", "lich", "Vareth the Undying", "void stalker",
        "titan construct", "the Hollow Knight", "demon warlord",
        "Kezara of the Deep", "an ancient revenant", "the Pale Sovereign"
    ];

    internal static readonly string[] CreaturesLegendary =
    [
        "the Hollow King", "an ancient dragon", "a god's remnant",
        "Aethon the World-Breaker", "the Unnamed One", "a fallen seraph",
        "Vorzeth the Undying Flame", "the Last Titan", "the Dreaming Horror",
        "a primordial revenant"
    ];

    // --- Materials by tier (Gather quests) ---

    internal static readonly string[] MaterialsNovice =
    [
        "herbs", "salvage", "common ore", "river clay", "wild grain",
        "pine resin", "dried mushrooms", "animal pelts", "flint shards",
        "marsh reeds"
    ];

    internal static readonly string[] MaterialsApprentice =
    [
        "silver ore", "enchanted bark", "alchemical roots", "ghost moss",
        "iron shavings", "cave crystals", "runic fragments", "troll bile",
        "cold iron dust", "witchwood splinters"
    ];

    internal static readonly string[] MaterialsVeteran =
    [
        "bloodstone", "wraithwood", "cursed relics", "demon ichor",
        "mithril ore", "soul amber", "plague samples", "void shards",
        "enchanted bones", "ancient seals"
    ];

    internal static readonly string[] MaterialsElite =
    [
        "titan ore", "abyssal crystal", "divine fragments", "elder runes",
        "void essence", "shattered godstone", "corrupted relics",
        "spectral iron", "lich dust", "world-thread"
    ];

    internal static readonly string[] MaterialsLegendary =
    [
        "void crystals", "dragon scales", "godstone fragments",
        "primordial essence", "shards of the First Flame",
        "remnants of a dead god", "the Undying Ember",
        "crystallized time", "the World's Blood", "unmaking dust"
    ];

    // --- Lookup helpers ---

    internal static string[] LocationsForTier(DifficultyTier tier) => tier switch
    {
        DifficultyTier.Novice      => LocationsNovice,
        DifficultyTier.Apprentice  => LocationsApprentice,
        DifficultyTier.Veteran     => LocationsVeteran,
        DifficultyTier.Elite       => LocationsElite,
        DifficultyTier.Legendary   => LocationsLegendary,
        _ => throw new ArgumentOutOfRangeException(nameof(tier), tier, null)
    };

    internal static string[] AdjectivesForRisk(string riskLevel) => riskLevel switch
    {
        "Low" => AdjectivesLow,
        "Medium" => AdjectivesMedium,
        "High" => AdjectivesHigh,
        _ => AdjectivesLow
    };

    internal static string[] CreaturesForTier(DifficultyTier tier) => tier switch
    {
        DifficultyTier.Novice      => CreaturesNovice,
        DifficultyTier.Apprentice  => CreaturesApprentice,
        DifficultyTier.Veteran     => CreaturesVeteran,
        DifficultyTier.Elite       => CreaturesElite,
        DifficultyTier.Legendary   => CreaturesLegendary,
        _ => throw new ArgumentOutOfRangeException(nameof(tier), tier, null)
    };

    internal static string[] MaterialsForTier(DifficultyTier tier) => tier switch
    {
        DifficultyTier.Novice      => MaterialsNovice,
        DifficultyTier.Apprentice  => MaterialsApprentice,
        DifficultyTier.Veteran     => MaterialsVeteran,
        DifficultyTier.Elite       => MaterialsElite,
        DifficultyTier.Legendary   => MaterialsLegendary,
        _ => throw new ArgumentOutOfRangeException(nameof(tier), tier, null)
    };
}
