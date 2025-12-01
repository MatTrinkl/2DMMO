# 2DMMO Issue-Roadmap bis Phase 4

Diese Dokumentation enthält:
1. **Sub-Issues** für existierende Issues, die aufgeteilt werden sollten
2. **Neue Issues** für fehlende Funktionalität bis Ende Phase 4

---

## Teil 1: Sub-Issues für existierende Issues

### Issue #8: NetworkServer und ClientConnection-Skelett → Sub-Issues

Das ursprüngliche Issue #8 sollte in folgende Sub-Issues aufgeteilt werden:

---

#### Sub-Issue 8a: NetworkServer TCP-Listener implementieren

**Labels:** `type:feature`, `area:server`, `area:network`, `priority:p1`

**Beschreibung:**
Den TCP-Listener des NetworkServers implementieren, der auf eingehende Verbindungen wartet und diese an die ClientConnection-Verwaltung weitergibt.

**Aufgaben:**
- [ ] `NetworkServer`-Klasse anlegen mit Port-Parameter im Konstruktor
- [ ] `RunAsync(CancellationToken)` Methode implementieren, die den Listener startet
- [ ] `TcpListener` auf dem konfigurierten Port starten
- [ ] Accept-Loop implementieren, der auf neue Verbindungen wartet
- [ ] Logging bei Start, Stop und Fehlern

**Akzeptanzkriterien:**
- [ ] Server startet und lauscht auf dem konfigurierten Port
- [ ] Server kann per CancellationToken sauber beendet werden
- [ ] Fehler beim Starten (z.B. Port belegt) werden geloggt

