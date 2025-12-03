# Copilot Code Review Instructions

## Über dieses Dokument

Diese Anweisungen gelten für Copilot bei Code Reviews in Pull Requests. 
Copilot soll den Code gegen unsere Projekt-Dokumentation prüfen und Abweichungen melden.

**Relevante Dokumentation:**
- `docs/ARCHITECTURE.md` - Technische Architektur
- `docs/TECHNICAL_DESIGN.md` - Detaillierte technische Entscheidungen
- `docs/PROTOTYPE_SCOPE.md` - Scope des Prototyps
- `docs/GAME_DESIGN_DOCUMENT.md` - Gameplay-Design

---

## 🔗 Pull Request & Issue Linking

### Issue-Referenz Prüfung

Bei JEDEM Pull Request Review:

1. **Prüfe ob Issues verlinkt sind:**
   - Suche nach `Closes #`, `Fixes #`, `Resolves #` in PR-Beschreibung
   - Suche nach Issue-Referenzen in Commit Messages
   - Wenn KEINE Issue-Referenz gefunden:
   
   > ❓ **Fehlende Issue-Referenz**
   > 
   > Dieser PR referenziert kein Issue. Bitte gib an, welche Issues dieser PR schließt:
   > - Füge `Closes #XX` zur PR-Beschreibung hinzu
   > - Oder erkläre, warum kein Issue existiert
   > 
   > **Offene Issues die relevant sein könnten:**
   > _(Liste hier passende offene Issues basierend auf dem PR-Inhalt)_

2. **Prüfe Issue-Vollständigkeit:**
   - Hole das verlinkte Issue
   - Prüfe ALLE Aufgaben (Checkboxen `- [ ]`) im Issue
   - Prüfe ALLE Akzeptanzkriterien im Issue
   - Vergleiche mit dem Code im PR

3. **Bei fehlenden Implementierungen:**

   > ⚠️ **Unvollständige Implementierung**
   > 
   > Das Issue #XX hat folgende Aufgaben/Akzeptanzkriterien die im PR nicht implementiert sind:
   > 
   > **Fehlende Aufgaben:**
   > - [ ] Aufgabe 1 aus dem Issue
   > - [ ] Aufgabe 2 aus dem Issue
   > 
   > **Fehlende Akzeptanzkriterien:**
   > - [ ] Kriterium 1
   > - [ ] Kriterium 2
   > 
   > **Optionen:**
   > 1. Implementiere die fehlenden Punkte in diesem PR
   > 2. Erstelle Follow-up Issues für die offenen Punkte
   > 3. Erkläre warum diese Punkte nicht mehr relevant sind

### Issue-Checkliste für Reviews

Bei jedem PR mit verlinktem Issue:

- [ ] PR-Beschreibung enthält `Closes #XX` oder ähnlich?
- [ ] Alle `- [ ]` Aufgaben im Issue sind im Code umgesetzt?
- [ ] Alle Akzeptanzkriterien im Issue sind erfüllt?
- [ ] Tests für die Akzeptanzkriterien vorhanden?
- [ ] Dokumentation aktualisiert (falls im Issue erwähnt)?

### Automatische Issue-Suche

Wenn ein PR kein Issue referenziert, suche nach passenden Issues basierend auf:
- Dateinamen im PR (z.B. `NetworkManager.cs` → Issues mit "NetworkManager")
- Ordnerstruktur (z.B. `server/` → Issues mit Label `area:server`)
- Commit Messages (Keywords)
- PR-Titel

Schlage dann passende Issues vor:

> 💡 **Mögliche zugehörige Issues:**
> - #42 - NetworkManager Autoload (Godot Singleton)
> - #38 - TCP Server mit TcpListener
> 
> Bitte verlinke das passende Issue mit `Closes #XX` in der PR-Beschreibung.

---

## 🏗️ Architektur-Konformität

### Server-Client-Trennung
- [ ] Code ist im richtigen Projekt (Server/Client/Shared)?
- [ ] Shared Library enthält NUR Messages, Enums, Constants, Interfaces
- [ ] Keine Server-Logik im Client und umgekehrt
- [ ] Game-Logik gehört auf den Server (authoritative)

### Komponenten-Struktur
- [ ] Gateway Server: Nur Connection-Handling, Auth, Routing
- [ ] Zone Server: Game Loop, Validation, Broadcasting
- [ ] Client: Rendering, Input, Prediction

### Kommunikation
- [ ] Client kommuniziert nur mit Gateway (nie direkt mit Zone Server)
- [ ] Cross-Zone Events über Redis Pub/Sub (nicht direkt)

---

## 📡 Netzwerk-Protokoll

