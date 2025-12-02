# 📨 Message-Spezifikation

## 2DMMO – Network Messages

**Version:** 1.1.0  
**Letzte Aktualisierung:** 2025-12-02  
**Teil von:** [Architektur-Dokumentation](../ARCHITECTURE.md)

---

## 📋 Übersicht

Diese Dokumentation beschreibt alle Netzwerk-Nachrichten, DTOs und die Serialisierung für das 2DMMO.

---

## Message Types

```csharp
public enum MessageType : byte
{
    // ═══════════════════════════════════════════════════
    // CONNECTION (1-9)
    // ═══════════════════════════════════════════════════
    LoginRequest = 1,
    LoginResponse = 2,
    Logout = 3,
    Heartbeat = 4,
    Disconnect = 5,
    
    // ═══════════════════════════════════════════════════
    // ZONE (10-19)
    // ═══════════════════════════════════════════════════
    JoinZone = 10,
    LeaveZone = 11,
    ZoneState = 12,
    PlayerJoinedZone = 13,
    PlayerLeftZone = 14,
    
    // ═══════════════════════════════════════════════════
    // MOVEMENT (20-29)
    // ═══════════════════════════════════════════════════
    PositionUpdate = 20,
    PositionBroadcast = 21,
    
    // ═══════════════════════════════════════════════════
    // COMBAT (30-39)
    // ═══════════════════════════════════════════════════
    ActionRequest = 30,
    ActionResult = 31,
    DamageEvent = 32,
    DeathEvent = 33,
    RespawnRequest = 34,
    RespawnResponse = 35,
    
    // ═══════════════════════════════════════════════════
    // CHAT (40-49)
    // ═══════════════════════════════════════════════════
    ChatMessage = 40,
    ChatBroadcast = 41,
    
    // ═══════════════════════════════════════════════════
    // INVENTORY (50-59)
    // ═══════════════════════════════════════════════════
    InventoryUpdate = 50,
    ItemPickup = 51,
    ItemDrop = 52,
    ItemUse = 53,
    
    // ═══════════════════════════════════════════════════
    // SOCIAL (60-69)
    // ═══════════════════════════════════════════════════
    GuildInvite = 60,
    GuildAccept = 61,
    GuildLeave = 62,
    PartyInvite = 63,
    PartyAccept = 64,
    PartyLeave = 65,
    
    // ═══════════════════════════════════════════════════
    // PVP (70-79)
    // ═══════════════════════════════════════════════════
    PvpFlagToggle = 70,
    PvpFlagStatus = 71,
}
```

---

## Core Messages (Shared Library)

### Connection Messages

```csharp
[MessagePackObject]
public class LoginRequest : INetworkMessage
{
    [IgnoreMember]
    public MessageType Type => MessageType.LoginRequest;
    
    [Key(0)]
    public string Email { get; set; } = string.Empty;
    
    [Key(1)]
    public string PasswordHash { get; set; } = string.Empty;  // Client-side hashed
    
    [Key(2)]
    public string ClientVersion { get; set; } = string.Empty;
}

[MessagePackObject]
public class LoginResponse : INetworkMessage
{
    [IgnoreMember]
    public MessageType Type => MessageType.LoginResponse;
    
    [Key(0)]
    public bool Success { get; set; }
    
    [Key(1)]
    public string? ErrorMessage { get; set; }
    
    [Key(2)]
    public Guid SessionId { get; set; }
    
    [Key(3)]
    public CharacterData[]? Characters { get; set; }
}
```

### Movement Messages

```csharp
[MessagePackObject]
public class PositionUpdate : INetworkMessage
{
    [IgnoreMember]
    public MessageType Type => MessageType.PositionUpdate;
    
    [Key(0)]
    public int PlayerId { get; set; }
    
    [Key(1)]
    public float X { get; set; }
    
    [Key(2)]
    public float Y { get; set; }
    
    [Key(3)]
    public float VelocityX { get; set; }
    
    [Key(4)]
    public float VelocityY { get; set; }
    
    [Key(5)]
    public long Timestamp { get; set; }
    
    [Key(6)]
    public int SequenceNumber { get; set; }
}

[MessagePackObject]
public class PositionBroadcast : INetworkMessage
{
    [IgnoreMember]
    public MessageType Type => MessageType.PositionBroadcast;
    
    [Key(0)]
    public PlayerPositionData[] Positions { get; set; } = Array.Empty<PlayerPositionData>();
    
    [Key(1)]
    public long ServerTimestamp { get; set; }
}

[MessagePackObject]
public class PlayerPositionData
{
    [Key(0)]
    public int PlayerId { get; set; }
    
    [Key(1)]
    public float X { get; set; }
    
    [Key(2)]
    public float Y { get; set; }
    
    [Key(3)]
    public float VelocityX { get; set; }
    
    [Key(4)]
    public float VelocityY { get; set; }
}
```

### Combat Messages

