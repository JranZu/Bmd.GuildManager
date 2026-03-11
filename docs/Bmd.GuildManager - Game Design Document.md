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

Characters have the following attributes:

| Attribute | Description |
| --------- | ----------- |
| Level | Numeric value starting at 1. Increases as XP thresholds are reached. |
| XP | Accumulated experience earned from completing quests. Increases with every quest regardless of outcome, with more XP awarded for harder quests and better outcomes. |
| Strength | Combat effectiveness. Contributes to Team Power during quest resolution. |
| Luck | Affects loot quality and loot drop probability. |
| Endurance | Affects survival chance during dangerous quest outcomes. |

### Character Tier

A character's Tier is a derived classification representing their current overall power level. It is not stored directly — it is calculated from the items they have equipped.

The tier scale used across the entire game (characters, items, quests) is:

| Tier | Numeric Value |
| ---- | ------------- |
| Novice | 1 |
| Apprentice | 2 |
| Veteran | 3 |
| Elite | 4 |
| Legendary | 5 |

Character Tier is calculated as:

```
Character Tier = sum of (tier numeric value of each equipped item) ÷ total equipment slots
```

The divisor is always total equipment slots, not the number of filled slots. A character with one Legendary item equipped and four empty slots is Novice-tier, not Legendary. Tier is rounded to the nearest integer and mapped back to the tier name.

A character with no items equipped has a tier of Novice (1).

### Equipment Slots

Each character has a fixed number of equipment slots. The exact number of slots and their named types (e.g. weapon, armor, ring) are to be defined in the Pre-Phase Design session for Phase 13.

Additional character properties:

* EquipmentIds — list of item IDs currently equipped, one per filled slot
* Xp — accumulated experience points
* Level — current numeric level derived from XP thresholds (exact thresholds defined in Phase 15 pre-phase design)
* Current status: Idle / OnQuest / Dead

Characters that die are permanently removed from the guild.

A character that is currently on a quest cannot be assigned to another quest or have equipment changes until the quest resolves.

---

# 5. Quests

Quests are time based activities that produce loot.

Each quest has:

| Property | Description |
| -------- | ----------- |
| Name | Procedurally generated from word-part combinations |
| Tier | Novice / Apprentice / Veteran / Elite / Legendary — aligns with the global tier scale |
| Duration | 1–30 minutes, scaled to tier |
| Required Adventurers | 1–5, scaled to tier |
| Risk Level | Low / Medium / High |
| Difficulty Rating | Numeric value representing the quest's power threshold. Used in quest resolution formula. Definition and ranges to be finalized in Phase 8 pre-phase design. |

### Quest Generation and Availability

Quests are procedurally generated. They are not a fixed list. At any given time, the system maintains a pool of available quests in the Quests Cosmos DB container. At least 2 quests must be available per tier at all times. A timer-triggered function is responsible for ensuring this minimum is maintained by generating new quests when supply falls below the threshold.

Quests are a shared world resource. A quest in Available status can be claimed by any player. When a player starts a quest, its status transitions from Available to InProgress and it becomes associated with that player and their assigned characters. This transition must be protected by optimistic concurrency (ETag) to prevent two players from claiming the same quest simultaneously.

The quest lifecycle is:

```
Available → InProgress → Completed → Archived (Blob Storage) → Deleted from Cosmos
```

The Quests container is operational state only and should remain small (approximately 20–40 active documents at any time). Completed quests are archived to Blob Storage and deleted from Cosmos.

Quest documents include an ActiveQuestSnapshot that is embedded on each assigned Character document at quest start. This snapshot contains enough information (questId, name, tier, estimatedCompletionAt) to render the character's current quest without a second lookup. It is cleared when the quest resolves.

Example quests:

Goblin Cave
Novice tier
1 adventurer
2 minutes

Bandit Stronghold
Veteran tier
2 adventurers
5 minutes

Dragon Lair
Legendary tier
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
Character Base Power      = Strength + Luck + Endurance + (Level × 2)
Character Equipment Bonus = sum of all stat bonuses from equipped items
Character Total Power     = Base Power + Equipment Bonus

