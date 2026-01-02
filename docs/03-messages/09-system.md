# ⚙️ Ping / Latency / System Messages (0900-0999)

**Kategorie:** 9  
**Range:** 0900-0999  
**Phase:** Prototyp  
**Status:** 🟢 In Entwicklung

[← Zurück zur Übersicht](README.md)

---

## 📋 Übersicht

Diese Kategorie umfasst alle Messages für **System-Monitoring, Error-Handling und Server-Management** im 2DMMO.

Das System-Message-System bietet:
- **Network-Monitoring**: Ping/Pong, Latency-Tracking, Connection-Quality
- **Error-Handling**: Universelle Error-Messages mit Codes
- **Server-Management**: Announcements, Kick/Ban, Maintenance, Shutdown
- **Anti-Cheat**: Warnings und Auto-Kicks
- **Configuration**: Server/Client-Config-Sync

**Wichtige Konstanten**:
- **PING_INTERVAL**: 5 Sekunden (parallel zu Heartbeat)
- **LATENCY_REPORT_INTERVAL**: 30 Sekunden
- **CONNECTION_QUALITY_UPDATE**: 10 Sekunden

---

## 📋 Inhaltsverzeichnis

- [Ping (900)](#ping-900)
- [Pong (901)](#pong-901)
- [LatencyReport (902)](#latencyreport-902)
- [NetworkStats (903)](#networkstats-903)
- [ConnectionQuality (904)](#connectionquality-904)
- [ErrorMessage (910)](#errormessage-910)
- [ServerAnnouncement (911)](#serverannouncement-911)
- [KickNotification (912)](#kicknotification-912)
- [MaintenanceWarning (913)](#maintenancewarning-913)
- [ServerShutdown (914)](#servershutdown-914)
- [VersionMismatch (915)](#versionmismatch-915)
- [BanNotification (916)](#bannotification-916)
- [RateLimitWarning (917)](#ratelimitwarning-917)
- [ServerStatus (918)](#serverstatus-918)
- [ServerMOTD (919)](#servermotd-919)
- [ServerTime (920)](#servertime-920)
- [ServerConfig (921)](#serverconfig-921)
- [ClientConfig (922)](#clientconfig-922)
- [FeatureToggle (923)](#featuretoggle-923)
- [AntiCheatWarning (924)](#anticheatwarning-924)
- [AntiCheatKick (925)](#anticheatkick-925)

---

## Ping (900)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig (alle 5 Sekunden)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Manueller Ping-Test zusätzlich zum Heartbeat (4). Wird für präzise Latency-Messung verwendet, unabhängig vom Keep-Alive-System.

### Im Scope ✅
- Latency-Messung (RTT - Round-Trip-Time)
- Network-Performance-Monitoring
- Separate von Heartbeat für spezifische Tests

### Nicht im Scope ❌
- Keep-Alive → verwende `Heartbeat` (4)
- Detaillierte Network-Stats → verwende `NetworkStats` (903)

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Timestamp | long | Client Unix Timestamp (Millisekunden) | Ja |
| SequenceNumber | uint | Ping-Sequence-Number | Ja |

### Erwartete Response
- **Immer:** `Pong` (901) mit gleichem Timestamp und SequenceNumber

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `Pong` | 901 | Response auf Ping |
| `Heartbeat` | 4 | Keep-Alive (parallel) |
| `LatencyReport` | 902 | Aggregierte Latency-Daten |

### Beispiel Payload
```csharp
var ping = new Ping
{
    Type = MessageType.Ping,
    Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
    SequenceNumber = pingSequence++
};

// Nach Pong-Empfang: RTT = CurrentTime - Timestamp
```

### Notizen
- **Interval**: 5 Sekunden (kann bei Bedarf höher sein)
- **RTT-Calculation**: `RTT = CurrentTime - ReceivedTimestamp`
- **Use-Case**: UI-Latency-Anzeige, Network-Diagnostics
- **Parallel zu Heartbeat**: Beide laufen gleichzeitig für unterschiedliche Zwecke

---

## Pong (901)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Response auf Ping. Server echo't Timestamp und SequenceNumber zurück. Client berechnet RTT.

### Im Scope ✅
- Echo von Timestamp und SequenceNumber
- RTT-Berechnung ermöglichen

### Nicht im Scope ❌
- Server-Side-Latency-Berechnung → Server logged selbst

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Timestamp | long | Echo von Ping-Timestamp | Ja |
| SequenceNumber | uint | Echo von Ping-SequenceNumber | Ja |
| ServerTime | long | Aktueller Server-Timestamp | Ja |

### Beispiel Payload
```csharp
var pong = new Pong
{
    Type = MessageType.Pong,
    Timestamp = receivedPing.Timestamp, // Echo
    SequenceNumber = receivedPing.SequenceNumber, // Echo
    ServerTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
};
```

### Notizen
- **RTT**: `CurrentTime - Timestamp`
- **Server-Time-Sync**: Client kann `ServerTime` für Clock-Sync nutzen
- **Sequence-Tracking**: Fehlende Sequences zeigen Packet-Loss

---

## LatencyReport (902)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten (alle 30 Sekunden)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Server sendet aggregierte Latency-Statistiken. Hilfreich für Client-UI-Anzeige und Diagnostics.

### Im Scope ✅
- Average Latency (RTT)
- Min/Max Latency
- Packet-Loss-Rate
- Jitter

### Nicht im Scope ❌
- Real-Time-Latency → verwende `Ping/Pong` (900/901)
- Detaillierte Stats → verwende `NetworkStats` (903)

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| AverageLatency | int | Durchschnittliche RTT (Millisekunden) | Ja |
| MinLatency | int | Niedrigste RTT | Ja |
| MaxLatency | int | Höchste RTT | Ja |
| PacketLossRate | float | Packet-Loss (0.0-1.0) | Ja |
| Jitter | int | Jitter (Millisekunden) | Ja |
| SampleCount | int | Anzahl Samples für Berechnung | Ja |

### Beispiel Payload
```csharp
var latencyReport = new LatencyReport
{
    Type = MessageType.LatencyReport,
    AverageLatency = 45, // 45ms
    MinLatency = 32,
    MaxLatency = 78,
    PacketLossRate = 0.02f, // 2%
    Jitter = 8,
    SampleCount = 60
};
```

### Notizen
- **Update-Interval**: 30 Sekunden
- **Sample-Period**: Basierend auf letzten 60 Sekunden
- **UI-Indicator**: Client kann Color-Coding verwenden (Green <50ms, Yellow 50-100ms, Red >100ms)

---

## NetworkStats (903)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten (auf Request oder alle 60 Sekunden)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Detaillierte Network-Statistiken für Diagnostics und Monitoring. Enthält Bandwidth, Packet-Stats, Connection-Info.

### Im Scope ✅
- Bytes Sent/Received
- Messages Sent/Received
- Packet-Loss-Details
- Bandwidth-Usage

### Nicht im Scope ❌
- Simplified Latency → verwende `LatencyReport` (902)

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| BytesSent | long | Bytes gesendet (seit Connection) | Ja |
| BytesReceived | long | Bytes empfangen | Ja |
| MessagesSent | long | Messages gesendet | Ja |
| MessagesReceived | long | Messages empfangen | Ja |
| PacketsSent | long | TCP-Packets gesendet | Ja |
| PacketsReceived | long | TCP-Packets empfangen | Ja |
| PacketsLost | long | Geschätzte Packet-Loss | Ja |
| AverageBandwidthOut | int | KB/s Out | Ja |
| AverageBandwidthIn | int | KB/s In | Ja |
| ConnectionUptime | long | Sekunden seit Connect | Ja |

### Beispiel Payload
```csharp
var netStats = new NetworkStats
{
    Type = MessageType.NetworkStats,
    BytesSent = 2_500_000, // 2.5 MB
    BytesReceived = 3_200_000, // 3.2 MB
    MessagesSent = 15_000,
    MessagesReceived = 18_500,
    PacketsSent = 5_000,
    PacketsReceived = 6_000,
    PacketsLost = 50,
    AverageBandwidthOut = 12, // 12 KB/s
    AverageBandwidthIn = 18, // 18 KB/s
    ConnectionUptime = 3600 // 1 Stunde
};
```

### Notizen
- **Debug-Tool**: Primär für Diagnostics und Support
- **On-Demand**: Client kann Request senden (geplant)
- **Bandwidth-Monitoring**: Hilfreich für Mobile-Clients

---

## ConnectionQuality (904)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig (alle 10 Sekunden)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
QoS (Quality-of-Service) Indicator. Simplified Status für Client-UI-Anzeige (Good/Fair/Poor/Bad).

### Im Scope ✅
- Simplified Quality-Rating
- Latency-Based
- Packet-Loss-Based
- UI-freundlich

### Nicht im Scope ❌
- Detaillierte Stats → verwende `NetworkStats` (903)

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Quality | string | "excellent", "good", "fair", "poor", "bad" | Ja |
| Latency | int | Current RTT (ms) | Ja |
| PacketLoss | float | Current Packet-Loss (0.0-1.0) | Ja |

### Beispiel Payload
```csharp
var connQuality = new ConnectionQuality
{
    Type = MessageType.ConnectionQuality,
    Quality = "good",
    Latency = 45,
    PacketLoss = 0.01f // 1%
};
```

### Quality-Thresholds
| Quality | Latency | Packet-Loss | UI-Color |
|---------|---------|-------------|----------|
| Excellent | <30ms | <0.5% | Green |
| Good | 30-60ms | 0.5-2% | Light Green |
| Fair | 60-100ms | 2-5% | Yellow |
| Poor | 100-200ms | 5-10% | Orange |
| Bad | >200ms | >10% | Red |

### Notizen
- **UI-Indicator**: Client zeigt Connection-Quality-Icon
- **Auto-Adapt**: Bei Poor/Bad kann Client Quality-Settings reduzieren
- **Warning**: Bei "Bad" kann Client Warnung anzeigen

---

## ErrorMessage (910)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten (bei Errors)  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung
Universelle Error-Message für alle Fehlerfälle. Enthält Error-Code, Message, Severity.

### Im Scope ✅
- Alle Server-Errors
- Standardisierte Error-Codes
- Severity-Levels (Info, Warning, Error, Fatal)
- Optional: Context-Data

### Nicht im Scope ❌
- Spezifische Error-Messages → verwende ErrorMessage mit passendem Code

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| ErrorCode | string | Error-Code (z.B. "INVALID_CREDENTIALS") | Ja |
| Message | string | Menschenlesbare Error-Message | Ja |
| Severity | string | "info", "warning", "error", "fatal" | Ja |
| Details | string | Optional zusätzliche Details | Nein |
| Timestamp | long | Server-Timestamp | Ja |

### Beispiel Payload
```csharp
// Login-Error
var loginError = new ErrorMessage
{
    Type = MessageType.ErrorMessage,
    ErrorCode = "INVALID_CREDENTIALS",
    Message = "Username or password incorrect",
    Severity = "error",
    Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
};

// Fatal-Error
var fatalError = new ErrorMessage
{
    Type = MessageType.ErrorMessage,
    ErrorCode = "DATABASE_CONNECTION_LOST",
    Message = "Server database connection lost. Please try again later.",
    Severity = "fatal",
    Details = "Contact support if problem persists",
    Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
};
```

### Common Error Codes
| Code | Severity | Beschreibung |
|------|----------|--------------|
| `INVALID_CREDENTIALS` | error | Login fehlgeschlagen |
| `SESSION_EXPIRED` | warning | Session abgelaufen |
| `RATE_LIMITED` | warning | Zu viele Requests |
| `INSUFFICIENT_PERMISSIONS` | error | Fehlende Berechtigung |
| `RESOURCE_NOT_FOUND` | error | Resource nicht gefunden |
| `SERVER_OVERLOADED` | error | Server überlastet |
| `DATABASE_ERROR` | fatal | Datenbank-Fehler |
| `INTERNAL_SERVER_ERROR` | fatal | Unerwarteter Server-Fehler |

### Notizen
- **UI-Display**: Client zeigt Error-Dialog basierend auf Severity
- **Logging**: Client logged alle Errors für Support
- **Fatal**: Bei Fatal-Errors wird Connection oft geschlossen

---

## ServerAnnouncement (911)

**Richtung:** 📡 Broadcast  
**Frequenz:** Selten  
**Authentifizierung:** Nein  
**Spezielle Rechte:** 👑 Server/Admin

### Beschreibung
Server-weite Announcement-Message (Events, Updates, Wichtige Infos). Alle online Spieler empfangen.

### Im Scope ✅
- Server-weite Announcements
- Event-Announcements
- Important Updates

### Nicht im Scope ❌
- System-Messages → verwende `ChatSystem` (410)
- Personal-Messages → verwende `ChatWhisper` (402)

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Message | string | Announcement-Text | Ja |
| Type | string | "info", "warning", "event", "update" | Ja |
| Duration | int | Anzeigedauer (Sekunden, 0=permanent bis dismiss) | Ja |
| Timestamp | long | Server-Timestamp | Ja |

### Beispiel Payload
```csharp
// Event-Announcement
var eventAnnounce = new ServerAnnouncement
{
    Type = MessageType.ServerAnnouncement,
    Message = "Double XP Weekend starts now!",
    Type = "event",
    Duration = 0, // Permanent bis dismiss
    Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
};

// Update-Announcement
var updateAnnounce = new ServerAnnouncement
{
    Type = MessageType.ServerAnnouncement,
    Message = "New patch 0.3.0 deployed! Check /changelog for details.",
    Type = "update",
    Duration = 30, // 30 Sekunden
    Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
};
```

### Notizen
- **UI-Display**: Client zeigt prominent im UI (z.B. Banner oben)
- **Sound**: Optional Notification-Sound
- **History**: Client speichert letzte 10 Announcements

---

## KickNotification (912)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** Nein  
**Spezielle Rechte:** 👑 Admin

### Beschreibung
Spieler wird gekickt (Admin-Action). Connection wird nach Message geschlossen.

### Im Scope ✅
- Kick-Grund
- Admin-Name (optional)
- Reconnect-Erlaubnis

### Nicht im Scope ❌
- Ban → verwende `BanNotification` (916)
- Normal Disconnect → verwende `Disconnect` (5)

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Reason | string | Kick-Grund | Ja |
| AdminName | string | Kickender Admin | Nein |
| CanReconnect | bool | Reconnect erlaubt? | Ja |
| ReconnectDelay | int | Sekunden bis Reconnect erlaubt | Nein |

### Beispiel Payload
```csharp
var kick = new KickNotification
{
    Type = MessageType.KickNotification,
    Reason = "Inappropriate behavior in chat",
    AdminName = "GM_John",
    CanReconnect = true,
    ReconnectDelay = 300 // 5 Minuten
};
```

### Notizen
- **Auto-Disconnect**: Connection wird 2 Sekunden nach Message geschlossen
- **UI-Dialog**: Client zeigt Kick-Reason prominent
- **Reconnect**: Bei CanReconnect=true nach Delay möglich

---

## MaintenanceWarning (913)

**Richtung:** 📡 Broadcast  
**Frequenz:** Selten  
**Authentifizierung:** Nein  
**Spezielle Rechte:** 👑 Server

### Beschreibung
Warnung vor anstehender Server-Wartung. Mehrere Warnings in absteigenden Intervallen.

### Im Scope ✅
- Time-Until-Shutdown
- Maintenance-Reason
- Expected-Duration

### Nicht im Scope ❌
- Actual-Shutdown → verwende `ServerShutdown` (914)

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| MinutesRemaining | int | Minuten bis Shutdown | Ja |
| Reason | string | Maintenance-Grund | Ja |
| ExpectedDuration | int | Erwartete Downtime (Minuten) | Nein |

### Beispiel Payload
```csharp
// 10 Minuten Warning
var warning10 = new MaintenanceWarning
{
    Type = MessageType.MaintenanceWarning,
    MinutesRemaining = 10,
    Reason = "Server update to version 0.3.0",
    ExpectedDuration = 30
};

// 1 Minute Warning
var warning1 = new MaintenanceWarning
{
    Type = MessageType.MaintenanceWarning,
    MinutesRemaining = 1,
    Reason = "Server update to version 0.3.0",
    ExpectedDuration = 30
};
```

### Warning-Schedule
| Time Remaining | Frequency |
|----------------|-----------|
| 60 min | Once |
| 30 min | Once |
| 15 min | Once |
| 10 min | Once |
| 5 min | Once |
| 1 min | Every 15s |

### Notizen
- **UI-Countdown**: Client zeigt Countdown-Timer
- **Sound**: Alert-Sound bei < 5 Minuten
- **Auto-Logout**: Spieler sollten rechtzeitig ausloggen

---

## ServerShutdown (914)

**Richtung:** 📡 Broadcast  
**Frequenz:** Einmalig  
**Authentifizierung:** Nein  
**Spezielle Rechte:** 👑 Server

### Beschreibung
Server fährt herunter. Alle Connections werden geschlossen. Final Message vor Shutdown.

### Im Scope ✅
- Shutdown-Notification
- Reason
- Expected-Restart-Time

### Nicht im Scope ❌
- Warnings → verwende `MaintenanceWarning` (913)

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Reason | string | Shutdown-Grund | Ja |
| ExpectedRestartTime | long | Unix Timestamp (geschätzter Restart) | Nein |
| Message | string | Additional Info | Nein |

### Beispiel Payload
```csharp
var shutdown = new ServerShutdown
{
    Type = MessageType.ServerShutdown,
    Reason = "Scheduled maintenance",
    ExpectedRestartTime = DateTimeOffset.UtcNow.AddMinutes(30).ToUnixTimeSeconds(),
    Message = "Server will be back online in approximately 30 minutes"
};
```

### Notizen
- **Grace-Period**: 5 Sekunden nach Message, dann Force-Disconnect
- **UI-Message**: Client zeigt Shutdown-Screen
- **Auto-Reconnect**: Client kann Auto-Reconnect nach ExpectedRestartTime versuchen

---

## VersionMismatch (915)

**Richtung:** 📥 Server → Client  
**Frequenz:** Einmalig (bei Login)  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung
Client-Version ist inkompatibel mit Server. Connection wird abgelehnt.

### Im Scope ✅
- Version-Check
- Required-Version
- Download-URL

### Nicht im Scope ❌
- Auto-Update → Client-Feature

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| ClientVersion | string | Erkannte Client-Version | Ja |
| RequiredVersion | string | Benötigte Version | Ja |
| DownloadUrl | string | URL für Update | Nein |
| Message | string | Info-Text | Ja |

### Beispiel Payload
```csharp
var versionMismatch = new VersionMismatch
{
    Type = MessageType.VersionMismatch,
    ClientVersion = "0.2.0",
    RequiredVersion = "0.3.0",
    DownloadUrl = "https://2dmmo.com/download",
    Message = "Your client is outdated. Please update to version 0.3.0."
};
```

### Notizen
- **Connection-Denied**: Client kann nicht connecten
- **UI-Dialog**: Client zeigt Update-Required-Dialog
- **Auto-Redirect**: Optional zu Download-URL

---

## BanNotification (916)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** Nein  
**Spezielle Rechte:** 👑 Admin

### Beschreibung
Account ist gebannt. Connection wird permanent abgelehnt.

### Im Scope ✅
- Ban-Grund
- Ban-Duration (Permanent oder Temp)
- Ban-Expiry (falls Temp-Ban)
- Appeal-URL

### Nicht im Scope ❌
- Kick (Temp) → verwende `KickNotification` (912)

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Reason | string | Ban-Grund | Ja |
| IsPermanent | bool | Permanent-Ban? | Ja |
| ExpiryTime | long | Unix Timestamp (Ban-Ende, falls Temp) | Nein |
| AppealUrl | string | URL für Ban-Appeal | Nein |
| Message | string | Zusätzliche Info | Nein |

### Beispiel Payload
```csharp
// Permanent-Ban
var permBan = new BanNotification
{
    Type = MessageType.BanNotification,
    Reason = "Cheating / Use of third-party tools",
    IsPermanent = true,
    AppealUrl = "https://2dmmo.com/ban-appeal",
    Message = "If you believe this is a mistake, please submit an appeal."
};

// Temp-Ban
var tempBan = new BanNotification
{
    Type = MessageType.BanNotification,
    Reason = "Toxic behavior in chat",
    IsPermanent = false,
    ExpiryTime = DateTimeOffset.UtcNow.AddDays(7).ToUnixTimeSeconds(),
    Message = "Your ban will expire in 7 days."
};
```

### Notizen
- **Connection-Denied**: Account kann nicht connecten
- **UI-Dialog**: Client zeigt Ban-Screen mit Details
- **Appeal**: Link zu Appeal-System falls verfügbar

---

## RateLimitWarning (917)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig (bei Violations)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Client sendet zu viele Requests. Rate-Limiting aktiv. Warnung vor Auto-Kick.

### Im Scope ✅
- Rate-Limit-Violation
- Cooldown-Time
- Violation-Count
- Auto-Kick-Threshold

### Nicht im Scope ❌
- Chat-Spam → verwende `ChatSpamWarning` (430)

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| LimitType | string | "message", "action", "login", "movement" | Ja |
| ViolationCount | int | Anzahl Violations | Ja |
| CooldownSeconds | int | Sekunden warten | Ja |
| ThresholdUntilKick | int | Noch erlaubte Violations bis Kick | Ja |

### Beispiel Payload
```csharp
var rateLimitWarn = new RateLimitWarning
{
    Type = MessageType.RateLimitWarning,
    LimitType = "action",
    ViolationCount = 2,
    CooldownSeconds = 5,
    ThresholdUntilKick = 1 // Noch 1 Violation erlaubt
};
```

### Rate-Limits
| Type | Limit | Window | Kick-Threshold |
|------|-------|--------|----------------|
| Movement | 25/s | 1s | 5 Violations |
| Action | 10/s | 1s | 3 Violations |
| Chat | Siehe ChatSpamWarning | - | - |
| Login | 3/min | 1min | 5 Violations |

### Notizen
- **Auto-Throttle**: Client sollte Requests reduzieren
- **Warning-UI**: Client zeigt Throttle-Warning
- **Auto-Kick**: Bei Threshold wird Client gekickt

---

## ServerStatus (918)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten (auf Request)  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung
Server-Status-Informationen (Player-Count, Uptime, Version).

### Im Scope ✅
- Online-Player-Count
- Server-Uptime
- Server-Version
- Server-Name

### Nicht im Scope ❌
- Detaillierte Stats → Admin-Only

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| ServerName | string | Server-Name | Ja |
| ServerVersion | string | Server-Version | Ja |
| PlayerCount | int | Aktuell online Spieler | Ja |
| MaxPlayers | int | Max Kapazität | Ja |
| UptimeSeconds | long | Uptime in Sekunden | Ja |
| Timestamp | long | Server-Timestamp | Ja |

### Beispiel Payload
```csharp
var serverStatus = new ServerStatus
{
    Type = MessageType.ServerStatus,
    ServerName = "2DMMO - EU Central",
    ServerVersion = "0.3.0",
    PlayerCount = 487,
    MaxPlayers = 1000,
    UptimeSeconds = 86400, // 1 Tag
    Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
};
```

### Notizen
- **Server-Select-Screen**: Anzeige im UI
- **Capacity-Indicator**: (487/1000) → 48.7% full

---

## ServerMOTD (919)

**Richtung:** 📥 Server → Client  
**Frequenz:** Einmalig (bei Login)  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung
Message-of-the-Day beim Login. Wichtige Infos, Events, Updates.

### Im Scope ✅
- Server-MOTD
- HTML-Formatting (optional)

### Nicht im Scope ❌
- Channel-MOTD → verwende `ChatMOTD` (428)

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Title | string | MOTD-Title | Ja |
| Message | string | MOTD-Content | Ja |
| IsHtml | bool | HTML-Formatting? | Ja |

### Beispiel Payload
```csharp
var motd = new ServerMOTD
{
    Type = MessageType.ServerMOTD,
    Title = "Welcome to 2DMMO!",
    Message = "New patch 0.3.0 is live!\n\n- New dungeon: Darkwood Crypt\n- Level cap increased to 20\n- New PvP arena\n\nHave fun!",
    IsHtml = false
};
```

### Notizen
- **UI-Dialog**: Client zeigt MOTD-Dialog beim Login
- **Cache**: Client kann MOTD cachen (Check täglich)

---

## ServerTime (920)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig (auf Request oder alle 60s)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Server-Zeit-Synchronisation. Client kann Clock-Offset berechnen.

### Im Scope ✅
- Präziser Server-Timestamp
- Time-Sync

### Nicht im Scope ❌
- Game-Time (Day/Night-Cycle) → geplant

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| ServerTime | long | Unix Timestamp (Millisekunden) | Ja |

### Beispiel Payload
```csharp
var serverTime = new ServerTime
{
    Type = MessageType.ServerTime,
    ServerTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
};
```

### Notizen
- **Clock-Sync**: Client berechnet Offset: `Offset = ServerTime - ClientTime + RTT/2`
- **Importance**: Für Timestamp-Based-Events (Cooldowns, Buffs, etc.)

---

## ServerConfig (921)

**Richtung:** 📥 Server → Client  
**Frequenz:** Einmalig (bei Login)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Server-Configuration für Client. Limits, Rates, Features.

### Im Scope ✅
- Tick-Rate
- Max-Speed
- Rate-Limits
- Feature-Flags

### Nicht im Scope ❌
- Sensitive-Config (DB-Connection, etc.) → nicht senden

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| TickRate | int | Server-Tick-Rate (Hz) | Ja |
| MaxSpeed | float | Max Movement-Speed | Ja |
| MaxPlayers | int | Max Spieler pro Zone | Ja |
| Features | Dictionary<string, bool> | Feature-Toggles | Ja |

### Beispiel Payload
```csharp
var serverConfig = new ServerConfig
{
    Type = MessageType.ServerConfig,
    TickRate = 25,
    MaxSpeed = 7.0f,
    MaxPlayers = 100,
    Features = new Dictionary<string, bool>
    {
        { "PvP", true },
        { "Trading", false },
        { "Guilds", true },
        { "Mounts", false }
    }
};
```

### Notizen
- **Client-Adaptation**: Client passt Verhalten an Server-Config an
- **Feature-Toggles**: Dynamische Feature-Aktivierung

---

## ClientConfig (922)

**Richtung:** 📤 Client → Server  
**Frequenz:** Einmalig (bei Login)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Client sendet Configuration an Server (Graphics-Settings, Input-Mode, etc.). Server kann Settings speichern.

### Im Scope ✅
- Graphics-Settings
- Input-Mode
- Keybindings (optional)

### Nicht im Scope ❌
- Sensitive-Data → nicht senden

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| GraphicsQuality | string | "low", "medium", "high", "ultra" | Ja |
| Resolution | string | "1920x1080", etc. | Ja |
| InputMode | string | "keyboard_mouse", "gamepad" | Ja |

### Beispiel Payload
```csharp
var clientConfig = new ClientConfig
{
    Type = MessageType.ClientConfig,
    GraphicsQuality = "high",
    Resolution = "1920x1080",
    InputMode = "keyboard_mouse"
};
```

### Notizen
- **Persistence**: Server speichert Settings (Cloud-Sync)
- **Multi-Device**: Settings sync über Devices

---

## FeatureToggle (923)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten (bei Feature-Change)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Server

### Beschreibung
Dynamisches An/Ausschalten von Features (A/B-Testing, Rollout, Emergency-Disable).

### Im Scope ✅
- Feature-Name
- Enabled/Disabled
- Reason (optional)

### Nicht im Scope ❌
- Static-Config → verwende `ServerConfig` (921)

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| FeatureName | string | Feature-Identifier | Ja |
| Enabled | bool | Feature an/aus | Ja |
| Reason | string | Optional Grund | Nein |

### Beispiel Payload
```csharp
// Enable Feature
var enablePvP = new FeatureToggle
{
    Type = MessageType.FeatureToggle,
    FeatureName = "PvP",
    Enabled = true,
    Reason = "PvP season started"
};

// Disable Feature (Emergency)
var disableTrading = new FeatureToggle
{
    Type = MessageType.FeatureToggle,
    FeatureName = "Trading",
    Enabled = false,
    Reason = "Emergency maintenance: dupe bug fix"
};
```

### Notizen
- **Hot-Toggle**: Ohne Server-Restart
- **A/B-Testing**: Verschiedene Features für verschiedene Spieler
- **Emergency-Disable**: Schnell Features deaktivieren bei Bugs

---

## AntiCheatWarning (924)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Anti-Cheat-System hat verdächtige Aktivität erkannt. Warnung an Spieler.

### Im Scope ✅
- Warning-Type
- Detection-Details (vague)
- Violation-Count

### Nicht im Scope ❌
- Detailed-Detection → Security-Risk
- Auto-Kick → verwende `AntiCheatKick` (925)

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| WarningType | string | "speed_anomaly", "position_anomaly", "action_anomaly" | Ja |
| ViolationCount | int | Anzahl Detections | Ja |
| Message | string | Warning-Text | Ja |

### Beispiel Payload
```csharp
var antiCheatWarn = new AntiCheatWarning
{
    Type = MessageType.AntiCheatWarning,
    WarningType = "speed_anomaly",
    ViolationCount = 1,
    Message = "Unusual movement detected. Please ensure you're not using any third-party tools."
};
```

### Notizen
- **First-Warning**: Bei erstem Verdacht
- **False-Positive**: Kann passieren bei Lag
- **Threshold**: 3 Warnings → Auto-Kick

---

## AntiCheatKick (925)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung
Anti-Cheat-System kicked Spieler. Connection wird geschlossen.

### Im Scope ✅
- Kick-Grund
- Detection-Type
- Appeal-Info

### Nicht im Scope ❌
- Permanent-Ban → verwende `BanNotification` (916)

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Reason | string | Kick-Grund | Ja |
| DetectionType | string | Type der Detection | Ja |
| CanReconnect | bool | Reconnect erlaubt? | Ja |
| AppealUrl | string | URL für Appeal | Nein |

### Beispiel Payload
```csharp
var antiCheatKick = new AntiCheatKick
{
    Type = MessageType.AntiCheatKick,
    Reason = "Cheating detected: Speed hack",
    DetectionType = "speed_hack",
    CanReconnect = false,
    AppealUrl = "https://2dmmo.com/anticheat-appeal"
};
```

### Notizen
- **Auto-Disconnect**: Connection sofort geschlossen
- **Review**: Alle Anti-Cheat-Kicks werden manuell reviewed
- **False-Positive**: Appeal-System für Fehler-Fälle
- **Escalation**: Wiederholte Kicks → Permanent-Ban

---

**Letzte Aktualisierung**: 2025-12-17  
**Version**: 1.0.0

[← Zurück zur Übersicht](README.md)
