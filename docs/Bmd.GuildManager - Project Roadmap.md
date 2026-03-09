# Guild Manager — Project Roadmap

## Purpose

This document serves two purposes:

1. **Primary** — Organizes and directs development work in a clear, sequential order.
2. **Secondary** — Acts as a living project status document. Update the `Status` field of each phase as work progresses.

## How to Use This Document

- Phases are **strictly sequential**. Do not begin a phase until all acceptance criteria of the prior phase are met.
- Each phase references the relevant design documents rather than repeating their content. Read those documents alongside this roadmap.
- Acceptance criteria define **done**. A phase is complete only when every criterion is verifiable.

## Status Key

| Symbol | Meaning         |
|--------|-----------------|
| ⬜     | Not started     |
| 🔄     | In progress     |
| ✅     | Complete        |

---

## Reference Documents

| Document | Path |
|----------|------|
| Game Design Document (GDD) | `docs/Bmd.GuildManager - Game Design Document.md` |
| Event Contract Specification (ECS) | `docs/Bmd.GuildManager - Event Contract Specification.md` |
| System Architecture Document (SAD) | `docs/Bmd.GuildManager - System Architecture Document.md` |
| System Architecture Diagrams | `docs/Bmd.GuildManager - System Architecture Diagrams.md` |

---

## Phase Summary

| # | Phase | Status |
|---|-------|--------|
| 1 | Repository & Solution Scaffold | ✅ |
| 2 | Azure Infrastructure Baseline | ⬜ |
| 3 | CI/CD Pipeline Activation | ⬜ |
| 4 | Event Envelope & Shared Contracts | ⬜ |
| 5 | Player & Guild Creation | ⬜ |
| 6 | Player Onboarding Flow | ⬜ |
| 7 | Character Domain | ⬜ |
| 8 | Quest Start | ⬜ |
| 9 | Quest Completion & Resolution | ⬜ |
| 10 | Character Death | ⬜ |
| 11 | Loot Generation | ⬜ |
| 12 | Inventory Management | ⬜ |
| 13 | Item Equip & Unequip | ⬜ |
| 14 | Gold & Economy System | ⬜ |
| 15 | Adventurer Recruitment | ⬜ |
| 16 | Market — Listing & Cancellation | ⬜ |
| 17 | Market — Sales & Pricing | ⬜ |
| 18 | Simulated Population System | ⬜ |
| 19 | Analytics Consumer & Event Archiving | ⬜ |
| 20 | World News Generator | ⬜ |
| 21 | Real-Time Notifications (SignalR) | ⬜ |
| 22 | Blazor Frontend — Shell & Auth | ⬜ |
| 23 | Blazor Frontend — Characters Panel | ⬜ |
| 24 | Blazor Frontend — Quests Panel | ⬜ |
| 25 | Blazor Frontend — Guild Management Panel | ⬜ |
| 26 | Observability & Structured Logging | ⬜ |
| 27 | Reliability Hardening | ⬜ |
| 28 | Configuration & Secrets Management | ⬜ |
| 29 | API Management & Security Hardening | ⬜ |
| 30 | End-to-End Integration Testing | ⬜ |

---

## Phases

---

### Phase 1 — Repository & Solution Scaffold

**Status:** ✅

**Goal:** Establish the foundational repository structure and .NET solution layout so all subsequent phases have a consistent place to build.

**Reference:** `backend/README.md`, `frontend/README.md`, root `README.md`

**Work Items:**

- Create the backend .NET solution under `/backend` with three projects:
  - `Bmd.GuildManager.Functions` — Azure Functions isolated worker
  - `Bmd.GuildManager.Core` — domain models and business logic
  - `Bmd.GuildManager.Tests` — unit and integration tests
- Create the Blazor WebAssembly project under `/frontend`
- Confirm all projects target `net10.0`
- Confirm nullable reference types are enabled in all projects
- Confirm warnings are treated as errors in all projects
- Verify the solution builds locally with `dotnet build`

**Acceptance Criteria:**

- [x] `dotnet build ./backend` succeeds with zero warnings and zero errors
- [x] `dotnet build ./frontend` succeeds with zero warnings and zero errors
- [x] `dotnet test ./backend` runs and reports zero failures (empty test suite is acceptable at this stage)
- [x] All projects declare `net10.0`, `<Nullable>enable</Nullable>`, and `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`

---

### Phase 2 — Azure Infrastructure Baseline

**Status:** ✅

**Goal:** Provision all Azure resources required by the architecture so that subsequent phases can target real infrastructure.

**Reference:** SAD §3 (Core Azure Services), SAD §15 (Security Model)

**Work Items:**

**Provisioned Resources (`dev`):**

