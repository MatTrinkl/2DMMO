# 🎮 Login-to-Play Message Flow

**Version:** 1.0.0  
**Letzte Aktualisierung:** 2025-12-26  
**Status:** In Entwicklung

[← Zurück zur Architektur-Übersicht](Architecture-Overview.md)

---

## 📋 Übersicht

Dieses Dokument beschreibt den kompletten Message-Flow vom initialen Login bis zum "Ready-to-Play" Status, bei dem der Spieler vollständig in der Spielwelt geladen ist und aktiv spielen kann.

Der Flow ist in mehrere Phasen unterteilt:
1. **Authentication** - Login und Session-Erstellung
2. **Character Selection** - Auswahl des Charakters
3. **Zone Loading** - Laden der Zonen-Daten
4. **World Entry** - Spawn in die Spielwelt
5. **Ready to Play** - Vollständig initialisiert

---

## 🔄 Kompletter Message Flow

```
┌─────────────┐                    ┌─────────────┐                    ┌─────────────┐
│   Client    │                    │   Gateway   │                    │ Zone Server │
│             │                    │   Server    │                    │             │
└──────┬──────┘                    └──────┬──────┘                    └──────┬──────┘
       │                                  │                                  │
       │                                  │                                  │
       ├─────────────────────────────────────────────────────────────────────┤
       │                   Phase 1: Authentication                           │
       ├─────────────────────────────────────────────────────────────────────┤
       │                                  │                                  │
       │  (1) LoginRequest                │                                  │
       │  ─────────────────────────────► │                                  │
       │     Username, Password           │                                  │
       │                                  │                                  │
       │                                  │  Validate Credentials            │
       │                                  │  Create Session                  │
       │                                  │  Store in Redis                  │
       │                                  │                                  │
       │  (2) LoginResponse               │                                  │
       │  ◄───────────────────────────── │                                  │
       │     Success, SessionToken        │                                  │
       │                                  │                                  │
       │                                  │                                  │
       ├─────────────────────────────────────────────────────────────────────┤
       │                 Phase 2: Character Selection                        │
       ├─────────────────────────────────────────────────────────────────────┤
       │                                  │                                  │
       │  (8) CharacterListRequest        │                                  │
       │  ─────────────────────────────► │                                  │
       │                                  │                                  │
       │                                  │  Load from DB                    │
       │                                  │                                  │
       │  (13) CharacterListResponse      │                                  │
       │  ◄───────────────────────────── │                                  │
       │     List of Characters           │                                  │
       │                                  │                                  │
       │  (9) CharacterSelect             │                                  │
       │  ─────────────────────────────► │                                  │
       │     CharacterId: 98765           │                                  │
       │                                  │                                  │
       │                                  │  Load Character from DB          │
       │                                  │  Determine Spawn Zone            │
       │                                  │                                  │
       │  (21) CharacterSelectResponse    │                                  │
       │  ◄───────────────────────────── │                                  │
       │     Success, SpawnZoneId: 1001  │                                  │
       │                                  │                                  │
       │                                  │                                  │
       ├─────────────────────────────────────────────────────────────────────┤
       │                   Phase 3: Zone Loading                             │
       ├─────────────────────────────────────────────────────────────────────┤
       │                                  │                                  │
       │  (117) GetZoneRequest            │                                  │
       │  ─────────────────────────────► │                                  │
       │     ZoneId: 1001                 │                                  │
       │                                  │                                  │
       │                                  │  Forward to Zone Server          │
       │                                  │  ──────────────────────────────► │
       │                                  │                                  │
       │                                  │                                  │  Load Zone Data
       │                                  │                                  │  Prepare ZoneState
       │                                  │                                  │
       │                                  │  (102) ZoneState                 │
       │                                  │  ◄────────────────────────────── │
       │                                  │     ZoneId, Weather, TimeOfDay   │
       │                                  │     Static Zone Info             │
       │                                  │                                  │
       │  (102) ZoneState                 │                                  │
       │  ◄───────────────────────────── │                                  │
       │     ZoneName: "Elwynn Forest"    │                                  │
       │     ZoneType: "outdoor"          │                                  │
       │     Weather: "sunny"             │                                  │
       │                                  │                                  │
       │                                  │                                  │
       │  Client loads Zone Assets        │                                  │
       │  (Textures, Models, etc.)        │                                  │
       │                                  │                                  │
       │                                  │                                  │
       ├─────────────────────────────────────────────────────────────────────┤
       │                   Phase 4: World Entry                              │
       ├─────────────────────────────────────────────────────────────────────┤
       │                                  │                                  │
       │  Zone Assets loaded              │                                  │
       │  Ready to spawn                  │                                  │
       │                                  │                                  │
       │                                  │  Notify Zone Server              │
       │                                  │  Player ready to spawn           │
       │                                  │  ──────────────────────────────► │
       │                                  │                                  │
       │                                  │                                  │  Create PlayerEntity
       │                                  │                                  │  Assign EntityId
       │                                  │                                  │  Load Equipment
       │                                  │                                  │  Load Active Effects
       │                                  │                                  │
       │                                  │  (100) JoinZone                  │
       │                                  │  ◄────────────────────────────── │
       │                                  │     Player: PlayerEntityDto      │
       │                                  │     SpawnX, SpawnY, SpawnZ       │
       │                                  │                                  │
       │  (100) JoinZone                  │                                  │
       │  ◄───────────────────────────── │                                  │
       │     ZoneId: 1001                 │                                  │
       │     Player: Complete Entity      │                                  │
       │       - EntityId: 50001          │                                  │
       │       - Health, Mana, Level      │                                  │
       │       - Equipment Snapshot       │                                  │
       │       - Active Effects           │                                  │
       │       - Gold, Flags, Cooldowns   │                                  │
       │     SpawnX: 100.5                │                                  │
       │     SpawnY: 250.0                │                                  │
       │                                  │                                  │
       │  Render Character at Spawn       │                                  │
       │                                  │                                  │
       │  (200) PositionUpdate            │                                  │
       │  ─────────────────────────────► │                                  │
       │     Confirm Spawn Position       │  ──────────────────────────────► │
       │                                  │                                  │
       │                                  │                                  │  Add to Zone
       │                                  │                                  │  Broadcast to others
       │                                  │                                  │
       │                                  │  (103) PlayerJoinedZone (Broadcast)
       │                                  │  ──────────────────────────────► │
       │                                  │     (To other players)           │
       │                                  │                                  │
       │                                  │                                  │
       ├─────────────────────────────────────────────────────────────────────┤
       │                   Phase 5: Ready to Play                            │
       ├─────────────────────────────────────────────────────────────────────┤
       │                                  │                                  │
       │  Parallel Initialization:        │                                  │
       │                                  │                                  │
       │  (1100) InventorySync Request    │                                  │
       │  ─────────────────────────────► │  ──────────────────────────────► │
       │                                  │                                  │
       │  (1000) QuestSync Request        │                                  │
       │  ─────────────────────────────► │  ──────────────────────────────► │
       │                                  │                                  │
       │  (800) SkillSync Request         │                                  │
       │  ─────────────────────────────► │  ──────────────────────────────► │
       │                                  │                                  │
       │  Responses arrive...             │                                  │
       │                                  │                                  │
       │  ✅ READY TO PLAY               │                                  │
       │     - Character visible          │                                  │
       │     - Can move and interact      │                                  │
       │     - Inventory loaded           │                                  │
       │     - Quests loaded              │                                  │
       │     - Skills loaded              │                                  │
       │                                  │                                  │
       │  Regular game loop starts        │                                  │
       │  (Movement, Combat, etc.)        │                                  │
       │                                  │                                  │
```

