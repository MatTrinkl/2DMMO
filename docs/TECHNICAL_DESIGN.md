# 🛠️ Technical Design Document

## 2DMMO – Technisches Design

**Version:** 1.0.0  
**Letzte Aktualisierung:** 2025-12-02  
**Status:** Prototyp-Phase

---

## 📋 Inhaltsverzeichnis

1. [Übersicht](#übersicht)
2. [Server Game Loop](#server-game-loop)
3. [Client Szenen-Struktur](#client-szenen-struktur)
4. [Thread-Modell](#thread-modell)
5. [Testing-Strategie](#testing-strategie)
6. [Zukünftige Themen](#zukünftige-themen)
7. [Entscheidungslog](#entscheidungslog)

---

## Übersicht

Dieses Dokument beschreibt die technischen Architektur-Entscheidungen für das 2DMMO Projekt. Es dient als Referenz für die Entwicklung und wird kontinuierlich aktualisiert.

### Tech-Stack

| Komponente | Technologie |
|------------|-------------|
| **Server** | C# / .NET 10 |
| **Client** | Godot 4.3 + C# |
| **Shared** | C# Class Library |
| **Serialization** | MessagePack |
| **Protokoll** | TCP |
| **Testing** | xUnit + Moq + GdUnit4 |
| **CI** | GitHub Actions |

---

## Server Game Loop

### Tick-Rate & Timing

| Aspekt | Entscheidung |
|--------|--------------|
| **Tick-Rate** | 30 Hz (33.33ms pro Tick) |
| **Timing-Strategie** | Fixed Timestep |
| **Phasen pro Tick** | Input → Update → Output → Wait |

### Game Loop Architektur

```
┌─────────────────────────────────────────────────────────────────┐
│                     SERVER GAME LOOP (30 Hz)                    │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│   ┌─────────────┐    ┌─────────────┐    ┌─────────────┐        │
│   │   INPUT     │───▶│   UPDATE    │───▶│   OUTPUT    │        │
│   │   PHASE     │    │   PHASE     │    │   PHASE     │        │
│   └─────────────┘    └─────────────┘    └─────────────┘        │
│         │                  │                  │                 │
│         ▼                  ▼                  ▼                 │
│   ┌───────────┐      ┌───────────┐      ┌───────────┐          │
│   │ Message   │      │ Validate  │      │ Broadcast │          │
│   │ Queue     │      │ Movement  │      │ WorldState│          │
│   │ lesen     │      │ Update    │      │ an alle   │          │
│   └───────────┘      └───────────┘      └───────────┘          │
│                                                                 │
│   ◄──────────────── ~33.33ms pro Tick ─────────────────►       │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

### WorldState Broadcast

| Phase | Strategie |
|-------|-----------|
| **Prototyp (jetzt)** | Jeden Tick kompletten WorldState senden |
| **Optimierung (später)** | Delta-Updates + Full-Sync alle 30-60 Ticks |

### Tick-Ablauf im Detail

```
┌─────────────────────────────────────────────────────────────────┐
│                    EIN TICK (33.33ms)                            │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  1️⃣ INPUT PHASE (~5ms)                                          │
│     • MessageQueue auslesen                                      │
│     • LoginRequest → HandleLogin()                               │
│     • PositionUpdate → HandleMovement()                          │
│     • ChatMessage → HandleChat()                                 │
│                                                                  │
│  2️⃣ UPDATE PHASE (~5ms)                                         │
│     • Positionen validieren                                      │
│     • Kollisionen prüfen                                         │
│     • GameState aktualisieren                                    │
│     • CurrentTick++                                              │
│                                                                  │
│  3️⃣ OUTPUT PHASE (~10ms)                                        │
│     • WorldState zusammenstellen                                 │
│     • An alle Clients senden                                     │
│                                                                  │
│  4️⃣ WAIT                                                        │
│     • Restliche Zeit bis 33.33ms warten                          │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

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

### Szenen-Details

#### Login.tscn
```
Login (Control)
├── VBoxContainer
│   ├── Label "2DMMO"
│   ├── LineEdit (Username)
│   ├── LineEdit (ServerIP) [optional]
│   ├── Button "Verbinden"
│   └── Label (StatusText)
└── Script: Login.cs
```

#### Game.tscn
```
Game (Node2D)
├── TileMapLayer (Ground)
├── TileMapLayer (Collision)
├── Players (Node2D Container)
├── Camera2D (folgt lokalem Spieler)
├── CanvasLayer (UI)
│   ├── HUD
│   └── Chat.tscn
└── Script: Game.cs
```

#### Player.tscn
```
Player (Node2D)
├── Sprite2D (64x64)
├── Label (Username)
└── Script: Player.cs
    ├── PlayerId, Username, IsLocal
    └── UpdatePosition(x, y)
```

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
│                    ASYNC SERVER MODELL                           │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  ┌──────────────────┐     ┌──────────────────┐                  │
│  │   Main Thread    │     │   Game Loop      │                  │
│  │   ────────────   │     │   ────────────   │                  │
│  │   • Start        │     │   • Eigener      │                  │
│  │   • TcpListener  │     │     Thread       │                  │
│  │   • AcceptAsync  │     │   • 30 Hz Timer  │                  │
│  └────────┬─────────┘     │   • Tick()       │                  │
│           │               └──────────────────┘                  │
│           │                        ▲                            │
│           ▼                        │                            │
│  ┌──────────────────┐              │                            │
│  │  Connection 1    │──────────────┤                            │
│  │  (async Task)    │   Message    │                            │
│  ├──────────────────┤   Queue      │                            │
│  │  Connection 2    │──────────────┤                            │
│  │  (async Task)    │  (Thread-    │                            │
│  ├──────────────────┤   safe)      │                            │
│  │  Connection N    │──────────────┘                            │
│  │  (async Task)    │                                           │
│  └──────────────────┘                                           │
│                                                                  │
│  💡 Connections auf ThreadPool (wenige Threads)                 │
│  💡 Game Loop auf dediziertem Thread                            │
│  💡 ConcurrentQueue verbindet beide                             │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
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
┌─────────────────────────────────────────────────────────────────┐
│                      CI PIPELINE                                 │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│                      ┌─────────────┐                            │
│                      │    BUILD    │                            │
│                      │  (dotnet)   │                            │
│                      └──────┬──────┘                            │
│                             │                                    │
│              ┌──────────────┴──────────────┐                    │
│              │                             │                     │
│              ▼                             ▼                     │
│     ┌─────────────────┐          ┌─────────────────┐            │
│     │   UNIT TESTS    │          │ INTEGRATION     │            │
│     │   (parallel)    │          │ TESTS (parallel)│            │
│     │                 │          │                 │            │
│     │ Mmo.Shared.Tests│          │ Mmo.Server.Tests│            │
│     └─────────────────┘          └─────────────────┘            │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
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

### Collision & Prediction (Phase 2+)

```
Client-Side Prediction:
├── Lokaler Spieler bewegt sich sofort
├── Server validiert und korrigiert bei Abweichung
└── Reconciliation bei Differenz

Collision Detection:
├── Client: Prediction-Collision (sofortige Reaktion)
├── Server: Authoritative Collision (finale Entscheidung)
└── Shared: Gleiche Collision-Daten für beide

Idee - Shared Collision Data:
└── shared/Mmo.Shared/Maps/CollisionData.cs
    ├── bool[,] WalkableTiles
    └── IsWalkable(x, y)
```

### Entity-System (Phase 2+)

```
Statische Map:
├── TileMap mit Collision-Layer
└── Wände, Wasser, Bäume

Dynamische Entities:
├── NPCs, Truhen, Türen
├── Eigene Szene/Prefab pro Entity-Typ
├── Server sendet Entity-State
└── Interaktion: Client → "Interact" → Server validiert

Später:
└── Entity-Component-System (ECS) Pattern?
```

### Mutation Tests (Stryker.NET)

```
Integration geplant:
├── Package: dotnet-stryker
├── Config: stryker-config.json
├── CI Job: Separater Job (läuft länger)
└── Report: Mutation Score Dashboard
```

---

## Entscheidungslog

### 2025-12-02

| Entscheidung | Wert | Begründung |
|--------------|------|------------|
| Tick-Rate | 30 Hz | Balance zwischen Responsiveness und Performance |
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

---

*Dieses Dokument wird kontinuierlich aktualisiert.*