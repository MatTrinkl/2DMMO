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

---

## JoinZone (100)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Server informiert Client dass Character in eine Zone gespawnt wird. Enthält Zone-ID, Spawn-Position und initiale Informationen. Dies ist die erste Message nach Character-Auswahl.

### Im Scope ✅
- Zone-ID und Name
- Spawn-Position (X, Y, Z)
- Zone-Type (Outdoor, Dungeon, City, etc.)
- Initialer Character-State (HP, Mana, etc.)

### Nicht im Scope ❌
- Vollständiger ZoneState mit anderen Spielern → verwende `ZoneState` (102)
- Entity-Liste → Server sendet separate `EntitySpawn` Messages (1400)

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| ZoneId | ushort | Eindeutige Zone-ID | Ja |
| ZoneName | string | Name der Zone | Ja |
| ZoneType | string | "outdoor", "dungeon", "city", "instance" | Ja |
| PlayerData | PlayerEntityDto | Kompletter Character-State | Ja |

**PlayerEntityDto** (siehe [DTO_ARCHITECTURE.md](DTO_ARCHITECTURE.md) für vollständige Referenz):
| Feld | Typ | Beschreibung |
|------|-----|--------------|
| RuntimeId | EntityIdentity | Runtime Entity-ID |
| Position | Position | Spawn-Position (X, Y, Z) |
| DisplayName | string | Character-Name |
| Level | int | Character-Level |
| CurrentHealth | int | Aktuelle HP |
| MaxHealth | int | Maximale HP |
| CurrentResource | int | Aktuelles Mana/Energy/Rage |
| MaxResource | int | Maximales Mana/Energy/Rage |
| CombatResourceType | CombatResourceType | Typ der Ressource |
| Race | Race | Rasse |
| Class | CharacterClass | Klasse |
| Gender | Gender | Geschlecht |
| ... | ... | (weitere Properties siehe DTO_ARCHITECTURE.md) |

**Hinweis**: `Experience`, `Gold` und `AccountId` sind **NICHT** enthalten (ServerOnly Properties).

### Erwartete Response
- Client sendet `PositionUpdate` (200) um Spawn zu bestätigen
- Danach: Client empfängt `ZoneState` (102) mit anderen Entities

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `ZoneState` | 102 | Enthält kompletten Zone-State |
| `LeaveZone` | 101 | Verlassen der Zone |
| `CharacterSelect` | 9 | Auslöser für JoinZone |
| `EntitySpawn` | 1400 | Andere Spieler/NPCs in Zone |

### Flow-Diagramm
```
Client                    Zone Server
  │                          │
  │  CharacterSelect (9)     │
  │─────────────────────────►│
  │                          │  Load Character
  │                          │  Find Spawn Point
  │                          │
  │  JoinZone (100)          │
  │◄─────────────────────────│
  │                          │
  │  PositionUpdate (200)    │
  │─────────────────────────►│
  │                          │
  │  ZoneState (102)         │
  │◄─────────────────────────│
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
    PlayerData = new PlayerEntityDto
    {
        RuntimeId = new EntityIdentity(1, 1001, 50001, 0, 1),
        PersistentId = characterGuid,
        Position = new Position(100.5f, 250.0f, 10.0f, 1001),
        DisplayName = "Alice",
        Level = 10,
        CurrentHealth = 850,
        MaxHealth = 1000,
        CurrentResource = 200,
        MaxResource = 300,
        CombatResourceType = CombatResourceType.Mana,
        Race = Race.Human,
        Class = CharacterClass.Mage,
        Gender = Gender.Female
        // Experience, Gold, AccountId sind NICHT enthalten (ServerOnly)
    }
};
```

### Notizen
- Loading-Screen im Client während Zone-Load
- Client lädt Zone-Assets basierend auf ZoneId
- Nach JoinZone: Server sendet andere Spieler als `EntitySpawn` (1400)
- Spawn-Position ist entweder: Last-Position, Hearthstone, oder Zone-Default

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
| ZoneId | ushort | Zone-ID | Ja |
| ServerTime | long | Server Unix Timestamp | Ja |
| Weather | string | "sunny", "rain", "snow", "fog" | Ja |
| TimeOfDay | float | 0.0-24.0 (Stunden) | Ja |
| Players | List\<PlayerEntityDto\> | Alle Spieler in Zone | Ja |
| NPCs | List\<NpcEntityDto\> | Alle NPCs in Range | Ja |