| Resource | Name |
|----------|------|
| Resource Group | `rg-guildmanager-dev` |
| Cosmos DB | `cosmos-guildmanager-dev` |
| Service Bus | `sb-guildmanager-dev` |
| Storage Account | `stguildmanagerdev` |
| Function App | `func-guildmanager-dev` |
| Static Web Apps | `stapp-guildmanager-dev` |
| SignalR | `sigr-guildmanager-dev` |
| Key Vault | `kv-guildmanager-dev` |
| App Configuration | `appcs-guildmanager-dev` |
| API Management | `apim-guildmanager-dev` |
| Application Insights | `appi-guildmanager-dev` |

**Acceptance Criteria:**

- [x] All resources exist and are accessible in the Azure portal
- [x] Cosmos DB containers are created: `Players`, `Characters`, `Items`, `Inventory`, `MarketListings`, `WorldPopulation`, `Events`
- [x] Service Bus topics/queues are created: `player-events`, `quest-events`, `loot-events`, `market-events`, `economy-events`, `population-events`, `notification-events`, `analytics-events`
- [x] Azure API Management instance is provisioned (optional per SAD §3; provision now so Phase 29 has a target)
- [x] Azure Functions can connect to Cosmos DB and Service Bus using managed identity or connection strings stored in Key Vault
- [x] Application Insights instrumentation key is retrievable

---

### Phase 3 — CI/CD Pipeline Activation

**Status:** ⬜

**Goal:** Activate the existing CI/CD pipeline so every push to `main` builds, tests, and deploys automatically.

**Reference:** `.github/workflows/ci-cd.yml`, root `README.md` (CI/CD section)

**Work Items:**

- Uncomment the backend build, test, and deploy steps in `ci-cd.yml`
- Uncomment the frontend build and deploy steps in `ci-cd.yml`
- Configure required GitHub repository secrets:
  - `AZURE_FUNCTIONAPP_PUBLISH_PROFILE`
  - `AZURE_STATIC_WEB_APPS_API_TOKEN`
- Configure required repository variable: `AZURE_FUNCTIONAPP_NAME`

**Acceptance Criteria:**

- [ ] A push to `main` triggers the pipeline
- [ ] Backend build and test job passes
- [ ] Frontend build job passes
- [ ] Backend deploys successfully to Azure Functions
- [ ] Frontend deploys successfully to Azure Static Web Apps
- [ ] A pull request targeting `main` runs build and test jobs but does not deploy

---

### Phase 4 — Event Envelope & Shared Contracts

**Status:** ⬜

**Goal:** Implement the shared event envelope and all event payload types in `Bmd.GuildManager.Core` so every subsequent phase can publish and consume typed events.

**Reference:** ECS §1 (Event Envelope), ECS §12 (Event Naming Conventions), ECS §10 (Versioning Strategy)

**Work Items:**

- Implement the base event envelope model as described in ECS §1
- Implement strongly typed C# record or class for every event payload defined in the ECS
- Write unit tests verifying correct JSON serialization of the envelope and all payload types

**Acceptance Criteria:**

- [ ] A base `EventEnvelope<T>` type exists with all fields from ECS §1
- [ ] A C# type exists for every event defined in the ECS (sections 2–9)
- [ ] All types serialize to valid JSON matching the ECS schemas
- [ ] All types deserialize correctly from their JSON representations
- [ ] `dotnet test` passes with full coverage of serialization round-trips

---

### Phase 5 — Player & Guild Creation

**Status:** ⬜

**Goal:** Implement the `CreatePlayerFunction` HTTP endpoint. A player can register, which creates a Player record, publishes `PlayerCreated`, and returns a player ID.

**Reference:** SAD §6 (Player Onboarding workflow), ECS §2 (`PlayerCreated`), SAD §4 (Player data model)

**Work Items:**

- Implement `CreatePlayerFunction` as an HTTP-triggered Azure Function
- Persist a `Player` document to Cosmos DB (`Players` container)
- Publish a `PlayerCreated` event to Service Bus (`player-events`)
- Return the new `playerId` in the HTTP response

**Acceptance Criteria:**

- [ ] `POST /api/players` with a valid body returns HTTP 201 and a `playerId`
- [ ] A `Player` document exists in Cosmos DB with correct fields
- [ ] A `PlayerCreated` event appears on the `player-events` topic with a valid envelope
- [ ] Duplicate registration (same request replayed) does not create a second player (idempotency)
- [ ] Unit tests cover the function logic
- [ ] Integration test verifies the Cosmos DB write and Service Bus publish

---

### Phase 6 — Player Onboarding Flow

**Status:** ⬜

**Goal:** Implement `OnboardPlayerFunction` which reacts to `PlayerCreated` and fully provisions the player's guild, starter characters, and starter items.

**Reference:** SAD §6 (Player Onboarding workflow), ECS §2 (`GuildCreated`, `StarterCharactersGranted`, `StarterItemsGranted`), GDD §11 (Guild Management)

**Work Items:**

- Implement `OnboardPlayerFunction` as a Service Bus-triggered function on `player-events`
- Create the player's guild with 500 starting gold and persist to Cosmos DB
- Publish `GuildCreated` event
- Grant starter characters (optional) and publish `StarterCharactersGranted`
- Grant starter items (optional) and publish `StarterItemsGranted`

