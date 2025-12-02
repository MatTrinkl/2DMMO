# 🏗️ Architektur-Dokumentation

## 2DMMO – Technische Architektur

**Version:** 1.0.0  
**Letzte Aktualisierung:** 2025-12-02  
**Status:** Finalisiert für Prototyp-Phase

---

## 📋 Inhaltsverzeichnis

1. [Übersicht](#1-übersicht)
2. [Server-Komponenten](#2-server-komponenten)
3. [Netzwerk-Protokoll](#3-netzwerk-protokoll)
4. [Message-Spezifikation](#4-message-spezifikation)
5. [Game Loop Design](#5-game-loop-design)
6. [Client-Server Kommunikation](#6-client-server-kommunikation)
7. [Redis-Strategie](#7-redis-strategie)
8. [Datenbank-Strategie](#8-datenbank-strategie)
9. [Sicherheit](#9-sicherheit)
10. [Azure Deployment](#10-azure-deployment)
11. [Skalierung](#11-skalierung)

---

## 1. Übersicht

### 1.1 Technologie-Stack

| Komponente | Technologie | Version |
|-----------|-------------|---------|
| **Game Client** | Godot Engine (.NET) | 4.3 |
| **Programmiersprache** | C# | 14 |
| **Server Runtime** | .NET | 10 |
| **Transport** | TCP + TLS | - |
| **Serialisierung** | MessagePack | Latest |
| **Cache** | Redis | 7+ |
| **Datenbank** | PostgreSQL | 16+ |
| **Cloud** | Microsoft Azure | - |

### 1.2 Architektur-Diagramm

```
┌────────────────────────────────────────────────────────────────────────────┐
│                              AZURE CLOUD                                    │
│                                                                             │
│  ┌───────────────────────────────────────────────────────────────────────┐ │
│  │                         GATEWAY LAYER                                  │ │
│  │                                                                        │ │
│  │    ┌──────────────┐         ┌──────────────┐         ┌──────────────┐ │ │
│  │    │   Gateway    │         │   Gateway    │         │   Gateway    │ │ │
│  │    │   Server 1   │         │   Server 2   │         │   Server N   │ │ │
│  │    │   (TCP+TLS)  │         │   (TCP+TLS)  │         │   (TCP+TLS)  │ │ │
│  │    └──────┬───────┘         └──────┬───────┘         └──────┬───────┘ │ │
│  │           │                        │                        │         │ │
│  └───────────┼────────────────────────┼────────────────────────┼─────────┘ │
│              │                        │                        │           │
│              └────────────────────────┼────────────────────────┘           │
│                                       │                                     │
│  ┌────────────────────────────────────┼────────────────────────────────┐   │
│  │                         REDIS CLUSTER                                │   │
│  │    ┌─────────────────────────────────────────────────────────────┐  │   │
│  │    │  • Session Store      • Zone Player Lists                   │  │   │
│  │    │  • Player Cache       • Pub/Sub (Cross-Zone Events)         │  │   │
│  │    │  • Zone Registry      • Rate Limiting                       │  │   │
│  │    └─────────────────────────────────────────────────────────────┘  │   │
│  └────────────────────────────────────┬────────────────────────────────┘   │
│                                       │                                     │
│              ┌────────────────────────┼────────────────────────┐           │
│              │                        │                        │           │
│  ┌───────────┼────────────────────────┼────────────────────────┼─────────┐ │
│  │           │       ZONE SERVER LAYER                         │         │ │
│  │  ┌────────┴─────┐  ┌───────┴────────┐  ┌────────┴─────┐              │ │
│  │  │ Zone Server  │  │  Zone Server   │  │ Zone Server  │              │ │
│  │  │              │  │                │  │              │              │ │
│  │  │  Startzone   │  │   Hauptstadt   │  │    Wald      │    ...       │ │
│  │  │              │  │                │  │              │              │ │
│  │  │ ┌─────────┐  │  │  ┌─────────┐   │  │ ┌─────────┐  │              │ │
│  │  │ │ Shard 1 │  │  │  │ Shard 1 │   │  │ │ Shard 1 │  │              │ │
│  │  │ │ Shard 2 │  │  │  │ Shard 2 │   │  │ │ Shard 2 │  │              │ │
│  │  │ │ Shard 3 │  │  │  └─────────┘   │  │ └─────────┘  │              │ │
│  │  │ └─────────┘  │  │                │  │ └─────────┘  │              │ │
│  │  └──────────────┘  └────────────────┘  └──────────────┘              │ │
│  └───────────────────────────────────────────────────────────────────────┘ │
│                                       │                                     │
│                          ┌────────────┴────────────┐                       │
│                          │      PostgreSQL         │                       │
│                          │   (Persistente Daten)   │                       │
│                          └─────────────────────────┘                       │
└────────────────────────────────────────────────────────────────────────────┘
```

---

## 2. Server-Komponenten

### 2.1 Gateway Server

**Verantwortung:** Erste Anlaufstelle für alle Client-Verbindungen.

```
┌─────────────────────────────────────────────────────────┐
│                    GATEWAY SERVER                        │
│                                                          │
│  Aufgaben:                                              │
│  ├─ TCP+TLS Verbindungen akzeptieren                   │
│  ├─ Authentication (Login-Validierung)                 │
│  ├─ Session-Erstellung in Redis                        │
│  ├─ Routing zu richtigem Zone Server                   │
│  ├─ Connection Health Monitoring                       │
│  └─ Rate Limiting (DoS-Schutz)                         │
│                                                          │
│  Skalierung: Horizontal (mehrere Gateway Instanzen)    │
│  Stateless: Ja (alle Daten in Redis)                   │
└─────────────────────────────────────────────────────────┘
```

### 2.2 Zone Server

**Verantwortung:** Verwaltet eine oder mehrere Zonen der Spielwelt.

```
┌─────────────────────────────────────────────────────────┐
│                     ZONE SERVER                          │
│                                                          │
│  Aufgaben:                                              │
│  ├─ Game Loop (30 Hz)                                  │
│  ├─ Spieler-Bewegung validieren                        │
│  ├─ Kampf-Logik                                        │
│  ├─ NPC/Monster AI                                     │
│  ├─ Loot & Drops                                       │
│  ├─ Position-Broadcasting                              │
│  └─ Zone-Chat                                          │
│                                                          │
│  Skalierung: Pro Zone, mit Sharding bei hoher Last     │
│  Stateful: Ja (Zone-State im Memory + Redis Sync)      │
└─────────────────────────────────────────────────────────┘
```

### 2.3 Komponenten-Kommunikation

```
┌─────────────────────────────────────────────────────────┐
│              KOMPONENTEN-KOMMUNIKATION                   │
│                                                          │
│  Client ──TCP+TLS──► Gateway                            │
│                         │                                │
│                         ▼                                │
│  Gateway ──Redis Pub/Sub──► Zone Server                 │
│                                                          │
│  Zone A ──Redis Pub/Sub──► Zone B  (Cross-Zone Events) │
│                                                          │
│  Zone Server ──SQL──► PostgreSQL (Persistenz)           │
│                                                          │
│  Alle Server ◄──► Redis (Session, Cache, Pub/Sub)       │
└─────────────────────────────────────────────────────────┘
```

---

## 3. Netzwerk-Protokoll

### 3.1 Transport Layer

| Eigenschaft | Wert |
|-------------|------|
| **Protokoll** | TCP |
| **Verschlüsselung** | TLS 1.3 |
| **Port** | 7777 (konfigurierbar) |
| **Encoding** | MessagePack (Binary) |

### 3.2 Message Framing

Jede Nachricht über TCP hat folgendes Format:

```
┌─────────────────────────────────────────────────────────┐
│                    MESSAGE FRAME                         │
│                                                          │
│  ┌──────────┬──────────┬─────────────────────────────┐  │
│  │  1 Byte  │  4 Bytes │       N Bytes               │  │
│  │   Type   │  Length  │       Payload               │  │
│  │          │ (uint32) │   (MessagePack Data)        │  │
│  └──────────┴──────────┴─────────────────────────────┘  │
│                                                          │
│  Type:    Message-Typ (siehe MessageType enum)          │
│  Length:  Länge des Payloads in Bytes (Little-Endian)   │
│  Payload: MessagePack-serialisierte Daten               │
│                                                          │
│  Beispiel: PositionUpdate                               │
│  ┌────┬────────────┬────────────────────────────────┐   │
│  │ 10 │ 17 00 00 00│ 94 CD 30 39 CA 43 16 80 00 ... │   │
│  └────┴────────────┴────────────────────────────────┘   │
│    │        │                    │                       │
│    │        │                    └─ MessagePack Payload  │
│    │        └─ 17 Bytes Payload-Länge                   │
│    └─ Type 10 = PositionUpdate                          │
└─────────────────────────────────────────────────────────┘
```

### 3.3 Connection Flow

```
┌─────────────────────────────────────────────────────────┐
│                  CONNECTION FLOW                         │
│                                                          │
│  Client                    Gateway           Zone Server │
│    │                          │                    │     │
│    │──── TCP Connect ────────►│                    │     │
│    │◄─── TLS Handshake ──────►│                    │     │
│    │                          │                    │     │
│    │──── LoginRequest ───────►│                    │     │
│    │                          │── Validate ───────►│     │
│    │                          │   (Redis/DB)       │     │
│    │                          │◄── Session ────────│     │
│    │◄─── LoginResponse ───────│                    │     │
│    │     (SessionId, Zone)    │                    │     │
│    │                          │                    │     │
│    │──── JoinZone ───────────►│                    │     │
│    │                          │── RegisterPlayer ─►│     │
│    │                          │◄── Ack ───────────│     │
│    │◄─── ZoneState ───────────│◄───────────────────│     │
│    │     (Players, NPCs)      │                    │     │
│    │                          │                    │     │
│    │◄════ Game Loop ═════════►│◄══════════════════►│     │
│    │                          │                    │     │
└─────────────────────────────────────────────────────────┘
```

---

## 4. Message-Spezifikation

### 4.1 Message Types

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

### 4.2 Core Messages (Shared Library)

```csharp
// ═══════════════════════════════════════════════════════════
// CONNECTION MESSAGES
// ═══════════════════════════════════════════════════════════

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

// ═══════════════════════════════════════════════════════════
// MOVEMENT MESSAGES
// ═══════════════════════════════════════════════════════════

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

// ═══════════════════════════════════════════════════════════
// COMBAT MESSAGES
// ═══════════════════════════════════════════════════════════

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

// ═══════════════════════════════════════════════════════════
// CHAT MESSAGES
// ═══════════════════════════════════════════════════════════

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

### 4.3 Serialization Helper

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

## 5. Game Loop Design

### 5.1 Server Game Loop (30 Hz)

```
┌─────────────────────────────────────────────────────────┐
│                  SERVER GAME LOOP                        │
│                   (33.33ms pro Tick)                    │
│                                                          │
│  ┌────────────────────────────────────────────────────┐ │
│  │                    TICK START                       │ │
│  │                        │                            │ │
│  │  ┌─────────────────────▼─────────────────────────┐ │ │
│  │  │  1. INPUT PHASE                                │ │ │
│  │  │     • Alle Messages aus Input-Queue lesen     │ │ │
│  │  │     • Nach Timestamp sortieren                │ │ │
│  │  │     • Duplikate entfernen                     │ │ │
│  │  └─────────────────────┬─────────────────────────┘ │ │
│  │                        │                            │ │
│  │  ┌─────────────────────▼─────────────────────────┐ │ │
│  │  │  2. VALIDATION PHASE                           │ │ │
│  │  │     • Movement validieren (Speed-Check)       │ │ │
│  │  │     • Action validieren (Range, Cooldown)     │ │ │
│  │  │     • Cheater-Detection                       │ │ │
│  │  └─────────────────────┬─────────────────────────┘ │ │
│  │                        │                            │ │
│  │  ┌─────────────────────▼─────────────────────────┐ │ │
│  │  │  3. SIMULATION PHASE                           │ │ │
│  │  │     • Positionen updaten                      │ │ │
│  │  │     • Kampf-Berechnungen                      │ │ │
│  │  │     • AI/NPC Updates                          │ │ │
│  │  │     • Respawn-Checks                          │ │ │
│  │  └─────────────────────┬─────────────────────────┘ │ │
│  │                        │                            │ │
│  │  ┌─────────────────────▼─────────────────────────┐ │ │
│  │  │  4. BROADCAST PHASE                            │ │ │
│  │  │     • Position-Broadcasts sammeln             │ │ │
│  │  │     • Event-Broadcasts sammeln                │ │ │
│  │  │     • An relevante Clients senden             │ │ │
│  │  └─────────────────────┬─────────────────────────┘ │ │
│  │                        │                            │ │
│  │  ┌─────────────────────▼─────────────────────────┐ │ │
│  │  │  5. PERSISTENCE PHASE (alle N Ticks)           │ │ │
│  │  │     • Dirty-Flags checken                     │ │ │
│  │  │     • Änderungen in Redis pushen              │ │ │
│  │  │     • Periodisch in PostgreSQL speichern      │ │ │
│  │  └─────────────────────┬─────────────────────────┘ │ │
│  │                        │                            │ │
│  │                    TICK END                         │ │
│  │              (Sleep bis nächster Tick)              │ │
│  └────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────┘
```

### 5.2 Tick Timing

```csharp
public class GameLoop
{
    private const int TICK_RATE = 30;  // Hz
    private const double TICK_INTERVAL_MS = 1000.0 / TICK_RATE;  // 33.33ms
    
    private readonly Stopwatch _tickTimer = new();
    private long _currentTick = 0;
    
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            _tickTimer.Restart();
            
            // ═══ TICK LOGIC ═══
            ProcessInputs();
            ValidateActions();
            SimulateWorld();
            BroadcastUpdates();
            
            if (_currentTick % 30 == 0)  // Jede Sekunde
            {
                PersistToRedis();
            }
            
            if (_currentTick % 300 == 0)  // Alle 10 Sekunden
            {
                PersistToDatabase();
            }
            
            _currentTick++;
            // ═══ END TICK ═══
            
            // Sleep für verbleibende Zeit
            double elapsed = _tickTimer.Elapsed.TotalMilliseconds;
            double sleepTime = TICK_INTERVAL_MS - elapsed;
            
            if (sleepTime > 0)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(sleepTime), cancellationToken);
            }
            else
            {
                // Tick dauerte zu lange! Logging/Warnung
                LogTickOverrun(elapsed);
            }
        }
    }
}
```

---

## 6. Client-Server Kommunikation

### 6.1 Client-Side Prediction

```
┌─────────────────────────────────────────────────────────┐
│              CLIENT-SIDE PREDICTION                      │
│                                                          │
│  Problem: Server ist 30 Hz, Client ist 60 FPS           │
│  Lösung:  Client sagt vorher, Server korrigiert         │
│                                                          │
│  ┌─────────────────────────────────────────────────┐    │
│  │                    CLIENT                        │    │
│  │                                                  │    │
│  │  1. Spieler drückt "W" (nach vorne)             │    │
│  │                                                  │    │
│  │  2. Client bewegt Spieler SOFORT lokal          │    │
│  │     Position: (100, 100) → (100, 105)           │    │
│  │                                                  │    │
│  │  3. Client sendet Input an Server               │    │
│  │     { seq: 42, input: "forward", timestamp: T } │    │
│  │                                                  │    │
│  │  4. Client speichert Prediction                 │    │
│  │     predictions[42] = { pos: (100, 105) }       │    │
│  └────────────────────────┬────────────────────────┘    │
│                           │                              │
│                           ▼                              │
│  ┌─────────────────────────────────────────────────┐    │
│  │                    SERVER                        │    │
│  │                                                  │    │
│  │  5. Server empfängt Input                       │    │
│  │                                                  │    │
│  │  6. Server validiert & simuliert                │    │
│  │     (evtl. andere Position wegen Kollision!)    │    │
│  │                                                  │    │
│  │  7. Server sendet authoritative Position        │    │
│  │     { seq: 42, pos: (100, 103), serverTime: T } │    │
│  └────────────────────────┬────────────────────────┘    │
│                           │                              │
│                           ▼                              │
│  ┌─────────────────────────────────────────────────┐    │
│  │              CLIENT RECONCILIATION               │    │
│  │                                                  │    │
│  │  8. Client empfängt Server-Position             │    │
│  │                                                  │    │
│  │  9. Vergleicht mit Prediction[42]               │    │
│  │     Predicted: (100, 105)                       │    │
│  │     Server:    (100, 103)                       │    │
│  │     Differenz: 2 Einheiten!                     │    │
│  │                                                  │    │
│  │  10. Client korrigiert Position                 │    │
│  │      (Interpolation für Smoothness)             │    │
│  │                                                  │    │
│  │  11. Alle Predictions nach seq:42 neu berechnen │    │
│  └─────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────┘
```

### 6.2 Entity Interpolation (für andere Spieler)

```
┌─────────────────────────────────────────────────────────┐
│             ENTITY INTERPOLATION                         │
│                                                          │
│  Problem: Andere Spieler kommen nur mit 30 Hz an        │
│  Lösung:  Zwischen zwei bekannten Positionen            │
│           interpolieren                                  │
│                                                          │
│  Server-Updates:                                        │
│                                                          │
│  T=0ms      T=33ms     T=66ms     T=100ms               │
│    ●──────────●──────────●──────────●                   │
│  (10,10)   (10,15)   (10,20)   (10,25)                 │
│                                                          │
│  Client Rendering (mit 100ms Buffer):                   │
│                                                          │
│  Render-Zeit   Interpolierte Position                   │
│  ──────────────────────────────────────                 │
│  T=100ms       (10,10)  ← Zeigt T=0 Daten              │
│  T=116ms       (10,12.5) ← Interpoliert                │
│  T=133ms       (10,15)  ← Zeigt T=33 Daten             │
│  T=150ms       (10,17.5) ← Interpoliert                │
│                                                          │
│  Buffer sorgt für smooth movement trotz Jitter!         │
└─────────────────────────────────────────────────────────┘
```

---

## 7. Redis-Strategie

### 7.1 Datenstruktur

```
┌─────────────────────────────────────────────────────────┐
│                  REDIS KEY SCHEMA                        │
│                                                          │
│  ═══════════════════════════════════════════════════    │
│  SESSIONS                                               │
│  ═══════════════════════════════════════════════════    │
│  session:{sessionId}          → Hash                    │
│    ├─ playerId                                          │
│    ├─ characterId                                       │
│    ├─ currentZone                                       │
│    ├─ currentShard                                      │
│    ├─ gatewayId                                         │
│    └─ lastActivity                                      │
│                                                          │
│  player:session:{playerId}    → String (sessionId)      │
│                                                          │
│  ═══════════════════════════════════════════════════    │
│  PLAYER DATA (Hot Cache)                                │
│  ═══════════════════════════════════════════════════    │
│  player:{playerId}            → Hash                    │
│    ├─ name                                              │
│    ├─ level                                             │
│    ├─ currentHp                                         │
│    ├─ maxHp                                             │
│    ├─ currentMana                                       │
│    ├─ maxMana                                           │
│    ├─ positionX                                         │
│    ├─ positionY                                         │
│    ├─ zone                                              │
│    └─ pvpFlagged                                        │
│                                                          │
│  ═══════════════════════════════════════════════════    │
│  ZONE DATA                                              │
│  ═══════════════════════════════════════════════════    │
│  zone:{zoneId}:players        → Set (playerIds)         │
│  zone:{zoneId}:shard:{n}:players → Set (playerIds)      │
│                                                          │
│  ═══════════════════════════════════════════════════    │
│  PUB/SUB CHANNELS                                       │
│  ═══════════════════════════════════════════════════    │
│  channel:zone:{zoneId}        → Zone-weite Events       │
│  channel:player:{playerId}    → Whispers, Party-Invite  │
│  channel:guild:{guildId}      → Guild-Chat, Events      │
│  channel:global               → Broadcasts              │
│                                                          │
│  ═══════════════════════════════════════════════════    │
│  RATE LIMITING                                          │
│  ═══════════════════════════════════════════════════    │
│  ratelimit:chat:{playerId}    → Counter (TTL: 60s)      │
│  ratelimit:action:{playerId}  → Counter (TTL: 1s)       │
└─────────────────────────────────────────────────────────┘
```

### 7.2 Caching-Strategie

| Daten | Redis TTL | Write-Through | Beschreibung |
|-------|-----------|---------------|--------------|
| Session | 30 min | Nein | Nur in Redis, bei Timeout → Logout |
| Position | - | Alle 10s | Hot in Redis, periodisch in DB |
| HP/Mana | - | Bei Änderung | Sofort in Redis, alle 30s in DB |
| Inventar | 5 min | Bei Änderung | Cache, Source of Truth = DB |
| Chat | Kein Cache | - | Direkt in DB (History) |

---

## 8. Datenbank-Strategie

### 8.1 Write-Strategien

```
┌─────────────────────────────────────────────────────────┐
│               PERSISTENCE STRATEGIE                      │
│                                                          │
│  ┌─────────────────────────────────────────────────┐    │
│  │  IMMEDIATE WRITE (Sofort in DB)                 │    │
│  │                                                  │    │
│  │  • Charakter-Erstellung                         │    │
│  │  • Wichtige Item-Transaktionen                  │    │
│  │  • Gold-Transfers                               │    │
│  │  • Gilden-Änderungen                            │    │
│  │  • Level-Up                                     │    │
│  └─────────────────────────────────────────────────┘    │
│                                                          │
│  ┌─────────────────────────────────────────────────┐    │
│  │  BATCHED WRITE (Alle 10-30 Sekunden)            │    │
│  │                                                  │    │
│  │  • Spieler-Positionen                           │    │
│  │  • HP/Mana Zustand                              │    │
│  │  • Spielzeit-Tracking                           │    │
│  │  • Quest-Fortschritt                            │    │
│  └─────────────────────────────────────────────────┘    │
│                                                          │
│  ┌─────────────────────────────────────────────────┐    │
│  │  LOGOUT WRITE (Bei Disconnect)                  │    │
│  │                                                  │    │
│  │  • Kompletter Charakter-State                   │    │
│  │  • Alle Pending Changes                         │    │
│  └─────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────┘
```

### 8.2 Connection Pooling

```csharp
// appsettings.json
{
  "ConnectionStrings": {
    "PostgreSQL": "Host=...;Database=2dmmo;Username=...;Password=...;Pooling=true;MinPoolSize=5;MaxPoolSize=100"
  }
}
```

---

## 9. Sicherheit

### 9.1 Security Layers

```
┌─────────────────────────────────────────────────────────┐
│                  SECURITY LAYERS                         │
│                                                          │
│  ┌─────────────────────────────────────────────────┐    │
│  │  LAYER 1: TRANSPORT (TLS 1.3)                   │    │
│  │                                                  │    │
│  │  • Verschlüsselte Verbindung                    │    │
│  │  • Server-Authentifizierung (Zertifikat)        │    │
│  │  • Man-in-the-Middle Schutz                     │    │
│  └─────────────────────────────────────────────────┘    │
│                                                          │
│  ┌─────────────────────────────────────────────────┐    │
│  │  LAYER 2: AUTHENTICATION                        │    │
│  │                                                  │    │
│  │  • Passwort-Hashing (Argon2id)                  │    │
│  │  • Session-Tokens (UUIDv4)                      │    │
│  │  • Session-Timeout (30 Minuten)                 │    │
│  └─────────────────────────────────────────────────┘    │
│                                                          │
│  ┌─────────────────────────────────────────────────┐    │
│  │  LAYER 3: AUTHORIZATION                         │    │
│  │                                                  │    │
│  │  • Spieler kann nur eigene Aktionen senden      │    │
│  │  • Zone-Zugehörigkeit wird geprüft              │    │
│  │  • PvP-Flag Status wird validiert               │    │
│  └─────────────────────────────────────────────────┘    │
│                                                          │
│  ┌─────────────────────────────────────────────────┐    │
│  │  LAYER 4: VALIDATION (Anti-Cheat)               │    │
│  │                                                  │    │
│  │  • Speed-Hack Detection                         │    │
│  │  • Teleport Detection                           │    │
│  │  • Action-Rate Limiting                         │    │
│  │  • Damage-Plausibility Checks                   │    │
│  └─────────────────────────────────────────────────┘    │
│                                                          │
│  ┌─────────────────────────────────────────────────┐    │
│  │  LAYER 5: RATE LIMITING                         │    │
│  │                                                  │    │
│  │  • Connection Rate: 5/min pro IP                │    │
│  │  • Login Rate: 3/min pro Account                │    │
│  │  • Chat Rate: 10/min pro Spieler                │    │
│  │  • Action Rate: 20/sec pro Spieler              │    │
│  └─────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────┘
```

### 9.2 Input Validation

```csharp
public class MovementValidator
{
    private const float MAX_SPEED = 10f;  // Einheiten/Sekunde
    private const float TOLERANCE = 1.2f;  // 20% Toleranz für Latenz
    
    public ValidationResult ValidateMovement(
        PlayerState current,
        PositionUpdate update,
        float deltaTime)
    {
        // 1. Besitzt der Spieler diese Session?
        if (update.PlayerId != current.PlayerId)
            return ValidationResult.Fail("Invalid player ID");
        
        // 2. Sequenznummer aufsteigend?
        if (update.SequenceNumber <= current.LastSequence)
            return ValidationResult.Fail("Stale sequence number");
        
        // 3. Bewegungsgeschwindigkeit plausibel?
        float distance = Vector2.Distance(
            new Vector2(current.X, current.Y),
            new Vector2(update.X, update.Y));
        
        float maxDistance = MAX_SPEED * deltaTime * TOLERANCE;
        
        if (distance > maxDistance)
            return ValidationResult.Fail("Speed hack detected");
        
        // 4. Position innerhalb der Welt?
        if (!WorldBounds.Contains(update.X, update.Y))
            return ValidationResult.Fail("Position out of bounds");
        
        // 5. Keine Kollision mit Wänden/Hindernissen?
        if (CollisionSystem.CheckCollision(update.X, update.Y))
            return ValidationResult.Fail("Collision detected");
        
        return ValidationResult.Success();
    }
}
```

---

## 10. Azure Deployment

### 10.1 Azure Services

| Komponente | Azure Service | Tier |
|-----------|---------------|------|
| Gateway Server | Azure Container Apps | Consumption |
| Zone Server | Azure Container Apps | Dedicated |
| Redis | Azure Cache for Redis | Standard C1+ |
| PostgreSQL | Azure Database for PostgreSQL | Flexible Server |
| Load Balancer | Azure Load Balancer | Standard |
| Monitoring | Application Insights | - |
| Secrets | Azure Key Vault | Standard |

### 10.2 Deployment-Diagramm

```
┌─────────────────────────────────────────────────────────┐
│                    AZURE DEPLOYMENT                      │
│                                                          │
│  ┌─────────────────────────────────────────────────┐    │
│  │              AZURE FRONT DOOR                    │    │
│  │           (DDoS Protection, WAF)                │    │
│  └────────────────────────┬────────────────────────┘    │
│                           │                              │
│  ┌────────────────────────┼────────────────────────┐    │
│  │         AZURE CONTAINER APPS ENVIRONMENT         │    │
│  │                        │                         │    │
│  │    ┌───────────────────┼───────────────────┐    │    │
│  │    │                   │                   │    │    │
│  │    ▼                   ▼                   ▼    │    │
│  │ ┌──────┐           ┌──────┐           ┌──────┐ │    │
│  │ │Gate- │           │Gate- │           │Gate- │ │    │
│  │ │way 1 │           │way 2 │           │way N │ │    │
│  │ └──┬───┘           └──┬───┘           └──┬───┘ │    │
│  │    │                  │                  │     │    │
│  │    ├──────────────────┼──────────────────┤     │    │
│  │    │                  │                  │     │    │
│  │    ▼                  ▼                  ▼     │    │
│  │ ┌──────┐          ┌──────┐          ┌──────┐  │    │
│  │ │Zone  │          │Zone  │          │Zone  │  │    │
│  │ │Start │          │Stadt │          │Wald  │  │    │
│  │ └──────┘          └──────┘          └──────┘  │    │
│  └─────────────────────────────────────────────────┘    │
│                           │                              │
│            ┌──────────────┼──────────────┐              │
│            │              │              │              │
│            ▼              ▼              ▼              │
│  ┌──────────────┐  ┌────────────┐  ┌────────────┐      │
│  │ Azure Cache  │  │  Azure DB  │  │  Key Vault │      │
│  │  for Redis   │  │ PostgreSQL │  │  (Secrets) │      │
│  └──────────────┘  └────────────┘  └────────────┘      │
│                                                          │
└─────────────────────────────────────────────────────────┘
```

---

## 11. Skalierung

### 11.1 Zone Sharding

```
┌─────────────────────────────────────────────────────────┐
│                   ZONE SHARDING                          │
│                                                          │
│  Trigger: Zone hat > 200 Spieler                        │
│                                                          │
│  ┌─────────────────────────────────────────────────┐    │
│  │               VOR SHARDING                       │    │
│  │                                                  │    │
│  │    Zone "Wald"                                  │    │
│  │    ┌─────────────────────────────────┐          │    │
│  │    │  👤👤👤👤👤👤👤👤👤👤           │          │    │
│  │    │  👤👤👤👤👤👤👤👤👤👤           │          │    │
│  │    │  ... 250 Spieler ...            │          │    │
│  │    │  Server-Last: 95% ⚠️             │          │    │
│  │    └─────────────────────────────────┘          │    │
│  └─────────────────────────────────────────────────┘    │
│                                                          │
│                         │                                │
│                         ▼                                │
│                                                          │
│  ┌─────────────────────────────────────────────────┐    │
│  │               NACH SHARDING                      │    │
│  │                                                  │    │
│  │    Zone "Wald" Shard 1    Zone "Wald" Shard 2  │    │
│  │    ┌───────────────────┐  ┌───────────────────┐ │    │
│  │    │  👤👤👤👤👤        │  │  👤👤👤👤👤        │ │    │
│  │    │  👤👤👤👤👤        │  │  👤👤👤👤👤        │ │    │
│  │    │  125 Spieler      │  │  125 Spieler      │ │    │
│  │    │  Last: 50% ✅      │  │  Last: 50% ✅      │ │    │
│  │    └───────────────────┘  └───────────────────┘ │    │
│  │                                                  │    │
│  │    Spieler können Shard wechseln                │    │
│  │    Gruppen bleiben zusammen                     │    │
│  └─────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────┘
```

### 11.2 Skalierungs-Metriken

| Metrik | Threshold | Aktion |
|--------|-----------|--------|
| Zone Spieler | > 200 | Neuen Shard erstellen |
| Zone Spieler | < 50 | Shards zusammenlegen |
| Server CPU | > 80% | Scale Out |
| Server Memory | > 85% | Scale Out |
| Gateway Connections | > 5000 | Neue Gateway Instanz |
| Redis Memory | > 80% | Scale Up |
| DB Connections | > 80 | Scale Up |

---

## 📝 Änderungshistorie

| Version | Datum | Änderungen |
|---------|-------|------------|
| 1.0.0 | 2025-12-02 | Initiale Architektur-Dokumentation |

---

*Dieses Dokument beschreibt die technische Architektur für die Prototyp-Phase und wird bei Bedarf erweitert.*