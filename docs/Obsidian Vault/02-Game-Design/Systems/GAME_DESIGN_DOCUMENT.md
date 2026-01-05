# 🎮 Game Design Document (GDD)

## 2DMMO – High Fantasy MMO

**Version:** 0.4.0  
**Last Updated:** 2026-01-01  
**Status:** In Development

**Verwandte Dokumente:** [Technical Design](../../04-Tech/Architecture/TECHNICAL_DESIGN.md) | [Message-Referenz](../../04-Tech/API/Message-Reference.md) | [Message-Spezifikation](../../04-Tech/Architecture/MESSAGES.md)

---

## 📋 Table of Contents

1. [Vision & Overview](#1-vision--overview)  
2. [Technical Architecture](#2-technical-architecture)  
3. [Gameplay Systems](#3-gameplay-systems)  
4. [Races & Classes](#4-races--classes)  
5. [World Design](#5-world-design)  
6. [Progression](#6-progression)  
7. [Database Schema](#7-database-schema)  
8. [Art Direction & UI](#8-art-direction--ui)  
9. [Milestones](#9-milestones)  
10. [Open Questions](#10-open-questions)

---

## 1. Vision & Overview

### 1.1 Elevator Pitch

> A 2D top-down MMO in a high-fantasy world, inspired by classics like World of Warcraft and Guild Wars. Players explore a living world full of diverse races, fight monsters, join guilds, and experience epic adventures – all in a charming high-resolution pixel art style.

### 1.2 Core Features

| Feature | Description | Status |
|---------|-------------|--------|
| **Multiplayer World** | Unlimited player count through zone sharding | 🔄 In Planning |
| **Races & Classes** | Diverse playable races and classes | 📝 Concept |
| **Combat System** | Classic Tank/Healer/DPS system | 📝 Concept |
| **Persistent World** | All progress is saved in Postgres | 🔄 In Planning |
| **Zone-based World** | Dynamically loading zones (WoW-style) | 📝 Concept |
| **PvP Flagging** | Optional PvP through flagging system | 📝 Concept |

### 1.3 Target Audience

- Fans of classic MMORPGs  
- Players who appreciate nostalgic 2D graphics  
- Casual to mid-core players

### 1.4 Unique Selling Points (USPs)

1. **Diversity of Races** – Far more than just humans and classic fantasy races  
2. **2D Charm** – High-resolution pixel art (64x64) with modern gameplay  
3. **Scalability** – Designed for large player counts from the start

---

## 2. Technical Architecture

> **Note:** Detailed technical documentation see [ARCHITECTURE.md](../../04-Tech/Architecture/Architecture-Overview.md)

### 2.1 Tech Stack

| Component | Technology | Version |
|-----------|------------|---------|
| **Game Client** | Godot Engine | 4.3 (.NET Edition) |
| **Client Language** | C# | 14 |
| **Game Server** | .NET | 10 |
| **Shared Library** | .NET Class Library | 10 |
| **Transport** | TCP + TLS | - |
| **Serialization** | MessagePack | Latest |
| **Database** | PostgreSQL | 16+ |
| **Cache** | Redis | 7+ |
| **Cloud Hosting** | Microsoft Azure | Germany West Central |

### 2.2 Architecture Overview

```
┌──────────────────────────────────────────────────────────────────┐
│                         AZURE CLOUD                              │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐               │
│  │ Zone Server │  │ Zone Server │  │ Zone Server │   ...         │
│  │  (Zone A)   │  │  (Zone B)   │  │  (Zone C)   │               │
│  └──────┬──────┘  └──────┬──────┘  └──────┬──────┘               │
│         │                │                │                      │
│         └────────────────┼────────────────┘                      │
│                          │                                       │
│                 ┌────────┴────────┐                              │
│                 │     Redis       │                              │
│                 │  (Cache/PubSub) │                              │
│                 └────────┬────────┘                              │
│                          │                                       │
│                 ┌────────┴────────┐                              │
│                 │   PostgreSQL    │                              │
│                 │  (Persistence)  │                              │
│                 └─────────────────┘                              │
└──────────────────────────────────────────────────────────────────┘
                           │
              ┌────────────┼────────────┐
              │            │            │
         ┌────┴────┐  ┌────┴────┐  ┌────┴────┐
         │ Client  │  │ Client  │  │ Client  │
         │(Godot)  │  │(Godot)  │  │(Godot)  │
         └─────────┘  └─────────┘  └─────────┘
```

### 2.3 Zone Sharding Concept

```
┌─────────────────────────────────────────────────────────┐
│                    GAME WORLD                            │
│                                                          │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐               │
│  │ Zone A   │  │ Zone B   │  │ Zone C   │               │
│  │ (Forest) │──│ (City)   │──│ (Desert) │               │
│  │ Shard 1  │  │ Shard 1  │  │ Shard 1  │               │
│  │ Shard 2  │  │ Shard 2  │  │          │               │
│  │ Shard 3  │  │          │  │          │               │
│  └──────────┘  └──────────┘  └──────────┘               │
│                                                          │
│  → Shards are created dynamically under high load       │
│  → Players can switch between shards                    │
│  → Guilds/groups are preferably on the same shard       │
│                                                          │
│  Prototype: Sharding-ready, but only 1 shard active     │
└─────────────────────────────────────────────────────────┘
```

### 2.4 Network Messages (Shared DTOs)

Already implemented/planned:
- `PlayerPositionUpdate` – Synchronize player position
- `ChatMessage` – Chat messages
- `ActionRequest/ActionResult` – Combat actions

---

## 3. Gameplay Systems

### 3.1 Core Gameplay Loop

```
┌─────────────────────────────────────────────────────────┐
│                                                          │
│   ┌─────────┐    ┌─────────┐    ┌─────────┐             │
│   │ EXPLORE │───▶│  FIGHT  │───▶│  LOOT   │             │
│   └─────────┘    └─────────┘    └─────────┘             │
│        ▲                              │                  │
│        │         ┌─────────┐          │                  │
│        │         │ LEVEL/  │          │                  │
│        └─────────│ UPGRADE │◀─────────┘                  │
│                  └─────────┘                             │
│                                                          │
└─────────────────────────────────────────────────────────┘
```

### 3.2 Combat System

**Type:** Classic tab-targeting (for now)

| Role | Description | Example Classes |
|------|-------------|-----------------|
| **Tank** | Draws aggro, high armor, protects the group | Warrior, Paladin |
| **Healer** | Heals allies, buffs, debuff removal | Priest, Druid |
| **DPS** | High damage, melee or ranged | Mage, Rogue, Hunter |

**Combat Flow:**
1. Player selects target (Tab or click)
2. Player activates ability (hotbar)
3. Client sends `ActionRequest` to server
4. Server validates (range, cooldown, mana, etc.)
5. Server calculates damage/effect
6. Server sends `ActionResult` to all affected clients
7. Clients display visualization (animation, damage numbers)

### 3.3 PvP System

**Type:** Flagging system (optional PvP)

| Status | Description | Rules |
|--------|-------------|-------|
| **Unflagged** | PvP disabled (default) | Cannot be attacked, cannot attack |
| **Flagged** | PvP enabled | Can be attacked by other flagged players | 

**Flagging Rules:**
- Player can activate PvP flag at any time (immediately active)
- Deactivation only possible after 5 minutes without combat
- Attacking a flagged player automatically flags you
- Special PvP zones can enforce automatic flagging

### 3.4 Death & Respawn System

```
┌─────────────────────────────────────────────────────────┐
│                    PLAYER DIES                           │
│                          │                               │
│                          ▼                               │
│              ┌───────────────────────┐                   │
│              │   Ghost Mode Active   │                   │
│              │  (Invisible, cannot   │                   │
│              │   interact)           │                   │
│              └───────────┬───────────┘                   │
│                          │                               │
│         ┌────────────────┼────────────────┐              │
│         ▼                                 ▼              │
│  ┌──────────────┐                 ┌──────────────┐       │
│  │  GHOST RUN   │                 │  GRAVEYARD   │       │
│  │              │                 │  RESPAWN     │       │
│  │ Run back to  │                 │              │       │
│  │ your corpse  │                 │ Instant at   │       │
│  │              │                 │ graveyard    │       │
│  │ ✓ No debuff  │                 │              │       │
│  │ ✓ Full HP    │                 │ ✗ Debuff:    │       │
│  └──────────────┘                 │   "Weakness" │       │
│                                   │   (2 min)    │       │
│                                   │ ✗ 50% HP     │       │
│                                   └──────────────┘       │
└─────────────────────────────────────────────────────────┘
```

**Open World:**
- **Option A: Ghost Run** – Player runs as ghost to corpse, full resurrection
- **Option B: Graveyard Respawn** – Instant at nearest graveyard with "Weakness" debuff (2 min, -25% stats)

**Instances & Raids:**
- Respawn always at instance entrance
- No ghost run possible
- Group can wipe and restart

### 3.5 Planned Systems (Post-Prototype)

- [ ] Party system (5-player groups)
- [ ] Dungeons (instanced areas)
- [ ] PvP arenas
- [ ] Crafting
- [ ] Auction house
- [ ] Achievements
- [ ] Mounts

---

## 4. Races & Classes

### 4.1 Playable Races (Phase 1 – Classic)

| Race | Description | Racial Bonus (Idea) |
|------|-------------|---------------------|
| **Humans** | Versatile, good diplomats | +5% XP gain |
| **Elves** | Magically gifted, long-lived | +5% Mana |
| **Dwarves** | Robust, master craftsmen | +5% Armor |
| **Orcs** | Warlike, strong | +5% Melee damage |
| **Halflings** | Quick, lucky | +5% Dodge |

### 4.2 Planned Races (Phase 2+)

> The world should be more diverse than classic fantasy. Planned expansions:

- Beast-folk (Cat people, Lizardmen, etc.)
- Elemental beings
- Fae folk
- Undead (playable?)
- Constructs/Golems
- *More based on lore development*

### 4.3 Classes (Phase 1)

| Class | Role | Weapons | Core Mechanic |
|-------|------|---------|---------------|
| **Warrior** | Tank/DPS | Sword, shield, axe | Rage system |
| **Mage** | DPS | Staff, spellbook | Mana, elemental damage |
| **Priest** | Healer | Staff, symbol | Mana, healing & protection spells |
| **Rogue** | DPS | Daggers, thrown weapons | Energy, combo points |
| **Hunter** | DPS | Bow, traps | Focus, pet system |

### 4.4 Race-Class Matrix

> Still open – will be defined during development.

---

## 5. World Design

### 5.1 World Structure

The game world consists of **dynamically loading zones**, similar to World of Warcraft.

```
                    ┌─────────────────┐
                    │  STARTING AREA  │
                    │   (Level 1-10)  │
                    └────────┬────────┘
                             │
              ┌──────────────┼──────────────┐
              │              │              │
     ┌────────┴────┐  ┌──────┴──────┐  ┌────┴────────┐
     │ FOREST AREA │  │   CAPITAL   │  │   COAST     │
     │ (Level 5-15)│  │   (Hub)     │  │ (Level 5-15)│
     └──────┬──────┘  └──────┬──────┘  └──────┬──────┘
            │                │                │
            └────────────────┼────────────────┘
                             │
                    ┌────────┴────────┐
                    │   HIGH-LEVEL    │
                    │     AREAS       │
                    │   (Level 15+)   │
                    └─────────────────┘
```

### 5.2 Zone Types

| Zone Type | Description | Examples |
|-----------|-------------|----------|
| **Starting Areas** | Race-specific, tutorial | Human village, Elven forest |
| **Capital Cities** | Social hubs, merchants, guilds | Capital of the realm |
| **Leveling Areas** | Quests, monsters, exploration | Dark forest, desert |
| **Dungeons** | Instanced, group content | Cursed mine |
| **PvP Zones** | Automatic PvP flagging | Borderlands |

### 5.3 Zone Transitions

1. Player approaches zone boundary
2. Client preloads new zone in background
3. On crossing: handoff to new zone server
4. Seamless transition (no loading screen if possible)

### 5.4 Factions

> **Status:** Still open – depends on lore development

Possible options:
- No factions (all players neutral)
- 2 factions (classic)
- Guild-based factions (sandbox)

---

## 6. Progression

### 6.1 Level System

| Level Range | Phase | Content |
|-------------|-------|---------|
| 1-10 | Tutorial | Starting area, basic mechanics |
| 11-30 | Leveling | Main story, first dungeons |
| 31-50 | Endgame preparation | Group content, crafting |
| 50 | Endgame | Raids, PvP, gear grind |

**XP Sources:**
- Defeating monsters
- Completing quests
- Dungeons/Raids
- Exploration (discovery XP)

### 6.2 Gear System

**Quality Tiers:**

| Color | Quality | Drop Source |
|-------|---------|-------------|
| ⬜ Gray | Junk | Everywhere |
| ⬛ White | Normal | Normal monsters |
| 🟩 Green | Uncommon | Elite monsters, quests |
| 🟦 Blue | Rare | Dungeon bosses |
| 🟪 Purple | Epic | Raid bosses |
| 🟧 Orange | Legendary | Special events/quests |

**Equipment Slots:**
- Head, shoulders, chest, hands, legs, feet
- Main hand, off-hand/shield
- 2x rings, 1x amulet
- Cloak, belt

### 6.3 Stat System

| Stat | Effect | Primary For |
|------|--------|-------------|
| **Strength** | Melee damage, block | Warrior |
| **Agility** | Crit, dodge | Rogue, Hunter |
| **Intelligence** | Spell damage, mana | Mage |
| **Willpower** | Healing power, mana regen | Priest |
| **Stamina** | HP | All (especially tanks) |
| **Armor** | Damage reduction | Tanks |

---

## 7. Database Schema

### 7.1 Entity-Relationship Diagram (Simplified)

> **Note:** This diagram shows persistent database IDs (UUID/Guid). For the complete ID system including runtime IDs (EntityId, ZoneId, ShardId) see [ID System Documentation](../../04-Tech/Architecture/ID_SYSTEM.md).

```
┌─────────────┐       ┌─────────────────┐       ┌─────────────┐
│   ACCOUNT   │       │    CHARACTER    │       │    ITEM     │
├─────────────┤       ├─────────────────┤       ├─────────────┤
│ id (PK)     │───┐   │ id (PK)         │   ┌───│ id (PK)     │
│ email       │   │   │ account_id (FK) │◀──┘   │ name        │
│ password    │   └──▶│ name            │       │ type        │
│ created_at  │       │ race            │       │ rarity      │
│ last_login  │       │ class           │       │ stats (JSON)│
│ is_banned   │       │ level           │       └─────────────┘
└─────────────┘       │ xp              │              │
                      │ position_x      │              │
                      │ position_y      │       ┌──────┴──────┐
                      │ position_zone   │       │  INVENTORY  │
                      │ pvp_flagged     │       ├─────────────┤
                      │ stats (JSON)    │       │ char_id(FK) │
                      │ created_at      │◀──────│ item_id(FK) │
                      └─────────────────┘       │ slot        │
                             │                  │ quantity    │
                             │                  └─────────────┘
                      ┌──────┴──────┐
                      │    GUILD    │
                      ├─────────────┤
                      │ id (PK)     │
                      │ name        │
                      │ leader_id   │
                      │ created_at  │
                      └─────────────┘
```

### 7.2 Tables (Phase 1 - Prototype)

#### `accounts`
```sql
CREATE TABLE accounts (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    email           VARCHAR(255) UNIQUE NOT NULL,
    password_hash   VARCHAR(255) NOT NULL,
    created_at      TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    last_login      TIMESTAMP,
    is_banned       BOOLEAN DEFAULT FALSE
);
```

#### `characters`
```sql
CREATE TABLE characters (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    account_id      UUID REFERENCES accounts(id) ON DELETE CASCADE,
    name            VARCHAR(50) UNIQUE NOT NULL,
    race            VARCHAR(20) NOT NULL,
    class           VARCHAR(20) NOT NULL,
    level           INTEGER DEFAULT 1,
    xp              BIGINT DEFAULT 0,
    position_x      FLOAT DEFAULT 0,
    position_y      FLOAT DEFAULT 0,
    position_zone   VARCHAR(50) DEFAULT 'starting_zone',
    current_hp      INTEGER,
    max_hp          INTEGER,
    current_mana    INTEGER,
    max_mana        INTEGER,
    pvp_flagged     BOOLEAN DEFAULT FALSE,
    stats           JSONB,
    created_at      TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    played_time     INTERVAL DEFAULT '0 seconds'
);
```

### 7.3 Tables (Phase 2+)

```sql
-- Inventory
CREATE TABLE inventory (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    character_id    UUID REFERENCES characters(id) ON DELETE CASCADE,
    item_id         UUID REFERENCES items(id),
    slot            INTEGER NOT NULL,
    quantity        INTEGER DEFAULT 1,
    UNIQUE(character_id, slot)
);

-- Items (Template table)
CREATE TABLE items (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name            VARCHAR(100) NOT NULL,
    type            VARCHAR(50) NOT NULL,
    subtype         VARCHAR(50),
    rarity          VARCHAR(20) DEFAULT 'common',
    level_req       INTEGER DEFAULT 1,
    stats           JSONB,
    icon            VARCHAR(255)
);

-- Guilds
CREATE TABLE guilds (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name            VARCHAR(50) UNIQUE NOT NULL,
    leader_id       UUID REFERENCES characters(id),
    created_at      TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    bank_gold       BIGINT DEFAULT 0
);

-- Guild members
CREATE TABLE guild_members (
    guild_id        UUID REFERENCES guilds(id) ON DELETE CASCADE,
    character_id    UUID REFERENCES characters(id) ON DELETE CASCADE,
    rank            VARCHAR(20) DEFAULT 'member',
    joined_at       TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (guild_id, character_id)
);
```

---

## 8. Art Direction & UI

### 8.1 Graphics Style

**Type:** High-resolution pixel art (64x64 base)

| Element | Specification |
|---------|---------------|
| **Tile Size** | 64x64 pixels |
| **Character Sprites** | 64x64 pixels (with animations) |
| **Color Palette** | Rich and diverse, fantasy-inspired |
| **Animations** | Smooth, at least 8 frames for movement |

**References:**
- Higher resolution than classic 16-bit
- Detailed but still clearly recognizable as pixel art
- Modern lighting/shaders possible

### 8.2 Audio Concept

> **Status:** 🚧 WIP – Will be defined later

**Guidelines:**
- Style must match pixel art
- Options: Orchestral with retro elements OR chiptune/synth
- For prototype: Placeholder sounds

### 8.3 UI Design

**Style:** Modern pixel art UI

| Element | Description |
|---------|-------------|
| **General** | Clean, modern, but in pixel art style |
| **Hotbar** | Centered at bottom (classic MMO layout) |
| **Minimap** | Top right, angular with pixel frame |
| **Chat** | Bottom left, semi-transparent |
| **Inventory** | Grid-based, pixel art icons |

**References:**
- Modern indie RPGs with pixel art UI
- Scalable for different resolutions
- Accessibility-friendly (readability!)

---

## 9. Milestones

### Phase 1: Prototype (Current)
> **Goal:** Two players connect, see each other, can move  
> **Environment:** Local (no cloud deployment)

- [x] Set up project structure
- [x] Basic client-server communication
- [ ] Synchronize player movement
- [ ] Simple tilemap world
- [ ] Basic chat

**Prototype Specifics:**
- Authentication: Username only (no real login)
- Sharding: Code is ready, but only 1 shard active
- Deployment: Local only

### Phase 2: Core Gameplay
> **Goal:** Playable gameplay loop

- [ ] Race and class selection
- [ ] Combat system (basic)
- [ ] Monster spawning
- [ ] Inventory system
- [ ] Persistence (Postgres)
- [ ] Death/respawn system
- [ ] PvP flagging
- [ ] Authentication (Email + OAuth)

### Phase 3: Content
> **Goal:** Playable demo with levels 1-10

- [ ] Starting area with quests
- [ ] First 5 races/classes
- [ ] First dungeon
- [ ] Guild system

### Phase 4: Polish & Scale
> **Goal:** Beta release

- [ ] Activate zone sharding
- [ ] Load balancing
- [ ] Azure deployment (Region: Germany West Central / Frankfurt)
- [ ] Performance optimization

---

## 10. Open Questions

> These questions must be clarified during development:

### Gameplay
- [x] ~~Should PvP be optional or mandatory?~~ → Flagging system
- [x] ~~How does the respawn system work?~~ → Ghost run + graveyard option
- [ ] Should there be factions? → Depends on lore

### Technical
- [x] ~~WebSocket or pure TCP for communication?~~ → TCP + TLS
- [x] ~~How often are positions synchronized? (Tick rate?)~~ → 25 Hz
- [x] ~~Caching strategy for frequent DB accesses?~~ → Redis
- [x] ~~Serialization?~~ → MessagePack
- [x] ~~Authentication for prototype?~~ → Username only
- [x] ~~Authentication for release?~~ → Email + OAuth
- [x] ~~Sharding for prototype?~~ → Code ready, 1 shard active
- [x] ~~Azure region?~~ → Germany West Central (Frankfurt)

### Design
- [x] ~~Graphics style?~~ → High-resolution pixel art (64x64)
- [ ] Audio concept? → 🚧 WIP (matching pixel art)
- [x] ~~UI design guidelines?~~ → Modern pixel art UI

---

## 📝 Changelog

| Version | Date | Changes |
|---------|------|---------|
| 0.1.0 | 2025-12-01 | First GDD created (prototype focus) |
| 0.2.0 | 2025-12-01 | Added PvP flagging, respawn system, art direction |
| 0.3.0 | 2025-12-02 | Finalized technical decisions (TCP, MessagePack, Redis, Auth, Sharding) |
| 0.4.0 | 2026-01-01 | Translated to English |

---

*This document is a living document and will be continuously updated.*

Source: docs/01-overview/GAME_DESIGN_DOCUMENT.md