**Acceptance Criteria:**

- [ ] A `PlayerCreated` event on `player-events` triggers onboarding
- [ ] A guild record exists in Cosmos DB with `startingGold: 500`
- [ ] `GuildCreated` event is published with correct `playerId` and `guildName`
- [ ] `StarterCharactersGranted` and `StarterItemsGranted` events are published when starter grants are configured
- [ ] Replaying the same `PlayerCreated` event does not create duplicate guild records (idempotency via `eventId` check)
- [ ] Unit and integration tests pass

---

### Phase 7 — Character Domain

**Status:** ⬜

**Goal:** Implement the `CharacterCreated` event handler and character persistence so characters can be stored, retrieved, and validated for use in later phases.

**Reference:** ECS §3 (`CharacterCreated`), SAD §4 (Character data model), GDD §4 (Characters)

**Work Items:**

- Implement a Service Bus consumer that handles `CharacterCreated` and persists a `Character` document to Cosmos DB
- Implement a `GET /api/players/{playerId}/characters` HTTP endpoint returning the player's roster
- Enforce character status values: `Idle`, `OnQuest`, `Dead`

**Acceptance Criteria:**

- [ ] A `CharacterCreated` event results in a persisted `Character` document with correct fields
- [ ] `GET /api/players/{playerId}/characters` returns the correct roster
- [ ] Character status defaults to `Idle` on creation
- [ ] Unit tests cover status validation logic

---

### Phase 8 — Quest Start

**Status:** ⬜

**Goal:** Implement `StartQuestFunction` so a player can dispatch characters on a quest. This phase introduces Service Bus scheduled messages.

**Reference:** SAD §6 (Quest Start workflow), ECS §4 (`QuestStarted`), GDD §5 (Quests), ECS §11 (Concurrency Rules — Character Rules), SAD §8 (Concurrency)

**Work Items:**

- Implement `StartQuestFunction` as an HTTP-triggered Azure Function
- Validate that all assigned characters are `Idle` and not `Dead`
- Update character status to `OnQuest` in Cosmos DB using optimistic concurrency (ETag)
- Publish `QuestStarted` event
- Schedule a `QuestCompleted` Service Bus message with the configured delay

**Acceptance Criteria:**

- [ ] `POST /api/quests` with valid characters returns HTTP 202 and a `questId`
- [ ] All assigned characters have status `OnQuest` in Cosmos DB
- [ ] A `QuestStarted` event is published with correct fields
- [ ] A `QuestCompleted` message is scheduled on Service Bus for the correct future time
- [ ] Assigning a `Dead` or `OnQuest` character returns HTTP 409
- [ ] Concurrent requests for the same character resolve correctly via ETag conflict detection
- [ ] Unit and integration tests pass

---

### Phase 9 — Quest Completion & Resolution

**Status:** ⬜

**Goal:** Implement `ResolveQuestFunction` which consumes the scheduled `QuestCompleted` message and publishes a `QuestResolved` event capturing the full outcome.

**Reference:** SAD §6 (Quest Completion workflow), ECS §4 (`QuestCompleted`, `QuestResolved`), GDD §6 (Quest Resolution), SAD §10 (Quest Resolution diagram)

**Work Items:**

- Implement `ResolveQuestFunction` triggered by the `QuestCompleted` Service Bus message
- Calculate quest outcome using team power vs. difficulty (GDD §6)
- Set character statuses back to `Idle` for survivors
- Publish `QuestResolved` with correct outcome, character survival list, and reward flags

**Acceptance Criteria:**

- [ ] A `QuestCompleted` message triggers resolution
- [ ] `QuestResolved` is published with one of: `Success`, `PartialSuccess`, `Failure`, `CatastrophicFailure`
- [ ] Surviving characters return to `Idle` status in Cosmos DB
- [ ] `lootGenerated` and `goldAwarded` fields in `QuestResolved` are correct for each outcome type
- [ ] Replaying a `QuestCompleted` message does not resolve the same quest twice
- [ ] Unit tests cover all four outcome types including boundary conditions

---

### Phase 10 — Character Death

**Status:** ⬜

**Goal:** Implement character death handling triggered by `QuestResolved`. Dead characters are permanently removed from active use and their equipped items are marked as lost.

**Reference:** ECS §3 (`CharacterDied`), GDD §6 (Character Death), ECS §11 (Item State Machine — `Lost` state)

**Work Items:**

- Within `ResolveQuestFunction` (or a downstream consumer), publish `CharacterDied` for each character that did not survive
- Implement a handler that sets character status to `Dead` in Cosmos DB
- Mark all items equipped by the dead character as `Lost` (terminal state)

**Acceptance Criteria:**