```csharp
[MessagePackObject]
public class ActionRequest : INetworkMessage
{
    [IgnoreMember]
    public MessageType Type => MessageType.ActionRequest;
    
    [Key(0)]
    public int PlayerId { get; set; }
    
    [Key(1)]
    public int AbilityId { get; set; }
    
    [Key(2)]
    public int TargetId { get; set; }
    
    [Key(3)]
    public float TargetX { get; set; }  // Für Ground-Targeted Abilities
    
    [Key(4)]
    public float TargetY { get; set; }
    
    [Key(5)]
    public long Timestamp { get; set; }
}

[MessagePackObject]
public class ActionResult : INetworkMessage
{
    [IgnoreMember]
    public MessageType Type => MessageType.ActionResult;
    
    [Key(0)]
    public int CasterId { get; set; }
    
    [Key(1)]
    public int AbilityId { get; set; }
    
    [Key(2)]
    public bool Success { get; set; }
    
    [Key(3)]
    public string? FailReason { get; set; }
    
    [Key(4)]
    public DamageEventData[]? DamageEvents { get; set; }
}

[MessagePackObject]
public class DamageEventData
{
    [Key(0)]
    public int TargetId { get; set; }
    
    [Key(1)]
    public int Damage { get; set; }
    
    [Key(2)]
    public bool IsCritical { get; set; }
    
    [Key(3)]
    public int RemainingHp { get; set; }
}
```

### Chat Messages

```csharp
[MessagePackObject]
public class ChatMessage : INetworkMessage
{
    [IgnoreMember]
    public MessageType Type => MessageType.ChatMessage;
    
    [Key(0)]
    public int SenderId { get; set; }
    
    [Key(1)]
    public string Content { get; set; } = string.Empty;
    
    [Key(2)]
    public ChatChannel Channel { get; set; }
    
    [Key(3)]
    public int? TargetPlayerId { get; set; }  // Für Whisper
}

public enum ChatChannel : byte
{
    Zone = 1,
    Global = 2,
    Party = 3,
    Guild = 4,
    Whisper = 5,
}
```

---

## Serialization Helper

```csharp
using MessagePack;
using System.Buffers;

namespace Mmo.Shared.Network;

public static class MessageSerializer
{
    /// <summary>
    /// Serialisiert eine Nachricht mit Type-Prefix und Length-Header
    /// </summary>
    public static byte[] Serialize<T>(T message) where T : INetworkMessage
    {
        // Payload serialisieren
        byte[] payload = MessagePackSerializer.Serialize(message);
        
        // Frame erstellen: [Type:1][Length:4][Payload:N]
        byte[] frame = new byte[1 + 4 + payload.Length];
        
        frame[0] = (byte)message.Type;
        BitConverter.TryWriteBytes(frame.AsSpan(1, 4), (uint)payload.Length);
        payload.CopyTo(frame.AsSpan(5));
        
        return frame;
    }
    
    /// <summary>
    /// Liest den Message-Header (Type + Length)
    /// </summary>
    public static (MessageType type, int length) ReadHeader(ReadOnlySpan<byte> data)
    {
        if (data.Length < 5)
            throw new ArgumentException("Not enough data for header");
            
        var type = (MessageType)data[0];
        var length = (int)BitConverter.ToUInt32(data.Slice(1, 4));
        
        return (type, length);
    }
    
    /// <summary>
    /// Deserialisiert eine Nachricht basierend auf dem Type
    /// </summary>
    public static INetworkMessage Deserialize(MessageType type, ReadOnlySpan<byte> payload)
    {
        byte[] payloadArray = payload.ToArray();
        
        return type switch
        {
            // Connection
            MessageType.LoginRequest => MessagePackSerializer.Deserialize<LoginRequest>(payloadArray),
            MessageType.LoginResponse => MessagePackSerializer.Deserialize<LoginResponse>(payloadArray),
            MessageType.Heartbeat => MessagePackSerializer.Deserialize<Heartbeat>(payloadArray),
            
            // Movement
            MessageType.PositionUpdate => MessagePackSerializer.Deserialize<PositionUpdate>(payloadArray),
            MessageType.PositionBroadcast => MessagePackSerializer.Deserialize<PositionBroadcast>(payloadArray),
            
            // Combat
            MessageType.ActionRequest => MessagePackSerializer.Deserialize<ActionRequest>(payloadArray),
            MessageType.ActionResult => MessagePackSerializer.Deserialize<ActionResult>(payloadArray),
            
            // Chat
            MessageType.ChatMessage => MessagePackSerializer.Deserialize<ChatMessage>(payloadArray),
            
            _ => throw new ArgumentException($"Unknown message type: {type}")
        };
    }
}
```

---

## Verwandte Dokumentation

- [Netzwerk-Protokoll](NETWORK_PROTOCOL.md) - Transport und Message Framing
- [Client-Server Sync](CLIENT_SERVER_SYNC.md) - Wie Messages verarbeitet werden
- [Sicherheit](SECURITY.md) - Input Validation

---

## 🔗 Nützliche Links

- [MessagePack-CSharp GitHub](https://github.com/MessagePack-CSharp/MessagePack-CSharp)
- [MessagePack Specification](https://msgpack.org/)

---

*Teil der [Architektur-Dokumentation](../ARCHITECTURE.md)*
