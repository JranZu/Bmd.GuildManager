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

### Game Constants

The following constants govern character stat generation and quest difficulty scaling throughout the system.

| Constant | Value | Description |
| -------- | ----- | ----------- |
| `MinStatValue` | 3 | Minimum value for any base stat (Strength, Luck, Endurance) at character creation |
| `MaxStatValue` | 10 | Maximum value for any base stat at character creation |
| `StatCount` | 3 | Number of stats (Strength, Luck, Endurance) |

These constants are defined in `GameConstants.cs` in `Bmd.GuildManager.Core` and are referenced by `QuestFactory.cs` for difficulty range calculation.

Characters have the following attributes:

| Attribute | Description |
| --------- | ----------- |
| Level | Numeric value starting at 1, maximum 20. Increases when accumulated XP reaches the threshold for the next level. |
| XP | Accumulated experience earned per character per quest resolution. XP is awarded regardless of outcome; more is awarded for harder quests and better outcomes. Exact amounts per tier and outcome are defined in GDD §6. |
| Strength | Combat effectiveness. Contributes to Team Power during quest resolution. |
| Luck | Affects loot quality and loot drop probability. |
| Endurance | Affects survival chance during dangerous quest outcomes. |

### Character Tier

A character's Tier is a **derived classification** based on the items they have equipped. It is not stored directly — it is calculated on demand from the character's `Equipment` list.

**Tier and Level are independent.** Level is a progression value (1–20) driven by accumulated XP. Tier reflects equipment power. A well-equipped Level 1 character can have a higher Tier than an unequipped Level 15 character. Neither implies the other.

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

The divisor is always **7** (total equipment slots), not the number of filled slots. A character with one Legendary item equipped and six empty slots is Novice-tier, not Legendary. Tier is rounded to the nearest integer and mapped back to the tier name.

A character with no items equipped has a tier of Novice (1).

> `Character.CalculateTier()` should be implemented as a method on the `Character` model in `Bmd.GuildManager.Core`.

### Equipment Slots

Each character has **7 equipment slots**:

| Slot | Description |
| ---- | ----------- |
| MainHand | Primary weapon or tool |
| Offhand | Shield, off-hand weapon, or secondary tool |
| Chest | Body armor |
| Head | Helmet or headwear |
| Feet | Boots or footwear |
| Ring | Ring or band |
| Accessory | Necklace, pendant, or trinket |

### XP Thresholds

| Level | XP Required to Reach Next Level |
| ----- | -------------------------------- |
| 1 → 2 | 100 |
| 2 → 3 | 250 |
| 3 → 4 | 500 |
| 4 → 5 | 1,000 |
| 5 → 6 | 2,000 |
| 6 → 7 | 4,000 |
| 7 → 8 | 8,000 |
| 8 → 9 | 16,000 |
| 9 → 10 | 32,000 |
| 10 → 11 | 64,000 |
| 11 → 12 | 128,000 |
| 12 → 13 | 256,000 |
| 13 → 14 | 512,000 |
| 14 → 15 | 1,024,000 |
| 15 → 16 | 2,048,000 |
| 16 → 17 | 4,096,000 |
| 17 → 18 | 8,192,000 |
| 18 → 19 | 16,384,000 |
| 19 → 20 | 32,768,000 |

Thresholds double each level starting from 100. Level 20 is the maximum level.

### Retirement (Future Design Item)

A Level 20 character who retires grants a permanent +1 bonus to their highest stat for all future characters created in that guild. The exact retirement mechanic — including how the bonus is stored, applied, and capped — is deferred to a future design session.

Additional character properties:

* Equipment — list of equipped Item objects, one per filled slot
* Xp — accumulated experience points
* Level — current numeric level derived from XP thresholds above
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
| Difficulty Rating | Numeric value representing the quest's power threshold. Used in quest resolution formula. Ranges are derived from `GameConstants` (see GDD §4) and are defined per tier below. |

### Difficulty Rating Ranges

`DifficultyRating` ranges are derived from `GameConstants` using the formula:

- Minimum for a tier = `MinStatValue × StatCount` at the lowest tier
- Each subsequent tier doubles the range

| Tier | DifficultyRating Range | Derivation |
| ---- | ---------------------- | ---------- |
| Novice | 9–60 | `MinStatValue × StatCount` to `MaxStatValue × StatCount × 2` |
| Apprentice | 60–120 | doubles Novice upper bound |
| Veteran | 120–240 | doubles Apprentice upper bound |
| Elite | 240–480 | doubles Veteran upper bound |
| Legendary | 480–960 | doubles Elite upper bound |

> `QuestFactory.cs` in `Bmd.GuildManager.Core` must use these exact ranges when generating quests. The ranges are updated as part of GM-009-01.

### Quest Generation and Availability

Quests are procedurally generated. They are not a fixed list. At any given time, the system maintains a pool of available quests in the Quests Cosmos DB container. At least 2 quests must be available per tier at all times. A timer-triggered function is responsible for ensuring this minimum is maintained by generating new quests when supply falls below the threshold.

Quests are a shared world resource. A quest in Available status can be claimed by any player. When a player starts a quest, its status transitions from Available to InProgress and it becomes associated with that player and their assigned characters. This transition must be protected by optimistic concurrency (ETag) to prevent two players from claiming the same quest simultaneously.

The quest lifecycle is:

```
Available → InProgress → [CriticalSuccess | Success | Failure | CatastrophicFailure] → Archived (Blob Storage) → Deleted from Cosmos
```

