# Guild Manager

## System Architecture Diagrams

---

# 1. High Level Architecture

```mermaid
flowchart TD

A[Browser Client]

B[Azure Static Web Apps]

C[Azure Functions API Layer]

D[Azure Service Bus]

E[Azure Functions Workers]

F[Cosmos DB]

G[Azure SignalR]

H[Blob Storage]

I[Application Insights]

N[Notification Function]

A -->|HTTPS| B
B -->|API Calls| C
C -->|Publish Events| D
D -->|Trigger| E
E -->|Read / Write| F
E --> H
D -->|Trigger| N
N -->|Push| G
G -->|Real Time| A

C --> I
E --> I
N --> I
```

Explanation:

The frontend calls API endpoints hosted by Azure Functions.
Functions publish events to Service Bus.
Worker functions consume events and update system state in Cosmos DB.
The Notification Function consumes domain events and pushes real time messages to the client through Azure SignalR.

Application Insights collects telemetry across the system.

---

# 2. Core Azure Services

```mermaid
flowchart LR

Client[Browser Client]

StaticWeb[Azure Static Web Apps]

Functions[Azure Functions]

ServiceBus[Azure Service Bus]

Cosmos[Cosmos DB]

SignalR[Azure SignalR]

Blob[Blob Storage]

Insights[Application Insights]

KeyVault[Azure Key Vault]

AppConfig[Azure App Configuration]

Notify[Notification Function]

Client --> StaticWeb
StaticWeb --> Functions

Functions --> ServiceBus
Functions --> Cosmos
Functions --> Blob

ServiceBus --> Notify
Notify --> SignalR
SignalR --> Client

Functions --> Insights
Notify --> Insights

Functions --> KeyVault
Functions --> AppConfig
```

This diagram shows how operational services integrate with the runtime system.

---

# 3. Quest Workflow

```mermaid
sequenceDiagram

participant Player
participant API as StartQuestFunction
participant SB as Service Bus
participant Resolver as ResolveQuestFunction
participant CharHandler as HandleQuestResolvedFunction
participant LootGen as GenerateLootFunction
participant GoldSvc as GoldTransactionFunction
participant DB as Cosmos DB
participant Notify as Notification Function
participant SignalR as Azure SignalR

Player->>API: Start Quest
API->>DB: Update Character Status
API->>SB: Publish QuestStarted
API->>SB: Schedule QuestCompleted (delay)

SB-->>Resolver: QuestCompleted Message

Resolver->>DB: Resolve Quest
Resolver->>SB: Publish QuestResolved

SB-->>CharHandler: QuestResolved Message
CharHandler->>DB: Apply XP, set survivors Idle, clear ActiveQuestSnapshot
CharHandler->>SB: Publish CharacterDied (per character, if death)

SB-->>LootGen: QuestResolved Message (if lootEligible)
LootGen->>SB: Publish LootGenerated

SB-->>GoldSvc: QuestResolved Message (if goldAwarded > 0)
GoldSvc->>DB: Credit Gold
GoldSvc->>SB: Publish GoldCredited

SB-->>Notify: QuestResolved Message
Notify->>SignalR: Push Notification
SignalR-->>Player: Quest Result
```

Key architectural concepts:

Service Bus **scheduled messages** replace background timers.

The **QuestResolved** event is the single resolution event. Downstream consumers (`HandleQuestResolvedFunction`, `GenerateLootFunction`, `GoldTransactionFunction`) each react to `QuestResolved` independently.

---

# 4. Player Onboarding Flow

```mermaid
sequenceDiagram

participant Player
participant API as CreatePlayerFunction
participant SB as Service Bus
participant Onboard as OnboardPlayerFunction
participant DB as Cosmos DB
participant Notify as Notification Function
participant SignalR as Azure SignalR

Player->>API: Register
API->>DB: Create Player Record
API->>SB: Publish PlayerCreated

SB-->>Onboard: PlayerCreated Message

Onboard->>DB: Create Guild with Starting Gold
Onboard->>SB: Publish GuildCreated
Onboard->>SB: Publish StarterCharactersGranted
Onboard->>SB: Publish StarterItemsGranted

SB-->>Notify: GuildCreated Message
Notify->>SignalR: Push Welcome Notification
SignalR-->>Player: Welcome to your Guild
```

The onboarding flow provisions a new player's guild, starter characters, and starter items through a sequence of domain events.

---

# 5. Loot Generation Pipeline

```mermaid
flowchart LR

A[QuestResolved\nlootEligible: true]

B[GenerateLootFunction]

C[Roll Rarity]

D[Generate Stats]

E[Generate Name]

F[Publish LootGenerated]

G[HandleLootGeneratedFunction]

H[ItemAddedToInventory]

A --> B
B --> C
C --> D
D --> E
E --> F
F --> G
G --> H
```

`GenerateLootFunction` consumes `QuestResolved` when `lootEligible` is true. It procedurally generates an item (rarity, stats, name) and publishes `LootGenerated`. The downstream `HandleLootGeneratedFunction` appends the item to `Player.stash` and publishes `ItemAddedToInventory`.

---

# 6. Market Workflow

