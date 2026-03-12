# Guild Manager

## Event Contract Specification

All events are transported via **Azure Service Bus** using JSON payloads.

Each event includes a shared metadata envelope to support observability, tracing, and debugging.

---

# 1. Event Envelope

All events include the following base structure.

```json
{
  "eventId": "guid",
  "eventType": "string",
  "timestamp": "utc datetime",
  "correlationId": "guid",
  "source": "service name",
  "version": 1,
  "data": { }
}
```

### Fields

| Field         | Description                                 |
| ------------- | ------------------------------------------- |
| eventId       | unique identifier for the event             |
| eventType     | name of the event                           |
| timestamp     | event creation time                         |
| correlationId | ties events from the same workflow together |
| source        | producing service                           |
| version       | schema version                              |
| data          | event payload                               |

The **correlationId** is critical for distributed tracing.

Example:

```
StartQuest → QuestCompleted → LootGenerated
```

All share the same correlationId.

Detailed example:

```
QuestStarted       correlationId = abc-123
QuestCompleted      correlationId = abc-123
QuestResolved       correlationId = abc-123
LootGenerated       correlationId = abc-123
ItemAddedToInventory correlationId = abc-123
```

This allows end-to-end tracing of a single quest workflow across all downstream events.

---

# 2. Player and Guild Lifecycle Events

## PlayerCreated

Published when a new player registers.

```json
{
  "eventType": "PlayerCreated",
  "data": {
    "playerId": "guid",
    "guildName": "string"
  }
}
```

Consumers:

* onboarding service
* analytics

---

## GuildCreated

Published after the player's guild is initialized.

```json
{
  "eventType": "GuildCreated",
  "data": {
    "playerId": "guid",
    "guildName": "string",
    "startingGold": 500
  }
}
```

Consumers:

* onboarding service
* analytics

---

## StarterCharactersGranted

Published when starter characters are added to a new guild. Optional event.

```json
{
  "eventType": "StarterCharactersGranted",
  "data": {
    "playerId": "guid",
    "characterIds": [
      "characterId1",
      "characterId2"
    ]
  }
}
```

Consumers:

* character service
* analytics

---

## StarterItemsGranted

Published when starter items are added to a new guild. Optional event.

```json
{
  "eventType": "StarterItemsGranted",
  "data": {
    "playerId": "guid",
    "itemIds": [
      "itemId1",
      "itemId2"
    ]
  }
}
```

Consumers:

* inventory service
* analytics

---

# 3. Character Events

## CharacterCreated

Published when a new character is recruited.

```json
{
  "eventType": "CharacterCreated",
  "data": {
    "playerId": "guid",
    "characterId": "guid",
    "name": "string",
    "level": 1,
    "strength": 10,
    "luck": 5,
    "endurance": 8
  }
}
```

Consumers:

* Character service
* analytics

---

## CharacterDied

Published when a character dies during a quest.

```json
{
  "eventType": "CharacterDied",
  "data": {
    "characterId": "guid",
    "playerId": "guid",
    "questId": "guid"
  }
}
```

Consumers:

* inventory cleanup
* notification service
* analytics
* world news system

---

# 4. Quest Events

## QuestStarted

Published when a player sends characters on a quest.

```json
{
  "eventType": "QuestStarted",
  "data": {
    "questId": "guid",
    "playerId": "guid",
    "questType": "GoblinCave",
    "characters": [
      "characterId1",
      "characterId2"
    ],
    "durationSeconds": 120
  }
}
```

Consumers:

* quest scheduler
* analytics

---

## QuestCompleted

Triggered by a scheduled Service Bus message.

```json
{
  "eventType": "QuestCompleted",
  "data": {
    "questId": "guid",
    "playerId": "guid",
    "success": true
  }
}
```

Consumers:

* quest resolution service

---

## QuestResolved

Published after the quest outcome has been fully evaluated. This is the unified resolution event that captures all possible outcomes.

Outcome values: `CriticalSuccess`, `Success`, `Failure`, `CatastrophicFailure`.

> Note: `PartialSuccess` has been removed. The four-outcome model is: CriticalSuccess ≥150%, Success 100–149%, Failure 60–99%, CatastrophicFailure <60%.

