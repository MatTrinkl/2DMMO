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

> **Hinweis:** Alle Issues in diesem Teil sind als **Epik-Issues** strukturiert und in Sub-Issues unterteilt.

### Phase 2 – Feinschliff

---

### Epik: Verbindungsaufbau-Flow mit Retry und Fehlerbehandlung

**Labels:** `type:epic`, `area:client`, `area:network`, `priority:p1`

**Beschreibung:**
Den Verbindungsaufbau im `NetworkClient` robuster gestalten mit automatischem Retry-Mechanismus, konfigurierbarem Timeout und Benutzer-Feedback bei Fehlern.

> **📌 Dies ist ein Epik-Issue.** Die Arbeit wurde in folgende Sub-Issues aufgeteilt:

**Sub-Issues:**
- [ ] Sub-Issue: ConnectionConfig und ConnectionState implementieren
- [ ] Sub-Issue: Retry-Logik im NetworkClient
- [ ] Sub-Issue: UI-Feedback für Verbindungsstatus

---

#### Sub-Issue: ConnectionConfig und ConnectionState implementieren

**Labels:** `type:feature`, `area:client`, `area:network`, `priority:p1`

**Beschreibung:**
Konfigurationsklasse und State-Enum für den Verbindungsaufbau erstellen.

**Aufgaben:**
- [ ] `ConnectionConfig`-Klasse erstellen:
  - [ ] Datei: `res://scripts/network/ConnectionConfig.cs`
  - [ ] Property `int TimeoutMs = 5000`
  - [ ] Property `int MaxRetries = 3`
  - [ ] Property `int RetryDelayMs = 1000`
  - [ ] Dokumentation der Properties mit XML-Kommentaren
- [ ] `ConnectionState`-Enum definieren:
  - [ ] Datei: `shared/Mmo.Shared/Enums/ConnectionState.cs`
  - [ ] Werte: `Disconnected = 0`, `Connecting = 1`, `Connected = 2`, `Reconnecting = 3`, `Failed = 4`
- [ ] Signal `ConnectionStateChanged` im NetworkClient definieren:
  ```csharp
  [Signal]
  public delegate void ConnectionStateChangedEventHandler(int state);
  ```

**Akzeptanzkriterien:**
- [ ] ConnectionConfig-Klasse existiert mit allen Properties
- [ ] ConnectionState-Enum ist im Shared-Projekt
- [ ] Signal ist definiert und dokumentiert