### Message-Format
- [ ] Frame-Format eingehalten: `[1 Byte Type][4 Bytes Length][N Bytes Payload]`
- [ ] Length ist Little-Endian uint32
- [ ] Payload ist MessagePack-serialisiert

### Message-Types
- [ ] Neue Message-Types im `MessageType` Enum registriert?
- [ ] Enum-Werte in korrektem Bereich? (Connection: 1-9, Zone: 10-19, Movement: 20-29, etc.)
- [ ] Keine Duplikate oder Lücken ohne Grund

### DTOs (Data Transfer Objects)
- [ ] `[MessagePackObject]` Attribut vorhanden?
- [ ] Alle Properties haben `[Key(n)]` Attribute mit aufsteigenden Indizes?
- [ ] **WICHTIG:** `MessageType Type` Property hat `[Key(0)]` (NICHT IgnoreMember!)
- [ ] DTOs sind in der Shared Library?
- [ ] DTOs implementieren `INetworkMessage` Interface?

### MessagePackObject Validierung (KRITISCH)

Bei JEDER Klasse mit `[MessagePackObject]` Attribut prüfen:

1. **MessageType Property MUSS vorhanden sein:**
   ```csharp
   [Key(0)]
   public MessageType Type => MessageType.XXX;
   ```

2. **Type MUSS Key(0) sein:**
   - ❌ FALSCH: `[IgnoreMember]` auf Type
   - ❌ FALSCH: Type ohne `[Key(0)]`
   - ❌ FALSCH: Type ist nicht Key(0) sondern Key(1) oder höher
   - ✅ RICHTIG: `[Key(0)] public MessageType Type => MessageType.XXX;`

3. **Alle anderen Properties mit aufsteigenden Keys:**
   ```csharp
   [Key(1)] public string Username { get; set; }
   [Key(2)] public int PlayerId { get; set; }
   // usw.
   ```

4. **Warum nicht auf Interface/Basisklasse?**
   - MessagePack ignoriert `[Key]` Attribute auf Interfaces
   - Auch bei Vererbung muss `[Key(0)]` in jeder konkreten Klasse stehen
   - Das Interface `INetworkMessage` dient nur der Typsicherheit, nicht der Serialisierung

5. **Fehlermeldung bei Verstoß:**

   > ⚠️ **MessagePack Konvention verletzt**
   > 
   > Die Klasse `{ClassName}` hat `[MessagePackObject]` aber:
   > - ❌ Kein `MessageType Type` Property gefunden
   > - ODER: ❌ `Type` hat `[IgnoreMember]` statt `[Key(0)]`
   > - ODER: ❌ `Type` ist nicht `[Key(0)]`
   > 
   > **Korrektur:**
   > ```csharp
   > [MessagePackObject]
   > public class {ClassName} : INetworkMessage
   > {
   >     [Key(0)]
   >     public MessageType Type => MessageType.{TypeName};
   >     
   >     [Key(1)]
   >     public string Property1 { get; set; }
   >     // ...
   > }
   > ```

6. **Enum braucht KEIN MessagePackObject:**
   - `enum MessageType : byte` wird automatisch als byte serialisiert
   - ❌ FALSCH: `[MessagePackObject]` auf enum
   - ✅ RICHTIG: Nur `public enum MessageType : byte { ... }`

### Serialization
- [ ] MessagePack für alle Netzwerk-Nachrichten verwendet?
- [ ] Keine JSON/XML für Game-Traffic
- [ ] MessageSerializer Helper verwendet?

---

## 🔄 Game Loop & Timing

### Tick-Rate
- [ ] Server Tick-Rate ist 30 Hz (33.33ms pro Tick)
- [ ] Keine Annahmen über höhere/niedrigere Tick-Rate
- [ ] `TICK_RATE` Konstante aus Shared Library verwendet?

### Tick-Phasen
- [ ] Korrekte Reihenfolge: Input → Update → Output → Wait
- [ ] Keine I/O-Blocking-Operationen im Tick
- [ ] Tick-Overrun wird geloggt (> 33ms)

### Timing
- [ ] Fixed Timestep verwendet (nicht variable)
- [ ] Timestamps für Nachrichten sind Server-Zeit
- [ ] Sequence Numbers für Input-Ordering

---

## 🎮 Client-Side Prediction & Sync

### Prediction (eigener Spieler)
- [ ] Client bewegt sofort lokal (Prediction)
- [ ] Input wird mit Sequence Number an Server gesendet
- [ ] Pending Inputs werden gespeichert bis Server bestätigt

### Reconciliation
- [ ] Bei Server-Korrektur: Snap oder Interpolation zur Server-Position
- [ ] Pending Inputs nach Korrektur neu anwenden
- [ ] Keine harten Teleports (smooth correction)

