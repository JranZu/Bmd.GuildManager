using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Bmd.GuildManager.Core.Abstractions;
using Bmd.GuildManager.Functions.Publishers;
using Bmd.GuildManager.Functions.Repositories;
using Bmd.GuildManager.Functions.Serialization;
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

builder.Services.AddSingleton<IEventPublisher>(sp =>
    new ServiceBusEventPublisher(
        sp.GetRequiredService<ServiceBusClient>(),
        "player-events"));

builder.Build().Run();
