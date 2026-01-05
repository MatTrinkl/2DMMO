# 👑 Admin Messages (2300-2399)

**Kategorie:** 23  
**Range:** 2300-2336  
**Phase:** Prototyp  
**Status:** 🟢 In Entwicklung

[← Zurück zur Übersicht](README.md)

---

## 📋 Inhaltsverzeichnis

1. [Übersicht](#übersicht)
2. [Admin-System Flow](#admin-system-flow)
   - [Server-Authoritative Architektur](#server-authoritative-architektur)
   - [Admin-Command Flow](#admin-command-flow)
   - [Permission-Check Flow](#permission-check-flow)
3. [DTOs & Enums](#dtos--enums)
   - [AdminRole Enum](#adminrole-enum)
   - [AdminErrorCode Enum](#adminerrorcode-enum)
   - [AdminActionDto](#adminactiondto)
   - [PlayerInfoDto](#playerinfodto)
   - [ServerInfoDto](#serverinfodto)
   - [Wichtige Konstanten](#wichtige-konstanten)
4. [Messages (2300-2336)](#messages-2300-2336)
   - [AdminCommand (2300)](#admincommand-2300)
   - [AdminCommandResult (2301)](#admincommandresult-2301)
   - [AdminTeleport (2302)](#adminteleport-2302)
   - [AdminTeleportPlayer (2303)](#adminteleportplayer-2303)
   - [AdminKick (2304)](#adminkick-2304)
   - [AdminBan (2305)](#adminban-2305)
   - [AdminUnban (2306)](#adminunban-2306)
   - [AdminMute (2307)](#adminmute-2307)
   - [AdminUnmute (2308)](#adminunmute-2308)
   - [AdminSpawn (2309)](#adminspawn-2309)
   - [AdminDespawn (2310)](#admindespawn-2310)
   - [AdminKill (2311)](#adminkill-2311)
   - [AdminRevive (2312)](#adminrevive-2312)
   - [AdminHeal (2313)](#adminheal-2313)
   - [AdminGodMode (2314)](#admingodmode-2314)
   - [AdminInvisible (2315)](#admininvisible-2315)
   - [AdminFreeze (2316)](#adminfreeze-2316)
   - [AdminUnfreeze (2317)](#adminunfreeze-2317)
   - [AdminGiveItem (2318)](#admingiveitem-2318)
   - [AdminRemoveItem (2319)](#adminremoveitem-2319)
   - [AdminSetLevel (2320)](#adminsetlevel-2320)
   - [AdminSetStat (2321)](#adminsetstat-2321)
   - [AdminSetReputation (2322)](#adminsetreputation-2322)
   - [AdminAddGold (2323)](#adminaddgold-2323)
   - [AdminRemoveGold (2324)](#adminremovegold-2324)
   - [AdminAnnounce (2325)](#adminannounce-2325)
   - [AdminWhisper (2326)](#adminwhisper-2326)
   - [AdminSummonPlayer (2327)](#adminsummonplayer-2327)
   - [AdminAppearPlayer (2328)](#adminappearplayer-2328)
   - [AdminPlayerInfo (2329)](#adminplayerinfo-2329)
   - [AdminServerInfo (2330)](#adminserverinfo-2330)
   - [AdminReloadConfig (2331)](#adminreloadconfig-2331)
   - [AdminReloadScripts (2332)](#adminreloadscripts-2332)
   - [AdminShutdown (2333)](#adminshutdown-2333)
   - [AdminRestart (2334)](#adminrestart-2334)
   - [AdminMaintenance (2335)](#adminmaintenance-2335)
   - [AdminLog (2336)](#adminlog-2336)
5. [Anhang](#anhang)
   - [MessageType Enum (Reihenfolge wie Code)](#messagetype-enum-reihenfolge-wie-code)
   - [Request/Response Paare](#requestresponse-paare)
   - [Fire-and-Forget Messages](#fire-and-forget-messages)
   - [Server-Initiated Messages](#server-initiated-messages)
   - [Datei-Struktur](#datei-struktur)

---

## Übersicht

Diese Kategorie umfasst alle Messages für **Admin- und GM-Tools** im 2DMMO. Admins und Game Masters (GMs) können Spieler verwalten, Server steuern und Debug-Aktionen durchführen.

**Wichtige Eigenschaften:**
- **Server-Authoritative:** Alle Admin-Aktionen werden serverseitig validiert
- **Permission-basiert:** Jede Aktion erfordert entsprechende Rechte (GM/Admin/Super-Admin)
- **Logging:** Alle Admin-Aktionen werden protokolliert
- **Rate-Limited:** Schutz vor Missbrauch durch Rate-Limiting

---

## Admin-System Flow

### Server-Authoritative Architektur

```
┌─────────────────────────────────────────────────────────────────┐
│                    ADMIN SYSTEM ARCHITECTURE                     │
├─────────────────────────────────────────────────────────────────┤
│                                                                   │
│  Admin-Client                 Zone-Server                DB      │
│      │                            │                       │      │
│      │  1. AdminCommand           │                       │      │
│      │  ─────────────────────────►│                       │      │
│      │                            │                       │      │
│      │                            │  2. Check Permission  │      │
│      │                            │  ─────────────────────►│      │
│      │                            │                       │      │
│      │                            │  3. Permission Result │      │
│      │                            │  ◄─────────────────────│      │
│      │                            │                       │      │
│      │                            │  4. Execute Action    │      │
│      │                            │  (if permitted)       │      │
│      │                            │                       │      │
│      │                            │  5. Log Action        │      │
│      │                            │  ─────────────────────►│      │
│      │                            │                       │      │
│      │  6. AdminCommandResult     │                       │      │
│      │  ◄─────────────────────────│                       │      │
│      │                            │                       │      │
│      │  7. AdminLog (broadcast)   │                       │      │
│      │  ◄─────────────────────────│                       │      │
│                                                                   │
└─────────────────────────────────────────────────────────────────┘
```

### Admin-Command Flow

```
Admin                     Server                     Target
  │                          │                          │
  │  AdminCommand (2300)     │                          │
  │─────────────────────────►│                          │
  │                          │                          │
  │                          │  Validate Permission     │
  │                          │  ────────────────────    │
  │                          │                          │
  │                          │  Execute on Target       │
  │                          │─────────────────────────►│
  │                          │                          │
  │  AdminCommandResult      │                          │
  │◄─────────────────────────│                          │
  │                          │                          │
  │  AdminLog (2336)         │                          │
  │◄─────────────────────────│                          │
```

### Permission-Check Flow

```
┌─────────────────────────────────────────────────────────────────┐
│                    PERMISSION HIERARCHY                          │
├─────────────────────────────────────────────────────────────────┤
│                                                                   │
│   ┌───────────────┐                                              │
│   │ Super-Admin   │  ← Alle Rechte + Server-Control              │
│   │ (Level 100)   │                                              │
│   └───────┬───────┘                                              │
│           │                                                       │
│   ┌───────▼───────┐                                              │
│   │    Admin      │  ← GM-Rechte + Ban/Spawn/GodMode             │
│   │ (Level 50+)   │                                              │
│   └───────┬───────┘                                              │
│           │                                                       │
│   ┌───────▼───────┐                                              │
│   │ Game Master   │  ← Kick/Mute/Teleport/PlayerInfo             │
│   │ (Level 10+)   │                                              │
│   └───────┬───────┘                                              │
│           │                                                       │
│   ┌───────▼───────┐                                              │
│   │   Moderator   │  ← Mute/Warn/Report-View                     │
│   │  (Level 1+)   │                                              │
│   └───────────────┘                                              │
│                                                                   │
└─────────────────────────────────────────────────────────────────┘
```

---

## DTOs & Enums

### AdminRole Enum

```csharp
public enum AdminRole : byte
{
    None = 0,           // Kein Admin
    Moderator = 1,      // Basis-Moderation (Mute, Warn)
    GameMaster = 10,    // GM-Rechte (Kick, Teleport)
    Admin = 50,         // Admin-Rechte (Ban, Spawn, GodMode)
    SuperAdmin = 100    // Alle Rechte inkl. Server-Control
}
```

### AdminErrorCode Enum

```csharp
public enum AdminErrorCode : byte
{
    None = 0,
    InsufficientPermission = 1,     // Keine ausreichenden Rechte
    TargetNotFound = 2,             // Ziel-Spieler nicht gefunden
    TargetOffline = 3,              // Ziel-Spieler offline
    TargetIsAdmin = 4,              // Kann keinen Admin targetieren
    InvalidCommand = 5,             // Unbekannter Command
    InvalidParameter = 6,           // Ungültiger Parameter
    RateLimited = 7,                // Zu viele Aktionen
    ActionFailed = 8,               // Aktion fehlgeschlagen
    AlreadyBanned = 9,              // Spieler bereits gebannt
    NotBanned = 10,                 // Spieler nicht gebannt
    AlreadyMuted = 11,              // Spieler bereits gemutet
    NotMuted = 12,                  // Spieler nicht gemutet
    ServerBusy = 13,                // Server zu beschäftigt
    MaintenanceActive = 14,         // Wartungsmodus aktiv
    CooldownActive = 15             // Aktion auf Cooldown
}
```

### AdminActionDto

```csharp
[MessagePackObject]
public class AdminActionDto
{
    [Key(0)] public MessageType Type => MessageType.AdminCommand;
    [Key(1)] public uint RequestId { get; set; }
    [Key(2)] public string Command { get; set; }
    [Key(3)] public string[] Parameters { get; set; }
    [Key(4)] public long TargetPlayerId { get; set; }
    [Key(5)] public string Reason { get; set; }
}
```

### PlayerInfoDto

```csharp
[MessagePackObject]
public class PlayerInfoDto
{
    [Key(0)] public long PlayerId { get; set; }
    [Key(1)] public string PlayerName { get; set; }
    [Key(2)] public long AccountId { get; set; }
    [Key(3)] public string AccountName { get; set; }
    [Key(4)] public int Level { get; set; }
    [Key(5)] public string Class { get; set; }
    [Key(6)] public int ZoneId { get; set; }
    [Key(7)] public float X { get; set; }
    [Key(8)] public float Y { get; set; }
    [Key(9)] public long Gold { get; set; }
    [Key(10)] public bool IsMuted { get; set; }
    [Key(11)] public long MuteExpiry { get; set; }
    [Key(12)] public bool IsBanned { get; set; }
    [Key(13)] public string BanReason { get; set; }
    [Key(14)] public string IpAddress { get; set; }
    [Key(15)] public long LastLogin { get; set; }
    [Key(16)] public long PlayTime { get; set; }
    [Key(17)] public AdminRole AdminLevel { get; set; }
}
```

### ServerInfoDto

```csharp
[MessagePackObject]
public class ServerInfoDto
{
    [Key(0)] public string ServerName { get; set; }
    [Key(1)] public string Version { get; set; }
    [Key(2)] public int OnlinePlayers { get; set; }
    [Key(3)] public int MaxPlayers { get; set; }
    [Key(4)] public long Uptime { get; set; }
    [Key(5)] public float CpuUsage { get; set; }
    [Key(6)] public long MemoryUsed { get; set; }
    [Key(7)] public long MemoryTotal { get; set; }
    [Key(8)] public int ActiveZones { get; set; }
    [Key(9)] public int TotalEntities { get; set; }
    [Key(10)] public float TickRate { get; set; }
    [Key(11)] public float AverageLatency { get; set; }
    [Key(12)] public bool MaintenanceMode { get; set; }
    [Key(13)] public long MaintenanceStart { get; set; }
}
```

### Wichtige Konstanten

| Konstante | Wert | Beschreibung |
|-----------|------|--------------|
| `MAX_BAN_DURATION` | 365 Tage | Maximale Ban-Dauer (0 = permanent) |
| `MAX_MUTE_DURATION` | 30 Tage | Maximale Mute-Dauer |
| `ADMIN_RATE_LIMIT` | 10/min | Max Admin-Commands pro Minute |
| `ANNOUNCE_COOLDOWN` | 30s | Cooldown für Server-Announcements |
| `SHUTDOWN_DELAY` | 60s | Mindestverzögerung vor Shutdown |
| `LOG_RETENTION` | 90 Tage | Admin-Log Aufbewahrung |

---

## Messages (2300-2336)

### AdminCommand (2300)

**Richtung:** 📤 Client → Server  
**Frequenz:** Bei Bedarf  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Moderator+

#### Beschreibung
Generischer Admin-Command für Text-basierte Befehle. Ermöglicht die Ausführung von Admin-Befehlen über Chat-Eingabe (z.B. `/kick PlayerName`).

#### Request Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| RequestId | uint | Eindeutige Request-ID für Korrelation | Ja |
| Command | string | Command-Name (ohne Slash) | Ja |
| Parameters | string[] | Command-Parameter | Nein |

#### Erwartete Response
- **Bei Erfolg:** `AdminCommandResult` (2301) mit Success=true
- **Bei Fehler:** `AdminCommandResult` (2301) mit ErrorCode

#### Beispiel Payload
```csharp
var adminCommand = new AdminCommand
{
    Type = MessageType.AdminCommand,
    RequestId = 12345,
    Command = "kick",
    Parameters = new[] { "PlayerName", "Reason for kick" }
};
```

---

### AdminCommandResult (2301)

**Richtung:** 📥 Server → Client  
**Frequenz:** Als Response  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Moderator+

#### Beschreibung
Antwort auf einen Admin-Command. Enthält Erfolg/Fehler-Status und optional Ergebnisdaten.

#### Response Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| RequestId | uint | Korrelation zum Request | Ja |
| Success | bool | Command erfolgreich ausgeführt? | Ja |
| ErrorCode | AdminErrorCode | Fehlercode bei Misserfolg | Bei Fehler |
| Message | string | Ergebnis-/Fehlermeldung | Ja |
| Data | object | Optionale Ergebnisdaten | Nein |

#### Beispiel Payload
```csharp
var result = new AdminCommandResult
{
    Type = MessageType.AdminCommandResult,
    RequestId = 12345,
    Success = true,
    Message = "Player 'PlayerName' has been kicked"
};
```

---

### AdminTeleport (2302)

**Richtung:** 📤 Client → Server  
**Frequenz:** Bei Bedarf  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 GameMaster+

#### Beschreibung
Admin teleportiert sich selbst zu einer bestimmten Position oder Zone.

#### Request Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| RequestId | uint | Eindeutige Request-ID | Ja |
| ZoneId | int | Ziel-Zone (0 = aktuelle Zone) | Ja |
| X | float | Ziel X-Koordinate | Ja |
| Y | float | Ziel Y-Koordinate | Ja |

#### Erwartete Response
- **Bei Erfolg:** `AdminCommandResult` (2301) mit Success=true
- **Bei Fehler:** `AdminCommandResult` (2301) mit ErrorCode

---

### AdminTeleportPlayer (2303)

**Richtung:** 📤 Client → Server  
**Frequenz:** Bei Bedarf  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 GameMaster+

#### Beschreibung
Admin teleportiert einen anderen Spieler zu einer bestimmten Position.

#### Request Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| RequestId | uint | Eindeutige Request-ID | Ja |
| TargetPlayerId | long | Ziel-Spieler ID | Ja |
| ZoneId | int | Ziel-Zone (0 = aktuelle Zone) | Ja |
| X | float | Ziel X-Koordinate | Ja |
| Y | float | Ziel Y-Koordinate | Ja |
| Reason | string | Grund für Teleport | Nein |

#### Erwartete Response
- **Bei Erfolg:** `AdminCommandResult` (2301) mit Success=true
- **Bei Fehler:** `AdminCommandResult` (2301) mit ErrorCode

---

### AdminKick (2304)

**Richtung:** 📤 Client → Server  
**Frequenz:** Bei Bedarf  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 GameMaster+

#### Beschreibung
Kickt einen Spieler vom Server. Der Spieler kann sich sofort wieder verbinden.

#### Request Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| RequestId | uint | Eindeutige Request-ID | Ja |
| TargetPlayerId | long | Ziel-Spieler ID | Ja |
| Reason | string | Kick-Grund (wird angezeigt) | Ja |

#### Erwartete Response
- **Bei Erfolg:** `AdminCommandResult` (2301) mit Success=true
- **Bei Fehler:** `AdminCommandResult` (2301) mit ErrorCode

---

### AdminBan (2305)

**Richtung:** 📤 Client → Server  
**Frequenz:** Bei Bedarf  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Admin+

#### Beschreibung
Bannt einen Spieler für eine bestimmte Dauer oder permanent.

#### Request Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| RequestId | uint | Eindeutige Request-ID | Ja |
| TargetPlayerId | long | Ziel-Spieler ID | Ja |
| Duration | int | Ban-Dauer in Minuten (0 = permanent) | Ja |
| Reason | string | Ban-Grund | Ja |
| BanIp | bool | IP auch bannen? | Nein |

#### Erwartete Response
- **Bei Erfolg:** `AdminCommandResult` (2301) mit Success=true
- **Bei Fehler:** `AdminCommandResult` (2301) mit ErrorCode

---

### AdminUnban (2306)

**Richtung:** 📤 Client → Server  
**Frequenz:** Bei Bedarf  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Admin+

#### Beschreibung
Entbannt einen gebannten Spieler.

#### Request Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| RequestId | uint | Eindeutige Request-ID | Ja |
| TargetAccountId | long | Ziel-Account ID | Ja |
| UnbanIp | bool | IP auch entbannen? | Nein |

#### Erwartete Response
- **Bei Erfolg:** `AdminCommandResult` (2301) mit Success=true
- **Bei Fehler:** `AdminCommandResult` (2301) mit ErrorCode

---

### AdminMute (2307)

**Richtung:** 📤 Client → Server  
**Frequenz:** Bei Bedarf  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Moderator+

#### Beschreibung
Mutet einen Spieler (kann keine Chat-Nachrichten senden).

#### Request Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| RequestId | uint | Eindeutige Request-ID | Ja |
| TargetPlayerId | long | Ziel-Spieler ID | Ja |
| Duration | int | Mute-Dauer in Minuten | Ja |
| Reason | string | Mute-Grund | Ja |

#### Erwartete Response
- **Bei Erfolg:** `AdminCommandResult` (2301) mit Success=true
- **Bei Fehler:** `AdminCommandResult` (2301) mit ErrorCode

---

### AdminUnmute (2308)

**Richtung:** 📤 Client → Server  
**Frequenz:** Bei Bedarf  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Moderator+

#### Beschreibung
Entmutet einen gemuteten Spieler.

#### Request Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| RequestId | uint | Eindeutige Request-ID | Ja |
| TargetPlayerId | long | Ziel-Spieler ID | Ja |

#### Erwartete Response
- **Bei Erfolg:** `AdminCommandResult` (2301) mit Success=true
- **Bei Fehler:** `AdminCommandResult` (2301) mit ErrorCode

---

### AdminSpawn (2309)

**Richtung:** 📤 Client → Server  
**Frequenz:** Bei Bedarf  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Admin+

#### Beschreibung
Spawnt ein NPC oder Objekt an einer bestimmten Position.

#### Request Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| RequestId | uint | Eindeutige Request-ID | Ja |
| EntityType | byte | Entity-Typ (NPC/Object/Item) | Ja |
| EntityId | int | Template-ID der Entity | Ja |
| X | float | Spawn X-Koordinate | Ja |
| Y | float | Spawn Y-Koordinate | Ja |
| Count | int | Anzahl (Default: 1) | Nein |

#### Erwartete Response
- **Bei Erfolg:** `AdminCommandResult` (2301) mit Success=true und SpawnedEntityIds
- **Bei Fehler:** `AdminCommandResult` (2301) mit ErrorCode

---

### AdminDespawn (2310)

**Richtung:** 📤 Client → Server  
**Frequenz:** Bei Bedarf  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Admin+

#### Beschreibung
Entfernt ein gespawntes NPC oder Objekt.

#### Request Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| RequestId | uint | Eindeutige Request-ID | Ja |
| TargetEntityId | long | Zu entfernende Entity ID | Ja |

#### Erwartete Response
- **Bei Erfolg:** `AdminCommandResult` (2301) mit Success=true
- **Bei Fehler:** `AdminCommandResult` (2301) mit ErrorCode

---

### AdminKill (2311)

**Richtung:** 📤 Client → Server  
**Frequenz:** Bei Bedarf  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Admin+

#### Beschreibung
Tötet eine Entity sofort (Spieler oder NPC).

#### Request Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| RequestId | uint | Eindeutige Request-ID | Ja |
| TargetEntityId | long | Ziel-Entity ID | Ja |

#### Erwartete Response
- **Bei Erfolg:** `AdminCommandResult` (2301) mit Success=true
- **Bei Fehler:** `AdminCommandResult` (2301) mit ErrorCode

---

### AdminRevive (2312)

**Richtung:** 📤 Client → Server  
**Frequenz:** Bei Bedarf  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Admin+

#### Beschreibung
Wiederbelebt einen toten Spieler an seiner aktuellen Position.

#### Request Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| RequestId | uint | Eindeutige Request-ID | Ja |
| TargetPlayerId | long | Ziel-Spieler ID | Ja |
| FullHealth | bool | Mit voller Gesundheit? (Default: true) | Nein |

#### Erwartete Response
- **Bei Erfolg:** `AdminCommandResult` (2301) mit Success=true
- **Bei Fehler:** `AdminCommandResult` (2301) mit ErrorCode

---

### AdminHeal (2313)

**Richtung:** 📤 Client → Server  
**Frequenz:** Bei Bedarf  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Admin+

#### Beschreibung
Heilt eine Entity auf volle Gesundheit/Mana.

#### Request Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| RequestId | uint | Eindeutige Request-ID | Ja |
| TargetEntityId | long | Ziel-Entity ID | Ja |
| HealHealth | bool | Health heilen? (Default: true) | Nein |
| HealMana | bool | Mana heilen? (Default: true) | Nein |

#### Erwartete Response
- **Bei Erfolg:** `AdminCommandResult` (2301) mit Success=true
- **Bei Fehler:** `AdminCommandResult` (2301) mit ErrorCode

---

### AdminGodMode (2314)

**Richtung:** 📤 Client → Server  
**Frequenz:** Bei Bedarf  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Admin+

#### Beschreibung
Aktiviert/Deaktiviert GodMode für den Admin (unverwundbar).

#### Request Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| RequestId | uint | Eindeutige Request-ID | Ja |
| Enabled | bool | GodMode aktivieren? | Ja |

#### Erwartete Response
- **Bei Erfolg:** `AdminCommandResult` (2301) mit Success=true
- **Bei Fehler:** `AdminCommandResult` (2301) mit ErrorCode

---

### AdminInvisible (2315)

**Richtung:** 📤 Client → Server  
**Frequenz:** Bei Bedarf  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Admin+

#### Beschreibung
Macht den Admin unsichtbar für normale Spieler.

#### Request Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| RequestId | uint | Eindeutige Request-ID | Ja |
| Enabled | bool | Unsichtbarkeit aktivieren? | Ja |

#### Erwartete Response
- **Bei Erfolg:** `AdminCommandResult` (2301) mit Success=true
- **Bei Fehler:** `AdminCommandResult` (2301) mit ErrorCode

---

### AdminFreeze (2316)

**Richtung:** 📤 Client → Server  
**Frequenz:** Bei Bedarf  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Admin+

#### Beschreibung
Friert einen Spieler ein (kann sich nicht bewegen oder handeln).

#### Request Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| RequestId | uint | Eindeutige Request-ID | Ja |
| TargetPlayerId | long | Ziel-Spieler ID | Ja |
| Reason | string | Grund für Freeze | Nein |

#### Erwartete Response
- **Bei Erfolg:** `AdminCommandResult` (2301) mit Success=true
- **Bei Fehler:** `AdminCommandResult` (2301) mit ErrorCode

---

### AdminUnfreeze (2317)

**Richtung:** 📤 Client → Server  
**Frequenz:** Bei Bedarf  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Admin+

#### Beschreibung
Taut einen eingefrorenen Spieler auf.

#### Request Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| RequestId | uint | Eindeutige Request-ID | Ja |
| TargetPlayerId | long | Ziel-Spieler ID | Ja |

#### Erwartete Response
- **Bei Erfolg:** `AdminCommandResult` (2301) mit Success=true
- **Bei Fehler:** `AdminCommandResult` (2301) mit ErrorCode

---

### AdminGiveItem (2318)

**Richtung:** 📤 Client → Server  
**Frequenz:** Bei Bedarf  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Admin+

#### Beschreibung
Gibt einem Spieler ein Item.

#### Request Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| RequestId | uint | Eindeutige Request-ID | Ja |
| TargetPlayerId | long | Ziel-Spieler ID | Ja |
| ItemId | int | Item Template-ID | Ja |
| Count | int | Anzahl (Default: 1) | Nein |

#### Erwartete Response
- **Bei Erfolg:** `AdminCommandResult` (2301) mit Success=true
- **Bei Fehler:** `AdminCommandResult` (2301) mit ErrorCode

---

### AdminRemoveItem (2319)

**Richtung:** 📤 Client → Server  
**Frequenz:** Bei Bedarf  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Admin+

#### Beschreibung
Entfernt ein Item von einem Spieler.

#### Request Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| RequestId | uint | Eindeutige Request-ID | Ja |
| TargetPlayerId | long | Ziel-Spieler ID | Ja |
| ItemId | int | Item Template-ID | Ja |
| Count | int | Anzahl (Default: alle) | Nein |

#### Erwartete Response
- **Bei Erfolg:** `AdminCommandResult` (2301) mit Success=true
- **Bei Fehler:** `AdminCommandResult` (2301) mit ErrorCode

---

### AdminSetLevel (2320)

**Richtung:** 📤 Client → Server  
**Frequenz:** Bei Bedarf  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Admin+

#### Beschreibung
Setzt das Level eines Spielers.

#### Request Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| RequestId | uint | Eindeutige Request-ID | Ja |
| TargetPlayerId | long | Ziel-Spieler ID | Ja |
| Level | int | Neues Level (1-100) | Ja |

#### Erwartete Response
- **Bei Erfolg:** `AdminCommandResult` (2301) mit Success=true
- **Bei Fehler:** `AdminCommandResult` (2301) mit ErrorCode

---

### AdminSetStat (2321)

**Richtung:** 📤 Client → Server  
**Frequenz:** Bei Bedarf  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Admin+

#### Beschreibung
Setzt einen Stat-Wert eines Spielers.

#### Request Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| RequestId | uint | Eindeutige Request-ID | Ja |
| TargetPlayerId | long | Ziel-Spieler ID | Ja |
| StatType | string | Stat-Name (z.B. "Strength") | Ja |
| Value | int | Neuer Wert | Ja |

#### Erwartete Response
- **Bei Erfolg:** `AdminCommandResult` (2301) mit Success=true
- **Bei Fehler:** `AdminCommandResult` (2301) mit ErrorCode

---

### AdminSetReputation (2322)

**Richtung:** 📤 Client → Server  
**Frequenz:** Bei Bedarf  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Admin+

#### Beschreibung
Setzt die Reputation eines Spielers bei einer Fraktion.

#### Request Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| RequestId | uint | Eindeutige Request-ID | Ja |
| TargetPlayerId | long | Ziel-Spieler ID | Ja |
| FactionId | int | Fraktions-ID | Ja |
| Value | int | Neuer Reputations-Wert | Ja |

#### Erwartete Response
- **Bei Erfolg:** `AdminCommandResult` (2301) mit Success=true
- **Bei Fehler:** `AdminCommandResult` (2301) mit ErrorCode

---

### AdminAddGold (2323)

**Richtung:** 📤 Client → Server  
**Frequenz:** Bei Bedarf  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Admin+

#### Beschreibung
Gibt einem Spieler Gold.

#### Request Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| RequestId | uint | Eindeutige Request-ID | Ja |
| TargetPlayerId | long | Ziel-Spieler ID | Ja |
| Amount | long | Gold-Betrag | Ja |

#### Erwartete Response
- **Bei Erfolg:** `AdminCommandResult` (2301) mit Success=true
- **Bei Fehler:** `AdminCommandResult` (2301) mit ErrorCode

---

### AdminRemoveGold (2324)

**Richtung:** 📤 Client → Server  
**Frequenz:** Bei Bedarf  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Admin+

#### Beschreibung
Entfernt Gold von einem Spieler.

#### Request Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| RequestId | uint | Eindeutige Request-ID | Ja |
| TargetPlayerId | long | Ziel-Spieler ID | Ja |
| Amount | long | Gold-Betrag | Ja |

#### Erwartete Response
- **Bei Erfolg:** `AdminCommandResult` (2301) mit Success=true
- **Bei Fehler:** `AdminCommandResult` (2301) mit ErrorCode

---

### AdminAnnounce (2325)

**Richtung:** 📤 Client → Server  
**Frequenz:** Bei Bedarf (Rate-Limited)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Admin+

#### Beschreibung
Sendet eine server-weite Ankündigung an alle Spieler.

#### Request Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| RequestId | uint | Eindeutige Request-ID | Ja |
| Message | string | Ankündigungs-Text | Ja |
| Type | byte | Anzeige-Typ (0=Normal, 1=Warning, 2=Alert) | Nein |

#### Erwartete Response
- **Bei Erfolg:** `AdminCommandResult` (2301) mit Success=true
- **Bei Fehler:** `AdminCommandResult` (2301) mit ErrorCode

---

### AdminWhisper (2326)

**Richtung:** 📤 Client → Server  
**Frequenz:** Bei Bedarf  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Moderator+

#### Beschreibung
GM-Whisper an einen Spieler (speziell formatiert).

#### Request Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| RequestId | uint | Eindeutige Request-ID | Ja |
| TargetPlayerId | long | Ziel-Spieler ID | Ja |
| Message | string | Nachricht | Ja |

#### Erwartete Response
- **Bei Erfolg:** `AdminCommandResult` (2301) mit Success=true
- **Bei Fehler:** `AdminCommandResult` (2301) mit ErrorCode

---

### AdminSummonPlayer (2327)

**Richtung:** 📤 Client → Server  
**Frequenz:** Bei Bedarf  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Admin+

#### Beschreibung
Ruft einen Spieler zur Position des Admins.

#### Request Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| RequestId | uint | Eindeutige Request-ID | Ja |
| TargetPlayerId | long | Ziel-Spieler ID | Ja |

#### Erwartete Response
- **Bei Erfolg:** `AdminCommandResult` (2301) mit Success=true
- **Bei Fehler:** `AdminCommandResult` (2301) mit ErrorCode

---

### AdminAppearPlayer (2328)

**Richtung:** 📤 Client → Server  
**Frequenz:** Bei Bedarf  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Admin+

#### Beschreibung
Teleportiert den Admin zu einem Spieler.

#### Request Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| RequestId | uint | Eindeutige Request-ID | Ja |
| TargetPlayerId | long | Ziel-Spieler ID | Ja |

#### Erwartete Response
- **Bei Erfolg:** `AdminCommandResult` (2301) mit Success=true
- **Bei Fehler:** `AdminCommandResult` (2301) mit ErrorCode

---

### AdminPlayerInfo (2329)

**Richtung:** 📤 Client → Server  
**Frequenz:** Bei Bedarf  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Moderator+

#### Beschreibung
Fordert detaillierte Informationen über einen Spieler an.

#### Request Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| RequestId | uint | Eindeutige Request-ID | Ja |
| TargetPlayerId | long | Ziel-Spieler ID | Ja |

#### Erwartete Response
- **Bei Erfolg:** `AdminCommandResult` (2301) mit PlayerInfoDto
- **Bei Fehler:** `AdminCommandResult` (2301) mit ErrorCode

---

### AdminServerInfo (2330)

**Richtung:** 📤 Client → Server  
**Frequenz:** Bei Bedarf  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Admin+

#### Beschreibung
Fordert Server-Status und Performance-Informationen an.

#### Request Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| RequestId | uint | Eindeutige Request-ID | Ja |

#### Erwartete Response
- **Bei Erfolg:** `AdminCommandResult` (2301) mit ServerInfoDto
- **Bei Fehler:** `AdminCommandResult` (2301) mit ErrorCode

---

### AdminReloadConfig (2331)

**Richtung:** 📤 Client → Server  
**Frequenz:** Bei Bedarf  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 SuperAdmin

#### Beschreibung
Lädt die Server-Konfiguration neu (ohne Neustart).

#### Request Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| RequestId | uint | Eindeutige Request-ID | Ja |
| ConfigSection | string | Spezifische Sektion (optional) | Nein |

#### Erwartete Response
- **Bei Erfolg:** `AdminCommandResult` (2301) mit Success=true
- **Bei Fehler:** `AdminCommandResult` (2301) mit ErrorCode

---

### AdminReloadScripts (2332)

**Richtung:** 📤 Client → Server  
**Frequenz:** Bei Bedarf  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 SuperAdmin

#### Beschreibung
Lädt Server-Scripts neu (Hot-Reload).

#### Request Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| RequestId | uint | Eindeutige Request-ID | Ja |
| ScriptName | string | Spezifisches Script (optional) | Nein |

#### Erwartete Response
- **Bei Erfolg:** `AdminCommandResult` (2301) mit Success=true
- **Bei Fehler:** `AdminCommandResult` (2301) mit ErrorCode

---

### AdminShutdown (2333)

**Richtung:** 📤 Client → Server  
**Frequenz:** Bei Bedarf  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 SuperAdmin

#### Beschreibung
Fährt den Server herunter (mit Verzögerung für Spieler-Benachrichtigung).

#### Request Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| RequestId | uint | Eindeutige Request-ID | Ja |
| Delay | int | Verzögerung in Sekunden (Min: 60) | Ja |
| Reason | string | Grund für Shutdown | Ja |

#### Erwartete Response
- **Bei Erfolg:** `AdminCommandResult` (2301) mit Success=true
- **Bei Fehler:** `AdminCommandResult` (2301) mit ErrorCode

---

### AdminRestart (2334)

**Richtung:** 📤 Client → Server  
**Frequenz:** Bei Bedarf  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 SuperAdmin

#### Beschreibung
Startet den Server neu (mit Verzögerung).

#### Request Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| RequestId | uint | Eindeutige Request-ID | Ja |
| Delay | int | Verzögerung in Sekunden (Min: 60) | Ja |
| Reason | string | Grund für Neustart | Ja |

#### Erwartete Response
- **Bei Erfolg:** `AdminCommandResult` (2301) mit Success=true
- **Bei Fehler:** `AdminCommandResult` (2301) mit ErrorCode

---

### AdminMaintenance (2335)

**Richtung:** 📤 Client → Server  
**Frequenz:** Bei Bedarf  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 SuperAdmin

#### Beschreibung
Aktiviert/Deaktiviert den Wartungsmodus.

#### Request Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| RequestId | uint | Eindeutige Request-ID | Ja |
| Enabled | bool | Wartungsmodus aktivieren? | Ja |
| Message | string | Wartungs-Nachricht | Bei Aktivierung |
| ExpectedDuration | int | Erwartete Dauer in Minuten | Nein |

#### Erwartete Response
- **Bei Erfolg:** `AdminCommandResult` (2301) mit Success=true
- **Bei Fehler:** `AdminCommandResult` (2301) mit ErrorCode

---

### AdminLog (2336)

**Richtung:** 📥 Server → Client  
**Frequenz:** Bei Admin-Aktionen  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Moderator+

#### Beschreibung
Server sendet Admin-Log-Einträge an verbundene Admins (für Live-Monitoring).

#### Response Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Timestamp | long | Unix Timestamp | Ja |
| AdminId | long | Admin der die Aktion ausführte | Ja |
| AdminName | string | Admin-Name | Ja |
| Action | string | Ausgeführte Aktion | Ja |
| TargetId | long | Ziel der Aktion (falls vorhanden) | Nein |
| TargetName | string | Ziel-Name | Nein |
| Details | string | Zusätzliche Details | Nein |

#### Beispiel Payload
```csharp
var log = new AdminLog
{
    Type = MessageType.AdminLog,
    Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
    AdminId = 1001,
    AdminName = "AdminPlayer",
    Action = "Kick",
    TargetId = 2002,
    TargetName = "ViolatingPlayer",
    Details = "Reason: Spamming in chat"
};
```

---

## Anhang

### MessageType Enum (Reihenfolge wie Code)

```csharp
// ═══════════════════════════════════════════════════════════════
// ADMIN (2300-2399)
// ═══════════════════════════════════════════════════════════════
AdminCommand = 2300,
AdminCommandResult = 2301,
AdminTeleport = 2302,
AdminTeleportPlayer = 2303,
AdminKick = 2304,
AdminBan = 2305,
AdminUnban = 2306,
AdminMute = 2307,
AdminUnmute = 2308,
AdminSpawn = 2309,
AdminDespawn = 2310,
AdminKill = 2311,
AdminRevive = 2312,
AdminHeal = 2313,
AdminGodMode = 2314,
AdminInvisible = 2315,
AdminFreeze = 2316,
AdminUnfreeze = 2317,
AdminGiveItem = 2318,
AdminRemoveItem = 2319,
AdminSetLevel = 2320,
AdminSetStat = 2321,
AdminSetReputation = 2322,
AdminAddGold = 2323,
AdminRemoveGold = 2324,
AdminAnnounce = 2325,
AdminWhisper = 2326,
AdminSummonPlayer = 2327,
AdminAppearPlayer = 2328,
AdminPlayerInfo = 2329,
AdminServerInfo = 2330,
AdminReloadConfig = 2331,
AdminReloadScripts = 2332,
AdminShutdown = 2333,
AdminRestart = 2334,
AdminMaintenance = 2335,
AdminLog = 2336,
```

### Request/Response Paare

| Request | ID | Response | ID | Beschreibung |
|---------|-----|----------|-----|--------------|
| AdminCommand | 2300 | AdminCommandResult | 2301 | Generischer Command |
| AdminTeleport | 2302 | AdminCommandResult | 2301 | Self-Teleport |
| AdminTeleportPlayer | 2303 | AdminCommandResult | 2301 | Player-Teleport |
| AdminKick | 2304 | AdminCommandResult | 2301 | Player kick |
| AdminBan | 2305 | AdminCommandResult | 2301 | Player ban |
| AdminUnban | 2306 | AdminCommandResult | 2301 | Player unban |
| AdminMute | 2307 | AdminCommandResult | 2301 | Player mute |
| AdminUnmute | 2308 | AdminCommandResult | 2301 | Player unmute |
| AdminSpawn | 2309 | AdminCommandResult | 2301 | Entity spawn |
| AdminDespawn | 2310 | AdminCommandResult | 2301 | Entity despawn |
| AdminKill | 2311 | AdminCommandResult | 2301 | Entity kill |
| AdminRevive | 2312 | AdminCommandResult | 2301 | Player revive |
| AdminHeal | 2313 | AdminCommandResult | 2301 | Entity heal |
| AdminGodMode | 2314 | AdminCommandResult | 2301 | Toggle godmode |
| AdminInvisible | 2315 | AdminCommandResult | 2301 | Toggle invisibility |
| AdminFreeze | 2316 | AdminCommandResult | 2301 | Player freeze |
| AdminUnfreeze | 2317 | AdminCommandResult | 2301 | Player unfreeze |
| AdminGiveItem | 2318 | AdminCommandResult | 2301 | Give item |
| AdminRemoveItem | 2319 | AdminCommandResult | 2301 | Remove item |
| AdminSetLevel | 2320 | AdminCommandResult | 2301 | Set level |
| AdminSetStat | 2321 | AdminCommandResult | 2301 | Set stat |
| AdminSetReputation | 2322 | AdminCommandResult | 2301 | Set reputation |
| AdminAddGold | 2323 | AdminCommandResult | 2301 | Add gold |
| AdminRemoveGold | 2324 | AdminCommandResult | 2301 | Remove gold |
| AdminAnnounce | 2325 | AdminCommandResult | 2301 | Server announcement |
| AdminWhisper | 2326 | AdminCommandResult | 2301 | GM whisper |
| AdminSummonPlayer | 2327 | AdminCommandResult | 2301 | Summon player |
| AdminAppearPlayer | 2328 | AdminCommandResult | 2301 | Appear at player |
| AdminPlayerInfo | 2329 | AdminCommandResult | 2301 | Get player info |
| AdminServerInfo | 2330 | AdminCommandResult | 2301 | Get server info |
| AdminReloadConfig | 2331 | AdminCommandResult | 2301 | Reload config |
| AdminReloadScripts | 2332 | AdminCommandResult | 2301 | Reload scripts |
| AdminShutdown | 2333 | AdminCommandResult | 2301 | Server shutdown |
| AdminRestart | 2334 | AdminCommandResult | 2301 | Server restart |
| AdminMaintenance | 2335 | AdminCommandResult | 2301 | Toggle maintenance |

### Fire-and-Forget Messages

Keine - Alle Admin-Commands erfordern eine Response für Feedback.

### Server-Initiated Messages

| Message | ID | Beschreibung |
|---------|-----|--------------|
| AdminLog | 2336 | Admin-Action Log Broadcast |

### Datei-Struktur

```
shared/Mmo.Shared/Messaging/
├── Enums/
│   ├── MessageType.cs          # Admin = 2300-2336
│   ├── AdminRole.cs            # Admin permission levels
│   └── AdminErrorCode.cs       # Admin error codes
├── DTOs/
│   └── Admin/
│       ├── AdminActionDto.cs
│       ├── PlayerInfoDto.cs
│       └── ServerInfoDto.cs
└── Messages/
    └── Admin/
        ├── AdminCommand.cs
        ├── AdminCommandResult.cs
        ├── AdminTeleport.cs
        ├── AdminTeleportPlayer.cs
        ├── AdminKick.cs
        ├── AdminBan.cs
        ├── AdminUnban.cs
        ├── AdminMute.cs
        ├── AdminUnmute.cs
        ├── AdminSpawn.cs
        ├── AdminDespawn.cs
        ├── AdminKill.cs
        ├── AdminRevive.cs
        ├── AdminHeal.cs
        ├── AdminGodMode.cs
        ├── AdminInvisible.cs
        ├── AdminFreeze.cs
        ├── AdminUnfreeze.cs
        ├── AdminGiveItem.cs
        ├── AdminRemoveItem.cs
        ├── AdminSetLevel.cs
        ├── AdminSetStat.cs
        ├── AdminSetReputation.cs
        ├── AdminAddGold.cs
        ├── AdminRemoveGold.cs
        ├── AdminAnnounce.cs
        ├── AdminWhisper.cs
        ├── AdminSummonPlayer.cs
        ├── AdminAppearPlayer.cs
        ├── AdminPlayerInfo.cs
        ├── AdminServerInfo.cs
        ├── AdminReloadConfig.cs
        ├── AdminReloadScripts.cs
        ├── AdminShutdown.cs
        ├── AdminRestart.cs
        ├── AdminMaintenance.cs
        └── AdminLog.cs
```

---

**Letzte Aktualisierung**: 2026-01-02  
**Version**: 3.0.0  
**Status**: ✅ Aligned mit MessageType Enum (37 Messages)

[← Zurück zur Übersicht](README.md)