- [ ] `CharacterDied` is published for every character that dies in a `CatastrophicFailure` (or probabilistic death in other outcomes per GDD §6)
- [ ] Dead characters have status `Dead` in Cosmos DB and cannot be assigned to future quests
- [ ] Items equipped to dead characters transition to `Lost` state in Cosmos DB
- [ ] `Lost` items cannot be equipped, sold, or discarded
- [ ] Unit tests cover death state transitions and item loss

---

### Phase 11 — Loot Generation

**Status:** ⬜

**Goal:** Implement `GenerateLootFunction` which reacts to `QuestResolved` (when `lootGenerated: true`) and procedurally creates an item, publishing `LootGenerated`.

**Reference:** ECS §5 (`LootGenerated`), GDD §7 (Loot System), SAD §6 (Loot Generation Pipeline diagram)

**Work Items:**

- Implement `GenerateLootFunction` triggered by `QuestResolved` on Service Bus
- Procedurally generate item name, tier, rarity, and stats
- Persist item to Cosmos DB (`Items` container) with status `InInventory`
- Publish `LootGenerated` event

**Acceptance Criteria:**

- [ ] `QuestResolved` with `lootGenerated: true` produces a `LootGenerated` event
- [ ] `QuestResolved` with `lootGenerated: false` produces no loot
- [ ] Generated item exists in Cosmos DB with status `InInventory`
- [ ] Item tier and rarity values are valid per GDD §7
- [ ] `LootGenerated` is followed by `ItemAddedToInventory` (see Phase 12)
- [ ] Unit tests cover procedural generation logic

---

### Phase 12 — Inventory Management

**Status:** ⬜

**Goal:** Implement inventory persistence and the `ItemAddedToInventory` event so items are tracked per player and can be retrieved.

**Reference:** ECS §5 (`ItemAddedToInventory`), SAD §4 (Inventory data model), GDD §7 (Loot System)

**Work Items:**

- Implement a handler for `LootGenerated` that publishes `ItemAddedToInventory` and updates the `Inventory` container in Cosmos DB
- Implement `GET /api/players/{playerId}/inventory` HTTP endpoint
- Implement `DELETE /api/players/{playerId}/inventory/{itemId}` (discard) publishing `ItemDiscarded` and setting item state to `Discarded`

**Acceptance Criteria:**

- [ ] `LootGenerated` results in an `ItemAddedToInventory` event and a Cosmos DB inventory record
- [ ] `GET /api/players/{playerId}/inventory` returns the correct item list
- [ ] Discarding an item publishes `ItemDiscarded` and sets item state to `Discarded` (terminal)
- [ ] Discarding an `Equipped` or `ListedForSale` item returns HTTP 409
- [ ] Discarding a `Discarded`, `Sold`, or `Lost` item returns HTTP 409
- [ ] Unit tests cover all item state transition validations

---

### Phase 13 — Item Equip & Unequip

**Status:** ⬜

**Goal:** Allow players to equip and unequip items on idle characters. Enforces all item and character state rules.

**Reference:** ECS §5 (`ItemEquipped`, `ItemUnequipped`), ECS §11 (Item State Machine, Concurrency Rules), GDD §7, GDD §11

**Work Items:**

- Implement `POST /api/players/{playerId}/characters/{characterId}/equip` publishing `ItemEquipped`
- Implement `POST /api/players/{playerId}/characters/{characterId}/unequip` publishing `ItemUnequipped`
- Update item state to `Equipped` / `InInventory` accordingly in Cosmos DB using optimistic concurrency

**Acceptance Criteria:**

- [ ] Equipping an `InInventory` item on an `Idle` character succeeds and sets item state to `Equipped`
- [ ] Equipping an item on an `OnQuest` character returns HTTP 409
- [ ] Equipping an already `Equipped` or `ListedForSale` item returns HTTP 409
- [ ] Unequipping an `Equipped` item returns it to `InInventory`
- [ ] `ItemEquipped` and `ItemUnequipped` events are published with correct fields
- [ ] Concurrent equip requests for the same item resolve correctly via ETag
- [ ] Unit and integration tests pass

---

### Phase 14 — Gold & Economy System

**Status:** ⬜

**Goal:** Implement the gold transaction system so the guild balance can be credited and debited reliably with full event publication.

**Reference:** ECS §7 (`GoldCredited`, `GoldDebited`), SAD §9 (Economy System), GDD §8 (Market System — gold earning), ECS §11 (Gold Rules)

**Work Items:**

- Implement `GoldTransactionFunction` triggered by relevant Service Bus events
- Handle `GoldCredited`: increase guild gold balance and persist to Cosmos DB
- Handle `GoldDebited`: validate balance, decrease gold, or reject if insufficient
- Use optimistic concurrency (ETag) on all balance writes

**Acceptance Criteria:**

