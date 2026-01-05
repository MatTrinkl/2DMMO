# 👾 Entity Messages (1400-1434)

**Kategorie:** 14  
**Range:** 1400-1434 (AKTIV)  
**Phase:** Prototyp  
**Status:** 🟢 In Entwicklung

[← Zurück zur Übersicht](README.md)

---

## 📋 Inhaltsverzeichnis

- [🔄 Entity Flow](#-entity-flow)
- [🧱 DTOs / Enums](#-dtos--enums)
- [📩 Aktive Messages (1400-1434)](#-aktive-messages-1400-1434)
  - [EntitySpawn (1400)](#entityspawn-1400)
  - [EntitySpawnBatch (1401)](#entityspawnbatch-1401)
  - [EntityDespawn (1402)](#entitydespawn-1402)
  - [EntityDespawnBatch (1403)](#entitydespawnbatch-1403)
  - [EntityUpdate (1404)](#entityupdate-1404)
  - [EntityUpdateBatch (1405)](#entityupdatebatch-1405)
  - [EntityListRequest (1406)](#entitylistrequest-1406)
  - [EntityListResponse (1407)](#entitylistresponse-1407)
  - [EntityPathUpdate (1408)](#entitypathupdate-1408)
  - [EntityStateChange (1409)](#entitystatechange-1409)
  - [EntityAnimation (1410)](#entityanimation-1410)
  - [EntityAnimationBatch (1411)](#entityanimationbatch-1411)
  - [EntityNameplate (1412)](#entitynameplate-1412)
  - [EntityNameplateUpdate (1413)](#entitynameplateupdate-1413)
  - [EntityFaction (1414)](#entityfaction-1414)
  - [EntityScale (1415)](#entityscale-1415)
  - [EntityMountUpdate (1416)](#entitymountupdate-1416)
  - [EntityEquipmentUpdate (1417)](#entityequipmentupdate-1417)
  - [EntityAuraUpdate (1418)](#entityauraupdate-1418)
  - [EntityEmote (1419)](#entityemote-1419)
  - [EntitySay (1420)](#entitysay-1420)
  - [EntityYell (1421)](#entityyell-1421)
  - [LootableSpawn (1430)](#lootablespawn-1430)
  - [LootableDespawn (1431)](#lootabledespawn-1431)
  - [ResourceNodeSpawn (1432)](#resourcenodespawn-1432)
  - [ResourceNodeDespawn (1433)](#resourcenodedespawn-1433)
  - [ResourceNodeState (1434)](#resourcenodestate-1434)
- [🗑️ Obsolete Messages](#️-obsolete-messages)
- [📎 Anhang](#-anhang)

---

## 🔄 Entity Flow

### Server-Authoritative Architecture

```
┌──────────────────────────────────────────────────────────────────────┐
│                    ENTITY SYSTEM OVERVIEW                            │
├──────────────────────────────────────────────────────────────────────┤
│                                                                      │
│  ┌─────────────────┐                ┌─────────────────┐             │
│  │     CLIENT      │                │     SERVER      │             │
│  │                 │                │  (Authoritative)│             │
│  └────────┬────────┘                └────────┬────────┘             │
│           │                                  │                      │
│           │ Zone Enter                       │                      │
│           │─────────────────────────────────►│                      │
│           │                                  │ Query visible entities│
│           │◄─────────────────────────────────│ EntitySpawnBatch     │
│           │                                  │                      │
│           │                                  │ [Tick 25 Hz]         │
│           │◄─────────────────────────────────│ EntityUpdateBatch    │
│           │                                  │ (only dirty props)   │
│           │                                  │                      │
│           │◄─────────────────────────────────│ EntityAnimation      │
│           │                                  │ (instant broadcast)  │
│           │                                  │                      │
│           │ Move out of range                │                      │
│           │─────────────────────────────────►│                      │
│           │◄─────────────────────────────────│ EntityDespawn        │
│           │                                  │                      │
│  ┌────────┴────────┐                ┌────────┴────────┐             │
│  │   Local Render  │                │  Entity Manager │             │
│  │   + Interpolate │                │  + Dirty Track  │             │
│  └─────────────────┘                └─────────────────┘             │
│                                                                      │
└──────────────────────────────────────────────────────────────────────┘
```

### Entity Spawn Flow

```
    Client A             Server            Client B (in range)
        │                   │                     │
        │  Moves into       │                     │
        │  range of B       │                     │
        │─────────────────►│                     │
        │                   │ Check Visibility   │
        │                   │ (Distance < 50m)   │
        │                   │                     │
        │                   │  EntitySpawn (1400)│
        │                   │  (Client A Data)   │
        │                   │────────────────────►│
        │                   │                     │
        │                   │                     │ (Render Client A)
        │                   │                     │
```

### Entity Update Flow (Delta-Based)

```
    Server                             Client
       │                                  │
       │ [Entity HP changes]              │
       │                                  │
       │ Mark dirty: HP                   │
       │                                  │
       │ [Tick 40ms]                      │
       │                                  │
       │ EntityUpdate (1404)              │
       │ { EntityId, CurrentHP }          │
       │─────────────────────────────────►│
       │                                  │
       │                                  │ Apply delta
       │                                  │ Update HP bar
       │                                  │
```

---

## 🧱 DTOs / Enums

### EntityType (enum : byte)

```csharp
public enum EntityType : byte
{
    Unknown    = 0,
    Player     = 1,
    Npc        = 2,
    Monster    = 3,
    Object     = 4,
    Projectile = 5,
    Vehicle    = 6,
    Pet        = 7,
    Lootable   = 8,
    Resource   = 9
}
```

### EntityState (enum : byte)

```csharp
public enum EntityState : byte
{
    Idle       = 0,
    Combat     = 1,
    Dead       = 2,
    Stunned    = 3,
    Rooted     = 4,
    Sleeping   = 5,
    Fleeing    = 6,
    Evading    = 7,
    Channeling = 8
}
```

### AnimationType (enum : uint)

```csharp
public enum AnimationType : uint
{
    Idle          = 1,
    Walk          = 2,
    Run           = 3,
    AttackMelee   = 10,
    AttackRanged  = 11,
    CastSpell     = 20,
    Death         = 30,
    EmoteWave     = 40,
    EmoteDance    = 41,
    EmoteBow      = 42,
    EmoteLaugh    = 43,
    EmoteCry      = 44
}
```

### EntitySpawnDto

```csharp
[MessagePackObject]
public class EntitySpawnDto
{
    [Key(0)] public int EntityId { get; set; }
    [Key(1)] public EntityType EntityType { get; set; }
    [Key(2)] public float X { get; set; }
    [Key(3)] public float Y { get; set; }
    [Key(4)] public float Rotation { get; set; }
    [Key(5)] public string? Name { get; set; }
    [Key(6)] public int Level { get; set; }
    [Key(7)] public int CurrentHp { get; set; }
    [Key(8)] public int MaxHp { get; set; }
    [Key(9)] public EntityState State { get; set; }
    [Key(10)] public uint ModelId { get; set; }
    [Key(11)] public byte[]? Appearance { get; set; }
    [Key(12)] public float VelocityX { get; set; }
    [Key(13)] public float VelocityY { get; set; }
}
```

### EntityUpdateDto

```csharp
[MessagePackObject]
public class EntityUpdateDto
{
    [Key(0)] public int EntityId { get; set; }
    [Key(1)] public int? CurrentHp { get; set; }
    [Key(2)] public int? MaxHp { get; set; }
    [Key(3)] public int? Level { get; set; }
    [Key(4)] public EntityState? State { get; set; }
    [Key(5)] public uint? ModelId { get; set; }
    [Key(6)] public float? X { get; set; }
    [Key(7)] public float? Y { get; set; }
}
```

### Wichtige Konstanten

| Konstante         | Wert  | Beschreibung                        |
| ----------------- | ----- | ----------------------------------- |
| VISIBILITY_RANGE  | 50m   | Spawn-Radius für Entities           |
| DESPAWN_RANGE     | 55m   | Despawn-Radius (Hysteresis)         |
| PLAYER_UPDATE_HZ  | 20 Hz | Update-Frequenz für Player-Movement |
| NPC_UPDATE_HZ     | 10 Hz | Update-Frequenz für NPC-Movement    |
| DEATH_DESPAWN_SEC | 5s    | Zeit bis Corpse despawnt            |

---

## 📩 Aktive Messages (1400-1434)

---

### EntitySpawn (1400)

**Richtung:** 📡 Broadcast (Server → Nearby Clients)  
**Frequenz:** ⚡ Sehr häufig (Zone-Enter, Range-Enter)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

#### Beschreibung

Server broadcastet Entity-Spawn an alle Spieler in Sichtweite (50m). Enthält vollständige Entity-Daten für initialen Render.

#### Im Scope ✅

- Initiales Spawning aller Entity-Types (Player, NPC, Monster, Object)
- Vollständige Entity-Daten (Position, Stats, Appearance)
- Broadcast an alle Clients in Sichtweite

#### Nicht im Scope ❌

- Movement-Updates → `EntityUpdate` (1404)
- Batch-Spawn (Zone-Enter) → `EntitySpawnBatch` (1401)
- Despawn → `EntityDespawn` (1402)

#### Broadcast Payload

| Feld       | Typ         | Beschreibung                            | Pflicht |
| ---------- | ----------- | --------------------------------------- | ------- |
| EntityId   | int         | Eindeutige Entity-ID (Zone-Scope)       | Ja      |
| EntityType | byte        | EntityType enum                         | Ja      |
| X          | float       | Position X                              | Ja      |
| Y          | float       | Position Y                              | Ja      |
| Rotation   | float       | Rotation (0-360°)                       | Ja      |
| Name       | string?     | Entity-Name                             | Nein    |
| Level      | int         | Level                                   | Nein    |
| CurrentHp  | int         | Aktuelle HP                             | Nein    |
| MaxHp      | int         | Max HP                                  | Nein    |
| State      | byte        | EntityState enum                        | Ja      |
| ModelId    | uint        | Model-ID für Rendering                  | Ja      |
| Appearance | byte[]?     | Appearance-Daten (für Players)          | Nein    |
| VelocityX  | float       | Initiale Velocity X                     | Nein    |
| VelocityY  | float       | Initiale Velocity Y                     | Nein    |

#### Erwartete Response

- Keine Response erforderlich (Broadcast)

#### Verwandte Messages

| Message             | ID   | Beziehung                     |
| ------------------- | ---- | ----------------------------- |
| `EntitySpawnBatch`  | 1401 | Batch-Version für Zone-Enter  |
| `EntityDespawn`     | 1402 | Gegenteil                     |
| `EntityUpdate`      | 1404 | Nachfolgende Updates          |

#### Code-Beispiel

```csharp
var spawn = new EntitySpawn
{
    Type = MessageType.EntitySpawn,
    EntityId = 98765,
    EntityType = (byte)EntityType.Player,
    X = 150.5f,
    Y = 200.3f,
    Rotation = 90.0f,
    Name = "Aragorn",
    Level = 10,
    CurrentHp = 950,
    MaxHp = 1200,
    State = (byte)EntityState.Idle,
    ModelId = 1001
};
```

---

### EntitySpawnBatch (1401)

**Richtung:** 📥 Server → Client  
**Frequenz:** ⚡ Häufig (Zone-Enter)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

#### Beschreibung

Server sendet mehrere Entity-Spawns gebatched bei Zone-Enter oder großer Sichtweiten-Änderung.

#### Response Payload

| Feld     | Typ               | Beschreibung            | Pflicht |
| -------- | ----------------- | ----------------------- | ------- |
| Entities | EntitySpawnDto[]  | Array von Entity-Spawns | Ja      |

#### Erwartete Response

- Keine Response erforderlich

#### Verwandte Messages

| Message         | ID   | Beziehung           |
| --------------- | ---- | ------------------- |
| `EntitySpawn`   | 1400 | Einzel-Version      |
| `ZoneState`     | 102  | Oft zusammen gesendet|

---

### EntityDespawn (1402)

**Richtung:** 📡 Broadcast (Server → Nearby Clients)  
**Frequenz:** ⚡ Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

#### Beschreibung

Server broadcastet Entity-Despawn. Gründe: Out-of-Range (>55m), Death (nach Animation), Object entfernt.

#### Broadcast Payload

| Feld     | Typ    | Beschreibung                                         | Pflicht |
| -------- | ------ | ---------------------------------------------------- | ------- |
| EntityId | int    | Entity-ID                                            | Ja      |
| Reason   | byte   | 0=OutOfRange, 1=Death, 2=Removed, 3=ZoneChange       | Ja      |

#### Erwartete Response

- Keine Response erforderlich

#### Verwandte Messages

| Message              | ID   | Beziehung               |
| -------------------- | ---- | ----------------------- |
| `EntitySpawn`        | 1400 | Gegenteil               |
| `EntityDespawnBatch` | 1403 | Batch-Version           |
| `DeathEvent`         | 303  | Oft vor Despawn         |

---

### EntityDespawnBatch (1403)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

#### Beschreibung

Server sendet mehrere Entity-Despawns gebatched (Zone-Leave, große Movement-Änderung).

#### Response Payload

| Feld      | Typ   | Beschreibung               | Pflicht |
| --------- | ----- | -------------------------- | ------- |
| EntityIds | int[] | Array von Entity-IDs       | Ja      |
| Reason    | byte  | Gemeinsamer Despawn-Grund  | Ja      |

#### Erwartete Response

- Keine Response erforderlich

---

### EntityUpdate (1404)

**Richtung:** 📡 Broadcast (Server → Nearby Clients)  
**Frequenz:** ⚡⚡ Sehr häufig (nur bei Changes)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

#### Beschreibung

Server broadcastet Entity-Updates (HP, State, Position). Verwendet Delta-Pattern: nur geänderte Properties werden übertragen.

#### Broadcast Payload

| Feld      | Typ    | Beschreibung                 | Pflicht |
| --------- | ------ | ---------------------------- | ------- |
| EntityId  | int    | Entity-ID                    | Ja      |
| CurrentHp | int?   | Neue HP (null = unverändert) | Nein    |
| MaxHp     | int?   | Neue Max-HP                  | Nein    |
| Level     | int?   | Neues Level                  | Nein    |
| State     | byte?  | Neuer State                  | Nein    |
| ModelId   | uint?  | Neues Model                  | Nein    |
| X         | float? | Neue Position X              | Nein    |
| Y         | float? | Neue Position Y              | Nein    |

#### Erwartete Response

- Keine Response erforderlich

#### Code-Beispiel

```csharp
var update = new EntityUpdate
{
    Type = MessageType.EntityUpdate,
    EntityId = 45678,
    CurrentHp = 450
};
```

---

### EntityUpdateBatch (1405)

**Richtung:** 📥 Server → Client  
**Frequenz:** ⚡⚡ Sehr häufig (25 Hz Tick)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

#### Beschreibung

Server sendet gebatchte Entity-Updates pro Tick. Alle Entities mit Änderungen in einer Message.

#### Response Payload

| Feld    | Typ               | Beschreibung              | Pflicht |
| ------- | ----------------- | ------------------------- | ------- |
| Updates | EntityUpdateDto[] | Array von Delta-Updates   | Ja      |

#### Erwartete Response

- Keine Response erforderlich

---

### EntityListRequest (1406)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

#### Beschreibung

Client fordert Liste aller sichtbaren Entities an. Verwendet bei Reconnect oder Sync-Problemen.

#### Request Payload

| Feld      | Typ  | Beschreibung           | Pflicht |
| --------- | ---- | ---------------------- | ------- |
| RequestId | uint | Correlation-ID         | Ja      |

#### Erwartete Response

- `EntityListResponse` (1407)

---

### EntityListResponse (1407)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

#### Beschreibung

Server antwortet mit Liste aller sichtbaren Entities.

#### Response Payload

| Feld      | Typ              | Beschreibung                | Pflicht |
| --------- | ---------------- | --------------------------- | ------- |
| RequestId | uint             | Correlation-ID              | Ja      |
| Entities  | EntitySpawnDto[] | Array aller sichtbaren Entities | Ja |

#### Verwandte Messages

| Message             | ID   | Beziehung |
| ------------------- | ---- | --------- |
| `EntityListRequest` | 1406 | Request   |

---

### EntityPathUpdate (1408)

**Richtung:** 📡 Broadcast (Server → Nearby Clients)  
**Frequenz:** Häufig (NPC/Monster Movement)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

#### Beschreibung

Server broadcastet geplanten Path für NPC/Monster. Client kann smooth interpolieren.

#### Broadcast Payload

| Feld       | Typ           | Beschreibung               | Pflicht |
| ---------- | ------------- | -------------------------- | ------- |
| EntityId   | int           | Entity-ID                  | Ja      |
| Waypoints  | Vector2[]     | Pfad-Wegpunkte             | Ja      |
| Speed      | float         | Bewegungsgeschwindigkeit   | Ja      |

#### Erwartete Response

- Keine Response erforderlich

---

### EntityStateChange (1409)

**Richtung:** 📡 Broadcast (Server → Nearby Clients)  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

#### Beschreibung

Server broadcastet State-Machine-Transition für Entity. Triggert Animationen und Behavior-Changes.

#### Broadcast Payload

| Feld     | Typ  | Beschreibung               | Pflicht |
| -------- | ---- | -------------------------- | ------- |
| EntityId | int  | Entity-ID                  | Ja      |
| OldState | byte | Vorheriger EntityState     | Ja      |
| NewState | byte | Neuer EntityState          | Ja      |

#### Erwartete Response

- Keine Response erforderlich

#### Code-Beispiel

```csharp
var stateChange = new EntityStateChange
{
    Type = MessageType.EntityStateChange,
    EntityId = 45678,
    OldState = (byte)EntityState.Idle,
    NewState = (byte)EntityState.Combat
};
```

---

### EntityAnimation (1410)

**Richtung:** 📡 Broadcast (Server → Nearby Clients)  
**Frequenz:** ⚡ Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

#### Beschreibung

Server broadcastet Animation-Trigger. Client spielt Animation ab. Combat-kritisch: instant Broadcast.

#### Broadcast Payload

| Feld        | Typ   | Beschreibung                | Pflicht |
| ----------- | ----- | --------------------------- | ------- |
| EntityId    | int   | Entity-ID                   | Ja      |
| AnimationId | uint  | AnimationType enum          | Ja      |
| Loop        | bool  | Loop Animation?             | Nein    |
| Speed       | float | Speed-Multiplier (1.0 = normal) | Nein |

#### Erwartete Response

- Keine Response erforderlich

#### Code-Beispiel

```csharp
var anim = new EntityAnimation
{
    Type = MessageType.EntityAnimation,
    EntityId = 98765,
    AnimationId = (uint)AnimationType.AttackMelee,
    Loop = false,
    Speed = 1.0f
};
```

---

### EntityAnimationBatch (1411)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

#### Beschreibung

Server sendet mehrere Animationen gebatched. Verwendet bei Zone-Enter für laufende Animationen.

#### Response Payload

| Feld       | Typ                   | Beschreibung                | Pflicht |
| ---------- | --------------------- | --------------------------- | ------- |
| Animations | EntityAnimationDto[]  | Array von Animationen       | Ja      |

#### Erwartete Response

- Keine Response erforderlich

---

### EntityNameplate (1412)

**Richtung:** 📡 Broadcast (Server → Nearby Clients)  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

#### Beschreibung

Server broadcastet Nameplate-Daten für Entity (Name, Title, Guild).

#### Broadcast Payload

| Feld      | Typ     | Beschreibung        | Pflicht |
| --------- | ------- | ------------------- | ------- |
| EntityId  | int     | Entity-ID           | Ja      |
| Name      | string  | Anzeigename         | Ja      |
| Title     | string? | Titel               | Nein    |
| GuildName | string? | Gildenname          | Nein    |
| GuildRank | string? | Gildenrang          | Nein    |

#### Erwartete Response

- Keine Response erforderlich

---

### EntityNameplateUpdate (1413)

**Richtung:** 📡 Broadcast (Server → Nearby Clients)  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

#### Beschreibung

Server broadcastet Nameplate-Update (nur geänderte Felder).

#### Broadcast Payload

| Feld      | Typ      | Beschreibung                | Pflicht |
| --------- | -------- | --------------------------- | ------- |
| EntityId  | int      | Entity-ID                   | Ja      |
| Name      | string?  | Neuer Name (null = unverändert) | Nein |
| Title     | string?  | Neuer Titel                 | Nein    |
| GuildName | string?  | Neue Gilde                  | Nein    |

#### Erwartete Response

- Keine Response erforderlich

---

### EntityFaction (1414)

**Richtung:** 📡 Broadcast (Server → Nearby Clients)  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

#### Beschreibung

Server broadcastet Factions-Änderung für Entity. Beeinflusst Nameplate-Farbe und Angreifbarkeit.

#### Broadcast Payload

| Feld      | Typ  | Beschreibung                               | Pflicht |
| --------- | ---- | ------------------------------------------ | ------- |
| EntityId  | int  | Entity-ID                                  | Ja      |
| FactionId | uint | Factions-ID                                | Ja      |
| Relation  | byte | 0=Hostile, 1=Neutral, 2=Friendly           | Ja      |

#### Erwartete Response

- Keine Response erforderlich

---

### EntityScale (1415)

**Richtung:** 📡 Broadcast (Server → Nearby Clients)  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

#### Beschreibung

Server broadcastet Größen-Änderung für Entity (z.B. Wachstums-Buffs, Boss-Phases).

#### Broadcast Payload

| Feld     | Typ   | Beschreibung                    | Pflicht |
| -------- | ----- | ------------------------------- | ------- |
| EntityId | int   | Entity-ID                       | Ja      |
| Scale    | float | Scale-Faktor (1.0 = normal)     | Ja      |

#### Erwartete Response

- Keine Response erforderlich

---

### EntityMountUpdate (1416)

**Richtung:** 📡 Broadcast (Server → Nearby Clients)  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

#### Beschreibung

Server broadcastet Mount-Status-Änderung (aufsteigen, absteigen).

#### Broadcast Payload

| Feld      | Typ  | Beschreibung                     | Pflicht |
| --------- | ---- | -------------------------------- | ------- |
| EntityId  | int  | Rider Entity-ID                  | Ja      |
| MountId   | uint | Mount-Model-ID (0 = dismounted)  | Ja      |
| MountType | byte | 0=Ground, 1=Flying, 2=Aquatic    | Ja      |

#### Erwartete Response

- Keine Response erforderlich

---

### EntityEquipmentUpdate (1417)

**Richtung:** 📡 Broadcast (Server → Nearby Clients)  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

#### Beschreibung

Server broadcastet sichtbare Equipment-Änderung für Entity (Weapon, Armor visuals).

#### Broadcast Payload

| Feld       | Typ    | Beschreibung             | Pflicht |
| ---------- | ------ | ------------------------ | ------- |
| EntityId   | int    | Entity-ID                | Ja      |
| Slot       | byte   | Equipment-Slot           | Ja      |
| ItemId     | uint   | Item-ID (0 = leer)       | Ja      |
| AppearanceId | uint | Visual-Override-ID       | Nein    |

#### Erwartete Response

- Keine Response erforderlich

---

### EntityAuraUpdate (1418)

**Richtung:** 📡 Broadcast (Server → Nearby Clients)  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

#### Beschreibung

Server broadcastet Aura/Buff-Änderung für Entity (für visuelle Effekte).

#### Broadcast Payload

| Feld       | Typ   | Beschreibung                     | Pflicht |
| ---------- | ----- | -------------------------------- | ------- |
| EntityId   | int   | Entity-ID                        | Ja      |
| AuraId     | uint  | Aura/Buff-ID                     | Ja      |
| Action     | byte  | 0=Applied, 1=Removed, 2=Refresh  | Ja      |
| Stacks     | byte  | Aktuelle Stack-Anzahl            | Nein    |
| Duration   | float | Verbleibende Sekunden            | Nein    |

#### Erwartete Response

- Keine Response erforderlich

---

### EntityEmote (1419)

**Richtung:** 📡 Broadcast (Server → Nearby Clients)  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

#### Beschreibung

Server broadcastet Emote (Animation + optional Sound). Combat-kritisch: instant Broadcast.

#### Broadcast Payload

| Feld     | Typ  | Beschreibung        | Pflicht |
| -------- | ---- | ------------------- | ------- |
| EntityId | int  | Emoting Entity      | Ja      |
| EmoteId  | uint | Emote-ID            | Ja      |
| TargetId | int? | Optional Target     | Nein    |

#### Erwartete Response

- Keine Response erforderlich

#### Code-Beispiel

```csharp
var emote = new EntityEmote
{
    Type = MessageType.EntityEmote,
    EntityId = 98765,
    EmoteId = (uint)AnimationType.EmoteWave,
    TargetId = 54321
};
```

---

### EntitySay (1420)

**Richtung:** 📡 Broadcast (Server → Nearby Clients)  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

#### Beschreibung

Server broadcastet NPC/Monster-Sprache in normaler Lautstärke (Sichtweite).

#### Broadcast Payload

| Feld     | Typ    | Beschreibung     | Pflicht |
| -------- | ------ | ---------------- | ------- |
| EntityId | int    | Speaking Entity  | Ja      |
| Text     | string | Gesprochener Text| Ja      |

#### Erwartete Response

- Keine Response erforderlich

---

### EntityYell (1421)

**Richtung:** 📡 Broadcast (Server → Zone)  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

#### Beschreibung

Server broadcastet NPC/Monster-Schrei (Zone-weit). Verwendet für Boss-Announcements.

#### Broadcast Payload

| Feld     | Typ    | Beschreibung      | Pflicht |
| -------- | ------ | ----------------- | ------- |
| EntityId | int    | Yelling Entity    | Ja      |
| Text     | string | Geschrieener Text | Ja      |

#### Erwartete Response

- Keine Response erforderlich

---

### LootableSpawn (1430)

**Richtung:** 📡 Broadcast (Server → Nearby Clients)  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

#### Beschreibung

Server broadcastet Spawn eines Lootable-Objects (Corpse mit Loot, Chest).

#### Broadcast Payload

| Feld       | Typ   | Beschreibung              | Pflicht |
| ---------- | ----- | ------------------------- | ------- |
| EntityId   | int   | Lootable Entity-ID        | Ja      |
| X          | float | Position X                | Ja      |
| Y          | float | Position Y                | Ja      |
| ModelId    | uint  | Model-ID (Corpse/Chest)   | Ja      |
| LootableBy | int[] | PlayerIDs die looten dürfen | Nein  |

#### Erwartete Response

- Keine Response erforderlich

---

### LootableDespawn (1431)

**Richtung:** 📡 Broadcast (Server → Nearby Clients)  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

#### Beschreibung

Server broadcastet Despawn eines Lootable-Objects (geplündert oder Timeout).

#### Broadcast Payload

| Feld     | Typ | Beschreibung           | Pflicht |
| -------- | --- | ---------------------- | ------- |
| EntityId | int | Lootable Entity-ID     | Ja      |

#### Erwartete Response

- Keine Response erforderlich

---

### ResourceNodeSpawn (1432)

**Richtung:** 📡 Broadcast (Server → Nearby Clients)  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

#### Beschreibung

Server broadcastet Spawn einer Resource-Node (Mining/Herbalism).

#### Broadcast Payload

| Feld         | Typ   | Beschreibung                | Pflicht |
| ------------ | ----- | --------------------------- | ------- |
| EntityId     | int   | Resource Entity-ID          | Ja      |
| X            | float | Position X                  | Ja      |
| Y            | float | Position Y                  | Ja      |
| ResourceType | uint  | Resource-Type-ID            | Ja      |
| ModelId      | uint  | Model-ID                    | Ja      |
| State        | byte  | 0=Available, 1=Depleted     | Ja      |

#### Erwartete Response

- Keine Response erforderlich

---

### ResourceNodeDespawn (1433)

**Richtung:** 📡 Broadcast (Server → Nearby Clients)  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

#### Beschreibung

Server broadcastet Despawn einer Resource-Node.

#### Broadcast Payload

| Feld     | Typ | Beschreibung           | Pflicht |
| -------- | --- | ---------------------- | ------- |
| EntityId | int | Resource Entity-ID     | Ja      |

#### Erwartete Response

- Keine Response erforderlich

---

### ResourceNodeState (1434)

**Richtung:** 📡 Broadcast (Server → Nearby Clients)  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

#### Beschreibung

Server broadcastet State-Änderung einer Resource-Node (Available ↔ Depleted).

#### Broadcast Payload

| Feld        | Typ   | Beschreibung                      | Pflicht |
| ----------- | ----- | --------------------------------- | ------- |
| EntityId    | int   | Resource Entity-ID                | Ja      |
| State       | byte  | 0=Available, 1=Depleted           | Ja      |
| RespawnTime | float | Sekunden bis Respawn (optional)   | Nein    |

#### Erwartete Response

- Keine Response erforderlich

---

## 🗑️ Obsolete Messages

Derzeit keine Obsolete Messages in dieser Kategorie.

---

## 📎 Anhang

### MessageType Enum (Exakte Reihenfolge aus Code)

```csharp
// ENTITY SPAWNING / SYNC (1400-1499)
EntitySpawn = 1400,
EntitySpawnBatch = 1401,
EntityDespawn = 1402,
EntityDespawnBatch = 1403,
EntityUpdate = 1404,
EntityUpdateBatch = 1405,
EntityListRequest = 1406,
EntityListResponse = 1407,
EntityPathUpdate = 1408,
EntityStateChange = 1409,
EntityAnimation = 1410,
EntityAnimationBatch = 1411,
EntityNameplate = 1412,
EntityNameplateUpdate = 1413,
EntityFaction = 1414,
EntityScale = 1415,
EntityMountUpdate = 1416,
EntityEquipmentUpdate = 1417,
EntityAuraUpdate = 1418,
EntityEmote = 1419,
EntitySay = 1420,
EntityYell = 1421,
LootableSpawn = 1430,
LootableDespawn = 1431,
ResourceNodeSpawn = 1432,
ResourceNodeDespawn = 1433,
ResourceNodeState = 1434,
```

### Request/Response Paare

| Request              | ID   | Response              | ID   |
| -------------------- | ---- | --------------------- | ---- |
| `EntityListRequest`  | 1406 | `EntityListResponse`  | 1407 |

### Datei-Struktur

```
shared/Mmo.Shared/Messaging/
├── Enums/
│   └── MessageType.cs          # Enum-Definition
├── Messages/Entity/
│   ├── EntitySpawn.cs
│   ├── EntitySpawnBatch.cs
│   ├── EntityDespawn.cs
│   ├── EntityDespawnBatch.cs
│   ├── EntityUpdate.cs
│   ├── EntityUpdateBatch.cs
│   ├── EntityListRequest.cs
│   ├── EntityListResponse.cs
│   ├── EntityPathUpdate.cs
│   ├── EntityStateChange.cs
│   ├── EntityAnimation.cs
│   ├── EntityAnimationBatch.cs
│   ├── EntityNameplate.cs
│   ├── EntityNameplateUpdate.cs
│   ├── EntityFaction.cs
│   ├── EntityScale.cs
│   ├── EntityMountUpdate.cs
│   ├── EntityEquipmentUpdate.cs
│   ├── EntityAuraUpdate.cs
│   ├── EntityEmote.cs
│   ├── EntitySay.cs
│   ├── EntityYell.cs
│   ├── LootableSpawn.cs
│   ├── LootableDespawn.cs
│   ├── ResourceNodeSpawn.cs
│   ├── ResourceNodeDespawn.cs
│   └── ResourceNodeState.cs
└── DTOs/Entity/
    ├── EntitySpawnDto.cs
    └── EntityUpdateDto.cs
```

---

**Letzte Aktualisierung**: 2026-01-02  
**Version**: 3.0.0  
**Status**: ✅ Aligned mit MessageType Enum (27 Messages)

[← Zurück zur Übersicht](README.md)