**Ressourcen:**
- [Godot Signals in C#](https://docs.godotengine.org/en/stable/tutorials/scripting/c_sharp/c_sharp_signals.html) – Custom Signals definieren
- [Enums in C#](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/enum) – Enum-Definition

---

#### Sub-Issue: Retry-Logik im NetworkClient

**Labels:** `type:feature`, `area:client`, `area:network`, `priority:p1`

**Beschreibung:**
Die Retry-Logik im NetworkClient implementieren mit Timeout und exponential Backoff.

**Aufgaben:**
- [ ] `NetworkClient.ConnectAsync()` erweitern:
  ```csharp
  public async Task<bool> ConnectAsync(string host, int port)
  {
      for (int attempt = 1; attempt <= _config.MaxRetries; attempt++)
      {
          EmitSignal(SignalName.ConnectionStateChanged, (int)ConnectionState.Connecting);
          
          using var cts = new CancellationTokenSource(_config.TimeoutMs);
          try
          {
              await _tcpClient.ConnectAsync(host, port, cts.Token);
              EmitSignal(SignalName.ConnectionStateChanged, (int)ConnectionState.Connected);
              return true;
          }
          catch (OperationCanceledException)
          {
              _logger.Warning($"Connection attempt {attempt} timed out");
          }
          catch (Exception ex)
          {
              _logger.Error($"Connection attempt {attempt} failed: {ex.Message}");
          }
          
          if (attempt < _config.MaxRetries)
          {
              EmitSignal(SignalName.ConnectionStateChanged, (int)ConnectionState.Reconnecting);
              await Task.Delay(_config.RetryDelayMs * attempt); // Exponential backoff
          }
      }
      
      EmitSignal(SignalName.ConnectionStateChanged, (int)ConnectionState.Failed);
      return false;
  }
  ```
- [ ] CancellationToken korrekt disposen
- [ ] State-Änderungen über Signal kommunizieren
- [ ] Exponential Backoff implementieren (Delay × Versuchsnummer)

**Akzeptanzkriterien:**
- [ ] Bei nicht erreichbarem Server: 3 Versuche mit je 5s Timeout
- [ ] State-Signal wird bei jeder Änderung gefeuert
- [ ] Nach MaxRetries wird `ConnectionState.Failed` gesetzt
- [ ] Keine Memory-Leaks (CancellationTokenSource disposed)

**Ressourcen:**
- [CancellationTokenSource](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtokensource) – Timeout-Implementierung
- [Task.Delay](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task.delay) – Verzögerung für Backoff

---

#### Sub-Issue: UI-Feedback für Verbindungsstatus

**Labels:** `type:feature`, `area:client`, `priority:p1`

**Beschreibung:**
Das LoginPanel mit visuellem Feedback für den Verbindungsstatus erweitern.

**Aufgaben:**
- [ ] Im `LoginPanel` auf `ConnectionStateChanged` reagieren:
  ```csharp
  public override void _Ready()
  {
      NetworkClient.Instance.ConnectionStateChanged += OnConnectionStateChanged;
  }
  
  private void OnConnectionStateChanged(int state)
  {
      var connectionState = (ConnectionState)state;
      CallDeferred(nameof(UpdateUI), connectionState);
  }
  
  private void UpdateUI(ConnectionState state)
  {
      switch (state)
      {
          case ConnectionState.Connecting:
              _statusLabel.Text = "Verbinde...";
              _loginButton.Disabled = true;
              _spinner.Visible = true;
              break;
          case ConnectionState.Reconnecting:
              _statusLabel.Text = "Neuer Versuch...";
              break;
          case ConnectionState.Failed:
              _statusLabel.Text = "Verbindung fehlgeschlagen";
              _loginButton.Disabled = false;
              _spinner.Visible = false;
              break;
          case ConnectionState.Connected:
              _statusLabel.Text = "Verbunden!";
              _spinner.Visible = false;
              break;
      }
  }
  ```
- [ ] Loading-Spinner zur LoginPanel-Szene hinzufügen
- [ ] StatusLabel für Fehlermeldungen hinzufügen
- [ ] Button deaktivieren während Verbindungsaufbau
- [ ] Differenzierte Fehlermeldungen anzeigen

**Akzeptanzkriterien:**
- [ ] UI zeigt aktuellen Status (Connecting, Retry, Failed)
- [ ] Spinner dreht während Verbindungsaufbau
- [ ] Button ist während Verbindung deaktiviert
- [ ] Nach Fehler kann erneut geklickt werden

**Ressourcen:**
- [Godot Control Nodes](https://docs.godotengine.org/en/stable/tutorials/ui/control_node_gallery.html) – UI-Elemente
- [CallDeferred](https://docs.godotengine.org/en/stable/classes/class_object.html#class-object-method-call-deferred) – Thread-sichere UI-Updates

---

### Epik: Konsistente Verwendung der Shared DTOs im Client

**Labels:** `type:epic`, `area:server`, `area:client`, `priority:p1`

**Beschreibung:**
Sicherstellen, dass der Godot-Client exakt dieselben DTO-Klassen aus `Mmo.Shared` verwendet wie der Server.

> **📌 Dies ist ein Epik-Issue.** Die Arbeit wurde in folgende Sub-Issues aufgeteilt:

**Sub-Issues:**
- [ ] Sub-Issue: Projektverweise konfigurieren
- [ ] Sub-Issue: Duplizierte Klassen entfernen
- [ ] Sub-Issue: Shared-Projekt dokumentieren

---

#### Sub-Issue: Projektverweise konfigurieren

**Labels:** `type:chore`, `area:client`, `priority:p1`

**Beschreibung:**
Das Godot-Projekt so konfigurieren, dass es das Shared-Projekt referenziert.

**Aufgaben:**
- [ ] Godot-Projekt `.csproj` erweitern:
  - [ ] Datei: `client/GodotProject/GodotProject.csproj`
  - [ ] Hinzufügen:
    ```xml
    <ItemGroup>
      <ProjectReference Include="../../shared/Mmo.Shared/Mmo.Shared.csproj" />
    </ItemGroup>
    ```
- [ ] Build-Reihenfolge prüfen:
  - [ ] `dotnet build Mmo.sln` auf Solution-Ebene testen
  - [ ] Sicherstellen, dass `Mmo.Shared` vor Client gebaut wird
- [ ] Godot-Editor neu starten und Build testen

**Akzeptanzkriterien:**
- [ ] `dotnet build Mmo.sln` baut alle Projekte fehlerfrei
- [ ] Godot-Projekt findet die Shared-Klassen
- [ ] Intellisense funktioniert für Shared-Typen

**Ressourcen:**
- [Projektverweise in .NET](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-add-reference) – `dotnet add reference` Befehl
- [Godot C# Projekte](https://docs.godotengine.org/en/stable/tutorials/scripting/c_sharp/c_sharp_basics.html#c-project) – C#-Projektstruktur

---

#### Sub-Issue: Duplizierte Klassen entfernen

**Labels:** `type:chore`, `area:client`, `priority:p1`

**Beschreibung:**
Alle duplizierten DTO-Klassen im Client entfernen und durch Shared-Referenzen ersetzen.

**Aufgaben:**
- [ ] Suche nach lokalen DTO-Klassen im Client:
  ```bash
  find client/ -name "*.cs" -exec grep -l "LoginRequest\|LoginResponse\|MessageType" {} \;
  ```
- [ ] Für jede gefundene Datei:
  - [ ] Lokale Klasse entfernen
  - [ ] `using Mmo.Shared.Messages;` hinzufügen
  - [ ] `using Mmo.Shared.Enums;` hinzufügen
- [ ] Kompilieren und Fehler beheben:
  - [ ] Property-Namen anpassen falls nötig
  - [ ] Namespace-Konflikte auflösen
- [ ] Namespace-Struktur vereinheitlichen:
  - [ ] Messages: `Mmo.Shared.Messages`
  - [ ] Enums: `Mmo.Shared.Enums`
  - [ ] Models: `Mmo.Shared.Models`

**Akzeptanzkriterien:**
- [ ] Keine duplizierten DTO-Klassen im Client-Ordner
- [ ] Alle Referenzen zeigen auf Shared-Projekt
- [ ] Client kompiliert ohne Fehler

**Ressourcen:**
- [Using Directive](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/using-directive) – Namespace-Imports
- [Namespace Best Practices](https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/namespace-names) – Namenskonventionen

---

#### Sub-Issue: Shared-Projekt dokumentieren

**Labels:** `type:documentation`, `priority:p2`

**Beschreibung:**
Dokumentation für das Shared-Projekt erstellen mit Liste aller DTOs.

**Aufgaben:**
- [ ] `shared/README.md` erstellen:
  ```markdown
  # Mmo.Shared
  
  Gemeinsame Typen für Client und Server.
  
  ## Messages
  - `LoginRequest` – Login-Anfrage mit Spielername
  - `LoginResponse` – Antwort mit PlayerId und Spawn-Position
  - `MoveRequest` – Bewegungsrichtung
  - `PlayerStateUpdate` – Spielerpositionen (Broadcast)
  - ...
  
  ## Enums
  - `MessageType` – Typ der Netzwerknachricht
  - `ConnectionState` – Verbindungsstatus
  - ...
  ```
- [ ] XML-Kommentare für alle öffentlichen Klassen
- [ ] Kurzbeschreibung pro DTO (Zweck, Properties)

**Akzeptanzkriterien:**
- [ ] README.md existiert mit vollständiger DTO-Liste
- [ ] Alle öffentlichen Klassen haben XML-Kommentare
- [ ] Dokumentation ist aktuell

**Ressourcen:**
- [XML Documentation Comments](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/xmldoc/) – C# Dokumentation
- [README Best Practices](https://docs.github.com/en/repositories/managing-your-repositorys-settings-and-features/customizing-your-repository/about-readmes) – README schreiben

---

### Epik: Grundlegendes Logging im Network-Code

**Labels:** `type:epic`, `area:server`, `area:client`, `priority:p2`

**Beschreibung:**
Strukturiertes Logging für alle Netzwerk-Operationen einführen. Auf dem Server wird `Microsoft.Extensions.Logging` verwendet, im Godot-Client ein Wrapper um `GD.Print`.

> **📌 Dies ist ein Epik-Issue.** Die Arbeit wurde in folgende Sub-Issues aufgeteilt:

**Sub-Issues:**
- [ ] Sub-Issue: Server-Logging einrichten
- [ ] Sub-Issue: Client-Logger-Klasse erstellen
- [ ] Sub-Issue: Log-Statements hinzufügen

---

#### Sub-Issue: Server-Logging einrichten

**Labels:** `type:feature`, `area:server`, `priority:p2`

**Beschreibung:**
Microsoft.Extensions.Logging im Server einrichten mit konfigurierbarem Log-Level.

**Aufgaben:**
- [ ] NuGet-Paket hinzufügen:
  ```bash
  cd server/Mmo.Server
  dotnet add package Microsoft.Extensions.Logging
  dotnet add package Microsoft.Extensions.Logging.Console
  ```
- [ ] `ILogger<T>` in Klassen injecten:
  - [ ] `NetworkServer` erhält `ILogger<NetworkServer>`
  - [ ] `ClientConnection` erhält `ILogger<ClientConnection>`
  - [ ] `MessageRouter` erhält `ILogger<MessageRouter>`
- [ ] Log-Konfiguration in `Program.cs`:
  ```csharp
  var builder = Host.CreateApplicationBuilder(args);
  builder.Logging.AddConsole();
  // Log-Level wird aus appsettings.json gelesen
  ```
- [ ] `appsettings.json` erstellen:
  ```json
  {
    "Logging": {
      "LogLevel": {
        "Default": "Information",
        "Mmo.Server": "Debug"
      }
    }
  }
  ```
- [ ] `appsettings.Development.json` für Debug:
  ```json
  {
    "Logging": {
      "LogLevel": {
        "Default": "Debug"
      }
    }
  }
  ```

**Akzeptanzkriterien:**
- [ ] Logging-Pakete sind installiert
- [ ] Logger wird in alle relevanten Klassen injected
- [ ] Log-Level ist über appsettings.json konfigurierbar

**Ressourcen:**
- [Logging in .NET](https://learn.microsoft.com/en-us/dotnet/core/extensions/logging) – Framework-Übersicht
- [Dependency Injection](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection) – DI für Logger

---

#### Sub-Issue: Client-Logger-Klasse erstellen

**Labels:** `type:feature`, `area:client`, `priority:p2`

**Beschreibung:**
Eine Logger-Singleton-Klasse für den Godot-Client erstellen.

**Aufgaben:**
- [ ] `Logger`-Klasse erstellen:
  - [ ] Datei: `res://scripts/singletons/Logger.cs`
  ```csharp
  public partial class Logger : Node
  {
      public static Logger Instance { get; private set; }
      
      public enum LogLevel { Debug, Info, Warning, Error }
      
      public LogLevel MinLevel { get; set; } = LogLevel.Info;
      
      public override void _Ready()
      {
          Instance = this;
      }
      
      public void Debug(string message) => Log(LogLevel.Debug, message);
      public void Info(string message) => Log(LogLevel.Info, message);
      public void Warning(string message) => Log(LogLevel.Warning, message);
      public void Error(string message) => Log(LogLevel.Error, message);
      
      public void Log(LogLevel level, string message)
      {
          if (level < MinLevel) return;
          
          var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
          var formatted = $"[{level}] {timestamp}: {message}";
          
          if (level == LogLevel.Error)
              GD.PrintErr(formatted);
          else
              GD.Print(formatted);
      }
  }
  ```
- [ ] Als Autoload registrieren in `project.godot`
- [ ] MinLevel über Projekteinstellung konfigurierbar machen

**Akzeptanzkriterien:**
- [ ] Logger-Singleton existiert und ist als Autoload registriert
- [ ] Alle Log-Levels funktionieren
- [ ] Formatierung enthält Timestamp und Level

**Ressourcen:**
- [Godot Singletons](https://docs.godotengine.org/en/stable/tutorials/scripting/singletons_autoload.html) – Autoload-Pattern
- [GD.Print](https://docs.godotengine.org/en/stable/classes/class_@globalscope.html#class-globalscope-method-print) – Konsolen-Ausgabe

---

#### Sub-Issue: Log-Statements hinzufügen

**Labels:** `type:feature`, `area:server`, `area:client`, `priority:p2`

**Beschreibung:**
Log-Statements in Server und Client hinzufügen.

**Aufgaben:**
- [ ] **Server-Logs:**
  - [ ] `NetworkServer`:
    ```csharp
    _logger.LogInformation("Server started on port {Port}", port);
    _logger.LogInformation("Server stopped");
    ```
  - [ ] `ClientConnection`:
    ```csharp
    _logger.LogDebug("Client {Id} connected from {Endpoint}", Id, RemoteEndpoint);
    _logger.LogWarning("Client {Id} disconnected unexpectedly", Id);
    ```
  - [ ] `MessageRouter`:
    ```csharp
    _logger.LogDebug("Routing {MessageType} from {ClientId}", msg.Type, clientId);
    _logger.LogError(ex, "Error processing message from {ClientId}", clientId);
    ```
- [ ] **Client-Logs:**
  - [ ] `NetworkClient`:
    ```csharp
    Logger.Instance.Info($"Connecting to {host}:{port}");
    Logger.Instance.Debug($"Sending {message.Type}");
    Logger.Instance.Error($"Connection failed: {ex.Message}");
    ```

**Akzeptanzkriterien:**
- [ ] Server-Start zeigt Info-Log mit Port
- [ ] Client-Verbindung erzeugt Logs auf beiden Seiten
- [ ] Fehler werden mit Details geloggt

**Ressourcen:**
- [Structured Logging](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/logging/#log-message-template) – Message-Templates
- [Logging Best Practices](https://learn.microsoft.com/en-us/dotnet/core/extensions/logging#log-level) – Log-Level-Richtlinien

---

### Phase 3 – Welt & Movement

---

### Epik: 2D-Tilemap für Prototyp-Welt erstellen

**Labels:** `type:epic`, `area:client`, `priority:p1`

**Beschreibung:**
Eine einfache 2D-Tilemap in Godot erstellen als visuelle Grundlage für die Spielwelt.

> **📌 Dies ist ein Epik-Issue.** Die Arbeit wurde in folgende Sub-Issues aufgeteilt:

**Sub-Issues:**
- [ ] Sub-Issue: TileSet erstellen
- [ ] Sub-Issue: TileMap-Node einrichten
- [ ] Sub-Issue: Test-Map gestalten

---

#### Sub-Issue: TileSet erstellen

**Labels:** `type:feature`, `area:client`, `priority:p1`

**Beschreibung:**
Ein einfaches TileSet mit Platzhalter-Tiles für den Prototyp erstellen.

**Aufgaben:**
- [ ] TileSet-Resource erstellen:
  - [ ] Datei: `res://assets/tilesets/prototype_tileset.tres`
- [ ] Platzhalter-Tiles erstellen (32x32 Pixel):
  - [ ] `grass.png` – Grünes Rechteck für begehbaren Bereich
  - [ ] `water.png` – Blaues Rechteck für Rand/Grenze
  - [ ] `stone.png` – Graues Rechteck für Hindernisse
  - [ ] Dateien in: `res://assets/tiles/`
- [ ] Tiles im TileSet-Editor einrichten:
  - [ ] Tiles importieren
  - [ ] Collision-Shapes für Wasser/Stein (optional)
  - [ ] Navigation-Tiles für Gras (optional)

**Akzeptanzkriterien:**
- [ ] TileSet-Resource existiert
- [ ] Mindestens 3 verschiedene Tile-Typen
- [ ] Tiles sind im Editor verwendbar

**Ressourcen:**
- [TileSet in Godot 4](https://docs.godotengine.org/en/stable/classes/class_tileset.html) – TileSet-Dokumentation
- [Creating Tiles](https://docs.godotengine.org/en/stable/tutorials/2d/using_tilemaps.html#creating-a-tileset) – Tiles erstellen

---

#### Sub-Issue: TileMap-Node einrichten

**Labels:** `type:feature`, `area:client`, `priority:p1`

**Beschreibung:**
TileMap-Node zur Spielszene hinzufügen und konfigurieren.

**Aufgaben:**
- [ ] `TileMap`-Node zur `Game.tscn` hinzufügen:
  - [ ] Als erstes Child unter Root (unterhalb der Spieler)
  - [ ] Name: `WorldMap`
- [ ] TileSet zuweisen
- [ ] Cell-Size konfigurieren (32x32)
- [ ] Z-Index unter Spieler-Sprites setzen (-1)
- [ ] Kamera-Limits basierend auf Map-Größe setzen

**Akzeptanzkriterien:**
- [ ] TileMap-Node existiert in der Szene
- [ ] TileSet ist zugewiesen
- [ ] Z-Ordering ist korrekt (Map hinter Spielern)

**Ressourcen:**
- [Using TileMaps](https://docs.godotengine.org/en/stable/tutorials/2d/using_tilemaps.html) – TileMap-Tutorial
- [Z-Index](https://docs.godotengine.org/en/stable/classes/class_canvasitem.html#class-canvasitem-property-z-index) – Rendering-Reihenfolge

---

#### Sub-Issue: Test-Map gestalten

**Labels:** `type:feature`, `area:client`, `priority:p1`

**Beschreibung:**
Eine kleine Test-Map mit dem TileMap-Editor erstellen.

**Aufgaben:**
- [ ] Map-Größe definieren: 50x50 Tiles (1600x1600 Pixel)
- [ ] Äußeren Rand mit Wasser-Tiles füllen (2 Tiles breit)
- [ ] Inneren Bereich mit Gras-Tiles füllen
- [ ] Spawn-Bereich in der Mitte markieren (optional: anderes Tile)
- [ ] Kamera-Limits setzen:
  ```csharp
  camera.LimitLeft = 0;
  camera.LimitTop = 0;
  camera.LimitRight = 1600;
  camera.LimitBottom = 1600;
  ```
- [ ] Dokumentieren: Map-Ursprung (0,0) = linke obere Ecke

**Akzeptanzkriterien:**
- [ ] Map ist 50x50 Tiles groß
- [ ] Rand ist klar erkennbar
- [ ] Spieler spawnt im begehbaren Bereich

**Ressourcen:**
- [TileMap Editor](https://docs.godotengine.org/en/stable/tutorials/2d/using_tilemaps.html#painting-with-the-tilemap-editor) – Map zeichnen
- [Camera2D Limits](https://docs.godotengine.org/en/stable/classes/class_camera2d.html#class-camera2d-property-limit-bottom) – Kamera begrenzen

---

### Epik: Serverseitige Startposition und Spawn-Logik

**Labels:** `type:epic`, `area:server`, `priority:p1`

**Beschreibung:**
Die Startposition für neue Spieler serverseitig definieren und Spawn-Logik implementieren.

> **📌 Dies ist ein Epik-Issue.** Die Arbeit wurde in folgende Sub-Issues aufgeteilt:

**Sub-Issues:**
- [ ] Sub-Issue: SpawnConfig-Klasse erstellen
- [ ] Sub-Issue: Spawn-Position-Berechnung implementieren
- [ ] Sub-Issue: LoginResponse erweitern

---

#### Sub-Issue: SpawnConfig-Klasse erstellen

**Labels:** `type:feature`, `area:server`, `priority:p1`

**Beschreibung:**
Konfigurationsklasse für Spawn-Parameter erstellen.

**Aufgaben:**
- [ ] `SpawnConfig`-Klasse erstellen:
  - [ ] Datei: `server/Mmo.Server/Config/SpawnConfig.cs`
  ```csharp
  public static class SpawnConfig
  {
      public const float DefaultX = 800f;
      public const float DefaultY = 800f;
      public const float SpawnRadius = 50f;
      public const int MaxSpawnAttempts = 5;
      public const float MinPlayerDistance = 30f;
  }
  ```
- [ ] Dokumentation der Konstanten mit XML-Kommentaren

**Akzeptanzkriterien:**
- [ ] SpawnConfig-Klasse existiert
- [ ] Alle Konstanten sind dokumentiert
- [ ] Werte sind sinnvoll für 1600x1600 Map

**Ressourcen:**
- [Static Classes](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/static-classes-and-static-class-members) – Konstanten-Container
- [XML Documentation](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/xmldoc/) – Dokumentation

---

#### Sub-Issue: Spawn-Position-Berechnung implementieren

**Labels:** `type:feature`, `area:server`, `priority:p1`

**Beschreibung:**
Methode zum Berechnen einer freien Spawn-Position implementieren.

**Aufgaben:**
- [ ] `World.GetSpawnPosition()` Methode:
  ```csharp
  public Vector2 GetSpawnPosition()
  {
      var random = new Random();
      
      for (int attempt = 0; attempt < SpawnConfig.MaxSpawnAttempts; attempt++)
      {
          var offsetX = (float)(random.NextDouble() * SpawnConfig.SpawnRadius * 2 - SpawnConfig.SpawnRadius);
          var offsetY = (float)(random.NextDouble() * SpawnConfig.SpawnRadius * 2 - SpawnConfig.SpawnRadius);
          var position = new Vector2(SpawnConfig.DefaultX + offsetX, SpawnConfig.DefaultY + offsetY);
          
          if (IsPositionFree(position))
              return position;
      }
      
      // Fallback: Default-Position
      return new Vector2(SpawnConfig.DefaultX, SpawnConfig.DefaultY);
  }
  
  private bool IsPositionFree(Vector2 position)
  {
      foreach (var player in _players.Values)
      {
          var distance = Vector2.Distance(position, new Vector2(player.X, player.Y));
          if (distance < SpawnConfig.MinPlayerDistance)
              return false;
      }
      return true;
  }
  ```
- [ ] Unit-Test für Spawn-Position

**Akzeptanzkriterien:**
- [ ] Spawn-Position ist innerhalb des SpawnRadius
- [ ] Kollisionsprüfung verhindert Überlappung
- [ ] Fallback auf Default-Position funktioniert

**Ressourcen:**
- [System.Random](https://learn.microsoft.com/en-us/dotnet/api/system.random) – Zufallszahlen
- [Vector2.Distance](https://learn.microsoft.com/en-us/dotnet/api/system.numerics.vector2.distance) – Distanzberechnung

---

#### Sub-Issue: LoginResponse erweitern

**Labels:** `type:feature`, `area:server`, `area:network`, `priority:p1`

**Beschreibung:**
LoginResponse DTO um Spawn-Position erweitern.

**Aufgaben:**
- [ ] `LoginResponse` erweitern:
  ```csharp
  public class LoginResponse : INetworkMessage
  {
      public MessageType Type => MessageType.LoginResponse;
      public bool Success { get; set; }
      public Guid PlayerId { get; set; }
      public float SpawnX { get; set; }
      public float SpawnY { get; set; }
      public string? ErrorMessage { get; set; }
  }
  ```
- [ ] Login-Handler anpassen:
  ```csharp
  var spawnPos = _world.GetSpawnPosition();
  var player = _world.CreatePlayer(request.UserName, connection, spawnPos);
  
  var response = new LoginResponse
  {
      Success = true,
      PlayerId = player.Id,
      SpawnX = player.X,
      SpawnY = player.Y
  };
  ```
- [ ] Client: Spawn-Position aus Response lesen und Spieler positionieren

**Akzeptanzkriterien:**
- [ ] LoginResponse enthält SpawnX/SpawnY
- [ ] Client positioniert Spieler korrekt
- [ ] Serialisierung funktioniert

**Ressourcen:**
- [DTOs erweitern](https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/how-to) – JSON-Serialisierung
- [DTO Best Practices](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/ddd-oriented-microservice) – DTO-Design

---

### Epik: Mapping von Server-Positionen auf Godot-Koordinaten

**Labels:** `type:epic`, `area:client`, `priority:p1`

**Beschreibung:**
Die Server-Koordinaten auf Godot-Weltkoordinaten mappen.

> **📌 Dies ist ein Epik-Issue.** Die Arbeit wurde in folgende Sub-Issues aufgeteilt:

**Sub-Issues:**
- [ ] Sub-Issue: CoordinateMapper-Singleton erstellen
- [ ] Sub-Issue: Koordinaten-Umrechnung integrieren

---

#### Sub-Issue: CoordinateMapper-Singleton erstellen

**Labels:** `type:feature`, `area:client`, `priority:p1`

**Beschreibung:**
Singleton-Klasse für Koordinatenumrechnung erstellen.

**Aufgaben:**
- [ ] `CoordinateMapper`-Klasse erstellen:
  - [ ] Datei: `res://scripts/singletons/CoordinateMapper.cs`
  ```csharp
  public partial class CoordinateMapper : Node
  {
      public static CoordinateMapper Instance { get; private set; }
      
      public const float PixelsPerUnit = 1.0f; // 1:1 Mapping
      
      public override void _Ready()
      {
          Instance = this;
      }
      
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
- [ ] Als Autoload in project.godot registrieren

**Akzeptanzkriterien:**
- [ ] CoordinateMapper ist als Autoload verfügbar
- [ ] 1:1 Mapping funktioniert korrekt
- [ ] Umrechnung in beide Richtungen

**Ressourcen:**
- [Godot Singletons](https://docs.godotengine.org/en/stable/tutorials/scripting/singletons_autoload.html) – Autoload-Pattern
- [Vector2 in Godot](https://docs.godotengine.org/en/stable/classes/class_vector2.html) – 2D-Vektoren

---

#### Sub-Issue: Koordinaten-Umrechnung integrieren

**Labels:** `type:feature`, `area:client`, `priority:p1`

**Beschreibung:**
CoordinateMapper in PlayerNode und GameManager integrieren.

**Aufgaben:**
- [ ] In `PlayerNode.UpdatePosition()`:
  ```csharp
  public void UpdatePosition(float serverX, float serverY)
  {
      var worldPos = CoordinateMapper.Instance.ServerToWorld(serverX, serverY);
      Position = worldPos;
  }
  ```
- [ ] In `LocalPlayerController` (falls Input-Position gesendet wird):
  ```csharp
  var serverPos = CoordinateMapper.Instance.WorldToServer(Position);
  // Request senden mit serverPos
  ```
- [ ] Kamera-Setup:
  - [ ] Camera2D als Child des lokalen Spielers
  - [ ] Position Smoothing aktivieren

**Akzeptanzkriterien:**
- [ ] Spielerpositionen werden korrekt dargestellt
- [ ] Bewegungsrichtungen stimmen
- [ ] Kamera folgt dem Spieler

**Ressourcen:**
- [Camera2D](https://docs.godotengine.org/en/stable/classes/class_camera2d.html) – Kamera-Dokumentation
- [Node2D Position](https://docs.godotengine.org/en/stable/classes/class_node2d.html#class-node2d-property-position) – Positionierung

---

### Epik: Weltgrenzen im Server implementieren

**Labels:** `type:epic`, `area:server`, `priority:p1`

**Beschreibung:**
Weltgrenzen auf dem Server implementieren, damit Spieler nicht außerhalb laufen können.

> **📌 Dies ist ein Epik-Issue.** Die Arbeit wurde in folgende Sub-Issues aufgeteilt:

**Sub-Issues:**
- [ ] Sub-Issue: WorldBounds-Konfiguration erstellen
- [ ] Sub-Issue: Position-Clamping implementieren

---

#### Sub-Issue: WorldBounds-Konfiguration erstellen

**Labels:** `type:feature`, `area:server`, `priority:p1`

**Beschreibung:**
Konfigurationsklasse für Weltgrenzen erstellen.

**Aufgaben:**
- [ ] `WorldBounds`-Klasse erstellen:
  - [ ] Datei: `server/Mmo.Server/Config/WorldBounds.cs`
  ```csharp
  public static class WorldBounds
  {
      public const float MinX = 64f;   // 2 Tiles Rand
      public const float MinY = 64f;
      public const float MaxX = 1536f; // 50 Tiles - 2 Tiles Rand
      public const float MaxY = 1536f;
  }
  ```
- [ ] Werte entsprechend der TileMap-Größe

**Akzeptanzkriterien:**
- [ ] WorldBounds-Klasse existiert
- [ ] Werte passen zur Map-Größe

**Ressourcen:**
- [Static Classes](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/static-classes-and-static-class-members) – Konstanten

---

#### Sub-Issue: Position-Clamping implementieren

**Labels:** `type:feature`, `area:server`, `priority:p1`

**Beschreibung:**
Position-Clamping im Player und Movement-Handler implementieren.

**Aufgaben:**
- [ ] `Player.ClampPosition()` Methode:
  ```csharp
  public void ClampPosition()
  {
      X = Math.Clamp(X, WorldBounds.MinX, WorldBounds.MaxX);
      Y = Math.Clamp(Y, WorldBounds.MinY, WorldBounds.MaxY);
  }
  ```
- [ ] Aufruf in `Player.Update()` nach Positionsänderung
- [ ] Aufruf im Movement-Handler nach Bewegung
- [ ] Unit-Tests für Boundary-Fälle

**Akzeptanzkriterien:**
- [ ] Spieler können nicht außerhalb der Grenzen
- [ ] Bewegung entlang der Grenzen funktioniert
- [ ] Keine Teleportation oder Stuck-Zustände

**Ressourcen:**
- [Math.Clamp](https://learn.microsoft.com/en-us/dotnet/api/system.math.clamp) – Werte begrenzen
- [Unit Testing](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-with-dotnet-test) – Boundary-Tests
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

### Epik: Datenmodell für Charakterpersistenz definieren

**Labels:** `type:epic`, `area:persistenz`, `priority:p1`

**Beschreibung:**
Das Datenmodell für die Speicherung von Charakterdaten definieren.

> **📌 Dies ist ein Epik-Issue.** Die Arbeit wurde in folgende Sub-Issues aufgeteilt:

**Sub-Issues:**
- [ ] Sub-Issue: CharacterData-Record erstellen
- [ ] Sub-Issue: Factory-Methoden und Hilfsmethoden
- [ ] Sub-Issue: Serialisierungs-Tests

---

#### Sub-Issue: CharacterData-Record erstellen

**Labels:** `type:feature`, `area:persistenz`, `priority:p1`

**Beschreibung:**
Das CharacterData-Record im Shared-Projekt erstellen.

**Aufgaben:**
- [ ] Datei erstellen: `shared/Mmo.Shared/Models/CharacterData.cs`
- [ ] Record mit allen Properties:
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
- [ ] XML-Kommentare für alle Properties

**Akzeptanzkriterien:**
- [ ] CharacterData existiert im Shared-Projekt
- [ ] Alle Properties sind definiert
- [ ] Record ist immutable

**Ressourcen:**
- [Records in C#](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/record) – Immutable Data Types
- [Required Properties](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/required) – Pflichtfelder

---

#### Sub-Issue: Factory-Methoden und Hilfsmethoden

**Labels:** `type:feature`, `area:persistenz`, `priority:p1`

**Beschreibung:**
Factory-Methoden für CharacterData erstellen.

**Aufgaben:**
- [ ] `CharacterData.CreateNew()` Factory-Methode:
  ```csharp
  public static CharacterData CreateNew(string name, float x, float y)
  {
      return new CharacterData
      {
          Id = Guid.NewGuid(),
          Name = name,
          X = x, Y = y,
          CreatedAt = DateTime.UtcNow,
          LastLoginAt = DateTime.UtcNow
      };
  }
  ```
- [ ] `CharacterData.WithPosition()` Methode:
  ```csharp
  public CharacterData WithPosition(float x, float y)
  {
      return this with { X = x, Y = y, LastLoginAt = DateTime.UtcNow };
  }
  ```

**Akzeptanzkriterien:**
- [ ] Factory-Methode erstellt valide Charaktere
- [ ] WithPosition erstellt neue Instanz mit Position
- [ ] Timestamps werden korrekt gesetzt

**Ressourcen:**
- [with-Expressions](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/with-expression) – Records modifizieren
- [Factory Pattern](https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/factory) – Factory-Methoden

---

#### Sub-Issue: Serialisierungs-Tests

**Labels:** `type:test`, `area:persistenz`, `priority:p1`

**Beschreibung:**
Unit-Tests für JSON-Serialisierung erstellen.

**Aufgaben:**
- [ ] Test-Klasse erstellen: `tests/Mmo.Tests/CharacterDataTests.cs`
- [ ] Tests implementieren:
  ```csharp
  [Fact]
  public void CharacterData_SerializesCorrectly()
  {
      var data = CharacterData.CreateNew("TestPlayer", 100, 200);
      var json = JsonSerializer.Serialize(data);
      var deserialized = JsonSerializer.Deserialize<CharacterData>(json);
      
      Assert.Equal(data.Id, deserialized.Id);
      Assert.Equal(data.Name, deserialized.Name);
  }
  
  [Fact]
  public void CharacterData_DefaultValues_AreApplied()
  {
      var data = CharacterData.CreateNew("Test", 0, 0);
      Assert.Equal(1, data.Level);
      Assert.Equal(0, data.Experience);
  }
  ```

**Akzeptanzkriterien:**
- [ ] JSON-Roundtrip funktioniert
- [ ] Default-Werte werden korrekt angewendet
- [ ] Alle Tests sind grün

**Ressourcen:**
- [xUnit Testing](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-with-dotnet-test) – Unit-Tests
- [System.Text.Json](https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/overview) – JSON-Serialisierung

---

### Epik: Persistenzschicht implementieren (File/JSON)

**Labels:** `type:epic`, `area:persistenz`, `area:server`, `priority:p1`

**Beschreibung:**
Repository-Pattern für Charakterpersistenz implementieren.

> **📌 Dies ist ein Epik-Issue.** Die Arbeit wurde in folgende Sub-Issues aufgeteilt:

**Sub-Issues:**
- [ ] Sub-Issue: ICharacterRepository Interface
- [ ] Sub-Issue: FileCharacterRepository implementieren
- [ ] Sub-Issue: Repository-Tests

---

#### Sub-Issue: ICharacterRepository Interface

**Labels:** `type:feature`, `area:persistenz`, `priority:p1`

**Beschreibung:**
Repository-Interface für Charakterdaten definieren.

**Aufgaben:**
- [ ] Interface erstellen: `server/Mmo.Server/Persistence/ICharacterRepository.cs`
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

**Akzeptanzkriterien:**
- [ ] Interface ist definiert
- [ ] Alle CRUD-Operationen sind abgedeckt
- [ ] Async-Pattern wird verwendet

**Ressourcen:**
- [Repository Pattern](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/infrastructure-persistence-layer-design) – Design Pattern
- [Async/Await](https://learn.microsoft.com/en-us/dotnet/csharp/asynchronous-programming/) – Async-Pattern

---

#### Sub-Issue: FileCharacterRepository implementieren

**Labels:** `type:feature`, `area:persistenz`, `area:server`, `priority:p1`

**Beschreibung:**
JSON-File-basiertes Repository implementieren.

**Aufgaben:**
- [ ] Klasse erstellen: `server/Mmo.Server/Persistence/FileCharacterRepository.cs`
- [ ] Thread-Safety mit SemaphoreSlim:
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
          finally { _lock.Release(); }
      }
      
      private string GetFilePath(string name)
      {
          var sanitized = SanitizeFileName(name.ToLowerInvariant());
          return Path.Combine(_dataPath, $"{sanitized}.json");
      }
      
      private static string SanitizeFileName(string name)
      {
          return Regex.Replace(name, @"[^a-z0-9_]", "_");
      }
  }
  ```
- [ ] Alle Interface-Methoden implementieren
- [ ] ⚠️ Sicherheit: Dateinamen sanitieren!

**Akzeptanzkriterien:**
- [ ] Speichern und Laden funktioniert
- [ ] Thread-Safety ist gewährleistet
- [ ] Dateinamen sind sicher

**Ressourcen:**
- [File I/O](https://learn.microsoft.com/en-us/dotnet/standard/io/) – Dateioperationen
- [SemaphoreSlim](https://learn.microsoft.com/en-us/dotnet/api/system.threading.semaphoreslim) – Thread-Sync

---

#### Sub-Issue: Repository-Tests

**Labels:** `type:test`, `area:persistenz`, `priority:p1`

**Beschreibung:**
Unit-Tests für FileCharacterRepository.

**Aufgaben:**
- [ ] Test-Klasse erstellen
- [ ] Tests für Save/Load/Delete
- [ ] Test für Thread-Safety
- [ ] Test für nicht-existierende Charaktere

**Akzeptanzkriterien:**
- [ ] Alle CRUD-Operationen getestet
- [ ] Edge-Cases abgedeckt
- [ ] Tests sind grün

**Ressourcen:**
- [xUnit](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-with-dotnet-test) – Testing
- [Temporary Files](https://learn.microsoft.com/en-us/dotnet/api/system.io.path.gettemppath) – Temp-Ordner

---

### Epik: Charakterdaten beim Login laden

**Labels:** `type:epic`, `area:persistenz`, `area:server`, `priority:p1`

**Beschreibung:**
Beim Login Charakterdaten aus Persistenz laden.

> **📌 Dies ist ein Epik-Issue.** Die Arbeit wurde in folgende Sub-Issues aufgeteilt:

**Sub-Issues:**
- [ ] Sub-Issue: Login-Handler erweitern
- [ ] Sub-Issue: Neuen Charakter erstellen

---

#### Sub-Issue: Login-Handler erweitern

**Labels:** `type:feature`, `area:persistenz`, `area:server`, `priority:p1`

**Beschreibung:**
Login-Handler um Repository-Abfrage erweitern.

**Aufgaben:**
- [ ] `ICharacterRepository` in Login-Handler injecten
- [ ] Charakter laden oder erstellen:
  ```csharp
  public async Task HandleLoginAsync(ClientConnection connection, LoginRequest request)
  {
      var existingCharacter = await _characterRepository.GetByNameAsync(request.UserName);
      
      CharacterData character;
      if (existingCharacter != null)
      {
          character = existingCharacter with { LastLoginAt = DateTime.UtcNow };
          _logger.LogInformation("Player {Name} loaded", character.Name);
      }
      else
      {
          var spawnPos = _world.GetSpawnPosition();
          character = CharacterData.CreateNew(request.UserName, spawnPos.X, spawnPos.Y);
          _logger.LogInformation("New player {Name} created", character.Name);
      }
      
      await _characterRepository.SaveAsync(character);
      var player = _world.CreatePlayer(character, connection);
      // Response senden...
  }
  ```

**Akzeptanzkriterien:**
- [ ] Existierender Charakter wird geladen
- [ ] Position wird wiederhergestellt
- [ ] LastLoginAt wird aktualisiert

**Ressourcen:**
- [Dependency Injection](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection) – DI
- [Logging](https://learn.microsoft.com/en-us/dotnet/core/extensions/logging) – Logging

---

#### Sub-Issue: Neuen Charakter erstellen

**Labels:** `type:feature`, `area:persistenz`, `area:server`, `priority:p1`

**Beschreibung:**
Neuen Charakter erstellen falls keiner existiert.

**Aufgaben:**
- [ ] Spawn-Position berechnen
- [ ] CharacterData.CreateNew() aufrufen
- [ ] Charakter speichern
- [ ] Player-Entity erstellen

**Akzeptanzkriterien:**
- [ ] Neuer Charakter spawnt an Spawn-Position
- [ ] Charakter wird sofort gespeichert
- [ ] Spieler kann sofort spielen

**Ressourcen:**
- [Factory Methods](https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/factory) – Objekt-Erstellung

---

### Epik: Charakterdaten beim Logout speichern

**Labels:** `type:epic`, `area:persistenz`, `area:server`, `priority:p1`

**Beschreibung:**
Bei Disconnect/Logout Charakterdaten speichern.

> **📌 Dies ist ein Epik-Issue.** Die Arbeit wurde in folgende Sub-Issues aufgeteilt:

**Sub-Issues:**
- [ ] Sub-Issue: Disconnect-Handler implementieren
- [ ] Sub-Issue: Server-Shutdown-Handler

---

#### Sub-Issue: Disconnect-Handler implementieren

**Labels:** `type:feature`, `area:persistenz`, `area:server`, `priority:p1`

**Beschreibung:**
Bei Client-Disconnect Spielerdaten speichern.

**Aufgaben:**
- [ ] In `NetworkServer.OnClientDisconnected`:
  ```csharp
  private async Task OnClientDisconnected(ClientConnection connection)
  {
      var player = _world.GetPlayerByConnection(connection);
      if (player != null)
      {
          await SavePlayerAsync(player);
          _world.RemovePlayerByConnection(connection);
      }
  }
  
  private async Task SavePlayerAsync(Player player)
  {
      try
      {
          var data = await _characterRepository.GetByIdAsync(player.Id);
          if (data != null)
          {
              var updated = data.WithPosition(player.X, player.Y);
              await _characterRepository.SaveAsync(updated);
          }
      }
      catch (Exception ex)
      {
          _logger.LogError(ex, "Failed to save player {Name}", player.Name);
      }
  }
  ```

**Akzeptanzkriterien:**
- [ ] Position wird bei Disconnect gespeichert
- [ ] Fehler beim Speichern crashen nicht
- [ ] Player wird aus World entfernt

**Ressourcen:**
- [Exception Handling](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/exceptions/) – Fehlerbehandlung

---

#### Sub-Issue: Server-Shutdown-Handler

**Labels:** `type:feature`, `area:persistenz`, `area:server`, `priority:p1`

**Beschreibung:**
Bei Server-Shutdown alle Spieler speichern.

**Aufgaben:**
- [ ] `IHostApplicationLifetime` nutzen:
  ```csharp
  _appLifetime.ApplicationStopping.Register(async () =>
  {
      _logger.LogInformation("Saving all players...");
      var saveTasks = _world.GetAllPlayers()
          .Select(p => SavePlayerAsync(p));
      await Task.WhenAll(saveTasks);
  });
  ```
- [ ] Alternativ: In `StopAsync()` von IHostedService

**Akzeptanzkriterien:**
- [ ] Alle Spieler werden bei Shutdown gespeichert
- [ ] Keine Datenverluste bei Ctrl+C
- [ ] Logging zeigt Fortschritt

**Ressourcen:**
- [IHostApplicationLifetime](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.hosting.ihostapplicationlifetime) – Lifecycle-Events
- [Graceful Shutdown](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services) – Sauberes Beenden

---

### Epik: Fehlerbehandlung für Persistenz

**Labels:** `type:epic`, `area:persistenz`, `area:server`, `priority:p2`

**Beschreibung:**
Robuste Fehlerbehandlung für Persistenz-Operationen.

> **📌 Dies ist ein Epik-Issue.** Die Arbeit wurde in folgende Sub-Issues aufgeteilt:

**Sub-Issues:**
- [ ] Sub-Issue: Retry-Logik implementieren
- [ ] Sub-Issue: Korrupte Daten behandeln

---

#### Sub-Issue: Retry-Logik implementieren

**Labels:** `type:feature`, `area:persistenz`, `priority:p2`

**Beschreibung:**
Retry-Logik für Speichervorgänge implementieren.

**Aufgaben:**
- [ ] Retry bei IOException:
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
          catch (IOException) when (attempt < maxRetries)
          {
              await Task.Delay(100 * attempt);
          }
      }
      throw new PersistenceException("Save failed after retries");
  }
  ```

**Akzeptanzkriterien:**
- [ ] Temporäre Fehler werden wiederholt
- [ ] Exponential Backoff
- [ ] Nach Max-Retries: Exception

**Ressourcen:**
- [Retry Pattern](https://learn.microsoft.com/en-us/azure/architecture/patterns/retry) – Retry-Strategien

---

#### Sub-Issue: Korrupte Daten behandeln

**Labels:** `type:feature`, `area:persistenz`, `priority:p2`

**Beschreibung:**
Korrupte JSON-Dateien erkennen und behandeln.

**Aufgaben:**
- [ ] Bei JsonException: Backup erstellen
  ```csharp
  catch (JsonException ex)
  {
      _logger.LogError(ex, "Corrupt file for {Name}", name);
      await CreateBackupAsync(name);
      return null; // Treat as new character
  }
  ```
- [ ] Backup-Dateien: `{name}.json.corrupt.{timestamp}`

**Akzeptanzkriterien:**
- [ ] Korrupte Dateien werden erkannt
- [ ] Backup wird erstellt
- [ ] Spieler kann trotzdem einloggen

**Ressourcen:**
- [Exception Handling](https://learn.microsoft.com/en-us/dotnet/standard/exceptions/) – Fehlerbehandlung
### Phase 4 – Chat-System

---

### Epik: Chat-Nachrichtentypen im Shared-Projekt

**Labels:** `type:epic`, `area:chat`, `priority:p1`

**Beschreibung:**
Die Nachrichtentypen für das Chat-System definieren.

> **📌 Dies ist ein Epik-Issue.** Die Arbeit wurde in folgende Sub-Issues aufgeteilt:

**Sub-Issues:**
- [ ] Sub-Issue: ChatChannel-Enum erstellen
- [ ] Sub-Issue: Chat-DTOs erstellen
- [ ] Sub-Issue: MessageType erweitern

---

#### Sub-Issue: ChatChannel-Enum erstellen

**Labels:** `type:feature`, `area:chat`, `priority:p1`

**Beschreibung:**
Enum für Chat-Kanäle erstellen.

**Aufgaben:**
- [ ] Datei: `shared/Mmo.Shared/Enums/ChatChannel.cs`
  ```csharp
  public enum ChatChannel
  {
      Global = 0,   // An alle Spieler
      Zone = 1,     // An Spieler in der Zone (später)
      Whisper = 2,  // Privat (später)
      System = 3    // Server-Nachrichten
  }
  ```

**Akzeptanzkriterien:**
- [ ] Enum ist definiert
- [ ] Werte sind dokumentiert

**Ressourcen:**
- [Enums in C#](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/enum) – Enumerationen

---

#### Sub-Issue: Chat-DTOs erstellen

**Labels:** `type:feature`, `area:chat`, `priority:p1`

**Beschreibung:**
ChatMessageRequest und ChatMessageBroadcast DTOs erstellen.

**Aufgaben:**
- [ ] `ChatMessageRequest`:
  ```csharp
  public class ChatMessageRequest : INetworkMessage
  {
      public MessageType Type => MessageType.ChatMessageRequest;
      public ChatChannel Channel { get; set; } = ChatChannel.Global;
      public required string Message { get; set; }
      public Guid? TargetPlayerId { get; set; }
  }
  ```
- [ ] `ChatMessageBroadcast`:
  ```csharp
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
- [ ] Konstante für max. Nachrichtenlänge: `ChatConstants.MaxMessageLength = 200`

**Akzeptanzkriterien:**
- [ ] DTOs sind serialisierbar
- [ ] Alle Properties sind definiert

**Ressourcen:**
- [System.Text.Json](https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/overview) – Serialisierung

---

#### Sub-Issue: MessageType erweitern

**Labels:** `type:feature`, `area:chat`, `priority:p1`

**Beschreibung:**
MessageType-Enum um Chat-Typen erweitern.

**Aufgaben:**
- [ ] MessageType erweitern:
  ```csharp
  public enum MessageType
  {
      // ... existing ...
      ChatMessageRequest = 10,
      ChatMessageBroadcast = 11,
      ChatRateLimit = 12
  }
  ```
- [ ] MessageSerializer für neue Typen erweitern

**Akzeptanzkriterien:**
- [ ] Neue MessageTypes sind definiert
- [ ] Serialisierung funktioniert

**Ressourcen:**
- [Enums erweitern](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/enum) – Enum-Werte

---

### Epik: Serverseitiges Chat-Handling

**Labels:** `type:epic`, `area:chat`, `area:server`, `priority:p1`

**Beschreibung:**
Chat-Handling auf dem Server implementieren.

> **📌 Dies ist ein Epik-Issue.** Die Arbeit wurde in folgende Sub-Issues aufgeteilt:

**Sub-Issues:**
- [ ] Sub-Issue: ChatHandler-Klasse erstellen
- [ ] Sub-Issue: Validierung und Sanitization
- [ ] Sub-Issue: Broadcast-Mechanismus

---

#### Sub-Issue: ChatHandler-Klasse erstellen

**Labels:** `type:feature`, `area:chat`, `area:server`, `priority:p1`

**Beschreibung:**
ChatHandler für Nachrichtenverarbeitung erstellen.

**Aufgaben:**
- [ ] Datei: `server/Mmo.Server/Handlers/ChatHandler.cs`
  ```csharp
  public class ChatHandler
  {
      private readonly World _world;
      private readonly ILogger<ChatHandler> _logger;
      
      public async Task<ChatMessageBroadcast?> HandleChatAsync(
          ClientConnection connection, 
          ChatMessageRequest request)
      {
          var sender = _world.GetPlayerByConnection(connection);
          if (sender == null) return null;
          
          if (!ValidateMessage(request.Message, out var error))
          {
              _logger.LogDebug("Invalid message: {Error}", error);
              return null;
          }
          
          return new ChatMessageBroadcast
          {
              SenderId = sender.Id,
              SenderName = sender.Name,
              Channel = request.Channel,
              Message = SanitizeMessage(request.Message),
              Timestamp = DateTime.UtcNow
          };
      }
  }
  ```

**Akzeptanzkriterien:**
- [ ] Handler verarbeitet Chat-Nachrichten
- [ ] Sender wird validiert

**Ressourcen:**
- [Handler Pattern](https://learn.microsoft.com/en-us/azure/architecture/patterns/cqrs) – Message-Handling

---

#### Sub-Issue: Validierung und Sanitization

**Labels:** `type:feature`, `area:chat`, `area:server`, `priority:p1`

**Beschreibung:**
Nachrichten validieren und sanitizen.

**Aufgaben:**
- [ ] `ValidateMessage()`:
  ```csharp
  private bool ValidateMessage(string message, out string error)
  {
      error = string.Empty;
      if (string.IsNullOrWhiteSpace(message))
      {
          error = "Message empty";
          return false;
      }
      if (message.Length > ChatConstants.MaxMessageLength)
      {
          error = "Message too long";
          return false;
      }
      return true;
  }
  ```
- [ ] `SanitizeMessage()`:
  ```csharp
  private string SanitizeMessage(string message)
  {
      message = message.Trim();
      message = Regex.Replace(message, @"[\x00-\x1F]", ""); // Control chars
      message = Regex.Replace(message, @"\s+", " "); // Multi-spaces
      return message;
  }
  ```

**Akzeptanzkriterien:**
- [ ] Leere/zu lange Nachrichten abgelehnt
- [ ] Control-Zeichen entfernt

**Ressourcen:**
- [Regex in C#](https://learn.microsoft.com/en-us/dotnet/standard/base-types/regular-expressions) – Text-Manipulation

---

#### Sub-Issue: Broadcast-Mechanismus

**Labels:** `type:feature`, `area:chat`, `area:server`, `priority:p1`

**Beschreibung:**
Nachrichten an alle Spieler broadcasten.

**Aufgaben:**
- [ ] Im MessageRouter:
  ```csharp
  case MessageType.ChatMessageRequest:
      var broadcast = await _chatHandler.HandleChatAsync(connection, (ChatMessageRequest)message);
      if (broadcast != null)
      {
          await BroadcastToAllAsync(broadcast);
      }
      break;
  ```
- [ ] `BroadcastToAllAsync()`:
  ```csharp
  private async Task BroadcastToAllAsync(INetworkMessage message)
  {
      var tasks = _world.GetAllPlayers()
          .Select(p => p.Connection.SendAsync(message));
      await Task.WhenAll(tasks);
  }
  ```

**Akzeptanzkriterien:**
- [ ] Nachrichten erreichen alle Spieler
- [ ] Paralleles Senden

**Ressourcen:**
- [Task.WhenAll](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task.whenall) – Parallele Tasks

---

### Epik: Rate-Limiting für Chat

**Labels:** `type:epic`, `area:chat`, `area:server`, `priority:p2`

**Beschreibung:**
Spam-Schutz für Chat implementieren.

> **📌 Dies ist ein Epik-Issue.** Die Arbeit wurde in folgende Sub-Issues aufgeteilt:

**Sub-Issues:**
- [ ] Sub-Issue: ChatRateLimiter-Klasse
- [ ] Sub-Issue: Integration in ChatHandler

---

#### Sub-Issue: ChatRateLimiter-Klasse

**Labels:** `type:feature`, `area:chat`, `priority:p2`

**Beschreibung:**
Rate-Limiter für Chat-Nachrichten.

**Aufgaben:**
- [ ] Datei: `server/Mmo.Server/Services/ChatRateLimiter.cs`
  ```csharp
  public class ChatRateLimiter
  {
      private readonly ConcurrentDictionary<Guid, PlayerRateInfo> _playerRates = new();
      private readonly int _maxMessages = 5;
      private readonly TimeSpan _timeWindow = TimeSpan.FromSeconds(10);
      
      public bool TryConsume(Guid playerId, out TimeSpan waitTime)
      {
          waitTime = TimeSpan.Zero;
          var info = _playerRates.GetOrAdd(playerId, _ => new PlayerRateInfo());
          
          lock (info)
          {
              if (DateTime.UtcNow - info.WindowStart > _timeWindow)
              {
                  info.WindowStart = DateTime.UtcNow;
                  info.MessageCount = 0;
              }
              
              if (info.MessageCount >= _maxMessages)
              {
                  waitTime = _timeWindow - (DateTime.UtcNow - info.WindowStart);
                  return false;
              }
              
              info.MessageCount++;
              return true;
          }
      }
  }
  ```

**Akzeptanzkriterien:**
- [ ] Max 5 Nachrichten pro 10 Sekunden
- [ ] Thread-safe

**Ressourcen:**
- [ConcurrentDictionary](https://learn.microsoft.com/en-us/dotnet/api/system.collections.concurrent.concurrentdictionary-2) – Thread-safe Dictionary

---

#### Sub-Issue: Integration in ChatHandler

**Labels:** `type:feature`, `area:chat`, `priority:p2`

**Beschreibung:**
Rate-Limiter in ChatHandler integrieren.

**Aufgaben:**
- [ ] In HandleChatAsync:
  ```csharp
  if (!_rateLimiter.TryConsume(sender.Id, out var waitTime))
  {
      return new ChatRateLimitResponse { WaitTime = waitTime };
  }
  ```
- [ ] ChatRateLimitResponse DTO erstellen

**Akzeptanzkriterien:**
- [ ] Spam wird begrenzt
- [ ] Spieler erhält Feedback

**Ressourcen:**
- [Rate Limiting](https://learn.microsoft.com/en-us/dotnet/core/extensions/http-ratelimiter) – Rate-Limiting-Konzepte

---

### Epik: Chat-UI im Godot-Client

**Labels:** `type:epic`, `area:chat`, `area:client`, `priority:p1`

**Beschreibung:**
Chat-UI im Client erstellen.

> **📌 Dies ist ein Epik-Issue.** Die Arbeit wurde in folgende Sub-Issues aufgeteilt:

**Sub-Issues:**
- [ ] Sub-Issue: ChatPanel-Szene erstellen
- [ ] Sub-Issue: ChatPanel-Script implementieren

---

#### Sub-Issue: ChatPanel-Szene erstellen

**Labels:** `type:feature`, `area:chat`, `area:client`, `priority:p1`

**Beschreibung:**
Chat-Panel UI-Szene erstellen.

**Aufgaben:**
- [ ] Szene: `res://scenes/ui/ChatPanel.tscn`
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
- [ ] Layout: Unten-links, 400x200 Pixel
- [ ] Halbtransparenter Hintergrund

**Akzeptanzkriterien:**
- [ ] Panel ist sichtbar
- [ ] Input-Feld funktioniert

**Ressourcen:**
- [Control Nodes](https://docs.godotengine.org/en/stable/tutorials/ui/control_node_gallery.html) – UI-Elemente

---

#### Sub-Issue: ChatPanel-Script implementieren

**Labels:** `type:feature`, `area:chat`, `area:client`, `priority:p1`

**Beschreibung:**
ChatPanel-Script mit Nachrichtenlogik.

**Aufgaben:**
- [ ] Script: `res://scripts/ui/ChatPanel.cs`
  ```csharp
  public partial class ChatPanel : Control
  {
      [Export] private LineEdit _inputField;
      [Export] private VBoxContainer _messageList;
      
      private const int MaxMessages = 50;
      
      public override void _Ready()
      {
          _inputField.TextSubmitted += OnInputSubmitted;
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
          label.Text = $"[b]{senderName}:[/b] {message}";
          
          _messageList.AddChild(label);
          
          while (_messageList.GetChildCount() > MaxMessages)
              _messageList.GetChild(0).QueueFree();
      }
      
      [Signal]
      public delegate void MessageSubmittedEventHandler(string message);
  }
  ```
- [ ] Auto-Scroll bei neuen Nachrichten

**Akzeptanzkriterien:**
- [ ] Nachrichten werden angezeigt
- [ ] Enter sendet Nachricht
- [ ] Max. Nachrichten begrenzt

**Ressourcen:**
- [RichTextLabel](https://docs.godotengine.org/en/stable/classes/class_richtextlabel.html) – Formatierter Text
- [Signals in C#](https://docs.godotengine.org/en/stable/tutorials/scripting/c_sharp/c_sharp_signals.html) – Custom Signals

---

### Epik: Chat-Integration im Client

**Labels:** `type:epic`, `area:chat`, `area:client`, `priority:p1`

**Beschreibung:**
Chat mit NetworkClient verbinden.

> **📌 Dies ist ein Epik-Issue.** Die Arbeit wurde in folgende Sub-Issues aufgeteilt:

**Sub-Issues:**
- [ ] Sub-Issue: ChatManager-Singleton erstellen
- [ ] Sub-Issue: Nachrichten senden und empfangen

---

#### Sub-Issue: ChatManager-Singleton erstellen

**Labels:** `type:feature`, `area:chat`, `area:client`, `priority:p1`

**Beschreibung:**
ChatManager für Nachrichtenverwaltung erstellen.

**Aufgaben:**
- [ ] Datei: `res://scripts/singletons/ChatManager.cs`
  ```csharp
  public partial class ChatManager : Node
  {
      public static ChatManager Instance { get; private set; }
      
      private ChatPanel _chatPanel;
      
      public override void _Ready()
      {
          Instance = this;
          NetworkClient.Instance.MessageReceived += OnMessageReceived;
      }
      
      public void Initialize(ChatPanel chatPanel)
      {
          _chatPanel = chatPanel;
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
  }
  ```
- [ ] Als Autoload registrieren

**Akzeptanzkriterien:**
- [ ] ChatManager ist Singleton
- [ ] Nachrichten werden gesendet

**Ressourcen:**
- [Autoload](https://docs.godotengine.org/en/stable/tutorials/scripting/singletons_autoload.html) – Singletons

---

#### Sub-Issue: Nachrichten senden und empfangen

**Labels:** `type:feature`, `area:chat`, `area:client`, `priority:p1`

**Beschreibung:**
Eingehende Nachrichten anzeigen.

**Aufgaben:**
- [ ] Message-Handler:
  ```csharp
  private void OnMessageReceived(INetworkMessage message)
  {
      switch (message)
      {
          case ChatMessageBroadcast chat:
              CallDeferred(nameof(AddMessageToPanel), 
                  chat.SenderName, chat.Message, (int)chat.Channel);
              break;
          case ChatRateLimitResponse rateLimit:
              CallDeferred(nameof(ShowRateLimitMessage), rateLimit.WaitTime);
              break;
      }
  }
  
  private void AddMessageToPanel(string sender, string msg, int channel)
  {
      _chatPanel.AddMessage(sender, msg, (ChatChannel)channel);
  }
  ```
- [ ] System-Nachrichten (Join/Leave) anzeigen

**Akzeptanzkriterien:**
- [ ] Nachrichten werden angezeigt
- [ ] Rate-Limit-Warnung erscheint
- [ ] Thread-safe (CallDeferred)

**Ressourcen:**
- [CallDeferred](https://docs.godotengine.org/en/stable/classes/class_object.html#class-object-method-call-deferred) – Thread-sichere Aufrufe
### Phase 4 – Erste Gameplay-Aktion

---

### Epik: Action-Nachrichtentypen definieren

**Labels:** `type:epic`, `area:gameplay`, `priority:p1`

**Beschreibung:**
Die Nachrichtentypen für Gameplay-Aktionen definieren.

> **📌 Dies ist ein Epik-Issue.** Die Arbeit wurde in folgende Sub-Issues aufgeteilt:

**Sub-Issues:**
- [ ] Sub-Issue: ActionType und EmoteType Enums
- [ ] Sub-Issue: ActionRequest und ActionEvent DTOs

---

#### Sub-Issue: ActionType und EmoteType Enums

**Labels:** `type:feature`, `area:gameplay`, `priority:p1`

**Beschreibung:**
Enums für Action-Typen erstellen.

**Aufgaben:**
- [ ] `ActionType`-Enum:
  ```csharp
  public enum ActionType { None = 0, Attack = 1, Emote = 2, Interact = 3 }
  ```
- [ ] `EmoteType`-Enum:
  ```csharp
  public enum EmoteType { Wave = 0, Dance = 1, Sit = 2, Laugh = 3, Cry = 4 }
  ```
- [ ] `ActionResult`-Enum:
  ```csharp
  public enum ActionResult { Success = 0, InvalidTarget = 1, OutOfRange = 2, OnCooldown = 3 }
  ```

**Akzeptanzkriterien:**
- [ ] Enums sind definiert
- [ ] Werte sind dokumentiert

**Ressourcen:**
- [Enums in C#](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/enum) – Enumerationen

---

#### Sub-Issue: ActionRequest und ActionEvent DTOs

**Labels:** `type:feature`, `area:gameplay`, `priority:p1`

**Beschreibung:**
DTOs für Actions erstellen.

**Aufgaben:**
- [ ] `ActionRequest`:
  ```csharp
  public class ActionRequest : INetworkMessage
  {
      public MessageType Type => MessageType.ActionRequest;
      public ActionType ActionType { get; set; }
      public Guid? TargetId { get; set; }
      public EmoteType? EmoteType { get; set; }
  }
  ```
- [ ] `ActionEvent`:
  ```csharp
  public class ActionEvent : INetworkMessage
  {
      public MessageType Type => MessageType.ActionEvent;
      public required Guid ActorId { get; set; }
      public ActionType ActionType { get; set; }
      public Guid? TargetId { get; set; }
      public EmoteType? EmoteType { get; set; }
      public ActionResult Result { get; set; }
  }
  ```

**Akzeptanzkriterien:**
- [ ] DTOs sind serialisierbar
- [ ] MessageType erweitert

**Ressourcen:**
- [System.Text.Json](https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/overview) – Serialisierung

---

### Epik: Action-Handler im Server

**Labels:** `type:epic`, `area:gameplay`, `area:server`, `priority:p1`

**Beschreibung:**
Server-seitige Verarbeitung von Aktionen.

> **📌 Dies ist ein Epik-Issue.** Die Arbeit wurde in folgende Sub-Issues aufgeteilt:

**Sub-Issues:**
- [ ] Sub-Issue: ActionHandler-Klasse erstellen
- [ ] Sub-Issue: Attack-Handling implementieren
- [ ] Sub-Issue: Emote-Handling implementieren

---

#### Sub-Issue: ActionHandler-Klasse erstellen

**Labels:** `type:feature`, `area:gameplay`, `area:server`, `priority:p1`

**Beschreibung:**
ActionHandler-Basisklasse erstellen.

**Aufgaben:**
- [ ] Datei: `server/Mmo.Server/Handlers/ActionHandler.cs`
  ```csharp
  public class ActionHandler
  {
      private readonly World _world;
      private readonly ConcurrentDictionary<Guid, DateTime> _attackCooldowns = new();
      
      public ActionEvent? HandleAction(ClientConnection conn, ActionRequest request)
      {
          var actor = _world.GetPlayerByConnection(conn);
          if (actor == null) return null;
          
          return request.ActionType switch
          {
              ActionType.Attack => HandleAttack(actor, request),
              ActionType.Emote => HandleEmote(actor, request),
              _ => null
          };
      }
  }
  ```

**Akzeptanzkriterien:**
- [ ] Handler routet nach ActionType
- [ ] Actor wird validiert

**Ressourcen:**
- [Pattern Matching](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/functional/pattern-matching) – Switch-Expressions

---

#### Sub-Issue: Attack-Handling implementieren

**Labels:** `type:feature`, `area:gameplay`, `area:server`, `priority:p1`

**Beschreibung:**
Attack-Logik mit Cooldown und Range-Check.

**Aufgaben:**
- [ ] `HandleAttack()`:
  ```csharp
  private ActionEvent? HandleAttack(Player actor, ActionRequest request)
  {
      // Cooldown check
      if (_attackCooldowns.TryGetValue(actor.Id, out var lastAttack))
      {
          if ((DateTime.UtcNow - lastAttack).TotalMilliseconds < 500)
          {
              return new ActionEvent { ActorId = actor.Id, Result = ActionResult.OnCooldown };
          }
      }
      
      // Range check if target
      if (request.TargetId.HasValue)
      {
          var target = _world.GetEntityById(request.TargetId.Value);
          if (target == null)
              return new ActionEvent { ActorId = actor.Id, Result = ActionResult.InvalidTarget };
          
          if (Vector2.Distance(...) > 100f)
              return new ActionEvent { ActorId = actor.Id, Result = ActionResult.OutOfRange };
      }
      
      _attackCooldowns[actor.Id] = DateTime.UtcNow;
      return new ActionEvent { ActorId = actor.Id, ActionType = ActionType.Attack, Result = ActionResult.Success };
  }
  ```

**Akzeptanzkriterien:**
- [ ] Cooldown wird geprüft
- [ ] Range wird geprüft
- [ ] Erfolg/Fehler wird gesendet

**Ressourcen:**
- [ConcurrentDictionary](https://learn.microsoft.com/en-us/dotnet/api/system.collections.concurrent.concurrentdictionary-2) – Cooldowns

---

#### Sub-Issue: Emote-Handling implementieren

**Labels:** `type:feature`, `area:gameplay`, `area:server`, `priority:p1`

**Beschreibung:**
Emote-Verarbeitung ohne Validierung.

**Aufgaben:**
- [ ] `HandleEmote()`:
  ```csharp
  private ActionEvent HandleEmote(Player actor, ActionRequest request)
  {
      _logger.LogDebug("Player {Name} uses emote {Emote}", actor.Name, request.EmoteType);
      return new ActionEvent
      {
          ActorId = actor.Id,
          ActionType = ActionType.Emote,
          EmoteType = request.EmoteType,
          Result = ActionResult.Success
      };
  }
  ```

**Akzeptanzkriterien:**
- [ ] Emote wird gebroadcastet
- [ ] Kein Cooldown nötig

**Ressourcen:**
- [Logging](https://learn.microsoft.com/en-us/dotnet/core/extensions/logging) – Debug-Logs

---

### Epik: Dummy-Mob für Attack-Ziel

**Labels:** `type:epic`, `area:gameplay`, `area:server`, `priority:p2`

**Beschreibung:**
Dummy-Mob als Angriffsziel erstellen.

> **📌 Dies ist ein Epik-Issue.** Die Arbeit wurde in folgende Sub-Issues aufgeteilt:

**Sub-Issues:**
- [ ] Sub-Issue: Entity-Basisklasse erstellen
- [ ] Sub-Issue: Mob-Klasse erstellen

---

#### Sub-Issue: Entity-Basisklasse erstellen

**Labels:** `type:feature`, `area:gameplay`, `area:server`, `priority:p2`

**Beschreibung:**
Abstrakte Entity-Basisklasse für alle Spielobjekte.

**Aufgaben:**
- [ ] Datei: `server/Mmo.Server/Entities/Entity.cs`
  ```csharp
  public abstract class Entity
  {
      public Guid Id { get; init; } = Guid.NewGuid();
      public float X { get; set; }
      public float Y { get; set; }
      public abstract EntityType EntityType { get; }
  }
  
  public enum EntityType { Player = 0, Mob = 1, Npc = 2 }
  ```

**Akzeptanzkriterien:**
- [ ] Basisklasse ist abstrakt
- [ ] Id und Position sind definiert

**Ressourcen:**
- [Abstract Classes](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/abstract-and-sealed-classes-and-class-members) – Abstrakte Klassen

---

#### Sub-Issue: Mob-Klasse erstellen

**Labels:** `type:feature`, `area:gameplay`, `area:server`, `priority:p2`

**Beschreibung:**
Mob-Klasse mit Health und Respawn.

**Aufgaben:**
- [ ] Datei: `server/Mmo.Server/Entities/Mob.cs`
  ```csharp
  public class Mob : Entity
  {
      public override EntityType EntityType => EntityType.Mob;
      public required string Name { get; init; }
      public int Health { get; set; } = 100;
      public int MaxHealth { get; init; } = 100;
      public bool IsAlive => Health > 0;
      
      public void TakeDamage(int damage)
      {
          Health = Math.Max(0, Health - damage);
      }
  }
  ```
- [ ] In World.SpawnDummyMob() aufrufen

**Akzeptanzkriterien:**
- [ ] Mob kann Schaden nehmen
- [ ] Mob ist attackierbar

**Ressourcen:**
- [Game Entities](https://learn.microsoft.com/en-us/archive/msdn-magazine/2015/march/c-game-programming-building-a-basic-game-engine) – Entity-Design

---

### Epik: Action-Input im Client

**Labels:** `type:epic`, `area:gameplay`, `area:client`, `priority:p1`

**Beschreibung:**
Eingabebehandlung für Aktionen.

> **📌 Dies ist ein Epik-Issue.** Die Arbeit wurde in folgende Sub-Issues aufgeteilt:

**Sub-Issues:**
- [ ] Sub-Issue: InputMap konfigurieren
- [ ] Sub-Issue: ActionController erstellen

---

#### Sub-Issue: InputMap konfigurieren

**Labels:** `type:feature`, `area:gameplay`, `area:client`, `priority:p1`

**Beschreibung:**
Tasten für Aktionen definieren.

**Aufgaben:**
- [ ] In Godot Editor → Project Settings → Input Map:
  - [ ] `action_attack`: Space
  - [ ] `action_emote`: E

**Akzeptanzkriterien:**
- [ ] Tasten sind konfiguriert
- [ ] Tasten werden erkannt

**Ressourcen:**
- [InputMap](https://docs.godotengine.org/en/stable/classes/class_inputmap.html) – Tastenbelegung

---

#### Sub-Issue: ActionController erstellen

**Labels:** `type:feature`, `area:gameplay`, `area:client`, `priority:p1`

**Beschreibung:**
Controller für Action-Input.

**Aufgaben:**
- [ ] Datei: `res://scripts/player/ActionController.cs`
  ```csharp
  public partial class ActionController : Node
  {
      private DateTime _lastAttackTime = DateTime.MinValue;
      
      public override void _Process(double delta)
      {
          if (Input.IsActionJustPressed("action_attack"))
              TryAttack();
          if (Input.IsActionJustPressed("action_emote"))
              TryEmote();
      }
      
      private void TryAttack()
      {
          if ((DateTime.UtcNow - _lastAttackTime).TotalMilliseconds < 500)
              return;
          
          var request = new ActionRequest { ActionType = ActionType.Attack };
          NetworkClient.Instance.Send(request);
          _lastAttackTime = DateTime.UtcNow;
      }
  }
  ```

**Akzeptanzkriterien:**
- [ ] Space löst Attack aus
- [ ] Client-seitiger Cooldown

**Ressourcen:**
- [Input in Godot](https://docs.godotengine.org/en/stable/tutorials/inputs/input_examples.html) – Input-Handling

---

### Epik: Action-Visualisierung

**Labels:** `type:epic`, `area:gameplay`, `area:client`, `priority:p1`

**Beschreibung:**
Visuelle Darstellung von Aktionen.

> **📌 Dies ist ein Epik-Issue.** Die Arbeit wurde in folgende Sub-Issues aufgeteilt:

**Sub-Issues:**
- [ ] Sub-Issue: Attack-Effekt erstellen
- [ ] Sub-Issue: Emote-Popup erstellen

---

#### Sub-Issue: Attack-Effekt erstellen

**Labels:** `type:feature`, `area:gameplay`, `area:client`, `priority:p1`

**Beschreibung:**
Visueller Effekt für Angriffe.

**Aufgaben:**
- [ ] Szene: `res://scenes/effects/AttackEffect.tscn`
  ```
  AttackEffect (Node2D)
  ├── Sprite2D (Slash-Grafik)
  └── Timer (0.3s → QueueFree)
  ```
- [ ] Ziel blinkt rot bei Treffer:
  ```csharp
  private void ShowDamageReaction(Node2D target)
  {
      var tween = CreateTween();
      tween.TweenProperty(target, "modulate", new Color(1, 0.3f, 0.3f), 0.1f);
      tween.TweenProperty(target, "modulate", Colors.White, 0.1f);
  }
  ```

**Akzeptanzkriterien:**
- [ ] Effekt ist sichtbar
- [ ] Ziel blinkt

**Ressourcen:**
- [Tween](https://docs.godotengine.org/en/stable/classes/class_tween.html) – Animationen

---

#### Sub-Issue: Emote-Popup erstellen

**Labels:** `type:feature`, `area:gameplay`, `area:client`, `priority:p1`

**Beschreibung:**
Popup für Emotes über Spielerkopf.

**Aufgaben:**
- [ ] Szene: `res://scenes/effects/EmotePopup.tscn`
- [ ] Emoji-Text anzeigen:
  ```csharp
  private string GetEmoteText(EmoteType emote) => emote switch
  {
      EmoteType.Wave => "👋",
      EmoteType.Dance => "💃",
      EmoteType.Sit => "🪑",
      _ => "❓"
  };
  ```
- [ ] Nach oben faden und verschwinden

**Akzeptanzkriterien:**
- [ ] Emoji erscheint über Spieler
- [ ] Verschwindet nach 2 Sekunden

**Ressourcen:**
- [Label](https://docs.godotengine.org/en/stable/classes/class_label.html) – Text-Anzeige
### Phase 4 – Abschluss & Refactoring

---

### Epik: Code-Cleanup und Naming-Konventionen

**Labels:** `type:epic`, `type:chore`, `priority:p2`

**Beschreibung:**
Code aufräumen und Konventionen vereinheitlichen.

> **📌 Dies ist ein Epik-Issue.** Die Arbeit wurde in folgende Sub-Issues aufgeteilt:

**Sub-Issues:**
- [ ] Sub-Issue: Naming-Konventionen prüfen
- [ ] Sub-Issue: Code-Formatierung durchführen
- [ ] Sub-Issue: Unused Code und TODOs bereinigen

---

#### Sub-Issue: Naming-Konventionen prüfen

**Labels:** `type:chore`, `priority:p2`

**Beschreibung:**
Alle Namen auf C#-Konventionen prüfen.

**Aufgaben:**
- [ ] Konventionen prüfen:
  - Klassen: PascalCase (`GameServer`)
  - Methoden: PascalCase (`HandleLogin`)
  - Private Felder: _camelCase (`_players`)
  - Lokale Variablen: camelCase (`player`)
  - Interfaces: IPascalCase (`INetworkMessage`)
- [ ] Abweichungen korrigieren

**Akzeptanzkriterien:**
- [ ] Alle Namen folgen Konventionen
- [ ] Dokumentiert

**Ressourcen:**
- [C# Coding Conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions) – Richtlinien

---

#### Sub-Issue: Code-Formatierung durchführen

**Labels:** `type:chore`, `priority:p2`

**Beschreibung:**
`dotnet format` ausführen.

**Aufgaben:**
- [ ] `dotnet format` ausführen
- [ ] Warnings aktivieren in .csproj
- [ ] Compiler-Warnings beheben

**Akzeptanzkriterien:**
- [ ] Keine Compiler-Warnings
- [ ] Einheitliche Formatierung

**Ressourcen:**
- [dotnet format](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-format) – Formatierung

---

#### Sub-Issue: Unused Code und TODOs bereinigen

**Labels:** `type:chore`, `priority:p2`

**Beschreibung:**
Toten Code und TODOs aufräumen.

**Aufgaben:**
- [ ] Unused usings entfernen
- [ ] Auskommentierten Code entfernen
- [ ] Alle `// TODO:` suchen und auflösen

**Akzeptanzkriterien:**
- [ ] Kein toter Code
- [ ] Keine offenen TODOs ohne Issue

**Ressourcen:**
- [Code Cleanup](https://learn.microsoft.com/en-us/visualstudio/ide/reference/remove-unused-usings) – VS Cleanup

---

### Epik: README und Dokumentation

**Labels:** `type:epic`, `type:documentation`, `priority:p1`

**Beschreibung:**
Dokumentation aktualisieren.

> **📌 Dies ist ein Epik-Issue.** Die Arbeit wurde in folgende Sub-Issues aufgeteilt:

**Sub-Issues:**
- [ ] Sub-Issue: README aktualisieren
- [ ] Sub-Issue: Setup-Anleitung schreiben

---

#### Sub-Issue: README aktualisieren

**Labels:** `type:documentation`, `priority:p1`

**Beschreibung:**
README mit aktuellem Feature-Stand.

**Aufgaben:**
- [ ] Feature-Übersicht:
  ```markdown
  ## Features
  - ✅ Multiplayer Login/Logout
  - ✅ Echtzeit-Bewegung
  - ✅ Charakter-Persistenz
  - ✅ Global-Chat
  - ✅ Basis-Aktionen
  ```
- [ ] Tech-Stack dokumentieren
- [ ] Bekannte Limitierungen

**Akzeptanzkriterien:**
- [ ] README ist aktuell
- [ ] Features sind gelistet

**Ressourcen:**
- [README Best Practices](https://docs.github.com/en/repositories/managing-your-repositorys-settings-and-features/customizing-your-repository/about-readmes) – README

---

#### Sub-Issue: Setup-Anleitung schreiben

**Labels:** `type:documentation`, `priority:p1`

**Beschreibung:**
Schritt-für-Schritt Setup-Anleitung.

**Aufgaben:**
- [ ] Voraussetzungen:
  - .NET 8 SDK
  - Godot 4.x mit .NET
- [ ] Build-Anleitung:
  ```bash
  # Server
  cd server/Mmo.Server
  dotnet run
  
  # Client
  # Godot öffnen → F5
  ```

**Akzeptanzkriterien:**
- [ ] Neue Entwickler können starten
- [ ] Anleitung ist getestet

**Ressourcen:**
- [Markdown](https://learn.microsoft.com/en-us/contribute/markdown-reference) – Formatierung

---

### Epik: Manueller Testplan

**Labels:** `type:epic`, `type:test`, `priority:p1`

**Beschreibung:**
Testplan für Phase 4 erstellen und durchführen.

> **📌 Dies ist ein Epik-Issue.** Die Arbeit wurde in folgende Sub-Issues aufgeteilt:

**Sub-Issues:**
- [ ] Sub-Issue: Testplan erstellen
- [ ] Sub-Issue: Tests durchführen

---

#### Sub-Issue: Testplan erstellen

**Labels:** `type:test`, `priority:p1`

**Beschreibung:**
Testfälle dokumentieren.

**Aufgaben:**
- [ ] Datei: `docs/TESTPLAN_PHASE4.md`
- [ ] Testfälle:
  - Login mit neuem/existierendem Charakter
  - Movement-Synchronisation
  - Chat senden/empfangen
  - Attack/Emote ausführen
  - Logout und Reconnect

**Akzeptanzkriterien:**
- [ ] Alle Features abgedeckt
- [ ] Schritte sind klar

**Ressourcen:**
- [Test Cases](https://learn.microsoft.com/en-us/azure/devops/test/create-test-cases) – Testplan-Struktur

---

#### Sub-Issue: Tests durchführen

**Labels:** `type:test`, `priority:p1`

**Beschreibung:**
Tests durchführen und dokumentieren.

**Aufgaben:**
- [ ] Jeden Testfall durchführen
- [ ] Ergebnisse dokumentieren
- [ ] Bugs als Issues erfassen

**Akzeptanzkriterien:**
- [ ] Alle Tests durchgeführt
- [ ] Ergebnisse dokumentiert
- [ ] Bugs erfasst

**Ressourcen:**
- [Bug Reports](https://learn.microsoft.com/en-us/azure/devops/boards/backlogs/manage-bugs) – Bug-Dokumentation
## Zusammenfassung

### Übersicht der Sub-Issues für bestehende Issues (#8, #9, #11, #12, #14, #15)

| Original-Issue | Sub-Issues |
|---------------|------------|
| #8 NetworkServer | 8a: TCP-Listener, 8b: ClientConnection, 8c: Message-Loop & Events |
| #9 MessageRouter | 9a: Router-Basisklasse, 9b: Login-Handler |
| #11 Login-UI | 11a: UI erstellen, 11b: Netzwerk-Integration, 11c: Response-Handling |
| #12 MoveRequest | 12a: LocalPlayerController, 12b: HandleMove Server |
| #14 Remote-Player | 14a: GameManager, 14b: PlayerNode-Szene, 14c: Interpolation |
| #15 Movement-Tests | 15a: Tuning, 15b: Unit-Tests, 15c: Manuelle Tests |

**Gesamt: 17 Sub-Issues für 6 bestehende Issues**

### Übersicht der neuen Epik-Issues und Sub-Issues

| Phase | Epik-Issue | Anzahl Sub-Issues |
|-------|-----------|-------------------|
| Phase 2 | Verbindungsaufbau-Flow | 3 |
| Phase 2 | Shared DTOs | 3 |
| Phase 2 | Logging | 3 |
| Phase 3 | 2D-Tilemap | 3 |
| Phase 3 | Spawn-Logik | 3 |
| Phase 3 | Koordinaten-Mapping | 2 |
| Phase 3 | Weltgrenzen | 2 |
| Phase 4 | Datenmodell Persistenz | 3 |
| Phase 4 | Persistenzschicht | 3 |
| Phase 4 | Login laden | 2 |
| Phase 4 | Logout speichern | 2 |
| Phase 4 | Fehlerbehandlung Persistenz | 2 |
| Phase 4 | Chat-Nachrichtentypen | 3 |
| Phase 4 | Chat-Handling Server | 3 |
| Phase 4 | Rate-Limiting | 2 |
| Phase 4 | Chat-UI | 2 |
| Phase 4 | Chat-Integration | 2 |
| Phase 4 | Action-Nachrichtentypen | 2 |
| Phase 4 | Action-Handler | 3 |
| Phase 4 | Dummy-Mob | 2 |
| Phase 4 | Action-Input | 2 |
| Phase 4 | Action-Visualisierung | 2 |
| Phase 4 | Code-Cleanup | 3 |
| Phase 4 | README/Doku | 2 |
| Phase 4 | Testplan | 2 |
| **Gesamt** | **25 Epik-Issues** | **~61 Sub-Issues** |

---

## Statistik

| Kategorie | Anzahl |
|-----------|--------|
| Bestehende Issues analysiert | 15 |
| Epik-Issues für bestehende Issues | 6 |
| Sub-Issues für bestehende Issues | 17 |
| Neue Epik-Issues | 25 |
| Neue Sub-Issues | ~61 |
| **Gesamt Issues nach Umsetzung** | **~93** |

---

## Nächste Schritte

1. ✅ Sub-Issues für die 6 bestehenden großen Issues definiert
2. ✅ 25 neue Epik-Issues mit ~61 Sub-Issues definiert
3. Original-Issues (#8, #9, #11, #12, #14, #15) mit Verweisen auf Sub-Issues aktualisieren (siehe `docs/ISSUE_UPDATES.md`)
4. Issues in GitHub erstellen mit passenden Labels
5. Issues in sinnvoller Reihenfolge priorisieren (siehe `docs/ISSUE_HIERARCHY.md`)
