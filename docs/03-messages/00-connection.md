# 🔐 Connection / Authentication Messages (0000-0099)

**Kategorie:** 0  
**Range:** 0000-0099  
**Phase:** Prototyp  
**Status:** 🟢 In Entwicklung

[← Zurück zur Übersicht](README.md)

---

## 📋 Inhaltsverzeichnis

- [LoginRequest (1)](#loginrequest-1)
- [LoginResponse (2)](#loginresponse-2)
- [LogoutRequest (3)](#logoutrequest-3)
- [Heartbeat (4)](#heartbeat-4)
- [Disconnect (5)](#disconnect-5)
- [ReconnectRequest (6)](#reconnectrequest-6)
- [ReconnectResponse (7)](#reconnectresponse-7)
- [SessionValidate (8)](#sessionvalidate-8)
- [CharacterSelect (9)](#characterselect-9)
- [CharacterCreate (10)](#charactercreate-10)
- [CharacterDelete (11)](#characterdelete-11)
- [CharacterListRequest (12)](#characterlistrequest-12)
- [CharacterListResponse (13)](#characterlistresponse-13)
- [ServerSelect (14)](#serverselect-14)
- [RealmListRequest (15)](#realmlistrequest-15)
- [RealmListResponse (16)](#realmlistresponse-16)
- [AccountDataRequest (17)](#accountdatarequest-17)
- [AccountDataResponse (18)](#accountdataresponse-18)
- [EncryptionHandshake (19)](#encryptionhandshake-19)
- [CompressionToggle (20)](#compressiontoggle-20)

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
- OAuth/Social Login → verwende externe Account-API (Phase 3)

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Username | string | Account-Name (3-16 Zeichen) | Ja* |
| Password | string | Passwort (Hash) | Ja* |
| SessionToken | string | Wiederverbindungs-Token | Ja* |
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
- Andere Spieler sehen `PlayerLeftZone` Broadcast
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

## Disconnect (5)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung
Server fordert Client auf, die Verbindung zu trennen. Kann verschiedene Gründe haben (Kick, Maintenance, Timeout, Ban).

### Im Scope ✅
- Forced Disconnect
- Disconnect-Grund (Code + Message)
- Reconnect-Erlaubnis (Flag)

### Nicht im Scope ❌
- Graceful Logout → Client sendet `LogoutRequest` (3)

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Reason | string | Disconnect-Grund (Code) | Ja |
| Message | string | Menschenlesbare Nachricht | Ja |
| CanReconnect | bool | Darf Client reconnecten? | Ja |
| ReconnectDelay | int | Sekunden bis Reconnect erlaubt | Nein |

### Erwartete Response
- Keine (Client schließt Connection)

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `LogoutRequest` | 3 | Client-initiierter Disconnect |
| `KickNotification` | 912 | Admin-Kick mit Details |
| `BanNotification` | 916 | Ban-Benachrichtigung |
| `ServerShutdown` | 914 | Shutdown-Warning vor Disconnect |

### Beispiel Payload
```csharp
// Timeout
var timeoutDisconnect = new Disconnect
{
    Type = MessageType.Disconnect,
    Reason = "TIMEOUT",
    Message = "Connection timed out due to inactivity",
    CanReconnect = true,
    ReconnectDelay = 0
};

// Admin Kick
var kickDisconnect = new Disconnect
{
    Type = MessageType.Disconnect,
    Reason = "KICKED",
    Message = "You have been kicked by an administrator",
    CanReconnect = true,
    ReconnectDelay = 300 // 5 Minuten
};

// Ban
var banDisconnect = new Disconnect
{
    Type = MessageType.Disconnect,
    Reason = "BANNED",
    Message = "Your account has been permanently banned",
    CanReconnect = false
};
```

### Disconnect Reasons
| Reason | CanReconnect | Beschreibung |
|--------|--------------|--------------|
| `TIMEOUT` | true | Keine Heartbeats empfangen |
| `KICKED` | true | Admin-Kick |
| `BANNED` | false | Account gesperrt |
| `MAINTENANCE` | true | Server-Wartung |
| `SERVER_SHUTDOWN` | true | Server fährt herunter |
| `DUPLICATE_LOGIN` | true | Andere Session auf gleichem Account |
| `PROTOCOL_ERROR` | true | Ungültige Message empfangen |
| `VERSION_MISMATCH` | false | Client muss updaten |

### Notizen
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
| ZoneId | int | Aktuelle Zone-ID | Bei Erfolg |
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

## SessionValidate (8)

**Richtung:** 🔀 Server ↔ Server (Internal)  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja (Server-to-Server)  
**Spezielle Rechte:** 👑 Server

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

---

## CharacterSelect (9)

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
- Character erstellen → verwende `CharacterCreate` (10)
- Character löschen → verwende `CharacterDelete` (11)

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| CharacterId | long | ID des zu wählenden Characters | Ja |

### Erwartete Response
- **Bei Erfolg:** `JoinZone` (100) für Character-Spawn
- **Bei Fehler:** `ErrorMessage` (910)

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `CharacterListResponse` | 13 | Zeigt verfügbare Characters |
| `JoinZone` | 100 | Nächster Schritt nach Auswahl |
| `CharacterCreate` | 10 | Character erstellen |

### Beispiel Payload
```csharp
var selectRequest = new CharacterSelect
{
    Type = MessageType.CharacterSelect,
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

## CharacterCreate (10)

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
- Premium-Features → Phase 3

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Name | string | Character-Name (3-16 Zeichen, alphanumerisch) | Ja |
| Race | int | Rassen-ID | Ja |
| Class | int | Klassen-ID | Ja |
| AppearanceData | byte[] | Serialisierte Appearance-Daten | Ja |

### Erwartete Response
- **Bei Erfolg:** `CharacterListResponse` (13) mit neuem Character
- **Bei Fehler:** `ErrorMessage` (910)

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `CharacterListResponse` | 13 | Zeigt erstellten Character |
| `CharacterDelete` | 11 | Character löschen |

### Beispiel Payload
```csharp
var createRequest = new CharacterCreate
{
    Type = MessageType.CharacterCreate,
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

## CharacterDelete (11)

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
- Character-Transfer → Phase 3

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| CharacterId | long | ID des zu löschenden Characters | Ja |
| Confirmation | string | Muss "DELETE" sein | Ja |

### Erwartete Response
- **Bei Erfolg:** `CharacterListResponse` (13) mit markiertem Character
- **Bei Fehler:** `ErrorMessage` (910)

### Beispiel Payload
```csharp
var deleteRequest = new CharacterDelete
{
    Type = MessageType.CharacterDelete,
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
| ZoneId | int | Letzte Zone |
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

## ServerSelect (14)

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
- **Bei Erfolg:** Connection wird zu gewähltem Realm transferiert
- **Bei Fehler:** `ErrorMessage` (910)

### Notizen
- Im Prototyp: Nur 1 Realm → Message wird nicht aktiv verwendet
- Phase 2: Multi-Realm Support

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
**Frequenz:** Einmalig  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung
**Phase 3 Feature** - TLS-Encryption Handshake für sichere Verbindung. Client initiiert Handshake, Server antwortet mit separater Response-Message.

### Notizen
- Im Prototyp: Nicht implementiert (TCP ohne TLS)
- Phase 3: TLS 1.3 für alle Verbindungen
- Server antwortet mit separater `EncryptionHandshakeResponse` Message (nicht bidirektional)

---

## CompressionToggle (20)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
**Phase 3 Feature** - Aktiviert/Deaktiviert Message-Kompression.

### Im Scope ✅
- Kompression aktivieren/deaktivieren
- Kompression-Level wählen

### Notizen
- Im Prototyp: Nicht implementiert
- Phase 3: LZ4 oder Zstd Kompression für Messages

---

**Letzte Aktualisierung**: 2025-12-17  
**Version**: 1.0.0

[← Zurück zur Übersicht](README.md)
