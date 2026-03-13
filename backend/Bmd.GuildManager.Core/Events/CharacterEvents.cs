using System.Text.Json.Serialization;
using Bmd.GuildManager.Core.Models;

namespace Bmd.GuildManager.Core.Events;

public record CharacterCreated(
    [property: JsonPropertyName("playerId")] Guid PlayerId,
    [property: JsonPropertyName("characterId")] Guid CharacterId,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("level")] int Level,
    [property: JsonPropertyName("strength")] int Strength,
    [property: JsonPropertyName("luck")] int Luck,
    [property: JsonPropertyName("endurance")] int Endurance);

public record CharacterDied(
    [property: JsonPropertyName("characterId")]   Guid CharacterId,
    [property: JsonPropertyName("playerId")]      Guid PlayerId,
    [property: JsonPropertyName("questId")]       Guid QuestId,
    [property: JsonPropertyName("characterTier")] DifficultyTier CharacterTier);
