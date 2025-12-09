# Issue-Aktualisierungs-Leitfaden

**Version:** 2.0.0  
**Letzte Aktualisierung:** 2025-12-09  
**Status:** Konsolidiert

---

## 📋 Übersicht

Dieses Dokument zeigt, wie bestehende Issues aktualisiert werden sollten, um:
1. Auf Sub-Issues zu verweisen (bei großen Epik-Issues)
2. Das Zone-Konzept zu integrieren (Architektur-Update)

> **📌 Hinweis:** Für die aktuelle ID-System-Architektur siehe [ID-System Dokumentation](../02-architecture/ID_SYSTEM.md), die `ushort ZoneId` und das vollständige EntityIdentity-System verwendet.

---

## 🏗️ Zone-Konzept Hintergrund

Die Architektur dokumentiert eine **Zone-basierte Server-Architektur** mit:
- Zone Server Layer (Startzone, Hauptstadt, Wald, etc.)
- Shards pro Zone für Skalierung
- Redis für Cross-Zone Events und Zone-Registry
- Gateway Layer für Connection Management

Die aktuellen Issues beschrieben ursprünglich eine **monolithische World-Klasse** ohne Zone-Konzept. Dieses Dokument beschreibt die notwendigen Anpassungen.

---

## Issue #7: World, Player und IEntity Interface implementieren

**Status:** Epik-Issue → Zone-Konzept integriert

### Empfohlene Aktualisierung des Issue-Bodies:

```markdown
Basisklassen für die serverseitige Spiellogik mit Entity-System.

> **📌 Zone-Konzept Update:** Die `World`-Klasse wird zu einem `ZoneManager` erweitert.

> **📌 Dies ist ein Epik-Issue.** Die Arbeit wurde in folgende Sub-Issues aufgeteilt:

**Sub-Issues:**
- [ ] #XX Zone-Klasse implementieren
- [ ] #XX ZoneManager implementieren (ersetzt/erweitert World)
- [ ] #XX Zone-Konfiguration laden
- [ ] #XX Player Zone-Zugehörigkeit

**Dieses Issue kann geschlossen werden, wenn alle Sub-Issues erledigt sind.**

---

<details>
<summary>Ursprüngliche Aufgaben (zur Referenz)</summary>

**Aufgaben:**
- [ ] `Dictionary<int, IEntity> _entities`
- [ ] `Dictionary<Guid, Player> _playersBySession`
- [ ] `Player CreatePlayer(string name, ClientConnection connection)`
- [ ] `void RemovePlayer(Guid sessionId)`
- [ ] `void Update(float deltaTime)`
- [ ] `IEnumerable<Player> GetAllPlayers()`

</details>
```

### Neue Zone-basierte Struktur

```csharp
public class Zone
{
    public ushort ZoneId { get; }
    public string ZoneName { get; }
    public ZoneBounds Bounds { get; }
    public Dictionary<int, IEntity> Entities { get; }
    public List<SpawnPoint> SpawnPoints { get; }
    
    public void Update(float deltaTime);
    public void AddEntity(IEntity entity);
    public void RemoveEntity(int entityId);
    public IEnumerable<Player> GetPlayers();
}

public class ZoneManager  // ersetzt/erweitert World
{
    private readonly Dictionary<ushort, Zone> _zones;
    
    public Zone GetZone(ushort zoneId);
    public Zone GetZoneForPosition(float x, float y);
    public void TransferEntity(IEntity entity, ushort fromZoneId, ushort toZoneId);
    public void Update(float deltaTime);  // updated alle Zones
}
```

### Aktualisiertes IEntity Interface

```csharp
public interface IEntity
{
    int Id { get; }
    EntityType Type { get; }
    float X { get; set; }
    float Y { get; set; }
    ushort ZoneId { get; set; }  // NEU: Zone-Zugehörigkeit
}
```

### Aktualisierte Aufgaben für Issue #7

**Zone Klasse:**
- [ ] `Zone` Klasse mit ZoneId, ZoneName, Bounds, Entities erstellen
- [ ] `Zone.Update(float deltaTime)` - Updated alle Entities in der Zone
- [ ] `Zone.AddEntity(IEntity entity)` - Fügt Entity zur Zone hinzu
- [ ] `Zone.RemoveEntity(int entityId)` - Entfernt Entity aus der Zone
- [ ] `Zone.GetPlayers()` - Gibt alle Spieler der Zone zurück

