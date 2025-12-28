# 🗺️ Chunk-Based Area of Interest (AOI) Delta Sync System

**Version:** 1.0.0  
**Letzte Aktualisierung:** 2025-12-28  
**Teil von:** [Architektur-Dokumentation](README.md)

---

## 📋 Übersicht

Dieses Dokument beschreibt das chunk-basierte AOI-System (Area of Interest) für effiziente Zone-State-Synchronisation zwischen Server und Clients. Das System reduziert Bandbreite drastisch durch selektives Senden von Änderungen nur für sichtbare Bereiche.

---

## ❌ Das Problem: Full Sync skaliert nicht

### Ursprüngliche Strategie

Im initialen Design sendete der Server jeden Tick einen kompletten `ZoneState` an alle Clients:

```
ALTE STRATEGIE (Full Broadcast):

Tick 0:  ZoneState mit ALLEN 500 Entities → Broadcast an 100 Spieler
Tick 1:  ZoneState mit ALLEN 500 Entities → Broadcast an 100 Spieler
Tick 2:  ZoneState mit ALLEN 500 Entities → Broadcast an 100 Spieler
...

Bandbreite pro Tick:
  - Entities pro ZoneState: 500
  - Bytes pro Entity: ~80 bytes
  - ZoneState-Größe: 500 * 80 = 40 KB
  - Spieler in Zone: 100
  - Bandbreite pro Tick: 40 KB * 100 = 4 MB
  - Bei 25 Hz: 4 MB * 25 = 100 MB/s
```

### Probleme

| Problem | Auswirkung |
|---------|-----------|
| **Bandbreite-Explosion** | 100 MB/s für eine einzige Zone nicht skalierbar |
| **Irrelevante Daten** | Spieler erhält Updates von Entities auf der anderen Seite der Map |
| **Client-Overhead** | Unnötiges Processing von 500 Entities obwohl nur 20 sichtbar |
| **Server-Last** | Serialisierung von 500 Entities * 100 Spieler = 50.000 Entities/Tick |

### Periodischer Full Sync

Eine Verbesserung war periodischer Full Sync (alle 25 Ticks = 1 Sekunde):

```
VERBESSERTE STRATEGIE (Periodic Full Sync):

Tick 0-24:  Delta Updates (nur geänderte Entities)
Tick 25:    ZoneState mit ALLEN Entities
Tick 26-49: Delta Updates
Tick 50:    ZoneState mit ALLEN Entities
...

Bandbreite-Reduktion: ~96% (nur alle 1 Sekunde Full Sync)
ABER: Immer noch ALLE Entities, nicht nur sichtbare!
```

**Problem bleibt:** Auch bei periodischem Sync werden irrelevante Entities gesendet.

---

## ✅ Die Lösung: Chunk-Based AOI Delta Sync

### Konzept

```
┌─────────────────────────────────────────────────────────────────┐
│                    CHUNK-BASED AOI SYSTEM                        │
│                                                                  │
│  1. Zone wird in Chunks unterteilt (Grid-basiert)               │
│  2. Jeder Client "abonniert" nur Chunks in Sichtweite           │
│  3. Server sendet nur Änderungen in abonnierten Chunks          │
│  4. Bandbreite skaliert mit Sichtbereich, nicht Zone-Größe      │
└─────────────────────────────────────────────────────────────────┘
```

---

## 🧱 Grid-Based Chunk System

### Chunk-Größe: 32x32 Tiles

```
World-Koordinaten → Chunk-Koordinaten:

ChunkX = Floor(WorldX / 32)
ChunkY = Floor(WorldY / 32)

Beispiel:
  Position (100, 250) → Chunk (3, 7)
  Position (0, 0) → Chunk (0, 0)
  Position (31, 31) → Chunk (0, 0)
  Position (32, 32) → Chunk (1, 1)
```

### Zone-Chunk-Grid

