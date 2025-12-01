# Aktualisierungen für bestehende Issues

Dieses Dokument zeigt, wie die bestehenden "großen" Issues aktualisiert werden sollten, um auf ihre Sub-Issues zu verweisen.

---

## Issue #8: NetworkServer und ClientConnection-Skelett

**Empfohlene Aktualisierung des Issue-Bodies:**

```markdown
Einen einfachen Netzwerkserver (TCP oder WebSocket) und eine `ClientConnection`-Abstraktion implementieren, der Verbindungen verwalten kann.

> **📌 Dies ist ein Epik-Issue.** Die Arbeit wurde in folgende Sub-Issues aufgeteilt:

**Sub-Issues:**
- [ ] #XX NetworkServer TCP-Listener implementieren
- [ ] #XX ClientConnection-Klasse implementieren  
- [ ] #XX Message-Lese-Loop und Events implementieren

**Dieses Issue kann geschlossen werden, wenn alle Sub-Issues erledigt sind.**

---

<details>
<summary>Ursprüngliche Aufgaben (zur Referenz)</summary>

**Aufgaben:**
- [ ] `NetworkServer`-Klasse:
  - [ ] Konstruktor mit Port-Parameter. 
  - [ ] `RunAsync(CancellationToken)` startet Listener.
  - [ ] Events:
    - [ ] `ClientConnected`
    - [ ] `ClientDisconnected`
    - [ ] `MessageReceived`
- [ ] `ClientConnection`-Klasse:
  - [ ] Eindeutige `Id` (Guid).
  - [ ] `Task SendAsync(INetworkMessage msg)`. 
  - [ ] Interner Lese-Loop für eingehende Nachrichten.
- [ ] `GameServer` registriert sich auf diese Events und loggt Verbindungsstatus.

</details>

**Akzeptanzkriterien:**
- [ ] Ein einfacher Test-Client kann eine Verbindung herstellen und wieder trennen.
- [ ] Server loggt Connect- und Disconnect-Ereignisse ohne Absturz.

## Ressourcen
- [TCP Sockets in .NET](https://learn.microsoft.com/en-us/dotnet/fundamentals/networking/sockets/socket-services) – Übersicht über Sockets
- [TcpListener und TcpClient](https://learn.microsoft.com/en-us/dotnet/fundamentals/networking/sockets/tcp-classes) – Doku zu .NET-TCP-Klassen
```

---

## Issue #9: MessageRouter implementieren und Login-Flow (In-Memory)

**Empfohlene Aktualisierung des Issue-Bodies:**

```markdown
Einen Message-Router einführen, der eingehende Nachrichten anhand des Typs an Handler verteilt, sowie einen simplen Login-Fluss in Memory implementieren. 

> **📌 Dies ist ein Epik-Issue.** Die Arbeit wurde in folgende Sub-Issues aufgeteilt:

**Sub-Issues:**
- [ ] #XX MessageRouter-Basisklasse implementieren
- [ ] #XX Login-Handler (In-Memory) implementieren

**Dieses Issue kann geschlossen werden, wenn alle Sub-Issues erledigt sind.**

---

<details>
<summary>Ursprüngliche Aufgaben (zur Referenz)</summary>

**Aufgaben:**
- [ ] `MessageRouter`-Klasse mit `Route(ClientConnection, INetworkMessage)`. 
- [ ] `HandleLogin(ClientConnection, LoginRequest)` implementieren:
  - [ ] Spieler in `World` erstellen.
  - [ ] `LoginResponse` mit `Success = true` und `PlayerId` senden.
- [ ] Spätere Handler (z. B. `HandleMove`) als Stub anlegen. 

</details>

**Akzeptanzkriterien:**
- [ ] Ein `LoginRequest` erzeugt einen Player in der `World`.
- [ ] Der Client erhält eine korrekte `LoginResponse`. 

## Ressourcen
- [C# Delegates & Events](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/delegates/) – Technische Basis für Nachrichten-Routing
- [DTOs in System.Text.Json](https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/overview) – Sinnvolle Datenverteilung
```

---

## Issue #11: Einfache Login-UI und LoginRequest-Senden im Client