The Quests container is operational state only and should remain small (approximately 20–40 active documents at any time). Resolved quests are archived to Blob Storage and deleted from Cosmos. The archived document carries the terminal outcome status — `CriticalSuccess`, `Success`, `Failure`, or `CatastrophicFailure` — making the Blob record self-describing without requiring a separate resolution lookup.

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

| Outcome | Description |
| -------- | ----------- |
| CriticalSuccess | full loot reward at quest tier + chance of one item from the tier above + gold bonus; scaled by how much the team exceeded the quest difficulty |
| Success | full loot reward, gold awarded |
| Failure | no loot, no gold |
| CatastrophicFailure | no reward; high character death probability |

Even successful quests have a small chance of character death.

Quest outcomes are calculated using:

```
Character Base Power      = Strength + Luck + Endurance + (Level × 2)
Character Equipment Bonus = sum of all stat bonuses from equipped items
Character Total Power     = Base Power + Equipment Bonus

Team Power = sum of all assigned characters' Total Power
```

### Random Variance

Before comparing Team Power against the quest's Difficulty Rating, a ±25% jitter is applied:

```
Effective Team Power = Team Power × Random(0.75, 1.25)
```

The team power ratio used for outcome determination is:

```
teamPowerRatio = Effective Team Power / Difficulty Rating  (expressed as a percentage)
```

### Outcome Thresholds

These values are initial design decisions and are expected to be tuned. They should be stored in Azure App Configuration, not hardcoded.

| teamPowerRatio | Outcome |
| -------------- | ------- |
| ≥ 150% | CriticalSuccess |
| 100–149% | Success |
| 60–99% | Failure |
| < 60% | CatastrophicFailure |

### Character Death Probability

| Outcome | Death Probability per Character |
| ------- | ------------------------------- |
| CriticalSuccess | 1% |
| Success | 2% |
| Failure | 20% |
| CatastrophicFailure | 60% |

Death is evaluated independently per character against their Endurance stat.

### XP Awarded per Character

XP is awarded per character per quest resolution. CriticalSuccess XP is scaled by how much the team exceeded the difficulty.

| Quest Tier | CriticalSuccess | Success | Failure | CatastrophicFailure |
| ---------- | --------------- | ------- | ------- | ------------------- |
| Novice | scaled (see formula) | 25 | 10 | 5 |
| Apprentice | scaled (see formula) | 60 | 20 | 5 |
| Veteran | scaled (see formula) | 120 | 40 | 5 |
| Elite | scaled (see formula) | 250 | 80 | 5 |
| Legendary | scaled (see formula) | 500 | 150 | 5 |

> XP is a single value applied equally to all surviving characters on the quest. It is carried in `QuestResolved.xpAwarded` and applied by the character service consumer, not by the quest resolver. This keeps XP application logic centralized in the character domain and reusable for future non-quest XP sources.

**CriticalSuccess XP formula:**

```
overageMultiplier = teamPowerRatio / 1.0     (e.g. 123% ratio → 1.23; 234% ratio → capped at 2.0)
xpAwarded = baseXP × jitter × min(overageMultiplier, 2.0)
```

`baseXP` is the Success XP for the quest tier. `jitter` is a small random multiplier (e.g. ×0.9–1.1) applied for variety.

### Gold Awarded per Outcome

Gold is only awarded for CriticalSuccess and Success outcomes. Failure and CatastrophicFailure award no gold.

| Quest Tier | CriticalSuccess | Success |
| ---------- | --------------- | ------- |
| Novice | 30–60 | 15–30 |
| Apprentice | 80–140 | 40–70 |
| Veteran | 160–280 | 80–140 |
| Elite | 350–600 | 175–300 |
| Legendary | 700–1,200 | 350–600 |

**CriticalSuccess gold formula:**

```
goldAwarded = baseGold × min(overageMultiplier, 2.0)   (then jittered within the tier's range)
```

`baseGold` is the midpoint of the Success gold range for the quest tier.

### Quest Document Lifecycle

After `QuestResolved` is published, the quest document is:

1. Serialized to Blob Storage — container: `quest-archive`, path: `{year}/{month}/{questId}.json`
2. Deleted from Cosmos DB

This keeps the Quests container small and operational. The Blob Storage archive is the permanent record.

`ResolveQuestFunction` publishes `QuestResolved` and archives the quest document. Character status updates (Idle for survivors, XP application, level-up check) are handled by a downstream `HandleQuestResolvedFunction` consumer. Character death is handled by Phase 10.

### Character Death

Characters that die during a quest are **permanently removed** from active use.

Consequences of character death:

* the character is set to `Dead` status and retained in Cosmos DB (soft-delete); they cannot be assigned to quests or have equipment changed
* equipped items remain on the dead character document with `Equipped` status and cannot be retrieved — no item status update is required (there is no separate Items container)
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
| BasePrice | Base gold value used in the market pricing formula. Defined per tier and rarity. |
| Status | Stashed / Equipping / Equipped / Unequipping / Selling / ForSale / Returning / Sold / Discarded / Lost |

### Item Drop Tier Rules

Items that drop from a quest are at or below the quest's tier. Finding an item more than one tier above the quest's tier is possible but very rare. In normal play the player should expect to receive items at or below the tier of quest they are running.

Example: a Veteran-tier quest will typically produce Veteran or lower items. An Elite item from a Veteran quest is a rare bonus, not the norm. A Legendary item from a Veteran quest does not occur.

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