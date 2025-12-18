# 🎮 2DMMO

![CI](https://github.com/MatTrinkl/2DMMO/actions/workflows/ci.yml/badge.svg)

A 2D top-down MMO in a high-fantasy world, developed with Godot 4 (C#) as client and .NET 10 as server.

> *Inspired by classics like World of Warcraft and Guild Wars – in charming pixel art style.*

## 🎯 Project Goal

Creation of a scalable online multiplayer game with:
- Real-time player movement & synchronization
- Classic MMO gameplay (Tank/Healer/DPS)
- Diverse races and classes
- Persistent game world
- Zone-based sharding for unlimited player counts

## 📚 Documentation

> **Note:** The documentation has been restructured. See [docs/README.md](docs/README.md) for the complete overview.

### Quick Access

| Document | Description |
|----------|--------------|
| [📚 Documentation Index](docs/README.md) | Central overview of all documents |
| [🎮 Game Design Document](docs/01-overview/GAME_DESIGN_DOCUMENT.md) | Gameplay, races, classes, systems |
| [🏗️ Architecture](docs/02-architecture/README.md) | Technical architecture, network, database |
| [🛠️ Technical Design](docs/03-technical-details/TECHNICAL_DESIGN.md) | Detailed technical decisions |
| [🎯 Prototype Scope](docs/01-overview/PROTOTYPE_SCOPE.md) | What the prototype must be able to do |

## 🛠️ Tech-Stack

| Komponente | Technologie | Version | Links |
|-----------|-------------|---------|-------|
| **Client** | Godot Engine (.NET Edition) | 4.3 | [Docs](https://docs.godotengine.org/) |
| **Server** | .NET | 10 | [Docs](https://learn.microsoft.com/en-us/dotnet/) |
| **Language** | C# | 14 | [Docs](https://learn.microsoft.com/en-us/dotnet/csharp/) |
| **Transport** | TCP + TLS | - | - |
| **Serialization** | MessagePack | Latest | [GitHub](https://github.com/MessagePack-CSharp/MessagePack-CSharp) |
| **Cache** | Redis | 7+ | [Docs](https://redis.io/docs/) |
| **Database** | PostgreSQL | 16+ | [Docs](https://www.postgresql.org/docs/) |
| **Cloud** | Microsoft Azure | - | [Docs](https://learn.microsoft.com/en-us/azure/) |
| **Logging** | Grafana Cloud | - | [Docs](https://grafana.com/docs/grafana-cloud/) |

## 📁 Project Structure

```
2DMMO/
├── client/                     # Godot Client
│   └── GodotProject/
│       ├── project.godot       # Godot project file
│       ├── GodotProject.csproj
│       ├── assets/             # Game Assets
│       │   ├── sprites/        # 2D Sprites (64x64 pixel art)
│       │   ├── audio/          # Sound effects and music
│       │   ├── fonts/          # Fonts
│       │   ├── ui/             # UI elements
│       │   └── shaders/        # Shader files
│       ├── scenes/             # Godot scenes (.tscn)
│       └── scripts/            # C# Scripts
│           └── Networking/     # Client-side networking
├── server/                     # .NET Server
│   └── Mmo.Server/
│       ├── Mmo.Server.csproj
│       ├── Program.cs
│       ├── Networking/         # TCP Server, connection handling
│       ├── GameLoop/           # 25Hz game loop
│       └── Zones/              # Zone management
├── shared/                     # Shared code library
│   └── Mmo.Shared/
│       ├── Mmo.Shared.csproj
│       ├── Messages/           # Network messages (MessagePack)
│       ├── Enums/              # Shared enums
│       └── Constants/          # Shared constants
├── tests/                      # Unit & integration tests
│   ├── Mmo.Shared.Tests/
│   └── Mmo.Server.Tests/
├── docs/                       # Documentation
│   ├── README.md               # Documentation index
│   ├── 01-overview/            # High-level project information
│   │   ├── GAME_DESIGN_DOCUMENT.md
│   │   ├── PROTOTYPE_SCOPE.md
│   │   └── ASSETS.md
│   ├── 02-architecture/        # Technical architecture
│   │   ├── README.md           # Architecture overview
│   │   ├── SERVER_COMPONENTS.md
│   │   ├── NETWORK_PROTOCOL.md
│   │   └── ...                 # More architecture documents
│   ├── 03-technical-details/   # Implementation details
│   │   └── TECHNICAL_DESIGN.md
│   └── 04-project-management/  # Issue tracking & planning
│       ├── ISSUE_HIERARCHY.md
│       └── ISSUES_ROADMAP.md
├── Mmo.sln                     # .NET Solution
└── .github/workflows/          # CI/CD Pipelines
```

## 🚀 Quick Start

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Godot 4.3 .NET Edition](https://godotengine.org/download)
- (Optional) [Redis](https://redis.io/) for caching
- (Optional) [PostgreSQL](https://www.postgresql.org/) for persistence

### Start Server

```bash
# Clone repository
git clone https://github.com/MatTrinkl/2DMMO.git
cd 2DMMO

# Restore dependencies
dotnet restore

# Build and start server
dotnet run --project server/Mmo.Server
```

### Start Client

1. Open Godot 4.3 (.NET Edition)
2. Import project: `client/GodotProject/project.godot`
3. Build: `Project > Build` (or Ctrl+Shift+B)
4. Play: F5 or Play button

### Run Tests

```bash
# Run all tests
dotnet test

# With detailed output
dotnet test --verbosity normal
```

## 🔧 Development

### Build

```bash
# Debug build
dotnet build Mmo.sln

# Release build
dotnet build Mmo.sln -c Release
```

### Code Quality

```bash
# Format code
dotnet format

# Code analysis
dotnet build -warnaserror
```

## 🎮 Current Status

### Phase 1: Prototype (Current)

**Goal:** Two players connect, see each other, can move

- [x] Set up project structure
- [x] Documentation (GDD, architecture, technical design)
- [ ] TCP server with MessagePack
- [ ] Client-server connection
- [ ] Synchronize player movement
- [ ] Simple tilemap world
- [ ] Basic chat

See [PROTOTYPE_SCOPE.md](docs/01-overview/PROTOTYPE_SCOPE.md) for details.

## 🧪 CI/CD

GitHub Actions Pipeline:
- ✅ Build verification
- ✅ Unit tests (parallel)
- ✅ Integration tests (parallel)
- ✅ Code formatting

## 🔗 Useful Links

### Development
- [.NET Documentation](https://learn.microsoft.com/en-us/dotnet/)
- [Godot 4 Documentation](https://docs.godotengine.org/en/stable/)
- [Godot C# Documentation](https://docs.godotengine.org/en/stable/tutorials/scripting/c_sharp/index.html)
- [MessagePack-CSharp](https://github.com/MessagePack-CSharp/MessagePack-CSharp)

### Networking
- [Gabriel Gambetta - Client-Server Game Architecture](https://www.gabrielgambetta.com/client-server-game-architecture.html)
- [Valve Source Multiplayer Networking](https://developer.valvesoftware.com/wiki/Source_Multiplayer_Networking)
- [Glenn Fiedler - Networked Physics](https://gafferongames.com/post/networked_physics_2004/)

### Testing
- [xUnit](https://xunit.net/)
- [GdUnit4 (Godot Testing)](https://mikeschulze.github.io/gdUnit4/)
- [Stryker.NET (Mutation Testing)](https://stryker-mutator.io/docs/stryker-net/introduction/)

### Monitoring
- [Grafana Cloud](https://grafana.com/products/cloud/)
- [Serilog](https://serilog.net/)

### Assets
- [Kenney.nl](https://kenney.nl/assets)
- [OpenGameArt](https://opengameart.org/)
- [itch.io Free Assets](https://itch.io/game-assets/free)

## 📋 Issue Tracking

See [Issues](https://github.com/MatTrinkl/2DMMO/issues) for current tasks.

## 📄 License

This project is private and not released for public use.

## 📞 Contact

- GitHub: [@MatTrinkl](https://github.com/MatTrinkl)

---

*Developed with ❤️ and lots of coffee*