```mermaid
sequenceDiagram

participant Player
participant API
participant SB as Service Bus
participant MarketService
participant DB as Cosmos DB
participant Population
participant GoldService as GoldTransactionFunction
participant Notify as Notification Function
participant SignalR as Azure SignalR

Player->>API: List Item
API->>SB: Publish ItemListed

SB-->>MarketService: ItemListed Message
MarketService->>Population: Check Demand
MarketService->>DB: Create Listing
MarketService->>DB: Schedule Potential Sale

Note over Player,API: Alternative: Cancel Listing
Player->>API: Cancel Listing
API->>SB: Publish ItemListingCanceled

Note over MarketService,DB: Sale Path
DB-->>MarketService: Sale Triggered
MarketService->>SB: Publish ItemSold

SB-->>GoldService: ItemSold Message
GoldService->>DB: Credit Gold
GoldService->>SB: Publish GoldCredited

SB-->>Notify: ItemSold Message
Notify->>SignalR: Push Sale Notification
SignalR-->>Player: Item Sold
```

Market price is influenced by population demand.

The full lifecycle includes listing, optional cancellation, sale, and gold crediting.

---

# 7. Population Update Flow

```mermaid
flowchart TD

A[Player Event Occurs]

B{Population Update Scheduled?}

C[Schedule Population Update]

D[Service Bus Delayed Message]

E[Population Update Function]

F[Adjust Population]

G[Save to Cosmos DB]

A --> B

B -- No --> C
B -- Yes --> Stop[Do Nothing]

C --> D
D --> E
E --> F
F --> G
```

Population updates only occur when the system is active.

This prevents unnecessary compute usage.

---

# 8. Observability Architecture

```mermaid
flowchart LR

Functions[Azure Functions]

Insights[Application Insights]

Monitor[Azure Monitor]

Alerts[Alert Rules]

Dashboard[Operational Dashboard]

Functions --> Insights
Insights --> Monitor
Monitor --> Alerts
Insights --> Dashboard
```

Observability pipeline:

Logs
Metrics
Distributed traces

All collected through Application Insights.

---

# 9. Event Driven System Overview

```mermaid
flowchart LR

PlayerCreated

GuildCreated

StartQuest[QuestStarted]

QuestCompleted

QuestResolved

LootGenerated

ItemEquipped

ItemListed

ItemSold

GoldCredited

AdventurerRecruited

PopulationUpdate[PopulationUpdated]

PlayerCreated --> GuildCreated
GuildCreated --> StartQuest
StartQuest --> QuestCompleted
QuestCompleted --> QuestResolved
QuestResolved --> LootGenerated
LootGenerated --> ItemEquipped
ItemEquipped --> ItemListed
ItemListed --> ItemSold
ItemSold --> GoldCredited
GoldCredited --> AdventurerRecruited
AdventurerRecruited --> PopulationUpdate
```

This illustrates how game state progresses through events from player creation to economy activity.

---

# 10. Quest Resolution Workflow

```mermaid
flowchart TD

A[QuestCompleted Event]

B[ResolveQuestFunction]

C{Outcome?}

D[CriticalSuccess]
E[Success]
F[Failure]
G[Catastrophic Failure]

H[Publish QuestResolved]

I[Publish LootGenerated\nif lootEligible]
J[Publish GoldCredited\nCriticalSuccess / Success only]
K[Publish CharacterDied\nper character death probability]

L[Notify Player]

A --> B
B --> C

C --> D
C --> E
C --> F
C --> G

D --> H
E --> H
F --> H
G --> H

H --> I
H --> J
H --> K

I --> L
J --> L
K --> L
```

The quest resolution evaluates outcomes and publishes appropriate downstream events based on the result.

Character death is evaluated independently per character for every outcome type (CriticalSuccess 1%, Success 2%, Failure 20%, CatastrophicFailure 60% — see GDD §6). Loot is only generated when `lootEligible` is true. Gold is only awarded for CriticalSuccess and Success outcomes.

---

# 11. Recruitment Workflow

```mermaid
sequenceDiagram

participant Player
participant API as RecruitCharacterFunction
participant DB as Cosmos DB
participant SB as Service Bus
participant GoldService as GoldTransactionFunction

Player->>API: Recruit Adventurer
API->>DB: Check Gold Balance

alt Sufficient Gold
    API->>DB: Deduct Gold
    API->>SB: Publish GoldDebited
    API->>DB: Create Character
    API->>SB: Publish AdventurerRecruited
    API->>SB: Publish CharacterCreated
else Insufficient Gold
    API-->>Player: Reject (insufficient gold)
end
```

Recruitment validates the guild balance before creating the character.

---

# 12. Deployment Pipeline

```mermaid
flowchart LR

Dev[Developer]

Repo[GitHub Repository]

Build[CI Build]

Test[Automated Tests]

DeployFunctions[Deploy Azure Functions]

DeployFrontend[Deploy Static Web App]

Dev --> Repo
Repo --> Build
Build --> Test
Test --> DeployFunctions
Test --> DeployFrontend
```

Continuous integration ensures reliable deployments.

---

# Suggested Repo Layout

You could store architecture docs like this:

```
/docs
    architecture.md
    system-diagrams.md
    event-model.md
```

Keeping these diagrams in the repository mimics **real enterprise documentation practices**.

---