# 🌐 Netzwerk-Protokoll

## 2DMMO – Network Protocol Specification

**Version:** 1.2.0  
**Letzte Aktualisierung:** 2025-12-09  
**Teil von:** [Architektur-Dokumentation](README.md)

---

## 📋 Übersicht

Diese Dokumentation beschreibt das Netzwerk-Protokoll für die Client-Server-Kommunikation im 2DMMO.

---

## Transport Layer

| Eigenschaft | Wert |
|-------------|------|
| **Protokoll** | TCP |
| **Verschlüsselung** | TLS 1.3 |
| **Port** | 7777 (konfigurierbar) |
| **Encoding** | MessagePack (Binary) |

---

## Message Framing

Jede Nachricht über TCP hat folgendes Format:

```
┌─────────────────────────────────────────────────────────┐
│                    MESSAGE FRAME                         │
│                                                          │
│  ┌──────────┬─────────────────────────────────────────┐ │
│  │  4 Bytes │       N Bytes                           │ │
│  │  Length  │       Payload                           │ │
│  │ (uint32) │   (MessagePack Data)                    │ │
│  └──────────┴─────────────────────────────────────────┘ │
│                                                          │
│  Length:  Länge des Payloads in Bytes (Little-Endian)   │
│  Payload: MessagePack-serialisierte Daten               │
│                                                          │
│  Beispiel: LoginRequest                                 │
│  ┌────────────┬────────────────────────────────────┐    │
│  │ 2A 00 00 00│ 93 01 A8 54 65 73 74 55 73 65 ... │    │
│  └────────────┴────────────────────────────────────┘    │
│         │                    │                           │
│         │                    └─ MessagePack Payload      │
│         │                       [Type:1, Username, ...]  │
│         └─ 42 Bytes Payload-Länge                       │
└─────────────────────────────────────────────────────────┘
```

### Warum kein separates Type-Byte?

**Entscheidung:** Der MessageType ist NICHT als separates Byte im Frame-Header, sondern **innerhalb der MessagePack-Daten** bei `Key(0)`.

**Begründung:**
- ✅ **Einfachere Deserialisierung:** MessagePack deserialisiert Type automatisch mit
- ✅ **Konsistente Datenstruktur:** Alle Felder sind im gleichen Format
- ✅ **Typensicherheit:** Type ist immer Teil der Message-Klasse
- ✅ **Weniger Fehleranfällig:** Keine manuelle Type-Byte-Extraktion nötig

**Implementierung:**

```csharp
// Alle Messages benötigen [NetworkMessage] Attribute für Auto-Registration
[MessagePackObject]
[NetworkMessage(MessageType.LoginRequest)]  // ← WICHTIG: Registriert Type automatisch
public class LoginRequest : INetworkMessage
{
    [Key(0)]
    public MessageType Type => MessageType.LoginRequest;
    
    [Key(1)]
    public string Username { get; set; }
    
    [Key(2)]
    public string Password { get; set; }
}

// Deserialisierung: Type wird automatisch per Dictionary-Lookup erkannt
// MessageSerializer scannt beim Start alle Types mit [NetworkMessage] Attribut
MessageHeader header = MessagePackSerializer.Deserialize<MessageHeader>(payload);
INetworkMessage message = MessageSerializer.Deserialize(payload);  // O(1) Lookup
```

**Wichtig:** Neue Message-Types benötigen NUR das `[NetworkMessage]` Attribut. 
Der `MessageSerializer` registriert sie automatisch beim Start.

---

## Server Networking Architektur

### Komponenten-Übersicht

Das Networking-System besteht aus mehreren Schlüssel-Komponenten, die zusammenarbeiten:

```
┌─────────────────────────────────────────────────────────────────┐
│                  SERVER NETWORKING ARCHITEKTUR                   │
│                                                                  │
│  ┌────────────────┐          ┌────────────────┐                 │
│  │  NetworkServer │          │   GameServer   │                 │
│  │  (geplant)     │          │   (GameLoop)   │                 │
│  ├────────────────┤          ├────────────────┤                 │
│  │ • TcpListener  │          │ • Tick Loop    │                 │
│  │ • Accept()     │──Events─▶│ • InputPhase   │                 │
│  │ • Manage       │          │ • UpdatePhase  │                 │
│  │   Connections  │◀─Calls──│ • OutputPhase  │                 │
│  └────────┬───────┘          └────────────────┘                 │
│           │                                                      │
│           │ Creates/Manages                                     │
│           ▼                                                      │
│  ┌────────────────┐                                             │
│  │ClientConnection│                                             │
│  │   (geplant)    │                                             │
│  ├────────────────┤                                             │
│  │ • TcpClient    │                                             │
│  │ • ReadAsync()  │                                             │
│  │ • WriteAsync() │                                             │
│  │ • SendQueue    │                                             │
│  │ • PlayerId     │                                             │
│  └────────────────┘                                             │
│                                                                  │
│  Message Flow:                                                  │
│  Client ──TCP──▶ NetworkServer ──Event──▶ GameServer           │
│         ◀──TCP── NetworkServer ◀──Call─── GameServer           │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

### Klassen-Übersicht

| Klasse | Status | Verantwortung | Datei |
|--------|--------|---------------|-------|
| **GameServer** | ✅ Implementiert | Game Loop (25 Hz), Input/Update/Output Phasen | `server/Mmo.Server/GameLoop/GameServer.cs` |
| **NetworkServer** | 🔄 Geplant | TCP-Listener, Connection-Management, Events | `server/Mmo.Server/Networking/NetworkServer.cs` |
| **ClientConnection** | 🔄 Geplant | Pro-Client TCP-Handling, Read/Write, SendQueue | `server/Mmo.Server/Networking/ClientConnection.cs` |
| **MessageSerializer** | ✅ Implementiert | Attribute-basierte Message-Registrierung (Dictionary, O(1) Lookup) | `shared/Mmo.Shared/Serialization/MessageSerializer.cs` |
| **ZoneManager** | ✅ Implementiert | Zone-State, Entity-Management | `server/Mmo.Server/Zones/ZoneManager.cs` |

### NetworkEvents (Geplant)

Die folgenden Event-Klassen werden für die Kommunikation zwischen NetworkServer und GameServer verwendet:

| Event | EventArgs | Beschreibung |
|-------|-----------|--------------|
| `ClientConnected` | `ClientConnectedEventArgs` | Neuer Client hat TCP-Verbindung aufgebaut |
| `ClientDisconnected` | `ClientDisconnectedEventArgs` | Client wurde getrennt (siehe DisconnectReason) |
| `MessageReceived` | `MessageReceivedEventArgs` | Neue Nachricht von einem Client empfangen |
| `NetworkError` | `NetworkErrorEventArgs` | Netzwerk-Fehler aufgetreten |

**ClientConnectedEventArgs:**
```csharp
public class ClientConnectedEventArgs : EventArgs
{
    public Guid ConnectionId { get; set; }
    public string RemoteEndpoint { get; set; }
}
```

**ClientDisconnectedEventArgs:**
```csharp
public class ClientDisconnectedEventArgs : EventArgs
{
    public Guid PlayerId { get; set; }
    public DisconnectReason Reason { get; set; }
}
```

**MessageReceivedEventArgs:**
```csharp
public class MessageReceivedEventArgs : EventArgs
{
    public Guid ConnectionId { get; set; }
    public INetworkMessage Message { get; set; }
}
```

**NetworkErrorEventArgs:**
```csharp
public class NetworkErrorEventArgs : EventArgs
{
    public Guid ConnectionId { get; set; }
    public Exception Exception { get; set; }
}
```

### DisconnectReason Enum (Geplant)

```csharp
public enum DisconnectReason : byte
{
    // Client-Initiated (1-9)
    ClientDisconnect = 1,      // Client hat normal getrennt
    ClientTimeout = 2,          // Client antwortet nicht mehr (Heartbeat Timeout)
    
