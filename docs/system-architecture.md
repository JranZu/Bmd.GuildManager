# System Architecture

**Project:** Bmd.GuildManager  
**Version:** 0.1.0 — Initial Draft  
**Status:** In Progress

---

## 1. High-Level Overview

Bmd.GuildManager is built on a **cloud-native, serverless architecture** hosted entirely on Microsoft Azure. The system separates concerns across three primary layers:

```
┌─────────────────────────────────────────────────────────┐
│                      Client (Browser)                   │
│          Blazor WebAssembly — Azure Static Web Apps     │
└─────────────────────┬───────────────────────────────────┘
                      │  HTTPS / REST
┌─────────────────────▼───────────────────────────────────┐
│                 API Layer                               │
│        Azure Functions (Isolated Worker, .NET 8)        │
│  HTTP Triggers · Timer Triggers · Service Bus Triggers  │
└──────┬──────────────┬────────────────┬──────────────────┘
       │              │                │
┌──────▼──────┐ ┌─────▼──────┐ ┌──────▼──────────────────┐
│  Azure      │ │  Azure     │ │  Azure Service Bus       │
│  Cosmos DB  │ │  Blob      │ │  (Async Event Messaging) │
│  (NoSQL)    │ │  Storage   │ │                          │
└─────────────┘ └────────────┘ └──────────────────────────┘
```

---

## 2. Components

### 2.1 Frontend — Azure Static Web Apps

| Property     | Value                              |
|--------------|------------------------------------|
| Framework    | Blazor WebAssembly (.NET 8)        |
| Hosting      | Azure Static Web Apps              |
| Auth         | Azure Static Web Apps built-in auth (AAD / GitHub) |
| CDN          | Integrated Azure CDN               |

The frontend is a single-page application (SPA) that communicates exclusively through the backend API layer. No direct database access is performed from the client.

### 2.2 Backend — Azure Functions

| Property     | Value                                     |
|--------------|-------------------------------------------|
| Runtime      | .NET 8 Isolated Worker                    |
| Trigger Types| HTTP, Timer, Service Bus                  |
| Auth         | Function-level API keys / AAD tokens      |
| Deployment   | Azure Functions Consumption Plan          |

**Function Groups:**

| Function Group   | Trigger | Responsibility                              |
|------------------|---------|---------------------------------------------|
| `GuildFunctions` | HTTP    | CRUD operations for guilds                  |
| `CharacterFunctions` | HTTP | Recruit, equip, and manage characters     |
| `QuestFunctions` | HTTP    | Start quests, poll status                   |
| `QuestResolver`  | Timer   | Resolves completed quest timers             |
| `MarketFunctions`| HTTP    | List items, query prices                    |
| `MarketSimulator`| Timer   | Runs NPC demand simulation on a schedule    |

### 2.3 Data — Azure Cosmos DB

| Property         | Value                        |
|------------------|------------------------------|
| API              | NoSQL (Core API)             |
| Consistency      | Session                      |
| Partition Key    | `/guildId`                   |

**Containers:**

| Container    | Description                          |
|--------------|--------------------------------------|
| `guilds`     | Guild profiles and metadata          |
| `characters` | Character stats, equipment, status   |
| `quests`     | Quest templates and active instances |
| `inventory`  | Item definitions and guild inventories |
| `market`     | Market listings and price history    |

### 2.4 Messaging — Azure Service Bus

Asynchronous events (e.g. `quest.completed`, `market.priceTick`) are published to a **Service Bus topic** and consumed by the relevant function triggers. This decouples time-sensitive operations from synchronous HTTP request cycles.

### 2.5 Storage — Azure Blob Storage

Static assets (character portraits, item icons) are stored in Azure Blob Storage with public CDN access.

---

## 3. CI/CD Pipeline

```
Developer Push (main)
        │
        ▼
GitHub Actions Workflow
   ├── Build & Test (dotnet build + dotnet test)
   ├── Lint & Static Analysis
   ├── Deploy Backend → Azure Functions
   └── Deploy Frontend → Azure Static Web Apps
```

Pipeline definitions live in `/.github/workflows/`.

---

## 4. Security Considerations

- All HTTP endpoints require authentication via Azure AD tokens or function API keys.
- Cosmos DB and Service Bus connection strings are stored in **Azure Key Vault** and injected as Application Settings at deploy time — never committed to source control.
- The Static Web App restricts unauthenticated access to protected routes via `staticwebapp.config.json`.

---

## 5. Scalability

The Consumption Plan for Azure Functions provides automatic scaling. Cosmos DB is configured with autoscale throughput. The architecture supports horizontal scaling with no code changes.

---

*This document is a living artifact and will be updated as the architecture evolves.*