Team Power = sum of all assigned characters' Total Power
```

Outcome Thresholds — These are initial values and are expected to be tuned. They should be stored in Azure App Configuration (Phase 28), not hardcoded.

| Team Power vs Quest Difficulty Rating | Outcome |
| ------------------------------------- | ------- |
| ≥ 150% | Success |
| 100–149% | Partial Success |
| 60–99% | Failure |
| < 60% | Catastrophic Failure |

The exact numeric ranges for difficultyRating per quest tier, and the exact death probability values per outcome type, are to be finalized in the Pre-Phase Design session for Phase 9.

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

| Property | Description |
| -------- | ----------- |
| Name | Procedurally generated from word-part combinations |
| Tier | Novice / Apprentice / Veteran / Elite / Legendary |
| Rarity | Common / Rare / Legendary |
| StrengthBonus | Integer bonus to character Strength when equipped. 0 if not a strength item. |
| LuckBonus | Integer bonus to character Luck when equipped. 0 if not a luck item. |
| EnduranceBonus | Integer bonus to character Endurance when equipped. 0 if not an endurance item. |
| BasePrice | Base gold value used in the market pricing formula. Defined per tier and rarity. Exact values to be defined in Phase 11 pre-phase design. |
| Status | InInventory / Equipped / ListedForSale / Sold / Discarded / Lost |
| OwnerId | PlayerId of the current owner |
| CharacterId | CharacterId of the character currently equipped to (null if not equipped) |

### Item Drop Tier Rules

Items that drop from a quest are at or below the quest's tier. Finding an item more than one tier above the quest's tier is possible but very rare. In normal play the player should expect to receive items at or below the tier of quest they are running.

Example: a Veteran-tier quest will typically produce Veteran or lower items. An Elite item from a Veteran quest is a rare bonus, not the norm. A Legendary item from a Veteran quest does not occur.

The exact stat bonus ranges per tier and rarity are to be defined in the Pre-Phase Design session for Phase 11.

Items can be:

* equipped to a character
* unequipped from a character
* sold on the market
* discarded permanently

A character's equipped items contribute to their effective stats during quests.

An item that is equipped cannot be listed for sale or discarded until it is unequipped.

Procedural item naming is used to increase variety.

---

# 9. Simulated Population

The world population consists of aggregated tiers rather than individual NPCs.

Example population state:

| Tier | Population |
| ---- | ---------- |
| Novice Adventurers | 1000 |
| Apprentice Adventurers | 500 |
| Veteran Adventurers | 200 |
| Elite Adventurers | 50 |
| Legendary Heroes | 10 |

Population changes over time due to:

* deaths
* level progression
* new recruits entering the world

Population changes influence market demand.

---

# 13. World Events

The game publishes human-readable world news messages in response to key domain events. These messages are persisted to the WorldNews container and surfaced to players via the world news feed.

## Message Templates

### CharacterDied

Messages are generated using the character's tier at the time of death:

| Tier | Message |
| ---- | ------- |
| Novice | "A novice adventurer has fallen in battle." |
| Apprentice | "An apprentice adventurer has fallen in battle." |
| Veteran | "A veteran adventurer has fallen in battle." |
| Elite | "An elite adventurer has fallen in battle." |
| Legendary | "A legendary hero has fallen in battle." |

### PopulationUpdated

Messages are generated based on which tier changed and the direction of change:

| Tier | Growth | Decline |
| ---- | ------ | ------- |
| Novice | "A surge of new adventurers enters the world. Novice population increased." | "The novice adventurer population has declined." |
| Apprentice | "More apprentice adventurers are making their mark. Apprentice population increased." | "The apprentice adventurer population has declined." |
| Veteran | "A new wave of veteran adventurers arrives. Veteran population increased." | "The veteran adventurer population has declined." |
| Elite | "Elite adventurers grow in number. Elite population increased." | "The elite adventurer population has declined." |
| Legendary | "A new legendary hero rises. Legendary population increased." | "The legendary hero population has declined." |

---

# 21. Design Process

Each phase in the Project Roadmap has a Pre-Phase Design section listing questions that must be answered in a design conversation before story writing begins. Those answers are written into the relevant GDD sections before any code is written. No phase story is considered ready to implement until its Pre-Phase Design items are resolved and the GDD reflects the agreed decisions.

---