- [ ] `GoldCredited` event increases guild balance in Cosmos DB
- [ ] `GoldDebited` event decreases guild balance when funds are sufficient
- [ ] `GoldDebited` with insufficient funds does not modify the balance and emits a rejection signal
- [ ] Replaying the same `GoldCredited` or `GoldDebited` event does not double-apply the transaction
- [ ] ETag conflict on concurrent balance writes triggers retry or rejection
- [ ] Unit tests cover credit, debit, insufficient funds, and idempotency scenarios

---

### Phase 15 — Adventurer Recruitment

**Status:** ⬜

**Goal:** Allow players to spend gold to recruit a new adventurer with randomly generated stats.

**Reference:** SAD §6 (Adventurer Recruitment workflow), ECS §7 (`AdventurerRecruited`), GDD §11 (Recruitment), SAD §11 (Recruitment diagram)

**Work Items:**

- Implement `RecruitCharacterFunction` as an HTTP-triggered Azure Function
- Validate guild gold balance before proceeding
- Publish `GoldDebited` and deduct gold
- Generate character with random stats (GDD §4)
- Publish `AdventurerRecruited` and `CharacterCreated`

**Acceptance Criteria:**

- [ ] `POST /api/players/{playerId}/recruit` with sufficient gold returns HTTP 201 and a `characterId`
- [ ] Guild gold balance is reduced by the recruitment cost
- [ ] `GoldDebited` event is published with `reason: "Recruitment"`
- [ ] `AdventurerRecruited` and `CharacterCreated` events are published
- [ ] New character appears in the player roster with status `Idle`
- [ ] Request with insufficient gold returns HTTP 402 and no character is created
- [ ] Unit and integration tests pass

---

### Phase 16 — Market — Listing & Cancellation

**Status:** ⬜

**Goal:** Allow players to list items for sale and cancel active listings. Enforces item state rules and publishes correct events.

**Reference:** ECS §6 (`ItemListed`, `ItemListingCanceled`), SAD §6 (Market Listing workflow), GDD §8 (Market System), ECS §11 (Item State Machine)

**Work Items:**

- Implement `POST /api/players/{playerId}/market/list` publishing `ItemListed` and setting item state to `ListedForSale`
- Implement `DELETE /api/players/{playerId}/market/{listingId}` publishing `ItemListingCanceled` and returning item to `InInventory`
- Persist listing to `MarketListings` container

**Acceptance Criteria:**

- [ ] Listing an `InInventory` item sets its state to `ListedForSale` and creates a `MarketListings` record
- [ ] `ItemListed` event is published with correct fields
- [ ] Listing an `Equipped` item returns HTTP 409
- [ ] Canceling an active listing returns the item to `InInventory` and publishes `ItemListingCanceled`
- [ ] Canceling an already-sold or non-existent listing returns HTTP 409 / 404
- [ ] Unit and integration tests pass

---

### Phase 17 — Market — Sales & Pricing

**Status:** ⬜

**Goal:** Implement market sale execution and dynamic pricing driven by the simulated population demand.

**Reference:** ECS §6 (`ItemSold`), SAD §6 (Market Workflow diagram), GDD §8 (Market System — pricing formula), ECS §11 (Market Rules)

**Work Items:**

- Implement `MarketPricingFunction` triggered by `ItemListed` to calculate demand-adjusted price and schedule a potential sale
- Implement `MarketSaleFunction` triggered by the scheduled sale message
- Publish `ItemSold` and transition item to `Sold` (terminal state)
- Trigger `GoldCredited` for the seller

**Acceptance Criteria:**

- [ ] `ItemListed` triggers price calculation using `basePrice × demand ÷ supply` (GDD §8)
- [ ] A sale is scheduled via Service Bus delayed message
- [ ] `ItemSold` event is published with correct `listingId`, `finalPrice`, and `buyerTier`
- [ ] Item state transitions to `Sold` in Cosmos DB
- [ ] `GoldCredited` is published for the seller with `reason: "ItemSold"`
- [ ] A canceled listing does not trigger a sale
- [ ] A listing cannot be sold more than once
- [ ] Unit tests cover the pricing formula and sale concurrency guard

---

### Phase 18 — Simulated Population System

**Status:** ⬜

**Goal:** Implement the population update system that adjusts NPC tier counts over time and influences market demand.

**Reference:** ECS §8 (`PopulationUpdateScheduled`, `PopulationUpdated`), ECS §9 (`PlayerEventOccurred`), SAD §6 (Population Update Flow), GDD §9–10 (Population)

**Work Items:**

- Implement `PlayerEventOccurred` publishing on relevant player actions
- Implement scheduling logic: if no update is already scheduled, queue a `PopulationUpdateScheduled` message delayed 5 minutes
- Implement `PopulationUpdateFunction` triggered by that message
- Apply random population changes respecting minimum tier floors (GDD §10)
- Persist updated `WorldPopulation` record
- Publish `PopulationUpdated`

**Acceptance Criteria:**