### Interpolation (andere Spieler)
- [ ] Andere Spieler werden interpoliert (nicht predicted)
- [ ] Buffer von ~100ms für Jitter-Ausgleich
- [ ] Extrapolation nur kurzzeitig bei Packet-Loss

### Server Authority
- [ ] Server ist IMMER authoritative
- [ ] Client kann nichts erzwingen
- [ ] Alle Validierung auf dem Server

---

## 💥 Kollision

### Shared Collision Data
- [ ] Kollisionsdaten sind in Shared Library?
- [ ] Client und Server nutzen gleiche Daten
- [ ] `CollisionData` Klasse verwendet?

### Validation
- [ ] Server validiert alle Bewegungen
- [ ] Speed-Check (MAX_SPEED * deltaTime * TOLERANCE)
- [ ] Bounds-Check (innerhalb der Welt)
- [ ] Collision-Check (keine Wände/Hindernisse)

---

## 👾 Entity-System

### IEntity Interface
- [ ] Neue Entities implementieren `IEntity`?
- [ ] Properties: Id, Type, X, Y vorhanden?

### EntityType Enum
- [ ] Neuer Entity-Typ im Enum registriert?
- [ ] Korrekter Wertebereich? (Player: 1, NPCs: 10+, Interactive: 20+)

### Interaktionen
- [ ] InteractRequest/Response Pattern verwendet?
- [ ] Server validiert: Distanz, Entity existiert, Berechtigung

---

## 🔒 Sicherheit

### Input Validation
- [ ] ALLE Client-Inputs werden validiert
- [ ] PlayerId aus Session, nicht aus Message vertrauen
- [ ] Plausibilitäts-Checks (Damage, Speed, Position)

### Rate Limiting
- [ ] Aktionen sind rate-limited? (z.B. Chat: 10/min)
- [ ] Keine Möglichkeit für DoS durch Message-Spam

### Sensible Daten
- [ ] Keine Passwörter im Klartext
- [ ] Keine sensiblen Daten in Logs
- [ ] Keine Server-Secrets im Client-Code
- [ ] Session-Tokens sind UUIDs, nicht vorhersagbar

### Anti-Cheat
- [ ] Speed-Hack Detection
- [ ] Teleport Detection
- [ ] Action-Rate Limiting
- [ ] Damage-Plausibility Checks

---

## 🌐 Connection Handling

### Timeouts
- [ ] CONNECTION_TIMEOUT: 10s für Login
- [ ] HEARTBEAT_INTERVAL: 5s Client → Server
- [ ] HEARTBEAT_TIMEOUT: 15s → Disconnect
- [ ] RECONNECT_WINDOW: 30s State behalten

### Disconnect
- [ ] Graceful Disconnect handling
- [ ] PlayerLeft wird gebroadcastet
- [ ] Resources werden aufgeräumt (Dispose)

### Reconnect
- [ ] ReconnectToken System verwendet?
- [ ] Exponential Backoff für Retries
- [ ] Max 10 Versuche, dann aufgeben

### Error Codes
- [ ] ErrorCode Enum aus Dokumentation verwendet?
- [ ] Passender Error Code für jeden Fehlerfall
- [ ] Error Messages sind nicht zu detailliert (Security)

---

## 📝 Logging

### Log Levels
- [ ] TRACE: Nur für Debugging (jeden Tick, jede Message)
- [ ] DEBUG: State Changes, Flow
- [ ] INFO: Login, Disconnect, wichtige Events (Production Standard)
- [ ] WARNING: Timeouts, Retries, Anomalien
- [ ] ERROR: Fehler, Server läuft weiter
- [ ] FATAL: Kritisch, Server stoppt

### Was loggen?
- [ ] Strukturiertes Logging (Serilog)
- [ ] Keine sensiblen Daten (Passwörter, Tokens)
- [ ] Player-IDs und Session-IDs für Debugging
- [ ] Timestamps für alle Einträge

### Performance
- [ ] Keine String-Interpolation in Hot Paths wenn Log-Level aus
- [ ] Log-Level ist konfigurierbar

---

## ⚡ Performance

### Game Loop
- [ ] Keine Allokationen im Game Loop (Object Pooling)
- [ ] Tick-Budget von 33ms beachten
- [ ] Keine Blocking-I/O im Tick

### Async/Await
- [ ] async/await korrekt verwendet (kein .Result oder .Wait())
- [ ] ConfigureAwait(false) in Library-Code
- [ ] CancellationToken wird durchgereicht

### Memory
- [ ] Große Objekte werden gepoolt
- [ ] Keine Memory Leaks (Event Handler abmelden)
- [ ] Span<T> für Buffer-Operationen wo möglich

---

## 🎮 Godot Client (C#)