---

## 📊 Message-Übersicht nach Phase

### Phase 1: Authentication

| # | Message | ID | Richtung | Beschreibung |
|---|---------|-----|----------|--------------|
| 1 | `LoginRequest` | 1 | Client → Gateway | Username/Password |
| 2 | `LoginResponse` | 2 | Gateway → Client | SessionToken bei Erfolg |

**Zeitaufwand:** ~100-500ms (DB-Lookup + Session-Erstellung)

---

### Phase 2: Character Selection

| # | Message | ID | Richtung | Beschreibung |
|---|---------|-----|----------|--------------|
| 3 | `CharacterListRequest` | 8 | Client → Gateway | Fordert Character-Liste an |
| 4 | `CharacterListResponse` | 13 | Gateway → Client | Liste aller Characters |
| 5 | `CharacterSelect` | 9 | Client → Gateway | Wählt Character aus |
| 6 | `CharacterSelectResponse` | 21 | Gateway → Client | Bestätigung + SpawnZoneId |

**Zeitaufwand:** ~500-2000ms (DB-Zugriff für Character-Daten)

---

### Phase 3: Zone Loading

| # | Message | ID | Richtung | Beschreibung |
|---|---------|-----|----------|--------------|
| 7 | `GetZoneRequest` | 117 | Client → Gateway → Zone | Zone-Metadaten anfordern |
| 8 | `ZoneState` | 102 | Zone → Gateway → Client | Zone-Daten (Name, Type, Weather, etc.) |

**Zeitaufwand:** ~200-1000ms (Zone-Daten laden + Client Asset-Loading)

**Client-Aktivität:**
- Lädt Zone-Assets (Textures, Models, Sounds)
- Initialisiert Rendering-Kontext
- Bereitet Spawn vor

---

### Phase 4: World Entry

| # | Message | ID | Richtung | Beschreibung |
|---|---------|-----|----------|--------------|
| 9 | `JoinZone` | 100 | Zone → Gateway → Client | PlayerEntity + Spawn-Position |
| 10 | `PositionUpdate` | 200 | Client → Gateway → Zone | Bestätigt Spawn |
| 11 | `PlayerJoinedZone` | 103 | Zone → Other Clients | Broadcast an andere Spieler |

