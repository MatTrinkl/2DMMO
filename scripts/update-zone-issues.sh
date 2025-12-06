#!/bin/bash
# =============================================================================
# Zone-Konzept Issues Update Script
# =============================================================================
# Dieses Script aktualisiert bestehende Issues und erstellt neue Sub-Issues
# für das Zone-Konzept basierend auf docs/ZONE_CONCEPT_UPDATES.md
#
# Voraussetzungen:
# - GitHub CLI (gh) installiert und authentifiziert
# - Ausführung im Repository-Root-Verzeichnis
#
# Verwendung:
#   chmod +x scripts/update-zone-issues.sh
#   ./scripts/update-zone-issues.sh
# =============================================================================

set -e

# Farben für Output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Repository-Informationen
REPO="MatTrinkl/2DMMO"

echo -e "${BLUE}==============================================================================${NC}"
echo -e "${BLUE}Zone-Konzept Issues Update Script${NC}"
echo -e "${BLUE}==============================================================================${NC}"
echo ""

# Prüfen ob gh installiert ist
if ! command -v gh &> /dev/null; then
    echo -e "${RED}FEHLER: GitHub CLI (gh) ist nicht installiert.${NC}"
    echo "Installation: https://cli.github.com/"
    exit 1
fi

# Prüfen ob gh authentifiziert ist
if ! gh auth status &> /dev/null; then
    echo -e "${RED}FEHLER: GitHub CLI ist nicht authentifiziert.${NC}"
    echo "Führe 'gh auth login' aus, um dich anzumelden."
    exit 1
fi

echo -e "${GREEN}✓ GitHub CLI ist installiert und authentifiziert${NC}"
echo ""

# =============================================================================
# Funktion: Issue aktualisieren
# =============================================================================
update_issue() {
    local issue_number=$1
    local body_file=$2
    local title=$3
    
    echo -e "${YELLOW}Aktualisiere Issue #${issue_number}...${NC}"
    
    if [ -n "$title" ]; then
        gh issue edit "$issue_number" --repo "$REPO" --body-file "$body_file" --title "$title"
    else
        gh issue edit "$issue_number" --repo "$REPO" --body-file "$body_file"
    fi
    
    echo -e "${GREEN}✓ Issue #${issue_number} aktualisiert${NC}"
}

# =============================================================================
# Funktion: Neues Issue erstellen
# =============================================================================
create_issue() {
    local title=$1
    local body_file=$2
    local labels=$3
    
    echo -e "${YELLOW}Erstelle Issue: ${title}...${NC}"
    
    local issue_url
    issue_url=$(gh issue create --repo "$REPO" --title "$title" --body-file "$body_file" --label "$labels")
    
    echo -e "${GREEN}✓ Issue erstellt: ${issue_url}${NC}"
    echo "$issue_url"
}

# =============================================================================
# Temporäre Dateien für Issue-Bodies erstellen
# =============================================================================
TEMP_DIR=$(mktemp -d)
trap "rm -rf $TEMP_DIR" EXIT

echo -e "${BLUE}Erstelle temporäre Issue-Body-Dateien...${NC}"
echo ""

# -----------------------------------------------------------------------------
# Issue #7: World, Player und IEntity Interface implementieren (Zone-Update)
# -----------------------------------------------------------------------------
cat > "$TEMP_DIR/issue-7.md" << 'EOF'
Basisklassen für die serverseitige Spiellogik mit Entity-System.

> ⚠️ **Zone-Konzept Update:** Die `World`-Klasse wird zu einem `ZoneManager` erweitert.
> Siehe [ZONE_CONCEPT_UPDATES.md](docs/ZONE_CONCEPT_UPDATES.md) für Details.

> **📌 Dies ist ein Epik-Issue.** Die Arbeit wurde in folgende Sub-Issues aufgeteilt:

**Sub-Issues:**
- [ ] Zone-Klasse implementieren
- [ ] ZoneManager implementieren (ersetzt/erweitert World)
- [ ] Zone-Konfiguration laden
- [ ] Player Zone-Zugehörigkeit

**Dieses Issue kann geschlossen werden, wenn alle Sub-Issues erledigt sind.**

---

<details>
<summary>Ursprüngliche Aufgaben (zur Referenz)</summary>