**ZoneManager Klasse (ersetzt monolithische World):**
- [ ] `ZoneManager` Klasse mit Dictionary<ushort, Zone> erstellen
- [ ] `ZoneManager.GetZone(ushort zoneId)` - Holt Zone nach ID
- [ ] `ZoneManager.GetZoneForPosition(float x, float y)` - Findet Zone für Position
- [ ] `ZoneManager.TransferEntity(...)` - Transferiert Entity zwischen Zonen
- [ ] `ZoneManager.Update(float deltaTime)` - Updated alle Zonen

**IEntity mit Zone-Zugehörigkeit:**
- [ ] `IEntity` bekommt `ushort ZoneId` Property
- [ ] `Player` Klasse mit Zone-Zugehörigkeit aktualisieren

**Akzeptanzkriterien:**
- [ ] Mehrere Zonen können parallel existieren
- [ ] Entities sind einer Zone zugeordnet
- [ ] Zone-Transfer ist vorbereitet (nicht implementiert)
- [ ] Alle bestehenden Tests funktionieren weiterhin

---

## Issue #74: WorldState Broadcast an alle Clients

**Status:** Aktualisiert für Zone-Konzept

### Aktualisierung erforderlich

Der WorldState muss Zone-spezifisch werden → **ZoneState**

### Neues DTO

```csharp
[MessagePackObject]
public class ZoneState : INetworkMessage
{
    [Key(0)]
    public MessageType Type => MessageType.ZoneState;
    
    [Key(1)]
    public ushort ZoneId { get; set; }
    
    [Key(2)]
    public long Tick { get; set; }
    
    [Key(3)]
    public List<EntityData> Entities { get; set; }
}
```

### Empfohlene Aktualisierung:

```markdown
Zone-spezifischer State-Broadcast an alle Clients in der Zone (25 Hz).

> **📌 Zone-Konzept:** Statt globalem WorldState wird pro Zone ein ZoneState gebroadcastet.

**Aufgaben:**
- [ ] `ZoneState` DTO erstellen (MessagePack)
- [ ] Zone.BroadcastState() Methode implementieren
- [ ] Nur an Clients in der gleichen Zone senden
- [ ] Tick-Counter pro Zone

**Akzeptanzkriterien:**
- [ ] Clients erhalten nur State ihrer Zone
- [ ] Broadcast erfolgt mit 25 Hz
- [ ] State enthält ZoneId und Tick
```

---

## Issue #75: Client erhält WorldState und rendert andere Spieler

**Status:** Aktualisiert für Zone-Konzept

### Empfohlene Aktualisierung:

```markdown
Client empfängt ZoneState und rendert andere Spieler in der Zone.

> **📌 Zone-Konzept:** Client erhält ZoneState statt WorldState.

**Aufgaben:**
- [ ] ZoneState Message Handler im Client
- [ ] Nur Entities der eigenen Zone rendern
- [ ] Zone-Transfer: Altes State cleanup + Neues State laden

**Akzeptanzkriterien:**
- [ ] Client zeigt nur Spieler in gleicher Zone
- [ ] Bei Zone-Wechsel: Alte Entities werden entfernt
- [ ] Neue Zone lädt korrekt
```

---

## Issue #76: Client sendet eigene Bewegung, Server validiert

**Status:** Zone-aware

### Empfohlene Aktualisierung:

```markdown
Client sendet PositionUpdate, Server validiert Zone-spezifisch.

> **📌 Zone-Konzept:** Validation berücksichtigt Zone-Grenzen.

**Aufgaben:**
- [ ] PositionUpdate enthält ZoneId (implizit aus Session)
- [ ] Server validiert gegen Zone-Bounds
- [ ] Zone-Transfer bei Grenzüberschreitung erkennen
- [ ] CollisionData der Zone verwenden

**Akzeptanzkriterien:**
- [ ] Bewegung wird gegen Zone-Bounds validiert
- [ ] Zone-Transfer wird erkannt (für später)
- [ ] Kollision mit Zone-Hindernissen prüfen
```

---

## Issue #8: NetworkServer und ClientConnection-Skelett

**Status:** Kann in Sub-Issues aufgeteilt werden

### Empfohlene Sub-Issues:

#### Sub-Issue 8a: NetworkServer TCP-Listener implementieren
```markdown
TCP-Listener des NetworkServers implementieren.

**Aufgaben:**
- [ ] NetworkServer-Klasse anlegen
- [ ] RunAsync(CancellationToken) Methode
- [ ] TcpListener starten
- [ ] Accept-Loop implementieren
- [ ] Logging bei Start, Stop, Fehlern

**Akzeptanzkriterien:**
- [ ] Server startet auf konfiguriertem Port
- [ ] Sauberes Shutdown mit CancellationToken
- [ ] Fehler werden geloggt
```

