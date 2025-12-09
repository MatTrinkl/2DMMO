# 🆔 ID-System

## 2DMMO – Entity Identity Architecture

**Version:** 1.0.0  
**Letzte Aktualisierung:** 2025-12-09  
**Teil von:** [Architektur-Dokumentation](../ARCHITECTURE.md)

---

## 📋 Inhaltsverzeichnis

1. [Übersicht](#übersicht)
2. [ID-Typen](#id-typen)
3. [EntityIdentity Struktur](#entityidentity-struktur)
4. [GlobalEntityKey](#globalentitykey)
5. [Persistente vs. Runtime IDs](#persistente-vs-runtime-ids)
6. [ZoneId Ranges](#zoneid-ranges)
7. [ID-Vergabe](#id-vergabe)
8. [Verhalten bei Zonenwechsel](#verhalten-bei-zonenwechsel)
9. [Code-Beispiele](#code-beispiele)
10. [Diagramme](#diagramme)

---

## Übersicht

Das ID-System des 2DMMO verwendet eine hierarchische Struktur zur eindeutigen Identifikation aller Entities in der Spielwelt. Das System ist optimiert für:

- **Performance:** Schnelle Lookups und Dictionary-Keys
- **Skalierbarkeit:** Multi-World, Multi-Zone, Multi-Shard Architektur
- **Flexibilität:** Unterstützt sowohl persistente als auch temporäre Entities
- **Netzwerk-Effizienz:** Kompakte Serialisierung (10 Bytes pro Entity)

---

## ID-Typen

### Übersichtstabelle

| ID | Typ | Änderung bei Zonenwechsel | Persistenz | Beschreibung |
|----|-----|---------------------------|------------|--------------|
| `AccountId` | Guid | ❌ Nie | Datenbank | Eindeutiger Spieler-Account (Login, OAuth) |
| `CharacterId` | int | ❌ Nie | Datenbank | Charakter des Spielers (mehrere pro Account möglich) |
| `WorldId` | byte | ❌ Selten | Runtime | Welt/Region (für Multi-Region Support) |
| `ZoneId` | ushort | ✅ Ja | Runtime | Aktuelle Zone (0-65535) |
| `ShardId` | byte | ✅ Ja | Runtime | Shard innerhalb der Zone (0-255) |
| `EntityId` | int | ✅ Ja | Runtime | Eindeutige ID innerhalb des Shards |
| `PrefabId` | ushort | ❌ Nie | Konstant | Entity-Template (z.B. "Goblin", "Chest") |

### Detaillierte Beschreibung

#### AccountId (Guid)
- **Zweck:** Eindeutiger Account-Identifier für Login und OAuth
- **Persistenz:** PostgreSQL Datenbank
- **Verwendung:** Authentication, Account-Management, Freundeslisten
- **Beispiel:** `f47ac10b-58cc-4372-a567-0e02b2c3d479`

#### CharacterId (int)
- **Zweck:** Eindeutiger Charakter innerhalb eines Accounts
- **Persistenz:** PostgreSQL Datenbank
- **Verwendung:** Charakter-Auswahl, Persistierung von Charakter-Daten
- **Range:** 1 - 2,147,483,647
- **Hinweis:** Ein Account kann mehrere Charaktere haben (z.B. max. 5)

#### WorldId (byte)
- **Zweck:** Unterscheidung verschiedener Welten/Regionen (für zukünftige Multi-Region Skalierung)
- **Persistenz:** Runtime (konfigurierbar, selten geändert)
- **Verwendung:** Multi-Region Support (EU, US, Asia)
- **Range:** 1 - 255 (0 = ungültig)
- **Prototyp:** Immer `1` (Single-World)

#### ZoneId (ushort)
- **Zweck:** Eindeutige Zone innerhalb einer Welt
- **Persistenz:** Runtime (aus Zone-Konfiguration)
- **Verwendung:** Zone-Lookups, Broadcasting, Zone-Transfers
- **Range:** 1 - 65535 (0 = ungültig)
- **Siehe auch:** [ZoneId Ranges](#zoneid-ranges) für Zone-Kategorien

#### ShardId (byte)
- **Zweck:** Shard innerhalb einer Zone für Skalierung bei hoher Spielerzahl
- **Persistenz:** Runtime (dynamisch vom ShardSelector vergeben)
- **Verwendung:** Last-Verteilung innerhalb einer Zone
- **Range:** 0 - 255
- **Prototyp:** Immer `0` (Single-Shard pro Zone)

#### EntityId (int)
- **Zweck:** Eindeutige ID innerhalb eines Zone-Shards
- **Persistenz:** Runtime (pro Zone/Shard Counter)
- **Verwendung:** Entity-Lookups innerhalb eines Shards
- **Range:** 1 - 2,147,483,647
- **Vergabe:** Aufsteigender Counter pro Zone/Shard

#### PrefabId (ushort)
- **Zweck:** Template/Typ der Entity (z.B. "Goblin", "Chest", "Player")
- **Persistenz:** Konstant (Teil der Spiel-Konfiguration)
- **Verwendung:** Entity-Spawning, Client-Rendering, Verhalten
- **Range:** 1 - 65535 (0 = ungültig)
- **Beispiele:** `1 = Player`, `100 = Goblin`, `500 = Wooden Chest`

---

## EntityIdentity Struktur

### Memory-Layout

```
┌─────────────────────────────────────────────────────────────────┐
│                      EntityIdentity                             │
├─────────────────────────────────────────────────────────────────┤
│  WorldId    │  ZoneId   │  ShardId  │  EntityId  │  PrefabId   │
│  (byte)     │  (ushort) │  (byte)   │  (int)     │  (ushort)   │
├─────────────────────────────────────────────────────────────────┤
│  1 Byte     │  2 Bytes  │  1 Byte   │  4 Bytes   │  2 Bytes    │
└─────────────────────────────────────────────────────────────────┘
                    Total: 10 Bytes pro Entity
```

### C# Implementierung

```csharp
using MessagePack;

namespace Mmo.Shared.Entities;

/// <summary>
///     Vollständige Identität einer Entity in der Spielwelt.
///     Ermöglicht eindeutige Identifikation über World, Zone, Shard und Entity-ID.
/// </summary>
[MessagePackObject]
public readonly struct EntityIdentity : IEquatable<EntityIdentity>
{
    /// <summary>
    ///     World/Region ID (für Multi-Region Support).
    /// </summary>
    [Key(0)]
    public byte WorldId { get; init; }

    /// <summary>
    ///     Zone ID innerhalb der World.
    /// </summary>
    [Key(1)]
    public ushort ZoneId { get; init; }

    /// <summary>
    ///     Shard ID innerhalb der Zone (für Skalierung).
    /// </summary>
    [Key(2)]
    public byte ShardId { get; init; }

    /// <summary>
    ///     Entity ID innerhalb des Shards.
    /// </summary>
    [Key(3)]
    public int EntityId { get; init; }

    /// <summary>
    ///     Prefab/Template ID (z.B. "Player", "Goblin", "Chest").
    /// </summary>
    [Key(4)]
    public ushort PrefabId { get; init; }

    /// <summary>
    ///     Erstellt eine neue EntityIdentity.
    /// </summary>
    public EntityIdentity(byte worldId, ushort zoneId, byte shardId, int entityId, ushort prefabId)
    {
        WorldId = worldId;
        ZoneId = zoneId;
        ShardId = shardId;
        EntityId = entityId;
        PrefabId = prefabId;
    }

    /// <summary>
    ///     Berechnet einen globalen Key für schnelle Lookups.
    ///     Format: WorldId (8 bits) | ZoneId (16 bits) | ShardId (8 bits) | EntityId (32 bits)
    /// </summary>
    [IgnoreMember]
    public long GlobalKey => ((long)WorldId << 56) | ((long)ZoneId << 40) | ((long)ShardId << 32) | (long)EntityId;

    public bool Equals(EntityIdentity other)
    {
        return WorldId == other.WorldId 
            && ZoneId == other.ZoneId 
            && ShardId == other.ShardId 
            && EntityId == other.EntityId 
            && PrefabId == other.PrefabId;
    }

    public override bool Equals(object? obj) => obj is EntityIdentity other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(WorldId, ZoneId, ShardId, EntityId, PrefabId);

    public static bool operator ==(EntityIdentity left, EntityIdentity right) => left.Equals(right);

    public static bool operator !=(EntityIdentity left, EntityIdentity right) => !left.Equals(right);

    public override string ToString() 
        => $"World:{WorldId} Zone:{ZoneId} Shard:{ShardId} Entity:{EntityId} Prefab:{PrefabId}";
}
```

### Eigenschaften

| Eigenschaft | Typ | Beschreibung |
|-------------|-----|--------------|
| `WorldId` | byte | Welt/Region (1-255) |
| `ZoneId` | ushort | Zone (1-65535) |
| `ShardId` | byte | Shard (0-255) |
| `EntityId` | int | Entity innerhalb Shard (1-2.1B) |
| `PrefabId` | ushort | Template/Typ (1-65535) |
| `GlobalKey` | long | Kombinierter Key für Dictionary-Lookups |

---

## GlobalEntityKey

### Konzept

Der `GlobalKey` ist ein 64-bit long-Wert, der aus den ersten 4 ID-Komponenten (WorldId, ZoneId, ShardId, EntityId) berechnet wird. Er dient als **performanter Dictionary-Key** für schnelle Entity-Lookups.

### Bit-Layout

```
┌─────────────────────────────────────────────────────────────────┐
│                     GlobalKey (64 bits / 8 bytes)               │
├────────────┬────────────────────┬──────────┬───────────────────┤
│  WorldId   │      ZoneId        │ ShardId  │     EntityId      │
│  (8 bits)  │    (16 bits)       │ (8 bits) │    (32 bits)      │
├────────────┴────────────────────┴──────────┴───────────────────┤
│   Byte 7   │   Byte 6-5        │  Byte 4  │   Byte 3-0        │
└─────────────────────────────────────────────────────────────────┘
```

### Berechnung

```csharp
long GlobalKey = ((long)WorldId << 56)   // WorldId an Position 56-63
               | ((long)ZoneId << 40)     // ZoneId an Position 40-55
               | ((long)ShardId << 32)    // ShardId an Position 32-39
               | (long)EntityId;          // EntityId an Position 0-31 (cast verhindert Sign-Extension)
```

### Verwendung

```csharp
// Dictionary mit GlobalKey als Index
private readonly Dictionary<long, EntityState> _entitiesByGlobalKey = new();

// Schneller Lookup
public EntityState? GetEntity(EntityIdentity identity)
{
    return _entitiesByGlobalKey.TryGetValue(identity.GlobalKey, out var entity) 
        ? entity 
        : null;
}

// Einfügen
public void AddEntity(EntityIdentity identity, EntityState state)
{
    _entitiesByGlobalKey[identity.GlobalKey] = state;
}
```

### Vorteile

✅ **Performance:** O(1) Lookup statt verschachtelter Dictionaries  
✅ **Memory:** Einzelner long (8 Bytes) als Key  
✅ **Einfachheit:** Keine komplexen Key-Objekte  
✅ **Sortierbar:** Entities können nach GlobalKey sortiert werden

### Hinweis

⚠️ **PrefabId ist NICHT Teil des GlobalKey**, da eine Entity-ID innerhalb eines Shards bereits eindeutig ist. Die PrefabId wird separat gespeichert.

---

## Persistente vs. Runtime IDs

### Persistente IDs (bleiben immer gleich)

Diese IDs werden in der PostgreSQL Datenbank gespeichert und ändern sich nie:

| ID | Typ | Persistenz | Use Case |
|----|-----|------------|----------|
| `AccountId` | Guid | ✅ Datenbank | Login, OAuth, Account-Management |
| `CharacterId` | int | ✅ Datenbank | Charakter-Persistierung, Inventar, Quests |
| `PrefabId` | ushort | ✅ Konstant | Entity-Templates, Spawning |

**Beispiel:** Ein Spieler mit `CharacterId = 42` behält diese ID für immer, egal in welcher Zone er sich befindet.

### Runtime IDs (ändern sich bei Zonenwechsel)

Diese IDs werden während der Laufzeit vergeben und ändern sich, wenn ein Spieler die Zone wechselt:

| ID | Typ | Persistenz | Use Case |
|----|-----|------------|----------|
| `WorldId` | byte | ⚠️ Runtime | Multi-Region (ändert sich selten) |
| `ZoneId` | ushort | ❌ Runtime | Aktuelle Zone |
| `ShardId` | byte | ❌ Runtime | Aktueller Shard |
| `EntityId` | int | ❌ Runtime | ID innerhalb des Shards |

**Beispiel:** Ein Spieler wechselt von Zone 1 (Startzone) zu Zone 2 (Hauptstadt):
- **Vorher:** `ZoneId=1, ShardId=0, EntityId=5`
- **Nachher:** `ZoneId=2, ShardId=0, EntityId=12` (neue EntityId!)

### Warum Runtime IDs?

**Skalierbarkeit:**
- Jeder Zone/Shard verwaltet seinen eigenen EntityId-Counter
- Keine globalen Locks oder Datenbank-Queries für ID-Vergabe
- Ermöglicht parallele ID-Vergabe in verschiedenen Zonen

**Performance:**
- Lokale ID-Vergabe ist extrem schnell (Interlocked.Increment)
- Keine Netzwerk-Latenz oder Datenbank-Zugriff

**Flexibilität:**
- Zone/Shard kann neu gestartet werden ohne ID-Kollisionen
- Einfache Shard-Migration bei Lastverteilung

---

## ZoneId Ranges

Die `ZoneId` (ushort, 0-65535) ist in Kategorien unterteilt, um verschiedene Zone-Typen zu unterscheiden:

```csharp
namespace Mmo.Shared;

/// <summary>
///     Definiert die Bereiche für verschiedene Zone-Typen.
/// </summary>
public static class ZoneIdRanges
{
    // ═══════════════════════════════════════════════════
    // PERSISTENTE WELTZONEN (1 - 9999)
    // ═══════════════════════════════════════════════════
    /// <summary>
    ///     Minimum ZoneId für persistente Weltzonen (inklusive).
    /// </summary>
    public const ushort PersistentZoneMin = 1;

    /// <summary>
    ///     Maximum ZoneId für persistente Weltzonen (inklusive).
    /// </summary>
    public const ushort PersistentZoneMax = 9999;

    // Beispiele: Startzone = 1, Hauptstadt = 2, Wald = 3, etc.

    // ═══════════════════════════════════════════════════
    // DUNGEONS (10000 - 19999)
    // ═══════════════════════════════════════════════════
    /// <summary>
    ///     Minimum ZoneId für Dungeons (inklusive).
    /// </summary>
    public const ushort DungeonZoneMin = 10000;

    /// <summary>
    ///     Maximum ZoneId für Dungeons (inklusive).
    /// </summary>
    public const ushort DungeonZoneMax = 19999;

    // Beispiele: Goblin Cave = 10001, Dragon Lair = 10002, etc.

    // ═══════════════════════════════════════════════════
    // ARENEN (20000 - 29999)
    // ═══════════════════════════════════════════════════
    /// <summary>
    ///     Minimum ZoneId für PvP Arenen (inklusive).
    /// </summary>
    public const ushort ArenaZoneMin = 20000;

    /// <summary>
    ///     Maximum ZoneId für PvP Arenen (inklusive).
    /// </summary>
    public const ushort ArenaZoneMax = 29999;

    // Beispiele: 1v1 Arena = 20001, 5v5 Battleground = 20002, etc.

    // ═══════════════════════════════════════════════════
    // HOUSING (30000 - 39999)
    // ═══════════════════════════════════════════════════
    /// <summary>
    ///     Minimum ZoneId für Housing Instanzen (inklusive).
    /// </summary>
    public const ushort HousingZoneMin = 30000;

    /// <summary>
    ///     Maximum ZoneId für Housing Instanzen (inklusive).
    /// </summary>
    public const ushort HousingZoneMax = 39999;

    // Beispiele: Player House 1 = 30001, Guild Hall 5 = 30005, etc.

    // ═══════════════════════════════════════════════════
    // EVENTS (40000 - 49999)
    // ═══════════════════════════════════════════════════
    /// <summary>
    ///     Minimum ZoneId für Event-Zonen (inklusive).
    /// </summary>
    public const ushort EventZoneMin = 40000;

    /// <summary>
    ///     Maximum ZoneId für Event-Zonen (inklusive).
    /// </summary>
    public const ushort EventZoneMax = 49999;

    // Beispiele: Christmas Event = 40001, Halloween Event = 40002, etc.

    // ═══════════════════════════════════════════════════
    // RESERVIERT (50000 - 65535)
    // ═══════════════════════════════════════════════════
    /// <summary>
    ///     Minimum ZoneId für reservierte/zukünftige Zone-Typen (inklusive).
    /// </summary>
    public const ushort ReservedZoneMin = 50000;

    /// <summary>
    ///     Maximum ZoneId (inklusive).
    /// </summary>
    public const ushort ReservedZoneMax = 65535;

    // Für zukünftige Erweiterungen reserviert

    // ═══════════════════════════════════════════════════
    // HELPER METHODS
    // ═══════════════════════════════════════════════════

    /// <summary>
    ///     Prüft ob die ZoneId eine persistente Weltzone ist.
    /// </summary>
    public static bool IsPersistentZone(ushort zoneId) 
        => zoneId >= PersistentZoneMin && zoneId <= PersistentZoneMax;

    /// <summary>
    ///     Prüft ob die ZoneId ein Dungeon ist.
    /// </summary>
    public static bool IsDungeon(ushort zoneId) 
        => zoneId >= DungeonZoneMin && zoneId <= DungeonZoneMax;

    /// <summary>
    ///     Prüft ob die ZoneId eine Arena ist.
    /// </summary>
    public static bool IsArena(ushort zoneId) 
        => zoneId >= ArenaZoneMin && zoneId <= ArenaZoneMax;

    /// <summary>
    ///     Prüft ob die ZoneId eine Housing-Instanz ist.
    /// </summary>
    public static bool IsHousing(ushort zoneId) 
        => zoneId >= HousingZoneMin && zoneId <= HousingZoneMax;

    /// <summary>
    ///     Prüft ob die ZoneId eine Event-Zone ist.
    /// </summary>
    public static bool IsEvent(ushort zoneId) 
        => zoneId >= EventZoneMin && zoneId <= EventZoneMax;

    /// <summary>
    ///     Prüft ob die ZoneId gültig ist.
    /// </summary>
    public static bool IsValid(ushort zoneId) => zoneId > 0;
}
```

### Übersichtstabelle

| Range | Zone-Typ | Persistierung | Instanzierung | Beispiele |
|-------|----------|---------------|---------------|-----------|
| **1 - 9999** | Persistente Weltzonen | ✅ Immer | Statisch beim Start | Startzone, Hauptstadt, Wald |
| **10000 - 19999** | Dungeons | ❌ Temporär | Dynamisch on-demand | Goblin Cave, Dragon Lair |
| **20000 - 29999** | Arenen | ❌ Temporär | Dynamisch on-demand | 1v1 Arena, 5v5 Battleground |
| **30000 - 39999** | Housing | ✅ Pro Spieler | Dynamisch bei Bedarf | Player Houses, Guild Halls |
| **40000 - 49999** | Events | ❌ Event-basiert | Dynamisch bei Event | Christmas, Halloween |
| **50000 - 65535** | Reserviert | - | - | Zukünftige Erweiterungen |

### Verwendung

```csharp
// Zone-Typ prüfen
if (ZoneIdRanges.IsDungeon(zoneId))
{
    // Dungeon-spezifische Logik
    // z.B. Loot-Boost, instanzierte Drops
}

if (ZoneIdRanges.IsPersistentZone(zoneId))
{
    // Persistente Zone
    // z.B. Position wird in DB gespeichert
}
```

---

## ID-Vergabe

### EntityId-Vergabe pro Zone/Shard

Jeder Zone/Shard hat seinen eigenen **EntityId-Counter**, der bei 1 startet und aufsteigend vergeben wird:

```csharp
namespace Mmo.Server.Zones;

/// <summary>
///     Verwaltet Entities innerhalb eines Zone/Shard Paares.
/// </summary>
public class ZoneEntityManager
{
    private readonly byte _worldId;
    private readonly ushort _zoneId;
    private readonly byte _shardId;
    
    private int _nextEntityId = 1;
    private readonly Dictionary<long, EntityState> _entities = new();

    public ZoneEntityManager(byte worldId, ushort zoneId, byte shardId)
    {
        _worldId = worldId;
        _zoneId = zoneId;
        _shardId = shardId;
    }

    /// <summary>
    ///     Erstellt eine neue EntityIdentity mit der nächsten verfügbaren EntityId.
    /// </summary>
    public EntityIdentity CreateEntityIdentity(ushort prefabId)
    {
        int entityId = Interlocked.Increment(ref _nextEntityId);
        
        return new EntityIdentity(
            worldId: _worldId,
            zoneId: _zoneId,
            shardId: _shardId,
            entityId: entityId,
            prefabId: prefabId
        );
    }

    /// <summary>
    ///     Registriert eine neue Entity im Manager.
    /// </summary>
    public void AddEntity(EntityIdentity identity, EntityState state)
    {
        _entities[identity.GlobalKey] = state;
    }

    /// <summary>
    ///     Entfernt eine Entity aus dem Manager.
    /// </summary>
    public void RemoveEntity(EntityIdentity identity)
    {
        _entities.Remove(identity.GlobalKey);
    }

    /// <summary>
    ///     Holt eine Entity nach Identity.
    /// </summary>
    public EntityState? GetEntity(EntityIdentity identity)
    {
        return _entities.TryGetValue(identity.GlobalKey, out var entity) 
            ? entity 
            : null;
    }

    /// <summary>
    ///     Gibt alle Entities zurück.
    /// </summary>
    public IEnumerable<EntityState> GetAllEntities()
    {
        return _entities.Values;
    }
}
```

### Vergabe-Eigenschaften

| Aspekt | Detail |
|--------|--------|
| **Thread-Safety** | Ja, via `Interlocked.Increment` |
| **Startwert** | 1 (0 ist ungültig) |
| **Performance** | O(1), keine Locks |
| **Kollisionen** | Unmöglich innerhalb eines Zone/Shard |
| **Wiederverwendung** | Nein, Counter läuft nur aufwärts |

### ShardId-Vergabe

Im Prototyp ist `ShardId` immer `0` (Single-Shard pro Zone). In einer skalierten Umgebung würde ein **ShardSelector** basierend auf Last entscheiden:

```csharp
public class ShardSelector
{
    private readonly Dictionary<ushort, List<ShardInfo>> _shardsByZone = new();

    /// <summary>
    ///     Wählt den Shard mit der geringsten Last.
    /// </summary>
    public byte SelectShard(ushort zoneId)
    {
        if (!_shardsByZone.TryGetValue(zoneId, out var shards) || shards.Count == 0)
        {
            return 0; // Default-Shard
        }

        // Shard mit geringster Spielerzahl
        var bestShard = shards.OrderBy(s => s.PlayerCount).FirstOrDefault();
        return bestShard?.ShardId ?? 0;
    }
}

public class ShardInfo
{
    public byte ShardId { get; init; }
    public int PlayerCount { get; set; }
    
    /// <summary>
    ///     Maximale Kapazität pro Shard. In der Produktion aus Konfiguration geladen.
    /// </summary>
    public int MaxCapacity { get; init; } = 200; // Konfigurierbar in appsettings.json
}
```

### WorldId-Konfiguration

Die `WorldId` ist serverseitig konfiguriert und ändert sich selten:

```json
// appsettings.json
{
  "Server": {
    "WorldId": 1,
    "Region": "EU"
  }
}
```

Für Multi-Region Support:
- **EU Server:** `WorldId = 1`
- **US Server:** `WorldId = 2`
- **Asia Server:** `WorldId = 3`

---

## Verhalten bei Zonenwechsel

### Was ändert sich, was bleibt gleich?

| Feld | Zone→Zone | Shard→Shard | Zone→Dungeon | Dungeon→Zone |
|------|-----------|-------------|--------------|--------------|
| **AccountId** | ✅ gleich | ✅ gleich | ✅ gleich | ✅ gleich |
| **CharacterId** | ✅ gleich | ✅ gleich | ✅ gleich | ✅ gleich |
| **PrefabId** | ✅ gleich | ✅ gleich | ✅ gleich | ✅ gleich |
| **WorldId** | ✅ gleich | ✅ gleich | ✅ gleich | ✅ gleich |
| **ZoneId** | ❌ neu | ✅ gleich | ❌ neu | ❌ neu |
| **ShardId** | ❌ neu | ❌ neu | ❌ neu | ❌ neu |
| **EntityId** | ❌ neu | ❌ neu | ❌ neu | ❌ neu |

### Zonenwechsel-Flow

```
┌─────────────────────────────────────────────────────────────────┐
│                  ZONENWECHSEL-FLOW                              │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  1️⃣ Spieler in Zone A (Startzone)                              │
│     Identity: World:1 Zone:1 Shard:0 Entity:5 Prefab:1         │
│                                                                 │
│  2️⃣ Spieler läuft zur Zonengrenze                              │
│     ZoneBounds.IsNearEdge(x, y, threshold) → true              │
│                                                                 │
│  3️⃣ Server initiiert Zone-Transfer                             │
│     • RemoveEntity(Zone A, EntityId:5)                          │
│     • Persistiere Charakter-Daten (Position, HP, etc.)         │
│                                                                 │
│  4️⃣ Spieler wechselt zu Zone B (Hauptstadt)                    │
│     • Neue EntityId wird vergeben: EntityId:12                 │
│     • Neue Identity: World:1 Zone:2 Shard:0 Entity:12 Prefab:1│
│     • AddEntity(Zone B, EntityId:12)                            │
│                                                                 │
│  5️⃣ Client-Benachrichtigung                                    │
│     • ZoneTransferMessage mit neuer Identity                   │
│     • Client lädt Zone B Assets                                │
│     • Neue Spieler-Liste für Zone B                            │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

### Code-Beispiel: Zone-Transfer

```csharp
public class ZoneTransferManager
{
    private readonly ZoneManager _zoneManager;
    private readonly CharacterRepository _characterRepo;

    /// <summary>
    ///     Transferiert einen Spieler von einer Zone zu einer anderen.
    /// </summary>
    public async Task<EntityIdentity> TransferPlayerAsync(
        Player player,
        ushort fromZoneId,
        ushort toZoneId)
    {
        // 1. Alte Zone
        var fromZone = _zoneManager.GetZone(fromZoneId);
        var oldIdentity = player.Identity;

        // 2. Aus alter Zone entfernen
        fromZone.RemoveEntity(oldIdentity);

        // 3. Charakter-Daten persistieren
        await _characterRepo.SaveCharacterStateAsync(player.CharacterId, player.State);

        // 4. Neue Zone
        var toZone = _zoneManager.GetZone(toZoneId);
        var shardId = SelectShardForZone(toZoneId); // Im Prototyp: 0

        // 5. Neue EntityIdentity erstellen
        var newIdentity = toZone.EntityManager.CreateEntityIdentity(player.PrefabId);

        // 6. Spieler-Daten aktualisieren
        player.Identity = newIdentity;
        player.Position = toZone.GetSpawnPosition(); // Spawn-Position in neuer Zone

        // 7. In neue Zone einfügen
        toZone.AddEntity(newIdentity, player.State);

        // 8. Client benachrichtigen
        await SendZoneTransferMessageAsync(player, newIdentity, toZone);

        // 9. Andere Spieler benachrichtigen
        await BroadcastPlayerJoinedAsync(toZone, player);
        await BroadcastPlayerLeftAsync(fromZone, oldIdentity);

        return newIdentity;
    }
}
```

### Wichtige Hinweise

⚠️ **EntityId ist NICHT stabil über Zonenwechsel!**
- Verwende NIEMALS `EntityId` alleine für persistente Referenzen
- Für persistente Daten: Verwende `CharacterId` oder `AccountId`
- `EntityId` ist nur innerhalb einer Zone/Shard eindeutig

✅ **Was persistent ist:**
- `AccountId` — Immer gleich
- `CharacterId` — Immer gleich
- `PrefabId` — Immer gleich (Spieler bleibt Spieler)

❌ **Was sich ändert:**
- `ZoneId` — Neue Zone
- `ShardId` — Neuer Shard (je nach Last-Verteilung)
- `EntityId` — Neue ID im neuen Shard

---

## Code-Beispiele

### Beispiel 1: Entity erstellen

```csharp
// Zone/Shard Manager hat WorldId, ZoneId, ShardId
var zoneManager = new ZoneEntityManager(
    worldId: 1,    // EU Server
    zoneId: 1,     // Startzone
    shardId: 0     // Erster Shard
);

// Neue Player-Entity erstellen
var playerIdentity = zoneManager.CreateEntityIdentity(prefabId: 1); // 1 = Player

// Ergebnis:
// World:1 Zone:1 Shard:0 Entity:1 Prefab:1

// Zweiter Player
var secondPlayerIdentity = zoneManager.CreateEntityIdentity(prefabId: 1);
// World:1 Zone:1 Shard:0 Entity:2 Prefab:1
```

### Beispiel 2: Entity-Lookup via GlobalKey

```csharp
// Dictionary mit GlobalKey
var entities = new Dictionary<long, EntityState>();

// Entity hinzufügen
entities[playerIdentity.GlobalKey] = new PlayerState
{
    CharacterId = 42,
    Username = "TestPlayer",
    Position = new Position(100, 200)
};

// Entity abrufen
if (entities.TryGetValue(playerIdentity.GlobalKey, out var player))
{
    Console.WriteLine($"Found player: {player.Username}");
}
```

### Beispiel 3: MessagePack Serialisierung

```csharp
using MessagePack;

// EntityIdentity serialisieren
var identity = new EntityIdentity(
    worldId: 1,
    zoneId: 1,
    shardId: 0,
    entityId: 5,
    prefabId: 1
);

byte[] bytes = MessagePackSerializer.Serialize(identity);
// Nur 10 Bytes!

// Deserialisieren
var deserialized = MessagePackSerializer.Deserialize<EntityIdentity>(bytes);

Console.WriteLine(deserialized.ToString());
// Output: World:1 Zone:1 Shard:0 Entity:5 Prefab:1
```

### Beispiel 4: Zone-spezifischer Broadcast

```csharp
public class ZoneBroadcaster
{
    /// <summary>
    ///     Sendet ZoneState nur an Spieler in der Zone.
    /// </summary>
    public async Task BroadcastZoneStateAsync(Zone zone)
    {
        var players = zone.GetPlayers();

        var zoneState = new ZoneState
        {
            ZoneId = zone.ZoneId,
            Entities = players
                .Select(p => new EntityData
                {
                    Identity = p.Identity,  // Vollständige EntityIdentity
                    Position = p.Position,
                    // ... weitere Daten
                })
                .ToList()
        };

        byte[] message = MessagePackSerializer.Serialize(zoneState);

        // Nur an Spieler in dieser Zone senden
        foreach (var player in players)
        {
            await player.Connection.SendAsync(message);
        }
    }
}
```

### Beispiel 5: ZoneId Range Check

```csharp
// Zone-Typ prüfen
public bool CanPlayerEnter(ushort zoneId, Player player)
{
    if (ZoneIdRanges.IsDungeon(zoneId))
    {
        // Dungeons haben Level-Anforderung
        return player.Level >= GetDungeonMinLevel(zoneId);
    }

    if (ZoneIdRanges.IsHousing(zoneId))
    {
        // Housing nur für Besitzer/Gildenmitglieder
        return player.CharacterId == GetHousingOwnerId(zoneId);
    }

    // Persistente Zonen sind für alle zugänglich
    return ZoneIdRanges.IsPersistentZone(zoneId);
}
```

---

## Diagramme

### Diagramm 1: EntityIdentity Hierarchie

```
┌─────────────────────────────────────────────────────────────────┐
│                    ENTITY IDENTITY HIERARCHIE                   │
│                                                                 │
│                         WORLD 1 (EU)                            │
│                              │                                  │
│              ┌───────────────┼───────────────┐                 │
│              │               │               │                 │
│          Zone 1          Zone 2          Zone 10001            │
│        (Startzone)    (Hauptstadt)      (Dungeon)              │
│              │               │               │                 │
│     ┌────────┴────────┐      │               │                 │
│  Shard 0          Shard 1    │               │                 │
│     │                 │      │               │                 │
│  Entities:        Entities:  │               │                 │
│  ├─ Entity 1      ├─ Entity 1│               │                 │
│  ├─ Entity 2      ├─ Entity 2│               │                 │
│  ├─ Entity 3      ├─ Entity 3│               │                 │
│  └─ Entity 4      └─ Entity 4│               │                 │
│                               │               │                 │
│                          Shard 0         Shard 0               │
│                               │               │                 │
│                           Entities:       Entities:            │
│                           ├─ Entity 1     ├─ Entity 1          │
│                           ├─ Entity 2     ├─ Entity 2          │
│                           └─ Entity 3     └─ Entity 3          │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

### Diagramm 2: ID-Flow bei Zonenwechsel

```
┌─────────────────────────────────────────────────────────────────┐
│               ID-FLOW BEI ZONENWECHSEL                          │
│                                                                 │
│  SPIELER IN ZONE A                                             │
│  ┌──────────────────────────────────────────────────────┐      │
│  │ AccountId:    f47ac10b-58cc-4372-a567-0e02b2c3d479  │      │
│  │ CharacterId:  42                        ◄─── PERSISTENT     │
│  │ PrefabId:     1 (Player)                ◄─── PERSISTENT     │
│  │ WorldId:      1                         ◄─── CONFIG         │
│  │ ─────────────────────────────────────────────────── │      │
│  │ ZoneId:       1  (Startzone)           ◄─── ZONE A          │
│  │ ShardId:      0                        ◄─── ZONE A          │
│  │ EntityId:     5                        ◄─── ZONE A          │
│  └──────────────────────────────────────────────────────┘      │
│                          │                                      │
│                          ▼                                      │
│                  [ZONE TRANSFER]                               │
│                          │                                      │
│                          ▼                                      │
│  SPIELER IN ZONE B                                             │
│  ┌──────────────────────────────────────────────────────┐      │
│  │ AccountId:    f47ac10b-58cc-4372-a567-0e02b2c3d479  │      │
│  │ CharacterId:  42                        ◄─── PERSISTENT     │
│  │ PrefabId:     1 (Player)                ◄─── PERSISTENT     │
│  │ WorldId:      1                         ◄─── CONFIG         │
│  │ ─────────────────────────────────────────────────── │      │
│  │ ZoneId:       2  (Hauptstadt)          ◄─── ZONE B (NEU!)   │
│  │ ShardId:      0                        ◄─── ZONE B (NEU!)   │
│  │ EntityId:     12                       ◄─── ZONE B (NEU!)   │
│  └──────────────────────────────────────────────────────┘      │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

### Diagramm 3: GlobalKey Berechnung

```
┌─────────────────────────────────────────────────────────────────┐
│                   GLOBALKEY BERECHNUNG                          │
│                                                                 │
│  Input:                                                         │
│  ├─ WorldId:  1    (byte)                                      │
│  ├─ ZoneId:   2    (ushort)                                    │
│  ├─ ShardId:  0    (byte)                                      │
│  └─ EntityId: 5    (int)                                       │
│                                                                 │
│  Bit-Shift:                                                     │
│  ┌──────────────────────────────────────────────────────┐      │
│  │                64-bit long                            │      │
│  ├─────────┬──────────────┬─────────┬──────────────────┤      │
│  │ WorldId │    ZoneId    │ ShardId │    EntityId      │      │
│  │ (8 bit) │   (16 bit)   │ (8 bit) │    (32 bit)      │      │
│  ├─────────┼──────────────┼─────────┼──────────────────┤      │
│  │    1    │      2       │    0    │       5          │      │
│  │  << 56  │    << 40     │  << 32  │     (keine)      │      │
│  └─────────┴──────────────┴─────────┴──────────────────┘      │
│                          │                                      │
│                          ▼ (OR alle Teile)                     │
│                                                                 │
│  GlobalKey = 0x0100020000000005                                │
│              └┬┘└──┬──┘└┬┘└────┬────┘                         │
│               1    2    0      5                               │
│                                                                 │
│  Verwendung:                                                    │
│  Dictionary<long, EntityState> entities;                       │
│  entities[globalKey] = playerState;                            │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

### Diagramm 4: Zone-Sharding mit EntityId

```
┌─────────────────────────────────────────────────────────────────┐
│              ZONE-SHARDING MIT ENTITYID                         │
│                                                                 │
│  Zone 2 (Hauptstadt) bei hoher Last:                           │
│                                                                 │
│  ┌─────────────────────────────────────────────────────────┐   │
│  │                  Shard 0 (150 Spieler)                   │   │
│  │  ┌────────────────────────────────────────────────────┐  │   │
│  │  │ EntityId: 1, 2, 3, ..., 150                        │  │   │
│  │  │ WorldId: 1, ZoneId: 2, ShardId: 0                  │  │   │
│  │  │                                                     │  │   │
│  │  │ 👤 Player 1:   World:1 Zone:2 Shard:0 Entity:1    │  │   │
│  │  │ 👤 Player 2:   World:1 Zone:2 Shard:0 Entity:2    │  │   │
│  │  │ 👤 Player 150: World:1 Zone:2 Shard:0 Entity:150  │  │   │
│  │  └────────────────────────────────────────────────────┘  │   │
│  └─────────────────────────────────────────────────────────┘   │
│                                                                 │
│  ┌─────────────────────────────────────────────────────────┐   │
│  │                  Shard 1 (120 Spieler)                   │   │
│  │  ┌────────────────────────────────────────────────────┐  │   │
│  │  │ EntityId: 1, 2, 3, ..., 120                        │  │   │
│  │  │ WorldId: 1, ZoneId: 2, ShardId: 1                  │  │   │
│  │  │                                                     │  │   │
│  │  │ 👤 Player 151: World:1 Zone:2 Shard:1 Entity:1    │  │   │
│  │  │ 👤 Player 152: World:1 Zone:2 Shard:1 Entity:2    │  │   │
│  │  │ 👤 Player 270: World:1 Zone:2 Shard:1 Entity:120  │  │   │
│  │  └────────────────────────────────────────────────────┘  │   │
│  └─────────────────────────────────────────────────────────┘   │
│                                                                 │
│  Wichtig: EntityId ist unabhängig pro Shard!                   │
│  Shard 0 kann EntityId 5 haben UND Shard 1 kann auch          │
│  EntityId 5 haben → Unterscheidung durch ShardId!              │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

---

## Verwandte Dokumentation

- [Architektur-Übersicht](../ARCHITECTURE.md) — Gesamtarchitektur
- [Zone Data Architecture](../ZONE_DATA_ARCHITECTURE.md) — ZoneBounds und CollisionData
- [Server-Komponenten](SERVER_COMPONENTS.md) — Zone Server Details
- [Messages](MESSAGES.md) — Netzwerk-Nachrichten mit IDs
- [Redis-Strategie](REDIS.md) — Session Storage mit IDs
- [Database](DATABASE.md) — Persistierung von CharacterId und AccountId
- [Scaling](SCALING.md) — Zone Sharding Details

---

## 🔗 Nützliche Links

- [MessagePack Specification](https://github.com/msgpack/msgpack/blob/master/spec.md)
- [C# Struct Best Practices](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/struct)
- [Dictionary Performance](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.dictionary-2)
- [Interlocked Operations](https://learn.microsoft.com/en-us/dotnet/api/system.threading.interlocked)

---

*Teil der [Architektur-Dokumentation](../ARCHITECTURE.md)*
