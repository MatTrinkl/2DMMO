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
    -   [ZoneConfigDto](#zoneconfigdto)
    -   [ZoneContextDto](#zonecontextdto)
    -   [ZoneListEntry](#zonelistentry)
-   [Aktive Messages](#aktive-messages)
    -   [LeaveZone (101)](#leavezone-101)
    -   [ZoneState (102)](#zonestate-102)
    -   [ZoneDelta (103)](#zonedelta-103)
    -   [CharacterJoinedZone (104)](#characterjoinedzone-104)
    -   [CharacterLeftZone (105)](#characterleftzone-105)
    -   [ZoneTransferRequest (106)](#zonetransferrequest-106)
    -   [ZoneTransferResponse (107)](#zonetransferresponse-107)
    -   [ZoneDiscovered (109)](#zonediscovered-109)
    -   [ZoneListRequest (110)](#zonelistrequest-110)
    -   [ZoneListResponse (111)](#zonelistresponse-111)
    -   [GetZoneRequest (117)](#getzonerequest-117)
    -   [ZoneLoadedAck (118)](#zoneloadedack-118)
-   [Phase 2 Messages](#phase-2-messages)
    -   [ShardTransfer (112)](#shardtransfer-112)
    -   [ShardListRequest (113)](#shardlistrequest-113)
    -   [ShardListResponse (114)](#shardlistresponse-114)
    -   [SubZoneEnter (115)](#subzoneenter-115)
    -   [SubZoneLeave (116)](#subzoneleave-116)
-   [Obsolete Messages](#obsolete-messages)
    -   [JoinZone (100)](#joinzone-100-obsolet)

---

## 🔄 Zone Loading Flow (Übersicht)

Der Zone-Loading-Prozess wurde vereinfacht. Eine einzige `ZoneState` Message enthält alle Daten die der Client zum Spawnen braucht.

### Architektur: ZoneConfigDto + ZoneContextDto vs ZoneState

```
┌─────────────────────────────────────────────────────────────────┐
│  ZoneConfigDto (statisch, cachebar) - from IZoneConfig          │
│  ├── ZoneId, DisplayName                                        │
│  ├── PvpType (PvpZoneType - Sanctuary, Normal, FFA)             │
│  ├── MinLevel, MaxLevel                                         │
│  └── (Internal: Bounds, Music, Ambience via config)             │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│  ZoneContextDto (dynamisch, Runtime) - from IZoneContext        │
│  ├── ShardId                                                    │
│  ├── CurrentWeather (WeatherType)                               │
│  ├── TimeOfDay (0.0 - 24.0)                                     │
│  ├── ControllingFaction (Faction?)                              │
│  ├── IsLocked, LockReason                                       │
│  └── IsInstance                                                 │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│  ZoneState (kombiniert, Runtime)                                │
│  ├── ZoneConfig: ZoneConfigDto (required)                       │
│  ├── ZoneContext: ZoneContextDto (required)                     │
│  ├── State: ZoneStateType (FullSync, etc.)                      │
│  ├── MyCharacter: CharacterEntityDto? (nullable)                │
│  ├── Entities: List<EntityDtoUnion>                             │
│  ├── HasMoreEntities, TotalEntitiesCount                        │
│  └── Timestamp (long)                                           │
└─────────────────────────────────────────────────────────────────┘
```

**Vorteile dieser Trennung:**

-   **Caching:** Client kann `ZoneConfigDto` lokal speichern
-   **Kleinere Messages:** Config + Context separat in Delta-Updates
-   **Wiederverwendung:** DTOs in mehreren Messages nutzbar
-   **Separate Updates:** Wetter/Zeit via ZoneDelta.ZoneContext änderbar

### Haupt-Flow: Login → Zone

```
Client                         Server
  │                              │
  │  CharacterSelectResponse     │
  │  (SpawnZoneId: 1001)         │
  │◄─────────────────────────────│
  │                              │
  │  GetZoneRequest (117)        │
  │  ZoneId: 1001                │
  │─────────────────────────────►│
  │                              │
  │  ZoneState (102)             │
  │  ├── ZoneConfig: ZoneConfigDto│
  │  ├── ZoneContext: ZoneContextDto│
  │  ├── State: FullSync         │
  │  ├── MyCharacter: CharacterEntityDto│
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
| `ZoneState`            | 102 | Server → Client    | Zone + MyCharacter + Entities     |
| `ZoneDelta`            | 103 | Server → Client    | Delta-Updates (jeden Tick)        |
| `ZoneLoadedAck`        | 118 | Client → Server    | Client bestätigt Ready            |
| `ZoneTransferRequest`  | 106 | Client → Server    | Zone wechseln wollen              |
| `ZoneTransferResponse` | 107 | Server → Client    | Transfer bestätigen/ablehnen      |
| `LeaveZone`            | 101 | Client → Server    | Logout/Exit/CharacterSwitch       |
| `CharacterJoinedZone`  | 104 | Server → Broadcast | Neuer Spieler in Zone             |
| `CharacterLeftZone`    | 105 | Server → Broadcast | Spieler verlässt Zone             |
| `ZoneDiscovered`       | 109 | Server → Client    | Neue Zone entdeckt                |
| `ZoneListRequest`      | 110 | Client → Server    | Liste aller Zonen anfordern       |
| `ZoneListResponse`     | 111 | Server → Client    | Liste aller Zonen                 |

### Obsolete Messages

| Message               | ID  | Ersetzt durch         |
| --------------------- | --- | --------------------- |
| `JoinZone`            | 100 | `ZoneState.MyCharacter` |

---

# DTOs

---

## ZoneConfigDto

**Zweck:** Statische Zone-Konfigurationsdaten, automatisch generiert aus `IZoneConfig`.

> **Hinweis:** DTOs werden automatisch aus Interfaces mit `[GenerateDto]` Attribut generiert.

### Felder (aus IZoneConfig)

| Feld        | Typ           | Beschreibung                        | Pflicht |
| ----------- | ------------- | ----------------------------------- | ------- |
| ZoneId      | ushort        | Eindeutige Zone-ID                  | Ja      |
| DisplayName | string        | Anzeigename der Zone                | Ja      |
| PvpType     | PvpZoneType   | PvP-Regelung (Sanctuary, Normal, FFA) | Ja      |
| MinLevel    | int           | Empfohlenes Mindest-Level           | Ja      |
| MaxLevel    | int           | Empfohlenes Höchst-Level            | Ja      |

> **Hinweis:** `InternalName`, `Contestable`, `ZoneFlags`, `OwningFaction`, `Bounds`, `MusicId`, `AmbienceId` sind mit `[IgnoreData]` markiert und nicht im DTO enthalten.

### Interface-Definition

```csharp
// Mmo.Shared/Zones/Interfaces/IZoneConfig.cs
[GenerateDto(DtoName = "ZoneConfigDto")]
[GenerateListEntry("ZoneListEntry")]
public interface IZoneConfig
{
    [BaseData]
    ushort ZoneId { get; }
    
    [IgnoreData]
    string InternalName { get; }
    
    [BaseData]
    string DisplayName { get; }
    
    [IgnoreData]
    bool Contestable { get; }
    
    [OptionalData("ZoneListEntry")]
    PvpZoneType PvpType { get; }
    
    [IgnoreData]
    ZoneFlags ZoneFlags { get; }
    
    [BaseData]
    int MinLevel { get; }
    
    [BaseData]
    int MaxLevel { get; }
    
    // ... weitere IgnoreData Felder
    
    // Computed Properties (nicht serialisiert)
    public bool IsPvpEnabled => PvpType != PvpZoneType.Sanctuary;
    public bool IsSanctuary => PvpType == PvpZoneType.Sanctuary;
    public bool IsInstance => ZoneFlags.HasFlag(ZoneFlags.IsInstance);
    // ...
}
```

---

## ZoneContextDto

**Zweck:** Dynamischer Zone-Runtime-State, automatisch generiert aus `IZoneContext`.

### Felder (aus IZoneContext)

| Feld                | Typ          | Beschreibung                            | Pflicht |
| ------------------- | ------------ | --------------------------------------- | ------- |
| ShardId             | ushort       | Aktuelle Shard-ID                       | Ja      |
| CurrentWeather      | WeatherType  | Aktuelles Wetter                        | Ja      |
| TimeOfDay           | float        | Tageszeit (0.0 - 24.0)                  | Ja      |
| ControllingFaction  | Faction?     | Kontrollierende Fraktion (null = neutral) | Nein    |
| IsLocked            | bool         | Zone gesperrt?                          | Ja      |
| LockReason          | string?      | Sperrgrund                              | Nein    |
| IsInstance          | bool         | Ist dies eine Instanz?                  | Ja      |

### Interface-Definition

```csharp
// Mmo.Shared/Zones/Interfaces/IZoneContext.cs
[GenerateDto(DtoName = "ZoneContextDto")]
[GenerateDirtyTracking(FlagsEnumType = "Mmo.Shared.Zones.Enums.ZoneDirtyFlags")]
public interface IZoneContext
{
    ushort ShardId { get; }
    
    [TrackDirty("Weather")]
    WeatherType CurrentWeather { get; }
    
    [TrackDirty("TimeOfDay")]
    float TimeOfDay { get; }
    
    bool IsNight => TimeOfDay is >= 20f or < 6f;
    
    [TrackDirty("ControllingFaction")]
    Faction? ControllingFaction { get; }
    
    bool IsLocked { get; }
    string? LockReason { get; }
    bool IsInstance { get; }
}
```

---

## ZoneListEntry

**Zweck:** Eintrag für die Zone-Liste, automatisch generiert mit `[GenerateListEntry]`.

### Felder

| Feld        | Typ           | Beschreibung                            |
| ----------- | ------------- | --------------------------------------- |
| ZoneId      | ushort        | Zone-ID                                 |
| DisplayName | string        | Anzeigename                             |
| MinLevel    | int           | Mindest-Level                           |
| MaxLevel    | int           | Max-Level                               |
| PvpType     | PvpZoneType   | PvP-Regelung (nur in ZoneListEntry)     |

### Client-Caching

```csharp
public class ZoneCache
{
    private readonly Dictionary<ushort, ZoneConfigDto> _cache = new();

    public void CacheZoneConfig(ZoneConfigDto config)
    {
        _cache[config.ZoneId] = config;
    }

    public ZoneConfigDto? GetZoneConfig(ushort zoneId)
    {
        return _cache.TryGetValue(zoneId, out var config) ? config : null;
    }

    public bool HasZone(ushort zoneId) => _cache.ContainsKey(zoneId);
}

// Verwendung bei ZoneState
public void OnZoneState(ZoneState state)
{
    // ZoneConfig cachen
    _zoneCache.CacheZoneConfig(state.ZoneConfig);

    // Dynamischen Context anwenden
    _weatherSystem.SetWeather(state.ZoneContext.CurrentWeather);
    _timeSystem.SetTime(state.ZoneContext.TimeOfDay);
    
    // MyCharacter spawnen (wenn vorhanden)
    if (state.MyCharacter != null)
    {
        _playerManager.SpawnMyCharacter(state.MyCharacter);
    }
    
    // Entities spawnen
    foreach (var entity in state.Entities)
    {
        _entityManager.SpawnEntity(entity);
    }
}
```

### Verwendung in Messages

| Message            | Verwendung                                        |
| ------------------ | ------------------------------------------------- |
| `ZoneState`        | `ZoneConfig: ZoneConfigDto`, `ZoneContext: ZoneContextDto` |
| `ZoneListResponse` | `List<ZoneListEntry>` enthält Zone-Übersicht      |
| `ZoneDelta`        | `ZoneContext: IZoneContextDelta?` für Updates     |

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
| `CharacterLeftZone`    | 105 | Broadcast an andere Spieler         |
| `ZoneTransferRequest`  | 106 | Für Zone-Wechsel (nicht LeaveZone!) |
| `CharacterListRequest` | 12  | Nach CharacterSwitch                |
| `ForceDisconnect`      | 5   | Connection-Level Disconnect         |

---

## ZoneState (102)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten (Initial Load, Zone Transfer, Periodic Sync, Reconnect)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Kompletter Snapshot des Zone-States. Dies ist die **Haupt-Message für Zone-Loading** und enthält:

-   **ZoneConfig**: Statische Zone-Konfiguration als `ZoneConfigDto`
-   **ZoneContext**: Dynamischer State (Wetter, Tageszeit) als `ZoneContextDto`
-   **MyCharacter**: Dein Character als `CharacterEntityDto` (nullable)
-   **Entities**: Alle anderen Entities als `List<EntityDtoUnion>`

### StateType Enum

| Wert         | Beschreibung                       | MyCharacter  |
| ------------ | ---------------------------------- | ------------ |
| `FullSync`   | Periodischer Full-Sync             | ✅/❌        |

### Payload

| Feld               | Typ                    | Beschreibung                     | Pflicht |
| ------------------ | ---------------------- | -------------------------------- | ------- |
| Type               | MessageType            | `MessageType.ZoneState`          | Ja      |
| Timestamp          | long                   | Server-Timestamp (Unix ms)       | Ja      |
| ZoneConfig         | ZoneConfigDto          | Statische Zone-Konfiguration     | Ja      |
| ZoneContext        | ZoneContextDto         | Dynamischer Zone-State           | Ja      |
| Entities           | List\<EntityDtoUnion\> | Alle Entities                    | Ja      |
| State              | ZoneStateType          | FullSync                         | Ja      |
| MyCharacter        | CharacterEntityDto?    | Dein Character (nullable)        | Nein    |
| HasMoreEntities    | bool                   | Gibt es weitere Entities?        | Ja      |
| TotalEntitiesCount | int                    | Gesamtzahl Entities in Zone      | Ja      |

### Code-Beispiel (aktueller Code)

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.ZoneState)]
public class ZoneState : IServerMessage, ITimestampedMessage
{
    [Key(0)] public MessageType Type => MessageType.ZoneState;
    [Key(1)] public long Timestamp { get; init; }
    [Key(2)] public required ZoneConfigDto ZoneConfig { get; init; }
    [Key(3)] public required ZoneContextDto ZoneContext { get; init; }
    [Key(4)] public required List<EntityDtoUnion> Entities { get; init; }
    [Key(5)] public ZoneStateType State { get; init; } = ZoneStateType.FullSync;
    [Key(6)] public CharacterEntityDto? MyCharacter { get; init; }
    [Key(7)] public bool HasMoreEntities { get; init; }
    [Key(8)] public int TotalEntitiesCount { get; init; }

    [IgnoreMember]
    public ushort ZoneId => ZoneConfig.ZoneId;
}
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

**✅ AKTIV UND PRODUKTIV** - Chunk-Based Delta Sync System

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
| SpawnedEntities | List\<EntityDtoUnion\>? | Neue Entities in sichtbaren Chunks | Nein |
| DespawnedEntityIds | List\<Guid\>? | Entfernte Entities (IDs) | Nein |
| EntityUpdates | List\<EntityDeltaUnion\>? | Entity-Änderungen (Delta pattern) | Nein |

### Entity Delta System

#### Automatisch Generierte Delta DTOs

Das System nutzt Source-Generatoren um Delta DTOs automatisch aus Entity-Interfaces zu erstellen:

```csharp
// Interface mit [GenerateDeltaDtoUnion] Attribut
[GenerateDeltaDtoUnion(UnionName = "EntityDeltaUnion", Namespace = "Mmo.Shared.Entities.Dtos")]
public interface IEntity
{
    Guid PersistentId { get; init; }
    EntityType Type { get; }
    Position Position { get; set; }
}
```

**Generated Delta DTOs:**
- `ICharacterEntityDelta` - Für Spieler-Charaktere
- `INpcEntityDelta` - Für NPCs
- `EntityDeltaUnion` - Union-Wrapper für polymorphe Listen

#### Nullable Pattern (null = unverändert)

Alle Felder in Delta DTOs sind **nullable**. Nur geänderte Properties haben einen Wert:

```csharp
// Beispiel: Nur Position hat sich geändert
var delta = new ICharacterEntityDelta
{
    PersistentId = entityId,
    Position = new Position(100, 200),  // ✅ Geändert → nicht-null
    CurrentHealth = null,                // ❌ Unverändert → null
    Level = null,                        // ❌ Unverändert → null
    IsInCombat = null                    // ❌ Unverändert → null
};
```

**Vorteile:**
- **Bandbreiten-Optimierung**: MessagePack omitted null-Felder (30-97% Einsparung!)
- **Eindeutige Semantik**: `null` = unverändert vs. `0` = Wert auf 0 geändert
- **Flexibilität**: Jede Entity kann unterschiedliche Felder ändern

#### O(1) Lookup mit IDeltaDto<TId>

Delta DTOs implementieren `IDeltaDto<Guid>` für effiziente Lookups:

```csharp
// ICharacterEntityDelta implementiert IDeltaDto<Guid>
public Guid GetId() => PersistentId;

// Client-seitig: Convert to dictionary for O(1) lookup
var deltaDict = zoneDelta.EntityUpdates
    .ToDictionary(d => ((IDeltaDto<Guid>)d).GetId());

// Fast lookup by entity ID - O(1)
if (deltaDict.TryGetValue(myEntityId, out var delta))
{
    ApplyDelta(delta);
}
```

**Performance:**
- **Linear search**: O(n) - langsam bei großen Listen
- **Dictionary lookup**: O(1) - schnell unabhängig von Listengröße

#### Beispiel Delta DTOs

```csharp
// ICharacterEntityDelta - Auto-generated
[MessagePackObject]
public class ICharacterEntityDelta : IDeltaDto<Guid>
{
    [Key(0)] public Guid PersistentId { get; set; }
    
    // Movement properties (nullable)
    [Key(1)] public Position? Position { get; set; }
    [Key(2)] public float? MovementSpeed { get; set; }
    
    // Combat properties (nullable)
    [Key(3)] public int? CurrentHealth { get; set; }
    [Key(4)] public int? MaxHealth { get; set; }
    [Key(5)] public int? CurrentResource { get; set; }
    [Key(6)] public int? MaxResource { get; set; }
    [Key(7)] public bool? IsInCombat { get; set; }
    [Key(8)] public Guid? TargetEntityId { get; set; }
    
    // Progression properties (nullable)
    [Key(9)] public int? Level { get; set; }
    
    // IDeltaDto implementation
    public Guid GetId() => PersistentId;
}

// INpcEntityDelta - Auto-generated
[MessagePackObject]
public class INpcEntityDelta : IDeltaDto<Guid>
{
    [Key(0)] public Guid PersistentId { get; set; }
    
    // NPC-specific properties (nullable)
    [Key(1)] public Position? Position { get; set; }
    [Key(2)] public int? CurrentHealth { get; set; }
    [Key(3)] public int? MaxHealth { get; set; }
    [Key(4)] public int? Level { get; set; }
    [Key(5)] public bool? IsInCombat { get; set; }
    
    // IDeltaDto implementation
    public Guid GetId() => PersistentId;
}
```

### Sub-DTOs

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
    
    // Unified Delta system - all entity changes in one list
    [Key(5)] public List<EntityDeltaUnion>? EntityUpdates { get; set; }
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
    
    var entityUpdates = new List<EntityDeltaUnion>();
    
    // Sammle Änderungen aus allen sichtbaren Chunks
    foreach (var chunk in visibleChunks)
    {
        var dirtyEntities = _chunkDirtyTracker.GetDirtyEntitiesInChunk(chunk);
        
        foreach (var entity in dirtyEntities)
        {
            if (entity.IsDirty)
            {
                // Generiere unified Delta DTO mit allen Änderungen
                var entityDelta = entity.ToDeltaUnion(); // Extension method
                entityUpdates.Add(entityDelta);
            }
        }
    }
    
    delta.EntityUpdates = entityUpdates.Any() ? entityUpdates : null;
    
    // Clear dirty flags after collecting
    foreach (var entity in visibleEntities)
    {
        entity.ClearDirtyFlags();
    }
    
    return delta;
}

// Extension method example (generated)
public static EntityDeltaUnion ToDeltaUnion(this ICharacterEntity character)
{
    return new ICharacterEntityDelta
    {
        PersistentId = character.PersistentId,
        Position = character.DirtyFlags.HasFlag(DirtyFlags.Position) 
            ? character.Position 
            : null,
        CurrentHealth = character.DirtyFlags.HasFlag(DirtyFlags.Health)
            ? character.CurrentHealth
            : null,
        Level = character.DirtyFlags.HasFlag(DirtyFlags.Level)
            ? character.Level
            : null,
        // ... other properties based on DirtyFlags
    };
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
    
    // 3. Entity Updates - polymorphic processing
    if (delta.EntityUpdates != null)
    {
        // Option A: Direct iteration with pattern matching
        foreach (var deltaUnion in delta.EntityUpdates)
        {
            switch (deltaUnion.Value)
            {
                case ICharacterEntityDelta charDelta:
                    ApplyCharacterDelta(charDelta);
                    break;
                    
                case INpcEntityDelta npcDelta:
                    ApplyNpcDelta(npcDelta);
                    break;
            }
        }
        
        // Option B: Convert to dictionary for O(1) lookup
        var deltaDict = delta.EntityUpdates
            .Select(u => u.Value)
            .OfType<IDeltaDto<Guid>>()
            .ToDictionary(d => d.GetId());
        
        foreach (var entity in _visibleEntities)
        {
            if (deltaDict.TryGetValue(entity.PersistentId, out var entityDelta))
            {
                ApplyDelta(entity, entityDelta);
            }
        }
    }
}

private void ApplyCharacterDelta(ICharacterEntityDelta delta)
{
    var entity = _entityManager.GetEntity(delta.PersistentId);
    if (entity == null) return;
    
    // Only update changed properties (non-null)
    if (delta.Position.HasValue)
        entity.Position = delta.Position.Value;
        
    if (delta.CurrentHealth.HasValue)
        entity.CurrentHealth = delta.CurrentHealth.Value;
        
    if (delta.MaxHealth.HasValue)
        entity.MaxHealth = delta.MaxHealth.Value;
        
    if (delta.Level.HasValue)
        entity.Level = delta.Level.Value;
        
    if (delta.IsInCombat.HasValue)
        entity.IsInCombat = delta.IsInCombat.Value;
        
    if (delta.TargetEntityId.HasValue)
        entity.TargetEntityId = delta.TargetEntityId.Value;
}

private void ApplyNpcDelta(INpcEntityDelta delta)
{
    var npc = _npcManager.GetNpc(delta.PersistentId);
    if (npc == null) return;
    
    // Apply only non-null (changed) fields
    if (delta.Position.HasValue)
        npc.Position = delta.Position.Value;
        
    if (delta.CurrentHealth.HasValue)
        npc.CurrentHealth = delta.CurrentHealth.Value;
        
    if (delta.Level.HasValue)
        npc.Level = delta.Level.Value;
}
```

### Beispiel Payload

```csharp
// Typisches Delta (3 bewegende Entities, 1 HP-Update)
var delta = new ZoneDelta
{
    Timestamp = 1234567890123,
    ZoneId = 1001,
    EntityUpdates = new List<EntityDeltaUnion>
    {
        // Player bewegt sich
        new ICharacterEntityDelta 
        { 
            PersistentId = player1Id, 
            Position = new Position(100.5f, 200.3f)
            // Andere Felder = null (unverändert)
        },
        
        // NPC bewegt sich
        new INpcEntityDelta 
        { 
            PersistentId = npc1Id, 
            Position = new Position(150.2f, 180.1f)
        },
        
        // Player nimmt Schaden
        new ICharacterEntityDelta 
        { 
            PersistentId = player2Id, 
            CurrentHealth = 450,
            MaxHealth = 600,
            IsInCombat = true
        }
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

// Nur Position-Updates (häufigstes Szenario)
var movementDelta = new ZoneDelta
{
    Timestamp = 1234567890789,
    ZoneId = 1001,
    EntityUpdates = new List<EntityDeltaUnion>
    {
        new ICharacterEntityDelta 
        { 
            PersistentId = entityId,
            Position = new Position(120.0f, 210.0f),
            // ALLE anderen Felder sind null → nicht serialisiert!
        }
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
ALTE STRATEGIE (Separate Position/State Lists):
  - Separate Listen für Position, State, etc.
  - Overhead pro Liste: ~20-40 bytes (List Headers)
  - Nullable pattern für State-Felder
  
NEUE STRATEGIE (Unified EntityDeltaUnion):
  - Ein EntityUpdates List für alle Änderungen
  - Overhead: ~20 bytes (ein List-Header)
  - Nullable pattern für ALLE Felder
  - Nur geänderte Properties werden serialisiert
  
BEISPIEL - Entity bewegt sich:
  Alte Strategie:
    - PositionUpdates List: ~20 bytes Header
    - EntityPositionDelta: ~50 bytes (EntityId + X/Y + Velocity)
    - Total: ~70 bytes
    
  Neue Strategie:
    - EntityUpdates List: ~20 bytes Header (geteilt)
    - ICharacterEntityDelta: ~40 bytes (EntityId + Position struct)
    - Total: ~40 bytes (wenn bereits List existiert)
    
REDUKTION: 40-70% weniger Overhead bei gemischten Updates!

BEISPIEL - 10 Entities, 5 bewegen sich, 3 nehmen Schaden, 2 beides:
  Alte Strategie:
    - PositionUpdates (7 Entities): 20 + 7*50 = 370 bytes
    - StateUpdates (5 Entities): 20 + 5*30 = 170 bytes
    - Total: 540 bytes
    
  Neue Strategie:
    - EntityUpdates (10 Entities): 20 + 5*40 + 3*25 + 2*60 = 395 bytes
    - Total: 395 bytes
    
REDUKTION: 27% weniger Bytes bei realistischem Mix!
```

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
- **Unified Delta**: Alle Entity-Properties in einem DTO (nullable pattern)
- **Bandbreiten-Reduktion**: 96%+ vs. Full Broadcast
- **Skalierung**: Konstante Bandbreite unabhängig von Zone-Größe

### Erweiterbarkeit

Das Delta-System ist **generisch und erweiterbar**:

1. **Neue Entity-Typen hinzufügen:**
   - Markiere Interface mit `[GenerateDeltaDtoUnion]`
   - Source Generator erstellt automatisch Delta DTO
   - Wird automatisch in `EntityDeltaUnion` integriert
   ```csharp
   [GenerateDeltaDtoUnion(UnionName = "EntityDeltaUnion")]
   public interface ICustomEntity : IEntity
   {
       string CustomProperty { get; set; }
   }
   // Generiert: ICustomEntityDelta mit nullable CustomProperty
   ```

2. **Neue Properties zu bestehenden Entities:**
   - Füge Property zu Interface hinzu
   - Generator updated automatisch Delta DTO
   - Nullable pattern wird beibehalten
   ```csharp
   public interface ICharacterEntity
   {
       // ... bestehende Properties
       
       [TrackedProperty(DirtyFlags.Equipment)]
       EquipmentSet? Equipment { get; set; }  // NEU
   }
   // ICharacterEntityDelta erhält automatisch:
   // [Key(XX)] public EquipmentSet? Equipment { get; set; }
   ```

3. **Eigene DirtyFlags definieren:**
   ```csharp
   [Flags]
   public enum CustomDirtyFlags : uint
   {
       Equipment = 1 << 10,
       Buffs = 1 << 11,
       Achievements = 1 << 12,
       // ... bis zu 32 Flags
   }
   ```

Siehe [Dirty-Tracking Architecture](../../02-architecture/DIRTY_TRACKING.md) für vollständige Dokumentation und Beispiele.

### Verwandte Dokumentation

- [Dirty-Tracking System](../../02-architecture/DIRTY_TRACKING.md) - Automatisches Change-Tracking
- [Chunk-Based Sync System](../../02-architecture/CHUNK_BASED_SYNC.md) - Vollständige Dokumentation
- [Game Loop - Output Phase](../../02-architecture/GAME_LOOP.md) - Delta-Building Logik
- [Entity Messages](14-entity.md) - Deprecated Messages

---

## CharacterJoinedZone (104)

**Richtung:** 📡 Broadcast (Server → All Clients in Zone)  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Broadcast an alle Spieler in der Zone wenn ein neuer Spieler spawnt. Der neue Spieler selbst erhält `ZoneState` (102), nicht diese Message.

### Payload

| Feld         | Typ               | Beschreibung                       | Pflicht |
| ------------ | ----------------- | ---------------------------------- | ------- |
| Type         | MessageType       | `MessageType.CharacterJoinedZone`  | Ja      |
| JoinedPlayer | CharacterEntityDto| Komplette sichtbare Spieler-Daten  | Ja      |
| ZoneId       | ushort            | Zone-ID für Validierung            | Ja      |

### Code-Beispiel (aktueller Code)

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.CharacterJoinedZone)]
public class CharacterJoinedZone : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.CharacterJoinedZone;
    [Key(1)] public required CharacterEntityDto JoinedPlayer { get; init; }
    [Key(2)] public ushort ZoneId { get; init; }
}
```

### Beispiel Payload

```csharp
var characterJoined = new CharacterJoinedZone
{
    JoinedPlayer = newPlayer.ToDto(),
    ZoneId = zone.ZoneId
};

// Broadcast an alle AUSSER dem neuen Spieler
zone.BroadcastExcept(characterJoined, newPlayer.ConnectionId);
```

### Verwandte Messages

| Message              | ID   | Beziehung                                  |
| -------------------- | ---- | ------------------------------------------ |
| `ZoneState`          | 102  | Was der neue Spieler selbst erhält         |
| `CharacterLeftZone`  | 105  | Gegenstück beim Verlassen                  |
| `EntitySpawn`        | 1400 | Generische Entity-Spawn Message (für NPCs) |

### Notizen

-   Wird **NUR** an bereits anwesende Spieler gesendet
-   Der joinierende Spieler erhält `ZoneState` (102) mit allen Entities
-   Client fügt Spieler zur lokalen Entity-Liste hinzu
-   Enthält komplettes `CharacterEntityDto` für sofortiges Rendering

---

## CharacterLeftZone (105)

**Richtung:** 📡 Broadcast (Server → All Clients in Zone)  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Broadcast an alle Spieler wenn ein Spieler die Zone verlässt. Der **Server** setzt den Reason basierend auf dem Ereignis.

### Payload

| Feld        | Typ             | Beschreibung                    | Pflicht |
| ----------- | --------------- | ------------------------------- | ------- |
| Type        | MessageType     | `MessageType.CharacterLeftZone` | Ja      |
| CharacterId | Guid            | PersistentId des Characters     | Ja      |
| LeaveReason | ZoneLeaveReason | Grund (vom Server gesetzt)      | Ja      |
| ZoneId      | ushort          | Zone-ID für Validierung         | Ja      |

### Code-Beispiel (aktueller Code)

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.CharacterLeftZone)]
public class CharacterLeftZone : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.CharacterLeftZone;
    [Key(1)] public Guid CharacterId { get; init; }
    [Key(2)] public ZoneLeaveReason LeaveReason { get; init; }
    [Key(3)] public ushort ZoneId { get; init; }
}
```

### Enum (aus Code)

```csharp
public enum ZoneLeaveReason : byte
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

## ZoneDiscovered (109)

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

## ZoneListRequest (110)

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

## ZoneListResponse (111)

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

## ZoneLoadedAck (118)

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

## ShardTransfer (112)

**Status:** 🔮 Phase 2  
**Beschreibung:** Transfer zu anderem Shard (Zone-Instance) für Load-Balancing bei überfüllten Zonen.

### Geplante Funktionalität

-   Automatischer Transfer bei Zone-Überlastung
-   Manueller Shard-Wechsel zu Freunden
-   Seamless Transition ohne Re-Login

---

## ShardListRequest (113)

**Status:** 🔮 Phase 2  
**Beschreibung:** Liste aller verfügbaren Shards für aktuelle Zone anfragen.

### Geplante Funktionalität

-   Shard-Auslastung anzeigen
-   Freunde auf anderen Shards finden
-   Bevorzugten Shard auswählen

---

## ShardListResponse (114)

**Status:** 🔮 Phase 2  
**Beschreibung:** Antwort mit Shard-Informationen (Population, Status, Freunde).

---

## SubZoneEnter (115)

**Status:** 🔮 Phase 2  
**Beschreibung:** Spieler betritt Sub-Zone (z.B. "Goldshire" innerhalb von "Elwynn Forest").

### Geplante Funktionalität

-   UI zeigt Sub-Zone-Name
-   Musik/Ambiente wechselt
-   Lokale Quests aktivieren

---

## SubZoneLeave (116)

**Status:** 🔮 Phase 2  
**Beschreibung:** Spieler verlässt Sub-Zone.

---

## ZonePhaseChange [PLANNED]

**Status:** 🔮 Phase 2 (No ID assigned yet)  
**Beschreibung:** Zone ändert Phase basierend auf Quest-Fortschritt (Phasing-System).

> **Hinweis:** Diese Message hat noch keine zugewiesene ID im `MessageType` Enum. Die ID wird in Phase 2 vergeben.

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

## ZoneLoadingProgress (108) [OBSOLET]

> ⚠️ **OBSOLET** - Nicht mehr benötigt
>
> Der Client lädt Zone-Assets lokal und sendet `ZoneLoadedAck` (118) wenn fertig. Server muss keinen Loading-Progress mehr senden.

**MessageType ID:** 108 - Reserviert, kann für zukünftige Zwecke wiederverwendet werden.

---

## GetZoneResponse [OBSOLET]

> ⚠️ **OBSOLET** - Ersetzt durch direkte `ZoneState` Antwort
>
> Server antwortet auf `GetZoneRequest` (117) direkt mit `ZoneState` (102). Ein separater Response-Wrapper ist nicht mehr nötig.

**Hinweis:** Diese Message hatte keine zugewiesene ID im Enum und wurde nie implementiert.

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
