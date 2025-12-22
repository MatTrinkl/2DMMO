# Docker Testing Environment

## Übersicht

Diese Dokumentation beschreibt die dockerisierte Testumgebung für Multi-Client- und End-to-End-Tests des MMO-Projekts.

Die Testumgebung besteht aus:
- **Docker Container** für den MMO-Server
- **Eigenständige C# Test-Clients** (außerhalb von Godot)
- **Integration Tests** mit xUnit und FluentAssertions
- **GitHub Actions CI** für automatisierte Tests

## Voraussetzungen

- Docker 20.10+
- Docker Compose 2.0+
- 4GB+ RAM verfügbar für Docker
- .NET 10.0 SDK (für lokale Entwicklung)

## Quickstart

### Linux/Mac
```bash
chmod +x scripts/docker-test.sh
./scripts/docker-test.sh
```

### Windows
```powershell
.\scripts\docker-test.ps1
```

### Manuell mit Docker Compose
```bash
# Images bauen
docker-compose -f docker-compose.test.yml build

# Server starten
docker-compose -f docker-compose.test.yml up -d mmo-server

# Auf Server warten
docker-compose -f docker-compose.test.yml ps

# Integration Tests ausführen
docker-compose -f docker-compose.test.yml run --rm integration-tests

# Logs anzeigen
docker-compose -f docker-compose.test.yml logs -f mmo-server

# Alles stoppen
docker-compose -f docker-compose.test.yml down -v
```

## Architektur

### Komponenten

#### 1. MMO Server Container (`docker/Dockerfile.server`)
- Multi-stage Build mit .NET 10.0
- Exponiert Port 7777
- Health-Check über TCP-Verbindung
- Logs werden in `./logs/server` gespeichert

#### 2. Integration Test Container (`docker/Dockerfile.tests`)
- Enthält xUnit Tests
- Test-Clients verbinden sich zum Server
- Test-Ergebnisse in `./test-results`

#### 3. Test-Clients (C# Implementation)
- **Nicht** Godot-basiert (eigenständige .NET-Anwendung)
- Nutzt die `Mmo.Shared` Library
- TCP-Verbindung zum Server
- MessagePack-Serialisierung
- In `tests/Mmo.Integration.Tests/Utilities/TestClient.cs`

### Netzwerk

Alle Container laufen im gleichen Docker-Netzwerk (`mmo-test-network`):
```
┌─────────────────────────────────────┐
│   mmo-test-network (bridge)         │
│                                     │
│  ┌──────────────┐   ┌─────────────┐│
│  │ mmo-server   │   │ integration-││
│  │ :7777        │◄──┤ tests       ││
│  └──────────────┘   └─────────────┘│
└─────────────────────────────────────┘
```

## Konfiguration

### Environment Variables

| Variable | Default | Beschreibung |
|----------|---------|--------------|
| SERVER_HOST | mmo-server | Hostname des Servers (im Docker-Netzwerk) |
| SERVER_PORT | 7777 | Port des Servers |
| LOG_LEVEL | Debug | Log-Level (Testing-Umgebung) |
| ASPNETCORE_ENVIRONMENT | Testing | ASP.NET Core Umgebung |

### Anpassung

**docker-compose.test.yml** für eigene Konfiguration bearbeiten:

```yaml
services:
  mmo-server:
    environment:
      - LOG_LEVEL=Info  # Log-Level ändern
```

## Test-Kategorien

### Single Client Tests (`SingleClientTests.cs`)

Tests für einzelne Client-Szenarien:

- ✅ **Client_CanConnect_ToServer** - Verbindungsaufbau
- ✅ **Client_CanAuthenticate_WithValidCredentials** - Login
- ✅ **Client_CanSpawnCharacter_AfterLogin** - Character-Spawning
- ✅ **Client_ReceivesZoneState_AfterSpawn** - Zone State
- ✅ **Client_ReceivesHeartbeat_AfterConnection** - Heartbeat-Empfang
- ✅ **Client_CanDisconnect_Gracefully** - Graceful Disconnect

**Ausführen:**
```bash
docker-compose -f docker-compose.test.yml run --rm integration-tests \
  --filter FullyQualifiedName~SingleClientTests
```

### Multi Client Tests (`MultiClientTests.cs`)

Tests für Multi-Client-Szenarien:

- ✅ **TwoClients_CanConnectSimultaneously** - Gleichzeitige Verbindungen
- ✅ **Client_ReceivesPlayerJoined_WhenOtherConnects** - PlayerJoined Event
- ✅ **Client_ReceivesPlayerLeft_WhenOtherDisconnects** - PlayerLeft Event
- ✅ **MultipleClients_CanConnectSimultaneously** - 5+ gleichzeitige Clients
- ✅ **TwoClients_InSameZone_CanSeeEachOther** - Gegenseitige Sichtbarkeit
- ✅ **Clients_CanConnect_AndDisconnect_Rapidly** - Rapid Connect/Disconnect

