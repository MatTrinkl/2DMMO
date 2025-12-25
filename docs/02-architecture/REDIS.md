# 🔴 Redis-Strategie

## 2DMMO – Redis Cache & Pub/Sub

**Version:** 2.0.0  
**Letzte Aktualisierung:** 2025-12-25  
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

#### Client-Facing Channels

| Channel | Zweck |
|---------|-------|
| `channel:zone:{zoneId}` | Zone-weite Events (Spawns, Deaths) |
| `channel:player:{playerId}` | Whispers, Party-Invites |
| `channel:guild:{guildId}` | Guild-Chat, Guild-Events |
| `channel:global` | Server-Broadcasts |

#### Server-to-Server Channels

**Siehe auch:** [SERVER_TO_SERVER.md](SERVER_TO_SERVER.md) für vollständige S2S-Architektur

| Channel Pattern | Zweck |
|-----------------|-------|
| `s2s:session:validate` | Session-Token Validierung (Gateway ↔ Zone) |
| `s2s:session:validate:response` | Validierungs-Responses |
| `s2s:server:{serverId}` | Server-spezifische Messages |
| `s2s:server:broadcast` | An alle Server (Handshake, Shutdown, Admin) |
| `s2s:transfer:zone:{zoneId}` | Zone-Transfer Requests |
| `s2s:transfer:shard:{shardId}` | Shard-Transfer Requests |
| `s2s:transfer:instance` | Instance-Management |
| `s2s:player:{playerId}` | Cross-Zone Player Messages (Whisper, Invites) |
| `s2s:guild:{guildId}` | Cross-Zone Guild Messages |
| `s2s:party:{partyId}` | Cross-Zone Party Messages |
| `s2s:matchmaking` | Matchmaking-Service Queue/Dequeue |
| `s2s:auction` | Auction-House-Service |
| `s2s:mail` | Mail-Service |
| `s2s:admin` | Admin-Commands |
| `s2s:alerts` | Monitoring-Alerts |
| `s2s:metrics` | Server-Metriken (Heartbeat, Performance) |

**Channel-Naming-Konvention:**
- Client-Channels: `channel:{scope}:{id}`
- Server-Channels: `s2s:{category}[:{id}]`

---

## Verwandte Dokumentation

- [ID-System](ID_SYSTEM.md) - AccountId, CharacterId, ZoneId für Redis Keys
- [Server-Komponenten](SERVER_COMPONENTS.md) - Redis-Integration
- [Game Loop](GAME_LOOP.md) - Persistence Phase
- [Datenbank-Strategie](DATABASE.md) - Write-Through Details
- [Server-zu-Server Kommunikation](SERVER_TO_SERVER.md) - S2S Messages & Load-Balancing

---

## 🔗 Nützliche Links

- [Redis Documentation](https://redis.io/documentation)
- [StackExchange.Redis](https://github.com/StackExchange/StackExchange.Redis)
- [Redis Pub/Sub](https://redis.io/docs/manual/pubsub/)

---

*Teil der [Architektur-Dokumentation](README.md)*
