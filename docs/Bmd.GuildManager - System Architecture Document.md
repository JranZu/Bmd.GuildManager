# System Architecture Document

## Project: Guild Manager

Platform: Web
Technology: .NET 10 / C# / Azure Serverless
Architecture Style: Event Driven Serverless Microservices

---

# 1. Architecture Overview

Guild Manager is implemented as a **serverless event driven system**. Player actions trigger events that flow through Azure messaging infrastructure and are processed by stateless compute functions.

The architecture prioritizes:

* asynchronous workflows
* horizontal scalability
* low idle cost
* high observability

---

# 2. High Level System Diagram

Conceptually the system looks like this:

```
Browser Client
      │
      │ HTTPS
      ▼
Azure Static Web Apps
      │
      │ API calls
      ▼
Azure Functions API Layer
      │
      │ publish events
      ▼
Azure Service Bus
      │
      │ event triggers
      ▼
Azure Functions Workers
      │
      │ read/write
      ▼
Cosmos DB
```

Supporting infrastructure:

```
Application Insights
Azure Monitor
Key Vault
App Configuration
Blob Storage
SignalR
```

---

# 3. Core Azure Services

## Azure Static Web Apps

Hosts the web client.

Responsibilities:

* serve frontend application
* provide authentication integration
* route API requests to Azure Functions

Possible UI technologies:

* Blazor WebAssembly
* React
* Vue

---

## Azure Functions

Functions act as **stateless compute units**.

They implement:

* API endpoints
* event handlers
* background jobs

Example functions:

```
CreatePlayerFunction
OnboardPlayerFunction
StartQuestFunction
ResolveQuestFunction
GenerateLootFunction
EquipItemFunction
MarketListItemFunction
MarketSaleFunction
RecruitCharacterFunction
PopulationUpdateFunction
NotificationFunction
GoldTransactionFunction
```

Functions are triggered by:

* HTTP requests
* Service Bus messages
* scheduled messages

---

## Azure Service Bus

Service Bus acts as the **event backbone** of the system.

Responsibilities:

* queueing events
* decoupling services
* handling retries
* scheduling delayed messages

Example topics/queues:

```
player-events
quest-events
loot-events
market-events
economy-events
population-events
notification-events
```

Key feature used heavily:

**Scheduled Messages**

Example:

```
QuestStarted
→ schedule QuestCompleted message in 2 minutes
```

---

## Cosmos DB

Cosmos DB stores the operational state of the game.

Suggested containers:

```
Players
Characters
Items
Inventory
MarketListings
WorldPopulation
Events
```

Cosmos DB advantages:

* schema flexibility
* horizontal scaling
* low latency

---

## Azure SignalR Service

Provides real time communication with the client.

Used for:

* quest completion notifications
* market sale notifications
* character death notifications
* world event announcements

### Notification Service

A dedicated **NotificationFunction** consumes domain events from Service Bus and pushes real time messages to the player through Azure SignalR.

Events consumed by the notification service:

* QuestResolved
* ItemSold
* CharacterDied
* PopulationUpdated
* GoldCredited

The notification service acts as a consumer of domain events. It does not produce domain events itself.

---

## Azure Blob Storage

Used for inexpensive object storage.

Possible uses:

* event archives
* analytics snapshots
* system logs
* backups

---

# 4. Data Model Overview

Example entities.

### Player

```
PlayerId
GuildName
Gold
CreatedDate
```

---

### Character

```
CharacterId
PlayerId
Name
Level
Strength
Luck
Endurance
Status
EquipmentIds
```

Status values:

```
Idle
OnQuest
Dead
```

---

### Item

```
ItemId
Name
Tier
Rarity
Stats
OwnerId
```

---

### Market Listing

```
ListingId
ItemId
SellerId
Price
Tier
CreatedDate
```

---

### World Population

Single global record.

```
BeginnerPopulation
VeteranPopulation
ElitePopulation
EpicPopulation
PopulationUpdateScheduled
LastUpdated
```

---

# 5. Event Model

Events represent state transitions.

Example event types:

```
PlayerCreated
GuildCreated
StarterCharactersGranted
StarterItemsGranted
CharacterCreated
AdventurerRecruited
QuestStarted
QuestCompleted
QuestResolved
CharacterDied
LootGenerated
ItemAddedToInventory
ItemEquipped
ItemUnequipped
ItemDiscarded
ItemListed
ItemSold
ItemListingCanceled
GoldCredited
GoldDebited
PopulationUpdateScheduled
PopulationUpdated
```

Events are published to Service Bus.

---

# 6. Example System Workflows

## Player Onboarding

```
New player registers
→ HTTP request to CreatePlayerFunction
→ PlayerCreated event published
→ OnboardPlayerFunction runs
→ guild created with starting gold
→ GuildCreated event published
→ starter characters granted (optional)
→ starter items granted (optional)
```

---

## Quest Start

```
Player clicks Start Quest
→ HTTP request to StartQuestFunction
→ Character state updated
→ QuestStarted event published
→ Service Bus scheduled message created
```

---

## Quest Completion

When scheduled message fires:

```
QuestCompleted event received
→ ResolveQuestFunction runs
→ calculate success/failure
→ death probability evaluated
→ QuestResolved event published
→ LootGenerated event published (if loot awarded)
→ GoldCredited event published (if gold awarded)
→ CharacterDied event published (if death occurred)
→ player notified via NotificationFunction
```

---

## Market Listing

```
ItemListed event
→ MarketPricingFunction
→ calculate demand multiplier
→ listing stored
→ optional sale scheduled
```