### IEntity Interface
```csharp
public interface IEntity
{
    int Id { get; }
    EntityType Type { get; }
    float X { get; set; }
    float Y { get; set; }
}
```

### EntityType Enum
```csharp
public enum EntityType : byte
{
    Player = 1,
    NpcVendor = 10,
    NpcQuestGiver = 11,
    Chest = 20,
    Door = 21,
    Sign = 22,
    Portal = 23,
}
```

### World Klasse
- [ ] `Dictionary<int, IEntity> _entities`
- [ ] `Dictionary<Guid, Player> _playersBySession`
- [ ] `Player CreatePlayer(string name, ClientConnection connection)`
- [ ] `void RemovePlayer(Guid sessionId)`
- [ ] `void Update(float deltaTime)`
- [ ] `IEnumerable<Player> GetAllPlayers()`

### Player Klasse (implementiert IEntity)
- [ ] `int Id`, `EntityType Type`, `Guid SessionId`
- [ ] `string Name`, `ClientConnection Connection`
- [ ] `float X, Y, VelocityX, VelocityY`
- [ ] `int LastProcessedSequence`
- [ ] `void Update(float deltaTime)`

</details>

---

## Aktualisierte Aufgaben (Zone-Konzept)

### Zone Klasse
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
```

- [ ] `Zone` Klasse mit ZoneId, ZoneName, Bounds, Entities erstellen
- [ ] `Zone.Update(float deltaTime)` - Updated alle Entities in der Zone
- [ ] `Zone.AddEntity(IEntity entity)` - Fügt Entity zur Zone hinzu
- [ ] `Zone.RemoveEntity(int entityId)` - Entfernt Entity aus der Zone
- [ ] `Zone.GetPlayers()` - Gibt alle Spieler der Zone zurück

### ZoneManager Klasse (ersetzt/erweitert World)
```csharp
public class ZoneManager
{
    private readonly Dictionary<string, Zone> _zones;
    
    public Zone GetZone(string zoneId);
    public Zone GetDefaultZone();
    public Zone GetZoneForPosition(float x, float y);
    public void TransferEntity(IEntity entity, string fromZoneId, string toZoneId);
    public void Update(float deltaTime);
}
```

- [ ] `ZoneManager` Klasse mit Dictionary<string, Zone>
- [ ] `ZoneManager.GetZone(string zoneId)` - Holt Zone nach ID
- [ ] `ZoneManager.GetDefaultZone()` - Gibt Startzone zurück
- [ ] `ZoneManager.Update(float deltaTime)` - Updated alle Zonen
- [ ] Zone-Transfer-Methode vorbereiten (Stub)

### IEntity mit Zone-Zugehörigkeit
```csharp
public interface IEntity
{
    int Id { get; }
    EntityType Type { get; }
    float X { get; set; }
    float Y { get; set; }
    string ZoneId { get; set; }  // NEU
}
```

- [ ] `IEntity` bekommt `string ZoneId` Property
- [ ] `Player` Klasse mit Zone-Zugehörigkeit aktualisieren

## Akzeptanzkriterien
- [ ] Spieler können erstellt und entfernt werden
- [ ] Spieler sind einer Zone zugeordnet
- [ ] `ZoneManager.Update()` wird pro Tick aufgerufen
- [ ] Alle Entities implementieren IEntity mit ZoneId

## 🔗 Ressourcen

- [Zone-Konzept Updates](docs/ZONE_CONCEPT_UPDATES.md) - Detaillierte Beschreibung
- [ARCHITECTURE.md](docs/ARCHITECTURE.md) - Zone Server Layer
- [Interfaces in C#](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/types/interfaces)
EOF

# -----------------------------------------------------------------------------
# Issue #74: WorldState Broadcast an alle Clients (Zone-Update)
# -----------------------------------------------------------------------------
cat > "$TEMP_DIR/issue-74.md" << 'EOF'
Server broadcastet jeden Tick den aktuellen ZoneState an alle Clients in der Zone.

> ⚠️ **Zone-Konzept Update:** WorldState wird zu ZoneState.
> Siehe [ZONE_CONCEPT_UPDATES.md](docs/ZONE_CONCEPT_UPDATES.md) für Details.

## Prototyp-Strategie
- Jeden Tick kompletten ZoneState senden (pro Zone)
- Später: Delta-Updates + Full-Sync alle 30-60 Ticks

## ZoneState DTO (ersetzt WorldState)
```csharp
[MessagePackObject]
public class ZoneState : INetworkMessage
{
    [Key(0)]
    public MessageType Type => MessageType.ZoneState;
    
