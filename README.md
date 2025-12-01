# 2DMMO

Ein einfaches 2D-Multiplayer-Online-Spiel, entwickelt mit Godot 4 (C#) als Client und .NET 10 als Server.

## 🎯 Projektziel

Erstellung eines funktionsfähigen Online-Multiplayer-Prototyps mit:
- Echtzeit-Spielerbewegung
- Mehrspieler-Synchronisation
- Persistenz (geplant)
- Chat-System (geplant)

## 🛠️ Tech-Stack

| Komponente | Technologie |
|-----------|-------------|
| **Server** | .NET 10, C# 14 |
| **Client** | Godot 4.3, C# 14, .NET 10 |
| **Shared Code** | .NET 10 Class Library |
| **Kommunikation** | TCP/WebSocket (geplant) |
| **Persistenz** | JSON/SQLite (geplant) |
| **Cloud** | Azure (optional, geplant) |

## 📁 Projektstruktur

```
2DMMO/
├── client/                    # Godot Client
│   └── GodotProject/
│       ├── project.godot      # Godot Projektdatei
│       ├── GodotProject.csproj
│       ├── assets/            # Game Assets
│       │   ├── sprites/       # 2D Sprites und Texturen
│       │   ├── audio/         # Sound-Effekte und Musik
│       │   ├── fonts/         # Schriftarten
│       │   ├── ui/            # UI-Elemente
│       │   └── shaders/       # Shader-Dateien
│       ├── scenes/            # Godot Szenen (.tscn)
│       └── scripts/           # C# Scripts
├── server/                    # .NET Server
│   └── Mmo.Server/
│       ├── Mmo.Server.csproj
│       └── Program.cs
├── shared/                    # Shared Code Library
│   └── Mmo.Shared/
│       ├── Mmo.Shared.csproj
│       └── SharedConstants.cs
├── tests/                     # Unit Tests
│   └── Mmo.Server.Tests/
├── docs/                      # Dokumentation
├── Mmo.sln                    # .NET Solution
└── .github/workflows/         # CI/CD Pipelines
```

## 🚀 Schnellstart

### Voraussetzungen

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Godot 4.3 .NET Edition](https://godotengine.org/download)

### Server starten

```bash
# Repository klonen
git clone https://github.com/MatTrinkl/2DMMO.git
cd 2DMMO

# Abhängigkeiten wiederherstellen
dotnet restore

# Server bauen
dotnet build

# Server starten
dotnet run --project server/Mmo.Server
```

### Client starten

1. Godot 4.3 (.NET Edition) öffnen
2. Projekt importieren: `client/GodotProject/project.godot`
3. F5 drücken oder Play-Button klicken

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
# Komplettes Projekt bauen
dotnet build Mmo.sln

# Release-Build
dotnet build Mmo.sln -c Release
```

### Code-Qualität

```bash
# Code formatieren
dotnet format

# Code-Analyse durchführen
dotnet build -warnaserror
```

### Mutation Tests

```bash
# Stryker.NET installieren (einmalig)
dotnet tool install -g dotnet-stryker

# Mutation Tests ausführen
cd tests/Mmo.Server.Tests
dotnet stryker
```

## 🧪 CI/CD

Die GitHub Actions Pipeline führt automatisch aus:

- ✅ Build-Verifikation
- ✅ Unit Tests
- ✅ Code-Formatierung
- ✅ Mutation Tests (Stryker.NET)

### Server-Mocking für Tests

Für Unit- und Integrationstests kann der Server gemockt werden:

```csharp
// Beispiel mit Moq
var mockServer = new Mock<IGameServer>();
mockServer.Setup(s => s.ProcessMessage(It.IsAny<INetworkMessage>()))
          .Returns(new SuccessResponse());

// Oder mit einem Test-Double
var testServer = new InMemoryGameServer();
```

**Geplante Test-Infrastruktur:**
- `IGameServer` Interface für Dependency Injection
- `InMemoryGameServer` für schnelle Unit-Tests ohne Netzwerk
- Integration Tests mit echtem TCP/WebSocket (optional)

## 📋 Issue-Tracking

Siehe [Issues](https://github.com/MatTrinkl/2DMMO/issues) für aktuelle Aufgaben und die Projekt-Roadmap.

## 🤝 Mitwirken

1. Fork erstellen
2. Feature-Branch erstellen (`git checkout -b feature/AmazingFeature`)
3. Änderungen committen (`git commit -m 'Add some AmazingFeature'`)
4. Branch pushen (`git push origin feature/AmazingFeature`)
5. Pull Request erstellen

## 📄 Lizenz

Dieses Projekt ist privat und nicht für die öffentliche Nutzung freigegeben.

## 📞 Kontakt

- GitHub: [@MatTrinkl](https://github.com/MatTrinkl)
