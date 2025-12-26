# 🗺️ Zone Events Messages (0100-0199)

**Kategorie:** 1  
**Range:** 0100-0199  
**Phase:** Prototyp  
**Status:** 🟢 In Entwicklung

[← Zurück zur Übersicht](README.md)

---

## 📋 Inhaltsverzeichnis

- [JoinZone (100)](#joinzone-100)
- [LeaveZone (101)](#leavezone-101)
- [ZoneState (102)](#zonestate-102)
- [PlayerJoinedZone (103)](#playerjoinedzone-103)
- [PlayerLeftZone (104)](#playerleftzone-104)
- [ZoneTransferRequest (105)](#zonetransferrequest-105)
- [ZoneTransferResponse (106)](#zonetransferresponse-106)
- [ZoneLoadingProgress (107)](#zoneloadingprogress-107)
- [ZoneDiscovered (108)](#zonediscovered-108)
- [ZoneListRequest (109)](#zonelistrequest-109)
- [ZoneListResponse (110)](#zonelistresponse-110)
- [ShardTransfer (111)](#shardtransfer-111)
- [ShardListRequest (112)](#shardlistrequest-112)
- [ShardListResponse (113)](#shardlistresponse-113)
- [SubZoneEnter (114)](#subzoneenter-114)
- [SubZoneLeave (115)](#subzoneleave-115)
- [ZonePhaseChange (116)](#zonephasechange-116)
- [GetZoneRequest (117)](#getzonerequest-117)
- [GetZoneResponse (118)](#getzoneresponse-118)

---

## JoinZone (100)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Server informiert Client dass Character in eine Zone gespawnt wird. Enthält Zone-ID, Spawn-Position und initiale Informationen. 

**Flow:** Nach `CharacterSelectResponse` sendet Client zuerst `GetZoneRequest` (117) um Zone-Metadaten zu laden und Assets vorzubereiten. Wenn bereit, sendet Server dann `JoinZone` mit vollständigem PlayerEntity.

### Im Scope ✅
- Zone-ID und Name
- Spawn-Position (X, Y, Z)
- Zone-Type (Outdoor, Dungeon, City, etc.)
- **Vollständiges PlayerEntity mit allen gameplay-relevanten Daten**
- **Equipment-Snapshot für korrektes Character-Rendering**
- **Aktive Buffs/Debuffs für UI-Anzeige**
- **Cooldowns für Ability-Verfügbarkeit**

### Nicht im Scope ❌
- Vollständiger ZoneState mit anderen Spielern → verwende `ZoneState` (102)
- Entity-Liste → Server sendet separate `EntitySpawn` Messages (1400)
- **Vollständiges Inventory** → verwende `InventorySync` (1100)
- **Quest-Log** → verwende `QuestSync` (1000)
- **Skill-Tree Details** → verwende `SkillSync` (800)

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| ZoneId | int | Eindeutige Zone-ID | Ja |
| ZoneName | string | Name der Zone | Ja |
| ZoneType | string | "outdoor", "dungeon", "city", "instance" | Ja |
| SpawnX | float | X-Koordinate | Ja |
| SpawnY | float | Y-Koordinate | Ja |
| SpawnZ | float | Z-Koordinate (Höhe) | Ja |
| Player | PlayerEntityDto | Vollständiges PlayerEntity für sofortiges Gameplay | Ja |

**PlayerEntityDto**:
| Feld | Typ | Beschreibung |
|------|-----|--------------|
| EntityId | int | Runtime Entity-ID in der Zone |
| PersistentId | Guid | Persistente Character-ID aus DB |
| Name | string | Character-Name |
| CurrentHealth | int | Aktuelle HP |
| MaxHealth | int | Maximale HP |
| CurrentMana | int | Aktuelles Mana |
| MaxMana | int | Maximales Mana |
| Level | int | Character-Level |
| Experience | long | Aktuelle XP |
| ExperienceToNextLevel | long | XP für nächstes Level |
| Race | byte | Rassen-ID |
| Class | byte | Klassen-ID |
| AppearanceData | byte[] | Serialisierte Appearance-Daten |
| Equipment | EquipmentSnapshotDto | Aktuell getragene Ausrüstung (für Rendering) |
| ActiveEffects | List<ActiveEffectDto> | Aktive Buffs/Debuffs |
| Gold | int | Aktuelles Gold |
| PremiumCurrency | int | Premium-Währung |
| IsPvpFlagged | bool | PvP-Flag aktiv? |
| IsInCombat | bool | Im Kampf? |
| IsResting | bool | Ruht der Spieler? |
| Cooldowns | Dictionary<int, long> | Aktive Ability-Cooldowns (AbilityId → ExpiryTimestamp) |

**EquipmentSnapshotDto**:
| Feld | Typ | Beschreibung |
|------|-----|--------------|
| HeadItemId | int? | Helm Item-ID |
| ChestItemId | int? | Brustpanzer Item-ID |
| LegsItemId | int? | Beinrüstung Item-ID |
| FeetItemId | int? | Schuhe Item-ID |
| MainHandItemId | int? | Haupthand-Waffe Item-ID |
| OffHandItemId | int? | Nebenhand Item-ID |
| BackItemId | int? | Umhang/Rücken Item-ID |

**ActiveEffectDto**:
| Feld | Typ | Beschreibung |
|------|-----|--------------|
| EffectId | int | Effekt-ID |
| EffectName | string | Name des Effekts |
| Stacks | int | Anzahl Stacks |
| RemainingDuration | float | Verbleibende Dauer (Sekunden) |
| IsDebuff | bool | Ist es ein negativer Effekt? |

### Erwartete Response
- Client sendet `PositionUpdate` (200) um Spawn zu bestätigen
- Danach: Client empfängt `ZoneState` (102) mit anderen Entities

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `ZoneState` | 102 | Enthält kompletten Zone-State |
| `LeaveZone` | 101 | Verlassen der Zone |
| `CharacterSelect` | 9 | Auslöser für JoinZone |
| `GetZoneRequest` | 117 | Wird vor JoinZone gesendet (Zone-Metadaten laden) |
| `EntitySpawn` | 1400 | Andere Spieler/NPCs in Zone |
| `InventorySync` | 1100 | Vollständiges Inventory (separat) |
| `QuestSync` | 1000 | Quest-Log (separat) |
| `SkillSync` | 800 | Skill-Tree (separat) |
| `FriendListResponse` | 2105 | Freundesliste (separat) |

### Flow-Diagramm
```
Client                    Zone Server
  │                          │
  │  CharacterSelect (9)     │
  │─────────────────────────►│
  │                          │  Load Character
  │                          │  Find Spawn Point
  │                          │
  │  CharacterSelectResponse │
  │  (SpawnZoneId: 1001)     │
  │◄─────────────────────────│
  │                          │
  │  GetZoneRequest (117)    │
  │  (ZoneId: 1001)          │
  │─────────────────────────►│
  │                          │
  │  GetZoneResponse (118)   │
  │  with ZoneState          │
  │◄─────────────────────────│
  │                          │
  │  Load Zone Assets        │
  │  (Textures, Models...)   │
  │                          │
  │  Assets Loaded,          │
  │  Ready to Spawn          │
  │                          │
  │  JoinZone (100)          │
  │◄─────────────────────────│
  │                          │
  │  PositionUpdate (200)    │
  │─────────────────────────►│
  │                          │
  │  EntitySpawn (1400) x N  │
  │◄─────────────────────────│
```

### Beispiel Payload
```csharp
var joinZone = new JoinZone
{
    Type = MessageType.JoinZone,
    ZoneId = 1001,
    ZoneName = "Elwynn Forest",
    ZoneType = "outdoor",
    SpawnX = 100.5f,
    SpawnY = 250.0f,
    SpawnZ = 10.0f,
    Player = new PlayerEntityDto
    {
        EntityId = 50001,
        PersistentId = Guid.Parse("a3f7c2b1-4d5e-6f7a-8b9c-0d1e2f3a4b5c"),
        Name = "Aragorn",
        CurrentHealth = 850,
        MaxHealth = 1000,
        CurrentMana = 200,
        MaxMana = 300,
        Level = 10,
        Experience = 45000,
        ExperienceToNextLevel = 55000,
        Race = 1, // Human
        Class = 2, // Warrior
        AppearanceData = new byte[] { /* ... */ },
        Equipment = new EquipmentSnapshotDto
        {
            HeadItemId = 1001,
            ChestItemId = 2005,
            MainHandItemId = 5001
        },
        ActiveEffects = new List<ActiveEffectDto>
        {
            new ActiveEffectDto 
            { 
                EffectId = 101, 
                EffectName = "Well Rested", 
                RemainingDuration = 3600,
                IsDebuff = false 
            }
        },
        Gold = 1250,
        IsPvpFlagged = false,
        IsInCombat = false,
        IsResting = true,
        Cooldowns = new Dictionary<int, long>()
    }
};
```

### Notizen
- Loading-Screen im Client während Zone-Load
- Client lädt Zone-Assets basierend auf ZoneId (nach `GetZoneRequest`)
- Nach JoinZone: Server sendet andere Spieler als `EntitySpawn` (1400)
- Spawn-Position ist entweder: Last-Position, Hearthstone, oder Zone-Default
- **Empfohlener Flow:** `CharacterSelectResponse` → `GetZoneRequest` → Asset Loading → `JoinZone`
- **PlayerEntityDto enthält alle Daten für sofortiges Gameplay** - Client muss keine weiteren Requests für Basis-Daten senden
- **Nicht enthalten in PlayerEntityDto** (separate Messages):
  - Vollständiges Inventory → `InventorySync` (1100)
  - Quest-Log → `QuestSync` (1000)
  - Skill-Tree → `SkillSync` (800)
  - Freundesliste → `FriendListResponse` (2105)
- **Geschätzte Message-Größe**: ~1-2 KB (akzeptabel für seltene Zone-Joins)
- Bei Reconnect: Cooldowns werden korrekt wiederhergestellt

---

## LeaveZone (101)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Client informiert Server dass Spieler die Zone verlassen will (Portal, Hearthstone, etc.). Server speichert State und transferiert zu neuer Zone.

### Im Scope ✅
- Freiwilliges Zone-Verlassen
- State-Speicherung
- Cleanup von Zone-Resources

### Nicht im Scope ❌
- Zone-Transfer → wird durch `ZoneTransferRequest` (105) gehandhabt
- Logout → verwende `LogoutRequest` (3)

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Reason | string | "logout", "portal", "hearthstone", "death" | Ja |
| TargetZoneId | int | Ziel-Zone (falls bekannt) | Nein |

### Erwartete Response
- Server bestätigt mit neuem `JoinZone` (100) falls TargetZoneId gesetzt
- Sonst: Connection wird geschlossen (bei logout)

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `JoinZone` | 100 | Zone betreten |
| `PlayerLeftZone` | 104 | Broadcast an andere Spieler |
| `ZoneTransferRequest` | 105 | Expliziter Zone-Transfer |

### Beispiel Payload
```csharp
var leaveZone = new LeaveZone
{
    Type = MessageType.LeaveZone,
    Reason = "portal",
    TargetZoneId = 1002
};
```

### Notizen
- Server broadcastet `PlayerLeftZone` (104) an alle Spieler in Zone
- Character wird aus Zone-Entity-Liste entfernt
- Position wird in DB gespeichert

---

## ZoneState (102)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten (einmalig nach JoinZone)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Kompletter Snapshot des Zone-States. Enthält alle Spieler, NPCs, und relevante Informationen die der Client benötigt. Wird nach `JoinZone` (100) gesendet.

### Im Scope ✅
- Liste aller sichtbaren Entities (Spieler + NPCs)
- Zone-Weather und Time-of-Day
- Zone-Events (falls aktiv)

### Nicht im Scope ❌
- Einzelne Entity-Updates → verwende `EntityUpdate` (1404)
- Position-Updates → verwende `PositionBroadcast` (201)

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| ZoneId | int | Zone-ID | Ja |
| ServerTime | long | Server Unix Timestamp | Ja |
| Weather | string | "sunny", "rain", "snow", "fog" | Ja |
| TimeOfDay | float | 0.0-24.0 (Stunden) | Ja |
| Players | List<PlayerEntity> | Alle Spieler in Zone | Ja |
| NPCs | List<NpcEntity> | Alle NPCs in Range | Ja |

**PlayerEntity**:
| Feld | Typ | Beschreibung |
|------|-----|--------------|
| EntityId | int | Runtime Entity-ID |
| Name | string | Spieler-Name |
| Level | int | Level |
| X | float | Position X |
| Y | float | Position Y |
| Race | int | Rassen-ID |
| Class | int | Klassen-ID |

**NpcEntity**:
| Feld | Typ | Beschreibung |
|------|-----|--------------|
| EntityId | int | Runtime Entity-ID |
| NpcId | int | NPC-Template-ID |
| X | float | Position X |
| Y | float | Position Y |
| Health | int | Aktuelles HP (% für Boss) |

### Beispiel Payload
```csharp
var zoneState = new ZoneState
{
    Type = MessageType.ZoneState,
    ZoneId = 1001,
    ServerTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
    Weather = "sunny",
    TimeOfDay = 14.5f, // 14:30
    Players = new List<PlayerEntity>
    {
        new PlayerEntity
        {
            EntityId = 50001,
            Name = "Legolas",
            Level = 8,
            X = 105.2f,
            Y = 248.7f,
            Race = 2, // Elf
            Class = 3  // Ranger
        }
    },
    NPCs = new List<NpcEntity>
    {
        new NpcEntity
        {
            EntityId = 60001,
            NpcId = 1234,
            X = 120.0f,
            Y = 260.0f,
            Health = 100 // 100%
        }
    }
};
```

### Notizen
- Große Message - kann bei vielen Entities mehrere KB sein
- Client cached ZoneState lokal
- Updates erfolgen inkrementell über `EntityUpdate` (1404)
- Bei Reconnect: Server sendet erneut kompletten ZoneState

---

## PlayerJoinedZone (103)

**Richtung:** 📡 Broadcast (Server → All Clients in Zone)  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Broadcast an alle Spieler in Zone wenn ein neuer Spieler spawnt. Ermöglicht Clients den neuen Spieler anzuzeigen.

### Im Scope ✅
- Neuer Spieler-Informationen
- Spawn-Position
- Basic-Stats (Level, Klasse, etc.)

### Nicht im Scope ❌
- Vollständige Character-Stats → verwende `InspectRequest` (3300)
- Equipment-Details → verwende `InspectEquipment` (3302)

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| EntityId | int | Runtime Entity-ID | Ja |
| CharacterId | long | Persistente Character-ID | Ja |
| Name | string | Character-Name | Ja |
| Level | int | Level | Ja |
| Race | int | Rassen-ID | Ja |
| Class | int | Klassen-ID | Ja |
| X | float | Position X | Ja |
| Y | float | Position Y | Ja |
| Z | float | Position Z | Ja |

### Beispiel Payload
```csharp
var playerJoined = new PlayerJoinedZone
{
    Type = MessageType.PlayerJoinedZone,
    EntityId = 50002,
    CharacterId = 98766,
    Name = "Gimli",
    Level = 12,
    Race = 3, // Dwarf
    Class = 2, // Warrior
    X = 98.5f,
    Y = 255.0f,
    Z = 10.2f
};
```

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `JoinZone` | 100 | Auslöser für diesen Broadcast |
| `PlayerLeftZone` | 104 | Gegenstück beim Verlassen |
| `EntitySpawn` | 1400 | Generische Entity-Spawn Message |

### Notizen
- Wird NUR an bereits anwesende Spieler gebroadcastet
- Der joinierende Spieler selbst empfängt `JoinZone` (100)
- Client fügt Spieler zur lokalen Entity-Liste hinzu

---

## PlayerLeftZone (104)

**Richtung:** 📡 Broadcast (Server → All Clients in Zone)  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Broadcast an alle Spieler wenn ein Spieler die Zone verlässt. Client entfernt den Spieler aus der Entity-Liste.

### Im Scope ✅
- Entity-ID des verlassenden Spielers
- Grund (optional)

### Nicht im Scope ❌
- Ziel-Zone (Privacy)

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| EntityId | int | Entity-ID des Spielers | Ja |
| Reason | string | "logout", "transfer", "disconnect", "death" | Ja |

### Beispiel Payload
```csharp
var playerLeft = new PlayerLeftZone
{
    Type = MessageType.PlayerLeftZone,
    EntityId = 50002,
    Reason = "transfer"
};
```

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `LeaveZone` | 101 | Client-Request der diesen Broadcast auslöst |
| `PlayerJoinedZone` | 103 | Gegenstück beim Betreten |
| `EntityDespawn` | 1402 | Generische Entity-Despawn Message |

### Notizen
- Client entfernt Spieler-Entity aus Render-Liste
- Bei "disconnect": Spieler bleibt 30s "geistern" (Reconnect Window)

---

## ZoneTransferRequest (105)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Expliziter Request um zu anderer Zone zu wechseln (Portal, Teleport, Dungeon-Eingang).

### Im Scope ✅
- Ziel-Zone-ID
- Transfer-Typ (Portal, Teleport, etc.)
- Position in Ziel-Zone (falls bekannt)

### Nicht im Scope ❌
- Forced Transfer (Server-initiiert) → Server sendet direkt `JoinZone` (100)

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| TargetZoneId | int | Ziel-Zone-ID | Ja |
| TransferType | string | "portal", "teleport", "dungeon_entrance" | Ja |
| TargetX | float | Ziel-X (falls bekannt) | Nein |
| TargetY | float | Ziel-Y (falls bekannt) | Nein |

### Erwartete Response
- **Bei Erfolg:** `ZoneTransferResponse` (106) → dann `JoinZone` (100)
- **Bei Fehler:** `ZoneTransferResponse` (106) mit ErrorCode

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `ZoneTransferResponse` | 106 | Response zu diesem Request |
| `JoinZone` | 100 | Nach erfolgreichem Transfer |
| `ZoneLoadingProgress` | 107 | Loading-Updates während Transfer |

### Beispiel Payload
```csharp
var transferRequest = new ZoneTransferRequest
{
    Type = MessageType.ZoneTransferRequest,
    TargetZoneId = 2001,
    TransferType = "dungeon_entrance",
    TargetX = 50.0f,
    TargetY = 50.0f
};
```

### Error Codes
| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `ZONE_NOT_FOUND` | Ziel-Zone existiert nicht | Fehler anzeigen |
| `ZONE_LOCKED` | Zone ist gesperrt (Maintenance) | Warten |
| `LEVEL_TOO_LOW` | Character-Level zu niedrig | Level erhöhen |
| `QUEST_REQUIRED` | Quest erforderlich | Quest abschließen |
| `IN_COMBAT` | Im Kampf | Kampf beenden |

### Notizen
- Server validiert Transfer-Berechtigung (Level, Quests, etc.)
- Loading-Screen während Transfer

---

## ZoneTransferResponse (106)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Bestätigung oder Ablehnung eines Zone-Transfer-Requests.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Success | bool | Transfer erlaubt? | Ja |
| ErrorCode | string | Fehlercode falls Failed | Nein |
| ErrorMessage | string | Fehlermeldung | Nein |
| EstimatedLoadTime | int | Sekunden (ca.) | Bei Erfolg |

### Beispiel Payload
```csharp
var transferResponse = new ZoneTransferResponse
{
    Type = MessageType.ZoneTransferResponse,
    Success = true,
    EstimatedLoadTime = 3
};
```

### Notizen
- Bei Success: Client zeigt Loading-Screen
- Während Load: Server sendet `ZoneLoadingProgress` (107)

---

## ZoneLoadingProgress (107)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig (während Loading)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Progress-Updates während Zone-Loading (Asset-Loading, State-Sync, etc.).

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Progress | float | 0.0 - 1.0 (0% - 100%) | Ja |
| Status | string | "loading_assets", "syncing_state", "spawning" | Ja |

### Beispiel Payload
```csharp
var loadingProgress = new ZoneLoadingProgress
{
    Type = MessageType.ZoneLoadingProgress,
    Progress = 0.65f,
    Status = "syncing_state"
};
```

### Notizen
- Client zeigt Progress-Bar im Loading-Screen
- Bei Progress=1.0: Kurz danach folgt `JoinZone` (100)

---

## ZoneDiscovered (108)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Benachrichtigung dass Spieler eine neue Zone entdeckt hat (Achievement, XP-Bonus).

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| ZoneId | int | Entdeckte Zone-ID | Ja |
| ZoneName | string | Zone-Name | Ja |
| XpBonus | int | XP-Belohnung für Entdeckung | Ja |

### Beispiel Payload
```csharp
var zoneDiscovered = new ZoneDiscovered
{
    Type = MessageType.ZoneDiscovered,
    ZoneId = 1005,
    ZoneName = "Darkwood Forest",
    XpBonus = 150
};
```

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `AchievementUnlocked` | 1900 | Kann gleichzeitig ausgelöst werden |
| `XpGain` | 601 | XP-Bonus für Entdeckung |

### Notizen
- Wird beim ersten Betreten einer Zone ausgelöst
- Client zeigt Discovery-UI

---

## ZoneListRequest (109)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Fordert Liste aller bekannten/entdeckten Zonen an.

### Request Payload
Keine zusätzlichen Felder

### Erwartete Response
- **Immer:** `ZoneListResponse` (110)

---

## ZoneListResponse (110)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung
Liste aller vom Character entdeckten Zonen.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Zones | List<ZoneInfo> | Entdeckte Zonen | Ja |

**ZoneInfo**:
| Feld | Typ | Beschreibung |
|------|-----|--------------|
| ZoneId | int | Zone-ID |
| Name | string | Zone-Name |
| Level | int | Empfohlenes Level |
| Discovered | bool | Bereits entdeckt? |

### Notizen
- Für Map-UI und Fast-Travel Funktionen

---

## ShardTransfer (111)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
**Phase 2 Feature** - Transfer zu anderem Shard (Zone-Instance) für Load-Balancing.

### Notizen
- Im Prototyp: Nicht implementiert (Single-Shard)
- Phase 2: Multi-Shard Support für Skalierung

---

## ShardListRequest (112)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
**Phase 2 Feature** - Liste aller Shards für aktuelle Zone.

### Notizen
- Im Prototyp: Nicht implementiert

---

## ShardListResponse (113)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung
**Phase 2 Feature** - Antwort mit Shard-Informationen.

### Notizen
- Im Prototyp: Nicht implementiert

---

## SubZoneEnter (114)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
**Phase 2 Feature** - Spieler betritt eine Sub-Zone (Bereich innerhalb einer Zone, z.B. "Goldshire" in "Elwynn Forest").

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| SubZoneId | int | Sub-Zone-ID | Ja |
| SubZoneName | string | Name der Sub-Zone | Ja |

### Notizen
- UI-Update: Zeige Sub-Zone-Name im HUD

---

## SubZoneLeave (115)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
**Phase 2 Feature** - Spieler verlässt Sub-Zone.

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| SubZoneId | int | Verlassene Sub-Zone-ID | Ja |

---

## ZonePhaseChange (116)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
**Phase 2 Feature** - Zone ändert Phase (Quest-Fortschritt, Story-Events). Spieler sehen unterschiedliche Versionen der gleichen Zone.

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| ZoneId | int | Zone-ID | Ja |
| PhaseId | int | Neue Phase-ID | Ja |
| PhaseName | string | Phase-Name | Ja |

### Beispiel
```csharp
// Nach Quest "Defeat the Dragon"
var phaseChange = new ZonePhaseChange
{
    Type = MessageType.ZonePhaseChange,
    ZoneId = 1001,
    PhaseId = 2,
    PhaseName = "Post-Dragon Victory"
};
```

### Notizen
- Server transferiert Spieler zu anderer Phase
- Kann andere Spieler, NPCs, und Objekte zeigen
- Wird durch Quest-Fortschritt ausgelöst

---

## GetZoneRequest (117)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Client fordert Zone-Metadaten an bevor er in die Zone spawnt. Dies ermöglicht dem Client, Zone-Assets (Textures, Models, etc.) vorzuladen während der Server den Spawn vorbereitet.

### Im Scope ✅
- Zone-Metadaten abrufen (Name, Type, Weather)
- Asset-Loading vorbereiten
- Zone-Info cachen

### Nicht im Scope ❌
- Spawn-Position → kommt in `JoinZone` (100)
- PlayerEntity-Daten → kommt in `JoinZone` (100)
- Andere Spieler/NPCs → kommt in `ZoneState` (102) und `EntitySpawn` (1400)

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| ZoneId | int | Zone-ID die angefragt wird | Ja |

### Erwartete Response
- **Immer:** `GetZoneResponse` (118) mit `ZoneState` (102)

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `GetZoneResponse` | 118 | Response mit ZoneState |
| `CharacterSelectResponse` | 21 | Liefert SpawnZoneId |
| `ZoneState` | 102 | Enthält Zone-Daten |
| `JoinZone` | 100 | Folgt nach Asset-Loading |

### Flow-Diagramm
```
Client                    Gateway                   Zone Server
  │                          │                          │
  │  CharacterSelectResponse │                          │
  │  (SpawnZoneId: 1001)     │                          │
  │◄─────────────────────────│                          │
  │                          │                          │
  │  GetZoneRequest (117)    │                          │
  │  ZoneId: 1001            │                          │
  │─────────────────────────►│                          │
  │                          │                          │
  │                          │  Forward Request         │
  │                          │─────────────────────────►│
  │                          │                          │
  │                          │                          │  Load Zone Data
  │                          │                          │
  │                          │  ZoneState (102)         │
  │                          │◄─────────────────────────│
  │                          │                          │
  │  GetZoneResponse (118)   │                          │
  │  with ZoneState          │                          │
  │◄─────────────────────────│                          │
  │                          │                          │
  │  Load Zone Assets        │                          │
  │  (Textures, Models...)   │                          │
  │                          │                          │
  │  Assets loaded           │                          │
  │  Ready for Spawn         │                          │
  │                          │                          │
  │                          │  Player Ready Signal     │
  │                          │─────────────────────────►│
  │                          │                          │
  │                          │  JoinZone (100)          │
  │                          │◄─────────────────────────│
  │  JoinZone (100)          │                          │
  │◄─────────────────────────│                          │
```

### Beispiel Payload
```csharp
var getZoneRequest = new GetZoneRequest
{
    Type = MessageType.GetZoneRequest,
    ZoneId = 1001
};
```

### Error Codes
| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `ZONE_NOT_FOUND` | Zone-ID existiert nicht | Client-Fehler, sollte nicht passieren |
| `ZONE_LOCKED` | Zone gesperrt (Maintenance) | Wartungsmeldung anzeigen |
| `ACCESS_DENIED` | Keine Berechtigung für Zone | Level/Quest-Anforderung anzeigen |

### Notizen
- Wird nach `CharacterSelectResponse` gesendet
- Client erhält ZoneState mit allen statischen Zone-Infos
- Client lädt Assets während Server Spawn vorbereitet
- Reduziert wahrgenommene Loading-Time durch paralleles Laden
- Zone-Daten können client-seitig gecached werden

---

## GetZoneResponse (118)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung
Response auf `GetZoneRequest` (117). Enthält als Payload eine `ZoneState` (102) Message mit allen Zone-Metadaten.

**Hinweis:** Diese Message ist ein Wrapper um `ZoneState` (102) im Request-Response Pattern. Der eigentliche Payload ist identisch mit `ZoneState`.

### Im Scope ✅
- Success/Error Status
- ZoneState bei Erfolg
- Error-Informationen bei Fehler

### Nicht im Scope ❌
- Player-spezifische Daten → kommt in `JoinZone` (100)

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Success | bool | Request erfolgreich? | Ja |
| ErrorCode | string | Fehlercode falls Success=false | Nein |
| ErrorMessage | string | Fehlermeldung | Nein |
| ZoneState | ZoneState | Zone-Daten (siehe Message 102) | Bei Erfolg |

**ZoneState** (siehe [ZoneState (102)](#zonestate-102) für vollständige Spezifikation):
| Feld | Typ | Beschreibung |
|------|-----|--------------|
| ZoneId | int | Zone-ID |
| ZoneName | string | Name der Zone |
| ZoneType | string | "outdoor", "dungeon", "city", "instance" |
| ServerTime | long | Server Unix Timestamp |
| Weather | string | "sunny", "rain", "snow", "fog" |
| TimeOfDay | float | 0.0-24.0 (Stunden) |
| Players | List<PlayerEntity> | **Leer** bei GetZoneResponse |
| NPCs | List<NpcEntity> | **Leer** bei GetZoneResponse |

**Wichtig:** Bei `GetZoneResponse` sind die `Players` und `NPCs` Listen **leer**, da der Spieler noch nicht in der Zone gespawnt ist. Diese Daten kommen später via separater `EntitySpawn` Messages.

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `GetZoneRequest` | 117 | Request zu dieser Response |
| `ZoneState` | 102 | Payload dieser Response |
| `JoinZone` | 100 | Folgt nach dieser Response |

### Beispiel Payload
```csharp
// Erfolg
var successResponse = new GetZoneResponse
{
    Type = MessageType.GetZoneResponse,
    Success = true,
    ZoneState = new ZoneState
    {
        ZoneId = 1001,
        ZoneName = "Elwynn Forest",
        ZoneType = "outdoor",
        ServerTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
        Weather = "sunny",
        TimeOfDay = 14.5f, // 14:30
        Players = new List<PlayerEntity>(), // Leer!
        NPCs = new List<NpcEntity>()        // Leer!
    }
};

// Fehler
var errorResponse = new GetZoneResponse
{
    Type = MessageType.GetZoneResponse,
    Success = false,
    ErrorCode = "ZONE_LOCKED",
    ErrorMessage = "Zone ist wegen Wartungsarbeiten gesperrt"
};
```

### Error Codes
| Code | Bedeutung |
|------|-----------|
| `ZONE_NOT_FOUND` | Zone-ID existiert nicht |
| `ZONE_LOCKED` | Zone gesperrt (Maintenance) |
| `ACCESS_DENIED` | Keine Berechtigung für Zone |

### Notizen
- Wrapper um `ZoneState` (102) für Request-Response Pattern
- Players und NPCs Listen sind immer leer
- Client sollte Zone-Daten cachen (basierend auf ZoneId + Version)
- Nach Erhalt: Client lädt Assets, dann bereit für `JoinZone` (100)

---

**Letzte Aktualisierung**: 2025-12-26  
**Version**: 1.1.0

[← Zurück zur Übersicht](README.md)
