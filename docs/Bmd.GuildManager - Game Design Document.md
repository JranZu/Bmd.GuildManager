# Game Design Document

## Project: Guild Manager (Working Title)

Genre: Guild management / asynchronous RPG
Platform: Web browser
Technology Stack: C# / .NET 10 / Azure serverless architecture

---

# 1. High Level Concept

Guild Manager is a web based management game where the player runs an adventurer guild. Characters are recruited, equipped, and sent on quests that take time to complete. Successful quests produce loot that can be equipped or sold into a simulated market influenced by the needs of a living NPC population.

The player continuously balances risk, profit, and roster management while responding to changes in the simulated world economy.

The system is intentionally built as an **event driven distributed application** to serve as a practical architecture learning project.

---

# 2. Core Gameplay Loop

Player actions occur in short cycles.

Basic loop:

```
Start quests
→ wait for completion timers
→ collect rewards
→ manage inventory
→ sell items on the market
→ recruit new adventurers
→ repeat
```

The design avoids idle downtime by ensuring multiple quests can run simultaneously and resolve frequently.

---

# 3. Player Role

The player is the **Guild Master**.

Responsibilities include:

* recruiting adventurers
* equipping characters
* assembling quest teams
* managing guild finances
* deciding which items to sell or keep
* responding to market demand

The player does not directly control characters during quests.

---

# 4. Characters

Each player can maintain multiple adventurers.

Characters have simple attributes:

| Attribute | Description             |
| --------- | ----------------------- |
| Level     | overall experience      |
| Strength  | combat effectiveness    |
| Luck      | affects loot quality    |
| Endurance | affects survival chance |

Additional character properties:

* Equipment
* Current status (Idle / On Quest / Dead)

Characters that die are permanently removed from the guild.

A character that is currently on a quest cannot be assigned to another quest or have equipment changes until the quest resolves.

---

# 5. Quests

Quests are time based activities that produce loot.

Each quest has:

| Property             | Example             |
| -------------------- | ------------------- |
| Difficulty           | Easy / Hard / Epic  |
| Duration             | 1–30 minutes        |
| Required Adventurers | 1–5                 |
| Risk Level           | Low / Medium / High |

Example quests:

Beginner Quest
Goblin Cave
1 adventurer
2 minutes

Veteran Quest
Bandit Stronghold
2 adventurers
5 minutes

Epic Quest
Dragon Lair
5 adventurers
20 minutes

---

# 6. Quest Resolution

When a quest completes, the system determines the outcome.

Possible results:

| Outcome              | Description                          |
| -------------------- | ------------------------------------ |
| Success              | full loot reward, gold may be awarded |
| Partial Success      | reduced loot, no gold bonus          |
| Failure              | no reward                            |
| Catastrophic Failure | no reward, characters may die        |

Even successful quests have a small chance of character death.

Quest outcomes are calculated using:

```
Team Power = sum(character stats + equipment)

vs

Quest Difficulty
```

The system publishes a unified **QuestResolved** event that captures the outcome type, character survival status, and resulting effects such as loot generation or gold awards.

### Character Death

Characters that die during a quest are **permanently removed** from the guild.

Consequences of character death:

* the character is marked as Dead and cannot be used again
* equipped items on a dead character are lost
* the player is notified in real time via SignalR
* the death is recorded for analytics and world news

---

# 7. Loot System

Loot is procedurally generated.

Each item contains:

| Property | Example                   |
| -------- | ------------------------- |
| Name     | Sword of Mild Regret      |
| Tier     | Beginner / Veteran / Epic |
| Stats    | Strength +4               |
| Rarity   | Common / Rare / Legendary |

Items can be:

* equipped to a character
* unequipped from a character
* sold on the market
* discarded permanently

A character's equipped items contribute to their effective stats during quests.

An item that is equipped cannot be listed for sale or discarded until it is unequipped.

Procedural item naming is used to increase variety.

---

# 8. Market System

The market allows players to sell items.

Demand is influenced by a simulated NPC population.

Item prices are determined by:

```
base price
× population demand
÷ current market supply
```

Example scenario:

Population

Beginner adventurers: 1000
Epic heroes: 10

Result

Beginner gear sells quickly and for higher prices.
Epic gear sells slowly due to limited buyers.

This encourages diverse quest strategies.

### Market Lifecycle

The full lifecycle of a market listing:

```
Player lists item
→ ItemListed event published
→ pricing service calculates demand
→ listing stored in database
→ potential sale scheduled
→ ItemSold event published when purchased
→ gold credited to seller
```

Players may also cancel an active listing before it is sold. Canceling a listing returns the item to the player's inventory.

Gold earned from market sales is credited to the guild balance through the economy system.

---

# 9. Simulated Population

The world population consists of aggregated tiers rather than individual NPCs.

Example population state:

| Tier                 | Population |
| -------------------- | ---------- |
| Beginner Adventurers | 1000       |
| Veteran Adventurers  | 200        |
| Elite Adventurers    | 50         |
| Epic Heroes          | 10         |

Population changes over time due to:

* deaths
* level progression
* new recruits entering the world

Population changes influence market demand.

---

# 10. Population Updates

Population updates occur periodically but only when players are active.

Trigger model:

```
Player event occurs
→ check if population update scheduled
→ if not scheduled, queue update in 5 minutes
```

Population update adjusts tier counts with random probabilities.

Example changes:

* adventurer deaths
* tier promotions
* new beginners entering the population

Minimum tier populations prevent collapse of the world economy.

---

# 11. Guild Management

Players manage several guild functions between quest completions.

Management tasks include:

Inventory management
Equipment optimization
Market selling
Recruiting adventurers

## Recruitment

Recruitment provides randomly generated characters with varying stats.

Recruiting an adventurer costs gold. The cost scales with the quality of the recruit.

The recruitment flow:

```
Player requests recruitment
→ guild gold balance validated
→ gold deducted
→ character generated
→ character added to guild roster
```

If the guild does not have enough gold the recruitment is rejected.

## Item Management

Players can perform the following item actions:

* Equip an item to an idle character
* Unequip an item from a character
* Discard an item permanently
* List an item for sale on the market
* Cancel an active market listing

Items that are currently equipped cannot be sold or discarded until they are unequipped.

---

# 12. User Interface

The interface is designed as a simple web application with three main panels.

Characters panel

Displays character status and quest assignments.

Active quests panel

Displays quests currently running and their remaining time.

Guild management panel

Inventory
Market
Recruitment

The interface emphasizes clarity and fast interaction.

---

# 13. World Events

The system can produce occasional global notifications.

Examples:

```
Realm News

An elite adventurer has fallen in battle.
Elite population reduced.

A surge of new adventurers enters the world.
Beginner population increased.
```

These messages reflect population changes and reinforce world immersion.

---

# 14. Technical Design Goals

The project intentionally demonstrates cloud architecture concepts.

Key goals:

* event driven system design
* asynchronous workflows
* serverless compute
* distributed state management
* observability and monitoring
* fault tolerant messaging

---

# 15. Azure Architecture Overview

Core services used in the project.

Frontend

Azure Static Web Apps

Backend

Azure Functions

Messaging

Azure Service Bus

Database

Azure Cosmos DB

Realtime updates

Azure SignalR

Storage

Azure Blob Storage

Operational services

Application Insights
Azure Monitor
Azure Key Vault
Azure App Configuration
API Management

---

# 16. Event Driven System

All gameplay mechanics operate through events.

Example event flow:

```
QuestStarted
→ QuestCompleted
→ QuestResolved
→ LootGenerated
→ ItemAddedToInventory
→ ItemListed
→ ItemSold
→ GoldCredited
```

Events are transported through Azure Service Bus.

Scheduled messages handle quest completion timing.

---

# 17. Observability

Application Insights provides monitoring.

Three categories of telemetry are tracked.

Logs

Quest events
market activity
character deaths

Metrics

quests completed
items sold
population distribution

Traces

end to end tracking of requests across functions

---

# 18. Reliability

The system incorporates several reliability patterns.

Retry policies for failed message processing
Dead letter queues for failed events
Idempotent event handlers to prevent duplicates
Alerting for abnormal error rates

---

# 19. Development Goals

The project is designed to serve as:

* a practical distributed systems exercise
* a refresher on Azure architecture
* a demonstration project for interviews

The architecture intentionally mirrors common enterprise patterns.

---

# 20. Future Expansion Possibilities

Potential additions if the project grows.

Crafting systems
Guild upgrades
Seasonal world events
Leaderboards
Player guild competition
Rare world bosses

These are not required for the initial release.

---