    [Key(1)]
    public string ZoneId { get; set; }  // NEU
    
    [Key(2)]
    public PlayerPositionData[] Players { get; set; }
    
    [Key(3)]
    public long ServerTick { get; set; }
    
    [Key(4)]
    public long ServerTimestamp { get; set; }
}
```

## Aufgaben

### ZoneState DTO
- [ ] `ZoneId` Property zu ZoneState hinzufügen (`[Key(1)]`)
- [ ] Keys für andere Properties anpassen (Players = Key(2), etc.)
- [ ] Serialisierung mit MessagePack testen

### Zone-spezifischer Broadcast
- [ ] ZoneState am Ende jedes Ticks zusammenstellen
- [ ] Nur Spieler der gleichen Zone in Players Array
- [ ] An alle verbundenen Clients **der Zone** senden
- [ ] Disconnecting Clients überspringen

### Client-Handling
- [ ] Client empfängt nur Updates seiner eigenen Zone
- [ ] Bei Zone-Wechsel: Alte Zone-Daten verwerfen

## Akzeptanzkriterien
- [ ] ZoneId wird im State mitgeschickt
- [ ] Clients erhalten 30x pro Sekunde Updates ihrer Zone
- [ ] Spieler sehen nur andere Spieler in ihrer Zone
- [ ] Neue Spieler sind sofort sichtbar (in ihrer Zone)

## 🔗 Ressourcen

- [Zone-Konzept Updates](docs/ZONE_CONCEPT_UPDATES.md) - ZoneState Details
- [Snapshot Interpolation](https://gafferongames.com/post/snapshot_interpolation/)
- [State Synchronization](https://www.gabrielgambetta.com/client-server-game-architecture.html)
EOF

# -----------------------------------------------------------------------------
# Issue #75: Spawn-System mit Startposition (Zone-Update)
# -----------------------------------------------------------------------------
cat > "$TEMP_DIR/issue-75.md" << 'EOF'
Startposition für neue Spieler in einer Zone definieren.

> ⚠️ **Zone-Konzept Update:** SpawnPoints sind Zone-spezifisch.
> Siehe [ZONE_CONCEPT_UPDATES.md](docs/ZONE_CONCEPT_UPDATES.md) für Details.

## SpawnPoint Klasse (Zone-spezifisch)
```csharp
public class SpawnPoint
{
    public string ZoneId { get; }
    public float X { get; }
    public float Y { get; }
    public float Radius { get; }  // Streuung
    public bool IsDefault { get; }  // Standard-Spawn für neue Spieler
}
```

## SpawnManager
```csharp
public class SpawnManager
{
    private readonly Dictionary<string, List<SpawnPoint>> _spawnPointsByZone;
    
