using Azure;
using Azure.Core;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace Bmd.GuildManager.Tests.Functions;

/// <summary>
/// Minimal in-memory stubs for BlobServiceClient/BlobContainerClient/BlobClient
/// to support unit-testing functions that archive quests to blob storage.
/// The Azure SDK clients expose protected parameterless ctors and virtual methods
/// specifically to enable this pattern without a mocking library.
/// </summary>
internal class FakeBlobServiceClient : BlobServiceClient
{
	public override BlobContainerClient GetBlobContainerClient(string blobContainerName)
		=> new FakeBlobContainerClient();
}

internal class FakeBlobContainerClient : BlobContainerClient
{
	public override Task<Response<BlobContainerInfo>> CreateIfNotExistsAsync(
		PublicAccessType publicAccessType = default,
		IDictionary<string, string>? metadata = null,
		BlobContainerEncryptionScopeOptions? encryptionScopeOptions = null,
		CancellationToken cancellationToken = default)
		=> Task.FromResult(
			Response.FromValue(
				BlobsModelFactory.BlobContainerInfo(ETag.All, DateTimeOffset.UtcNow),
				new FakeResponse()));

	public override BlobClient GetBlobClient(string blobName) => new FakeBlobClient();
}

internal class FakeBlobClient : BlobClient
{
	public override Task<Response<BlobContentInfo>> UploadAsync(
		BinaryData content,
		bool overwrite = false,
		CancellationToken cancellationToken = default)
		=> Task.FromResult(
			Response.FromValue(
				BlobsModelFactory.BlobContentInfo(
					ETag.All, DateTimeOffset.UtcNow, Array.Empty<byte>(), string.Empty, 0),
				new FakeResponse()));
}

/// <summary>
/// Minimal concrete Response for the Azure SDK factory methods.
/// </summary>
internal class FakeResponse : Response
{
	public override int Status => 200;
	public override string ReasonPhrase => "OK";
	public override Stream? ContentStream { get; set; }
	public override string ClientRequestId { get; set; } = string.Empty;

	public override void Dispose() { GC.SuppressFinalize(this); }

	protected override bool TryGetHeader(string name, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out string? value)
	{
		value = null;
		return false;
	}

	protected override bool TryGetHeaderValues(string name, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out IEnumerable<string>? values)
	{
		values = null;
		return false;
	}

	protected override bool ContainsHeader(string name) => false;

	protected override IEnumerable<HttpHeader> EnumerateHeaders() => [];
}