```
Zone "Elwynn Forest" (512x512 Tiles):

┌─────────────────────────────────────────────────────────────┐
│ (0,0)  (1,0)  (2,0)  (3,0)  ...  (15,0)                    │
│                                                             │
│ (0,1)  (1,1)  (2,1)  (3,1)  ...  (15,1)                    │
│                                                             │
│ (0,2)  (1,2)  (2,2)  (3,2)  ...  (15,2)                    │
│                                                             │
│  ...    ...    ...    ...          ...                     │
│                                                             │
│ (0,15) (1,15) (2,15) (3,15) ...  (15,15)                   │
└─────────────────────────────────────────────────────────────┘

Total Chunks: 16 x 16 = 256 Chunks
```

### ChunkCoord Struct

```csharp
/// <summary>
/// Identifiziert einen Chunk innerhalb einer Zone.
/// </summary>
[MessagePackObject]
public struct ChunkCoord : IEquatable<ChunkCoord>
{
    [Key(0)] public short X { get; set; }
    [Key(1)] public short Y { get; set; }

    public ChunkCoord(short x, short y)
    {
        X = x;
        Y = y;
    }

    /// <summary>
    /// Berechnet ChunkCoord aus World-Position.
    /// </summary>
    public static ChunkCoord FromWorldPosition(float worldX, float worldY)
    {
        const int CHUNK_SIZE = 32;
        return new ChunkCoord(
            (short)Math.Floor(worldX / CHUNK_SIZE),
            (short)Math.Floor(worldY / CHUNK_SIZE)
        );
    }

    public bool Equals(ChunkCoord other) =>
        X == other.X && Y == other.Y;

    public override int GetHashCode() =>
        HashCode.Combine(X, Y);
}
```

---

## 👁️ Client Subscription Model

### Sichtbereich: 3x3 Chunks

Jeder Client "sieht" 9 Chunks (3x3 Grid um den eigenen Chunk):

```
Spieler bei World-Position (100, 100):
  → Chunk (3, 3)

Sichtbare Chunks (3x3 um Chunk (3,3)):

    ┌─────┬─────┬─────┐
    │(2,2)│(3,2)│(4,2)│
    ├─────┼─────┼─────┤
    │(2,3)│(3,3)│(4,3)│  ← Spieler ist in (3,3)
    ├─────┼─────┼─────┤
    │(2,4)│(3,4)│(4,4)│
    └─────┴─────┴─────┘

Subscription: {(2,2), (3,2), (4,2), (2,3), (3,3), (4,3), (2,4), (3,4), (4,4)}
```

### Subscription-Updates bei Bewegung

```
FALL 1: Bewegung innerhalb des gleichen Chunks
  → Keine Subscription-Änderung
  
FALL 2: Bewegung in benachbarten Chunk
  → Alte Chunks abbestellen (3 Chunks)
  → Neue Chunks abonnieren (3 Chunks)
  → 6 Chunks-Änderungen

Beispiel: Spieler bewegt sich von Chunk (3,3) → (4,3)

  VORHER:                    NACHHER:
  ┌─────┬─────┬─────┐       ┌─────┬─────┬─────┐
  │(2,2)│(3,2)│(4,2)│       │(3,2)│(4,2)│(5,2)│
  ├─────┼─────┼─────┤       ├─────┼─────┼─────┤
  │(2,3)│(3,3)│(4,3)│  →    │(3,3)│(4,3)│(5,3)│ ← Spieler
  ├─────┼─────┼─────┤       ├─────┼─────┼─────┤
  │(2,4)│(3,4)│(4,4)│       │(3,4)│(4,4)│(5,4)│
  └─────┴─────┴─────┘       └─────┴─────┴─────┘

  Unsubscribe: (2,2), (2,3), (2,4)
  Subscribe:   (5,2), (5,3), (5,4)
```

---

## 🔍 ClientViewService

Der `ClientViewService` verwaltet welche Clients welche Chunks sehen.

### Konzept

