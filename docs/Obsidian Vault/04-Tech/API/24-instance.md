# 🏛️ Instance Messages (2400-2499)

**Kategorie:** 24  
**Range:** 2400-2452  
**Phase:** Prototyp  
**Status:** 🟢 In Entwicklung

[← Zurück zur Übersicht](Message-Reference.md)

---

## 📋 Inhaltsverzeichnis

- [Übersicht](#übersicht)
- [Instance Flow](#instance-flow)
  - [Server-Authoritative Architektur](#server-authoritative-architektur)
  - [Instance Lifecycle Flow](#instance-lifecycle-flow)
  - [Dungeon/Raid Finder Flow](#dungeonraid-finder-flow)
- [DTOs und Enums](#dtos-und-enums)
  - [InstanceType Enum](#instancetype-enum)
  - [InstanceDifficulty Enum](#instancedifficulty-enum)
  - [FinderRole Enum](#finderrole-enum)
  - [InstanceErrorCode Enum](#instanceerrorcode-enum)
  - [InstanceInfoDto](#instanceinfodto)
  - [LockoutDto](#lockoutdto)
  - [EncounterDto](#encounterdto)
  - [FinderStatusDto](#finderstatusdto)
  - [Wichtige Konstanten](#wichtige-konstanten)
- [Messages](#messages)
  - [Instance Management (2400-2412)](#instance-management-2400-2412)
  - [Encounter Events (2420-2425)](#encounter-events-2420-2425)
  - [Raid Management (2430-2435)](#raid-management-2430-2435)
  - [Dungeon Finder (2440-2445)](#dungeon-finder-2440-2445)
  - [Raid Finder (2450-2452)](#raid-finder-2450-2452)
- [Anhang](#anhang)

---

## Übersicht

Diese Kategorie umfasst alle Messages für **Instanced Content** (Dungeons, Raids, Finder-Systeme) im 2DMMO.

**Sub-Kategorien:**
| Bereich | Range | Beschreibung |
|---------|-------|--------------|
| Instance Management | 2400-2412 | Erstellen, Beitreten, Verlassen, Reset, Lockout |
| Encounter Events | 2420-2425 | Boss-Kämpfe, Wipes, Checkpoints |
| Raid Management | 2430-2435 | Raid-Konvertierung, Gruppen, Ready-Checks |
| Dungeon Finder | 2440-2445 | Dungeon-Matchmaking |
| Raid Finder | 2450-2452 | Raid-Matchmaking |

---

## Instance Flow

### Server-Authoritative Architektur

```
┌─────────────────────────────────────────────────────────────────┐
│                    INSTANCE SYSTEM                              │
│                                                                 │
│  ┌─────────────┐    ┌─────────────┐    ┌─────────────┐         │
│  │   DUNGEON   │    │    RAID     │    │   FINDER    │         │
│  │   MANAGER   │    │   MANAGER   │    │   SERVICE   │         │
│  └──────┬──────┘    └──────┬──────┘    └──────┬──────┘         │
│         │                  │                  │                 │
│         └──────────────────┼──────────────────┘                 │
│                            ▼                                    │
│              ┌─────────────────────────┐                        │
│              │    INSTANCE SERVER      │                        │
│              │  • Encounter Logic      │                        │
│              │  • Lockout Management   │                        │
│              │  • Progress Tracking    │                        │
│              └─────────────────────────┘                        │
│                            │                                    │
│              Server ist AUTORITATIV                             │
│              Client zeigt nur an                                │
└─────────────────────────────────────────────────────────────────┘
```

### Instance Lifecycle Flow

```
Client                    Instance Server                 DB
  │                            │                           │
  │  InstanceCreate (2400)     │                           │
  │───────────────────────────►│                           │
  │                            │  Create Instance          │
  │                            │──────────────────────────►│
  │  InstanceCreateResult(2401)│                           │
  │◄───────────────────────────│                           │
  │                            │                           │
  │  InstanceJoin (2402)       │                           │
  │───────────────────────────►│                           │
  │                            │  Validate + Add Player    │
  │  InstanceJoinResult (2403) │                           │
  │◄───────────────────────────│                           │
  │                            │                           │
  │       [ENCOUNTER EVENTS]   │                           │
  │  InstanceEncounterStart    │                           │
  │◄───────────────────────────│                           │
  │  InstanceEncounterUpdate   │                           │
  │◄───────────────────────────│  (periodic)               │
  │  InstanceBossKill (2423)   │                           │
  │◄───────────────────────────│                           │
  │                            │  Save Lockout             │
  │  InstanceLockout (2407)    │──────────────────────────►│
  │◄───────────────────────────│                           │
  │                            │                           │
  │  InstanceLeave (2404)      │                           │
  │───────────────────────────►│                           │
```

### Dungeon/Raid Finder Flow

```
Client                    Finder Service                  Match
  │                            │                           │
  │  DungeonFinderJoin (2440)  │                           │
  │───────────────────────────►│                           │
  │                            │  Add to Queue             │
  │  DungeonFinderUpdate(2442) │                           │
  │◄───────────────────────────│  (Wait Time, Position)    │
  │                            │                           │
  │                            │  Match Found!             │
  │                            │◄──────────────────────────│
  │  DungeonFinderProposal     │                           │
  │◄───────────────────────────│  (2443)                   │
  │                            │                           │
  │  DungeonFinderAccept(2444) │                           │
  │───────────────────────────►│                           │
  │                            │  All Accepted?            │
  │                            │──────────────────────────►│
  │  InstanceJoinResult (2403) │                           │
  │◄───────────────────────────│  (Teleport to Dungeon)    │
```

---

## DTOs und Enums

### InstanceType Enum

```csharp
public enum InstanceType : byte
{
    Dungeon = 0,           // 5-Mann Dungeon
    HeroicDungeon = 1,     // Heroic 5-Mann
    MythicDungeon = 2,     // Mythic+ Dungeon
    Raid10 = 3,            // 10-Mann Raid
    Raid25 = 4,            // 25-Mann Raid
    Raid40 = 5,            // 40-Mann Legacy Raid
    Scenario = 6,          // 1-3 Mann Scenario
    WorldBoss = 7          // Open-World Boss Instance
}
```

### InstanceDifficulty Enum

```csharp
public enum InstanceDifficulty : byte
{
    Normal = 0,            // Standard-Schwierigkeit
    Heroic = 1,            // Erhöhte Schwierigkeit
    Mythic = 2,            // Höchste Schwierigkeit
    MythicPlus = 3,        // Mythic+ mit Keystone Level
    Timewalking = 4,       // Level-skaliert
    LegacyNormal = 5,      // Legacy Normal (trivial)
    LegacyHeroic = 6       // Legacy Heroic (trivial)
}
```

### FinderRole Enum

```csharp
public enum FinderRole : byte
{
    Tank = 0,              // Tank-Rolle
    Healer = 1,            // Heiler-Rolle
    DamageDealer = 2       // DPS-Rolle
}
```

### InstanceErrorCode Enum

```csharp
public enum InstanceErrorCode : byte
{
    None = 0,
    InstanceNotFound = 1,
    AlreadyInInstance = 2,
    NotInInstance = 3,
    InstanceFull = 4,
    LevelRequirementNotMet = 5,
    GearRequirementNotMet = 6,
    QuestRequirementNotMet = 7,
    AlreadyLocked = 8,
    NotPartyLeader = 9,
    NotRaidLeader = 10,
    InstanceLimitReached = 11,
    FinderCooldown = 12,
    RoleNotSelected = 13,
    InvalidDifficulty = 14,
    EncounterInProgress = 15,
    InstanceExpired = 16
}
```

### InstanceInfoDto

```csharp
[MessagePackObject]
public class InstanceInfoDto
{
    [Key(0)] public Guid InstanceId { get; set; }
    [Key(1)] public int TemplateId { get; set; }
    [Key(2)] public string Name { get; set; }
    [Key(3)] public InstanceType Type { get; set; }
    [Key(4)] public InstanceDifficulty Difficulty { get; set; }
    [Key(5)] public int MaxPlayers { get; set; }
    [Key(6)] public int CurrentPlayers { get; set; }
    [Key(7)] public List<int> CompletedBosses { get; set; }
    [Key(8)] public int TotalBosses { get; set; }
    [Key(9)] public long CreatedAt { get; set; }
    [Key(10)] public long ExpiresAt { get; set; }
}
```

### LockoutDto

```csharp
[MessagePackObject]
public class LockoutDto
{
    [Key(0)] public Guid LockoutId { get; set; }
    [Key(1)] public int InstanceTemplateId { get; set; }
    [Key(2)] public string InstanceName { get; set; }
    [Key(3)] public InstanceDifficulty Difficulty { get; set; }
    [Key(4)] public List<int> KilledBosses { get; set; }
    [Key(5)] public int TotalBosses { get; set; }
    [Key(6)] public long ExpiresAt { get; set; }
    [Key(7)] public bool IsExtended { get; set; }
}
```

### EncounterDto

```csharp
[MessagePackObject]
public class EncounterDto
{
    [Key(0)] public int EncounterId { get; set; }
    [Key(1)] public string BossName { get; set; }
    [Key(2)] public float HealthPercent { get; set; }
    [Key(3)] public int Phase { get; set; }
    [Key(4)] public int EnrageTimer { get; set; }
    [Key(5)] public Dictionary<string, int> Mechanics { get; set; }
}
```

### FinderStatusDto

```csharp
[MessagePackObject]
public class FinderStatusDto
{
    [Key(0)] public bool InQueue { get; set; }
    [Key(1)] public int EstimatedWaitSeconds { get; set; }
    [Key(2)] public int QueuePosition { get; set; }
    [Key(3)] public List<FinderRole> SelectedRoles { get; set; }
    [Key(4)] public List<int> SelectedDungeons { get; set; }
    [Key(5)] public bool TankNeeded { get; set; }
    [Key(6)] public bool HealerNeeded { get; set; }
    [Key(7)] public int DpsNeeded { get; set; }
}
```

### Wichtige Konstanten

| Konstante | Wert | Beschreibung |
|-----------|------|--------------|
| `MAX_DUNGEON_PLAYERS` | 5 | Maximale Spieler im Dungeon |
| `MAX_RAID_PLAYERS` | 40 | Maximale Spieler im Raid |
| `LOCKOUT_DURATION_HOURS` | 168 | Raid-Lockout Dauer (7 Tage) |
| `DUNGEON_LIMIT_PER_HOUR` | 10 | Max. Dungeons pro Stunde |
| `FINDER_COOLDOWN_SEC` | 30 | Cooldown nach Ablehnen |
| `PROPOSAL_TIMEOUT_SEC` | 40 | Zeit zum Akzeptieren |
| `READY_CHECK_TIMEOUT_SEC` | 30 | Ready-Check Timeout |
| `WIPE_RELEASE_DELAY_SEC` | 30 | Zeit bis Release nach Wipe |
| `INSTANCE_EXPIRE_HOURS` | 2 | Leere Instance verfällt |

---

## Messages

---

## Instance Management (2400-2412)

---

### InstanceCreate (2400)

**Richtung:** 📤 Client → Server  
**Frequenz:** Bei Bedarf (Dungeon-Eingang)  
**Authentifizierung:** Ja  
**Spezielle Rechte:** Party-Leader

#### Beschreibung
Client (Party-Leader) erstellt eine neue Instanz. Server validiert Berechtigung, Gruppenzusammensetzung und erstellt die Instanz.

#### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| RequestId | Guid | Eindeutige Request-ID | Ja |
| TemplateId | int | ID des Dungeon/Raid-Templates | Ja |
| Difficulty | InstanceDifficulty | Gewünschte Schwierigkeit | Ja |
| KeystoneLevel | int | Mythic+ Level (0 wenn nicht M+) | Nein |

#### Erwartete Response
- **Bei Erfolg:** `InstanceCreateResult` (2401) mit InstanceInfo
- **Bei Fehler:** `InstanceCreateResult` (2401) mit ErrorCode

---

### InstanceCreateResult (2401)

**Richtung:** 📥 Server → Client  
**Frequenz:** Antwort auf InstanceCreate  
**Authentifizierung:** Nein

#### Beschreibung
Server-Antwort auf Instanz-Erstellung mit Erfolg oder Fehlercode.

#### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| RequestId | Guid | Request-ID zur Korrelation | Ja |
| Success | bool | Erfolg true/false | Ja |
| ErrorCode | InstanceErrorCode | Fehlercode bei Misserfolg | Nein |
| InstanceInfo | InstanceInfoDto | Instanz-Details bei Erfolg | Nein |

---

### InstanceJoin (2402)

**Richtung:** 📤 Client → Server  
**Frequenz:** Bei Betreten einer Instanz  
**Authentifizierung:** Ja

#### Beschreibung
Client betritt eine bestehende Instanz. Kann durch Dungeon-Eingang oder Teleport erfolgen.

#### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| RequestId | Guid | Eindeutige Request-ID | Ja |
| InstanceId | Guid | ID der Ziel-Instanz | Ja |
| FromPortal | bool | Betreten via Portal (nicht Teleport) | Ja |

#### Erwartete Response
- **Bei Erfolg:** `InstanceJoinResult` (2403) mit Success=true
- **Bei Fehler:** `InstanceJoinResult` (2403) mit ErrorCode

---

### InstanceJoinResult (2403)

**Richtung:** 📥 Server → Client  
**Frequenz:** Antwort auf InstanceJoin  
**Authentifizierung:** Nein

#### Beschreibung
Server-Antwort auf Instanz-Beitritt. Bei Erfolg enthält ZoneId für Teleport.

#### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| RequestId | Guid | Request-ID zur Korrelation | Ja |
| Success | bool | Erfolg true/false | Ja |
| ErrorCode | InstanceErrorCode | Fehlercode bei Misserfolg | Nein |
| InstanceInfo | InstanceInfoDto | Instanz-Details | Nein |
| ZoneId | int | Zone-ID für Teleport | Nein |
| SpawnPosition | Vector2 | Spawn-Position in Instanz | Nein |

---

### InstanceLeave (2404)

**Richtung:** 📤 Client → Server  
**Frequenz:** Bei Verlassen der Instanz  
**Authentifizierung:** Ja

#### Beschreibung
Client verlässt die aktuelle Instanz. Server teleportiert zurück zum Eingang.

#### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| RequestId | Guid | Eindeutige Request-ID | Ja |

#### Erwartete Response
- **Implizit:** `ZoneTransferResponse` (107) mit Teleport zum Eingang

---

### InstanceReset (2405)

**Richtung:** 📤 Client → Server  
**Frequenz:** Bei manueller Reset-Anfrage  
**Authentifizierung:** Ja  
**Spezielle Rechte:** Party/Raid-Leader

#### Beschreibung
Party-Leader setzt die Instanz zurück. Nur möglich wenn keine Spieler in der Instanz sind.

#### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| RequestId | Guid | Eindeutige Request-ID | Ja |
| InstanceId | Guid | ID der zu resettenden Instanz | Ja |

#### Erwartete Response
- **Bei Erfolg:** `InstanceResetResult` (2406) mit Success=true
- **Bei Fehler:** `InstanceResetResult` (2406) mit ErrorCode

---

### InstanceResetResult (2406)

**Richtung:** 📥 Server → Client  
**Frequenz:** Antwort auf InstanceReset  
**Authentifizierung:** Nein

#### Beschreibung
Server-Antwort auf Instanz-Reset.

#### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| RequestId | Guid | Request-ID zur Korrelation | Ja |
| Success | bool | Erfolg true/false | Ja |
| ErrorCode | InstanceErrorCode | Fehlercode bei Misserfolg | Nein |
| NewInstanceId | Guid | ID der neuen Instanz | Nein |

---

### InstanceLockout (2407)

**Richtung:** 📥 Server → Client  
**Frequenz:** Nach Boss-Kill oder bei Login  
**Authentifizierung:** Nein

#### Beschreibung
Server informiert Client über neuen oder aktualisierten Lockout.

#### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Lockout | LockoutDto | Lockout-Informationen | Ja |
| IsNew | bool | Neuer Lockout (nicht Update) | Ja |

---

### InstanceLockoutList (2408)

**Richtung:** 📥 Server → Client  
**Frequenz:** Bei Login / UI-Öffnung  
**Authentifizierung:** Nein

#### Beschreibung
Server sendet vollständige Liste aller aktiven Lockouts.

#### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Lockouts | List\<LockoutDto\> | Alle aktiven Lockouts | Ja |

---

### InstanceDifficultySet (2409)

**Richtung:** 📤 Client → Server  
**Frequenz:** Bei Schwierigkeits-Änderung  
**Authentifizierung:** Ja  
**Spezielle Rechte:** Party/Raid-Leader

#### Beschreibung
Leader ändert die Dungeon/Raid-Schwierigkeit. Nur außerhalb der Instanz möglich.

#### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| RequestId | Guid | Eindeutige Request-ID | Ja |
| TemplateId | int | Dungeon/Raid-ID | Ja |
| Difficulty | InstanceDifficulty | Neue Schwierigkeit | Ja |

#### Erwartete Response
- **Broadcast:** Alle Party-Mitglieder erhalten Update via Party-Channel

---

### InstanceDifficultyVote (2410)

**Richtung:** 📤 Client → Server  
**Frequenz:** Bei Schwierigkeits-Abstimmung  
**Authentifizierung:** Ja

#### Beschreibung
Spieler stimmt für Schwierigkeits-Änderung (wenn Vote-System aktiv).

#### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| RequestId | Guid | Eindeutige Request-ID | Ja |
| VoteId | Guid | ID der Abstimmung | Ja |
| Accept | bool | Zustimmung ja/nein | Ja |

---

### InstanceSaved (2411)

**Richtung:** 📥 Server → Client  
**Frequenz:** Nach Speicherung des Fortschritts  
**Authentifizierung:** Nein

#### Beschreibung
Server bestätigt dass Instanz-Fortschritt gespeichert wurde.

#### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| InstanceId | Guid | ID der Instanz | Ja |
| SavePoint | string | Bezeichnung des Speicherpunkts | Ja |
| Timestamp | long | Unix-Timestamp der Speicherung | Ja |

---

### InstanceExtend (2412)

**Richtung:** 📤 Client → Server  
**Frequenz:** Bei Lockout-Verlängerung  
**Authentifizierung:** Ja

#### Beschreibung
Spieler verlängert einen ablaufenden Lockout um eine weitere Reset-Periode.

#### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| RequestId | Guid | Eindeutige Request-ID | Ja |
| LockoutId | Guid | ID des zu verlängernden Lockouts | Ja |

#### Erwartete Response
- **Bei Erfolg:** `InstanceLockout` (2407) mit aktualisiertem Lockout
- **Bei Fehler:** `ErrorMessage` (910)

---

## Encounter Events (2420-2425)

---

### InstanceEncounterStart (2420)

**Richtung:** 📥 Server → Client  
**Frequenz:** Bei Encounter-Beginn  
**Authentifizierung:** Nein

#### Beschreibung
Server signalisiert Start eines Boss-Encounters. Client aktiviert Combat-UI.

#### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| EncounterId | int | ID des Encounters | Ja |
| BossName | string | Anzeigename des Bosses | Ja |
| BossHealth | long | Maximale HP des Bosses | Ja |
| EnrageTimer | int | Sekunden bis Enrage (0=kein Timer) | Ja |
| PhaseCount | int | Anzahl der Phasen | Ja |

---

### InstanceEncounterEnd (2421)

**Richtung:** 📥 Server → Client  
**Frequenz:** Bei Encounter-Ende  
**Authentifizierung:** Nein

#### Beschreibung
Server signalisiert Ende eines Encounters (Sieg, Wipe, Reset).

#### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| EncounterId | int | ID des Encounters | Ja |
| Success | bool | Encounter erfolgreich | Ja |
| DurationSeconds | int | Kampfdauer in Sekunden | Ja |
| WipeReason | string | Grund bei Wipe (optional) | Nein |

---

### InstanceEncounterUpdate (2422)

**Richtung:** 📥 Server → Client  
**Frequenz:** Periodisch während Encounter (1-5 Hz)  
**Authentifizierung:** Nein

#### Beschreibung
Periodisches Update während Boss-Encounter mit Phase, HP, Timer.

#### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| EncounterId | int | ID des Encounters | Ja |
| HealthPercent | float | Aktuelle HP in Prozent | Ja |
| Phase | int | Aktuelle Phase (1-based) | Ja |
| EnrageRemaining | int | Sekunden bis Enrage | Ja |
| ActiveMechanics | List\<string\> | Aktive Mechaniken | Nein |

---

### InstanceBossKill (2423)

**Richtung:** 📥 Server → Client  
**Frequenz:** Bei Boss-Tod  
**Authentifizierung:** Nein

#### Beschreibung
Server meldet erfolgreichen Boss-Kill. Trigger für Loot und Lockout.

#### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| EncounterId | int | ID des Encounters | Ja |
| BossName | string | Name des getöteten Bosses | Ja |
| FirstKill | bool | Erster Kill für Spieler | Ja |
| ServerFirst | bool | Server-First Kill | Ja |
| AchievementIds | List\<int\> | Freigeschaltete Achievements | Nein |

---

### InstanceWipe (2424)

**Richtung:** 📥 Server → Client  
**Frequenz:** Bei Gruppen-Wipe  
**Authentifizierung:** Nein

#### Beschreibung
Server signalisiert Gruppen-Wipe (alle tot). UI zeigt Release-Timer.

#### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| EncounterId | int | ID des Encounters (0 wenn Trash) | Ja |
| ReleaseDelay | int | Sekunden bis Release möglich | Ja |
| GraveyardPosition | Vector2 | Respawn-Position | Ja |

---

### InstanceCheckpoint (2425)

**Richtung:** 📥 Server → Client  
**Frequenz:** Bei Checkpoint-Erreichen  
**Authentifizierung:** Nein

#### Beschreibung
Server bestätigt Erreichen eines Checkpoints. Neuer Spawn-Point bei Wipe.

#### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| CheckpointId | int | ID des Checkpoints | Ja |
| CheckpointName | string | Anzeigename | Ja |
| SpawnPosition | Vector2 | Neue Spawn-Position | Ja |

---

## Raid Management (2430-2435)

---

### RaidConvert (2430)

**Richtung:** 📤 Client → Server  
**Frequenz:** Bei Gruppen-Konvertierung  
**Authentifizierung:** Ja  
**Spezielle Rechte:** Party-Leader

#### Beschreibung
Party-Leader konvertiert 5-Mann-Gruppe zu Raid (oder umgekehrt).

#### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| RequestId | Guid | Eindeutige Request-ID | Ja |
| ToRaid | bool | true=zu Raid, false=zu Gruppe | Ja |

#### Erwartete Response
- **Bei Erfolg:** Party-Update Broadcast an alle Mitglieder
- **Bei Fehler:** `ErrorMessage` (910)

---

### RaidDisband (2431)

**Richtung:** 📤 Client → Server  
**Frequenz:** Bei Raid-Auflösung  
**Authentifizierung:** Ja  
**Spezielle Rechte:** Raid-Leader

#### Beschreibung
Raid-Leader löst den Raid auf. Alle Mitglieder werden entfernt.

#### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| RequestId | Guid | Eindeutige Request-ID | Ja |
| Confirm | bool | Bestätigung erforderlich | Ja |

#### Erwartete Response
- **Bei Erfolg:** `PartyDisbanded` Broadcast an alle
- **Bei Fehler:** `ErrorMessage` (910)

---

### RaidGroupSet (2432)

**Richtung:** 📤 Client → Server  
**Frequenz:** Bei Gruppen-Zuweisung  
**Authentifizierung:** Ja  
**Spezielle Rechte:** Raid-Leader/Assist

#### Beschreibung
Leader/Assist weist Spieler einer Raid-Gruppe (1-8) zu.

#### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| RequestId | Guid | Eindeutige Request-ID | Ja |
| PlayerId | int | ID des Spielers | Ja |
| GroupNumber | byte | Ziel-Gruppe (1-8) | Ja |

#### Erwartete Response
- **Bei Erfolg:** Raid-Update Broadcast
- **Bei Fehler:** `ErrorMessage` (910)

---

### RaidTargetSet (2433)

**Richtung:** 📤 Client → Server  
**Frequenz:** Bei Marker-Setzen  
**Authentifizierung:** Ja  
**Spezielle Rechte:** Raid-Leader/Assist

#### Beschreibung
Leader/Assist setzt Raid-Target-Marker (Skull, Cross, etc.) auf Entity.

#### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| RequestId | Guid | Eindeutige Request-ID | Ja |
| TargetEntityId | int | Entity-ID für Marker | Ja |
| MarkerType | byte | Marker-Typ (0-7, 255=entfernen) | Ja |

#### Erwartete Response
- **Bei Erfolg:** Marker-Update Broadcast an Raid

---

### RaidReadyCheck (2434)

**Richtung:** 📤 Client → Server  
**Frequenz:** Vor Boss-Pull  
**Authentifizierung:** Ja  
**Spezielle Rechte:** Raid-Leader/Assist

#### Beschreibung
Leader/Assist startet Ready-Check. Alle Mitglieder müssen antworten.

#### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| RequestId | Guid | Eindeutige Request-ID | Ja |

#### Erwartete Response
- **Broadcast:** `RaidReadyCheck` an alle Mitglieder (Server → Client)
- **Sammlung:** Server sammelt `RaidReadyResponse` (2435)

---

### RaidReadyResponse (2435)

**Richtung:** 📤 Client → Server  
**Frequenz:** Antwort auf Ready-Check  
**Authentifizierung:** Ja

#### Beschreibung
Spieler beantwortet laufenden Ready-Check.

#### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| RequestId | Guid | Request-ID des Ready-Checks | Ja |
| Ready | bool | Bereit ja/nein | Ja |

#### Erwartete Response
- **Broadcast:** Ready-Status Update an alle Mitglieder

---

## Dungeon Finder (2440-2445)

---

### DungeonFinderJoin (2440)

**Richtung:** 📤 Client → Server  
**Frequenz:** Bei Queue-Beitritt  
**Authentifizierung:** Ja

#### Beschreibung
Client tritt der Dungeon-Finder-Queue bei. Muss Rollen ausgewählt haben.

#### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| RequestId | Guid | Eindeutige Request-ID | Ja |
| Roles | List\<FinderRole\> | Ausgewählte Rollen | Ja |
| DungeonIds | List\<int\> | Ausgewählte Dungeons (leer=alle) | Ja |
| Difficulty | InstanceDifficulty | Gewünschte Schwierigkeit | Ja |

#### Erwartete Response
- **Bei Erfolg:** `DungeonFinderUpdate` (2442) mit Queue-Status
- **Bei Fehler:** `ErrorMessage` (910) mit Grund

---

### DungeonFinderLeave (2441)

**Richtung:** 📤 Client → Server  
**Frequenz:** Bei Queue-Verlassen  
**Authentifizierung:** Ja

#### Beschreibung
Client verlässt die Dungeon-Finder-Queue.

#### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| RequestId | Guid | Eindeutige Request-ID | Ja |

#### Erwartete Response
- **Implizit:** Client erhält keine weiteren Finder-Updates

---

### DungeonFinderUpdate (2442)

**Richtung:** 📥 Server → Client  
**Frequenz:** Periodisch während Queue (alle 5s)  
**Authentifizierung:** Nein

#### Beschreibung
Server sendet aktuellen Queue-Status (Wartezeit, Position, etc.).

#### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Status | FinderStatusDto | Queue-Status Informationen | Ja |

---

### DungeonFinderProposal (2443)

**Richtung:** 📥 Server → Client  
**Frequenz:** Bei Match-Fund  
**Authentifizierung:** Nein

#### Beschreibung
Server hat Match gefunden. Client muss akzeptieren/ablehnen.

#### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| ProposalId | Guid | ID des Proposals | Ja |
| DungeonId | int | Gefundener Dungeon | Ja |
| DungeonName | string | Name des Dungeons | Ja |
| TimeoutSeconds | int | Zeit zum Antworten | Ja |
| PlayersAccepted | int | Bereits akzeptiert | Ja |
| PlayersTotal | int | Gesamt-Spieler | Ja |

---

### DungeonFinderAccept (2444)

**Richtung:** 📤 Client → Server  
**Frequenz:** Antwort auf Proposal  
**Authentifizierung:** Ja

#### Beschreibung
Client akzeptiert gefundenen Dungeon.

#### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| ProposalId | Guid | ID des Proposals | Ja |

#### Erwartete Response
- **Wenn alle akzeptieren:** `InstanceJoinResult` (2403) mit Teleport
- **Wenn einer ablehnt:** Zurück in Queue

---

### DungeonFinderDecline (2445)

**Richtung:** 📤 Client → Server  
**Frequenz:** Antwort auf Proposal  
**Authentifizierung:** Ja

#### Beschreibung
Client lehnt gefundenen Dungeon ab. Erhält Cooldown.

#### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| ProposalId | Guid | ID des Proposals | Ja |

#### Erwartete Response
- **Implizit:** Finder-Cooldown wird angewendet

---

## Raid Finder (2450-2452)

---

### RaidFinderJoin (2450)

**Richtung:** 📤 Client → Server  
**Frequenz:** Bei Queue-Beitritt  
**Authentifizierung:** Ja

#### Beschreibung
Client tritt der Raid-Finder-Queue bei. Funktioniert wie Dungeon-Finder für Raids.

#### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| RequestId | Guid | Eindeutige Request-ID | Ja |
| Roles | List\<FinderRole\> | Ausgewählte Rollen | Ja |
| RaidId | int | Ausgewählter Raid | Ja |
| BossWing | int | Spezifischer Boss-Flügel (0=alle) | Nein |

#### Erwartete Response
- **Bei Erfolg:** `RaidFinderUpdate` (2452) mit Queue-Status
- **Bei Fehler:** `ErrorMessage` (910)

---

### RaidFinderLeave (2451)

**Richtung:** 📤 Client → Server  
**Frequenz:** Bei Queue-Verlassen  
**Authentifizierung:** Ja

#### Beschreibung
Client verlässt die Raid-Finder-Queue.

#### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| RequestId | Guid | Eindeutige Request-ID | Ja |

#### Erwartete Response
- **Implizit:** Client erhält keine weiteren Finder-Updates

---

### RaidFinderUpdate (2452)

**Richtung:** 📥 Server → Client  
**Frequenz:** Periodisch während Queue (alle 10s)  
**Authentifizierung:** Nein

#### Beschreibung
Server sendet aktuellen Raid-Finder Queue-Status.

#### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Status | FinderStatusDto | Queue-Status Informationen | Ja |
| RaidName | string | Name des Raids | Ja |
| WingName | string | Name des Boss-Flügels | Nein |

---

## Anhang

### MessageType Enum (Reihenfolge wie im Code)

```csharp
// Instance Management (2400-2412)
InstanceCreate = 2400,
InstanceCreateResult = 2401,
InstanceJoin = 2402,
InstanceJoinResult = 2403,
InstanceLeave = 2404,
InstanceReset = 2405,
InstanceResetResult = 2406,
InstanceLockout = 2407,
InstanceLockoutList = 2408,
InstanceDifficultySet = 2409,
InstanceDifficultyVote = 2410,
InstanceSaved = 2411,
InstanceExtend = 2412,

// Encounter Events (2420-2425)
InstanceEncounterStart = 2420,
InstanceEncounterEnd = 2421,
InstanceEncounterUpdate = 2422,
InstanceBossKill = 2423,
InstanceWipe = 2424,
InstanceCheckpoint = 2425,

// Raid Management (2430-2435)
RaidConvert = 2430,
RaidDisband = 2431,
RaidGroupSet = 2432,
RaidTargetSet = 2433,
RaidReadyCheck = 2434,
RaidReadyResponse = 2435,

// Dungeon Finder (2440-2445)
DungeonFinderJoin = 2440,
DungeonFinderLeave = 2441,
DungeonFinderUpdate = 2442,
DungeonFinderProposal = 2443,
DungeonFinderAccept = 2444,
DungeonFinderDecline = 2445,

// Raid Finder (2450-2452)
RaidFinderJoin = 2450,
RaidFinderLeave = 2451,
RaidFinderUpdate = 2452,
```

### Request/Response Paare

| Request | Response | Beschreibung |
|---------|----------|--------------|
| InstanceCreate (2400) | InstanceCreateResult (2401) | Instanz erstellen |
| InstanceJoin (2402) | InstanceJoinResult (2403) | Instanz betreten |
| InstanceLeave (2404) | ZoneTransferResponse (107) | Instanz verlassen |
| InstanceReset (2405) | InstanceResetResult (2406) | Instanz zurücksetzen |
| InstanceDifficultySet (2409) | Party/Raid Broadcast | Schwierigkeit ändern |
| InstanceDifficultyVote (2410) | Vote Result Broadcast | Für Schwierigkeit stimmen |
| InstanceExtend (2412) | InstanceLockout (2407) | Lockout verlängern |
| RaidConvert (2430) | Party Update Broadcast | Gruppe konvertieren |
| RaidDisband (2431) | PartyDisbanded Broadcast | Raid auflösen |
| RaidGroupSet (2432) | Raid Update Broadcast | Gruppenzuweisung |
| RaidTargetSet (2433) | Marker Broadcast | Marker setzen |
| RaidReadyCheck (2434) | RaidReadyCheck Broadcast | Ready-Check starten |
| RaidReadyResponse (2435) | Ready Status Broadcast | Ready-Check antworten |
| DungeonFinderJoin (2440) | DungeonFinderUpdate (2442) | Dungeon-Queue beitreten |
| DungeonFinderLeave (2441) | (implizit) | Dungeon-Queue verlassen |
| DungeonFinderAccept (2444) | InstanceJoinResult (2403) | Dungeon akzeptieren |
| DungeonFinderDecline (2445) | (implizit + Cooldown) | Dungeon ablehnen |
| RaidFinderJoin (2450) | RaidFinderUpdate (2452) | Raid-Queue beitreten |
| RaidFinderLeave (2451) | (implizit) | Raid-Queue verlassen |

### Server-initiierte Messages (Fire-and-Forget)

| Message | ID | Auslöser |
|---------|-----|----------|
| InstanceLockout | 2407 | Nach Boss-Kill |
| InstanceLockoutList | 2408 | Bei Login |
| InstanceSaved | 2411 | Nach Fortschritt-Speicherung |
| InstanceEncounterStart | 2420 | Bei Encounter-Beginn |
| InstanceEncounterEnd | 2421 | Bei Encounter-Ende |
| InstanceEncounterUpdate | 2422 | Periodisch während Encounter |
| InstanceBossKill | 2423 | Bei Boss-Tod |
| InstanceWipe | 2424 | Bei Gruppen-Wipe |
| InstanceCheckpoint | 2425 | Bei Checkpoint-Erreichen |
| DungeonFinderUpdate | 2442 | Periodisch in Queue |
| DungeonFinderProposal | 2443 | Bei Match-Fund |
| RaidFinderUpdate | 2452 | Periodisch in Queue |

### Datei-Struktur

```
shared/Mmo.Shared/Messaging/
├── Enums/
│   └── MessageType.cs              // 2400-2452 Instance
├── Messages/Instance/
│   ├── InstanceCreateMessage.cs
│   ├── InstanceCreateResultMessage.cs
│   ├── InstanceJoinMessage.cs
│   ├── InstanceJoinResultMessage.cs
│   ├── InstanceLeaveMessage.cs
│   ├── InstanceResetMessage.cs
│   ├── InstanceResetResultMessage.cs
│   ├── InstanceLockoutMessage.cs
│   ├── InstanceLockoutListMessage.cs
│   ├── InstanceDifficultySetMessage.cs
│   ├── InstanceDifficultyVoteMessage.cs
│   ├── InstanceSavedMessage.cs
│   ├── InstanceExtendMessage.cs
│   ├── InstanceEncounterStartMessage.cs
│   ├── InstanceEncounterEndMessage.cs
│   ├── InstanceEncounterUpdateMessage.cs
│   ├── InstanceBossKillMessage.cs
│   ├── InstanceWipeMessage.cs
│   ├── InstanceCheckpointMessage.cs
│   ├── RaidConvertMessage.cs
│   ├── RaidDisbandMessage.cs
│   ├── RaidGroupSetMessage.cs
│   ├── RaidTargetSetMessage.cs
│   ├── RaidReadyCheckMessage.cs
│   ├── RaidReadyResponseMessage.cs
│   ├── DungeonFinderJoinMessage.cs
│   ├── DungeonFinderLeaveMessage.cs
│   ├── DungeonFinderUpdateMessage.cs
│   ├── DungeonFinderProposalMessage.cs
│   ├── DungeonFinderAcceptMessage.cs
│   ├── DungeonFinderDeclineMessage.cs
│   ├── RaidFinderJoinMessage.cs
│   ├── RaidFinderLeaveMessage.cs
│   └── RaidFinderUpdateMessage.cs
└── DTOs/Instance/
    ├── InstanceInfoDto.cs
    ├── LockoutDto.cs
    ├── EncounterDto.cs
    └── FinderStatusDto.cs
```

---

**Letzte Aktualisierung**: 2026-01-02  
**Version**: 3.0.0  
**Status**: ✅ Aligned mit MessageType Enum (35 Messages)

[← Zurück zur Übersicht](Message-Reference.md)

Source: docs/03-messages/24-instance.md
