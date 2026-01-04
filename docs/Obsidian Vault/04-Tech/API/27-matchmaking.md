# 🎮 Matchmaking Messages (2700-2732)

**Kategorie:** 27  
**Range:** 2700–2732  
**Phase:** ✅ Production-Ready  
**Status:** ✅ Vollständig spezifiziert (17 Messages)

[← Zurück zur Übersicht](Message-Reference.md)

---

## 📋 Inhaltsverzeichnis

1. [Übersicht](#-übersicht)
2. [Matchmaking Flow](#-matchmaking-flow)
   - [Server-Authoritative Architektur](#server-authoritative-architektur)
   - [Queue-Join Flow](#queue-join-flow)
   - [Queue-Pop und Accept Flow](#queue-pop-und-accept-flow)
3. [DTOs / Enums](#-dtos--enums)
   - [QueueType](#queuetype)
   - [QueueRole](#queuerole)
   - [MatchmakingErrorCode](#matchmakingerrorcode)
   - [QueueInfoDto](#queueinfodto)
   - [QueueStatusDto](#queuestatusdto)
   - [MatchFoundDto](#matchfounddto)
   - [Wichtige Konstanten](#wichtige-konstanten)
4. [Messages](#-messages)
   - [Queue Grundfunktionen (2700-2710)](#queue-grundfunktionen-2700-2710)
   - [Rollen-Auswahl (2720-2722)](#rollen-auswahl-2720-2722)
   - [Skirmish (2730-2732)](#skirmish-2730-2732)
5. [Anhang](#-anhang)
   - [MessageType Enum](#messagetype-enum-auszug)
   - [Request/Response Paare](#requestresponse-paare)
   - [Server-initiated Messages](#server-initiated-messages)
   - [Datei-Struktur](#datei-struktur)

---

## 📖 Übersicht

Diese Kategorie umfasst alle Messages für das **Matchmaking-System** im 2DMMO:

- **Queue-System**: Beitritt, Verlassen, Status-Updates
- **Match-Finding**: Pop, Accept, Decline, Timeout
- **Rollen-Auswahl**: Tank/Healer/DPS Selection und Call-to-Arms
- **Skirmish**: Spontane PvP-Kämpfe ohne feste Gruppenstruktur

**Prinzipien:**
1. **Server-Authoritative**: Alle Matchmaking-Entscheidungen werden serverseitig getroffen
2. **Fair Matching**: ELO/MMR-basiertes Matching für ausgeglichene Spiele
3. **Rolle-Garantie**: Spieler erhalten immer ihre gewählte Rolle
4. **Deserter-System**: Penalties für vorzeitiges Verlassen

---

## 🔄 Matchmaking Flow

### Server-Authoritative Architektur

```
┌─────────────────────────────────────────────────────────────────┐
│                    MATCHMAKING SERVER                           │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────────────────┐ │
│  │ Queue Pool  │  │ Matcher     │  │ Instance Spawner        │ │
│  │ - Dungeon   │  │ - MMR-Based │  │ - Creates Instance      │ │
│  │ - Raid      │  │ - Role Req  │  │ - Teleports Players     │ │
│  │ - Arena     │  │ - Size Req  │  │                         │ │
│  │ - Skirmish  │  │             │  │                         │ │
│  └─────────────┘  └─────────────┘  └─────────────────────────┘ │
└─────────────────────────────────────────────────────────────────┘
         ▲                                      │
         │ QueueJoin/Leave                      │ QueuePop
         │                                      ▼
┌─────────────────────────────────────────────────────────────────┐
│                         CLIENTS                                  │
│   Player A          Player B          Player C          ...     │
│   (Tank)            (Healer)          (DPS)                     │
└─────────────────────────────────────────────────────────────────┘
```

### Queue-Join Flow

```
Client                          Server                      Queue Pool
   │                               │                            │
   │  QueueJoin (2700)             │                            │
   │  - QueueType                  │                            │
   │  - SelectedRoles[]            │                            │
   │  - ActivityId (Dungeon-ID)    │                            │
   │──────────────────────────────>│                            │
   │                               │                            │
   │                               │  Validate:                 │
   │                               │  - Eligible?               │
   │                               │  - Not in Queue?           │
   │                               │  - Gear sufficient?        │
   │                               │                            │
   │                               │  Add to Pool               │
   │                               │─────────────────────────────>
   │                               │                            │
   │  QueueJoinResult (2701)       │                            │
   │  - Success: true              │                            │
   │  - QueueId                    │                            │
   │  - EstimatedWait              │                            │
   │<──────────────────────────────│                            │
   │                               │                            │
   │  [Periodic: QueueEstimate]    │                            │
   │  QueueEstimate (2704)         │                            │
   │  - EstimatedWaitSeconds       │                            │
   │<──────────────────────────────│                            │
   │                               │                            │
   │  [Periodic: QueueUpdate]      │                            │
   │  QueueUpdate (2703)           │                            │
   │  - PlayersInQueue             │                            │
   │  - AverageWait                │                            │
   │<──────────────────────────────│                            │
   │                               │                            │
```

### Queue-Pop und Accept Flow

```
Client A       Client B       Client C       Server            Instance
   │              │              │              │                  │
   │              │              │              │  Match Found!    │
   │              │              │              │                  │
   │  QueuePop (2705)            │              │                  │
   │  - MatchId                  │              │                  │
   │  - ActivityName             │              │                  │
   │  - AcceptDeadlineSec: 30    │              │                  │
   │<─────────────────────────────────────────────                 │
   │              │<──────────────────────────────                 │
   │              │              │<───────────────                 │
   │                                                               │
   │  QueueAccept (2706)         │              │                  │
   │  - MatchId                  │              │                  │
   │────────────────────────────────────────────>                  │
   │              │                                                │
   │              │  QueueAccept (2706)         │                  │
   │              │────────────────────────────>│                  │
   │              │              │                                 │
   │              │              │  QueueAccept (2706)             │
   │              │              │────────────>│                   │
   │                                                               │
   │                               │  All Accepted!               │
   │                               │  Spawn Instance              │
   │                               │─────────────────────────────>│
   │                                                               │
   │  [Teleport to Instance]       │              │               │
   │<──────────────────────────────│──────────────│───────────────│
```

**Decline / Timeout Szenario:**

```
Client A       Client B       Client C       Server
   │              │              │              │
   │              │              │              │  Match Found!
   │  QueuePop    │  QueuePop    │  QueuePop    │
   │<─────────────│<─────────────│<─────────────│
   │                                            │
   │  QueueAccept │              │              │
   │────────────────────────────────────────────>
   │              │                             │
   │              │  QueueDecline (2707)        │
   │              │─────────────────────────────>
   │                                            │
   │                               │  [30s Pass]│
   │                               │  Timeout!  │
   │                                            │
   │  QueueTimeout (2708)          │            │
   │<──────────────────────────────│────────────│
   │              │                             │
   │              │  QueueDeserter (2710)       │
   │              │<────────────────────────────│
   │                                            │
   │  [Re-queued automatically]    │            │
```

---

## 🧱 DTOs / Enums

### QueueType

```csharp
public enum QueueType : byte
{
    Dungeon = 0,        // Random Dungeon (5-man)
    HeroicDungeon = 1,  // Heroic Dungeon (5-man)
    MythicDungeon = 2,  // Mythic+ Dungeon (5-man)
    Raid = 3,           // Looking for Raid (10-25)
    Arena2v2 = 4,       // Arena 2v2
    Arena3v3 = 5,       // Arena 3v3
    Battleground = 6,   // Random Battleground
    Skirmish = 7,       // Casual PvP
    Brawl = 8           // Weekly Brawl
}
```

### QueueRole

```csharp
[Flags]
public enum QueueRole : byte
{
    None = 0,
    Tank = 1,       // 0b0001
    Healer = 2,     // 0b0010
    Damage = 4      // 0b0100
}
```

### MatchmakingErrorCode

```csharp
public enum MatchmakingErrorCode : ushort
{
    Success = 0,
    AlreadyInQueue = 1,
    NotInQueue = 2,
    InvalidQueueType = 3,
    NoRoleSelected = 4,
    RoleNotAvailable = 5,        // Class can't play this role
    GearTooLow = 6,              // Item level requirement not met
    LevelTooLow = 7,
    ContentNotUnlocked = 8,      // Dungeon/Raid not unlocked
    DeserterDebuff = 9,          // Can't queue due to deserter
    GroupTooLarge = 10,          // Group exceeds queue limit
    GroupTooSmall = 11,          // Group doesn't meet minimum
    MixedFactions = 12,          // Horde+Alliance in group
    MatchExpired = 13,           // Accept window closed
    MatchCancelled = 14,         // Other player declined
    ServerFull = 15,             // Instance server unavailable
    InCombat = 16                // Can't queue in combat
}
```

### QueueInfoDto

```csharp
[MessagePackObject]
public class QueueInfoDto
{
    [Key(0)] public Guid QueueId { get; set; }
    [Key(1)] public QueueType QueueType { get; set; }
    [Key(2)] public int ActivityId { get; set; }        // Specific dungeon/raid
    [Key(3)] public QueueRole SelectedRoles { get; set; }
    [Key(4)] public DateTime JoinedAt { get; set; }
    [Key(5)] public int EstimatedWaitSec { get; set; }
}
```

### QueueStatusDto

```csharp
[MessagePackObject]
public class QueueStatusDto
{
    [Key(0)] public QueueType QueueType { get; set; }
    [Key(1)] public int TanksInQueue { get; set; }
    [Key(2)] public int HealersInQueue { get; set; }
    [Key(3)] public int DamageInQueue { get; set; }
    [Key(4)] public int AverageWaitSec { get; set; }
    [Key(5)] public bool ShortageBonus { get; set; }    // Call-to-Arms active
    [Key(6)] public QueueRole ShortageRole { get; set; }
}
```

### MatchFoundDto

```csharp
[MessagePackObject]
public class MatchFoundDto
{
    [Key(0)] public Guid MatchId { get; set; }
    [Key(1)] public QueueType QueueType { get; set; }
    [Key(2)] public int ActivityId { get; set; }
    [Key(3)] public string ActivityName { get; set; }
    [Key(4)] public QueueRole AssignedRole { get; set; }
    [Key(5)] public int AcceptDeadlineSec { get; set; }
    [Key(6)] public int AcceptedCount { get; set; }
    [Key(7)] public int RequiredCount { get; set; }
}
```

### Wichtige Konstanten

| Konstante | Wert | Beschreibung |
|-----------|------|--------------|
| `QUEUE_ACCEPT_WINDOW_SEC` | 30 | Zeit zum Akzeptieren eines Pops |
| `DESERTER_DURATION_SEC` | 1800 | 30 Minuten Deserter-Debuff |
| `QUEUE_UPDATE_INTERVAL_SEC` | 30 | Intervall für QueueEstimate |
| `MAX_QUEUES_SIMULTANEOUS` | 3 | Max gleichzeitige Queues |
| `MIN_DUNGEON_ILVL` | 200 | Minimum Item Level für Dungeons |
| `MIN_HEROIC_ILVL` | 280 | Minimum Item Level für Heroic |
| `MIN_MYTHIC_ILVL` | 340 | Minimum Item Level für Mythic |
| `ROLE_SHORTAGE_BONUS_GOLD` | 500 | Call-to-Arms Gold-Bonus |

---

## 📩 Messages

### Queue Grundfunktionen (2700-2710)

---

### QueueJoin (2700)

**Richtung:** 📤 Client → Server  
**Response:** QueueJoinResult (2701)

#### Beschreibung
Client möchte einer Matchmaking-Queue beitreten. Unterstützt mehrere Queues gleichzeitig (bis zu MAX_QUEUES_SIMULTANEOUS).

#### Payload

| Feld | Typ | Beschreibung |
|------|-----|--------------|
| `Type` | `MessageType` | = `QueueJoin` (2700) |
| `RequestId` | `uint` | Eindeutige Request-ID für Korrelation |
| `QueueType` | `QueueType` | Art der Queue (Dungeon, Raid, etc.) |
| `ActivityId` | `int` | Spezifische Aktivität (0 = Random) |
| `SelectedRoles` | `QueueRole` | Flags: Tank, Healer, Damage |

#### Validierung
- Spieler nicht bereits in dieser Queue
- Mindestens eine Rolle ausgewählt
- Klasse kann gewählte Rolle(n) spielen
- Item Level erfüllt Anforderungen
- Kein Deserter-Debuff aktiv
- Content ist freigeschaltet

---

### QueueJoinResult (2701)

**Richtung:** 📥 Server → Client  
**Trigger:** QueueJoin (2700)

#### Beschreibung
Ergebnis des Queue-Beitrittsversuchs.

#### Payload

| Feld | Typ | Beschreibung |
|------|-----|--------------|
| `Type` | `MessageType` | = `QueueJoinResult` (2701) |
| `RequestId` | `uint` | Korrelation mit QueueJoin |
| `Success` | `bool` | Erfolgreich beigetreten |
| `ErrorCode` | `MatchmakingErrorCode` | Fehlergrund (wenn !Success) |
| `QueueInfo` | `QueueInfoDto?` | Queue-Details (wenn Success) |

---

### QueueLeave (2702)

**Richtung:** 📤 Client → Server  
**Response:** QueueUpdate (2703) mit PlayersInQueue = 0

#### Beschreibung
Client verlässt eine aktive Queue. Kein Deserter-Debuff, da noch kein Match gestartet.

#### Payload

| Feld | Typ | Beschreibung |
|------|-----|--------------|
| `Type` | `MessageType` | = `QueueLeave` (2702) |
| `QueueType` | `QueueType` | Welche Queue verlassen wird |

#### Validierung
- Spieler muss in dieser Queue sein
- Wenn bereits in Accept-Phase: Deserter-Debuff

---

### QueueUpdate (2703)

**Richtung:** 📥 Server → Client  
**Trigger:** Periodisch, bei Änderungen

#### Beschreibung
Aktualisiert den Spieler über den aktuellen Queue-Status.

#### Payload

| Feld | Typ | Beschreibung |
|------|-----|--------------|
| `Type` | `MessageType` | = `QueueUpdate` (2703) |
| `Status` | `QueueStatusDto` | Aktueller Queue-Status |

---

### QueueEstimate (2704)

**Richtung:** 📥 Server → Client  
**Trigger:** Alle QUEUE_UPDATE_INTERVAL_SEC

#### Beschreibung
Aktualisierte Wartezeit-Schätzung basierend auf aktueller Queue-Aktivität.

#### Payload

| Feld | Typ | Beschreibung |
|------|-----|--------------|
| `Type` | `MessageType` | = `QueueEstimate` (2704) |
| `QueueType` | `QueueType` | Für welche Queue |
| `EstimatedWaitSec` | `int` | Geschätzte Wartezeit in Sekunden |
| `QueuePosition` | `int` | Position in der Queue (0 = unbekannt) |

---

### QueuePop (2705)

**Richtung:** 📥 Server → Client  
**Trigger:** Match gefunden

#### Beschreibung
Ein passendes Match wurde gefunden. Spieler hat QUEUE_ACCEPT_WINDOW_SEC Zeit zum Akzeptieren.

#### Payload

| Feld | Typ | Beschreibung |
|------|-----|--------------|
| `Type` | `MessageType` | = `QueuePop` (2705) |
| `Match` | `MatchFoundDto` | Match-Details |

#### Verhalten
- UI zeigt Accept/Decline Dialog
- Timer läuft ab nach AcceptDeadlineSec
- Spieler kann weiter spielen während Timer läuft

---

### QueueAccept (2706)

**Richtung:** 📤 Client → Server  
**Response:** (bei vollem Accept) Instance-Teleport oder QueueTimeout (2708)

#### Beschreibung
Spieler akzeptiert das gefundene Match.

#### Payload

| Feld | Typ | Beschreibung |
|------|-----|--------------|
| `Type` | `MessageType` | = `QueueAccept` (2706) |
| `MatchId` | `Guid` | ID des Matches aus QueuePop |

#### Validierung
- MatchId muss gültig sein
- Accept-Fenster noch offen
- Spieler war Teil dieses Matches

---

### QueueDecline (2707)

**Richtung:** 📤 Client → Server  
**Response:** QueueDeserter (2710)

#### Beschreibung
Spieler lehnt das gefundene Match ab. Führt zu Deserter-Debuff.

#### Payload

| Feld | Typ | Beschreibung |
|------|-----|--------------|
| `Type` | `MessageType` | = `QueueDecline` (2707) |
| `MatchId` | `Guid` | ID des Matches aus QueuePop |

#### Konsequenzen
- Deserter-Debuff für DESERTER_DURATION_SEC
- Andere Spieler werden zurück in Queue gesetzt
- Kein Penalty für andere Spieler

---

### QueueTimeout (2708)

**Richtung:** 📥 Server → Client  
**Trigger:** Accept-Fenster abgelaufen ohne vollständige Annahme

#### Beschreibung
Das Match wurde nicht rechtzeitig von allen akzeptiert.

#### Payload

| Feld | Typ | Beschreibung |
|------|-----|--------------|
| `Type` | `MessageType` | = `QueueTimeout` (2708) |
| `MatchId` | `Guid` | ID des abgelaufenen Matches |
| `ReQueued` | `bool` | Automatisch wieder in Queue gesetzt |
| `Reason` | `string` | Grund (z.B. "1 Spieler hat nicht akzeptiert") |

---

### QueueKick (2709)

**Richtung:** 📥 Server → Client  
**Trigger:** Administrative Aktion oder Systemfehler

#### Beschreibung
Spieler wurde aus der Queue entfernt (nicht durch eigene Aktion).

#### Payload

| Feld | Typ | Beschreibung |
|------|-----|--------------|
| `Type` | `MessageType` | = `QueueKick` (2709) |
| `QueueType` | `QueueType` | Aus welcher Queue |
| `Reason` | `string` | Grund für den Kick |
| `ErrorCode` | `MatchmakingErrorCode` | Maschinenlesbarer Fehlercode |

#### Mögliche Gründe
- Server-Wartung
- Spieler wurde offline
- Gruppenleiter hat Queue verlassen
- Eligibility-Änderung (z.B. Gear verkauft)

---

### QueueDeserter (2710)

**Richtung:** 📥 Server → Client  
**Trigger:** Decline, Timeout ohne Accept, Leave während Instance

#### Beschreibung
Spieler hat Deserter-Debuff erhalten und kann temporär nicht queuen.

#### Payload

| Feld | Typ | Beschreibung |
|------|-----|--------------|
| `Type` | `MessageType` | = `QueueDeserter` (2710) |
| `DurationSec` | `int` | Verbleibende Dauer des Debuffs |
| `ExpiresAt` | `DateTime` | Exakter Ablaufzeitpunkt (UTC) |
| `Reason` | `string` | Grund für Deserter-Status |

---

### Rollen-Auswahl (2720-2722)

---

### RoleSelect (2720)

**Richtung:** 📤 Client → Server  
**Response:** RoleConfirm (2721)

#### Beschreibung
Spieler wählt oder ändert seine Queue-Rollen für zukünftige Queues.

#### Payload

| Feld | Typ | Beschreibung |
|------|-----|--------------|
| `Type` | `MessageType` | = `RoleSelect` (2720) |
| `RequestId` | `uint` | Eindeutige Request-ID für Korrelation |
| `Roles` | `QueueRole` | Flags: Kombinationen von Tank/Healer/Damage |

#### Validierung
- Mindestens eine Rolle ausgewählt
- Spielerklasse kann gewählte Rollen spielen

---

### RoleConfirm (2721)

**Richtung:** 📥 Server → Client  
**Trigger:** RoleSelect (2720)

#### Beschreibung
Bestätigung der Rollen-Auswahl.

#### Payload

| Feld | Typ | Beschreibung |
|------|-----|--------------|
| `Type` | `MessageType` | = `RoleConfirm` (2721) |
| `RequestId` | `uint` | Korrelation mit RoleSelect |
| `Success` | `bool` | Rollen erfolgreich gesetzt |
| `ActiveRoles` | `QueueRole` | Aktuell aktive Rollen |
| `AvailableRoles` | `QueueRole` | Welche Rollen die Klasse spielen kann |
| `ErrorCode` | `MatchmakingErrorCode` | Fehlergrund (wenn !Success) |

---

### RoleShortage (2722)

**Richtung:** 📥 Server → Client  
**Trigger:** Call-to-Arms Aktivierung/Deaktivierung

#### Beschreibung
Informiert über Rollen-Mangel und mögliche Bonus-Belohnungen (Call-to-Arms).

#### Payload

| Feld | Typ | Beschreibung |
|------|-----|--------------|
| `Type` | `MessageType` | = `RoleShortage` (2722) |
| `QueueType` | `QueueType` | Für welche Queue |
| `ShortageRole` | `QueueRole` | Welche Rolle Mangel hat |
| `BonusActive` | `bool` | Call-to-Arms aktiv |
| `BonusGold` | `int` | Extra Gold als Bonus |
| `BonusItems` | `int[]` | Item-IDs als Bonus |

---

### Skirmish (2730-2732)

---

### SkirmishJoin (2730)

**Richtung:** 📤 Client → Server  
**Response:** SkirmishUpdate (2732)

#### Beschreibung
Spieler möchte einem Skirmish (casual PvP) beitreten. Schnelleres Matching, weniger strenge Regeln.

#### Payload

| Feld | Typ | Beschreibung |
|------|-----|--------------|
| `Type` | `MessageType` | = `SkirmishJoin` (2730) |
| `RequestId` | `uint` | Eindeutige Request-ID für Korrelation |
| `Solo` | `bool` | Solo-Queue (keine Gruppe) |

#### Eigenschaften
- Kein Item Level Requirement
- Kein Rollen-System
- Schnelleres Matching
- Reduzierte Rewards

---

### SkirmishLeave (2731)

**Richtung:** 📤 Client → Server  
**Response:** SkirmishUpdate (2732) mit InQueue = false

#### Beschreibung
Spieler verlässt die Skirmish-Queue.

#### Payload

| Feld | Typ | Beschreibung |
|------|-----|--------------|
| `Type` | `MessageType` | = `SkirmishLeave` (2731) |

#### Hinweis
Kein Deserter-Debuff für Skirmish-Queues.

---

### SkirmishUpdate (2732)

**Richtung:** 📥 Server → Client  
**Trigger:** SkirmishJoin, SkirmishLeave, Status-Änderung

#### Beschreibung
Status-Update für Skirmish-Queue.

#### Payload

| Feld | Typ | Beschreibung |
|------|-----|--------------|
| `Type` | `MessageType` | = `SkirmishUpdate` (2732) |
| `RequestId` | `uint` | Korrelation mit SkirmishJoin (0 bei Updates) |
| `Success` | `bool` | Operation erfolgreich |
| `InQueue` | `bool` | Aktuell in Skirmish-Queue |
| `EstimatedWaitSec` | `int` | Geschätzte Wartezeit |
| `PlayersWaiting` | `int` | Anzahl wartender Spieler |
| `ErrorCode` | `MatchmakingErrorCode` | Fehlergrund (wenn !Success) |

---

## 📎 Anhang

### MessageType Enum (Auszug)

```csharp
// ═══════════════════════════════════════════════════════════════
// CATEGORY 27: MATCHMAKING (2700-2799)
// ═══════════════════════════════════════════════════════════════

// --- Queue Grundfunktionen (2700-2710) ---
QueueJoin = 2700,
QueueJoinResult = 2701,
QueueLeave = 2702,
QueueUpdate = 2703,
QueueEstimate = 2704,
QueuePop = 2705,
QueueAccept = 2706,
QueueDecline = 2707,
QueueTimeout = 2708,
QueueKick = 2709,
QueueDeserter = 2710,

// --- Rollen-Auswahl (2720-2722) ---
RoleSelect = 2720,
RoleConfirm = 2721,
RoleShortage = 2722,

// --- Skirmish (2730-2732) ---
SkirmishJoin = 2730,
SkirmishLeave = 2731,
SkirmishUpdate = 2732,
```

### Request/Response Paare

| Request | ID | Response | ID | Beschreibung |
|---------|----|-----------|----|--------------|
| QueueJoin | 2700 | QueueJoinResult | 2701 | Queue beitreten |
| QueueLeave | 2702 | QueueUpdate | 2703 | Queue verlassen |
| QueueAccept | 2706 | - | - | Match akzeptieren (führt zu Teleport) |
| QueueDecline | 2707 | QueueDeserter | 2710 | Match ablehnen |
| RoleSelect | 2720 | RoleConfirm | 2721 | Rollen auswählen |
| SkirmishJoin | 2730 | SkirmishUpdate | 2732 | Skirmish beitreten |
| SkirmishLeave | 2731 | SkirmishUpdate | 2732 | Skirmish verlassen |

### Server-initiated Messages

| Message | ID | Beschreibung |
|---------|-----|--------------|
| QueueUpdate | 2703 | Periodische Queue-Status Updates |
| QueueEstimate | 2704 | Wartezeit-Schätzungen |
| QueuePop | 2705 | Match gefunden Notification |
| QueueTimeout | 2708 | Accept-Fenster abgelaufen |
| QueueKick | 2709 | Aus Queue entfernt |
| QueueDeserter | 2710 | Deserter-Debuff erhalten |
| RoleShortage | 2722 | Call-to-Arms Updates |

### Fire-and-Forget Messages

| Message | ID | Beschreibung |
|---------|-----|--------------|
| QueueLeave | 2702 | Keine explizite Bestätigung nötig |
| SkirmishLeave | 2731 | Keine explizite Bestätigung nötig |

### Datei-Struktur

```
shared/Mmo.Shared/
├── Messaging/
│   ├── Enums/
│   │   ├── MessageType.cs          # QueueJoin = 2700, ...
│   │   ├── QueueType.cs            # Dungeon, Raid, Arena, ...
│   │   ├── QueueRole.cs            # Tank, Healer, Damage
│   │   └── MatchmakingErrorCode.cs # Error codes
│   └── Matchmaking/
│       ├── QueueJoinMessage.cs
│       ├── QueueJoinResultMessage.cs
│       ├── QueueLeaveMessage.cs
│       ├── QueueUpdateMessage.cs
│       ├── QueueEstimateMessage.cs
│       ├── QueuePopMessage.cs
│       ├── QueueAcceptMessage.cs
│       ├── QueueDeclineMessage.cs
│       ├── QueueTimeoutMessage.cs
│       ├── QueueKickMessage.cs
│       ├── QueueDeserterMessage.cs
│       ├── RoleSelectMessage.cs
│       ├── RoleConfirmMessage.cs
│       ├── RoleShortageMessage.cs
│       ├── SkirmishJoinMessage.cs
│       ├── SkirmishLeaveMessage.cs
│       └── SkirmishUpdateMessage.cs
└── Dtos/
    └── Matchmaking/
        ├── QueueInfoDto.cs
        ├── QueueStatusDto.cs
        └── MatchFoundDto.cs
```

---

**Letzte Aktualisierung:** 2026-01-02  
**Version:** 3.0.0  
**Status:** ✅ Vollständig spezifiziert (17 Messages)

[← Zurück zur Übersicht](Message-Reference.md)

Source: docs/03-messages/27-matchmaking.md