- [ ] A player action triggers a `PlayerEventOccurred` event
- [ ] If no update is scheduled, a `PopulationUpdateScheduled` message is queued with a 5-minute delay
- [ ] If an update is already scheduled, no additional message is queued
- [ ] `PopulationUpdateFunction` adjusts tier populations with valid random changes
- [ ] No tier falls below its defined minimum floor
- [ ] `PopulationUpdated` is published with the new tier counts
- [ ] `WorldPopulation` record in Cosmos DB reflects the updated values
- [ ] Unit tests cover scheduling deduplication and population floor enforcement

---

### Phase 19 — Analytics Consumer & Event Archiving

**Status:** ⬜

**Goal:** Implement a dedicated analytics consumer that subscribes to all analytically significant events and archives them to Azure Blob Storage for offline analysis and auditing.

**Reference:** ECS §2–§8 (analytics listed as a consumer on `PlayerCreated`, `GuildCreated`, `CharacterCreated`, `QuestStarted`, `LootGenerated`, `ItemDiscarded`, `ItemSold`, `GoldCredited`, `GoldDebited`, `PopulationUpdated`, and others), SAD §3 (Blob Storage — event archives, analytics snapshots)

**Work Items:**

- Create an `analytics-events` Service Bus subscription (or topic forward) that receives a copy of all analytically relevant events
- Implement `AnalyticsArchiveFunction` triggered by the `analytics-events` topic
- Serialize each received event envelope to JSON and write it to Azure Blob Storage using a partitioned path (e.g. `analytics/{eventType}/YYYY/MM/DD/{eventId}.json`)
- Ensure the function is idempotent: writing the same `eventId` twice overwrites rather than duplicates

**Acceptance Criteria:**

- [ ] `analytics-events` topic/subscription exists on Service Bus and receives forwarded copies of: `PlayerCreated`, `GuildCreated`, `CharacterCreated`, `QuestStarted`, `QuestResolved`, `LootGenerated`, `ItemDiscarded`, `ItemSold`, `GoldCredited`, `GoldDebited`, `AdventurerRecruited`, `PopulationUpdated`
- [ ] Each received event is written to Blob Storage at the correct partitioned path within a configurable time window
- [ ] Blob content is valid JSON matching the event envelope schema (ECS §1)
- [ ] Replaying an event with the same `eventId` does not create a duplicate blob (idempotency)
- [ ] Unit tests cover path generation and serialization logic
- [ ] Integration test confirms a published event results in a blob in the correct container and path

---

### Phase 20 — World News Generator

**Status:** ⬜

**Goal:** Implement a backend service that consumes `CharacterDied` and `PopulationUpdated` events, generates human-readable world news messages, and persists them as a queryable log — distinct from the real-time SignalR push handled in Phase 21.

**Reference:** ECS §3 (`CharacterDied` — consumer: world news system), ECS §8 (`PopulationUpdated` — consumer: world news generator), GDD §13 (World Events)

**Work Items:**

- Implement `WorldNewsFunction` triggered by `CharacterDied` and `PopulationUpdated` events on Service Bus
- Generate a world news message string for each event type using templates from GDD §13
- Persist each news entry to a `WorldNews` container in Cosmos DB (fields: `newsId`, `message`, `eventType`, `timestamp`)
- Expose a `GET /api/world/news` HTTP endpoint returning the most recent N news entries

**Acceptance Criteria:**

- [ ] A `CharacterDied` event produces a world news entry in Cosmos DB referencing the character tier and event
- [ ] A `PopulationUpdated` event produces a world news entry reflecting the population change
- [ ] `GET /api/world/news` returns entries in reverse chronological order
- [ ] News entries are human-readable strings consistent with the examples in GDD §13
- [ ] The endpoint is paginated or limited to avoid unbounded result sets
- [ ] Unit tests cover message template generation for all supported event types
- [ ] Integration test confirms event → Cosmos DB persistence round-trip

---

### Phase 21 — Real-Time Notifications (SignalR)

**Status:** ⬜

**Goal:** Implement the `NotificationFunction` that consumes domain events and pushes real-time updates to connected players via Azure SignalR.

**Reference:** SAD §3 (Azure SignalR Service, Notification Service), SAD §6 (Quest Workflow diagram — SignalR steps), ECS events consumed: `QuestResolved`, `ItemSold`, `CharacterDied`, `PopulationUpdated`, `GoldCredited`

**Work Items:**

- Implement `NotificationFunction` triggered by the `notification-events` Service Bus topic
- Route each event type to an appropriate player-scoped SignalR message
- Connect Azure SignalR Service to the function

**Acceptance Criteria:**

- [ ] `QuestResolved` pushes a quest result notification to the correct player
- [ ] `ItemSold` pushes a sale notification to the correct player
- [ ] `CharacterDied` pushes a death notification to the correct player
- [ ] `GoldCredited` pushes a balance update notification to the correct player
- [ ] `PopulationUpdated` pushes a world news message (broadcast)
- [ ] Notifications are only delivered to the target player (not broadcast, except world news)
- [ ] Integration test verifies SignalR message delivery