### Szenen-Struktur
- [ ] Szenen-Hierarchie aus TECHNICAL_DESIGN.md eingehalten?
- [ ] Main.tscn → Login.tscn → Game.tscn Flow
- [ ] Player.tscn als Prefab für Spieler

### Autoloads (Singletons)
- [ ] NetworkManager für alle Netzwerk-Operationen
- [ ] GameManager für Game State
- [ ] Keine anderen globalen Singletons ohne Grund

### Signale
- [ ] Godot Signale statt direkter Methodenaufrufe
- [ ] Events für UI-Updates
- [ ] Lose Kopplung zwischen Komponenten

### Layers
- [ ] Layer 0: Ground (keine Collision)
- [ ] Layer 1: Collision (Wasser, Bäume, Wände)
- [ ] Layer 2: Entities (NPCs, Interactives)
- [ ] Layer 3: Players
- [ ] Layer 4: UI (CanvasLayer)

### Input
- [ ] Input Actions definiert (nicht hardcoded Keys)
- [ ] _Process für Rendering, _PhysicsProcess für Bewegung
- [ ] Input wird an Server gesendet, nicht lokal verarbeitet (außer Prediction)

---

## 🗄️ Redis (Phase 2+)

### Key Schema
- [ ] Key-Naming aus Dokumentation eingehalten?
- [ ] `session:{sessionId}`, `player:{playerId}`, etc.
- [ ] TTL für Sessions (30 min)

### Pub/Sub
- [ ] Channels korrekt: `channel:zone:{zoneId}`, `channel:player:{playerId}`
- [ ] Keine großen Payloads über Pub/Sub

---

## 🗃️ Datenbank (Phase 2+)

### Write-Strategien
- [ ] IMMEDIATE: Charakter-Erstellung, Item-Transaktionen, Gold, Level-Up
- [ ] BATCHED (10-30s): Positionen, HP/Mana, Quest-Fortschritt
- [ ] LOGOUT: Kompletter State

### Connection Pooling
- [ ] Npgsql Connection Pooling aktiviert
- [ ] MinPoolSize: 5, MaxPoolSize: 100

---

## 🎯 Prototype Scope

### Im Scope
- [ ] Ist das Feature im Prototyp-Scope (`docs/PROTOTYPE_SCOPE.md`)?
- [ ] TCP Server/Client, MessagePack, Game Loop, Bewegung, Chat

### Außerhalb Scope
- [ ] Kampfsystem, NPCs, Quests, Inventar → Phase 2+
- [ ] Echte Auth, Datenbank, Redis, Azure → Phase 2+
- [ ] Wenn außerhalb: Ist es als "später" markiert?

---

## 📊 Tests

### Unit Tests
- [ ] Neue Logik hat Unit Tests?
- [ ] xUnit + Moq + FluentAssertions verwendet?
- [ ] Tests sind im richtigen Test-Projekt?

### Was testen?
- [ ] Message Serialization/Deserialization
- [ ] Validation Logic (Movement, Actions)
- [ ] Game Loop Logic (ohne I/O)

### Godot Tests
- [ ] GdUnit4 für Client-Tests
- [ ] NetworkManager Tests (mit Mocks)

---

## 📝 Feedback-Format

Bei Abweichungen von der Dokumentation:

1. **Zitiere** die relevante Stelle aus der Dokumentation
2. **Erkläre** die Abweichung klar und präzise
3. **Schlage** eine konkrete Korrektur vor
4. **Verweise** auf das relevante Dokument mit Pfad

### Beispiel:

> ⚠️ **Architektur-Abweichung**
> 
> Laut `docs/TECHNICAL_DESIGN.md` soll MessagePack für Serialisierung verwendet werden:
> > "Serialization: MessagePack"
> 
> Dieser Code verwendet jedoch JSON:
> ```csharp
> var json = JsonSerializer.Serialize(message);
> ```
> 
> **Vorschlag:** Verwende stattdessen:
> ```csharp
> var bytes = MessagePackSerializer.Serialize(message);
> ```

---

## 🔗 Relevante Dokumentation

- [ARCHITECTURE.md](docs/ARCHITECTURE.md) - Technische Architektur
- [TECHNICAL_DESIGN.md](docs/TECHNICAL_DESIGN.md) - Detaillierte technische Entscheidungen
- [PROTOTYPE_SCOPE.md](docs/PROTOTYPE_SCOPE.md) - Was ist im Prototyp
- [GAME_DESIGN_DOCUMENT.md](docs/GAME_DESIGN_DOCUMENT.md) - Gameplay Design
- [ASSETS.md](docs/ASSETS.md) - Asset-Spezifikationen

---

*Diese Anweisungen helfen dabei, die Code-Qualität und Architektur-Konsistenz zu gewährleisten.*