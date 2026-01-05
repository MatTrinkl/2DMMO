# 🔐 Connection / Authentication Messages (0000-0099)

**Kategorie:** 0  
**Range:** 0000-0099  
**Phase:** Prototyp  
**Status:** 🟢 In Entwicklung

[← Zurück zur Übersicht](Message-Reference.md)

---

## 📋 Inhaltsverzeichnis

- [LoginRequest (1)](#loginrequest-1)
- [LoginResponse (2)](#loginresponse-2)
- [LogoutRequest (3)](#logoutrequest-3)
- [Heartbeat (4)](#heartbeat-4)
- [ForceDisconnect (5)](#forcedisconnect-5)
- [ReconnectRequest (6)](#reconnectrequest-6)
- [ReconnectResponse (7)](#reconnectresponse-7)
- [SessionValidate (8) - Deprecated](#sessionvalidate-8)
- [CharacterSelectRequest (9)](#characterselectrequest-9)
- [CharacterCreateRequest (10)](#charactercreaterequest-10)
- [CharacterDeleteRequest (11)](#characterdeleterequest-11)
- [CharacterListRequest (12)](#characterlistrequest-12)
- [CharacterListResponse (13)](#characterlistresponse-13)
- [ServerSelectRequest (14)](#serverselectrequest-14)
- [RealmListRequest (15)](#realmlistrequest-15)
- [RealmListResponse (16)](#realmlistresponse-16)
- [AccountDataRequest (17)](#accountdatarequest-17)
- [AccountDataResponse (18)](#accountdataresponse-18)
- [EncryptionHandshake (19)](#encryptionhandshake-19)
- [CompressionToggle (20)](#compressiontoggle-20)
- [CharacterSelectResponse (21)](#characterselectresponse-21)
- [CharacterCreateResponse (22)](#charactercreateresponse-22)
- [CharacterDeleteResponse (23)](#characterdeleteresponse-23)
- [ServerSelectResponse (24)](#serverselectresponse-24)

---

## LoginRequest (1)

**Richtung:** 📤 Client → Server  
**Frequenz:** Einmalig pro Session  
**Authentifizierung:** Nein (ist Authentifizierung selbst)  
**Spezielle Rechte:** Keine

### Beschreibung
Initiiert den Login-Prozess. Der Client sendet Credentials (Username/Password oder Token) an den Gateway Server. Dies ist die erste Message nach TCP-Verbindungsaufbau.

### Im Scope ✅
- Username/Password Authentifizierung
- Session Token Authentifizierung
- Client Version Prüfung
- Hardware ID für Anti-Cheat

### Nicht im Scope ❌
- Registrierung → verwende externe Account-API
- Passwort-Reset → verwende externe Account-API
- OAuth/Social Login → verwende externe Account-API (geplant)

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Username | string | Account-Name (3-16 Zeichen) | Ja* |
| Password | string | Passwort (Hash) | Ja* |
| ~~SessionToken~~ | ~~string~~ | ~~Wiederverbindungs-Token~~ | Wird durch 6 ReconnectRequest ersetzt |
| ClientVersion | string | z.B. "0.1.0" | Ja |
| HardwareId | string | Eindeutige Hardware-ID | Ja |

\* Entweder Username+Password ODER SessionToken

### Erwartete Response
- **Bei Erfolg:** `LoginResponse` (2) mit Success=true
- **Bei Fehler:** `LoginResponse` (2) mit ErrorCode

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `LoginResponse` | 2 | Response zu diesem Request |
| `ReconnectRequest` | 6 | Für Wiederverbindung nach Disconnect |
| `SessionValidate` | 8 | Server-interne Token-Validierung |
| `ErrorMessage` | 910 | Bei kritischen Fehlern |

### Flow-Diagramm
```
Client                    Gateway Server             Auth DB
  │                            │                        │
  │  LoginRequest (1)          │                        │
  │───────────────────────────►│                        │
  │                            │  Validate Credentials  │
  │                            │───────────────────────►│
  │                            │                        │
  │                            │  Result                │
  │                            │◄───────────────────────│
  │  LoginResponse (2)         │                        │
  │◄───────────────────────────│                        │
```

### Beispiel Payload
```csharp
var loginRequest = new LoginRequest
{
    Type = MessageType.LoginRequest,
    Username = "PlayerOne",
    Password = ComputePasswordHash("mySecurePassword123"),
    ClientVersion = "0.1.0",
    HardwareId = GetHardwareId()
};
```

### Error Codes
| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `INVALID_CREDENTIALS` | Username oder Passwort falsch | Erneut versuchen mit korrekten Daten |
| `ACCOUNT_BANNED` | Account ist gesperrt | Zeige Ban-Grund und Dauer |
| `VERSION_MISMATCH` | Client-Version zu alt/neu | Update herunterladen |
| `SERVER_FULL` | Server hat maximale Spielerzahl | Warten oder anderen Server wählen |
| `ALREADY_LOGGED_IN` | Account ist bereits eingeloggt | Auf Timeout warten oder erzwingen |
| `RATE_LIMITED` | Zu viele Login-Versuche | Exponential Backoff anwenden |

### Notizen
- Password MUSS als Hash gesendet werden (nicht Klartext)
- ConnectionTimeout: 10 Sekunden für Antwort
- Nach 3 fehlgeschlagenen Versuchen: Rate Limiting aktiv (60s Cooldown)
- HardwareId wird für Multi-Account-Detection verwendet
- Bei SUCCESS wird SessionToken in Response zurückgegeben

---

## LoginResponse (2)

**Richtung:** 📥 Server → Client  
**Frequenz:** Einmalig pro Login-Versuch  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung
Antwort auf LoginRequest. Enthält Erfolg/Fehler-Status, SessionToken bei Erfolg, und weitere Account-Informationen.

### Im Scope ✅
- Erfolgs-Status (Success/Failure)
- SessionToken bei Erfolg
- Account-ID und Basic-Info
- Error-Code bei Fehler

### Nicht im Scope ❌
- Character-Liste → verwende `CharacterListResponse` (13)
- Realm-Liste → verwende `RealmListResponse` (16)
- Detaillierte Account-Statistiken → verwende `AccountDataResponse` (18)

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Success | bool | Login erfolgreich? | Ja |
| ErrorCode | string | Fehlercode falls Success=false | Nein |
| ErrorMessage | string | Menschenlesbare Fehlermeldung | Nein |
| SessionToken | string | Token für Session (UUID) | Bei Erfolg |
| AccountId | long | Eindeutige Account-ID | Bei Erfolg |
| AccountName | string | Display-Name des Accounts | Bei Erfolg |
| IsPremium | bool | Hat Account Premium-Status? | Bei Erfolg |
| ServerTime | long | Unix Timestamp (Server-Zeit) | Ja |

### Erwartete Response
- Keine (ist selbst Response)

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `LoginRequest` | 1 | Request zu dieser Response |
| `CharacterListRequest` | 12 | Nächster Schritt nach erfolgreichem Login |
| `RealmListRequest` | 15 | Für Realm-Auswahl nach Login |

### Beispiel Payload
```csharp
// Erfolg
var successResponse = new LoginResponse
{
    Type = MessageType.LoginResponse,
    Success = true,
    SessionToken = "a3f7c2b1-4d5e-6f7a-8b9c-0d1e2f3a4b5c",
    AccountId = 12345678,
    AccountName = "PlayerOne",
    IsPremium = false,
    ServerTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
};

// Fehler
var errorResponse = new LoginResponse
{
    Type = MessageType.LoginResponse,
    Success = false,
    ErrorCode = "INVALID_CREDENTIALS",
    ErrorMessage = "Username or password incorrect",
    ServerTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
};
```

### Error Codes
Siehe [LoginRequest (1)](#loginrequest-1) Error Codes

### Notizen
- SessionToken ist ein UUID und 30 Tage gültig
- Bei Fehler: Client sollte Rate-Limiting respektieren
- ServerTime wird für Client-Zeit-Synchronisation verwendet

---

## LogoutRequest (3)

**Richtung:** 📤 Client → Server  
**Frequenz:** Einmalig (Session-Ende)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Client meldet sich ordnungsgemäß vom Server ab. Server speichert Character-State und beendet die Session graceful.

### Im Scope ✅
- Graceful Disconnect
- Character State Save
- Session Cleanup

### Nicht im Scope ❌
- Forced Disconnect → Server sendet `Disconnect` (5)
- Network Error Disconnect → wird durch Timeout gehandhabt

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Reason | string | Optional: Grund für Logout | Nein |

### Erwartete Response
- Server bestätigt implizit durch Connection-Close
- Keine explizite Response-Message

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `Disconnect` | 5 | Server-initiierter Disconnect |
| `LeaveZone` | 101 | Wird automatisch ausgelöst vor Logout |

### Flow-Diagramm
```
Client                    Server
  │                          │
  │  LogoutRequest (3)       │
  │─────────────────────────►│
  │                          │  Save State
  │                          │  Broadcast LeaveZone
  │                          │  Cleanup Session
  │                          │
  │  TCP FIN                 │
  │◄─────────────────────────│
  │  TCP FIN ACK             │
  │─────────────────────────►│
```

### Beispiel Payload
```csharp
var logoutRequest = new LogoutRequest
{
    Type = MessageType.LogoutRequest,
    Reason = "User quit"
};
```

### Notizen
- Server hat 5 Sekunden Zeit um State zu speichern
- Andere Spieler sehen `CharacterLeftZone` (105) Broadcast
- Party-Mitglieder sehen `PartyMemberUpdate` (710) mit Online=false
- Guild-Mitglieder sehen `GuildMemberUpdate` mit Online=false
- Freunde sehen `FriendOffline` (2108)
- SessionToken bleibt gültig für Reconnect

---

## Heartbeat (4)

**Richtung:** 📤 Client → Server  
**Frequenz:** ⚡ High-Frequency (alle 5 Sekunden)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Keep-Alive Message um Connection aktiv zu halten. Client sendet alle 5 Sekunden Heartbeat an Server. Server antwortet mit `Pong` (901) Message oder dediziertem `HeartbeatAck`. Misst auch Latency.

### Im Scope ✅
- Connection Keep-Alive (Client → Server)
- Latency Measurement
- Packet Loss Detection

### Nicht im Scope ❌
- Detaillierte Network-Statistiken → verwende `NetworkStats` (903)
- Quality-Reporting → verwende `ConnectionQuality` (904)

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Timestamp | long | Unix Timestamp (Millisekunden) | Ja |
| SequenceNumber | uint | Aufsteigende Nummer | Ja |

### Erwartete Response
- Server sendet `Pong` (901) zurück mit Timestamp und SequenceNumber
- Alternativ: Dediziertes `HeartbeatAck` (falls implementiert)

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `Pong` | 901 | Server-Response auf Heartbeat |
| `Ping` | 900 | Für manuelle Latency-Tests |
| `LatencyReport` | 902 | Aggregierte Latency-Statistiken |
| `ConnectionQuality` | 904 | Quality-of-Service Metrics |

### Beispiel Payload
```csharp
// Client → Server
var heartbeat = new Heartbeat
{
    Type = MessageType.Heartbeat,
    Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
    SequenceNumber = currentSequence++
};
```

### Notizen
- **HEARTBEAT_INTERVAL**: 5 Sekunden (Client sendet)
- **HEARTBEAT_TIMEOUT**: 15 Sekunden (3 fehlgeschlagene Heartbeats = Disconnect)
- RTT (Round-Trip-Time) = `Current Time - Pong.Timestamp` (in Millisekunden)
- Fehlende Sequence Numbers zeigen Packet Loss
- **Wichtig**: Dies ist KEINE bidirektionale Message mehr - Client sendet Heartbeat, Server antwortet mit Pong

---

## ForceDisconnect (5)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung
Server fordert Client auf, die Verbindung zu trennen. Kann verschiedene Gründe haben (Kick, Maintenance, Timeout, Ban).

### Im Scope ✅
- Forced Disconnect
- Disconnect-Grund (DisconnectReason Enum)
- Reconnect-Erlaubnis (computed property)

### Nicht im Scope ❌
- Graceful Logout → Client sendet `LogoutRequest` (3)

### Payload
| Feld           | Typ              | Beschreibung                     | Pflicht |
|----------------|------------------|----------------------------------|---------|
| Type           | MessageType      | `MessageType.ForceDisconnect`    | Ja      |
| Reason         | DisconnectReason | Disconnect-Grund (Enum)          | Ja      |
| Message        | string?          | Menschenlesbare Nachricht        | Nein    |
| ReconnectDelay | int?             | Sekunden bis Reconnect erlaubt   | Nein    |

### DisconnectReason Enum (aus Code)

```csharp
public enum DisconnectReason
{
    ClientDisconnected,  // Client closed connection gracefully
    Timeout,             // No heartbeat received
    NetworkError,        // Network error occurred
    ServerShutdown,      // Server is shutting down
    Kicked,              // Client was kicked by server
    ProtocolError,       // Protocol violation or invalid data
    Banned,              // Client was banned
    Maintenance,         // Server maintenance
    DuplicateLogin,      // Another session with same account
    VersionMismatch      // Client version incompatible
}
```

### Code-Beispiel (aktueller Code)

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.ForceDisconnect)]
public class ForceDisconnect : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.ForceDisconnect;
    [Key(1)] public DisconnectReason Reason { get; init; }
    [Key(2)] public string? Message { get; set; }
    [Key(3)] public int? ReconnectDelay { get; set; }
    
    // Computed property (not serialized)
    [IgnoreMember]
    public bool CanReconnect => Reason is not (DisconnectReason.Banned or DisconnectReason.VersionMismatch);
}
```

### Beispiel Payloads

```csharp
// Timeout
var timeoutDisconnect = new ForceDisconnect
{
    Reason = DisconnectReason.Timeout,
    Message = "Connection timed out due to inactivity",
    ReconnectDelay = 0
};

// Admin Kick
var kickDisconnect = new ForceDisconnect
{
    Reason = DisconnectReason.Kicked,
    Message = "You have been kicked by an administrator",
    ReconnectDelay = 300 // 5 Minuten
};

// Ban (CanReconnect = false)
var banDisconnect = new ForceDisconnect
{
    Reason = DisconnectReason.Banned,
    Message = "Your account has been permanently banned"
};
```

### Disconnect Reasons
| Reason             | CanReconnect | Beschreibung                        |
| ------------------ | ------------ | ----------------------------------- |
| `Timeout`          | true         | Keine Heartbeats empfangen          |
| `Kicked`           | true         | Admin-Kick                          |
| `Banned`           | false        | Account gesperrt                    |
| `VersionMismatch`  | false        | Client-Version inkompatibel         |
| `Maintenance`      | true         | Server-Wartung                      |
| `DuplicateLogin`   | true         | Anderer Login mit gleichem Account  |
| `MAINTENANCE`      | true         | Server-Wartung                      |
| `SERVER_SHUTDOWN`  | true         | Server fährt herunter               |
| `DUPLICATE_LOGIN`  | true         | Andere Session auf gleichem Account |
| `PROTOCOL_ERROR`   | true         | Ungültige Message empfangen         |
| `VERSION_MISMATCH` | false        | Client muss updaten                 |

### Notizen
- Server hat 5 Sekunden Zeit um State zu speichern
- Andere Spieler sehen `CharacterLeftZone` (105) Broadcast
- Party-Mitglieder sehen `PartyMemberUpdate` (710) mit Online=false
- Guild-Mitglieder sehen `GuildMemberUpdate` mit Online=false
- Freunde sehen `FriendOffline` (2108)
- Client sollte Reconnect-Delay respektieren
- Bei `CanReconnect=false`: SessionToken wird invalidiert
- Nach Disconnect: Client zeigt Message im UI

---

## ReconnectRequest (6)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** Nein (verwendet SessionToken)  
**Spezielle Rechte:** Keine

### Beschreibung
Wiederverbindung nach ungewolltem Disconnect. Client sendet SessionToken statt Username/Password.

### Im Scope ✅
- Wiederverbindung mit SessionToken
- State-Recovery (Position, HP, etc.)
- Reconnect innerhalb des Reconnect-Windows (30s)

### Nicht im Scope ❌
- Erstes Login → verwende `LoginRequest` (1)
- Reconnect nach Logout → verwende `LoginRequest` (1) mit Token

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| SessionToken | string | Token aus vorheriger Session | Ja |
| LastSequenceNumber | uint | Letzte empfangene Sequence Number | Ja |
| ClientVersion | string | Client Version | Ja |

### Erwartete Response
- **Bei Erfolg:** `ReconnectResponse` (7) mit Success=true
- **Bei Fehler:** `ReconnectResponse` (7) mit ErrorCode

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `ReconnectResponse` | 7 | Response zu diesem Request |
| `LoginRequest` | 1 | Alternative für neuen Login |
| `ZoneState` | 102 | Wird nach Reconnect gesendet |

### Beispiel Payload
```csharp
var reconnectRequest = new ReconnectRequest
{
    Type = MessageType.ReconnectRequest,
    SessionToken = storedSessionToken,
    LastSequenceNumber = lastReceivedSeqNum,
    ClientVersion = "0.1.0"
};
```

### Error Codes
| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `SESSION_EXPIRED` | Token abgelaufen | Neuer Login nötig |
| `SESSION_NOT_FOUND` | Token ungültig | Neuer Login nötig |
| `RECONNECT_WINDOW_CLOSED` | >30s seit Disconnect | Neuer Login nötig |
| `DIFFERENT_CLIENT` | HardwareId stimmt nicht | Neuer Login nötig (Security) |

### Notizen
- **RECONNECT_WINDOW**: 30 Sekunden
- Während Window: Server hält Character-State
- Nach Erfolg: Resend aller Messages seit LastSequenceNumber
- Exponential Backoff: 1s, 2s, 4s, 8s (max 10 Versuche)

---

## ReconnectResponse (7)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung
Antwort auf ReconnectRequest. Bestätigt erfolgreiche Wiederverbindung oder gibt Fehler zurück.

### Im Scope ✅
- Erfolgs-Status
- Wiederhergestellter State (Zone, Position)
- Resync-Informationen

### Nicht im Scope ❌
- Vollständiger ZoneState → wird in separater `ZoneState` (102) Message gesendet

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Success | bool | Reconnect erfolgreich? | Ja |
| ErrorCode | string | Fehlercode falls Failed | Nein |
| ErrorMessage | string | Menschenlesbare Message | Nein |
| ZoneId | ushort | Aktuelle Zone-ID | Bei Erfolg |
| LastServerSequence | uint | Letzte Server Sequence Number | Bei Erfolg |
| ResyncRequired | bool | Muss Client vollständig resynced werden? | Bei Erfolg |

### Beispiel Payload
```csharp
// Erfolg
var successResponse = new ReconnectResponse
{
    Type = MessageType.ReconnectResponse,
    Success = true,
    ZoneId = 1001,
    LastServerSequence = 54321,
    ResyncRequired = false
};

// Fehler
var errorResponse = new ReconnectResponse
{
    Type = MessageType.ReconnectResponse,
    Success = false,
    ErrorCode = "SESSION_EXPIRED",
    ErrorMessage = "Your session has expired. Please log in again."
};
```

### Notizen
- Bei `ResyncRequired=true`: Server sendet kompletten `ZoneState` (102)
- Bei `ResyncRequired=false`: Server sendet nur Delta-Updates seit LastSequenceNumber

---

## SessionValidate (8) - ⚠️ DEPRECATED

**Richtung:** 🔀 Server ↔ Server (Internal)  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja (Server-to-Server)  
**Spezielle Rechte:** 👑 Server  
**Status:** ⚠️ **DEPRECATED** - Ersetzt durch `S2S_SessionValidate (5000)`

### Deprecation Notice

Diese Message ist **veraltet** und wird durch das neue **Server-zu-Server (S2S) Message-System** ersetzt.

**Migration:**
- **Alt:** `SessionValidate (8)` im Connection-Range (0-99)
- **Neu:** `S2S_SessionValidate (5000)` im S2S-Range (5000-5999)
- **Siehe:** [SERVER_TO_SERVER.md](../Architecture/SERVER_TO_SERVER.md#51-s2s_sessionvalidate-5000)

### Beschreibung
Interne Message zwischen Gateway Server und Zone Server zur Validierung von SessionTokens. Client sendet diese Message NICHT.

**Wichtig**: Dies ist eine Server-interne Message und verwendet NICHT die Client/Server Interface-Hierarchie. Wird über Redis Pub/Sub ausgetauscht.

### Im Scope ✅
- Token-Validierung zwischen Servern
- Session-Informationen Transfer

### Nicht im Scope ❌
- Client-Server Kommunikation

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| SessionToken | string | Zu validierendes Token | Ja |
| RequestingServer | string | Server-ID des anfragenden Servers | Ja |

### Notizen
- **NUR FÜR SERVER-TO-SERVER KOMMUNIKATION**
- Client sollte diese Message niemals senden oder empfangen
- Implementiert über Redis Pub/Sub
- **Für neue Implementierungen verwende `S2S_SessionValidate (5000)`**
- **Backward-Compatibility**: Wird noch unterstützt, aber neue Systeme sollten S2S-Messages verwenden

---

## CharacterSelectRequest (9)

**Richtung:** 📤 Client → Server  
**Frequenz:** Einmalig pro Session  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Client wählt einen Character aus der Character-Liste. Server lädt Character-Daten und spawnt in Start-Zone.

### Im Scope ✅
- Character-Auswahl
- Character-Daten laden
- Spawn in letzte Zone oder Start-Zone

### Nicht im Scope ❌
- Character erstellen → verwende `CharacterCreateRequest` (10)
- Character löschen → verwende `CharacterDeleteRequest` (11)

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| CharacterId | long | ID des zu wählenden Characters | Ja |

### Erwartete Response
- `CharacterSelectResponse` (21)

### Folge-Messages bei Erfolg
- `JoinZone` (100) für Character-Spawn in Zone
- `ZoneState` (102) für vollständige Zone-Informationen

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `CharacterSelectResponse` | 21 | Response zu diesem Request |
| `CharacterListResponse` | 13 | Zeigt verfügbare Characters |
| `JoinZone` | 100 | Folgt nach erfolgreicher Auswahl |
| `CharacterCreateRequest` | 10 | Character erstellen |

### Beispiel Payload
```csharp
var selectRequest = new CharacterSelectRequest
{
    Type = MessageType.CharacterSelectRequest,
    CharacterId = 98765
};
```

### Error Codes
| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `CHARACTER_NOT_FOUND` | Character-ID existiert nicht | Character-Liste neu laden |
| `CHARACTER_NOT_OWNED` | Character gehört nicht dem Account | Security-Log |
| `CHARACTER_IN_USE` | Character bereits eingeloggt | Warten oder erzwingen |
| `CHARACTER_DELETED` | Character wurde gelöscht | Character-Liste neu laden |

### Notizen
- Server lädt Character aus DB (kann 1-2 Sekunden dauern)
- Loading-Screen im Client während Ladezeit
- Nach Erfolg: Client empfängt `ZoneState` (102) Message

---

## CharacterSelectResponse (21)

**Richtung:** 📥 Server → Client  
**Frequenz:** Einmalig pro Character-Auswahl  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung
Antwort auf CharacterSelect Request. Bestätigt erfolgreiche Character-Auswahl oder gibt Fehler zurück.

### Im Scope ✅
- Erfolgs-Status (Success/Failure)
- Character-ID und Spawn-Zone bei Erfolg
- Error-Code bei Fehler

### Nicht im Scope ❌
- Vollständiger Character-State → wird in `CharacterInfo` (600) gesendet
- Zone-Daten → verwende `ZoneState` (102)

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Success | bool | Auswahl erfolgreich? | Ja |
| ErrorCode | string | Fehlercode falls Success=false | Nein |
| ErrorMessage | string | Menschenlesbare Fehlermeldung | Nein |
| CharacterId | long | ID des gewählten Characters | Bei Erfolg |
| SpawnZoneId | ushort | Zone in der gespawnt wird | Bei Erfolg |

### Erwartete Response
- Keine (ist selbst Response)

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `CharacterSelect` | 9 | Request zu dieser Response |
| `JoinZone` | 100 | Folgt nach erfolgreicher Response |
| `ZoneState` | 102 | Enthält Zone-Informationen |

### Beispiel Payload
```csharp
// Erfolg
var successResponse = new CharacterSelectResponse
{
    Type = MessageType.CharacterSelectResponse,
    Success = true,
    CharacterId = 98765,
    SpawnZoneId = 1001
};

// Fehler
var errorResponse = new CharacterSelectResponse
{
    Type = MessageType.CharacterSelectResponse,
    Success = false,
    ErrorCode = "CHARACTER_NOT_FOUND",
    ErrorMessage = "Character does not exist or has been deleted"
};
```

### Error Codes
| Code | Bedeutung |
|------|-----------|
| `CHARACTER_NOT_FOUND` | Character-ID existiert nicht |
| `CHARACTER_NOT_OWNED` | Character gehört nicht dem Account |
| `CHARACTER_IN_USE` | Character bereits eingeloggt |
| `CHARACTER_DELETED` | Character wurde gelöscht |

### Notizen
- Nach erfolgreicher Response folgt `JoinZone` (100) Message
- Dann folgt `ZoneState` (102) mit vollständigen Zone-Informationen
- Loading-Screen wird zwischen Response und JoinZone angezeigt

---

## CharacterCreateRequest (10)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Erstellt einen neuen Character für den Account. Server validiert Namen, Rasse, Klasse und speichert in DB.

### Im Scope ✅
- Character-Name (Validierung)
- Rasse und Klasse Auswahl
- Aussehen (Appearance) Daten
- Prüfung auf max. Characters pro Account

### Nicht im Scope ❌
- Account-Registrierung → externe API
- Premium-Features → geplant

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Name | string | Character-Name (3-16 Zeichen, alphanumerisch) | Ja |
| Race | int | Rassen-ID | Ja |
| Class | int | Klassen-ID | Ja |
| AppearanceData | byte[] | Serialisierte Appearance-Daten | Ja |

### Erwartete Response
- `CharacterCreateResponse` (22)

### Folge-Messages bei Erfolg
- `CharacterListResponse` (13) mit aktualisierter Character-Liste

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `CharacterCreateResponse` | 22 | Response zu diesem Request |
| `CharacterListResponse` | 13 | Zeigt erstellten Character |
| `CharacterDeleteRequest` | 11 | Character löschen |

### Beispiel Payload
```csharp
var createRequest = new CharacterCreateRequest
{
    Type = MessageType.CharacterCreateRequest,
    Name = "Aragorn",
    Race = 1, // Human
    Class = 2, // Warrior
    AppearanceData = SerializeAppearance(new Appearance
    {
        HairStyle = 3,
        HairColor = 0x8B4513, // Brown
        SkinColor = 0xFFDBBE,
        FaceType = 1
    })
};
```

### Error Codes
| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `NAME_TAKEN` | Name bereits vergeben | Anderen Namen wählen |
| `NAME_INVALID` | Name verstößt gegen Regeln | Gültigen Namen wählen |
| `MAX_CHARACTERS_REACHED` | Max. Characters erreicht (Standard: 5) | Character löschen oder Premium |
| `INVALID_RACE` | Ungültige Rassen-ID | Gültige Rasse wählen |
| `INVALID_CLASS` | Ungültige Klassen-ID | Gültige Klasse wählen |
| `INVALID_RACE_CLASS_COMBO` | Kombination nicht erlaubt | Andere Kombination wählen |

### Notizen
- Namen sind **Server-weit unique**
- Namens-Regeln: 3-16 Zeichen, alphanumerisch, keine Sonderzeichen
- Profanity-Filter wird angewendet
- Max. Characters pro Account: 5 (Free), 10 (Premium)
- Character-Erstellung kann 1-2 Sekunden dauern (DB-Write)

---

## CharacterCreateResponse (22)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung
Antwort auf CharacterCreate Request. Bestätigt erfolgreiche Character-Erstellung oder gibt Fehler zurück.

### Im Scope ✅
- Erfolgs-Status (Success/Failure)
- Neue Character-ID bei Erfolg
- Error-Code bei Fehler

### Nicht im Scope ❌
- Vollständige Character-Liste → Server sendet separate `CharacterListResponse` (13)

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Success | bool | Erstellung erfolgreich? | Ja |
| ErrorCode | string | Fehlercode falls Success=false | Nein |
| ErrorMessage | string | Menschenlesbare Fehlermeldung | Nein |
| CharacterId | long | ID des neuen Characters | Bei Erfolg |
| Name | string | Character-Name | Bei Erfolg |

### Erwartete Response
- Keine (ist selbst Response)

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `CharacterCreate` | 10 | Request zu dieser Response |
| `CharacterListResponse` | 13 | Folgt mit aktualisierter Liste |

### Beispiel Payload
```csharp
// Erfolg
var successResponse = new CharacterCreateResponse
{
    Type = MessageType.CharacterCreateResponse,
    Success = true,
    CharacterId = 98765,
    Name = "Aragorn"
};

// Fehler
var errorResponse = new CharacterCreateResponse
{
    Type = MessageType.CharacterCreateResponse,
    Success = false,
    ErrorCode = "NAME_TAKEN",
    ErrorMessage = "Character name is already in use"
};
```

### Error Codes
| Code | Bedeutung |
|------|-----------|
| `NAME_TAKEN` | Name bereits vergeben |
| `NAME_INVALID` | Name verstößt gegen Regeln |
| `MAX_CHARACTERS_REACHED` | Max. Characters erreicht (Standard: 5) |
| `INVALID_RACE` | Ungültige Rassen-ID |
| `INVALID_CLASS` | Ungültige Klassen-ID |
| `INVALID_RACE_CLASS_COMBO` | Kombination nicht erlaubt |

### Notizen
- Nach erfolgreicher Response sendet Server `CharacterListResponse` (13) mit aktualisierter Liste
- Character-Erstellung kann 1-2 Sekunden dauern (DB-Write)

---

## CharacterDeleteRequest (11)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Löscht einen Character permanent. Sicherheits-Mechanismus: Character wird erst nach 24h Cooldown wirklich gelöscht (Soft-Delete).

### Im Scope ✅
- Character zum Löschen markieren
- Soft-Delete (24h Cooldown)
- Bestätigung erforderlich

### Nicht im Scope ❌
- Sofortiges Löschen (Security-Risiko)
- Character-Transfer → geplant

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| CharacterId | long | ID des zu löschenden Characters | Ja |
| Confirmation | string | Muss "DELETE" sein | Ja |

### Erwartete Response
- `CharacterDeleteResponse` (23)

### Folge-Messages bei Erfolg
- `CharacterListResponse` (13) mit aktualisierter Character-Liste

### Beispiel Payload
```csharp
var deleteRequest = new CharacterDeleteRequest
{
    Type = MessageType.CharacterDeleteRequest,
    CharacterId = 98765,
    Confirmation = "DELETE"
};
```

### Error Codes
| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `CHARACTER_NOT_FOUND` | Character existiert nicht | Character-Liste neu laden |
| `CHARACTER_NOT_OWNED` | Character gehört nicht dem Account | Security-Log |
| `INVALID_CONFIRMATION` | Confirmation fehlt oder falsch | Erneut mit "DELETE" |
| `CHARACTER_IN_GUILD` | Character ist Guild-Leader | Guild übergeben oder verlassen |

### Notizen
- **Soft-Delete**: Character wird 24h zum Löschen markiert
- Während 24h: Character kann **nicht** gespielt werden
- Abbruch möglich: Löschung kann rückgängig gemacht werden (Support-Request)
- Nach 24h: Hard-Delete aus DB
- Guild-Leader können NICHT gelöscht werden (müssen Guild übergeben)

---

## CharacterDeleteResponse (23)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung
Antwort auf CharacterDelete Request. Bestätigt erfolgreiche Markierung zum Löschen oder gibt Fehler zurück.

### Im Scope ✅
- Erfolgs-Status (Success/Failure)
- Lösch-Zeitpunkt bei Erfolg
- Error-Code bei Fehler

### Nicht im Scope ❌
- Vollständige Character-Liste → Server sendet separate `CharacterListResponse` (13)

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Success | bool | Markierung erfolgreich? | Ja |
| ErrorCode | string | Fehlercode falls Success=false | Nein |
| ErrorMessage | string | Menschenlesbare Fehlermeldung | Nein |
| CharacterId | long | ID des markierten Characters | Bei Erfolg |
| DeletionTime | long | Unix Timestamp wann gelöscht wird (24h) | Bei Erfolg |

### Erwartete Response
- Keine (ist selbst Response)

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `CharacterDelete` | 11 | Request zu dieser Response |
| `CharacterListResponse` | 13 | Folgt mit aktualisierter Liste |

### Beispiel Payload
```csharp
// Erfolg
var successResponse = new CharacterDeleteResponse
{
    Type = MessageType.CharacterDeleteResponse,
    Success = true,
    CharacterId = 98765,
    DeletionTime = DateTimeOffset.UtcNow.AddHours(24).ToUnixTimeSeconds()
};

// Fehler
var errorResponse = new CharacterDeleteResponse
{
    Type = MessageType.CharacterDeleteResponse,
    Success = false,
    ErrorCode = "CHARACTER_IN_GUILD",
    ErrorMessage = "Character is guild leader. Transfer guild ownership first."
};
```

### Error Codes
| Code | Bedeutung |
|------|-----------|
| `CHARACTER_NOT_FOUND` | Character existiert nicht |
| `CHARACTER_NOT_OWNED` | Character gehört nicht dem Account |
| `INVALID_CONFIRMATION` | Confirmation fehlt oder falsch |
| `CHARACTER_IN_GUILD` | Character ist Guild-Leader |

### Notizen
- Nach erfolgreicher Response sendet Server `CharacterListResponse` (13) mit markiertem Character
- Character kann 24h lang nicht gespielt werden
- Nach 24h wird Character permanent gelöscht (Hard-Delete)

---

## CharacterListRequest (12)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Fordert Liste aller Characters des Accounts an. Wird nach erfolgreichem Login gesendet.

### Im Scope ✅
- Liste aller Characters des Accounts
- Basic-Informationen (Name, Level, Klasse, etc.)

### Nicht im Scope ❌
- Detaillierte Character-Statistiken → verwende `CharacterInfo` (606)

### Request Payload
Keine zusätzlichen Felder (nur MessageType)

### Erwartete Response
- **Immer:** `CharacterListResponse` (13)

### Beispiel Payload
```csharp
var listRequest = new CharacterListRequest
{
    Type = MessageType.CharacterListRequest
};
```

### Notizen
- Wird automatisch nach `LoginResponse` (2) gesendet
- Liste enthält auch zum Löschen markierte Characters (mit Flag)

---

## CharacterListResponse (13)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung
Liste aller Characters des Accounts mit Basic-Informationen für Character-Select Screen.

### Im Scope ✅
- Character-ID, Name, Level
- Rasse, Klasse
- Last-Played Timestamp
- Lösch-Status (falls markiert)

### Nicht im Scope ❌
- Vollständige Stats → verwende `CharacterInfo` (606)
- Equipment → verwende `InspectEquipment` (3302)

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Characters | List<CharacterInfo> | Liste der Characters | Ja |

**CharacterInfo**:

| Feld | Typ | Beschreibung |
|------|-----|--------------|
| CharacterId | long | Eindeutige Character-ID |
| Name | string | Character-Name |
| Race | int | Rassen-ID |
| Class | int | Klassen-ID |
| Level | int | Aktuelles Level |
| ZoneId | ushort | Letzte Zone |
| LastPlayed | long | Unix Timestamp |
| PendingDeletion | bool | Zum Löschen markiert? |
| DeletionTime | long | Wann wird gelöscht (Unix Timestamp) |

### Beispiel Payload
```csharp
var listResponse = new CharacterListResponse
{
    Type = MessageType.CharacterListResponse,
    Characters = new List<CharacterInfo>
    {
        new CharacterInfo
        {
            CharacterId = 98765,
            Name = "Aragorn",
            Race = 1, // Human
            Class = 2, // Warrior
            Level = 10,
            ZoneId = 1001,
            LastPlayed = DateTimeOffset.UtcNow.AddDays(-2).ToUnixTimeSeconds(),
            PendingDeletion = false
        },
        new CharacterInfo
        {
            CharacterId = 98766,
            Name = "Legolas",
            Race = 2, // Elf
            Class = 3, // Ranger
            Level = 8,
            ZoneId = 1002,
            LastPlayed = DateTimeOffset.UtcNow.AddDays(-7).ToUnixTimeSeconds(),
            PendingDeletion = true,
            DeletionTime = DateTimeOffset.UtcNow.AddHours(12).ToUnixTimeSeconds()
        }
    }
};
```

### Notizen
- Liste ist nach `LastPlayed` sortiert (neueste zuerst)
- Max. 5 Characters (Free) oder 10 (Premium)

---

## ServerSelectRequest (14)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Wählt einen Game-Server (Realm) aus. Nur relevant wenn mehrere Realms existieren.

### Im Scope ✅
- Realm-Auswahl
- Transfer zu anderem Realm

### Nicht im Scope ❌
- Realm-Erstellung (Admin-Funktion)

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| RealmId | int | ID des Realms | Ja |

### Erwartete Response
- `ServerSelectResponse` (24)

### Folge-Messages bei Erfolg
- Connection wird zu gewähltem Realm transferiert
- Nach Transfer: Neuer Login-Flow auf dem Ziel-Realm

### Notizen
- Im Prototyp: Nur 1 Realm → Message wird nicht aktiv verwendet
- Phase 2: Multi-Realm Support

---

## ServerSelectResponse (24)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung
Antwort auf ServerSelect Request. Bestätigt Realm-Auswahl oder gibt Fehler zurück.

### Im Scope ✅
- Erfolgs-Status (Success/Failure)
- Realm-Informationen bei Erfolg
- Error-Code bei Fehler

### Nicht im Scope ❌
- Realm-Liste → verwende `RealmListResponse` (16)

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Success | bool | Realm-Auswahl erfolgreich? | Ja |
| ErrorCode | string | Fehlercode falls Success=false | Nein |
| ErrorMessage | string | Menschenlesbare Fehlermeldung | Nein |
| RealmId | int | ID des gewählten Realms | Bei Erfolg |
| RealmName | string | Name des Realms | Bei Erfolg |
| TransferToken | string | Token für Realm-Transfer | Bei Erfolg |

### Erwartete Response
- Keine (ist selbst Response)

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `ServerSelect` | 14 | Request zu dieser Response |
| `RealmListResponse` | 16 | Liste verfügbarer Realms |

### Beispiel Payload
```csharp
// Erfolg
var successResponse = new ServerSelectResponse
{
    Type = MessageType.ServerSelectResponse,
    Success = true,
    RealmId = 1,
    RealmName = "Azeroth-EU",
    TransferToken = "a3f7c2b1-4d5e-6f7a-8b9c-0d1e2f3a4b5c"
};

// Fehler
var errorResponse = new ServerSelectResponse
{
    Type = MessageType.ServerSelectResponse,
    Success = false,
    ErrorCode = "REALM_FULL",
    ErrorMessage = "Realm is currently full. Please try again later."
};
```

### Error Codes
| Code | Bedeutung |
|------|-----------|
| `REALM_NOT_FOUND` | Realm existiert nicht |
| `REALM_OFFLINE` | Realm ist offline |
| `REALM_FULL` | Realm ist voll |
| `REALM_LOCKED` | Realm ist gesperrt (Maintenance) |

### Notizen
- Im Prototyp: Nur 1 Realm → Message wird nicht aktiv verwendet
- Phase 2: Multi-Realm Support mit Connection-Transfer
- Bei Erfolg: Client verbindet sich zum neuen Realm mit TransferToken

---

## RealmListRequest (15)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Fordert Liste aller verfügbaren Realms an.

### Request Payload
Keine zusätzlichen Felder

### Erwartete Response
- **Immer:** `RealmListResponse` (16)

### Notizen
- Im Prototyp: Nur 1 Realm

---

## RealmListResponse (16)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung
Liste aller verfügbaren Realms mit Status-Informationen.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Realms | List<RealmInfo> | Liste der Realms | Ja |

**RealmInfo**:
| Feld | Typ | Beschreibung |
|------|-----|--------------|
| RealmId | int | Eindeutige Realm-ID |
| Name | string | Realm-Name |
| Type | string | PvP, PvE, RP, etc. |
| Population | int | Spielerzahl (Low/Medium/High/Full) |
| Online | bool | Realm online? |

### Notizen
- Im Prototyp: Nur 1 Realm in Liste

---

## AccountDataRequest (17)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Fordert detaillierte Account-Daten an (nicht Character-spezifisch).

### Im Scope ✅
- Premium-Status
- Account-Alter
- Spielzeit (gesamt)

### Nicht im Scope ❌
- Character-Daten → verwende `CharacterInfo` (606)

### Request Payload
Keine zusätzlichen Felder

### Erwartete Response
- **Immer:** `AccountDataResponse` (18)

---

## AccountDataResponse (18)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung
Detaillierte Account-Informationen.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| AccountId | long | Account-ID | Ja |
| Email | string | E-Mail (teilweise maskiert) | Ja |
| IsPremium | bool | Premium-Status | Ja |
| PremiumUntil | long | Unix Timestamp (Premium-Ende) | Nein |
| CreatedAt | long | Account-Erstellung (Unix Timestamp) | Ja |
| TotalPlaytime | long | Gesamte Spielzeit (Sekunden) | Ja |

---

## EncryptionHandshake (19)

**Richtung:** 📤 Client → Server  
**Frequenz:** Einmalig (vor Login)  
**Authentifizierung:** Nein (ist Teil des Verbindungsaufbaus)  
**Spezielle Rechte:** Keine  
**Phase:** Phase 3

### Beschreibung

TLS-ähnlicher Encryption Handshake für sichere Verbindung.  Client initiiert den Handshake als erste Message nach TCP-Connect.  Server antwortet mit `EncryptionHandshakeResponse (25)`. Nach erfolgreichem Handshake ist die gesamte Kommunikation verschlüsselt.

### Im Scope ✅

- ECDH Key Exchange initiieren
- Cipher Suite Negotiation
- Client Random für Key Derivation

### Nicht im Scope ❌

- Plaintext-Fallback → immer Encryption oder Disconnect
- Certificate Pinning → Standard CA-Validation
- Re-Keying während Session → geplant für Phase 4

### Request Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| ClientPublicKey | byte[] | Client's ECDH Public Key (32 Bytes) | Ja |
| SupportedCipherSuites | ushort[] | Liste unterstützter Cipher Suites | Ja |
| ClientRandom | byte[32] | 32-Byte Random für Key Derivation | Ja |
| ProtocolVersion | ushort | Encryption Protocol Version | Ja |

### Unterstützte Cipher Suites

| ID | Name | Beschreibung |
|----|------|--------------|
| 0x0001 | `ECDHE_AES128_GCM_SHA256` | ECDH + AES-128-GCM (empfohlen) |
| 0x0002 | `ECDHE_AES256_GCM_SHA384` | ECDH + AES-256-GCM |
| 0x0003 | `ECDHE_CHACHA20_POLY1305` | ECDH + ChaCha20-Poly1305 |

### Erwartete Response

- **Bei Erfolg:** `EncryptionHandshakeResponse` (25) mit Success=true
- **Bei Fehler:** `EncryptionHandshakeResponse` (25) mit ErrorCode, danach Disconnect

### Verwandte Messages

| Message | ID | Beziehung |
|---------|-----|-----------|
| `EncryptionHandshakeResponse` | 25 | Response zu diesem Request |
| `LoginRequest` | 1 | Darf erst NACH erfolgreichem Handshake gesendet werden |
| `CompressionToggle` | 20 | Kann nach Encryption aktiviert werden |

### Flow-Diagramm

```
Client                              Server
  │                                    │
  │  TCP Connect                       │
  │═══════════════════════════════════►│
  │                                    │
  │  EncryptionHandshake (19)          │
  │  [ClientPubKey, CipherSuites]      │
  │───────────────────────────────────►│
  │                                    │  Validate
  │                                    │  Generate ServerKeyPair
  │                                    │  Select CipherSuite
  │  EncryptionHandshakeResponse (25)  │
  │  [ServerPubKey, SelectedSuite]     │
  │◄───────────────────────────────────│
  │                                    │
  │  ══════ ENCRYPTED FROM HERE ══════ │
  │                                    │
  │  LoginRequest (1)                  │
  │───────────────────────────────────►│
```

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.EncryptionHandshake)]
public class EncryptionHandshake :  IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.EncryptionHandshake;
    [Key(1)] public byte[] ClientPublicKey { get; set; } = Array.Empty<byte>();
    [Key(2)] public ushort[] SupportedCipherSuites { get; set; } = Array.Empty<ushort>();
    [Key(3)] public byte[] ClientRandom { get; set; } = new byte[32];
    [Key(4)] public ushort ProtocolVersion { get; set; } = 1;
}
```

### Beispiel Payload

```csharp
// Client generiert ECDH Keypair
using var ecdh = ECDiffieHellman. Create(ECCurve. NamedCurves. nistP256);
var clientPublicKey = ecdh. PublicKey. ExportSubjectPublicKeyInfo();

var handshake = new EncryptionHandshake
{
    ClientPublicKey = clientPublicKey,
    SupportedCipherSuites = new ushort[] { 0x0001, 0x0002, 0x0003 },
    ClientRandom = RandomNumberGenerator.GetBytes(32),
    ProtocolVersion = 1
};
```

### Error Codes

| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `UNSUPPORTED_CIPHER` | Keine gemeinsame Cipher Suite | Client updaten |
| `INVALID_PUBLIC_KEY` | Public Key ungültig | Erneut versuchen |
| `PROTOCOL_VERSION_MISMATCH` | Encryption-Version inkompatibel | Client updaten |
| `HANDSHAKE_TIMEOUT` | Handshake dauerte > 5 Sekunden | Erneut verbinden |

### Notizen

- **MUSS** erste Message nach TCP-Connect sein
- **Timeout:** 5 Sekunden für kompletten Handshake
- **Bei Fehler:** Server schließt Connection sofort
- **Nach Erfolg:** Alle weiteren Messages sind mit SharedSecret verschlüsselt
- **Key Derivation:** HKDF-SHA256 aus SharedSecret + ClientRandom + ServerRandom
- **Im Prototyp:** Noch nicht implementiert (Phase 3)

---

## EncryptionHandshakeResponse (25)

**Richtung:** 📥 Server → Client  
**Frequenz:** Einmalig (nach EncryptionHandshake)  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine  
**Phase:** Phase 3

### Beschreibung

Server-Antwort auf den Encryption Handshake.  Enthält Server's Public Key und die gewählte Cipher Suite.  Nach dieser Message ist die Verbindung verschlüsselt.

### Im Scope ✅

- Server ECDH Public Key
- Gewählte Cipher Suite (aus Client-Liste)
- Server Random für Key Derivation
- Erfolgs-/Fehler-Status

### Nicht im Scope ❌

- Certificate Chain → TLS auf Socket-Ebene
- Session Resumption → geplant

### Response Payload

| Feld                | Typ       | Beschreibung                        | Pflicht    |
| ------------------- | --------- | ----------------------------------- | ---------- |
| Success             | bool      | Handshake erfolgreich?              | Ja         |
| ErrorCode           | string?   | Fehlercode bei Failure              | Nein       |
| ErrorMessage        | string?   | Menschenlesbare Fehlermeldung       | Nein       |
| ServerPublicKey     | byte[]?   | Server's ECDH Public Key (32 Bytes) | Bei Erfolg |
| SelectedCipherSuite | ushort    | Gewählte Cipher Suite               | Bei Erfolg |
| ServerRandom        | byte[32]? | 32-Byte Random für Key Derivation   | Bei Erfolg |
| ProtocolVersion     | ushort    | Verwendete Protocol Version         | Bei Erfolg |

### Verwandte Messages

| Message | ID | Beziehung |
|---------|-----|-----------|
| `EncryptionHandshake` | 19 | Request zu dieser Response |
| `LoginRequest` | 1 | Nächste Message nach erfolgreichem Handshake |

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.EncryptionHandshakeResponse)]
public class EncryptionHandshakeResponse : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.EncryptionHandshakeResponse;
    [Key(1)] public bool Success { get; set; }
    [Key(2)] public string?  ErrorCode { get; set; }
    [Key(3)] public string? ErrorMessage { get; set; }
    [Key(4)] public byte[]? ServerPublicKey { get; set; }
    [Key(5)] public ushort SelectedCipherSuite { get; set; }
    [Key(6)] public byte[]? ServerRandom { get; set; }
    [Key(7)] public ushort ProtocolVersion { get; set; }
}
```

### Beispiel Payloads

```csharp
// Erfolg
var successResponse = new EncryptionHandshakeResponse
{
    Success = true,
    ServerPublicKey = serverEcdh.PublicKey. ExportSubjectPublicKeyInfo(),
    SelectedCipherSuite = 0x0001, // ECDHE_AES128_GCM_SHA256
    ServerRandom = RandomNumberGenerator.GetBytes(32),
    ProtocolVersion = 1
};

// Fehler
var errorResponse = new EncryptionHandshakeResponse
{
    Success = false,
    ErrorCode = "UNSUPPORTED_CIPHER",
    ErrorMessage = "No common cipher suite found.  Please update your client."
};
```

### Client-Verhalten nach Response

```csharp
public void OnEncryptionHandshakeResponse(EncryptionHandshakeResponse response)
{
    if (! response.Success)
    {
        ShowError(response.ErrorMessage);
        Disconnect();
        return;
    }
    
    // Derive shared secret
    var serverPublicKey = ECDiffieHellman.Create();
    serverPublicKey.ImportSubjectPublicKeyInfo(response.ServerPublicKey, out _);
    
    byte[] sharedSecret = _clientEcdh.DeriveKeyMaterial(serverPublicKey.PublicKey);
    
    // Derive encryption keys using HKDF
    byte[] keyMaterial = HKDF.DeriveKey(
        HashAlgorithmName.SHA256,
        sharedSecret,
        64, // 32 bytes client key + 32 bytes server key
        salt: _clientRandom. Concat(response.ServerRandom).ToArray(),
        info:  Encoding.UTF8.GetBytes("2DMMO-Encryption-v1")
    );
    
    // Enable encryption
    _connection.EnableEncryption(
        clientKey: keyMaterial[.. 32],
        serverKey: keyMaterial[32..],
        cipherSuite: response.SelectedCipherSuite
    );
    
    // Now safe to send LoginRequest
    SendLoginRequest();
}
```

### Notizen

- **Nach Success:** Nächste Message (LoginRequest) ist bereits verschlüsselt
- **Bei Fehler:** Connection wird vom Server geschlossen
- **Key Derivation:** Beide Seiten berechnen identisches SharedSecret
- **Cipher Selection:** Server wählt stärkste gemeinsame Suite

---

## CompressionToggle (20)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten (typisch 1x nach Login)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine  
**Phase:** Phase 3

### Beschreibung

Aktiviert oder deaktiviert Message-Kompression für die Verbindung.  Kann vom Client angefordert werden, Server bestätigt mit `CompressionToggleResponse (26)`. Kompression reduziert Bandbreite, erhöht aber CPU-Last.

### Im Scope ✅

- Kompression aktivieren/deaktivieren
- Kompression-Level wählen (0-9)
- Algorithmus wählen (LZ4, Zstd)

### Nicht im Scope ❌

- Per-Message Kompression → Global Toggle für alle Messages
- Asymmetrische Kompression → Beide Richtungen nutzen gleiche Einstellungen
- Selective Compression → Alle oder keine Messages

### Request Payload

| Feld           | Typ                  | Beschreibung                               | Pflicht |
| -------------- | -------------------- | ------------------------------------------ | ------- |
| Enabled        | bool                 | Kompression aktivieren?                    | Ja      |
| Level          | byte                 | Kompression-Level (0-9, 0=fastest, 9=best) | Ja      |
| Algorithm      | CompressionAlgorithm | LZ4 oder Zstd                              | Ja      |
| MinMessageSize | ushort               | Mindestgröße für Kompression (Bytes)       | Nein    |

### CompressionAlgorithm Enum

```csharp
public enum CompressionAlgorithm :  byte
{
    None = 0,    // Keine Kompression
    LZ4 = 1,     // Schnell, moderate Kompression (empfohlen für Games)
    Zstd = 2     // Langsamer, bessere Kompression
}
```

### Erwartete Response

- **Immer:** `CompressionToggleResponse` (26)
- **Ab nächster Message:** Kompression aktiv/inaktiv

### Verwandte Messages

| Message | ID | Beziehung |
|---------|-----|-----------|
| `CompressionToggleResponse` | 26 | Response zu diesem Request |
| `EncryptionHandshake` | 19 | Sollte VOR Kompression erfolgen |
| `MessageBundle` | 950 | Profitiert stark von Kompression |

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.CompressionToggle)]
public class CompressionToggle : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.CompressionToggle;
    [Key(1)] public bool Enabled { get; set; }
    [Key(2)] public byte Level { get; set; } = 1; // Fast
    [Key(3)] public CompressionAlgorithm Algorithm { get; set; } = CompressionAlgorithm.LZ4;
    [Key(4)] public ushort MinMessageSize { get; set; } = 64; // Nur Messages > 64 Bytes komprimieren
}
```

### Beispiel Payloads

```csharp
// Kompression aktivieren (empfohlene Settings)
var enableCompression = new CompressionToggle
{
    Enabled = true,
    Level = 1, // LZ4 fast
    Algorithm = CompressionAlgorithm.LZ4,
    MinMessageSize = 64
};

// Kompression deaktivieren
var disableCompression = new CompressionToggle
{
    Enabled = false,
    Level = 0,
    Algorithm = CompressionAlgorithm.None
};

// Maximale Kompression (für langsame Verbindungen)
var maxCompression = new CompressionToggle
{
    Enabled = true,
    Level = 6,
    Algorithm = CompressionAlgorithm. Zstd,
    MinMessageSize = 32
};
```

### Notizen

- **Empfehlung:** LZ4 Level 1 für beste Balance (Speed vs.  Ratio)
- **MinMessageSize:** Messages unter dieser Größe werden nicht komprimiert (Overhead)
- **Timing:** Kompression gilt ab der Message NACH der Response
- **CPU-Trade-off:** Höheres Level = mehr CPU, weniger Bandbreite
- **Im Prototyp:** Noch nicht implementiert (Phase 3)

---

## CompressionToggleResponse (26)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine  
**Phase:** Phase 3

### Beschreibung

Server-Antwort auf CompressionToggle Request. Bestätigt die aktivierte Kompression oder gibt Fehler zurück.  Ab der nächsten Message nach dieser Response ist die neue Kompression aktiv.

### Im Scope ✅

- Bestätigung der Kompression-Einstellungen
- Mögliche Anpassungen durch Server (z.B. Level-Cap)
- Fehler bei nicht unterstütztem Algorithmus

### Nicht im Scope ❌

- Kompression pro Message-Typ

### Response Payload

| Feld           | Typ                  | Beschreibung                  | Pflicht    |
| -------------- | -------------------- | ----------------------------- | ---------- |
| Success        | bool                 | Toggle erfolgreich?           | Ja         |
| ErrorCode      | string?              | Fehlercode bei Failure        | Nein       |
| ErrorMessage   | string?              | Menschenlesbare Fehlermeldung | Nein       |
| Enabled        | bool                 | Kompression jetzt aktiv?      | Ja         |
| Level          | byte                 | Tatsächlich verwendetes Level | Bei Erfolg |
| Algorithm      | CompressionAlgorithm | Verwendeter Algorithmus       | Bei Erfolg |
| MinMessageSize | ushort               | Effektive Mindestgröße        | Bei Erfolg |
|                |                      |                               |            |

### Verwandte Messages

| Message | ID | Beziehung |
|---------|-----|-----------|
| `CompressionToggle` | 20 | Request zu dieser Response |

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType. CompressionToggleResponse)]
public class CompressionToggleResponse : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.CompressionToggleResponse;
    [Key(1)] public bool Success { get; set; }
    [Key(2)] public string? ErrorCode { get; set; }
    [Key(3)] public string? ErrorMessage { get; set; }
    [Key(4)] public bool Enabled { get; set; }
    [Key(5)] public byte Level { get; set; }
    [Key(6)] public CompressionAlgorithm Algorithm { get; set; }
    [Key(7)] public ushort MinMessageSize { get; set; }
}
```

### Beispiel Payloads

```csharp
// Erfolg - Settings wie angefordert
var successResponse = new CompressionToggleResponse
{
    Success = true,
    Enabled = true,
    Level = 1,
    Algorithm = CompressionAlgorithm.LZ4,
    MinMessageSize = 64
};

// Erfolg - Server hat Level angepasst
var adjustedResponse = new CompressionToggleResponse
{
    Success = true,
    Enabled = true,
    Level = 3, // Client wollte 9, Server capped auf 3
    Algorithm = CompressionAlgorithm.LZ4,
    MinMessageSize = 64
};

// Fehler
var errorResponse = new CompressionToggleResponse
{
    Success = false,
    ErrorCode = "UNSUPPORTED_ALGORITHM",
    ErrorMessage = "Zstd compression is not supported on this server",
    Enabled = false
};
```

### Error Codes

| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `UNSUPPORTED_ALGORITHM` | Algorithmus nicht unterstützt | Anderen Algorithmus wählen |
| `COMPRESSION_DISABLED` | Server erlaubt keine Kompression | Ohne Kompression weitermachen |
| `INVALID_LEVEL` | Level außerhalb 0-9 | Gültiges Level wählen |

### Frame-Format mit Kompression

```
┌─────────────────────────────────────────────────────────────┐
│  COMPRESSED MESSAGE FRAME                                    │
│                                                              │
│  ┌──────────┬──────────┬──────────┬───────────────────────┐ │
│  │  1 Byte  │  4 Bytes │  4 Bytes │       N Bytes         │ │
│  │  Flags   │  Length  │ Original │  Compressed Payload   │ │
│  │          │(compress)│  Length  │                       │ │
│  └──────────┴──────────┴──────────┴───────────────────────┘ │
│                                                              │
│  Flags Byte:                                                  │
│  ┌─────┬─────┬─────┬─────┬─────┬─────┬─────┬─────┐         │
│  │  7  │  6  │  5  │  4  │  3  │  2  │  1  │  0  │         │
│  │ Res │ Res │ Res │ Res │ Res │ Res │ Alg │Comp │         │
│  └─────┴─────┴─────┴─────┴─────┴─────┴─────┴─────┘         │
│                                                              │
│  Bit 0: IsCompressed (1 = ja, 0 = nein)                     │
│  Bit 1: Algorithm (0 = LZ4, 1 = Zstd)                       │
└─────────────────────────────────────────────────────────────┘
```

### Notizen

- **Timing:** Kompression aktiv ab Message NACH dieser Response
- **Beide Richtungen:** Client und Server nutzen gleiche Einstellungen
- **Overhead:** Kleine Messages (<MinMessageSize) werden nicht komprimiert
- **Fallback:** Bei Dekompressions-Fehler → Disconnect

---

## MessageType Enum Erweiterung

```csharp
// In MessageType.cs - CONNECTION / AUTHENTICATION (0000-0099)
EncryptionHandshake = 19,
CompressionToggle = 20,
CharacterSelectResponse = 21,
CharacterCreateResponse = 22,
CharacterDeleteResponse = 23,
ServerSelectResponse = 24,
EncryptionHandshakeResponse = 25,  // NEU
CompressionToggleResponse = 26,     // NEU
```

---

**Letzte Aktualisierung**: 2026-01-04  
**Version**: 2.1.0

[← Zurück zur Übersicht](Message-Reference.md)

Source: docs/03-messages/00-connection.md
