using System.Text.Json;

namespace Bmd.GuildManager.Functions.Serialization;

internal static class FunctionJsonOptions
{
    internal static readonly JsonSerializerOptions Default = CreateDefault();

    private static JsonSerializerOptions CreateDefault()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        options.MakeReadOnly(populateMissingResolver: true);
        return options;
    }
}
