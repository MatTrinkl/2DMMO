# 🎯 Prototype Scope

## 2DMMO – Phase 1: Prototype

**Version:** 1.2.0  
**Last Updated:** 2026-01-01  
**Status:** In Development

---

## 📋 Overview

This document defines the **exact scope** of the prototype. It serves as a clear boundary of what must be implemented and what is explicitly **not** part of the prototype.

### Prototype Goal

> **Two players can connect over the network, see each other in a simple 2D world, and can move around.**

### Development Environment

| Aspect | Prototype |
|--------|-----------|
| **Deployment** | Local only (localhost) |
| **Authentication** | Username only (no password) |
| **Database** | None (in-memory only) |
| **Redis** | Optional (can run without) |
| **TLS** | Optional/Self-signed |

---

## ✅ Must Be Implemented

### 1. Server Foundation

- [ ] TCP Server starts and accepts connections
- [ ] MessagePack serialization works
- [ ] Game loop runs at 25 Hz (tick rate)
- [ ] Connection management (connect/disconnect)
- [ ] Logging (console output is sufficient)

**Relevant Docs:**
- [.NET TCP Server](https://learn.microsoft.com/en-us/dotnet/fundamentals/networking/sockets/socket-services)
- [MessagePack-CSharp](https://github.com/MessagePack-CSharp/MessagePack-CSharp)

### 2. Client Foundation

- [ ] TCP Client connects to server
- [ ] MessagePack serialization works
- [ ] Godot scene loads and displays game world
- [ ] Input handling (WASD or arrow keys)
- [ ] Connection status display (Connected/Disconnected)

**Relevant Docs:**
- [Godot StreamPeerTCP](https://docs.godotengine.org/en/stable/classes/class_streampeertcp.html)
- [Godot Input Handling](https://docs.godotengine.org/en/stable/tutorials/inputs/index.html)

### 3. Shared Library

- [ ] Message types defined (Enum)
- [ ] DTOs for all messages (MessagePack attributes)
- [ ] Shared constants (port, tick rate, etc.)

### 4. Network Messages

| Message | Direction | Description |
|---------|-----------|-------------|
| `LoginRequest` | Client → Server | Send username |
| `LoginResponse` | Server → Client | Success + PlayerId |
| `PlayerJoined` | Server → Clients | New player has joined |
| `PlayerLeft` | Server → Clients | Player has left |
| `PositionUpdate` | Client → Server | Send own position |
| `WorldState` | Server → Clients | All player positions |
| `ChatMessageSend` | Client → Server | Send chat message |
| `ChatBroadcast` | Server → Clients | Receive chat message |

### 5. Player Movement

- [ ] Client sends input to server
- [ ] Server validates movement (basic check)
- [ ] Server broadcasts new positions
- [ ] Client displays other players at correct position
- [ ] Client-side prediction for smooth movement (optional but recommended)

**Relevant Docs:**
- [Gabriel Gambetta - Client-Side Prediction](https://www.gabrielgambetta.com/client-side-prediction-server-reconciliation.html)
- [Valve Multiplayer Networking](https://developer.valvesoftware.com/wiki/Source_Multiplayer_Networking)

### 6. Game World (Minimal)

- [ ] Simple tilemap (grass, maybe water/obstacles)
- [ ] Fixed size (e.g., 50x50 tiles)
- [ ] No zones, no transitions
- [ ] Player sprite (placeholder OK)

**Relevant Docs:**
- [Godot TileMap](https://docs.godotengine.org/en/stable/classes/class_tilemap.html)
- [Godot 2D Tutorial](https://docs.godotengine.org/en/stable/getting_started/first_2d_game/index.html)

### 7. Chat (Basic)

- [ ] Text input in client
- [ ] Send message to server
- [ ] Server broadcasts to all
- [ ] Chat display in client (simple list)

---

## ❌ Explicitly NOT in Prototype

### Gameplay
- ❌ Combat system
- ❌ NPCs / Monsters
- ❌ Quests
- ❌ Inventory / Items
- ❌ Race or class selection
- ❌ Leveling / XP
- ❌ Death / Respawn

### Technical
- ❌ Real authentication (email, password, OAuth)
- ❌ Database persistence (PostgreSQL)
- ❌ Redis integration
- ❌ TLS certificates (real)
- ❌ Cloud deployment (Azure)
- ❌ Zone sharding (code can be prepared, but not active)
- ❌ Load balancing
- ❌ Rate limiting
- ❌ Grafana Cloud integration (prepared but not active)

### Assets
- ❌ Final graphics
- ❌ Animations (except basic movement)
- ❌ Sound / Music
- ❌ UI design (functional is enough)

---

## 🎨 Placeholder Assets (Prototype)

For the prototype we use simple placeholders:

| Asset | Description | Source |
|-------|-------------|--------|
| **Player Sprite** | Simple colored rectangle or basic sprite | Self-created or free |
| **Tileset** | Simple grass tile, maybe water | Free assets |
| **Font** | Godot default or simple pixel font | Godot built-in |

**Possible Asset Sources:**
- [OpenGameArt.org](https://opengameart.org/)
- [Kenney.nl](https://kenney.nl/assets) ⭐ Recommended (CC0)
- [itch.io Free Assets](https://itch.io/game-assets/free)
- [Ninja Adventure Pack](https://pixel-boy.itch.io/ninja-adventure-asset-pack) - Complete package

---

## 📐 Technical Specifications

### Network

| Parameter | Value |
|-----------|-------|
| **Protocol** | TCP |
| **Port** | 7777 |
| **Serialization** | MessagePack |
| **Tick Rate** | 25 Hz |
| **Max Players (Test)** | 2-10 |

### Message Frame Format

```
┌──────────┬──────────┬─────────────────────────────┐
│  1 Byte  │  4 Bytes │       N Bytes               │
│   Type   │  Length  │       Payload               │
│          │ (uint32) │   (MessagePack Data)        │
└──────────┴──────────┴─────────────────────────────┘
```

### Client-Server Flow

```
┌────────────────────────────────────────────────────────┐
│                    PROTOTYPE FLOW                       │
│                                                         │
│  1. Client starts                                      │
│     └─► Shows "Enter username" dialog                  │
│                                                         │
│  2. Client connects                                    │
│     └─► TCP connect to localhost:7777                  │
│     └─► Sends LoginRequest { Username }                │
│                                                         │
│  3. Server accepts                                     │
│     └─► Creates PlayerId                               │
│     └─► Sends LoginResponse { Success, PlayerId }      │
│     └─► Broadcasts PlayerJoined to others              │
│                                                         │
│  4. Gameplay Loop                                      │
│     └─► Client: Input → PositionUpdate to Server       │
│     └─► Server: Validates, updates state               │
│     └─► Server: WorldState to all clients (25 Hz)      │
│     └─► Client: Renders all players                    │
│                                                         │
│  5. Chat                                               │
│     └─► Client: ChatMessage to Server                  │
│     └─► Server: Broadcasts to all                      │
│                                                         │
│  6. Disconnect                                         │
│     └─► Server: Removes player                         │
│     └─► Server: Broadcasts PlayerLeft                  │
└────────────────────────────────────────────────────────┘
```

---

## 🧪 Acceptance Criteria

The prototype is **complete** when:

### Must Have
1. ✅ Server starts without errors
2. ✅ Client connects successfully
3. ✅ Player sees their own character
4. ✅ Second player connects
5. ✅ Both players see each other
6. ✅ Movement of one player is visible to the other
7. ✅ Chat message arrives at the other player

### Should Have
8. ⬜ Movement feels smooth (no strong stuttering)
9. ⬜ Disconnect is handled cleanly
10. ⬜ Multiple clients (3+) work simultaneously

### Nice to Have
11. ⬜ Simple tilemap instead of empty area
12. ⬜ Player name above character
13. ⬜ Ping/latency display

---

## 📅 Estimated Tasks

| Task | Estimated Time | Dependencies |
|------|----------------|--------------|
| Shared Library (Messages, DTOs) | 2-3h | - |
| TCP Server Foundation | 3-4h | Shared |
| TCP Client Foundation | 3-4h | Shared |
| Login Flow | 2h | Server, Client |
| Position Sync | 4-5h | Login Flow |
| Godot Scene (Basic) | 2-3h | - |
| Player Rendering | 2-3h | Position Sync, Scene |
| Chat | 2h | Login Flow |
| Testing & Bugfixing | 3-4h | Everything |
| **Total** | **~25-30h** | - |

---

## 🚦 Definition of Done

A feature is **done** when:

- [ ] Code is written and compiles
- [ ] No obvious bugs
- [ ] Works with 2 clients simultaneously
- [ ] Code is in `main` branch

---

## 🔗 Useful Links

### Documentation
- [.NET Docs](https://learn.microsoft.com/en-us/dotnet/)
- [Godot 4 Docs](https://docs.godotengine.org/en/stable/)
- [MessagePack-CSharp](https://github.com/MessagePack-CSharp/MessagePack-CSharp)

### Networking
- [Gabriel Gambetta - Fast-Paced Multiplayer](https://www.gabrielgambetta.com/client-server-game-architecture.html)
- [Valve Source Multiplayer Networking](https://developer.valvesoftware.com/wiki/Source_Multiplayer_Networking)

### Testing
- [xUnit](https://xunit.net/)
- [GdUnit4](https://mikeschulze.github.io/gdUnit4/)

### Assets
- [Kenney.nl](https://kenney.nl/assets)
- [OpenGameArt](https://opengameart.org/)
- [itch.io Free](https://itch.io/game-assets/free)

---

## 📝 Notes

- **Focus on functionality, not perfection**
- Placeholder assets are OK
- Code quality is important, but avoid over-engineering
- Documentation can be minimal (code comments are sufficient)

---

*This document defines the scope. Everything outside is Phase 2+.*

Source: docs/01-overview/PROTOTYPE_SCOPE.md
