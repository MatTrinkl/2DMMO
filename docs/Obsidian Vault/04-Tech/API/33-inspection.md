# �� Inspection Messages (3300-3399)

**Kategorie:** 33  
**Range:** 3300-3399  
**Phase:** Prototyp  
**Status:** 🟢 In Entwicklung

[← Zurück zur Übersicht](Message-Reference.md)

---

## 📋 Inhaltsverzeichnis

- [📋 Überblick](#-überblick)
- [🧠 Datenmodell](#-datenmodell)
- [🔒 Privacy & Permissions](#-privacy--permissions)
- [🔄 Sync, Caching & Staleness](#-sync-caching--staleness)
- [🧱 DTOs / Interfaces](#-dtos--interfaces)
- [🧩 Enums / ErrorCodes / Flags](#-enums--errorcodes--flags)
- [⚙️ Regeln & Sicherheit](#️-regeln--sicherheit)
- [📩 Aktive Messages 3300–3399](#-aktive-messages-33003399)
- [🧨 Edge Cases & Fehlerfälle](#-edge-cases--fehlerfälle)
- [📎 Anhang](#-anhang)

---

## 📋 Überblick

Das Inspection-System ermöglicht Spielern, Profile und Ausrüstung anderer Charaktere einzusehen. Es liefert **Snapshots** (keine Live-Streams) mit Revision/GeneratedAt + CacheTtlMs für effizientes Client-Caching.

### Ziele

- **Profil-Ansicht**: Schnelle UI zum Anzeigen von Character-Informationen
- **Equipment-Inspektion**: Ausrüstung anderer Spieler einsehen
- **Statistik-Abfrage**: Stats, Achievements, PvP-Daten abrufen
- **Datenschutz**: Privacy-Einstellungen respektieren

### Architektur

```
Client                         Server                        Database
  │                              │                              │
  │  InspectRequest (3300)       │                              │
  │  TargetCharacterId           │                              │
  │─────────────────────────────►│                              │
  │                              │  Load Profile + Privacy      │
  │                              │─────────────────────────────►│
  │                              │                              │
  │                              │  Profile Data                │
  │                              │◄─────────────────────────────│
  │                              │                              │
  │  InspectResponse (3301)      │  [Privacy Check]             │
  │  InspectProfile Snapshot     │  [Build Snapshot]            │
  │◄─────────────────────────────│                              │
```

---

## 🧠 Datenmodell

### InspectProfile

Das zentrale Datenobjekt für Inspection-Snapshots.

| Feld | Typ | Beschreibung |
|------|-----|--------------|
| CharacterId | Guid | Eindeutige Character-ID |
| DisplayName | string | Anzeigename des Characters |
| Level | int | Aktuelles Level |
| ClassId | int | Klassen-ID |
| RaceId | int | Rassen-ID |
| Title | string? | Aktiver Titel (optional) |
| GuildName | string? | Guild-Name (optional, privacy-abhängig) |
| GuildRank | string? | Guild-Rang (optional) |
| ZoneName | string? | Aktuelle Zone (optional, privacy-abhängig) |
| OnlineState | OnlineState | Online/Offline/Away/DND |
| Revision | long | Snapshot-Revision für Caching |
| GeneratedAt | long | Unix Timestamp der Generierung |
| CacheTtlMs | int | Cache Time-To-Live in Millisekunden |

### EquipmentSnapshot

| Feld | Typ | Beschreibung |
|------|-----|--------------|
| Slots | List\<EquipmentSlotData\> | Ausrüstungs-Slots |
| GearScore | int | Berechneter GearScore |
| AverageItemLevel | int | Durchschnittliches Item-Level |

### EquipmentSlotData

| Feld | Typ | Beschreibung |
|------|-----|--------------|
| SlotId | EquipmentSlot | Slot-Enum (Head, Chest, etc.) |
| ItemTemplateId | int | Item-Template-ID |
| ItemLevel | int | Item-Level |
| Rarity | ItemRarity | Seltenheit |
| EnchantId | int? | Enchant-ID (optional) |
| GemIds | List\<int\>? | Socket-Gems (optional) |
| TransmogId | int? | Cosmetic Override (optional) |

### StatsSnapshot

| Feld | Typ | Beschreibung |
|------|-----|--------------|
| Stats | Dictionary\<StatType, int\> | Whitelisted Stats |
| Resistances | Dictionary\<DamageType, int\>? | Resistenzen (optional) |

**Annahme:** Nur whitelisted Stats werden übertragen (keine sensiblen/hidden Werte wie interne Cooldowns).

### PvpSnapshot

| Feld | Typ | Beschreibung |
|------|-----|--------------|
| HonorableKills | int | Ehrenhafte Kills |
| HonorPoints | int | Ehrenpunkte |
| ArenaRating | int? | Arena-Rating (optional) |
| BattlegroundWins | int | BG-Siege |

### AchievementSnapshot

| Feld | Typ | Beschreibung |
|------|-----|--------------|
| TotalPoints | int | Gesamte Achievement-Punkte |
| CompletedCount | int | Anzahl abgeschlossener Achievements |
| RecentAchievements | List\<AchievementEntry\> | Letzte 5 Achievements |

### GuildSnapshot

| Feld | Typ | Beschreibung |
|------|-----|--------------|
| GuildId | Guid | Guild-ID |
| GuildName | string | Guild-Name |
| GuildRank | string | Rang des Spielers |
| MemberCount | int | Mitgliederanzahl |
| GuildLevel | int | Guild-Level |

### TalentSnapshot

| Feld | Typ | Beschreibung |
|------|-----|--------------|
| SpecializationId | int | Aktive Spezialisierung |
| TalentChoices | List\<TalentChoice\> | Gewählte Talente |

---

## 🔒 Privacy & Permissions

### InspectPrivacySetting Enum

Jeder Spieler kann einstellen, wer ihn inspizieren darf:

```csharp
public enum InspectPrivacySetting : byte
{
    Everyone = 0,      // Jeder kann inspizieren
    Friends = 1,       // Nur Freunde
    Guild = 2,         // Nur Guild-Mitglieder
    FriendsAndGuild = 3, // Freunde und Guild
    None = 4           // Niemand (außer Self-Inspect)
}
```

### Feldbasierte Privacy

Einzelne Felder können versteckt werden:

| Feld | Default | Beschreibung |
|------|---------|--------------|
| ShowEquipment | true | Equipment anzeigen |
| ShowTalents | true | Talente anzeigen |
| ShowStats | true | Stats anzeigen |
| ShowGuild | true | Guild-Info anzeigen |
| ShowLocation | false | Zone/Position anzeigen |
| ShowAchievements | true | Achievements anzeigen |
| ShowPvpStats | true | PvP-Statistiken anzeigen |

### Blocklist-Integration

- Wenn Target auf Blocklist des Requesters: generischer Fehler `TARGET_UNAVAILABLE`
- Wenn Requester auf Blocklist des Targets: generischer Fehler `TARGET_UNAVAILABLE`
- **Keine Details** warum Inspect fehlschlug (Privacy/Security)

### Permission-Matrix

| Requester → Target | Everyone | Friends | Guild | FriendsAndGuild | None |
|-------------------|----------|---------|-------|-----------------|------|
| Stranger | ✅ | ❌ | ❌ | ❌ | ❌ |
| Friend | ✅ | ✅ | ❌ | ✅ | ❌ |
| Guild Member | ✅ | ❌ | ✅ | ✅ | ❌ |
| Friend + Guild | ✅ | ✅ | ✅ | ✅ | ❌ |
| Self | ✅ | ✅ | ✅ | ✅ | ✅ |

---

## 🔄 Sync, Caching & Staleness

### Snapshot-Philosophie

- Inspect liefert **SNAPSHOTS** (keine Live-Streams)
- Jeder Snapshot hat `Revision` + `GeneratedAt` + `CacheTtlMs`
- Client cached Snapshots lokal

### ETag/Revision System

```
InspectRequest
├── TargetCharacterId: Guid
└── LastKnownRevision: long?   ← Optional: letzte bekannte Revision

InspectResponse
├── Success: true
├── Profile: InspectProfile?   ← null wenn NotModified
├── NotModified: bool          ← true wenn Revision gleich
└── Revision: long             ← Aktuelle Revision
```

### Cache-Flow

```
1. Erster Request:
   Client: InspectRequest(targetId, lastKnownRevision: null)
   Server: InspectResponse(profile: full, revision: 123)
   Client: Cache profile mit revision 123

2. Zweiter Request (innerhalb TTL):
   Client: Nutzt gecachten Snapshot, kein Request

3. Request nach TTL:
   Client: InspectRequest(targetId, lastKnownRevision: 123)
   Server: [Revision unchanged] InspectResponse(profile: null, notModified: true, revision: 123)
   Client: Weiter cached nutzen

4. Request nach Equipment-Change:
   Client: InspectRequest(targetId, lastKnownRevision: 123)
   Server: [Revision bumped to 124] InspectResponse(profile: full, notModified: false, revision: 124)
   Client: Cache invalidieren, neuen Snapshot speichern
```

### Cache-Konstanten

| Konstante | Wert | Beschreibung |
|-----------|------|--------------|
| INSPECT_CACHE_TTL_MS | 30000 | 30 Sekunden Client-Cache |
| INSPECT_SERVER_CACHE_TTL_MS | 5000 | 5 Sekunden Server-Cache |
| REVISION_BUMP_DEBOUNCE_MS | 1000 | 1 Sekunde Debounce für Revision-Bumps |

### Invalidation-Events

Revision wird gebumpt bei:
- Equipment-Änderung
- Level-Up
- Talent-Änderung
- Title-Wechsel
- Guild-Beitritt/-Verlassen
- Achievement-Unlock

---

## 🧱 DTOs / Interfaces

### InspectProfileDto

```csharp
[MessagePackObject]
public class InspectProfileDto
{
    [Key(0)] public Guid CharacterId { get; set; }
    [Key(1)] public string DisplayName { get; set; } = "";
    [Key(2)] public int Level { get; set; }
    [Key(3)] public int ClassId { get; set; }
    [Key(4)] public int RaceId { get; set; }
    [Key(5)] public string? Title { get; set; }
    [Key(6)] public string? GuildName { get; set; }
    [Key(7)] public string? GuildRank { get; set; }
    [Key(8)] public string? ZoneName { get; set; }
    [Key(9)] public OnlineState OnlineState { get; set; }
    [Key(10)] public long Revision { get; set; }
    [Key(11)] public long GeneratedAt { get; set; }
    [Key(12)] public int CacheTtlMs { get; set; }
}
```

### EquipmentSnapshotDto

```csharp
[MessagePackObject]
public class EquipmentSnapshotDto
{
    [Key(0)] public List<EquipmentSlotDto> Slots { get; set; } = new();
    [Key(1)] public int GearScore { get; set; }
    [Key(2)] public int AverageItemLevel { get; set; }
}

[MessagePackObject]
public class EquipmentSlotDto
{
    [Key(0)] public EquipmentSlot SlotId { get; set; }
    [Key(1)] public int ItemTemplateId { get; set; }
    [Key(2)] public int ItemLevel { get; set; }
    [Key(3)] public ItemRarity Rarity { get; set; }
    [Key(4)] public int? EnchantId { get; set; }
    [Key(5)] public List<int>? GemIds { get; set; }
    [Key(6)] public int? TransmogId { get; set; }
}
```

### StatsSnapshotDto

```csharp
[MessagePackObject]
public class StatsSnapshotDto
{
    [Key(0)] public Dictionary<StatType, int> Stats { get; set; } = new();
    [Key(1)] public Dictionary<DamageType, int>? Resistances { get; set; }
}
```

### PvpSnapshotDto

```csharp
[MessagePackObject]
public class PvpSnapshotDto
{
    [Key(0)] public int HonorableKills { get; set; }
    [Key(1)] public int HonorPoints { get; set; }
    [Key(2)] public int? ArenaRating { get; set; }
    [Key(3)] public int BattlegroundWins { get; set; }
}
```

### AchievementSnapshotDto

```csharp
[MessagePackObject]
public class AchievementSnapshotDto
{
    [Key(0)] public int TotalPoints { get; set; }
    [Key(1)] public int CompletedCount { get; set; }
    [Key(2)] public List<AchievementEntryDto> RecentAchievements { get; set; } = new();
}

[MessagePackObject]
public class AchievementEntryDto
{
    [Key(0)] public int AchievementId { get; set; }
    [Key(1)] public string Name { get; set; } = "";
    [Key(2)] public long CompletedAt { get; set; }
}
```

### GuildSnapshotDto

```csharp
[MessagePackObject]
public class GuildSnapshotDto
{
    [Key(0)] public Guid GuildId { get; set; }
    [Key(1)] public string GuildName { get; set; } = "";
    [Key(2)] public string GuildRank { get; set; } = "";
    [Key(3)] public int MemberCount { get; set; }
    [Key(4)] public int GuildLevel { get; set; }
}
```

### TalentSnapshotDto

```csharp
[MessagePackObject]
public class TalentSnapshotDto
{
    [Key(0)] public int SpecializationId { get; set; }
    [Key(1)] public List<TalentChoiceDto> TalentChoices { get; set; } = new();
}

[MessagePackObject]
public class TalentChoiceDto
{
    [Key(0)] public int TierId { get; set; }
    [Key(1)] public int TalentId { get; set; }
}
```

---

## 🧩 Enums / ErrorCodes / Flags

### InspectResponseErrorCode

```csharp
public enum InspectResponseErrorCode : byte
{
    None = 0,
    TargetNotFound = 1,        // Character existiert nicht
    TargetUnavailable = 2,     // Privacy/Blocklist (generisch)
    TargetOffline = 3,         // Nur Online-Inspect erlaubt (optional)
    InvalidRequest = 4,        // Ungültige Request-Parameter
    InternalError = 5          // Server-Fehler
}
```

### ArmoryResponseErrorCode

```csharp
public enum ArmoryResponseErrorCode : byte
{
    None = 0,
    CharacterNotFound = 1,
    CharacterUnavailable = 2,
    InvalidRequest = 3,
    InternalError = 4
}
```

### GearScoreCalculateResponseErrorCode

```csharp
public enum GearScoreCalculateResponseErrorCode : byte
{
    None = 0,
    CharacterNotFound = 1,
    InvalidRequest = 2,
    InternalError = 3
}
```

### StatisticsResponseErrorCode

```csharp
public enum StatisticsResponseErrorCode : byte
{
    None = 0,
    CharacterNotFound = 1,
    CharacterUnavailable = 2,
    InvalidStatType = 3,
    InternalError = 4
}
```

### OnlineState

```csharp
public enum OnlineState : byte
{
    Offline = 0,
    Online = 1,
    Away = 2,
    DoNotDisturb = 3,
    Invisible = 4   // Zeigt als Offline, aber tatsächlich Online
}
```

### EquipmentSlot

```csharp
public enum EquipmentSlot : byte
{
    Head = 0,
    Neck = 1,
    Shoulder = 2,
    Back = 3,
    Chest = 4,
    Wrist = 5,
    Hands = 6,
    Waist = 7,
    Legs = 8,
    Feet = 9,
    Ring1 = 10,
    Ring2 = 11,
    Trinket1 = 12,
    Trinket2 = 13,
    MainHand = 14,
    OffHand = 15,
    Ranged = 16,
    Tabard = 17,
    Shirt = 18
}
```

### ItemRarity

```csharp
public enum ItemRarity : byte
{
    Poor = 0,       // Grau
    Common = 1,     // Weiß
    Uncommon = 2,   // Grün
    Rare = 3,       // Blau
    Epic = 4,       // Lila
    Legendary = 5,  // Orange
    Artifact = 6    // Gold
}
```

---

## ⚙️ Regeln & Sicherheit

### Rate Limiting

| Aktion | Limit | Zeitfenster | Cooldown |
|--------|-------|-------------|----------|
| InspectRequest | 30 | 60 Sekunden | 2 Sekunden zwischen Requests |
| ArmoryRequest | 10 | 60 Sekunden | 5 Sekunden zwischen Requests |
| StatisticsRequest | 20 | 60 Sekunden | 3 Sekunden zwischen Requests |
| GearScoreCalculate | 10 | 60 Sekunden | 5 Sekunden zwischen Requests |
| PlayedTimeRequest | 5 | 60 Sekunden | 10 Sekunden zwischen Requests |

### Anti-Scraping

- Rate Limiting pro Requester (siehe oben)
- Keine Enumeration-Leaks: bei `TargetNotFound` und `TargetUnavailable` ähnliche Response-Zeit
- Keine Details bei Blocked/Privacy-Reject
- Server-Log bei verdächtigen Patterns (>100 unique targets/minute)

### Input-Validierung

| Feld | Validierung |
|------|-------------|
| TargetCharacterId | Muss gültige GUID sein |
| LastKnownRevision | Muss >= 0 sein wenn gesetzt |
| StatTypes | Nur whitelisted StatTypes erlaubt |

### Server-Authority

- Server validiert ALLE Requests
- Client-seitige Caches sind nur Optimierung
- Server entscheidet über Privacy-Zugriff
- Server generiert Revision (Client kann nicht manipulieren)

---

## 📩 Aktive Messages 3300–3399

Die Messages sind in der Reihenfolge des `MessageType.cs` Enums aufgelistet.

---

### InspectRequest (3300)

**Richtung:** 📤 Client → Server  
**Frequenz:** Mittel (throttled)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

#### Beschreibung

Client fordert Inspection-Daten eines anderen Spielers an. Unterstützt Conditional-Requests via `LastKnownRevision`.

#### Im Scope ✅

- Profil-Daten abrufen
- Equipment-Snapshot abrufen
- Conditional Request mit Revision

#### Nicht im Scope ❌

- Live-Streaming von Daten → nur Snapshots
- Detaillierte Statistiken → verwende `StatisticsRequest` (3330)
- Armory-Daten → verwende `ArmoryRequest` (3310)

#### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.InspectRequest` | Ja |
| TargetCharacterId | Guid | ID des zu inspizierenden Characters | Ja |
| LastKnownRevision | long? | Letzte bekannte Revision für Conditional-Request | Nein |
| IncludeEquipment | bool | Equipment-Snapshot einschließen | Ja |
| IncludeTalents | bool | Talent-Snapshot einschließen | Ja |

#### Erwartete Response

- `InspectResponse` (3301)

#### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.InspectRequest)]
public class InspectRequest : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.InspectRequest;
    [Key(1)] public Guid TargetCharacterId { get; set; }
    [Key(2)] public long? LastKnownRevision { get; set; }
    [Key(3)] public bool IncludeEquipment { get; set; } = true;
    [Key(4)] public bool IncludeTalents { get; set; } = true;
}
```

#### Server-Verhalten

1. Rate-Limit prüfen
2. Target existiert? → `TargetNotFound`
3. Blocklist prüfen → `TargetUnavailable`
4. Privacy-Setting prüfen → `TargetUnavailable`
5. Revision vergleichen → `NotModified` oder Full-Snapshot
6. Snapshot generieren und senden

#### Client-Verhalten

1. Cache prüfen (TTL)
2. Request mit LastKnownRevision senden
3. Bei `NotModified`: Cache weiter nutzen
4. Bei Full-Snapshot: Cache aktualisieren
5. UI anzeigen

#### Flow-Diagramm

```
Client                         Server                        
  │                              │
  │  InspectRequest (3300)       │
  │  TargetId, LastKnownRevision │
  │─────────────────────────────►│
  │                              │  [Rate Limit Check]
  │                              │  [Target Exists?]
  │                              │  [Privacy Check]
  │                              │  [Revision Check]
  │                              │
  │  InspectResponse (3301)      │
  │  Profile OR NotModified      │
  │◄─────────────────────────────│
```

#### Beispiel Payloads

```csharp
// Erster Request (ohne Revision)
var firstRequest = new InspectRequest
{
    TargetCharacterId = Guid.Parse("a1b2c3d4-..."),
    LastKnownRevision = null,
    IncludeEquipment = true,
    IncludeTalents = true
};

// Conditional Request (mit Revision)
var conditionalRequest = new InspectRequest
{
    TargetCharacterId = Guid.Parse("a1b2c3d4-..."),
    LastKnownRevision = 12345,
    IncludeEquipment = true,
    IncludeTalents = false
};
```

#### Error Codes

| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `TargetNotFound` | Character existiert nicht | UI: "Spieler nicht gefunden" |
| `TargetUnavailable` | Privacy/Blocklist | UI: "Spieler nicht verfügbar" |
| `RateLimited` | Zu viele Requests | Warten, Backoff |

#### Verwandte Messages

| Message | ID | Beziehung |
|---------|-----|-----------|
| `InspectResponse` | 3301 | Response zu diesem Request |
| `ArmoryRequest` | 3310 | Für detailliertere Daten |
| `StatisticsRequest` | 3330 | Für Statistiken |

#### Notizen

- Self-Inspect ist immer erlaubt (optimierter Pfad)
- Cross-Zone Inspect ist erlaubt
- Offline-Spieler können inspiziert werden (letzte Daten)

---

### InspectResponse (3301)

**Richtung:** 📥 Server → Client  
**Frequenz:** Mittel  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

#### Beschreibung

Antwort auf `InspectRequest`. Enthält den kompletten Inspection-Snapshot oder `NotModified`-Flag.

#### Im Scope ✅

- Profil-Snapshot
- Equipment-Snapshot (optional)
- Talent-Snapshot (optional)
- NotModified-Handling

#### Nicht im Scope ❌

- PvP-Details → verwende `InspectPvp` (3305)
- Achievement-Details → verwende `InspectAchievements` (3304)
- Guild-Details → verwende `InspectGuild` (3306)

#### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.InspectResponse` | Ja |
| Success | bool | Request erfolgreich? | Ja |
| GlobalError | GlobalErrorCode | Globaler Fehlercode | Ja |
| ErrorCode | InspectResponseErrorCode? | Spezifischer Fehlercode | Nein |
| ErrorMessage | string? | Fehlermeldung | Nein |
| Profile | InspectProfileDto? | Profil-Snapshot | Bei Erfolg |
| Equipment | EquipmentSnapshotDto? | Equipment-Snapshot | Optional |
| Talents | TalentSnapshotDto? | Talent-Snapshot | Optional |
| NotModified | bool | Revision unverändert | Ja |
| Revision | long | Aktuelle Revision | Ja |
| Timestamp | long | Server-Timestamp | Ja |

#### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.InspectResponse)]
public class InspectResponse : IResponseMessage<InspectResponseErrorCode>, ITimestampedMessage
{
    [Key(0)] public MessageType Type => MessageType.InspectResponse;
    [Key(1)] public bool Success { get; init; }
    [Key(2)] public GlobalErrorCode GlobalError { get; init; }
    [Key(3)] public InspectResponseErrorCode? ErrorCode { get; init; }
    [Key(4)] public string? ErrorMessage { get; init; }
    [Key(5)] public InspectProfileDto? Profile { get; init; }
    [Key(6)] public EquipmentSnapshotDto? Equipment { get; init; }
    [Key(7)] public TalentSnapshotDto? Talents { get; init; }
    [Key(8)] public bool NotModified { get; init; }
    [Key(9)] public long Revision { get; init; }
    [Key(10)] public long Timestamp { get; init; }
}
```

#### Beispiel Payloads

```csharp
// Erfolgreiche Response mit Snapshot
var successResponse = new InspectResponse
{
    Success = true,
    GlobalError = GlobalErrorCode.None,
    Profile = new InspectProfileDto
    {
        CharacterId = Guid.Parse("a1b2c3d4-..."),
        DisplayName = "Aragorn",
        Level = 60,
        ClassId = 1,
        RaceId = 1,
        Title = "Champion",
        GuildName = "Fellowship",
        OnlineState = OnlineState.Online,
        Revision = 12346,
        GeneratedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
        CacheTtlMs = 30000
    },
    Equipment = new EquipmentSnapshotDto { ... },
    Talents = new TalentSnapshotDto { ... },
    NotModified = false,
    Revision = 12346,
    Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
};

// NotModified Response
var notModifiedResponse = new InspectResponse
{
    Success = true,
    GlobalError = GlobalErrorCode.None,
    Profile = null,
    Equipment = null,
    Talents = null,
    NotModified = true,
    Revision = 12345,
    Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
};

// Fehler Response
var errorResponse = new InspectResponse
{
    Success = false,
    GlobalError = GlobalErrorCode.None,
    ErrorCode = InspectResponseErrorCode.TargetUnavailable,
    ErrorMessage = "Player is not available for inspection",
    NotModified = false,
    Revision = 0,
    Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
};
```

---

### InspectEquipment (3302)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

#### Beschreibung

Detaillierter Equipment-Snapshot. Kann als Zusatz-Message nach `InspectResponse` gesendet werden, wenn mehr Details benötigt werden.

#### Im Scope ✅

- Vollständige Equipment-Details
- Item-Tooltips-Daten
- Enchants und Gems

#### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.InspectEquipment` | Ja |
| CharacterId | Guid | Character-ID | Ja |
| Equipment | EquipmentSnapshotDto | Vollständiger Equipment-Snapshot | Ja |
| Timestamp | long | Server-Timestamp | Ja |

#### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.InspectEquipment)]
public class InspectEquipment : IServerMessage, ITimestampedMessage
{
    [Key(0)] public MessageType Type => MessageType.InspectEquipment;
    [Key(1)] public Guid CharacterId { get; init; }
    [Key(2)] public EquipmentSnapshotDto Equipment { get; init; } = new();
    [Key(3)] public long Timestamp { get; init; }
}
```

---

### InspectTalents (3303)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

#### Beschreibung

Detaillierter Talent-Snapshot des inspizierten Spielers.

#### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.InspectTalents` | Ja |
| CharacterId | Guid | Character-ID | Ja |
| Talents | TalentSnapshotDto | Talent-Snapshot | Ja |
| Timestamp | long | Server-Timestamp | Ja |

#### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.InspectTalents)]
public class InspectTalents : IServerMessage, ITimestampedMessage
{
    [Key(0)] public MessageType Type => MessageType.InspectTalents;
    [Key(1)] public Guid CharacterId { get; init; }
    [Key(2)] public TalentSnapshotDto Talents { get; init; } = new();
    [Key(3)] public long Timestamp { get; init; }
}
```

---

### InspectAchievements (3304)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

#### Beschreibung

Achievement-Snapshot des inspizierten Spielers.

#### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.InspectAchievements` | Ja |
| CharacterId | Guid | Character-ID | Ja |
| Achievements | AchievementSnapshotDto | Achievement-Snapshot | Ja |
| Timestamp | long | Server-Timestamp | Ja |

#### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.InspectAchievements)]
public class InspectAchievements : IServerMessage, ITimestampedMessage
{
    [Key(0)] public MessageType Type => MessageType.InspectAchievements;
    [Key(1)] public Guid CharacterId { get; init; }
    [Key(2)] public AchievementSnapshotDto Achievements { get; init; } = new();
    [Key(3)] public long Timestamp { get; init; }
}
```

---

### InspectPvp (3305)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

#### Beschreibung

PvP-Statistiken des inspizierten Spielers.

#### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.InspectPvp` | Ja |
| CharacterId | Guid | Character-ID | Ja |
| PvpStats | PvpSnapshotDto | PvP-Snapshot | Ja |
| Timestamp | long | Server-Timestamp | Ja |

#### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.InspectPvp)]
public class InspectPvp : IServerMessage, ITimestampedMessage
{
    [Key(0)] public MessageType Type => MessageType.InspectPvp;
    [Key(1)] public Guid CharacterId { get; init; }
    [Key(2)] public PvpSnapshotDto PvpStats { get; init; } = new();
    [Key(3)] public long Timestamp { get; init; }
}
```

---

### InspectGuild (3306)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

#### Beschreibung

Guild-Information des inspizierten Spielers.

#### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.InspectGuild` | Ja |
| CharacterId | Guid | Character-ID | Ja |
| Guild | GuildSnapshotDto? | Guild-Snapshot (null wenn keine Guild) | Nein |
| Timestamp | long | Server-Timestamp | Ja |

#### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.InspectGuild)]
public class InspectGuild : IServerMessage, ITimestampedMessage
{
    [Key(0)] public MessageType Type => MessageType.InspectGuild;
    [Key(1)] public Guid CharacterId { get; init; }
    [Key(2)] public GuildSnapshotDto? Guild { get; init; }
    [Key(3)] public long Timestamp { get; init; }
}
```

---

### ArmoryRequest (3310)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

#### Beschreibung

Fordert detaillierte Armory-Daten an (umfassender als Inspect).

#### Im Scope ✅

- Vollständige Character-Daten
- Alle Equipment-Details
- Alle Achievements
- Progression-Daten

#### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.ArmoryRequest` | Ja |
| CharacterName | string | Character-Name (für Suche) | Nein* |
| CharacterId | Guid? | Character-ID (direkt) | Nein* |

*Entweder CharacterName ODER CharacterId muss gesetzt sein.

#### Erwartete Response

- `ArmoryResponse` (3311)

#### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.ArmoryRequest)]
public class ArmoryRequest : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.ArmoryRequest;
    [Key(1)] public string? CharacterName { get; set; }
    [Key(2)] public Guid? CharacterId { get; set; }
}
```

---

### ArmoryResponse (3311)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

#### Beschreibung

Vollständige Armory-Daten als Antwort auf `ArmoryRequest`.

#### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.ArmoryResponse` | Ja |
| Success | bool | Request erfolgreich? | Ja |
| GlobalError | GlobalErrorCode | Globaler Fehlercode | Ja |
| ErrorCode | ArmoryResponseErrorCode? | Spezifischer Fehlercode | Nein |
| ErrorMessage | string? | Fehlermeldung | Nein |
| Profile | InspectProfileDto? | Profil-Daten | Bei Erfolg |
| Equipment | EquipmentSnapshotDto? | Equipment | Bei Erfolg |
| Talents | TalentSnapshotDto? | Talente | Bei Erfolg |
| Achievements | AchievementSnapshotDto? | Achievements | Bei Erfolg |
| PvpStats | PvpSnapshotDto? | PvP-Stats | Bei Erfolg |
| Statistics | StatsSnapshotDto? | Stats | Bei Erfolg |
| Timestamp | long | Server-Timestamp | Ja |

#### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.ArmoryResponse)]
public class ArmoryResponse : IResponseMessage<ArmoryResponseErrorCode>, ITimestampedMessage
{
    [Key(0)] public MessageType Type => MessageType.ArmoryResponse;
    [Key(1)] public bool Success { get; init; }
    [Key(2)] public GlobalErrorCode GlobalError { get; init; }
    [Key(3)] public ArmoryResponseErrorCode? ErrorCode { get; init; }
    [Key(4)] public string? ErrorMessage { get; init; }
    [Key(5)] public InspectProfileDto? Profile { get; init; }
    [Key(6)] public EquipmentSnapshotDto? Equipment { get; init; }
    [Key(7)] public TalentSnapshotDto? Talents { get; init; }
    [Key(8)] public AchievementSnapshotDto? Achievements { get; init; }
    [Key(9)] public PvpSnapshotDto? PvpStats { get; init; }
    [Key(10)] public StatsSnapshotDto? Statistics { get; init; }
    [Key(11)] public long Timestamp { get; init; }
}
```

---

### GearScoreCalculate (3320)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

#### Beschreibung

Client fordert GearScore-Berechnung für eigenen Character an.

#### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.GearScoreCalculate` | Ja |
| CharacterId | Guid? | Character-ID (optional, default: eigener) | Nein |

#### Erwartete Response

- `GearScoreCalculateResponse` (3321)

#### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.GearScoreCalculate)]
public class GearScoreCalculate : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.GearScoreCalculate;
    [Key(1)] public Guid? CharacterId { get; set; }
}
```

---

### GearScoreCalculateResponse (3321)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

#### Beschreibung

Antwort auf `GearScoreCalculate` mit berechnetem GearScore.

#### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.GearScoreCalculateResponse` | Ja |
| Success | bool | Berechnung erfolgreich? | Ja |
| GlobalError | GlobalErrorCode | Globaler Fehlercode | Ja |
| ErrorCode | GearScoreCalculateResponseErrorCode? | Spezifischer Fehlercode | Nein |
| ErrorMessage | string? | Fehlermeldung | Nein |
| CharacterId | Guid | Character-ID | Bei Erfolg |
| GearScore | int | Berechneter GearScore | Bei Erfolg |
| AverageItemLevel | int | Durchschnittliches Item-Level | Bei Erfolg |
| Timestamp | long | Server-Timestamp | Ja |

#### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.GearScoreCalculateResponse)]
public class GearScoreCalculateResponse : IResponseMessage<GearScoreCalculateResponseErrorCode>, ITimestampedMessage
{
    [Key(0)] public MessageType Type => MessageType.GearScoreCalculateResponse;
    [Key(1)] public bool Success { get; init; }
    [Key(2)] public GlobalErrorCode GlobalError { get; init; }
    [Key(3)] public GearScoreCalculateResponseErrorCode? ErrorCode { get; init; }
    [Key(4)] public string? ErrorMessage { get; init; }
    [Key(5)] public Guid CharacterId { get; init; }
    [Key(6)] public int GearScore { get; init; }
    [Key(7)] public int AverageItemLevel { get; init; }
    [Key(8)] public long Timestamp { get; init; }
}
```

---

### GearScoreUpdate (3322)

**Richtung:** 📥 Server → Client  
**Frequenz:** Mittel  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

#### Beschreibung

Server-initiiertes Update wenn sich der GearScore des eigenen Characters ändert.

#### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.GearScoreUpdate` | Ja |
| GearScore | int | Neuer GearScore | Ja |
| AverageItemLevel | int | Neues durchschnittliches Item-Level | Ja |
| Timestamp | long | Server-Timestamp | Ja |

#### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.GearScoreUpdate)]
public class GearScoreUpdate : IServerMessage, ITimestampedMessage
{
    [Key(0)] public MessageType Type => MessageType.GearScoreUpdate;
    [Key(1)] public int GearScore { get; init; }
    [Key(2)] public int AverageItemLevel { get; init; }
    [Key(3)] public long Timestamp { get; init; }
}
```

---

### ItemLevelUpdate (3323)

**Richtung:** 📥 Server → Client  
**Frequenz:** Mittel  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

#### Beschreibung

Server-initiiertes Update wenn sich das Item-Level eines Slots ändert.

#### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.ItemLevelUpdate` | Ja |
| SlotId | EquipmentSlot | Betroffener Slot | Ja |
| ItemLevel | int | Neues Item-Level | Ja |
| NewAverageItemLevel | int | Neues Durchschnitts-IL | Ja |
| Timestamp | long | Server-Timestamp | Ja |

#### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.ItemLevelUpdate)]
public class ItemLevelUpdate : IServerMessage, ITimestampedMessage
{
    [Key(0)] public MessageType Type => MessageType.ItemLevelUpdate;
    [Key(1)] public EquipmentSlot SlotId { get; init; }
    [Key(2)] public int ItemLevel { get; init; }
    [Key(3)] public int NewAverageItemLevel { get; init; }
    [Key(4)] public long Timestamp { get; init; }
}
```

---

### StatisticsRequest (3330)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

#### Beschreibung

Client fordert Statistiken eines Characters an.

#### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.StatisticsRequest` | Ja |
| CharacterId | Guid? | Character-ID (optional, default: eigener) | Nein |
| StatTypes | List\<StatType\>? | Spezifische Stats (optional, default: alle) | Nein |

#### Erwartete Response

- `StatisticsResponse` (3331)

#### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.StatisticsRequest)]
public class StatisticsRequest : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.StatisticsRequest;
    [Key(1)] public Guid? CharacterId { get; set; }
    [Key(2)] public List<StatType>? StatTypes { get; set; }
}
```

---

### StatisticsResponse (3331)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

#### Beschreibung

Antwort auf `StatisticsRequest` mit Statistik-Daten.

#### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.StatisticsResponse` | Ja |
| Success | bool | Request erfolgreich? | Ja |
| GlobalError | GlobalErrorCode | Globaler Fehlercode | Ja |
| ErrorCode | StatisticsResponseErrorCode? | Spezifischer Fehlercode | Nein |
| ErrorMessage | string? | Fehlermeldung | Nein |
| CharacterId | Guid | Character-ID | Bei Erfolg |
| Statistics | StatsSnapshotDto? | Stats-Snapshot | Bei Erfolg |
| Timestamp | long | Server-Timestamp | Ja |

#### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.StatisticsResponse)]
public class StatisticsResponse : IResponseMessage<StatisticsResponseErrorCode>, ITimestampedMessage
{
    [Key(0)] public MessageType Type => MessageType.StatisticsResponse;
    [Key(1)] public bool Success { get; init; }
    [Key(2)] public GlobalErrorCode GlobalError { get; init; }
    [Key(3)] public StatisticsResponseErrorCode? ErrorCode { get; init; }
    [Key(4)] public string? ErrorMessage { get; init; }
    [Key(5)] public Guid CharacterId { get; init; }
    [Key(6)] public StatsSnapshotDto? Statistics { get; init; }
    [Key(7)] public long Timestamp { get; init; }
}
```

---

### StatisticsUpdate (3332)

**Richtung:** 📥 Server → Client  
**Frequenz:** Mittel  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

#### Beschreibung

Server-initiiertes Update wenn sich Stats des eigenen Characters ändern.

#### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.StatisticsUpdate` | Ja |
| ChangedStats | Dictionary\<StatType, int\> | Geänderte Stats | Ja |
| Timestamp | long | Server-Timestamp | Ja |

#### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.StatisticsUpdate)]
public class StatisticsUpdate : IServerMessage, ITimestampedMessage
{
    [Key(0)] public MessageType Type => MessageType.StatisticsUpdate;
    [Key(1)] public Dictionary<StatType, int> ChangedStats { get; init; } = new();
    [Key(2)] public long Timestamp { get; init; }
}
```

---

### PlayedTimeRequest (3340)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

#### Beschreibung

Client fordert Spielzeit-Statistiken an.

#### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.PlayedTimeRequest` | Ja |

#### Erwartete Response

- `PlayedTimeResponse` (3341)

#### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.PlayedTimeRequest)]
public class PlayedTimeRequest : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.PlayedTimeRequest;
}
```

---

### PlayedTimeResponse (3341)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

#### Beschreibung

Antwort auf `PlayedTimeRequest` mit Spielzeit-Daten.

#### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.PlayedTimeResponse` | Ja |
| TotalPlayedSeconds | long | Gesamte Spielzeit in Sekunden | Ja |
| LevelPlayedSeconds | long | Spielzeit auf aktuellem Level | Ja |
| SessionPlayedSeconds | long | Aktuelle Session-Zeit | Ja |
| Timestamp | long | Server-Timestamp | Ja |

#### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.PlayedTimeResponse)]
public class PlayedTimeResponse : IServerMessage, ITimestampedMessage
{
    [Key(0)] public MessageType Type => MessageType.PlayedTimeResponse;
    [Key(1)] public long TotalPlayedSeconds { get; init; }
    [Key(2)] public long LevelPlayedSeconds { get; init; }
    [Key(3)] public long SessionPlayedSeconds { get; init; }
    [Key(4)] public long Timestamp { get; init; }
}
```

---

## 🧨 Edge Cases & Fehlerfälle

### Target loggt aus während Request

```
Client                         Server
  │  InspectRequest              │
  │─────────────────────────────►│
  │                              │  [Target logged out during processing]
  │  InspectResponse             │
  │  Success: true               │  ← Letzte bekannte Daten
  │  OnlineState: Offline        │
  │◄─────────────────────────────│
```

**Verhalten:** Server sendet letzte bekannte Daten mit `OnlineState: Offline`.

### Target wechselt Equipment zwischen Requests

```
Request 1: Revision 100
Request 2 (cached): Client nutzt Revision 100
Request 3: Client sendet Revision 100
Server: Revision ist jetzt 105 (Equipment changed)
Response: Full Snapshot mit Revision 105
```

**Verhalten:** Server erkennt Revision-Mismatch und sendet neuen Full-Snapshot.

### Request-Spam (Rate Limited)

```
Client                         Server
  │  InspectRequest #1           │
  │─────────────────────────────►│ ✅
  │  InspectRequest #2           │
  │─────────────────────────────►│ ✅
  │  ...                         │
  │  InspectRequest #31          │
  │─────────────────────────────►│
  │  InspectResponse             │
  │  GlobalError: RateLimited    │
  │◄─────────────────────────────│
```

**Verhalten:** Nach 30 Requests/Minute wird `RateLimited` zurückgegeben.

### Self-Inspect

```
Client                         Server
  │  InspectRequest              │
  │  TargetId = OwnCharacterId   │
  │─────────────────────────────►│
  │                              │  [Optimized Path - no privacy check]
  │  InspectResponse             │
  │  Full data (no restrictions) │
  │◄─────────────────────────────│
```

**Verhalten:** Self-Inspect ist immer erlaubt und bypassed Privacy-Checks.

### Cross-Zone Inspect

**Verhalten:** Erlaubt. Server hat Zugriff auf alle Character-Daten unabhängig von Zone.

### Hidden Equipment Mode

Wenn `ShowEquipment = false` in Privacy-Settings:

```csharp
var response = new InspectResponse
{
    Success = true,
    Profile = profileData,
    Equipment = null,  // Hidden
    Talents = talentData
};
```

**Verhalten:** `Equipment` ist `null` in Response, Rest normal.

### Desync: Client nutzt stale Revision

```
Client sendet: LastKnownRevision = 500
Server hat: Revision = 100 (rebuild after restart)

Server: Revision < Client's → sendet Full Snapshot
```

**Verhalten:** Server sendet immer Full-Snapshot wenn Client-Revision > Server-Revision.

---

## 📎 Anhang

### MessageType Enum Updates

Die folgenden Änderungen wurden am `MessageType` Enum vorgenommen:

```csharp
// INSPECTION / CHARACTER INFO (3300-3399)
InspectRequest = 3300,
InspectResponse = 3301,
InspectEquipment = 3302,
InspectTalents = 3303,
InspectAchievements = 3304,
InspectPvp = 3305,
InspectGuild = 3306,
ArmoryRequest = 3310,
ArmoryResponse = 3311,
GearScoreCalculate = 3320,
GearScoreCalculateResponse = 3321,  // NEU - Response für GearScoreCalculate
GearScoreUpdate = 3322,             // Verschoben von 3321
ItemLevelUpdate = 3323,             // Verschoben von 3322
StatisticsRequest = 3330,
StatisticsResponse = 3331,
StatisticsUpdate = 3332,
PlayedTimeRequest = 3340,
PlayedTimeResponse = 3341,
```

### Integrationshinweise

#### Social/Block Integration

- `BlockListResponse` (2124) prüfen vor Inspect
- Bei Block: generischer `TargetUnavailable` Error

#### Guild Integration

- `GuildRosterResponse` (811) für Guild-Member-Check
- Privacy-Setting `Guild` erlaubt Guild-Mitgliedern Inspect

#### Inventory/Equipment Integration

- `EquipmentSync` (3904) triggert Revision-Bump
- `EquipItem` (3900) triggert Revision-Bump

#### Cosmetics Integration

- `TransmogId` in `EquipmentSlotData` für visuelle Overrides
- Zeigt Transmog statt tatsächlichem Item-Look

#### Zone/World Integration

- `ZoneName` in Profile nur wenn `ShowLocation = true`
- Cross-Zone Inspect funktioniert

### Request→Response Übersicht

| Request | ID | Response | ID |
|---------|-----|----------|-----|
| InspectRequest | 3300 | InspectResponse | 3301 |
| ArmoryRequest | 3310 | ArmoryResponse | 3311 |
| GearScoreCalculate | 3320 | GearScoreCalculateResponse | 3321 |
| StatisticsRequest | 3330 | StatisticsResponse | 3331 |
| PlayedTimeRequest | 3340 | PlayedTimeResponse | 3341 |

### Server-Events (kein Request nötig)

| Event | ID | Trigger |
|-------|-----|---------|
| InspectEquipment | 3302 | Nach InspectRequest (zusätzliche Details) |
| InspectTalents | 3303 | Nach InspectRequest (zusätzliche Details) |
| InspectAchievements | 3304 | Nach InspectRequest (zusätzliche Details) |
| InspectPvp | 3305 | Nach InspectRequest (zusätzliche Details) |
| InspectGuild | 3306 | Nach InspectRequest (zusätzliche Details) |
| GearScoreUpdate | 3322 | Equipment-Änderung |
| ItemLevelUpdate | 3323 | Item-Level-Änderung |
| StatisticsUpdate | 3332 | Stat-Änderung |

---

**Letzte Aktualisierung:** 2026-01-03  
**Version:** 2.1.0

[← Zurück zur Übersicht](Message-Reference.md)

Source: docs/03-messages/33-inspection.md