```csharp
/// <summary>
/// Verwaltet Client-Subscriptions zu Chunks.
/// Ermöglicht effiziente Broadcast-Filterung.
/// </summary>
public class ClientViewService
{
    // Chunk → Liste von Clients die diesen Chunk sehen
    private readonly Dictionary<ChunkCoord, HashSet<Guid>> _chunkToClients;
    
    // Client → Liste von Chunks die der Client sieht
    private readonly Dictionary<Guid, HashSet<ChunkCoord>> _clientToChunks;

    /// <summary>
    /// Client abonniert neue Chunks basierend auf Position.
    /// </summary>
    public void UpdateClientView(Guid clientId, float worldX, float worldY)
    {
        var playerChunk = ChunkCoord.FromWorldPosition(worldX, worldY);
        var visibleChunks = GetVisibleChunks(playerChunk); // 3x3 Grid

        var oldChunks = _clientToChunks.GetValueOrDefault(clientId) ?? new();
        var newChunks = visibleChunks.ToHashSet();

        // Unsubscribe von alten Chunks
        foreach (var chunk in oldChunks.Except(newChunks))
        {
            _chunkToClients[chunk].Remove(clientId);
        }

        // Subscribe zu neuen Chunks
        foreach (var chunk in newChunks.Except(oldChunks))
        {
            if (!_chunkToClients.ContainsKey(chunk))
                _chunkToClients[chunk] = new HashSet<Guid>();
            
            _chunkToClients[chunk].Add(clientId);
        }

        _clientToChunks[clientId] = newChunks;
    }

    /// <summary>
    /// Gibt alle Clients zurück die einen bestimmten Chunk sehen.
    /// </summary>
    public IEnumerable<Guid> GetClientsViewingChunk(ChunkCoord chunk)
    {
        return _chunkToClients.GetValueOrDefault(chunk) ?? Enumerable.Empty<Guid>();
    }

    /// <summary>
    /// Gibt alle Chunks zurück die ein Client sieht.
    /// </summary>
    public IEnumerable<ChunkCoord> GetClientVisibleChunks(Guid clientId)
    {
        return _clientToChunks.GetValueOrDefault(clientId) ?? Enumerable.Empty<ChunkCoord>();
    }

    /// <summary>
    /// Berechnet 3x3 Chunk-Grid um einen Chunk.
    /// </summary>
    private IEnumerable<ChunkCoord> GetVisibleChunks(ChunkCoord center)
    {
        for (short dx = -1; dx <= 1; dx++)
        {
            for (short dy = -1; dy <= 1; dy++)
            {
                yield return new ChunkCoord(
                    (short)(center.X + dx),
                    (short)(center.Y + dy)
                );
            }
        }
    }
}
```

---

## 📊 ChunkDirtyTracker

Der `ChunkDirtyTracker` sammelt Änderungen pro Chunk für effiziente Delta-Erstellung.

### Konzept

```csharp
/// <summary>
/// Sammelt Entity-Änderungen pro Chunk.
/// Ermöglicht chunk-spezifische Delta-Broadcasts.
/// </summary>
public class ChunkDirtyTracker
{
    // Chunk → Liste von Entities mit Änderungen
    private readonly Dictionary<ChunkCoord, HashSet<Guid>> _dirtyEntitiesPerChunk;

    /// <summary>
    /// Markiert Entity als geändert in einem Chunk.
    /// </summary>
    public void MarkEntityDirty(Guid entityId, float worldX, float worldY)
    {
        var chunk = ChunkCoord.FromWorldPosition(worldX, worldY);
        
        if (!_dirtyEntitiesPerChunk.ContainsKey(chunk))
            _dirtyEntitiesPerChunk[chunk] = new HashSet<Guid>();
        
        _dirtyEntitiesPerChunk[chunk].Add(entityId);
    }

    /// <summary>
    /// Gibt alle geänderten Entities in einem Chunk zurück.
    /// </summary>
    public IEnumerable<Guid> GetDirtyEntitiesInChunk(ChunkCoord chunk)
    {
        return _dirtyEntitiesPerChunk.GetValueOrDefault(chunk) ?? Enumerable.Empty<Guid>();
    }

    /// <summary>
    /// Alle Chunks mit Änderungen.
    /// </summary>
    public IEnumerable<ChunkCoord> GetDirtyChunks()
    {
        return _dirtyEntitiesPerChunk.Keys;
    }

    /// <summary>
    /// Dirty Flags zurücksetzen (nach Tick).
    /// </summary>
    public void ClearDirtyFlags()
    {
        _dirtyEntitiesPerChunk.Clear();
    }
}
```