```json
{
  "eventType": "QuestResolved",
  "data": {
    "questId": "guid",
    "playerId": "guid",
    "questTier": "string",
    "outcome": "CriticalSuccess",
    "characters": [
      {
        "characterId": "guid",
        "survived": true,
        "xpAwarded": 75
      }
    ],
    "lootEligible": true,
    "goldAwarded": 0
  }
}
```

| Field | Description |
| ----- | ----------- |
| `questTier` | The tier of the resolved quest: `Novice`, `Apprentice`, `Veteran`, `Elite`, or `Legendary` |
| `outcome` | One of: `CriticalSuccess`, `Success`, `Failure`, `CatastrophicFailure` |
| `characters[].xpAwarded` | XP awarded to this character for this quest resolution (int) |
| `lootEligible` | `true` if this quest outcome qualifies for loot generation; the loot generator consumes this flag and creates the item |
| `goldAwarded` | Bonus gold awarded directly by the quest outcome. Distinct from gold earned through market sales. Zero for Failure and CatastrophicFailure. |

> `goldAwarded` represents a bonus gold award granted directly by the quest outcome. It is distinct from gold earned through market sales, which flows through the economy system via `ItemSold` → `GoldCredited`.

Consumers:

* loot generator
* character service
* gold service
* notification service
* analytics

---

# 5. Loot Events

## LootGenerated

Published when loot is created.

```json
{
  "eventType": "LootGenerated",
  "data": {
    "itemId": "guid",
    "playerId": "guid",
    "questId": "guid",
    "itemTier": "Veteran",
    "rarity": "Rare"
  }
}
```

Consumers:

* inventory service
* analytics

---

## ItemAddedToInventory

```json
{
  "eventType": "ItemAddedToInventory",
  "data": {
    "itemId": "guid",
    "playerId": "guid"
  }
}
```

---

## ItemEquipped

Published when a player equips an item to a character.

```json
{
  "eventType": "ItemEquipped",
  "data": {
    "itemId": "guid",
    "characterId": "guid",
    "playerId": "guid"
  }
}
```

Consumers:

* character service
* analytics

---

## ItemUnequipped

Published when a player removes an item from a character.

```json
{
  "eventType": "ItemUnequipped",
  "data": {
    "itemId": "guid",
    "characterId": "guid",
    "playerId": "guid"
  }
}
```

Consumers:

* character service
* inventory service

---

## ItemDiscarded

Published when a player permanently discards an item.

```json
{
  "eventType": "ItemDiscarded",
  "data": {
    "itemId": "guid",
    "playerId": "guid"
  }
}
```

Consumers:

* inventory service
* analytics

---

# 6. Market Events

## ItemListed

Published when an item is placed on the market.

```json
{
  "eventType": "ItemListed",
  "data": {
    "listingId": "guid",
    "itemId": "guid",
    "playerId": "guid",
    "itemTier": "Beginner",
    "basePrice": 100
  }
}
```

Consumers:

* pricing service
* market service

---

## ItemSold

```json
{
  "eventType": "ItemSold",
  "data": {
    "listingId": "guid",
    "buyerTier": "Beginner",
    "finalPrice": 150
  }
}
```

Consumers:

* gold service
* notification service
* analytics

---

## ItemListingCanceled

Published when a player cancels an active market listing. Optional event.

```json
{
  "eventType": "ItemListingCanceled",
  "data": {
    "listingId": "guid",
    "itemId": "guid",
    "playerId": "guid"
  }
}
```

Consumers:

* inventory service
* market service

---

# 7. Economy Events

## GoldCredited

Published when gold is added to a guild balance.

```json
{
  "eventType": "GoldCredited",
  "data": {
    "playerId": "guid",
    "amount": 150,
    "reason": "ItemSold",
    "referenceId": "guid"
  }
}
```

Consumers:

* guild finance service
* analytics

---

## GoldDebited

Published when gold is deducted from a guild balance.

```json
{
  "eventType": "GoldDebited",
  "data": {
    "playerId": "guid",
    "amount": 200,
    "reason": "Recruitment",
    "referenceId": "guid"
  }
}
```