---

### Phase 22 — Blazor Frontend — Shell & Auth

**Status:** ⬜

**Goal:** Scaffold the Blazor WebAssembly application with routing, layout, and authentication so subsequent UI phases have a working shell.

**Reference:** SAD §3 (Azure Static Web Apps), GDD §12 (User Interface), `frontend/README.md`

**Work Items:**

- Scaffold Blazor WebAssembly project under `/frontend`
- Implement app shell with navigation and three main panel placeholders (Characters, Active Quests, Guild Management)
- Integrate authentication via Azure Static Web Apps built-in auth
- Configure API base URL via environment/configuration

**Acceptance Criteria:**

- [ ] Application loads in browser via Azure Static Web Apps URL
- [ ] Navigation between three main panel placeholders works
- [ ] Unauthenticated users are redirected to login
- [ ] Authenticated users see their `playerId` available in app state
- [ ] API calls include authentication headers
- [ ] Frontend build passes in CI/CD pipeline

---

### Phase 23 — Blazor Frontend — Characters Panel

**Status:** ⬜

**Goal:** Implement the Characters panel showing the player's roster with status, stats, equipped items, and recruitment action.

**Reference:** GDD §4 (Characters), GDD §12 (UI — Characters panel)

**Work Items:**

- Display roster fetched from `GET /api/players/{playerId}/characters`
- Show character name, level, stats, status badge (`Idle`, `OnQuest`, `Dead`)
- Show equipped items per character
- Provide equip/unequip item actions
- Provide recruit adventurer action (calls Phase 15 endpoint)

**Acceptance Criteria:**

- [ ] Roster loads and displays correctly for a player with multiple characters
- [ ] Status badges update when characters go on quests or die (via SignalR)
- [ ] Equip/unequip actions call the correct API and update the UI
- [ ] Recruit action deducts gold and adds the new character to the roster
- [ ] Dead characters are visually distinguished and non-interactive

---

### Phase 24 — Blazor Frontend — Quests Panel

**Status:** ⬜

**Goal:** Implement the Active Quests panel showing running quests and their countdown timers, and allow starting new quests.

**Reference:** GDD §5 (Quests), GDD §12 (UI — Active quests panel)

**Work Items:**

- Display active quests with quest type, assigned characters, and live countdown timer
- Provide a Start Quest form (select quest type and assign idle characters)
- Show quest result notification when `QuestResolved` arrives via SignalR

**Acceptance Criteria:**

- [ ] Active quests load and display with correct remaining time
- [ ] Countdown timers decrement in real time
- [ ] Start Quest form only allows selection of `Idle` characters
- [ ] Submitting the form calls `POST /api/quests` and adds the quest to the active list
- [ ] Quest completion notification is displayed to the player via SignalR push

---

### Phase 25 — Blazor Frontend — Guild Management Panel

**Status:** ⬜

**Goal:** Implement the Guild Management panel covering inventory, market, gold balance, and world news display.

**Reference:** GDD §11 (Guild Management), GDD §12 (UI — Guild management panel), GDD §13 (World Events)

**Work Items:**

- Display inventory items with tier, rarity, stats, and available actions (equip, list, discard)
- Display active market listings with price and cancel action
- Display guild gold balance, updated in real time via SignalR
- Display world news feed fetched from `GET /api/world/news` (Phase 20) and updated in real time when `PopulationUpdated` or `CharacterDied` arrives via SignalR

**Acceptance Criteria:**

- [ ] Inventory displays all `InInventory` items with correct metadata
- [ ] List, discard, and equip actions call the correct APIs and update the UI
- [ ] Active market listings display with cancel option
- [ ] Gold balance updates in real time when `GoldCredited` or `GoldDebited` arrives
- [ ] World news feed loads on page open from `GET /api/world/news` and appends new entries when `PopulationUpdated` or `CharacterDied` arrives via SignalR

---

### Phase 26 — Observability & Structured Logging

**Status:** ⬜

**Goal:** Instrument all Azure Functions with structured logging, custom metrics, and distributed tracing via Application Insights.

**Reference:** SAD §10 (Observability), SAD §11 (Monitoring and Alerts), GDD §17 (Observability)

**Work Items:**

- Add structured `ILogger` log statements to all functions using the patterns in SAD §10
- Emit custom metrics for: quests started, quests completed, market sales, character deaths, population tier distribution
- Verify distributed traces correlate across functions using `correlationId` from the event envelope (ECS §1)
- Configure Azure Monitor alert rules for: function failure rate, Service Bus queue depth, Cosmos DB RU spikes

**Acceptance Criteria:**

- [ ] Application Insights shows structured logs for all key events
- [ ] Custom metrics are visible in Application Insights Metrics explorer
- [ ] An end-to-end trace for a quest workflow (`QuestStarted` → `ItemAddedToInventory`) is visible in Application Insights
- [ ] Azure Monitor alerts are configured and fire on synthetic threshold breaches in a test run
- [ ] No function lacks log coverage for its primary success and failure paths

