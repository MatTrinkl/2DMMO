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

## MessageBundle - Batching System

### Motivation

Das **MessageBundle-System** ermöglicht es dem Server, mehrere ausgehende Nachrichten pro Tick und Client zu bündeln. Dies reduziert:

- **TCP-Overhead**: Weniger Frame-Headers pro Nachricht
- **Syscalls**: Weniger `send()`-Aufrufe für kleine Payloads
- **Latenz**: Konsistentere State-Updates (alle Änderungen kommen zusammen)

### MessageBundle Frame-Format

Wenn mehrere Messages gebündelt werden, verwendet der Server einen `MessageBundle`-Container:

```
┌─────────────────────────────────────────────────────────────┐
│                    MESSAGE BUNDLE FRAME                      │
│                                                              │
│  ┌──────────┬───────────────────────────────────────────┐   │
│  │  4 Bytes │       N Bytes                             │   │
│  │  Length  │   MessageBundle Payload                   │   │
│  └──────────┴───────────────────────────────────────────┘   │
│                                                              │
│  MessageBundle Payload (MessagePack):                       │
│  {                                                           │
│    Type: 950,              // MessageType.MessageBundle     │
│    ServerTick: 12345,      // Current tick number           │
│    Timestamp: 1704398400,  // Server timestamp (ms)         │
│    Messages: [             // Array von pre-serialized msgs │
│      <bytes>,  // EntityUpdate (serialized)                 │
│      <bytes>,  // PositionBroadcast (serialized)            │
│      <bytes>,  // StatUpdate (serialized)                   │
│      ...                                                     │
│    ]                                                         │
│  }                                                           │
│                                                              │
│  Beispiel: 3 Messages in einem Bundle                       │
│  ┌────────────┬──────────────────────────────────────┐      │
│  │ 8C 01 00 00│ 95 D6 03 B6 ... (MessageBundle)      │      │
│  └────────────┴──────────────────────────────────────┘      │
│         │                    │                               │
│         │                    └─ MessagePack Payload         │
│         │                       [Type:950, Tick, Messages]  │
│         └─ 396 Bytes Bundle-Länge                           │
└─────────────────────────────────────────────────────────────┘
```

### Wann Bundling verwenden?

**✅ SOLLTE gebündelt werden:**

- `EntityUpdateBatch` (1405) - Entity-State-Updates
- `PositionBroadcast` (201) - Bewegungen anderer Spieler
- `StatUpdate` (602) - Character-Stat-Änderungen
- `BuffApplied` / `BuffRemoved` (1500/1501) - Buff-Events
- `ChatBroadcast` (401) - Chat-Nachrichten (nicht zeitkritisch)
- `EntitySpawnBatch` (1401) / `EntityDespawnBatch` (1403)

**❌ NIEMALS bündeln (sofort senden):**

- `ForceDisconnect` (5) - Muss sofort ankommen
- `MovementCorrection` (202) - Latenz-kritisch für Prediction
- `Pong` (901) - Für präzise Ping-Messung erforderlich
- `KickNotification` (912) - Muss vor Connection-Close ankommen
- `ServerShutdown` (914) - Kritische Server-Message

**🟡 OPTIONAL bündeln (je nach Latenz-Anforderung):**

- `DamageEvent` (302) - Combat-Feedback (möglichst schnell)
- `HealEvent` (304) - Combat-Feedback
- `TargetUpdate` (1202) - Target-Frame-Updates

### MessageBundle Struktur

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.MessageBundle)]
public class MessageBundle : IServerMessage
{
    [Key(0)]
    public MessageType Type => MessageType.MessageBundle;
    
    [Key(1)]
    public long ServerTick { get; set; }          // Current server tick number
    
    [Key(2)]
    public long Timestamp { get; set; }           // Server timestamp (ms)
    
    [Key(3)]
    public List<byte[]> Messages { get; set; }    // Pre-serialized sub-messages
}
```

### Client-Side Handling

```csharp
// Client empfängt MessageBundle und entpackt alle Sub-Messages:
public void HandleMessageBundle(MessageBundle bundle)
{
    foreach (var messageBytes in bundle.Messages)
    {
        // Deserialize each sub-message
        var message = MessageSerializer.Deserialize(messageBytes);
        
        // Process normally
        MessageRouter.Route(message);
    }
}
```

### Performance-Vorteile

**Beispiel: 10 Messages pro Tick ohne Bundling:**
```
10 Messages × (4 Bytes Length + ~50 Bytes Payload) = ~540 Bytes
10 TCP Frames = 10 Syscalls
```

**Mit Bundling:**
```
1 MessageBundle × (4 Bytes Length + 4 Bytes Type + 8 Bytes Tick + 8 Bytes Timestamp + 10×50 Bytes) = ~524 Bytes
1 TCP Frame = 1 Syscall
```

**Einsparung:**
- **Weniger Overhead**: ~16 Bytes gespart (durch gemeinsame Header)
- **90% weniger Syscalls**: 1 statt 10 `send()` Aufrufe
- **Konsistenz**: Alle Updates kommen im gleichen Frame an

### Implementierungs-Strategie

**Server-Side (pro Client, pro Tick):**

```csharp
// In OutputPhase des Game-Loops:
var messagesToSend = new List<IServerMessage>();

