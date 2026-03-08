# Bmd.GuildManager

Guild Manager is a web-based management game where the player runs an adventurer guild. Characters are recruited, equipped, and sent on quests that take time to complete. Successful quests produce loot that can be equipped or sold into a simulated market influenced by the needs of a living NPC population.

---

## Tech Stack

| Layer    | Technology                                    |
|----------|-----------------------------------------------|
| Frontend | Blazor WebAssembly — Azure Static Web Apps    |
| Backend  | .NET 10 Azure Functions (Isolated Worker)     |
| Database | Azure Cosmos DB (NoSQL)                       |
| Messaging| Azure Service Bus                             |
| CI/CD    | GitHub Actions                                |

### C# Compiler Settings

All C# projects in this repository use:

- **Target framework:** `net10.0`
- **Nullable reference types:** enabled
- **Treat warnings as errors:** enabled

---

## Repository Structure

```
Bmd.GuildManager/
├── .github/
│   └── workflows/
│       └── ci-cd.yml          # CI/CD pipeline (GitHub Actions)
├── backend/                   # Azure Functions (.NET 10)
│   └── README.md
├── docs/                      # Architecture & design documentation
│   ├── game-design-document.md
│   ├── event-contract-specification.md
│   └── system-architecture.md
├── frontend/                  # Blazor WebAssembly client
│   └── README.md
├── .gitignore
└── README.md
```

---

## Documentation

| Document | Description |
|----------|-------------|
| [Game Design Document](docs/game-design-document.md) | Core game loop, progression, characters, quests, and economy |
| [Event Contract Specification](docs/event-contract-specification.md) | JSON event schemas for all backend events |
| [System Architecture](docs/system-architecture.md) | High-level architecture diagrams and component descriptions |

---

## Getting Started

> Full project scaffolding will be added in subsequent commits. See the `/frontend` and `/backend` READMEs for setup instructions once the project code is in place.

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Azure Functions Core Tools v4](https://learn.microsoft.com/azure/azure-functions/functions-run-local)
- [Node.js 20+](https://nodejs.org/) (for any frontend tooling)
- An Azure subscription (for cloud deployment)

---

## CI/CD

The pipeline defined in `.github/workflows/ci-cd.yml` automatically:

1. Builds and tests the backend on every push / PR targeting `main`.
2. Builds the frontend on every push / PR targeting `main`.
3. Deploys both to Azure on merge to `main` (once deployment secrets are configured).

See the workflow file for instructions on configuring the required GitHub repository secrets.