    public Vector2 GetSpawnPosition(string zoneId);
    public SpawnPoint GetDefaultSpawn();  // Für neue Spieler (Startzone)
    public void RegisterSpawnPoint(SpawnPoint spawnPoint);
}
```

## Aufgaben

### SpawnPoint Klasse
- [ ] `SpawnPoint` Klasse mit ZoneId, X, Y, Radius, IsDefault erstellen
- [ ] `SpawnPoint` ist immutable (nur Konstruktor-Initialisierung)

### SpawnManager
- [ ] `SpawnManager` Klasse erstellen
- [ ] `GetSpawnPosition(string zoneId)` - Position mit Streuung in Zone
- [ ] `GetDefaultSpawn()` - Standard-Spawn für neue Spieler (Startzone)
- [ ] `RegisterSpawnPoint(SpawnPoint)` - Spawn-Punkt registrieren

### Zone-Integration
- [ ] Jede Zone hat eigene SpawnPoints (in Zone-Config)
- [ ] Startzone für neue Spieler konfigurierbar
- [ ] Bei LoginRequest → Spawn-Position in Startzone berechnen
- [ ] In LoginResponse mit ZoneId mitschicken
- [ ] Collision-Check (nicht in Wand spawnen)

## Akzeptanzkriterien
- [ ] Neue Spieler spawnen in der Startzone
- [ ] Jede Zone hat eigene SpawnPoints
- [ ] Spawn-Position ist innerhalb der Zone-Grenzen
- [ ] Keine Spieler in Wänden

## 🔗 Ressourcen

- [Zone-Konzept Updates](docs/ZONE_CONCEPT_UPDATES.md) - SpawnPoint Details
- [Random in Circle](https://stackoverflow.com/questions/5837572/generate-a-random-point-within-a-circle-uniformly)
EOF

# -----------------------------------------------------------------------------
# Issue #76: Weltgrenzen und Position-Clamping (Zone-Update)
# -----------------------------------------------------------------------------
cat > "$TEMP_DIR/issue-76.md" << 'EOF'
Spieler können nicht außerhalb ihrer Zone laufen.

> ⚠️ **Zone-Konzept Update:** WorldBounds wird zu ZoneBounds.
> Siehe [ZONE_CONCEPT_UPDATES.md](docs/ZONE_CONCEPT_UPDATES.md) für Details.

## ZoneBounds Klasse (ersetzt WorldBounds)
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

## Aufgaben

### ZoneBounds Klasse
- [ ] `ZoneBounds` Klasse mit ZoneId, MinX, MaxX, MinY, MaxY erstellen
- [ ] `Contains(float x, float y)` - Prüft ob Position in Zone
- [ ] `Clamp(Vector2 position)` - Begrenzt Position auf Zone
- [ ] `IsNearEdge(...)` - Für spätere Zone-Übergänge vorbereiten

### Zone-Integration
- [ ] Jede Zone hat eigene ZoneBounds
- [ ] ZoneBounds wird bei Zone-Erstellung gesetzt
- [ ] Position-Clamping pro Zone

### MovementValidator Anpassung
- [ ] MovementValidator prüft Zone-spezifische Bounds
- [ ] Bei Überschreitung: Position auf Zone-Grenze clampen
- [ ] Später: Zone-Übergang bei Grenzüberschreitung

## Akzeptanzkriterien
- [ ] Spieler stoppt am Zone-Rand
- [ ] Jede Zone hat unabhängige Grenzen
- [ ] Kein Teleport außerhalb der Zone möglich

## 🔗 Ressourcen

- [Zone-Konzept Updates](docs/ZONE_CONCEPT_UPDATES.md) - ZoneBounds Details
- [Math.Clamp](https://learn.microsoft.com/en-us/dotnet/api/system.math.clamp)
EOF

# -----------------------------------------------------------------------------
# Sub-Issue A: Zone-Klasse implementieren
# -----------------------------------------------------------------------------
cat > "$TEMP_DIR/sub-issue-zone-class.md" << 'EOF'
Basis-Klasse für eine Spielzone implementieren.

> **Teil von:** Issue #7 (World, Player und IEntity Interface implementieren)

## Zone Klasse

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

## Aufgaben

- [ ] Zone-Klasse mit grundlegenden Properties erstellen
- [ ] Konstruktor mit ZoneId, ZoneName, ZoneBounds
- [ ] Entity-Verwaltung implementieren:
  - [ ] `AddEntity(IEntity entity)` - Entity hinzufügen, ZoneId setzen
  - [ ] `RemoveEntity(int entityId)` - Entity entfernen
  - [ ] `GetEntity(int entityId)` - Entity nach ID holen
- [ ] `Update(float deltaTime)` - Alle Entities updaten
- [ ] `GetPlayers()` - Nur Player-Entities zurückgeben
- [ ] `GetEntitiesInRadius(x, y, radius)` - Spatial Query
- [ ] Unit-Tests für Zone-Klasse

## Akzeptanzkriterien

- [ ] Zone verwaltet Entities korrekt (Add/Remove/Get)
- [ ] Update ruft Entity.Update() für alle Entities auf
- [ ] GetPlayers filtert korrekt nur Spieler
- [ ] Entity.ZoneId wird beim Hinzufügen gesetzt
- [ ] Unit-Tests vorhanden und grün

## 🔗 Ressourcen

- [Zone-Konzept Updates](docs/ZONE_CONCEPT_UPDATES.md)
- [Dictionary<TKey, TValue>](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.dictionary-2)
EOF

# -----------------------------------------------------------------------------
# Sub-Issue B: ZoneManager implementieren
# -----------------------------------------------------------------------------
cat > "$TEMP_DIR/sub-issue-zone-manager.md" << 'EOF'
Manager-Klasse für mehrere Zonen implementieren.

> **Teil von:** Issue #7 (World, Player und IEntity Interface implementieren)

## ZoneManager Klasse

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

## Aufgaben

- [ ] ZoneManager-Klasse erstellen
- [ ] Konstruktor mit defaultZoneId
- [ ] Zone-Registrierung implementieren:
  - [ ] `RegisterZone(Zone zone)` - Zone hinzufügen
  - [ ] `UnregisterZone(string zoneId)` - Zone entfernen
- [ ] Zone-Lookup implementieren:
  - [ ] `GetZone(string zoneId)` - Zone nach ID
  - [ ] `GetDefaultZone()` - Startzone zurückgeben
  - [ ] `GetZoneForPosition(x, y)` - Zone für Position (wenn Zones räumlich angeordnet)
- [ ] `Update(float deltaTime)` - Alle Zonen updaten
- [ ] `TransferEntity(...)` als Stub vorbereiten (später implementieren)
- [ ] Unit-Tests für ZoneManager

## Akzeptanzkriterien

- [ ] Mehrere Zonen können registriert werden
- [ ] Default-Zone ist konfigurierbar und abrufbar
- [ ] Update() ruft Zone.Update() für alle Zonen auf
- [ ] TransferEntity-Stub existiert (throws NotImplementedException)
- [ ] Unit-Tests vorhanden und grün

## 🔗 Ressourcen

- [Zone-Konzept Updates](docs/ZONE_CONCEPT_UPDATES.md)
- [Manager Pattern](https://gameprogrammingpatterns.com/singleton.html)
EOF

# -----------------------------------------------------------------------------
# Sub-Issue C: Zone-Konfiguration laden
# -----------------------------------------------------------------------------
cat > "$TEMP_DIR/sub-issue-zone-config.md" << 'EOF'
Zonen aus Konfigurationsdateien laden.

> **Teil von:** Issue #7 (World, Player und IEntity Interface implementieren)

## ZoneConfig DTO

```csharp
public class ZoneConfig
{
    public string ZoneId { get; set; }
    public string ZoneName { get; set; }
    public ZoneBoundsConfig Bounds { get; set; }
    public SpawnPointConfig[] SpawnPoints { get; set; }
    public bool IsDefault { get; set; }
}

