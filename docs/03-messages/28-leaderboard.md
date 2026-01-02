# 📊 Leaderboard Messages (2800-2899)

**Kategorie:** 28  
**Range:** 2800-2899  
**Phase:** Prototyp  
**Status:** 🟢 In Entwicklung

[← Zurück zur Übersicht](README.md)

---

## 📋 Inhaltsverzeichnis

- [Überblick](#-überblick)
- [Datenmodell](#-datenmodell)
- [Query & Pagination](#-query--pagination)
- [Update Pipeline & Refresh Cadence](#-update-pipeline--refresh-cadence)
- [DTOs / Interfaces](#-dtos--interfaces)
- [Enums / ErrorCodes / Flags](#-enums--errorcodes--flags)
- [Regeln & Sicherheit](#-regeln--sicherheit)
- [Aktive Messages 2800-2899](#-aktive-messages-2800-2899)
  - [LeaderboardRequest (2800)](#leaderboardrequest-2800)
  - [LeaderboardResponse (2801)](#leaderboardresponse-2801)
  - [LeaderboardUpdate (2802)](#leaderboardupdate-2802)
  - [RankingPersonal (2803)](#rankingpersonal-2803)
  - [RankingGuild (2804)](#rankingguild-2804)
  - [PvpRatingRequest (2810)](#pvpratingrequest-2810)
  - [PvpRatingResponse (2811)](#pvpratingresponse-2811)
  - [PvpSeasonInfo (2812)](#pvpseasoninfo-2812)
  - [PvpSeasonEnd (2813)](#pvpseasonend-2813)
  - [PvpSeasonReward (2814)](#pvpseasonreward-2814)
  - [MythicRankingRequest (2820)](#mythicrankingrequest-2820)
  - [MythicRankingResponse (2821)](#mythicrankingresponse-2821)
  - [RaidProgressRankingRequest (2822)](#raidprogressrankingrequest-2822)
  - [RaidProgressRankingResponse (2823)](#raidprogressrankingresponse-2823)
  - [AchievementRankingRequest (2824)](#achievementrankingrequest-2824)
  - [AchievementRankingResponse (2825)](#achievementrankingresponse-2825)
- [Obsolete Messages](#-obsolete-messages)
- [Edge Cases & Fehlerfälle](#-edge-cases--fehlerfälle)
- [Anhang](#-anhang)

---

## 📋 Überblick

Diese Kategorie umfasst alle Messages für **Leaderboards und Rankings** im 2DMMO. Leaderboards sind **read-heavy** Systeme mit hohem Caching-Bedarf.

### Scope

- Allgemeine Leaderboards (Level, Gold, Kills, etc.)
- PvP-Ratings und Seasons
- Mythic+ Dungeon Rankings
- Raid-Progress Rankings
- Achievement-Punkte Rankings
- Guild-Rankings
- Persönliche Ranking-Abfragen

### Architektur-Prinzipien

| Aspekt | Strategie |
|--------|-----------|
| **Read/Write Ratio** | 99:1 (extrem read-heavy) |
| **Caching** | Server-seitiger Cache mit TTL |
| **Pagination** | Cursor-basiert (opaque token) |
| **Privacy** | Nur CharacterId + DisplayName, keine AccountIds |
| **Updates** | Asynchron, gebatched, throttled |

---

## 🧠 Datenmodell

### LeaderboardType

Verschiedene Leaderboard-Kategorien:

```csharp
public enum LeaderboardType : byte
{
    Level = 1,
    AchievementPoints = 2,
    HonorableKills = 3,
    TotalGold = 4,
    QuestsCompleted = 5,
    DungeonsCleared = 6,
    RaidsCleared = 7,
    PvpRating2v2 = 10,
    PvpRating3v3 = 11,
    PvpRatingRbg = 12,
    MythicPlusScore = 20,
    RaidProgress = 30,
    GuildLevel = 40,
    GuildAchievements = 41
}
```

### LeaderboardEntry

Ein Eintrag im Leaderboard:

```csharp
[MessagePackObject]
public class LeaderboardEntry
{
    [Key(0)] public int Rank { get; set; }
    [Key(1)] public Guid CharacterId { get; set; }
    [Key(2)] public string DisplayName { get; set; } = "";
    [Key(3)] public long Score { get; set; }
    [Key(4)] public byte? ClassId { get; set; }
    [Key(5)] public byte? FactionId { get; set; }
    [Key(6)] public Guid? GuildId { get; set; }
    [Key(7)] public string? GuildName { get; set; }
    [Key(8)] public long UpdatedAt { get; set; }
}
```

### SeasonInfo

PvP-Season Informationen:

```csharp
[MessagePackObject]
public class SeasonInfo
{
    [Key(0)] public int SeasonId { get; set; }
    [Key(1)] public string SeasonName { get; set; } = "";
    [Key(2)] public long StartTime { get; set; }
    [Key(3)] public long EndTime { get; set; }
    [Key(4)] public bool IsActive { get; set; }
}
```

---

## 🔎 Query & Pagination

### Cursor-basierte Pagination

Alle Leaderboard-Abfragen nutzen **opaque cursor tokens** statt Offset-basierter Pagination:

```csharp
[MessagePackObject]
public class PaginationRequest
{
    [Key(0)] public string? Cursor { get; set; }      // null = erste Seite
    [Key(1)] public int PageSize { get; set; } = 25;  // max 100
}

[MessagePackObject]
public class PaginationResponse
{
    [Key(0)] public string? NextCursor { get; set; }  // null = letzte Seite
    [Key(1)] public string? PrevCursor { get; set; }
    [Key(2)] public int TotalCount { get; set; }
    [Key(3)] public bool HasMore { get; set; }
}
```

### Query-Modi

| Modus | Beschreibung | Parameter |
|-------|--------------|-----------|
| **Top** | Top N Einträge | `PageSize` |
| **AroundMe** | Eigene Position ± N | `CharacterId`, `Range` |
| **ByGuild** | Nur Guild-Mitglieder | `GuildId` |
| **ByFaction** | Nur eine Fraktion | `FactionId` |
| **ByClass** | Nur eine Klasse | `ClassId` |

### Deterministische Tie-Breaker

Bei gleichem Score wird sortiert nach:
1. `Score` DESC
2. `UpdatedAt` ASC (früher erreicht = besser)
3. `CharacterId` ASC (deterministisch)

---

## 🔄 Update Pipeline & Refresh Cadence

### Server-seitige Updates

| Event | Update-Strategie |
|-------|------------------|
| Level Up | Sofort in Queue |
| PvP Match Ende | Sofort in Queue |
| Achievement Unlock | Sofort in Queue |
| Dungeon Clear | Sofort in Queue |

### Refresh-Intervalle

| Leaderboard-Typ | Cache TTL | Rebuild-Intervall |
|-----------------|-----------|-------------------|
| Level | 5 Minuten | 1 Minute |
| PvP Rating | 1 Minute | 30 Sekunden |
| Mythic+ | 5 Minuten | 1 Minute |
| Achievement | 15 Minuten | 5 Minuten |
| Guild | 30 Minuten | 10 Minuten |

### Response-Metadaten

Jede Response enthält:

```csharp
[Key(X)] public long ServerGeneratedAt { get; set; }  // Timestamp
[Key(Y)] public int CacheTtlMs { get; set; }          // Verbleibende TTL
```

---

## 🧱 DTOs / Interfaces

### LeaderboardQueryDto

```csharp
[MessagePackObject]
public class LeaderboardQueryDto
{
    [Key(0)] public LeaderboardType BoardType { get; set; }
    [Key(1)] public LeaderboardQueryMode QueryMode { get; set; }
    [Key(2)] public string? Cursor { get; set; }
    [Key(3)] public int PageSize { get; set; } = 25;
    [Key(4)] public Guid? TargetCharacterId { get; set; }
    [Key(5)] public Guid? GuildId { get; set; }
    [Key(6)] public byte? FactionFilter { get; set; }
    [Key(7)] public byte? ClassFilter { get; set; }
    [Key(8)] public int? SeasonId { get; set; }
}
```

### LeaderboardResultDto

```csharp
[MessagePackObject]
public class LeaderboardResultDto
{
    [Key(0)] public LeaderboardType BoardType { get; set; }
    [Key(1)] public List<LeaderboardEntry> Entries { get; set; } = new();
    [Key(2)] public PaginationResponse Pagination { get; set; } = new();
    [Key(3)] public LeaderboardEntry? MyEntry { get; set; }
    [Key(4)] public long ServerGeneratedAt { get; set; }
    [Key(5)] public int CacheTtlMs { get; set; }
}
```

---

## 🧩 Enums / ErrorCodes / Flags

### LeaderboardQueryMode

```csharp
public enum LeaderboardQueryMode : byte
{
    Top = 1,
    AroundMe = 2,
    ByGuild = 3,
    ByFaction = 4,
    ByClass = 5,
    Friends = 6
}
```

### LeaderboardErrorCode

```csharp
public enum LeaderboardErrorCode : byte
{
    None = 0,
    InvalidBoardType = 1,
    InvalidPageSize = 2,
    InvalidCursor = 3,
    CharacterNotFound = 4,
    GuildNotFound = 5,
    SeasonNotFound = 6,
    NotInSeason = 7,
    LeaderboardUnavailable = 8,
    TooManyRequests = 9
}
```

### PvpBracket

```csharp
public enum PvpBracket : byte
{
    Arena2v2 = 1,
    Arena3v3 = 2,
    RatedBattleground = 3,
    Skirmish = 4
}
```

---

## ⚙️ Regeln & Sicherheit

### Privacy

- **Keine AccountIds** in Leaderboards
- Nur `CharacterId` + `DisplayName` (Snapshot zum Zeitpunkt des Updates)
- Optionales Opt-out für Leaderboard-Sichtbarkeit (Account-Setting)

### Rate Limits

| Request-Typ | Limit | Cooldown |
|-------------|-------|----------|
| LeaderboardRequest | 10/min | 6 Sekunden |
| PvpRatingRequest | 20/min | 3 Sekunden |
| MythicRankingRequest | 10/min | 6 Sekunden |
| RaidProgressRankingRequest | 5/min | 12 Sekunden |
| AchievementRankingRequest | 10/min | 6 Sekunden |

### Tamper Resistance

- Server ist **authoritative** für alle Scores
- Client kann nur abfragen, nicht modifizieren
- Score-Updates nur durch Server-Events (Level Up, Match Ende, etc.)
- Historische Scores werden archiviert (Audit Trail)

---

## 📩 Aktive Messages 2800-2899

---

## LeaderboardRequest (2800)

**Richtung:** 📤 Client → Server  
**Frequenz:** Mittel  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Client fordert ein allgemeines Leaderboard an. Unterstützt verschiedene Board-Typen und Query-Modi mit Cursor-basierter Pagination.

### Im Scope ✅

- Allgemeine Leaderboards (Level, Gold, Kills, etc.)
- Cursor-basierte Pagination
- Filter nach Fraktion/Klasse/Guild
- "Around Me" Abfragen

### Nicht im Scope ❌

- PvP-spezifische Rankings → verwende `PvpRatingRequest` (2810)
- Mythic+ Rankings → verwende `MythicRankingRequest` (2820)
- Raid Progress → verwende `RaidProgressRankingRequest` (2822)

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.LeaderboardRequest` | Ja |
| Query | LeaderboardQueryDto | Query-Parameter | Ja |

### Erwartete Response

- **Bei Erfolg:** `LeaderboardResponse` (2801) mit Entries
- **Bei Fehler:** `LeaderboardResponse` (2801) mit ErrorCode

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.LeaderboardRequest)]
public class LeaderboardRequest : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.LeaderboardRequest;
    [Key(1)] public LeaderboardQueryDto Query { get; set; } = new();
}
```

### Server-Verhalten

1. Validiere Query-Parameter (BoardType, PageSize, Cursor)
2. Prüfe Rate Limit
3. Lade aus Cache oder generiere neu
4. Füge `MyEntry` hinzu wenn Spieler im Board
5. Sende Response mit Pagination-Info

### Client-Verhalten

1. Zeige Loading-Indikator
2. Bei Erfolg: Rendere Leaderboard-UI
3. Bei Fehler: Zeige Fehlermeldung
4. Cache Response lokal (CacheTtlMs)

### Flow-Diagramm

```
Client                    Server                    Cache/DB
  │                          │                          │
  │  LeaderboardRequest      │                          │
  │─────────────────────────►│                          │
  │                          │  Check Rate Limit        │
  │                          │  Check Cache             │
  │                          │─────────────────────────►│
  │                          │                          │
  │                          │  Cached/Fresh Data       │
  │                          │◄─────────────────────────│
  │  LeaderboardResponse     │                          │
  │◄─────────────────────────│                          │
```

### Beispiel Payloads

```csharp
// Top 25 Level-Leaderboard
var topRequest = new LeaderboardRequest
{
    Query = new LeaderboardQueryDto
    {
        BoardType = LeaderboardType.Level,
        QueryMode = LeaderboardQueryMode.Top,
        PageSize = 25
    }
};

// Around Me mit Klassen-Filter
var aroundMeRequest = new LeaderboardRequest
{
    Query = new LeaderboardQueryDto
    {
        BoardType = LeaderboardType.AchievementPoints,
        QueryMode = LeaderboardQueryMode.AroundMe,
        TargetCharacterId = myCharacterId,
        ClassFilter = 2, // Warrior
        PageSize = 10
    }
};
```

### Error Codes

| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `InvalidBoardType` | Ungültiger LeaderboardType | Korrigiere BoardType |
| `InvalidPageSize` | PageSize < 1 oder > 100 | Korrigiere PageSize |
| `InvalidCursor` | Cursor abgelaufen/ungültig | Starte ohne Cursor |
| `TooManyRequests` | Rate Limit erreicht | Warte und retry |

### Verwandte Messages

| Message | ID | Beziehung |
|---------|-----|-----------|
| `LeaderboardResponse` | 2801 | Response zu diesem Request |
| `LeaderboardUpdate` | 2802 | Push-Update bei Änderungen |
| `RankingPersonal` | 2803 | Eigene Platzierung |

### Notizen

- PageSize: min 1, max 100, default 25
- Cursor ist **opaque** - Client darf nicht parsen
- Cache TTL wird in Response mitgeliefert

---

## LeaderboardResponse (2801)

**Richtung:** 📥 Server → Client  
**Frequenz:** Mittel  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung

Antwort auf `LeaderboardRequest`. Enthält die angeforderten Leaderboard-Einträge mit Pagination-Informationen.

### Im Scope ✅

- Liste von Leaderboard-Einträgen
- Pagination-Informationen (Cursor)
- Eigene Platzierung (MyEntry)
- Cache-Metadaten

### Nicht im Scope ❌

- Live-Updates → verwende `LeaderboardUpdate` (2802)

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.LeaderboardResponse` | Ja |
| Success | bool | Request erfolgreich? | Ja |
| GlobalError | GlobalErrorCode | Globaler Fehler | Ja |
| ErrorCode | LeaderboardErrorCode? | Spezifischer Fehler | Nein |
| ErrorMessage | string? | Fehlerbeschreibung | Nein |
| Result | LeaderboardResultDto? | Ergebnis-Daten | Bei Erfolg |

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.LeaderboardResponse)]
public class LeaderboardResponse : IResponseMessage<LeaderboardErrorCode>
{
    [Key(0)] public MessageType Type => MessageType.LeaderboardResponse;
    [Key(1)] public bool Success { get; init; }
    [Key(2)] public GlobalErrorCode GlobalError { get; init; }
    [Key(3)] public LeaderboardErrorCode? ErrorCode { get; init; }
    [Key(4)] public string? ErrorMessage { get; init; }
    [Key(5)] public LeaderboardResultDto? Result { get; init; }
}
```

### Beispiel Payloads

```csharp
// Erfolgreiche Response
var successResponse = new LeaderboardResponse
{
    Success = true,
    GlobalError = GlobalErrorCode.None,
    Result = new LeaderboardResultDto
    {
        BoardType = LeaderboardType.Level,
        Entries = new List<LeaderboardEntry>
        {
            new() { Rank = 1, CharacterId = guid1, DisplayName = "TopPlayer", Score = 60 },
            new() { Rank = 2, CharacterId = guid2, DisplayName = "SecondBest", Score = 59 },
            // ...
        },
        Pagination = new PaginationResponse
        {
            NextCursor = "eyJvZmZzZXQiOjI1fQ==",
            TotalCount = 1000,
            HasMore = true
        },
        MyEntry = new LeaderboardEntry { Rank = 42, CharacterId = myGuid, DisplayName = "Me", Score = 45 },
        ServerGeneratedAt = 1735850000000,
        CacheTtlMs = 300000
    }
};

// Fehler-Response
var errorResponse = new LeaderboardResponse
{
    Success = false,
    GlobalError = GlobalErrorCode.None,
    ErrorCode = LeaderboardErrorCode.TooManyRequests,
    ErrorMessage = "Rate limit exceeded. Try again in 6 seconds."
};
```

### Verwandte Messages

| Message | ID | Beziehung |
|---------|-----|-----------|
| `LeaderboardRequest` | 2800 | Request zu dieser Response |

---

## LeaderboardUpdate (2802)

**Richtung:** 📥 Server → Client (Push)  
**Frequenz:** Selten (throttled)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Server pusht Leaderboard-Updates an Clients, die ein bestimmtes Leaderboard abonniert haben. Updates werden **gebatched und throttled** um Bandbreite zu sparen.

### Im Scope ✅

- Inkrementelle Updates für abonnierte Leaderboards
- Rank-Änderungen im sichtbaren Bereich
- Eigene Rank-Änderungen

### Nicht im Scope ❌

- Vollständige Leaderboard-Daten → verwende `LeaderboardResponse` (2801)

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.LeaderboardUpdate` | Ja |
| BoardType | LeaderboardType | Welches Leaderboard | Ja |
| UpdatedEntries | List\<LeaderboardEntry\> | Geänderte Einträge | Ja |
| RemovedRanks | List\<int\>? | Entfernte Ränge | Nein |
| MyNewRank | int? | Eigener neuer Rang | Nein |
| Timestamp | long | Server-Timestamp | Ja |

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.LeaderboardUpdate)]
public class LeaderboardUpdate : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.LeaderboardUpdate;
    [Key(1)] public LeaderboardType BoardType { get; set; }
    [Key(2)] public List<LeaderboardEntry> UpdatedEntries { get; set; } = new();
    [Key(3)] public List<int>? RemovedRanks { get; set; }
    [Key(4)] public int? MyNewRank { get; set; }
    [Key(5)] public long Timestamp { get; set; }
}
```

### Server-Verhalten

1. Sammle Änderungen in Buffer (max 5 Sekunden)
2. Batch alle Änderungen zusammen
3. Sende nur an Clients mit aktivem Leaderboard-View
4. Throttle: max 1 Update/10 Sekunden pro Client

### Client-Verhalten

1. Merge UpdatedEntries in lokalen Cache
2. Entferne RemovedRanks
3. Aktualisiere UI mit Animation
4. Highlighte eigene Rang-Änderung

### Notizen

- Updates werden nur gesendet wenn Client Leaderboard-UI offen hat
- Client muss Subscription durch `LeaderboardRequest` aktivieren
- Subscription läuft nach 5 Minuten Inaktivität ab

---

## RankingPersonal (2803)

**Richtung:** 📥 Server → Client (Push)  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Server informiert Client über eigene Platzierungsänderungen in relevanten Leaderboards. Wird automatisch gesendet bei signifikanten Rang-Änderungen.

### Im Scope ✅

- Eigene Rang-Änderungen
- Neue persönliche Bestleistungen
- Milestone-Benachrichtigungen (Top 100, Top 10, etc.)

### Nicht im Scope ❌

- Vollständige Leaderboard-Abfrage → verwende `LeaderboardRequest` (2800)

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.RankingPersonal` | Ja |
| BoardType | LeaderboardType | Welches Leaderboard | Ja |
| OldRank | int? | Vorheriger Rang (null wenn neu) | Nein |
| NewRank | int | Neuer Rang | Ja |
| Score | long | Aktueller Score | Ja |
| IsMilestone | bool | Top 100/10/1 erreicht? | Ja |
| MilestoneType | MilestoneType? | Art des Milestones | Nein |
| Timestamp | long | Server-Timestamp | Ja |

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.RankingPersonal)]
public class RankingPersonal : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.RankingPersonal;
    [Key(1)] public LeaderboardType BoardType { get; set; }
    [Key(2)] public int? OldRank { get; set; }
    [Key(3)] public int NewRank { get; set; }
    [Key(4)] public long Score { get; set; }
    [Key(5)] public bool IsMilestone { get; set; }
    [Key(6)] public MilestoneType? MilestoneType { get; set; }
    [Key(7)] public long Timestamp { get; set; }
}

public enum MilestoneType : byte
{
    Top1000 = 1,
    Top500 = 2,
    Top100 = 3,
    Top50 = 4,
    Top10 = 5,
    Top3 = 6,
    Top1 = 7
}
```

### Beispiel Payload

```csharp
var milestone = new RankingPersonal
{
    BoardType = LeaderboardType.Level,
    OldRank = 101,
    NewRank = 99,
    Score = 60,
    IsMilestone = true,
    MilestoneType = MilestoneType.Top100,
    Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
};
```

### Client-Verhalten

1. Zeige Toast-Notification
2. Bei Milestone: Spezielle Animation/Sound
3. Aktualisiere lokalen Cache

---

## RankingGuild (2804)

**Richtung:** 📥 Server → Client (Push)  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Server informiert Guild-Mitglieder über Guild-Ranking-Änderungen.

### Im Scope ✅

- Guild-Rang-Änderungen
- Guild-Milestones

### Nicht im Scope ❌

- Individuelle Rankings → verwende `RankingPersonal` (2803)

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.RankingGuild` | Ja |
| GuildId | Guid | Guild-ID | Ja |
| GuildName | string | Guild-Name | Ja |
| BoardType | LeaderboardType | Welches Leaderboard | Ja |
| OldRank | int? | Vorheriger Rang | Nein |
| NewRank | int | Neuer Rang | Ja |
| Score | long | Aktueller Score | Ja |
| Timestamp | long | Server-Timestamp | Ja |

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.RankingGuild)]
public class RankingGuild : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.RankingGuild;
    [Key(1)] public Guid GuildId { get; set; }
    [Key(2)] public string GuildName { get; set; } = "";
    [Key(3)] public LeaderboardType BoardType { get; set; }
    [Key(4)] public int? OldRank { get; set; }
    [Key(5)] public int NewRank { get; set; }
    [Key(6)] public long Score { get; set; }
    [Key(7)] public long Timestamp { get; set; }
}
```

---

## PvpRatingRequest (2810)

**Richtung:** 📤 Client → Server  
**Frequenz:** Mittel  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Client fordert PvP-Rating-Leaderboard für eine bestimmte Bracket und Season an.

### Im Scope ✅

- Arena 2v2/3v3 Rankings
- Rated Battleground Rankings
- Season-spezifische Abfragen
- Historical Rankings (vergangene Seasons)

### Nicht im Scope ❌

- Allgemeine Leaderboards → verwende `LeaderboardRequest` (2800)

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.PvpRatingRequest` | Ja |
| Bracket | PvpBracket | Arena/RBG Bracket | Ja |
| SeasonId | int? | Season (null = aktuelle) | Nein |
| QueryMode | LeaderboardQueryMode | Top/AroundMe/etc. | Ja |
| Cursor | string? | Pagination Cursor | Nein |
| PageSize | int | Einträge pro Seite | Ja |

### Erwartete Response

- `PvpRatingResponse` (2811)

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.PvpRatingRequest)]
public class PvpRatingRequest : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.PvpRatingRequest;
    [Key(1)] public PvpBracket Bracket { get; set; }
    [Key(2)] public int? SeasonId { get; set; }
    [Key(3)] public LeaderboardQueryMode QueryMode { get; set; }
    [Key(4)] public string? Cursor { get; set; }
    [Key(5)] public int PageSize { get; set; } = 25;
}
```

### Beispiel Payload

```csharp
var pvpRequest = new PvpRatingRequest
{
    Bracket = PvpBracket.Arena3v3,
    SeasonId = null, // aktuelle Season
    QueryMode = LeaderboardQueryMode.Top,
    PageSize = 50
};
```

### Error Codes

| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `SeasonNotFound` | Season existiert nicht | Korrigiere SeasonId |
| `NotInSeason` | Keine aktive Season | Warte auf Season-Start |

---

## PvpRatingResponse (2811)

**Richtung:** 📥 Server → Client  
**Frequenz:** Mittel  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung

Antwort auf `PvpRatingRequest` mit PvP-Leaderboard-Daten.

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.PvpRatingResponse` | Ja |
| Success | bool | Request erfolgreich? | Ja |
| GlobalError | GlobalErrorCode | Globaler Fehler | Ja |
| ErrorCode | LeaderboardErrorCode? | Spezifischer Fehler | Nein |
| ErrorMessage | string? | Fehlerbeschreibung | Nein |
| Bracket | PvpBracket | Angefragte Bracket | Bei Erfolg |
| Season | SeasonInfo? | Season-Info | Bei Erfolg |
| Entries | List\<PvpLeaderboardEntry\>? | Einträge | Bei Erfolg |
| Pagination | PaginationResponse? | Pagination | Bei Erfolg |
| MyEntry | PvpLeaderboardEntry? | Eigener Eintrag | Nein |
| ServerGeneratedAt | long | Timestamp | Ja |
| CacheTtlMs | int | Cache TTL | Ja |

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.PvpRatingResponse)]
public class PvpRatingResponse : IResponseMessage<LeaderboardErrorCode>
{
    [Key(0)] public MessageType Type => MessageType.PvpRatingResponse;
    [Key(1)] public bool Success { get; init; }
    [Key(2)] public GlobalErrorCode GlobalError { get; init; }
    [Key(3)] public LeaderboardErrorCode? ErrorCode { get; init; }
    [Key(4)] public string? ErrorMessage { get; init; }
    [Key(5)] public PvpBracket Bracket { get; init; }
    [Key(6)] public SeasonInfo? Season { get; init; }
    [Key(7)] public List<PvpLeaderboardEntry>? Entries { get; init; }
    [Key(8)] public PaginationResponse? Pagination { get; init; }
    [Key(9)] public PvpLeaderboardEntry? MyEntry { get; init; }
    [Key(10)] public long ServerGeneratedAt { get; init; }
    [Key(11)] public int CacheTtlMs { get; init; }
}

[MessagePackObject]
public class PvpLeaderboardEntry
{
    [Key(0)] public int Rank { get; set; }
    [Key(1)] public Guid CharacterId { get; set; }
    [Key(2)] public string DisplayName { get; set; } = "";
    [Key(3)] public int Rating { get; set; }
    [Key(4)] public int Wins { get; set; }
    [Key(5)] public int Losses { get; set; }
    [Key(6)] public byte ClassId { get; set; }
    [Key(7)] public byte SpecId { get; set; }
    [Key(8)] public byte FactionId { get; set; }
}
```

---

## PvpSeasonInfo (2812)

**Richtung:** 📥 Server → Client (Push)  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Server informiert über aktuelle/kommende PvP-Season-Details. Wird bei Login und Season-Änderungen gesendet.

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.PvpSeasonInfo` | Ja |
| CurrentSeason | SeasonInfo? | Aktuelle Season | Nein |
| NextSeason | SeasonInfo? | Nächste Season | Nein |
| MyRatings | Dictionary\<PvpBracket, int\>? | Eigene Ratings | Nein |

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.PvpSeasonInfo)]
public class PvpSeasonInfo : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.PvpSeasonInfo;
    [Key(1)] public SeasonInfo? CurrentSeason { get; set; }
    [Key(2)] public SeasonInfo? NextSeason { get; set; }
    [Key(3)] public Dictionary<PvpBracket, int>? MyRatings { get; set; }
}
```

---

## PvpSeasonEnd (2813)

**Richtung:** 📥 Server → Client (Broadcast)  
**Frequenz:** Sehr selten (einmal pro Season)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Server informiert alle Spieler über das Ende einer PvP-Season.

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.PvpSeasonEnd` | Ja |
| EndedSeason | SeasonInfo | Beendete Season | Ja |
| NextSeason | SeasonInfo? | Nächste Season | Nein |
| FinalRank | int? | Finaler Rang des Spielers | Nein |
| RewardsEarned | bool | Hat Spieler Rewards verdient? | Ja |

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.PvpSeasonEnd)]
public class PvpSeasonEnd : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.PvpSeasonEnd;
    [Key(1)] public SeasonInfo EndedSeason { get; set; } = new();
    [Key(2)] public SeasonInfo? NextSeason { get; set; }
    [Key(3)] public int? FinalRank { get; set; }
    [Key(4)] public bool RewardsEarned { get; set; }
}
```

---

## PvpSeasonReward (2814)

**Richtung:** 📥 Server → Client  
**Frequenz:** Sehr selten (einmal pro Season)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Server vergibt Season-Rewards an qualifizierte Spieler.

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.PvpSeasonReward` | Ja |
| SeasonId | int | Season-ID | Ja |
| Bracket | PvpBracket | Bracket für Reward | Ja |
| FinalRank | int | Finaler Rang | Ja |
| FinalRating | int | Finales Rating | Ja |
| Rewards | List\<RewardItem\> | Verdiente Rewards | Ja |
| TitleUnlocked | string? | Freigeschalteter Titel | Nein |
| MountUnlocked | int? | Freigeschaltetes Mount | Nein |

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.PvpSeasonReward)]
public class PvpSeasonReward : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.PvpSeasonReward;
    [Key(1)] public int SeasonId { get; set; }
    [Key(2)] public PvpBracket Bracket { get; set; }
    [Key(3)] public int FinalRank { get; set; }
    [Key(4)] public int FinalRating { get; set; }
    [Key(5)] public List<RewardItem> Rewards { get; set; } = new();
    [Key(6)] public string? TitleUnlocked { get; set; }
    [Key(7)] public int? MountUnlocked { get; set; }
}

[MessagePackObject]
public class RewardItem
{
    [Key(0)] public int ItemId { get; set; }
    [Key(1)] public int Quantity { get; set; }
}
```

---

## MythicRankingRequest (2820)

**Richtung:** 📤 Client → Server  
**Frequenz:** Mittel  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Client fordert Mythic+ Dungeon-Ranking an.

### Im Scope ✅

- Mythic+ Score Rankings
- Dungeon-spezifische Rankings
- Affix-Wochen Rankings

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.MythicRankingRequest` | Ja |
| DungeonId | int? | Spezifischer Dungeon (null = alle) | Nein |
| AffixWeek | int? | Affix-Woche (null = aktuelle) | Nein |
| QueryMode | LeaderboardQueryMode | Top/AroundMe/etc. | Ja |
| Cursor | string? | Pagination Cursor | Nein |
| PageSize | int | Einträge pro Seite | Ja |

### Erwartete Response

- `MythicRankingResponse` (2821)

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.MythicRankingRequest)]
public class MythicRankingRequest : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.MythicRankingRequest;
    [Key(1)] public int? DungeonId { get; set; }
    [Key(2)] public int? AffixWeek { get; set; }
    [Key(3)] public LeaderboardQueryMode QueryMode { get; set; }
    [Key(4)] public string? Cursor { get; set; }
    [Key(5)] public int PageSize { get; set; } = 25;
}
```

---

## MythicRankingResponse (2821)

**Richtung:** 📥 Server → Client  
**Frequenz:** Mittel  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung

Antwort auf `MythicRankingRequest` mit Mythic+ Rankings.

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.MythicRankingResponse` | Ja |
| Success | bool | Request erfolgreich? | Ja |
| GlobalError | GlobalErrorCode | Globaler Fehler | Ja |
| ErrorCode | LeaderboardErrorCode? | Spezifischer Fehler | Nein |
| ErrorMessage | string? | Fehlerbeschreibung | Nein |
| Entries | List\<MythicLeaderboardEntry\>? | Einträge | Bei Erfolg |
| Pagination | PaginationResponse? | Pagination | Bei Erfolg |
| MyEntry | MythicLeaderboardEntry? | Eigener Eintrag | Nein |
| ServerGeneratedAt | long | Timestamp | Ja |
| CacheTtlMs | int | Cache TTL | Ja |

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.MythicRankingResponse)]
public class MythicRankingResponse : IResponseMessage<LeaderboardErrorCode>
{
    [Key(0)] public MessageType Type => MessageType.MythicRankingResponse;
    [Key(1)] public bool Success { get; init; }
    [Key(2)] public GlobalErrorCode GlobalError { get; init; }
    [Key(3)] public LeaderboardErrorCode? ErrorCode { get; init; }
    [Key(4)] public string? ErrorMessage { get; init; }
    [Key(5)] public List<MythicLeaderboardEntry>? Entries { get; init; }
    [Key(6)] public PaginationResponse? Pagination { get; init; }
    [Key(7)] public MythicLeaderboardEntry? MyEntry { get; init; }
    [Key(8)] public long ServerGeneratedAt { get; init; }
    [Key(9)] public int CacheTtlMs { get; init; }
}

[MessagePackObject]
public class MythicLeaderboardEntry
{
    [Key(0)] public int Rank { get; set; }
    [Key(1)] public Guid CharacterId { get; set; }
    [Key(2)] public string DisplayName { get; set; } = "";
    [Key(3)] public int MythicPlusScore { get; set; }
    [Key(4)] public int HighestKeyLevel { get; set; }
    [Key(5)] public int DungeonsCompleted { get; set; }
    [Key(6)] public byte ClassId { get; set; }
    [Key(7)] public byte SpecId { get; set; }
}
```

---

## RaidProgressRankingRequest (2822)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Client fordert Raid-Progress-Ranking an (Guild-basiert).

### Im Scope ✅

- Guild Raid Progress Rankings
- Raid-Tier spezifische Rankings
- Boss-Kill Rankings

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.RaidProgressRankingRequest` | Ja |
| RaidId | int? | Spezifischer Raid (null = aktueller Tier) | Nein |
| Difficulty | RaidDifficulty | Normal/Heroic/Mythic | Ja |
| QueryMode | LeaderboardQueryMode | Top/AroundMe/etc. | Ja |
| Cursor | string? | Pagination Cursor | Nein |
| PageSize | int | Einträge pro Seite | Ja |

### Erwartete Response

- `RaidProgressRankingResponse` (2823)

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.RaidProgressRankingRequest)]
public class RaidProgressRankingRequest : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.RaidProgressRankingRequest;
    [Key(1)] public int? RaidId { get; set; }
    [Key(2)] public RaidDifficulty Difficulty { get; set; }
    [Key(3)] public LeaderboardQueryMode QueryMode { get; set; }
    [Key(4)] public string? Cursor { get; set; }
    [Key(5)] public int PageSize { get; set; } = 25;
}

public enum RaidDifficulty : byte
{
    Normal = 1,
    Heroic = 2,
    Mythic = 3
}
```

---

## RaidProgressRankingResponse (2823)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung

Antwort auf `RaidProgressRankingRequest` mit Raid-Progress-Rankings.

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.RaidProgressRankingResponse` | Ja |
| Success | bool | Request erfolgreich? | Ja |
| GlobalError | GlobalErrorCode | Globaler Fehler | Ja |
| ErrorCode | LeaderboardErrorCode? | Spezifischer Fehler | Nein |
| ErrorMessage | string? | Fehlerbeschreibung | Nein |
| Entries | List\<RaidProgressEntry\>? | Einträge | Bei Erfolg |
| Pagination | PaginationResponse? | Pagination | Bei Erfolg |
| MyGuildEntry | RaidProgressEntry? | Eigene Guild | Nein |
| ServerGeneratedAt | long | Timestamp | Ja |
| CacheTtlMs | int | Cache TTL | Ja |

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.RaidProgressRankingResponse)]
public class RaidProgressRankingResponse : IResponseMessage<LeaderboardErrorCode>
{
    [Key(0)] public MessageType Type => MessageType.RaidProgressRankingResponse;
    [Key(1)] public bool Success { get; init; }
    [Key(2)] public GlobalErrorCode GlobalError { get; init; }
    [Key(3)] public LeaderboardErrorCode? ErrorCode { get; init; }
    [Key(4)] public string? ErrorMessage { get; init; }
    [Key(5)] public List<RaidProgressEntry>? Entries { get; init; }
    [Key(6)] public PaginationResponse? Pagination { get; init; }
    [Key(7)] public RaidProgressEntry? MyGuildEntry { get; init; }
    [Key(8)] public long ServerGeneratedAt { get; init; }
    [Key(9)] public int CacheTtlMs { get; init; }
}

[MessagePackObject]
public class RaidProgressEntry
{
    [Key(0)] public int Rank { get; set; }
    [Key(1)] public Guid GuildId { get; set; }
    [Key(2)] public string GuildName { get; set; } = "";
    [Key(3)] public byte FactionId { get; set; }
    [Key(4)] public int BossesDefeated { get; set; }
    [Key(5)] public int TotalBosses { get; set; }
    [Key(6)] public long FirstKillTime { get; set; }
    [Key(7)] public long LastKillTime { get; set; }
}
```

---

## AchievementRankingRequest (2824)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Client fordert Achievement-Punkte-Ranking an.

### Im Scope ✅

- Achievement-Punkte Rankings
- Kategorie-spezifische Rankings

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.AchievementRankingRequest` | Ja |
| CategoryId | int? | Achievement-Kategorie (null = alle) | Nein |
| QueryMode | LeaderboardQueryMode | Top/AroundMe/etc. | Ja |
| Cursor | string? | Pagination Cursor | Nein |
| PageSize | int | Einträge pro Seite | Ja |

### Erwartete Response

- `AchievementRankingResponse` (2825)

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.AchievementRankingRequest)]
public class AchievementRankingRequest : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.AchievementRankingRequest;
    [Key(1)] public int? CategoryId { get; set; }
    [Key(2)] public LeaderboardQueryMode QueryMode { get; set; }
    [Key(3)] public string? Cursor { get; set; }
    [Key(4)] public int PageSize { get; set; } = 25;
}
```

---

## AchievementRankingResponse (2825)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung

Antwort auf `AchievementRankingRequest` mit Achievement-Rankings.

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.AchievementRankingResponse` | Ja |
| Success | bool | Request erfolgreich? | Ja |
| GlobalError | GlobalErrorCode | Globaler Fehler | Ja |
| ErrorCode | LeaderboardErrorCode? | Spezifischer Fehler | Nein |
| ErrorMessage | string? | Fehlerbeschreibung | Nein |
| Entries | List\<AchievementLeaderboardEntry\>? | Einträge | Bei Erfolg |
| Pagination | PaginationResponse? | Pagination | Bei Erfolg |
| MyEntry | AchievementLeaderboardEntry? | Eigener Eintrag | Nein |
| ServerGeneratedAt | long | Timestamp | Ja |
| CacheTtlMs | int | Cache TTL | Ja |

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.AchievementRankingResponse)]
public class AchievementRankingResponse : IResponseMessage<LeaderboardErrorCode>
{
    [Key(0)] public MessageType Type => MessageType.AchievementRankingResponse;
    [Key(1)] public bool Success { get; init; }
    [Key(2)] public GlobalErrorCode GlobalError { get; init; }
    [Key(3)] public LeaderboardErrorCode? ErrorCode { get; init; }
    [Key(4)] public string? ErrorMessage { get; init; }
    [Key(5)] public List<AchievementLeaderboardEntry>? Entries { get; init; }
    [Key(6)] public PaginationResponse? Pagination { get; init; }
    [Key(7)] public AchievementLeaderboardEntry? MyEntry { get; init; }
    [Key(8)] public long ServerGeneratedAt { get; init; }
    [Key(9)] public int CacheTtlMs { get; init; }
}

[MessagePackObject]
public class AchievementLeaderboardEntry
{
    [Key(0)] public int Rank { get; set; }
    [Key(1)] public Guid CharacterId { get; set; }
    [Key(2)] public string DisplayName { get; set; } = "";
    [Key(3)] public int AchievementPoints { get; set; }
    [Key(4)] public int AchievementsCompleted { get; set; }
    [Key(5)] public byte ClassId { get; set; }
    [Key(6)] public byte FactionId { get; set; }
}
```

---

## 🗑️ Obsolete Messages

Keine obsoleten Messages in dieser Kategorie.

---

## 🧨 Edge Cases & Fehlerfälle

### Cursor Expiration

- Cursor sind 5 Minuten gültig
- Bei abgelaufenem Cursor: `InvalidCursor` Error
- Client soll ohne Cursor neu anfragen

### Rate Limiting

- Bei Überschreitung: `TooManyRequests` Error
- Response enthält `Retry-After` in Sekunden
- Client soll exponential backoff implementieren

### Empty Leaderboards

- Neue Season/Woche: Leaderboard kann leer sein
- Response mit `Entries = []` ist valid
- Client zeigt "Keine Einträge" Message

### Character Not Ranked

- `MyEntry = null` wenn Spieler nicht im Leaderboard
- Bei AroundMe-Query ohne eigenen Rank: Top N zurückgeben

### Concurrent Updates

- Leaderboard kann sich während Pagination ändern
- Client muss mit Rank-Lücken/-Duplikaten umgehen
- `ServerGeneratedAt` zeigt Aktualität

---

## 📎 Anhang

### MessageType Enum Updates

Die folgenden Enum-Einträge wurden zu `MessageType.cs` hinzugefügt:

```csharp
// LEADERBOARD / RANKINGS (2800-2899)
LeaderboardRequest = 2800,
LeaderboardResponse = 2801,
LeaderboardUpdate = 2802,
RankingPersonal = 2803,
RankingGuild = 2804,
PvpRatingRequest = 2810,
PvpRatingResponse = 2811,
PvpSeasonInfo = 2812,
PvpSeasonEnd = 2813,
PvpSeasonReward = 2814,
MythicRankingRequest = 2820,
MythicRankingResponse = 2821,
RaidProgressRankingRequest = 2822,   // NEU (vorher: RaidProgressRanking)
RaidProgressRankingResponse = 2823,  // NEU (vorher: AchievementRanking)
AchievementRankingRequest = 2824,    // NEU
AchievementRankingResponse = 2825,   // NEU
```

### Integrationshinweise

1. **Caching**: Implementiere Client-seitigen Cache basierend auf `CacheTtlMs`
2. **Rate Limiting**: Respektiere Server-Limits, implementiere Backoff
3. **Privacy**: Zeige nur `DisplayName`, keine Account-Informationen
4. **Subscriptions**: Leaderboard-Updates nur bei aktivem UI

### Request→Response Mapping

| Request | Response |
|---------|----------|
| LeaderboardRequest (2800) | LeaderboardResponse (2801) |
| PvpRatingRequest (2810) | PvpRatingResponse (2811) |
| MythicRankingRequest (2820) | MythicRankingResponse (2821) |
| RaidProgressRankingRequest (2822) | RaidProgressRankingResponse (2823) |
| AchievementRankingRequest (2824) | AchievementRankingResponse (2825) |

---

**Letzte Aktualisierung**: 2026-01-02  
**Version**: 2.1.0

[← Zurück zur Übersicht](README.md)