    // Server-Initiated (10-19)
    ServerShutdown = 10,        // Server fährt herunter
    Kicked = 11,                // Von Admin gekickt
    Banned = 12,                // Gebannt
    
    // Error Cases (20-29)
    ProtocolError = 20,         // Ungültige Message
    AuthenticationFailed = 21,  // Login fehlgeschlagen
    DuplicateConnection = 22,   // Spieler bereits verbunden
    
    // Network Issues (30-39)
    ConnectionLost = 30,        // TCP-Verbindung verloren
    ReadError = 31,             // Fehler beim Lesen
    WriteError = 32,            // Fehler beim Schreiben
}
```

---

## Connection Flow

```
┌─────────────────────────────────────────────────────────────────┐
│                  CONNECTION FLOW                                 │
│                                                                  │
│  Client              NetworkServer         GameServer            │
│    │                      │                    │                 │
│    │──── TCP Connect ────▶│                    │                 │
│    │◄─── TLS Handshake ──▶│                    │                 │
│    │                      │                    │                 │
│    │                      │── ClientConnected ▶│                 │
│    │                      │    Event           │                 │
│    │                      │                    │                 │
│    │──── LoginRequest ───▶│                    │                 │
│    │                      │── MessageReceived ▶│                 │
│    │                      │                    │── Validate      │
│    │                      │                    │   (Auth)        │
│    │                      │◄── Send ───────────│                 │
│    │◄─── LoginResponse ───│                    │                 │
│    │     (Success,        │                    │                 │
│    │      PlayerId)       │                    │                 │
│    │                      │                    │                 │
│    │──── JoinZone ───────▶│                    │                 │
│    │                      │── MessageReceived ▶│                 │
│    │                      │                    │── Add Player    │
│    │                      │                    │   to Zone       │
│    │                      │◄── Broadcast ──────│                 │
│    │◄─── ZoneState ───────│                    │                 │
│    │     (All Entities)   │                    │                 │
│    │                      │                    │                 │
│    │◄════ Game Loop ═════▶│◄══════════════════▶│                 │
│    │   (PositionUpdate,   │    (25 Hz Tick)    │                 │
│    │    WorldState, etc.) │                    │                 │
│    │                      │                    │                 │
│    │──── Disconnect ─────▶│                    │                 │
│    │                      │── ClientDisconnect▶│                 │
│    │                      │    Event           │── Remove Player │
│    │                      │                    │   from Zone     │
│    │                      │◄── Broadcast ──────│                 │
│    │◄─── (TCP Close) ─────│                    │                 │
│    │                      │                    │                 │
└─────────────────────────────────────────────────────────────────┘
```

### Flow-Beschreibung

1. **TCP Connect:** Client stellt TCP-Verbindung her, optional mit TLS 1.3
2. **ClientConnected Event:** NetworkServer informiert GameServer über neue Verbindung
3. **Login:** Client sendet LoginRequest, Server validiert und sendet LoginResponse
4. **Zone Join:** Client joint eine Zone, erhält ZoneState mit allen Entities
5. **Game Loop:** Kontinuierlicher Austausch von Updates (25 Hz)
6. **Disconnect:** Client trennt, Server räumt auf und informiert andere Spieler

---

## Verwandte Dokumentation

- [Server-Komponenten](SERVER_COMPONENTS.md) - Gateway und Zone Server Details
- [Messages](MESSAGES.md) - Message Types und Serialisierung
- [Client-Server Sync](CLIENT_SERVER_SYNC.md) - Prediction und Interpolation

---

## 🔗 Nützliche Links

- [TCP/TLS in .NET](https://learn.microsoft.com/en-us/dotnet/fundamentals/networking/sockets/socket-services)
- [TLS 1.3 Specification](https://datatracker.ietf.org/doc/html/rfc8446)
- [MessagePack-CSharp](https://github.com/MessagePack-CSharp/MessagePack-CSharp)

---

*Teil der [Architektur-Dokumentation](README.md)*
