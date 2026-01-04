# ⚔️ PvP Messages (2500-2599)

**Kategorie:** 25  
**Range:** 2500-2599  
**Phase:** Prototyp  
**Status:** 🟢 In Entwicklung

[← Zurück zur Übersicht](Message-Reference.md)

---

## 📋 Inhaltsverzeichnis

- [🔄 PvP Flow](#-pvp-flow)
  - [Server-Authoritative Architektur](#server-authoritative-architektur)
  - [PvP-Flag Flow](#pvp-flag-flow)
  - [Arena Flow](#arena-flow)
  - [Battleground Flow](#battleground-flow)
- [🧱 DTOs & Enums](#-dtos--enums)
  - [PvpRank Enum](#pvprank-enum)
  - [ArenaTeamSize Enum](#arenateamsize-enum)
  - [BattlegroundType Enum](#battlegroundtype-enum)
  - [PvpErrorCode Enum](#pvperrorcode-enum)
  - [ArenaTeamDto](#arenateamdto)
  - [ArenaMatchDto](#arenamatchdto)
  - [BattlegroundInfoDto](#battlegroundinfodto)
  - [PvpPlayerStatsDto](#pvpplayerstatsdto)
  - [Wichtige Konstanten](#wichtige-konstanten)
- [📩 Messages](#-messages)
  - [PvP Grundfunktionen (2500-2507)](#pvp-grundfunktionen-2500-2507)
  - [Arena-Team Verwaltung (2520-2525)](#arena-team-verwaltung-2520-2525)
  - [Arena Queue & Match (2530-2537)](#arena-queue--match-2530-2537)
  - [Battleground Queue & Match (2550-2560)](#battleground-queue--match-2550-2560)
  - [World PvP (2570-2571)](#world-pvp-2570-2571)
- [📎 Anhang](#-anhang)
  - [MessageType Enum](#messagetype-enum)
  - [Request/Response Paare](#requestresponse-paare)
  - [Datei-Struktur](#datei-struktur)

---

## 🔄 PvP Flow

### Server-Authoritative Architektur

```
┌─────────────────────────────────────────────────────────────────┐
│                    SERVER (Zone/Arena/BG)                        │
│                                                                  │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐           │
│  │ PvP-Manager  │  │ Arena-System │  │ BG-System    │           │
│  │              │  │              │  │              │           │
│  │ • Flag-State │  │ • Team-Mgmt  │  │ • Queue      │           │
│  │ • Honor      │  │ • Rating     │  │ • Objectives │           │
│  │ • Rank       │  │ • Matchmaking│  │ • Scores     │           │
│  └──────────────┘  └──────────────┘  └──────────────┘           │
│                                                                  │
│  ALLE PVP-ENTSCHEIDUNGEN WERDEN SERVERSEITIG GETROFFEN          │
│  Client erhält nur Ergebnisse zum Rendern                       │
└─────────────────────────────────────────────────────────────────┘
         │                    │                    │
         ▼                    ▼                    ▼
   ┌─────────┐          ┌─────────┐          ┌─────────┐
   │ Client  │          │ Client  │          │ Client  │
   │ (Render)│          │ (Render)│          │ (Render)│
   └─────────┘          └─────────┘          └─────────┘
```

### PvP-Flag Flow

```
Client                        Zone Server
  │                               │
  │  PvpFlagRequest (2500)        │
  │  {Enable: true}               │
  │──────────────────────────────►│
  │                               │
  │                               │  [Validate: In City? Combat?]
  │                               │
  │  PvpFlagUpdate (2501)         │
  │  {PlayerId, FlagActive: true} │
  │◄──────────────────────────────│
  │                               │
  │      ... Zeit vergeht ...     │
  │                               │
  │  PvpFlagExpiring (2502)       │
  │  {TimeRemaining: 30s}         │
  │◄──────────────────────────────│
```

### Arena Flow

```
Team A Client                Arena Server                Team B Client
  │                               │                              │
  │  ArenaJoinQueue (2530)        │                              │
  │  {TeamSize: 2v2}              │                              │
  │──────────────────────────────►│                              │
  │                               │   ArenaJoinQueue (2530)      │
  │                               │◄──────────────────────────────│
  │                               │                              │
  │  ArenaQueueUpdate (2532)      │   [Matchmaking]              │
  │  {Position: 5}                │                              │
  │◄──────────────────────────────│                              │
  │                               │                              │
  │                               │   [Match gefunden]           │
  │                               │                              │
  │  ArenaMatchFound (2533)       │   ArenaMatchFound (2533)     │
  │  {EnemyTeam: ...}             │   {EnemyTeam: ...}           │
  │◄──────────────────────────────│──────────────────────────────►│
  │                               │                              │
  │  ArenaMatchStart (2534)       │   ArenaMatchStart (2534)     │
  │◄──────────────────────────────│──────────────────────────────►│
  │                               │                              │
  │      ... Kampf ...            │      ... Kampf ...           │
  │                               │                              │
  │  ArenaMatchEnd (2535)         │   ArenaMatchEnd (2535)       │
  │◄──────────────────────────────│──────────────────────────────►│
  │                               │                              │
  │  ArenaMatchResult (2536)      │   ArenaMatchResult (2536)    │
  │  ArenaRatingUpdate (2537)     │   ArenaRatingUpdate (2537)   │
  │◄──────────────────────────────│──────────────────────────────►│
```

### Battleground Flow

```
Player Client              BG Queue Server               BG Instance
  │                               │                           │
  │  BattlegroundJoinQueue (2550) │                           │
  │  {BgType: CaptureFlag}        │                           │
  │──────────────────────────────►│                           │
  │                               │                           │
  │  BattlegroundQueueUpdate(2552)│                           │
  │  {QueuePosition: 12}          │                           │
  │◄──────────────────────────────│                           │
  │                               │                           │
  │                               │  [BG Pop!]                │
  │  BattlegroundJoin (2553)      │                           │
  │◄──────────────────────────────│──────────────────────────►│
  │                               │                           │
  │                               │  BattlegroundStart (2555) │
  │◄──────────────────────────────────────────────────────────│
  │                               │                           │
  │      ... Spielen ...          │                           │
  │                               │                           │
  │  BattlegroundScoreUpdate(2558)│                           │
  │◄──────────────────────────────────────────────────────────│
  │                               │                           │
  │  BattlegroundEnd (2556)       │                           │
  │  BattlegroundScore (2557)     │                           │
  │◄──────────────────────────────────────────────────────────│
```

---

## 🧱 DTOs & Enums

### PvpRank Enum

```csharp
public enum PvpRank : byte
{
    None = 0,
    Private = 1,
    Corporal = 2,
    Sergeant = 3,
    MasterSergeant = 4,
    Lieutenant = 5,
    Captain = 6,
    Major = 7,
    Colonel = 8,
    General = 9,
    HighWarlord = 10
}
```

### ArenaTeamSize Enum

```csharp
public enum ArenaTeamSize : byte
{
    Solo = 1,      // 1v1 (Skirmish only)
    Team2v2 = 2,   // 2v2 Rated
    Team3v3 = 3,   // 3v3 Rated
    Team5v5 = 5    // 5v5 Rated
}
```

### BattlegroundType Enum

```csharp
public enum BattlegroundType : byte
{
    CaptureTheFlag = 0,     // CTF
    DominationPoints = 1,   // Capture & Hold
    TeamDeathmatch = 2,     // TDM
    KingOfTheHill = 3,      // Single Capture Point
    PayloadEscort = 4,      // Escort NPC/Cart
    ResourceRace = 5        // Gather Resources
}
```

### PvpErrorCode Enum

```csharp
public enum PvpErrorCode : byte
{
    None = 0,
    NotInPvpZone = 1,
    InCombat = 2,
    OnCooldown = 3,
    AlreadyQueued = 4,
    NotQueued = 5,
    TeamFull = 6,
    TeamNotFound = 7,
    NotTeamLeader = 8,
    RatingTooLow = 9,
    RatingTooHigh = 10,
    LevelTooLow = 11,
    AlreadyInArena = 12,
    AlreadyInBattleground = 13,
    BgNotAvailable = 14,
    InvalidTeamSize = 15,
    DeserterDebuff = 16
}
```

### ArenaTeamDto

```csharp
[MessagePackObject]
public class ArenaTeamDto
{
    [Key(0)] public int TeamId { get; set; }
    [Key(1)] public string TeamName { get; set; }
    [Key(2)] public ArenaTeamSize TeamSize { get; set; }
    [Key(3)] public int Rating { get; set; }
    [Key(4)] public int SeasonWins { get; set; }
    [Key(5)] public int SeasonLosses { get; set; }
    [Key(6)] public int WeeklyWins { get; set; }
    [Key(7)] public int WeeklyLosses { get; set; }
    [Key(8)] public int CaptainPlayerId { get; set; }
    [Key(9)] public List<int> MemberIds { get; set; }
}
```

### ArenaMatchDto

```csharp
[MessagePackObject]
public class ArenaMatchDto
{
    [Key(0)] public int MatchId { get; set; }
    [Key(1)] public ArenaTeamSize TeamSize { get; set; }
    [Key(2)] public int ArenaMapId { get; set; }
    [Key(3)] public List<int> TeamAPlayerIds { get; set; }
    [Key(4)] public List<int> TeamBPlayerIds { get; set; }
    [Key(5)] public int TimeLimit { get; set; }       // Sekunden
    [Key(6)] public long StartTimestamp { get; set; }
}
```

### BattlegroundInfoDto

```csharp
[MessagePackObject]
public class BattlegroundInfoDto
{
    [Key(0)] public int InstanceId { get; set; }
    [Key(1)] public BattlegroundType Type { get; set; }
    [Key(2)] public int MapId { get; set; }
    [Key(3)] public int MaxPlayers { get; set; }
    [Key(4)] public int CurrentPlayersTeamA { get; set; }
    [Key(5)] public int CurrentPlayersTeamB { get; set; }
    [Key(6)] public int TimeLimit { get; set; }
    [Key(7)] public int ScoreTeamA { get; set; }
    [Key(8)] public int ScoreTeamB { get; set; }
}
```

### PvpPlayerStatsDto

```csharp
[MessagePackObject]
public class PvpPlayerStatsDto
{
    [Key(0)] public int PlayerId { get; set; }
    [Key(1)] public int Kills { get; set; }
    [Key(2)] public int Deaths { get; set; }
    [Key(3)] public int Assists { get; set; }
    [Key(4)] public int DamageDealt { get; set; }
    [Key(5)] public int HealingDone { get; set; }
    [Key(6)] public int ObjectivePoints { get; set; }
    [Key(7)] public int HonorGained { get; set; }
}
```

### Wichtige Konstanten

| Konstante | Wert | Beschreibung |
|-----------|------|--------------|
| `PVP_FLAG_DURATION` | 300s | Standard PvP-Flag Dauer (5 Min) |
| `PVP_FLAG_EXPIRY_WARNING` | 30s | Warnung vor Flag-Ablauf |
| `HONOR_DECAY_PERCENT` | 25% | Wöchentlicher Honor-Decay |
| `ARENA_QUEUE_TIMEOUT` | 1800s | Max Queue-Zeit (30 Min) |
| `BG_QUEUE_TIMEOUT` | 2400s | Max Queue-Zeit (40 Min) |
| `ARENA_START_COUNTDOWN` | 60s | Countdown vor Match-Start |
| `BG_START_COUNTDOWN` | 120s | Countdown vor BG-Start |
| `DESERTER_DURATION` | 900s | Deserter Debuff (15 Min) |
| `MIN_LEVEL_PVP` | 10 | Minimum Level für PvP |
| `MIN_LEVEL_ARENA` | 20 | Minimum Level für Arena |
| `MIN_LEVEL_BG` | 15 | Minimum Level für Battleground |

---

## 📩 Messages

### PvP Grundfunktionen (2500-2507)

---

### PvpFlagRequest (2500)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten (manuell)  
**Authentifizierung:** Ja (Session)  
**Spezielle Rechte:** Keine

#### Beschreibung
Client aktiviert oder deaktiviert den PvP-Flag. PvP-Flag ermöglicht es, andere Spieler mit aktivem Flag anzugreifen.

#### Im Scope ✅
- PvP-Flag aktivieren
- PvP-Flag deaktivieren
- Cooldown-Prüfung

#### Nicht im Scope ❌
- Auto-Flag bei Angriff → wird serverseitig gesetzt

#### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `PvpFlagRequest` (2500) | Ja |
| RequestId | uint | Korrelation | Ja |
| Enable | bool | true = aktivieren, false = deaktivieren | Ja |

#### Erwartete Response
- **Bei Erfolg:** `PvpFlagUpdate` (2501) mit neuem Status
- **Bei Fehler:** `PvpFlagUpdate` (2501) mit ErrorCode

#### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `PvpFlagUpdate` | 2501 | Response mit neuem Status |
| `PvpFlagExpiring` | 2502 | Warnung vor Ablauf |

#### Error Codes
| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `InCombat` | Im Kampf | Warten bis Kampf beendet |
| `OnCooldown` | Cooldown aktiv | 5 Min warten |
| `NotInPvpZone` | Zone erlaubt kein PvP | Zone wechseln |

---

### PvpFlagUpdate (2501)

**Richtung:** 📥 Server → Client  
**Frequenz:** Bei Status-Änderung  
**Authentifizierung:** N/A (Server-initiated)  
**Spezielle Rechte:** Keine

#### Beschreibung
Server informiert Client über PvP-Flag Status eines Spielers. Wird an den betroffenen Spieler und alle Spieler in Sichtweite gesendet.

#### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `PvpFlagUpdate` (2501) | Ja |
| RequestId | uint | Korrelation (falls Response) | Nein |
| PlayerId | int | Betroffener Spieler | Ja |
| FlagActive | bool | PvP-Flag aktiv | Ja |
| ExpiresAt | long | Unix-Timestamp Ablauf | Nein |
| ErrorCode | PvpErrorCode | Fehler (falls vorhanden) | Nein |

---

### PvpFlagExpiring (2502)

**Richtung:** 📥 Server → Client  
**Frequenz:** 30s vor Ablauf  
**Authentifizierung:** N/A  
**Spezielle Rechte:** Keine

#### Beschreibung
Server warnt den Spieler, dass sein PvP-Flag bald abläuft. Gibt dem Spieler Zeit, den Flag zu erneuern.

#### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `PvpFlagExpiring` (2502) | Ja |
| TimeRemaining | int | Sekunden bis Ablauf | Ja |

---

### PvpKill (2503)

**Richtung:** 📡 Broadcast (Zone)  
**Frequenz:** Bei jedem PvP-Kill  
**Authentifizierung:** N/A  
**Spezielle Rechte:** Keine

#### Beschreibung
Broadcast eines PvP-Kills an alle Spieler in der Zone. Enthält Killer und Opfer.

#### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `PvpKill` (2503) | Ja |
| KillerId | int | Spieler, der getötet hat | Ja |
| KillerName | string | Name des Killers | Ja |
| VictimId | int | Spieler, der gestorben ist | Ja |
| VictimName | string | Name des Opfers | Ja |
| HonorAwarded | int | Honor für den Kill | Ja |
| IsHonorKill | bool | Zählt als Honor-Kill | Ja |

---

### PvpDeath (2504)

**Richtung:** 📥 Server → Client (Opfer)  
**Frequenz:** Bei PvP-Tod  
**Authentifizierung:** N/A  
**Spezielle Rechte:** Keine

#### Beschreibung
Server informiert den getöteten Spieler über seinen PvP-Tod. Enthält Details zum Killer und Respawn-Optionen.

#### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `PvpDeath` (2504) | Ja |
| KillerId | int | Wer hat getötet | Ja |
| KillerName | string | Name des Killers | Ja |
| KillerLevel | int | Level des Killers | Ja |
| RespawnDelay | int | Sekunden bis Respawn | Ja |
| CanReleaseSpirit | bool | Geist freilassen möglich | Ja |

---

### PvpHonorGain (2505)

**Richtung:** 📥 Server → Client  
**Frequenz:** Bei Honor-Gewinn  
**Authentifizierung:** N/A  
**Spezielle Rechte:** Keine

#### Beschreibung
Server informiert den Spieler über erhaltene Honor-Punkte. Kann durch Kills, Objectives oder BG-Siege entstehen.

#### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `PvpHonorGain` (2505) | Ja |
| Amount | int | Erhaltene Honor-Punkte | Ja |
| Source | string | Quelle (Kill, Objective, Win) | Ja |
| TargetId | int | Opfer-ID (falls Kill) | Nein |
| NewTotal | int | Neuer Honor-Gesamtwert | Ja |

---

### PvpHonorUpdate (2506)

**Richtung:** 📥 Server → Client  
**Frequenz:** Bei Änderung  
**Authentifizierung:** N/A  
**Spezielle Rechte:** Keine

#### Beschreibung
Server sendet vollständiges Honor-Update. Enthält aktuellen Stand, wöchentliche und saisonale Werte.

#### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `PvpHonorUpdate` (2506) | Ja |
| TotalHonor | int | Lifetime Honor | Ja |
| CurrentHonor | int | Aktuelle unausgegebene Honor | Ja |
| WeeklyHonor | int | Diese Woche verdient | Ja |
| SeasonHonor | int | Diese Saison verdient | Ja |
| HonorKills | int | Lifetime Honor-Kills | Ja |
| WeeklyKills | int | Kills diese Woche | Ja |

---

### PvpRankUpdate (2507)

**Richtung:** 📥 Server → Client  
**Frequenz:** Bei Rang-Änderung  
**Authentifizierung:** N/A  
**Spezielle Rechte:** Keine

#### Beschreibung
Server informiert über Rang-Aufstieg oder -Abstieg. PvP-Rang basiert auf wöchentlicher Honor-Performance.

#### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `PvpRankUpdate` (2507) | Ja |
| OldRank | PvpRank | Vorheriger Rang | Ja |
| NewRank | PvpRank | Neuer Rang | Ja |
| RankProgress | float | Fortschritt zum nächsten Rang (0-100%) | Ja |
| IsPromotion | bool | Aufstieg oder Abstieg | Ja |

---

### Arena-Team Verwaltung (2520-2525)

---

### ArenaTeamCreate (2520)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** Ja  
**Spezielle Rechte:** Keine

#### Beschreibung
Client erstellt ein neues Arena-Team. Erfordert Mindestlevel und genügend Gold für die Registrierungsgebühr.

#### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `ArenaTeamCreate` (2520) | Ja |
| RequestId | uint | Korrelation | Ja |
| TeamName | string | Name des Teams (3-24 Zeichen) | Ja |
| TeamSize | ArenaTeamSize | 2v2, 3v3 oder 5v5 | Ja |

#### Erwartete Response
- **Bei Erfolg:** `ArenaTeamUpdate` (2525) mit neuem Team
- **Bei Fehler:** `ArenaTeamUpdate` (2525) mit ErrorCode

---

### ArenaTeamDisband (2521)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** Ja  
**Spezielle Rechte:** Team-Captain

#### Beschreibung
Team-Captain löst das Arena-Team auf. Alle Mitglieder werden entfernt.

#### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `ArenaTeamDisband` (2521) | Ja |
| RequestId | uint | Korrelation | Ja |
| TeamId | int | ID des aufzulösenden Teams | Ja |
| ConfirmDisband | bool | Bestätigung (muss true sein) | Ja |

#### Erwartete Response
- `ArenaTeamUpdate` (2525) mit TeamId = 0 (Team aufgelöst)

---

### ArenaTeamInvite (2522)

**Richtung:** 📤 Client → Server  
**Frequenz:** Gelegentlich  
**Authentifizierung:** Ja  
**Spezielle Rechte:** Team-Captain

#### Beschreibung
Team-Captain lädt einen Spieler ins Arena-Team ein.

#### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `ArenaTeamInvite` (2522) | Ja |
| RequestId | uint | Korrelation | Ja |
| TeamId | int | Team-ID | Ja |
| TargetPlayerName | string | Einzuladender Spieler | Ja |

#### Erwartete Response
- `ArenaTeamUpdate` (2525) mit Status

---

### ArenaTeamLeave (2523)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** Ja  
**Spezielle Rechte:** Keine

#### Beschreibung
Spieler verlässt sein Arena-Team freiwillig.

#### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `ArenaTeamLeave` (2523) | Ja |
| RequestId | uint | Korrelation | Ja |
| TeamId | int | Team-ID | Ja |

#### Erwartete Response
- `ArenaTeamUpdate` (2525)

---

### ArenaTeamKick (2524)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** Ja  
**Spezielle Rechte:** Team-Captain

#### Beschreibung
Team-Captain entfernt ein Mitglied aus dem Arena-Team.

#### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `ArenaTeamKick` (2524) | Ja |
| RequestId | uint | Korrelation | Ja |
| TeamId | int | Team-ID | Ja |
| TargetPlayerId | int | Zu entfernender Spieler | Ja |

#### Erwartete Response
- `ArenaTeamUpdate` (2525)

---

### ArenaTeamUpdate (2525)

**Richtung:** 📥 Server → Client  
**Frequenz:** Bei Änderung  
**Authentifizierung:** N/A  
**Spezielle Rechte:** Keine

#### Beschreibung
Server sendet Arena-Team Update. Dient als Response für alle Team-Requests und als Push bei Änderungen.

#### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `ArenaTeamUpdate` (2525) | Ja |
| RequestId | uint | Korrelation | Nein |
| Team | ArenaTeamDto | Team-Daten (null wenn aufgelöst) | Nein |
| ErrorCode | PvpErrorCode | Fehler (falls vorhanden) | Nein |
| Message | string | Status-Nachricht | Nein |

---

### Arena Queue & Match (2530-2537)

---

### ArenaJoinQueue (2530)

**Richtung:** 📤 Client → Server  
**Frequenz:** Gelegentlich  
**Authentifizierung:** Ja  
**Spezielle Rechte:** Keine

#### Beschreibung
Spieler/Team tritt der Arena-Queue bei. Erfordert vollständiges Team für Rated Games.

#### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `ArenaJoinQueue` (2530) | Ja |
| RequestId | uint | Korrelation | Ja |
| TeamSize | ArenaTeamSize | Queue-Typ (2v2, 3v3, 5v5) | Ja |
| IsRated | bool | Rated oder Skirmish | Ja |
| TeamId | int | Team-ID (nur für Rated) | Nein |

#### Erwartete Response
- `ArenaQueueUpdate` (2532) mit Queue-Status

---

### ArenaLeaveQueue (2531)

**Richtung:** 📤 Client → Server  
**Frequenz:** Gelegentlich  
**Authentifizierung:** Ja  
**Spezielle Rechte:** Keine

#### Beschreibung
Spieler/Team verlässt die Arena-Queue.

#### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `ArenaLeaveQueue` (2531) | Ja |
| RequestId | uint | Korrelation | Ja |
| TeamSize | ArenaTeamSize | Welche Queue verlassen | Ja |

#### Erwartete Response
- `ArenaQueueUpdate` (2532) mit QueueActive = false

---

### ArenaQueueUpdate (2532)

**Richtung:** 📥 Server → Client  
**Frequenz:** Regelmäßig während Queue  
**Authentifizierung:** N/A  
**Spezielle Rechte:** Keine

#### Beschreibung
Server informiert über Queue-Status. Wird periodisch gesendet (alle 30s) und bei Status-Änderungen.

#### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `ArenaQueueUpdate` (2532) | Ja |
| RequestId | uint | Korrelation | Nein |
| TeamSize | ArenaTeamSize | Queue-Typ | Ja |
| QueueActive | bool | In Queue oder nicht | Ja |
| EstimatedWait | int | Geschätzte Wartezeit (Sekunden) | Nein |
| QueuePosition | int | Position in Queue | Nein |
| ErrorCode | PvpErrorCode | Fehler | Nein |

---

### ArenaMatchFound (2533)

**Richtung:** 📥 Server → Client  
**Frequenz:** Wenn Match gefunden  
**Authentifizierung:** N/A  
**Spezielle Rechte:** Keine

#### Beschreibung
Server informiert, dass ein Arena-Match gefunden wurde. Spieler müssen bestätigen.

#### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `ArenaMatchFound` (2533) | Ja |
| MatchId | int | Match-ID | Ja |
| TeamSize | ArenaTeamSize | Match-Typ | Ja |
| ArenaMapId | int | Karten-ID | Ja |
| AcceptDeadline | long | Timestamp für Accept | Ja |

---

### ArenaMatchStart (2534)

**Richtung:** 📥 Server → Client  
**Frequenz:** Bei Match-Start  
**Authentifizierung:** N/A  
**Spezielle Rechte:** Keine

#### Beschreibung
Server startet das Arena-Match. Enthält alle Match-Details.

#### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `ArenaMatchStart` (2534) | Ja |
| Match | ArenaMatchDto | Vollständige Match-Daten | Ja |
| Countdown | int | Sekunden bis Kampfbeginn | Ja |
| SpawnPosition | Vector2 | Spawn-Position | Ja |

---

### ArenaMatchEnd (2535)

**Richtung:** 📥 Server → Client  
**Frequenz:** Bei Match-Ende  
**Authentifizierung:** N/A  
**Spezielle Rechte:** Keine

#### Beschreibung
Server beendet das Arena-Match. Kann durch Sieg, Timeout oder Aufgabe erfolgen.

#### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `ArenaMatchEnd` (2535) | Ja |
| MatchId | int | Match-ID | Ja |
| WinnerTeam | byte | 0 = Team A, 1 = Team B | Ja |
| EndReason | string | Win, Timeout, Surrender | Ja |
| Duration | int | Match-Dauer in Sekunden | Ja |

---

### ArenaMatchResult (2536)

**Richtung:** 📥 Server → Client  
**Frequenz:** Nach Match-Ende  
**Authentifizierung:** N/A  
**Spezielle Rechte:** Keine

#### Beschreibung
Server sendet detaillierte Match-Ergebnisse mit Statistiken.

#### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `ArenaMatchResult` (2536) | Ja |
| MatchId | int | Match-ID | Ja |
| Won | bool | Spieler hat gewonnen | Ja |
| TeamAStats | List<PvpPlayerStatsDto> | Stats Team A | Ja |
| TeamBStats | List<PvpPlayerStatsDto> | Stats Team B | Ja |
| HonorGained | int | Erhaltene Honor | Ja |
| ArenaPointsGained | int | Erhaltene Arena-Punkte | Ja |

---

### ArenaRatingUpdate (2537)

**Richtung:** 📥 Server → Client  
**Frequenz:** Nach Rated Match  
**Authentifizierung:** N/A  
**Spezielle Rechte:** Keine

#### Beschreibung
Server informiert über Rating-Änderung nach einem Rated Arena Match.

#### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `ArenaRatingUpdate` (2537) | Ja |
| TeamId | int | Team-ID | Ja |
| OldRating | int | Rating vor Match | Ja |
| NewRating | int | Rating nach Match | Ja |
| RatingChange | int | Differenz (positiv/negativ) | Ja |
| PersonalRating | int | Persönliches Rating | Ja |

---

### Battleground Queue & Match (2550-2560)

---

### BattlegroundJoinQueue (2550)

**Richtung:** 📤 Client → Server  
**Frequenz:** Gelegentlich  
**Authentifizierung:** Ja  
**Spezielle Rechte:** Keine

#### Beschreibung
Spieler tritt einer Battleground-Queue bei.

#### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `BattlegroundJoinQueue` (2550) | Ja |
| RequestId | uint | Korrelation | Ja |
| BgType | BattlegroundType | Welcher BG-Typ | Ja |
| AsGroup | bool | Mit Gruppe queuen | Ja |

#### Erwartete Response
- `BattlegroundQueueUpdate` (2552)

---

### BattlegroundLeaveQueue (2551)

**Richtung:** 📤 Client → Server  
**Frequenz:** Gelegentlich  
**Authentifizierung:** Ja  
**Spezielle Rechte:** Keine

#### Beschreibung
Spieler verlässt die Battleground-Queue.

#### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `BattlegroundLeaveQueue` (2551) | Ja |
| RequestId | uint | Korrelation | Ja |
| BgType | BattlegroundType | Welche Queue | Ja |

#### Erwartete Response
- `BattlegroundQueueUpdate` (2552) mit QueueActive = false

---

### BattlegroundQueueUpdate (2552)

**Richtung:** 📥 Server → Client  
**Frequenz:** Regelmäßig  
**Authentifizierung:** N/A  
**Spezielle Rechte:** Keine

#### Beschreibung
Server sendet BG-Queue Status-Update.

#### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `BattlegroundQueueUpdate` (2552) | Ja |
| RequestId | uint | Korrelation | Nein |
| BgType | BattlegroundType | BG-Typ | Ja |
| QueueActive | bool | In Queue | Ja |
| EstimatedWait | int | Geschätzte Wartezeit | Nein |
| ErrorCode | PvpErrorCode | Fehler | Nein |

---

### BattlegroundJoin (2553)

**Richtung:** 📤 Client → Server  
**Frequenz:** Bei BG-Pop  
**Authentifizierung:** Ja  
**Spezielle Rechte:** Keine

#### Beschreibung
Spieler akzeptiert den BG-Pop und betritt das Battleground.

#### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `BattlegroundJoin` (2553) | Ja |
| RequestId | uint | Korrelation | Ja |
| InstanceId | int | BG-Instanz-ID | Ja |
| Accept | bool | true = beitreten, false = ablehnen | Ja |

#### Erwartete Response
- Bei Accept: Zone-Transfer zum BG
- Bei Decline: Zurück in Queue oder Deserter

---

### BattlegroundLeave (2554)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** Ja  
**Spezielle Rechte:** Keine

#### Beschreibung
Spieler verlässt ein laufendes Battleground. Resultiert in Deserter-Debuff.

#### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `BattlegroundLeave` (2554) | Ja |
| RequestId | uint | Korrelation | Ja |
| Confirm | bool | Deserter-Warnung bestätigt | Ja |

#### Erwartete Response
- Zone-Transfer zurück zur vorherigen Zone
- Deserter-Debuff (15 Min)

---

### BattlegroundStart (2555)

**Richtung:** 📥 Server → Client  
**Frequenz:** Bei BG-Start  
**Authentifizierung:** N/A  
**Spezielle Rechte:** Keine

#### Beschreibung
Server startet das Battleground. Gates öffnen sich nach Countdown.

#### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `BattlegroundStart` (2555) | Ja |
| BgInfo | BattlegroundInfoDto | BG-Daten | Ja |
| Countdown | int | Sekunden bis Start | Ja |
| PlayerTeam | byte | 0 = Team A, 1 = Team B | Ja |
| SpawnPosition | Vector2 | Spawn-Position | Ja |

---

### BattlegroundEnd (2556)

**Richtung:** 📥 Server → Client  
**Frequenz:** Bei BG-Ende  
**Authentifizierung:** N/A  
**Spezielle Rechte:** Keine

#### Beschreibung
Server beendet das Battleground und kündigt Gewinner an.

#### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `BattlegroundEnd` (2556) | Ja |
| InstanceId | int | BG-Instanz | Ja |
| WinnerTeam | byte | 0 = Team A, 1 = Team B | Ja |
| FinalScoreTeamA | int | Endpunktzahl Team A | Ja |
| FinalScoreTeamB | int | Endpunktzahl Team B | Ja |
| Duration | int | BG-Dauer in Sekunden | Ja |
| TeleportDelay | int | Sekunden bis Auto-Teleport | Ja |

---

### BattlegroundScore (2557)

**Richtung:** 📥 Server → Client  
**Frequenz:** Bei BG-Ende  
**Authentifizierung:** N/A  
**Spezielle Rechte:** Keine

#### Beschreibung
Server sendet vollständiges Scoreboard nach BG-Ende.

#### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `BattlegroundScore` (2557) | Ja |
| InstanceId | int | BG-Instanz | Ja |
| TeamAPlayers | List<PvpPlayerStatsDto> | Stats Team A | Ja |
| TeamBPlayers | List<PvpPlayerStatsDto> | Stats Team B | Ja |
| PersonalHonor | int | Eigene Honor-Punkte | Ja |
| BonusHonor | int | Bonus für Sieg | Ja |

---

### BattlegroundScoreUpdate (2558)

**Richtung:** 📥 Server → Client  
**Frequenz:** Bei Score-Änderung  
**Authentifizierung:** N/A  
**Spezielle Rechte:** Keine

#### Beschreibung
Server sendet Score-Update während laufendem BG. Nur Gesamtpunktzahl, nicht individuell.

#### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `BattlegroundScoreUpdate` (2558) | Ja |
| ScoreTeamA | int | Aktuelle Punkte Team A | Ja |
| ScoreTeamB | int | Aktuelle Punkte Team B | Ja |
| TimeRemaining | int | Verbleibende Zeit | Ja |

---

### BattlegroundObjective (2559)

**Richtung:** 📥 Server → Client  
**Frequenz:** Bei Objective-Änderung  
**Authentifizierung:** N/A  
**Spezielle Rechte:** Keine

#### Beschreibung
Server informiert über Objective-Status (Capture Points, Flags, etc.).

#### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `BattlegroundObjective` (2559) | Ja |
| ObjectiveId | int | Objective-ID | Ja |
| ObjectiveName | string | Name | Ja |
| ControllingTeam | byte | 0, 1, oder 255 (neutral) | Ja |
| CaptureProgress | float | 0-100% | Nein |
| ContestingTeam | byte | Wer versucht zu erobern | Nein |

---

### BattlegroundFlag (2560)

**Richtung:** 📥 Server → Client  
**Frequenz:** Bei Flag-Event  
**Authentifizierung:** N/A  
**Spezielle Rechte:** Keine

#### Beschreibung
Server informiert über Flag-Status in CTF-Battlegrounds.

#### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `BattlegroundFlag` (2560) | Ja |
| FlagTeam | byte | Welches Team die Flagge gehört | Ja |
| CarrierId | int | Wer trägt die Flagge (0 = am Boden/Base) | Ja |
| CarrierName | string | Name des Trägers | Nein |
| Position | Vector2 | Position der Flagge | Ja |
| State | string | AtBase, Carried, Dropped | Ja |

---

### World PvP (2570-2571)

---

### WorldPvpObjective (2570)

**Richtung:** 📥 Server → Client  
**Frequenz:** Bei Änderung  
**Authentifizierung:** N/A  
**Spezielle Rechte:** Keine

#### Beschreibung
Server informiert über World-PvP Objectives in Open-World Zonen.

#### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `WorldPvpObjective` (2570) | Ja |
| ZoneId | int | Zone-ID | Ja |
| ObjectiveId | int | Objective-ID | Ja |
| ObjectiveName | string | Name | Ja |
| ControllingFaction | byte | 0 = neutral, 1 = Faction A, 2 = Faction B | Ja |
| CaptureProgress | float | 0-100% | Ja |
| DefenderCount | int | Anzahl Verteidiger | Ja |
| AttackerCount | int | Anzahl Angreifer | Ja |

---

### WorldPvpZoneUpdate (2571)

**Richtung:** 📥 Server → Client  
**Frequenz:** Regelmäßig / Bei Änderung  
**Authentifizierung:** N/A  
**Spezielle Rechte:** Keine

#### Beschreibung
Server sendet Gesamtstatus einer World-PvP Zone mit allen Objectives.

#### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `WorldPvpZoneUpdate` (2571) | Ja |
| ZoneId | int | Zone-ID | Ja |
| ZoneName | string | Zone-Name | Ja |
| ControllingFaction | byte | Welche Fraktion kontrolliert die Zone | Ja |
| ObjectiveCount | int | Anzahl Objectives | Ja |
| CapturedByA | int | Von Faction A kontrolliert | Ja |
| CapturedByB | int | Von Faction B kontrolliert | Ja |
| BonusActive | bool | Zone-Bonus aktiv | Ja |
| NextBattleTime | long | Timestamp nächste Schlacht | Nein |

---

## 📎 Anhang

### MessageType Enum

```csharp
// PVP / ARENA / BATTLEGROUND (2500-2599)
// ═══════════════════════════════════════════════════════════════
PvpFlagRequest = 2500,
PvpFlagUpdate = 2501,
PvpFlagExpiring = 2502,
PvpKill = 2503,
PvpDeath = 2504,
PvpHonorGain = 2505,
PvpHonorUpdate = 2506,
PvpRankUpdate = 2507,
ArenaTeamCreate = 2520,
ArenaTeamDisband = 2521,
ArenaTeamInvite = 2522,
ArenaTeamLeave = 2523,
ArenaTeamKick = 2524,
ArenaTeamUpdate = 2525,
ArenaJoinQueue = 2530,
ArenaLeaveQueue = 2531,
ArenaQueueUpdate = 2532,
ArenaMatchFound = 2533,
ArenaMatchStart = 2534,
ArenaMatchEnd = 2535,
ArenaMatchResult = 2536,
ArenaRatingUpdate = 2537,
BattlegroundJoinQueue = 2550,
BattlegroundLeaveQueue = 2551,
BattlegroundQueueUpdate = 2552,
BattlegroundJoin = 2553,
BattlegroundLeave = 2554,
BattlegroundStart = 2555,
BattlegroundEnd = 2556,
BattlegroundScore = 2557,
BattlegroundScoreUpdate = 2558,
BattlegroundObjective = 2559,
BattlegroundFlag = 2560,
WorldPvpObjective = 2570,
WorldPvpZoneUpdate = 2571,
```

### Request/Response Paare

| Request | ID | Response | ID | Beschreibung |
|---------|-----|----------|-----|--------------|
| `PvpFlagRequest` | 2500 | `PvpFlagUpdate` | 2501 | PvP-Flag Toggle |
| `ArenaTeamCreate` | 2520 | `ArenaTeamUpdate` | 2525 | Team erstellen |
| `ArenaTeamDisband` | 2521 | `ArenaTeamUpdate` | 2525 | Team auflösen |
| `ArenaTeamInvite` | 2522 | `ArenaTeamUpdate` | 2525 | Einladung senden |
| `ArenaTeamLeave` | 2523 | `ArenaTeamUpdate` | 2525 | Team verlassen |
| `ArenaTeamKick` | 2524 | `ArenaTeamUpdate` | 2525 | Spieler kicken |
| `ArenaJoinQueue` | 2530 | `ArenaQueueUpdate` | 2532 | Queue beitreten |
| `ArenaLeaveQueue` | 2531 | `ArenaQueueUpdate` | 2532 | Queue verlassen |
| `BattlegroundJoinQueue` | 2550 | `BattlegroundQueueUpdate` | 2552 | BG-Queue beitreten |
| `BattlegroundLeaveQueue` | 2551 | `BattlegroundQueueUpdate` | 2552 | BG-Queue verlassen |
| `BattlegroundJoin` | 2553 | Zone-Transfer | - | BG beitreten |
| `BattlegroundLeave` | 2554 | Zone-Transfer | - | BG verlassen |

### Server-initiated Messages

| Message | ID | Trigger |
|---------|-----|---------|
| `PvpFlagExpiring` | 2502 | 30s vor Flag-Ablauf |
| `PvpKill` | 2503 | Bei PvP-Kill (Broadcast) |
| `PvpDeath` | 2504 | Bei PvP-Tod (an Opfer) |
| `PvpHonorGain` | 2505 | Bei Honor-Gewinn |
| `PvpHonorUpdate` | 2506 | Bei Honor-Änderung |
| `PvpRankUpdate` | 2507 | Bei Rang-Änderung |
| `ArenaMatchFound` | 2533 | Match gefunden |
| `ArenaMatchStart` | 2534 | Match beginnt |
| `ArenaMatchEnd` | 2535 | Match endet |
| `ArenaMatchResult` | 2536 | Match-Ergebnis |
| `ArenaRatingUpdate` | 2537 | Rating-Änderung |
| `BattlegroundStart` | 2555 | BG startet |
| `BattlegroundEnd` | 2556 | BG endet |
| `BattlegroundScore` | 2557 | Finales Scoreboard |
| `BattlegroundScoreUpdate` | 2558 | Score während BG |
| `BattlegroundObjective` | 2559 | Objective-Status |
| `BattlegroundFlag` | 2560 | Flag-Status |
| `WorldPvpObjective` | 2570 | World-PvP Objective |
| `WorldPvpZoneUpdate` | 2571 | World-PvP Zone-Status |

### Datei-Struktur

```
shared/Mmo.Shared/
├── Messaging/
│   ├── Enums/
│   │   └── MessageType.cs          # Message IDs 2500-2571
│   └── Messages/
│       └── Pvp/
│           ├── PvpFlagRequest.cs
│           ├── PvpFlagUpdate.cs
│           ├── PvpFlagExpiring.cs
│           ├── PvpKill.cs
│           ├── PvpDeath.cs
│           ├── PvpHonorGain.cs
│           ├── PvpHonorUpdate.cs
│           ├── PvpRankUpdate.cs
│           ├── ArenaTeamCreate.cs
│           ├── ArenaTeamDisband.cs
│           ├── ArenaTeamInvite.cs
│           ├── ArenaTeamLeave.cs
│           ├── ArenaTeamKick.cs
│           ├── ArenaTeamUpdate.cs
│           ├── ArenaJoinQueue.cs
│           ├── ArenaLeaveQueue.cs
│           ├── ArenaQueueUpdate.cs
│           ├── ArenaMatchFound.cs
│           ├── ArenaMatchStart.cs
│           ├── ArenaMatchEnd.cs
│           ├── ArenaMatchResult.cs
│           ├── ArenaRatingUpdate.cs
│           ├── BattlegroundJoinQueue.cs
│           ├── BattlegroundLeaveQueue.cs
│           ├── BattlegroundQueueUpdate.cs
│           ├── BattlegroundJoin.cs
│           ├── BattlegroundLeave.cs
│           ├── BattlegroundStart.cs
│           ├── BattlegroundEnd.cs
│           ├── BattlegroundScore.cs
│           ├── BattlegroundScoreUpdate.cs
│           ├── BattlegroundObjective.cs
│           ├── BattlegroundFlag.cs
│           ├── WorldPvpObjective.cs
│           └── WorldPvpZoneUpdate.cs
└── Dto/
    └── Pvp/
        ├── ArenaTeamDto.cs
        ├── ArenaMatchDto.cs
        ├── BattlegroundInfoDto.cs
        └── PvpPlayerStatsDto.cs
```

---

**Letzte Aktualisierung**: 2026-01-02  
**Version**: 3.0.0  
**Status**: ✅ Aligned mit MessageType Enum (35 Messages)

[← Zurück zur Übersicht](Message-Reference.md)

Source: docs/03-messages/25-pvp.md
