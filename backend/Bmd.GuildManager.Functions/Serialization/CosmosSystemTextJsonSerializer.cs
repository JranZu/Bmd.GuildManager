using Azure.Core.Serialization;
using Microsoft.Azure.Cosmos;
using System.Text.Json;

namespace Bmd.GuildManager.Functions.Serialization;

public class CosmosSystemTextJsonSerializer(JsonSerializerOptions options)
	: CosmosSerializer
{
	private readonly JsonObjectSerializer _serializer = new(options);

	public override T FromStream<T>(Stream stream)
	{
		using (stream)
		{
			if (stream.Length == 0) return default!;
			return (T)_serializer.Deserialize(stream, typeof(T), default)!;
		}
	}

	public override Stream ToStream<T>(T input)
	{
		var stream = new MemoryStream();
		_serializer.Serialize(stream, input, typeof(T), default);
		stream.Position = 0;
		return stream;
	}
}