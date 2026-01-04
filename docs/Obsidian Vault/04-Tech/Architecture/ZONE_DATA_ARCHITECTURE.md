# 🗺️ Zone Data Architecture

## Architektur-Entscheidungen für Zone-bezogene Datenstrukturen

**Version:** 1.0.0  
**Letzte Aktualisierung:** 2025-12-08  
**Status:** Design-Dokumentation

---

## 📋 Inhaltsverzeichnis

1. [ZoneBounds vs CollisionData](#1-zonebounds-vs-collisiondata)
2. [ZoneBounds Design-Entscheidungen](#2-zonebounds-design-entscheidungen)
3. [Speicherort: Shared Library](#3-speicherort-shared-library)
4. [Datenverteilung: Client vs. Server](#4-datenverteilung-client-vs-server)
5. [CollisionData Design](#5-collisiondata-design)
6. [Zusammenfassung der Entscheidungen](#6-zusammenfassung-der-entscheidungen)
7. [Ressourcen](#ressourcen)

---

## 1. ZoneBounds vs CollisionData

### Zwei separate Konzepte

**ZoneBounds** und **CollisionData** sind zwei unterschiedliche Konzepte, die verschiedene Zwecke erfüllen:

| Aspekt | ZoneBounds | CollisionData |
|--------|-----------|---------------|
| **Zweck** | Äußere Grenzen der Zone definieren | Innere Hindernisse (Wände, Objekte) |
| **Datenstruktur** | 4 Floats (MinX, MaxX, MinY, MaxY) | bool[,] Tile-Grid + Metadata |
| **Performance** | O(1) - einfache Vergleiche | O(n) - Bresenham Line Tracing |
| **Verwendung** | Bounds-Check, Zone-Wechsel | Collision Detection beim Movement |
| **Speicher** | ~16 Bytes | Width × Height × 1 Byte |
| **Änderungshäufigkeit** | Statisch (Zone-Definition) | Statisch (Zone-Definition) |

### Visuelle Darstellung

```
┌────────────────────────────────────────────────────┐  ← MaxY (ZoneBounds)
│                    ZONE BOUNDS                     │
│  ┌──┐                                              │
│  │██│  ← Obstacle (CollisionData: false)           │
│  └──┘                                              │
│                                                     │
│         ┌──────────┐                               │
│         │          │  ← Building                    │
│         │  ████    │     (CollisionData: false)     │
│         │  ████    │                               │
│         └──────────┘                               │
│                                                     │
│  🧑 ← Player                                        │
│      (in walkable area)                            │
│                                                     │
└────────────────────────────────────────────────────┘  ← MinY (ZoneBounds)
↑                                                    ↑
MinX                                                MaxX
```

### Code-Beispiel: MovementValidator

```csharp
public class MovementValidator
{
    private readonly ZoneBounds _zoneBounds;
    private readonly CollisionData _collisionData;
    
    public bool ValidateMovement(float fromX, float fromY, float toX, float toY)
    {
        // 1. Prüfe äußere Grenzen mit ZoneBounds (schnell)
        if (!_zoneBounds.Contains(toX, toY))
        {
            return false; // Außerhalb der Zone
        }
        
        // 2. Prüfe innere Kollisionen mit CollisionData (langsamer)
        if (!_collisionData.CanMoveTo(fromX, fromY, toX, toY))
        {
            return false; // Kollision mit Hindernis
        }
        
        return true; // Bewegung ist gültig
    }
}
```

**Reihenfolge wichtig:** Erst ZoneBounds (O(1)), dann CollisionData (O(n)) - Fail-Fast-Prinzip.

---

## 2. ZoneBounds Design-Entscheidungen

### ❌ KEINE ZoneId in ZoneBounds

**Entscheidung:** `ZoneBounds` enthält KEINE `ZoneId` Property.

#### Begründungen:

1. **Zirkuläre Abhängigkeit vermeiden**
   ```csharp
   // ❌ FALSCH - Zirkuläre Abhängigkeit
   public class Zone
   {
       public string ZoneId { get; }
       public ZoneBounds Bounds { get; }
   }
   
   public class ZoneBounds
   {
       public string ZoneId { get; }  // ← Redundant!
   }
   
   // ✅ RICHTIG - Klare Hierarchie
   public class Zone
   {
       public string ZoneId { get; }
       public ZoneBounds Bounds { get; }  // ← Bounds ist Value Object
   }
   
   public class ZoneBounds
   {
       public float MinX { get; }
       // ... keine ZoneId
   }
   ```

2. **Redundanz**
   - Zone besitzt bereits die ZoneId
   - ZoneBounds ist ein Teil der Zone (Komposition)
   - Redundante Daten führen zu Inkonsistenzen

3. **Single Responsibility Principle**
   - `ZoneBounds`: Geometrische Grenzen definieren
   - `Zone`: Identity und Kontext verwalten

4. **Wiederverwendbarkeit**
   - ZoneBounds kann für temporäre Bereiche genutzt werden (z.B. Spawn-Radius)
   - Keine Kopplung an Zone-Identity

### ✅ Inclusive-Exclusive Ranges: `[Min, Max)`

**Entscheidung:** MinX/MinY sind **inclusive**, MaxX/MaxY sind **exclusive**.

```
[MinX, MaxX) × [MinY, MaxY)
```

#### Beispiel

```
Zone A: [0, 1000) × [0, 1000)    → 0 ≤ x < 1000, 0 ≤ y < 1000
Zone B: [1000, 2000) × [0, 1000) → 1000 ≤ x < 2000, 0 ≤ y < 1000
                                    ↑
                            Nahtlose Grenze
```

#### Begründungen:

1. **Nahtlose Zonen**
   - Keine Überlappung: Zone A endet bei x=999.99..., Zone B startet bei x=1000.0
   - Keine Lücken: Jeder Punkt gehört zu genau einer Zone

2. **Mathematisch sauber**
   - Breite = MaxX - MinX
   - Höhe = MaxY - MinY
   - Keine ±1 Fehler

3. **Industrie-Standard**
   - Arrays: `[0, length)`
   - Ranges in Python: `range(0, 10)` → 0-9
   - C++ STL Iterators: `[begin, end)`

4. **Tile-kompatibel**
   ```csharp
   int tileX = (int)Math.Floor(worldX / TileSize);
   int tileY = (int)Math.Floor(worldY / TileSize);
   // Floor rundet ab → passt zu [Min, Max)
   ```

### Code-Beispiel: Contains()

```csharp
public class ZoneBounds
{
    public float MinX { get; }
    public float MaxX { get; }
    public float MinY { get; }
    public float MaxY { get; }
    
    public ZoneBounds(float minX, float maxX, float minY, float maxY)
    {
        if (minX >= maxX || minY >= maxY)
            throw new ArgumentException("Min must be less than Max");
            
        MinX = minX;
        MaxX = maxX;
        MinY = minY;
        MaxY = maxY;
    }
    
    /// <summary>
    /// Prüft ob Position innerhalb der Bounds liegt.
    /// Verwendet [Min, Max) Convention (Min inclusive, Max exclusive).
    /// </summary>
    public bool Contains(float x, float y)
    {
        return x >= MinX && x < MaxX 
            && y >= MinY && y < MaxY;
    }
    
    /// <summary>
    /// Begrenzt Position auf die Zone-Bounds.
    /// </summary>
    public (float x, float y) Clamp(float x, float y)
    {
        // Clamp to [MinX, MaxX - epsilon] um innerhalb zu bleiben
        const float epsilon = 0.001f;
        
        float clampedX = Math.Clamp(x, MinX, MaxX - epsilon);
        float clampedY = Math.Clamp(y, MinY, MaxY - epsilon);
        
        return (clampedX, clampedY);
    }
    
    /// <summary>
    /// Prüft ob Position nahe am Rand ist (für Zone-Übergang).
    /// </summary>
    public bool IsNearEdge(float x, float y, float threshold = 10f)
    {
        return (x - MinX < threshold) ||
               (MaxX - x < threshold) ||
               (y - MinY < threshold) ||
               (MaxY - y < threshold);
    }
    
    /// <summary>
    /// Berechnet die Fläche der Zone.
    /// </summary>
    public float Area => (MaxX - MinX) * (MaxY - MinY);
}
```

---

## 3. Speicherort: Shared Library

### 🔗 ZoneBounds und CollisionData in `Mmo.Shared`

**Entscheidung:** Beide Klassen liegen im `Mmo.Shared` Projekt.

```
shared/Mmo.Shared/
├── Entities/
│   ├── IEntity.cs
│   └── EntityState.cs
├── Zones/                    ← NEU
│   ├── ZoneBounds.cs         ← Hier
│   └── CollisionData.cs      ← Hier
├── Messages/
│   └── ZoneEvents/
│       └── ZoneState.cs
└── SharedConstants.cs
```

### Begründungen:

#### 1. **Client-Side Prediction braucht Collision-Daten**

```csharp
// CLIENT - Lokale Bewegung (Prediction)
public class ClientPlayerController
{
    private readonly CollisionData _collisionData;
    
    public void LocalMove(float deltaTime)
    {
        Vector2 newPos = _position + _velocity * deltaTime;
        
        // Client prüft sofort (ohne Server)
        if (_collisionData.CanMoveTo(_position.X, _position.Y, newPos.X, newPos.Y))
        {
            _position = newPos;  // Sofortiges Feedback
        }
    }
}
```

Ohne Shared Library:
- ❌ Client muss auf Server-Response warten (Lag)
- ❌ Ruckelige Bewegung bei schlechter Verbindung
- ❌ Client kann nicht lokal validieren

#### 2. **Keine geheime Logik**

Collision-Daten sind NICHT sicherheitsrelevant:
- Kein Gameplay-Vorteil durch Kenntnis der Daten
- Spieler sieht Hindernisse visuell sowieso
- Server validiert IMMER (authoritative)

```csharp
// SERVER - Finale Validierung
public class MovementHandler
{
    public void HandleMoveRequest(MoveRequest request)
    {
        // Server prüft IMMER selbst, unabhängig vom Client
        if (!ValidateMovement(request))
        {
            SendPositionCorrection();
        }
    }
}
```

#### 3. **Weniger Boilerplate**

Ohne Shared Library:
```csharp
// ❌ Duplikation
// Server:
public class ServerZoneBounds { public float MinX { get; set; } ... }
// Client:
public class ClientZoneBounds { public float MinX { get; set; } ... }
// + Konvertierungs-Code zwischen beiden!
```

Mit Shared Library:
```csharp
// ✅ Eine Klasse, beide nutzen sie
public class ZoneBounds { public float MinX { get; } ... }
```

### Dateistruktur-Übersicht

```csharp
// shared/Mmo.Shared/Zones/ZoneBounds.cs
namespace Mmo.Shared.Zones;

public class ZoneBounds
{
    public float MinX { get; }
    public float MaxX { get; }
    public float MinY { get; }
    public float MaxY { get; }
    
    // Implementation...
}

// shared/Mmo.Shared/Zones/CollisionData.cs
namespace Mmo.Shared.Zones;

public class CollisionData
{
    public int Width { get; }
    public int Height { get; }
    public int TileSize { get; }
    
    private readonly bool[,] _walkable;
    
    // Implementation...
}
```

### MessagePack-Attribute für Netzwerk-Übertragung

```csharp
using MessagePack;

namespace Mmo.Shared.Zones;

/// <summary>
/// Zone Bounds für Netzwerk-Übertragung.
/// </summary>
[MessagePackObject]
public class ZoneBoundsDto
{
    [Key(0)]
    public float MinX { get; set; }
    
    [Key(1)]
    public float MaxX { get; set; }
    
    [Key(2)]
    public float MinY { get; set; }
    
    [Key(3)]
    public float MaxY { get; set; }
}

/// <summary>
/// Collision Data für Netzwerk-Übertragung (komprimiert).
/// </summary>
[MessagePackObject]
public class CollisionDataDto
{
    [Key(0)]
    public int Width { get; set; }
    
    [Key(1)]
    public int Height { get; set; }
    
    [Key(2)]
    public int TileSize { get; set; }
    
    /// <summary>
    /// Run-Length Encoded (RLE) Walkable-Daten.
    /// Format: [count, value, count, value, ...]
    /// Beispiel: [10, true, 5, false, 20, true] = 10× walkable, 5× blocked, 20× walkable
    /// </summary>
    [Key(3)]
    public byte[] WalkableDataRLE { get; set; } = Array.Empty<byte>();
}
```

---

## 4. Datenverteilung: Client vs. Server

### 🎯 MVP: Statische Daten fix im Client

**Für den Prototyp:** Collision-Daten sind Teil des Client-Builds.

```
client/assets/zones/
├── startzone_collision.tres   ← Godot Resource
├── town_collision.tres
└── forest_collision.tres
```

#### Vorteile (MVP):
- ✅ Keine Netzwerk-Übertragung nötig
- ✅ Client startet sofort (keine Download-Zeit)
- ✅ Einfache Implementierung
- ✅ Für Prototyp ausreichend

#### Nachteile (MVP):
- ❌ Client-Update nötig bei Collision-Änderungen
- ❌ Keine dynamischen Zonen
- ❌ Größerer Client-Download

### 🚀 Später: Hybrid-Ansatz mit Version-Check

**Post-MVP:** Server sendet Collision-Daten nur bei Änderungen.

```
┌──────────┐                                    ┌──────────┐
│  Client  │                                    │  Server  │
└─────┬────┘                                    └─────┬────┘
      │                                               │
      │  JoinZone (ZoneId="startzone", Version=42)   │
      │──────────────────────────────────────────────▶│
      │                                               │
      │                                               │ Check: 
      │                                               │ Server Version == 42?
      │                                               │
      ◄─ YES ──────────────────────────────────────────┤
      │  ZoneJoined (UseLocalCollisionData=true)      │
      │                                               │
      
      ◄─ NO ───────────────────────────────────────────┤
      │  ZoneJoined (CollisionDataDto=...)            │
      │  ← Download & Cache                           │
```

#### Vorteile (Post-MVP):
- ✅ Hotfixes für Collision-Bugs ohne Client-Update
- ✅ Dynamische Zonen (Events, Terrain-Änderungen)
- ✅ Kleinerer Client-Download (nur Basis-Daten)
- ✅ A/B Testing von Zone-Layouts

#### Implementierung:
```csharp
// Server
public class ZoneJoinHandler
{
    public void HandleJoinZone(JoinZone message, ClientSession session)
    {
        var zone = _zoneManager.GetZone(message.ZoneId);
        var clientVersion = message.CollisionDataVersion;
        var serverVersion = zone.CollisionData.Version;
        
        if (clientVersion == serverVersion)
        {
            // Client hat aktuelle Daten
            SendZoneJoined(session, useLocalData: true);
        }
        else
        {
            // Client braucht Update
            var collisionDto = zone.CollisionData.ToDto();
            SendZoneJoined(session, collisionDto);
        }
    }
}

// Client
public class ZoneManager
{
    private readonly Dictionary<string, CollisionData> _cachedCollisionData = new();
    
    public void HandleZoneJoined(ZoneJoined message)
    {
        if (message.UseLocalData)
        {
            // Nutze eingebaute Daten
            _currentCollisionData = LoadLocalCollisionData(message.ZoneId);
        }
        else
        {
            // Server hat neue Daten geschickt
            _currentCollisionData = message.CollisionData.ToCollisionData();
            CacheCollisionData(message.ZoneId, _currentCollisionData);
        }
    }
}
```

### Roadmap

| Phase | Strategie | Status |
|-------|-----------|--------|
| **MVP (jetzt)** | Statische Daten im Client | ✅ Für Prototyp |
| **Beta** | Hybrid mit Version-Check | 🔄 Geplant |
| **Live** | Server-Side mit Caching | 🔮 Später |

---

## 5. CollisionData Design

### 🎲 Tile-basiertes Grid-System

**Entscheidung:** `bool[,]` Grid mit fester Tile-Größe.

```
┌───┬───┬───┬───┬───┐
│ ✓ │ ✓ │ ✓ │ ✓ │ ✓ │  ← Walkable (true)
├───┼───┼───┼───┼───┤
│ ✓ │ ✗ │ ✗ │ ✗ │ ✓ │  ← Blocked (false)
├───┼───┼───┼───┼───┤
│ ✓ │ ✗ │   │ ✗ │ ✓ │
├───┼───┼───┼───┼───┤
│ ✓ │ ✓ │ ✓ │ ✓ │ ✓ │
└───┴───┴───┴───┴───┘
  ↑
  Ein Tile (z.B. 64×64 Pixel)
```

### Struktur

```csharp
namespace Mmo.Shared.Zones;

public class CollisionData
{
    /// <summary>
    /// Breite der Collision-Map in Tiles.
    /// </summary>
    public int Width { get; }
    
    /// <summary>
    /// Höhe der Collision-Map in Tiles.
    /// </summary>
    public int Height { get; }
    
    /// <summary>
    /// Größe eines Tiles in World-Units (Pixel).
    /// Standard: 64×64 Pixel pro Tile.
    /// </summary>
    public int TileSize { get; }
    
    /// <summary>
    /// Grid mit Walkable-Informationen.
    /// [x, y] = true → Tile ist begehbar
    /// [x, y] = false → Tile ist blockiert
    /// </summary>
    private readonly bool[,] _walkable;
    
    public CollisionData(int width, int height, int tileSize = 64)
    {
        if (width <= 0 || height <= 0 || tileSize <= 0)
            throw new ArgumentException("Dimensions must be positive");
            
        Width = width;
        Height = height;
        TileSize = tileSize;
        _walkable = new bool[width, height];
        
        // Default: Alles begehbar
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                _walkable[x, y] = true;
            }
        }
    }
    
    /// <summary>
    /// Prüft ob ein einzelnes Tile begehbar ist.
    /// </summary>
    public bool IsWalkable(int tileX, int tileY)
    {
        if (tileX < 0 || tileX >= Width || tileY < 0 || tileY >= Height)
            return false;  // Außerhalb der Map
            
        return _walkable[tileX, tileY];
    }
    
    /// <summary>
    /// Konvertiert Welt-Koordinaten zu Tile-Koordinaten.
    /// </summary>
    public (int x, int y) WorldToTile(float worldX, float worldY)
    {
        int tileX = (int)Math.Floor(worldX / TileSize);
        int tileY = (int)Math.Floor(worldY / TileSize);
        return (tileX, tileY);
    }
    
    /// <summary>
    /// Setzt ein Tile als begehbar oder blockiert.
    /// </summary>
    public void SetWalkable(int tileX, int tileY, bool walkable)
    {
        if (tileX < 0 || tileX >= Width || tileY < 0 || tileY >= Height)
            throw new ArgumentOutOfRangeException("Tile position out of bounds");
            
        _walkable[tileX, tileY] = walkable;
    }
}
```

### CanMoveTo mit Bresenham-Line-Algorithmus

**Problem:** Spieler bewegt sich zwischen zwei Punkten. Alle Tiles auf dem Weg müssen begehbar sein.

```
From (10, 10) → To (50, 30)
        ↓
Prüfe alle Tiles entlang der Linie
```

**Lösung:** Bresenham's Line Algorithm (effizient, keine Floating-Point-Operationen)

```csharp
/// <summary>
/// Prüft ob eine Bewegung von (fromX, fromY) zu (toX, toY) möglich ist.
/// Nutzt Bresenham's Line Algorithm um alle Tiles auf dem Weg zu prüfen.
/// </summary>
public bool CanMoveTo(float fromX, float fromY, float toX, float toY)
{
    var (startTileX, startTileY) = WorldToTile(fromX, fromY);
    var (endTileX, endTileY) = WorldToTile(toX, toY);
    
    // Bresenham's Line Algorithm
    int dx = Math.Abs(endTileX - startTileX);
    int dy = Math.Abs(endTileY - startTileY);
    int sx = startTileX < endTileX ? 1 : -1;
    int sy = startTileY < endTileY ? 1 : -1;
    int err = dx - dy;
    
    int x = startTileX;
    int y = startTileY;
    
    while (true)
    {
        // Prüfe aktuelles Tile
        if (!IsWalkable(x, y))
        {
            return false;  // Kollision gefunden
        }
        
        // Ziel erreicht?
        if (x == endTileX && y == endTileY)
        {
            return true;  // Weg ist frei
        }
        
        // Nächster Schritt
        int e2 = 2 * err;
        
        if (e2 > -dy)
        {
            err -= dy;
            x += sx;
        }
        
        if (e2 < dx)
        {
            err += dx;
            y += sy;
        }
    }
}
```

### Performance-Charakteristiken

| Aspekt | Wert |
|--------|------|
| **Speicher** | Width × Height × 1 Byte |
| **Beispiel** | 100×100 Tiles = 10 KB |
| **IsWalkable()** | O(1) - Array-Lookup |
| **CanMoveTo()** | O(n) - n = Anzahl Tiles auf Linie |
| **Worst Case** | Diagonale über gesamte Map |

### Optimierungen (für später)

1. **Spatial Hashing** für große Zonen
2. **Quadtree** für dynamische Hindernisse
3. **Navigation Mesh** für komplexe Pathfinding

---

## 6. Zusammenfassung der Entscheidungen

| # | Entscheidung | Begründung |
|---|--------------|------------|
| 1 | **ZoneBounds ≠ CollisionData** | Verschiedene Zwecke (Bounds vs. Hindernisse) |
| 2 | **Keine ZoneId in ZoneBounds** | Vermeidet Redundanz und zirkuläre Abhängigkeit |
| 3 | **[Min, Max) Convention** | Nahtlose Zonen, mathematisch sauber, Industrie-Standard |
| 4 | **Shared Library** | Client-Side Prediction, keine geheime Logik, weniger Code |
| 5 | **MVP: Statische Client-Daten** | Einfach, schnell, für Prototyp ausreichend |
| 6 | **Später: Hybrid mit Versioning** | Hotfixes, dynamische Zonen, kleinerer Client |
| 7 | **Tile-basiertes Grid** | Einfach, performant, gut für 2D Top-Down |
| 8 | **bool[,] Array** | Speicher-effizient, O(1) Lookup |
| 9 | **Bresenham für CanMoveTo** | Effizient, keine Floating-Point-Fehler |
| 10 | **TileSize = 64 Pixel** | Balance zwischen Granularität und Performance |

---

## Ressourcen

### Interne Dokumentation

- **[ID-System](ID_SYSTEM.md)** - Entity Identity, ZoneId Ranges, GlobalKey
- **[Issue Updates Guide](../../01-Planning/ISSUE_UPDATES_GUIDE.md)** - Zone-Konzept Integration in Issues
- **[TECHNICAL_DESIGN.md](TECHNICAL_DESIGN.md)** - Technische Design-Entscheidungen
- **[ARCHITECTURE.md](Architecture-Overview.md)** - Gesamt-Architektur Übersicht

### Verwandte Issues

- **Issue #107** - Zone Data Architecture Documentation (dieses Dokument)
- **Issue #75** - Spawn-System mit Startposition
- **Issue #76** - Weltgrenzen und Position-Clamping

### Externe Ressourcen

- [Bresenham's Line Algorithm](https://en.wikipedia.org/wiki/Bresenham%27s_line_algorithm)
- [Spatial Partitioning](https://gameprogrammingpatterns.com/spatial-partition.html)
- [Zone-Based MMO Architecture](https://www.gamedeveloper.com/programming/zone-based-world-management-in-mmos)

---

**Letzte Aktualisierung:** 2025-12-08  
**Autoren:** Development Team  
**Status:** ✅ Finalisiert

Source: docs/02-architecture/ZONE_DATA_ARCHITECTURE.md
