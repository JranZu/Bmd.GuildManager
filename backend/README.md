# Backend

This folder contains the .NET / C# **Azure Functions** that serve as the serverless compute units and API endpoints for the Guild Manager game.

## Architecture

The backend is built on a **stateless, serverless** model using Azure Functions (Isolated Worker). Each function handles a discrete game domain (guilds, quests, characters, market, etc.) and communicates via well-defined event contracts.

## C# Project Settings

All backend projects target **.NET 10** and use the following compiler settings:

- **Nullable reference types enabled** (`<Nullable>enable</Nullable>`) — all reference types are non-nullable by default; nullability must be declared explicitly.
- **Warnings treated as errors** (`<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`) — the build fails on any compiler warning to enforce code quality.

## Getting Started

> Project scaffold to be added in a subsequent commit.

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Azure Functions Core Tools v4](https://learn.microsoft.com/azure/azure-functions/functions-run-local)
- An Azure subscription (for cloud deployment)

## Structure (Planned)

```
/backend
├── Bmd.GuildManager.Functions/   # Azure Functions project
├── Bmd.GuildManager.Core/        # Domain models and business logic
├── Bmd.GuildManager.Tests/       # Unit and integration tests
└── README.md
```

## Deployment

Backend functions are deployed automatically via the CI/CD pipeline defined in `/.github/workflows` on every push to the `main` branch.
