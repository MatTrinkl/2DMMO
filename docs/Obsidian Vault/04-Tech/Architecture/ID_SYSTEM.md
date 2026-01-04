# 🆔 ID-System Dokumentation

## 2DMMO – Entity Identity System

**Version:** 1.0.0  
**Letzte Aktualisierung:** 2025-12-16  
**Teil von:** [Architektur-Dokumentation](Architecture-Overview.md)  
**Siehe auch:** [Issue #137 - ID-System Refactoring](https://github.com/MatTrinkl/2DMMO/issues/137)

---

## 📋 Inhaltsverzeichnis

1. [Übersicht](#übersicht)
2. [ID-Typen](#id-typen)
3. [EntityIdentity Struktur](#entityidentity-struktur)
4. [GlobalEntityKey](#globalentitykey)
5. [PersistentId vs RuntimeId](#persistentid-vs-runtimeid)
6. [IdRegistry](#idregistry)
7. [ZoneId Ranges](#zoneid-ranges)
8. [ID-Vergabe](#id-vergabe)
9. [Verhalten bei Zonenwechsel](#verhalten-bei-zonenwechsel)
10. [Code-Beispiele](#code-beispiele)
11. [Diagramme](#diagramme)

---

## 📋 Übersicht

Das ID-System ist das zentrale Fundament für Entity-Identifikation, Multiplayer-Kommunikation, Persistenz und alle Lookup-Operationen im 2DMMO.

### Design-Prinzipien

1. **Single Source of Truth** - IdRegistry ist die zentrale Stelle für alle Lookups und ID-Vergabe
2. **Klarheit** - Jede ID hat einen eindeutigen Zweck
3. **Stabilität** - PersistentId ändert sich nie, RuntimeId nur bei Zone-Transfer
4. **Skalierbarkeit** - Vorbereitet für Multi-Server und Sharding
5. **Performance** - O(1) Lookups für alle kritischen Pfade

### Wichtige Konzepte

| Konzept | Beschreibung |
|---------|--------------|
| **ServerId** | Identifiziert den physischen Server/Region (Multi-Server Support) |
| **ZoneId** | Identifiziert die Zone innerhalb eines Servers |
| **ShardId** | Identifiziert den Shard innerhalb einer Zone (Horizontal Scaling) |
| **LocalId** | Eindeutige ID innerhalb eines Zone/Shard Paares |
| **PrefabId** | Referenz auf Entity-Template (Skin, NPC-Typ, etc.) |
| **PersistentId** | Stabile GUID die sich nie ändert |

---

## 🏷️ ID-Typen

### Übersicht aller IDs

```
┌────────────────────────────────────────────────────────────────┐
│                        Entity Identity                         │
├─────────────┬──────────┬──────────┬───────────┬───────────────┤
│  ServerId   │  ZoneId  │ ShardId  │  LocalId  │   PrefabId    │
│   (byte)    │ (ushort) │ (ushort) │   (int)   │   (ushort)    │
│   1 Byte    │  2 Bytes │  2 Bytes │  4 Bytes  │    2 Bytes    │
└─────────────┴──────────┴──────────┴───────────┴───────────────┘
                              │
                              ▼
┌────────────────────────────────────────────────────────────────┐
│                    GlobalEntityKey (long)                      │
│    = (ServerId << 56) | (ZoneId << 40) | (ShardId << 24)      │
│      | (LocalId & 0xFFFFFF)                                    │
└────────────────────────────────────────────────────────────────┘
                              │
                              │  zusätzlich
                              ▼
┌────────────────────────────────────────────────────────────────┐
│                      PersistentId (Guid)                       │
│              128-bit UUID - ändert sich NIEMALS                │
└────────────────────────────────────────────────────────────────┘
```

### Detaillierte Beschreibung

| ID | Typ | Größe | Scope | Änderung |
|----|-----|-------|-------|----------|
| **ServerId** | `byte` | 1 Byte | Global | Nur bei Server-Transfer |
| **ZoneId** | `ushort` | 2 Bytes | Server | Bei Zone-Transfer |
| **ShardId** | `ushort` | 2 Bytes | Zone | Bei Shard-Transfer |
| **LocalId** | `int` | 4 Bytes | Zone/Shard | Bei jeder Zone-Änderung |
| **PrefabId** | `ushort` | 2 Bytes | Global | Niemals |
| **PersistentId** | `Guid` | 16 Bytes | Global | Niemals |

---

## 🏗️ EntityIdentity Struktur

Die `EntityIdentity` ist ein Value-Type (struct) der alle Laufzeit-IDs kapselt:

```csharp
[MessagePackObject]
public struct EntityIdentity : IEquatable<EntityIdentity>
{
    [Key(0)] public byte ServerId { get; private set; }
    [Key(1)] public ushort ZoneId { get; private set; }
    [Key(2)] public ushort ShardId { get; private set; }
    [Key(3)] public int LocalId { get; private set; }
    [Key(4)] public ushort PrefabId { get; }  // Immutable!
    
    [IgnoreMember]
    public long GlobalKey => /* Berechnung siehe unten */;
    
    [IgnoreMember]
    public bool IsAssigned => ZoneId > 0 && LocalId > 0;
}
```

### Unveränderliche Felder

- **PrefabId** ist unveränderlich (readonly property)
- Wird im Konstruktor gesetzt und definiert das Entity-Template

### Veränderliche Felder

- **ServerId**, **ZoneId**, **ShardId**, **LocalId** ändern sich bei Transfers
- Werden nur über spezielle Transfer-Methoden geändert

---

## �� GlobalEntityKey

Der GlobalEntityKey ist ein kompakter 64-bit Identifier für Netzwerk-Kommunikation:

```
Bit-Layout:
┌──────────┬──────────────┬──────────────┬────────────────────────┐
│ 56-63    │    40-55     │    24-39     │         0-23           │
│ ServerId │    ZoneId    │    ShardId   │    LocalId (24 bits)   │
│ 8 bits   │   16 bits    │    16 bits   │       24 bits          │
└──────────┴──────────────┴──────────────┴────────────────────────┘
```

### Berechnung

```csharp
public long GlobalKey =>
    ((long)ServerId << 56) | 
    ((long)ZoneId << 40) | 
    ((long)ShardId << 24) | 
    (uint)(LocalId & 0xFFFFFF);
```

### Dekodierung

```csharp
public static (byte ServerId, ushort ZoneId, ushort ShardId, int LocalId) 
    DecodeGlobalKey(long globalKey)
{
    return (
        (byte)(globalKey >> 56),
        (ushort)(globalKey >> 40),
        (ushort)(globalKey >> 24),
        (int)(globalKey & 0xFFFFFF)
    );
}
```

### Limitierungen

- **LocalId** wird auf 24 bits maskiert (max. 16.777.215 Entities pro Zone/Shard)
- Reicht für alle praktischen Anwendungsfälle aus

---

## 🔄 PersistentId vs RuntimeId

### PersistentId (Guid)

- **Niemals ändert sich** - auch nicht bei Zone-Transfer oder Server-Restart
- Verwendet für: Datenbank-Referenzen, Cross-Session Identity, Netzwerk-Messages
- Generiert: Bei Entity-Erstellung (Spieler: aus DB, Mobs: UUID)

### RuntimeId (EntityIdentity)

- **Ändert sich bei Zone-Transfer** - neue LocalId in neuer Zone
- Verwendet für: Zone-interne Lookups, Position-Updates, Collision
- Generiert: Von Zone bei AddEntity()

### Wann welche ID verwenden?

| Use Case | ID-Typ |
|----------|--------|
| Datenbank-Queries | PersistentId |
| Netzwerk-Messages an Client | PersistentId |
| Cross-Zone Events | PersistentId |
| Zone-interne Entity-Suche | RuntimeId.LocalId |
| Server-Status Updates | RuntimeId.GlobalKey |
| Entity-Template Lookup | RuntimeId.PrefabId |

---

## 📚 IdRegistry

Die `IdRegistry` ist die zentrale Singleton-Klasse für alle ID-bezogenen Operationen.

### Verantwortlichkeiten

1. **ID-Vergabe** - Neue LocalIds für Entities in Zonen
2. **Lookups** - Mapping zwischen verschiedenen ID-Typen
3. **Tracking** - Verbindung, Zone und Entity Zuordnungen

### Interface

```csharp
public interface IIdRegistry
{
    // === ID-Vergabe ===
    int GetNextLocalId(ushort zoneId, ushort shardId = 0);
    void ReleaseLocalId(ushort zoneId, ushort shardId, int localId);
    
    // === Entity-Lookups ===
    bool TryGetEntity(Guid persistentId, out IEntity? entity);
    bool TryGetEntity(long globalKey, out IEntity? entity);
    bool TryGetEntity(ushort zoneId, int localId, out IEntity? entity);
    
    // === Connection-Lookups ===
    bool TryGetEntityByConnection(Guid connectionId, out IEntity? entity);
    bool TryGetConnectionByEntity(Guid persistentId, out Guid connectionId);
    
    // === Registration ===
    void RegisterEntity(IEntity entity, ushort zoneId);
    void UnregisterEntity(Guid persistentId);
    void RegisterConnection(Guid connectionId, Guid persistentId);
    void UnregisterConnection(Guid connectionId);
}
```

### Verwendung

```csharp
// Singleton-Zugriff
var registry = IdRegistry.Instance;

// Entity nachschlagen
if (registry.TryGetEntity(message.TargetId, out var target))
{
    // target gefunden
}

// Neue Entity registrieren
registry.RegisterEntity(player, zoneId: 1);

// Connection-Mapping
registry.RegisterConnection(connectionId, player.PersistentId);
```

---

## 🗺️ ZoneId Ranges

### Prototyp (Single-Server)

| Range | Beschreibung |
|-------|--------------|
| 1-999 | Overworld-Zonen |
| 1000-1999 | Dungeons |
| 2000-2999 | Arenen/PvP |
| 3000-3999 | Instanzen |
| 60000-65534 | Test-Zonen |

### Zukunft (Multi-Server)

Mit ServerId können mehrere Server dieselben ZoneIds verwenden.

---

## 🔄 ID-Vergabe

### LocalId-Vergabe

1. Zone führt Counter `_nextEntityId`
2. Bei AddEntity: `newId = _nextEntityId++`
3. Bei RemoveEntity: LocalId wird in `_freedIds` Queue gespeichert
4. Freed IDs werden wiederverwendet (Memory-Effizienz)

```csharp
public void AddEntity(IEntity entity)
{
    int newEntityId = _freedIds.Count > 0 
        ? _freedIds.Dequeue() 
        : _nextEntityId++;
    
    entity.SetEntityId(newEntityId, ZoneId);
    Entities.Add(newEntityId, entity);
}
```

### PersistentId-Vergabe

- **Spieler**: CharacterId aus Datenbank (vor Login bekannt)
- **NPCs**: Vordefiniert in Zone-Konfiguration
- **Mobs**: `Guid.NewGuid()` beim Spawn
- **Projektile/Drops**: `Guid.NewGuid()` bei Erstellung

---

## 🚀 Verhalten bei Zonenwechsel

### Transfer-Ablauf

```
┌─────────────────────────────────────────────────────────────────┐
│                      Zone Transfer Flow                         │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  1. RemoveEntity(oldZone, entity.LocalId)                      │
│     └─ Entity bekommt RuntimeId.Unassigned()                   │
│                                                                 │
│  2. AddEntity(newZone, entity)                                 │
│     └─ Zone vergibt neue LocalId                               │
│     └─ Entity.SetEntityId(newLocalId, newZoneId)               │
│                                                                 │
│  3. entity.ChangeZone(newZoneId)                               │
│     └─ Entity kann auf Zonenwechsel reagieren                  │
│                                                                 │
│  PersistentId bleibt UNVERÄNDERT!                              │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

### Code-Beispiel

```csharp
public void TransferEntity(IEntity entity, ushort fromZoneId, ushort toZoneId)
{
    Zone oldZone = _zones[fromZoneId];
    Zone newZone = _zones[toZoneId];
    
    // Aus alter Zone entfernen (gibt LocalId frei)
    oldZone.RemoveEntity(entity.RuntimeId.LocalId);
    
    // In neue Zone einfügen (vergibt neue LocalId)
    newZone.AddEntity(entity);
    
    // Entity benachrichtigen
    entity.ChangeZone(toZoneId);
    
    // PersistentId-Lookups müssen NICHT aktualisiert werden!
}
```

---

## 💻 Code-Beispiele

### Entity erstellen und registrieren

```csharp
// Spieler aus DB laden
var player = new PlayerEntity(
    characterId: dbCharacter.Id,    // PersistentId aus DB
    displayName: dbCharacter.Name,
    position: new Position(100, 200)
);

// Bei Zone anmelden
zoneManager.AddPlayer(serverPlayer, zoneId: 1);
// → Zone vergibt LocalId
// → IdRegistry trackt Connection + Entity
```

### Entity-Lookup

```csharp
// Nach PersistentId (empfohlen für Netzwerk-Messages)
if (IdRegistry.Instance.TryGetEntity(message.TargetPersistentId, out var target))
{
    // target gefunden
}

// Nach GlobalKey (für kompakte Identifikation)
if (IdRegistry.Instance.TryGetEntity(globalKey, out var entity))
{
    // entity gefunden
}

// Nach Zone + LocalId (für zone-interne Operationen)
if (IdRegistry.Instance.TryGetEntity(zoneId, localId, out var entity))
{
    // entity gefunden
}
```

### PrefabId verwenden

```csharp
// PrefabId definiert das Entity-Template
var goblin = new MobEntity(
    position: spawnPoint,
    prefabId: PrefabIds.MobGoblin  // ushort 1000
);

// Client lädt passendes Sprite basierend auf PrefabId
switch (entity.RuntimeId.PrefabId)
{
    case PrefabIds.MobGoblin:
        LoadSprite("res://sprites/goblin.png");
        break;
    // ...
}
```

---

## 📊 Diagramme

### Entity-ID Lebenszyklus

```
                  ┌─────────────────┐
                  │  Entity Spawn   │
                  └────────┬────────┘
                           │
                           ▼
              ┌────────────────────────┐
              │  PersistentId erzeugt  │
              │  (Guid.NewGuid() oder  │
              │   aus Datenbank)       │
              └────────────┬───────────┘
                           │
                           ▼
              ┌────────────────────────┐
              │  RuntimeId.Unassigned  │
              │  (PrefabId gesetzt)    │
              └────────────┬───────────┘
                           │
                           ▼
              ┌────────────────────────┐
              │    Zone.AddEntity()    │
              │  LocalId vergeben      │
              │  RuntimeId vollständig │
              └────────────┬───────────┘
                           │
           ┌───────────────┼───────────────┐
           │               │               │
           ▼               ▼               ▼
    ┌─────────────┐ ┌─────────────┐ ┌─────────────┐
    │ Zone-intern │ │Zone-Transfer│ │   Despawn   │
    │   Updates   │ │ neue LocalId│ │  Cleanup    │
    └─────────────┘ └─────────────┘ └─────────────┘
```

### Lookup-Strategien

```
┌─────────────────────────────────────────────────────────────────┐
│                       IdRegistry Lookups                        │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  PersistentId (Guid)                                           │
│      │                                                          │
│      ├──► _entitiesByPersistentId[guid] ──► IEntity            │
│      │                                                          │
│      └──► _connectionsByPersistentId[guid] ──► ConnectionId    │
│                                                                 │
│  ConnectionId (Guid)                                           │
│      │                                                          │
│      └──► _entitiesByConnection[connId] ──► IEntity            │
│                                                                 │
│  GlobalKey (long)                                              │
│      │                                                          │
│      ├──► Decode: (ServerId, ZoneId, ShardId, LocalId)         │
│      │                                                          │
│      └──► _zones[zoneId].Entities[localId] ──► IEntity         │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

---

## 🔗 Verwandte Dokumentation

- [Message-Spezifikation](MESSAGES.md) - Netzwerk-Nachrichten und Serialisierung
- [Netzwerk-Protokoll](NETWORK_PROTOCOL.md) - Transport und Message Framing
- [Server-Komponenten](SERVER_COMPONENTS.md) - ZoneManager und GameServer
- [Skalierung](SCALING.md) - Multi-Server und Sharding

---

## 📝 Changelog

### v1.0.0 (2025-12-16)

- Initiale Dokumentation des ID-Systems
- Beschreibung von EntityIdentity, GlobalKey, PersistentId
- IdRegistry als zentrale Lookup-Komponente
- ZoneId Ranges für Prototyp definiert

---

*Teil der [Architektur-Dokumentation](Architecture-Overview.md)*

Source: docs/02-architecture/ID_SYSTEM.md
