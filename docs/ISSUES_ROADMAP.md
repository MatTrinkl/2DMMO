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
Den Verbindungsaufbau im Client robuster gestalten mit automatischem Retry und Benutzer-Feedback bei Fehlern.

**Aufgaben:**
- [ ] Timeout für Verbindungsaufbau konfigurieren (z.B. 5 Sekunden)
- [ ] Retry-Logik implementieren (z.B. 3 Versuche)
- [ ] Verbindungsstatus-Event im NetworkClient
- [ ] UI-Feedback während Verbindungsaufbau (Loading-Indicator)
- [ ] Fehlermeldung bei endgültigem Fehlschlag

**Akzeptanzkriterien:**
- [ ] Timeout wird korrekt behandelt
- [ ] Retry funktioniert transparent
- [ ] Benutzer sieht Fortschritt/Status

**Ressourcen:**
- [Polly für Retry-Patterns](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/implement-http-call-retries-exponential-backoff-polly) – Resiliente Anwendungen
- [CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken) – Abbruch von Operationen

---

#### Issue: Konsistente Verwendung der Shared DTOs

**Labels:** `type:chore`, `area:server`, `area:client`, `priority:p1`

**Beschreibung:**
Sicherstellen, dass Client und Server exakt dieselben DTO-Klassen aus dem Shared-Projekt verwenden.

**Aufgaben:**
- [ ] Shared-Projekt als NuGet-Referenz oder Projektlink im Client
- [ ] Keine Duplikation von DTO-Klassen im Client
- [ ] Namespace-Konventionen vereinheitlichen
- [ ] Dokumentation der Shared-Typen

**Akzeptanzkriterien:**
- [ ] Eine einzige Quelle für alle DTOs
- [ ] Client und Server verwenden identische Typen
- [ ] Build funktioniert für beide Projekte

