# 🗺️ Zone Events Messages (0100-0199)

**Kategorie:** 1  
**Range:** 0100-0199  
**Phase:** Prototyp  
**Status:** 🟢 In Entwicklung

[← Zurück zur Übersicht](README.md)

---

## 📋 Inhaltsverzeichnis

-   [Zone Loading Flow (Übersicht)](#-zone-loading-flow-übersicht)
-   [JoinZone (100)](#joinzone-100)
-   [LeaveZone (101)](#leavezone-101)
-   [ZoneState (102)](#zonestate-102)
-   [PlayerJoinedZone (103)](#playerjoinedzone-103)
-   [PlayerLeftZone (104)](#playerleftzone-104)
-   [ZoneTransferRequest (105)](#zonetransferrequest-105)
-   [ZoneTransferResponse (106)](#zonetransferresponse-106)
-   [ZoneLoadingProgress (107)](#zoneloadingprogress-107)
-   [ZoneDiscovered (108)](#zonediscovered-108)
-   [ZoneListRequest (109)](#zonelistrequest-109)
-   [ZoneListResponse (110)](#zonelistresponse-110)
-   [ShardTransfer (111)](#shardtransfer-111)
-   [ShardListRequest (112)](#shardlistrequest-112)
-   [ShardListResponse (113)](#shardlistresponse-113)
-   [SubZoneEnter (114)](#subzoneenter-114)
-   [SubZoneLeave (115)](#subzoneleave-115)
-   [ZonePhaseChange (116)](#zonephasechange-116)
-   [GetZoneRequest (117)](#getzonerequest-117)
-   [GetZoneResponse (118)](#getzoneresponse-118)
-   [ZoneLoadedAck (119)](#zoneloadedack-119)

---

## 🔄 Zone Loading Flow (Übersicht)

Der Zone-Loading-Prozess wurde vereinfacht. Eine einzige `ZoneState` Message enthält alle Daten die der Client zum Spawnen braucht.

### Vereinfachter Flow

```
Client                         Server
  │                              │
  │  CharacterSelectResponse     │
  │  (SpawnZoneId: 1001)         │
  │◄─────────────────────────────│
  │                              │
  │  GetZoneRequest (117)        │
  │  "Gib mir Zone 1001"         │
  │─────────────────────────────►│
  │                              │
  │  ZoneState (102)             │  ← ALLES in einer Message!
  │  ├── Zone-Metadaten          │
  │  ├── MyPlayer: PlayerEntityDto │
  │  └── Entities: List<IEntityDto>│
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

### Message-Übersicht

| Message | ID | Richtung | Status |
|---------|-----|----------|--------|
| `GetZoneRequest` | 117 | Client → Server | ✅ Aktiv |
| `ZoneState` | 102 | Server → Client | ✅ Erweitert (MyPlayer + Entities) |
| `ZoneLoadedAck` | 119 | Client → Server | ✅ **NEU** |
| `GetZoneResponse` | 118 | Server → Client | ❌ **OBSOLET** (in ZoneState integriert) |
| `JoinZone` | 100 | Server → Client | ❌ **OBSOLET** (in ZoneState integriert) |

### Vorteile des neuen Flows

- **4 Messages statt 6+** - Einfacherer Flow
- **Atomarer State-Snapshot** - Keine Race Conditions
- **Client-Kontrolle** - ZoneLoadedAck bestätigt Bereitschaft
- **Server wartet** - Keine Updates während Client lädt

---

## JoinZone (100)

> ⚠️ **OBSOLET** - Diese Message wurde in `ZoneState` (102) integriert.
> `MyPlayer` in `ZoneState` ersetzt die Funktionalität von `JoinZone`.
> Siehe [Zone Loading Flow](#-zone-loading-flow-übersicht) für den neuen Prozess.

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Server informiert Client dass Character in eine Zone gespawnt wird. Enthält Zone-ID, Spawn-Position und initiale Informationen.

**Flow:** Nach `CharacterSelectResponse` sendet Client zuerst `GetZoneRequest` (117) um Zone-Metadaten zu laden und Assets vorzubereiten. Wenn bereit, sendet Server dann `JoinZone` mit vollständigem PlayerEntity.

### Im Scope ✅

-   Zone-ID und Name
-   Spawn-Position (X, Y, Z)
-   Zone-Type (Outdoor, Dungeon, City, etc.)
-   **Vollständiges PlayerEntity mit allen gameplay-relevanten Daten**
-   **Equipment-Snapshot für korrektes Character-Rendering**
-   **Aktive Buffs/Debuffs für UI-Anzeige**
-   **Cooldowns für Ability-Verfügbarkeit**

### Nicht im Scope ❌

-   Vollständiger ZoneState mit anderen Spielern → verwende `ZoneState` (102)
-   Entity-Liste → Server sendet separate `EntitySpawn` Messages (1400)
-   **Vollständiges Inventory** → verwende `InventorySync` (1100)
-   **Quest-Log** → verwende `QuestSync` (1000)
-   **Skill-Tree Details** → verwende `SkillSync` (800)

### Payload

| Feld       | Typ             | Beschreibung                             | Pflicht |
| ---------- | --------------- | ---------------------------------------- | ------- |
| ZoneId     | ushort          | Eindeutige Zone-ID                       | Ja      |
| ZoneName   | string          | Name der Zone                            | Ja      |
| ZoneType   | string          | "outdoor", "dungeon", "city", "instance" | Ja      |
| PlayerData | PlayerEntityDto | Kompletter Character-State               | Ja      |

**PlayerEntityDto** (siehe [DTO_ARCHITECTURE.md](DTO_ARCHITECTURE.md) für vollständige Referenz):
| Feld | Typ | Beschreibung |
|------|-----|--------------|
| RuntimeId | EntityIdentity | Runtime Entity-ID |
| PersistentId | Guid | Persistente Character-ID |
| Position | Position | Spawn-Position (X, Y, Z) |
| DisplayName | string | Character-Name |
| Level | int | Character-Level |
| CurrentHealth | int | Aktuelle HP |
| MaxHealth | int | Maximale HP |
| CurrentResource | int | Aktuelles Mana/Energy/Rage |
| MaxResource | int | Maximales Mana/Energy/Rage |
| CombatResourceType | CombatResourceType | Typ der Ressource |
| Race | Race | Rasse |
| Class | CharacterClass | Klasse |
| Gender | Gender | Geschlecht |
| State | CharacterState | Alive/Dead/Ghost |
| IsPvpFlagged | bool | PvP-Flag aktiv? |
| IsInCombat | bool | Im Kampf? |
| MovementFlags | MovementFlags | Walking/Running Animation |
| ... | ... | (weitere Properties siehe DTO_ARCHITECTURE.md) |

> **⚠️ ServerOnly Properties (NICHT im DTO enthalten):**
>
> -   `Experience` - Cheating-Prevention
> -   `Gold` - Sicherheitskritisch
> -   `AccountId` - Privacy
> -   `AttackPower`, `Armor` - Server-interne Berechnungen

### Erwartete Response

-   Client sendet `PositionUpdate` (200) um Spawn zu bestätigen
-   Danach: Client empfängt `ZoneState` (102) mit anderen Entities

### Verwandte Messages

| Message              | ID   | Beziehung                                         |
| -------------------- | ---- | ------------------------------------------------- |
| `ZoneState`          | 102  | Enthält kompletten Zone-State                     |
| `LeaveZone`          | 101  | Verlassen der Zone                                |
| `CharacterSelect`    | 9    | Auslöser für JoinZone                             |
| `GetZoneRequest`     | 117  | Wird vor JoinZone gesendet (Zone-Metadaten laden) |
| `EntitySpawn`        | 1400 | Andere Spieler/NPCs in Zone                       |
| `InventorySync`      | 1100 | Vollständiges Inventory (separat)                 |
| `QuestSync`          | 1000 | Quest-Log (separat)                               |
| `SkillSync`          | 800  | Skill-Tree (separat)                              |
| `FriendListResponse` | 2105 | Freundesliste (separat)                           |

### Flow-Diagramm

```
Client                    Zone Server
  │                          │
  │  CharacterSelect (9)     │
  │─────────────────────────►│
  │                          │  Load Character
  │                          │  Find Spawn Point
  │                          │
  │  CharacterSelectResponse │
  │  (SpawnZoneId: 1001)     │
  │◄─────────────────────────│
  │                          │
  │  GetZoneRequest (117)    │
  │  (ZoneId: 1001)          │
  │─────────────────────────►│
  │                          │
  │  GetZoneResponse (118)   │
  │  with ZoneState          │
  │◄─────────────────────────│
  │                          │
  │  Load Zone Assets        │
  │  (Textures, Models...)   │
  │                          │
  │  Assets Loaded,          │
  │  Ready to Spawn          │
  │                          │
  │  JoinZone (100)          │
  │◄─────────────────────────│
  │                          │
  │  PositionUpdate (200)    │
  │─────────────────────────►│
  │                          │
  │  EntitySpawn (1400) x N  │
  │◄─────────────────────────│
```

### Beispiel Payload

```csharp
var joinZone = new JoinZone
{
    Type = MessageType.JoinZone,
    ZoneId = 1001,
    ZoneName = "Elwynn Forest",
    ZoneType = "outdoor",
    PlayerData = new PlayerEntityDto
    {
        RuntimeId = new EntityIdentity(1, 1001, 50001, 0, 1),
        PersistentId = characterGuid,
        Position = new Position(100.5f, 250.0f, 10.0f, 1001),
        DisplayName = "Alice",
        Level = 10,
        CurrentHealth = 850,
        MaxHealth = 1000,
        CurrentResource = 200,
        MaxResource = 300,
        CombatResourceType = CombatResourceType.Mana,
        Race = Race.Human,
        Class = CharacterClass.Mage,
        Gender = Gender.Female
        // Experience, Gold, AccountId sind NICHT enthalten (ServerOnly)
    }
};
```

### Notizen

-   Loading-Screen im Client während Zone-Load
-   Client lädt Zone-Assets basierend auf ZoneId (nach `GetZoneRequest`)
-   Nach JoinZone: Server sendet andere Spieler als `EntitySpawn` (1400)
-   Spawn-Position ist entweder: Last-Position, Hearthstone, oder Zone-Default
-   **Empfohlener Flow:** `CharacterSelectResponse` → `GetZoneRequest` → Asset Loading → `JoinZone`
-   **PlayerEntityDto enthält alle Daten für sofortiges Gameplay** - Client muss keine weiteren Requests für Basis-Daten senden
-   **Nicht enthalten in PlayerEntityDto** (separate Messages):
    -   Vollständiges Inventory → `InventorySync` (1100)
    -   Quest-Log → `QuestSync` (1000)
    -   Skill-Tree → `SkillSync` (800)
    -   Freundesliste → `FriendListResponse` (2105)
-   **Geschätzte Message-Größe**: ~1-2 KB (akzeptabel für seltene Zone-Joins)
-   Bei Reconnect: Cooldowns werden korrekt wiederhergestellt

---

## LeaveZone (101)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Client informiert Server dass Spieler die Zone verlassen will (Portal, Hearthstone, etc.). Server speichert State und transferiert zu neuer Zone.

### Im Scope ✅

-   Freiwilliges Zone-Verlassen
-   State-Speicherung
-   Cleanup von Zone-Resources

### Nicht im Scope ❌

-   Zone-Transfer → wird durch `ZoneTransferRequest` (105) gehandhabt
-   Logout → verwende `LogoutRequest` (3)

### Request Payload

| Feld         | Typ    | Beschreibung                               | Pflicht |
| ------------ | ------ | ------------------------------------------ | ------- |
| Reason       | string | "logout", "portal", "hearthstone", "death" | Ja      |
| TargetZoneId | ushort | Ziel-Zone (falls bekannt)                  | Nein    |

### Erwartete Response

-   **Bei Zone-Transfer:** Server sendet `ZoneState` (102) für die neue Zone
-   **Bei Logout:** Connection wird geschlossen

### Flow bei Zone-Transfer

```
Client                         Server
  │                              │
  │  LeaveZone (101)             │
  │  Reason:  "portal"            │
  │  TargetZoneId:  2001          │
  │─────────────────────────────►│
  │                              │
  │  [Server: PlayerLeftZone     │
  │   broadcast an alte Zone]    │
  │                              │
  │  ZoneState (102)             │  ← Neue Zone!
  │  ZoneId:  2001                │
  │  StateType: Transfer         │
  │  MyPlayer: PlayerEntityDto   │
  │◄─────────────────────────────│
  │                              │
  │  [Client lädt Assets]        │
  │                              │
  │  ZoneLoadedAck (119)         │
  │─────────────────────────────►│
  │                              │
  │  [Server startet Updates]    │
```

### Verwandte Messages

| Message               | ID  | Beziehung                                    |
| --------------------- | --- | -------------------------------------------- |
| `ZoneState`           | 102 | Neue Zone betreten (mit StateType: Transfer) |
| `ZoneLoadedAck`       | 119 | Client bestätigt Zone-Load                   |
| `PlayerLeftZone`      | 104 | Broadcast an andere Spieler in alter Zone    |
| `ZoneTransferRequest` | 105 | Expliziter Zone-Transfer                     |

### Beispiel Payload

```csharp
var leaveZone = new LeaveZone
{
    Type = MessageType.LeaveZone,
    Reason = "portal",
    TargetZoneId = 2001
};
```

### Notizen

-   Server broadcastet `PlayerLeftZone` (104) an alle Spieler in der alten Zone
-   Character wird aus alter Zone-Entity-Liste entfernt
-   Position wird in DB gespeichert
-   Bei Zone-Transfer: `ZoneState` (102) mit `StateType = Transfer` wird gesendet

---

## ZoneState (102)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten (Initial Load, Zone Transfer, Periodic Sync)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Kompletter Snapshot des Zone-States. Dies ist die **Haupt-Message für Zone-Loading** und enthält:
- Zone-Metadaten (Name, Typ, Wetter, etc.)
- **MyPlayer**: Dein Character als PlayerEntityDto
- **Entities**: Alle anderen Entities in der Zone

### StateType Enum

| Wert | Beschreibung |
|------|--------------|
| `Initial` (1) | Erster Login, Character spawnt zum ersten Mal |
| `Transfer` (2) | Zone-Wechsel durch Portal/Teleport |
| `FullSync` (3) | Periodischer Full-Sync (alle 60s) |
| `Reconnect` (4) | Nach Verbindungsabbruch |

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.ZoneState` | Ja |
| Timestamp | long | Server-Timestamp | Ja |
| ZoneId | ushort | Zone-ID | Ja |
| ZoneName | string | Name der Zone | Ja |
| ZoneType | string | "outdoor", "dungeon", "city", "instance" | Ja |
| Weather | string | "sunny", "rain", "snow", "fog" | Ja |
| TimeOfDay | float | 0.0-24.0 (Stunden) | Ja |
| StateType | ZoneStateType | Initial, Transfer, FullSync, Reconnect | Ja |
| MyPlayer | PlayerEntityDto? | Dein Character (null bei FullSync) | Bei Initial/Transfer |
| Entities | List\<IEntityDto\> | Alle Entities (max 100 pro Message) | Ja |
| HasMoreEntities | bool | Gibt es weitere Entity-Batches? | Ja |
| TotalEntityCount | int | Gesamtzahl Entities in Zone | Ja |

### Code-Beispiel

```csharp
[MessagePackObject]
public class ZoneState : ITimestampedServerMessage
{
    [Key(0)] public MessageType Type => MessageType.ZoneState;
    [Key(1)] public long Timestamp { get; set; }
    
    // Zone-Metadaten
    [Key(2)] public ushort ZoneId { get; set; }
    [Key(3)] public string ZoneName { get; set; }
    [Key(4)] public string ZoneType { get; set; }
    [Key(5)] public string Weather { get; set; }
    [Key(6)] public float TimeOfDay { get; set; }
    
    // State-Type
    [Key(7)] public ZoneStateType StateType { get; set; }
    
    // Dein Character (null bei FullSync)
    [Key(8)] public PlayerEntityDto? MyPlayer { get; set; }
    
    // Entities (max ~100 pro Message)
    [Key(9)] public List<IEntityDto> Entities { get; set; } = new();
    
    // Chunking
    [Key(10)] public bool HasMoreEntities { get; set; }
    [Key(11)] public int TotalEntityCount { get; set; }
}

public enum ZoneStateType : byte
{
    Initial = 1,
    Transfer = 2,
    FullSync = 3,
    Reconnect = 4
}
```

### Beispiel Payload

```csharp
var zoneState = new ZoneState
{
    Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
    ZoneId = 1001,
    ZoneName = "Elwynn Forest",
    ZoneType = "outdoor",
    Weather = "sunny",
    TimeOfDay = 14.5f,
    StateType = ZoneStateType.Initial,
    MyPlayer = PlayerEntityDto.FromEntity(playerEntity),
    Entities = zone.GetAllEntities()
        .Where(e => e.PersistentId != playerEntity.PersistentId)
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
ZoneState (102)       → MyPlayer + erste 100 Entities + HasMoreEntities=true
EntityBatch (120)     → nächste 100 Entities
EntityBatch (120)     → letzte 50 Entities + IsLast=true
ZoneLoadedAck (119)   → Client ready
```

### Verwandte Messages

| Message | ID | Beziehung |
|---------|-----|-----------|
| `GetZoneRequest` | 117 | Request der ZoneState auslöst |
| `ZoneLoadedAck` | 119 | Client-Bestätigung nach ZoneState |
| `EntityBatch` | 120 | Weitere Entities bei Chunking |
| `EntitySpawn` | 1400 | Einzelne Entity spawnt später |
| `EntityDespawn` | 1402 | Entity verlässt Zone |

### Use-Cases

| Use-Case | StateType | MyPlayer | Entities |
|----------|-----------|----------|----------|
| Login | Initial | ✅ Dein Character | ✅ Alle |
| Portal/Teleport | Transfer | ✅ Dein Character | ✅ Alle in neuer Zone |
| Reconnect | Reconnect | ✅ Restored Character | ✅ Alle |
| Periodic Sync | FullSync | ❌ null | ✅ Alle (Sync-Check) |

---

## PlayerJoinedZone (103)

**Richtung:** 📡 Broadcast (Server → All Clients in Zone)  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Broadcast an alle Spieler in Zone wenn ein neuer Spieler spawnt. Ermöglicht Clients den neuen Spieler anzuzeigen.

### Im Scope ✅

-   Neuer Spieler-Informationen
-   Spawn-Position
-   Basic-Stats (Level, Klasse, etc.)

### Nicht im Scope ❌

-   Vollständige Character-Stats → verwende `InspectRequest` (3300)
-   Equipment-Details → verwende `InspectEquipment` (3302)

### Payload

| Feld       | Typ             | Beschreibung                    | Pflicht |
| ---------- | --------------- | ------------------------------- | ------- |
| PlayerData | PlayerEntityDto | Komplette Spieler-Informationen | Ja      |

**PlayerEntityDto** (siehe [DTO_ARCHITECTURE.md](DTO_ARCHITECTURE.md) für vollständige Referenz):

Das DTO enthält alle sichtbaren Informationen über den neuen Spieler:

-   Runtime- und Persistent-IDs
-   Position und Movement
-   Display-Name, Level, Race, Class
-   Health/Resource Status
-   Combat-State
-   **NICHT** enthalten: Experience, Gold, AccountId (ServerOnly)

### Beispiel Payload

```csharp
var playerJoined = new PlayerJoinedZone
{
    Type = MessageType.PlayerJoinedZone,
    PlayerData = new PlayerEntityDto
    {
        RuntimeId = new EntityIdentity(1, 1001, 50002, 0, 1),
        PersistentId = characterGuid,
        CharacterId = characterGuid,
        DisplayName = "Gimli",
        Level = 12,
        Race = Race.Dwarf,
        Class = CharacterClass.Warrior,
        Position = new Position(98.5f, 255.0f, 10.2f, 1001),
        CurrentHealth = 800,
        MaxHealth = 1200,
        CurrentResource = 100,
        MaxResource = 100,
        CombatResourceType = CombatResourceType.Rage
    }
};
```

### Verwandte Messages

| Message          | ID   | Beziehung                       |
| ---------------- | ---- | ------------------------------- |
| `JoinZone`       | 100  | Auslöser für diesen Broadcast   |
| `PlayerLeftZone` | 104  | Gegenstück beim Verlassen       |
| `EntitySpawn`    | 1400 | Generische Entity-Spawn Message |

### Notizen

-   Wird NUR an bereits anwesende Spieler gebroadcastet
-   Der joinierende Spieler selbst empfängt `JoinZone` (100)
-   Client fügt Spieler zur lokalen Entity-Liste hinzu

---

## PlayerLeftZone (104)

**Richtung:** 📡 Broadcast (Server → All Clients in Zone)  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Broadcast an alle Spieler wenn ein Spieler die Zone verlässt. Client entfernt den Spieler aus der Entity-Liste.

### Im Scope ✅

-   Entity-ID des verlassenden Spielers
-   Grund (optional)

### Nicht im Scope ❌

-   Ziel-Zone (Privacy)

### Payload

| Feld     | Typ    | Beschreibung                                | Pflicht |
| -------- | ------ | ------------------------------------------- | ------- |
| PlayerId | Guid   | Persistent Player-ID                        | Ja      |
| Reason   | string | "logout", "transfer", "disconnect", "death" | Ja      |

**Hinweis:** PlayerId entspricht `PlayerEntityDto.PersistentId` für konsistente ID-Referenzierung.

### Beispiel Payload

```csharp
var playerLeft = new PlayerLeftZone
{
    Type = MessageType.PlayerLeftZone,
    PlayerId = playerGuid, // Persistent ID
    Reason = "transfer"
};
```

### Verwandte Messages

| Message            | ID   | Beziehung                                   |
| ------------------ | ---- | ------------------------------------------- |
| `LeaveZone`        | 101  | Client-Request der diesen Broadcast auslöst |
| `PlayerJoinedZone` | 103  | Gegenstück beim Betreten                    |
| `EntityDespawn`    | 1402 | Generische Entity-Despawn Message           |

### Notizen

-   Client entfernt Spieler-Entity aus Render-Liste
-   Bei "disconnect": Spieler bleibt 30s "geistern" (Reconnect Window)

---

## ZoneTransferRequest (105)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Expliziter Request um zu anderer Zone zu wechseln (Portal, Teleport, Dungeon-Eingang).

### Im Scope ✅

-   Ziel-Zone-ID
-   Transfer-Typ (Portal, Teleport, etc.)
-   Position in Ziel-Zone (falls bekannt)

### Nicht im Scope ❌

-   Forced Transfer (Server-initiiert) → Server sendet direkt `JoinZone` (100)

### Request Payload

| Feld         | Typ    | Beschreibung                             | Pflicht |
| ------------ | ------ | ---------------------------------------- | ------- |
| TargetZoneId | int    | Ziel-Zone-ID                             | Ja      |
| TransferType | string | "portal", "teleport", "dungeon_entrance" | Ja      |
| TargetX      | float  | Ziel-X (falls bekannt)                   | Nein    |
| TargetY      | float  | Ziel-Y (falls bekannt)                   | Nein    |

### Erwartete Response

-   **Bei Erfolg:** `ZoneTransferResponse` (106) → dann `JoinZone` (100)
-   **Bei Fehler:** `ZoneTransferResponse` (106) mit ErrorCode

### Verwandte Messages

| Message                | ID  | Beziehung                        |
| ---------------------- | --- | -------------------------------- |
| `ZoneTransferResponse` | 106 | Response zu diesem Request       |
| `JoinZone`             | 100 | Nach erfolgreichem Transfer      |
| `ZoneLoadingProgress`  | 107 | Loading-Updates während Transfer |

### Beispiel Payload

```csharp
var transferRequest = new ZoneTransferRequest
{
    Type = MessageType.ZoneTransferRequest,
    TargetZoneId = 2001,
    TransferType = "dungeon_entrance",
    TargetX = 50.0f,
    TargetY = 50.0f
};
```

### Error Codes

| Code             | Bedeutung                       | Aktion            |
| ---------------- | ------------------------------- | ----------------- |
| `ZONE_NOT_FOUND` | Ziel-Zone existiert nicht       | Fehler anzeigen   |
| `ZONE_LOCKED`    | Zone ist gesperrt (Maintenance) | Warten            |
| `LEVEL_TOO_LOW`  | Character-Level zu niedrig      | Level erhöhen     |
| `QUEST_REQUIRED` | Quest erforderlich              | Quest abschließen |
| `IN_COMBAT`      | Im Kampf                        | Kampf beenden     |

### Notizen

-   Server validiert Transfer-Berechtigung (Level, Quests, etc.)
-   Loading-Screen während Transfer

---

## ZoneTransferResponse (106)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Bestätigung oder Ablehnung eines Zone-Transfer-Requests.

### Response Payload

| Feld              | Typ    | Beschreibung            | Pflicht    |
| ----------------- | ------ | ----------------------- | ---------- |
| Success           | bool   | Transfer erlaubt?       | Ja         |
| ErrorCode         | string | Fehlercode falls Failed | Nein       |
| ErrorMessage      | string | Fehlermeldung           | Nein       |
| EstimatedLoadTime | int    | Sekunden (ca.)          | Bei Erfolg |

### Beispiel Payload

```csharp
var transferResponse = new ZoneTransferResponse
{
    Type = MessageType.ZoneTransferResponse,
    Success = true,
    EstimatedLoadTime = 3
};
```

### Notizen

-   Bei Success: Client zeigt Loading-Screen
-   Während Load: Server sendet `ZoneLoadingProgress` (107)

---

## ZoneLoadingProgress (107)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig (während Loading)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Progress-Updates während Zone-Loading (Asset-Loading, State-Sync, etc.).

### Payload

| Feld     | Typ    | Beschreibung                                  | Pflicht |
| -------- | ------ | --------------------------------------------- | ------- |
| Progress | float  | 0.0 - 1.0 (0% - 100%)                         | Ja      |
| Status   | string | "loading_assets", "syncing_state", "spawning" | Ja      |

### Beispiel Payload

```csharp
var loadingProgress = new ZoneLoadingProgress
{
    Type = MessageType.ZoneLoadingProgress,
    Progress = 0.65f,
    Status = "syncing_state"
};
```

### Notizen

-   Client zeigt Progress-Bar im Loading-Screen
-   Bei Progress=1.0: Kurz danach folgt `JoinZone` (100)

---

## ZoneDiscovered (108)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Benachrichtigung dass Spieler eine neue Zone entdeckt hat (Achievement, XP-Bonus).

### Payload

| Feld     | Typ    | Beschreibung                | Pflicht |
| -------- | ------ | --------------------------- | ------- |
| ZoneId   | int    | Entdeckte Zone-ID           | Ja      |
| ZoneName | string | Zone-Name                   | Ja      |
| XpBonus  | int    | XP-Belohnung für Entdeckung | Ja      |

### Beispiel Payload

```csharp
var zoneDiscovered = new ZoneDiscovered
{
    Type = MessageType.ZoneDiscovered,
    ZoneId = 1005,
    ZoneName = "Darkwood Forest",
    XpBonus = 150
};
```

### Verwandte Messages

| Message               | ID   | Beziehung                          |
| --------------------- | ---- | ---------------------------------- |
| `AchievementUnlocked` | 1900 | Kann gleichzeitig ausgelöst werden |
| `XpGain`              | 601  | XP-Bonus für Entdeckung            |

### Notizen

-   Wird beim ersten Betreten einer Zone ausgelöst
-   Client zeigt Discovery-UI

---

## ZoneListRequest (109)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Fordert Liste aller bekannten/entdeckten Zonen an.

### Request Payload

Keine zusätzlichen Felder

### Erwartete Response

-   **Immer:** `ZoneListResponse` (110)

---

## ZoneListResponse (110)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung

Liste aller vom Character entdeckten Zonen.

### Response Payload

| Feld  | Typ            | Beschreibung    | Pflicht |
| ----- | -------------- | --------------- | ------- |
| Zones | List<ZoneInfo> | Entdeckte Zonen | Ja      |

**ZoneInfo**:
| Feld | Typ | Beschreibung |
|------|-----|--------------|
| ZoneId | int | Zone-ID |
| Name | string | Zone-Name |
| Level | int | Empfohlenes Level |
| Discovered | bool | Bereits entdeckt? |

### Notizen

-   Für Map-UI und Fast-Travel Funktionen

---

## ShardTransfer (111)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

**Phase 2 Feature** - Transfer zu anderem Shard (Zone-Instance) für Load-Balancing.

### Notizen

-   Im Prototyp: Nicht implementiert (Single-Shard)
-   Phase 2: Multi-Shard Support für Skalierung

---

## ShardListRequest (112)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

**Phase 2 Feature** - Liste aller Shards für aktuelle Zone.

### Notizen

-   Im Prototyp: Nicht implementiert

---

## ShardListResponse (113)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung

**Phase 2 Feature** - Antwort mit Shard-Informationen.

### Notizen

-   Im Prototyp: Nicht implementiert

---

## SubZoneEnter (114)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

**Phase 2 Feature** - Spieler betritt eine Sub-Zone (Bereich innerhalb einer Zone, z.B. "Goldshire" in "Elwynn Forest").

### Payload

| Feld        | Typ    | Beschreibung      | Pflicht |
| ----------- | ------ | ----------------- | ------- |
| SubZoneId   | int    | Sub-Zone-ID       | Ja      |
| SubZoneName | string | Name der Sub-Zone | Ja      |

### Notizen

-   UI-Update: Zeige Sub-Zone-Name im HUD

---

## SubZoneLeave (115)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

**Phase 2 Feature** - Spieler verlässt Sub-Zone.

### Payload

| Feld      | Typ | Beschreibung           | Pflicht |
| --------- | --- | ---------------------- | ------- |
| SubZoneId | int | Verlassene Sub-Zone-ID | Ja      |

---

## ZonePhaseChange (116)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

**Phase 2 Feature** - Zone ändert Phase (Quest-Fortschritt, Story-Events). Spieler sehen unterschiedliche Versionen der gleichen Zone.

### Payload

| Feld      | Typ    | Beschreibung  | Pflicht |
| --------- | ------ | ------------- | ------- |
| ZoneId    | int    | Zone-ID       | Ja      |
| PhaseId   | int    | Neue Phase-ID | Ja      |
| PhaseName | string | Phase-Name    | Ja      |

### Beispiel

```csharp
// Nach Quest "Defeat the Dragon"
var phaseChange = new ZonePhaseChange
{
    Type = MessageType.ZonePhaseChange,
    ZoneId = 1001,
    PhaseId = 2,
    PhaseName = "Post-Dragon Victory"
};
```

### Notizen

-   Server transferiert Spieler zu anderer Phase
-   Kann andere Spieler, NPCs, und Objekte zeigen
-   Wird durch Quest-Fortschritt ausgelöst

---

## GetZoneRequest (117)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Client fordert Zone-Metadaten an bevor er in die Zone spawnt. Dies ermöglicht dem Client, Zone-Assets (Textures, Models, etc.) vorzuladen während der Server den Spawn vorbereitet.

### Im Scope ✅

-   Zone-Metadaten abrufen (Name, Type, Weather)
-   Asset-Loading vorbereiten
-   Zone-Info cachen

### Nicht im Scope ❌

-   Spawn-Position → kommt in `JoinZone` (100)
-   PlayerEntity-Daten → kommt in `JoinZone` (100)
-   Andere Spieler/NPCs → kommt in `ZoneState` (102) und `EntitySpawn` (1400)

### Request Payload

| Feld   | Typ | Beschreibung               | Pflicht |
| ------ | --- | -------------------------- | ------- |
| ZoneId | int | Zone-ID die angefragt wird | Ja      |

### Erwartete Response

-   **Immer:** `GetZoneResponse` (118) mit `ZoneState` (102)

### Verwandte Messages

| Message                   | ID  | Beziehung                |
| ------------------------- | --- | ------------------------ |
| `GetZoneResponse`         | 118 | Response mit ZoneState   |
| `CharacterSelectResponse` | 21  | Liefert SpawnZoneId      |
| `ZoneState`               | 102 | Enthält Zone-Daten       |
| `JoinZone`                | 100 | Folgt nach Asset-Loading |

### Flow-Diagramm

```
Client                    Gateway                   Zone Server
  │                          │                          │
  │  CharacterSelectResponse │                          │
  │  (SpawnZoneId: 1001)     │                          │
  │◄─────────────────────────│                          │
  │                          │                          │
  │  GetZoneRequest (117)    │                          │
  │  ZoneId: 1001            │                          │
  │─────────────────────────►│                          │
  │                          │                          │
  │                          │  Forward Request         │
  │                          │─────────────────────────►│
  │                          │                          │
  │                          │                          │  Load Zone Data
  │                          │                          │
  │                          │  ZoneState (102)         │
  │                          │◄─────────────────────────│
  │                          │                          │
  │  GetZoneResponse (118)   │                          │
  │  with ZoneState          │                          │
  │◄─────────────────────────│                          │
  │                          │                          │
  │  Load Zone Assets        │                          │
  │  (Textures, Models...)   │                          │
  │                          │                          │
  │  Assets loaded           │                          │
  │  Ready for Spawn         │                          │
  │                          │                          │
  │                          │  Player Ready Signal     │
  │                          │─────────────────────────►│
  │                          │                          │
  │                          │  JoinZone (100)          │
  │                          │◄─────────────────────────│
  │  JoinZone (100)          │                          │
  │◄─────────────────────────│                          │
```

### Beispiel Payload

```csharp
var getZoneRequest = new GetZoneRequest
{
    Type = MessageType.GetZoneRequest,
    ZoneId = 1001
};
```

### Error Codes

| Code             | Bedeutung                   | Aktion                                |
| ---------------- | --------------------------- | ------------------------------------- |
| `ZONE_NOT_FOUND` | Zone-ID existiert nicht     | Client-Fehler, sollte nicht passieren |
| `ZONE_LOCKED`    | Zone gesperrt (Maintenance) | Wartungsmeldung anzeigen              |
| `ACCESS_DENIED`  | Keine Berechtigung für Zone | Level/Quest-Anforderung anzeigen      |

### Notizen

-   Wird nach `CharacterSelectResponse` gesendet
-   Client erhält ZoneState mit allen statischen Zone-Infos
-   Client lädt Assets während Server Spawn vorbereitet
-   Reduziert wahrgenommene Loading-Time durch paralleles Laden
-   Zone-Daten können client-seitig gecached werden

---

## GetZoneResponse (118)

> ⚠️ **OBSOLET** - Diese Message wurde in `ZoneState` (102) integriert.
> Siehe [Zone Loading Flow](#-zone-loading-flow-übersicht) für den neuen Prozess.

---

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung

Response auf `GetZoneRequest` (117). Enthält als Payload eine `ZoneState` (102) Message mit allen Zone-Metadaten.

**Hinweis:** Diese Message ist ein Wrapper um `ZoneState` (102) im Request-Response Pattern. Der eigentliche Payload ist identisch mit `ZoneState`.

### Im Scope ✅

-   Success/Error Status
-   ZoneState bei Erfolg
-   Error-Informationen bei Fehler

### Nicht im Scope ❌

-   Player-spezifische Daten → kommt in `JoinZone` (100)

### Response Payload

| Feld         | Typ       | Beschreibung                   | Pflicht    |
| ------------ | --------- | ------------------------------ | ---------- |
| Success      | bool      | Request erfolgreich?           | Ja         |
| ErrorCode    | string    | Fehlercode falls Success=false | Nein       |
| ErrorMessage | string    | Fehlermeldung                  | Nein       |
| ZoneState    | ZoneState | Zone-Daten (siehe Message 102) | Bei Erfolg |

**ZoneState** (siehe [ZoneState (102)](#zonestate-102) für vollständige Spezifikation):
| Feld | Typ | Beschreibung |
|------|-----|--------------|
| ZoneId | int | Zone-ID |
| ZoneName | string | Name der Zone |
| ZoneType | string | "outdoor", "dungeon", "city", "instance" |
| ServerTime | long | Server Unix Timestamp |
| Weather | string | "sunny", "rain", "snow", "fog" |
| TimeOfDay | float | 0.0-24.0 (Stunden) |
| Players | List<PlayerEntity> | **Leer** bei GetZoneResponse |
| NPCs | List<NpcEntity> | **Leer** bei GetZoneResponse |

**Wichtig:** Bei `GetZoneResponse` sind die `Players` und `NPCs` Listen **leer**, da der Spieler noch nicht in der Zone gespawnt ist. Diese Daten kommen später via separater `EntitySpawn` Messages.

### Verwandte Messages

| Message          | ID  | Beziehung                  |
| ---------------- | --- | -------------------------- |
| `GetZoneRequest` | 117 | Request zu dieser Response |
| `ZoneState`      | 102 | Payload dieser Response    |
| `JoinZone`       | 100 | Folgt nach dieser Response |

### Beispiel Payload

```csharp
// Erfolg
var successResponse = new GetZoneResponse
{
    Type = MessageType.GetZoneResponse,
    Success = true,
    ZoneState = new ZoneState
    {
        ZoneId = 1001,
        ZoneName = "Elwynn Forest",
        ZoneType = "outdoor",
        ServerTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
        Weather = "sunny",
        TimeOfDay = 14.5f, // 14:30
        Players = new List<PlayerEntity>(), // Leer!
        NPCs = new List<NpcEntity>()        // Leer!
    }
};

// Fehler
var errorResponse = new GetZoneResponse
{
    Type = MessageType.GetZoneResponse,
    Success = false,
    ErrorCode = "ZONE_LOCKED",
    ErrorMessage = "Zone ist wegen Wartungsarbeiten gesperrt"
};
```

### Error Codes

| Code             | Bedeutung                   |
| ---------------- | --------------------------- |
| `ZONE_NOT_FOUND` | Zone-ID existiert nicht     |
| `ZONE_LOCKED`    | Zone gesperrt (Maintenance) |
| `ACCESS_DENIED`  | Keine Berechtigung für Zone |

### Notizen

-   Wrapper um `ZoneState` (102) für Request-Response Pattern
-   Players und NPCs Listen sind immer leer
-   Client sollte Zone-Daten cachen (basierend auf ZoneId + Version)
-   Nach Erhalt: Client lädt Assets, dann bereit für `JoinZone` (100)

---

## ZoneLoadedAck (119)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten (einmal pro Zone-Load)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Client bestätigt dass Zone-Assets geladen und ZoneState verarbeitet wurde. Server startet erst nach diesem Ack die hochfrequenten Updates (PositionBroadcast, etc.).

### Warum diese Message?

| Ohne ZoneLoadedAck | Mit ZoneLoadedAck |
|--------------------|-------------------|
| Server sendet 25Hz Updates während Client lädt | Server wartet auf Client |
| Bandbreite verschwendet | Bandbreite optimal |
| Client-Buffer wächst | Kein unnötiger Buffer |
| Server weiß nicht ob Client ready | Klarer Handshake |

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.ZoneLoadedAck` | Ja |
| ZoneId | ushort | Zone-ID zur Validierung | Ja |
| LoadTimeMs | int | Wie lange hat Loading gedauert? (Metrics) | Nein |

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

### Server-Logik

```csharp
public void HandleZoneLoadedAck(ClientConnection conn, ZoneLoadedAck ack)
{
    var player = GetPlayer(conn);
    
    if (player.CurrentZoneId != ack.ZoneId)
    {
        _log.Warn("ZoneLoadedAck for wrong zone");
        return;
    }
    
    player.IsZoneReady = true;
    
    // Jetzt hochfrequente Updates senden
    _log.Info("Player {Name} ready in zone {ZoneId}, load time: {Ms}ms",
        player.Name, ack.ZoneId, ack.LoadTimeMs);
}
```

### Client-Logik

```csharp
public async Task LoadZoneAsync(ZoneState zoneState)
{
    var stopwatch = Stopwatch.StartNew();
    
    // Assets laden
    await LoadZoneAssets(zoneState.ZoneId);
    
    // ZoneState anwenden
    ApplyZoneState(zoneState);
    
    // Message-Buffer abarbeiten
    ProcessMessageBuffer();
    
    stopwatch.Stop();
    
    // Server informieren
    SendMessage(new ZoneLoadedAck
    {
        ZoneId = zoneState.ZoneId,
        LoadTimeMs = (int)stopwatch.ElapsedMilliseconds
    });
}
```

### Timeout-Handling

Server wartet maximal 30 Sekunden auf ZoneLoadedAck:

```csharp
// Server-Side
if (player.WaitingForZoneAckSince?.AddSeconds(30) < DateTime.UtcNow)
{
    // Timeout - Client reagiert nicht
    DisconnectPlayer(player, DisconnectReason.LoadingTimeout);
}
```

---

**Letzte Aktualisierung**: 2025-12-26  
**Version**: 1.1.0

[← Zurück zur Übersicht](README.md)