public class ZoneBoundsConfig
{
    public float MinX { get; set; }
    public float MaxX { get; set; }
    public float MinY { get; set; }
    public float MaxY { get; set; }
}

public class SpawnPointConfig
{
    public float X { get; set; }
    public float Y { get; set; }
    public float Radius { get; set; }
    public bool IsDefault { get; set; }
}
```

## ZoneLoader

```csharp
public class ZoneLoader
{
    public ZoneConfig LoadZoneConfig(string configPath);
    public Zone CreateZoneFromConfig(ZoneConfig config);
    public IEnumerable<Zone> LoadAllZones(string zonesDirectory);
}
```

## Aufgaben

- [ ] `ZoneConfig`, `ZoneBoundsConfig`, `SpawnPointConfig` DTOs erstellen
- [ ] `ZoneLoader` Klasse erstellen
- [ ] `LoadZoneConfig(string path)` - JSON-Datei laden und deserialisieren
- [ ] `CreateZoneFromConfig(ZoneConfig)` - Zone aus Config erstellen
- [ ] `LoadAllZones(string dir)` - Alle Zonen aus Verzeichnis laden
- [ ] Startzone-Konfiguration erstellen: `config/zones/startzone.json`
- [ ] Fehlerbehandlung für ungültige/fehlende Configs
- [ ] Unit-Tests für ZoneLoader

## Beispiel-Konfiguration

```json
{
  "zoneId": "startzone",
  "zoneName": "Startzone",
  "bounds": {
    "minX": 0,
    "maxX": 1600,
    "minY": 0,
    "maxY": 1600
  },
  "spawnPoints": [
    {
      "x": 800,
      "y": 800,
      "radius": 50,
      "isDefault": true
    }
  ],
  "isDefault": true
}
```

## Akzeptanzkriterien

- [ ] Zonen werden aus JSON geladen
- [ ] Startzone-Config existiert und wird korrekt geladen
- [ ] Fehlerhafte Config wirft aussagekräftige Exception
- [ ] Unit-Tests vorhanden und grün

## 🔗 Ressourcen

- [Zone-Konzept Updates](docs/ZONE_CONCEPT_UPDATES.md)
- [System.Text.Json](https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/overview)
EOF

# -----------------------------------------------------------------------------
# Sub-Issue D: Player Zone-Zugehörigkeit
# -----------------------------------------------------------------------------
cat > "$TEMP_DIR/sub-issue-player-zone.md" << 'EOF'
Spieler haben eine Zone-Zugehörigkeit.

> **Teil von:** Issue #7 (World, Player und IEntity Interface implementieren)

## Player mit Zone-Zugehörigkeit

```csharp
public class Player : IEntity
{
    // Bestehende Properties...
    public int Id { get; }
    public EntityType Type => EntityType.Player;
    public float X { get; set; }
    public float Y { get; set; }
    