---

### Phase 27 — Reliability Hardening

**Status:** ⬜

**Goal:** Verify and enforce all reliability patterns: idempotency, dead letter queues, retry policies, and optimistic concurrency.

**Reference:** SAD §7 (Reliability Architecture), ECS §11 (Idempotency and Concurrency), SAD §8 (Concurrency and Domain Rules)

**Work Items:**

- Audit all Service Bus consumers for idempotency using `eventId` check pattern (ECS §11)
- Verify dead letter queues are configured on all Service Bus topics/queues
- Verify retry policies are set appropriately on all consumers
- Write integration tests that replay duplicate events and assert no double-processing
- Write integration tests that simulate ETag conflicts and assert correct retry/reject behavior

**Acceptance Criteria:**

- [ ] Replaying any event twice produces no duplicate state changes
- [ ] All Service Bus topics/queues have dead letter queues configured
- [ ] Failed messages after max retries land in the dead letter queue
- [ ] ETag conflicts on Cosmos DB writes are detected and handled correctly
- [ ] All domain invariants (ECS §11 concurrency rules) are enforced and covered by tests

---

### Phase 28 — Configuration & Secrets Management

**Status:** ⬜

**Goal:** Migrate all runtime configuration to Azure App Configuration and all secrets to Azure Key Vault.

**Reference:** SAD §12 (Configuration Management), SAD §13 (Secrets Management)

**Work Items:**

- Move all runtime settings to Azure App Configuration (quest durations, death probability, market base prices, population update interval)
- Move all connection strings and API keys to Azure Key Vault
- Update all functions to retrieve configuration from App Configuration and secrets from Key Vault at runtime
- Verify no secrets exist in `local.settings.json` committed to the repository (`.gitignore` already excludes this file)

**Acceptance Criteria:**

- [ ] No secrets are hardcoded in source code or committed configuration files
- [ ] All functions read runtime settings from Azure App Configuration
- [ ] All functions retrieve connection strings from Azure Key Vault
- [ ] Changing a setting in App Configuration takes effect without redeployment
- [ ] `local.settings.json` is confirmed absent from the repository

---

### Phase 29 — API Management & Security Hardening

**Status:** ⬜

**Goal:** Front all HTTP-triggered Azure Functions with Azure API Management and enforce the full security model across API endpoints and inter-service communication.

**Reference:** SAD §15 (Security Model), SAD §3 (API Management — optional operational service)

**Note:** API Management is listed as optional in the SAD. This phase treats it as implemented. If the decision is made to skip APIM, update this phase to use Azure Static Web Apps built-in auth directly and remove the APIM-specific criteria below.

**Work Items:**

- Configure Azure API Management to proxy all HTTP-triggered Azure Function endpoints provisioned in Phase 2
- Apply authentication policies in APIM to require valid tokens on all player-facing endpoints
- Configure managed identities for all service-to-service communication (Functions → Cosmos DB, Functions → Service Bus)
- Enforce HTTPS for all endpoints (APIM gateway and Static Web Apps)
- Confirm no connection strings use shared access keys where managed identity is available

**Acceptance Criteria:**

- [ ] All HTTP API calls are routed through the APIM gateway
- [ ] Unauthenticated requests to all player-facing endpoints return HTTP 401
- [ ] APIM is configured with correct backend URLs pointing to Azure Functions
- [ ] All inter-service communication uses managed identity, not shared access key connection strings where supported
- [ ] All endpoints are HTTPS only — HTTP requests are rejected or redirected
- [ ] Security is verified by attempting unauthenticated and HTTP requests against each endpoint

---

### Phase 30 — End-to-End Integration Testing

**Status:** ⬜

**Goal:** Validate the complete game loop from player registration through market sale using automated end-to-end tests against the live Azure environment.

**Reference:** All workflows in SAD §6, GDD §2 (Core Gameplay Loop)

**Work Items:**

- Implement an automated end-to-end test suite covering the full gameplay loop:
  1. Register player → guild created with starting gold
  2. Recruit adventurer → gold deducted, character on roster
  3. Start quest → characters on quest, scheduled message queued
  4. Quest resolves → loot generated, item in inventory
  5. Equip item → character stats updated
  6. List item on market → listing created
  7. Item sold → gold credited, listing closed
- Verify all expected events appear on Service Bus in correct order
- Verify all Cosmos DB state transitions are correct at each step

**Acceptance Criteria:**

- [ ] Full gameplay loop test runs green against the deployed Azure environment
- [ ] All expected events are published in the correct sequence
- [ ] All Cosmos DB documents reflect correct final state after the loop completes
- [ ] No orphaned records or inconsistent states remain after test run
- [ ] Test run is executable from the CI/CD pipeline as a post-deploy smoke test

---

*Last updated: Phase 1 not yet started. All phases are ⬜ Not started.*