---

## 🎯 Wie Deltas pro Client gebaut werden

### Ablauf im Game Loop

```
┌────────────────────────────────────────────────────────────────┐
│              OUTPUT PHASE - DELTA BROADCAST                     │
│                                                                 │
│  1️⃣ ChunkDirtyTracker sammelt geänderte Entities              │
│     → Chunk (3,3): {Entity_123, Entity_456}                    │
│     → Chunk (4,3): {Entity_789}                                │
│                                                                 │
│  2️⃣ Für jeden Dirty Chunk:                                    │
│     → ClientViewService: Welche Clients sehen Chunk (3,3)?     │
│       → {Client_A, Client_B, Client_C}                         │
│                                                                 │
│  3️⃣ Für jeden Client:                                         │
│     → Sammle alle Entities in ALLEN sichtbaren Chunks          │
│     → Baue ZoneDelta mit diesen Entities                       │
│     → Sende ZoneDelta an Client                                │
│                                                                 │
│  4️⃣ ChunkDirtyTracker.ClearDirtyFlags()                       │
└────────────────────────────────────────────────────────────────┘
```

### Pseudocode

```csharp
// Output Phase im Game Loop
public void BroadcastChunkDeltas()
{
    // Alle Chunks mit Änderungen
    var dirtyChunks = _chunkDirtyTracker.GetDirtyChunks();

    if (!dirtyChunks.Any())
        return; // Keine Änderungen

    // Pro-Client Delta sammeln
    var clientDeltas = new Dictionary<Guid, ZoneDelta>();

    foreach (var chunk in dirtyChunks)
    {
        // Welche Clients sehen diesen Chunk?
        var viewingClients = _clientViewService.GetClientsViewingChunk(chunk);
        
        // Entities in diesem Chunk die sich geändert haben
        var dirtyEntityIds = _chunkDirtyTracker.GetDirtyEntitiesInChunk(chunk);

        foreach (var clientId in viewingClients)
        {
            // Initialisiere Delta für Client falls noch nicht vorhanden
            if (!clientDeltas.ContainsKey(clientId))
                clientDeltas[clientId] = new ZoneDelta();

            // Füge Entities zu Client-Delta hinzu
            foreach (var entityId in dirtyEntityIds)
            {
                var entity = _entityManager.GetEntity(entityId);
                
                if (entity.IsNewlySpawned)
                    clientDeltas[clientId].SpawnedEntities.Add(entity.ToDto());
                else if (entity.PositionChanged)
                    clientDeltas[clientId].PositionUpdates.Add(entity.ToPositionDelta());
                else if (entity.StateChanged)
                    clientDeltas[clientId].StateUpdates.Add(entity.ToStateDelta());
            }
        }
    }

    // Sende Deltas an Clients
    foreach (var (clientId, delta) in clientDeltas)
    {
        delta.Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        SendToClient(clientId, delta);
    }

    // Dirty Flags zurücksetzen
    _chunkDirtyTracker.ClearDirtyFlags();
}
```

---

## 📈 Diagramme

### Zone mit Chunks und Spielern

```
Zone "Elwynn Forest" (16x16 Chunks):

┌─────┬─────┬─────┬─────┬─────┬─────┬─────┬─────┐
│     │     │     │     │     │     │     │     │
├─────┼─────┼─────┼─────┼─────┼─────┼─────┼─────┤
│     │     │     │  E  │  E  │     │     │     │  E = Entity
├─────┼─────┼─────┼─────┼─────┼─────┼─────┼─────┤
│     │     │ [A] │  E  │     │     │     │     │  [A] = Player A
├─────┼─────┼─────┼─────┼─────┼─────┼─────┼─────┤
│     │  E  │  E  │     │     │     │     │     │
├─────┼─────┼─────┼─────┼─────┼─────┼─────┼─────┤
│     │     │     │     │     │ [B] │  E  │     │  [B] = Player B
├─────┼─────┼─────┼─────┼─────┼─────┼─────┼─────┤
│     │     │     │     │  E  │  E  │     │     │
├─────┼─────┼─────┼─────┼─────┼─────┼─────┼─────┤
│     │     │     │     │     │     │     │     │
├─────┼─────┼─────┼─────┼─────┼─────┼─────┼─────┤
│     │     │     │     │     │     │     │     │
└─────┴─────┴─────┴─────┴─────┴─────┴─────┴─────┘

Player A sieht: 3x3 Chunks um Position → 9 Chunks, 4 Entities
Player B sieht: 3x3 Chunks um Position → 9 Chunks, 4 Entities

OHNE Chunk-System: Beide erhalten Updates für ALLE ~12 Entities
MIT Chunk-System:  Beide erhalten Updates nur für ihre 4 Entities
```