**Zeitaufwand:** ~100-300ms (Entity-Erstellung + Spawn)

---

### Phase 5: Ready to Play

| # | Message | ID | Richtung | Beschreibung |
|---|---------|-----|----------|--------------|
| 12 | `InventorySync` | 1100 | Client ↔ Zone | Inventory-Daten laden |
| 13 | `QuestSync` | 1000 | Client ↔ Zone | Quest-Log laden |
| 14 | `SkillSync` | 800 | Client ↔ Zone | Skill-Tree laden |

**Zeitaufwand:** ~200-500ms (parallele Requests)

---

## ⏱️ Gesamt-Zeitaufwand

| Szenario | Zeit |
|----------|------|
| **Optimal** (LAN, leere Zone) | ~1.1s - 2.3s |
| **Normal** (Good Connection, normale Zone) | ~2.0s - 4.0s |
| **Langsam** (High Latency, volle Zone) | ~4.0s - 8.0s |

**Bottlenecks:**
- Datenbankzugriffe (Character laden, Zone laden)
- Client Asset-Loading (Texturen, Models)
- Netzwerk-Latenz

---

## 🔑 Design-Entscheidungen

### Warum GetZone vor JoinZone?

**Problem:** JoinZone enthielt redundante Daten (ZoneName, ZoneType), die bereits durch ZoneId identifiziert werden können.

**Lösung:** 
1. Client empfängt `SpawnZoneId` in `CharacterSelectResponse`
2. Client requested Zone-Metadaten via `GetZoneRequest`
3. Client lädt Zone-Assets während der Antwort
4. Server sendet `JoinZone` erst wenn Client bereit ist

**Vorteile:**
- ✅ Keine Daten-Duplikation
- ✅ Client kann Assets vorladen
- ✅ Bessere Trennung von Concerns
- ✅ Flexibleres Caching möglich

**Nachteile:**
- ❌ Ein zusätzlicher Roundtrip
- ❌ Geringfügig komplexerer Flow

---

## 📝 Wichtige Notizen

### JoinZone enthält PlayerEntityDto

Ab Version 2.0 der Dokumentation enthält `JoinZone` das vollständige `PlayerEntityDto` mit:
- Character Stats (Health, Mana, Level, XP)
- Equipment Snapshot (für Rendering)
- Active Effects (Buffs/Debuffs)
- Gold und Currencies
- Flags (PvP, Combat, Resting)
- Ability Cooldowns

**Rationale:** Reduziert Round-Trips für sofortiges Gameplay. Client kann Character sofort rendern und Status anzeigen.

### Was ist NICHT in JoinZone?

Folgende Daten werden separat geladen (Phase 5):
- **Vollständiges Inventory** → `InventorySync` (1100)
- **Quest-Log** → `QuestSync` (1000)
- **Skill-Tree Details** → `SkillSync` (800)
- **Freundesliste** → `FriendListResponse` (2105)

---

## 🔗 Verwandte Dokumentation

- [Connection Messages (00-99)](../API/00-connection.md)
- [Zone Messages (100-199)](../API/01-zone.md)
- [Movement Messages (200-299)](../API/02-movement.md)
- [Network Protocol](NETWORK_PROTOCOL.md)
- [Client-Server Sync](CLIENT_SERVER_SYNC.md)

---

## 🐛 Error Handling

### Mögliche Fehler und Recovery

| Phase | Fehler | Recovery |
|-------|--------|----------|
| Authentication | `INVALID_CREDENTIALS` | Zeige Login-Fehler, erneuter Versuch |
| Authentication | `ACCOUNT_BANNED` | Zeige Ban-Grund, keine Recovery |
| Character Selection | `CHARACTER_NOT_FOUND` | Reload Character-Liste |
| Character Selection | `CHARACTER_IN_USE` | Warten oder Force-Disconnect |
| Zone Loading | `ZONE_NOT_FOUND` | Sollte nicht passieren, Log + Support |
| Zone Loading | `ZONE_LOCKED` | Zeige Maintenance-Message |
| World Entry | `SPAWN_FAILED` | Retry mit Default-Spawn |
| World Entry | `ZONE_FULL` | Queue oder alternativer Shard |

---

## 📊 Monitoring & Metrics

Wichtige Metriken für diesen Flow:

| Metrik | Ziel | Alert bei |
|--------|------|-----------|
| Login-to-Ready Time | < 3s (p95) | > 5s |
| Character Select Time | < 1s (p95) | > 2s |
| Zone Load Time | < 1s (p95) | > 3s |
| JoinZone Time | < 300ms (p95) | > 1s |
| Failed Logins | < 1% | > 5% |
| Failed Zone Joins | < 0.1% | > 1% |

---

**Letzte Aktualisierung:** 2025-12-26  
**Version:** 1.0.0

[← Zurück zur Architektur-Übersicht](Architecture-Overview.md)

Source: docs/02-architecture/LOGIN_TO_PLAY_FLOW.md
