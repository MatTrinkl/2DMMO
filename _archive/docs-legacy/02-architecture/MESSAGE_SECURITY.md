# 🔒 Message Security

## 2DMMO – Sichere Trennung und Validierung von Netzwerk-Nachrichten

**Version:** 1.0.0  
**Letzte Aktualisierung:** 2025-12-23  
**Teil von:** [Architektur-Dokumentation](README.md)  
**Siehe auch:** [Issue #177 - Sichere Trennung und Validierung von Netzwerk-Nachrichten](https://github.com/MatTrinkl/2DMMO/issues/177)

---

## 📋 Übersicht

Dieses Dokument beschreibt die **Sicherheitsarchitektur** des Message-Systems im 2DMMO, insbesondere die **strikte Richtungstrennung** zwischen Client→Server und Server→Client Messages.

**Kernprinzip**: Es gibt **KEINE bidirektionalen Messages**. Jede Message hat exakt EINE Richtung.

---

## 🚨 Warum strikte Richtungstrennung?

### Das Problem mit bidirektionalen Messages

Wenn Messages als "bidirektional" dokumentiert werden (z.B. `Heartbeat`, `Ping`), entsteht ein **kritisches Sicherheitsrisiko**:

1. **Server könnte theoretisch JEDE Message vom Client akzeptieren**
2. **Client könnte Server-Messages senden** (Message-Spoofing)
3. **Keine klare Validierungs-Logik** - welche MessageTypes sind erlaubt?
4. **Code-Reviews sind schwieriger** - Richtung ist nicht aus dem Code ersichtlich

### Beispiel: Heartbeat-Angriff

**Unsicher** (bidirektional):
```csharp
// ❌ Client könnte Server-Messages senden!
if (message.Type == MessageType.Heartbeat)
{
    // Ist das vom Client ODER vom Server?
    // Welche Validierung ist nötig?
    HandleHeartbeat(message);
}
```

**Sicher** (strikte Trennung):
```csharp
// ✅ Client sendet nur Heartbeat, Server antwortet mit Pong
if (message is IClientMessage clientMsg)
{
    switch (clientMsg.Type)
    {
        case MessageType.Heartbeat:
            // Klar: Client → Server
            HandleClientHeartbeat(clientMsg);
            SendPong(clientMsg.Timestamp, clientMsg.SequenceNumber);
            break;
    }
}
```

### Sicherheitsvorteile

✅ **Whitelist-Validierung**: Server akzeptiert nur `IClientMessage` Types  
✅ **Verhindert Message-Spoofing**: Client kann keine `IServerMessage` senden  
✅ **Klare Verantwortlichkeiten**: Interface zeigt Richtung  
✅ **Einfachere Code-Reviews**: Richtung ist aus Interface ersichtlich  
✅ **Compile-Time Safety**: Falsche Richtung = Compiler-Fehler  

---

## 🏗️ Interface-Hierarchie

### Übersicht

```
INetworkMessage (Basis-Interface)
│
├── IClientMessage : INetworkMessage
│   │   → Client → Server Messages
│   │   → Requests, Input, Commands
│   │
│   └── ITimestampedClientMessage : IClientMessage, ITimestampedMessage
│       → Timestamped Client Messages (z.B. PositionUpdate)
│
└── IServerMessage : INetworkMessage
    │   → Server → Client Messages
    │   → Responses, State Updates, Broadcasts
    │
    └── ITimestampedServerMessage : IServerMessage, ITimestampedMessage
        → Timestamped Server Messages (z.B. PositionBroadcast)
```

### Interface-Definitionen

```csharp
namespace Mmo.Shared.Messaging.Interfaces;

/// <summary>
/// Basis-Interface für alle Netzwerk-Nachrichten
/// </summary>
public interface INetworkMessage
{
    MessageType Type { get; }
}

/// <summary>
/// Marker-Interface für Messages MIT Timestamp
/// </summary>
public interface ITimestampedMessage
{
    long Timestamp { get; }
}

/// <summary>
/// Client → Server Messages
/// NUR diese Messages dürfen vom Client gesendet werden
/// </summary>
public interface IClientMessage : INetworkMessage
{
}

/// <summary>
/// Server → Client Messages
/// NUR diese Messages dürfen vom Server gesendet werden
/// </summary>
public interface IServerMessage : INetworkMessage
{
}

/// <summary>
/// Timestamped Client Messages (Client → Server)
/// </summary>
public interface ITimestampedClientMessage : IClientMessage, ITimestampedMessage
{
}

/// <summary>
/// Timestamped Server Messages (Server → Client)
/// </summary>
public interface ITimestampedServerMessage : IServerMessage, ITimestampedMessage
{
}
```

### ITimestampedMessage - Wichtiger Hinweis

**`ITimestampedMessage` ist ein Marker-Interface** und wird **NUR** in Kombination mit `IClientMessage` oder `IServerMessage` verwendet:

✅ **Korrekt**:
```csharp
public class PositionUpdate : ITimestampedClientMessage { ... }
public class PositionBroadcast : ITimestampedServerMessage { ... }
```

❌ **FALSCH**:
```csharp
// ❌ Niemals nur ITimestampedMessage alleine!
public class SomeMessage : ITimestampedMessage { ... }

// ❌ Niemals INetworkMessage + ITimestampedMessage!
public class SomeMessage : INetworkMessage, ITimestampedMessage { ... }
```

---

## 🛡️ Server-Whitelist Pattern

### Konzept

Der Server verwendet eine **Whitelist** für erlaubte MessageTypes vom Client. Nur Messages mit `IClientMessage` Interface dürfen vom Client gesendet werden.

```
┌─────────────────────────────────────────────────────────┐
│            SERVER MESSAGE VALIDATION                     │
│                                                          │
│  Client Message ──► Deserialize ──► Whitelist-Check     │
│                          │                │              │
│                          ▼                ▼              │
│                     Length OK?      IClientMessage?      │
│                     Type Valid?     Type in Whitelist?   │
│                          │                │              │
│                          ├────────────────┤              │
│                          │                │              │
│                          ▼                ▼              │
│                     ✅ VALID         ❌ INVALID          │
│                          │                │              │
│                          ▼                ▼              │
│                    Process         Disconnect            │
│                    Message      (ProtocolError)          │
│                                                          │
└─────────────────────────────────────────────────────────┘
```

### Implementierungs-Beispiel

```csharp
namespace Mmo.Server.Networking.Validation;

/// <summary>
/// Validiert eingehende Messages vom Client
/// </summary>
public class MessageValidator
{
    private readonly ILogger<MessageValidator> _logger;
    private static readonly HashSet<MessageType> _clientAllowedTypes;
    
    static MessageValidator()
    {
        // Automatische Whitelist-Generierung beim Start
        _clientAllowedTypes = ScanClientMessageTypes();
    }
    
    /// <summary>
    /// Scannt alle Types mit IClientMessage Interface
    /// </summary>
    private static HashSet<MessageType> ScanClientMessageTypes()
    {
        return Assembly.GetAssembly(typeof(IClientMessage))
            .GetTypes()
            .Where(t => typeof(IClientMessage).IsAssignableFrom(t) 
                     && !t.IsInterface 
                     && !t.IsAbstract)
            .Select(t => GetMessageTypeFromAttribute(t))
            .Where(type => type.HasValue)
            .Select(type => type.Value)
            .ToHashSet();
    }
    
    /// <summary>
    /// Extrahiert MessageType aus [NetworkMessage] Attribut
    /// </summary>
    private static MessageType? GetMessageTypeFromAttribute(Type type)
    {
        var attribute = type.GetCustomAttribute<NetworkMessageAttribute>();
        return attribute?.MessageType;
    }
    
    /// <summary>
    /// Validiert eingehende Message vom Client
    /// </summary>
    public ValidationResult ValidateIncomingMessage(
        INetworkMessage message, 
        Guid connectionId)
    {
        // 1. Prüfe ob Message IClientMessage implementiert
        if (message is not IClientMessage)
        {
            _logger.LogWarning(
                "Security Violation: Connection {ConnectionId} sent non-client message {MessageType}",
                connectionId,
                message.Type
            );
            
            return ValidationResult.Fail(
                DisconnectReason.ProtocolError,
                "Message must implement IClientMessage"
            );
        }
        
        // 2. Prüfe ob MessageType in Whitelist ist
        if (!_clientAllowedTypes.Contains(message.Type))
        {
            _logger.LogWarning(
                "Security Violation: Connection {ConnectionId} sent disallowed MessageType {MessageType}",
                connectionId,
                message.Type
            );
            
            return ValidationResult.Fail(
                DisconnectReason.ProtocolError,
                $"MessageType {message.Type} not allowed from client"
            );
        }
        
        // 3. Weitere Validierungen (z.B. Session, Permissions, etc.)
        // ...
        
        return ValidationResult.Success();
    }
}

/// <summary>
/// Validation-Result
/// </summary>
public class ValidationResult
{
    public bool IsValid { get; init; }
    public DisconnectReason? Reason { get; init; }
    public string? ErrorMessage { get; init; }
    
    public static ValidationResult Success() => new() { IsValid = true };
    
    public static ValidationResult Fail(DisconnectReason reason, string message) 
        => new() 
        { 
            IsValid = false, 
            Reason = reason, 
            ErrorMessage = message 
        };
}
```

### Verwendung im NetworkServer

```csharp
public class NetworkServer
{
    private readonly MessageValidator _validator;
    
    private async Task HandleIncomingMessage(
        Guid connectionId, 
        INetworkMessage message)
    {
        // Validiere Message
        var validationResult = _validator.ValidateIncomingMessage(
            message, 
            connectionId
        );
        
        if (!validationResult.IsValid)
        {
            // Disconnect Client bei Security-Violation
            await DisconnectClient(
                connectionId, 
                validationResult.Reason!.Value, 
                validationResult.ErrorMessage!
            );
            
            // Security-Metrik inkrementieren
            _metrics.IncrementSecurityViolations(message.Type);
            
            return;
        }
        
        // Message ist valide → weiterleiten an Handler
        await _messageDispatcher.DispatchMessage(connectionId, message);
    }
}
```

---

## ✅ Validierungs-Best-Practices

### 1. Defense in Depth

Mehrere Validierungs-Schichten:

```csharp
public async Task<ValidationResult> ValidateMessage(INetworkMessage message)
{
    // Layer 1: Interface-Check
    if (message is not IClientMessage)
        return Fail("Not a client message");
    
    // Layer 2: Whitelist-Check
    if (!IsWhitelisted(message.Type))
        return Fail("MessageType not whitelisted");
    
    // Layer 3: Session-Check
    if (!HasValidSession(connectionId))
        return Fail("No valid session");
    
    // Layer 4: Permission-Check
    if (!HasPermission(message.Type))
        return Fail("No permission for this action");
    
    // Layer 5: Rate-Limiting
    if (IsRateLimited(connectionId, message.Type))
        return Fail("Rate limit exceeded");
    
    // Layer 6: Payload-Validation
    if (!ValidatePayload(message))
        return Fail("Invalid payload");
    
    return Success();
}
```

### 2. Fail-Fast Prinzip

Bei **JEDEM** Security-Fehler → **sofort disconnecten**:

```csharp
// ❌ FALSCH: Error-Response senden
await SendError(connectionId, "Invalid message type");

// ✅ RICHTIG: Sofort disconnecten
await Disconnect(connectionId, DisconnectReason.ProtocolError);
```

**Warum?** Error-Responses geben Angreifern Informationen über das System.

### 3. Security-Logging

Alle Security-Violations loggen:

```csharp
_logger.LogWarning(
    "Security Violation: {Type} from {ConnectionId} ({IP}): {Reason}",
    violationType,
    connectionId,
    clientIp,
    reason
);

// Optional: Bei wiederholten Verstößen → IP-Ban
_securityMonitor.RecordViolation(clientIp, violationType);
```

### 4. Metrics & Monitoring

Security-Metriken tracken:

```csharp
_metrics.IncrementSecurityViolations(
    violationType: "invalid_message_type",
    messageType: message.Type.ToString(),
    severity: "high"
);
```

---

## 📐 Beispiele: Korrekte Interface-Wahl

### Beispiel 1: Ping/Pong

**Falsch** ❌: Eine bidirektionale "Ping" Message

```csharp
// ❌ Bidirektional - NICHT erlaubt!
[MessagePackObject]
public class Ping : INetworkMessage
{
    [Key(0)]
    public MessageType Type => MessageType.Ping;
    
    [Key(1)]
    public long Timestamp { get; set; }
}
```

**Richtig** ✅: Zwei separate Messages

```csharp
// ✅ Client → Server
[MessagePackObject]
[NetworkMessage(MessageType.Ping)]
public class PingRequest : ITimestampedClientMessage
{
    [Key(0)]
    public MessageType Type => MessageType.Ping;
    
    [Key(1)]
    public long Timestamp { get; set; }
    
    [Key(2)]
    public uint SequenceNumber { get; set; }
}

// ✅ Server → Client
[MessagePackObject]
[NetworkMessage(MessageType.Pong)]
public class PongResponse : ITimestampedServerMessage
{
    [Key(0)]
    public MessageType Type => MessageType.Pong;
    
    [Key(1)]
    public long Timestamp { get; set; }
    
    [Key(2)]
    public uint SequenceNumber { get; set; }
}
```

### Beispiel 2: Heartbeat

**Falsch** ❌: Bidirektionale "Heartbeat" Message

```csharp
// ❌ Bidirektional - NICHT erlaubt!
[MessagePackObject]
public class Heartbeat : INetworkMessage
{
    [Key(0)]
    public MessageType Type => MessageType.Heartbeat;
    
    [Key(1)]
    public long Timestamp { get; set; }
}
```

**Richtig** ✅: Client sendet Heartbeat, Server antwortet mit Pong

```csharp
// ✅ Client → Server
[MessagePackObject]
[NetworkMessage(MessageType.Heartbeat)]
public class HeartbeatRequest : ITimestampedClientMessage
{
    [Key(0)]
    public MessageType Type => MessageType.Heartbeat;
    
    [Key(1)]
    public long Timestamp { get; set; }
    
    [Key(2)]
    public uint SequenceNumber { get; set; }
}

// Server antwortet mit existierendem Pong (901)
// ODER dediziertem HeartbeatAck (falls gewünscht)
```

### Beispiel 3: Position Updates

**Richtig** ✅: Separate Messages für Client Input und Server Broadcast

```csharp
// ✅ Client → Server (Input)
[MessagePackObject]
[NetworkMessage(MessageType.PositionUpdate)]
public class PositionUpdate : ITimestampedClientMessage
{
    [Key(0)]
    public MessageType Type => MessageType.PositionUpdate;
    
    [Key(1)]
    public long Timestamp { get; set; }
    
    [Key(2)]
    public float X { get; set; }
    
    [Key(3)]
    public float Y { get; set; }
    
    [Key(4)]
    public uint SequenceNumber { get; set; }
}

// ✅ Server → Client (Broadcast für andere Spieler)
[MessagePackObject]
[NetworkMessage(MessageType.PositionBroadcast)]
public class PositionBroadcast : ITimestampedServerMessage
{
    [Key(0)]
    public MessageType Type => MessageType.PositionBroadcast;
    
    [Key(1)]
    public long Timestamp { get; set; }
    
    [Key(2)]
    public int PlayerId { get; set; }
    
    [Key(3)]
    public float X { get; set; }
    
    [Key(4)]
    public float Y { get; set; }
}
```

### Beispiel 4: Chat Messages

**Richtig** ✅: Separate Messages für Send und Broadcast

```csharp
// ✅ Client → Server (Senden)
[MessagePackObject]
[NetworkMessage(MessageType.ChatMessage)]
public class ChatMessageSend : IClientMessage
{
    [Key(0)]
    public MessageType Type => MessageType.ChatMessage;
    
    [Key(1)]
    public string Message { get; set; }
    
    [Key(2)]
    public string ChannelType { get; set; }  // "zone", "party", "guild", etc.
}

// ✅ Server → Client (Empfangen)
[MessagePackObject]
[NetworkMessage(MessageType.ChatBroadcast)]
public class ChatMessageBroadcast : IServerMessage
{
    [Key(0)]
    public MessageType Type => MessageType.ChatBroadcast;
    
    [Key(1)]
    public int PlayerId { get; set; }
    
    [Key(2)]
    public string PlayerName { get; set; }
    
    [Key(3)]
    public string Message { get; set; }
    
    [Key(4)]
    public string ChannelType { get; set; }
    
    [Key(5)]
    public long Timestamp { get; set; }
}
```

---

## 🚫 Error-Handling bei ungültigen Messages

### Disconnect-Reasons

```csharp
public enum DisconnectReason : byte
{
    // ... andere Reasons ...
    
    /// <summary>
    /// Client hat ungültige Message gesendet
    /// Beispiele:
    /// - IServerMessage vom Client
    /// - MessageType nicht in Whitelist
    /// - Kein IClientMessage Interface
    /// </summary>
    ProtocolError = 20,
}
```

### Error-Handling Flow

```
Client sendet ungültige Message
        │
        ▼
Server validiert Message
        │
        ├─► ✅ Valid → Process Message
        │
        └─► ❌ Invalid
                │
                ├─► Log Security-Warning
                │
                ├─► Increment Security-Metrics
                │
                ├─► Disconnect Client (ProtocolError)
                │
                └─► Optional: Bei wiederholten Verstößen → IP-Ban
```

### Implementierung

```csharp
public async Task HandleInvalidMessage(
    Guid connectionId, 
    INetworkMessage message,
    string reason)
{
    // 1. Security-Log
    _logger.LogWarning(
        "Security Violation from {ConnectionId}: {Reason}. MessageType: {MessageType}",
        connectionId,
        reason,
        message.Type
    );
    
    // 2. Metrics
    _metrics.IncrementSecurityViolations(
        connectionId: connectionId,
        messageType: message.Type,
        reason: reason
    );
    
    // 3. Disconnect (KEINE Error-Response!)
    await DisconnectClient(
        connectionId,
        DisconnectReason.ProtocolError,
        reason
    );
    
    // 4. Optional: Ban bei wiederholten Verstößen
    if (_securityMonitor.ShouldBan(connectionId))
    {
        var clientIp = GetClientIp(connectionId);
        await _banManager.BanIp(clientIp, TimeSpan.FromHours(24), reason);
    }
}
```

---

## 📊 Zusammenfassung

### Do's ✅

- **IMMER** korrektes Interface verwenden (`IClientMessage` oder `IServerMessage`)
- **IMMER** Whitelist-Validierung im Server
- **IMMER** sofort disconnecten bei Security-Violations
- **IMMER** Security-Violations loggen
- Ping/Pong als **zwei separate Messages** implementieren
- Heartbeat vom Client, Pong vom Server
- Chat Send (Client→Server) und Chat Broadcast (Server→Client) trennen

### Don'ts ❌

- **NIEMALS** bidirektionale Messages verwenden
- **NIEMALS** nur `INetworkMessage` oder `ITimestampedMessage` alleine
- **NIEMALS** Server-Messages vom Client akzeptieren
- **NIEMALS** Error-Responses bei Security-Violations senden
- **NIEMALS** Interface-Checks weglassen

### Checkliste für neue Messages

- [ ] Korrekte Richtung bestimmt (Client→Server ODER Server→Client)?
- [ ] Korrektes Interface gewählt (`IClientMessage` oder `IServerMessage`)?
- [ ] Bei Timestamps: `ITimestampedClientMessage` oder `ITimestampedServerMessage`?
- [ ] `[NetworkMessage(MessageType.XXX)]` Attribut vorhanden?
- [ ] Dokumentation enthält **KEINE** "🔄 Bidirektional" Markierung?
- [ ] Whitelist wird automatisch generiert oder manuell gepflegt?

---

## 🔗 Verwandte Dokumentation

- [Message-Spezifikation](MESSAGES.md) - Technische Details zum Message-System
- [Netzwerk-Protokoll](NETWORK_PROTOCOL.md) - Transport und Framing
- [Sicherheit](SECURITY.md) - Allgemeine Security-Architektur
- [Message-Referenz](../03-messages/README.md) - Detaillierte Message-Dokumentation
- [Issue #177](https://github.com/MatTrinkl/2DMMO/issues/177) - Ursprüngliches Issue

---

**Letzte Aktualisierung**: 2025-12-23  
**Version**: 1.0.0  
**Maintainer**: 2DMMO Team

*Teil der [Architektur-Dokumentation](README.md)*