### Players mit unterschiedlichen Sichtbereichen

```
Spieler A bei Chunk (2,2):
┌───────────────┐
│ ╔═══╦═══╦═══╗ │
│ ║(1,1)║(2,1)║(3,1)║ │
│ ╠═══╬═══╬═══╣ │
│ ║(1,2)║[A]║(3,2)║ │
│ ╠═══╬═══╬═══╣ │
│ ║(1,3)║(2,3)║(3,3)║ │
│ ╚═══╩═══╩═══╝ │
└───────────────┘

Spieler B bei Chunk (5,4):
┌───────────────┐
│ ╔═══╦═══╦═══╗ │
│ ║(4,3)║(5,3)║(6,3)║ │
│ ╠═══╬═══╬═══╣ │
│ ║(4,4)║[B]║(6,4)║ │
│ ╠═══╬═══╬═══╣ │
│ ║(4,5)║(5,5)║(6,5)║ │
│ ╚═══╩═══╩═══╝ │
└───────────────┘

Keine Überschneidung → Beide erhalten komplett unterschiedliche Deltas!
```

### Delta-Erstellung Beispiel

```
Tick N:
  → Entity_123 in Chunk (3,3) bewegt sich → Position-Update
  → Entity_456 in Chunk (3,3) HP-Änderung → State-Update
  → Entity_789 in Chunk (7,7) bewegt sich → Position-Update

ChunkDirtyTracker:
  Chunk (3,3): {Entity_123, Entity_456}
  Chunk (7,7): {Entity_789}

ClientViewService:
  Chunk (3,3) sichtbar für: {Client_A, Client_C}
  Chunk (7,7) sichtbar für: {Client_B}

Delta-Erstellung:
  Client_A erhält ZoneDelta:
    ├── PositionUpdates: [Entity_123]
    └── StateUpdates: [Entity_456]
  
  Client_B erhält ZoneDelta:
    └── PositionUpdates: [Entity_789]
  
  Client_C erhält ZoneDelta:
    ├── PositionUpdates: [Entity_123]
    └── StateUpdates: [Entity_456]

Client_D (sieht andere Chunks): Keine Updates!
```

---

## 📊 Bandbreiten-Vergleich

### Szenario

| Parameter | Wert |
|-----------|------|
| Zone-Größe | 512x512 Tiles (16x16 Chunks) |
| Entities in Zone | 500 |
| Spieler in Zone | 100 |
| Entities pro Chunk (Durchschnitt) | 500 / 256 = ~2 |
| Sichtbare Chunks pro Spieler | 9 (3x3) |
| Sichtbare Entities pro Spieler | 2 * 9 = ~18 |
| Movement-Updates pro Tick | 50% der Entities bewegen sich |

### Berechnung: OHNE Chunk-System

```
ALTE STRATEGIE (Periodic Full Sync):

Ticks 0-24 (Delta):
  - Geänderte Entities: 500 * 50% = 250
  - Bytes pro Update: ~40 bytes
  - Update-Größe: 250 * 40 = 10 KB
  - Spieler: 100
  - Bandbreite: 10 KB * 100 = 1 MB/Tick
  - Bei 25 Hz: 1 MB * 25 = 25 MB/s

Tick 25 (Full Sync):
  - Entities: 500
  - Bytes pro Entity: ~80 bytes
  - ZoneState-Größe: 500 * 80 = 40 KB
  - Spieler: 100
  - Bandbreite: 40 KB * 100 = 4 MB

Durchschnitt über 1 Sekunde:
  (24 * 1 MB + 1 * 4 MB) / 25 = 28 MB / 25 = 1.12 MB/Tick
  Bei 25 Hz: 1.12 MB * 25 = 28 MB/s
```

