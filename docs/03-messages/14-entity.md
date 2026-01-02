# 👾 Entity Messages (1400-1499)

**Kategorie:** 14  
**Range:** 1400-1499  



[← Zurück zur Übersicht](README.md)

---

## 📋 Inhaltsverzeichnis

-   [EntitySpawn (1400)](#entityspawn-1400)
-   [EntityDespawn (1401)](#entitydespawn-1401)
-   [EntityMove (1402)](#entitymove-1402)
-   [EntityUpdate (1403)](#entityupdate-1403)
-   [EntityAnimation (1404)](#entityanimation-1404)
-   [EntityStateChange (1405)](#entitystatechange-1405)
-   [EntityInteract (1410)](#entityinteract-1410)
-   [EntityTarget (1420)](#entitytarget-1420)
-   [EntityAggro (1421)](#entityaggro-1421)
-   [EntityEmote (1430)](#entityemote-1430)
-   [EntityTargetResponse (1440)](#entitytargetresponse-1440)

---

## 📋 Übersicht

Diese Kategorie umfasst alle Messages für **Entity** Funktionalität im 2DMMO.

Das Entity-System ist die Basis für alle Game-Objects:

-   **Entity-Types**: Player, NPC, Monster, Object, Projectile, Vehicle
-   **Lifecycle**: Spawn → Move/Update → Despawn
-   **Interactions**: Target, Interact, Aggro
-   **Animations**: Idle, Walk, Run, Attack, Cast, Death
-   **State-Machine**: Idle, Combat, Dead, Stunned, Rooted
-   **Visibility-System**: Entities spawnen nur in Sichtweite (~50m)

**Server Authority**: Alle Entity-Changes sind server-authoritative.

**Entity-ID**: Eindeutige ID pro Zone (int32, Range: 1-2147483647)

**Update-Frequency**:

-   Movement: 20 Hz (50ms) für Players, 10 Hz (100ms) für NPCs
-   Stats: nur bei Changes
-   Animation: nur bei Changes

**Visibility-Range**: 50m Radius (configurable per Zone)

**🔄 DTO-System & Dirty-Tracking:**  
Entity Messages verwenden später **automatisch generierte DTOs** und **Delta DTO Unions**:

-   `EntitySpawn` verwendet `EntityDtoUnion` (polymorphes Interface für Characters, NPCs, etc.)
-   `EntityUpdate` ist ersetzt durch **`ZoneDelta.EntityUpdates`** mit **Delta DTO Unions**
-   Delta DTOs werden automatisch aus Interfaces generiert (z.B. `ICharacterEntityDelta`, `INpcEntityDelta`)
-   **Factory Methods:** `entity.ToDeltaUnion()` erzeugt automatisch das passende Delta-DTO
-   **O(1) Lookup:** Alle Delta-DTOs implementieren `IDeltaDto<TId>` für schnelle Dictionary-Lookups
-   **97% Bandwidth-Reduzierung** durch Nullable-Pattern (nur geänderte Properties werden übertragen)

**Beispiel - Server erstellt Delta Updates:**
```csharp
// Characters und NPCs in einer Liste
var entityUpdates = new List<EntityDeltaUnion>
{
    // Character mit Factory-Methode
    character.ToDeltaUnion(),  // Nur geänderte Properties
    
    // NPC manuell
    new INpcEntityDelta 
    { 
        PersistentId = npcId,
        CurrentHealth = 120,  // Nur Health hat sich geändert
        Position = null       // Position unverändert (wird nicht serialisiert)
    }
};

var zoneDelta = new ZoneDelta { EntityUpdates = entityUpdates };
```

**Beispiel - Client wendet Delta Updates an:**
```csharp
// O(1) Lookup für schnelles Finden
var deltaDict = zoneDelta.EntityUpdates.ToDictionary(d => d.GetId());

if (deltaDict.TryGetValue(myCharacterId, out var delta))
{
    // Type-safe Pattern-Matching
    switch (delta)
    {
        case ICharacterEntityDelta charDelta:
            if (charDelta.CurrentHealth.HasValue)
                myCharacter.CurrentHealth = charDelta.CurrentHealth.Value;
            if (charDelta.Position.HasValue)
                myCharacter.Position = charDelta.Position.Value;
            break;
        case INpcEntityDelta npcDelta:
            // Handle NPC delta
            break;
    }
}
```

Siehe [DIRTY_TRACKING.md](../../02-architecture/DIRTY_TRACKING.md) für Details zum Delta-DTO-System, Union-Pattern und automatischer Code-Generierung.

---

## EntitySpawn (1400)

**Richtung:** 📡 Broadcast (Server → Nearby Players)  
**Frequenz:** ⚡ Sehr häufig (bei Zone-Enter, Range-Enter)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Server broadcastet Entity-Spawn an alle Spieler in Sichtweite. Wird gesendet wenn:

-   Spieler in Sichtweite kommt (Zone-Enter oder Movement)
-   NPC/Monster spawnt
-   Object erscheint (Loot, Chest, etc.)

Enthält vollständige Entity-Daten für initialen Render.

### Im Scope ✅

-   Initiales Spawning von allen Entity-Types
-   Vollständige Entity-Daten (Position, Stats, Appearance)
-   Broadcast an alle in Sichtweite
-   Support für alle Entity-Types

### Nicht im Scope ❌

-   Movement-Updates → verwende `EntityMove` (1402)
-   Stat-Updates → verwende `EntityUpdate` (1403)
-   Despawn → verwende `EntityDespawn` (1401)

### Broadcast Payload

| Feld       | Typ    | Beschreibung                                                  | Pflicht |
| ---------- | ------ | ------------------------------------------------------------- | ------- |
| EntityId   | int    | Eindeutige Entity-ID (in Zone)                                | Ja      |
| EntityType | byte   | 1=Player, 2=NPC, 3=Monster, 4=Object, 5=Projectile, 6=Vehicle | Ja      |
| X          | float  | Position X                                                    | Ja      |
| Y          | float  | Position Y                                                    | Ja      |
| Rotation   | float  | Rotation (0-360°)                                             | Ja      |
| Name       | string | Entity-Name (z.B. Player-Name, "Goblin Warrior")              | Nein    |
| Level      | int    | Level (für NPCs/Monsters/Players)                             | Nein    |
| CurrentHP  | int    | Aktuelle HP (sichtbar für alle)                               | Nein    |
| MaxHP      | int    | Max HP                                                        | Nein    |
| State      | byte   | Initial-State (0=Idle, 1=Combat, 2=Dead, etc.)                | Ja      |
| ModelId    | uint   | Model-ID für Rendering                                        | Ja      |
| Appearance | byte[] | Appearance-Daten (für Players)                                | Nein    |
| VelocityX  | float  | Initiale Velocity X                                           | Nein    |
| VelocityY  | float  | Initiale Velocity Y                                           | Nein    |

### Erwartete Response

-   Keine Response erforderlich

### Verwandte Messages

| Message         | ID   | Beziehung                     |
| --------------- | ---- | ----------------------------- |
| `EntityDespawn` | 1401 | Gegenteil (Despawn)           |
| `EntityMove`    | 1402 | Nachfolgende Movement-Updates |
| `EntityUpdate`  | 1403 | Nachfolgende Stat-Updates     |
| `JoinZone`      | 100  | Player-Spawn in Zone          |

### Flow-Diagramm

```
Player A                  Server              Player B (in Range)
  │                          │                          │
  │  (Moves into range       │                          │
  │   of Player B)           │                          │
  │                          │  Check Visibility        │
  │                          │  (Distance < 50m)        │
  │                          │                          │
  │                          │  EntitySpawn (1400)      │
  │                          │  (Player A Data)         │
  │                          │─────────────────────────►│
  │                          │                          │
  │                          │                          │  (Render Player A)
```

### Beispiel Payload

```csharp
// Player-Spawn
var playerSpawn = new EntitySpawn
{
    Type = MessageType.EntitySpawn,
    EntityId = 98765,
    EntityType = 1, // Player
    X = 150.5f,
    Y = 200.3f,
    Rotation = 90.0f, // Facing East
    Name = "Aragorn",
    Level = 10,
    CurrentHP = 950,
    MaxHP = 1200,
    State = 0, // Idle
    ModelId = 1001, // Human-Male-Warrior
    Appearance = SerializeAppearance(...),
    VelocityX = 0,
    VelocityY = 0
};

// Monster-Spawn
var monsterSpawn = new EntitySpawn
{
    Type = MessageType.EntitySpawn,
    EntityId = 45678,
    EntityType = 3, // Monster
    X = 200.0f,
    Y = 250.0f,
    Rotation = 180.0f,
    Name = "Goblin Warrior",
    Level = 8,
    CurrentHP = 600,
    MaxHP = 600,
    State = 0, // Idle (Patrolling)
    ModelId = 2050, // Goblin-Model
    VelocityX = 1.5f, // Moving
    VelocityY = 0
};

// Object-Spawn (Loot)
var objectSpawn = new EntitySpawn
{
    Type = MessageType.EntitySpawn,
    EntityId = 99999,
    EntityType = 4, // Object
    X = 175.0f,
    Y = 220.0f,
    Rotation = 0,
    Name = "Loot Bag",
    State = 0,
    ModelId = 5001 // Loot-Bag-Model
};
```

### Notizen

-   **Visibility-Check**: Server sendet nur an Spieler in 50m Range
-   **Batching**: Multiple Spawns werden gebatched (z.B. beim Zone-Enter)
-   **Model-ID**: Client resolved Model-ID zu 3D-Model/Sprite
-   **Appearance**: Nur für Players (Hairstyle, Colors, etc.)
-   **Performance**: Kritische Message für Performance (sehr häufig)
-   **Despawn-Range**: Bei >55m wird `EntityDespawn` gesendet (Hysteresis)

---

## EntityDespawn (1401)

**Richtung:** 📡 Broadcast (Server → Nearby Players)  
**Frequenz:** ⚡ Sehr häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Server broadcastet Entity-Despawn an alle Spieler. Wird gesendet wenn:

-   Spieler aus Sichtweite geht (>55m)
-   Entity stirbt (nach Death-Animation)
-   Object verschwindet (Loot geplündert, etc.)
-   Entity removed (Admin-Delete, etc.)

Client removed Entity aus Render-List.

### Im Scope ✅

-   Despawn aller Entity-Types
-   Broadcast an alle die Entity sehen konnten
-   Cleanup-Signal für Client

### Nicht im Scope ❌

-   Death-Event → verwende `DeathEvent` (303) vor Despawn
-   Respawn → neuer `EntitySpawn` (1400)

### Broadcast Payload

| Feld     | Typ    | Beschreibung                                      | Pflicht |
| -------- | ------ | ------------------------------------------------- | ------- |
| EntityId | int    | Entity-ID                                         | Ja      |
| Reason   | string | "out_of_range", "death", "removed", "zone_change" | Nein    |

### Erwartete Response

-   Keine Response erforderlich

### Verwandte Messages

| Message       | ID   | Beziehung                     |
| ------------- | ---- | ----------------------------- |
| `EntitySpawn` | 1400 | Gegenteil (Spawn)             |
| `DeathEvent`  | 303  | Oft vor Despawn               |
| `LeaveZone`   | 101  | Player-Despawn bei Zone-Leave |

### Beispiel Payload

```csharp
// Out-of-Range Despawn
var despawn = new EntityDespawn
{
    Type = MessageType.EntityDespawn,
    EntityId = 98765,
    Reason = "out_of_range"
};

// Death Despawn (nach Death-Animation)
var deathDespawn = new EntityDespawn
{
    Type = MessageType.EntityDespawn,
    EntityId = 45678,
    Reason = "death" // 5s nach DeathEvent
};
```

### Notizen

-   **Death-Delay**: 5 Sekunden nach `DeathEvent` für Corpse-Loot
-   **Range**: >55m für Out-of-Range (Hysteresis zu 50m Spawn)
-   **Cleanup**: Client removed Entity aus Memory
-   **No Animation**: Keine Animation bei "out_of_range" (smooth fadeout)

---

## EntityMove (1402)

> ⚠️ **DEPRECATED später**
> 
> This message is replaced by **`ZoneDelta.EntityUpdates`** (with Delta DTO Unions) for better batching performance.
> Position changes are now sent via `IEntityPositionDelta` or entity-specific Delta DTOs like `ICharacterEntityDelta`.
> See [Chunk-Based Sync](../../02-architecture/CHUNK_BASED_SYNC.md) and [DIRTY_TRACKING.md](../../02-architecture/DIRTY_TRACKING.md) for details.

**Richtung:** 📡 Broadcast (Server → Nearby Players)  
**Frequenz:** ⚡⚡ Extrem häufig (20-50 Updates/Sekunde)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Server broadcastet Entity-Movement an alle Spieler in Sichtweite. Dies ist die performance-kritischste Message im gesamten System.

Für Players: Client sendet Movement-Input, Server validiert und broadcastet.
Für NPCs/Monsters: Server berechnet AI-Movement und broadcastet.

### Im Scope ✅

-   Position-Updates für alle Entity-Types
-   Velocity für Interpolation
-   Rotation/Facing-Direction
-   Server-Authority (kein Client-Prediction für andere Entities)

### Nicht im Scope ❌

-   Player-Input → verwende `PlayerMove` (200)
-   Teleport → verwende `EntityTeleport` (dedizierte Message)
-   Stat-Updates → verwende `EntityUpdate` (1403)

### Broadcast Payload

| Feld      | Typ   | Beschreibung                        | Pflicht |
| --------- | ----- | ----------------------------------- | ------- |
| EntityId  | int   | Entity-ID                           | Ja      |
| X         | float | Neue Position X                     | Ja      |
| Y         | float | Neue Position Y                     | Ja      |
| VelocityX | float | Velocity X (für Interpolation)      | Ja      |
| VelocityY | float | Velocity Y                          | Ja      |
| Rotation  | float | Facing-Direction (0-360°)           | Nein    |
| Timestamp | uint  | Server-Timestamp (für Latency-Comp) | Ja      |

### Erwartete Response

-   Keine Response erforderlich

### Verwandte Messages

| Message          | ID   | Beziehung                      |
| ---------------- | ---- | ------------------------------ |
| `PlayerMove`     | 200  | Player-Input (Client → Server) |
| `EntitySpawn`    | 1400 | Initiale Position              |
| `EntityTeleport` | 207  | Instant-Position-Change        |

### Beispiel Payload

```csharp
var entityMove = new EntityMove
{
    Type = MessageType.EntityMove,
    EntityId = 98765,
    X = 155.2f,
    Y = 205.8f,
    VelocityX = 5.0f, // Moving right
    VelocityY = 0,
    Rotation = 90.0f, // Facing East
    Timestamp = GetServerTick()
};
```

### Notizen

-   **Update-Frequency**:
    -   Players: 20 Hz (50ms) wenn moving
    -   NPCs: 10 Hz (100ms) wenn moving
    -   Idle Entities: 0 Hz (keine Updates)
-   **Interpolation**: Client interpolated zwischen Updates
-   **Velocity**: Für smooth Interpolation
-   **Timestamp**: Für Latency-Compensation
-   **Batching**: Multiple EntityMoves werden gebatched
-   **Delta-Compression**: Nur Changes werden gesendet (geplant)
-   **Performance**: Größter Bandwidth-Consumer im Game

---

## EntityUpdate (1403)

> ⚠️ **DEPRECATED später**
> 
> This message is replaced by **`ZoneDelta.EntityUpdates`** (with Delta DTO Unions) for better batching performance.
> State changes (HP, Level, Combat-State, etc.) are now sent via Delta DTOs like `ICharacterEntityDelta` or `INpcEntityDelta`.
> All Delta DTOs use nullable pattern - only changed properties are transmitted, achieving 97% bandwidth reduction.
> See [Chunk-Based Sync](../../02-architecture/CHUNK_BASED_SYNC.md) and [DIRTY_TRACKING.md](../../02-architecture/DIRTY_TRACKING.md) for details.

**Richtung:** 📡 Broadcast (Server → Nearby Players)  
**Frequenz:** Häufig (nur bei Changes)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Server broadcastet Entity-Stat-Updates (HP, State, Buffs, etc.). Wird nur bei Changes gesendet (Event-Based).

### Im Scope ✅

-   HP-Updates (Combat)
-   State-Changes (Idle → Combat, Stunned, etc.)
-   Level-Changes (für NPCs die leveln)
-   Model-Changes (Polymorphed, etc.)

### Nicht im Scope ❌

-   Movement → verwende `EntityMove` (1402)
-   Detailed Stats → verwende `CharacterInfo` (600) für Players

### Broadcast Payload

| Feld      | Typ  | Beschreibung                 | Pflicht |
| --------- | ---- | ---------------------------- | ------- |
| EntityId  | int  | Entity-ID                    | Ja      |
| CurrentHP | int  | Neue HP (falls geändert)     | Nein    |
| MaxHP     | int  | Neue Max-HP (falls geändert) | Nein    |
| Level     | int  | Neues Level                  | Nein    |
| State     | byte | Neuer State                  | Nein    |
| ModelId   | uint | Neues Model (Polymorph)      | Nein    |

### Erwartete Response

-   Keine Response erforderlich

### Beispiel Payload

```csharp
// HP-Update (Combat)
var hpUpdate = new EntityUpdate
{
    Type = MessageType.EntityUpdate,
    EntityId = 45678,
    CurrentHP = 450, // Was 600
    MaxHP = 600
};

// State-Change (Idle → Combat)
var stateChange = new EntityUpdate
{
    Type = MessageType.EntityUpdate,
    EntityId = 45678,
    State = 1 // Combat
};

// Polymorph (Model-Change)
var polymorph = new EntityUpdate
{
    Type = MessageType.EntityUpdate,
    EntityId = 98765,
    ModelId = 9001 // Sheep-Model
};
```

### Notizen

-   **Delta-Only**: Nur geänderte Felder werden gesendet
-   **HP-Bar**: Client updated HP-Bar above Entity
-   **State-Driven**: Trigger Animations basierend auf State

---

## EntityAnimation (1404)

**Richtung:** 📡 Broadcast (Server → Nearby Players)  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Server broadcastet Animation-Trigger für Entity. Client spielt Animation ab.

### Im Scope ✅

-   All Animation-Types (Attack, Cast, Emote, Death, etc.)
-   Animation-Parameters (Speed, Loop, etc.)
-   Broadcast an alle in Sichtweite

### Broadcast Payload

| Feld        | Typ   | Beschreibung                              | Pflicht |
| ----------- | ----- | ----------------------------------------- | ------- |
| EntityId    | int   | Entity-ID                                 | Ja      |
| AnimationId | uint  | Animation-ID                              | Ja      |
| Loop        | bool  | Loop Animation?                           | Nein    |
| Speed       | float | Animation-Speed-Multiplier (1.0 = normal) | Nein    |

**Animation-IDs**:

-   1: Idle
-   2: Walk
-   3: Run
-   10: Attack-Melee
-   11: Attack-Ranged
-   20: Cast-Spell
-   30: Death
-   40: Emote-Wave
-   41: Emote-Dance
-   ... etc.

### Erwartete Response

-   Keine Response erforderlich

### Beispiel Payload

```csharp
// Attack-Animation
var attack = new EntityAnimation
{
    Type = MessageType.EntityAnimation,
    EntityId = 98765,
    AnimationId = 10, // Attack-Melee
    Loop = false,
    Speed = 1.0f
};

// Death-Animation
var death = new EntityAnimation
{
    Type = MessageType.EntityAnimation,
    EntityId = 45678,
    AnimationId = 30, // Death
    Loop = false,
    Speed = 1.0f
};
```

### Notizen

-   **Sync**: Animation-Trigger sind synchronisiert via Server
-   **Client-Driven**: Movement-Animations (Idle, Walk, Run) sind client-driven

---

## EntityStateChange (1405)

> ⚠️ **DEPRECATED später**
> 
> This message is replaced by **`ZoneDelta.EntityUpdates`** (with Delta DTO Unions) for better batching performance.
> State changes are now included in Delta DTOs (e.g., `IsInCombat`, `IsDead` properties in entity-specific Delta DTOs).
> See [Chunk-Based Sync](../../02-architecture/CHUNK_BASED_SYNC.md) and [DIRTY_TRACKING.md](../../02-architecture/DIRTY_TRACKING.md) for details.

**Richtung:** 📡 Broadcast (Server → Nearby Players)  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Server broadcastet State-Machine-Change für Entity. Trigger Animations und Behavior-Changes.

### Broadcast Payload

| Feld     | Typ  | Beschreibung     | Pflicht |
| -------- | ---- | ---------------- | ------- |
| EntityId | int  | Entity-ID        | Ja      |
| OldState | byte | Vorheriger State | Ja      |
| NewState | byte | Neuer State      | Ja      |

**States**:

-   0: Idle
-   1: Combat
-   2: Dead
-   3: Stunned
-   4: Rooted
-   5: Sleeping
-   6: Fleeing
-   7: Evading

### Beispiel Payload

```csharp
var stateChange = new EntityStateChange
{
    Type = MessageType.EntityStateChange,
    EntityId = 45678,
    OldState = 0, // Idle
    NewState = 1  // Combat
};
```

### Notizen

-   **Animations**: State-Change trigger Animations
-   **Behavior**: Client ändert Rendering (z.B. Red-Tint für Combat)

---

## 🎯 Phase 2: Delta DTO Union System

### ✅ Modernes Delta-Batching mit Unions (ZoneDelta.EntityUpdates)

Phase 2 ersetzt einzelne Entity-Messages durch **ein einheitliches Delta-DTO-System mit MessagePack Unions**:

**Alte Messages (DEPRECATED):**
- ❌ `EntityMove` (1402) - Eretzt durch Delta-DTOs mit Position-Property
- ❌ `EntityUpdate` (1403) - Ersetzt durch Delta-DTOs mit State/HP/Level Properties
- ❌ `EntityStateChange` (1405) - Ersetzt durch Delta-DTOs mit State-Properties

**Neues System:**
✅ **`ZoneDelta.EntityUpdates: List<EntityDeltaUnion>?`** - Eine Liste für alle Entity-Typen

**Was ist EntityDeltaUnion?**
Ein MessagePack Union-Interface das polymorphe Serialisierung ermöglicht:

```csharp
// Automatisch generiert vom DeltaDtoUnionGenerator
[Union(0, typeof(ICharacterEntityDelta))]
[Union(1, typeof(INpcEntityDelta))]
// ... weitere Entity-Typen können einfach hinzugefügt werden
public interface EntityDeltaUnion { }
```

**Delta DTOs für verschiedene Entity-Typen:**
```csharp
// Character Delta (automatisch generiert)
public class ICharacterEntityDelta : IDeltaDto<Guid>, EntityDeltaUnion
{
    [DeltaId] public Guid PersistentId { get; set; }
    public Position? Position { get; set; }          // null = unverändert
    public int? CurrentHealth { get; set; }          // null = unverändert
    public int? Level { get; set; }                  // null = unverändert
    public bool? IsInCombat { get; set; }            // null = unverändert
    // ... weitere Properties aus ICharacterEntity-Hierarchie
}

// NPC Delta (automatisch generiert)
public class INpcEntityDelta : IDeltaDto<Guid>, EntityDeltaUnion
{
    [DeltaId] public Guid PersistentId { get; set; }
    public Position? Position { get; set; }
    public int? CurrentHealth { get; set; }
    // ... NPC-spezifische Properties
}
```

**Vorteile:**
- ✅ **Eine Liste für alle Entity-Typen** - Characters, NPCs, Chests, etc. in einer Sammlung
- ✅ **97% Bandwidth-Reduzierung** - Nur geänderte Properties werden übertragen (Nullable-Pattern)
- ✅ **Type-Safe** - MessagePack deserializiert automatisch zum korrekten konkreten Typ
- ✅ **Erweiterbar** - Neue Entity-Typen erfordern keine Änderung an ZoneDelta
- ✅ **O(1) Lookup** - Alle Delta-DTOs implementieren `IDeltaDto<TId>` für schnelle Dictionary-Lookups
- ✅ **Factory Methods** - `entity.ToDeltaUnion()` generiert automatisch das passende Delta-DTO

**Server-Seite - Delta-Erstellung:**
```csharp
// Mix von verschiedenen Entity-Typen in einer Liste
var entityUpdates = new List<EntityDeltaUnion>
{
    // Character (nur geänderte Properties)
    character.ToDeltaUnion(),  // Factory-Methode generiert ICharacterEntityDelta
    
    // NPC (manuell)
    new INpcEntityDelta 
    {
        PersistentId = npcId,
        CurrentHealth = 120,   // Nur Health hat sich geändert
        Position = null,       // Unverändert - wird nicht serialisiert
        Level = null           // Unverändert - wird nicht serialisiert
    }
};

var zoneDelta = new ZoneDelta
{
    EntityUpdates = entityUpdates,  // Einzige Entity-Update-Liste
    Timestamp = serverTick
};
```

**Client-Seite - Delta-Anwendung:**
```csharp
if (zoneDelta.EntityUpdates == null) return;

// O(1) Lookup mit Extension-Methode
var deltaDict = zoneDelta.EntityUpdates.ToDictionary(d => d.GetId());

// Schnelles Finden für eigene Entity
if (deltaDict.TryGetValue(myCharacterId, out var delta))
{
    // Type-Safe Pattern-Matching
    switch (delta)
    {
        case ICharacterEntityDelta charDelta:
            // Nur geänderte Properties anwenden
            if (charDelta.Position.HasValue)
                myCharacter.Position = charDelta.Position.Value;
            if (charDelta.CurrentHealth.HasValue)
                myCharacter.CurrentHealth = charDelta.CurrentHealth.Value;
            if (charDelta.IsInCombat.HasValue)
                myCharacter.EnterCombat(charDelta.IsInCombat.Value);
            break;
            
        case INpcEntityDelta npcDelta:
            // NPC-spezifische Behandlung
            if (npcDelta.CurrentHealth.HasValue)
                UpdateNpcHealthBar(npcDelta.PersistentId, npcDelta.CurrentHealth.Value);
            break;
    }
}

// Alle Entities durchgehen
foreach (var entityDelta in zoneDelta.EntityUpdates)
{
    ApplyDeltaToEntity(entityDelta);
}
```

**Update-Frequency:**
- **Tick-Rate**: 25 Hz (40ms) - siehe [GAME_LOOP.md](../../02-architecture/GAME_LOOP.md)
- **Batching**: Alle Entity-Changes pro Tick in **einer** ZoneDelta-Message
- **Bandwidth**: ~1.5 MB/s statt 50 MB/s (97% Reduktion)

**Erweiterbarkeit - Neue Entity-Typen hinzufügen:**
```csharp
// 1. Interface erstellen und markieren
[DeltaDtoUnionMember(2, typeof(IEntity))]  // Nächster Union-Index
[GenerateDirtyTracking(IdPropertyName = "PersistentId")]
public interface IChestEntity : IEntity
{
    [TrackedProperty(DirtyFlags.State)]
    bool IsOpen { get; set; }
}

// 2. Build - Generator erstellt automatisch:
//    - IChestEntityDelta (mit EntityDeltaUnion-Interface)
//    - Aktualisierte EntityDeltaUnion mit IChestEntityDelta
//    - Factory-Methode ToDeltaUnion()

// 3. Sofort verwendbar - keine Änderung an ZoneDelta nötig!
var chestDelta = chest.ToDeltaUnion();
```

### ✅ Noch aktive Messages (Immediate Event-Based)

Die folgenden Entity-Messages bleiben **AKTIV** und werden **NICHT** durch `ZoneDelta` ersetzt:

| Message | ID | Grund |
|---------|-----|-------|
| **EntityAnimation** | 1404 | Combat-kritisch, braucht instant Feedback (~0 Latency) |
| **EntityAggro** | 1421 | Combat-kritisch, Threat-Management |
| **EntityEmote** | 1430 | Social Feature, erwartete ~0 Latency |
| **EntityInteract** | 1410 | Request/Response-Pattern |
| **EntityTarget** | 1420 | Request/Response-Pattern |

### Warum nicht gebatched?

**Sofortige Reaktion erforderlich:**
- **EntityAnimation**: Angriffs-Animations müssen instant sichtbar sein für Combat-Feedback
- **EntityAggro**: Spieler muss sofort sehen wenn Monster auf ihn zuläuft
- **EntityEmote**: Social-Interaktion erwartet ~0 Latency (Wave, Dance, etc.)

**Request/Response-Pattern:**
- **EntityInteract**: Client → Server Request, braucht direkte Response
- **EntityTarget**: Client → Server Request, braucht direkte Response

**Performance:**
Diese Messages sind **selten genug** dass Batching keinen Vorteil bringt:
- EntityAnimation: ~1-5/Sekunde pro Entity
- EntityAggro: ~0.1/Sekunde pro Entity
- EntityEmote: ~0.01/Sekunde pro Entity

**Batching würde schaden:**
- Künstliche Latenz durch warten auf nächsten Tick
- Schlechteres Spielgefühl bei Combat und Social-Features
- Komplexere Logik ohne Bandbreiten-Gewinn

Siehe [Chunk-Based Sync](../../02-architecture/CHUNK_BASED_SYNC.md) und [DIRTY_TRACKING.md](../../02-architecture/DIRTY_TRACKING.md) für Details zum Delta-DTO-Union-System.

---

## EntityInteract (1410)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Client möchte mit Entity interagieren (Talk, Loot, Open, etc.). Server validiert Range und Entity-Type, dann triggert entsprechende Action.

### Im Scope ✅

-   Interact mit allen Entity-Types
-   Range-Check (5m)
-   Type-Specific Actions (NPC → Dialog, Object → Loot, etc.)

### Request Payload

| Feld     | Typ | Beschreibung  | Pflicht |
| -------- | --- | ------------- | ------- |
| EntityId | int | Target-Entity | Ja      |

### Erwartete Response

-   **Bei NPC:** `NPCDialogOpen` (1301)
-   **Bei Object (Loot):** `LootRequest` Response
-   **Bei Object (Chest):** Chest-Open Message
-   **Bei Fehler:** `ErrorMessage` (910)

### Verwandte Messages

| Message       | ID   | Beziehung            |
| ------------- | ---- | -------------------- |
| `NPCInteract` | 1300 | Alternative für NPCs |
| `LootRequest` | 3100 | Für Loot-Objects     |

### Beispiel Payload

```csharp
var interact = new EntityInteract
{
    Type = MessageType.EntityInteract,
    EntityId = 12345 // NPC or Object
};
```

### Error Codes

| Code               | Bedeutung                              | Aktion           |
| ------------------ | -------------------------------------- | ---------------- |
| `OUT_OF_RANGE`     | Entity zu weit weg (>5m)               | Näher herangehen |
| `ENTITY_NOT_FOUND` | Entity existiert nicht                 | -                |
| `CANNOT_INTERACT`  | Entity ist nicht interactable          | -                |
| `IN_COMBAT`        | Kann nicht interagieren während Combat | Combat beenden   |

### Notizen

-   **Range**: Max 5m
-   **Facing**: Muss Entity anschauen (±90°)
-   **Priority**: Right-Click oder 'F'-Key

---

## EntityTarget (1420)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Client setzt Target (für Combat, Interact, Inspect). Server validiert und broadcastet Target-Change.

### Im Scope ✅

-   Target-Setting
-   Target-Clear
-   Broadcast an Nearby-Players (für UI-Indication)

### Request Payload

| Feld           | Typ | Beschreibung                     | Pflicht |
| -------------- | --- | -------------------------------- | ------- |
| TargetEntityId | int | Target-Entity (0 = clear target) | Ja      |

### Erwartete Response

-   `EntityTargetResponse` (1440)

### Folge-Messages bei Erfolg

-   `EntityTargetUpdate` Broadcast an nahe Spieler

### Beispiel Payload

```csharp
// Set Target
var setTarget = new EntityTarget
{
    Type = MessageType.EntityTarget,
    TargetEntityId = 45678 // Monster
};

// Clear Target
var clearTarget = new EntityTarget
{
    Type = MessageType.EntityTarget,
    TargetEntityId = 0
};
```

### Notizen

-   **UI**: Target-Frame wird angezeigt
-   **Combat**: Erfordert Target für meiste Actions
-   **Inspect**: Erlaubt Inspect von anderen Players

---

## EntityAggro (1421)

**Richtung:** 📡 Broadcast (Server → Nearby Players)  
**Frequenz:** Häufig (Combat)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Server broadcastet Aggro-Change für Monster/NPC. Zeigt wer aktuell Target ist.

### Broadcast Payload

| Feld        | Typ | Beschreibung     | Pflicht |
| ----------- | --- | ---------------- | ------- |
| EntityId    | int | Monster/NPC      | Ja      |
| TargetId    | int | Aktuelles Target | Ja      |
| ThreatLevel | int | Threat-Amount    | Nein    |

### Beispiel Payload

```csharp
var aggro = new EntityAggro
{
    Type = MessageType.EntityAggro,
    EntityId = 45678, // Goblin
    TargetId = 98765, // Aragorn
    ThreatLevel = 5000
};
```

### Notizen

-   **Visual**: Red-Line von Monster zu Target
-   **Threat**: Siehe `ThreatUpdate` (312) für Details

---

## EntityEmote (1430)

**Richtung:** 📡 Broadcast (Server → Nearby Players)  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Server broadcastet Emote (Visual-Animation + optional Text). Siehe auch `ChatEmote` (413) für Text-Only.

### Broadcast Payload

| Feld     | Typ  | Beschreibung    | Pflicht |
| -------- | ---- | --------------- | ------- |
| EntityId | int  | Emoting Entity  | Ja      |
| EmoteId  | uint | Emote-ID        | Ja      |
| TargetId | int  | Optional Target | Nein    |

**Emote-IDs**:

-   1: Wave
-   2: Bow
-   3: Dance
-   4: Laugh
-   5: Cry
-   ... etc.

### Beispiel Payload

```csharp
var emote = new EntityEmote
{
    Type = MessageType.EntityEmote,
    EntityId = 98765,
    EmoteId = 1, // Wave
    TargetId = 54321 // Wave at Legolas
};
```

### Notizen

-   **Animation**: Client spielt Emote-Animation
-   **Sound**: Optional Emote-Sound
-   **Text**: Siehe `ChatEmote` (413) für "_Aragorn waves at Legolas._"

---

## EntityTargetResponse (1440)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung

Antwort auf EntityTarget Request. Bestätigt erfolgreiche Entity-Target-Änderung oder gibt Fehler zurück.

### Response Payload

| Feld         | Typ    | Beschreibung                   | Pflicht    |
| ------------ | ------ | ------------------------------ | ---------- |
| Success      | bool   | Target-Änderung erfolgreich?   | Ja         |
| ErrorCode    | string | Fehlercode falls Success=false | Nein       |
| ErrorMessage | string | Menschenlesbare Fehlermeldung  | Nein       |
| EntityId     | int    | Entity die Target geändert hat | Bei Erfolg |
| NewTargetId  | int    | Neues Target                   | Bei Erfolg |

### Error Codes

| Code             | Bedeutung                           |
| ---------------- | ----------------------------------- |
| `INVALID_TARGET` | Target kann nicht selektiert werden |

---

## 🔗 Verwandte Kategorien

-   **Movement (02)**: Player-Movement → `PlayerMove` (200), `PlayerTeleport` (207)
-   **Combat (03)**: Combat-Actions → `ActionRequest` (300), `DamageEvent` (302)
-   **Targeting (12)**: Target-System → `TargetEntity` (1200)
-   **NPC (13)**: NPC-Specific → `NPCInteract` (1300), `NPCDialogOpen` (1301)
-   **Loot (31)**: Object-Interaction → `LootRequest` (3100)

---

**Letzte Aktualisierung**: 2025-12-25  
**Version**: 2.1.0  
**Status**: ✅ Vollständig dokumentiert (11/11 Messages)

[← Zurück zur Übersicht](README.md)
