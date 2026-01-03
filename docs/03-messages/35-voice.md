# 🎙️ Voice Messages (3500-3599)

**Kategorie:** 35  
**Range:** 3500-3599  
**Phase:** Prototyp  
**Status:** 🟢 In Entwicklung

[← Zurück zur Übersicht](README.md)

---

## 📋 Inhaltsverzeichnis

- [📋 Überblick](#-überblick)
- [🧠 Datenmodell](#-datenmodell)
- [🔒 Permissions & Moderation](#-permissions--moderation)
- [🔄 Presence & Indicators](#-presence--indicators)
- [🌐 Transport / Integration](#-transport--integration)
- [🧱 DTOs / Interfaces](#-dtos--interfaces)
- [🧩 Enums / ErrorCodes / Flags](#-enums--errorcodes--flags)
- [⚙️ Regeln & Sicherheit](#️-regeln--sicherheit)
- [📩 Aktive Messages 3500-3599](#-aktive-messages-3500-3599)
- [🗑️ Obsolete Messages](#️-obsolete-messages)
- [🧨 Edge Cases & Fehlerfälle](#-edge-cases--fehlerfälle)
- [📎 Anhang](#-anhang)

---

## 📋 Überblick

Diese Kategorie umfasst alle Messages für **Voice Chat Signaling und Kontrolle** im 2DMMO. 

> **Annahme:** Audio-Streams (PCM/Opus-Daten) werden NICHT über den MMO Message Bus transportiert. Der MMO-Server koordiniert nur Signaling, Channel-Management, Permissions und Presence-States. Die eigentliche Audio-Übertragung erfolgt über einen externen Voice-Service (z.B. WebRTC, Photon Voice, Vivox).

### Im Scope ✅
- Voice Channel Join/Leave Signaling
- Mute/Deafen/Volume State Synchronisation
- Speaking Indicators (Push-to-Talk State, Voice Activity)
- Channel-Management (Liste, Permissions)
- Server-side Audio Events (Trigger, Music, Ambience)
- Moderation (Server-Mute, Kick from Voice)

### Nicht im Scope ❌
- Audio-Stream Transport (PCM/Opus Frames) → externer Voice-Service
- Echo Cancellation, Noise Suppression → Client-side DSP
- Codec Negotiation → externer Voice-Service
- TURN/STUN Server Management → externer Voice-Service

---

## 🧠 Datenmodell

### VoiceSession

Repräsentiert die Voice-Session eines Spielers.

| Feld | Typ | Beschreibung |
|------|-----|--------------|
| SessionId | Guid | Eindeutige Session-ID |
| PlayerId | Guid | Spieler PersistentId |
| CurrentChannelId | Guid? | Aktueller Voice-Channel (null = nicht verbunden) |
| IsMuted | bool | Selbst gemutet? |
| IsDeafened | bool | Selbst taubgestellt? |
| IsSpeaking | bool | Spricht gerade? |
| InputVolume | byte | Mikrofon-Lautstärke (0-100) |
| OutputVolume | byte | Ausgabe-Lautstärke (0-100) |
| PushToTalkActive | bool | PTT-Taste gedrückt? |
| JoinedAt | long | Unix Timestamp Beitritt |

### VoiceChannel

Repräsentiert einen Voice-Channel.

| Feld | Typ | Beschreibung |
|------|-----|--------------|
| ChannelId | Guid | Eindeutige Channel-ID |
| ChannelType | VoiceChannelType | Party, Guild, Zone, Custom |
| Name | string | Anzeigename |
| OwnerId | Guid? | Besitzer (bei Custom-Channels) |
| MaxParticipants | int | Max. Teilnehmer (0 = unbegrenzt) |
| Participants | List&lt;Guid&gt; | Aktuelle Teilnehmer (PlayerIds) |
| RequiredPermission | VoicePermission | Benötigte Berechtigung |
| IsLocked | bool | Channel gesperrt? |

### VoiceParticipant

Repräsentiert einen Teilnehmer in einem Voice-Channel.

| Feld | Typ | Beschreibung |
|------|-----|--------------|
| PlayerId | Guid | Spieler PersistentId |
| DisplayName | string | Anzeigename |
| IsMuted | bool | Selbst gemutet |
| IsDeafened | bool | Selbst taubgestellt |
| IsServerMuted | bool | Vom Server/Moderator gemutet |
| IsSpeaking | bool | Spricht gerade |
| JoinedAt | long | Beitritts-Timestamp |

---

## 🔒 Permissions & Moderation

### Berechtigungsstufen

| Permission | Beschreibung | Kann |
|------------|--------------|------|
| `None` | Keine Voice-Rechte | Nichts |
| `Listen` | Nur zuhören | Anderen zuhören |
| `Speak` | Sprechen erlaubt | Zuhören + Sprechen |
| `Moderate` | Moderator | + Server-Mute, Kick |
| `Admin` | Administrator | + Channel erstellen/löschen, Permissions ändern |

### Moderation-Aktionen

| Aktion | Berechtigung | Beschreibung |
|--------|--------------|--------------|
| Server-Mute | Moderate | Spieler kann nicht mehr sprechen |
| Kick | Moderate | Spieler aus Channel entfernen |
| Ban | Admin | Spieler dauerhaft von Channel ausschließen |
| Lock Channel | Admin | Keine neuen Teilnehmer |

### Permission-Vererbung

```
Guild Voice:
├── Guild Master → Admin
├── Officer → Moderate
├── Member → Speak
└── Guest → Listen

Party Voice:
├── Party Leader → Moderate
└── Member → Speak

Zone Voice:
└── Alle → Speak (falls aktiviert)
```

---

## 🔄 Presence & Indicators

### Speaking Indicator

Der Server trackt wann ein Spieler spricht und broadcastet dies an alle Channel-Teilnehmer.

```
Client A                    Server                    Client B
   │                          │                          │
   │  [PTT pressed]           │                          │
   │  VoiceSpeaking(true)     │                          │
   │─────────────────────────►│                          │
   │                          │  VoiceSpeaking broadcast │
   │                          │─────────────────────────►│
   │                          │                          │
   │  [PTT released]          │                          │
   │  VoiceSpeaking(false)    │                          │
   │─────────────────────────►│                          │
   │                          │  VoiceSpeaking broadcast │
   │                          │─────────────────────────►│
```

### State Synchronisation

Bei Channel-Join erhält der Client den vollständigen State aller Teilnehmer:

```csharp
// VoiceJoinResult enthält:
- ChannelInfo (Name, Type, etc.)
- List<VoiceParticipant> (alle Teilnehmer mit aktuellem State)
- VoiceServerEndpoint (für Audio-Verbindung)
```

---

## 🌐 Transport / Integration

### Architektur-Übersicht

```
┌─────────────┐     MMO Protocol      ┌─────────────┐
│   Client    │◄────────────────────►│  MMO Server │
│             │   (Signaling only)    │             │
└──────┬──────┘                       └─────────────┘
       │
       │ WebRTC/UDP
       │ (Audio Stream)
       ▼
┌─────────────┐
│Voice Server │  (Externer Service)
│ (SFU/MCU)   │
└─────────────┘
```

### Was läuft über MMO-Messages?

| Über MMO-Protocol | Über Voice-Service |
|-------------------|-------------------|
| Channel Join/Leave Requests | Audio Frames (Opus) |
| Mute/Deafen State | Jitter Buffer |
| Speaking Indicators | Echo Cancellation |
| Permission Checks | Codec Negotiation |
| Participant List | NAT Traversal |
| Server Audio Events | Mixing |

### Voice Server Endpoint

Bei erfolgreichem Channel-Join erhält der Client Verbindungsdaten für den externen Voice-Service:

```csharp
public class VoiceServerEndpoint
{
    public string Host { get; set; }      // "voice.example.com"
    public int Port { get; set; }          // 5060
    public string Protocol { get; set; }   // "webrtc" / "udp"
    public string Token { get; set; }      // Auth-Token für Voice-Server
    public long ExpiresAt { get; set; }    // Token-Ablauf
}
```

---

## 🧱 DTOs / Interfaces

### VoiceChannelDto

```csharp
[MessagePackObject]
public class VoiceChannelDto
{
    [Key(0)] public Guid ChannelId { get; set; }
    [Key(1)] public VoiceChannelType ChannelType { get; set; }
    [Key(2)] public string Name { get; set; } = string.Empty;
    [Key(3)] public int ParticipantCount { get; set; }
    [Key(4)] public int MaxParticipants { get; set; }
    [Key(5)] public bool IsLocked { get; set; }
    [Key(6)] public VoicePermission RequiredPermission { get; set; }
}
```

### VoiceParticipantDto

```csharp
[MessagePackObject]
public class VoiceParticipantDto
{
    [Key(0)] public Guid PlayerId { get; set; }
    [Key(1)] public string DisplayName { get; set; } = string.Empty;
    [Key(2)] public bool IsMuted { get; set; }
    [Key(3)] public bool IsDeafened { get; set; }
    [Key(4)] public bool IsServerMuted { get; set; }
    [Key(5)] public bool IsSpeaking { get; set; }
}
```

### VoiceServerEndpointDto

```csharp
[MessagePackObject]
public class VoiceServerEndpointDto
{
    [Key(0)] public string Host { get; set; } = string.Empty;
    [Key(1)] public int Port { get; set; }
    [Key(2)] public string Protocol { get; set; } = "webrtc";
    [Key(3)] public string Token { get; set; } = string.Empty;
    [Key(4)] public long ExpiresAt { get; set; }
}
```

### VoiceStateDto

```csharp
[MessagePackObject]
public class VoiceStateDto
{
    [Key(0)] public bool IsMuted { get; set; }
    [Key(1)] public bool IsDeafened { get; set; }
    [Key(2)] public bool IsSpeaking { get; set; }
    [Key(3)] public byte InputVolume { get; set; }
    [Key(4)] public byte OutputVolume { get; set; }
}
```

---

## 🧩 Enums / ErrorCodes / Flags

### VoiceChannelType

```csharp
public enum VoiceChannelType : byte
{
    Zone = 1,       // Zone-weiter Voice (Proximity)
    Party = 2,      // Party Voice
    Guild = 3,      // Guild Voice
    Raid = 4,       // Raid Voice
    Custom = 5,     // Benutzerdefinierter Channel
    Whisper = 6     // 1:1 Voice Call
}
```

### VoicePermission

```csharp
public enum VoicePermission : byte
{
    None = 0,
    Listen = 1,
    Speak = 2,
    Moderate = 3,
    Admin = 4
}
```

### VoiceErrorCode

```csharp
public enum VoiceErrorCode : ushort
{
    Success = 0,
    
    // Channel Errors (3500-3509)
    ChannelNotFound = 3500,
    ChannelFull = 3501,
    ChannelLocked = 3502,
    AlreadyInChannel = 3503,
    NotInChannel = 3504,
    
    // Permission Errors (3510-3519)
    InsufficientPermission = 3510,
    ServerMuted = 3511,
    Banned = 3512,
    
    // State Errors (3520-3529)
    AlreadyMuted = 3520,
    AlreadyUnmuted = 3521,
    AlreadyDeafened = 3522,
    AlreadyUndeafened = 3523,
    
    // Rate Limit (3530-3539)
    RateLimited = 3530,
    TooManyRequests = 3531,
    
    // Technical Errors (3540-3549)
    VoiceServiceUnavailable = 3540,
    ConnectionFailed = 3541,
    TokenExpired = 3542
}
```

### VoiceStateFlags

```csharp
[Flags]
public enum VoiceStateFlags : byte
{
    None = 0,
    Muted = 1 << 0,
    Deafened = 1 << 1,
    ServerMuted = 1 << 2,
    Speaking = 1 << 3,
    PushToTalkActive = 1 << 4
}
```

---

## ⚙️ Regeln & Sicherheit

### Rate Limits

| Action | Limit | Cooldown |
|--------|-------|----------|
| Channel Join | 5/min | - |
| Channel Leave | 10/min | - |
| Mute/Unmute Toggle | 20/min | - |
| Speaking State Update | 50/s | - |
| Volume Change | 10/min | - |

### Anti-Abuse

| Check | Beschreibung | Aktion |
|-------|--------------|--------|
| Spam Detection | Zu häufiges Mute/Unmute Toggle | Temporärer Voice-Ban |
| Permission Check | Jede Aktion gegen Berechtigungen prüfen | Request ablehnen |
| Channel Validation | Existiert Channel? Ist Spieler Mitglied? | Request ablehnen |
| Server-Mute Bypass | Versuch trotz Server-Mute zu sprechen | Ignorieren + Log |

### Privacy

- Voice-Channel Membership ist nur für Channel-Teilnehmer sichtbar
- Speaking-State nur an Channel-Teilnehmer broadcasten
- Keine Aufzeichnung von Audio-Daten auf dem MMO-Server
- Voice-Server Tokens sind kurzlebig (5 Minuten)

---

## 📩 Aktive Messages 3500-3599

---

## VoiceJoinChannel (3500)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine (Permission wird serverseitig geprüft)

### Beschreibung

Client möchte einem Voice-Channel beitreten. Server validiert Berechtigung und gibt bei Erfolg Verbindungsdaten für den externen Voice-Service zurück.

### Im Scope ✅
- Party/Guild/Zone Voice beitreten
- Custom Channel beitreten (mit Password falls erforderlich)

### Nicht im Scope ❌
- Audio-Verbindung aufbauen → Client verbindet direkt zum Voice-Service

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.VoiceJoinChannel` | Ja |
| ChannelId | Guid | Ziel-Channel-ID | Ja |
| Password | string? | Optional für passwortgeschützte Channels | Nein |

### Erwartete Response
- **Bei Erfolg:** `VoiceJoinResult` (3501) mit Success=true, Teilnehmerliste und VoiceServerEndpoint
- **Bei Fehler:** `VoiceJoinResult` (3501) mit ErrorCode

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.VoiceJoinChannel)]
public class VoiceJoinChannel : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.VoiceJoinChannel;
    [Key(1)] public Guid ChannelId { get; set; }
    [Key(2)] public string? Password { get; set; }
}
```

### Server-Verhalten

1. Validiere Session und Spieler-ID
2. Prüfe ob Channel existiert
3. Prüfe ob Channel voll/gesperrt
4. Prüfe Berechtigung (Party-Member, Guild-Member, etc.)
5. Prüfe Password falls erforderlich
6. Füge Spieler zur Teilnehmerliste hinzu
7. Generiere Voice-Server Token
8. Sende VoiceJoinResult an Client
9. Broadcaste VoiceParticipantJoined an andere Teilnehmer

### Client-Verhalten

1. Sende VoiceJoinChannel Request
2. Warte auf VoiceJoinResult
3. Bei Erfolg: Verbinde zum Voice-Server mit erhaltenen Credentials
4. Aktualisiere UI mit Teilnehmerliste

### Flow-Diagramm

```
Client                    MMO Server               Voice Service
   │                          │                          │
   │  VoiceJoinChannel        │                          │
   │─────────────────────────►│                          │
   │                          │  Validate permissions    │
   │                          │  Generate token          │
   │  VoiceJoinResult         │                          │
   │◄─────────────────────────│                          │
   │                          │                          │
   │  WebRTC Connect          │                          │
   │─────────────────────────────────────────────────────►│
   │                          │                          │
   │  Audio Stream            │                          │
   │◄────────────────────────────────────────────────────►│
```

### Beispiel Payload

```csharp
var joinRequest = new VoiceJoinChannel
{
    ChannelId = partyVoiceChannelId,
    Password = null // Party-Voice benötigt kein Password
};
```

### Error Codes

| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `ChannelNotFound` | Channel existiert nicht | UI-Fehler anzeigen |
| `ChannelFull` | Max. Teilnehmer erreicht | Warten oder anderen Channel wählen |
| `ChannelLocked` | Channel gesperrt | Warten bis entsperrt |
| `InsufficientPermission` | Keine Berechtigung | UI-Fehler anzeigen |
| `AlreadyInChannel` | Bereits in diesem Channel | Ignorieren |
| `Banned` | Von Channel gebannt | UI-Fehler anzeigen |

### Verwandte Messages

| Message | ID | Beziehung |
|---------|-----|-----------|
| `VoiceJoinResult` | 3501 | Response zu diesem Request |
| `VoiceLeaveChannel` | 3502 | Channel verlassen |
| `VoiceChannelList` | 3503 | Verfügbare Channels |

### Notizen

- Client muss alten Channel verlassen bevor er neuen beitritt
- Voice-Server Token ist 5 Minuten gültig
- Bei Party/Guild-Auflösung wird Client automatisch aus Channel entfernt

---

## VoiceJoinResult (3501)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Antwort auf VoiceJoinChannel Request. Enthält bei Erfolg alle Informationen für Voice-Verbindung.

### Im Scope ✅
- Erfolgs-/Fehlerstatus
- Channel-Informationen
- Teilnehmerliste
- Voice-Server Verbindungsdaten

### Nicht im Scope ❌
- Audio-Daten

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.VoiceJoinResult` | Ja |
| Success | bool | Beitritt erfolgreich? | Ja |
| ErrorCode | VoiceErrorCode | Fehlercode bei Failure | Nein |
| ErrorMessage | string? | Menschenlesbare Fehlermeldung | Nein |
| Channel | VoiceChannelDto? | Channel-Informationen | Bei Erfolg |
| Participants | List&lt;VoiceParticipantDto&gt;? | Aktuelle Teilnehmer | Bei Erfolg |
| Endpoint | VoiceServerEndpointDto? | Voice-Server Verbindungsdaten | Bei Erfolg |

### Erwartete Response
- Keine (ist selbst Response)

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.VoiceJoinResult)]
public class VoiceJoinResult : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.VoiceJoinResult;
    [Key(1)] public bool Success { get; set; }
    [Key(2)] public VoiceErrorCode ErrorCode { get; set; }
    [Key(3)] public string? ErrorMessage { get; set; }
    [Key(4)] public VoiceChannelDto? Channel { get; set; }
    [Key(5)] public List<VoiceParticipantDto>? Participants { get; set; }
    [Key(6)] public VoiceServerEndpointDto? Endpoint { get; set; }
}
```

### Beispiel Payloads

```csharp
// Erfolg
var successResult = new VoiceJoinResult
{
    Success = true,
    Channel = new VoiceChannelDto
    {
        ChannelId = channelId,
        ChannelType = VoiceChannelType.Party,
        Name = "Party Voice",
        ParticipantCount = 3,
        MaxParticipants = 5
    },
    Participants = new List<VoiceParticipantDto>
    {
        new() { PlayerId = player1Id, DisplayName = "Player1", IsSpeaking = false },
        new() { PlayerId = player2Id, DisplayName = "Player2", IsSpeaking = true }
    },
    Endpoint = new VoiceServerEndpointDto
    {
        Host = "voice.example.com",
        Port = 5060,
        Protocol = "webrtc",
        Token = "eyJhbGciOiJIUzI1NiIs...",
        ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(5).ToUnixTimeSeconds()
    }
};

// Fehler
var errorResult = new VoiceJoinResult
{
    Success = false,
    ErrorCode = VoiceErrorCode.ChannelFull,
    ErrorMessage = "Voice channel is full (5/5 participants)"
};
```

---

## VoiceLeaveChannel (3502)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Client verlässt den aktuellen Voice-Channel.

### Im Scope ✅
- Graceful Leave
- Disconnect-Notification an andere Teilnehmer

### Nicht im Scope ❌
- Audio-Verbindung trennen → Client trennt selbst vom Voice-Service

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.VoiceLeaveChannel` | Ja |
| ChannelId | Guid | Channel-ID (zur Validierung) | Ja |

### Erwartete Response
- Server bestätigt implizit durch Broadcast an andere Teilnehmer
- Kein dediziertes Response-Message (Fire-and-Forget)

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.VoiceLeaveChannel)]
public class VoiceLeaveChannel : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.VoiceLeaveChannel;
    [Key(1)] public Guid ChannelId { get; set; }
}
```

### Server-Verhalten

1. Validiere dass Spieler in diesem Channel ist
2. Entferne Spieler aus Teilnehmerliste
3. Broadcaste VoiceParticipantLeft an verbleibende Teilnehmer
4. Invalidiere Voice-Server Token

### Verwandte Messages

| Message | ID | Beziehung |
|---------|-----|-----------|
| `VoiceJoinChannel` | 3500 | Channel beitreten |
| `VoiceChannelList` | 3503 | Verfügbare Channels |

---

## VoiceChannelList (3503)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Liste aller für den Spieler verfügbaren Voice-Channels. Wird bei Login und bei Änderungen (Party-Join, Guild-Join, etc.) gesendet.

### Im Scope ✅
- Party Voice (falls in Party)
- Guild Voice (falls in Guild)
- Zone Voice (falls aktiviert)
- Custom Channels (öffentliche oder eingeladene)

### Nicht im Scope ❌
- Private Channels anderer Spieler

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.VoiceChannelList` | Ja |
| Channels | List&lt;VoiceChannelDto&gt; | Verfügbare Channels | Ja |

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.VoiceChannelList)]
public class VoiceChannelList : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.VoiceChannelList;
    [Key(1)] public List<VoiceChannelDto> Channels { get; set; } = new();
}
```

### Beispiel Payload

```csharp
var channelList = new VoiceChannelList
{
    Channels = new List<VoiceChannelDto>
    {
        new() 
        { 
            ChannelId = partyChannelId, 
            ChannelType = VoiceChannelType.Party, 
            Name = "Party Voice",
            ParticipantCount = 2,
            MaxParticipants = 5
        },
        new() 
        { 
            ChannelId = guildChannelId, 
            ChannelType = VoiceChannelType.Guild, 
            Name = "Guild Voice",
            ParticipantCount = 15,
            MaxParticipants = 50
        }
    }
};
```

---

## VoiceMute (3510)

**Richtung:** 📤 Client → Server  
**Frequenz:** Mittel  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Client mutet sich selbst (Mikrofon aus).

### Im Scope ✅
- Selbst-Mute

### Nicht im Scope ❌
- Server-Mute anderer Spieler → Admin/Moderator Aktion

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.VoiceMute` | Ja |

### Erwartete Response
- Server broadcastet Mute-State an alle Channel-Teilnehmer via VoiceStateUpdate

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.VoiceMute)]
public class VoiceMute : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.VoiceMute;
}
```

### Server-Verhalten

1. Setze Spieler.IsMuted = true
2. Broadcaste State-Update an alle Channel-Teilnehmer

---

## VoiceUnmute (3511)

**Richtung:** 📤 Client → Server  
**Frequenz:** Mittel  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Client hebt Selbst-Mute auf (Mikrofon an).

### Im Scope ✅
- Selbst-Unmute

### Nicht im Scope ❌
- Aufheben von Server-Mute → nur durch Admin/Moderator

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.VoiceUnmute` | Ja |

### Erwartete Response
- Server broadcastet Unmute-State an alle Channel-Teilnehmer
- Bei Server-Mute: ErrorCode `ServerMuted`

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.VoiceUnmute)]
public class VoiceUnmute : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.VoiceUnmute;
}
```

---

## VoiceDeafen (3512)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Client taubstellt sich selbst (hört nichts, wird automatisch auch gemutet).

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.VoiceDeafen` | Ja |

### Erwartete Response
- Server broadcastet Deafen-State an alle Channel-Teilnehmer

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.VoiceDeafen)]
public class VoiceDeafen : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.VoiceDeafen;
}
```

### Notizen
- Deafen impliziert automatisch Mute
- Beim Undeafen wird vorheriger Mute-State wiederhergestellt

---

## VoiceUndeafen (3513)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Client hebt Taubstellung auf.

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.VoiceUndeafen` | Ja |

### Erwartete Response
- Server broadcastet State-Update an alle Channel-Teilnehmer

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.VoiceUndeafen)]
public class VoiceUndeafen : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.VoiceUndeafen;
}
```

---

## VoiceSpeaking (3514)

**Richtung:** 📡 Bidirektional  
**Frequenz:** ⚡ Häufig (bei Voice-Aktivität)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Speaking-Indicator für UI (wer spricht gerade). Client sendet bei Push-to-Talk oder Voice-Activity-Detection, Server broadcastet an alle Teilnehmer.

### Im Scope ✅
- PTT Start/Stop
- Voice Activity Detection Threshold überschritten
- Speaking-Indicator UI Update

### Nicht im Scope ❌
- Audio-Daten

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.VoiceSpeaking` | Ja |
| PlayerId | Guid | Spieler der spricht (nur bei Broadcast) | Bei Broadcast |
| IsSpeaking | bool | Spricht gerade? | Ja |

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.VoiceSpeaking)]
public class VoiceSpeaking : INetworkMessage
{
    [Key(0)] public MessageType Type => MessageType.VoiceSpeaking;
    [Key(1)] public Guid PlayerId { get; set; }
    [Key(2)] public bool IsSpeaking { get; set; }
}
```

### Client-Verhalten (Senden)

```csharp
// Bei PTT-Taste gedrückt
_networkClient.Send(new VoiceSpeaking { IsSpeaking = true });

// Bei PTT-Taste losgelassen
_networkClient.Send(new VoiceSpeaking { IsSpeaking = false });
```

### Client-Verhalten (Empfangen)

```csharp
public void OnVoiceSpeaking(VoiceSpeaking msg)
{
    var participant = _voiceUI.GetParticipant(msg.PlayerId);
    if (participant != null)
    {
        participant.SetSpeakingIndicator(msg.IsSpeaking);
    }
}
```

---

## VoiceVolume (3515)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Client ändert Lautstärke-Einstellungen (Input/Output Volume).

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.VoiceVolume` | Ja |
| InputVolume | byte | Mikrofon-Lautstärke (0-100) | Ja |
| OutputVolume | byte | Ausgabe-Lautstärke (0-100) | Ja |

### Erwartete Response
- Server speichert Einstellungen, keine explizite Response

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.VoiceVolume)]
public class VoiceVolume : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.VoiceVolume;
    [Key(1)] public byte InputVolume { get; set; }
    [Key(2)] public byte OutputVolume { get; set; }
}
```

---

## VoiceData (3520)

**Richtung:** 📡 Broadcast  
**Frequenz:** ⚡⚡ Extrem häufig (wenn Audio läuft)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

> **ANNAHME:** Diese Message ist für Fallback/Low-Latency Szenarien reserviert, wo kein externer Voice-Service verfügbar ist. Im Normalbetrieb werden Audio-Daten NICHT über den MMO Message Bus transportiert.

Audio-Daten-Frames (Opus-kodiert). Nur verwendet wenn kein externer Voice-Service konfiguriert ist.

### Im Scope ✅
- Opus-kodierte Audio-Frames
- Sequence Number für Ordering
- Timestamp für Jitter-Buffer

### Nicht im Scope ❌
- Raw PCM (zu groß)
- Video

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.VoiceData` | Ja |
| PlayerId | Guid | Sender | Ja |
| SequenceNumber | ushort | Frame-Sequenz | Ja |
| Timestamp | uint | RTP-Timestamp | Ja |
| Data | byte[] | Opus-Frame | Ja |

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.VoiceData)]
public class VoiceData : INetworkMessage
{
    [Key(0)] public MessageType Type => MessageType.VoiceData;
    [Key(1)] public Guid PlayerId { get; set; }
    [Key(2)] public ushort SequenceNumber { get; set; }
    [Key(3)] public uint Timestamp { get; set; }
    [Key(4)] public byte[] Data { get; set; } = Array.Empty<byte>();
}
```

### Notizen

- Frame-Größe: ~60-80 Bytes bei 20ms Opus @64kbps
- Server macht KEIN Mixing, nur Routing
- Client-seitig: Jitter-Buffer, Packet-Loss Concealment
- **Im Prototyp deaktiviert** - verwende externen Voice-Service

---

## AudioTrigger (3530)

**Richtung:** 📥 Server → Client  
**Frequenz:** Mittel  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Server triggert einen Audio-Effekt auf dem Client (z.B. Quest-Complete Sound, Achievement-Sound, Combat-Sound).

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.AudioTrigger` | Ja |
| SoundId | string | Sound-Asset-ID | Ja |
| Volume | float | Lautstärke (0.0-1.0) | Ja |
| Position | Position? | 3D-Position (null = non-positional) | Nein |
| Loop | bool | Sound loopen? | Ja |

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.AudioTrigger)]
public class AudioTrigger : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.AudioTrigger;
    [Key(1)] public string SoundId { get; set; } = string.Empty;
    [Key(2)] public float Volume { get; set; } = 1.0f;
    [Key(3)] public Position? Position { get; set; }
    [Key(4)] public bool Loop { get; set; }
}
```

### Beispiel Payloads

```csharp
// Quest Complete Sound
var questSound = new AudioTrigger
{
    SoundId = "sfx_quest_complete",
    Volume = 1.0f,
    Position = null, // UI Sound
    Loop = false
};

// Ambient Fire Sound (positional)
var fireSound = new AudioTrigger
{
    SoundId = "sfx_fire_crackling",
    Volume = 0.8f,
    Position = new Position(100f, 200f),
    Loop = true
};
```

---

## AudioStop (3531)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Server stoppt einen laufenden Audio-Effekt.

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.AudioStop` | Ja |
| SoundId | string | Sound-Asset-ID zum Stoppen | Ja |
| FadeOutMs | int | Fade-Out Dauer in ms (0 = sofort) | Ja |

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.AudioStop)]
public class AudioStop : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.AudioStop;
    [Key(1)] public string SoundId { get; set; } = string.Empty;
    [Key(2)] public int FadeOutMs { get; set; }
}
```

---

## MusicChange (3532)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Server wechselt die Hintergrundmusik (z.B. bei Zone-Wechsel, Boss-Fight, Event).

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.MusicChange` | Ja |
| MusicId | string | Musik-Asset-ID | Ja |
| FadeOutMs | int | Fade-Out der alten Musik | Ja |
| FadeInMs | int | Fade-In der neuen Musik | Ja |
| Volume | float | Ziel-Lautstärke (0.0-1.0) | Ja |

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.MusicChange)]
public class MusicChange : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.MusicChange;
    [Key(1)] public string MusicId { get; set; } = string.Empty;
    [Key(2)] public int FadeOutMs { get; set; } = 2000;
    [Key(3)] public int FadeInMs { get; set; } = 2000;
    [Key(4)] public float Volume { get; set; } = 0.7f;
}
```

### Beispiel Payload

```csharp
// Boss-Fight Musik
var bossMusic = new MusicChange
{
    MusicId = "music_boss_epic",
    FadeOutMs = 1000,
    FadeInMs = 500,
    Volume = 0.9f
};
```

---

## AmbienceChange (3533)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Server wechselt den Ambiente-Sound (z.B. bei Zone-Wechsel, Tag/Nacht, Wetter).

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.AmbienceChange` | Ja |
| AmbienceId | string | Ambience-Asset-ID | Ja |
| FadeOutMs | int | Fade-Out des alten Ambience | Ja |
| FadeInMs | int | Fade-In des neuen Ambience | Ja |
| Volume | float | Ziel-Lautstärke (0.0-1.0) | Ja |

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.AmbienceChange)]
public class AmbienceChange : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.AmbienceChange;
    [Key(1)] public string AmbienceId { get; set; } = string.Empty;
    [Key(2)] public int FadeOutMs { get; set; } = 3000;
    [Key(3)] public int FadeInMs { get; set; } = 3000;
    [Key(4)] public float Volume { get; set; } = 0.5f;
}
```

---

## 🗑️ Obsolete Messages

Derzeit keine obsoleten Messages in dieser Kategorie.

---

## 🧨 Edge Cases & Fehlerfälle

### Verbindungsabbruch während Voice-Session

| Szenario | Server-Verhalten | Client-Verhalten |
|----------|------------------|------------------|
| Client disconnected | Entferne aus Channel, Broadcast an Teilnehmer | Reconnect-Versuch, dann Channel rejoin |
| Voice-Server down | Invalidiere alle Tokens, informiere Clients | Zeige Fehler, deaktiviere Voice UI |
| Token expired | Reject neue Audio-Verbindungen | Request neuen Token via VoiceJoinChannel |

### Gleichzeitige Aktionen

| Szenario | Verhalten |
|----------|-----------|
| Join während Leave | Leave gewinnt, Join wird rejected |
| Mute während Speaking | Mute gewinnt, Speaking=false |
| Deafen während Unmute | Deafen gewinnt, bleibt muted |

### Permission-Änderungen

| Szenario | Verhalten |
|----------|-----------|
| Party aufgelöst während Voice | Alle werden aus Party-Voice entfernt |
| Aus Guild gekickt | Wird aus Guild-Voice entfernt |
| Server-Mute während Sprechen | Audio wird sofort unterbrochen |

---

## 📎 Anhang

### MessageType Enum Updates

Die folgenden MessageTypes sind bereits im `MessageType.cs` definiert (Range 3500-3599):

```csharp
// VOICE CHAT / AUDIO (3500-3599)
VoiceJoinChannel = 3500,
VoiceJoinResult = 3501,
VoiceLeaveChannel = 3502,
VoiceChannelList = 3503,
VoiceMute = 3510,
VoiceUnmute = 3511,
VoiceDeafen = 3512,
VoiceUndeafen = 3513,
VoiceSpeaking = 3514,
VoiceVolume = 3515,
VoiceData = 3520,
AudioTrigger = 3530,
AudioStop = 3531,
MusicChange = 3532,
AmbienceChange = 3533,
```

### Integrationshinweise

#### Voice-Service Integration

Für die Integration eines externen Voice-Services (empfohlen):

1. **WebRTC (Empfohlen für Web-Clients)**
   - Verwende einen SFU (Selective Forwarding Unit) wie Janus, mediasoup
   - MMO-Server generiert JWT-Tokens für Voice-Server Auth
   - Client verbindet direkt zum WebRTC-Server nach VoiceJoinResult

2. **Photon Voice (Empfohlen für Unity-Clients)**
   - Photon Room ID = VoiceChannel.ChannelId
   - MMO-Server steuert Room-Membership
   - Photon übernimmt Audio-Transport

3. **Vivox (Enterprise-Lösung)**
   - Integrierte Moderation
   - Positional Audio Support
   - Enterprise-SLA

#### Client-Implementation Checklist

- [ ] VoiceJoinChannel/Leave UI
- [ ] Mute/Deafen Toggle Buttons
- [ ] Speaking Indicator Animation
- [ ] Volume Slider (Input/Output)
- [ ] Push-to-Talk Key Binding
- [ ] Voice Activity Detection Threshold
- [ ] Participant List UI
- [ ] Error Handling (Disconnects, Permission Denied)

#### Server-Implementation Checklist

- [ ] VoiceChannel Management (Create, Delete, Lock)
- [ ] Participant Tracking
- [ ] Permission Validation
- [ ] Token Generation für Voice-Service
- [ ] Broadcast-Logik für State-Updates
- [ ] Cleanup bei Disconnect/Logout
- [ ] Rate Limiting
- [ ] Moderation Actions (Server-Mute, Kick, Ban)

---

**Letzte Aktualisierung**: 2026-01-03  
**Version**: 3.0.0  
**Status**: 🟢 In Entwicklung

[← Zurück zur Übersicht](README.md)