**Ausführen:**
```bash
docker-compose -f docker-compose.test.yml run --rm integration-tests \
  --filter FullyQualifiedName~MultiClientTests
```

### Stress Tests (`StressTests.cs`)

⚠️ **Manuell ausführen** (standardmäßig übersprungen):

- 🔥 **Server_Handles_50ConcurrentClients** - 50 gleichzeitige Clients
- 🔥 **Server_Handles_RapidConnectDisconnect** - 100 schnelle Connects
- 🔥 **Server_Handles_100ConcurrentClients** - 100 gleichzeitige Clients
- 🔥 **Server_MaintainsStability_UnderContinuousLoad** - 60s Dauerlast

**Ausführen:**
```bash
# Alle Stress-Tests
docker-compose -f docker-compose.test.yml run --rm integration-tests \
  --filter FullyQualifiedName~StressTests

# Einzelner Test
docker-compose -f docker-compose.test.yml run --rm integration-tests \
  --filter FullyQualifiedName~Server_Handles_50ConcurrentClients
```

## CI Integration

Die Integration Tests laufen automatisch in GitHub Actions bei:
- Push auf `main`, `develop`, `ServiceImplementation`
- Pull Requests auf `main`

### Workflow: `.github/workflows/integration-tests.yml`

**Jobs:**
1. Build Docker Images
2. Start Server
3. Wait for Health
4. Run Integration Tests
5. Upload Artifacts (Logs, Test-Ergebnisse)

**Artifacts:**
- `integration-test-results` - Test-Ergebnisse und Logs (14 Tage)
- `server-logs-failed` - Server-Logs bei Fehlern (7 Tage)

## Entwicklung

### Lokale Entwicklung

```bash
# .NET SDK verwenden (schneller für Entwicklung)
cd tests/Mmo.Integration.Tests
dotnet test --filter FullyQualifiedName~SingleClientTests

# Server lokal starten
cd server/Mmo.Server
dotnet run
```

### Neue Tests hinzufügen

1. Test-Datei in `tests/Mmo.Integration.Tests/` erstellen
2. `[Collection("Integration")]` Attribut verwenden
3. `IAsyncLifetime` implementieren für Setup/Cleanup
4. `IntegrationTestFixture` und `TestClientFactory` nutzen

**Beispiel:**
```csharp
[Collection("Integration")]
public class MyNewTests : IAsyncLifetime
{
    private readonly IntegrationTestFixture _fixture;
    private TestClientFactory? _clientFactory;

    public MyNewTests()
    {
        _fixture = new IntegrationTestFixture();
    }

    public Task InitializeAsync()
    {
        _clientFactory = _fixture.CreateClientFactory();
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        _clientFactory?.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task MyTest()
    {
        var client = await _clientFactory!.CreateAuthenticatedClientAsync();
        // Test logic...
        client.Dispose();
    }
}
```

### Test-Client API

**TestClient** (`Utilities/TestClient.cs`):

```csharp
// Verbinden
var client = new TestClient("localhost", 7777);
await client.ConnectAsync(TimeSpan.FromSeconds(5));

// Login
var response = await client.LoginAsync("username", "password", timeout);

// Nachricht senden
await client.SendMessageAsync(new SomeMessage());

// Auf Nachricht warten
var msg = await client.WaitForMessageAsync<SomeMessageType>(timeout);

// Alle Nachrichten eines Typs abrufen
var messages = client.GetMessages<SomeMessageType>();

// Cleanup
client.Dispose();
```

**TestClientFactory** (`Utilities/TestClientFactory.cs`):

```csharp
var factory = new TestClientFactory();

// Einfacher Client
var client = factory.CreateClient();

// Verbundener Client
var client = await factory.CreateAndConnectAsync();

// Authentifizierter Client
var client = await factory.CreateAuthenticatedClientAsync();

// Mehrere Clients
var clients = await factory.CreateMultipleClientsAsync(5);

// Cleanup
factory.Dispose();
```

## Troubleshooting

### Server startet nicht

**Logs prüfen:**
```bash
docker-compose -f docker-compose.test.yml logs mmo-server
```

**Häufige Ursachen:**
- Port 7777 bereits belegt
- Firewall blockiert
- Nicht genug RAM

