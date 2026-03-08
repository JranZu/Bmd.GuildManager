# Game Design Document (GDD)

**Project:** Bmd.GuildManager  
**Version:** 0.1.0 — Initial Draft  
**Status:** In Progress

---

## 1. Game Overview

**Genre:** Web-based idle / management simulation  
**Platform:** Browser (Azure Static Web Apps)  
**Target Audience:** Casual to mid-core strategy players

Guild Manager is a web-based management game where the player operates an adventurer guild. The player recruits characters, equips them with gear, and dispatches them on time-based quests. Successful quests yield loot that can be equipped or sold on a simulated market influenced by a living NPC population.

---

## 2. Core Game Loop

```
Recruit Character → Equip Gear → Send on Quest → Await Completion → Collect Loot → Sell / Equip
                                                         ↑                                  |
                                                         └──────────── Market Influence ────┘
```

1. **Recruit** — Hire adventurers with unique stats and classes from a rotating roster.
2. **Equip** — Assign weapons, armour, and consumables from guild inventory.
3. **Quest** — Select a quest from available contracts; assign party members; begin timer.
4. **Await** — Quests run in real-time (or accelerated server-time) without active input.
5. **Collect** — On completion, loot is added to guild inventory and XP awarded.
6. **Market** — Sell surplus loot; prices fluctuate based on NPC supply and demand.

---

## 3. Player Progression

| Milestone           | Unlock                                       |
|---------------------|----------------------------------------------|
| Guild Rank 1 (Bronze) | Basic quests, 3 roster slots               |
| Guild Rank 2 (Silver) | Advanced quests, 6 roster slots, crafting  |
| Guild Rank 3 (Gold)   | Elite quests, 10 roster slots, NPC alliances|
| Guild Rank 4 (Platinum)| Legendary quests, unlimited slots, market board |

---

## 4. Characters

### 4.1 Attributes

- **Strength** — Melee damage and carrying capacity
- **Agility** — Speed, dodge chance, ranged accuracy
- **Intelligence** — Magic power, skill cooldown reduction
- **Endurance** — Hit points and stamina recovery
- **Luck** — Loot quality multiplier, critical chance

### 4.2 Classes

| Class    | Primary Stat | Role          |
|----------|-------------|---------------|
| Warrior  | Strength    | Tank / DPS    |
| Rogue    | Agility     | DPS / Scout   |
| Mage     | Intelligence| AOE / Utility |
| Cleric   | Endurance   | Healer        |
| Ranger   | Agility     | Ranged DPS    |

---

## 5. Quests

### 5.1 Quest Properties

- **Difficulty Rating** (1–10)
- **Duration** (minutes to hours)
- **Recommended Party Composition**
- **Loot Table** (tiered: Common, Uncommon, Rare, Legendary)
- **Success Conditions** (stat thresholds, party composition rules)

### 5.2 Quest Failure

Failure may result in partial loot, character injury (recovery timer), or equipment damage. Characters cannot be permanently killed in the initial release.

---

## 6. Market System

- Prices are driven by an internal NPC demand simulation.
- Each NPC type (e.g., Blacksmith, Alchemist, Merchant) consumes specific item categories.
- High demand drives prices up; oversupply deflates them.
- Players can post items on the **Market Board** for NPC or future player-to-player purchase.

---

## 7. Economy

| Currency | Source                      | Usage                        |
|----------|-----------------------------|------------------------------|
| Gold     | Quest rewards, market sales | Equipment, upgrades, roster  |
| Reputation | Quest completion         | Unlocks higher-tier content  |

---

## 8. Technical Notes

- All game state is persisted in **Azure Cosmos DB**.
- Quest timers are managed server-side to prevent client manipulation.
- The market simulation runs as a scheduled **Azure Function** (Timer Trigger).

---

*This document is a living artifact and will be updated as design decisions are made.*
