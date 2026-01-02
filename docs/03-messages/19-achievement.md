# 🏆 Achievement / Title Messages (1900-1999)

**Kategorie:** 19  
**Range:** 1900-1999 (aktiv: 1900-1924)  
**Phase:** Prototyp  
**Status:** 🟢 In Entwicklung

[← Zurück zur Übersicht](README.md)

---

## 📋 Inhaltsverzeichnis

- [🔄 Achievement-Flow](#-achievement-flow)
  - [Server-Authoritative Architektur](#server-authoritative-architektur)
  - [Achievement-Unlock Flow](#achievement-unlock-flow)
  - [Title-Select Flow](#title-select-flow)
- [🧱 DTOs / Enums](#-dtos--enums)
  - [AchievementCategory (enum)](#achievementcategory-enum)
  - [AchievementState (enum)](#achievementstate-enum)
  - [TitleRarity (enum)](#titlerarity-enum)
  - [AchievementErrorCode (enum)](#achievementerrorcode-enum)
  - [AchievementDto](#achievementdto)
  - [AchievementCriteriaDto](#achievementcriteriadto)
  - [TitleDto](#titledto)
  - [Wichtige Konstanten](#wichtige-konstanten)
- [📩 Messages](#-messages)
  - [AchievementUnlocked (1900)](#achievementunlocked-1900)
  - [AchievementProgress (1901)](#achievementprogress-1901)
  - [AchievementListRequest (1902)](#achievementlistrequest-1902)
  - [AchievementListResponse (1903)](#achievementlistresponse-1903)
  - [AchievementCriteriaUpdate (1904)](#achievementcriteriaupdate-1904)
  - [AchievementPointsUpdate (1905)](#achievementpointsupdate-1905)
  - [AchievementToast (1906)](#achievementtoast-1906)
  - [AchievementLink (1907)](#achievementlink-1907)
  - [AchievementCompare (1908)](#achievementcompare-1908)
  - [AchievementCompareResult (1909)](#achievementcompareresult-1909)
  - [TitleUnlock (1920)](#titleunlock-1920)
  - [TitleSelectMsg (1921)](#titleselectmsg-1921)
  - [TitleClear (1922)](#titleclear-1922)
  - [TitleListRequest (1923)](#titlelistrequest-1923)
  - [TitleListResponse (1924)](#titlelistresponse-1924)
- [📎 Anhang](#-anhang)
  - [MessageType Enum (Auszug)](#messagetype-enum-auszug)
  - [Request/Response Paare](#requestresponse-paare)
  - [Datei-Struktur](#datei-struktur)

---

## 🔄 Achievement-Flow

### Server-Authoritative Architektur

Das Achievement-System ist vollständig **server-authoritative**:

1. **Server tracked alle Fortschritte** – Client kann keine Achievements selbst freischalten
2. **Criteria-Updates werden gepusht** – Server sendet Fortschritts-Updates bei relevanten Events
3. **Unlock-Events werden broadcast** – Andere Spieler können Achievement-Unlocks sehen
4. **Points werden serverseitig berechnet** – Achievement-Punkte sind manipulationssicher

```
┌─────────────────────────────────────────────────────────────────┐
│                    Achievement-System Architektur               │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  Client                    Server                    Database   │
│    │                         │                           │      │
│    │ [Game Event]            │                           │      │
│    │────────────────────────►│ Update Criteria          │      │
│    │                         │──────────────────────────►│      │
│    │                         │                           │      │
│    │ AchievementCriteria-    │ Check Completion         │      │
│    │ Update (1904)           │◄──────────────────────────│      │
│    │◄────────────────────────│                           │      │
│    │                         │                           │      │
│    │ [If Complete]           │                           │      │
│    │ AchievementUnlocked     │ Mark Complete            │      │
│    │ (1900)                  │──────────────────────────►│      │
│    │◄────────────────────────│                           │      │
│    │                         │                           │      │
│    │ AchievementPoints-      │                           │      │
│    │ Update (1905)           │                           │      │
│    │◄────────────────────────│                           │      │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

### Achievement-Unlock Flow

```
Client                         Server                    Other Clients
  │                              │                            │
  │  [Kill Boss Event]           │                            │
  │─────────────────────────────►│                            │
  │                              │                            │
  │                              │ Check Achievement Criteria │
  │                              │ "Boss Slayer" (100 Bosses) │
  │                              │                            │
  │  AchievementCriteriaUpdate   │                            │
  │  (1904) Progress: 100/100    │                            │
  │◄─────────────────────────────│                            │
  │                              │                            │
  │  AchievementUnlocked (1900)  │                            │
  │  "Boss Slayer" +10 Points    │                            │
  │◄─────────────────────────────│                            │
  │                              │                            │
  │  AchievementPointsUpdate     │                            │
  │  (1905) Total: 1250          │                            │
  │◄─────────────────────────────│                            │
  │                              │                            │
  │  AchievementToast (1906)     │ AchievementToast (1906)    │
  │  [Show Toast UI]             │ "[Player] earned Boss      │
  │◄─────────────────────────────│  Slayer"                   │
  │                              │────────────────────────────►│
  │                              │                            │
```

### Title-Select Flow

```
Client                         Server
  │                              │
  │  TitleSelectMsg (1921)       │
  │  TitleId: 42                 │
  │─────────────────────────────►│
  │                              │
  │                              │ Validate:
  │                              │ - Title unlocked?
  │                              │ - Title not expired?
  │                              │
  │  [Success]                   │
  │  CharacterUpdate Broadcast   │
  │  (TitleId changed)           │
  │◄─────────────────────────────│
  │                              │
  │  [Error - Not Unlocked]      │
  │  ErrorMessage (910)          │
  │  Code: TITLE_NOT_UNLOCKED    │
  │◄─────────────────────────────│
```

---

## 🧱 DTOs / Enums

### AchievementCategory (enum)

```csharp
public enum AchievementCategory : byte
{
    General = 0,        // Allgemeine Achievements
    Combat = 1,         // Kampf-bezogene
    Exploration = 2,    // Erkundung
    Social = 3,         // Soziale Interaktionen
    Crafting = 4,       // Handwerk
    Quests = 5,         // Quest-bezogene
    PvP = 6,            // PvP-Achievements
    Dungeons = 7,       // Dungeon/Raid
    Collection = 8,     // Sammel-Achievements
    Seasonal = 9,       // Saisonale Events
    Legacy = 10,        // Nicht mehr erreichbar
    Secret = 11         // Versteckte Achievements
}
```

### AchievementState (enum)

```csharp
public enum AchievementState : byte
{
    Locked = 0,         // Nicht sichtbar (Kriterien versteckt)
    InProgress = 1,     // Sichtbar, Fortschritt läuft
    Completed = 2,      // Abgeschlossen
    Claimed = 3         // Belohnung abgeholt (falls vorhanden)
}
```

### TitleRarity (enum)

```csharp
public enum TitleRarity : byte
{
    Common = 0,         // Weiß - Leicht erhältlich
    Uncommon = 1,       // Grün - Etwas Aufwand
    Rare = 2,           // Blau - Signifikanter Aufwand
    Epic = 3,           // Lila - Schwer zu erlangen
    Legendary = 4,      // Orange - Sehr schwer
    Unique = 5          // Gold - Einzigartig/Server-First
}
```

### AchievementErrorCode (enum)

```csharp
public enum AchievementErrorCode : byte
{
    None = 0,
    AchievementNotFound = 1,
    AchievementAlreadyCompleted = 2,
    CriteriaNotMet = 3,
    TitleNotUnlocked = 4,
    TitleExpired = 5,
    PlayerNotFound = 6,
    CompareNotAllowed = 7,     // Privatsphäre-Einstellung
    InvalidAchievementId = 8,
    InvalidTitleId = 9,
    TooManyRequests = 10,      // Rate-Limit
    InternalError = 255
}
```

### AchievementDto

```csharp
[MessagePackObject]
public class AchievementDto
{
    [Key(0)] public int AchievementId { get; set; }
    [Key(1)] public string Name { get; set; }
    [Key(2)] public string Description { get; set; }
    [Key(3)] public AchievementCategory Category { get; set; }
    [Key(4)] public AchievementState State { get; set; }
    [Key(5)] public int Points { get; set; }
    [Key(6)] public string IconId { get; set; }
    [Key(7)] public long? CompletedAt { get; set; }      // Unix timestamp
    [Key(8)] public List<AchievementCriteriaDto> Criteria { get; set; }
    [Key(9)] public int? RewardTitleId { get; set; }     // Falls Title als Belohnung
    [Key(10)] public int? RewardItemId { get; set; }     // Falls Item als Belohnung
}
```

### AchievementCriteriaDto

```csharp
[MessagePackObject]
public class AchievementCriteriaDto
{
    [Key(0)] public int CriteriaId { get; set; }
    [Key(1)] public string Description { get; set; }
    [Key(2)] public int CurrentProgress { get; set; }
    [Key(3)] public int RequiredProgress { get; set; }
    [Key(4)] public bool IsComplete { get; set; }
}
```

### TitleDto

```csharp
[MessagePackObject]
public class TitleDto
{
    [Key(0)] public int TitleId { get; set; }
    [Key(1)] public string Prefix { get; set; }          // Vor dem Namen (z.B. "Sir")
    [Key(2)] public string Suffix { get; set; }          // Nach dem Namen (z.B. "the Brave")
    [Key(3)] public TitleRarity Rarity { get; set; }
    [Key(4)] public string Source { get; set; }          // Wie erhältlich
    [Key(5)] public bool IsUnlocked { get; set; }
    [Key(6)] public long? UnlockedAt { get; set; }       // Unix timestamp
    [Key(7)] public long? ExpiresAt { get; set; }        // Für temporäre Titel
}
```

### Wichtige Konstanten

| Konstante | Wert | Beschreibung |
|-----------|------|--------------|
| MAX_ACHIEVEMENT_POINTS | 99999 | Maximale Achievement-Punkte |
| MAX_CRITERIA_PER_ACHIEVEMENT | 10 | Max Kriterien pro Achievement |
| MAX_ACHIEVEMENTS_PER_CATEGORY | 100 | Max Achievements pro Kategorie |
| MAX_TITLES | 500 | Maximale Anzahl Titel |
| TOAST_DISPLAY_TIME_MS | 5000 | Toast-Anzeige Dauer |
| COMPARE_COOLDOWN_SECONDS | 30 | Cooldown zwischen Vergleichen |
| ACHIEVEMENT_LIST_PAGE_SIZE | 50 | Achievements pro Seite |

---

## 📩 Messages

### AchievementUnlocked (1900)

**Richtung:** 📡 Server → Client (Broadcast)  
**Frequenz:** Selten (bei Achievement-Abschluss)  
**Authentifizierung:** 🔒 Ja

#### Beschreibung
Server informiert Client und optional andere Spieler über freigeschaltetes Achievement.

#### Im Scope ✅
- Achievement-ID und Name
- Erzielte Punkte
- Optionale Belohnung (Title/Item)
- Broadcast an Nearby-Spieler (konfigurierbar)

#### Nicht im Scope ❌
- Achievement-Details → verwende `AchievementListResponse` (1903)

#### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `AchievementUnlocked` (1900) | Ja |
| CharacterId | int | Spieler der Achievement erreicht hat | Ja |
| CharacterName | string | Name für Broadcast | Ja |
| AchievementId | int | ID des Achievements | Ja |
| AchievementName | string | Name für Toast | Ja |
| Points | int | Erzielte Punkte | Ja |
| TotalPoints | int | Neue Gesamtpunktzahl | Ja |
| RewardTitleId | int? | Freigeschalteter Titel | Nein |
| RewardItemId | int? | Belohnungs-Item | Nein |
| Timestamp | long | Unix timestamp | Ja |

#### Erwartete Response
- Keine (Server-Push)

#### Beispiel Payload
```csharp
var msg = new AchievementUnlockedMessage
{
    Type = MessageType.AchievementUnlocked,
    CharacterId = 12345,
    CharacterName = "Dragonslayer",
    AchievementId = 1001,
    AchievementName = "Dragon Slayer",
    Points = 25,
    TotalPoints = 1250,
    RewardTitleId = 42,
    Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
};
```

---

### AchievementProgress (1901)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig (bei Fortschritts-Events)  
**Authentifizierung:** 🔒 Ja

#### Beschreibung
Server sendet Achievement-Fortschritts-Update. Wird gesendet wenn sich der Fortschritt eines verfolgten Achievements ändert.

#### Im Scope ✅
- Fortschritts-Update für einzelnes Achievement
- Prozentuale Vervollständigung

#### Nicht im Scope ❌
- Detaillierte Kriterien → verwende `AchievementCriteriaUpdate` (1904)

#### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `AchievementProgress` (1901) | Ja |
| AchievementId | int | Achievement-ID | Ja |
| CurrentProgress | int | Aktueller Fortschritt | Ja |
| RequiredProgress | int | Erforderlicher Fortschritt | Ja |
| PercentComplete | float | 0.0 - 1.0 | Ja |

#### Erwartete Response
- Keine (Server-Push)

#### Beispiel Payload
```csharp
var msg = new AchievementProgressMessage
{
    Type = MessageType.AchievementProgress,
    AchievementId = 1001,
    CurrentProgress = 45,
    RequiredProgress = 100,
    PercentComplete = 0.45f
};
```

---

### AchievementListRequest (1902)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten (UI-Öffnung)  
**Authentifizierung:** 🔒 Ja

#### Beschreibung
Client fordert Achievement-Liste an. Kann nach Kategorie gefiltert werden.

#### Im Scope ✅
- Paginierte Achievement-Liste
- Kategorie-Filter
- Nur eigene Achievements

#### Nicht im Scope ❌
- Anderer Spieler Achievements → verwende `AchievementCompare` (1908)

#### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `AchievementListRequest` (1902) | Ja |
| RequestId | int | Korrelations-ID | Ja |
| Category | AchievementCategory? | Filter nach Kategorie | Nein |
| IncludeCompleted | bool | Abgeschlossene inkludieren | Ja |
| Page | int | Seiten-Nummer (0-basiert) | Ja |

#### Erwartete Response
- `AchievementListResponse` (1903)

#### Beispiel Payload
```csharp
var msg = new AchievementListRequestMessage
{
    Type = MessageType.AchievementListRequest,
    RequestId = 12345,
    Category = AchievementCategory.Combat,
    IncludeCompleted = true,
    Page = 0
};
```

---

### AchievementListResponse (1903)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

#### Beschreibung
Server sendet Achievement-Liste als Antwort auf Request.

#### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `AchievementListResponse` (1903) | Ja |
| RequestId | int | Korrelations-ID | Ja |
| Achievements | List\<AchievementDto\> | Achievement-Liste | Ja |
| TotalCount | int | Gesamtzahl (für Pagination) | Ja |
| Page | int | Aktuelle Seite | Ja |
| TotalPages | int | Gesamtzahl Seiten | Ja |
| TotalPoints | int | Spieler-Punktestand | Ja |

#### Erwartete Response
- Keine (ist Response)

#### Beispiel Payload
```csharp
var msg = new AchievementListResponseMessage
{
    Type = MessageType.AchievementListResponse,
    RequestId = 12345,
    Achievements = new List<AchievementDto> { /* ... */ },
    TotalCount = 127,
    Page = 0,
    TotalPages = 3,
    TotalPoints = 1250
};
```

---

### AchievementCriteriaUpdate (1904)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja

#### Beschreibung
Server sendet detailliertes Update zu einem Achievement-Kriterium.

#### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `AchievementCriteriaUpdate` (1904) | Ja |
| AchievementId | int | Achievement-ID | Ja |
| CriteriaId | int | Kriterium-ID | Ja |
| CurrentProgress | int | Aktueller Fortschritt | Ja |
| RequiredProgress | int | Erforderlich | Ja |
| IsComplete | bool | Kriterium erfüllt? | Ja |

#### Erwartete Response
- Keine (Server-Push)

---

### AchievementPointsUpdate (1905)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

#### Beschreibung
Server sendet neue Achievement-Punkte-Summe nach Unlock.

#### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `AchievementPointsUpdate` (1905) | Ja |
| TotalPoints | int | Neue Gesamtpunktzahl | Ja |
| PointsGained | int | Punkte aus letztem Achievement | Ja |

#### Erwartete Response
- Keine (Server-Push)

---

### AchievementToast (1906)

**Richtung:** 📡 Server → Client (Broadcast)  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

#### Beschreibung
Server sendet Toast-Notification für Achievement-Unlock an nahestehende Spieler.

#### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `AchievementToast` (1906) | Ja |
| CharacterId | int | Spieler-ID | Ja |
| CharacterName | string | Spieler-Name | Ja |
| AchievementId | int | Achievement-ID | Ja |
| AchievementName | string | Achievement-Name | Ja |
| Rarity | byte | 0-5 für Farbe | Ja |
| IconId | string | Icon für Toast | Ja |

#### Erwartete Response
- Keine (Broadcast)

---

### AchievementLink (1907)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

#### Beschreibung
Client linkt Achievement im Chat. Server validiert und generiert Link-Daten.

#### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `AchievementLink` (1907) | Ja |
| AchievementId | int | Achievement zum Linken | Ja |
| ChatChannel | int | Ziel-Chat-Kanal | Ja |

#### Erwartete Response
- `ChatMessage` (403) mit eingebettetem Achievement-Link
- Oder `ErrorMessage` (910) bei Fehler

---

### AchievementCompare (1908)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

#### Beschreibung
Client vergleicht eigene Achievements mit anderem Spieler.

#### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `AchievementCompare` (1908) | Ja |
| RequestId | int | Korrelations-ID | Ja |
| TargetCharacterId | int | Ziel-Spieler ID | Ja |
| Category | AchievementCategory? | Filter | Nein |

#### Erwartete Response
- `AchievementCompareResult` (1909)

---

### AchievementCompareResult (1909)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

#### Beschreibung
Server sendet Vergleichsergebnis zwischen zwei Spielern.

#### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `AchievementCompareResult` (1909) | Ja |
| RequestId | int | Korrelations-ID | Ja |
| Success | bool | Vergleich möglich? | Ja |
| ErrorCode | AchievementErrorCode | Bei Fehler | Nein |
| MyPoints | int | Eigene Punkte | Bei Erfolg |
| TheirPoints | int | Andere Punkte | Bei Erfolg |
| MyCompleted | int | Eigene abgeschlossen | Bei Erfolg |
| TheirCompleted | int | Andere abgeschlossen | Bei Erfolg |
| OnlyMine | List\<int\> | Nur ich habe | Bei Erfolg |
| OnlyTheirs | List\<int\> | Nur andere hat | Bei Erfolg |
| Both | List\<int\> | Beide haben | Bei Erfolg |

#### Erwartete Response
- Keine (ist Response)

---

### TitleUnlock (1920)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

#### Beschreibung
Server informiert Client über freigeschalteten neuen Titel.

#### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `TitleUnlock` (1920) | Ja |
| TitleId | int | Freigeschalteter Titel | Ja |
| Prefix | string | Titel-Präfix | Nein |
| Suffix | string | Titel-Suffix | Nein |
| Rarity | TitleRarity | Seltenheit | Ja |
| Source | string | Achievement-Name o.ä. | Ja |
| ExpiresAt | long? | Ablauf für temp. Titel | Nein |

#### Erwartete Response
- Keine (Server-Push)

---

### TitleSelectMsg (1921)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

#### Beschreibung
Client wählt einen freigeschalteten Titel aus.

#### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `TitleSelectMsg` (1921) | Ja |
| TitleId | int | Gewählter Titel | Ja |

#### Erwartete Response
- `CharacterUpdate` Broadcast bei Erfolg (enthält neuen TitleId)
- `ErrorMessage` (910) bei Fehler

#### Validierung (Server)
- Titel muss freigeschaltet sein
- Titel darf nicht abgelaufen sein

---

### TitleClear (1922)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

#### Beschreibung
Client entfernt aktuellen Titel (zeigt keinen Titel mehr).

#### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `TitleClear` (1922) | Ja |

#### Erwartete Response
- `CharacterUpdate` Broadcast (TitleId = null)

---

### TitleListRequest (1923)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

#### Beschreibung
Client fordert Liste aller verfügbaren/freigeschalteten Titel an.

#### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `TitleListRequest` (1923) | Ja |
| RequestId | int | Korrelations-ID | Ja |
| IncludeLocked | bool | Auch gesperrte zeigen? | Ja |

#### Erwartete Response
- `TitleListResponse` (1924)

---

### TitleListResponse (1924)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

#### Beschreibung
Server sendet Titel-Liste als Antwort auf Request.

#### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `TitleListResponse` (1924) | Ja |
| RequestId | int | Korrelations-ID | Ja |
| Titles | List\<TitleDto\> | Titel-Liste | Ja |
| CurrentTitleId | int? | Aktuell aktiver Titel | Nein |
| TotalUnlocked | int | Anzahl freigeschaltet | Ja |

#### Erwartete Response
- Keine (ist Response)

---

## 📎 Anhang

### MessageType Enum (Auszug)

```csharp
// ACHIEVEMENTS / TITLES (1900-1999)
AchievementUnlocked = 1900,
AchievementProgress = 1901,
AchievementListRequest = 1902,
AchievementListResponse = 1903,
AchievementCriteriaUpdate = 1904,
AchievementPointsUpdate = 1905,
AchievementToast = 1906,
AchievementLink = 1907,
AchievementCompare = 1908,
AchievementCompareResult = 1909,
TitleUnlock = 1920,
TitleSelectMsg = 1921,
TitleClear = 1922,
TitleListRequest = 1923,
TitleListResponse = 1924,
```

### Request/Response Paare

| Request | ID | Response | ID | Beschreibung |
|---------|----|---------|----|--------------|
| `AchievementListRequest` | 1902 | `AchievementListResponse` | 1903 | Achievement-Liste abrufen |
| `AchievementCompare` | 1908 | `AchievementCompareResult` | 1909 | Achievements vergleichen |
| `TitleListRequest` | 1923 | `TitleListResponse` | 1924 | Titel-Liste abrufen |

### Fire-and-Forget Messages

| Message | ID | Beschreibung |
|---------|-----|--------------|
| `AchievementLink` | 1907 | Achievement in Chat linken |
| `TitleSelectMsg` | 1921 | Titel auswählen |
| `TitleClear` | 1922 | Titel entfernen |

### Server-Initiated Messages (Push)

| Message | ID | Trigger |
|---------|-----|---------|
| `AchievementUnlocked` | 1900 | Achievement abgeschlossen |
| `AchievementProgress` | 1901 | Fortschritt geändert |
| `AchievementCriteriaUpdate` | 1904 | Kriterium aktualisiert |
| `AchievementPointsUpdate` | 1905 | Punkte geändert |
| `AchievementToast` | 1906 | Achievement freigeschaltet (Broadcast) |
| `TitleUnlock` | 1920 | Neuer Titel freigeschaltet |

### Datei-Struktur

```
shared/Mmo.Shared/
├── Messaging/
│   ├── Enums/
│   │   └── MessageType.cs              # Achievement messages 1900-1924
│   └── Messages/
│       └── Achievement/
│           ├── AchievementUnlockedMessage.cs
│           ├── AchievementProgressMessage.cs
│           ├── AchievementListRequestMessage.cs
│           ├── AchievementListResponseMessage.cs
│           ├── AchievementCriteriaUpdateMessage.cs
│           ├── AchievementPointsUpdateMessage.cs
│           ├── AchievementToastMessage.cs
│           ├── AchievementLinkMessage.cs
│           ├── AchievementCompareMessage.cs
│           ├── AchievementCompareResultMessage.cs
│           ├── TitleUnlockMessage.cs
│           ├── TitleSelectMsgMessage.cs
│           ├── TitleClearMessage.cs
│           ├── TitleListRequestMessage.cs
│           └── TitleListResponseMessage.cs
└── DTOs/
    └── Achievement/
        ├── AchievementDto.cs
        ├── AchievementCriteriaDto.cs
        └── TitleDto.cs
```

---

**Letzte Aktualisierung**: 2026-01-02  
**Version**: 3.0.0  
**Status**: ✅ Aligned mit MessageType Enum (15 Messages: 1900-1909, 1920-1924)

[← Zurück zur Übersicht](README.md)
