using System.Text.Json;

namespace Bmd.GuildManager.Functions.Serialization;

internal static class FunctionJsonOptions
{
    internal static readonly JsonSerializerOptions Default = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
}