---

## Population Update

Population updates occur only when the system is active.

Trigger process:

```
PlayerEvent
→ EnsurePopulationUpdateScheduled
→ schedule PopulationUpdate in 5 minutes
```

Execution:

```
PopulationUpdateFunction
→ apply random population changes
→ update population record
→ publish PopulationUpdated event
```

---

## Adventurer Recruitment

```
Player requests recruitment
→ HTTP request to RecruitCharacterFunction
→ guild gold balance validated
→ GoldDebited event published
→ character generated with random stats
→ AdventurerRecruited event published
→ CharacterCreated event published
```

If the guild does not have sufficient gold the request is rejected.

---

## Item Sale

```
ItemSold event received
→ GoldTransactionFunction runs
→ GoldCredited event published
→ guild balance updated
→ player notified via NotificationFunction
```

---

# 7. Reliability Architecture

The system uses several distributed system reliability patterns.

### Retry Policies

Service Bus automatically retries failed message processing.

---

### Dead Letter Queues

Messages that fail repeatedly are moved to a Dead Letter Queue.

This allows inspection and manual reprocessing.

---

### Idempotent Handlers

Functions must tolerate duplicate messages.

Example:

```
QuestCompleted processed twice
→ second execution ignored
```

---

### Scheduled Messaging

Used for delayed workflows like quest completion.

This avoids polling or background timers.

---

# 8. Concurrency and Domain Rules

The system enforces domain invariants to prevent conflicting state transitions.

### Character Rules

* A character cannot join multiple quests simultaneously.
* A dead character cannot be assigned to a quest.
* Equipment changes are blocked while a character is on a quest.

### Item Rules

* An item cannot be equipped and listed for sale at the same time.
* An equipped item must be unequipped before it can be sold or discarded.

### Market Rules

* A listing can only be sold once.
* A canceled listing cannot be sold.

### Gold Rules

* Gold spending must validate the current guild balance before deducting.
* Insufficient balance must reject the transaction.

### Optimistic Concurrency

State mutations use **Cosmos DB optimistic concurrency** with ETags.

Each write includes the document ETag. If a conflicting update has occurred, the write fails and the operation is retried or rejected.

This pattern prevents race conditions in concurrent workflows such as simultaneous quest starts or market sales.

---

# 9. Economy System

Guild finances are managed through gold transaction events.

### Earning Gold

Gold is credited to the guild when:

* an item is sold on the market (ItemSold → GoldCredited)
* a quest awards a gold bonus (QuestResolved → GoldCredited)

### Spending Gold

Gold is debited from the guild when:

* an adventurer is recruited (AdventurerRecruited → GoldDebited)

### Gold Transaction Flow

```
Spending action requested
→ validate guild gold balance
→ if sufficient → deduct gold → publish GoldDebited
→ if insufficient → reject transaction
```

Earning and spending events are processed by the **GoldTransactionFunction** which updates the guild balance in Cosmos DB.

---

# 10. Observability

Application Insights provides system telemetry.

Three categories are collected.

### Logs

Structured logging using ILogger.

Example:

```
Quest {QuestId} completed for player {PlayerId}
```

---

### Metrics

Custom metrics tracked:

* quests started
* quests completed
* market sales
* character deaths
* population tier distribution

---

### Distributed Tracing

Application Insights correlates events across services.

Example trace:

```
HTTP Request
→ StartQuestFunction
→ Service Bus message
→ ResolveQuestFunction
→ GenerateLootFunction
→ Cosmos DB write
```

---

# 11. Monitoring and Alerts

Azure Monitor provides operational alerts.

Examples:

```
Function failure rate > threshold
Service Bus queue depth high
Cosmos DB RU consumption spikes
```

Alerts notify operators of system issues.

---

# 12. Configuration Management

Azure App Configuration stores runtime settings.

Examples:

```
QuestDurationSettings
DeathProbability
MarketBasePrices
PopulationUpdateInterval
```

Configuration can be updated without redeploying code.

---

# 13. Secrets Management

Azure Key Vault stores sensitive data.

Examples:

```
CosmosDB connection string
Service Bus connection string
API keys
```

Applications retrieve secrets securely.

---

# 14. Deployment Pipeline

CI/CD pipeline automates deployment.

Example pipeline:

```
GitHub push
→ build .NET 10 solution
→ run tests
→ deploy Azure Functions
→ deploy Static Web App
```

GitHub Actions or Azure DevOps can be used.

---

# 15. Security Model

Security measures include:

* HTTPS for all communication
* managed identities for service authentication
* secrets stored in Key Vault
* API endpoints protected by authentication

---

# 16. Scalability Model

The architecture scales automatically.

Azure Functions scale based on:

* HTTP traffic
* Service Bus queue depth

Cosmos DB scales using provisioned throughput.

Service Bus handles message buffering and load balancing.

---

# 17. Architectural Benefits

This architecture provides:

Low idle cost
High scalability
Loose service coupling
Fault tolerant workflows
Strong observability

It mirrors patterns used in modern enterprise cloud systems.

---

# 18. Known Tradeoffs

Event driven systems introduce complexity.

Potential challenges include:

* eventual consistency
* debugging asynchronous workflows
* message duplication handling

Proper logging and tracing mitigate these issues.

---

# Final Note

Guild Manager serves as both:

* a playable management game
* a practical demonstration of **cloud native architecture**

The system intentionally exercises real world architectural patterns such as event driven workflows, serverless compute, distributed messaging, and observability.

---