**PlayerEntityDto** (siehe [DTO_ARCHITECTURE.md](DTO_ARCHITECTURE.md)):
| Feld | Typ | Beschreibung |
|------|-----|--------------|
| RuntimeId | EntityIdentity | Runtime Entity-ID |
| DisplayName | string | Spieler-Name |
| Level | int | Level |
| Position | Position | Position (X, Y, Z) |
| Race | Race | Rasse |
| Class | CharacterClass | Klasse |
| CurrentHealth | int | Aktuelle HP |
| MaxHealth | int | Max HP |
| ... | ... | (weitere Properties siehe DTO_ARCHITECTURE.md) |

**NpcEntityDto** (geplant, aktuell noch NpcEntity):
| Feld | Typ | Beschreibung |
|------|-----|--------------|
| RuntimeId | EntityIdentity | Runtime Entity-ID |
| NpcTemplateId | int | NPC-Template-ID |
| Position | Position | Position (X, Y, Z) |
| CurrentHealth | int | Aktuelles HP (% für Boss) |

### Beispiel Payload
```csharp
var zoneState = new ZoneState
{
    Type = MessageType.ZoneState,
    ZoneId = 1001,
    ServerTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
    Weather = "sunny",
    TimeOfDay = 14.5f, // 14:30
    Players = new List<PlayerEntityDto>
    {
        new PlayerEntityDto
        {
            RuntimeId = new EntityIdentity(1, 1001, 50001, 0, 1),
            DisplayName = "Legolas",
            Level = 8,
            Position = new Position(105.2f, 248.7f, 10.0f, 1001),
            Race = Race.Elf,
            Class = CharacterClass.Ranger,
            CurrentHealth = 450,
            MaxHealth = 500
            // Experience und Gold sind NICHT enthalten (ServerOnly)
        }
    },
    NPCs = new List<NpcEntityDto>
    {
        new NpcEntityDto
        {
            RuntimeId = new EntityIdentity(1, 1001, 60001, 0, 1234),
            NpcTemplateId = 1234,
            Position = new Position(120.0f, 260.0f, 10.0f, 1001),
            CurrentHealth = 100,
            MaxHealth = 100
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
| PlayerData | PlayerEntityDto | Komplette Spieler-Informationen | Ja |

**PlayerEntityDto** (siehe [DTO_ARCHITECTURE.md](DTO_ARCHITECTURE.md) für vollständige Referenz):

Das DTO enthält alle sichtbaren Informationen über den neuen Spieler:
- Runtime- und Persistent-IDs
- Position und Movement
- Display-Name, Level, Race, Class
- Health/Resource Status
- Combat-State
- **NICHT** enthalten: Experience, Gold, AccountId (ServerOnly)

### Beispiel Payload
```csharp
var playerJoined = new PlayerJoinedZone
{
    Type = MessageType.PlayerJoinedZone,
    PlayerData = new PlayerEntityDto
    {
        RuntimeId = new EntityIdentity(1, 1001, 50002, 0, 1),
        PersistentId = characterGuid,
        CharacterId = characterGuid,
        DisplayName = "Gimli",
        Level = 12,
        Race = Race.Dwarf,
        Class = CharacterClass.Warrior,
        Position = new Position(98.5f, 255.0f, 10.2f, 1001),
        CurrentHealth = 800,
        MaxHealth = 1200,
        CurrentResource = 100,
        MaxResource = 100,
        CombatResourceType = CombatResourceType.Rage
    }
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
| PlayerId | Guid | Persistent Player-ID | Ja |
| Reason | string | "logout", "transfer", "disconnect", "death" | Ja |

**Hinweis:** PlayerId entspricht `PlayerEntityDto.PersistentId` für konsistente ID-Referenzierung.

### Beispiel Payload
```csharp
var playerLeft = new PlayerLeftZone
{
    Type = MessageType.PlayerLeftZone,
    PlayerId = playerGuid, // Persistent ID
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

**Letzte Aktualisierung**: 2025-12-17  
**Version**: 1.0.0

[← Zurück zur Übersicht](README.md)
