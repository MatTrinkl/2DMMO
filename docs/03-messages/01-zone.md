# 🗺️ Zone Events Messages (0100-0199)

**Kategorie:** 1  
**Range:** 0100-0199  
**Phase:** Prototyp  
**Status:** 🟢 In Entwicklung

[← Zurück zur Übersicht](README.md)

---

## 📋 Inhaltsverzeichnis

-   [Zone Loading Flow (Übersicht)](#-zone-loading-flow-übersicht)
-   [DTOs](#dtos)
    -   [ZoneDto](#zonedto)
    -   [ZoneListItemDto](#zonelistitemdto)
-   [Aktive Messages](#aktive-messages)
    -   [LeaveZone (101)](#leavezone-101)
    -   [ZoneState (102)](#zonestate-102)
    -   [ZoneDelta (103)](#zonedelta-103)
    -   [PlayerJoinedZone (104)](#playerjoinedzone-104)
    -   [PlayerLeftZone (105)](#playerleftzone-105)
    -   [ZoneTransferRequest (106)](#zonetransferrequest-106)
    -   [ZoneTransferResponse (107)](#zonetransferresponse-107)
    -   [ZoneDiscovered (108)](#zonediscovered-108)
    -   [ZoneListRequest (109)](#zonelistrequest-109)
    -   [ZoneListResponse (110)](#zonelistresponse-110)
    -   [GetZoneRequest (117)](#getzonerequest-117)
    -   [ZoneLoadedAck (119)](#zoneloadedack-119)
    -   [EntityBatch (120)](#entitybatch-120)
-   [Phase 2 Messages](#phase-2-messages)
    -   [ShardTransfer (111)](#shardtransfer-111)
    -   [ShardListRequest (112)](#shardlistrequest-112)
    -   [ShardListResponse (113)](#shardlistresponse-113)
    -   [SubZoneEnter (114)](#subzoneenter-114)
    -   [SubZoneLeave (115)](#subzoneleave-115)
    -   [ZonePhaseChange (116)](#zonephasechange-116)
-   [Obsolete Messages](#obsolete-messages)
    -   [JoinZone (100)](#joinzone-100-obsolet)
    -   [ZoneLoadingProgress (107)](#zoneloadingprogress-107-obsolet)
    -   [GetZoneResponse (118)](#getzoneresponse-118-obsolet)

---

## 🔄 Zone Loading Flow (Übersicht)

Der Zone-Loading-Prozess wurde vereinfacht. Eine einzige `ZoneState` Message enthält alle Daten die der Client zum Spawnen braucht.

### Architektur: ZoneDto vs ZoneState

```
┌─────────────────────────────────────────────────────────────────┐
│  ZoneDto (statisch, cachebar)                                   │
│  ├── ZoneId, Name                                               │
│  ├── Flags (ZoneFlags - PvP, Restrictions, etc.)               │
│  ├── RecommendedLevel, ControllingFaction                      │
│  ├── SpawnPoints, Graveyard                                     │
│  └── Music, Ambience, Bounds                                    │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│  ZoneState (dynamisch, Runtime)                                 │
│  ├── ZoneId (Referenz)                                          │
│  ├── ZoneInfo:  ZoneDto?  (nur bei erstem Besuch)                │
│  ├── CurrentWeather, TimeOfDay (dynamisch)                      │
│  ├── StateType (Initial/Transfer/Reconnect/FullSync)           │
│  ├── MyPlayer:  PlayerEntityDto                                  │
│  └── Entities:  List<EntityDtoUnion>                             │
└─────────────────────────────────────────────────────────────────┘
```

**Vorteile dieser Trennung:**

-   **Caching:** Client kann `ZoneDto` lokal speichern
-   **Kleinere Messages:** `ZoneDto` nur bei erstem Besuch einer Zone
-   **Wiederverwendung:** `ZoneDto` in mehreren Messages nutzbar
-   **Separate Updates:** Wetter/Zeit ohne vollen ZoneState änderbar

### Haupt-Flow: Login → Zone

```
Client                         Server
  │                              │
  │  CharacterSelectResponse     │
  │  (SpawnZoneId:  1001)         │
  │◄─────────────────────────────│
  │                              │
  │  GetZoneRequest (117)        │
  │  ZoneId: 1001                │
  │─────────────────────────────►│
  │                              │
  │  ZoneState (102)             │
  │  ├── ZoneInfo: ZoneDto       │  ← Nur bei erstem Besuch!
  │  ├── CurrentWeather, TimeOfDay│
  │  ├── StateType:  Initial      │
  │  ├── MyPlayer: PlayerEntityDto│
  │  └── Entities: List<EntityDtoUnion>│
  │◄─────────────────────────────│
  │                              │
  │  [Optional bei >100 Entities]│
  │  EntityBatch (120)           │
  │◄─────────────────────────────│
  │                              │
  │  [Client cached ZoneDto]     │
  │  [Client lädt Assets]        │
  │                              │
  │  ZoneLoadedAck (119)         │
  │─────────────────────────────►│
  │                              │
  │  [Server startet Updates]    │
  │  PositionBroadcast (201)     │
  │◄─────────────────────────────│
```

### Zone Transfer Flow (Wiederbesuch)

```
Client                         Server
  │                              │
  │  ZoneTransferRequest (105)   │
  │  TargetZoneId: 1001          │  ← Zone bereits besucht
  │─────────────────────────────►│
  │                              │
  │  ZoneTransferResponse (106)  │
  │  Success: true               │
  │◄─────────────────────────────│
  │                              │
  │  ZoneState (102)             │
  │  ├── ZoneInfo:  null          │  ← KEIN ZoneDto (bereits gecached)
  │  ├── CurrentWeather, TimeOfDay│
  │  ├── StateType: Transfer     │
  │  ├── MyPlayer:  PlayerEntityDto│
  │  └── Entities: [...]         │
  │◄─────────────────────────────│
  │                              │
  │  [Client nutzt cached ZoneDto]│
  │  [Client lädt Assets]        │
  │                              │
  │  ZoneLoadedAck (119)         │
  │─────────────────────────────►│
```

### Logout Flow (Client-initiiert)

```
Client                         Server
  │                              │
  │  LeaveZone (101)             │
  │  Reason: Logout              │
  │─────────────────────────────►│
  │                              │
  │  [Server speichert State]    │
  │                              │
  │  [PlayerLeftZone broadcast]  │
  │  Reason: Logout              │
  │                              │
  │  [Connection close]          │
```

### Disconnect Flow (Server erkennt)

```
Client                         Server
  │                              │
  │  [Verbindung bricht ab]      │
  │      ╳ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─│
  │                              │
  │                              │  [Heartbeat-Timeout]
  │                              │  [oder TCP Error]
  │                              │
  │                              │  [PlayerLeftZone broadcast]
  │                              │  Reason: Disconnect
  │                              │
  │                              │  [30s Reconnect-Window]
```

### Tod Flow (Server-Event)

```
Client                         Server
  │                              │
  │                              │  [Spieler HP <= 0]
  │                              │
  │  DeathEvent (303)            │  ← Combat-Message
  │◄─────────────────────────────│
  │                              │
  │  [PlayerLeftZone broadcast]  │
  │  Reason: Death               │
  │                              │
  │  [Optional: Ghost-Zone]      │
  │  ZoneState (102)             │
  │  StateType: Transfer         │
  │◄─────────────────────────────│
```

### Message-Übersicht

| Message                | ID  | Richtung           | Zweck                             |
| ---------------------- | --- | ------------------ | --------------------------------- |
| `GetZoneRequest`       | 117 | Client → Server    | Zone-Daten anfordern              |
| `ZoneState`            | 102 | Server → Client    | Zone + MyPlayer + Entities        |
| `ZoneDelta`            | 103 | Server → Client    | Delta-Updates (jeden Tick)        |
| `EntityBatch`          | 120 | Server → Client    | Weitere Entities (Chunking)       |
| `ZoneLoadedAck`        | 119 | Client → Server    | Client bestätigt Ready            |
| `ZoneTransferRequest`  | 106 | Client → Server    | Zone wechseln wollen              |
| `ZoneTransferResponse` | 107 | Server → Client    | Transfer bestätigen/ablehnen      |
| `LeaveZone`            | 101 | Client → Server    | Logout/Exit/CharacterSwitch       |
| `PlayerJoinedZone`     | 104 | Server → Broadcast | Neuer Spieler in Zone             |
| `PlayerLeftZone`       | 105 | Server → Broadcast | Spieler verlässt Zone             |

### Obsolete Messages

| Message               | ID  | Ersetzt durch         |
| --------------------- | --- | --------------------- |
| `JoinZone`            | 100 | `ZoneState. MyPlayer` |
| `ZoneLoadingProgress` | 107 | Client lädt lokal     |
| `GetZoneResponse`     | 118 | `ZoneState` direkt    |

---

# DTOs

---

## ZoneDto

**Zweck:** Statische Zone-Metadaten die sich selten ändern und client-seitig gecached werden können.

> **Hinweis:** Verwendet das existierende `ZoneFlags` Enum für Zone-Eigenschaften.

### Felder

| Feld                | Typ        | Beschreibung                                 | Pflicht |
| ------------------- | ---------- | -------------------------------------------- | ------- |
| ZoneId              | ushort     | Eindeutige Zone-ID                           | Ja      |
| Name                | string     | Anzeigename der Zone                         | Ja      |
| Flags               | ZoneFlags  | Zone-Eigenschaften (PvP, Restrictions, etc.) | Ja      |
| RecommendedMinLevel | int        | Empfohlenes Mindest-Level                    | Ja      |
| RecommendedMaxLevel | int        | Empfohlenes Höchst-Level                     | Ja      |
| ControllingFaction  | Faction?   | Kontrollierende Fraktion (null = neutral)    | Nein    |
| DefaultSpawnPoint   | Position   | Standard-Spawn-Position für neue Spieler     | Ja      |
| GraveyardPosition   | Position?  | Friedhof-Position für Wiederbelebung         | Nein    |
| MusicId             | string     | Musik-Asset-ID                               | Ja      |
| AmbienceId          | string     | Ambiente-Sound-Asset-ID                      | Ja      |
| Bounds              | ZoneBounds | Zone-Grenzen (für Minimap)                   | Ja      |

### Existierendes ZoneFlags Enum

Das `ZoneFlags` Enum existiert bereits und ersetzt einen separaten `ZoneType`:

```csharp
// Mmo.Shared/Zones/Enums/ZoneFlags.cs (bereits vorhanden)
[Flags]
public enum ZoneFlags :  ushort
{
    None = 0,

    // PvP
    PvpEnabled = 1 << 0,
    AutoFlagPvp = 1 << 1,

    // Restrictions
    NoMounting = 1 << 2,
    NoFlying = 1 << 3,
    NoCombat = 1 << 4,
    NoSpellCast = 1 << 5,
    NoSummon = 1 << 6,

    // Special
    IsCapital = 1 << 7,
    IsInstance = 1 << 8,
    IsRaid = 1 << 9,
    IsBattleground = 1 << 10,
    IsArena = 1 << 11,

    // Environment
    IsIndoor = 1 << 12,
    IsUnderwater = 1 << 13,

    // Rest
    HasRestXp = 1 << 14,

    // NEU: Für Ghost-Zone nach Tod
    IsGhostZone = 1 << 15
}
```

### Code-Beispiel

```csharp
[MessagePackObject]
public class ZoneDto
{
    [Key(0)] public ushort ZoneId { get; set; }
    [Key(1)] public string Name { get; set; } = "";
    [Key(2)] public ZoneFlags Flags { get; set; }
    [Key(3)] public int RecommendedMinLevel { get; set; }
    [Key(4)] public int RecommendedMaxLevel { get; set; }
    [Key(5)] public Faction?  ControllingFaction { get; set; }
    [Key(6)] public Position DefaultSpawnPoint { get; set; }
    [Key(7)] public Position? GraveyardPosition { get; set; }
    [Key(8)] public string MusicId { get; set; } = "";
    [Key(9)] public string AmbienceId { get; set; } = "";
    [Key(10)] public ZoneBounds Bounds { get; set; }

    // Convenience Properties (nicht serialisiert)
    [IgnoreMember] public bool IsPvPEnabled => Flags.HasFlag(ZoneFlags.PvpEnabled);
    [IgnoreMember] public bool IsInstance => Flags.HasFlag(ZoneFlags.IsInstance);
    [IgnoreMember] public bool IsCapital => Flags.HasFlag(ZoneFlags.IsCapital);
    [IgnoreMember] public bool IsSanctuary => Flags.HasFlag(ZoneFlags.NoCombat);
    [IgnoreMember] public bool HasRestXp => Flags.HasFlag(ZoneFlags.HasRestXp);
    [IgnoreMember] public bool IsIndoor => Flags.HasFlag(ZoneFlags.IsIndoor);
    [IgnoreMember] public bool AllowsMounting => ! Flags.HasFlag(ZoneFlags.NoMounting);
    [IgnoreMember] public bool AllowsFlying => !Flags.HasFlag(ZoneFlags.NoFlying);
}

/// <summary>
/// Extension methods for Zone to DTO conversion.
/// </summary>
public static class ZoneDtoExtensions
{
    /// <summary>
    /// Converts a Zone to ZoneDto.
    /// </summary>
    public static ZoneDto ToDto(this Zone zone) => new()
    {
        ZoneId = zone.Id,
        Name = zone.Name,
        Flags = zone.Flags,
        RecommendedMinLevel = zone.RecommendedMinLevel,
        RecommendedMaxLevel = zone.RecommendedMaxLevel,
        ControllingFaction = zone.ControllingFaction,
        DefaultSpawnPoint = zone.DefaultSpawnPoint,
        GraveyardPosition = zone.GraveyardPosition,
        MusicId = zone.MusicId,
        AmbienceId = zone.AmbienceId,
        Bounds = zone.Bounds
    };
}
```

### Client-Caching

```csharp
public class ZoneCache
{
    private readonly Dictionary<ushort, ZoneDto> _cache = new();

    public void CacheZone(ZoneDto zone)
    {
        _cache[zone.ZoneId] = zone;
    }

    public ZoneDto?  GetZone(ushort zoneId)
    {
        return _cache.TryGetValue(zoneId, out var zone) ? zone : null;
    }

    public bool HasZone(ushort zoneId) => _cache.ContainsKey(zoneId);
}

// Verwendung bei ZoneState
public void OnZoneState(ZoneState state)
{
    // ZoneDto cachen wenn vorhanden (erster Besuch)
    if (state.ZoneInfo != null)
    {
        _zoneCache.CacheZone(state.ZoneInfo);
    }

    // Zone-Infos aus Cache holen
    var zoneInfo = _zoneCache.GetZone(state.ZoneId);
    if (zoneInfo == null)
    {
        _log.Error("ZoneDto not found for zone {ZoneId}!", state.ZoneId);
        RequestDisconnect("Missing zone data");
        return;
    }

    // Statische Zone-Daten anwenden
    _audioManager.PlayMusic(zoneInfo.MusicId);
    _audioManager.PlayAmbience(zoneInfo. AmbienceId);
    _minimapManager.SetBounds(zoneInfo.Bounds);
    _pvpManager.SetPvPState(zoneInfo.IsPvPEnabled);

    // Mount-Button basierend auf Flags
    _uiManager.SetMountButtonEnabled(zoneInfo.AllowsMounting);

    // Dynamischen State anwenden
    _weatherSystem.SetWeather(state.CurrentWeather);
    _timeSystem.SetTime(state.TimeOfDay);
}
```

### Verwendung in Messages

| Message            | Verwendung                                    |
| ------------------ | --------------------------------------------- |
| `ZoneState`        | `ZoneInfo:  ZoneDto?` - nur bei erstem Besuch |
| `ZoneListResponse` | `List<ZoneListItemDto>` enthält `ZoneDto`     |
| `ZoneDiscovered`   | `Zone:  ZoneDto` - neue Zone entdeckt         |

---

## ZoneListItemDto

**Zweck:** Kombination aus statischen Zone-Daten und spieler-spezifischen Informationen für die Zonen-Liste.

### Felder

| Feld                | Typ       | Beschreibung                              | Pflicht |
| ------------------- | --------- | ----------------------------------------- | ------- |
| Zone                | ZoneDto   | Statische Zone-Daten                      | Ja      |
| IsDiscovered        | bool      | Hat der Spieler diese Zone entdeckt?      | Ja      |
| HasFlightPath       | bool      | Hat der Spieler hier einen Flugpunkt?     | Ja      |
| FlightPathPosition  | Position? | Position des Flugpunkts (falls vorhanden) | Nein    |
| CompletedQuestCount | int       | Anzahl abgeschlossener Quests in Zone     | Ja      |
| TotalQuestCount     | int       | Gesamtzahl Quests in Zone                 | Ja      |

### Code-Beispiel

```csharp
[MessagePackObject]
public class ZoneListItemDto
{
    [Key(0)] public ZoneDto Zone { get; set; } = null!;
    [Key(1)] public bool IsDiscovered { get; set; }
    [Key(2)] public bool HasFlightPath { get; set; }
    [Key(3)] public Position?  FlightPathPosition { get; set; }
    [Key(4)] public int CompletedQuestCount { get; set; }
    [Key(5)] public int TotalQuestCount { get; set; }

    public static ZoneListItemDto Create(Zone zone, PlayerEntity player) => new()
    {
        Zone = zone.ToDto(),
        IsDiscovered = player. DiscoveredZones.Contains(zone. Id),
        HasFlightPath = player.UnlockedFlightPaths.Contains(zone.Id),
        FlightPathPosition = zone.FlightPathPosition,
        CompletedQuestCount = player.GetCompletedQuestCountForZone(zone.Id),
        TotalQuestCount = zone.TotalQuestCount
    };
}
```

---

# Aktive Messages

---

## LeaveZone (101)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Client informiert Server über beabsichtigtes Verlassen. Wird **NUR** für Client-initiierte Aktionen verwendet:

-   Logout-Button geklickt
-   Spiel beenden
-   Zurück zur Character-Auswahl

> **Wichtig:**
>
> -   Für Zone-Wechsel (Portal, Teleport, Hearthstone) verwende `ZoneTransferRequest` (105)!
> -   Bei Verbindungsabbruch erkennt der Server dies selbst (Heartbeat-Timeout)
> -   Bei Tod sendet der Server `DeathEvent` (303) - kein Client-Request nötig

### Im Scope ✅

-   Normaler Logout
-   Spiel beenden
-   Character wechseln

### Nicht im Scope ❌

-   Zone-Transfer → verwende `ZoneTransferRequest` (105)
-   Disconnect → Server erkennt selbst via Heartbeat
-   Tod → Server-Event via `DeathEvent` (303)

### Payload

| Feld   | Typ         | Beschreibung                      | Pflicht |
| ------ | ----------- | --------------------------------- | ------- |
| Type   | MessageType | `MessageType.LeaveZone`           | Ja      |
| Reason | LeaveReason | Logout, ExitGame, CharacterSwitch | Ja      |

### Enums

```csharp
public enum LeaveReason :  byte
{
    Logout = 1,           // Logout-Button → zurück zum Login
    ExitGame = 2,         // Spiel komplett beenden
    CharacterSwitch = 3   // Zurück zur Character-Auswahl
}
```

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.LeaveZone)]
public class LeaveZone : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.LeaveZone;
    [Key(1)] public LeaveReason Reason { get; set; }
}
```

### Server-Verhalten

| Reason            | Server-Aktion                                                        |
| ----------------- | -------------------------------------------------------------------- |
| `Logout`          | State speichern → PlayerLeftZone broadcast → Connection close        |
| `ExitGame`        | State speichern → PlayerLeftZone broadcast → Connection close        |
| `CharacterSwitch` | State speichern → PlayerLeftZone broadcast → Zurück zu CharacterList |

### Beispiel Payloads

```csharp
// Logout
var logout = new LeaveZone
{
    Reason = LeaveReason. Logout
};

// Zurück zur Character-Auswahl
var switchChar = new LeaveZone
{
    Reason = LeaveReason.CharacterSwitch
};
```

### Verwandte Messages

| Message                | ID  | Beziehung                           |
| ---------------------- | --- | ----------------------------------- |
| `PlayerLeftZone`       | 104 | Broadcast an andere Spieler         |
| `ZoneTransferRequest`  | 105 | Für Zone-Wechsel (nicht LeaveZone!) |
| `CharacterListRequest` | 12  | Nach CharacterSwitch                |
| `Disconnect`           | 5   | Connection-Level Disconnect         |

---

## ZoneState (102)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten (Initial Load, Zone Transfer, Periodic Sync, Reconnect)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Kompletter Snapshot des Zone-States. Dies ist die **Haupt-Message für Zone-Loading** und enthält:

-   **ZoneInfo**: Statische Zone-Daten als `ZoneDto` (nur bei erstem Besuch, sonst `null`)
-   **Dynamischer State**: Wetter, Tageszeit
-   **MyPlayer**: Dein Character als `PlayerEntityDto`
-   **Entities**: Alle anderen Entities **in sichtbaren Chunks** als DTOs

> **Phase 2 Update - Chunk-Based Filtering:**  
> Ab Phase 2 werden nur Entities in **sichtbaren Chunks** (3x3 Grid um Spieler) gesendet.  
> Siehe [Chunk-Based Sync](../../02-architecture/CHUNK_BASED_SYNC.md) für Details.

### StateType Enum

| Wert            | Beschreibung                       | MyPlayer     | ZoneInfo             |
| --------------- | ---------------------------------- | ------------ | -------------------- |
| `Initial` (1)   | Erster Login, Character spawnt     | ✅ Enthalten | ✅ Bei erstem Besuch |
| `Transfer` (2)  | Zone-Wechsel (Portal/Teleport/Tod) | ✅ Enthalten | ✅ Bei erstem Besuch |
| `Reconnect` (3) | Nach Verbindungsabbruch            | ✅ Enthalten | ❌ null (gecached)   |
| `FullSync` (4)  | Periodischer Full-Sync             | ❌ null      | ❌ null              |

### Payload

| Feld             | Typ                | Beschreibung                            | Pflicht     |
| ---------------- | ------------------ | --------------------------------------- | ----------- |
| Type             | MessageType        | `MessageType.ZoneState`                 | Ja          |
| Timestamp        | long               | Server-Timestamp (Unix ms)              | Ja          |
| ZoneId           | ushort             | Zone-ID                                 | Ja          |
| ZoneInfo         | ZoneDto?           | Statische Daten (nur bei erstem Besuch) | Conditional |
| CurrentWeather   | WeatherType        | Aktuelles Wetter                        | Ja          |
| TimeOfDay        | float              | 0. 0-24.0 (Stunden)                     | Ja          |
| StateType        | ZoneStateType      | Initial, Transfer, Reconnect, FullSync  | Ja          |
| MyPlayer         | PlayerEntityDto?   | Dein Character (null bei FullSync)      | Conditional |
| Entities         | List\<EntityDtoUnion\> | Alle Entities (max 100 pro Message)     | Ja          |
| HasMoreEntities  | bool               | Gibt es weitere Entity-Batches?         | Ja          |
| TotalEntityCount | int                | Gesamtzahl Entities in Zone             | Ja          |

### Enums

```csharp
public enum ZoneStateType : byte
{
    Initial = 1,    // Erster Login
    Transfer = 2,   // Zone-Wechsel (Portal, Teleport, Tod, etc.)
    Reconnect = 3,  // Nach Verbindungsabbruch
    FullSync = 4    // Periodischer Sync
}

public enum WeatherType : byte
{
    Clear = 1,
    Cloudy = 2,
    Rain = 3,
    HeavyRain = 4,
    Snow = 5,
    Blizzard = 6,
    Fog = 7,
    Storm = 8,
    Sandstorm = 9
}
```

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.ZoneState)]
public class ZoneState : ITimestampedServerMessage
{
    [Key(0)] public MessageType Type => MessageType.ZoneState;
    [Key(1)] public long Timestamp { get; set; }

    // Zone-Referenz
    [Key(2)] public ushort ZoneId { get; set; }

    // Statische Zone-Daten (nur bei erstem Besuch, sonst null)
    [Key(3)] public ZoneDto? ZoneInfo { get; set; }

    // Dynamischer Zone-State
    [Key(4)] public WeatherType CurrentWeather { get; set; }
    [Key(5)] public float TimeOfDay { get; set; }
    [Key(6)] public ZoneStateType StateType { get; set; }

    // Spieler-Daten
    [Key(7)] public PlayerEntityDto? MyPlayer { get; set; }

    // Entities
    [Key(8)] public List<EntityDtoUnion> Entities { get; set; } = new();
    [Key(9)] public bool HasMoreEntities { get; set; }
    [Key(10)] public int TotalEntityCount { get; set; }
}
```

### Server-Logik

```csharp
public ZoneState CreateZoneState(PlayerEntity player, Zone zone, ZoneStateType stateType)
{
    // ZoneDto nur senden wenn Spieler Zone noch nie besucht hat
    bool firstVisit = ! player.VisitedZones.Contains(zone.Id);

    // Bei erstem Besuch: Zone als besucht markieren
    if (firstVisit)
    {
        player.VisitedZones.Add(zone.Id);
    }

    return new ZoneState
    {
        Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
        ZoneId = zone.Id,
        ZoneInfo = firstVisit ? zone.ToDto() : null,
        CurrentWeather = zone.CurrentWeather,
        TimeOfDay = zone.TimeOfDay,
        StateType = stateType,
        MyPlayer = stateType != ZoneStateType. FullSync
            ? player.ToDto()
            : null,
        Entities = zone.GetVisibleEntities(player)
            .ToUnionDtoList()
            .Take(100)
            .ToList(),
        HasMoreEntities = zone.GetVisibleEntityCount(player) > 100,
        TotalEntityCount = zone.GetVisibleEntityCount(player)
    };
}
```

### Client-Logik

```csharp
public void OnZoneState(ZoneState state)
{
    // 1. ZoneDto cachen wenn vorhanden (erster Besuch)
    if (state.ZoneInfo != null)
    {
        _zoneCache.CacheZone(state.ZoneInfo);
    }

    // 2. Zone-Infos aus Cache holen
    var zoneInfo = _zoneCache. GetZone(state.ZoneId);
    if (zoneInfo == null)
    {
        _log.Error("ZoneDto not found for zone {ZoneId}!", state.ZoneId);
        RequestDisconnect("Missing zone data");
        return;
    }

    // 3. Statische Zone-Daten anwenden
    _audioManager.PlayMusic(zoneInfo.MusicId);
    _audioManager.PlayAmbience(zoneInfo.AmbienceId);
    _minimapManager.SetBounds(zoneInfo.Bounds);
    _pvpManager.SetPvPState(zoneInfo.IsPvPEnabled);

    // 4. Dynamischen State anwenden
    _weatherSystem.SetWeather(state.CurrentWeather);
    _timeSystem.SetTime(state.TimeOfDay);

    // 5. MyPlayer spawnen (falls vorhanden)
    if (state.MyPlayer != null)
    {
        _playerController.InitializeFromDto(state.MyPlayer);
    }

    // 6. Entities spawnen
    foreach (var entityDto in state.Entities)
    {
        _entityManager. SpawnFromDto(entityDto);
    }

    // 7. Auf weitere Batches warten oder Loading starten
    if (! state.HasMoreEntities)
    {
        StartAssetLoadingAndSendAck();
    }
}
```

### Beispiel Payloads

```csharp
// Erster Besuch einer Zone (mit ZoneDto)
var firstVisit = new ZoneState
{
    Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
    ZoneId = 1001,
    ZoneInfo = new ZoneDto
    {
        ZoneId = 1001,
        Name = "Elwynn Forest",
        Flags = ZoneFlags.HasRestXp,
        RecommendedMinLevel = 1,
        RecommendedMaxLevel = 10,
        MusicId = "music_elwynn",
        AmbienceId = "ambience_forest"
    },
    CurrentWeather = WeatherType.Clear,
    TimeOfDay = 14.5f,
    StateType = ZoneStateType.Initial,
    MyPlayer = playerDto,
    Entities = entityList,
    HasMoreEntities = false,
    TotalEntityCount = 45
};

// Wiederbesuch (ohne ZoneDto - gecached)
var revisit = new ZoneState
{
    Timestamp = DateTimeOffset. UtcNow.ToUnixTimeMilliseconds(),
    ZoneId = 1001,
    ZoneInfo = null,  // Bereits gecached!
    CurrentWeather = WeatherType.Rain,  // Wetter hat sich geändert
    TimeOfDay = 18.0f,
    StateType = ZoneStateType.Transfer,
    MyPlayer = playerDto,
    Entities = entityList,
    HasMoreEntities = false,
    TotalEntityCount = 52
};
```

### Chunking bei großen Zonen

Bei Zonen mit mehr als 100 Entities wird Chunking verwendet:

```
Zone mit 250 Entities:

ZoneState (102)
├── ZoneInfo:  ZoneDto (bei erstem Besuch)
├── MyPlayer: PlayerEntityDto
├── Entities: [0-99]        (100 Entities)
├── HasMoreEntities: true
└── TotalEntityCount: 250

EntityBatch (120)
├── BatchIndex: 1
├── Entities: [100-199]     (100 Entities)
└── IsLastBatch: false

EntityBatch (120)
├── BatchIndex: 2
├── Entities: [200-249]     (50 Entities)
└── IsLastBatch: true

ZoneLoadedAck (119)         (Client ist ready)
```

### Use-Cases

| Use-Case               | StateType | MyPlayer | ZoneInfo       | Entities |
| ---------------------- | --------- | -------- | -------------- | -------- |
| Login (erste Zone)     | Initial   | ✅       | ✅             | ✅       |
| Login (bekannte Zone)  | Initial   | ✅       | ❌ null        | ✅       |
| Portal (neue Zone)     | Transfer  | ✅       | ✅             | ✅       |
| Portal (bekannte Zone) | Transfer  | ✅       | ❌ null        | ✅       |
| Hearthstone            | Transfer  | ✅       | ❌ null        | ✅       |
| Tod → Ghost-Zone       | Transfer  | ✅       | ✅ (falls neu) | ✅       |
| Reconnect              | Reconnect | ✅       | ❌ null        | ✅       |
| Periodic Sync          | FullSync  | ❌ null  | ❌ null        | ✅       |

### Verwandte Messages

| Message           | ID   | Beziehung                               |
| ----------------- | ---- | --------------------------------------- |
| `GetZoneRequest`  | 117  | Request der ZoneState auslöst           |
| `ZoneLoadedAck`   | 119  | Client-Bestätigung nach ZoneState       |
| `EntityBatch`     | 120  | Weitere Entities bei Chunking           |
| `EntitySpawn`     | 1400 | Einzelne Entity spawnt später (Runtime) |
| `EntityDespawn`   | 1402 | Entity verlässt Zone (Runtime)          |
| `WeatherUpdate`   | 2600 | Dynamische Wetter-Änderung              |
| `TimeOfDayUpdate` | 2602 | Zeit-Synchronisation                    |

### Notizen

-   **Message-Größe**: ~2-10 KB (abhängig von Entity-Anzahl und ZoneDto)
-   **ZoneDto-Größe**: ~200-500 Bytes
-   **Chunking-Threshold**: 100 Entities pro Message
-   **MyPlayer enthält KEINE ServerOnly Properties** (Experience, Gold, AccountId)
-   Bei `FullSync`: Client vergleicht mit lokalem State für Desync-Detection

---

## ZoneDelta (103)

**Richtung:** 📥 Server → Client  
**Frequenz:** ⚡⚡ Extrem häufig (Jeden Tick = 40ms, gebatched)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

**Phase 2 - Chunk-Based Delta Sync System**

Hochfrequente Delta-Updates für Entity-Änderungen. Ersetzt einzelne Entity-Messages (`EntityMove`, `EntityUpdate`, etc.) durch ein **gebatchtes** Update-System.

> **Wichtig:**  
> Nur Änderungen in **sichtbaren Chunks** (3x3 Grid um Spieler) werden gesendet.  
> Siehe [Chunk-Based Sync](../../02-architecture/CHUNK_BASED_SYNC.md) für Details zum Chunk-System.
> 
> **Dirty-Tracking System:**
> ZoneDelta nutzt das automatische Dirty-Tracking System für optimale Bandbreitennutzung.  
> Siehe [Dirty-Tracking Architecture](../../02-architecture/DIRTY_TRACKING.md) für vollständige Dokumentation.

### Im Scope ✅

- **Batched Entity Updates**: Alle Änderungen in einem Message
- **Chunk-Filtered**: Nur sichtbare Chunks (3x3 = 9 Chunks)
- **Delta-Only**: Nur geänderte Properties
- **High-Frequency**: Jeden Tick (40ms) bei Änderungen
- **Bandwidth-Optimiert**: 96%+ Reduktion vs. Full Sync

### Nicht im Scope ❌

- Sofort-Events → verwende `EntityAnimation`, `EntityAggro`, `EntityEmote` (nicht gebatched!)
- Zone-Loading → verwende `ZoneState` (102)
- Request/Response → verwende `EntityInteract`, `EntityTarget` (dediziert)

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.ZoneDelta` | Ja |
| Timestamp | long | Server-Timestamp (Unix ms) | Ja |
| ZoneId | ushort | Zone-ID | Ja |
| SpawnedEntities | List<EntityDtoUnion>? | Neue Entities in sichtbaren Chunks | Nein |
| DespawnedEntityIds | List<Guid>? | Entfernte Entities (IDs) | Nein |
| PositionUpdates | List<EntityPositionDelta>? | Position-Änderungen | Nein |
| StateUpdates | List<EntityStateDelta>? | HP, State-Änderungen | Nein |
| ContextDelta | ZoneContextDelta? | Wetter, Zeit-Änderungen | Nein |

### Sub-DTOs

#### EntityPositionDelta

```csharp
[MessagePackObject]
public class EntityPositionDelta
{
    [Key(0)] public Guid EntityId { get; set; }
    [Key(1)] public float X { get; set; }
    [Key(2)] public float Y { get; set; }
    [Key(3)] public float VelocityX { get; set; }
    [Key(4)] public float VelocityY { get; set; }
    [Key(5)] public float? Rotation { get; set; }  // Optional
}
```

#### EntityStateDelta

```csharp
[MessagePackObject]
public class EntityStateDelta
{
    [Key(0)] public Guid EntityId { get; set; }
    [Key(1)] public int? CurrentHP { get; set; }      // Nur wenn geändert
    [Key(2)] public int? MaxHP { get; set; }          // Nur wenn geändert
    [Key(3)] public byte? State { get; set; }         // EntityState (Idle, Combat, etc.)
    [Key(4)] public uint? ModelId { get; set; }       // Polymorph, etc.
    [Key(5)] public int? Level { get; set; }          // Level-up
}
```

#### ZoneContextDelta

```csharp
[MessagePackObject]
public class ZoneContextDelta
{
    [Key(0)] public WeatherType? CurrentWeather { get; set; }  // Nur wenn geändert
    [Key(1)] public float? TimeOfDay { get; set; }             // Nur wenn geändert
}
```

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.ZoneDelta)]
public class ZoneDelta : ITimestampedServerMessage
{
    [Key(0)] public MessageType Type => MessageType.ZoneDelta;
    [Key(1)] public long Timestamp { get; set; }
    [Key(2)] public ushort ZoneId { get; set; }
    
    // Delta-Arrays (null wenn keine Änderungen)
    [Key(3)] public List<EntityDtoUnion>? SpawnedEntities { get; set; }
    [Key(4)] public List<Guid>? DespawnedEntityIds { get; set; }
    [Key(5)] public List<EntityPositionDelta>? PositionUpdates { get; set; }
    [Key(6)] public List<EntityStateDelta>? StateUpdates { get; set; }
    [Key(7)] public ZoneContextDelta? ContextDelta { get; set; }
}
```

### Server-Logik (Chunk-Based)

```csharp
public ZoneDelta BuildDeltaForClient(Guid clientId, long currentTick)
{
    var player = _playerService.GetPlayer(clientId);
    var visibleChunks = _clientViewService.GetClientVisibleChunks(clientId);
    
    var delta = new ZoneDelta
    {
        Timestamp = currentTick,
        ZoneId = player.CurrentZoneId
    };
    
    // Sammle Änderungen aus allen sichtbaren Chunks
    foreach (var chunk in visibleChunks)
    {
        var dirtyEntities = _chunkDirtyTracker.GetDirtyEntitiesInChunk(chunk);
        
        foreach (var entityId in dirtyEntities)
        {
            var entity = _entityManager.GetEntity(entityId);
            
            if (entity.IsNewlySpawned)
            {
                delta.SpawnedEntities ??= new List<EntityDtoUnion>();
                delta.SpawnedEntities.Add(entity.ToDto());
            }
            else if (entity.PositionChanged)
            {
                delta.PositionUpdates ??= new List<EntityPositionDelta>();
                delta.PositionUpdates.Add(new EntityPositionDelta
                {
                    EntityId = entity.PersistentId,
                    X = entity.Position.X,
                    Y = entity.Position.Y,
                    VelocityX = entity.Velocity.X,
                    VelocityY = entity.Velocity.Y,
                    Rotation = entity.Rotation
                });
            }
            
            if (entity.StateChanged)
            {
                delta.StateUpdates ??= new List<EntityStateDelta>();
                delta.StateUpdates.Add(new EntityStateDelta
                {
                    EntityId = entity.PersistentId,
                    CurrentHP = entity.HPChanged ? entity.CurrentHP : null,
                    MaxHP = entity.MaxHPChanged ? entity.MaxHP : null,
                    State = entity.StateChanged ? (byte)entity.State : null,
                    ModelId = entity.ModelChanged ? entity.ModelId : null
                });
            }
        }
    }
    
    // Zone-Context Änderungen (nicht chunk-spezifisch)
    if (_zoneManager.WeatherChanged || _zoneManager.TimeChanged)
    {
        delta.ContextDelta = new ZoneContextDelta
        {
            CurrentWeather = _zoneManager.WeatherChanged ? _zoneManager.CurrentWeather : null,
            TimeOfDay = _zoneManager.TimeChanged ? _zoneManager.TimeOfDay : null
        };
    }
    
    return delta;
}
```

### Client-Logik

```csharp
public void OnZoneDelta(ZoneDelta delta)
{
    // 1. Spawned Entities
    if (delta.SpawnedEntities != null)
    {
        foreach (var entityDto in delta.SpawnedEntities)
        {
            _entityManager.SpawnFromDto(entityDto);
        }
    }
    
    // 2. Despawned Entities
    if (delta.DespawnedEntityIds != null)
    {
        foreach (var entityId in delta.DespawnedEntityIds)
        {
            _entityManager.Despawn(entityId);
        }
    }
    
    // 3. Position Updates
    if (delta.PositionUpdates != null)
    {
        foreach (var update in delta.PositionUpdates)
        {
            var entity = _entityManager.GetEntity(update.EntityId);
            if (entity != null)
            {
                entity.UpdatePosition(
                    update.X, update.Y,
                    update.VelocityX, update.VelocityY,
                    update.Rotation
                );
            }
        }
    }
    
    // 4. State Updates
    if (delta.StateUpdates != null)
    {
        foreach (var update in delta.StateUpdates)
        {
            var entity = _entityManager.GetEntity(update.EntityId);
            if (entity != null)
            {
                if (update.CurrentHP.HasValue)
                    entity.CurrentHP = update.CurrentHP.Value;
                if (update.MaxHP.HasValue)
                    entity.MaxHP = update.MaxHP.Value;
                if (update.State.HasValue)
                    entity.SetState((EntityState)update.State.Value);
                if (update.ModelId.HasValue)
                    entity.SetModel(update.ModelId.Value);
                if (update.Level.HasValue)
                    entity.Level = update.Level.Value;
            }
        }
    }
    
    // 5. Zone Context
    if (delta.ContextDelta != null)
    {
        if (delta.ContextDelta.CurrentWeather.HasValue)
            _weatherSystem.SetWeather(delta.ContextDelta.CurrentWeather.Value);
        if (delta.ContextDelta.TimeOfDay.HasValue)
            _timeSystem.SetTime(delta.ContextDelta.TimeOfDay.Value);
    }
}
```

### Beispiel Payload

```csharp
// Typisches Delta (3 bewegende Entities, 1 HP-Update)
var delta = new ZoneDelta
{
    Timestamp = 1234567890123,
    ZoneId = 1001,
    PositionUpdates = new List<EntityPositionDelta>
    {
        new() { EntityId = entity1Id, X = 100.5f, Y = 200.3f, VelocityX = 5.0f, VelocityY = 0f },
        new() { EntityId = entity2Id, X = 150.2f, Y = 180.1f, VelocityX = 0f, VelocityY = -3.0f },
        new() { EntityId = entity3Id, X = 120.0f, Y = 210.0f, VelocityX = 2.5f, VelocityY = 2.5f }
    },
    StateUpdates = new List<EntityStateDelta>
    {
        new() { EntityId = entity4Id, CurrentHP = 450, MaxHP = 600 }
    }
};

// Spawn + Despawn Delta
var spawnDespawnDelta = new ZoneDelta
{
    Timestamp = 1234567890456,
    ZoneId = 1001,
    SpawnedEntities = new List<EntityDtoUnion>
    {
        new PlayerEntityDto { ... },  // Neuer Spieler in Sichtweite
        new NpcEntityDto { ... }       // NPC spawned
    },
    DespawnedEntityIds = new List<Guid>
    {
        oldEntity1Id,
        oldEntity2Id
    }
};

// Wetter-Änderung
var weatherDelta = new ZoneDelta
{
    Timestamp = 1234567890789,
    ZoneId = 1001,
    ContextDelta = new ZoneContextDelta
    {
        CurrentWeather = WeatherType.Rain,
        TimeOfDay = 18.5f
    }
};
```

### Sync-Strategie Tabelle

| Sync Type | Interval | Scope | Zweck |
|-----------|----------|-------|-------|
| **ZoneDelta** | Jeden Tick (40ms) | **Nur sichtbare Chunks pro Client** | Hochfrequente Änderungen |
| **ZoneState** | Alle 25 Ticks (1s) | **Nur sichtbare Chunks pro Client** | Periodischer Full-Sync (Desync-Prevention) |
| **ZoneState** | Bei Zone-Join | **Nur sichtbare Chunks** | Initiales Loading |

### Bandbreiten-Optimierung

```
ALTE STRATEGIE (Einzelne EntityMove Messages):
  - Pro bewegte Entity: ~60 bytes Overhead (Message-Header, etc.)
  - 10 bewegte Entities: 10 * 60 = 600 bytes Overhead
  - Payload: 10 * 40 = 400 bytes
  - Total: 1000 bytes

NEUE STRATEGIE (ZoneDelta Batching):
  - Ein Message-Header: ~20 bytes
  - 10 PositionDeltas: 10 * 40 = 400 bytes
  - Total: 420 bytes
  
REDUKTION: 58% weniger Overhead!
```

### Ersetzt folgende Messages

| Old Message | ID | Grund |
|-------------|-----|-------|
| `EntityMove` | 1402 | → `ZoneDelta.PositionUpdates` (Batching!) |
| `EntityUpdate` | 1403 | → `ZoneDelta.StateUpdates` (Batching!) |
| `EntityStateChange` | 1405 | → `ZoneDelta.StateUpdates` (Batching!) |

> ⚠️ Diese Messages sind in Phase 2 **DEPRECATED**.  
> Siehe [Entity Messages](14-entity.md) für Details.

### Verbleibende Event-Based Messages

Diese Messages bleiben **NICHT** gebatched:

| Message | ID | Grund |
|---------|-----|-------|
| `EntityAnimation` | 1404 | Combat-kritisch, braucht instant Feedback |
| `EntityAggro` | 1421 | Combat-kritisch |
| `EntityEmote` | 1430 | Social Feature, erwartete ~0 Latency |
| `EntityInteract` | 1410 | Request/Response-Pattern |
| `EntityTarget` | 1420 | Request/Response-Pattern |

### Verwandte Messages

| Message | ID | Beziehung |
|---------|-----|-----------|
| `ZoneState` | 102 | Periodischer Full-Sync (alle 1s) |
| `EntityAnimation` | 1404 | Nicht gebatched, sofort |
| `EntityAggro` | 1421 | Nicht gebatched, sofort |
| `EntityEmote` | 1430 | Nicht gebatched, sofort |

### Notizen

- **Message-Größe**: ~50-500 bytes (abhängig von Änderungen)
- **Frequency**: Jeden Tick (40ms) wenn Änderungen vorhanden
- **Chunk-Filtered**: Nur sichtbare 9 Chunks (3x3 Grid)
- **Batching**: Alle Änderungen in einer Message
- **Delta-Only**: Nur geänderte Properties (null-Felder bei StateUpdates)
- **Bandbreiten-Reduktion**: 96%+ vs. Full Broadcast
- **Skalierung**: Konstante Bandbreite unabhängig von Zone-Größe

### Erweiterbarkeit

Das Delta-System ist **generisch und erweiterbar**:

1. **Neue Delta-Typen hinzufügen:**
   - Erstelle neue Delta-DTO (z.B. `EntityEquipmentDelta`, `EntityBuffDelta`)
   - Folge dem Nullable-Pattern (null = nicht geändert)
   - Füge als nullable List zu `ZoneDelta` hinzu

2. **Eigene DirtyFlags definieren:**
   ```csharp
   [Flags]
   public enum CustomDirtyFlags : uint
   {
       Equipment = 1 << 10,
       Buffs = 1 << 11,
       // ... bis zu 32 Flags
   }
   ```

3. **Beispiel - Equipment Delta:**
   ```csharp
   [MessagePackObject]
   public class EntityEquipmentDelta
   {
       [Key(0)] public Guid EntityId { get; set; }
       [Key(1)] public uint? Helmet { get; set; }  // null = nicht geändert
       [Key(2)] public uint? Weapon { get; set; }
   }
   ```

Siehe [Dirty-Tracking Architecture](../../02-architecture/DIRTY_TRACKING.md) für vollständige Dokumentation und Beispiele.

### Verwandte Dokumentation

- [Dirty-Tracking System](../../02-architecture/DIRTY_TRACKING.md) - Automatisches Change-Tracking
- [Chunk-Based Sync System](../../02-architecture/CHUNK_BASED_SYNC.md) - Vollständige Dokumentation
- [Game Loop - Output Phase](../../02-architecture/GAME_LOOP.md) - Delta-Building Logik
- [Entity Messages](14-entity.md) - Deprecated Messages

---

## PlayerJoinedZone (104)

**Richtung:** 📡 Broadcast (Server → All Clients in Zone)  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Broadcast an alle Spieler in der Zone wenn ein neuer Spieler spawnt. Der neue Spieler selbst erhält `ZoneState` (102), nicht diese Message.

### Payload

| Feld   | Typ             | Beschreibung                      | Pflicht |
| ------ | --------------- | --------------------------------- | ------- |
| Type   | MessageType     | `MessageType.PlayerJoinedZone`    | Ja      |
| Player | PlayerEntityDto | Komplette sichtbare Spieler-Daten | Ja      |

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.PlayerJoinedZone)]
public class PlayerJoinedZone :  IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.PlayerJoinedZone;
    [Key(1)] public PlayerEntityDto Player { get; set; } = null!;
}
```

### Beispiel Payload

```csharp
var playerJoined = new PlayerJoinedZone
{
    Player = newPlayer.ToDto()
};

// Broadcast an alle AUSSER dem neuen Spieler
zone.BroadcastExcept(playerJoined, newPlayer. ConnectionId);
```

### Verwandte Messages

| Message          | ID   | Beziehung                                  |
| ---------------- | ---- | ------------------------------------------ |
| `ZoneState`      | 102  | Was der neue Spieler selbst erhält         |
| `PlayerLeftZone` | 104  | Gegenstück beim Verlassen                  |
| `EntitySpawn`    | 1400 | Generische Entity-Spawn Message (für NPCs) |

### Notizen

-   Wird **NUR** an bereits anwesende Spieler gesendet
-   Der joinierende Spieler erhält `ZoneState` (102) mit allen Entities
-   Client fügt Spieler zur lokalen Entity-Liste hinzu
-   Enthält komplettes `PlayerEntityDto` für sofortiges Rendering

---

## PlayerLeftZone (105)

**Richtung:** 📡 Broadcast (Server → All Clients in Zone)  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Broadcast an alle Spieler wenn ein Spieler die Zone verlässt. Der **Server** setzt den Reason basierend auf dem Ereignis.

### Payload

| Feld     | Typ              | Beschreibung                 | Pflicht |
| -------- | ---------------- | ---------------------------- | ------- |
| Type     | MessageType      | `MessageType.PlayerLeftZone` | Ja      |
| PlayerId | Guid             | PersistentId des Spielers    | Ja      |
| Reason   | PlayerLeftReason | Grund (vom Server gesetzt)   | Ja      |

### Enum

```csharp
public enum PlayerLeftReason : byte
{
    Logout = 1,       // Spieler hat sich ausgeloggt
    Transfer = 2,     // Spieler wechselt Zone (Portal, Teleport, etc.)
    Disconnect = 3,   // Server hat Verbindungsabbruch erkannt
    Death = 4,        // Spieler ist gestorben → Ghost-Zone
    Kicked = 5,       // Von Admin/GM gekickt
    Banned = 6        // Account wurde gebannt
}
```

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType. PlayerLeftZone)]
public class PlayerLeftZone : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.PlayerLeftZone;
    [Key(1)] public Guid PlayerId { get; set; }
    [Key(2)] public PlayerLeftReason Reason { get; set; }
}
```

### Server-Logik

```csharp
// Bei Client-Logout
public void HandleLeaveZone(ClientConnection conn, LeaveZone msg)
{
    var player = GetPlayer(conn);

    BroadcastToZone(player. ZoneId, new PlayerLeftZone
    {
        PlayerId = player.PersistentId,
        Reason = PlayerLeftReason.Logout
    });

    SavePlayerState(player);
    CloseConnection(conn);
}

// Bei Heartbeat-Timeout (Disconnect)
public void HandleHeartbeatTimeout(ClientConnection conn)
{
    var player = GetPlayer(conn);

    BroadcastToZone(player.ZoneId, new PlayerLeftZone
    {
        PlayerId = player.PersistentId,
        Reason = PlayerLeftReason.Disconnect
    });

    // 30s Reconnect-Window
    StartReconnectTimer(player);
}

// Bei Tod
public void HandlePlayerDeath(PlayerEntity player, ICombatEntity killer)
{
    BroadcastToZone(player.ZoneId, new PlayerLeftZone
    {
        PlayerId = player.PersistentId,
        Reason = PlayerLeftReason.Death
    });

    // Transfer zu Ghost-Zone
    TransferToGhostZone(player);
}
```

### Client-Verhalten

| Reason       | Client-Aktion                                   |
| ------------ | ----------------------------------------------- |
| `Logout`     | Entity sofort entfernen                         |
| `Transfer`   | Entity sofort entfernen                         |
| `Disconnect` | Entity als "Geist" markieren (transparent, 30s) |
| `Death`      | Todes-Animation abspielen, dann entfernen       |
| `Kicked`     | Entity sofort entfernen                         |
| `Banned`     | Entity sofort entfernen                         |

### Beispiel Payloads

```csharp
// Logout
var logout = new PlayerLeftZone
{
    PlayerId = player.PersistentId,
    Reason = PlayerLeftReason.Logout
};

// Disconnect (Server erkannt)
var disconnect = new PlayerLeftZone
{
    PlayerId = player.PersistentId,
    Reason = PlayerLeftReason.Disconnect
};

// Tod
var death = new PlayerLeftZone
{
    PlayerId = player.PersistentId,
    Reason = PlayerLeftReason.Death
};
```

### Verwandte Messages

| Message            | ID   | Beziehung                         |
| ------------------ | ---- | --------------------------------- |
| `LeaveZone`        | 101  | Client-Request (Logout/Exit)      |
| `PlayerJoinedZone` | 103  | Gegenstück beim Betreten          |
| `DeathEvent`       | 303  | Combat-Message bei Tod            |
| `EntityDespawn`    | 1402 | Generische Entity-Despawn Message |

---

## ZoneTransferRequest (106)

**Richtung:** 📤 Client → Server  
**Frequenz:** Mittel  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Client möchte zu einer anderen Zone wechseln. Wird für **alle** Arten von Zone-Wechseln verwendet:

-   Portal benutzen
-   Teleport-Spell
-   Hearthstone
-   Dungeon-Eingang betreten
-   Flugmeister-Route beenden

> **Hinweis:** Für Logout verwende `LeaveZone` (101), nicht `ZoneTransferRequest`!

### Payload

| Feld           | Typ          | Beschreibung                      | Pflicht |
| -------------- | ------------ | --------------------------------- | ------- |
| Type           | MessageType  | `MessageType.ZoneTransferRequest` | Ja      |
| TargetZoneId   | ushort       | Ziel-Zone-ID                      | Ja      |
| TransferType   | TransferType | Art des Transfers                 | Ja      |
| TargetPosition | Position?    | Ziel-Position (falls bekannt)     | Nein    |

### Enum

```csharp
public enum TransferType : byte
{
    Portal = 1,           // Bestehendes Portal benutzen
    Teleport = 2,         // Teleport-Spell (Mage)
    Hearthstone = 3,      // Hearthstone benutzen
    DungeonEntrance = 4,  // Dungeon/Raid betreten
    DungeonExit = 5,      // Dungeon/Raid verlassen
    FlightPath = 6,       // Flugmeister-Route beendet
    SpellTeleport = 7,    // Anderer Teleport-Spell
    SummonAccept = 8,     // Beschwörung akzeptiert
    GraveyardTeleport = 9 // Vom Geist zum Friedhof
}
```

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType. ZoneTransferRequest)]
public class ZoneTransferRequest : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.ZoneTransferRequest;
    [Key(1)] public ushort TargetZoneId { get; set; }
    [Key(2)] public TransferType TransferType { get; set; }
    [Key(3)] public Position? TargetPosition { get; set; }
}
```

### Beispiel Payloads

```csharp
// Portal benutzen
var portalTransfer = new ZoneTransferRequest
{
    TargetZoneId = 2001,
    TransferType = TransferType.Portal,
    TargetPosition = new Position(50.0f, 50.0f, 0f, 2001)
};

// Hearthstone
var hearthstone = new ZoneTransferRequest
{
    TargetZoneId = player.HearthstoneZoneId,
    TransferType = TransferType.Hearthstone,
    TargetPosition = player.HearthstonePosition
};

// Dungeon betreten
var dungeon = new ZoneTransferRequest
{
    TargetZoneId = 5001,
    TransferType = TransferType.DungeonEntrance
};
```

### Server-Validierung

| Check                  | Error Code        | Beschreibung                |
| ---------------------- | ----------------- | --------------------------- |
| Zone existiert         | `ZONE_NOT_FOUND`  | Ziel-Zone-ID ungültig       |
| Zone nicht gesperrt    | `ZONE_LOCKED`     | Maintenance                 |
| Level-Requirement      | `LEVEL_TOO_LOW`   | Min-Level nicht erreicht    |
| Quest-Requirement      | `QUEST_REQUIRED`  | Quest nicht abgeschlossen   |
| Nicht im Combat        | `IN_COMBAT`       | Combat muss beendet sein    |
| Cooldown (Hearthstone) | `COOLDOWN_ACTIVE` | Hearthstone auf Cooldown    |
| Instanz voll           | `INSTANCE_FULL`   | Max. Spielerzahl erreicht   |
| Gruppe erforderlich    | `NOT_IN_PARTY`    | Gruppen-Dungeon ohne Gruppe |
| Falsche Fraktion       | `WRONG_FACTION`   | Fraktions-Zone              |

### Verwandte Messages

| Message                | ID  | Beziehung                  |
| ---------------------- | --- | -------------------------- |
| `ZoneTransferResponse` | 106 | Antwort auf diesen Request |
| `ZoneState`            | 102 | Folgt bei Erfolg           |
| `PlayerLeftZone`       | 104 | Broadcast an alte Zone     |

---

## ZoneTransferResponse (107)

**Richtung:** 📥 Server → Client  
**Frequenz:** Mittel  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Antwort auf `ZoneTransferRequest`. Bei Erfolg folgt danach `ZoneState` (102) für die neue Zone.

### Payload

| Feld         | Typ         | Beschreibung                       | Pflicht |
| ------------ | ----------- | ---------------------------------- | ------- |
| Type         | MessageType | `MessageType.ZoneTransferResponse` | Ja      |
| Success      | bool        | Transfer erlaubt?                  | Ja      |
| ErrorCode    | string?     | Fehlercode bei Failure             | Nein    |
| ErrorMessage | string?     | Benutzerfreundliche Fehlermeldung  | Nein    |

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.ZoneTransferResponse)]
public class ZoneTransferResponse : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.ZoneTransferResponse;
    [Key(1)] public bool Success { get; set; }
    [Key(2)] public string?  ErrorCode { get; set; }
    [Key(3)] public string? ErrorMessage { get; set; }
}
```

### Beispiel Payloads

```csharp
// Erfolg
var success = new ZoneTransferResponse
{
    Success = true
};

// Fehler - Level zu niedrig
var levelError = new ZoneTransferResponse
{
    Success = false,
    ErrorCode = "LEVEL_TOO_LOW",
    ErrorMessage = "Du musst mindestens Level 20 sein um diese Zone zu betreten."
};

// Fehler - Im Combat
var combatError = new ZoneTransferResponse
{
    Success = false,
    ErrorCode = "IN_COMBAT",
    ErrorMessage = "Du kannst nicht teleportieren während du im Kampf bist."
};
```

### Flow bei Erfolg

```
ZoneTransferRequest (105)
         ↓
ZoneTransferResponse (106) Success=true
         ↓
PlayerLeftZone (104) broadcast an alte Zone
         ↓
ZoneState (102) für neue Zone
         ↓
[Client lädt Assets]
         ↓
ZoneLoadedAck (119)
```

### Error Codes

| Code              | Beschreibung                         |
| ----------------- | ------------------------------------ |
| `ZONE_NOT_FOUND`  | Ziel-Zone existiert nicht            |
| `ZONE_LOCKED`     | Zone ist für Wartung gesperrt        |
| `LEVEL_TOO_LOW`   | Level-Requirement nicht erfüllt      |
| `LEVEL_TOO_HIGH`  | Level-Cap für Zone überschritten     |
| `QUEST_REQUIRED`  | Quest muss erst abgeschlossen werden |
| `IN_COMBAT`       | Nicht möglich während Combat         |
| `COOLDOWN_ACTIVE` | Hearthstone/Teleport auf Cooldown    |
| `INSTANCE_FULL`   | Instanz hat maximale Spielerzahl     |
| `NOT_IN_PARTY`    | Gruppen-Instanz erfordert Gruppe     |
| `WRONG_FACTION`   | Zone für andere Fraktion             |
| `INVALID_TARGET`  | Ungültige Ziel-Position              |

---

## ZoneDiscovered (108)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Benachrichtigung dass der Spieler eine neue Zone zum ersten Mal betreten hat. Enthält die vollständigen Zone-Daten als `ZoneDto` zum Cachen.

### Payload

| Feld    | Typ         | Beschreibung                 | Pflicht |
| ------- | ----------- | ---------------------------- | ------- |
| Type    | MessageType | `MessageType.ZoneDiscovered` | Ja      |
| Zone    | ZoneDto     | Vollständige Zone-Daten      | Ja      |
| XpBonus | int         | XP-Belohnung (0 wenn keine)  | Ja      |

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.ZoneDiscovered)]
public class ZoneDiscovered : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.ZoneDiscovered;
    [Key(1)] public ZoneDto Zone { get; set; } = null!;
    [Key(2)] public int XpBonus { get; set; }
}
```

### Beispiel Payload

```csharp
var discovered = new ZoneDiscovered
{
    Zone = zone.ToDto(),
    XpBonus = 150
};
```

### Client-Verhalten

```csharp
public void OnZoneDiscovered(ZoneDiscovered msg)
{
    // 1. ZoneDto cachen
    _zoneCache.CacheZone(msg.Zone);

    // 2. UI anzeigen
    _uiManager.ShowZoneDiscovered(msg.Zone. Name);

    // 3. XP-Gewinn animieren (falls > 0)
    if (msg.XpBonus > 0)
    {
        _uiManager.ShowXpGain(msg.XpBonus);
    }

    // 4. Sound-Effekt
    _audioManager.PlaySound("zone_discovered");

    // 5. Weltkarte aktualisieren
    _worldMap. MarkAsDiscovered(msg.Zone.ZoneId);
}
```

### Verwandte Messages

| Message               | ID   | Beziehung                          |
| --------------------- | ---- | ---------------------------------- |
| `AchievementUnlocked` | 1900 | "Explorer" Achievement             |
| `XpGain`              | 601  | XP-Bonus Anzeige                   |
| `ZoneState`           | 102  | Kommt zusammen bei erstem Betreten |

---

## ZoneListRequest (109)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Client fordert Liste aller Zonen an (für Weltkarte, Fast-Travel UI).

### Payload

| Feld                | Typ         | Beschreibung                  | Pflicht |
| ------------------- | ----------- | ----------------------------- | ------- |
| Type                | MessageType | `MessageType.ZoneListRequest` | Ja      |
| IncludeUndiscovered | bool        | Auch nicht-entdeckte Zonen?   | Ja      |

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.ZoneListRequest)]
public class ZoneListRequest : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.ZoneListRequest;
    [Key(1)] public bool IncludeUndiscovered { get; set; }
}
```

### Beispiel Payload

```csharp
// Nur entdeckte Zonen (für Fast-Travel)
var discoveredOnly = new ZoneListRequest
{
    IncludeUndiscovered = false
};

// Alle Zonen (für Weltkarte)
var allZones = new ZoneListRequest
{
    IncludeUndiscovered = true
};
```

---

## ZoneListResponse (110)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Liste aller (entdeckten) Zonen für Weltkarte und Fast-Travel. Enthält `ZoneListItemDto` mit statischen und spieler-spezifischen Daten.

### Payload

| Feld  | Typ                     | Beschreibung                   | Pflicht |
| ----- | ----------------------- | ------------------------------ | ------- |
| Type  | MessageType             | `MessageType.ZoneListResponse` | Ja      |
| Zones | List\<ZoneListItemDto\> | Zone-Informationen             | Ja      |

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.ZoneListResponse)]
public class ZoneListResponse : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.ZoneListResponse;
    [Key(1)] public List<ZoneListItemDto> Zones { get; set; } = new();
}
```

### Server-Logik

```csharp
public ZoneListResponse HandleZoneListRequest(PlayerEntity player, ZoneListRequest request)
{
    var zones = _zoneManager.GetAllZones();

    if (! request.IncludeUndiscovered)
    {
        zones = zones.Where(z => player. DiscoveredZones.Contains(z.Id));
    }

    return new ZoneListResponse
    {
        Zones = zones.Select(z => ZoneListItemDto.Create(z, player)).ToList()
    };
}
```

### Client-Logik

```csharp
public void OnZoneListResponse(ZoneListResponse response)
{
    foreach (var item in response.Zones)
    {
        // ZoneDtos cachen
        _zoneCache.CacheZone(item. Zone);

        // Weltkarte aktualisieren
        _worldMap.UpdateZone(item);
    }
}
```

### Beispiel Payload

```csharp
var response = new ZoneListResponse
{
    Zones = new List<ZoneListItemDto>
    {
        new ZoneListItemDto
        {
            Zone = new ZoneDto
            {
                ZoneId = 1001,
                Name = "Elwynn Forest",
                Flags = ZoneFlags.HasRestXp,
                RecommendedMinLevel = 1,
                RecommendedMaxLevel = 10
            },
            IsDiscovered = true,
            HasFlightPath = true,
            FlightPathPosition = new Position(100f, 200f, 0f, 1001),
            CompletedQuestCount = 12,
            TotalQuestCount = 15
        },
        new ZoneListItemDto
        {
            Zone = new ZoneDto
            {
                ZoneId = 1005,
                Name = "Darkwood Forest",
                Flags = ZoneFlags.PvpEnabled,
                RecommendedMinLevel = 15,
                RecommendedMaxLevel = 25
            },
            IsDiscovered = false,
            HasFlightPath = false,
            CompletedQuestCount = 0,
            TotalQuestCount = 20
        }
    }
};
```

---

## GetZoneRequest (117)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Client fordert Zone-Daten an. Wird nach `CharacterSelectResponse` gesendet um die Spawn-Zone zu laden.

> **Hinweis:** Server antwortet direkt mit `ZoneState` (102), nicht mit einer separaten Response-Message.

### Payload

| Feld   | Typ         | Beschreibung                    | Pflicht |
| ------ | ----------- | ------------------------------- | ------- |
| Type   | MessageType | `MessageType.GetZoneRequest`    | Ja      |
| ZoneId | ushort      | Zone-ID die geladen werden soll | Ja      |

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.GetZoneRequest)]
public class GetZoneRequest : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.GetZoneRequest;
    [Key(1)] public ushort ZoneId { get; set; }
}
```

### Beispiel Payload

```csharp
// Nach CharacterSelectResponse
var request = new GetZoneRequest
{
    ZoneId = characterSelectResponse.SpawnZoneId
};
```

### Server-Antwort

Server antwortet mit `ZoneState` (102):

```csharp
public void HandleGetZoneRequest(ClientConnection conn, GetZoneRequest request)
{
    var player = GetPlayer(conn);
    var zone = _zoneManager.GetZone(request.ZoneId);

    if (zone == null)
    {
        SendError(conn, "ZONE_NOT_FOUND");
        return;
    }

    // ZoneState mit ZoneDto (falls erster Besuch) senden
    var zoneState = CreateZoneState(player, zone, ZoneStateType.Initial);
    Send(conn, zoneState);

    // Warte auf ZoneLoadedAck
    player.WaitingForZoneAckSince = DateTime. UtcNow;
}
```

### Verwandte Messages

| Message                   | ID  | Beziehung                |
| ------------------------- | --- | ------------------------ |
| `CharacterSelectResponse` | 21  | Enthält SpawnZoneId      |
| `ZoneState`               | 102 | Direkte Antwort          |
| `ZoneLoadedAck`           | 119 | Client bestätigt Loading |

---

## ZoneLoadedAck (119)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten (einmal pro Zone-Load)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Client bestätigt dass Zone-Assets geladen und `ZoneState` verarbeitet wurde. Server startet erst nach diesem Ack die hochfrequenten Updates.

### Warum diese Message?

| Ohne ZoneLoadedAck                             | Mit ZoneLoadedAck        |
| ---------------------------------------------- | ------------------------ |
| Server sendet 25Hz Updates während Client lädt | Server wartet auf Client |
| Bandbreite verschwendet                        | Bandbreite optimal       |
| Client-Buffer wächst unkontrolliert            | Kein unnötiger Buffer    |
| Server weiß nicht ob Client ready              | Klarer Handshake         |

### Payload

| Feld       | Typ         | Beschreibung                  | Pflicht |
| ---------- | ----------- | ----------------------------- | ------- |
| Type       | MessageType | `MessageType.ZoneLoadedAck`   | Ja      |
| ZoneId     | ushort      | Zone-ID zur Validierung       | Ja      |
| LoadTimeMs | int?        | Loading-Dauer in ms (Metrics) | Nein    |

### Code-Beispiel

[MessagePackObject]
[NetworkMessage(MessageType.ZoneLoadedAck)]
public class ZoneLoadedAck : IClientMessage
{
[Key(0)] public MessageType Type => MessageType. ZoneLoadedAck;
[Key(1)] public ushort ZoneId { get; set; }
[Key(2)] public int? LoadTimeMs { get; set; }
}

````

### Client-Logik

```csharp
public async Task LoadZoneAsync(ZoneState zoneState)
{
    var stopwatch = Stopwatch.StartNew();

    // 1. ZoneDto cachen (falls vorhanden)
    if (zoneState.ZoneInfo != null)
    {
        _zoneCache.CacheZone(zoneState.ZoneInfo);
    }

    // 2. Zone-Infos aus Cache holen
    var zoneInfo = _zoneCache.GetZone(zoneState.ZoneId);
    if (zoneInfo == null)
    {
        _log.Error("ZoneDto not found for zone {ZoneId}!", zoneState.ZoneId);
        RequestDisconnect("Missing zone data");
        return;
    }

    // 3. Assets laden (Texturen, Models, etc.)
    await _assetLoader.LoadZoneAssetsAsync(zoneInfo);

    // 4. Audio starten
    _audioManager.PlayMusic(zoneInfo.MusicId);
    _audioManager.PlayAmbience(zoneInfo.AmbienceId);

    // 5. Minimap initialisieren
    _minimapManager.SetBounds(zoneInfo. Bounds);

    // 6. Dynamischen State anwenden
    _weatherSystem.SetWeather(zoneState.CurrentWeather);
    _timeSystem.SetTime(zoneState.TimeOfDay);

    // 7. MyPlayer spawnen (falls vorhanden)
    if (zoneState.MyPlayer != null)
    {
        _playerController.InitializeFromDto(zoneState.MyPlayer);
    }

    // 8. Entities spawnen
    foreach (var entityDto in zoneState.Entities)
    {
        _entityManager.SpawnFromDto(entityDto);
    }

    // 9. Gebufferte Messages abarbeiten
    _messageBuffer.ProcessAll();

    stopwatch.Stop();

    // 10. Server informieren - JETZT bereit für Updates!
    _networkClient.Send(new ZoneLoadedAck
    {
        ZoneId = zoneState. ZoneId,
        LoadTimeMs = (int)stopwatch.ElapsedMilliseconds
    });
}
````

### Server-Logik

```csharp
public void HandleZoneLoadedAck(ClientConnection conn, ZoneLoadedAck ack)
{
    var player = GetPlayer(conn);

    // Validierung
    if (player.CurrentZoneId != ack.ZoneId)
    {
        _log. Warn("ZoneLoadedAck for wrong zone:  expected {Expected}, got {Got}",
            player.CurrentZoneId, ack.ZoneId);
        return;
    }

    // Spieler ist ready!
    player.IsZoneReady = true;
    player.WaitingForZoneAckSince = null;

    // Metrics
    if (ack.LoadTimeMs. HasValue)
    {
        _metrics.RecordZoneLoadTime(ack.ZoneId, ack.LoadTimeMs.Value);
    }

    _log.Info("Player {Name} ready in zone {ZoneId} (load time: {Ms}ms)",
        player.DisplayName, ack.ZoneId, ack.LoadTimeMs ??  -1);

    // Jetzt können hochfrequente Updates gesendet werden
    // (PositionBroadcast, EntityUpdates, etc.)
}
```

### Timeout-Handling

Server wartet maximal 30 Sekunden auf `ZoneLoadedAck`:

```csharp
// Im Server-Tick
public void CheckZoneLoadTimeouts()
{
    var now = DateTime.UtcNow;

    foreach (var player in _players.Where(p => ! p.IsZoneReady))
    {
        if (player.WaitingForZoneAckSince?. AddSeconds(30) < now)
        {
            _log.Warn("Player {Name} zone loading timeout after 30s",
                player.DisplayName);
            DisconnectPlayer(player, DisconnectReason.LoadingTimeout);
        }
    }
}
```

### Verwandte Messages

| Message             | ID  | Beziehung                  |
| ------------------- | --- | -------------------------- |
| `ZoneState`         | 102 | Vorherige Message          |
| `EntityBatch`       | 120 | Muss auch verarbeitet sein |
| `PositionBroadcast` | 201 | Startet nach Ack           |

---

## EntityBatch (120)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten (nur bei großen Zonen)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Enthält weitere Entities wenn eine Zone mehr als 100 Entities hat. Folgt auf `ZoneState` (102) wenn `HasMoreEntities = true`.

### Payload

| Feld        | Typ                | Beschreibung                | Pflicht |
| ----------- | ------------------ | --------------------------- | ------- |
| Type        | MessageType        | `MessageType.EntityBatch`   | Ja      |
| ZoneId      | ushort             | Zone-ID zur Validierung     | Ja      |
| BatchIndex  | int                | Batch-Nummer (1, 2, 3, ...) | Ja      |
| Entities    | List\<EntityDtoUnion\> | Weitere Entities (max 100)  | Ja      |
| IsLastBatch | bool               | Ist dies der letzte Batch?  | Ja      |

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.EntityBatch)]
public class EntityBatch : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.EntityBatch;
    [Key(1)] public ushort ZoneId { get; set; }
    [Key(2)] public int BatchIndex { get; set; }
    [Key(3)] public List<EntityDtoUnion> Entities { get; set; } = new();
    [Key(4)] public bool IsLastBatch { get; set; }
}
```

### Chunking-Flow Beispiel

```
Zone mit 250 Entities:

1. ZoneState (102)
   ├── ZoneInfo:  ZoneDto (bei erstem Besuch)
   ├── MyPlayer: PlayerEntityDto
   ├── Entities: [Entity 0-99]     (100 Entities)
   ├── HasMoreEntities: true
   └── TotalEntityCount: 250

2. EntityBatch (120)
   ├── BatchIndex: 1
   ├── Entities: [Entity 100-199]  (100 Entities)
   └── IsLastBatch: false

3. EntityBatch (120)
   ├── BatchIndex:  2
   ├── Entities: [Entity 200-249]  (50 Entities)
   └── IsLastBatch: true

4. ZoneLoadedAck (119)
   └── Client ist ready
```

### Server-Logik

```csharp
public void SendZoneStateWithChunking(ClientConnection conn, PlayerEntity player, Zone zone)
{
    const int CHUNK_SIZE = 100;

    var allEntities = zone.GetVisibleEntities(player).ToList();
    var firstChunk = allEntities.Take(CHUNK_SIZE).ToUnionDtoList();
    var remainingEntities = allEntities.Skip(CHUNK_SIZE).ToList();

    // Prüfen ob erster Besuch
    bool firstVisit = ! player. VisitedZones.Contains(zone.Id);
    if (firstVisit)
    {
        player.VisitedZones.Add(zone.Id);
    }

    // 1. ZoneState mit erstem Chunk
    var zoneState = new ZoneState
    {
        Timestamp = DateTimeOffset.UtcNow. ToUnixTimeMilliseconds(),
        ZoneId = zone.Id,
        ZoneInfo = firstVisit ? zone.ToDto() : null,
        CurrentWeather = zone.CurrentWeather,
        TimeOfDay = zone.TimeOfDay,
        StateType = ZoneStateType.Initial,
        MyPlayer = player.ToDto(),
        Entities = firstChunk,
        HasMoreEntities = remainingEntities.Any(),
        TotalEntityCount = allEntities.Count
    };
    Send(conn, zoneState);

    // 2. Weitere Batches
    int batchIndex = 1;
    while (remainingEntities.Any())
    {
        var batch = remainingEntities.Take(CHUNK_SIZE).ToList();
        remainingEntities = remainingEntities.Skip(CHUNK_SIZE).ToList();

        var entityBatch = new EntityBatch
        {
            ZoneId = zone.Id,
            BatchIndex = batchIndex++,
            Entities = batch.ToUnionDtoList(),
            IsLastBatch = ! remainingEntities.Any()
        };
        Send(conn, entityBatch);
    }

    // Warte auf ZoneLoadedAck
    player.WaitingForZoneAckSince = DateTime. UtcNow;
}
```

### Client-Handling

```csharp
private int _expectedBatches;
private int _receivedBatches;
private List<EntityDtoUnion> _pendingEntities = new();
private ZoneState?  _pendingZoneState;

public void OnZoneState(ZoneState state)
{
    _pendingZoneState = state;
    _pendingEntities.Clear();
    _pendingEntities.AddRange(state.Entities);

    if (! state.HasMoreEntities)
    {
        // Keine weiteren Batches, direkt laden
        _ = ProcessZoneLoadAsync();
    }
    else
    {
        // Auf weitere Batches warten
        _expectedBatches = (state.TotalEntityCount - 100 + 99) / 100; // Ceiling division
        _receivedBatches = 0;
    }
}

public void OnEntityBatch(EntityBatch batch)
{
    // Validierung
    if (_pendingZoneState == null || batch.ZoneId != _pendingZoneState.ZoneId)
    {
        _log. Warn("EntityBatch for wrong zone or no pending ZoneState");
        return;
    }

    // Entities hinzufügen
    _pendingEntities.AddRange(batch.Entities);
    _receivedBatches++;

    if (batch.IsLastBatch)
    {
        _ = ProcessZoneLoadAsync();
    }
}

private async Task ProcessZoneLoadAsync()
{
    if (_pendingZoneState == null) return;

    var stopwatch = Stopwatch.StartNew();

    // 1. ZoneDto cachen
    if (_pendingZoneState.ZoneInfo != null)
    {
        _zoneCache.CacheZone(_pendingZoneState.ZoneInfo);
    }

    // 2. Zone-Infos holen
    var zoneInfo = _zoneCache.GetZone(_pendingZoneState.ZoneId);
    if (zoneInfo == null)
    {
        _log.Error("ZoneDto not found!");
        return;
    }

    // 3. Assets laden
    await _assetLoader.LoadZoneAssetsAsync(zoneInfo);

    // 4. Audio/Visuals
    _audioManager.PlayMusic(zoneInfo.MusicId);
    _weatherSystem.SetWeather(_pendingZoneState.CurrentWeather);
    _timeSystem.SetTime(_pendingZoneState.TimeOfDay);

    // 5. MyPlayer spawnen
    if (_pendingZoneState.MyPlayer != null)
    {
        _playerController.InitializeFromDto(_pendingZoneState.MyPlayer);
    }

    // 6. ALLE Entities spawnen (inkl. Batches)
    foreach (var entityDto in _pendingEntities)
    {
        _entityManager.SpawnFromDto(entityDto);
    }

    // 7. Buffer abarbeiten
    _messageBuffer. ProcessAll();

    stopwatch.Stop();

    // 8. Cleanup
    var zoneId = _pendingZoneState. ZoneId;
    _pendingZoneState = null;
    _pendingEntities.Clear();

    // 9. Server informieren
    _networkClient.Send(new ZoneLoadedAck
    {
        ZoneId = zoneId,
        LoadTimeMs = (int)stopwatch.ElapsedMilliseconds
    });
}
```

### Verwandte Messages

| Message         | ID   | Beziehung                    |
| --------------- | ---- | ---------------------------- |
| `ZoneState`     | 102  | Erster Teil der Daten        |
| `ZoneLoadedAck` | 119  | Nach letztem Batch           |
| `EntitySpawn`   | 1400 | Für Runtime-Spawns (einzeln) |

---

# Phase 2 Messages

Die folgenden Messages sind für Phase 2 geplant und noch nicht implementiert.

---

## ShardTransfer (111)

**Status:** 🔮 Phase 2  
**Beschreibung:** Transfer zu anderem Shard (Zone-Instance) für Load-Balancing bei überfüllten Zonen.

### Geplante Funktionalität

-   Automatischer Transfer bei Zone-Überlastung
-   Manueller Shard-Wechsel zu Freunden
-   Seamless Transition ohne Re-Login

---

## ShardListRequest (112)

**Status:** 🔮 Phase 2  
**Beschreibung:** Liste aller verfügbaren Shards für aktuelle Zone anfragen.

### Geplante Funktionalität

-   Shard-Auslastung anzeigen
-   Freunde auf anderen Shards finden
-   Bevorzugten Shard auswählen

---

## ShardListResponse (113)

**Status:** 🔮 Phase 2  
**Beschreibung:** Antwort mit Shard-Informationen (Population, Status, Freunde).

---

## SubZoneEnter (114)

**Status:** 🔮 Phase 2  
**Beschreibung:** Spieler betritt Sub-Zone (z.B. "Goldshire" innerhalb von "Elwynn Forest").

### Geplante Funktionalität

-   UI zeigt Sub-Zone-Name
-   Musik/Ambiente wechselt
-   Lokale Quests aktivieren

---

## SubZoneLeave (115)

**Status:** 🔮 Phase 2  
**Beschreibung:** Spieler verlässt Sub-Zone.

---

## ZonePhaseChange (116)

**Status:** 🔮 Phase 2  
**Beschreibung:** Zone ändert Phase basierend auf Quest-Fortschritt (Phasing-System).

### Geplante Funktionalität

-   Unterschiedliche Zone-Zustände pro Spieler
-   Quest-Progress beeinflusst Zone-Aussehen
-   Spieler in unterschiedlichen Phasen sehen sich nicht

---

# Obsolete Messages

Die folgenden Messages wurden durch den neuen Zone Loading Flow ersetzt.

---

## JoinZone (100) [OBSOLET]

> ⚠️ **OBSOLET** - Ersetzt durch `ZoneState. MyPlayer`
>
> Die Funktionalität wurde in `ZoneState` (102) integriert. Das Feld `MyPlayer` enthält jetzt alle Daten die früher in `JoinZone` gesendet wurden.

**MessageType ID:** 100 - Kann für zukünftige Zwecke wiederverwendet werden.

---

## ZoneLoadingProgress (107) [OBSOLET]

> ⚠️ **OBSOLET** - Nicht mehr benötigt
>
> Der Client lädt Zone-Assets lokal und sendet `ZoneLoadedAck` (119) wenn fertig. Server muss keinen Loading-Progress mehr senden.

**MessageType ID:** 107 - Kann für zukünftige Zwecke wiederverwendet werden.

---

## GetZoneResponse (118) [OBSOLET]

> ⚠️ **OBSOLET** - Ersetzt durch direkte `ZoneState` Antwort
>
> Server antwortet auf `GetZoneRequest` (117) direkt mit `ZoneState` (102). Ein separater Response-Wrapper ist nicht mehr nötig.

**MessageType ID:** 118 - Kann für zukünftige Zwecke wiederverwendet werden.

---

# Anhang

## MessageType Enum Updates

Die folgenden Änderungen müssen im `MessageType` Enum vorgenommen werden:

```csharp
// ZONE EVENTS (0100-0199)
// JoinZone = 100,              // OBSOLET - Reserved
LeaveZone = 101,
ZoneState = 102,
PlayerJoinedZone = 103,
PlayerLeftZone = 104,
ZoneTransferRequest = 105,
ZoneTransferResponse = 106,
// ZoneLoadingProgress = 107,   // OBSOLET - Reserved
ZoneDiscovered = 108,
ZoneListRequest = 109,
ZoneListResponse = 110,
ShardTransfer = 111,            // Phase 2
ShardListRequest = 112,         // Phase 2
ShardListResponse = 113,        // Phase 2
SubZoneEnter = 114,             // Phase 2
SubZoneLeave = 115,             // Phase 2
ZonePhaseChange = 116,          // Phase 2
GetZoneRequest = 117,           // NEU - muss hinzugefügt werden!
// GetZoneResponse = 118,       // OBSOLET - Reserved
ZoneLoadedAck = 119,            // NEU - muss hinzugefügt werden!
EntityBatch = 120,              // NEU - muss hinzugefügt werden!
```

## Neue Enums

Die folgenden Enums müssen erstellt werden:

```csharp
// Mmo. Shared/Zones/Enums/LeaveReason.cs
public enum LeaveReason :  byte
{
    Logout = 1,
    ExitGame = 2,
    CharacterSwitch = 3
}

// Mmo. Shared/Zones/Enums/PlayerLeftReason.cs
public enum PlayerLeftReason : byte
{
    Logout = 1,
    Transfer = 2,
    Disconnect = 3,
    Death = 4,
    Kicked = 5,
    Banned = 6
}

// Mmo. Shared/Zones/Enums/TransferType.cs
public enum TransferType : byte
{
    Portal = 1,
    Teleport = 2,
    Hearthstone = 3,
    DungeonEntrance = 4,
    DungeonExit = 5,
    FlightPath = 6,
    SpellTeleport = 7,
    SummonAccept = 8,
    GraveyardTeleport = 9
}

// Mmo. Shared/Zones/Enums/ZoneStateType.cs
public enum ZoneStateType : byte
{
    Initial = 1,
    Transfer = 2,
    Reconnect = 3,
    FullSync = 4
}

// Mmo. Shared/Zones/Enums/WeatherType.cs
public enum WeatherType : byte
{
    Clear = 1,
    Cloudy = 2,
    Rain = 3,
    HeavyRain = 4,
    Snow = 5,
    Blizzard = 6,
    Fog = 7,
    Storm = 8,
    Sandstorm = 9
}

// Mmo.Shared/Zones/Enums/Faction.cs
public enum Faction :  byte
{
    Neutral = 0,
    Alliance = 1,
    Horde = 2
}
```

## ZoneFlags Erweiterung

Das existierende `ZoneFlags` Enum sollte um `IsGhostZone` erweitert werden:

```csharp
// Mmo.Shared/Zones/Enums/ZoneFlags.cs (bereits vorhanden - erweitern)
[Flags]
public enum ZoneFlags : ushort
{
    // ... existierende Flags ...

    /// <summary>
    ///     This is a ghost/spirit world zone (after death).
    /// </summary>
    IsGhostZone = 1 << 15
}
```

## Zone Class (aktualisiert)

Die Zone wurde von `struct` zu `class` geändert:

```csharp
// Mmo.Shared/Zones/Zone.cs
using Mmo.Shared. Zones.Enums;

namespace Mmo.Shared. Zones;

/// <summary>
/// Repräsentiert eine Spiel-Zone mit allen zugehörigen Daten.
/// </summary>
public class Zone
{
    private readonly HashSet<Guid> _entityIds = new();

    public Zone(ushort zoneId, string name, ZoneBounds bounds)
    {
        Id = zoneId;
        Name = name;
        Bounds = bounds;
    }

    // ═══════════════════════════════════════════════════════════════
    // IDENTIFIKATION
    // ═══════════════════════════════════════════════════════════════

    /// <summary>Eindeutige Zone-ID. </summary>
    public ushort Id { get; }

    /// <summary>Anzeigename der Zone.</summary>
    public string Name { get; }

    // ═══════════════════════════════════════════════════════════════
    // ZONE-EIGENSCHAFTEN (für ZoneDto)
    // ═══════════════════════════════════════════════════════════════

    /// <summary>Zone-Flags (PvP, Restrictions, etc.).</summary>
    public ZoneFlags Flags { get; set; } = ZoneFlags.None;

    /// <summary>Empfohlenes Mindest-Level.</summary>
    public int RecommendedMinLevel { get; set; } = 1;

    /// <summary>Empfohlenes Höchst-Level.</summary>
    public int RecommendedMaxLevel { get; set; } = 60;

    /// <summary>Kontrollierende Fraktion (null = neutral).</summary>
    public Faction? ControllingFaction { get; set; }

    /// <summary>Zone-Grenzen für Minimap und Collision.</summary>
    public ZoneBounds Bounds { get; }

    /// <summary>Standard-Spawn-Position für neue Spieler.</summary>
    public Position DefaultSpawnPoint { get; set; }

    /// <summary>Friedhof-Position für Wiederbelebung.</summary>
    public Position?  GraveyardPosition { get; set; }

    /// <summary>Musik-Asset-ID. </summary>
    public string MusicId { get; set; } = "";

    /// <summary>Ambiente-Sound-Asset-ID.</summary>
    public string AmbienceId { get; set; } = "";

    // ═══════════════════════════════════════════════════════════════
    // DYNAMISCHER STATE (NICHT im ZoneDto)
    // ═══════════════════════════════════════════════════════════════

    /// <summary>Aktuelles Wetter in der Zone.</summary>
    public WeatherType CurrentWeather { get; set; } = WeatherType.Clear;

    /// <summary>Aktuelle Tageszeit (0.0 - 24.0).</summary>
    public float TimeOfDay { get; set; } = 12.0f;

    // ═══════════════════════════════════════════════════════════════
    // ENTITY-MANAGEMENT
    // ═══════════════════════════════════════════════════════════════

    /// <summary>Anzahl Entities in der Zone.</summary>
    public int EntityCount => _entityIds.Count;

    /// <summary>Alle Entity-IDs in der Zone.</summary>
    public IEnumerable<Guid> GetEntityIds() => _entityIds;

    /// <summary>Prüft ob eine Entity in der Zone ist.</summary>
    public bool HasEntity(Guid persistentId) => _entityIds.Contains(persistentId);

    /// <summary>Fügt eine Entity zur Zone hinzu. </summary>
    public void AddEntity(Guid persistentId) => _entityIds.Add(persistentId);

    /// <summary>Entfernt eine Entity aus der Zone.</summary>
    public bool RemoveEntity(Guid persistentId) => _entityIds.Remove(persistentId);

    // ═══════════════════════════════════════════════════════════════
    // CONVENIENCE PROPERTIES
    // ═══════════════════════════════════════════════════════════════

    public bool IsPvPEnabled => Flags.HasFlag(ZoneFlags.PvpEnabled);
    public bool IsInstance => Flags.HasFlag(ZoneFlags.IsInstance);
    public bool IsCapital => Flags.HasFlag(ZoneFlags.IsCapital);
    public bool IsSanctuary => Flags.HasFlag(ZoneFlags.NoCombat);
    public bool HasRestXp => Flags.HasFlag(ZoneFlags.HasRestXp);
    public bool IsIndoor => Flags.HasFlag(ZoneFlags. IsIndoor);
    public bool AllowsMounting => ! Flags.HasFlag(ZoneFlags.NoMounting);
    public bool AllowsFlying => !Flags.HasFlag(ZoneFlags.NoFlying);
}
```

## Datei-Struktur

```
Mmo.Shared/
├── Zones/
│   ├── Zone.cs                         ← Haupt-Klasse (jetzt Class, nicht Struct)
│   ├── Enums/
│   │   ├── ZoneFlags.cs                ← Bereits vorhanden (erweitern)
│   │   ├── LeaveReason.cs              ← NEU
│   │   ├── PlayerLeftReason.cs         ← NEU
│   │   ├── TransferType.cs             ← NEU
│   │   ├── ZoneStateType.cs            ← NEU
│   │   ├── WeatherType.cs              ← NEU
│   │   └── Faction.cs                  ← NEU
│   ├── Dtos/
│   │   ├── ZoneDto.cs                  ← NEU
│   │   └── ZoneListItemDto.cs          ← NEU
│   ├── Structs/
│   │   └── ZoneBounds.cs               ← Bereits vorhanden
│   └── Messages/
│       ├── LeaveZone.cs                ← Aktualisieren
│       ├── ZoneState.cs                ← Aktualisieren
│       ├── PlayerJoinedZone.cs
│       ├── PlayerLeftZone.cs           ← Aktualisieren
│       ├── ZoneTransferRequest.cs      ← Aktualisieren
│       ├── ZoneTransferResponse.cs
│       ├── ZoneDiscovered.cs           ← Aktualisieren
│       ├── ZoneListRequest.cs
│       ├── ZoneListResponse.cs         ← Aktualisieren
│       ├── GetZoneRequest.cs           ← NEU
│       ├── ZoneLoadedAck.cs            ← NEU
│       └── EntityBatch.cs              ← NEU
│
├── Generators/
│   └── Attributes/
│       ├── GenerateDtoAttribute.cs
│       ├── DtoImplementsAttribute.cs
│       ├── ServerOnlyAttribute.cs
│       ├── DtoIgnoreAttribute.cs
│       └── DtoPropertyAttribute.cs
│
└── Messaging/
    └── Enums/
        └── MessageType.cs              ← Aktualisieren (117, 119, 120 hinzufügen)
```

---

**Letzte Aktualisierung:** 2025-12-28  
**Version:** 2.0.0

[← Zurück zur Übersicht](README.md)
