# ⚙️ System / Network Messages (0900-0999)

**Kategorie:** 9  
**Range:** 0900-0999 (AKTIV)  
**Phase:** Prototyp  
**Status:** 🟢 In Entwicklung

[← Zurück zur Übersicht](README.md)

---

## 📋 Inhaltsverzeichnis

- [🔄 System Flow](#-system-flow)
- [🧱 DTOs / Enums / Interfaces](#-dtos--enums--interfaces)
- [📩 Aktive Messages (0900-0999)](#-aktive-messages-0900-0999)
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
  - [ServerMotd (919)](#servermotd-919)
  - [ServerTime (920)](#servertime-920)
  - [ServerConfig (921)](#serverconfig-921)
  - [ClientConfig (922)](#clientconfig-922)
  - [FeatureToggle (923)](#featuretoggle-923)
  - [AntiCheatWarning (924)](#anticheatwarning-924)
  - [AntiCheatKick (925)](#anticheatkick-925)
  - [MessageBundle (950)](#messagebundle-950)
- [🗑️ Obsolete Messages](#️-obsolete-messages)
- [📎 Anhang](#-anhang)

---

## 🔄 System Flow

### Architektur: Network Monitoring & System Events

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                        SYSTEM / NETWORK LAYER                               │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  ┌───────────────┐    ┌────────────────┐    ┌────────────────┐             │
│  │ NETWORK       │    │ ERROR          │    │ SERVER         │             │
│  │ MONITORING    │    │ HANDLING       │    │ MANAGEMENT     │             │
│  ├───────────────┤    ├────────────────┤    ├────────────────┤             │
│  │ Ping (900)    │    │ ErrorMessage   │    │ Announcement   │             │
│  │ Pong (901)    │    │ (910)          │    │ (911)          │             │
│  │ LatencyReport │    │                │    │ Kick (912)     │             │
│  │ (902)         │    │                │    │ Maintenance    │             │
│  │ NetworkStats  │    │                │    │ (913)          │             │
│  │ (903)         │    │                │    │ Shutdown (914) │             │
│  │ ConnQuality   │    │                │    │ Ban (916)      │             │
│  │ (904)         │    │                │    │ Status (918)   │             │
│  └───────────────┘    └────────────────┘    └────────────────┘             │
│                                                                             │
│  ┌───────────────┐    ┌────────────────┐    ┌────────────────┐             │
│  │ CONFIG        │    │ SECURITY       │    │ TIME           │             │
│  │ SYNC          │    │ / ANTI-CHEAT   │    │ SYNC           │             │
│  ├───────────────┤    ├────────────────┤    ├────────────────┤             │
│  │ ServerConfig  │    │ Version (915)  │    │ ServerTime     │             │
│  │ (921)         │    │ RateLimit(917) │    │ (920)          │             │
│  │ ClientConfig  │    │ ACWarning(924) │    │                │             │
│  │ (922)         │    │ ACKick (925)   │    │                │             │
│  │ FeatureToggle │    │                │    │                │             │
│  │ (923)         │    │                │    │                │             │
│  └───────────────┘    └────────────────┘    └────────────────┘             │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### Ping/Pong Flow (Latency Measurement)

```
Client                            Server
  │                                  │
  │  Ping (900)                      │
  │  [Timestamp, SequenceNumber]     │
  │─────────────────────────────────►│
  │                                  │
  │                    Pong (901)    │
  │      [Timestamp, SequenceNumber, │
  │                     ServerTime]  │
  │◄─────────────────────────────────│
  │                                  │
  │  RTT = CurrentTime - Timestamp   │
  │                                  │
  │    LatencyReport (902)           │
  │    [AverageLatency, Min, Max,    │
  │     PacketLoss, Jitter]          │
  │◄─────────────────────────────────│ (alle 30s)
  │                                  │
  │    ConnectionQuality (904)       │
  │    [Quality, Latency, Loss]      │
  │◄─────────────────────────────────│ (alle 10s)
```

### Server Maintenance Flow

```
Server                              All Clients
  │                                      │
  │  MaintenanceWarning (913)            │
  │  [MinutesRemaining=60, Reason]       │
  │────────────────────────────────────►│ (60 min)
  │                                      │
  │  MaintenanceWarning (913)            │
  │  [MinutesRemaining=30, Reason]       │
  │────────────────────────────────────►│ (30 min)
  │                                      │
  │  MaintenanceWarning (913)            │
  │  [MinutesRemaining=10, Reason]       │
  │────────────────────────────────────►│ (10 min)
  │                                      │
  │  MaintenanceWarning (913)            │
  │  [MinutesRemaining=1, Reason]        │
  │────────────────────────────────────►│ (1 min)
  │                                      │
  │  ServerShutdown (914)                │
  │  [Reason, ExpectedRestartTime]       │
  │────────────────────────────────────►│
  │                                      │
  │  [All connections closed]            │
```

### Anti-Cheat Detection Flow

```
Server                              Client
  │                                    │
  │  [Detects Speed Anomaly]           │
  │                                    │
  │  AntiCheatWarning (924)            │
  │  [WarningType="speed_anomaly",     │
  │   ViolationCount=1]                │
  │───────────────────────────────────►│
  │                                    │
  │  [Second Violation]                │
  │                                    │
  │  AntiCheatWarning (924)            │
  │  [ViolationCount=2]                │
  │───────────────────────────────────►│
  │                                    │
  │  [Third Violation - Threshold]     │
  │                                    │
  │  AntiCheatKick (925)               │
  │  [Reason, DetectionType,           │
  │   CanReconnect=false]              │
  │───────────────────────────────────►│
  │                                    │
  │  [Connection terminated]           │
```

---

## 🧱 DTOs / Enums / Interfaces

### ConnectionQualityLevel (Enum)

```csharp
public enum ConnectionQualityLevel : byte
{
    Excellent = 0,  // <30ms, <0.5% loss
    Good = 1,       // 30-60ms, 0.5-2% loss
    Fair = 2,       // 60-100ms, 2-5% loss
    Poor = 3,       // 100-200ms, 5-10% loss
    Bad = 4         // >200ms, >10% loss
}
```

### ErrorSeverity (Enum)

```csharp
public enum ErrorSeverity : byte
{
    Info = 0,       // Informational
    Warning = 1,    // Warning, continue operation
    Error = 2,      // Error, operation failed
    Fatal = 3       // Fatal, disconnect required
}
```

### AnnouncementType (Enum)

```csharp
public enum AnnouncementType : byte
{
    Info = 0,       // General info
    Warning = 1,    // Warning message
    Event = 2,      // Event announcement
    Update = 3      // Update/patch info
}
```

### RateLimitType (Enum)

```csharp
public enum RateLimitType : byte
{
    Message = 0,    // Chat/general messages
    Action = 1,     // Game actions
    Login = 2,      // Login attempts
    Movement = 3    // Movement updates
}
```

### AntiCheatDetectionType (Enum)

```csharp
public enum AntiCheatDetectionType : byte
{
    SpeedAnomaly = 0,       // Movement too fast
    PositionAnomaly = 1,    // Impossible position
    ActionAnomaly = 2,      // Impossible action rate
    SpeedHack = 3,          // Confirmed speed hack
    TeleportHack = 4        // Confirmed teleport hack
}
```

### LatencyReportDto

```csharp
[MessagePackObject]
public class LatencyReportDto
{
    [Key(0)] public int AverageLatency { get; set; }   // ms
    [Key(1)] public int MinLatency { get; set; }       // ms
    [Key(2)] public int MaxLatency { get; set; }       // ms
    [Key(3)] public float PacketLossRate { get; set; } // 0.0-1.0
    [Key(4)] public int Jitter { get; set; }           // ms
    [Key(5)] public int SampleCount { get; set; }
}
```

### NetworkStatsDto

```csharp
[MessagePackObject]
public class NetworkStatsDto
{
    [Key(0)] public long BytesSent { get; set; }
    [Key(1)] public long BytesReceived { get; set; }
    [Key(2)] public long MessagesSent { get; set; }
    [Key(3)] public long MessagesReceived { get; set; }
    [Key(4)] public long PacketsSent { get; set; }
    [Key(5)] public long PacketsReceived { get; set; }
    [Key(6)] public long PacketsLost { get; set; }
    [Key(7)] public int AverageBandwidthOut { get; set; }  // KB/s
    [Key(8)] public int AverageBandwidthIn { get; set; }   // KB/s
    [Key(9)] public long ConnectionUptime { get; set; }    // seconds
}
```

### ServerConfigDto

```csharp
[MessagePackObject]
public class ServerConfigDto
{
    [Key(0)] public int TickRate { get; set; }           // Hz
    [Key(1)] public float MaxSpeed { get; set; }         // units/s
    [Key(2)] public int MaxPlayersPerZone { get; set; }
    [Key(3)] public Dictionary<string, bool> Features { get; set; }
}
```

---

## 📩 Aktive Messages (0900-0999)

### Ping (900)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig (alle 5 Sekunden)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

#### Beschreibung
Manueller Ping-Test zusätzlich zum Heartbeat (4). Wird für präzise Latency-Messung verwendet, unabhängig vom Keep-Alive-System.

#### Im Scope ✅
- Latency-Messung (RTT - Round-Trip-Time)
- Network-Performance-Monitoring
- Separate von Heartbeat für spezifische Tests

#### Nicht im Scope ❌
- Keep-Alive → verwende `Heartbeat` (4)
- Detaillierte Network-Stats → verwende `NetworkStats` (903)

#### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Timestamp | long | Client Unix Timestamp (Millisekunden) | Ja |
| SequenceNumber | uint | Ping-Sequence-Number | Ja |

#### Erwartete Response
- **Immer:** `Pong` (901) mit gleichem Timestamp und SequenceNumber

#### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `Pong` | 901 | Response auf Ping |
| `Heartbeat` | 4 | Keep-Alive (parallel) |
| `LatencyReport` | 902 | Aggregierte Latency-Daten |

#### Beispiel Payload
```csharp
var ping = new Ping
{
    Type = MessageType.Ping,
    Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
    SequenceNumber = pingSequence++
};
```

#### Notizen
- **Interval**: 5 Sekunden (kann bei Bedarf höher sein)
- **RTT-Calculation**: `RTT = CurrentTime - ReceivedTimestamp`
- **Use-Case**: UI-Latency-Anzeige, Network-Diagnostics

---

### Pong (901)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

#### Beschreibung
Response auf Ping. Server echo't Timestamp und SequenceNumber zurück. Client berechnet RTT.

#### Im Scope ✅
- Echo von Timestamp und SequenceNumber
- RTT-Berechnung ermöglichen

#### Nicht im Scope ❌
- Server-Side-Latency-Berechnung → Server logged selbst

#### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Timestamp | long | Echo von Ping-Timestamp | Ja |
| SequenceNumber | uint | Echo von Ping-SequenceNumber | Ja |
| ServerTime | long | Aktueller Server-Timestamp | Ja |

#### Beispiel Payload
```csharp
var pong = new Pong
{
    Type = MessageType.Pong,
    Timestamp = receivedPing.Timestamp,
    SequenceNumber = receivedPing.SequenceNumber,
    ServerTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
};
```

#### Notizen
- **RTT**: `CurrentTime - Timestamp`
- **Server-Time-Sync**: Client kann `ServerTime` für Clock-Sync nutzen
- **Sequence-Tracking**: Fehlende Sequences zeigen Packet-Loss

---

### LatencyReport (902)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten (alle 30 Sekunden)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

#### Beschreibung
Server sendet aggregierte Latency-Statistiken. Hilfreich für Client-UI-Anzeige und Diagnostics.

#### Im Scope ✅
- Average Latency (RTT)
- Min/Max Latency
- Packet-Loss-Rate
- Jitter

#### Nicht im Scope ❌
- Real-Time-Latency → verwende `Ping/Pong` (900/901)
- Detaillierte Stats → verwende `NetworkStats` (903)

#### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| AverageLatency | int | Durchschnittliche RTT (Millisekunden) | Ja |
| MinLatency | int | Niedrigste RTT | Ja |
| MaxLatency | int | Höchste RTT | Ja |
| PacketLossRate | float | Packet-Loss (0.0-1.0) | Ja |
| Jitter | int | Jitter (Millisekunden) | Ja |
| SampleCount | int | Anzahl Samples für Berechnung | Ja |

#### Beispiel Payload
```csharp
var latencyReport = new LatencyReport
{
    Type = MessageType.LatencyReport,
    AverageLatency = 45,
    MinLatency = 32,
    MaxLatency = 78,
    PacketLossRate = 0.02f,
    Jitter = 8,
    SampleCount = 60
};
```

---


---

### NetworkStats (903)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten (alle 60 Sekunden)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

#### Beschreibung
Detaillierte Network-Statistiken für Diagnostics und Monitoring.

#### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| BytesSent | long | Bytes gesendet | Ja |
| BytesReceived | long | Bytes empfangen | Ja |
| MessagesSent | long | Messages gesendet | Ja |
| MessagesReceived | long | Messages empfangen | Ja |
| PacketsLost | long | Geschätzte Packet-Loss | Ja |
| AverageBandwidthOut | int | KB/s Out | Ja |
| AverageBandwidthIn | int | KB/s In | Ja |
| ConnectionUptime | long | Sekunden seit Connect | Ja |

---

### ConnectionQuality (904)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig (alle 10 Sekunden)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

#### Beschreibung
QoS (Quality-of-Service) Indicator für Client-UI (Excellent/Good/Fair/Poor/Bad).

#### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Quality | byte | ConnectionQualityLevel (0-4) | Ja |
| Latency | int | Current RTT (ms) | Ja |
| PacketLoss | float | Current Packet-Loss (0.0-1.0) | Ja |

---

### ErrorMessage (910)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten (bei Errors)  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

#### Beschreibung
Universelle Error-Message für alle Fehlerfälle mit Error-Code und Severity.

#### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| ErrorCode | string | Error-Code | Ja |
| Message | string | Error-Message | Ja |
| Severity | byte | ErrorSeverity (0-3) | Ja |
| Details | string | Zusätzliche Details | Nein |
| Timestamp | long | Server-Timestamp | Ja |

---

### ServerAnnouncement (911)

**Richtung:** 📡 Broadcast  
**Frequenz:** Selten  
**Authentifizierung:** Nein  
**Spezielle Rechte:** 👑 Server/Admin

#### Beschreibung
Server-weite Announcement-Message (Events, Updates).

#### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Message | string | Announcement-Text | Ja |
| AnnouncementType | byte | Type (0-3) | Ja |
| Duration | int | Anzeigedauer (Sekunden) | Ja |
| Timestamp | long | Server-Timestamp | Ja |

---

### KickNotification (912)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** Nein  
**Spezielle Rechte:** 👑 Admin

#### Beschreibung
Spieler wird gekickt. Connection wird nach Message geschlossen.

#### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Reason | string | Kick-Grund | Ja |
| AdminName | string | Kickender Admin | Nein |
| CanReconnect | bool | Reconnect erlaubt? | Ja |
| ReconnectDelay | int | Sekunden bis Reconnect | Nein |

---

### MaintenanceWarning (913)

**Richtung:** 📡 Broadcast  
**Frequenz:** Selten  
**Authentifizierung:** Nein  
**Spezielle Rechte:** 👑 Server

#### Beschreibung
Warnung vor anstehender Server-Wartung.

#### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| MinutesRemaining | int | Minuten bis Shutdown | Ja |
| Reason | string | Maintenance-Grund | Ja |
| ExpectedDuration | int | Erwartete Downtime (min) | Nein |

---

### ServerShutdown (914)

**Richtung:** 📡 Broadcast  
**Frequenz:** Einmalig  
**Authentifizierung:** Nein  
**Spezielle Rechte:** 👑 Server

#### Beschreibung
Server fährt herunter. Alle Connections werden geschlossen.

#### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Reason | string | Shutdown-Grund | Ja |
| ExpectedRestartTime | long | Unix Timestamp (Restart) | Nein |
| Message | string | Additional Info | Nein |

---

### VersionMismatch (915)

**Richtung:** 📥 Server → Client  
**Frequenz:** Einmalig (bei Login)  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

#### Beschreibung
Client-Version ist inkompatibel. Connection wird abgelehnt.

#### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| ClientVersion | string | Erkannte Client-Version | Ja |
| RequiredVersion | string | Benötigte Version | Ja |
| DownloadUrl | string | URL für Update | Nein |
| Message | string | Info-Text | Ja |

---

### BanNotification (916)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** Nein  
**Spezielle Rechte:** 👑 Admin

#### Beschreibung
Account ist gebannt. Connection wird permanent abgelehnt.

#### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Reason | string | Ban-Grund | Ja |
| IsPermanent | bool | Permanent-Ban? | Ja |
| ExpiryTime | long | Ban-Ende (falls Temp) | Nein |
| AppealUrl | string | URL für Appeal | Nein |
| Message | string | Zusätzliche Info | Nein |

---

### RateLimitWarning (917)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig (bei Violations)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

#### Beschreibung
Rate-Limiting aktiv. Warnung vor Auto-Kick.

#### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| LimitType | byte | RateLimitType (0-3) | Ja |
| ViolationCount | int | Anzahl Violations | Ja |
| CooldownSeconds | int | Sekunden warten | Ja |
| ThresholdUntilKick | int | Violations bis Kick | Ja |

---

### ServerStatus (918)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

#### Beschreibung
Server-Status-Informationen (Player-Count, Uptime, Version).

#### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| ServerName | string | Server-Name | Ja |
| ServerVersion | string | Server-Version | Ja |
| PlayerCount | int | Online Spieler | Ja |
| MaxPlayers | int | Max Kapazität | Ja |
| UptimeSeconds | long | Uptime (Sekunden) | Ja |
| Timestamp | long | Server-Timestamp | Ja |

---

### ServerMotd (919)

**Richtung:** 📥 Server → Client  
**Frequenz:** Einmalig (bei Login)  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

#### Beschreibung
Message-of-the-Day beim Login.

#### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Title | string | MOTD-Title | Ja |
| Message | string | MOTD-Content | Ja |
| IsHtml | bool | HTML-Formatting? | Ja |

---

### ServerTime (920)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig (alle 60s)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

#### Beschreibung
Server-Zeit-Synchronisation für Clock-Offset.

#### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| ServerTime | long | Unix Timestamp (ms) | Ja |

---

### ServerConfig (921)

**Richtung:** 📥 Server → Client  
**Frequenz:** Einmalig (bei Login)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

#### Beschreibung
Server-Configuration für Client (Tick-Rate, Max-Speed, Features).

#### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| TickRate | int | Server-Tick-Rate (Hz) | Ja |
| MaxSpeed | float | Max Movement-Speed | Ja |
| MaxPlayers | int | Max Spieler pro Zone | Ja |
| Features | Dictionary<string, bool> | Feature-Toggles | Ja |

---

### ClientConfig (922)

**Richtung:** 📤 Client → Server  
**Frequenz:** Einmalig (bei Login)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

#### Beschreibung
Client sendet Configuration an Server (Cloud-Sync).

#### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| GraphicsQuality | string | "low"/"medium"/"high"/"ultra" | Ja |
| Resolution | string | z.B. "1920x1080" | Ja |
| InputMode | string | "keyboard_mouse"/"gamepad" | Ja |

#### Erwartete Response
- **Immer:** `ServerConfig` (921) als Acknowledgment

---

### FeatureToggle (923)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Server

#### Beschreibung
Dynamisches An/Ausschalten von Features (Hot-Toggle).

#### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| FeatureName | string | Feature-Name | Ja |
| Enabled | bool | An/Aus | Ja |
| Reason | string | Grund | Nein |

---

### AntiCheatWarning (924)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

#### Beschreibung
Anti-Cheat hat verdächtige Aktivität erkannt. Warnung an Spieler.

#### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| WarningType | string | Detection-Type | Ja |
| ViolationCount | int | Anzahl Violations | Ja |
| Message | string | Warning-Text | Ja |

---

### AntiCheatKick (925)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

#### Beschreibung
Anti-Cheat kicked Spieler. Connection wird geschlossen.

#### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Reason | string | Kick-Grund | Ja |
| DetectionType | string | Detection-Type | Ja |
| CanReconnect | bool | Reconnect erlaubt? | Ja |
| AppealUrl | string | URL für Appeal | Nein |

---

### MessageBundle (950)

**Richtung:** 📥 Server → Client  
**Frequenz:** ⚡ High-Frequency (every tick)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

#### Beschreibung

Container-Message zum Bündeln mehrerer ausgehender Nachrichten pro Tick pro Client. Reduziert TCP-Overhead, Syscalls und verbessert Netzwerk-Effizienz durch das Zusammenfassen kleiner Payloads in einen Frame.

#### Im Scope ✅

- Bündeln mehrerer Server→Client Messages in einem TCP-Frame
- Reduzierung von TCP-Overhead (weniger Frame-Headers)
- Reduzierung von Syscalls (ein `send()` statt vielen)
- Konsistente State-Updates (alle Änderungen kommen zusammen an)
- ServerTick und Timestamp für Synchronisation

#### Nicht im Scope ❌

- Client→Server Messages bündeln
- Latenz-kritische Messages bündeln (z.B. `MovementCorrection`, `Pong`)
- Kritische Disconnect-Messages bündeln (z.B. `ForceDisconnect`)
- Automatisches Entpacken (Client muss jede Sub-Message einzeln deserialisieren)

#### Response Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| ServerTick | long | Aktueller Server-Tick-Number | Ja |
| Timestamp | long | Server-Timestamp (Millisekunden) | Ja |
| Messages | List&lt;byte[]&gt; | Array von pre-serialisierten Sub-Messages | Ja |

#### Sub-Messages die GEBÜNDELT werden sollten

✅ **High-Frequency Updates:**
- `EntityUpdateBatch` (1405) - Entity-State-Änderungen
- `PositionBroadcast` (201) - Bewegungen anderer Spieler
- `StatUpdate` (602) - Character-Stat-Änderungen
- `BuffApplied` (1500) / `BuffRemoved` (1501) - Buff-Events
- `EntitySpawnBatch` (1401) / `EntityDespawnBatch` (1403)
- `ChatBroadcast` (401) - Chat-Messages (nicht zeitkritisch)

#### Sub-Messages die NIEMALS gebündelt werden dürfen

❌ **Latenz-kritisch oder System-kritisch:**
- `ForceDisconnect` (5) - Muss sofort ankommen
- `MovementCorrection` (202) - Latenz-kritisch für Client-Side Prediction
- `Pong` (901) - Für präzise RTT-Messung erforderlich
- `KickNotification` (912) - Muss vor Connection-Close ankommen
- `ServerShutdown` (914) - Kritische Server-Message
- `BanNotification` (916) - Muss sofort zugestellt werden

#### Verwandte Messages

| Message | ID | Beziehung |
|---------|-----|-----------|
| `EntityUpdateBatch` | 1405 | Häufig in Bundle enthalten |
| `PositionBroadcast` | 201 | Häufig in Bundle enthalten |
| `StatUpdate` | 602 | Häufig in Bundle enthalten |
| Alle anderen High-Frequency Messages | - | Potentielle Sub-Messages |

#### Beispiel Payload

```csharp
// Server-Side: Messages sammeln und bündeln
var messagesToSend = new List<IServerMessage>
{
    new PositionBroadcast { PlayerId = 123, X = 10.5f, Y = 20.3f },
    new StatUpdate { PlayerId = 123, Health = 85, MaxHealth = 100 },
    new BuffApplied { PlayerId = 123, BuffId = 5, Duration = 30 }
};

// Pre-serialize alle Sub-Messages
var serializedMessages = messagesToSend
    .Select(msg => MessageSerializer.Serialize(msg))
    .ToList();

// Erstelle MessageBundle
var bundle = new MessageBundle
{
    Type = MessageType.MessageBundle,
    ServerTick = gameServer.CurrentTick,
    Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
    Messages = serializedMessages
};

// Sende Bundle (ein TCP-Frame statt drei)
await clientConnection.SendAsync(bundle);
```

```csharp
// Client-Side: Bundle empfangen und entpacken
public void HandleMessageBundle(MessageBundle bundle)
{
    // Verarbeite jeden Sub-Message einzeln
    foreach (var messageBytes in bundle.Messages)
    {
        // Deserialize Sub-Message
        var message = MessageSerializer.Deserialize(messageBytes);
        
        // Route zur entsprechenden Handler-Methode
        MessageRouter.Route(message);
    }
}
```

#### Notizen

- **Bundling-Strategie**: Server sammelt alle ausgehenden Messages pro Client während der Output-Phase (Game-Loop)
- **Threshold**: Nur bündeln wenn ≥2 Messages vorhanden (sonst direktes Senden)
- **Performance**: Bei 10 Messages pro Tick: ~90% weniger Syscalls, ~3% weniger Bytes
- **Client-Implementation**: Client muss alle Sub-Messages einzeln deserialisieren und routen
- **Server-Tick**: Ermöglicht Client-Side Interpolation/Prediction-Adjustments
- **Timestamp**: Für Latency-Compensation und Time-Sync

---

## 🗑️ Obsolete Messages

*Derzeit keine obsoleten Messages in dieser Kategorie.*

---

## 📎 Anhang

### MessageType Enum (Kategorie 9 - System)

```csharp
// Exact order from MessageType.cs
Ping = 900,
Pong = 901,
LatencyReport = 902,
NetworkStats = 903,
ConnectionQuality = 904,
ErrorMessage = 910,
ServerAnnouncement = 911,
KickNotification = 912,
MaintenanceWarning = 913,
ServerShutdown = 914,
VersionMismatch = 915,
BanNotification = 916,
RateLimitWarning = 917,
ServerStatus = 918,
ServerMotd = 919,
ServerTime = 920,
ServerConfig = 921,
ClientConfig = 922,
FeatureToggle = 923,
AntiCheatWarning = 924,
AntiCheatKick = 925,
MessageBundle = 950,
```

### Request/Response Paare

| Request | ID | Response | ID |
|---------|-----|----------|-----|
| `Ping` | 900 | `Pong` | 901 |
| `ClientConfig` | 922 | `ServerConfig` | 921 |

### Wichtige Konstanten

```csharp
public static class SystemConstants
{
    public const int PING_INTERVAL_MS = 5000;
    public const int LATENCY_REPORT_INTERVAL_MS = 30000;
    public const int CONNECTION_QUALITY_INTERVAL_MS = 10000;
    public const int SERVER_TIME_SYNC_INTERVAL_MS = 60000;
    public const int LATENCY_EXCELLENT_MS = 30;
    public const int LATENCY_GOOD_MS = 60;
    public const int LATENCY_FAIR_MS = 100;
    public const int LATENCY_POOR_MS = 200;
    public const int ANTICHEAT_WARNING_THRESHOLD = 3;
}
```

---

**Letzte Aktualisierung**: 2026-01-04  
**Version**: 3.1.0

[← Zurück zur Übersicht](README.md)