// Sammle alle Messages für diesen Client in diesem Tick
messagesToSend.Add(new PositionBroadcast { ... });
messagesToSend.Add(new StatUpdate { ... });
messagesToSend.Add(new BuffApplied { ... });

// Bundling-Logik:
if (messagesToSend.Count > 1)
{
    // Serialize alle Messages
    var serializedMessages = messagesToSend
        .Select(msg => MessageSerializer.Serialize(msg))
        .ToList();
    
    // Erstelle Bundle
    var bundle = new MessageBundle
    {
        ServerTick = currentTick,
        Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
        Messages = serializedMessages
    };
    
    // Sende Bundle statt einzelne Messages
    await clientConnection.SendAsync(bundle);
}
else if (messagesToSend.Count == 1)
{
    // Einzelne Message direkt senden (kein Bundle-Overhead)
    await clientConnection.SendAsync(messagesToSend[0]);
}
```

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
    ProtocolError = 20,         // Ungültige Message (z.B. Client hat unerlaubten MessageType gesendet)
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

## Server-Side Message Validation

### Whitelist-Konzept

Der Server verwendet eine **Whitelist** für erlaubte MessageTypes vom Client. Nur Messages mit `IClientMessage` Interface dürfen vom Client gesendet werden.

```csharp
// Beispiel: Whitelist-Validierung
public class MessageValidator
{
    private static readonly HashSet<MessageType> ClientAllowedTypes = new()
    {
        // Connection Messages
        MessageType.LoginRequest,
        MessageType.LogoutRequest,
        MessageType.Heartbeat,
        MessageType.ReconnectRequest,
        
        // Zone Messages
        MessageType.JoinZone,
        MessageType.LeaveZone,
        
        // Movement Messages
        MessageType.PositionUpdate,
        MessageType.TeleportRequest,
        
        // ... weitere Client→Server Messages
    };
    
    public static bool IsClientMessageAllowed(MessageType type)
    {
        return ClientAllowedTypes.Contains(type);
    }
    
    public static ValidationResult ValidateIncomingMessage(INetworkMessage message)
    {
        // 1. Prüfe ob MessageType vom Client erlaubt ist
        if (!IsClientMessageAllowed(message.Type))
        {
            return ValidationResult.Fail(
                DisconnectReason.ProtocolError,
                $"Client sent disallowed MessageType: {message.Type}"
            );
        }
        
        // 2. Prüfe ob Message IClientMessage implementiert
        if (message is not IClientMessage)
        {
            return ValidationResult.Fail(
                DisconnectReason.ProtocolError,
                "Message must implement IClientMessage"
            );
        }
        
        return ValidationResult.Success();
    }
}
```

### Automatische Whitelist-Generierung

Die Whitelist kann automatisch aus allen Types generiert werden, die `IClientMessage` implementieren:

```csharp
// Beim Server-Start: Scanne alle IClientMessage Types
var clientMessageTypes = Assembly.GetAssembly(typeof(IClientMessage))
    .GetTypes()
    .Where(t => typeof(IClientMessage).IsAssignableFrom(t) && !t.IsInterface)
    .Select(t => GetMessageType(t))  // Extract MessageType from [NetworkMessage] attribute
    .ToHashSet();
```

### Validierungs-Pipeline

```
Client Message ──► Deserialize ──► Whitelist-Check ──► Interface-Check ──► Handler
                        │                 │                   │
                        │                 │                   │
                        ▼                 ▼                   ▼
                   Length Check    ClientAllowedTypes   IClientMessage?
                   Type Valid           .Contains()
                        │                 │                   │
                        │                 │                   │
                        └─────────────────┴───────────────────┘
                                          │
                                          ▼
                                   ProtocolError = Disconnect
```

### Error Handling

Wenn ein Client eine nicht-erlaubte Message sendet:

1. **Log**: Security-Warning mit PlayerId, IP, MessageType
2. **Disconnect**: Client wird mit `DisconnectReason.ProtocolError` getrennt
3. **Metrics**: Zähler für Sicherheitsverstöße inkrementieren
4. **Optional**: Bei wiederholten Verstößen → IP-Ban

**Wichtig**: Server sendet **KEINE** Error-Response zum Client zurück. Der Client wird sofort getrennt, um weitere Angriffe zu verhindern.

Weitere Details: [MESSAGE_SECURITY.md](MESSAGE_SECURITY.md) | [Sicherheit](SECURITY.md)

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
