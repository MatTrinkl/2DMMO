# 🗺️ Map / Minimap / Waypoint Messages (3400-3499)

**Kategorie:** 34  
**Range:** 3400-3499  
**Phase:** Prototyp  
**Status:** 🟢 In Entwicklung

[← Zurück zur Übersicht](README.md)

---

## 📋 Inhaltsverzeichnis

- [Überblick](#-überblick)
- [Datenmodell](#-datenmodell)
  - [MapDefinition](#mapdefinition)
  - [MapChunk](#mapchunk)
  - [MapMarker](#mapmarker)
  - [Waypoint](#waypoint)
  - [FlightpathNode](#flightpathnode)
- [Discovery & Fog of War](#-discovery--fog-of-war)
- [Markers, Waypoints & Pings](#-markers-waypoints--pings)
- [Flightpaths & Navigation](#-flightpaths--navigation)
- [Sync, Caching & Versioning](#-sync-caching--versioning)
- [DTOs / Interfaces](#-dtos--interfaces)
- [Enums / ErrorCodes / Flags](#-enums--errorcodes--flags)
- [Regeln & Sicherheit](#-regeln--sicherheit)
- [Aktive Messages 3400-3499](#-aktive-messages-3400-3499)
  - [MapExplore (3400)](#mapexplore-3400)
  - [MapExploreUpdate (3401)](#mapexploreupdate-3401)
  - [MapFogReveal (3402)](#mapfogreveal-3402)
  - [MapExploreResponse (3403)](#mapexploreresponse-3403)
  - [WaypointSet (3410)](#waypointset-3410)
  - [WaypointSetResponse (3411)](#waypointsetresponse-3411)
  - [WaypointClear (3412)](#waypointclear-3412)
  - [WaypointClearResponse (3413)](#waypointclearresponse-3413)
  - [WaypointShare (3414)](#waypointshare-3414)
  - [WaypointShareResponse (3415)](#waypointshareresponse-3415)
  - [WaypointAccept (3416)](#waypointaccept-3416)
  - [WaypointAcceptResponse (3417)](#waypointacceptresponse-3417)
  - [WaypointUpdatedEvent (3418)](#waypointupdatedevent-3418)
  - [PingMap (3420)](#pingmap-3420)
  - [PingMapResponse (3421)](#pingmapresponse-3421)
  - [PingMapEvent (3422)](#pingmapevent-3422)
  - [FlightpathDiscover (3430)](#flightpathdiscover-3430)
  - [FlightpathListRequest (3431)](#flightpathlistrequest-3431)
  - [FlightpathListResponse (3432)](#flightpathlistresponse-3432)
  - [FlightpathRequest (3433)](#flightpathrequest-3433)
  - [FlightpathResponse (3434)](#flightpathresponse-3434)
  - [FlightpathStart (3435)](#flightpathstart-3435)
  - [MapMarkerAdd (3440)](#mapmarkeradd-3440)
  - [MapMarkerAddResponse (3441)](#mapmarkeraddresponse-3441)
  - [MapMarkerRemove (3442)](#mapmarkerremove-3442)
  - [MapMarkerRemoveResponse (3443)](#mapmarkerremoveresponse-3443)
  - [MapMarkerUpdate (3444)](#mapmarkerupdate-3444)
  - [MapMarkerSyncEvent (3445)](#mapmarkersyncevent-3445)
  - [WorldMapRequest (3450)](#worldmaprequest-3450)
  - [WorldMapResponse (3451)](#worldmapresponse-3451)
  - [MinimapUpdate (3452)](#minimapupdate-3452)
  - [AreaDiscovered (3453)](#areadiscovered-3453)
  - [MapDiscoveryDeltaEvent (3454)](#mapdiscoverydeltaevent-3454)
  - [MapStateSyncRequest (3455)](#mapstatesyncrequest-3455)
  - [MapStateSyncResponse (3456)](#mapstatesyncresponse-3456)
- [Obsolete Messages](#-obsolete-messages)
- [Edge Cases & Fehlerfälle](#-edge-cases--fehlerfälle)
- [Anhang](#-anhang)

---

## 📋 Überblick

Das Map-System verwaltet World Map, Minimap, Fog of War, Waypoints, Pings und Flightpaths. **Der Server ist authoritative** für Discovery-Daten und validiert alle Map-Interaktionen.

### Scope

| Feature | Beschreibung |
|---------|--------------|
| **World Map** | Vollständige Weltkarte mit Zonen, POIs, Flightpaths |
| **Minimap** | Lokale Kartenansicht mit Echtzeit-Updates |
| **Fog of War** | Pro-Character Discovery-System |
| **Waypoints** | Persönliche und geteilte Wegpunkte |
| **Pings** | Temporäre Map-Markierungen für Kommunikation |
| **Markers** | Permanente benutzerdefinierte Markierungen |
| **Flightpaths** | Schnellreise-System zwischen entdeckten Punkten |
| **Area Discovery** | XP und Achievements für Erkundung |

### Architektur

```
┌─────────────────────────────────────────────────────────────────┐
│  Client                                                         │
│  ├── WorldMapUI (Pan, Zoom, Markers, Flightpaths)              │
│  ├── MinimapUI (Rotation, Zoom, Local POIs)                    │
│  ├── WaypointSystem (Navigation Arrow, Distance)               │
│  └── FogOfWarRenderer (Chunk-based reveal)                     │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼ Requests & Position Updates
┌─────────────────────────────────────────────────────────────────┐
│  Server (Authoritative)                                         │
│  ├── MapService (Zone Data, POIs, Static Data)                 │
│  ├── DiscoveryService (Fog of War, Area Discovery)             │
│  ├── WaypointService (Personal, Shared, Party)                 │
│  ├── FlightpathService (Routes, Costs, Travel)                 │
│  └── PingService (Rate Limiting, Broadcast)                    │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼ Events & Sync
┌─────────────────────────────────────────────────────────────────┐
│  Client (Render & Cache)                                        │
│  ├── Map Tile Cache (LRU, Version-based)                       │
│  ├── Discovery Mask (Per-Zone Bitmask)                         │
│  └── Marker Database (Local + Synced)                          │
└─────────────────────────────────────────────────────────────────┘
```

### Map Data Flow

```
Client                         Server
  │                              │
  │  [Character moves]           │
  │  PositionUpdate (200)        │
  │─────────────────────────────►│
  │                              │
  │                              │  Check Discovery
  │                              │  New chunk revealed?
  │                              │
  │  MapExploreUpdate (3401)     │  ← Only if new area
  │  ChunkIds: [12, 13]          │
  │◄─────────────────────────────│
  │                              │
  │  [Client updates fog mask]   │
  │                              │
  │  AreaDiscovered (3453)       │  ← If named area
  │  AreaId: "Northshire Valley" │
  │  XpGained: 50                │
  │◄─────────────────────────────│
```

---

## 🧠 Datenmodell

### MapDefinition

Statische Kartendefinition pro Zone (Server-seitig, Client cached).

| Feld | Typ | Beschreibung |
|------|-----|--------------|
| ZoneId | ushort | Zone-ID |
| MapId | ushort | Map-Asset-ID |
| DisplayName | string | Anzeigename |
| Bounds | Rect | Kartengrenzen (Min/Max Coords) |
| ChunkSize | int | Größe eines Fog-Chunks (z.B. 64x64) |
| ChunksX | int | Anzahl Chunks horizontal |
| ChunksY | int | Anzahl Chunks vertikal |
| TotalChunks | int | Gesamtzahl Chunks |
| DiscoverableAreas | List\<DiscoverableArea\> | Benannte Gebiete |
| POIs | List\<MapPOI\> | Points of Interest |
| FlightpathNodes | List\<FlightpathNode\> | Flugpunkte |
| MapVersion | uint | Version für Caching |

### MapChunk

Einzelner Fog-of-War Chunk. 

| Feld | Typ | Beschreibung |
|------|-----|--------------|
| ChunkId | int | Chunk-Index (0 to TotalChunks-1) |
| ChunkX | int | X-Position im Grid |
| ChunkY | int | Y-Position im Grid |
| Bounds | Rect | Chunk-Grenzen |
| IsDiscovered | bool | Vom Character entdeckt?  |
| DiscoveredAt | long?  | Unix Timestamp |

### MapMarker

Benutzerdefinierte Kartenmarkierung.

| Feld | Typ | Beschreibung |
|------|-----|--------------|
| MarkerId | Guid | Eindeutige Marker-ID |
| ZoneId | ushort | Zone der Markierung |
| Position | Position | Koordinaten |
| MarkerType | MarkerType | Icon-Typ |
| Label | string | Benutzerdefinierter Text (max.  32 Zeichen) |
| Color | uint | ARGB-Farbe |
| Visibility | MarkerVisibility | Personal, Party, Guild, Public |
| CreatedBy | long | Character-ID des Erstellers |
| CreatedAt | long | Unix Timestamp |
| ExpiresAt | long?  | Ablaufzeit (null = permanent) |

### Waypoint

Navigations-Wegpunkt.

| Feld | Typ | Beschreibung |
|------|-----|--------------|
| WaypointId | Guid | Eindeutige Waypoint-ID |
| ZoneId | ushort | Ziel-Zone |
| Position | Position | Ziel-Koordinaten |
| Label | string?  | Optionaler Name |
| Source | WaypointSource | Manual, Quest, Party, Shared |
| SharedBy | long?  | Character-ID wenn geteilt |
| CreatedAt | long | Unix Timestamp |
| ExpiresAt | long?  | Ablaufzeit für temporäre Waypoints |

### FlightpathNode

Flugpunkt für Schnellreise.

| Feld | Typ | Beschreibung |
|------|-----|--------------|
| NodeId | ushort | Eindeutige Node-ID |
| ZoneId | ushort | Zone des Flugpunkts |
| Position | Position | Koordinaten |
| DisplayName | string | Anzeigename |
| Faction | Faction | Zugehörige Fraktion |
| RequiredLevel | int | Mindest-Level |
| Connections | List\<FlightpathConnection\> | Verbindungen zu anderen Nodes |

### FlightpathConnection

Verbindung zwischen zwei Flugpunkten. 

| Feld | Typ | Beschreibung |
|------|-----|--------------|
| TargetNodeId | ushort | Ziel-Flugpunkt |
| Cost | int | Kosten in Kupfer |
| Duration | float | Flugzeit in Sekunden |
| PathPoints | List\<Position\> | Wegpunkte für Animation |

---

## 🌫️ Discovery & Fog of War

### Chunk-basiertes System

```
┌──────────────────────────────────────────────────────────────┐
│  Zone Map (z. B. 2048x2048 Units)                             │
│  ┌────┬────┬────┬────┬────┬────┬────┬────┐                  │
│  │ 0  │ 1  │ 2  │ 3  │ 4  │ 5  │ 6  │ 7  │  ChunkSize:  256  │
│  ├────┼────┼────┼────┼────┼────┼────┼────┤                  │
│  │ 8  │ 9  │████│████│████│████│ 14 │ 15 │  ████ = Hidden   │
│  ├────┼────┼────┼────┼────┼────┼────┼────┤                  │
│  │ 16 │ 17 │████│ 🧍 │████│████│ 22 │ 23 │  🧍 = Player     │
│  ├────┼────┼────┼────┼────┼────┼────┼────┤                  │
│  │ 24 │ 25 │████│████│████│████│ 30 │ 31 │  Revealed:  19    │
│  └────┴────┴────┴────┴────┴────┴────┴────┘                  │
│                                                              │
│  Discovery Radius: 2 Chunks (512 Units)                      │
│  Player at Chunk 19 reveals:  10, 11, 12, 18, 19, 20, 26, 27 │
└──────────────────────────────────────────────────────────────┘
```

### Discovery-Regeln

| Regel | Beschreibung |
|-------|--------------|
| **Radius** | 2 Chunks (512 Units bei ChunkSize 256) |
| **Persistence** | Pro Character, permanent gespeichert |
| **Sync** | Bei Zone-Betreten wird Discovery-Maske geladen |
| **Delta Updates** | Nur neu entdeckte Chunks werden gesendet |
| **Account-Wide Option** | Optional:  Discovery zwischen Characters teilen |

### Discovery Bitmask

```csharp
// Effiziente Speicherung als Bitmask
public class ZoneDiscoveryMask
{
    public ushort ZoneId { get; set; }
    public byte[] ChunkMask { get; set; } // 1 bit per chunk
    public uint Revision { get; set; }
    
    public bool IsChunkDiscovered(int chunkId)
    {
        int byteIndex = chunkId / 8;
        int bitIndex = chunkId % 8;
        return (ChunkMask[byteIndex] & (1 << bitIndex)) != 0;
    }
    
    public void SetChunkDiscovered(int chunkId)
    {
        int byteIndex = chunkId / 8;
        int bitIndex = chunkId % 8;
        ChunkMask[byteIndex] |= (byte)(1 << bitIndex);
        Revision++;
    }
}
```

### Area Discovery

Benannte Gebiete geben XP und Achievement-Progress bei erster Entdeckung.

| Feld | Typ | Beschreibung |
|------|-----|--------------|
| AreaId | string | Eindeutige Area-ID |
| DisplayName | string | Anzeigename |
| ChunkIds | List\<int\> | Zugehörige Chunks |
| XpReward | int | XP bei Entdeckung |
| AchievementId | string?  | Verknüpftes Achievement |
| MinLevel | int | Empfohlenes Mindest-Level |
| MaxLevel | int | Empfohlenes Höchst-Level |

---

## 📌 Markers, Waypoints & Pings

### Marker-System

```
┌─────────────────────────────────────────────────────────────────┐
│  Marker Types                                                   │
├─────────────────────────────────────────────────────────────────┤
│  🏠 Home        - Hearthstone Location                         │
│  ⭐ Favorite    - Allgemeine Favoriten                          │
│  💀 Danger      - Gefährliche Gebiete                          │
│  💰 Resource    - Ressourcen-Spots                             │
│  📍 Custom      - Benutzerdefiniert                            │
│  ❓ Quest       - Quest-relevante Orte                         │
│  👥 Meeting     - Treffpunkte                                  │
│  🏰 Dungeon     - Dungeon-Eingänge                             │
└─────────────────────────────────────────────────────────────────┘
```

### Marker-Limits

| Typ | Max.  pro Character | Max. pro Party/Guild |
|-----|-------------------|---------------------|
| Personal | 50 | - |
| Party | - | 10 |
| Guild | - | 100 |
| Temporary | 10 | 20 |

### Waypoint-System

```
Client                         Server                    Party Members
  │                              │                           │
  │  WaypointSet (3410)          │                           │
  │  Position:  (100, 200)        │                           │
  │  Label: "Boss Location"      │                           │
  │─────────────────────────────►│                           │
  │                              │                           │
  │  WaypointSetResponse (3411)  │                           │
  │  WaypointId: xyz             │                           │
  │◄─────────────────────────────│                           │
  │                              │                           │
  │  [Navigation arrow shows]    │                           │
  │                              │                           │
  │  WaypointShare (3414)        │                           │
  │  WaypointId:  xyz             │                           │
  │  ShareTarget: Party          │                           │
  │─────────────────────────────►│                           │
  │                              │  WaypointUpdatedEvent     │
  │                              │──────────────────────────►│
  │  WaypointShareResponse       │                           │
  │◄─────────────────────────────│                           │
```

### Ping-System

Temporäre Markierungen für schnelle Kommunikation.

| Ping-Typ | Icon | Dauer | Sound |
|----------|------|-------|-------|
| `Alert` | ❗ | 5s | Warning |
| `Attack` | ⚔️ | 5s | Attack |
| `Defend` | 🛡️ | 5s | Defend |
| `Assist` | 🆘 | 5s | Help |
| `OnMyWay` | 🏃 | 3s | Confirm |
| `Danger` | ☠️ | 8s | Danger |
| `Generic` | 📍 | 5s | Ping |

### Ping Rate Limits

| Kontext | Limit | Cooldown |
|---------|-------|----------|
| Solo | 5/min | - |
| Party | 10/min | 3s zwischen Pings |
| Raid | 20/min (Leader) | 2s zwischen Pings |
| Guild | 5/min | 5s zwischen Pings |

---

## ✈️ Flightpaths & Navigation

### Flightpath-System

```
┌─────────────────────────────────────────────────────────────────┐
│  Flightpath Network                                             │
│                                                                 │
│        [Ironforge] ───────── [Stormwind] ───────── [Darkshire] │
│             │                     │                     │       │
│             │                     │                     │       │
│        [Menethil] ──────── [Sentinel Hill] ────── [Lakeshire]  │
│                                                                 │
│  ════════════════════════════════════════════════════════════  │
│  Discovered:  ●  Undiscovered: ○  Hostile: ✕                    │
└─────────────────────────────────────────────────────────────────┘
```

### Flightpath Discovery

| Methode | Beschreibung |
|---------|--------------|
| **NPC Interaction** | Sprechen mit Flugmeister |
| **Auto-Discovery** | Bei Annäherung (< 50 Units) |
| **Quest Reward** | Bestimmte Quests schalten Flugpunkte frei |

### Flightpath Travel

```
Client                         Server
  │                              │
  │  FlightpathRequest (3433)    │
  │  FromNodeId: 1 (Stormwind)   │
  │  ToNodeId: 5 (Ironforge)     │
  │─────────────────────────────►│
  │                              │
  │                              │  Validate: 
  │                              │  - Both nodes discovered
  │                              │  - Path exists
  │                              │  - Enough gold
  │                              │  - Not in combat
  │                              │
  │  FlightpathResponse (3434)   │
  │  Success: true               │
  │  Route: [1, 3, 5]            │
  │  TotalCost: 50c              │
  │  TotalDuration: 180s         │
  │◄─────────────────────────────│
  │                              │
  │  [Player confirms]           │
  │                              │
  │  FlightpathStart (3435)      │
  │  RouteId: xyz                │
  │◄─────────────────────────────│
  │                              │
  │  [Flight animation begins]   │
  │  [Player immune/untargetable]│
```

### Flightpath Kosten

| Distanz | Basis-Kosten | Mit Reputation |
|---------|--------------|----------------|
| Short (< 1 Zone) | 10c | 8c |
| Medium (1-2 Zones) | 50c | 40c |
| Long (3+ Zones) | 1s+ | 80c+ |

---

## 🔄 Sync, Caching & Versioning

### Map Data Versioning

```csharp
public class MapVersionInfo
{
    public ushort ZoneId { get; set; }
    public uint MapVersion { get; set; }      // Static map data
    public uint DiscoveryRevision { get; set; } // Player discovery
    public uint MarkerRevision { get; set; }   // Custom markers
    public long LastSync { get; set; }         // Unix timestamp
}
```

### Sync-Strategien

| Daten-Typ | Strategie | Trigger |
|-----------|-----------|---------|
| **Map Definition** | Full Sync | Zone-Betreten (wenn Version != cached) |
| **Discovery Mask** | Full + Delta | Login, Zone-Betreten, Movement |
| **Markers** | Delta | Marker CRUD, Party/Guild Events |
| **Waypoints** | Delta | Set/Clear/Share |
| **Flightpaths** | Full Sync | Zone-Betreten, Discovery |

### Client Caching

```csharp
public class MapCache
{
    // LRU Cache für Map-Definitionen (max 20 Zones)
    private readonly LRUCache<ushort, MapDefinition> _mapCache = new(20);
    
    // Discovery Masks pro Zone
    private readonly Dictionary<ushort, ZoneDiscoveryMask> _discoveryCache = new();
    
    // Marker Cache
    private readonly Dictionary<Guid, MapMarker> _markerCache = new();
    
    public bool NeedsSync(ushort zoneId, uint serverVersion)
    {
        if (! _mapCache.TryGet(zoneId, out var cached))
            return true;
        return cached.MapVersion < serverVersion;
    }
}
```

### Delta Sync Flow

```
Client                         Server
  │                              │
  │  MapStateSyncRequest (3455)  │
  │  ZoneId: 1001                │
  │  ClientDiscoveryRev: 5       │
  │  ClientMarkerRev: 3          │
  │─────────────────────────────►│
  │                              │
  │                              │  Server Discovery:  8
  │                              │  Server Markers: 5
  │                              │
  │  MapStateSyncResponse (3456) │
  │  DiscoveryDelta: [chunks]    │  ← Nur Chunks 6, 7, 8
  │  MarkerDelta:  [markers]      │  ← Nur Markers 4, 5
  │  NewDiscoveryRev: 8          │
  │  NewMarkerRev: 5             │
  │◄─────────────────────────────│
```

---

## 🧱 DTOs / Interfaces

### MapDefinitionDto

```csharp
[MessagePackObject]
public class MapDefinitionDto
{
    [Key(0)] public ushort ZoneId { get; set; }
    [Key(1)] public ushort MapId { get; set; }
    [Key(2)] public string DisplayName { get; set; } = "";
    [Key(3)] public RectDto Bounds { get; set; } = new();
    [Key(4)] public int ChunkSize { get; set; }
    [Key(5)] public int ChunksX { get; set; }
    [Key(6)] public int ChunksY { get; set; }
    [Key(7)] public List<DiscoverableAreaDto> DiscoverableAreas { get; set; } = new();
    [Key(8)] public List<MapPOIDto> POIs { get; set; } = new();
    [Key(9)] public uint MapVersion { get; set; }
}
```

### DiscoveryMaskDto

```csharp
[MessagePackObject]
public class DiscoveryMaskDto
{
    [Key(0)] public ushort ZoneId { get; set; }
    [Key(1)] public byte[] ChunkMask { get; set; } = Array.Empty<byte>();
    [Key(2)] public uint Revision { get; set; }
    [Key(3)] public int TotalDiscovered { get; set; }
    [Key(4)] public int TotalChunks { get; set; }
}
```

### MapMarkerDto

```csharp
[MessagePackObject]
public class MapMarkerDto
{
    [Key(0)] public Guid MarkerId { get; set; }
    [Key(1)] public ushort ZoneId { get; set; }
    [Key(2)] public Position Position { get; set; }
    [Key(3)] public MarkerType MarkerType { get; set; }
    [Key(4)] public string Label { get; set; } = "";
    [Key(5)] public uint Color { get; set; }
    [Key(6)] public MarkerVisibility Visibility { get; set; }
    [Key(7)] public long CreatedAt { get; set; }
    [Key(8)] public long?  ExpiresAt { get; set; }
}
```

### WaypointDto

```csharp
[MessagePackObject]
public class WaypointDto
{
    [Key(0)] public Guid WaypointId { get; set; }
    [Key(1)] public ushort ZoneId { get; set; }
    [Key(2)] public Position Position { get; set; }
    [Key(3)] public string?  Label { get; set; }
    [Key(4)] public WaypointSource Source { get; set; }
    [Key(5)] public long?  SharedBy { get; set; }
    [Key(6)] public string? SharedByName { get; set; }
    [Key(7)] public long CreatedAt { get; set; }
}
```

### FlightpathNodeDto

```csharp
[MessagePackObject]
public class FlightpathNodeDto
{
    [Key(0)] public ushort NodeId { get; set; }
    [Key(1)] public ushort ZoneId { get; set; }
    [Key(2)] public Position Position { get; set; }
    [Key(3)] public string DisplayName { get; set; } = "";
    [Key(4)] public Faction Faction { get; set; }
    [Key(5)] public bool IsDiscovered { get; set; }
    [Key(6)] public List<FlightpathConnectionDto> Connections { get; set; } = new();
}

[MessagePackObject]
public class FlightpathConnectionDto
{
    [Key(0)] public ushort TargetNodeId { get; set; }
    [Key(1)] public int Cost { get; set; }
    [Key(2)] public float Duration { get; set; }
}
```

### PingDto

```csharp
[MessagePackObject]
public class PingDto
{
    [Key(0)] public Guid PingId { get; set; }
    [Key(1)] public ushort ZoneId { get; set; }
    [Key(2)] public Position Position { get; set; }
    [Key(3)] public PingType PingType { get; set; }
    [Key(4)] public long SenderId { get; set; }
    [Key(5)] public string SenderName { get; set; } = "";
    [Key(6)] public long Timestamp { get; set; }
    [Key(7)] public float Duration { get; set; }
}
```

---

## 🧩 Enums / ErrorCodes / Flags

### MarkerType

```csharp
public enum MarkerType :  byte
{
    Generic = 0,
    Home = 1,
    Favorite = 2,
    Danger = 3,
    Resource = 4,
    Quest = 5,
    Meeting = 6,
    Dungeon = 7,
    Vendor = 8,
    Trainer = 9,
    Mailbox = 10,
    Bank = 11,
    Auction = 12,
    Inn = 13,
    Flightmaster = 14,
    Custom = 99
}
```

### MarkerVisibility

```csharp
public enum MarkerVisibility :  byte
{
    Personal = 0,
    Party = 1,
    Raid = 2,
    Guild = 3,
    Public = 4  // Nur für spezielle Events
}
```

### WaypointSource

```csharp
public enum WaypointSource : byte
{
    Manual = 0,
    Quest = 1,
    Party = 2,
    Shared = 3,
    Flightpath = 4,
    Death = 5  // Corpse location
}
```

### PingType

```csharp
public enum PingType : byte
{
    Generic = 0,
    Alert = 1,
    Attack = 2,
    Defend = 3,
    Assist = 4,
    OnMyWay = 5,
    Danger = 6,
    Retreat = 7,
    OmW = 8  // On my Way shortcut
}
```

### MapErrorCode

```csharp
public enum MapErrorCode : byte
{
    None = 0,
    InvalidZone = 1,
    InvalidPosition = 2,
    MarkerLimitReached = 3,
    WaypointLimitReached = 4,
    InvalidMarkerId = 5,
    InvalidWaypointId = 6,
    PermissionDenied = 7,
    NotInParty = 8,
    NotInGuild = 9,
    LabelTooLong = 10,
    InvalidMarkerType = 11,
    PingRateLimited = 12,
    FlightpathNotDiscovered = 13,
    FlightpathNotConnected = 14,
    NotEnoughGold = 15,
    InCombat = 16,
    AlreadyFlying = 17,
    InvalidFlightpathNode = 18,
    ZoneNotLoaded = 19,
    ChunkOutOfBounds = 20
}
```

---

## ⚙️ Regeln & Sicherheit

### Server Authoritative Rules

| Regel | Beschreibung |
|-------|--------------|
| **Discovery Validation** | Server berechnet sichtbare Chunks basierend auf Position |
| **Marker Ownership** | Nur Ersteller kann Personal Markers löschen |
| **Ping Broadcast** | Server validiert Ping-Berechtigung vor Broadcast |
| **Flightpath Validation** | Server prüft Discovery, Gold, Combat-Status |

### Anti-Cheat

| Prüfung | Beschreibung |
|---------|--------------|
| **Position Validation** | Discovery nur für gültige Positionen |
| **Speed Check** | Discovery-Rate vs.  Movement-Speed |
| **Chunk Bounds** | Chunks müssen im gültigen Bereich sein |
| **Flightpath Abuse** | Keine Flüge während Flug |

### Rate Limits

| Operation | Limit | Cooldown |
|-----------|-------|----------|
| MarkerAdd | 20/min | - |
| MarkerUpdate | 30/min | - |
| WaypointSet | 10/min | - |
| WaypointShare | 5/min | 10s |
| PingMap | Siehe Ping-Tabelle | - |
| FlightpathRequest | 3/min | - |
| MapStateSyncRequest | 1/10s | - |

---

## 📩 Aktive Messages 3400-3499

---

## MapExplore (3400)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig (Movement-based)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Client meldet Position für Discovery-Berechnung.  Wird automatisch bei Bewegung in neue Bereiche gesendet.

### Im Scope ✅
- Position für Discovery melden
- Chunk-basierte Berechnung triggern

### Nicht im Scope ❌
- Direkte Chunk-Freischaltung → Server berechnet
- Named Area Discovery → Separate Message

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.MapExplore` | Ja |
| ZoneId | ushort | Aktuelle Zone | Ja |
| Position | Position | Aktuelle Position | Ja |

### Erwartete Response
- `MapExploreUpdate` (3401) bei neuen Chunks
- `MapExploreResponse` (3403) bei Fehler

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.MapExplore)]
public class MapExplore : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.MapExplore;
    [Key(1)] public ushort ZoneId { get; set; }
    [Key(2)] public Position Position { get; set; }
}
```

### Server-Verhalten

```csharp
public void HandleMapExplore(MapExplore request, Character character)
{
    // 1. Validate Position
    if (!_zoneService.IsValidPosition(request.ZoneId, request.Position))
        return SendError(MapErrorCode.InvalidPosition);
    
    // 2. Calculate visible chunks
    var visibleChunks = _mapService.GetVisibleChunks(
        request.ZoneId, 
        request.Position, 
        discoveryRadius: 2
    );
    
    // 3. Find newly discovered chunks
    var discoveryMask = _discoveryService.GetMask(character.Id, request.ZoneId);
    var newChunks = visibleChunks
        .Where(c => ! discoveryMask.IsChunkDiscovered(c))
        .ToList();
    
    if (newChunks.Count == 0)
        return; // No update needed
    
    // 4. Update discovery
    foreach (var chunk in newChunks)
    {
        discoveryMask.SetChunkDiscovered(chunk);
    }
    _discoveryService.Save(discoveryMask);
    
    // 5. Send update
    SendToClient(character, new MapExploreUpdate
    {
        ZoneId = request.ZoneId,
        DiscoveredChunkIds = newChunks,
        NewRevision = discoveryMask. Revision
    });
    
    // 6. Check for named area discovery
    CheckAreaDiscovery(character, request.ZoneId, newChunks);
}
```

### Flow-Diagramm

```
Client                         Server
  │                              │
  │  [Player moves]              │
  │  [Enter new chunk area]      │
  │                              │
  │  MapExplore (3400)           │
  │  ZoneId: 1001                │
  │  Position:  (500, 300)        │
  │─────────────────────────────►│
  │                              │
  │                              │  Calculate visible chunks
  │                              │  Chunks: [18, 19, 26, 27]
  │                              │  
  │                              │  Check discovery mask
  │                              │  New:  [26, 27]
  │                              │
  │  MapExploreUpdate (3401)     │
  │  DiscoveredChunkIds: [26,27] │
  │  NewRevision: 15             │
  │◄─────────────────────────────│
  │                              │
  │  [Update fog of war]         │
```

---

## MapExploreUpdate (3401)

**Richtung:** 📥 Server → Client  
**Frequenz:** Mittel (bei neuen Entdeckungen)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Server informiert Client über neu entdeckte Map-Chunks.

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.MapExploreUpdate` | Ja |
| ZoneId | ushort | Zone der Entdeckung | Ja |
| DiscoveredChunkIds | List\<int\> | Neu entdeckte Chunk-IDs | Ja |
| NewRevision | uint | Neue Discovery-Revision | Ja |
| TotalDiscovered | int | Gesamtzahl entdeckter Chunks | Ja |
| DiscoveryPercent | float | Prozent der Zone entdeckt | Ja |

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.MapExploreUpdate)]
public class MapExploreUpdate : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.MapExploreUpdate;
    [Key(1)] public ushort ZoneId { get; set; }
    [Key(2)] public List<int> DiscoveredChunkIds { get; set; } = new();
    [Key(3)] public uint NewRevision { get; set; }
    [Key(4)] public int TotalDiscovered { get; set; }
    [Key(5)] public float DiscoveryPercent { get; set; }
}
```

---

## MapFogReveal (3402)

**Richtung:** 📥 Server → Client (Event)  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Server

### Beschreibung

Server-initiiertes Aufdecken von Fog of War (z.B. durch Quest, Item, oder GM).

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.MapFogReveal` | Ja |
| ZoneId | ushort | Betroffene Zone | Ja |
| ChunkIds | List\<int\> | Aufzudeckende Chunks | Ja |
| Source | FogRevealSource | Grund der Aufdeckung | Ja |
| NewRevision | uint | Neue Revision | Ja |

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.MapFogReveal)]
public class MapFogReveal : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.MapFogReveal;
    [Key(1)] public ushort ZoneId { get; set; }
    [Key(2)] public List<int> ChunkIds { get; set; } = new();
    [Key(3)] public FogRevealSource Source { get; set; }
    [Key(4)] public uint NewRevision { get; set; }
}

public enum FogRevealSource :  byte
{
    Exploration = 0,
    Quest = 1,
    Item = 2,
    GM = 3,
    Achievement = 4,
    PartyShare = 5
}
```

---

## MapExploreResponse (3403)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten (nur bei Fehler)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Fehler-Response auf `MapExplore`.

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.MapExploreResponse` | Ja |
| Success | bool | Erfolgreich?  | Ja |
| ErrorCode | MapErrorCode | Fehlercode | Bei Fehler |

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.MapExploreResponse)]
public class MapExploreResponse :  IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.MapExploreResponse;
    [Key(1)] public bool Success { get; set; }
    [Key(2)] public MapErrorCode ErrorCode { get; set; }
}
```

---

## WaypointSet (3410)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Client setzt einen persönlichen Waypoint zur Navigation.

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.WaypointSet` | Ja |
| ZoneId | ushort | Ziel-Zone | Ja |
| Position | Position | Ziel-Position | Ja |
| Label | string?  | Optionaler Name (max.  32 Zeichen) | Nein |
| ReplaceExisting | bool | Vorhandenen Waypoint ersetzen | Nein |

### Erwartete Response
- `WaypointSetResponse` (3411)

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.WaypointSet)]
public class WaypointSet : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.WaypointSet;
    [Key(1)] public ushort ZoneId { get; set; }
    [Key(2)] public Position Position { get; set; }
    [Key(3)] public string?  Label { get; set; }
    [Key(4)] public bool ReplaceExisting { get; set; }
}
```

---

## WaypointSetResponse (3411)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Antwort auf `WaypointSet`.

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.WaypointSetResponse` | Ja |
| Success | bool | Erfolgreich? | Ja |
| ErrorCode | MapErrorCode | Fehlercode | Bei Fehler |
| Waypoint | WaypointDto?  | Der gesetzte Waypoint | Bei Erfolg |

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.WaypointSetResponse)]
public class WaypointSetResponse : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.WaypointSetResponse;
    [Key(1)] public bool Success { get; set; }
    [Key(2)] public MapErrorCode ErrorCode { get; set; }
    [Key(3)] public WaypointDto?  Waypoint { get; set; }
}
```

---

## WaypointClear (3412)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Client entfernt den aktuellen Waypoint.

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.WaypointClear` | Ja |
| WaypointId | Guid?  | Spezifischer Waypoint (null = aktiver) | Nein |

### Erwartete Response
- `WaypointClearResponse` (3413)

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.WaypointClear)]
public class WaypointClear : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.WaypointClear;
    [Key(1)] public Guid?  WaypointId { get; set; }
}
```

---

## WaypointClearResponse (3413)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Antwort auf `WaypointClear`.

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.WaypointClearResponse` | Ja |
| Success | bool | Erfolgreich? | Ja |
| ErrorCode | MapErrorCode | Fehlercode | Bei Fehler |
| ClearedWaypointId | Guid? | Entfernter Waypoint | Bei Erfolg |

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.WaypointClearResponse)]
public class WaypointClearResponse : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.WaypointClearResponse;
    [Key(1)] public bool Success { get; set; }
    [Key(2)] public MapErrorCode ErrorCode { get; set; }
    [Key(3)] public Guid? ClearedWaypointId { get; set; }
}
```

---

## WaypointShare (3414)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Client teilt einen Waypoint mit Party/Raid/Guild.

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.WaypointShare` | Ja |
| WaypointId | Guid | Zu teilender Waypoint | Ja |
| ShareTarget | ShareTarget | Party, Raid, Guild | Ja |

### Erwartete Response
- `WaypointShareResponse` (3415)

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.WaypointShare)]
public class WaypointShare : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.WaypointShare;
    [Key(1)] public Guid WaypointId { get; set; }
    [Key(2)] public ShareTarget ShareTarget { get; set; }
}

public enum ShareTarget : byte
{
    Party = 0,
    Raid = 1,
    Guild = 2
}
```

---

## WaypointShareResponse (3415)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Antwort auf `WaypointShare`.

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.WaypointShareResponse` | Ja |
| Success | bool | Erfolgreich?  | Ja |
| ErrorCode | MapErrorCode | Fehlercode | Bei Fehler |
| SharedWith | int | Anzahl Empfänger | Bei Erfolg |

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.WaypointShareResponse)]
public class WaypointShareResponse : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.WaypointShareResponse;
    [Key(1)] public bool Success { get; set; }
    [Key(2)] public MapErrorCode ErrorCode { get; set; }
    [Key(3)] public int SharedWith { get; set; }
}
```

---

## WaypointAccept (3416)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Client akzeptiert einen geteilten Waypoint.

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.WaypointAccept` | Ja |
| WaypointId | Guid | Geteilter Waypoint | Ja |

### Erwartete Response
- `WaypointAcceptResponse` (3417)

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.WaypointAccept)]
public class WaypointAccept : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.WaypointAccept;
    [Key(1)] public Guid WaypointId { get; set; }
}
```

---

## WaypointAcceptResponse (3417)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Antwort auf `WaypointAccept`.

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.WaypointAcceptResponse` | Ja |
| Success | bool | Erfolgreich?  | Ja |
| ErrorCode | MapErrorCode | Fehlercode | Bei Fehler |
| Waypoint | WaypointDto?  | Akzeptierter Waypoint | Bei Erfolg |

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.WaypointAcceptResponse)]
public class WaypointAcceptResponse : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.WaypointAcceptResponse;
    [Key(1)] public bool Success { get; set; }
    [Key(2)] public MapErrorCode ErrorCode { get; set; }
    [Key(3)] public WaypointDto? Waypoint { get; set; }
}
```

---

## WaypointUpdatedEvent (3418)

**Richtung:** 📥 Server → Client (Event/Broadcast)  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Server informiert über einen neuen/aktualisierten geteilten Waypoint.

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.WaypointUpdatedEvent` | Ja |
| Waypoint | WaypointDto | Der Waypoint | Ja |
| Action | WaypointAction | Created, Updated, Deleted | Ja |
| SharedBy | long | Character-ID des Teilenden | Ja |
| SharedByName | string | Character-Name | Ja |

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.WaypointUpdatedEvent)]
public class WaypointUpdatedEvent : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.WaypointUpdatedEvent;
    [Key(1)] public WaypointDto Waypoint { get; set; } = new();
    [Key(2)] public WaypointAction Action { get; set; }
    [Key(3)] public long SharedBy { get; set; }
    [Key(4)] public string SharedByName { get; set; } = "";
}

public enum WaypointAction : byte
{
    Created = 0,
    Updated = 1,
    Deleted = 2
}
```

---

## PingMap (3420)

**Richtung:** 📤 Client → Server  
**Frequenz:** Mittel  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Client pingt eine Position auf der Karte für Kommunikation mit Party/Raid. 

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.PingMap` | Ja |
| ZoneId | ushort | Zone des Pings | Ja |
| Position | Position | Ping-Position | Ja |
| PingType | PingType | Art des Pings | Ja |

### Erwartete Response
- `PingMapResponse` (3421)

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.PingMap)]
public class PingMap :  IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.PingMap;
    [Key(1)] public ushort ZoneId { get; set; }
    [Key(2)] public Position Position { get; set; }
    [Key(3)] public PingType PingType { get; set; }
}
```

### Server-Verhalten

1. Rate Limit prüfen
2. Party/Raid-Mitgliedschaft prüfen
3. Ping an alle relevanten Clients broadcasten
4. PingMapResponse an Sender

---

## PingMapResponse (3421)

**Richtung:** 📥 Server → Client  
**Frequenz:** Mittel  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Antwort auf `PingMap`.

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.PingMapResponse` | Ja |
| Success | bool | Erfolgreich? | Ja |
| ErrorCode | MapErrorCode | Fehlercode | Bei Fehler |
| PingId | Guid?  | Ping-ID | Bei Erfolg |

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.PingMapResponse)]
public class PingMapResponse : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.PingMapResponse;
    [Key(1)] public bool Success { get; set; }
    [Key(2)] public MapErrorCode ErrorCode { get; set; }
    [Key(3)] public Guid? PingId { get; set; }
}
```

---

## PingMapEvent (3422)

**Richtung:** 📥 Server → Client (Broadcast)  
**Frequenz:** Mittel  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Server broadcastet einen Ping an Party/Raid-Mitglieder.

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.PingMapEvent` | Ja |
| Ping | PingDto | Ping-Daten | Ja |

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.PingMapEvent)]
public class PingMapEvent : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType. PingMapEvent;
    [Key(1)] public PingDto Ping { get; set; } = new();
}
```

---

## FlightpathDiscover (3430)

**Richtung:** 📥 Server → Client (Event)  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Server informiert über einen neu entdeckten Flugpunkt.

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.FlightpathDiscover` | Ja |
| Node | FlightpathNodeDto | Entdeckter Flugpunkt | Ja |
| Source | DiscoverySource | NPC, Quest, Auto | Ja |

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.FlightpathDiscover)]
public class FlightpathDiscover : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.FlightpathDiscover;
    [Key(1)] public FlightpathNodeDto Node { get; set; } = new();
    [Key(2)] public DiscoverySource Source { get; set; }
}

public enum DiscoverySource : byte
{
    NPC = 0,
    Quest = 1,
    Auto = 2,
    GM = 3
}
```

---

## FlightpathListRequest (3431)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Client fordert Liste aller Flugpunkte (entdeckt + verfügbar) an.

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.FlightpathListRequest` | Ja |
| ZoneId | ushort?  | Nur für bestimmte Zone (null = alle) | Nein |
| IncludeUndiscovered | bool | Auch unentdeckte anzeigen | Nein |

### Erwartete Response
- `FlightpathListResponse` (3432)

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.FlightpathListRequest)]
public class FlightpathListRequest : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.FlightpathListRequest;
    [Key(1)] public ushort? ZoneId { get; set; }
    [Key(2)] public bool IncludeUndiscovered { get; set; }
}
```

---

## FlightpathListResponse (3432)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifiz