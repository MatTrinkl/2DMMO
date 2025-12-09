# Zone-Konzept in Issues integrieren

> **Hinweis:** Dieses Dokument beschreibt das Zone-Konzept aus einer älteren Phase. Für die aktuelle ID-System-Architektur siehe [ID-System Dokumentation](architecture/ID_SYSTEM.md), die `ushort ZoneId` (statt `string`) und das vollständige EntityIdentity-System verwendet.

## Hintergrund

Die `ARCHITECTURE.md` dokumentiert eine **Zone-basierte Server-Architektur** mit:
- Zone Server Layer (Startzone, Hauptstadt, Wald, etc.)
- Shards pro Zone für Skalierung
- Redis für Cross-Zone Events und Zone-Registry
- Gateway Layer für Connection Management

Die aktuellen Issues beschreiben jedoch eine **monolithische World-Klasse** ohne Zone-Konzept. Dieses Dokument beschreibt die notwendigen Anpassungen, um die Issues an die Zone-basierte Architektur anzupassen.

---

## 1. Issue #7 aktualisieren ("World, Player und IEntity Interface implementieren")

Die `World`-Klasse soll zu einem `ZoneManager` werden, der mehrere `Zone`-Instanzen verwaltet.

### Neue Struktur

```csharp
public class Zone
{
    public string ZoneId { get; }
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
    private readonly Dictionary<string, Zone> _zones;
    
    public Zone GetZone(string zoneId);
    public Zone GetZoneForPosition(float x, float y);
    public void TransferEntity(IEntity entity, string fromZoneId, string toZoneId);
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
    string ZoneId { get; set; }  // NEU: Zone-Zugehörigkeit
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
- [ ] `ZoneManager` Klasse mit Dictionary<string, Zone> erstellen
- [ ] `ZoneManager.GetZone(string zoneId)` - Holt Zone nach ID
- [ ] `ZoneManager.GetZoneForPosition(float x, float y)` - Findet Zone für Position
- [ ] `ZoneManager.TransferEntity(...)` - Transferiert Entity zwischen Zonen
- [ ] `ZoneManager.Update(float deltaTime)` - Updated alle Zonen

**IEntity mit Zone-Zugehörigkeit:**
- [ ] `IEntity` bekommt `string ZoneId` Property
- [ ] `Player` Klasse mit Zone-Zugehörigkeit aktualisieren

**Akzeptanzkriterien:**
- [ ] Mehrere Zonen können parallel existieren
- [ ] Entities sind einer Zone zugeordnet
- [ ] Zone-Transfer ist vorbereitet (nicht implementiert)
- [ ] Alle bestehenden Tests funktionieren weiterhin

---

## 2. Issue #74 aktualisieren ("WorldState Broadcast an alle Clients")

Der WorldState muss Zone-spezifisch werden.

### Aktualisiertes DTO

```csharp
[MessagePackObject]
public class ZoneState : INetworkMessage
{
    [Key(0)]
    public MessageType Type => MessageType.ZoneState;
    
    [Key(1)]
    public string ZoneId { get; set; }  // NEU: Zone-ID
    
    [Key(2)]
    public PlayerPositionData[] Players { get; set; }
    
    [Key(3)]
    public long ServerTick { get; set; }
    
    [Key(4)]
    public long ServerTimestamp { get; set; }
}
```

### Aktualisierte Aufgaben für Issue #74

**ZoneState DTO:**
- [ ] `ZoneId` Property zu ZoneState hinzufügen (`[Key(1)]`)
- [ ] Keys für andere Properties anpassen (Players = Key(2), etc.)
- [ ] Serialisierung mit MessagePack testen

**Zone-spezifischer Broadcast:**
- [ ] Nur Spieler der gleichen Zone in den State inkludieren
- [ ] Broadcast nur an Clients in der gleichen Zone senden
- [ ] Zone-ID im Broadcast-Loop verwenden

**Client-Handling:**
- [ ] Client empfängt nur Updates seiner eigenen Zone
- [ ] Bei Zone-Wechsel: Alte Zone-Daten verwerfen

**Akzeptanzkriterien:**
- [ ] Spieler erhalten nur Updates ihrer Zone
- [ ] ZoneId wird korrekt serialisiert/deserialisiert
- [ ] Keine Zone-übergreifenden Updates

---

## 3. Issue #75 aktualisieren ("Spawn-System mit Startposition")

Spawn-Points müssen Zone-spezifisch sein.

### Neue Strukturen

```csharp
public class SpawnPoint
{
    public string ZoneId { get; }
    public float X { get; }
    public float Y { get; }
    public float Radius { get; }  // Streuung
    public bool IsDefault { get; }  // Standard-Spawn für neue Spieler
}

