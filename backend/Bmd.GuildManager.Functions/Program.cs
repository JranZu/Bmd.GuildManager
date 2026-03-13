using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using Bmd.GuildManager.Core.Abstractions;
using Bmd.GuildManager.Core.Services;
using Bmd.GuildManager.Functions.Publishers;
using Bmd.GuildManager.Functions.Repositories;
using Bmd.GuildManager.Functions.Serialization;
using Bmd.GuildManager.Functions.Services;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

builder.Services.AddSingleton(_ =>
{
	var connectionString = builder.Configuration["CosmosDbConnectionString"]
		?? throw new InvalidOperationException("CosmosDbConnectionString is not configured.");

	var jsonOptions = new JsonSerializerOptions
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase
	};

	return new CosmosClient(connectionString, new CosmosClientOptions
	{
		Serializer = new CosmosSystemTextJsonSerializer(jsonOptions)
	});
});

builder.Services.AddSingleton(_ =>
{
    var connectionString = builder.Configuration["ServiceBusConnectionString"]
        ?? throw new InvalidOperationException("ServiceBusConnectionString is not configured.");
    return new ServiceBusClient(connectionString);
});

builder.Services.AddSingleton<IPlayerRepository, CosmosPlayerRepository>();
builder.Services.AddSingleton<ICharacterRepository, CosmosCharacterRepository>();
builder.Services.AddSingleton<IQuestRepository, CosmosQuestRepository>();
builder.Services.AddSingleton<IQuestGeneratorService, QuestGeneratorService>();
builder.Services.AddSingleton<IMessageScheduler, ServiceBusMessageScheduler>();
builder.Services.AddSingleton<IRandomProvider, DefaultRandomProvider>();
builder.Services.AddSingleton<QuestResolutionService>();

builder.Services.AddKeyedSingleton<IEventPublisher>("player-events", (sp, _) =>
	new ServiceBusEventPublisher(
		sp.GetRequiredService<ServiceBusClient>(),
		"player-events"));

builder.Services.AddKeyedSingleton<IEventPublisher>("quest-events", (sp, _) =>
	new ServiceBusEventPublisher(
		sp.GetRequiredService<ServiceBusClient>(),
		"quest-events"));

builder.Services.AddSingleton(_ =>
{
	var connectionString = builder.Configuration["BlobStorageConnectionString"]
		?? throw new InvalidOperationException("BlobStorageConnectionString is not configured.");
	return new BlobServiceClient(connectionString);
});

builder.Build().Run();