### Berechnung: MIT Chunk-System

```
NEUE STRATEGIE (Chunk-Based Delta Sync):

Delta-Updates (jeden Tick):
  - Geänderte Entities gesamt: 250
  - Sichtbare Entities pro Spieler: ~18
  - Geänderte sichtbare Entities: 18 * 50% = ~9
  - Bytes pro Update: ~40 bytes
  - Update-Größe pro Spieler: 9 * 40 = 360 bytes
  - Spieler: 100
  - Bandbreite: 360 bytes * 100 = 36 KB/Tick
  - Bei 25 Hz: 36 KB * 25 = 900 KB/s

Full Sync (alle 25 Ticks):
  - Sichtbare Entities: 18
  - Bytes pro Entity: ~80 bytes
  - ZoneState-Größe: 18 * 80 = 1.44 KB
  - Spieler: 100
  - Bandbreite: 1.44 KB * 100 = 144 KB

Durchschnitt über 1 Sekunde:
  (24 * 36 KB + 1 * 144 KB) / 25 = 1008 KB / 25 = 40.32 KB/Tick
  Bei 25 Hz: 40.32 KB * 25 = 1008 KB/s ≈ 1 MB/s
```

### Bandbreiten-Vergleich Tabelle

| Strategie | Bandbreite/Tick | Bandbreite/Sekunde | Reduktion |
|-----------|-----------------|-------------------|-----------|
| **Full Broadcast (alt)** | 4 MB | 100 MB/s | 0% (Baseline) |
| **Periodic Full Sync** | 1.12 MB | 28 MB/s | **72% Reduktion** |
| **Chunk-Based Delta Sync** | 40 KB | 1 MB/s | **96% Reduktion** |

### Skalierung bei wachsender Zone-Größe

| Zone-Größe | Entities | OHNE Chunks | MIT Chunks | Reduktion |
|------------|----------|-------------|------------|-----------|
| 512x512 (16x16 Chunks) | 500 | 28 MB/s | 1 MB/s | **96%** |
| 1024x1024 (32x32 Chunks) | 2000 | 112 MB/s | 1 MB/s | **99%** |
| 2048x2048 (64x64 Chunks) | 8000 | 448 MB/s | 1 MB/s | **99.7%** |

**Wichtig:** Bandbreite mit Chunks bleibt konstant, da Spieler immer nur ~9 Chunks sehen!

---

## 🔄 Integration mit bestehendem System

### ZoneState vs. ZoneDelta

| Message | Frequenz | Inhalt | Scope |
|---------|----------|--------|-------|
| **ZoneState** (102) | Alle 25 Ticks (1s) | Alle Entities | **NUR sichtbare Chunks** |
| **ZoneDelta** (103) | Jeden Tick (40ms) | Nur Änderungen | **NUR sichtbare Chunks** |

**Änderung:** Beide Messages werden jetzt chunk-gefiltert!

### Deprecated Messages

Die folgenden Entity-Messages werden durch `ZoneDelta` ersetzt:

| Old Message | ID | Ersetzt durch |
|-------------|-----|---------------|
| `EntityMove` | 1402 | `ZoneDelta.PositionUpdates` |
| `EntityUpdate` | 1403 | `ZoneDelta.StateUpdates` |
| `EntityStateChange` | 1405 | `ZoneDelta.StateUpdates` |

**Grund:** Batching! Statt einzelner Messages pro Entity → Ein ZoneDelta mit allen Änderungen.

### Noch aktive Messages (Event-Based)

Diese Messages bleiben als sofortige Broadcasts:

| Message | ID | Grund |
|---------|-----|-------|
| `EntityAnimation` | 1404 | Combat-kritisch, braucht instant Feedback |
| `EntityAggro` | 1421 | Combat-kritisch |
| `EntityEmote` | 1430 | Social Feature, erwartete ~0 Latency |
| `EntityInteract` | 1410 | Request/Response-Pattern |
| `EntityTarget` | 1420 | Request/Response-Pattern |

**Warum nicht batched?** Diese Events erfordern sofortige Reaktion und sind selten genug dass Batching nicht hilft.

---

## 🎯 Vorteile

### ✅ Performance

- **96%+ Bandbreiten-Reduktion** in typischen Szenarien
- **Skaliert mit Sichtbereich**, nicht mit Zone-Größe
- **Konstante Bandbreite** pro Spieler (unabhängig von Zone-Größe)
- **Reduzierte Server-Last** (weniger Serialisierung)

### ✅ Client-Optimierung

- **Weniger Processing** (nur sichtbare Entities)
- **Weniger Deserialisierung**
- **Bessere Frame-Zeiten**

### ✅ Skalierbarkeit

- **Größere Zonen** möglich ohne Bandbreiten-Explosion
- **Mehr Spieler** pro Zone möglich
- **Mehr Entities** pro Zone möglich

### ✅ Netzwerk-Effizienz

- **Batching** von Änderungen pro Tick
- **Delta-Compression** (nur Änderungen)
- **Relevanz-Filter** (nur sichtbare Bereiche)

---

## 🚀 Performance-Überlegungen

### Chunk-Größe Tuning

| Chunk-Größe | Pros | Cons |
|-------------|------|------|
| **16x16** | Feinere Granularität | Mehr Subscriptions, mehr Chunk-Wechsel |
| **32x32** | ✅ **Optimales Balance** | - |
| **64x64** | Weniger Subscriptions | Gröbere Granularität, mehr irrelevante Entities |

**Gewählt: 32x32** - Beste Balance zwischen Granularität und Overhead.

### Memory-Overhead

```csharp
// Pro Zone:
//   ChunkDirtyTracker: ~256 * 64 bytes = 16 KB
//   ClientViewService: ~100 Spieler * 9 Chunks * 24 bytes = ~22 KB
// Total: ~38 KB zusätzlicher Memory pro Zone
// Bei 100 Zones: ~3.8 MB (vernachlässigbar)
```

### CPU-Overhead

```csharp
// Pro Tick:
//   Chunk-Berechnung: O(Entities-Moved) = ~250 Operationen
//   Client-Lookup: O(Dirty-Chunks * Clients-Per-Chunk) = ~10 * 10 = 100 Lookups
//   Delta-Building: O(Clients * Entities-Per-Client) = 100 * 9 = 900 Operationen
// Total: ~1250 Operationen (sehr performant!)
```

---

## 📋 Implementation Checklist

Für die Code-Implementierung (nicht Teil dieser PR):

- [ ] `ChunkCoord` struct in `Mmo.Shared/Zones/Structs/`
- [ ] `ClientViewService` in `Mmo.Server.ZoneServer/Services/`
- [ ] `ChunkDirtyTracker` in `Mmo.Server.ZoneServer/Services/`
- [ ] `ZoneDelta` Message in `Mmo.Shared/Zones/Messages/`
- [ ] `EntityPositionDelta` DTO in `Mmo.Shared/Entities/Dtos/`
- [ ] `EntityStateDelta` DTO in `Mmo.Shared/Entities/Dtos/`
- [ ] Integration in Game Loop Output Phase
- [ ] Unit Tests für Chunk-System
- [ ] Performance-Tests (Bandbreiten-Verifikation)

---

## 🔗 Verwandte Dokumentation

- [Game Loop Design](GAME_LOOP.md) - Output/Broadcast Phase
- [Client-Server Sync](CLIENT_SERVER_SYNC.md) - Interpolation mit Deltas
- [Zone Messages](../03-messages/01-zone.md) - ZoneState & ZoneDelta
- [Entity Messages](../03-messages/14-entity.md) - Deprecated Messages

---

**Letzte Aktualisierung:** 2025-12-28  
**Version:** 1.0.0

[← Zurück zur Architektur-Übersicht](README.md)