**Empfohlene Aktualisierung des Issue-Bodies:**

```markdown
Eine simple Login-Oberfläche implementieren, über die der Spieler seinen Namen eingibt und ein `LoginRequest` an den Server gesendet wird.

> **📌 Dies ist ein Epik-Issue.** Die Arbeit wurde in folgende Sub-Issues aufgeteilt:

**Sub-Issues:**
- [ ] #XX LoginPanel-UI in Godot erstellen
- [ ] #XX Login-Button-Handler und Netzwerk-Integration
- [ ] #XX LoginResponse verarbeiten und Fehlerbehandlung

**Dieses Issue kann geschlossen werden, wenn alle Sub-Issues erledigt sind.**

---

<details>
<summary>Ursprüngliche Aufgaben (zur Referenz)</summary>

**Aufgaben:**
- [ ] `LoginPanel`-UI mit Textfeld und Button erstellen.
- [ ] Button-Handler:
  - [ ] Verbindung zu `localhost:7777` herstellen.
  - [ ] `LoginRequest` mit eingegebenem Namen senden.
- [ ] `LoginResponse` im Client auswerten:
  - [ ] Bei Erfolg: Login-UI ausblenden. 
  - [ ] Bei Fehler: Fehlermeldung in der UI anzeigen. 

</details>

**Akzeptanzkriterien:**
- [ ] Ein Spieler kann sich per UI mit Namen einloggen.
- [ ] Der Server erhält und verarbeitet `LoginRequest`. 
- [ ] Erfolg/Misserfolg wird in der UI sichtbar.

## Ressourcen
- [UI Controls in Godot](https://docs.godotengine.org/en/4.4/tutorials/ui/control_node_gallery.html) – Überblick über Control Nodes für UI
- [Godot C#/.NET für UI-Events](https://docs.godotengine.org/en/stable/tutorials/scripting/c_sharp/index.html) – Grundlagen zur C#-UI-Programmierung in Godot
```

---

## Issue #12: MoveRequest verarbeiten und Player-Position serverseitig updaten

**Empfohlene Aktualisierung des Issue-Bodies:**

```markdown
Die serverseitige Verarbeitung von Bewegungsbefehlen implementieren, sodass der Server Positionsänderungen verwaltet.

> **📌 Dies ist ein Epik-Issue.** Die Arbeit wurde in folgende Sub-Issues aufgeteilt:

**Sub-Issues:**
- [ ] #XX LocalPlayerController im Client implementieren
- [ ] #XX HandleMove im Server implementieren

**Dieses Issue kann geschlossen werden, wenn alle Sub-Issues erledigt sind.**

---

<details>
<summary>Ursprüngliche Aufgaben (zur Referenz)</summary>

**Aufgaben:**
- [ ] Client:
  - [ ] `LocalPlayerController` implementieren, der Input (WASD/Arrow-Keys) liest. 
  - [ ] Richtung als `MoveRequest { DirX, DirY }` an den Server senden. 
- [ ] Server:
  - [ ] `HandleMove(ClientConnection, MoveRequest)` im `MessageRouter` implementieren. 
  - [ ] Zugehörigen Player finden. 
  - [ ] Position mit Geschwindigkeit * `deltaTime` aktualisieren und Begrenzungen berücksichtigen. 

</details>

**Akzeptanzkriterien:**
- [ ] Serverposition des Players ändert sich konsistent mit den Eingaben.
- [ ] Keine Exceptions bei schnellen oder häufigen Eingaben. 

## Ressourcen
- [Input-Beispiele Godot 4](https://docs.godotengine.org/en/4.4/tutorials/inputs/input_examples.html) – Input-Handling mit Tastatur (inkl. WASD & Arrow-Keys)
- [Delegates/Eventhandler in C#](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/delegates/) – Ereignisse für Movement-Handling
```

---

## Issue #14: Remote-Player-Instanzen im Godot-Client anzeigen

**Empfohlene Aktualisierung des Issue-Bodies:**

