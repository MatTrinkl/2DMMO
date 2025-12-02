# 🎮 2DMMO

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

| Dokument | Beschreibung |
|----------|--------------|
| [Game Design Document](docs/GAME_DESIGN_DOCUMENT.md) | Gameplay, Rassen, Klassen, Systeme |
| [Architektur](docs/ARCHITECTURE.md) | Technische Architektur, Netzwerk, Datenbank |
| [Prototyp-Scope](docs/PROTOTYPE_SCOPE.md) | Was der Prototyp können muss |

## 🛠️ Tech-Stack

| Komponente | Technologie | Version |
|-----------|-------------|---------|
| **Client** | Godot Engine (.NET Edition) | 4.3 |
| **Server** | .NET | 10 |
| **Sprache** | C# | 14 |
| **Transport** | TCP + TLS | - |
| **Serialisierung** | MessagePack | Latest |
| **Cache** | Redis | 7+ |
| **Datenbank** | PostgreSQL | 16+ |
| **Cloud** | Microsoft Azure | Germany West Central |

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
│       ├── GameLoop/           # 30Hz Game Loop
│       └── Zones/              # Zone Management
├── shared/                     # Shared Code Library
│   └── Mmo.Shared/
│       ├── Mmo.Shared.csproj
│       ├── Messages/           # Network Messages (MessagePack)
│       ├── Enums/              # Shared Enums
│       └── Constants/          # Shared Constants
├── tests/                      # Unit Tests
│   └── Mmo.Server.Tests/
├── docs/                       # Dokumentation
│   ├── GAME_DESIGN_DOCUMENT.md
│   ├── ARCHITECTURE.md
│   └── PROTOTYPE_SCOPE.md
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
- [x] Dokumentation (GDD, Architektur)
- [ ] TCP Server mit MessagePack
- [ ] Client-Server Verbindung
- [ ] Spieler-Bewegung synchronisieren
- [ ] Einfache Tilemap-Welt
- [ ] Basis-Chat

Siehe [PROTOTYPE_SCOPE.md](docs/PROTOTYPE_SCOPE.md) für Details.

## 🧪 CI/CD

GitHub Actions Pipeline:
- ✅ Build-Verifikation
- ✅ Unit Tests
- ✅ Code-Formatierung

## 📋 Issue-Tracking

Siehe [Issues](https://github.com/MatTrinkl/2DMMO/issues) für aktuelle Aufgaben.

## 📄 Lizenz

Dieses Projekt ist privat und nicht für die öffentliche Nutzung freigegeben.

## 📞 Kontakt

- GitHub: [@MatTrinkl](https://github.com/MatTrinkl)

---

*Entwickelt mit ❤️ und viel Kaffee*