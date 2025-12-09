# 🎮 2DMMO

![CI](https://github.com/MatTrinkl/2DMMO/actions/workflows/ci.yml/badge.svg)

Ein 2D Top-Down MMO in einer High-Fantasy-Welt, entwickelt mit Godot 4 (C#) als Client und .NET 10 als Server.

> *Inspiriert von Klassikern wie World of Warcraft und Guild Wars – im charmanten Pixel-Art-Stil.*

## 🎯 Projektziel

Erstellung eines skalierbaren Online-Multiplayer-Spiels mit:
- Echtzeit-Spielerbewegung & Synchronisation
- Klassisches MMO-Gameplay (Tank/Healer/DPS)
- Vielfältige Rassen und Klassen
- Persistente Spielwelt
- Zone-basiertes Sharding für unbegrenzte Spielerzahlen

## 📚 Dokumentation

> **Hinweis:** Die Dokumentation wurde neu strukturiert. Siehe [docs/README.md](docs/README.md) für die vollständige Übersicht.

### Schnellzugriff

| Dokument | Beschreibung |
|----------|--------------|
| [📚 Dokumentations-Index](docs/README.md) | Zentrale Übersicht aller Dokumente |
| [🎮 Game Design Document](docs/01-overview/GAME_DESIGN_DOCUMENT.md) | Gameplay, Rassen, Klassen, Systeme |
| [🏗️ Architektur](docs/02-architecture/README.md) | Technische Architektur, Netzwerk, Datenbank |
| [🛠️ Technical Design](docs/03-technical-details/TECHNICAL_DESIGN.md) | Detaillierte technische Entscheidungen |
| [🎯 Prototyp-Scope](docs/01-overview/PROTOTYPE_SCOPE.md) | Was der Prototyp können muss |

## 🛠️ Tech-Stack

| Komponente | Technologie | Version | Links |
|-----------|-------------|---------|-------|
| **Client** | Godot Engine (.NET Edition) | 4.3 | [Docs](https://docs.godotengine.org/) |
| **Server** | .NET | 10 | [Docs](https://learn.microsoft.com/en-us/dotnet/) |
| **Sprache** | C# | 14 | [Docs](https://learn.microsoft.com/en-us/dotnet/csharp/) |
| **Transport** | TCP + TLS | - | - |
| **Serialisierung** | MessagePack | Latest | [GitHub](https://github.com/MessagePack-CSharp/MessagePack-CSharp) |
| **Cache** | Redis | 7+ | [Docs](https://redis.io/docs/) |
| **Datenbank** | PostgreSQL | 16+ | [Docs](https://www.postgresql.org/docs/) |
| **Cloud** | Microsoft Azure | - | [Docs](https://learn.microsoft.com/en-us/azure/) |
| **Logging** | Grafana Cloud | - | [Docs](https://grafana.com/docs/grafana-cloud/) |

## 📁 Projektstruktur

```
2DMMO/
├── client/                     # Godot Client
│   └── GodotProject/
│       ├── project.godot       # Godot Projektdatei
│       ├── GodotProject.csproj
│       ├── assets/             # Game Assets
│       │   ├── sprites/        # 2D Sprites (64x64 Pixel Art)
│       │   ├── audio/          # Sound-Effekte und Musik
│       │   ├── fonts/          # Schriftarten
│       │   ├── ui/             # UI-Elemente
│       │   └── shaders/        # Shader-Dateien
│       ├── scenes/             # Godot Szenen (.tscn)
│       └── scripts/            # C# Scripts
│           └── Networking/     # Client-side Networking
├── server/                     # .NET Server
│   └── Mmo.Server/
│       ├── Mmo.Server.csproj
│       ├── Program.cs
│       ├── Networking/         # TCP Server, Connection Handling
│       ├── GameLoop/           # 25Hz Game Loop
│       └── Zones/              # Zone Management
├── shared/                     # Shared Code Library
│   └── Mmo.Shared/
│       ├── Mmo.Shared.csproj
│       ├── Messages/           # Network Messages (MessagePack)
│       ├── Enums/              # Shared Enums
│       └── Constants/          # Shared Constants
├── tests/                      # Unit & Integration Tests
│   ├── Mmo.Shared.Tests/
│   └── Mmo.Server.Tests/
├── docs/                       # Dokumentation
│   ├── README.md               # Dokumentations-Index
│   ├── 01-overview/            # High-level Projektinformationen
│   │   ├── GAME_DESIGN_DOCUMENT.md
│   │   ├── PROTOTYPE_SCOPE.md
│   │   └── ASSETS.md
│   ├── 02-architecture/        # Technische Architektur
│   │   ├── README.md           # Architektur-Übersicht
│   │   ├── SERVER_COMPONENTS.md
│   │   ├── NETWORK_PROTOCOL.md
│   │   └── ...                 # weitere Architektur-Dokumente
│   ├── 03-technical-details/   # Implementierungs-Details
│   │   └── TECHNICAL_DESIGN.md
│   └── 04-project-management/  # Issue-Tracking & Planung
│       ├── ISSUE_HIERARCHY.md
│       └── ISSUES_ROADMAP.md
├── Mmo.sln                     # .NET Solution
└── .github/workflows/          # CI/CD Pipelines
```

## 🚀 Schnellstart

### Voraussetzungen

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Godot 4.3 .NET Edition](https://godotengine.org/download)
- (Optional) [Redis](https://redis.io/) für Caching
- (Optional) [PostgreSQL](https://www.postgresql.org/) für Persistenz

### Server starten

```bash
# Repository klonen
git clone https://github.com/MatTrinkl/2DMMO.git
cd 2DMMO

# Abhängigkeiten wiederherstellen
dotnet restore

# Server bauen und starten
dotnet run --project server/Mmo.Server
```

### Client starten

1. Godot 4.3 (.NET Edition) öffnen
2. Projekt importieren: `client/GodotProject/project.godot`
3. Build: `Projekt > Build` (oder Ctrl+Shift+B)
4. Play: F5 oder Play-Button

### Tests ausführen

```bash
# Alle Tests ausführen
dotnet test

# Mit detaillierter Ausgabe
dotnet test --verbosity normal
```

## 🔧 Entwicklung

### Build

```bash
# Debug-Build
dotnet build Mmo.sln

# Release-Build
dotnet build Mmo.sln -c Release
```

### Code-Qualität

```bash
# Code formatieren
dotnet format

# Code-Analyse
dotnet build -warnaserror
```

## 🎮 Aktueller Status

### Phase 1: Prototyp (Aktuell)

**Ziel:** Zwei Spieler verbinden sich, sehen sich, können sich bewegen

- [x] Projekt-Struktur aufsetzen
- [x] Dokumentation (GDD, Architektur, Technical Design)
- [ ] TCP Server mit MessagePack
- [ ] Client-Server Verbindung
- [ ] Spieler-Bewegung synchronisieren
- [ ] Einfache Tilemap-Welt
- [ ] Basis-Chat

Siehe [PROTOTYPE_SCOPE.md](docs/01-overview/PROTOTYPE_SCOPE.md) für Details.

## 🧪 CI/CD

GitHub Actions Pipeline:
- ✅ Build-Verifikation
- ✅ Unit Tests (parallel)
- ✅ Integration Tests (parallel)
- ✅ Code-Formatierung

## 🔗 Nützliche Links

### Entwicklung
- [.NET Dokumentation](https://learn.microsoft.com/en-us/dotnet/)
- [Godot 4 Dokumentation](https://docs.godotengine.org/en/stable/)
- [Godot C# Dokumentation](https://docs.godotengine.org/en/stable/tutorials/scripting/c_sharp/index.html)
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

## 📋 Issue-Tracking

Siehe [Issues](https://github.com/MatTrinkl/2DMMO/issues) für aktuelle Aufgaben.

## 📄 Lizenz

Dieses Projekt ist privat und nicht für die öffentliche Nutzung freigegeben.

## 📞 Kontakt

- GitHub: [@MatTrinkl](https://github.com/MatTrinkl)

---

*Entwickelt mit ❤️ und viel Kaffee*