```markdown
Die vom Server gesendeten `PlayerStateUpdate`s im Client nutzen, um andere Spieler als Sprites in der Welt darzustellen. 

> **📌 Dies ist ein Epik-Issue.** Die Arbeit wurde in folgende Sub-Issues aufgeteilt:

**Sub-Issues:**
- [ ] #XX GameManager für Spieler-Verwaltung erstellen
- [ ] #XX PlayerNode-Szene und Sprite erstellen
- [ ] #XX Positions-Interpolation für Remote-Spieler (optional)

**Dieses Issue kann geschlossen werden, wenn alle Sub-Issues erledigt sind.**

---

<details>
<summary>Ursprüngliche Aufgaben (zur Referenz)</summary>

**Aufgaben:**
- [ ] `GameManager`-Script im Client:
  - [ ] Map von `PlayerId → PlayerNode` verwalten.
  - [ ] Neue Spieler instanziieren, wenn sie im Update auftauchen. 
  - [ ] Position vorhandener PlayerNodes aktualisieren. 
- [ ] Einfache Platzhalter-Sprites für Spieler verwenden.
- [ ] Optional: Interpolation zwischen alten und neuen Positionen.

</details>

**Akzeptanzkriterien:**
- [ ] Zwei Clients sehen jeweils die Avatare der anderen Spieler. 
- [ ] Positionsänderungen sind sichtbar und nachvollziehbar. 

## Ressourcen
- [Godot C#/.NET Scripting](https://docs.godotengine.org/en/stable/tutorials/scripting/c_sharp/index.html) – Grundlagen und Best Practices
- [Godot Input und Szene-Updates](https://docs.godotengine.org/en/stable/tutorials/inputs/input_examples.html) – Tipps zur Verarbeitung von Updates in Szenen
```

---

## Issue #15: Movement-Prototyp stabilisieren und einfache Tests

**Empfohlene Aktualisierung des Issue-Bodies:**

```markdown
Movement-Prototyp verfeinern, Basis-Tests hinzufügen und das Zusammenspiel von Server und Client kurz harttesten.

> **📌 Dies ist ein Epik-Issue.** Die Arbeit wurde in folgende Sub-Issues aufgeteilt:

**Sub-Issues:**
- [ ] #XX Tickrate und Geschwindigkeit tunen
- [ ] #XX Unit-Tests für World und Movement
- [ ] #XX Manuelle Integrationstests dokumentieren

**Dieses Issue kann geschlossen werden, wenn alle Sub-Issues erledigt sind.**

---

<details>
<summary>Ursprüngliche Aufgaben (zur Referenz)</summary>

**Aufgaben:**
- [ ] Geschwindigkeit und Tickraten so anpassen, dass sich Bewegung flüssig anfühlt.
- [ ] Einfache Unit-Tests:
  - [ ] `World.CreatePlayer` und `RemovePlayerByConnection`. 
  - [ ] Movement-Berechnung (inklusive Umgang mit 0 als Grenzwert).
- [ ] Manuelle Tests mit mehreren Clients:
  - [ ] Ein-/Ausloggen. 
  - [ ] Dauerhaftes Movement.
  - [ ] Verhalten bei Server-Stop.

</details>

**Akzeptanzkriterien:**
- [ ] Der Prototyp läuft mehrere Minuten stabil mit 2–3 Clients. 
- [ ] Movement wirkt ausreichend responsiv für einen ersten Online-Prototyp.

## Ressourcen
- [Unit Testing mit xUnit](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-csharp-with-xunit) – Tests in .NET/C#-Projekten
- [Best Practices für Unit-Tests](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices) – hilfreiche Testmuster und -tipps
```

---

## Hinweise zur Umsetzung

1. **Sub-Issues erstellen:** Für jedes Epik-Issue die entsprechenden Sub-Issues mit den Details aus `ISSUES_ROADMAP.md` erstellen.

2. **Issue-Nummern eintragen:** Nach dem Erstellen der Sub-Issues die Platzhalter `#XX` durch die tatsächlichen Issue-Nummern ersetzen.

3. **Labels setzen:** 
   - Epik-Issues: `status:epic` Label hinzufügen
   - Sub-Issues: entsprechende `area:*` und `priority:*` Labels

4. **Reihenfolge:** Die Sub-Issues in der empfohlenen Reihenfolge bearbeiten (siehe `ISSUE_HIERARCHY.md`)
