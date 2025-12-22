# 🔴 Redis-Strategie

## 2DMMO – Redis Cache & Pub/Sub

**Version:** 1.1.0  
**Letzte Aktualisierung:** 2025-12-02  
**Teil von:** [Architektur-Dokumentation](README.md)

---

## 📋 Übersicht

Diese Dokumentation beschreibt die Redis-Strategie für das 2DMMO, einschließlich Key Schema, Caching-Strategien und Pub/Sub Channels.

---

## Datenstruktur

```
┌─────────────────────────────────────────────────────────┐
│                  REDIS KEY SCHEMA                        │
│                                                          │
│  ═══════════════════════════════════════════════════    │
│  SESSIONS                                               │
│  ═══════════════════════════════════════════════════    │
│  session:{sessionId}          → Hash                    │
│    ├─ playerId                                          │
│    ├─ characterId                                       │
│    ├─ currentZone                                       │
│    ├─ currentShard                                      │
│    ├─ gatewayId                                         │
│    └─ lastActivity                                      │
│                                                          │
│  player:session:{playerId}    → String (sessionId)      │
│                                                          │
│  ═══════════════════════════════════════════════════    │
│  PLAYER DATA (Hot Cache)                                │
│  ═══════════════════════════════════════════════════    │
│  player:{playerId}            → Hash                    │
│    ├─ name                                              │
│    ├─ level                                             │
│    ├─ currentHp                                         │
│    ├─ maxHp                                             │
│    ├─ currentMana                                       │
│    ├─ maxMana                                           │
│    ├─ positionX                                         │
│    ├─ positionY                                         │
│    ├─ zone                                              │
│    └─ pvpFlagged                                        │
│                                                          │
│  ═══════════════════════════════════════════════════    │
│  ZONE DATA                                              │
│  ═══════════════════════════════════════════════════    │
│  zone:{zoneId}:players        → Set (playerIds)         │
│  zone:{zoneId}:shard:{n}:players → Set (playerIds)      │
│                                                          │
│  ═══════════════════════════════════════════════════    │
│  PUB/SUB CHANNELS                                       │
│  ═══════════════════════════════════════════════════    │
│  channel:zone:{zoneId}        → Zone-weite Events       │
│  channel:player:{playerId}    → Whispers, Party-Invite  │
│  channel:guild:{guildId}      → Guild-Chat, Events      │
│  channel:global               → Broadcasts              │
│                                                          │
│  ═══════════════════════════════════════════════════    │
│  RATE LIMITING                                          │
│  ═══════════════════════════════════════════════════    │
│  ratelimit:chat:{playerId}    → Counter (TTL: 60s)      │
│  ratelimit:action:{playerId}  → Counter (TTL: 1s)       │
└─────────────────────────────────────────────────────────┘
```

---

## Caching-Strategie

| Daten | Redis TTL | Write-Through | Beschreibung |
|-------|-----------|---------------|--------------|
| Session | 30 min | Nein | Nur in Redis, bei Timeout → Logout |
| Position | - | Alle 10s | Hot in Redis, periodisch in DB |
| HP/Mana | - | Bei Änderung | Sofort in Redis, alle 30s in DB |
| Inventar | 5 min | Bei Änderung | Cache, Source of Truth = DB |
| Chat | Kein Cache | - | Direkt in DB (History) |

---

## Key Schema Details

### Sessions

```
session:{guid}
├─ playerId: int
├─ characterId: int
├─ currentZone: string
├─ currentShard: int
├─ gatewayId: string
└─ lastActivity: timestamp
```

### Player Data

```
player:{playerId}
├─ name: string
├─ level: int
├─ currentHp: int
├─ maxHp: int
├─ currentMana: int
├─ maxMana: int
├─ positionX: float
├─ positionY: float
├─ zone: string
└─ pvpFlagged: bool
```

### Pub/Sub Channels

| Channel | Zweck |
|---------|-------|
| `channel:zone:{zoneId}` | Zone-weite Events (Spawns, Deaths) |
| `channel:player:{playerId}` | Whispers, Party-Invites |
| `channel:guild:{guildId}` | Guild-Chat, Guild-Events |
| `channel:global` | Server-Broadcasts |

---

## Verwandte Dokumentation

- [ID-System](ID_SYSTEM.md) - AccountId, CharacterId, ZoneId für Redis Keys
- [Server-Komponenten](SERVER_COMPONENTS.md) - Redis-Integration
- [Game Loop](GAME_LOOP.md) - Persistence Phase
- [Datenbank-Strategie](DATABASE.md) - Write-Through Details

---

## 🔗 Nützliche Links

- [Redis Documentation](https://redis.io/documentation)
- [StackExchange.Redis](https://github.com/StackExchange/StackExchange.Redis)
- [Redis Pub/Sub](https://redis.io/docs/manual/pubsub/)

---

*Teil der [Architektur-Dokumentation](../ARCHITECTURE.md)*