**Ressourcen:**
- [Projektverweise in .NET](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-add-reference) – Projektreferenzen
- [Shared Projects](https://learn.microsoft.com/en-us/xamarin/cross-platform/app-fundamentals/shared-projects) – Shared Code Pattern

---

#### Issue: Grundlegendes Logging im Network-Code

**Labels:** `type:feature`, `area:server`, `area:client`, `priority:p2`

**Beschreibung:**
Strukturiertes Logging für Netzwerk-Operationen einführen.

**Aufgaben:**
- [ ] Logging-Framework wählen (z.B. Microsoft.Extensions.Logging)
- [ ] Logger in NetworkServer und ClientConnection
- [ ] Log-Level: Debug für Details, Info für wichtige Events, Error für Fehler
- [ ] Logging im Client (Godot GD.Print oder eigenes System)
- [ ] Log-Ausgabe formatieren (Timestamp, Level, Message)

**Akzeptanzkriterien:**
- [ ] Alle wichtigen Netzwerk-Events werden geloggt
- [ ] Log-Level ist konfigurierbar
- [ ] Logs helfen beim Debugging

**Ressourcen:**
- [Logging in .NET](https://learn.microsoft.com/en-us/dotnet/core/extensions/logging) – Logging-Framework
- [ILogger Interface](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.logging.ilogger) – Logger-API

---

### Phase 3 – Welt & Movement

---

#### Issue: 2D-Tilemap für Prototyp-Welt erstellen

**Labels:** `type:feature`, `area:client`, `priority:p1`

**Beschreibung:**
Eine einfache 2D-Tilemap in Godot erstellen als visuelle Grundlage für die Spielwelt.

**Aufgaben:**
- [ ] TileMap-Node zur Hauptszene hinzufügen
- [ ] Einfaches Tileset erstellen oder Platzhalter-Tiles verwenden
- [ ] Kleine Test-Map gestalten (z.B. 50x50 Tiles)
- [ ] Boden-/Gras-Tiles für begehbaren Bereich
- [ ] Optionale Rand-Tiles für Grenzen

**Akzeptanzkriterien:**
- [ ] Tilemap ist sichtbar im Spiel
- [ ] Spieler bewegt sich über die Map
- [ ] Map hat klare Grenzen

**Ressourcen:**
- [TileMap in Godot 4](https://docs.godotengine.org/en/stable/tutorials/2d/using_tilemaps.html) – TileMap-Tutorial
- [TileSet Editor](https://docs.godotengine.org/en/stable/classes/class_tileset.html) – TileSet-Dokumentation

---

#### Issue: Serverseitige Startposition und Spawn-Logik

**Labels:** `type:feature`, `area:server`, `priority:p1`

**Beschreibung:**
Die Startposition für neue Spieler serverseitig definieren und Spawn-Logik implementieren.

**Aufgaben:**
- [ ] Standard-Spawn-Position definieren (z.B. Weltmitte)
- [ ] Spawn-Position bei `World.CreatePlayer` setzen
- [ ] Optional: Mehrere Spawn-Punkte für Variation
- [ ] Spawn-Position in LoginResponse oder erstem StateUpdate übermitteln
- [ ] Kollisionsprüfung bei Spawn (keine Überlappung)

**Akzeptanzkriterien:**
- [ ] Neue Spieler spawnen an definierter Position
- [ ] Position ist innerhalb der Weltgrenzen
- [ ] Mehrere Spieler spawnen nicht exakt übereinander

**Ressourcen:**
- [Random in C#](https://learn.microsoft.com/en-us/dotnet/api/system.random) – Zufällige Spawn-Variation
- [System.Numerics](https://learn.microsoft.com/en-us/dotnet/api/system.numerics) – Vektor-Mathematik

---

#### Issue: Mapping von Server-Positionen auf Godot-Koordinaten

**Labels:** `type:feature`, `area:client`, `priority:p1`

**Beschreibung:**
Die Server-Koordinaten (Float-Werte) auf Godot-Weltkoordinaten mappen.

**Aufgaben:**
- [ ] Koordinatensystem definieren (1 Server-Einheit = X Pixel)
- [ ] Umrechnungsfunktionen erstellen
- [ ] Server-Positionen auf Node2D.Position anwenden
- [ ] Kamera auf Spieler zentrieren
- [ ] Dokumentation des Koordinatensystems

**Akzeptanzkriterien:**
- [ ] Spielerpositionen werden korrekt dargestellt
- [ ] Bewegungsrichtungen stimmen (oben/unten/links/rechts)
- [ ] Skalierung ist für Gameplay angemessen

**Ressourcen:**
- [2D-Koordinatensystem in Godot](https://docs.godotengine.org/en/stable/tutorials/2d/2d_movement.html) – 2D-Grundlagen
- [Camera2D in Godot](https://docs.godotengine.org/en/stable/classes/class_camera2d.html) – Kamera-Steuerung

---

#### Issue: Weltgrenzen im Server implementieren

**Labels:** `type:feature`, `area:server`, `priority:p1`

**Beschreibung:**
Weltgrenzen auf dem Server implementieren, damit Spieler nicht außerhalb der Karte laufen können.

**Aufgaben:**
- [ ] Weltgrenzen als Konstanten/Config definieren (MinX, MaxX, MinY, MaxY)
- [ ] Position bei Movement-Update clampen
- [ ] Grenzprüfung in `Player.Update()` oder Movement-Handler
- [ ] Weltgröße an Client kommunizieren (optional)

**Akzeptanzkriterien:**
- [ ] Spieler können nicht außerhalb der Grenzen laufen
- [ ] Bewegung entlang der Grenzen funktioniert
- [ ] Keine Teleportation oder Stuck-Zustände

**Ressourcen:**
- [Math.Clamp in C#](https://learn.microsoft.com/en-us/dotnet/api/system.math.clamp) – Werte begrenzen
- [Collision Detection Basics](https://learn.microsoft.com/en-us/archive/msdn-magazine/2014/march/windows-phone-building-a-2d-physics-game-engine) – Kollisionsprüfung

---

### Phase 4 – Persistenz

---

#### Issue: Datenmodell für Charakterpersistenz definieren

**Labels:** `type:feature`, `area:persistenz`, `priority:p1`

**Beschreibung:**
Das Datenmodell für die Speicherung von Charakterdaten definieren.

**Aufgaben:**
- [ ] `CharacterData`-Klasse im Shared-Projekt erstellen
- [ ] Properties: PlayerId, Name, X, Y, LastLogin, CreatedAt
- [ ] Optional: Level, Experience (für spätere Erweiterung)
- [ ] JSON-Serialisierbarkeit sicherstellen
- [ ] Dokumentation der Datenstruktur

**Akzeptanzkriterien:**
- [ ] Datenmodell ist definiert und dokumentiert
- [ ] Klasse ist serialisierbar
- [ ] Alle notwendigen Felder sind vorhanden

**Ressourcen:**
- [Records in C#](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/record) – Immutable Data Types
- [System.Text.Json Serialization](https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/how-to) – JSON-Serialisierung

---

#### Issue: Persistenzschicht implementieren (File/JSON oder SQLite)

**Labels:** `type:feature`, `area:persistenz`, `area:server`, `priority:p1`

**Beschreibung:**
Eine einfache Persistenzschicht implementieren, die Charakterdaten speichern und laden kann.

**Aufgaben:**
- [ ] `ICharacterRepository` Interface definieren
- [ ] Methoden: `SaveAsync`, `LoadAsync`, `GetByNameAsync`, `ExistsAsync`
- [ ] `FileCharacterRepository` implementieren (JSON-Dateien)
- [ ] Alternativ: `SqliteCharacterRepository` implementieren
- [ ] Unit-Tests für Repository

**Akzeptanzkriterien:**
- [ ] Charakterdaten können gespeichert werden
- [ ] Charakterdaten können geladen werden
- [ ] Repository ist austauschbar (Interface)

**Ressourcen:**
- [File I/O in .NET](https://learn.microsoft.com/en-us/dotnet/standard/io/) – Dateioperationen
- [SQLite mit ADO.NET](https://learn.microsoft.com/en-us/dotnet/standard/data/sqlite/) – SQLite in .NET

---

#### Issue: Charakterdaten beim Login laden

**Labels:** `type:feature`, `area:persistenz`, `area:server`, `priority:p1`

**Beschreibung:**
Beim Login prüfen, ob ein Charakter existiert und dessen Daten laden.

**Aufgaben:**
- [ ] Im Login-Handler Repository abfragen
- [ ] Falls Charakter existiert: Daten laden und Position setzen
- [ ] Falls neuer Charakter: Standardwerte setzen und speichern
- [ ] Geladene Position an den Client senden
- [ ] Logging beim Laden/Erstellen

**Akzeptanzkriterien:**
- [ ] Existierender Charakter behält seine Position
- [ ] Neuer Charakter startet an Spawn-Position
- [ ] Fehler beim Laden werden behandelt

**Ressourcen:**
- [Exception Handling](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/exceptions/) – Fehlerbehandlung
- [Async/Await Best Practices](https://learn.microsoft.com/en-us/archive/msdn-magazine/2013/march/async-await-best-practices-in-asynchronous-programming) – Asynchrone Programmierung

---

#### Issue: Charakterdaten beim Logout/Disconnect speichern

**Labels:** `type:feature`, `area:persistenz`, `area:server`, `priority:p1`

**Beschreibung:**
Bei Logout oder Verbindungsabbruch die aktuellen Charakterdaten speichern.

**Aufgaben:**
- [ ] Bei Disconnect: Spielerdaten aus World holen
- [ ] CharacterData aktualisieren (Position, LastLogin)
- [ ] Über Repository speichern
- [ ] Bei Server-Shutdown: Alle Spieler speichern
- [ ] Fehlerbehandlung (Speichern darf nicht blocken)

**Akzeptanzkriterien:**
- [ ] Position wird bei normalem Logout gespeichert
- [ ] Position wird bei Verbindungsabbruch gespeichert
- [ ] Server-Shutdown speichert alle Spieler

**Ressourcen:**
- [IHostApplicationLifetime](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.hosting.ihostapplicationlifetime) – Application Shutdown Events
- [Task.WhenAll](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task.whenall) – Parallele Speicherung

---

#### Issue: Fehlerbehandlung für Persistenz

**Labels:** `type:feature`, `area:persistenz`, `area:server`, `priority:p2`

**Beschreibung:**
Robuste Fehlerbehandlung für Persistenz-Operationen implementieren.

**Aufgaben:**
- [ ] Fehler beim Laden: Warnung loggen, Default-Werte verwenden
- [ ] Fehler beim Speichern: Retry-Logik, Warnung loggen
- [ ] Korrupte Daten erkennen und behandeln
- [ ] Backup-Mechanismus für wichtige Daten (optional)
- [ ] Monitoring/Alerting bei persistenten Fehlern

**Akzeptanzkriterien:**
- [ ] Server crashed nicht bei Persistenz-Fehlern
- [ ] Fehler werden geloggt
- [ ] Spieler können weiterspielen auch bei Speicher-Fehlern

**Ressourcen:**
- [Resilient Applications](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/) – Resilienz-Muster
- [Structured Logging](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/logging/) – Strukturiertes Logging

---

### Phase 4 – Chat-System

---

#### Issue: Chat-Nachrichtentypen im Shared-Projekt

**Labels:** `type:feature`, `area:chat`, `priority:p1`

**Beschreibung:**
Die Nachrichtentypen für das Chat-System im Shared-Projekt definieren.

**Aufgaben:**
- [ ] `ChatMessageRequest` DTO: PlayerId, Channel, Message
- [ ] `ChatMessageBroadcast` DTO: PlayerId, PlayerName, Channel, Message, Timestamp
- [ ] MessageType-Enum um Chat-Typen erweitern
- [ ] Channel-Enum: Global, Zone, Whisper (für später)
- [ ] Validierungslogik für Nachrichtenlänge

**Akzeptanzkriterien:**
- [ ] DTOs sind definiert und serialisierbar
- [ ] MessageType-Enum ist erweitert
- [ ] Maximale Nachrichtenlänge ist definiert

**Ressourcen:**
- [System.Text.Json](https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/overview) – JSON-Serialisierung
- [Enums in C#](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/enum) – Enumerationen

---

#### Issue: Serverseitiges Chat-Handling implementieren

**Labels:** `type:feature`, `area:chat`, `area:server`, `priority:p1`

**Beschreibung:**
Das Chat-Handling auf dem Server implementieren, das Nachrichten empfängt und an Clients broadcastet.

**Aufgaben:**
- [ ] `HandleChatMessage` im MessageRouter
- [ ] Validierung: Absender ist eingeloggt, Nachricht nicht leer
- [ ] Spielername zum Broadcast hinzufügen
- [ ] Global-Channel: An alle Clients broadcasten
- [ ] Timestamp setzen

**Akzeptanzkriterien:**
- [ ] Chat-Nachrichten werden an alle Clients gesendet
- [ ] Absendername wird korrekt angezeigt
- [ ] Ungültige Nachrichten werden abgelehnt

**Ressourcen:**
- [Broadcasting Patterns](https://learn.microsoft.com/en-us/dotnet/api/system.collections.concurrent.concurrentdictionary-2) – Thread-sichere Collections
- [DateTime in .NET](https://learn.microsoft.com/en-us/dotnet/api/system.datetime) – Zeitstempel

---

#### Issue: Einfaches Rate-Limiting für Chat

**Labels:** `type:feature`, `area:chat`, `area:server`, `priority:p2`

**Beschreibung:**
Ein einfaches Rate-Limiting implementieren, um Chat-Spam zu verhindern.

**Aufgaben:**
- [ ] Nachrichten-Counter pro Spieler
- [ ] Zeitfenster definieren (z.B. max 5 Nachrichten/10 Sekunden)
- [ ] Bei Überschreitung: Nachricht ablehnen, Warnung senden
- [ ] Counter nach Zeitfenster zurücksetzen
- [ ] Konfigurierbare Limits

**Akzeptanzkriterien:**
- [ ] Spam wird begrenzt
- [ ] Normale Nutzung wird nicht beeinträchtigt
- [ ] Spieler erhalten Feedback bei Limit

**Ressourcen:**
- [Rate Limiting in .NET](https://learn.microsoft.com/en-us/dotnet/api/system.threading.ratelimiting) – Rate Limiting API
- [Stopwatch für Timing](https://learn.microsoft.com/en-us/dotnet/api/system.diagnostics.stopwatch) – Zeitmessung

---

#### Issue: Chat-UI im Godot-Client erstellen

**Labels:** `type:feature`, `area:chat`, `area:client`, `priority:p1`

**Beschreibung:**
Die Chat-UI im Godot-Client implementieren mit Eingabefeld und Nachrichtenanzeige.

**Aufgaben:**
- [ ] `ChatPanel`-Szene erstellen
- [ ] RichTextLabel oder ItemList für Nachrichtenlog
- [ ] LineEdit für Eingabe
- [ ] Enter-Taste sendet Nachricht
- [ ] Scroll-Funktion für ältere Nachrichten

**Akzeptanzkriterien:**
- [ ] Chat-Fenster ist sichtbar im Spiel
- [ ] Nachrichten können eingegeben werden
- [ ] Empfangene Nachrichten werden angezeigt

**Ressourcen:**
- [RichTextLabel in Godot](https://docs.godotengine.org/en/stable/classes/class_richtextlabel.html) – Formatierter Text
- [LineEdit in Godot](https://docs.godotengine.org/en/stable/classes/class_lineedit.html) – Texteingabe

---

#### Issue: Chat-Nachrichten senden und empfangen im Client

**Labels:** `type:feature`, `area:chat`, `area:client`, `priority:p1`

**Beschreibung:**
Die Integration von Chat-Nachrichten mit dem NetworkClient.

**Aufgaben:**
- [ ] Handler für ChatMessageBroadcast registrieren
- [ ] Empfangene Nachrichten ans ChatPanel weiterleiten
- [ ] ChatMessageRequest beim Absenden erstellen
- [ ] Nachricht über NetworkClient senden
- [ ] Eingabefeld nach Senden leeren

**Akzeptanzkriterien:**
- [ ] Gesendete Nachrichten erscheinen bei anderen Spielern
- [ ] Eigene Nachrichten werden ebenfalls angezeigt
- [ ] Keine doppelten Nachrichten

**Ressourcen:**
- [Signals in Godot C#](https://docs.godotengine.org/en/stable/tutorials/scripting/c_sharp/c_sharp_signals.html) – Ereigniskommunikation
- [Godot Input Handling](https://docs.godotengine.org/en/stable/tutorials/inputs/input_examples.html) – Enter-Taste abfangen

---

### Phase 4 – Erste Gameplay-Aktion

---

#### Issue: Action-Nachrichtentypen definieren

**Labels:** `type:feature`, `area:gameplay`, `priority:p1`

**Beschreibung:**
Die Nachrichtentypen für Gameplay-Aktionen (Attack/Emote) definieren.

**Aufgaben:**
- [ ] `ActionRequest` DTO: PlayerId, ActionType, TargetId (optional)
- [ ] `ActionEvent` DTO: PlayerId, ActionType, TargetId, Result
- [ ] ActionType-Enum: Attack, Emote, Interact
- [ ] EmoteType-Enum: Wave, Dance, Sit (beispielhaft)
- [ ] MessageType-Enum erweitern

**Akzeptanzkriterien:**
- [ ] DTOs sind definiert
- [ ] Enums sind definiert
- [ ] Serialisierung funktioniert

**Ressourcen:**
- [Enums in C#](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/enum) – Enumerationen
- [System.Text.Json Polymorphism](https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/polymorphism) – Polymorphe Serialisierung

---

#### Issue: Action-Handler im Server implementieren

**Labels:** `type:feature`, `area:gameplay`, `area:server`, `priority:p1`

**Beschreibung:**
Die serverseitige Verarbeitung von Aktionen implementieren.

**Aufgaben:**
- [ ] `HandleAction` im MessageRouter
- [ ] ActionType-basierte Verarbeitung
- [ ] Attack: Ziel validieren (falls vorhanden), Dummy-Effekt
- [ ] Emote: Keine Validierung nötig
- [ ] ActionEvent an relevante Clients broadcasten

**Akzeptanzkriterien:**
- [ ] Aktionen werden verarbeitet
- [ ] ActionEvents werden gebroadcastet
- [ ] Ungültige Aktionen werden abgelehnt

**Ressourcen:**
- [Pattern Matching](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/functional/pattern-matching) – Switch-Expressions
- [Broadcasting](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1.foreach) – Iteration über Clients

---

#### Issue: Dummy-Mob für Attack-Ziel erstellen

**Labels:** `type:feature`, `area:gameplay`, `area:server`, `priority:p2`

**Beschreibung:**
Einen einfachen Dummy-Mob auf dem Server erstellen, der als Angriffsziel dient.

**Aufgaben:**
- [ ] `Mob`-Klasse mit Id, Position, Type
- [ ] Dummy-Mob beim Server-Start erstellen
- [ ] Mob-Position in PlayerStateUpdate oder separatem Update
- [ ] Attack auf Mob: Einfache Reaktion (Log/Event)
- [ ] Optional: Mob-Respawn nach "Tod"

**Akzeptanzkriterien:**
- [ ] Dummy-Mob existiert in der Welt
- [ ] Mob ist auf dem Client sichtbar
- [ ] Attack auf Mob löst Event aus

**Ressourcen:**
- [Entity Component System Patterns](https://learn.microsoft.com/en-us/archive/msdn-magazine/2015/march/c-game-programming-building-a-basic-game-engine) – Game-Entities
- [Object Pooling](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.objectpool) – Effizientes Mob-Management

---

#### Issue: Action-Input im Client implementieren

**Labels:** `type:feature`, `area:gameplay`, `area:client`, `priority:p1`

**Beschreibung:**
Die Eingabebehandlung für Aktionen im Client implementieren.

**Aufgaben:**
- [ ] Taste für Attack (z.B. Leertaste)
- [ ] Taste für Emote (z.B. E)
- [ ] ActionRequest bei Tastendruck erstellen
- [ ] Nächstes Ziel ermitteln (für Attack)
- [ ] Request über NetworkClient senden

**Akzeptanzkriterien:**
- [ ] Tasten lösen Aktionen aus
- [ ] ActionRequests werden gesendet
- [ ] Kein Spam durch Cooldown/Debouncing

**Ressourcen:**
- [Input Actions in Godot](https://docs.godotengine.org/en/stable/tutorials/inputs/inputevent.html) – Input-System
- [InputMap](https://docs.godotengine.org/en/stable/classes/class_inputmap.html) – Tastenbelegung

---

#### Issue: Action-Visualisierung im Client

**Labels:** `type:feature`, `area:gameplay`, `area:client`, `priority:p1`

**Beschreibung:**
Die visuelle Darstellung von Aktionen im Client implementieren.

**Aufgaben:**
- [ ] Handler für ActionEvent im Client
- [ ] Attack: Einfache Animation oder Effekt (Particles, Sprite-Blink)
- [ ] Emote: Text-Popup oder Icon über Spieler
- [ ] AnimationPlayer für Sprite-Animationen (optional)
- [ ] Sound-Effekte (optional)

**Akzeptanzkriterien:**
- [ ] Aktionen sind sichtbar für alle Spieler
- [ ] Effekte sind kurz und nicht störend
- [ ] Performance bleibt akzeptabel

**Ressourcen:**
- [AnimationPlayer in Godot](https://docs.godotengine.org/en/stable/classes/class_animationplayer.html) – Animationen
- [Particles in Godot 4](https://docs.godotengine.org/en/stable/tutorials/2d/particle_systems_2d.html) – Partikel-Effekte

---

### Phase 4 – Abschluss & Refactoring

---

#### Issue: Code-Cleanup und Naming-Konventionen

**Labels:** `type:chore`, `priority:p2`

**Beschreibung:**
Code aufräumen und einheitliche Naming-Konventionen anwenden.

**Aufgaben:**
- [ ] Alle Klassen/Methoden auf einheitliche Benennung prüfen
- [ ] Unused Code entfernen
- [ ] TODO-Kommentare auflösen oder in Issues umwandeln
- [ ] Code-Formatierung vereinheitlichen
- [ ] Warnings bereinigen

**Akzeptanzkriterien:**
- [ ] Keine Compiler-Warnings
- [ ] Einheitliche Namensgebung
- [ ] Kein toter Code

**Ressourcen:**
- [C# Coding Conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions) – Microsoft Coding Guidelines
- [.NET Naming Guidelines](https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/naming-guidelines) – Namenskonventionen

---

#### Issue: README und Dokumentation aktualisieren

**Labels:** `type:documentation`, `priority:p1`

**Beschreibung:**
Die README und grundlegende Dokumentation für Phase 4 aktualisieren.

**Aufgaben:**
- [ ] README: Feature-Übersicht aktualisieren
- [ ] Build- und Run-Anleitung prüfen/aktualisieren
- [ ] Architektur-Diagramm (optional, als Text oder einfache Grafik)
- [ ] Bekannte Limitierungen dokumentieren
- [ ] Nächste Schritte/Roadmap-Ausblick

**Akzeptanzkriterien:**
- [ ] README ist aktuell
- [ ] Neue Entwickler können dem Setup folgen
- [ ] Aktuelle Features sind beschrieben

**Ressourcen:**
- [Markdown Syntax](https://learn.microsoft.com/en-us/contribute/markdown-reference) – Markdown-Referenz
- [Good README Practices](https://learn.microsoft.com/en-us/azure/devops/repos/git/create-a-readme) – README Best Practices

---

#### Issue: Manueller Testplan für Phase 4

**Labels:** `type:test`, `priority:p1`

**Beschreibung:**
Einen manuellen Testplan erstellen und durchführen, der alle Phase-4-Features abdeckt.

**Aufgaben:**
- [ ] Testplan-Dokument erstellen
- [ ] Testfälle:
  - [ ] Login mit neuem Charakter
  - [ ] Login mit existierendem Charakter (Position wiederhergestellt)
  - [ ] Movement-Synchronisation
  - [ ] Chat senden und empfangen
  - [ ] Action/Emote ausführen
  - [ ] Logout und Reconnect
  - [ ] Mehrere Clients gleichzeitig
- [ ] Tests durchführen und dokumentieren

**Akzeptanzkriterien:**
- [ ] Alle kritischen Pfade sind getestet
- [ ] Testergebnisse sind dokumentiert
- [ ] Gefundene Bugs sind als Issues erfasst

**Ressourcen:**
- [Test Plan Templates](https://learn.microsoft.com/en-us/azure/devops/test/create-test-cases) – Testplanung
- [Manual Testing Best Practices](https://learn.microsoft.com/en-us/azure/devops/test/) – Manuelle Tests

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
