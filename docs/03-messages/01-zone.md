# 🗺️ Zone Events Messages (0100-0199)

**Kategorie:** 1  
**Range:** 0100-0199  
**Phase:** Prototyp  
**Status:** 🟢 In Entwicklung

[← Zurück zur Übersicht](README.md)

---

## 📋 Inhaltsverzeichnis

-   [Zone Loading Flow (Übersicht)](#-zone-loading-flow-übersicht)
-   [Aktive Messages](#aktive-messages)
    -   [LeaveZone (101)](#leavezone-101)
    -   [ZoneState (102)](#zonestate-102)
    -   [PlayerJoinedZone (103)](#playerjoinedzone-103)
    -   [PlayerLeftZone (104)](#playerleftzone-104)
    -   [ZoneTransferRequest (105)](#zonetransferrequest-105)
    -   [ZoneTransferResponse (106)](#zonetransferresponse-106)
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
  │  ZoneState (102)             │  ← ALLES in einer Message!
  │  ├── Zone-Metadaten          │
  │  ├── StateType: Initial      │
  │  ├── MyPlayer:  PlayerEntityDto│
  │  └── Entities: List<IEntityDto>│
  │◄─────────────────────────────│
  │                              │
  │  [Optional bei >100 Entities]│
  │  EntityBatch (120)           │
  │◄─────────────────────────────│
  │                              │
  │  [Client buffert + lädt Assets]
  │                              │
  │  ZoneLoadedAck (119)         │  ← Client ist ready
  │─────────────────────────────►│
  │                              │
  │  [Server startet Updates]    │
  │  PositionBroadcast (201)     │
  │◄─────────────────────────────│
```

### Zone Transfer Flow: Portal/Teleport/Hearthstone

```
Client                         Server
  │                              │
  │  ZoneTransferRequest (105)   │
  │  TargetZoneId: 2001          │
  │  TransferType: Portal        │
  │─────────────────────────────►│
  │                              │
  │  ZoneTransferResponse (106)  │
  │  Success: true               │
  │◄─────────────────────────────│
  │                              │
  │  [PlayerLeftZone broadcast   │
  │   an alte Zone]              │
  │                              │
  │  ZoneState (102)             │
  │  ├── StateType: Transfer     │
  │  ├── MyPlayer: PlayerEntityDto│
  │  └── Entities:  [...]         │
  │◄─────────────────────────────│
  │                              │
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
  │                              │  Reason:  Disconnect
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
  │  StateType:  Transfer         │
  │◄─────────────────────────────│
```

### Message-Übersicht

| Message                | ID  | Richtung           | Zweck                        |
| ---------------------- | --- | ------------------ | ---------------------------- |
| `GetZoneRequest`       | 117 | Client → Server    | Zone-Daten anfordern         |
| `ZoneState`            | 102 | Server → Client    | Zone + MyPlayer + Entities   |
| `EntityBatch`          | 120 | Server → Client    | Weitere Entities (Chunking)  |
| `ZoneLoadedAck`        | 119 | Client → Server    | Client bestätigt Ready       |
| `ZoneTransferRequest`  | 105 | Client → Server    | Zone wechseln wollen         |
| `ZoneTransferResponse` | 106 | Server → Client    | Transfer bestätigen/ablehnen |
| `LeaveZone`            | 101 | Client → Server    | Logout/Exit/CharacterSwitch  |
| `PlayerJoinedZone`     | 103 | Server → Broadcast | Neuer Spieler in Zone        |
| `PlayerLeftZone`       | 104 | Server → Broadcast | Spieler verlässt Zone        |

### Obsolete Messages

| Message               | ID  | Ersetzt durch         |
| --------------------- | --- | --------------------- |
| `JoinZone`            | 100 | `ZoneState. MyPlayer` |
| `ZoneLoadingProgress` | 107 | Client lädt lokal     |
| `GetZoneResponse`     | 118 | `ZoneState` direkt    |

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
public enum LeaveReason : byte
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
public class LeaveZone :  IClientMessage
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

-   Zone-Metadaten (Name, Typ, Wetter, etc.)
-   **MyPlayer**: Dein Character als PlayerEntityDto
-   **Entities**: Alle anderen Entities in der Zone (als DTOs)

### StateType Enum

| Wert            | Beschreibung                                       | MyPlayer     |
| --------------- | -------------------------------------------------- | ------------ |
| `Initial` (1)   | Erster Login, Character spawnt zum ersten Mal      | ✅ Enthalten |
| `Transfer` (2)  | Zone-Wechsel durch Portal/Teleport/Hearthstone/Tod | ✅ Enthalten |
| `Reconnect` (3) | Nach Verbindungsabbruch (innerhalb 30s Window)     | ✅ Enthalten |
| `FullSync` (4)  | Periodischer Full-Sync (alle 60s)                  | ❌ null      |

### Payload

| Feld             | Typ                | Beschreibung                            | Pflicht     |
| ---------------- | ------------------ | --------------------------------------- | ----------- |
| Type             | MessageType        | `MessageType.ZoneState`                 | Ja          |
| Timestamp        | long               | Server-Timestamp (Unix ms)              | Ja          |
| ZoneId           | ushort             | Zone-ID                                 | Ja          |
| ZoneName         | string             | Name der Zone                           | Ja          |
| ZoneType         | ZoneType           | Outdoor, Dungeon, City, Instance, Arena | Ja          |
| Weather          | WeatherType        | Sunny, Rain, Snow, Fog, Storm           | Ja          |
| TimeOfDay        | float              | 0.0-24.0 (Stunden)                      | Ja          |
| StateType        | ZoneStateType      | Initial, Transfer, Reconnect, FullSync  | Ja          |
| MyPlayer         | PlayerEntityDto?   | Dein Character (null bei FullSync)      | Conditional |
| Entities         | List\<IEntityDto\> | Alle Entities (max 100 pro Message)     | Ja          |
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

public enum ZoneType : byte
{
    Outdoor = 1,
    Dungeon = 2,
    City = 3,
    Instance = 4,
    Arena = 5,
    Battleground = 6,
    Sanctuary = 7,    // Keine PvP-Zone
    GhostZone = 8     // Nach Tod
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
public class ZoneState :  ITimestampedServerMessage
{
    [Key(0)] public MessageType Type => MessageType.ZoneState;
    [Key(1)] public long Timestamp { get; set; }

    // Zone-Metadaten
    [Key(2)] public ushort ZoneId { get; set; }
    [Key(3)] public string ZoneName { get; set; } = "";
    [Key(4)] public ZoneType ZoneType { get; set; }
    [Key(5)] public WeatherType Weather { get; set; }
    [Key(6)] public float TimeOfDay { get; set; }

    // State-Type
    [Key(7)] public ZoneStateType StateType { get; set; }

    // Dein Character (null bei FullSync)
    [Key(8)] public PlayerEntityDto?  MyPlayer { get; set; }

    // Entities (max ~100 pro Message)
    [Key(9)] public List<IEntityDto> Entities { get; set; } = new();

    // Chunking
    [Key(10)] public bool HasMoreEntities { get; set; }
    [Key(11)] public int TotalEntityCount { get; set; }
}
```

### Beispiel Payload

```csharp
var zoneState = new ZoneState
{
    Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
    ZoneId = 1001,
    ZoneName = "Elwynn Forest",
    ZoneType = ZoneType.Outdoor,
    Weather = WeatherType. Clear,
    TimeOfDay = 14.5f, // 14:30
    StateType = ZoneStateType.Initial,
    MyPlayer = PlayerEntityDto.FromEntity(playerEntity),
    Entities = zone.GetVisibleEntities(playerEntity)
        .Select(e => e.ToDto())
        .Take(100)
        .ToList(),
    HasMoreEntities = zone.EntityCount > 100,
    TotalEntityCount = zone.EntityCount
};
```

### Chunking bei großen Zonen

Bei Zonen mit mehr als 100 Entities wird Chunking verwendet:

```
Zone mit 250 Entities:

ZoneState (102)
├── MyPlayer:  PlayerEntityDto
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

| Use-Case         | StateType | MyPlayer        | Entities               |
| ---------------- | --------- | --------------- | ---------------------- |
| Login            | Initial   | ✅ Vollständig  | ✅ Alle sichtbaren     |
| Portal/Teleport  | Transfer  | ✅ Aktualisiert | ✅ Alle in neuer Zone  |
| Hearthstone      | Transfer  | ✅ Aktualisiert | ✅ Alle in Heimat-Zone |
| Tod → Ghost-Zone | Transfer  | ✅ Als Geist    | ✅ Alle in Ghost-Zone  |
| Reconnect        | Reconnect | ✅ Restored     | ✅ Alle sichtbaren     |
| Periodic Sync    | FullSync  | ❌ null         | ✅ Alle (Validation)   |

### Verwandte Messages

| Message          | ID   | Beziehung                               |
| ---------------- | ---- | --------------------------------------- |
| `GetZoneRequest` | 117  | Request der ZoneState auslöst           |
| `ZoneLoadedAck`  | 119  | Client-Bestätigung nach ZoneState       |
| `EntityBatch`    | 120  | Weitere Entities bei Chunking           |
| `EntitySpawn`    | 1400 | Einzelne Entity spawnt später (Runtime) |
| `EntityDespawn`  | 1402 | Entity verlässt Zone (Runtime)          |

### Notizen

-   **Message-Größe**: ~2-10 KB (abhängig von Entity-Anzahl)
-   **Chunking-Threshold**: 100 Entities pro Message
-   **MyPlayer enthält KEINE ServerOnly Properties** (Experience, Gold, AccountId)
-   Bei `FullSync`: Client vergleicht mit lokalem State für Desync-Detection

---

## PlayerJoinedZone (103)

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
    Player = PlayerEntityDto.FromEntity(newPlayer)
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

## PlayerLeftZone (104)

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

## ZoneTransferRequest (105)

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
    TargetZoneId = 5001, // Dungeon-ID
    TransferType = TransferType. DungeonEntrance
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

## ZoneTransferResponse (106)

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

Benachrichtigung dass der Spieler eine neue Zone zum ersten Mal betreten hat. Kann XP-Bonus und Achievement auslösen.

### Payload

| Feld     | Typ         | Beschreibung                 | Pflicht |
| -------- | ----------- | ---------------------------- | ------- |
| Type     | MessageType | `MessageType.ZoneDiscovered` | Ja      |
| ZoneId   | ushort      | Entdeckte Zone-ID            | Ja      |
| ZoneName | string      | Zone-Name für UI             | Ja      |
| XpBonus  | int         | XP-Belohnung (0 wenn keine)  | Ja      |

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.ZoneDiscovered)]
public class ZoneDiscovered : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.ZoneDiscovered;
    [Key(1)] public ushort ZoneId { get; set; }
    [Key(2)] public string ZoneName { get; set; } = "";
    [Key(3)] public int XpBonus { get; set; }
}
```

### Beispiel Payload

```csharp
var discovered = new ZoneDiscovered
{
    ZoneId = 1005,
    ZoneName = "Darkwood Forest",
    XpBonus = 150
};
```

### Client-Verhalten

-   "Zone Discovered" UI-Element anzeigen
-   Zone-Name prominent darstellen
-   XP-Gewinn animieren (falls > 0)
-   Sound-Effekt abspielen
-   Zone in Weltkarte als "entdeckt" markieren

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

Liste aller (entdeckten) Zonen für Weltkarte und Fast-Travel.

### Payload

| Feld  | Typ                 | Beschreibung                   | Pflicht |
| ----- | ------------------- | ------------------------------ | ------- |
| Type  | MessageType         | `MessageType.ZoneListResponse` | Ja      |
| Zones | List\<ZoneInfoDto\> | Zone-Informationen             | Ja      |

### ZoneInfoDto

```csharp
[MessagePackObject]
public class ZoneInfoDto
{
    [Key(0)] public ushort ZoneId { get; set; }
    [Key(1)] public string Name { get; set; } = "";
    [Key(2)] public ZoneType ZoneType { get; set; }
    [Key(3)] public int RecommendedMinLevel { get; set; }
    [Key(4)] public int RecommendedMaxLevel { get; set; }
    [Key(5)] public bool IsDiscovered { get; set; }
    [Key(6)] public bool HasFlightPath { get; set; }
    [Key(7)] public Position? FlightPathPosition { get; set; }
    [Key(8)] public bool IsPvPZone { get; set; }
    [Key(9)] public bool IsContested { get; set; }
    [Key(10)] public Faction?  ControllingFaction { get; set; }
}
```

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.ZoneListResponse)]
public class ZoneListResponse : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.ZoneListResponse;
    [Key(1)] public List<ZoneInfoDto> Zones { get; set; } = new();
}
```

### Beispiel Payload

```csharp
var response = new ZoneListResponse
{
    Zones = new List<ZoneInfoDto>
    {
        new ZoneInfoDto
        {
            ZoneId = 1001,
            Name = "Elwynn Forest",
            ZoneType = ZoneType.Outdoor,
            RecommendedMinLevel = 1,
            RecommendedMaxLevel = 10,
            IsDiscovered = true,
            HasFlightPath = true,
            FlightPathPosition = new Position(100f, 200f, 0f, 1001),
            IsPvPZone = false
        },
        new ZoneInfoDto
        {
            ZoneId = 1005,
            Name = "Darkwood Forest",
            ZoneType = ZoneType.Outdoor,
            RecommendedMinLevel = 15,
            RecommendedMaxLevel = 25,
            IsDiscovered = false, // Noch nicht entdeckt
            HasFlightPath = true,
            IsPvPZone = true,
            IsContested = true
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

Server antwortet mit `ZoneState` (102) - es gibt keine separate `GetZoneResponse` mehr.

```csharp
// Server-Handler
public void HandleGetZoneRequest(ClientConnection conn, GetZoneRequest request)
{
    var player = GetPlayer(conn);
    var zone = _zoneManager.GetZone(request.ZoneId);

    if (zone == null)
    {
        // Error-Handling
        SendError(conn, "ZONE_NOT_FOUND");
        return;
    }

    // ZoneState direkt senden
    var zoneState = new ZoneState
    {
        ZoneId = zone.Id,
        ZoneName = zone.Name,
        ZoneType = zone.Type,
        Weather = zone.CurrentWeather,
        TimeOfDay = zone.TimeOfDay,
        StateType = ZoneStateType.Initial,
        MyPlayer = PlayerEntityDto.FromEntity(player),
        Entities = zone.GetVisibleEntities(player).Select(e => e.ToDto()).ToList(),
        HasMoreEntities = false,
        TotalEntityCount = zone.EntityCount
    };

    Send(conn, zoneState);
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

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.ZoneLoadedAck)]
public class ZoneLoadedAck : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.ZoneLoadedAck;
    [Key(1)] public ushort ZoneId { get; set; }
    [Key(2)] public int? LoadTimeMs { get; set; }
}
```

### Client-Logik

```csharp
public async Task LoadZoneAsync(ZoneState zoneState)
{
    var stopwatch = Stopwatch.StartNew();

    // 1. Assets laden (Texturen, Models, etc.)
    await _assetLoader.LoadZoneAssetsAsync(zoneState. ZoneId);

    // 2. ZoneState anwenden (Entities spawnen)
    _zoneManager.ApplyZoneState(zoneState);

    // 3. Gebufferte Messages abarbeiten
    _messageBuffer.ProcessAll();

    stopwatch.Stop();

    // 4. Server informieren - JETZT bereit für Updates!
    _networkClient.Send(new ZoneLoadedAck
    {
        ZoneId = zoneState.ZoneId,
        LoadTimeMs = (int)stopwatch.ElapsedMilliseconds
    });
}
```

### Server-Logik

```csharp
public void HandleZoneLoadedAck(ClientConnection conn, ZoneLoadedAck ack)
{
    var player = GetPlayer(conn);

    // Validierung
    if (player. CurrentZoneId != ack.ZoneId)
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
    var now = DateTime. UtcNow;

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

| Feld        | Typ                | Beschreibung                 | Pflicht |
| ----------- | ------------------ | ---------------------------- | ------- |
| Type        | MessageType        | `MessageType.EntityBatch`    | Ja      |
| ZoneId      | ushort             | Zone-ID zur Validierung      | Ja      |
| BatchIndex  | int                | Batch-Nummer (1, 2, 3, .. .) | Ja      |
| Entities    | List\<IEntityDto\> | Weitere Entities (max 100)   | Ja      |
| IsLastBatch | bool               | Ist dies der letzte Batch?   | Ja      |

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.EntityBatch)]
public class EntityBatch : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.EntityBatch;
    [Key(1)] public ushort ZoneId { get; set; }
    [Key(2)] public int BatchIndex { get; set; }
    [Key(3)] public List<IEntityDto> Entities { get; set; } = new();
    [Key(4)] public bool IsLastBatch { get; set; }
}
```

### Chunking-Flow Beispiel

```
Zone mit 250 Entities:

1. ZoneState (102)
   ├── MyPlayer:  PlayerEntityDto
   ├── Entities: [Entity 0-99]     (100 Entities)
   ├── HasMoreEntities: true
   └── TotalEntityCount: 250

2. EntityBatch (120)
   ├── BatchIndex: 1
   ├── Entities: [Entity 100-199]  (100 Entities)
   └── IsLastBatch: false

3. EntityBatch (120)
   ├── BatchIndex: 2
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
    var firstChunk = allEntities.Take(CHUNK_SIZE).Select(e => e.ToDto()).ToList();
    var remainingEntities = allEntities.Skip(CHUNK_SIZE).ToList();

    // 1. ZoneState mit erstem Chunk
    var zoneState = new ZoneState
    {
        ZoneId = zone.Id,
        ZoneName = zone.Name,
        StateType = ZoneStateType.Initial,
        MyPlayer = PlayerEntityDto.FromEntity(player),
        Entities = firstChunk,
        HasMoreEntities = remainingEntities.Any(),
        TotalEntityCount = allEntities.Count
    };
    Send(conn, zoneState);

    // 2. Weitere Batches
    int batchIndex = 1;
    while (remainingEntities. Any())
    {
        var batch = remainingEntities.Take(CHUNK_SIZE).ToList();
        remainingEntities = remainingEntities.Skip(CHUNK_SIZE).ToList();

        var entityBatch = new EntityBatch
        {
            ZoneId = zone.Id,
            BatchIndex = batchIndex++,
            Entities = batch. Select(e => e.ToDto()).ToList(),
            IsLastBatch = ! remainingEntities.Any()
        };
        Send(conn, entityBatch);
    }
}
```

### Client-Handling

```csharp
private int _expectedBatches;
private int _receivedBatches;
private List<IEntityDto> _pendingEntities = new();

public void OnZoneState(ZoneState state)
{
    // Initiale Entities speichern
    _pendingEntities.AddRange(state.Entities);

    if (! state.HasMoreEntities)
    {
        // Keine weiteren Batches, direkt laden
        ApplyEntitiesAndStartLoading();
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
    if (batch.ZoneId != _currentZoneId)
    {
        _log.Warn("EntityBatch for wrong zone");
        return;
    }

    // Entities hinzufügen
    _pendingEntities.AddRange(batch. Entities);
    _receivedBatches++;

    if (batch.IsLastBatch)
    {
        ApplyEntitiesAndStartLoading();
    }
}

private void ApplyEntitiesAndStartLoading()
{
    _zoneManager.SpawnEntities(_pendingEntities);
    _pendingEntities.Clear();
    StartAssetLoading();
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
// Mmo. Shared/Zone/Enums/LeaveReason.cs
public enum LeaveReason :  byte
{
    Logout = 1,
    ExitGame = 2,
    CharacterSwitch = 3
}

// Mmo. Shared/Zone/Enums/PlayerLeftReason.cs
public enum PlayerLeftReason : byte
{
    Logout = 1,
    Transfer = 2,
    Disconnect = 3,
    Death = 4,
    Kicked = 5,
    Banned = 6
}

// Mmo.Shared/Zone/Enums/TransferType.cs
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

// Mmo. Shared/Zone/Enums/ZoneStateType.cs
public enum ZoneStateType : byte
{
    Initial = 1,
    Transfer = 2,
    Reconnect = 3,
    FullSync = 4
}

// Mmo. Shared/Zone/Enums/ZoneType.cs
public enum ZoneType : byte
{
    Outdoor = 1,
    Dungeon = 2,
    City = 3,
    Instance = 4,
    Arena = 5,
    Battleground = 6,
    Sanctuary = 7,
    GhostZone = 8
}

// Mmo.Shared/Zone/Enums/WeatherType.cs
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

---

**Letzte Aktualisierung:** 2025-12-27  
**Version:** 2.0.0

[← Zurück zur Übersicht](README)