public class SpawnManager
{
    private readonly Dictionary<string, List<SpawnPoint>> _spawnPointsByZone;
    
    public Vector2 GetSpawnPosition(string zoneId);
    public SpawnPoint GetDefaultSpawn();  // Für neue Spieler (Startzone)
    public void RegisterSpawnPoint(SpawnPoint spawnPoint);
}
```

### Aktualisierte Aufgaben für Issue #75

**SpawnPoint Klasse:**
- [ ] `SpawnPoint` Klasse mit ZoneId, X, Y, Radius, IsDefault erstellen
- [ ] `SpawnPoint` ist immutable (nur Konstruktor-Initialisierung)

**SpawnManager:**
- [ ] `SpawnManager` Klasse erstellen
- [ ] `GetSpawnPosition(string zoneId)` - Position mit Streuung in Zone
- [ ] `GetDefaultSpawn()` - Standard-Spawn für neue Spieler (Startzone)
- [ ] `RegisterSpawnPoint(SpawnPoint)` - Spawn-Punkt registrieren

**Zone-Integration:**
- [ ] Jede Zone hat eigene SpawnPoints (in Zone-Config)
- [ ] Startzone für neue Spieler konfigurierbar
- [ ] Collision-Check mit Zone-Bounds

**Akzeptanzkriterien:**
- [ ] Neue Spieler spawnen in der Startzone
- [ ] Spawn-Position ist innerhalb der Zone-Grenzen
- [ ] Mehrere Spawn-Punkte pro Zone möglich

---

## 4. Issue #76 aktualisieren ("Weltgrenzen und Position-Clamping")

Bounds müssen Zone-spezifisch sein.

### Neue Struktur

```csharp
public class ZoneBounds
{
    public string ZoneId { get; }
    public float MinX { get; }
    public float MaxX { get; }
    public float MinY { get; }
    public float MaxY { get; }
    
    public bool Contains(float x, float y);
    public Vector2 Clamp(Vector2 position);
    public bool IsNearEdge(float x, float y, float threshold);  // Für Zone-Übergang
}
```

### Aktualisierte Aufgaben für Issue #76

**ZoneBounds Klasse (ersetzt WorldBounds):**
- [ ] `ZoneBounds` Klasse mit ZoneId, MinX, MaxX, MinY, MaxY erstellen
- [ ] `ZoneBounds.Contains(float x, float y)` - Prüft ob Position in Zone
- [ ] `ZoneBounds.Clamp(Vector2 position)` - Begrenzt Position auf Zone
- [ ] `ZoneBounds.IsNearEdge(...)` - Für spätere Zone-Übergänge

**Zone-Integration:**
- [ ] Jede Zone hat eigene ZoneBounds
- [ ] ZoneBounds wird bei Zone-Erstellung gesetzt
- [ ] Position-Clamping pro Zone

**MovementValidator Anpassung:**
- [ ] MovementValidator prüft Zone-spezifische Bounds
- [ ] Bei Überschreitung: Position auf Zone-Grenze clampen
- [ ] Später: Zone-Übergang bei Grenzüberschreitung

**Akzeptanzkriterien:**
- [ ] Spieler können nicht außerhalb ihrer Zone laufen
- [ ] Jede Zone hat unabhängige Grenzen
- [ ] Position-Clamping funktioniert pro Zone

---

## 5. Neue Sub-Issues für Zone-Konzept

Die folgenden Sub-Issues sollten als Teil des Zone-Konzepts erstellt werden:

### Sub-Issue A: "Zone-Klasse implementieren"

**Labels:** `type:feature`, `area:server`, `priority:p1`

**Beschreibung:**
Basis-Klasse für eine Spielzone implementieren.

**Aufgaben:**

```csharp
public class Zone
{
    public string ZoneId { get; }
    public string ZoneName { get; }
    public ZoneBounds Bounds { get; }
    public CollisionData CollisionData { get; }
    