**Ressourcen:**
- [TcpListener-Klasse](https://learn.microsoft.com/en-us/dotnet/api/system.net.sockets.tcplistener) – Grundlagen zum TCP-Listener in .NET
- [Async/Await Pattern in .NET](https://learn.microsoft.com/en-us/dotnet/csharp/asynchronous-programming/) – Best Practices für asynchrone Programmierung

---

#### Sub-Issue 8b: ClientConnection-Klasse implementieren

**Labels:** `type:feature`, `area:server`, `area:network`, `priority:p1`

**Beschreibung:**
Die ClientConnection-Klasse implementieren, die eine einzelne Client-Verbindung repräsentiert und Nachrichten senden/empfangen kann.

**Aufgaben:**
- [ ] `ClientConnection`-Klasse mit eindeutiger `Id` (Guid) anlegen
- [ ] Konstruktor mit `TcpClient` Parameter
- [ ] Property für den Verbindungsstatus
- [ ] `Task SendAsync(INetworkMessage msg)` implementieren
- [ ] `Task DisconnectAsync()` implementieren
- [ ] Logging bei Verbindungsaktionen

**Akzeptanzkriterien:**
- [ ] Jede ClientConnection hat eine eindeutige ID
- [ ] Nachrichten können asynchron gesendet werden
- [ ] Disconnect räumt Ressourcen sauber auf

**Ressourcen:**
- [TcpClient-Klasse](https://learn.microsoft.com/en-us/dotnet/api/system.net.sockets.tcpclient) – API für TCP-Client-Verbindungen
- [NetworkStream-Klasse](https://learn.microsoft.com/en-us/dotnet/api/system.net.sockets.networkstream) – Streams für Netzwerkkommunikation

---

#### Sub-Issue 8c: Message-Lese-Loop und Events implementieren

**Labels:** `type:feature`, `area:server`, `area:network`, `priority:p1`

**Beschreibung:**
Den internen Lese-Loop für eingehende Nachrichten implementieren und Events für Verbindungs-/Nachrichtenereignisse bereitstellen.

**Aufgaben:**
- [ ] Interner Lese-Loop in ClientConnection, der kontinuierlich Nachrichten liest
- [ ] Event `ClientConnected` im NetworkServer
- [ ] Event `ClientDisconnected` im NetworkServer
- [ ] Event `MessageReceived` im NetworkServer
- [ ] Fehlerbehandlung bei Verbindungsabbruch

**Akzeptanzkriterien:**
- [ ] Events werden korrekt ausgelöst bei Connect/Disconnect/Message
- [ ] Verbindungsabbruch führt zu sauberem Disconnect-Event
- [ ] Keine Exceptions bei normalem Verbindungsende

**Ressourcen:**
- [Ereignisse in C#](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/events/) – Events und Event-Handler
- [Exception Handling Best Practices](https://learn.microsoft.com/en-us/dotnet/standard/exceptions/best-practices-for-exceptions) – Fehlerbehandlung

---

### Issue #9: MessageRouter und Login-Flow → Sub-Issues

Das ursprüngliche Issue #9 sollte in folgende Sub-Issues aufgeteilt werden:

---

#### Sub-Issue 9a: MessageRouter-Basisklasse implementieren

**Labels:** `type:feature`, `area:server`, `priority:p1`

**Beschreibung:**
Die MessageRouter-Klasse implementieren, die eingehende Nachrichten anhand des MessageType an die passenden Handler weiterleitet.

**Aufgaben:**
- [ ] `MessageRouter`-Klasse anlegen
- [ ] `Route(ClientConnection, INetworkMessage)` Methode implementieren
- [ ] Switch/Dictionary-basiertes Routing nach MessageType
- [ ] Logging für unbekannte Nachrichtentypen
- [ ] Stub-Handler für zukünftige Message-Typen anlegen

**Akzeptanzkriterien:**
- [ ] Nachrichten werden korrekt an Handler geroutet
- [ ] Unbekannte Nachrichtentypen werden geloggt, crashen aber nicht
- [ ] Router ist erweiterbar für neue Nachrichtentypen

**Ressourcen:**
- [Pattern Matching in C#](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/functional/pattern-matching) – Switch-Expressions und Pattern Matching
- [Dependency Injection in .NET](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection) – Für spätere Erweiterbarkeit

---

#### Sub-Issue 9b: Login-Handler (In-Memory) implementieren

**Labels:** `type:feature`, `area:server`, `priority:p1`

**Beschreibung:**
Den Login-Handler implementieren, der LoginRequests verarbeitet und Spieler in der World erstellt.

**Aufgaben:**
- [ ] `HandleLogin(ClientConnection, LoginRequest)` Methode implementieren
- [ ] Spielernamen validieren (nicht leer, passende Länge)
- [ ] Player über `World.CreatePlayer()` erstellen
- [ ] `LoginResponse` mit `Success = true` und `PlayerId` senden
- [ ] Bei Fehler: `LoginResponse` mit `Success = false` und Fehlermeldung

**Akzeptanzkriterien:**
- [ ] Gültiger LoginRequest erzeugt einen Player in der World
- [ ] Client erhält korrekte LoginResponse
- [ ] Ungültige Anfragen werden mit Fehlermeldung beantwortet

**Ressourcen:**
- [Validierung in .NET](https://learn.microsoft.com/en-us/dotnet/api/system.componentmodel.dataannotations) – DataAnnotations für Validierung
- [System.Text.Json](https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/overview) – JSON-Serialisierung

---

### Issue #11: Login-UI und LoginRequest-Senden → Sub-Issues

Das ursprüngliche Issue #11 sollte in folgende Sub-Issues aufgeteilt werden:

---

#### Sub-Issue 11a: LoginPanel-UI in Godot erstellen

**Labels:** `type:feature`, `area:client`, `priority:p1`

**Beschreibung:**
Die Login-UI mit Eingabefeld und Button in Godot erstellen.

**Aufgaben:**
- [ ] `LoginPanel`-Szene erstellen (Control Node)
- [ ] LineEdit für Spielernamen-Eingabe
- [ ] Button zum Einloggen
- [ ] Label für Fehlermeldungen (initial versteckt)
- [ ] Einfaches, sauberes Layout

**Akzeptanzkriterien:**
- [ ] UI ist sichtbar und bedienbar
- [ ] Textfeld akzeptiert Eingaben
- [ ] Button ist klickbar

**Ressourcen:**
- [Control Nodes in Godot](https://docs.godotengine.org/en/stable/tutorials/ui/control_node_gallery.html) – Überblick über UI-Elemente
- [GUI-Design in Godot](https://docs.godotengine.org/en/stable/tutorials/ui/index.html) – UI-Tutorial

---

#### Sub-Issue 11b: Login-Button-Handler und Netzwerk-Integration

**Labels:** `type:feature`, `area:client`, `area:network`, `priority:p1`

**Beschreibung:**
Den Button-Handler implementieren, der die Verbindung herstellt und LoginRequest sendet.

**Aufgaben:**
- [ ] Signal-Verbindung für Button-Click
- [ ] Bei Klick: Verbindung zu `localhost:7777` über NetworkClient
- [ ] `LoginRequest` mit eingegebenem Namen senden
- [ ] UI während des Vorgangs deaktivieren (Button, Textfeld)

**Akzeptanzkriterien:**
- [ ] Klick auf Button initiiert Verbindung
- [ ] LoginRequest wird gesendet
- [ ] UI reagiert auf den Vorgang

**Ressourcen:**
- [Signals in Godot C#](https://docs.godotengine.org/en/stable/tutorials/scripting/c_sharp/c_sharp_signals.html) – Signal-Verbindungen in C#
- [Godot Networking Basics](https://docs.godotengine.org/en/stable/tutorials/networking/index.html) – Netzwerk-Grundlagen

---

#### Sub-Issue 11c: LoginResponse verarbeiten und Fehlerbehandlung

**Labels:** `type:feature`, `area:client`, `priority:p1`

**Beschreibung:**
Die LoginResponse vom Server verarbeiten und entsprechend in der UI reagieren.

**Aufgaben:**
- [ ] Handler für LoginResponse im NetworkClient registrieren
- [ ] Bei Erfolg: Login-UI ausblenden, zur Spielszene wechseln
- [ ] Bei Fehler: Fehlermeldung im Label anzeigen
- [ ] UI nach Fehler wieder aktivieren

**Akzeptanzkriterien:**
- [ ] Erfolgreicher Login führt zum Szenenwechsel
- [ ] Fehler werden in der UI angezeigt
- [ ] Nach Fehler kann erneut versucht werden

**Ressourcen:**
- [SceneTree in Godot](https://docs.godotengine.org/en/stable/classes/class_scenetree.html) – Szenenwechsel
- [Godot C# Basics](https://docs.godotengine.org/en/stable/tutorials/scripting/c_sharp/c_sharp_basics.html) – C#-Grundlagen in Godot

---

### Issue #12: MoveRequest verarbeiten → Sub-Issues

Das ursprüngliche Issue #12 sollte in folgende Sub-Issues aufgeteilt werden:

---

#### Sub-Issue 12a: LocalPlayerController im Client implementieren

**Labels:** `type:feature`, `area:client`, `priority:p1`

**Beschreibung:**
Den LocalPlayerController implementieren, der Tastatureingaben liest und in MoveRequests umwandelt.

**Aufgaben:**
- [ ] `LocalPlayerController`-Script erstellen
- [ ] Input-Handling für WASD und Pfeiltasten
- [ ] Richtungsvektor aus Eingaben berechnen
- [ ] Normalisierung des Richtungsvektors
- [ ] `MoveRequest { DirX, DirY }` an NetworkClient senden

**Akzeptanzkriterien:**
- [ ] Tastatureingaben werden erkannt
- [ ] MoveRequests werden kontinuierlich gesendet bei Bewegung
- [ ] Keine Bewegung → kein MoveRequest oder MoveRequest mit (0,0)

**Ressourcen:**
- [Input Handling in Godot](https://docs.godotengine.org/en/stable/tutorials/inputs/input_examples.html) – Input-Beispiele
- [InputMap in Godot](https://docs.godotengine.org/en/stable/tutorials/inputs/inputevent.html) – Input-Events

---

#### Sub-Issue 12b: HandleMove im Server implementieren

**Labels:** `type:feature`, `area:server`, `priority:p1`

**Beschreibung:**
Die serverseitige Verarbeitung von MoveRequests implementieren.

**Aufgaben:**
- [ ] `HandleMove(ClientConnection, MoveRequest)` im MessageRouter
- [ ] Zugehörigen Player über Connection finden
- [ ] Richtung validieren (Länge ≤ 1)
- [ ] Position mit Geschwindigkeit × deltaTime aktualisieren
- [ ] Weltgrenzen berücksichtigen

**Akzeptanzkriterien:**
- [ ] Serverposition ändert sich konsistent mit Eingaben
- [ ] Keine Teleportation durch ungültige Richtungswerte
- [ ] Spieler bleibt innerhalb der Weltgrenzen

**Ressourcen:**
- [Vector-Mathematik in C#](https://learn.microsoft.com/en-us/dotnet/api/system.numerics.vector2) – System.Numerics.Vector2
- [Game Loop Patterns](https://learn.microsoft.com/en-us/archive/msdn-magazine/2015/march/c-game-programming-building-a-basic-game-engine) – Spiellogik-Grundlagen

---

### Issue #14: Remote-Player-Instanzen → Sub-Issues

Das ursprüngliche Issue #14 sollte in folgende Sub-Issues aufgeteilt werden:

---

#### Sub-Issue 14a: GameManager für Spieler-Verwaltung erstellen

**Labels:** `type:feature`, `area:client`, `priority:p1`

**Beschreibung:**
Den GameManager implementieren, der Remote-Spieler-Instanzen verwaltet.

**Aufgaben:**
- [ ] `GameManager`-Script als Autoload oder Szenen-Root
- [ ] Dictionary `PlayerId → PlayerNode` für Spielerverwaltung
- [ ] Methode `UpdatePlayers(PlayerStateUpdate)` 
- [ ] Neue Spieler instanziieren, wenn sie erstmalig auftauchen
- [ ] Spieler entfernen, die nicht mehr in Updates vorkommen

**Akzeptanzkriterien:**
- [ ] Spieler werden bei erstem Auftauchen erstellt
- [ ] Spieler werden entfernt, wenn sie verschwinden
- [ ] Eigener Spieler wird nicht als Remote erstellt

**Ressourcen:**
- [Instanziieren von Szenen](https://docs.godotengine.org/en/stable/tutorials/scripting/instancing.html) – Szenen zur Laufzeit erstellen
- [Node-Verwaltung in Godot](https://docs.godotengine.org/en/stable/classes/class_node.html) – Node-API

---

#### Sub-Issue 14b: PlayerNode-Szene und Sprite erstellen

**Labels:** `type:feature`, `area:client`, `priority:p1`

**Beschreibung:**
Die PlayerNode-Szene mit Platzhalter-Sprite für Remote-Spieler erstellen.

**Aufgaben:**
- [ ] `PlayerNode.tscn` Szene erstellen
- [ ] Platzhalter-Sprite (z.B. farbiges Rechteck oder Icon)
- [ ] Label für Spielernamen über dem Sprite
- [ ] Script mit Property für PlayerId und Name
- [ ] Methode `UpdatePosition(float x, float y)`

**Akzeptanzkriterien:**
- [ ] PlayerNode ist sichtbar in der Szene
- [ ] Name wird korrekt angezeigt
- [ ] Position kann aktualisiert werden

**Ressourcen:**
- [Sprite2D in Godot](https://docs.godotengine.org/en/stable/classes/class_sprite2d.html) – Sprite-Dokumentation
- [Label in Godot](https://docs.godotengine.org/en/stable/classes/class_label.html) – Text-Label

---

#### Sub-Issue 14c: Positions-Interpolation für Remote-Spieler

**Labels:** `type:feature`, `area:client`, `priority:p2`

**Beschreibung:**
Optionale Interpolation implementieren, um Bewegungen von Remote-Spielern zu glätten.

**Aufgaben:**
- [ ] Zielposition und aktuelle Position speichern
- [ ] Lineare Interpolation in `_Process()`
- [ ] Interpolationsgeschwindigkeit konfigurierbar
- [ ] Fallback bei großen Positionssprüngen (Teleport)

**Akzeptanzkriterien:**
- [ ] Bewegungen wirken flüssiger
- [ ] Keine Verzögerung bei Teleports
- [ ] Performance bleibt akzeptabel

**Ressourcen:**
- [Lerp-Funktion in Godot](https://docs.godotengine.org/en/stable/classes/class_@globalscope.html#class-globalscope-method-lerp) – Lineare Interpolation
- [Smooth Movement Tutorial](https://docs.godotengine.org/en/stable/tutorials/physics/using_kinematic_body_2d.html) – Bewegungsglättung

---

### Issue #15: Movement-Prototyp stabilisieren → Sub-Issues

Das ursprüngliche Issue #15 sollte in folgende Sub-Issues aufgeteilt werden:

---

#### Sub-Issue 15a: Tickrate und Geschwindigkeit tunen

**Labels:** `type:chore`, `area:server`, `area:client`, `priority:p1`

**Beschreibung:**
Tickrate und Bewegungsgeschwindigkeit so anpassen, dass sich der Prototyp flüssig anfühlt.

**Aufgaben:**
- [ ] Server-Tickrate evaluieren (20-60 Hz)
- [ ] Client-Update-Rate für Broadcasts evaluieren (10-20/s)
- [ ] Bewegungsgeschwindigkeit anpassen
- [ ] Inputrate vom Client evaluieren
- [ ] Werte als Konstanten/Config auslagern

**Akzeptanzkriterien:**
- [ ] Bewegung fühlt sich responsiv an
- [ ] Keine sichtbare Verzögerung bei Eingaben
- [ ] CPU-Last bleibt niedrig

**Ressourcen:**
- [Game Loop Timing](https://learn.microsoft.com/en-us/archive/msdn-magazine/2014/november/windows-with-c-high-performance-game-timing) – Timing in Spielen
- [Godot _Process vs _PhysicsProcess](https://docs.godotengine.org/en/stable/tutorials/scripting/idle_and_physics_processing.html) – Update-Loops

---

#### Sub-Issue 15b: Unit-Tests für World und Movement

**Labels:** `type:test`, `area:server`, `priority:p1`

**Beschreibung:**
Unit-Tests für die Kernlogik von World und Movement implementieren.

**Aufgaben:**
- [ ] Testprojekt `Mmo.Server.Tests` erstellen
- [ ] Tests für `World.CreatePlayer`
- [ ] Tests für `World.RemovePlayerByConnection`
- [ ] Tests für Movement-Berechnung
- [ ] Edge-Cases: 0-Werte, Grenzen, ungültige Eingaben

**Akzeptanzkriterien:**
- [ ] Alle Tests laufen durch
- [ ] Kritische Pfade sind getestet
- [ ] Tests sind in CI integrierbar

**Ressourcen:**
- [xUnit Testing](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-with-dotnet-test) – Unit-Tests mit xUnit
- [Best Practices für Unit-Tests](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices) – Testmuster

---

#### Sub-Issue 15c: Manuelle Integrationstests dokumentieren

**Labels:** `type:chore`, `area:server`, `area:client`, `priority:p2`

**Beschreibung:**
Manuelle Testfälle dokumentieren und durchführen.

**Aufgaben:**
- [ ] Testplan-Dokument erstellen
- [ ] Testfall: Ein-/Ausloggen mit mehreren Clients
- [ ] Testfall: Dauerhaftes Movement über mehrere Minuten
- [ ] Testfall: Server-Stop während aktiver Verbindungen
- [ ] Ergebnisse dokumentieren

**Akzeptanzkriterien:**
- [ ] Prototyp läuft 5+ Minuten stabil mit 2-3 Clients
- [ ] Keine Crashes oder Memory-Leaks beobachtet
- [ ] Testplan ist dokumentiert

**Ressourcen:**
- [Manuelles Testen](https://learn.microsoft.com/en-us/azure/devops/test/create-test-cases) – Testfälle erstellen
- [Exploratory Testing](https://learn.microsoft.com/en-us/azure/devops/test/exploratory-testing-with-cuit) – Exploratives Testen

---

## Teil 2: Neue Issues für fehlende Funktionalität

### Phase 2 – Feinschliff

---

#### Issue: Verbindungsaufbau-Flow mit Retry und Fehlerbehandlung

**Labels:** `type:feature`, `area:client`, `area:network`, `priority:p1`

**Beschreibung:**
Den Verbindungsaufbau im `NetworkClient` robuster gestalten mit automatischem Retry-Mechanismus, konfigurierbarem Timeout und Benutzer-Feedback bei Fehlern. Der Client soll bei temporären Netzwerkproblemen nicht sofort aufgeben, sondern mehrere Verbindungsversuche unternehmen.

**Aufgaben:**
- [ ] `ConnectionConfig`-Klasse erstellen mit Properties:
  - [ ] `int TimeoutMs = 5000` – Timeout für einzelnen Verbindungsversuch
  - [ ] `int MaxRetries = 3` – Maximale Anzahl Wiederholungen
  - [ ] `int RetryDelayMs = 1000` – Wartezeit zwischen Versuchen
- [ ] `ConnectionState`-Enum definieren: `Disconnected`, `Connecting`, `Connected`, `Reconnecting`, `Failed`
- [ ] `NetworkClient.ConnectAsync()` erweitern:
  - [ ] `CancellationTokenSource` für Timeout erstellen
  - [ ] Retry-Loop mit exponential Backoff implementieren
  - [ ] State-Änderungen über Signal/Event kommunizieren
- [ ] Signal `ConnectionStateChanged(ConnectionState state)` im NetworkClient
- [ ] Im `LoginPanel`:
  - [ ] Auf `ConnectionStateChanged` reagieren
  - [ ] Loading-Spinner oder Progress-Bar während `Connecting`
  - [ ] Button deaktivieren während Verbindungsaufbau
- [ ] Fehlermeldungen differenziert anzeigen:
  - [ ] "Verbindung fehlgeschlagen – Server nicht erreichbar"
  - [ ] "Zeitüberschreitung – bitte erneut versuchen"
  - [ ] "Maximale Versuche erreicht"

**Akzeptanzkriterien:**
- [ ] Bei nicht erreichbarem Server: 3 Versuche mit je 5s Timeout
- [ ] UI zeigt aktuellen Status (Connecting, Retry 1/3, Failed)
- [ ] Nach `MaxRetries` wird `ConnectionState.Failed` gesetzt
- [ ] Benutzer kann nach Fehlschlag erneut auf Login klicken
- [ ] Keine Memory-Leaks bei Timeout (CancellationToken korrekt disposed)

**Ressourcen:**
- [CancellationTokenSource](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtokensource) – Timeout-Implementierung mit CancellationToken
- [Async/Await Best Practices](https://learn.microsoft.com/en-us/dotnet/csharp/asynchronous-programming/async-scenarios) – Asynchrone Patterns in C#
- [Godot Signals in C#](https://docs.godotengine.org/en/stable/tutorials/scripting/c_sharp/c_sharp_signals.html) – Events zwischen Nodes kommunizieren
- [Task.Delay für Retry](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task.delay) – Verzögerung zwischen Retry-Versuchen

---

#### Issue: Konsistente Verwendung der Shared DTOs im Client

**Labels:** `type:chore`, `area:server`, `area:client`, `priority:p1`

**Beschreibung:**
Sicherstellen, dass der Godot-Client exakt dieselben DTO-Klassen aus `Mmo.Shared` verwendet wie der Server. Dies verhindert Serialisierungsfehler und doppelte Code-Pflege. Das Shared-Projekt muss als Projektlink in die Godot-Solution eingebunden werden.

**Aufgaben:**
- [ ] Godot-Projekt `.csproj` erweitern:
  - [ ] `<ProjectReference Include="../../shared/Mmo.Shared/Mmo.Shared.csproj" />` hinzufügen
  - [ ] Sicherstellen, dass Godot das referenzierte Projekt findet
- [ ] Namespace-Struktur vereinheitlichen:
  - [ ] Shared: `Mmo.Shared.Messages` für alle DTOs
  - [ ] Shared: `Mmo.Shared.Enums` für MessageType, ConnectionState etc.
- [ ] Prüfen ob duplizierte Klassen im Client existieren und entfernen:
  - [ ] Suche nach lokalen `LoginRequest`, `LoginResponse` etc.
  - [ ] Ersetzen durch `using Mmo.Shared.Messages;`
- [ ] Build-Reihenfolge sicherstellen:
  - [ ] `Mmo.Shared` muss vor Client gebaut werden
  - [ ] `dotnet build` auf Solution-Ebene testen
- [ ] Dokumentation erstellen:
  - [ ] `shared/README.md` mit Liste aller DTOs
  - [ ] Kurzbeschreibung pro DTO (Zweck, Properties)

**Akzeptanzkriterien:**
- [ ] `dotnet build Mmo.sln` baut Server und Shared fehlerfrei
- [ ] Godot-Projekt baut fehlerfrei mit Shared-Referenz
- [ ] Keine duplizierten DTO-Klassen im Client-Ordner
- [ ] Client verwendet exakt dieselben Typen wie Server
- [ ] Änderungen an DTOs werden in beiden Projekten wirksam

**Ressourcen:**
- [Projektverweise in .NET](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-add-reference) – `dotnet add reference` Befehl
- [Godot C# Projekte](https://docs.godotengine.org/en/stable/tutorials/scripting/c_sharp/c_sharp_basics.html#c-project) – C#-Projektstruktur in Godot
- [Multi-Projekt-Solutions](https://learn.microsoft.com/en-us/dotnet/core/tutorials/library-with-visual-studio-code) – Mehrere Projekte in einer Solution
- [Shared Code Best Practices](https://learn.microsoft.com/en-us/dotnet/standard/library-guidance/cross-platform-targeting) – Plattformübergreifender Code

---

#### Issue: Grundlegendes Logging im Network-Code

**Labels:** `type:feature`, `area:server`, `area:client`, `priority:p2`

**Beschreibung:**
Strukturiertes Logging für alle Netzwerk-Operationen einführen. Auf dem Server wird `Microsoft.Extensions.Logging` verwendet, im Godot-Client ein Wrapper um `GD.Print` mit Log-Levels. Alle wichtigen Events (Verbindungen, Nachrichten, Fehler) werden geloggt.

**Aufgaben:**
- [ ] **Server-Logging einrichten:**
  - [ ] NuGet-Paket `Microsoft.Extensions.Logging` hinzufügen
  - [ ] `ILogger<T>` in `NetworkServer`, `ClientConnection`, `MessageRouter` injecten
  - [ ] Log-Konfiguration in `Program.cs`:
    ```csharp
    builder.Logging.AddConsole();
    builder.Logging.SetMinimumLevel(LogLevel.Debug);
    ```
- [ ] **Log-Statements im Server hinzufügen:**
  - [ ] `NetworkServer`: `LogInformation("Server started on port {Port}", port)`
  - [ ] `ClientConnection`: `LogDebug("Client {Id} connected from {Endpoint}")`
  - [ ] `ClientConnection`: `LogWarning("Client {Id} disconnected unexpectedly")`
  - [ ] `MessageRouter`: `LogDebug("Routing {MessageType} from {ClientId}")`
  - [ ] Fehler: `LogError(ex, "Error processing message from {ClientId}")`
- [ ] **Client-Logging einrichten:**
  - [ ] `Logger`-Singleton-Klasse erstellen
  - [ ] Enum `LogLevel`: `Debug`, `Info`, `Warning`, `Error`
  - [ ] Methoden: `Log(LogLevel, string)`, `Debug(string)`, `Info(string)`, `Error(string)`
  - [ ] Intern: `GD.Print($"[{level}] {timestamp}: {message}")`
- [ ] **Log-Statements im Client hinzufügen:**
  - [ ] `NetworkClient`: Connection-Events loggen
  - [ ] Message-Empfang und -Versand loggen
  - [ ] Fehler und Timeouts loggen
- [ ] **Log-Level konfigurierbar machen:**
  - [ ] Server: über `appsettings.json` oder Umgebungsvariable
  - [ ] Client: über Projekteinstellung oder Konstante

**Akzeptanzkriterien:**
- [ ] Server-Start zeigt Info-Log mit Port
- [ ] Client-Verbindung erzeugt Log auf Server und Client
- [ ] Message-Routing ist im Debug-Log nachvollziehbar
- [ ] Fehler werden mit Stack-Trace geloggt
- [ ] Log-Level kann ohne Neukompilierung geändert werden

**Ressourcen:**
- [Logging in .NET](https://learn.microsoft.com/en-us/dotnet/core/extensions/logging) – Offizielles Logging-Framework
- [ILogger Interface](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.logging.ilogger) – API-Dokumentation
- [Structured Logging](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/logging/#log-message-template) – Message-Templates mit Platzhaltern
- [Godot GD.Print](https://docs.godotengine.org/en/stable/classes/class_@globalscope.html#class-globalscope-method-print) – Konsolen-Ausgabe in Godot

---

### Phase 3 – Welt & Movement

---

#### Issue: 2D-Tilemap für Prototyp-Welt erstellen

**Labels:** `type:feature`, `area:client`, `priority:p1`

**Beschreibung:**
Eine einfache 2D-Tilemap in Godot erstellen als visuelle Grundlage für die Spielwelt. Die Map dient als Prototyp-Umgebung für Bewegungstests und zeigt klare Grenzen, in denen sich Spieler bewegen können.

**Aufgaben:**
- [ ] **TileSet erstellen:**
  - [ ] Neues TileSet-Resource erstellen: `res://assets/tilesets/prototype_tileset.tres`
  - [ ] Platzhalter-Tiles (16x16 oder 32x32 Pixel):
    - [ ] Gras-Tile (grün) für begehbaren Bereich
    - [ ] Wasser-Tile (blau) für Rand/Grenze
    - [ ] Stein-Tile (grau) für optionale Hindernisse
  - [ ] Tiles können einfache farbige Rechtecke sein (kein Art-Asset nötig)
- [ ] **TileMap-Node einrichten:**
  - [ ] `TileMap`-Node zur `Game.tscn` Szene hinzufügen
  - [ ] TileSet zuweisen
  - [ ] Cell-Size auf Tile-Größe setzen (z.B. 32x32)
  - [ ] Z-Index unter Spieler-Sprites setzen
- [ ] **Test-Map gestalten:**
  - [ ] Größe: 50x50 Tiles (1600x1600 Pixel bei 32px)
  - [ ] Äußeren Rand mit Wasser-Tiles füllen (2 Tiles breit)
  - [ ] Inneren Bereich mit Gras-Tiles füllen
  - [ ] Spawn-Bereich in der Mitte markieren (optional anderes Tile)
- [ ] **Kamera-Begrenzung:**
  - [ ] `Camera2D` Limits auf Map-Größe setzen
  - [ ] `limit_left`, `limit_top`, `limit_right`, `limit_bottom` konfigurieren
- [ ] **Koordinatensystem dokumentieren:**
  - [ ] Map-Ursprung (0,0) = linke obere Ecke
  - [ ] Spielbarer Bereich: (64, 64) bis (1536, 1536)

**Akzeptanzkriterien:**
- [ ] TileMap ist im Editor sichtbar und bearbeitbar
- [ ] Im Spiel ist der Boden als Raster erkennbar
- [ ] Spieler-Sprite bewegt sich über die Tiles
- [ ] Kamera zeigt nur den Map-Bereich

**Ressourcen:**
- [Using TileMaps in Godot 4](https://docs.godotengine.org/en/stable/tutorials/2d/using_tilemaps.html) – Schritt-für-Schritt TileMap-Tutorial
- [TileSet Class](https://docs.godotengine.org/en/stable/classes/class_tileset.html) – TileSet-Konfiguration
- [Camera2D Limits](https://docs.godotengine.org/en/stable/classes/class_camera2d.html#class-camera2d-property-limit-bottom) – Kamera begrenzen
- [2D Coordinate System](https://docs.godotengine.org/en/stable/tutorials/2d/2d_movement.html) – Koordinatensystem in Godot

---

#### Issue: Serverseitige Startposition und Spawn-Logik

**Labels:** `type:feature`, `area:server`, `priority:p1`

**Beschreibung:**
Die Startposition für neue Spieler serverseitig definieren und Spawn-Logik implementieren. Neue Spieler sollen in der Weltmitte spawnen, mit leichter Variation um Überlappungen zu vermeiden. Die Spawn-Position wird beim Login an den Client übermittelt.

**Aufgaben:**
- [ ] **Spawn-Konfiguration erstellen:**
  - [ ] `SpawnConfig`-Klasse oder Konstanten in `World`:
    ```csharp
    public static class SpawnConfig
    {
        public const float DefaultX = 800f;
        public const float DefaultY = 800f;
        public const float SpawnRadius = 50f;
    }
    ```
- [ ] **Spawn-Position-Berechnung:**
  - [ ] `World.GetSpawnPosition()` Methode:
    ```csharp
    public Vector2 GetSpawnPosition()
    {
        var random = new Random();
        var offsetX = (float)(random.NextDouble() * SpawnRadius * 2 - SpawnRadius);
        var offsetY = (float)(random.NextDouble() * SpawnRadius * 2 - SpawnRadius);
        return new Vector2(DefaultX + offsetX, DefaultY + offsetY);
    }
    ```
  - [ ] Prüfung ob Position frei ist (keine Kollision mit anderen Spielern)
  - [ ] Falls belegt: neue Position berechnen (max 5 Versuche)
- [ ] **Integration in Login-Flow:**
  - [ ] `World.CreatePlayer()` verwendet `GetSpawnPosition()`
  - [ ] Position wird im `Player`-Objekt gesetzt
  - [ ] `LoginResponse` erweitern um `SpawnX`, `SpawnY`:
    ```csharp
    public class LoginResponse : INetworkMessage
    {
        public bool Success { get; set; }
        public Guid PlayerId { get; set; }
        public float SpawnX { get; set; }
        public float SpawnY { get; set; }
        public string? ErrorMessage { get; set; }
    }
    ```
- [ ] **Spawn-Position an Client senden:**
  - [ ] Im Login-Handler: Position aus Player-Objekt lesen
  - [ ] In LoginResponse eintragen
  - [ ] Client positioniert lokalen Spieler an dieser Position

**Akzeptanzkriterien:**
- [ ] Neue Spieler spawnen in der Weltmitte (±50 Pixel Variation)
- [ ] Zwei gleichzeitig einloggende Spieler haben unterschiedliche Positionen
- [ ] Client erhält Spawn-Position über LoginResponse
- [ ] Spieler-Sprite erscheint an korrekter Position

**Ressourcen:**
- [System.Random](https://learn.microsoft.com/en-us/dotnet/api/system.random) – Zufallszahlen für Spawn-Variation
- [System.Numerics.Vector2](https://learn.microsoft.com/en-us/dotnet/api/system.numerics.vector2) – 2D-Vektoren in .NET
- [Collision Detection Patterns](https://learn.microsoft.com/en-us/archive/msdn-magazine/2014/march/windows-phone-building-a-2d-physics-game-engine) – Kollisionsprüfung
- [DTOs erweitern](https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/how-to) – JSON-Serialisierung neuer Properties

---

#### Issue: Mapping von Server-Positionen auf Godot-Koordinaten

**Labels:** `type:feature`, `area:client`, `priority:p1`

**Beschreibung:**
Die Server-Koordinaten (Float-Werte in logischen Einheiten) auf Godot-Weltkoordinaten (Pixel) mappen. Ein einheitliches Koordinatensystem ermöglicht konsistente Darstellung auf verschiedenen Auflösungen.

**Aufgaben:**
- [ ] **CoordinateMapper-Singleton erstellen:**
  - [ ] Neues Script `CoordinateMapper.cs` als Autoload
  - [ ] Konfigurierbare Skalierung:
    ```csharp
    public partial class CoordinateMapper : Node
    {
        public const float PixelsPerUnit = 1.0f; // 1 Server-Einheit = 1 Pixel
        
        public Vector2 ServerToWorld(float serverX, float serverY)
        {
            return new Vector2(serverX * PixelsPerUnit, serverY * PixelsPerUnit);
        }
        
        public (float x, float y) WorldToServer(Vector2 worldPos)
        {
            return (worldPos.X / PixelsPerUnit, worldPos.Y / PixelsPerUnit);
        }
    }
    ```
- [ ] **In project.godot als Autoload registrieren:**
  - [ ] Pfad: `res://scripts/singletons/CoordinateMapper.cs`
  - [ ] Name: `CoordinateMapper`
- [ ] **PlayerNode-Positionierung anpassen:**
  - [ ] In `UpdatePosition(float x, float y)`:
    ```csharp
    var worldPos = CoordinateMapper.Instance.ServerToWorld(x, y);
    Position = worldPos;
    ```
- [ ] **LocalPlayerController anpassen:**
  - [ ] Position aus NetworkClient kommt als Server-Koordinaten
  - [ ] Umrechnung vor dem Setzen der Node-Position
- [ ] **Kamera-Setup:**
  - [ ] `Camera2D` als Child des lokalen Spielers
  - [ ] `Position Smoothing` aktivieren (Speed: 5)
  - [ ] Kamera folgt automatisch dem Spieler
- [ ] **Koordinatensystem dokumentieren:**
  - [ ] Server: Koordinaten in abstrakten Einheiten (z.B. 0-1600)
  - [ ] Client: 1:1 Mapping zu Pixeln (anpassbar über `PixelsPerUnit`)
  - [ ] Ursprung: Links oben (0, 0)
  - [ ] Positive X: nach rechts, Positive Y: nach unten

**Akzeptanzkriterien:**
- [ ] Server-Position (800, 800) erscheint in Godot bei Pixel (800, 800)
- [ ] Bewegung nach oben (negative Y-Server) bewegt Sprite nach oben
- [ ] Kamera folgt dem lokalen Spieler sanft
- [ ] Koordinaten sind dokumentiert für spätere Skalierung

**Ressourcen:**
- [Godot Singletons/Autoload](https://docs.godotengine.org/en/stable/tutorials/scripting/singletons_autoload.html) – Globale Singletons
- [Camera2D Following](https://docs.godotengine.org/en/stable/classes/class_camera2d.html#class-camera2d-property-position-smoothing-enabled) – Kamera-Smoothing
- [Godot Coordinate System](https://docs.godotengine.org/en/stable/tutorials/2d/2d_movement.html#moving-in-2d) – Y-Achse zeigt nach unten
- [Vector2 in Godot](https://docs.godotengine.org/en/stable/classes/class_vector2.html) – 2D-Vektoroperationen

---

#### Issue: Weltgrenzen im Server implementieren

**Labels:** `type:feature`, `area:server`, `priority:p1`

**Beschreibung:**
Weltgrenzen auf dem Server implementieren, damit Spieler nicht außerhalb der definierten Spielfläche laufen können. Die Grenzen entsprechen der Tilemap-Größe und werden bei jeder Positionsänderung überprüft.

**Aufgaben:**
- [ ] **WorldBounds-Konfiguration:**
  - [ ] Konstanten oder Config-Klasse:
    ```csharp
    public static class WorldBounds
    {
        public const float MinX = 64f;   // 2 Tiles Rand
        public const float MinY = 64f;
        public const float MaxX = 1536f; // 50 Tiles - 2 Tiles Rand
        public const float MaxY = 1536f;
    }
    ```
- [ ] **Position-Clamping im Player:**
  - [ ] `Player.ClampPosition()` Methode:
    ```csharp
    public void ClampPosition()
    {
        X = Math.Clamp(X, WorldBounds.MinX, WorldBounds.MaxX);
        Y = Math.Clamp(Y, WorldBounds.MinY, WorldBounds.MaxY);
    }
    ```
  - [ ] Aufruf in `Player.Update()` nach Positionsänderung
- [ ] **Integration im Movement-Handler:**
  - [ ] Nach `player.X += velocity.X * deltaTime`:
    ```csharp
    player.ClampPosition();
    ```
  - [ ] Keine negative Rückmeldung an Client nötig (Server ist authoritative)
- [ ] **Boundary-Check für Teleportation:**
  - [ ] Falls später Teleport-Features: Position validieren
  - [ ] `World.IsValidPosition(float x, float y)` Methode
- [ ] **Weltgröße an Client kommunizieren (optional):**
  - [ ] `WorldInfoMessage` mit Bounds senden
  - [ ] Oder: Client kennt Bounds fest (synchron mit Server)
- [ ] **Tests:**
  - [ ] Unit-Test: Position an Grenze bleibt unverändert
  - [ ] Unit-Test: Position außerhalb wird geclampt
  - [ ] Manueller Test: An allen 4 Rändern entlang laufen

**Akzeptanzkriterien:**
- [ ] Spieler können nicht außerhalb MinX/MaxX/MinY/MaxY
- [ ] Bewegung entlang der Grenze funktioniert (kein "Kleben")
- [ ] Diagonale Bewegung an Ecken verhält sich korrekt
- [ ] Server-Position ist immer innerhalb der Bounds

**Ressourcen:**
- [Math.Clamp](https://learn.microsoft.com/en-us/dotnet/api/system.math.clamp) – Werte auf Bereich begrenzen
- [Unit Testing in .NET](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-with-dotnet-test) – Boundary-Tests
- [Vector Math](https://learn.microsoft.com/en-us/dotnet/api/system.numerics.vector2) – Für Bewegungsberechnung
- [Game Boundary Patterns](https://learn.microsoft.com/en-us/archive/msdn-magazine/2014/march/windows-phone-building-a-2d-physics-game-engine) – Spielgrenzen-Design

---

### Phase 4 – Persistenz

---

#### Issue: Datenmodell für Charakterpersistenz definieren

**Labels:** `type:feature`, `area:persistenz`, `priority:p1`

**Beschreibung:**
Das Datenmodell für die Speicherung von Charakterdaten im Shared-Projekt definieren. Die Klasse muss JSON-serialisierbar sein und alle für den Prototyp relevanten Spielerdaten enthalten.

**Aufgaben:**
- [ ] **CharacterData-Record erstellen:**
  - [ ] Datei: `shared/Mmo.Shared/Models/CharacterData.cs`
  - [ ] Als `record` für Immutability:
    ```csharp
    namespace Mmo.Shared.Models;
    
    public record CharacterData
    {
        public required Guid Id { get; init; }
        public required string Name { get; init; }
        public float X { get; init; }
        public float Y { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime LastLoginAt { get; init; }
        public int Level { get; init; } = 1;
        public int Experience { get; init; } = 0;
    }
    ```
- [ ] **JSON-Serialisierbarkeit testen:**
  - [ ] Unit-Test: Serialisieren und Deserialisieren
  - [ ] Prüfen ob alle Properties erhalten bleiben
  - [ ] Prüfen ob Default-Werte (Level=1) funktionieren
- [ ] **CharacterData.CreateNew() Factory-Methode:**
  ```csharp
  public static CharacterData CreateNew(string name, float x, float y)
  {
      return new CharacterData
      {
          Id = Guid.NewGuid(),
          Name = name,
          X = x,
          Y = y,
          CreatedAt = DateTime.UtcNow,
          LastLoginAt = DateTime.UtcNow
      };
  }
  ```
- [ ] **CharacterData.WithPosition() Methode:**
  ```csharp
  public CharacterData WithPosition(float x, float y)
  {
      return this with { X = x, Y = y, LastLoginAt = DateTime.UtcNow };
  }
  ```
- [ ] **Dokumentation:**
  - [ ] XML-Kommentare für alle Properties
  - [ ] Beschreibung der Einheiten (X/Y in Server-Koordinaten)

**Akzeptanzkriterien:**
- [ ] `CharacterData` ist im Shared-Projekt definiert
- [ ] JSON-Roundtrip funktioniert fehlerfrei
- [ ] Immutability durch `record` und `init` gewährleistet
- [ ] Factory-Methoden vereinfachen die Erstellung

**Ressourcen:**
- [Records in C#](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/record) – Immutable Data Types mit `with`-Expressions
- [Required Properties](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/required) – Pflichtfelder in C# 11
- [System.Text.Json und Records](https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/immutability) – Serialisierung von Records
- [DateTime Best Practices](https://learn.microsoft.com/en-us/dotnet/api/system.datetime?#datetime-values) – UTC vs Local Time

---

#### Issue: Persistenzschicht implementieren (File/JSON oder SQLite)

**Labels:** `type:feature`, `area:persistenz`, `area:server`, `priority:p1`

**Beschreibung:**
Eine einfache Persistenzschicht implementieren mit austauschbarem Repository-Pattern. Für den Prototyp wird ein JSON-File-basiertes Repository implementiert, das später durch SQLite ersetzt werden kann.

**Aufgaben:**
- [ ] **ICharacterRepository Interface definieren:**
  - [ ] Datei: `server/Mmo.Server/Persistence/ICharacterRepository.cs`
    ```csharp
    public interface ICharacterRepository
    {
        Task<CharacterData?> GetByNameAsync(string name);
        Task<CharacterData?> GetByIdAsync(Guid id);
        Task<bool> ExistsAsync(string name);
        Task SaveAsync(CharacterData character);
        Task DeleteAsync(Guid id);
        Task<IEnumerable<CharacterData>> GetAllAsync();
    }
    ```
- [ ] **FileCharacterRepository implementieren:**
  - [ ] Datei: `server/Mmo.Server/Persistence/FileCharacterRepository.cs`
  - [ ] Speicherort: `data/characters/` Ordner
  - [ ] Ein JSON-File pro Charakter: `{name}.json`
  - [ ] Thread-Safe durch `SemaphoreSlim`:
    ```csharp
    public class FileCharacterRepository : ICharacterRepository
    {
        private readonly string _dataPath;
        private readonly SemaphoreSlim _lock = new(1, 1);
        
        public FileCharacterRepository(string dataPath = "data/characters")
        {
            _dataPath = dataPath;
            Directory.CreateDirectory(_dataPath);
        }
        
        public async Task SaveAsync(CharacterData character)
        {
            await _lock.WaitAsync();
            try
            {
                var filePath = GetFilePath(character.Name);
                var json = JsonSerializer.Serialize(character, _jsonOptions);
                await File.WriteAllTextAsync(filePath, json);
            }
            finally
            {
                _lock.Release();
            }
        }
        
        private string GetFilePath(string name) 
            => Path.Combine(_dataPath, $"{name.ToLowerInvariant()}.json");
    }
    ```
- [ ] **JSON-Optionen konfigurieren:**
  ```csharp
  private static readonly JsonSerializerOptions _jsonOptions = new()
  {
      WriteIndented = true,
      PropertyNamingPolicy = JsonNamingPolicy.CamelCase
  };
  ```
- [ ] **Dependency Injection einrichten:**
  - [ ] In `Program.cs`: `services.AddSingleton<ICharacterRepository, FileCharacterRepository>();`
- [ ] **Unit-Tests erstellen:**
  - [ ] Test: Save und Load eines Characters
  - [ ] Test: GetByName für nicht existierenden Character → null
  - [ ] Test: Exists für existierenden Character → true
  - [ ] Test: Concurrent Save (Thread-Safety)

**Akzeptanzkriterien:**
- [ ] Charakterdaten werden als JSON-Files gespeichert
- [ ] Laden eines gespeicherten Characters funktioniert
- [ ] Thread-Safety bei gleichzeitigen Zugriffen
- [ ] Repository ist austauschbar durch Interface

**Ressourcen:**
- [Repository Pattern](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/infrastructure-persistence-layer-design) – Design Pattern für Datenzugriff
- [File.WriteAllTextAsync](https://learn.microsoft.com/en-us/dotnet/api/system.io.file.writealltextasync) – Asynchrone Dateischreibvorgänge
- [SemaphoreSlim](https://learn.microsoft.com/en-us/dotnet/api/system.threading.semaphoreslim) – Thread-Synchronisation
- [JsonSerializer Options](https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/configure-options) – JSON-Formatierung

---

#### Issue: Charakterdaten beim Login laden

**Labels:** `type:feature`, `area:persistenz`, `area:server`, `priority:p1`

**Beschreibung:**
Beim Login-Vorgang prüfen, ob ein Charakter mit dem angegebenen Namen existiert. Falls ja, werden die gespeicherten Daten geladen (Position, Stats). Falls nein, wird ein neuer Charakter erstellt und gespeichert.

**Aufgaben:**
- [ ] **Login-Handler erweitern:**
  - [ ] `ICharacterRepository` als Dependency injecten
  - [ ] Bestehenden Character laden oder neuen erstellen:
    ```csharp
    public async Task HandleLoginAsync(ClientConnection connection, LoginRequest request)
    {
        var existingCharacter = await _characterRepository.GetByNameAsync(request.UserName);
        
        CharacterData character;
        if (existingCharacter != null)
        {
            // Existing character - load saved position
            character = existingCharacter with { LastLoginAt = DateTime.UtcNow };
            _logger.LogInformation("Player {Name} loaded at ({X}, {Y})", 
                character.Name, character.X, character.Y);
        }
        else
        {
            // New character - create with spawn position
            var spawnPos = _world.GetSpawnPosition();
            character = CharacterData.CreateNew(request.UserName, spawnPos.X, spawnPos.Y);
            _logger.LogInformation("New player {Name} created at spawn", character.Name);
        }
        
        // Create Player entity from CharacterData
        var player = _world.CreatePlayer(character, connection);
        
        // Save updated character (LastLoginAt)
        await _characterRepository.SaveAsync(character);
        
        // Send response
        var response = new LoginResponse
        {
            Success = true,
            PlayerId = player.Id,
            SpawnX = character.X,
            SpawnY = character.Y
        };
        await connection.SendAsync(response);
    }
    ```
- [ ] **World.CreatePlayer() erweitern:**
  - [ ] Überladung die `CharacterData` akzeptiert:
    ```csharp
    public Player CreatePlayer(CharacterData data, ClientConnection connection)
    {
        var player = new Player
        {
            Id = data.Id,
            Name = data.Name,
            X = data.X,
            Y = data.Y,
            Connection = connection
        };
        _players[connection.Id] = player;
        return player;
    }
    ```
- [ ] **Error-Handling:**
  - [ ] Try-Catch um Repository-Aufrufe
  - [ ] Bei Load-Fehler: Warnung loggen, Spawn-Position verwenden
  - [ ] Bei kritischem Fehler: LoginResponse mit Success=false

**Akzeptanzkriterien:**
- [ ] Existierender Charakter behält seine letzte Position
- [ ] Neuer Charakter startet an Spawn-Position
- [ ] LastLoginAt wird bei jedem Login aktualisiert
- [ ] Fehler beim Laden crashen den Login nicht

**Ressourcen:**
- [Null-Conditional Operator](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/member-access-operators#null-conditional-operators--and-) – Sicherer Null-Check
- [Exception Handling](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/exceptions/exception-handling) – Try-Catch Patterns
- [Logging Best Practices](https://learn.microsoft.com/en-us/dotnet/core/extensions/logging#log-message-template) – Strukturierte Log-Messages
- [with-Expressions](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/with-expression) – Records modifizieren

---

#### Issue: Charakterdaten beim Logout/Disconnect speichern

**Labels:** `type:feature`, `area:persistenz`, `area:server`, `priority:p1`

**Beschreibung:**
Bei Logout oder unerwartetem Verbindungsabbruch die aktuellen Charakterdaten (Position, Stats) speichern. Auch beim Server-Shutdown sollen alle aktiven Spieler gespeichert werden.

**Aufgaben:**
- [ ] **Disconnect-Handler erweitern:**
  - [ ] In `NetworkServer.OnClientDisconnected`:
    ```csharp
    private async Task OnClientDisconnected(ClientConnection connection)
    {
        var player = _world.GetPlayerByConnection(connection);
        if (player != null)
        {
            await SavePlayerAsync(player);
            _world.RemovePlayerByConnection(connection);
            _logger.LogInformation("Player {Name} disconnected and saved", player.Name);
        }
    }
    ```
- [ ] **SavePlayerAsync Methode:**
  ```csharp
  private async Task SavePlayerAsync(Player player)
  {
      try
      {
          var existingData = await _characterRepository.GetByIdAsync(player.Id);
          if (existingData != null)
          {
              var updatedData = existingData.WithPosition(player.X, player.Y);
              await _characterRepository.SaveAsync(updatedData);
          }
      }
      catch (Exception ex)
      {
          _logger.LogError(ex, "Failed to save player {Name}", player.Name);
          // Don't rethrow - disconnect should complete even if save fails
      }
  }
  ```
- [ ] **Server-Shutdown-Handler:**
  - [ ] `IHostApplicationLifetime` für Shutdown-Event nutzen:
    ```csharp
    public class GameServer : IHostedService
    {
        private readonly IHostApplicationLifetime _appLifetime;
        
        public GameServer(IHostApplicationLifetime appLifetime)
        {
            _appLifetime = appLifetime;
            _appLifetime.ApplicationStopping.Register(OnShutdown);
        }
        
        private void OnShutdown()
        {
            _logger.LogInformation("Server shutting down, saving all players...");
            var saveTasks = _world.GetAllPlayers()
                .Select(p => SavePlayerAsync(p));
            Task.WhenAll(saveTasks).GetAwaiter().GetResult();
            _logger.LogInformation("All players saved");
        }
    }
    ```
- [ ] **Periodisches Auto-Save (optional):**
  - [ ] Timer alle 5 Minuten
  - [ ] Alle aktiven Spieler speichern
  - [ ] Logging der gespeicherten Anzahl
- [ ] **Tests:**
  - [ ] Test: Disconnect speichert Position
  - [ ] Test: Shutdown speichert alle Spieler
  - [ ] Test: Save-Fehler unterbricht Disconnect nicht

**Akzeptanzkriterien:**
- [ ] Position wird bei normalem Logout gespeichert
- [ ] Position wird bei Verbindungsabbruch gespeichert
- [ ] Server-Shutdown speichert alle aktiven Spieler
- [ ] Save-Fehler führen nicht zu Crashes

**Ressourcen:**
- [IHostApplicationLifetime](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.hosting.ihostapplicationlifetime) – Application Lifecycle Events
- [Task.WhenAll](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task.whenall) – Parallele Task-Ausführung
- [Background Tasks](https://learn.microsoft.com/en-us/dotnet/core/extensions/timer-service) – Timer für Auto-Save
- [Graceful Shutdown](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services#consuming-a-scoped-service-in-a-background-task) – Sauberes Herunterfahren

---

#### Issue: Fehlerbehandlung für Persistenz

**Labels:** `type:feature`, `area:persistenz`, `area:server`, `priority:p2`

**Beschreibung:**
Robuste Fehlerbehandlung für alle Persistenz-Operationen implementieren. Der Server soll bei Datei-/Datenbankfehlern stabil bleiben und Spieler sollen weiterspielen können.

**Aufgaben:**
- [ ] **PersistenceException definieren:**
  ```csharp
  public class PersistenceException : Exception
  {
      public PersistenceOperation Operation { get; }
      public string? CharacterName { get; }
      
      public PersistenceException(PersistenceOperation op, string? name, Exception inner)
          : base($"Persistence error during {op} for {name ?? "unknown"}", inner)
      {
          Operation = op;
          CharacterName = name;
      }
  }
  
  public enum PersistenceOperation { Load, Save, Delete }
  ```
- [ ] **Retry-Logik im Repository:**
  ```csharp
  public async Task SaveAsync(CharacterData character)
  {
      const int maxRetries = 3;
      for (int attempt = 1; attempt <= maxRetries; attempt++)
      {
          try
          {
              await SaveInternalAsync(character);
              return;
          }
          catch (IOException ex) when (attempt < maxRetries)
          {
              _logger.LogWarning("Save attempt {Attempt} failed, retrying...", attempt);
              await Task.Delay(100 * attempt); // Exponential backoff
          }
      }
      throw new PersistenceException(PersistenceOperation.Save, character.Name, lastException);
  }
  ```
- [ ] **Korrupte Daten erkennen:**
  ```csharp
  public async Task<CharacterData?> GetByNameAsync(string name)
  {
      try
      {
          var json = await File.ReadAllTextAsync(GetFilePath(name));
          return JsonSerializer.Deserialize<CharacterData>(json, _jsonOptions);
      }
      catch (JsonException ex)
      {
          _logger.LogError(ex, "Corrupt character file for {Name}, creating backup", name);
          await CreateBackupAsync(name);
          return null; // Treat as new character
      }
  }
  ```
- [ ] **Backup-Mechanismus:**
  ```csharp
  private async Task CreateBackupAsync(string name)
  {
      var source = GetFilePath(name);
      var backup = $"{source}.corrupt.{DateTime.UtcNow:yyyyMMddHHmmss}";
      if (File.Exists(source))
      {
          File.Move(source, backup);
      }
  }
  ```
- [ ] **Graceful Degradation im Login:**
  - [ ] Bei Load-Fehler: Neuen Character erstellen
  - [ ] Warnung an Spieler (optional)
  - [ ] Incident loggen für spätere Analyse
- [ ] **Monitoring-Events:**
  - [ ] Counter für erfolgreiche/fehlgeschlagene Saves
  - [ ] Logging bei kritischen Fehlern
  - [ ] Optional: Health-Check-Endpoint

**Akzeptanzkriterien:**
- [ ] Server crashed nicht bei IO-Fehlern
- [ ] Retry bei temporären Fehlern (z.B. File-Lock)
- [ ] Korrupte Dateien werden als Backup gesichert
- [ ] Spieler können weiterspielen auch bei Persistenz-Fehlern

**Ressourcen:**
- [Exception Types in .NET](https://learn.microsoft.com/en-us/dotnet/standard/exceptions/) – Custom Exceptions
- [Polly for Resilience](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/implement-http-call-retries-exponential-backoff-polly) – Retry-Patterns
- [IOException Handling](https://learn.microsoft.com/en-us/dotnet/api/system.io.ioexception) – Datei-Fehler
- [Health Checks](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks) – Monitoring

---

### Phase 4 – Chat-System

---

#### Issue: Chat-Nachrichtentypen im Shared-Projekt

**Labels:** `type:feature`, `area:chat`, `priority:p1`

**Beschreibung:**
Die Nachrichtentypen für das Chat-System im Shared-Projekt definieren. Diese DTOs werden von Server und Client für das Senden und Empfangen von Chat-Nachrichten verwendet.

**Aufgaben:**
- [ ] **ChatChannel-Enum erstellen:**
  - [ ] Datei: `shared/Mmo.Shared/Enums/ChatChannel.cs`
    ```csharp
    namespace Mmo.Shared.Enums;
    
    public enum ChatChannel
    {
        Global = 0,   // An alle Spieler
        Zone = 1,     // An Spieler in der gleichen Zone (später)
        Whisper = 2,  // Privat an einen Spieler (später)
        System = 3    // Server-Nachrichten (Login, etc.)
    }
    ```
- [ ] **ChatMessageRequest DTO:**
  - [ ] Datei: `shared/Mmo.Shared/Messages/ChatMessageRequest.cs`
    ```csharp
    namespace Mmo.Shared.Messages;
    
    public class ChatMessageRequest : INetworkMessage
    {
        public MessageType Type => MessageType.ChatMessageRequest;
        public ChatChannel Channel { get; set; } = ChatChannel.Global;
        public required string Message { get; set; }
        public Guid? TargetPlayerId { get; set; } // Für Whisper
    }
    ```
- [ ] **ChatMessageBroadcast DTO:**
  - [ ] Datei: `shared/Mmo.Shared/Messages/ChatMessageBroadcast.cs`
    ```csharp
    namespace Mmo.Shared.Messages;
    
    public class ChatMessageBroadcast : INetworkMessage
    {
        public MessageType Type => MessageType.ChatMessageBroadcast;
        public required Guid SenderId { get; set; }
        public required string SenderName { get; set; }
        public ChatChannel Channel { get; set; }
        public required string Message { get; set; }
        public DateTime Timestamp { get; set; }
    }
    ```
- [ ] **MessageType-Enum erweitern:**
  ```csharp
  public enum MessageType
  {
      // ... existing types ...
      ChatMessageRequest = 10,
      ChatMessageBroadcast = 11
  }
  ```
- [ ] **Validierungskonstanten:**
  ```csharp
  public static class ChatConstants
  {
      public const int MaxMessageLength = 200;
      public const int MinMessageLength = 1;
  }
  ```
- [ ] **MessageSerializer erweitern:**
  - [ ] Neue MessageTypes in Deserialisierung aufnehmen

**Akzeptanzkriterien:**
- [ ] DTOs sind im Shared-Projekt vorhanden
- [ ] JSON-Serialisierung funktioniert
- [ ] MessageType-Enum enthält Chat-Typen
- [ ] Konstanten für Validierung sind definiert

**Ressourcen:**
- [Enums in C#](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/enum) – Enumerationen definieren
- [Required Properties](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/required) – Pflichtfelder
- [System.Text.Json](https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/overview) – JSON-Serialisierung
- [INetworkMessage Pattern](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/types/interfaces) – Interface-Implementierung

---

#### Issue: Serverseitiges Chat-Handling implementieren

**Labels:** `type:feature`, `area:chat`, `area:server`, `priority:p1`

**Beschreibung:**
Das Chat-Handling auf dem Server implementieren, das eingehende Nachrichten validiert, mit Metadaten anreichert und an alle relevanten Clients broadcastet.

**Aufgaben:**
- [ ] **ChatHandler-Klasse erstellen:**
  - [ ] Datei: `server/Mmo.Server/Handlers/ChatHandler.cs`
    ```csharp
    public class ChatHandler
    {
        private readonly World _world;
        private readonly ILogger<ChatHandler> _logger;
        
        public ChatHandler(World world, ILogger<ChatHandler> logger)
        {
            _world = world;
            _logger = logger;
        }
        
        public async Task<ChatMessageBroadcast?> HandleChatAsync(
            ClientConnection connection, 
            ChatMessageRequest request)
        {
            // Get sender
            var sender = _world.GetPlayerByConnection(connection);
            if (sender == null)
            {
                _logger.LogWarning("Chat from unknown connection {Id}", connection.Id);
                return null;
            }
            
            // Validate message
            if (!ValidateMessage(request.Message, out var error))
            {
                _logger.LogDebug("Invalid chat message from {Name}: {Error}", 
                    sender.Name, error);
                return null;
            }
            
            // Create broadcast
            var broadcast = new ChatMessageBroadcast
            {
                SenderId = sender.Id,
                SenderName = sender.Name,
                Channel = request.Channel,
                Message = SanitizeMessage(request.Message),
                Timestamp = DateTime.UtcNow
            };
            
            _logger.LogDebug("[{Channel}] {Name}: {Message}", 
                request.Channel, sender.Name, request.Message);
            
            return broadcast;
        }
    }
    ```
- [ ] **Validierung implementieren:**
  ```csharp
  private bool ValidateMessage(string message, out string error)
  {
      error = string.Empty;
      
      if (string.IsNullOrWhiteSpace(message))
      {
          error = "Message cannot be empty";
          return false;
      }
      
      if (message.Length > ChatConstants.MaxMessageLength)
      {
          error = $"Message too long (max {ChatConstants.MaxMessageLength})";
          return false;
      }
      
      return true;
  }
  ```
- [ ] **Sanitization implementieren:**
  ```csharp
  private string SanitizeMessage(string message)
  {
      // Trim whitespace
      message = message.Trim();
      // Remove control characters
      message = Regex.Replace(message, @"[\x00-\x1F]", "");
      // Limit consecutive whitespace
      message = Regex.Replace(message, @"\s+", " ");
      return message;
  }
  ```
- [ ] **Im MessageRouter registrieren:**
  ```csharp
  case MessageType.ChatMessageRequest:
      var broadcast = await _chatHandler.HandleChatAsync(connection, (ChatMessageRequest)message);
      if (broadcast != null)
      {
          await BroadcastToAllAsync(broadcast);
      }
      break;
  ```
- [ ] **Broadcast-Methode:**
  ```csharp
  private async Task BroadcastToAllAsync(INetworkMessage message)
  {
      var tasks = _world.GetAllPlayers()
          .Select(p => p.Connection.SendAsync(message));
      await Task.WhenAll(tasks);
  }
  ```

**Akzeptanzkriterien:**
- [ ] Chat-Nachrichten werden an alle Spieler gesendet
- [ ] Absendername wird korrekt angezeigt
- [ ] Leere/zu lange Nachrichten werden abgelehnt
- [ ] Nachrichten werden sanitized (keine Control-Chars)

**Ressourcen:**
- [Regex in C#](https://learn.microsoft.com/en-us/dotnet/standard/base-types/regular-expressions) – Text-Manipulation
- [Task.WhenAll](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task.whenall) – Paralleles Senden
- [String Sanitization](https://learn.microsoft.com/en-us/dotnet/api/system.text.regularexpressions.regex.replace) – Regex.Replace
- [Input Validation](https://learn.microsoft.com/en-us/aspnet/core/mvc/models/validation) – Validierungsmuster

---

#### Issue: Einfaches Rate-Limiting für Chat

**Labels:** `type:feature`, `area:chat`, `area:server`, `priority:p2`

**Beschreibung:**
Ein einfaches Rate-Limiting implementieren, um Chat-Spam zu verhindern. Pro Spieler wird ein Nachrichtenzähler geführt, der nach einem Zeitfenster zurückgesetzt wird.

**Aufgaben:**
- [ ] **RateLimiter-Klasse erstellen:**
  - [ ] Datei: `server/Mmo.Server/Services/ChatRateLimiter.cs`
    ```csharp
    public class ChatRateLimiter
    {
        private readonly ConcurrentDictionary<Guid, PlayerRateInfo> _playerRates = new();
        private readonly int _maxMessages;
        private readonly TimeSpan _timeWindow;
        
        public ChatRateLimiter(int maxMessages = 5, int windowSeconds = 10)
        {
            _maxMessages = maxMessages;
            _timeWindow = TimeSpan.FromSeconds(windowSeconds);
        }
        
        public bool TryConsume(Guid playerId, out TimeSpan waitTime)
        {
            waitTime = TimeSpan.Zero;
            var now = DateTime.UtcNow;
            
            var info = _playerRates.GetOrAdd(playerId, _ => new PlayerRateInfo());
            
            lock (info)
            {
                // Reset if window expired
                if (now - info.WindowStart > _timeWindow)
                {
                    info.WindowStart = now;
                    info.MessageCount = 0;
                }
                
                // Check limit
                if (info.MessageCount >= _maxMessages)
                {
                    waitTime = _timeWindow - (now - info.WindowStart);
                    return false;
                }
                
                info.MessageCount++;
                return true;
            }
        }
        
        private class PlayerRateInfo
        {
            public DateTime WindowStart { get; set; } = DateTime.UtcNow;
            public int MessageCount { get; set; } = 0;
        }
    }
    ```
- [ ] **RateLimitExceededResponse DTO:**
  ```csharp
  public class ChatRateLimitResponse : INetworkMessage
  {
      public MessageType Type => MessageType.ChatRateLimit;
      public TimeSpan WaitTime { get; set; }
      public string Message => $"Bitte warte {WaitTime.Seconds} Sekunden";
  }
  ```
- [ ] **In ChatHandler integrieren:**
  ```csharp
  public async Task<INetworkMessage?> HandleChatAsync(...)
  {
      // Rate limit check
      if (!_rateLimiter.TryConsume(sender.Id, out var waitTime))
      {
          _logger.LogDebug("Player {Name} rate limited for {Seconds}s", 
              sender.Name, waitTime.TotalSeconds);
          return new ChatRateLimitResponse { WaitTime = waitTime };
      }
      
      // ... rest of handling
  }
  ```
- [ ] **Konfiguration über DI:**
  ```csharp
  services.AddSingleton(new ChatRateLimiter(
      maxMessages: config.Chat.MaxMessagesPerWindow,
      windowSeconds: config.Chat.WindowSeconds
  ));
  ```
- [ ] **Cleanup-Timer für inaktive Spieler:**
  - [ ] Alle 5 Minuten: Spieler ohne kürzliche Messages entfernen
  - [ ] Verhindert Memory-Leak bei vielen Spielern

**Akzeptanzkriterien:**
- [ ] Max 5 Nachrichten in 10 Sekunden (konfigurierbar)
- [ ] Bei Überschreitung: Wartezeit-Info an Client
- [ ] Normale Nutzung wird nicht beeinträchtigt
- [ ] Rate-Limiter ist thread-safe

**Ressourcen:**
- [ConcurrentDictionary](https://learn.microsoft.com/en-us/dotnet/api/system.collections.concurrent.concurrentdictionary-2) – Thread-sichere Dictionary
- [Rate Limiting in .NET](https://learn.microsoft.com/en-us/dotnet/core/extensions/http-ratelimiter) – Rate-Limiting-Konzepte
- [Lock Statement](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/statements/lock) – Thread-Synchronisation
- [Timer für Cleanup](https://learn.microsoft.com/en-us/dotnet/api/system.threading.timer) – Periodische Aufräumarbeiten

---

#### Issue: Chat-UI im Godot-Client erstellen

**Labels:** `type:feature`, `area:chat`, `area:client`, `priority:p1`

**Beschreibung:**
Die Chat-UI im Godot-Client implementieren mit Eingabefeld, Nachrichtenanzeige und Auto-Scroll. Das Chat-Panel soll unaufdringlich am unteren Bildschirmrand positioniert sein.

**Aufgaben:**
- [ ] **ChatPanel-Szene erstellen:**
  - [ ] Datei: `res://scenes/ui/ChatPanel.tscn`
  - [ ] Struktur:
    ```
    ChatPanel (Control)
    ├── Background (Panel)
    ├── VBoxContainer
    │   ├── MessageContainer (ScrollContainer)
    │   │   └── MessageList (VBoxContainer)
    │   └── InputContainer (HBoxContainer)
    │       ├── InputField (LineEdit)
    │       └── SendButton (Button)
    ```
- [ ] **Layout konfigurieren:**
  - [ ] Panel: Anker unten-links, Größe 400x200 Pixel
  - [ ] Halbtransparenter Hintergrund (Alpha 0.8)
  - [ ] Margin: 10px von Rand
- [ ] **ChatPanel.cs Script:**
  ```csharp
  public partial class ChatPanel : Control
  {
      [Export] private LineEdit _inputField;
      [Export] private VBoxContainer _messageList;
      [Export] private ScrollContainer _scrollContainer;
      
      private const int MaxMessages = 50;
      
      public override void _Ready()
      {
          _inputField.TextSubmitted += OnInputSubmitted;
          _inputField.FocusEntered += OnInputFocused;
          _inputField.FocusExited += OnInputUnfocused;
      }
      
      private void OnInputSubmitted(string text)
      {
          if (!string.IsNullOrWhiteSpace(text))
          {
              EmitSignal(SignalName.MessageSubmitted, text);
              _inputField.Clear();
          }
      }
      
      public void AddMessage(string senderName, string message, ChatChannel channel)
      {
          var label = new RichTextLabel();
          label.BbcodeEnabled = true;
          label.FitContent = true;
          label.Text = FormatMessage(senderName, message, channel);
          
          _messageList.AddChild(label);
          
          // Remove old messages
          while (_messageList.GetChildCount() > MaxMessages)
          {
              _messageList.GetChild(0).QueueFree();
          }
          
          // Scroll to bottom
          await ToSignal(GetTree(), "process_frame");
          _scrollContainer.ScrollVertical = (int)_scrollContainer.GetVScrollBar().MaxValue;
      }
      
      private string FormatMessage(string sender, string msg, ChatChannel ch)
      {
          var color = ch switch
          {
              ChatChannel.System => "gray",
              ChatChannel.Global => "white",
              _ => "white"
          };
          return $"[color={color}][b]{sender}:[/b] {msg}[/color]";
      }
      
      [Signal] public delegate void MessageSubmittedEventHandler(string message);
  }
  ```
- [ ] **Input-Fokus-Handling:**
  - [ ] Wenn Chat fokussiert: Bewegungs-Input deaktivieren
  - [ ] Enter öffnet Chat, Escape schließt
  ```csharp
  public override void _Input(InputEvent @event)
  {
      if (@event.IsActionPressed("chat_open") && !_inputField.HasFocus())
      {
          _inputField.GrabFocus();
          GetViewport().SetInputAsHandled();
      }
      else if (@event.IsActionPressed("ui_cancel") && _inputField.HasFocus())
      {
          _inputField.ReleaseFocus();
      }
  }
  ```
- [ ] **Input-Map konfigurieren:**
  - [ ] `chat_open`: Enter oder T
  - [ ] Dokumentieren in project.godot

**Akzeptanzkriterien:**
- [ ] Chat-Panel ist am unteren Bildschirmrand sichtbar
- [ ] Nachrichten können eingegeben werden
- [ ] Auto-Scroll bei neuen Nachrichten
- [ ] Maximale Nachrichtenanzahl wird begrenzt
- [ ] Enter öffnet Chat-Eingabe

**Ressourcen:**
- [Control Nodes in Godot](https://docs.godotengine.org/en/stable/tutorials/ui/control_node_gallery.html) – UI-Elemente
- [RichTextLabel](https://docs.godotengine.org/en/stable/classes/class_richtextlabel.html) – BBCode-formatierter Text
- [ScrollContainer](https://docs.godotengine.org/en/stable/classes/class_scrollcontainer.html) – Scrollbare Container
- [Signals in C#](https://docs.godotengine.org/en/stable/tutorials/scripting/c_sharp/c_sharp_signals.html) – Custom Signals

---

#### Issue: Chat-Nachrichten senden und empfangen im Client

**Labels:** `type:feature`, `area:chat`, `area:client`, `priority:p1`

**Beschreibung:**
Die Integration von Chat-Nachrichten zwischen ChatPanel und NetworkClient implementieren. Ausgehende Nachrichten werden an den Server gesendet, eingehende Broadcasts werden im Chat angezeigt.

**Aufgaben:**
- [ ] **ChatManager-Singleton erstellen:**
  - [ ] Datei: `res://scripts/singletons/ChatManager.cs`
    ```csharp
    public partial class ChatManager : Node
    {
        public static ChatManager Instance { get; private set; }
        
        private ChatPanel _chatPanel;
        private Guid _localPlayerId;
        
        public override void _Ready()
        {
            Instance = this;
            NetworkClient.Instance.MessageReceived += OnMessageReceived;
        }
        
        public void Initialize(ChatPanel chatPanel, Guid localPlayerId)
        {
            _chatPanel = chatPanel;
            _localPlayerId = localPlayerId;
            _chatPanel.MessageSubmitted += OnMessageSubmitted;
        }
        
        private void OnMessageSubmitted(string message)
        {
            var request = new ChatMessageRequest
            {
                Channel = ChatChannel.Global,
                Message = message
            };
            NetworkClient.Instance.Send(request);
        }
        
        private void OnMessageReceived(INetworkMessage message)
        {
            switch (message)
            {
                case ChatMessageBroadcast chat:
                    HandleChatBroadcast(chat);
                    break;
                case ChatRateLimitResponse rateLimit:
                    HandleRateLimit(rateLimit);
                    break;
            }
        }
        
        private void HandleChatBroadcast(ChatMessageBroadcast broadcast)
        {
            // Call on main thread
            CallDeferred(nameof(AddMessageToPanel), 
                broadcast.SenderName, 
                broadcast.Message, 
                (int)broadcast.Channel);
        }
        
        private void AddMessageToPanel(string sender, string msg, int channel)
        {
            _chatPanel.AddMessage(sender, msg, (ChatChannel)channel);
        }
        
        private void HandleRateLimit(ChatRateLimitResponse response)
        {
            _chatPanel.AddMessage("System", response.Message, ChatChannel.System);
        }
    }
    ```
- [ ] **System-Nachrichten hinzufügen:**
  ```csharp
  public void ShowSystemMessage(string message)
  {
      _chatPanel.AddMessage("System", message, ChatChannel.System);
  }
  
  // Called from GameManager
  public void ShowPlayerJoined(string playerName)
  {
      ShowSystemMessage($"{playerName} ist beigetreten.");
  }
  
  public void ShowPlayerLeft(string playerName)
  {
      ShowSystemMessage($"{playerName} hat das Spiel verlassen.");
  }
  ```
- [ ] **Als Autoload registrieren:**
  - [ ] In project.godot unter Autoload
  - [ ] Nach NetworkClient, vor GameManager
- [ ] **In Game-Szene integrieren:**
  - [ ] ChatPanel zur Game.tscn hinzufügen
  - [ ] Im GameManager: `ChatManager.Instance.Initialize(chatPanel, playerId)`
- [ ] **Lokale Echo-Nachricht (optional):**
  - [ ] Eigene Nachrichten sofort anzeigen (für Responsiveness)
  - [ ] Server-Broadcast trotzdem verarbeiten (für Konsistenz)
- [ ] **Thread-Safety:**
  - [ ] `CallDeferred` für UI-Updates aus Netzwerk-Thread
  - [ ] Keine Race-Conditions bei schnellen Nachrichten

**Akzeptanzkriterien:**
- [ ] Gesendete Nachrichten erscheinen bei allen Spielern
- [ ] Eigene Nachrichten werden angezeigt
- [ ] System-Nachrichten (Join/Leave) erscheinen
- [ ] Rate-Limit-Warnung wird angezeigt
- [ ] Keine UI-Freezes bei vielen Nachrichten

**Ressourcen:**
- [CallDeferred](https://docs.godotengine.org/en/stable/classes/class_object.html#class-object-method-call-deferred) – Thread-sichere Aufrufe
- [Signals zwischen Nodes](https://docs.godotengine.org/en/stable/tutorials/scripting/c_sharp/c_sharp_signals.html) – Event-Kommunikation
- [Autoload Singletons](https://docs.godotengine.org/en/stable/tutorials/scripting/singletons_autoload.html) – Globale Instanzen
- [Networking in Godot](https://docs.godotengine.org/en/stable/tutorials/networking/high_level_multiplayer.html) – Netzwerk-Grundlagen

---

### Phase 4 – Erste Gameplay-Aktion

---

#### Issue: Action-Nachrichtentypen definieren

**Labels:** `type:feature`, `area:gameplay`, `priority:p1`

**Beschreibung:**
Die Nachrichtentypen für Gameplay-Aktionen (Attack/Emote/Interact) im Shared-Projekt definieren. Diese DTOs ermöglichen dem Client, Aktionen an den Server zu senden und Events von anderen Spielern zu empfangen.

**Aufgaben:**
- [ ] **ActionType-Enum erstellen:**
  - [ ] Datei: `shared/Mmo.Shared/Enums/ActionType.cs`
    ```csharp
    namespace Mmo.Shared.Enums;
    
    public enum ActionType
    {
        None = 0,
        Attack = 1,
        Emote = 2,
        Interact = 3
    }
    ```
- [ ] **EmoteType-Enum erstellen:**
  - [ ] Datei: `shared/Mmo.Shared/Enums/EmoteType.cs`
    ```csharp
    namespace Mmo.Shared.Enums;
    
    public enum EmoteType
    {
        Wave = 0,
        Dance = 1,
        Sit = 2,
        Laugh = 3,
        Cry = 4
    }
    ```
- [ ] **ActionRequest DTO:**
  - [ ] Datei: `shared/Mmo.Shared/Messages/ActionRequest.cs`
    ```csharp
    namespace Mmo.Shared.Messages;
    
    public class ActionRequest : INetworkMessage
    {
        public MessageType Type => MessageType.ActionRequest;
        public ActionType ActionType { get; set; }
        public Guid? TargetId { get; set; }          // Für Attack
        public EmoteType? EmoteType { get; set; }     // Für Emote
        public float DirectionX { get; set; }         // Für Attack ohne Ziel
        public float DirectionY { get; set; }
    }
    ```
- [ ] **ActionEvent DTO (Server → Client):**
  - [ ] Datei: `shared/Mmo.Shared/Messages/ActionEvent.cs`
    ```csharp
    namespace Mmo.Shared.Messages;
    
    public class ActionEvent : INetworkMessage
    {
        public MessageType Type => MessageType.ActionEvent;
        public required Guid ActorId { get; set; }
        public ActionType ActionType { get; set; }
        public Guid? TargetId { get; set; }
        public EmoteType? EmoteType { get; set; }
        public ActionResult Result { get; set; }
        public float ActorX { get; set; }
        public float ActorY { get; set; }
    }
    
    public enum ActionResult
    {
        Success = 0,
        InvalidTarget = 1,
        OutOfRange = 2,
        OnCooldown = 3
    }
    ```
- [ ] **MessageType-Enum erweitern:**
  ```csharp
  public enum MessageType
  {
      // ... existing types ...
      ActionRequest = 20,
      ActionEvent = 21
  }
  ```
- [ ] **Konstanten für Gameplay:**
  ```csharp
  public static class ActionConstants
  {
      public const float AttackRange = 100f;
      public const float AttackCooldownMs = 500f;
      public const float EmoteDurationMs = 2000f;
  }
  ```

**Akzeptanzkriterien:**
- [ ] Alle DTOs und Enums sind definiert
- [ ] JSON-Serialisierung funktioniert für alle Typen
- [ ] MessageType-Enum enthält Action-Typen
- [ ] Konstanten sind dokumentiert

**Ressourcen:**
- [Nullable Reference Types](https://learn.microsoft.com/en-us/dotnet/csharp/nullable-references) – Optional Properties mit `?`
- [Enums mit Werten](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/enum) – Explizite Enum-Werte
- [System.Text.Json Enums](https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/customize-properties#enums-as-strings) – Enum-Serialisierung
- [Game Design Patterns](https://learn.microsoft.com/en-us/gaming/) – Spielmechanik-Grundlagen

---

#### Issue: Action-Handler im Server implementieren

**Labels:** `type:feature`, `area:gameplay`, `area:server`, `priority:p1`

**Beschreibung:**
Die serverseitige Verarbeitung von Aktionen implementieren. Der Server validiert Aktionen, prüft Cooldowns und broadcastet Events an relevante Clients.

**Aufgaben:**
- [ ] **ActionHandler-Klasse erstellen:**
  - [ ] Datei: `server/Mmo.Server/Handlers/ActionHandler.cs`
    ```csharp
    public class ActionHandler
    {
        private readonly World _world;
        private readonly ILogger<ActionHandler> _logger;
        private readonly ConcurrentDictionary<Guid, DateTime> _attackCooldowns = new();
        
        public ActionHandler(World world, ILogger<ActionHandler> logger)
        {
            _world = world;
            _logger = logger;
        }
        
        public ActionEvent? HandleAction(ClientConnection connection, ActionRequest request)
        {
            var actor = _world.GetPlayerByConnection(connection);
            if (actor == null)
            {
                _logger.LogWarning("Action from unknown connection {Id}", connection.Id);
                return null;
            }
            
            return request.ActionType switch
            {
                ActionType.Attack => HandleAttack(actor, request),
                ActionType.Emote => HandleEmote(actor, request),
                ActionType.Interact => HandleInteract(actor, request),
                _ => null
            };
        }
    }
    ```
- [ ] **HandleAttack implementieren:**
  ```csharp
  private ActionEvent? HandleAttack(Player actor, ActionRequest request)
  {
      // Cooldown check
      if (_attackCooldowns.TryGetValue(actor.Id, out var lastAttack))
      {
          var cooldownRemaining = ActionConstants.AttackCooldownMs - 
              (DateTime.UtcNow - lastAttack).TotalMilliseconds;
          if (cooldownRemaining > 0)
          {
              return new ActionEvent
              {
                  ActorId = actor.Id,
                  ActionType = ActionType.Attack,
                  Result = ActionResult.OnCooldown
              };
          }
      }
      
      // Find target
      Entity? target = null;
      if (request.TargetId.HasValue)
      {
          target = _world.GetEntityById(request.TargetId.Value);
          if (target == null)
          {
              return new ActionEvent
              {
                  ActorId = actor.Id,
                  ActionType = ActionType.Attack,
                  Result = ActionResult.InvalidTarget
              };
          }
          
          // Range check
          var distance = Vector2.Distance(
              new Vector2(actor.X, actor.Y),
              new Vector2(target.X, target.Y));
          if (distance > ActionConstants.AttackRange)
          {
              return new ActionEvent
              {
                  ActorId = actor.Id,
                  ActionType = ActionType.Attack,
                  TargetId = target.Id,
                  Result = ActionResult.OutOfRange
              };
          }
      }
      
      // Update cooldown
      _attackCooldowns[actor.Id] = DateTime.UtcNow;
      
      _logger.LogDebug("Player {Name} attacks {Target}", 
          actor.Name, target?.Id.ToString() ?? "air");
      
      return new ActionEvent
      {
          ActorId = actor.Id,
          ActionType = ActionType.Attack,
          TargetId = target?.Id,
          Result = ActionResult.Success,
          ActorX = actor.X,
          ActorY = actor.Y
      };
  }
  ```
- [ ] **HandleEmote implementieren:**
  ```csharp
  private ActionEvent HandleEmote(Player actor, ActionRequest request)
  {
      _logger.LogDebug("Player {Name} uses emote {Emote}", 
          actor.Name, request.EmoteType);
      
      return new ActionEvent
      {
          ActorId = actor.Id,
          ActionType = ActionType.Emote,
          EmoteType = request.EmoteType,
          Result = ActionResult.Success,
          ActorX = actor.X,
          ActorY = actor.Y
      };
  }
  ```
- [ ] **Im MessageRouter registrieren:**
  ```csharp
  case MessageType.ActionRequest:
      var actionEvent = _actionHandler.HandleAction(connection, (ActionRequest)message);
      if (actionEvent != null)
      {
          await BroadcastInRangeAsync(actionEvent, actionEvent.ActorX, actionEvent.ActorY);
      }
      break;
  ```
- [ ] **Range-basierter Broadcast:**
  ```csharp
  private async Task BroadcastInRangeAsync(INetworkMessage msg, float x, float y, float range = 500f)
  {
      var nearbyPlayers = _world.GetPlayersInRange(x, y, range);
      var tasks = nearbyPlayers.Select(p => p.Connection.SendAsync(msg));
      await Task.WhenAll(tasks);
  }
  ```

**Akzeptanzkriterien:**
- [ ] Attack-Aktionen werden mit Cooldown verarbeitet
- [ ] Emote-Aktionen werden sofort verarbeitet
- [ ] Range-Check verhindert Angriffe auf entfernte Ziele
- [ ] ActionEvents werden an nahe Spieler gebroadcastet

**Ressourcen:**
- [ConcurrentDictionary](https://learn.microsoft.com/en-us/dotnet/api/system.collections.concurrent.concurrentdictionary-2) – Thread-sichere Cooldown-Speicherung
- [Pattern Matching](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/functional/pattern-matching) – Switch-Expressions
- [Vector2 Distance](https://learn.microsoft.com/en-us/dotnet/api/system.numerics.vector2.distance) – Distanzberechnung
- [LINQ Select](https://learn.microsoft.com/en-us/dotnet/api/system.linq.enumerable.select) – Projektion für Broadcasts

---

#### Issue: Dummy-Mob für Attack-Ziel erstellen

**Labels:** `type:feature`, `area:gameplay`, `area:server`, `priority:p2`

**Beschreibung:**
Einen einfachen Dummy-Mob auf dem Server erstellen, der als Angriffsziel für Tests dient. Der Mob hat eine Position, kann angegriffen werden und reagiert visuell auf Angriffe.

**Aufgaben:**
- [ ] **Entity-Basisklasse erstellen:**
  - [ ] Datei: `server/Mmo.Server/Entities/Entity.cs`
    ```csharp
    public abstract class Entity
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public float X { get; set; }
        public float Y { get; set; }
        public abstract EntityType EntityType { get; }
    }
    
    public enum EntityType
    {
        Player = 0,
        Mob = 1,
        Npc = 2,
        Object = 3
    }
    ```
- [ ] **Mob-Klasse erstellen:**
  - [ ] Datei: `server/Mmo.Server/Entities/Mob.cs`
    ```csharp
    public class Mob : Entity
    {
        public override EntityType EntityType => EntityType.Mob;
        public required string Name { get; init; }
        public MobType MobType { get; init; }
        public int Health { get; set; } = 100;
        public int MaxHealth { get; init; } = 100;
        public bool IsAlive => Health > 0;
        public DateTime? DeathTime { get; set; }
        
        public void TakeDamage(int damage)
        {
            Health = Math.Max(0, Health - damage);
            if (!IsAlive)
            {
                DeathTime = DateTime.UtcNow;
            }
        }
        
        public void Respawn()
        {
            Health = MaxHealth;
            DeathTime = null;
        }
    }
    
    public enum MobType
    {
        Dummy = 0,
        Slime = 1,
        Goblin = 2
    }
    ```
- [ ] **World um Mob-Verwaltung erweitern:**
  ```csharp
  public class World
  {
      private readonly Dictionary<Guid, Mob> _mobs = new();
      
      public void SpawnDummyMob(float x, float y)
      {
          var mob = new Mob
          {
              Name = "Training Dummy",
              MobType = MobType.Dummy,
              X = x,
              Y = y,
              MaxHealth = 1000,
              Health = 1000
          };
          _mobs[mob.Id] = mob;
      }
      
      public Entity? GetEntityById(Guid id)
      {
          if (_players.Values.FirstOrDefault(p => p.Id == id) is Player player)
              return player;
          if (_mobs.TryGetValue(id, out var mob))
              return mob;
          return null;
      }
      
      public IEnumerable<Mob> GetAllMobs() => _mobs.Values;
  }
  ```
- [ ] **Mobs im WorldStateUpdate senden:**
  - [ ] `MobState` DTO erstellen:
    ```csharp
    public class MobState
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public MobType MobType { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public int Health { get; set; }
        public int MaxHealth { get; set; }
    }
    ```
  - [ ] In PlayerStateUpdate oder separater WorldStateUpdate
- [ ] **Dummy-Mob beim Server-Start spawnen:**
  ```csharp
  // In GameServer.StartAsync()
  _world.SpawnDummyMob(SpawnConfig.DefaultX + 100, SpawnConfig.DefaultY);
  _logger.LogInformation("Spawned training dummy");
  ```
- [ ] **Respawn-Logik (optional):**
  ```csharp
  public void Update(TimeSpan deltaTime)
  {
      foreach (var mob in _mobs.Values.Where(m => !m.IsAlive))
      {
          if (DateTime.UtcNow - mob.DeathTime > TimeSpan.FromSeconds(10))
          {
              mob.Respawn();
              _logger.LogDebug("Mob {Name} respawned", mob.Name);
          }
      }
  }
  ```

**Akzeptanzkriterien:**
- [ ] Dummy-Mob existiert in der Welt bei fester Position
- [ ] Mob erscheint im WorldStateUpdate
- [ ] Attack auf Mob reduziert Health
- [ ] Mob respawnt nach 10 Sekunden (optional)

**Ressourcen:**
- [Abstract Classes](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/abstract-and-sealed-classes-and-class-members) – Entity-Basisklasse
- [LINQ FirstOrDefault](https://learn.microsoft.com/en-us/dotnet/api/system.linq.enumerable.firstordefault) – Entity-Suche
- [Game Entity Patterns](https://learn.microsoft.com/en-us/archive/msdn-magazine/2015/march/c-game-programming-building-a-basic-game-engine) – Spielobjekte
- [DateTime Operations](https://learn.microsoft.com/en-us/dotnet/api/system.datetime) – Respawn-Timer

---

#### Issue: Action-Input im Client implementieren

**Labels:** `type:feature`, `area:gameplay`, `area:client`, `priority:p1`

**Beschreibung:**
Die Eingabebehandlung für Aktionen im Godot-Client implementieren. Tasten werden auf Aktionen gemappt, Ziele werden ermittelt und ActionRequests an den Server gesendet.

**Aufgaben:**
- [ ] **InputMap konfigurieren:**
  - [ ] In project.godot:
    ```
    [input]
    action_attack={
        "deadzone": 0.5,
        "events": [Object(InputEventKey,"resource_local_to_scene":false,"resource_name":"","device":-1,"window_id":0,"alt_pressed":false,"shift_pressed":false,"ctrl_pressed":false,"meta_pressed":false,"pressed":false,"keycode":32,"physical_keycode":0,"unicode":32)]
    }
    action_emote={
        "deadzone": 0.5,
        "events": [Object(InputEventKey,"resource_local_to_scene":false,"resource_name":"","device":-1,"window_id":0,"alt_pressed":false,"shift_pressed":false,"ctrl_pressed":false,"meta_pressed":false,"pressed":false,"keycode":69,"physical_keycode":0,"unicode":101)]
    }
    ```
  - [ ] `action_attack`: Leertaste (Space)
  - [ ] `action_emote`: E-Taste
- [ ] **ActionController-Script erstellen:**
  - [ ] Datei: `res://scripts/player/ActionController.cs`
    ```csharp
    public partial class ActionController : Node
    {
        private GameManager _gameManager;
        private DateTime _lastAttackTime = DateTime.MinValue;
        private EmoteType _currentEmote = EmoteType.Wave;
        
        public override void _Ready()
        {
            _gameManager = GetNode<GameManager>("/root/GameManager");
        }
        
        public override void _Process(double delta)
        {
            if (Input.IsActionJustPressed("action_attack"))
            {
                TryAttack();
            }
            
            if (Input.IsActionJustPressed("action_emote"))
            {
                TryEmote();
            }
            
            // Cycle emotes with number keys (optional)
            for (int i = 1; i <= 5; i++)
            {
                if (Input.IsActionJustPressed($"emote_{i}"))
                {
                    _currentEmote = (EmoteType)(i - 1);
                }
            }
        }
        
        private void TryAttack()
        {
            // Client-side cooldown check
            var timeSinceLastAttack = DateTime.UtcNow - _lastAttackTime;
            if (timeSinceLastAttack.TotalMilliseconds < ActionConstants.AttackCooldownMs)
            {
                return; // Still on cooldown
            }
            
            var target = FindNearestTarget();
            
            var request = new ActionRequest
            {
                ActionType = ActionType.Attack,
                TargetId = target?.Id
            };
            
            NetworkClient.Instance.Send(request);
            _lastAttackTime = DateTime.UtcNow;
        }
        
        private void TryEmote()
        {
            var request = new ActionRequest
            {
                ActionType = ActionType.Emote,
                EmoteType = _currentEmote
            };
            
            NetworkClient.Instance.Send(request);
        }
    }
    ```
- [ ] **Target-Finding implementieren:**
  ```csharp
  private Entity? FindNearestTarget()
  {
      var localPlayer = _gameManager.LocalPlayer;
      if (localPlayer == null) return null;
      
      var playerPos = localPlayer.GlobalPosition;
      Entity? nearest = null;
      float nearestDist = ActionConstants.AttackRange;
      
      // Check mobs
      foreach (var mob in _gameManager.GetVisibleMobs())
      {
          var dist = playerPos.DistanceTo(mob.GlobalPosition);
          if (dist < nearestDist)
          {
              nearestDist = dist;
              nearest = mob.EntityData;
          }
      }
      
      return nearest;
  }
  ```
- [ ] **Targeting-Indikator (optional):**
  - [ ] Kreis oder Highlight um das aktuelle Ziel
  - [ ] Tab-Taste zum Durchschalten von Zielen
- [ ] **Cooldown-UI-Feedback:**
  - [ ] Grauer Button/Icon während Cooldown
  - [ ] Oder: kleine Fortschrittsanzeige

**Akzeptanzkriterien:**
- [ ] Leertaste löst Attack aus
- [ ] E-Taste löst aktuelles Emote aus
- [ ] Nächstes Ziel im Range wird automatisch getroffen
- [ ] Cooldown verhindert Spam
- [ ] Keine Aktion wenn Chat fokussiert

**Ressourcen:**
- [Input in Godot](https://docs.godotengine.org/en/stable/tutorials/inputs/input_examples.html) – Input-Handling
- [InputMap](https://docs.godotengine.org/en/stable/classes/class_inputmap.html) – Tastenbelegung definieren
- [IsActionJustPressed](https://docs.godotengine.org/en/stable/classes/class_input.html#class-input-method-is-action-just-pressed) – Einmaliger Tastendruck
- [Vector2.DistanceTo](https://docs.godotengine.org/en/stable/classes/class_vector2.html#class-vector2-method-distance-to) – Distanz berechnen

---

#### Issue: Action-Visualisierung im Client

**Labels:** `type:feature`, `area:gameplay`, `area:client`, `priority:p1`

**Beschreibung:**
Die visuelle Darstellung von Aktionen im Godot-Client implementieren. Spieler sehen Angriffs-Effekte, Emote-Icons und Reaktionen von getroffenen Entities.

**Aufgaben:**
- [ ] **ActionEventHandler im Client:**
  - [ ] Datei: `res://scripts/managers/ActionVisualizer.cs`
    ```csharp
    public partial class ActionVisualizer : Node
    {
        [Export] private PackedScene _attackEffectScene;
        [Export] private PackedScene _emotePopupScene;
        
        private GameManager _gameManager;
        
        public override void _Ready()
        {
            _gameManager = GetNode<GameManager>("/root/GameManager");
            NetworkClient.Instance.MessageReceived += OnMessageReceived;
        }
        
        private void OnMessageReceived(INetworkMessage message)
        {
            if (message is ActionEvent actionEvent)
            {
                CallDeferred(nameof(HandleActionEvent), actionEvent);
            }
        }
        
        private void HandleActionEvent(ActionEvent evt)
        {
            switch (evt.ActionType)
            {
                case ActionType.Attack:
                    ShowAttackEffect(evt);
                    break;
                case ActionType.Emote:
                    ShowEmotePopup(evt);
                    break;
            }
        }
    }
    ```
- [ ] **Attack-Effekt erstellen:**
  - [ ] `AttackEffect.tscn` Szene:
    ```
    AttackEffect (Node2D)
    ├── Sprite2D (Slash-Grafik oder Kreis)
    ├── AnimationPlayer
    └── Timer (AutoStart, 0.3s → QueueFree)
    ```
  - [ ] Im Script:
    ```csharp
    private void ShowAttackEffect(ActionEvent evt)
    {
        var actor = _gameManager.GetPlayerNode(evt.ActorId);
        if (actor == null) return;
        
        var effect = _attackEffectScene.Instantiate<Node2D>();
        actor.AddChild(effect);
        effect.Position = Vector2.Zero; // Relativ zum Spieler
        
        // Animation starten
        var anim = effect.GetNode<AnimationPlayer>("AnimationPlayer");
        anim.Play("slash");
        
        // Target-Reaktion
        if (evt.TargetId.HasValue)
        {
            var target = _gameManager.GetEntityNode(evt.TargetId.Value);
            if (target != null)
            {
                ShowDamageReaction(target);
            }
        }
    }
    
    private void ShowDamageReaction(Node2D target)
    {
        // Kurzes Blinken
        var tween = CreateTween();
        tween.TweenProperty(target, "modulate", new Color(1, 0.3f, 0.3f), 0.1f);
        tween.TweenProperty(target, "modulate", Colors.White, 0.1f);
    }
    ```
- [ ] **Emote-Popup erstellen:**
  - [ ] `EmotePopup.tscn` Szene:
    ```
    EmotePopup (Control)
    ├── Label oder TextureRect (Emote-Icon)
    └── Timer (2s → QueueFree)
    ```
  - [ ] Im Script:
    ```csharp
    private void ShowEmotePopup(ActionEvent evt)
    {
        var actor = _gameManager.GetPlayerNode(evt.ActorId);
        if (actor == null) return;
        
        var popup = _emotePopupScene.Instantiate<Control>();
        actor.AddChild(popup);
        popup.Position = new Vector2(0, -50); // Über dem Spieler
        
        var label = popup.GetNode<Label>("Label");
        label.Text = GetEmoteText(evt.EmoteType ?? EmoteType.Wave);
        
        // Aufwärts-Animation
        var tween = CreateTween();
        tween.TweenProperty(popup, "position:y", popup.Position.Y - 20, 1.5f);
        tween.Parallel().TweenProperty(popup, "modulate:a", 0f, 1.5f);
    }
    
    private string GetEmoteText(EmoteType emote) => emote switch
    {
        EmoteType.Wave => "👋",
        EmoteType.Dance => "💃",
        EmoteType.Sit => "🪑",
        EmoteType.Laugh => "😂",
        EmoteType.Cry => "😢",
        _ => "❓"
    };
    ```
- [ ] **Sound-Effekte (optional):**
  - [ ] `AudioStreamPlayer2D` für Angriffsgeräusch
  - [ ] Verschiedene Sounds pro Emote
- [ ] **Performance-Optimierung:**
  - [ ] Object-Pooling für häufige Effekte
  - [ ] Effekte außerhalb des Sichtbereichs ignorieren

**Akzeptanzkriterien:**
- [ ] Attack zeigt visuellen Slash-Effekt am Angreifer
- [ ] Getroffene Ziele blinken rot
- [ ] Emotes zeigen Emoji/Text über dem Spieler
- [ ] Effekte verschwinden nach kurzer Zeit
- [ ] Keine Performance-Probleme bei vielen gleichzeitigen Aktionen

**Ressourcen:**
- [AnimationPlayer](https://docs.godotengine.org/en/stable/classes/class_animationplayer.html) – Animationen abspielen
- [Tween in Godot 4](https://docs.godotengine.org/en/stable/classes/class_tween.html) – Einfache Animationen
- [Particle Systems](https://docs.godotengine.org/en/stable/tutorials/2d/particle_systems_2d.html) – Partikel-Effekte
- [Object Pooling Pattern](https://docs.godotengine.org/en/stable/tutorials/best_practices/scenes_versus_scripts.html) – Performance-Optimierung

---

### Phase 4 – Abschluss & Refactoring

---

#### Issue: Code-Cleanup und Naming-Konventionen

**Labels:** `type:chore`, `priority:p2`

**Beschreibung:**
Code aufräumen, einheitliche Naming-Konventionen anwenden und technische Schulden abbauen. Der Code soll den Microsoft C#-Richtlinien entsprechen und wartbar sein.

**Aufgaben:**
- [ ] **Naming-Konventionen prüfen:**
  - [ ] Klassen: PascalCase (z.B. `GameServer`, `PlayerController`)
  - [ ] Methoden: PascalCase (z.B. `HandleLogin`, `SendAsync`)
  - [ ] Private Felder: _camelCase (z.B. `_players`, `_logger`)
  - [ ] Lokale Variablen: camelCase (z.B. `player`, `message`)
  - [ ] Konstanten: PascalCase (z.B. `MaxPlayers`, `DefaultPort`)
  - [ ] Interfaces: IPascalCase (z.B. `INetworkMessage`, `ICharacterRepository`)
- [ ] **Code-Analyse durchführen:**
  - [ ] `dotnet format` ausführen für Formatierung
  - [ ] Warnings aktivieren in `.csproj`:
    ```xml
    <PropertyGroup>
      <TreatWarningsAsErrors>false</TreatWarningsAsErrors>
      <WarningLevel>5</WarningLevel>
      <Nullable>enable</Nullable>
    </PropertyGroup>
    ```
  - [ ] Alle Compiler-Warnings auflisten und beheben
- [ ] **Unused Code entfernen:**
  - [ ] Nicht verwendete `using` Statements
  - [ ] Auskommentierter Code (nach Git-History prüfen)
  - [ ] Nicht aufgerufene Methoden
  - [ ] Leere Catch-Blöcke dokumentieren oder füllen
- [ ] **TODO-Kommentare auflösen:**
  - [ ] Alle `// TODO:` suchen
  - [ ] Entweder implementieren oder als Issue erfassen
  - [ ] `// HACK:` durch saubere Lösung ersetzen
- [ ] **Code-Dokumentation:**
  - [ ] XML-Kommentare für öffentliche APIs
  - [ ] Kurze Inline-Kommentare für komplexe Logik
  - [ ] Keine offensichtlichen Kommentare (z.B. "// increment i")
- [ ] **Datei-Organisation:**
  - [ ] Eine Klasse pro Datei (Ausnahme: kleine Helper-Klassen)
  - [ ] Ordnerstruktur entspricht Namespaces
  - [ ] `global using` für häufig verwendete Namespaces

**Akzeptanzkriterien:**
- [ ] `dotnet build` zeigt keine Warnings
- [ ] Keine TODO/HACK-Kommentare ohne Issue-Referenz
- [ ] Einheitliche Formatierung im gesamten Projekt
- [ ] Nullable Reference Types aktiviert und Warnings behoben

**Ressourcen:**
- [C# Coding Conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions) – Offizielle Microsoft-Richtlinien
- [.NET Naming Guidelines](https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/naming-guidelines) – Namenskonventionen
- [dotnet format](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-format) – Code-Formatierung
- [Nullable Reference Types](https://learn.microsoft.com/en-us/dotnet/csharp/nullable-references) – Null-Safety

---

#### Issue: README und Dokumentation aktualisieren

**Labels:** `type:documentation`, `priority:p1`

**Beschreibung:**
Die README und grundlegende Dokumentation für Phase 4 aktualisieren, damit neue Entwickler das Projekt verstehen und schnell starten können.

**Aufgaben:**
- [ ] **README.md aktualisieren:**
  - [ ] Projektbeschreibung (Was ist 2DMMO?)
  - [ ] Aktueller Feature-Stand nach Phase 4:
    ```markdown
    ## Features
    - ✅ Multiplayer Login/Logout
    - ✅ Echtzeit-Bewegungssynchronisation
    - ✅ Charakter-Persistenz (Name, Position)
    - ✅ Global-Chat
    - ✅ Basis-Aktionen (Attack, Emote)
    ```
  - [ ] Tech-Stack:
    ```markdown
    ## Tech-Stack
    - **Client:** Godot 4.x mit C#
    - **Server:** .NET 8 / C# 12
    - **Protokoll:** TCP mit JSON-Serialisierung
    - **Persistenz:** File-basiert (JSON)
    ```
- [ ] **Setup-Anleitung:**
  ```markdown
  ## Voraussetzungen
  - [.NET 8 SDK](https://dotnet.microsoft.com/download)
  - [Godot 4.x mit .NET](https://godotengine.org/download)
  
  ## Build & Run
  
  ### Server
  ```bash
  cd server/Mmo.Server
  dotnet run
  ```
  
  ### Client
  1. Godot öffnen
  2. Projekt `client/GodotProject` importieren
  3. F5 zum Starten
  ```
- [ ] **Architektur-Überblick:**
  ```markdown
  ## Architektur
  
  ```
  ┌─────────────┐     TCP/JSON     ┌─────────────┐
  │   Godot     │◄───────────────►│  .NET 8     │
  │   Client    │                  │   Server    │
  └─────────────┘                  └──────┬──────┘
                                          │
                                   ┌──────▼──────┐
                                   │   JSON      │
                                   │   Files     │
                                   └─────────────┘
  ```
  ```
- [ ] **Ordnerstruktur dokumentieren:**
  ```markdown
  ## Projektstruktur
  ```
  2DMMO/
  ├── server/
  │   └── Mmo.Server/         # .NET Server
  ├── shared/
  │   └── Mmo.Shared/         # Gemeinsame DTOs
  ├── client/
  │   └── GodotProject/       # Godot Client
  └── docs/                   # Dokumentation
  ```
  ```
- [ ] **Bekannte Limitierungen:**
  ```markdown
  ## Bekannte Limitierungen
  - Keine Authentifizierung (jeder Name akzeptiert)
  - Keine Verschlüsselung (Klartext TCP)
  - File-Persistenz nicht für Produktion geeignet
  - Keine Zonen/Instanzen (eine Welt für alle)
  ```
- [ ] **Nächste Schritte (Roadmap):**
  ```markdown
  ## Roadmap (Phase 5+)
  - [ ] SQLite-Persistenz
  - [ ] Authentifizierung mit Passwort
  - [ ] Zonen-System
  - [ ] Kampfsystem mit Mobs
  - [ ] Items & Inventar
  ```

**Akzeptanzkriterien:**
- [ ] README enthält vollständige Setup-Anleitung
- [ ] Neue Entwickler können in <10 Minuten starten
- [ ] Aktueller Feature-Stand ist dokumentiert
- [ ] Bekannte Limitierungen sind transparent

**Ressourcen:**
- [README Best Practices](https://learn.microsoft.com/en-us/azure/devops/repos/git/create-a-readme) – Gute README schreiben
- [Markdown Syntax](https://learn.microsoft.com/en-us/contribute/markdown-reference) – Markdown-Referenz
- [ASCII Diagrams](https://docs.microsoft.com/en-us/contribute/code-in-docs) – Architekturdiagramme im Text
- [GitHub Markdown](https://docs.github.com/en/get-started/writing-on-github) – GitHub-spezifisches Markdown

---

#### Issue: Manueller Testplan für Phase 4

**Labels:** `type:test`, `priority:p1`

**Beschreibung:**
Einen manuellen Testplan erstellen und durchführen, der alle Phase-4-Features systematisch abdeckt. Ergebnisse werden dokumentiert und gefundene Bugs als Issues erfasst.

**Aufgaben:**
- [ ] **Testplan-Dokument erstellen:**
  - [ ] Datei: `docs/TESTPLAN_PHASE4.md`
  - [ ] Format pro Testfall:
    ```markdown
    ### TC-001: Login mit neuem Charakter
    **Vorbedingung:** Server läuft, kein Charakter-File existiert
    **Schritte:**
    1. Client starten
    2. Namen "TestPlayer1" eingeben
    3. Login-Button klicken
    **Erwartetes Ergebnis:**
    - Login erfolgreich
    - Spieler spawnt in Weltmitte
    - Charakter-File wird erstellt
    **Tatsächliches Ergebnis:** _[Auszufüllen]_
    **Status:** ⬜ Bestanden / ⬜ Fehlgeschlagen
    ```
- [ ] **Testfälle definieren:**
  - [ ] **Login/Logout:**
    - TC-001: Login mit neuem Charakter
    - TC-002: Login mit existierendem Charakter
    - TC-003: Logout speichert Position
    - TC-004: Reconnect nach Disconnect
  - [ ] **Movement:**
    - TC-010: Bewegung in alle Richtungen
    - TC-011: Bewegung an Weltgrenzen
    - TC-012: Andere Spieler sichtbar
    - TC-013: Position synchronisiert korrekt
  - [ ] **Persistenz:**
    - TC-020: Position nach Neustart erhalten
    - TC-021: Korruptes File wird behandelt
    - TC-022: Server-Shutdown speichert alle
  - [ ] **Chat:**
    - TC-030: Nachricht senden und empfangen
    - TC-031: Rate-Limit wird angezeigt
    - TC-032: System-Nachrichten erscheinen
  - [ ] **Aktionen:**
    - TC-040: Attack auf Dummy-Mob
    - TC-041: Emote wird angezeigt
    - TC-042: Cooldown funktioniert
  - [ ] **Mehrere Clients:**
    - TC-050: 2 Clients gleichzeitig
    - TC-051: 3+ Clients (Lasttest)
    - TC-052: Schnelles Ein-/Ausloggen
- [ ] **Tests durchführen:**
  - [ ] Jeden Testfall einzeln ausführen
  - [ ] Ergebnis dokumentieren
  - [ ] Screenshots bei Fehlern
- [ ] **Bugs als Issues erfassen:**
  - [ ] Pro gefundenem Bug ein Issue erstellen
  - [ ] Label `type:bug` und `priority:*` setzen
  - [ ] Reproduktionsschritte dokumentieren
- [ ] **Testergebnis-Zusammenfassung:**
  ```markdown
  ## Zusammenfassung
  - **Gesamtzahl Testfälle:** 20
  - **Bestanden:** 18
  - **Fehlgeschlagen:** 2
  - **Kritische Bugs:** 0
  - **Nicht-kritische Bugs:** 2
  
  ### Gefundene Bugs
  - #XX: Beschreibung Bug 1
  - #XX: Beschreibung Bug 2
  ```

**Akzeptanzkriterien:**
- [ ] Testplan-Dokument existiert mit allen Testfällen
- [ ] Alle Testfälle wurden durchgeführt
- [ ] Ergebnisse sind dokumentiert
- [ ] Gefundene Bugs sind als Issues erfasst
- [ ] Kein kritischer Bug ungelöst

**Ressourcen:**
- [Test Plan Templates](https://learn.microsoft.com/en-us/azure/devops/test/create-test-cases) – Testplan-Struktur
- [Exploratory Testing](https://learn.microsoft.com/en-us/azure/devops/test/perform-exploratory-tests) – Exploratives Testen
- [Bug Report Best Practices](https://learn.microsoft.com/en-us/azure/devops/boards/backlogs/manage-bugs) – Bug-Dokumentation
- [Test Documentation](https://learn.microsoft.com/en-us/azure/devops/test/) – Test-Management

---

## Zusammenfassung

### Übersicht der Sub-Issues (zu erstellen)

| Original-Issue | Sub-Issues |
|---------------|------------|
| #8 NetworkServer | 8a: TCP-Listener, 8b: ClientConnection, 8c: Message-Loop & Events |
| #9 MessageRouter | 9a: Router-Basisklasse, 9b: Login-Handler |
| #11 Login-UI | 11a: UI erstellen, 11b: Netzwerk-Integration, 11c: Response-Handling |
| #12 MoveRequest | 12a: LocalPlayerController, 12b: HandleMove Server |
| #14 Remote-Player | 14a: GameManager, 14b: PlayerNode-Szene, 14c: Interpolation |
| #15 Movement-Tests | 15a: Tuning, 15b: Unit-Tests, 15c: Manuelle Tests |

### Übersicht der neuen Issues (zu erstellen)

| Phase | Kategorie | Anzahl Issues |
|-------|-----------|---------------|
| Phase 2 | Feinschliff | 3 |
| Phase 3 | Welt & Movement | 4 |
| Phase 4 | Persistenz | 5 |
| Phase 4 | Chat-System | 5 |
| Phase 4 | Gameplay-Action | 5 |
| Phase 4 | Abschluss | 3 |
| **Gesamt** | | **25 neue Issues** |

---

## Nächste Schritte

1. Sub-Issues für die 6 identifizierten großen Issues erstellen
2. Original-Issues mit Verweisen auf Sub-Issues aktualisieren
3. 25 neue Issues für Phase 2-4 erstellen
4. Issues mit passenden Labels versehen
5. Issues in sinnvoller Reihenfolge priorisieren