Consumers:

* guild finance service
* analytics

---

## AdventurerRecruited

Published when a player spends gold to recruit a new adventurer.

```json
{
  "eventType": "AdventurerRecruited",
  "data": {
    "playerId": "guid",
    "characterId": "guid",
    "recruitmentCost": 200
  }
}
```

Consumers:

* character service
* gold service
* analytics

---

# 8. Population Events

## PopulationUpdateScheduled

Emitted when a population update is queued.

```json
{
  "eventType": "PopulationUpdateScheduled",
  "data": {
    "scheduledTime": "utc datetime"
  }
}
```

---

## PopulationUpdated

Published after population adjustment.

```json
{
  "eventType": "PopulationUpdated",
  "data": {
    "beginner": 1020,
    "veteran": 198,
    "elite": 49,
    "epic": 9
  }
}
```

Consumers:

* market demand service
* notification service
* analytics
* world news generator

---

# 9. System Events

## PlayerEventOccurred

Generic event used to trigger population update scheduling.

```json
{
  "eventType": "PlayerEventOccurred",
  "data": {
    "playerId": "guid",
    "eventName": "QuestCompleted"
  }
}
```

---

# 10. Versioning Strategy

Event contracts should never change destructively.

Rules:

1. Add new fields rather than modifying existing ones.
2. Use the version field for major changes.
3. Maintain backward compatibility.

Example:

```
LootGenerated v1
LootGenerated v2
```

---

# 11. Idempotency and Concurrency

Consumers must treat events as **at least once delivery**.

Handlers must safely ignore duplicates.

Example pattern:

```
if eventId already processed
    ignore event
```

This prevents duplicate inventory updates or market sales.

## Concurrency Rules

Consumers must enforce domain invariants to prevent conflicting state transitions.

Character rules:

* A character cannot join multiple quests simultaneously.
* A dead character cannot be assigned to a quest.

Item rules:

* An item cannot be equipped and listed for sale at the same time.
* An equipped item must be unequipped before it can be discarded or sold.

Market rules:

* A listing can only be sold once.
* A canceled listing cannot be sold.

Gold rules:

* Gold spending must validate the current guild balance before deducting.
* Insufficient balance must reject the transaction.

Concurrency handling should use **Cosmos DB optimistic concurrency** with ETags. Each write operation must include the current ETag to detect conflicting updates.

Example pattern:

```
read document with ETag
apply business logic
write document with If-Match: ETag
if conflict → retry or reject
```

## Item State Machine

Items follow a defined set of states throughout their lifecycle.

```
InInventory → Equipped
InInventory → ListedForSale
InInventory → Discarded
Equipped → InInventory (via unequip)
ListedForSale → InInventory (via cancel listing)
ListedForSale → Sold
Equipped → Lost (via character death)
```

Valid states:

| State         | Description                              |
| ------------- | ---------------------------------------- |
| InInventory   | item is in the player's inventory        |
| Equipped      | item is equipped to a character          |
| ListedForSale | item is listed on the market             |
| Sold          | item has been sold (terminal state)      |
| Discarded     | item has been permanently removed        |
| Lost          | item was lost due to character death     |

State transition rules:

* An item in `Equipped` state cannot be listed, sold, or discarded.
* An item in `ListedForSale` state cannot be equipped or discarded.
* `Sold`, `Discarded`, and `Lost` are terminal states.

---

# 12. Event Naming Conventions

Events use **past tense verbs**.

Examples:

```
PlayerCreated
GuildCreated
QuestStarted
QuestResolved
LootGenerated
ItemEquipped
ItemDiscarded
ItemListed
ItemSold
GoldCredited
GoldDebited
AdventurerRecruited
PopulationUpdateScheduled
PopulationUpdated
```

This communicates that the event describes something that already happened.

> **Payload Type Naming**
> Payload types in `Bmd.GuildManager.Core` must be named exactly as their `eventType` string. The `EventEnvelope<T>` factory derives `eventType` from `typeof(T).Name` — a mismatch between the type name and the ECS string will produce an incorrect value on the wire.

---