    private readonly Dictionary<int, IEntity> _entities;
    private readonly List<SpawnPoint> _spawnPoints;
    
    public Zone(string zoneId, string zoneName, ZoneBounds bounds);
    public void Update(float deltaTime);
    public void AddEntity(IEntity entity);
    public void RemoveEntity(int entityId);
    public IEntity GetEntity(int entityId);
    public IEnumerable<Player> GetPlayers();
    public IEnumerable<IEntity> GetEntitiesInRadius(float x, float y, float radius);
}
```

- [ ] Zone-Klasse mit grundlegenden Properties
- [ ] Entity-Verwaltung (Add, Remove, Get)
- [ ] Update-Loop für alle Entities
- [ ] Spatial Query für Entities in Radius
- [ ] Unit-Tests für Zone-Klasse

**Akzeptanzkriterien:**
- [ ] Zone verwaltet Entities korrekt
- [ ] Update ruft Entity.Update() auf
- [ ] GetPlayers filtert nur Spieler

---

### Sub-Issue B: "ZoneManager implementieren"

**Labels:** `type:feature`, `area:server`, `priority:p1`

**Beschreibung:**
Manager-Klasse für mehrere Zonen implementieren.

**Aufgaben:**

```csharp
public class ZoneManager
{
    private readonly Dictionary<string, Zone> _zones;
    private readonly string _defaultZoneId;
    
    public ZoneManager(string defaultZoneId);
    public void RegisterZone(Zone zone);
    public void UnregisterZone(string zoneId);
    public Zone GetZone(string zoneId);
    public Zone GetDefaultZone();
    public Zone GetZoneForPosition(float x, float y);
    public void Update(float deltaTime);
    
    // Später: Zone-Transfer
    public void TransferEntity(IEntity entity, string fromZoneId, string toZoneId);
}
```

- [ ] ZoneManager-Klasse erstellen
- [ ] Zone-Registrierung (Register/Unregister)
- [ ] Zone-Lookup (GetZone, GetDefaultZone)
- [ ] Update-Loop für alle Zonen
- [ ] Transfer-Methode als Stub vorbereiten
- [ ] Unit-Tests für ZoneManager

**Akzeptanzkriterien:**
- [ ] Mehrere Zonen können registriert werden
- [ ] Default-Zone ist konfigurierbar
- [ ] Update updated alle Zonen

---

### Sub-Issue C: "Zone-Konfiguration laden"

**Labels:** `type:feature`, `area:server`, `priority:p2`

**Beschreibung:**
Zonen aus Konfigurationsdateien laden.

**Aufgaben:**

```csharp
public class ZoneConfig
{
    public string ZoneId { get; set; }
    public string ZoneName { get; set; }
    public ZoneBoundsConfig Bounds { get; set; }
    public SpawnPointConfig[] SpawnPoints { get; set; }
    public bool IsDefault { get; set; }
}

public class ZoneLoader
{
    public ZoneConfig LoadZoneConfig(string configPath);
    public Zone CreateZoneFromConfig(ZoneConfig config);
}
```

- [ ] ZoneConfig DTO für JSON-Konfiguration
- [ ] ZoneLoader zum Laden der Konfiguration
- [ ] Factory-Methode für Zone-Erstellung
- [ ] Startzone-Konfiguration (`zones/startzone.json`)

**Akzeptanzkriterien:**
- [ ] Zonen werden aus JSON geladen
- [ ] Startzone ist konfiguriert
- [ ] Fehlerhafte Config wird abgefangen

---

### Sub-Issue D: "Player Zone-Zugehörigkeit"

**Labels:** `type:feature`, `area:server`, `priority:p1`

**Beschreibung:**
Spieler haben eine Zone-Zugehörigkeit.

**Aufgaben:**

```csharp
public class Player : IEntity
{
    // Bestehende Properties...
    
