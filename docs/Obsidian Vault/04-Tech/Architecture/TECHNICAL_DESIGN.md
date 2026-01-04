# 🛠️ Technical Design Document

## 2DMMO – Technisches Design

**Version:** 1.2.0  
**Letzte Aktualisierung:** 2025-12-09  
**Status:** Prototyp-Phase

---

## 📋 Inhaltsverzeichnis

1. [Übersicht](#übersicht)
2. [Server Game Loop](#server-game-loop)
3. [Client Szenen-Struktur](#client-szenen-struktur)
4. [Thread-Modell](#thread-modell)
5. [Collision & Prediction](#collision--prediction)
6. [Entity-System](#entity-system)
7. [Zone-Definition & Statische Entities](#zone-definition--statische-entities)
8. [Netzwerk-Flow](#netzwerk-flow)
9. [Logging & Monitoring](#logging--monitoring)
10. [Testing-Strategie](#testing-strategie)
11. [Zukünftige Themen](#zukünftige-themen)
12. [Nützliche Links](#nützliche-links)
13. [Entscheidungslog](#entscheidungslog)

---

## Übersicht

Dieses Dokument beschreibt die technischen Architektur-Entscheidungen für das 2DMMO Projekt. Es dient als Referenz für die Entwicklung und wird kontinuierlich aktualisiert.

**Verwandte Dokumente:**  
- [Message-Referenz](../API/Message-Reference.md)  
- [Message-Spezifikation](MESSAGES.md)  
- [Netzwerk-Protokoll](NETWORK_PROTOCOL.md)  
- [Game Design Document](../../02-Game-Design/Systems/GAME_DESIGN_DOCUMENT.md)

### Tech-Stack

| Komponente | Technologie | Links |
|------------|-------------|-------|
| **Server** | C# / .NET 10 | [.NET Docs](https://learn.microsoft.com/en-us/dotnet/) |
| **Client** | Godot 4.3 + C# | [Godot Docs](https://docs.godotengine.org/en/stable/) |
| **Shared** | C# Class Library | - |
| **Serialization** | MessagePack | [MessagePack-CSharp](https://github.com/MessagePack-CSharp/MessagePack-CSharp) |
| **Protokoll** | TCP | - |
| **Testing** | xUnit + Moq + GdUnit4 | [xUnit](https://xunit.net/), [GdUnit4](https://mikeschulze.github.io/gdUnit4/) |
| **CI** | GitHub Actions | [Actions Docs](https://docs.github.com/en/actions) |
| **Logging** | Serilog + Grafana Cloud | [Grafana Cloud](https://grafana.com/products/cloud/) |

---

## Server Game Loop

> **📌 Siehe:** [Game Loop Design](GAME_LOOP.md) für vollständige Details zum Server Game Loop, Tick Timing und Phasen.

### Kurz-Zusammenfassung

| Aspekt | Wert |
|--------|------|
| **Tick-Rate** | 25 Hz (40ms pro Tick) |
| **Timing** | Fixed Timestep |
| **Phasen** | Input → Validation → Simulation → Broadcast → Persistence |

Die vollständige Dokumentation des Game Loop Designs, inklusive Code-Beispiele, Timing-Details und Best Practices, finden Sie in der [Game Loop Dokumentation](GAME_LOOP.md).

---

## Client Szenen-Struktur

### Szenen-Übersicht

```
client/GodotProject/scenes/
│
├── 🎮 Main.tscn                 # Einstiegspunkt
│   └── Lädt Login oder Game
│
├── 🔐 Login.tscn                # Login-Bildschirm
│   ├── Username-Eingabe
│   ├── Server-IP (optional)
│   └── Connect-Button
│
├── 🌍 Game.tscn                 # Hauptspiel-Szene
│   ├── TileMap (Welt)
│   ├── Camera2D
│   ├── Players (Container)
│   ├── UI (HUD)
│   └── Chat
│
├── 👤 Player.tscn               # Spieler-Prefab
│   ├── Sprite2D
│   └── Label (Username)
│
└── 💬 Chat.tscn                 # Chat-UI
    ├── MessageList
    └── InputField
```

### Szenen-Flow

```
┌──────────────┐
│   Main.tscn  │
└──────┬───────┘
       │
       ▼
┌──────────────┐
│  Login.tscn  │
│  [Username]  │
│  [Connect]   │
└──────┬───────┘
       │ LoginResponse (Success)
       ▼
┌──────────────┐
│  Game.tscn   │
│  TileMap     │
│  Players     │
│  Chat        │
└──────┬───────┘
       │ Disconnect
       ▼
┌──────────────┐
│  Login.tscn  │ ◄── Zurück zum Login
└──────────────┘
```

### Autoloads (Singletons)

| Autoload | Zweck |
|----------|-------|
| **NetworkManager** | TCP Connection, Send/Receive, Signale |
| **GameManager** | PlayerId, Players Dict, IsConnected |

### Map & Entity Layers

```
Layer 0: Ground (TileMapLayer)     → Gras, Wege (keine Collision)
Layer 1: Collision (TileMapLayer)  → Wasser, Bäume, Wände (MIT Collision)
Layer 2: Entities (Node2D)         → NPCs, Truhen, Türen (später)
Layer 3: Players (Node2D)          → Spieler-Instanzen
Layer 4: UI (CanvasLayer)          → HUD, Chat
```

---

## Thread-Modell

### Entscheidung: async/await

| Aspekt | Entscheidung |
|--------|--------------|
| **I/O Modell** | async/await |
| **Connections** | Async Tasks (ReadAsync/WriteAsync) |
| **Game Loop** | Dedizierter Thread mit Timer |
| **Kommunikation** | ConcurrentQueue<IncomingMessage> |
| **Locking** | Minimal (nur für shared state) |

### Server Thread-Architektur

```
┌─────────────────────────────────────────────────────────────────┐
│                    SERVER THREAD-MODELL                          │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  ┌──────────────────┐     ┌──────────────────┐                  │
│  │   Main Thread    │     │   Game Loop      │                  │
│  │   ────────────   │     │   ────────────   │                  │
│  │   • Program      │     │   • GameServer   │                  │
│  │   • Start        │────▶│   • Eigener      │                  │
│  │                  │     │     Thread       │                  │
│  └──────────────────┘     │   • 25 Hz Timer  │                  │
│                           │   • Tick()       │                  │
│                           └────────┬─────────┘                  │
│                                    │                            │
│                           ┌────────┴─────────┐                  │
│                           │  Message Queues  │                  │
│                           ├──────────────────┤                  │
│                           │ _incomingMessages│                  │
│                           │ _pendingBroadcasts│                 │
│                           └────────┬─────────┘                  │
│                                    │                            │
│                                    ▼                            │
│  ┌──────────────────────────────────────────────┐              │
│  │          NetworkServer (geplant)             │              │
│  │  ┌──────────────────────────────────────┐   │              │
│  │  │       TcpListener Thread             │   │              │
│  │  │  • AcceptAsync()                     │   │              │
│  │  │  • Neue Connections erstellen        │   │              │
│  │  └──────────────────────────────────────┘   │              │
│  │                    │                         │              │
│  │          ┌─────────┴─────────┐               │              │
│  │          │                   │               │              │
│  │  ┌───────▼─────────┐  ┌──────▼────────┐     │              │
│  │  │  ClientConn 1   │  │ ClientConn 2  │ ... │              │
│  │  │  (async Task)   │  │ (async Task)  │     │              │
│  │  ├─────────────────┤  ├───────────────┤     │              │
│  │  │ • ReadAsync()   │  │ • ReadAsync() │     │              │
│  │  │ • WriteAsync()  │  │ • WriteAsync()│     │              │
│  │  │ • SendQueue     │  │ • SendQueue   │     │              │
│  │  └─────────────────┘  └───────────────┘     │              │
│  └──────────────────────────────────────────────┘              │
│                                                                  │
│  💡 Connections auf ThreadPool (wenige Threads)                 │
│  💡 Game Loop auf dediziertem Thread                            │
│  💡 ConcurrentQueues für Thread-sichere Kommunikation           │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

### Event-Flow zwischen Komponenten

```
┌─────────────────────────────────────────────────────────────────┐
│                      EVENT-FLOW DIAGRAMM                         │
│                                                                  │
│  Client 1        ClientConnection      NetworkServer  GameServer│
│    │                    │                    │            │      │
│    │──── Packet ───────▶│                    │            │      │
│    │                    │── Deserialize ────▶│            │      │
│    │                    │                    │            │      │
│    │                    │   ┌────────────────┴──────┐     │      │
│    │                    │   │ IncomingMessages      │     │      │
│    │                    │   │ ConcurrentQueue       │     │      │
│    │                    │   └────────────────┬──────┘     │      │
│    │                    │                    │            │      │
│    │                    │                    │◄─ Dequeue ─│      │
│    │                    │                    │   (Input   │      │
│    │                    │                    │    Phase)  │      │
│    │                    │                    │            │      │
│    │                    │                    │            │──┐   │
│    │                    │                    │            │  │   │
│    │                    │                    │            │ ◀┘   │
│    │                    │                    │            │ Update│
│    │                    │                    │            │ Phase│
│    │                    │                    │            │      │
│    │                    │                    │            │──┐   │
│    │                    │                    │            │  │   │
│    │                    │   ┌────────────────┴──────┐    │ ◀┘   │
│    │                    │   │ PendingBroadcasts     │◄───│ Output│
│    │                    │   │ ConcurrentQueue       │    │ Phase│
│    │                    │   └────────────────┬──────┘    │      │
│    │                    │                    │            │      │
│    │                    │◄─── Dequeue ───────│            │      │
│    │                    │     (Network       │            │      │
│    │                    │      Thread)       │            │      │
│    │◄─── Packet ────────│                    │            │      │
│    │                    │                    │            │      │
└─────────────────────────────────────────────────────────────────┘
```

### Klassen-Übersicht

| Klasse | Status | Thread | Verantwortung |
|--------|--------|--------|---------------|
| **GameServer** | ✅ Implementiert | Dedizierter Thread | Game Loop (25 Hz), Input/Update/Output Phasen |
| **NetworkServer** | 🔄 Geplant | ThreadPool (TcpListener) | TCP-Listener, Connection-Management, Events |
| **ClientConnection** | 🔄 Geplant | ThreadPool (pro Client) | Read/Write Loop, SendQueue, Deserialisierung |
| **ZoneManager** | ✅ Implementiert | Game Loop Thread | Zone-State, Entity-Management |
| **MessageSerializer** | ✅ Implementiert | Beide | Attribute-basierte Auto-Registrierung (Dictionary, O(1) Lookup, Compiled Delegates) |

### NetworkEvents (Geplant)

Die Kommunikation zwischen NetworkServer und GameServer erfolgt über Events:

#### Event-Definitionen

```csharp
// NetworkServer definiert diese Events
public class NetworkServer
{
    public event EventHandler<ClientConnectedEventArgs>? ClientConnected;
    public event EventHandler<ClientDisconnectedEventArgs>? ClientDisconnected;
    public event EventHandler<MessageReceivedEventArgs>? MessageReceived;
    public event EventHandler<NetworkErrorEventArgs>? NetworkError;
}
```

#### EventArgs-Klassen

**ClientConnectedEventArgs:**
```csharp
public class ClientConnectedEventArgs : EventArgs
{
    public Guid ConnectionId { get; set; }
    public string RemoteEndpoint { get; set; }
    public DateTime ConnectedAt { get; set; }
}
```

**ClientDisconnectedEventArgs:**
```csharp
public class ClientDisconnectedEventArgs : EventArgs
{
    public Guid ConnectionId { get; set; }
    public Guid? PlayerId { get; set; }  // null wenn noch nicht eingeloggt
    public DisconnectReason Reason { get; set; }
    public DateTime DisconnectedAt { get; set; }
}
```

**MessageReceivedEventArgs:**
```csharp
public class MessageReceivedEventArgs : EventArgs
{
    public Guid ConnectionId { get; set; }
    public INetworkMessage Message { get; set; }
    public long Timestamp { get; set; }
}
```

**NetworkErrorEventArgs:**
```csharp
public class NetworkErrorEventArgs : EventArgs
{
    public Guid ConnectionId { get; set; }
    public Exception Exception { get; set; }
    public ErrorSeverity Severity { get; set; }
}

public enum ErrorSeverity : byte
{
    Warning = 1,   // Kann ignoriert werden
    Error = 2,     // Sollte geloggt werden
    Critical = 3   // Führt zu Disconnect
}
```

#### DisconnectReason Enum

```csharp
/// <summary>
/// Grund für einen Client-Disconnect
/// </summary>
public enum DisconnectReason : byte
{
    // Client-Initiated (1-9)
    ClientDisconnect = 1,      // Client hat normal getrennt
    ClientTimeout = 2,          // Client antwortet nicht mehr (Heartbeat Timeout)
    
    // Server-Initiated (10-19)
    ServerShutdown = 10,        // Server fährt herunter
    Kicked = 11,                // Von Admin gekickt
    Banned = 12,                // Gebannt
    
    // Error Cases (20-29)
    ProtocolError = 20,         // Ungültige Message
    AuthenticationFailed = 21,  // Login fehlgeschlagen
    DuplicateConnection = 22,   // Spieler bereits verbunden
    
    // Network Issues (30-39)
    ConnectionLost = 30,        // TCP-Verbindung verloren
    ReadError = 31,             // Fehler beim Lesen
    WriteError = 32,            // Fehler beim Schreiben
}
```

#### Event-Handler Beispiele

**GameServer Event-Handling:**

```csharp
public class GameServer
{
    public GameServer(NetworkServer networkServer)
    {
        // Events abonnieren
        networkServer.ClientConnected += OnClientConnected;
        networkServer.ClientDisconnected += OnClientDisconnected;
        networkServer.MessageReceived += OnMessageReceived;
        networkServer.NetworkError += OnNetworkError;
    }
    
    private void OnClientConnected(object? sender, ClientConnectedEventArgs e)
    {
        _log.Info("Client connected: {ConnectionId} from {Endpoint}",
            e.ConnectionId, e.RemoteEndpoint);
    }
    
    private void OnClientDisconnected(object? sender, ClientDisconnectedEventArgs e)
    {
        _log.Info("Client disconnected: {ConnectionId}, Reason: {Reason}",
            e.ConnectionId, e.Reason);
            
        // In Queue für nächsten Tick
        _incomingMessages.Enqueue(new IncomingMessage
        {
            ConnectionId = e.ConnectionId,
            Message = new Disconnect(e.PlayerId ?? Guid.Empty),
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        });
    }
    
    private void OnMessageReceived(object? sender, MessageReceivedEventArgs e)
    {
        // In Queue für nächsten Tick
        _incomingMessages.Enqueue(new IncomingMessage
        {
            ConnectionId = e.ConnectionId,
            Message = e.Message,
            Timestamp = e.Timestamp
        });
    }
    
    private void OnNetworkError(object? sender, NetworkErrorEventArgs e)
    {
        _log.Error(e.Exception, "Network error for {ConnectionId}: {Severity}",
            e.ConnectionId, e.Severity);
            
        if (e.Severity == ErrorSeverity.Critical)
        {
            // Connection wird automatisch getrennt
        }
    }
}
```

---

## Collision & Prediction

### Client-Side Prediction

```
┌─────────────────────────────────────────────────────────────────┐
│                 CLIENT-SIDE PREDICTION                           │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  OHNE Prediction (laggy):                                        │
│  ────────────────────────                                        │
│  Client ──Input──▶ Server ──Process──▶ Client (100ms+ Delay)    │
│                                                                  │
│  MIT Prediction (smooth):                                        │
│  ─────────────────────────                                       │
│  Client ──Input──▶ Server                                        │
│     │ (bewegt sofort!)        ──Process──▶                      │
│     └──────────────────────────────────── Client (Confirm)      │
│                                                                  │
│  ✅ Spieler fühlt SOFORTIGE Reaktion                            │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

### Server Reconciliation

Wenn Client und Server nicht übereinstimmen:

1. Client predicted Position (z.B. X=10)
2. Server validiert → Position ungültig (Wand bei X=8)
3. Server sendet korrigierte Position (X=7)
4. Client korrigiert (Reconciliation)
5. Client re-applied pending inputs

### Collision-Strategie

| Wo | Was | Warum |
|----|-----|-------|
| **Client** | Prediction-Collision | Sofortige Reaktion |
| **Server** | Authoritative Collision | Anti-Cheat, finale Entscheidung |
| **Shared** | CollisionData | Gleiche Daten für beide |

### Shared Collision Data

```csharp
// shared/Mmo.Shared/Collision/CollisionData.cs

public class CollisionData
{
    private readonly bool[,] _walkable;
    public int TileSize { get; } = 64;
    
    public bool IsWalkable(int tileX, int tileY) { ... }
    public (int x, int y) WorldToTile(float worldX, float worldY) { ... }
    public bool CanMoveTo(float fromX, float fromY, float toX, float toY) { ... }
}
```

---

## Entity-System

> **Hinweis:** Für die vollständige ID-System-Architektur siehe [ID-System Dokumentation](ID_SYSTEM.md).

### Entity-Hierarchie

```
                    ┌───────────┐
                    │  IEntity  │
                    │───────────│
                    │ • Id      │
                    │ • Type    │
                    │ • X, Y    │
                    └─────┬─────┘
                          │
          ┌───────────────┼───────────────┐
          │               │               │
          ▼               ▼               ▼
   ┌─────────────┐ ┌─────────────┐ ┌─────────────┐
   │   Player    │ │     NPC     │ │ Interactive │
   └─────────────┘ └─────────────┘ └──────┬──────┘
                                          │
                          ┌───────────────┼───────────────┐
                          ▼               ▼               ▼
                   ┌───────────┐   ┌───────────┐   ┌───────────┐
                   │   Chest   │   │   Door    │   │   Sign    │
                   └───────────┘   └───────────┘   └───────────┘
```

### Entity Types

```csharp
public enum EntityType : byte
{
    Player = 1,
    NpcVendor = 10,
    NpcQuestGiver = 11,
    Chest = 20,
    Door = 21,
    Sign = 22,
    Portal = 23,
}
```

### Interaktions-Flow

```
Client                              Server
  │                                   │
  │ ─── InteractRequest ────────────▶ │
  │     { EntityId: chest-123 }       │
  │                                   │ Validiert:
  │                                   │ • Spieler nah genug?
  │                                   │ • Entity existiert?
  │                                   │
  │ ◀─── InteractResponse ─────────── │
  │     { Success: true, Loot: ... }  │
  │                                   │
  │ ◀─── EntityUpdated ────────────── │
  │     { IsOpen: true }              │
```

### Client Entity Szenen

```
client/GodotProject/scenes/entities/
├── Entity.tscn          # Basis-Szene
├── Chest.tscn           # Truhe
├── Door.tscn            # Tür
├── Sign.tscn            # Schild
└── NpcVendor.tscn       # Händler NPC
```

---

## Zone-Definition & Statische Entities

Zone-Definitionen beschreiben alle statischen Entities einer Zone (Türen, Portale, Kisten, Schilder, NPCs, Spawnpoints). Diese werden als **JSON-Dateien** gespeichert und von Server UND Client über die `Mmo.Shared` Library geladen.

### Datenformat-Entscheidungen

| Aspekt | Entscheidung | Begründung |
|--------|--------------|------------|
| **Format** | JSON | Menschenlesbar, versionierbar in Git |
| **Speicherort** | `/data/zones/*.zone.json` | Zentral, von Server + Client nutzbar |
| **Shared DTOs** | `Mmo.Shared.Zones` Namespace | Keine Duplikation |
| **Loader** | `ZoneDefinitionLoader` (Shared) | Einheitliche Ladelogik |

### Entity-ID Bereiche

> **Hinweis:** Dieses Dokument beschreibt eine frühe Version des ID-Systems. Für die aktuelle, umfassende ID-System-Architektur siehe [ID-System Dokumentation](ID_SYSTEM.md), die EntityIdentity, ZoneId, ShardId, GlobalKey und persistente vs. Runtime IDs behandelt.

| Bereich | Verwendung |
|---------|------------|
| 1 - 999 | Player (auto-increment) |
| 1000 - 1999 | Statische Entities (Türen, Portale, Kisten, Schilder) |
| 2000 - 2999 | Dynamische Mobs |
| 3000+ | Reserviert für zukünftige Erweiterungen |

### Zone-Definition Struktur

```
ZoneDefinition
├── ZoneId (string)
├── ZoneName (string)
├── Bounds
│   ├── MinX, MaxX, MinY, MaxY
├── SpawnPoints[]
│   ├── Id (string)
│   ├── X, Y (float)
│   ├── Type (SpawnPointType)
│   └── IsDefault (bool)
├── StaticEntities[]
│   ├── Id (int)
│   ├── Type (EntityType)
│   ├── X, Y (float)
│   └── Properties (Dictionary<string, object>)
```

### Beispiel Zone-Definition (JSON)

```json
{
  "zoneId": "startzone",
  "zoneName": "Startzone",
  "bounds": {
    "minX": 0,
    "maxX": 2048,
    "minY": 0,
    "maxY": 2048
  },
  "spawnPoints": [
    {
      "id": "default-spawn",
      "x": 1024,
      "y": 1024,
      "type": "PlayerSpawn",
      "isDefault": true
    }
  ],
  "staticEntities": [
    {
      "id": 1000,
      "type": "Door",
      "x": 512,
      "y": 768,
      "properties": {
        "targetZone": "Hauptstadt",
        "targetSpawn": "from-startzone",
        "requiredKey": null
      }
    },
    {
      "id": 1001,
      "type": "Sign",
      "x": 1024,
      "y": 900,
      "properties": {
        "text": "Willkommen in der Startzone!"
      }
    }
  ]
}
```

### Loader-Implementierung

```csharp
// shared/Mmo.Shared/Zones/ZoneDefinitionLoader.cs

public class ZoneDefinitionLoader
{
    public static ZoneDefinition LoadFromFile(string filePath)
    {
        string json = File.ReadAllText(filePath);
        return JsonSerializer.Deserialize<ZoneDefinition>(json);
    }
    
    public static Dictionary<string, ZoneDefinition> LoadAllZones(string zonesDirectory)
    {
        var zones = new Dictionary<string, ZoneDefinition>();
        foreach (var file in Directory.GetFiles(zonesDirectory, "*.zone.json"))
        {
            var zone = LoadFromFile(file);
            zones[zone.ZoneId] = zone;
        }
        return zones;
    }
}
```

### Verwendung auf Server und Client

**Server (Mmo.Server):**
```csharp
// Beim Start: Zonen laden
var zones = ZoneDefinitionLoader.LoadAllZones("data/zones");
foreach (var (zoneId, definition) in zones)
{
    var zone = new Zone(definition);
    zoneManager.AddZone(zone);
}
```

**Client (Godot):**
```csharp
// Beim Zone-Wechsel: Statische Entities spawnen
var definition = ZoneDefinitionLoader.LoadFromFile("res://data/zones/startzone.zone.json");
foreach (var entity in definition.StaticEntities)
{
    SpawnStaticEntity(entity);
}
```

> **Hinweis:** In der Implementierung sollte Error-Handling für fehlende Dateien und ungültiges JSON hinzugefügt werden.

---

## Netzwerk-Flow

### Connection Lifecycle

```
┌─────────────────────────────────────────────────────────────────┐
│                 CONNECTION LIFECYCLE                             │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  1️⃣ CONNECT       → TCP Connect + TLS Handshake                 │
│  2️⃣ LOGIN         → LoginRequest/Response                       │
│  3️⃣ INITIAL STATE → WorldState mit allen Spielern/Entities     │
│  4️⃣ GAME LOOP     → PositionUpdates, WorldState (25 Hz)        │
│  5️⃣ DISCONNECT    → Cleanup, PlayerLeft broadcast              │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

### Timeout-Konfiguration

| Timeout | Wert | Beschreibung |
|---------|------|--------------|
| **CONNECTION_TIMEOUT** | 10s | Max. Zeit für Login |
| **HEARTBEAT_INTERVAL** | 5s | Client sendet Ping |
| **HEARTBEAT_TIMEOUT** | 15s | Kein Ping → Disconnect |
| **RECONNECT_WINDOW** | 30s | State bleibt für Reconnect |

### Reconnection-Flow

```
1. Client verliert Verbindung
2. Server startet RECONNECT_WINDOW (30s)
3. Client versucht Reconnect mit ReconnectToken
4. Bei Erfolg: Spieler an alter Position
5. Bei Timeout: State wird gelöscht
```

### Retry-Strategie

Exponential Backoff:
- Versuch 1: Sofort
- Versuch 2: 1s
- Versuch 3: 2s
- Versuch 4: 4s
- ...
- Maximum: 30s (Cap)
- Max Versuche: 10

### Error Codes

```csharp
public enum ErrorCode : byte
{
    // Connection (1-19)
    ServerFull = 1,
    InvalidVersion = 3,
    
    // Authentication (20-39)
    InvalidUsername = 20,
    SessionExpired = 23,
    
    // Game Logic (40-59)
    TooFarAway = 41,
    TargetNotFound = 43,
    
    // Server (60-79)
    ServerShuttingDown = 60,
}
```

### Connection State Machine

```
DISCONNECTED ──Connect()──▶ CONNECTING
                                │
                    Timeout ◀───┴───▶ TCP Connected
                      │                    │
                      ▼                    ▼
               DISCONNECTED          AUTHENTICATING
                                          │
                          Login Failed ◀──┴──▶ Login Success
                               │                    │
                               ▼                    ▼
                         DISCONNECTED          CONNECTED
                                                   │
                                    Connection Lost│
                                                   ▼
                                            RECONNECTING
                                                   │
                              Max Retries ◀────────┴────────▶ Reconnected
                                   │                              │
                                   ▼                              │
                             DISCONNECTED ◀───────────────────────┘
```

---

## Logging & Monitoring

### Log-Level Strategie

| Level | Beschreibung | Wann nutzen |
|-------|--------------|-------------|
| **TRACE** | Jeder Tick, jede Message | Nur lokal bei Debugging |
| **DEBUG** | State Changes, Flow | Development |
| **INFO** | Login, Disconnect, wichtige Events | Production Standard |
| **WARNING** | Timeouts, Retries, Anomalien | Sollte untersucht werden |
| **ERROR** | Fehler, Server läuft weiter | Muss gefixt werden |
| **FATAL** | Kritisch, Server stoppt | Sofortige Aktion |

### Was loggen wir?

**Server INFO:**
- Server gestartet/gestoppt
- Spieler Login/Logout
- Zone geladen

**Server WARNING:**
- Spieler Timeout
- Tick dauerte > 40ms
- Ungültige Nachricht

**Server ERROR:**
- Deserialisierung fehlgeschlagen
- Exception in Game Loop

### Metriken

| Kategorie | Metrik | Beschreibung |
|-----------|--------|--------------|
| **Performance** | tick_duration_ms | Tick-Dauer |
| **Performance** | tick_overrun_count | Ticks > 40ms |
| **Connections** | players_online | Aktuelle Spieler |
| **Connections** | logins_per_minute | Login-Rate |
| **Network** | messages_per_second | Nachrichten/s |
| **Network** | bytes_in/out_per_second | Bandbreite |

### Grafana Cloud Integration

**Gewählte Plattform:** [Grafana Cloud](https://grafana.com/products/cloud/)

| Aspekt | Details |
|--------|---------|
| **Free Tier** | 50 GB Logs, 10k Metriken, 3 Users |
| **Features** | Logs + Metriken + Dashboards + Alerting |
| **Integration** | Serilog + OpenTelemetry |

**Setup:**

```csharp
// NuGet: Serilog, Serilog.Sinks.Grafana.Loki

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.GrafanaLoki("https://logs-prod-eu-west-0.grafana.net", 
        labels: new[] { new LokiLabel { Key = "app", Value = "mmo-server" } },
        credentials: new LokiCredentials { Login = "user", Password = "api-key" })
    .Enrich.WithProperty("Application", "Mmo.Server")
    .CreateLogger();
```

---

## Testing-Strategie

### Test-Pyramide

```
                    ┌─────────┐
                    │  E2E    │  ← Manuell (2 Clients)
                   ┌┴─────────┴┐
                   │Integration│  ← Server + Fake Client
                  ┌┴───────────┴┐
                  │  Unit Tests │  ← Logik, Serialization
                 ┌┴─────────────┴┐
                 │   Schnell!     │
                 └────────────────┘
```

### Test-Projekte

| Projekt | Framework | Fokus |
|---------|-----------|-------|
| **Mmo.Shared.Tests** | xUnit + Moq + FluentAssertions | MessageSerializer, DTOs |
| **Mmo.Server.Tests** | xUnit + Moq + FluentAssertions | GameLoop, State, Networking |
| **Mmo.Client.Tests** | GdUnit4 | NetworkManager, Game Logic |

### CI Pipeline

```
                      ┌─────────────┐
                      │    BUILD    │
                      └──────┬──────┘
                             │
              ┌──────────────┴──────────────┐
              │                             │
              ▼                             ▼
     ┌─────────────────┐          ┌─────────────────┐
     │   UNIT TESTS    │          │ INTEGRATION     │
     │   (parallel)    │          │ TESTS (parallel)│
     └─────────────────┘          └─────────────────┘
```

### Test-Entscheidungen

| Aspekt | Entscheidung |
|--------|--------------|
| **Framework** | xUnit |
| **Mocking** | Moq |
| **Assertions** | FluentAssertions |
| **Godot Tests** | GdUnit4 |
| **CI** | GitHub Actions (von Anfang an) |
| **CD** | Später |
| **Mutation Tests** | Stryker.NET (bald) |
| **Load Tests** | Später |

---

## Zukünftige Themen

### Zone-Konzept (Phase 2+)

- Startzone Design
- Zone-Übergänge
- Zone-Sharding bei hoher Last

### Mutation Tests (Stryker.NET)

```
Integration geplant:
├── Package: dotnet-stryker
├── Config: stryker-config.json
├── CI Job: Separater Job (läuft länger)
└── Report: Mutation Score Dashboard
```

### Load Tests

- Simulated Clients
- Stress Testing
- Performance Benchmarks

---

## Nützliche Links

### Offizielle Dokumentation

| Thema | Link |
|-------|------|
| **.NET** | [learn.microsoft.com/dotnet](https://learn.microsoft.com/en-us/dotnet/) |
| **Godot 4** | [docs.godotengine.org](https://docs.godotengine.org/en/stable/) |
| **Godot C#** | [Godot C# Docs](https://docs.godotengine.org/en/stable/tutorials/scripting/c_sharp/index.html) |
| **MessagePack** | [github.com/MessagePack-CSharp](https://github.com/MessagePack-CSharp/MessagePack-CSharp) |

### Testing

| Thema | Link |
|-------|------|
| **xUnit** | [xunit.net](https://xunit.net/) |
| **Moq** | [github.com/moq](https://github.com/moq/moq4) |
| **FluentAssertions** | [fluentassertions.com](https://fluentassertions.com/) |
| **GdUnit4** | [mikeschulze.github.io/gdUnit4](https://mikeschulze.github.io/gdUnit4/) |
| **Stryker.NET** | [stryker-mutator.io](https://stryker-mutator.io/docs/stryker-net/introduction/) |

### Logging & Monitoring

| Thema | Link |
|-------|------|
| **Grafana Cloud** | [grafana.com/products/cloud](https://grafana.com/products/cloud/) |
| **Grafana Loki** | [grafana.com/oss/loki](https://grafana.com/oss/loki/) |
| **Serilog** | [serilog.net](https://serilog.net/) |
| **OpenTelemetry .NET** | [opentelemetry.io/docs/instrumentation/net](https://opentelemetry.io/docs/instrumentation/net/) |

### Netzwerk & Game Dev

| Thema | Link |
|-------|------|
| **Game Networking** | [Gabriel Gambetta - Fast-Paced Multiplayer](https://www.gabrielgambetta.com/client-server-game-architecture.html) |
| **Client-Side Prediction** | [Valve Source Multiplayer Networking](https://developer.valvesoftware.com/wiki/Source_Multiplayer_Networking) |
| **Entity Interpolation** | [Glenn Fiedler - Networked Physics](https://gafferongames.com/post/networked_physics_2004/) |

### Assets (für Prototyp)

| Thema | Link |
|-------|------|
| **OpenGameArt** | [opengameart.org](https://opengameart.org/) |
| **Kenney Assets** | [kenney.nl/assets](https://kenney.nl/assets) |
| **itch.io Free** | [itch.io/game-assets/free](https://itch.io/game-assets/free) |

---

## Entscheidungslog

### 2025-12-02

| Entscheidung | Wert | Begründung |
|--------------|------|------------|
| Tick-Rate | 25 Hz | Balance zwischen Responsiveness und Performance |
| Timing | Fixed Timestep | Konsistente Spiellogik, reproduzierbar |
| WorldState (jetzt) | Jeden Tick komplett | Einfacher für Prototyp |
| WorldState (später) | Delta + Full-Sync | Bandbreiten-Optimierung |
| Player-Node | Node2D | Server validiert, keine Client-Physik nötig |
| Camera | Folgt Spieler | Standard für Top-Down Games |
| Thread-Modell | async/await | Skaliert gut, modern |
| Game Loop Thread | Dediziert | Konsistentes Timing |
| Message Queue | ConcurrentQueue | Thread-safe, einfach |
| Test Framework | xUnit | Standard für .NET |
| CI | Sofort | Qualität von Anfang an |
| CD | Später | Nicht nötig für Prototyp |
| Logging Platform | Grafana Cloud | Logs + Metriken + Dashboards, großzügiger Free Tier |
| Prediction | Client-Side | Sofortige Reaktion, Server validiert |
| Collision | Shared Data | Gleiche Logik Client + Server |
| Entity System | IEntity Interface | Flexibel, erweiterbar |
| Reconnect Window | 30 Sekunden | Gute Balance UX vs. Ressourcen |
| Retry Strategy | Exponential Backoff | Standard, verhindert Server-Überlastung |

---

*Dieses Dokument wird kontinuierlich aktualisiert.*

Source: docs/03-technical-details/TECHNICAL_DESIGN.md