    // NEU: Zone-Zugehörigkeit
    public string ZoneId { get; set; }
    
    public void ChangeZone(string newZoneId);
}
```

## IEntity Interface Update

```csharp
public interface IEntity
{
    int Id { get; }
    EntityType Type { get; }
    float X { get; set; }
    float Y { get; set; }
    string ZoneId { get; set; }  // NEU
}
```

## Aufgaben

- [ ] `IEntity` Interface um `ZoneId` Property erweitern
- [ ] `Player` Klasse: `ZoneId` Property implementieren
- [ ] `Player.ChangeZone(string newZoneId)` Methode als Stub
- [ ] Login-Flow anpassen:
  - [ ] Bei Login: Spieler in Startzone (oder gespeicherte Zone) setzen
  - [ ] `ZoneId` in `LoginResponse` mitschicken
- [ ] Bestehende Entities auf IEntity.ZoneId prüfen
- [ ] Unit-Tests für Zone-Zugehörigkeit

## Akzeptanzkriterien

- [ ] Alle IEntity-Implementierungen haben ZoneId
- [ ] Spieler hat nach Login eine gültige ZoneId
- [ ] ZoneId wird im LoginResponse mitgesendet
- [ ] ChangeZone-Stub existiert
- [ ] Unit-Tests vorhanden und grün

## 🔗 Ressourcen

- [Zone-Konzept Updates](docs/ZONE_CONCEPT_UPDATES.md)
- [Interface Design](https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/interface)
EOF

# -----------------------------------------------------------------------------
# Sub-Issue E: Zone-spezifischer Broadcast
# -----------------------------------------------------------------------------
cat > "$TEMP_DIR/sub-issue-zone-broadcast.md" << 'EOF'
State-Updates nur an Spieler in der gleichen Zone senden.

> **Teil von:** Issue #74 (WorldState Broadcast an alle Clients)

## ZoneBroadcaster

```csharp
public class ZoneBroadcaster
{
    private readonly ZoneManager _zoneManager;
    
    public ZoneBroadcaster(ZoneManager zoneManager);
    
    public void BroadcastZoneState(Zone zone);
    public void BroadcastAllZones();
    
