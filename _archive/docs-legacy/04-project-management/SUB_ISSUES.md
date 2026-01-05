# Sub-Issues für existierende Issues

**Version:** 1.0.0  
**Letzte Aktualisierung:** 2025-12-09  
**Status:** Aufgeteilt aus ISSUES_ROADMAP.md

---

## 📋 Übersicht

Dieses Dokument enthält **detaillierte Sub-Issue-Vorschläge** für bestehende große Issues, die in kleinere, besser handhabbare Aufgaben aufgeteilt werden sollten.

> **📌 Hinweis:** Für Zone-Konzept-Updates siehe [ISSUE_UPDATES_GUIDE.md](ISSUE_UPDATES_GUIDE.md).

---

# 2DMMO Issue-Roadmap bis Phase 4

Diese Dokumentation enthält:
1. **Sub-Issues** für existierende Issues, die aufgeteilt werden sollten
2. **Neue Issues** für fehlende Funktionalität bis Ende Phase 4

> **📌 Zone-Konzept:** Die Issues #7, #74, #75 und #76 wurden aktualisiert, um das Zone-Konzept aus der Architektur zu integrieren. Details siehe [ISSUE_UPDATES_GUIDE.md](ISSUE_UPDATES_GUIDE.md).

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