#### Sub-Issue 8b: ClientConnection-Klasse implementieren
```markdown
ClientConnection-Klasse für einzelne Verbindung.

**Aufgaben:**
- [ ] ClientConnection-Klasse erstellen
- [ ] SendAsync(byte[] data) Methode
- [ ] ReceiveLoop mit NetworkStream
- [ ] Disconnect-Handling

**Akzeptanzkriterien:**
- [ ] Kann Nachrichten senden/empfangen
- [ ] Disconnect wird erkannt
- [ ] Ressourcen werden aufgeräumt
```

#### Sub-Issue 8c: ClientConnection-Manager
```markdown
Verwaltung aller aktiven Verbindungen.

**Aufgaben:**
- [ ] ConnectionManager-Klasse
- [ ] Add/Remove Connections
- [ ] Broadcast-Methode
- [ ] Cleanup bei Disconnect

**Akzeptanzkriterien:**
- [ ] Alle Connections werden verwaltet
- [ ] Broadcast an alle/gefilterte Clients
- [ ] Memory Leaks vermieden
```

---

## Issue #9: Message Framing implementieren

**Status:** Klein genug, keine Aufteilung nötig

Bleibt als einzelnes Issue.

---

## Issue #10: Login-Flow implementieren

**Status:** Kann in Sub-Issues aufgeteilt werden

### Empfohlene Sub-Issues:

#### Sub-Issue 10a: LoginRequest/Response Messages
```markdown
DTOs für Login-Flow definieren.

**Aufgaben:**
- [ ] LoginRequest DTO (Username)
- [ ] LoginResponse DTO (Success, PlayerId, Token)
- [ ] MessageType Enum erweitern

**Akzeptanzkriterien:**
- [ ] Messages sind MessagePack-serialisierbar
- [ ] Alle nötigen Felder vorhanden
```

#### Sub-Issue 10b: Server Login-Handler
```markdown
Login-Verarbeitung auf dem Server.

**Aufgaben:**
- [ ] HandleLogin() Methode
- [ ] Session erstellen
- [ ] Player-Instanz erstellen
- [ ] LoginResponse senden

**Akzeptanzkriterien:**
- [ ] Validierung (Username nicht leer)
- [ ] Session-Token generieren
- [ ] Player wird erstellt
```

#### Sub-Issue 10c: Client Login-UI
```markdown
Login-Screen im Godot Client.

**Aufgaben:**
- [ ] Login-Scene erstellen
- [ ] Username-Eingabe
- [ ] Login-Button
- [ ] LoginRequest senden

**Akzeptanzkriterien:**
- [ ] UI ist bedienbar
- [ ] Sendet LoginRequest
- [ ] Wechselt zu Game-Scene bei Erfolg
```

---

## 📊 Zusammenfassung der Änderungen

### Zone-Konzept Updates
- **Issue #7**: World → ZoneManager + Zone-Klasse
- **Issue #74**: WorldState → ZoneState
- **Issue #75**: Client rendert Zone-spezifisch
- **Issue #76**: Zone-aware Validation

### Sub-Issue Aufspaltungen
- **Issue #8**: → 8a (Listener), 8b (Connection), 8c (Manager)
- **Issue #10**: → 10a (DTOs), 10b (Server), 10c (Client)

---

## 🔗 Verwandte Dokumentation

- [ID-System](../02-architecture/ID_SYSTEM.md) - EntityIdentity, ZoneId Ranges
- [Architektur-Übersicht](../02-architecture/README.md) - Zone Server Layer
- [Server-Komponenten](../02-architecture/SERVER_COMPONENTS.md) - Zone Server Details
- [Redis-Strategie](../02-architecture/REDIS.md) - Zone-Registry
- [Issue-Hierarchie](ISSUE_HIERARCHY.md) - Issue-Beziehungen
- [Issues Roadmap](ISSUES_ROADMAP.md) - Feature-Roadmap

---

## 📝 Verwendung

1. **Für bestehende Issues**: Nutze die "Empfohlene Aktualisierung" Abschnitte
2. **Für neue Sub-Issues**: Nutze die vorbereiteten Templates
3. **Zone-Konzept**: Berücksichtige `ushort ZoneId` statt `string`
4. **Referenziere dieses Dokument** in Issue-Updates

---

**Navigation**: [← Zurück zur Projekt-Management-Übersicht](README.md)