    public string ZoneId { get; set; }
    public string CurrentZoneName => _zoneManager.GetZone(ZoneId)?.ZoneName;
    
    public void ChangeZone(string newZoneId);
}
```

- [ ] ZoneId Property in Player
- [ ] Bei Login: Spieler in Startzone/gespeicherte Zone
- [ ] Zone-Wechsel vorbereiten (Methode)
- [ ] Unit-Tests für Zone-Zugehörigkeit

**Akzeptanzkriterien:**
- [ ] Spieler hat immer eine Zone
- [ ] Zone wird bei Login gesetzt
- [ ] Zone-Wechsel ist vorbereitet

---

### Sub-Issue E: "Zone-spezifischer Broadcast"

**Labels:** `type:feature`, `area:server`, `area:network`, `priority:p1`

**Beschreibung:**
State-Updates nur an Spieler in der gleichen Zone senden.

**Aufgaben:**

```csharp
public class ZoneBroadcaster
{
    public void BroadcastZoneState(Zone zone);
    
    private ZoneState BuildZoneState(Zone zone)
    {
        return new ZoneState
        {
            ZoneId = zone.ZoneId,
            Players = zone.GetPlayers()
                          .Select(p => new PlayerPositionData { ... })
                          .ToArray(),
            ServerTick = _gameLoop.CurrentTick,
            ServerTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        };
    }
}
```

- [ ] ZoneBroadcaster-Klasse erstellen
- [ ] State nur an Spieler der Zone senden
- [ ] Effizientes Broadcasting (parallel)
- [ ] Unit-Tests für Broadcaster

**Akzeptanzkriterien:**
- [ ] Spieler erhalten nur Updates ihrer Zone
- [ ] Keine Zone-übergreifenden Updates
- [ ] Performance ist akzeptabel

---

## Migrationsstrategie

Um eine schrittweise Migration zu ermöglichen:

1. **Phase 1:** ZoneBounds und SpawnPoint Zone-aware machen (Issues #75, #76)
2. **Phase 2:** Zone-Klasse implementieren (Sub-Issue A)
3. **Phase 3:** ZoneManager als Wrapper um World (Sub-Issue B)
4. **Phase 4:** World durch ZoneManager ersetzen (Issue #7)
5. **Phase 5:** Zone-spezifischen Broadcast aktivieren (Issue #74)

### Abwärtskompatibilität

Während der Migration:
- `World`-Klasse wird zu einer einzelnen "Default"-Zone
- Bestehende APIs bleiben zunächst erhalten
- Neue Zone-APIs werden parallel eingeführt

---

## Auswirkungen auf andere Issues

| Issue | Auswirkung |
|-------|-----------|
| #8 NetworkServer | Keine direkten Änderungen |
| #9 MessageRouter | Route muss Zone-ID berücksichtigen |
| #10 NetworkClient | Muss ZoneId in Messages verarbeiten |
| #12 MoveRequest | MovementValidator prüft Zone-Bounds |
| #13 PlayerStateUpdate | Wird zu ZoneState |
| #14 Remote-Player | Client filtert nach ZoneId |

---

## Ressourcen

### Architektur-Dokumentation
- [ARCHITECTURE.md](ARCHITECTURE.md) - Zone Server Layer
- [SERVER_COMPONENTS.md](architecture/SERVER_COMPONENTS.md) - Zone Server Details
- [REDIS.md](architecture/REDIS.md) - Zone-Registry

### Externe Ressourcen
- [Zone-Based MMO Architecture](https://www.gamedeveloper.com/programming/zone-based-world-management-in-mmos)
- [Spatial Partitioning](https://gameprogrammingpatterns.com/spatial-partition.html)

---

*Dieses Dokument beschreibt die notwendigen Änderungen zur Integration des Zone-Konzepts. Bei Fragen oder Ergänzungen bitte ein Issue erstellen.*
