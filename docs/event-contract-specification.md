# Event Contract Specification

**Project:** Bmd.GuildManager  
**Version:** 0.1.0 — Initial Draft  
**Status:** In Progress

---

## Overview

This document defines the event-driven contracts used between the frontend, backend Azure Functions, and internal services. All events are JSON-serialised payloads transmitted over HTTP (Azure Functions HTTP Triggers) or via Azure Service Bus for async workflows.

---

## Conventions

- All timestamps are **ISO 8601 UTC** (e.g. `2026-01-01T00:00:00Z`).
- All IDs are **GUID strings** (UUID v4).
- HTTP responses follow standard status codes; error bodies include `{ "error": "<message>" }`.
- Event envelope wraps every async message.

### Async Event Envelope

```json
{
  "eventId": "string (GUID)",
  "eventType": "string",
  "timestamp": "string (ISO 8601)",
  "version": "string (semver)",
  "payload": { }
}
```

---

## 1. Guild Events

### 1.1 `guild.created`

Fired when a new guild is created for a player account.

```json
{
  "eventType": "guild.created",
  "payload": {
    "guildId": "string (GUID)",
    "ownerId": "string (GUID)",
    "name": "string",
    "rank": "Bronze",
    "createdAt": "string (ISO 8601)"
  }
}
```

### 1.2 `guild.rankUpdated`

Fired when a guild advances to a new rank.

```json
{
  "eventType": "guild.rankUpdated",
  "payload": {
    "guildId": "string (GUID)",
    "previousRank": "string",
    "newRank": "string",
    "updatedAt": "string (ISO 8601)"
  }
}
```

---

## 2. Character Events

### 2.1 `character.recruited`

Fired when a character is added to the guild roster.

```json
{
  "eventType": "character.recruited",
  "payload": {
    "characterId": "string (GUID)",
    "guildId": "string (GUID)",
    "name": "string",
    "class": "Warrior | Rogue | Mage | Cleric | Ranger",
    "stats": {
      "strength": "integer",
      "agility": "integer",
      "intelligence": "integer",
      "endurance": "integer",
      "luck": "integer"
    },
    "recruitedAt": "string (ISO 8601)"
  }
}
```

### 2.2 `character.equipmentChanged`

Fired when a character's loadout is modified.

```json
{
  "eventType": "character.equipmentChanged",
  "payload": {
    "characterId": "string (GUID)",
    "guildId": "string (GUID)",
    "slot": "Weapon | Armour | Accessory | Consumable",
    "previousItemId": "string (GUID) | null",
    "newItemId": "string (GUID) | null",
    "changedAt": "string (ISO 8601)"
  }
}
```

---

## 3. Quest Events

### 3.1 `quest.started`

Fired when a party is dispatched on a quest.

```json
{
  "eventType": "quest.started",
  "payload": {
    "questInstanceId": "string (GUID)",
    "questTemplateId": "string (GUID)",
    "guildId": "string (GUID)",
    "partyMemberIds": ["string (GUID)"],
    "startedAt": "string (ISO 8601)",
    "expectedCompletionAt": "string (ISO 8601)"
  }
}
```

### 3.2 `quest.completed`

Fired when a quest timer expires and the result is resolved.

```json
{
  "eventType": "quest.completed",
  "payload": {
    "questInstanceId": "string (GUID)",
    "guildId": "string (GUID)",
    "success": "boolean",
    "loot": [
      {
        "itemId": "string (GUID)",
        "name": "string",
        "rarity": "Common | Uncommon | Rare | Legendary",
        "quantity": "integer"
      }
    ],
    "goldAwarded": "integer",
    "reputationAwarded": "integer",
    "completedAt": "string (ISO 8601)"
  }
}
```

### 3.3 `quest.failed`

Fired when a quest ends in failure.

```json
{
  "eventType": "quest.failed",
  "payload": {
    "questInstanceId": "string (GUID)",
    "guildId": "string (GUID)",
    "reason": "string",
    "injuredCharacterIds": ["string (GUID)"],
    "failedAt": "string (ISO 8601)"
  }
}
```

---

## 4. Market Events

### 4.1 `market.itemListed`

Fired when a player lists an item on the market board.

```json
{
  "eventType": "market.itemListed",
  "payload": {
    "listingId": "string (GUID)",
    "guildId": "string (GUID)",
    "itemId": "string (GUID)",
    "quantity": "integer",
    "askingPrice": "integer",
    "listedAt": "string (ISO 8601)"
  }
}
```

### 4.2 `market.priceTick`

Fired by the scheduled market simulation function.

```json
{
  "eventType": "market.priceTick",
  "payload": {
    "tickId": "string (GUID)",
    "itemPrices": [
      {
        "itemTemplateId": "string (GUID)",
        "currentPrice": "integer",
        "delta": "integer"
      }
    ],
    "tickAt": "string (ISO 8601)"
  }
}
```

---

*This document is a living artifact and will be updated as new endpoints and events are introduced.*
