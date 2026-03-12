namespace Bmd.GuildManager.Core.Models;

/// <summary>
/// Pairs a Cosmos DB document with its ETag, enabling optimistic concurrency
/// on subsequent writes via IfMatchEtag.
/// </summary>
public record CosmosDocument<T>(T Document, string ETag);