    private ZoneState BuildZoneState(Zone zone)
    {
        return new ZoneState
        {
            ZoneId = zone.ZoneId,
            Players = zone.GetPlayers()
                          .Select(p => new PlayerPositionData 
                          { 
                              PlayerId = p.Id,
                              X = p.X,
                              Y = p.Y,
                              // ...
                          })
                          .ToArray(),
            ServerTick = _gameLoop.CurrentTick,
            ServerTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        };
    }
}
```

## Aufgaben

- [ ] `ZoneBroadcaster` Klasse erstellen
- [ ] `BuildZoneState(Zone zone)` - ZoneState für eine Zone bauen
- [ ] `BroadcastZoneState(Zone zone)` - State an alle Spieler der Zone senden
- [ ] `BroadcastAllZones()` - Alle Zonen broadcasten (für Game Loop)
- [ ] Paralleles Broadcasting für Performance
- [ ] Disconnecting Clients überspringen
- [ ] Unit-Tests für ZoneBroadcaster

## Integration in Game Loop

```csharp
// In GameLoop.Tick()
_zoneBroadcaster.BroadcastAllZones();
```

## Akzeptanzkriterien

- [ ] Spieler erhalten nur Updates ihrer eigenen Zone
- [ ] Keine Zone-übergreifenden Updates
- [ ] Broadcasting ist effizient (parallel wenn möglich)
- [ ] Disconnected Clients werden übersprungen
- [ ] Unit-Tests vorhanden und grün

## 🔗 Ressourcen

- [Zone-Konzept Updates](docs/ZONE_CONCEPT_UPDATES.md)
- [Task.WhenAll](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task.whenall)
EOF

echo -e "${GREEN}✓ Temporäre Issue-Body-Dateien erstellt${NC}"
echo ""

# =============================================================================
# Hauptmenü
# =============================================================================
echo -e "${BLUE}Was möchtest du tun?${NC}"
echo ""
echo "1) Alle Issues aktualisieren und erstellen (empfohlen)"
echo "2) Nur bestehende Issues aktualisieren (#7, #74, #75, #76)"
echo "3) Nur neue Sub-Issues erstellen"
echo "4) Einzelnes Issue aktualisieren"
echo "5) Einzelnes Sub-Issue erstellen"
echo "6) Trockenlauf (zeigt was passieren würde)"
echo "0) Abbrechen"
echo ""
read -p "Auswahl (0-6): " choice

case $choice in
    1)
        echo ""
        echo -e "${BLUE}=== Aktualisiere bestehende Issues ===${NC}"
        echo ""
        
        update_issue 7 "$TEMP_DIR/issue-7.md" "World, Player und IEntity Interface implementieren"
        update_issue 74 "$TEMP_DIR/issue-74.md" "WorldState Broadcast an alle Clients (30 Hz)"
        update_issue 75 "$TEMP_DIR/issue-75.md" "Spawn-System mit Startposition"
        update_issue 76 "$TEMP_DIR/issue-76.md" "Weltgrenzen und Position-Clamping"
        
        echo ""
        echo -e "${BLUE}=== Erstelle neue Sub-Issues ===${NC}"
        echo ""
        
        create_issue "Zone-Klasse implementieren" "$TEMP_DIR/sub-issue-zone-class.md" "type:feature,area:server,priority:p1"
        create_issue "ZoneManager implementieren" "$TEMP_DIR/sub-issue-zone-manager.md" "type:feature,area:server,priority:p1"
        create_issue "Zone-Konfiguration laden" "$TEMP_DIR/sub-issue-zone-config.md" "type:feature,area:server,priority:p2"
        create_issue "Player Zone-Zugehörigkeit" "$TEMP_DIR/sub-issue-player-zone.md" "type:feature,area:server,priority:p1"
        create_issue "Zone-spezifischer Broadcast" "$TEMP_DIR/sub-issue-zone-broadcast.md" "type:feature,area:server,area:network,priority:p1"
        
        echo ""
        echo -e "${GREEN}=== Fertig! ===${NC}"
        ;;
        
    2)
        echo ""
        echo -e "${BLUE}=== Aktualisiere bestehende Issues ===${NC}"
        echo ""
        
        update_issue 7 "$TEMP_DIR/issue-7.md" "World, Player und IEntity Interface implementieren"
        update_issue 74 "$TEMP_DIR/issue-74.md" "WorldState Broadcast an alle Clients (30 Hz)"
        update_issue 75 "$TEMP_DIR/issue-75.md" "Spawn-System mit Startposition"
        update_issue 76 "$TEMP_DIR/issue-76.md" "Weltgrenzen und Position-Clamping"
        
        echo ""
        echo -e "${GREEN}=== Fertig! ===${NC}"
        ;;
        
    3)
        echo ""
        echo -e "${BLUE}=== Erstelle neue Sub-Issues ===${NC}"
        echo ""
        
        create_issue "Zone-Klasse implementieren" "$TEMP_DIR/sub-issue-zone-class.md" "type:feature,area:server,priority:p1"
        create_issue "ZoneManager implementieren" "$TEMP_DIR/sub-issue-zone-manager.md" "type:feature,area:server,priority:p1"
        create_issue "Zone-Konfiguration laden" "$TEMP_DIR/sub-issue-zone-config.md" "type:feature,area:server,priority:p2"
        create_issue "Player Zone-Zugehörigkeit" "$TEMP_DIR/sub-issue-player-zone.md" "type:feature,area:server,priority:p1"
        create_issue "Zone-spezifischer Broadcast" "$TEMP_DIR/sub-issue-zone-broadcast.md" "type:feature,area:server,area:network,priority:p1"
        
        echo ""
        echo -e "${GREEN}=== Fertig! ===${NC}"
        ;;
        
    4)
        echo ""
        echo "Welches Issue aktualisieren?"
        echo "7)  Issue #7 - World, Player und IEntity Interface"
        echo "74) Issue #74 - WorldState Broadcast"
        echo "75) Issue #75 - Spawn-System"
        echo "76) Issue #76 - Weltgrenzen"
        echo ""
        read -p "Issue-Nummer: " issue_num
        
        case $issue_num in
            7)  update_issue 7 "$TEMP_DIR/issue-7.md" "World, Player und IEntity Interface implementieren" ;;
            74) update_issue 74 "$TEMP_DIR/issue-74.md" "WorldState Broadcast an alle Clients (30 Hz)" ;;
            75) update_issue 75 "$TEMP_DIR/issue-75.md" "Spawn-System mit Startposition" ;;
            76) update_issue 76 "$TEMP_DIR/issue-76.md" "Weltgrenzen und Position-Clamping" ;;
            *)  echo -e "${RED}Ungültige Auswahl${NC}" ;;
        esac
        ;;
        
    5)
        echo ""
        echo "Welches Sub-Issue erstellen?"
        echo "a) Zone-Klasse implementieren"
        echo "b) ZoneManager implementieren"
        echo "c) Zone-Konfiguration laden"
        echo "d) Player Zone-Zugehörigkeit"
        echo "e) Zone-spezifischer Broadcast"
        echo ""
        read -p "Auswahl (a-e): " sub_choice
        
        case $sub_choice in
            a) create_issue "Zone-Klasse implementieren" "$TEMP_DIR/sub-issue-zone-class.md" "type:feature,area:server,priority:p1" ;;
            b) create_issue "ZoneManager implementieren" "$TEMP_DIR/sub-issue-zone-manager.md" "type:feature,area:server,priority:p1" ;;
            c) create_issue "Zone-Konfiguration laden" "$TEMP_DIR/sub-issue-zone-config.md" "type:feature,area:server,priority:p2" ;;
            d) create_issue "Player Zone-Zugehörigkeit" "$TEMP_DIR/sub-issue-player-zone.md" "type:feature,area:server,priority:p1" ;;
            e) create_issue "Zone-spezifischer Broadcast" "$TEMP_DIR/sub-issue-zone-broadcast.md" "type:feature,area:server,area:network,priority:p1" ;;
            *) echo -e "${RED}Ungültige Auswahl${NC}" ;;
        esac
        ;;
        
    6)
        echo ""
        echo -e "${YELLOW}=== TROCKENLAUF ===${NC}"
        echo ""
        echo "Folgende Aktionen würden durchgeführt:"
        echo ""
        echo -e "${BLUE}Bestehende Issues aktualisieren:${NC}"
        echo "  - Issue #7: World, Player und IEntity Interface implementieren"
        echo "  - Issue #74: WorldState Broadcast an alle Clients (30 Hz)"
        echo "  - Issue #75: Spawn-System mit Startposition"
        echo "  - Issue #76: Weltgrenzen und Position-Clamping"
        echo ""
        echo -e "${BLUE}Neue Sub-Issues erstellen:${NC}"
        echo "  - Zone-Klasse implementieren (Labels: type:feature, area:server, priority:p1)"
        echo "  - ZoneManager implementieren (Labels: type:feature, area:server, priority:p1)"
        echo "  - Zone-Konfiguration laden (Labels: type:feature, area:server, priority:p2)"
        echo "  - Player Zone-Zugehörigkeit (Labels: type:feature, area:server, priority:p1)"
        echo "  - Zone-spezifischer Broadcast (Labels: type:feature, area:server, area:network, priority:p1)"
        echo ""
        echo -e "${YELLOW}Dies war ein Trockenlauf. Keine Änderungen wurden vorgenommen.${NC}"
        ;;
        
    0)
        echo -e "${YELLOW}Abgebrochen.${NC}"
        exit 0
        ;;
        
    *)
        echo -e "${RED}Ungültige Auswahl${NC}"
        exit 1
        ;;
esac

echo ""
echo -e "${BLUE}==============================================================================${NC}"
echo -e "${GREEN}Script beendet.${NC}"
echo -e "${BLUE}==============================================================================${NC}"