**Lösung:**
```bash
# Anderen Port verwenden
docker-compose -f docker-compose.test.yml down -v
# docker-compose.test.yml bearbeiten: "8888:7777"
```

### Tests schlagen fehl

**Alle Logs anzeigen:**
```bash
docker-compose -f docker-compose.test.yml logs
```

**Container-Status:**
```bash
docker-compose -f docker-compose.test.yml ps
```

**Tests lokal ausführen:**
```bash
# Server im Docker starten
docker-compose -f docker-compose.test.yml up -d mmo-server

# Tests lokal ausführen (mit Debugger)
cd tests/Mmo.Integration.Tests
SERVER_HOST=localhost dotnet test --logger "console;verbosity=detailed"
```

### Server wird nicht "healthy"

**Health-Check Status:**
```bash
docker inspect mmo-test-server | grep -A 10 Health
```

**Manueller Health-Check:**
```bash
docker exec mmo-test-server bash -c 'timeout 1 bash -c "</dev/tcp/localhost/7777"'
echo $?  # 0 = OK, 1 = Fehler
```

**Alternative Health-Check:**
```bash
# nc verwenden (wenn bash nicht funktioniert)
docker exec mmo-test-server nc -z localhost 7777
```

### Verbindung zum Server schlägt fehl

**Netzwerk prüfen:**
```bash
docker network inspect mmo-test-network
```

**Server von Test-Container erreichen:**
```bash
docker-compose -f docker-compose.test.yml run --rm integration-tests \
  bash -c "nc -zv mmo-server 7777"
```

### Docker Build schlägt fehl

**Cache löschen:**
```bash
docker-compose -f docker-compose.test.yml build --no-cache
```

**Alte Images entfernen:**
```bash
docker system prune -a
docker volume prune
```

## Performance

### Build-Zeiten optimieren

**Layer-Caching nutzen:**
- Dockerfiles verwenden bereits Multi-Stage-Builds
- `dotnet restore` wird separat ausgeführt
- Nur geänderte Dateien triggern Rebuild

**BuildKit aktivieren:**
```bash
export DOCKER_BUILDKIT=1
docker-compose -f docker-compose.test.yml build
```

### Test-Zeiten

| Test-Kategorie | Dauer | Beschreibung |
|----------------|-------|--------------|
| Single Client | ~30s | 6 Tests |
| Multi Client | ~60s | 6 Tests |
| Stress Tests | 5-10min | Manuell |
| **Total (CI)** | **~2min** | Ohne Stress-Tests |

## Best Practices

### Tests schreiben

✅ **DO:**
- Immer `Dispose()` für Clients aufrufen
- `TestClientFactory` verwenden für automatisches Cleanup
- Timeouts großzügig setzen (CI ist langsamer)
- FluentAssertions für lesbare Assertions
- Jeder Test sollte unabhängig sein

❌ **DON'T:**
- Tests voneinander abhängig machen
- Hardcoded Timeouts < 5 Sekunden
- Zustand zwischen Tests teilen
- Exceptions ignorieren

### Debugging

**Einzelnen Test mit Logs:**
```bash
docker-compose -f docker-compose.test.yml run --rm integration-tests \
  --filter FullyQualifiedName~Client_CanConnect_ToServer \
  --logger "console;verbosity=detailed"
```

**Server-Logs live verfolgen:**
```bash
docker-compose -f docker-compose.test.yml logs -f mmo-server
```

## Roadmap

### Phase 1 ✅ (Aktuell)
- [x] Docker-Setup für Server
- [x] Eigenständige C# Test-Clients
- [x] Single Client Tests
- [x] Multi Client Tests
- [x] Stress Tests
- [x] GitHub Actions CI

### Phase 2 (Zukünftig)
- [ ] Godot-Client in Docker (für UI-Tests)
- [ ] Performance-Metriken (Prometheus/Grafana)
- [ ] Automatische Last-Tests (z.B. jede Nacht)
- [ ] Datenbank-Container (wenn benötigt)
- [ ] Redis-Container (für Cross-Zone Events)

### Phase 3 (Optional)
- [ ] Kubernetes-Setup
- [ ] Load Balancer Tests
- [ ] Multi-Zone Tests
- [ ] Chaos Engineering

## Weitere Ressourcen

- [Docker Compose Dokumentation](https://docs.docker.com/compose/)
- [xUnit Dokumentation](https://xunit.net/)
- [FluentAssertions Dokumentation](https://fluentassertions.com/)
- [GitHub Actions Dokumentation](https://docs.github.com/actions)

---

**Bei Fragen oder Problemen:**
- Issue auf GitHub öffnen
- Logs und Container-Status mitschicken
- Fehlermeldungen vollständig inkludieren
