# 🏃 Movement / Position Messages (0200-0221)

**Kategorie:** 2  
**Range:** 0200-0221 (AKTIV)  
**Phase:** Prototyp  
**Status:** 🟢 In Entwicklung

[← Zurück zur Übersicht](Message-Reference.md)

---

## 📋 Inhaltsverzeichnis

- [Movement Flow (Übersicht)](#-movement-flow-übersicht)
  - [Architektur: Client-Prediction + Server-Authority](#architektur-client-prediction--server-authority)
  - [Position-Update Flow](#position-update-flow)
  - [Teleport Flow](#teleport-flow)
  - [Jump Flow](#jump-flow)
  - [Stuck/Recovery Flow](#stuckrecovery-flow)
  - [CC-Effects Flow (Root/Stun/Knockback)](#cc-effects-flow-rootstunknockback)
- [DTOs / Enums / Interfaces](#-dtos--enums--interfaces)
  - [MovementMode](#movementmode)
  - [MovementInputFlags](#movementinputflags)
  - [CorrectionReason](#correctionreason)
  - [TeleportType](#teleporttype)
  - [PositionDto](#positiondto)
  - [VelocityDto](#velocitydto)
- [Aktive Messages (0200-0221)](#aktive-messages-0200-0221)
  - [PositionUpdate (200)](#positionupdate-200)
  - [PositionBroadcast (201)](#positionbroadcast-201)
  - [MovementCorrection (202)](#movementcorrection-202)
  - [TeleportRequest (203)](#teleportrequest-203)
  - [TeleportExecute (204)](#teleportexecute-204)
  - [MovementSpeedUpdate (205)](#movementspeedupdate-205)
  - [JumpRequest (206)](#jumprequest-206)
  - [JumpBroadcast (207)](#jumpbroadcast-207)
  - [FallDamage (208)](#falldamage-208)
  - [StuckRequest (209)](#stuckrequest-209)
  - [StuckResponse (210)](#stuckresponse-210)
  - [PathfindingRequest (211)](#pathfindingrequest-211)
  - [PathfindingResponse (212)](#pathfindingresponse-212)
  - [ForcePosition (213)](#forceposition-213)
  - [MovementModeChange (214)](#movementmodechange-214)
  - [CollisionEvent (215)](#collisionevent-215)
  - [KnockbackEvent (216)](#knockbackevent-216)
  - [PullEvent (217)](#pullevent-217)
  - [RootEvent (218)](#rootevent-218)
  - [StunMovement (219)](#stunmovement-219)
  - [TeleportResponse (220)](#teleportresponse-220)
  - [JumpResponse (221)](#jumpresponse-221)
- [Obsolete Messages](#-obsolete-messages)
- [Anhang](#-anhang)
  - [MessageType Enum Updates](#messagetype-enum-updates)
  - [Datei-Struktur](#datei-struktur)

---

## 🔄 Movement Flow (Übersicht)

Das Movement-System basiert auf **Client-Side Prediction** mit **Server-Authority**. Der Client sendet Inputs und predicts seine Position lokal, während der Server alle Positionen validiert und authoritative Updates broadcastet.

### Architektur: Client-Prediction + Server-Authority

```
┌─────────────────────────────────────────────────────────────────┐
│  CLIENT (Prediction)                                            │
│  ├── Lokale Input-Verarbeitung                                  │
│  ├── Lokale Position-Prediction                                 │
│  ├── Input-History für Reconciliation                           │
│  └── Interpolation für andere Spieler (~100ms Buffer)           │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼ PositionUpdate (200)
┌─────────────────────────────────────────────────────────────────┐
│  SERVER (Authority)                                             │
│  ├── Input-Validation (Speed, Collision, Bounds)                │
│  ├── Position-Storage                                           │
│  ├── Broadcast an Clients in Range (AoI)                        │
│  └── Correction bei Desync (MovementCorrection 202)             │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼ PositionBroadcast (201)
┌─────────────────────────────────────────────────────────────────┐
│  OTHER CLIENTS                                                  │
│  ├── Interpolation zum Ziel (~100ms)                            │
│  ├── Extrapolation bei Packet-Loss                              │
│  └── Dead-Reckoning mit Velocity                                │
└─────────────────────────────────────────────────────────────────┘
```

**Technische Parameter:**
- **Tick-Rate**: 25 Hz (40ms pro Tick)
- **Position-Update-Rate**: Bis zu 20 Hz (optimiert für Bandwidth)
- **Interpolation-Buffer**: ~100ms für smooth Movement anderer Spieler
- **Max Speed**: 7.0 m/s (Sprint), 5.0 m/s (Walk)
- **Speed-Tolerance**: 1.1x (für Latency-Kompensation)
- **AoI-Range**: ~50m (Area of Interest für Broadcasts)

### Position-Update Flow

```
Client                         Server                    Other Clients
  │                              │                              │
  │  [Input: WASD + Mouse]       │                              │
  │  [Predict local position]    │                              │
  │                              │                              │
  │  PositionUpdate (200)        │                              │
  │  ├── SequenceNumber: 1234    │                              │
  │  ├── X, Y, Z                 │                              │
  │  ├── VelocityX, VelocityY    │                              │
  │  └── InputFlags              │                              │
  │─────────────────────────────►│                              │
  │                              │  [Validate:]                 │
  │                              │  ├── Speed ≤ MAX * TOLERANCE │
  │                              │  ├── No collision            │
  │                              │  └── Within bounds           │
  │                              │                              │
  │                              │  PositionBroadcast (201)     │
  │                              │  ├── EntityId                │
  │                              │  ├── X, Y, Z (validated)     │
  │                              │  └── Timestamp               │
  │                              │─────────────────────────────►│
  │                              │                              │  [Interpolate]
  │                              │                              │
  │  [CASE: Desync detected]     │                              │
  │  MovementCorrection (202)    │                              │
  │  ├── SequenceNumber: 1234    │                              │
  │  ├── Reason: "collision"     │                              │
  │  └── X, Y, Z (corrected)     │                              │
  │◄─────────────────────────────│                              │
  │                              │                              │
  │  [Snap to server pos]        │                              │
  │  [Re-apply inputs > 1234]    │                              │
```

### Teleport Flow

```
Client                         Server
  │                              │
  │  TeleportRequest (203)       │
  │  ├── TargetZoneId: 1002      │
  │  ├── TargetX, TargetY        │
  │  └── TeleportType: hearthstone│
  │─────────────────────────────►│
  │                              │
  │                              │  [Validate:]
  │                              │  ├── Cooldown check
  │                              │  ├── Combat check
  │                              │  └── Zone access
  │                              │
  │  TeleportResponse (220)      │
  │  ├── Success: true           │
  │  └── TargetZoneId, X, Y      │
  │◄─────────────────────────────│
  │                              │
  │  [Show loading screen]       │
  │                              │
  │  TeleportExecute (204)       │
  │  ├── ZoneId: 1002            │
  │  └── X, Y, Z                 │
  │◄─────────────────────────────│
  │                              │
  │  [Set position instantly]    │
  │  [Hide loading screen]       │
```

### Jump Flow

```
Client                         Server                    Other Clients
  │                              │                              │
  │  [Space pressed]             │                              │
  │  [Local jump prediction]     │                              │
  │                              │                              │
  │  JumpRequest (206)           │                              │
  │  ├── SequenceNumber: 5678    │                              │
  │  ├── X, Y, Z (at jump)       │                              │
  │  └── VelocityX, VelocityY    │                              │
  │─────────────────────────────►│                              │
  │                              │                              │
  │                              │  [Validate:]                 │
  │                              │  ├── Not already jumping     │
  │                              │  ├── Not rooted/stunned      │
  │                              │  ├── Stamina available       │
  │                              │  └── On ground               │
  │                              │                              │
  │  JumpResponse (221)          │                              │
  │  ├── Success: true           │                              │
  │  ├── SequenceNumber: 5678    │                              │
  │  └── StaminaCost: 10         │                              │
  │◄─────────────────────────────│                              │
  │                              │                              │
  │                              │  JumpBroadcast (207)         │
  │                              │  ├── EntityId                │
  │                              │  └── X, Y, Z, JumpPower      │
  │                              │─────────────────────────────►│
  │                              │                              │  [Play jump anim]
```

### Stuck/Recovery Flow

```
Client                         Server
  │                              │
  │  [Player stuck in wall]      │
  │                              │
  │  StuckRequest (209)          │
  │  ├── Reason: "in_wall"       │
  │  └── X, Y, Z (current)       │
  │─────────────────────────────►│
  │                              │
  │                              │  [Find safe position:]
  │                              │  ├── Last safe position
  │                              │  └── Or zone spawn
  │                              │
  │  StuckResponse (210)         │
  │  ├── Success: true           │
  │  └── X, Y, Z (safe pos)      │
  │◄─────────────────────────────│
  │                              │
  │  [Teleport to safe pos]      │
  │  [5s invulnerability]        │
```

### CC-Effects Flow (Root/Stun/Knockback)

```
Server                         Client                    Other Clients
  │                              │                              │
  │  [Spell hits target]         │                              │
  │                              │                              │
  │  KnockbackEvent (216)        │                              │
  │  ├── EntityId: 50001         │─────────────────────────────►│
  │  ├── DirectionX, DirectionY  │                              │  [Apply knockback]
  │  ├── Distance: 5.0           │                              │
  │  └── Duration: 500ms         │                              │
  │─────────────────────────────►│                              │
  │                              │  [Control loss]              │
  │                              │  [Animate knockback]         │
  │                              │                              │
  │  [After knockback ends]      │                              │
  │                              │                              │
  │  RootEvent (218)             │                              │
  │  ├── EntityId: 50001         │─────────────────────────────►│
  │  ├── Duration: 3000ms        │                              │  [Show root VFX]
  │  └── AuraId: 5678            │                              │
  │─────────────────────────────►│                              │
  │                              │  [Speed = 0]                 │
  │                              │  [Can still cast]            │
```

---

## 🧱 DTOs / Enums / Interfaces

### MovementMode

```csharp
/// <summary>
/// Current movement mode affecting physics and allowed actions.
/// </summary>
public enum MovementMode : byte
{
    Walking = 0,    // Standard ground movement
    Swimming = 1,   // In water (60% speed, no jump, no cast)
    Flying = 2,     // Airborne with control (future feature)
    Falling = 3,    // Uncontrolled fall with air-control
    Mounted = 4     // On mount (uses mount speed)
}
```

### MovementInputFlags

```csharp
/// <summary>
/// Bit flags for movement input state, sent with PositionUpdate.
/// </summary>
[Flags]
public enum MovementInputFlags : byte
{
    None = 0,
    Forward = 1,      // W key
    Backward = 2,     // S key
    Left = 4,         // A key
    Right = 8,        // D key
    Jump = 16,        // Space key
    Sprint = 32       // Shift key
}
```

### CorrectionReason

```csharp
/// <summary>
/// Reason for server-side position correction.
/// </summary>
public enum CorrectionReason : byte
{
    Collision = 0,      // Client position inside wall/object
    SpeedTooHigh = 1,   // Velocity exceeds MAX_SPEED * TOLERANCE
    OutOfBounds = 2,    // Position outside zone boundaries
    Stuck = 3,          // Server detected stuck state
    AntiCheat = 4       // General anti-cheat correction
}
```

### TeleportType

```csharp
/// <summary>
/// Type of teleport, affects cooldown and validation rules.
/// </summary>
public enum TeleportType : byte
{
    Hearthstone = 0,    // 30min CD, requires not in combat
    Portal = 1,         // Mage portal, no CD, requires portal object
    Spell = 2,          // Spell-based teleport, uses mana
    FastTravel = 3,     // Discovered location fast travel
    Admin = 4           // GM teleport, no restrictions
}
```

### PositionDto

```csharp
/// <summary>
/// Position data used in movement messages.
/// </summary>
[MessagePackObject]
public class PositionDto
{
    [Key(0)] public float X { get; set; }
    [Key(1)] public float Y { get; set; }
    [Key(2)] public float Z { get; set; }     // Height
    [Key(3)] public float Yaw { get; set; }   // Rotation 0-360
}
```

### VelocityDto

```csharp
/// <summary>
/// Velocity data for prediction and validation.
/// </summary>
[MessagePackObject]
public class VelocityDto
{
    [Key(0)] public float X { get; set; }
    [Key(1)] public float Y { get; set; }
    [Key(2)] public float Z { get; set; }   // Fall speed
}
```

---

## Aktive Messages (0200-0221)

> **Reihenfolge:** Exakt wie in `MessageType.cs` definiert.

## PositionUpdate (200)

**Richtung:** 📤 Client → Server  
**Frequenz:** ⚡ High-Frequency  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Client sendet aktuelle Position, Velocity, Rotation und Input-State. Dies ist die wichtigste High-Frequency Message im Spiel und wird bis zu 20x pro Sekunde gesendet.

### Im Scope ✅
- Position (X, Y) und Höhe (Z)
- Velocity (VX, VY, VZ) für Server-Prediction
- Rotation (Yaw) in Grad
- Input-Flags (Forward, Backward, Left, Right, Jump, Sprint)
- Sequence Number für Input-Reconciliation
- Client-Timestamp für Latency-Compensation

### Nicht im Scope ❌
- Animation-State → verwende `EntityAnimation` (1410)
- Mounted-Status → verwende `MountSummon` (2000)
- Combat-Actions → verwende `ActionRequest` (300)
- Emotes → verwende `EmoteRequest` (2200)

### Request Payload
| Feld           | Typ   | Beschreibung                                                      | Pflicht |
| -------------- | ----- | ----------------------------------------------------------------- | ------- |
| SequenceNumber | uint  | Aufsteigende Input-Sequence                                       | Ja      |
| Timestamp      | long  | Client Unix Timestamp (ms)                                        | Ja      |
| X              | float | Position X-Koordinate                                             | Ja      |
| Y              | float | Position Y-Koordinate                                             | Ja      |
| Z              | float | Position Z-Koordinate (Höhe)                                      | Ja      |
| VelocityX      | float | Velocity X                                                        | Ja      |
| VelocityY      | float | Velocity Y                                                        | Ja      |
| VelocityZ      | float | Velocity Z (Fallgeschwindigkeit)                                  | Ja      |
| Yaw            | float | Rotation (0-360 Grad)                                             | Ja      |
| InputFlags     | byte  | Bit-Flags: Forward=1, Back=2, Left=4, Right=8, Jump=16, Sprint=32 | Ja      |

### Erwartete Response
- **Bei Erfolg:** `PositionBroadcast` (201) an andere Spieler in Range
- **Bei Desync:** `MovementCorrection` (202) zurück an Client
- **Bei Speed-Hack:** `ErrorMessage` (910) mit Code `SPEED_TOO_HIGH` + potentieller Kick

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `PositionBroadcast` | 201 | Server broadcastet validierte Position |
| `MovementCorrection` | 202 | Korrektur bei Desync oder Cheating-Verdacht |
| `MovementSpeedUpdate` | 205 | Speed hat sich geändert (Buff/Debuff/Mount) |
| `CollisionEvent` | 215 | Collision mit Environment während Movement |

### Flow-Diagramm
```
Client                    Server                    Other Clients
  │                          │                          │
  │  PositionUpdate (200)    │                          │
  │─────────────────────────►│ Validate:                │
  │                          │ - Speed Check            │
  │                          │ - Collision Check        │
  │                          │ - Bounds Check           │
  │                          │                          │
  │                          │  PositionBroadcast (201) │
  │                          │─────────────────────────►│
  │                          │                          │ Interpolate
  │  (Correction if needed)  │                          │ to Position
  │◄─────────────────────────│                          │
```

### Beispiel Payload
```csharp
var posUpdate = new PositionUpdate
{
    Type = MessageType.PositionUpdate,
    SequenceNumber = currentInputSequence++,
    Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
    X = playerPosition.x,
    Y = playerPosition.y,
    Z = playerPosition.z,
    VelocityX = velocity.x,
    VelocityY = velocity.y,
    VelocityZ = velocity.z,
    Yaw = transform.rotation.eulerAngles.y,
    InputFlags = (byte)((moveForward ? 1 : 0) | (sprint ? 32 : 0)) // Forward + Sprint
};
```

### Error Codes
| Code                     | Bedeutung                        | Aktion                                 |
| ------------------------ | -------------------------------- | -------------------------------------- |
| `SPEED_TOO_HIGH`         | Velocity > MAX_SPEED * TOLERANCE | Disconnect bei 3+ Verstößen            |
| `POSITION_OUT_OF_BOUNDS` | Position außerhalb der Zone      | Teleport zur letzten gültigen Position |
| `COLLISION_INVALID`      | Position in Wand/Objekt          | Correction zur gültigen Position       |
| `SEQUENCE_TOO_OLD`       | SequenceNumber < erwartete       | Ignorieren (Packet-Loss)               |

### Notizen
- **Bandbreite**: ~60 Bytes pro Update bei 20 Hz = ~1.2 KB/s pro Spieler
- **Server-Validation**: MAX_SPEED = 7.0 m/s (Sprint), TOLERANCE = 1.1x (Latency)
- **Anti-Cheat**: 3 aufeinanderfolgende Speed-Violations = Auto-Kick
- **Optimierung**: Nur senden wenn Position/Velocity sich geändert hat (Delta-Compression)
- **Dead-Reckoning**: Server nutzt Velocity für Prediction zwischen Updates
- **Input-Reconciliation**: Client speichert Inputs ab SequenceNumber für Replay nach Correction

---

## PositionBroadcast (201)

**Richtung:** 📡 Broadcast (Server → All Clients in Range)  
**Frequenz:** ⚡ High-Frequency  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Server broadcastet validierte Position eines Spielers an alle anderen Clients in sichtbarer Range. Dies ist die authoritative Position die für Interpolation verwendet wird.

### Im Scope ✅
- Validierte Position (X, Y, Z) nach Server-Checks
- Velocity für Client-Interpolation
- Rotation (Yaw)
- Server-Timestamp für Synchronisation
- EntityId (Runtime-ID des Spielers)

### Nicht im Scope ❌
- Eigene Position → Client predicted selbst, bei Desync kommt `MovementCorrection` (202)
- Position-History → Client speichert selbst für Interpolation
- Animation-State → verwende `EntityAnimation` (1410)
- Health/Mana → verwende `EntityUpdate` (1404)

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| EntityId | int | Runtime Entity-ID des Spielers | Ja |
| Timestamp | long | Server Unix Timestamp (ms) | Ja |
| X | float | Validierte Position X | Ja |
| Y | float | Validierte Position Y | Ja |
| Z | float | Validierte Position Z | Ja |
| VelocityX | float | Velocity X | Ja |
| VelocityY | float | Velocity Y | Ja |
| VelocityZ | float | Velocity Z | Ja |
| Yaw | float | Rotation (0-360 Grad) | Ja |

### Erwartete Response
- **Keine** - Client interpoliert zur Position

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `PositionUpdate` | 200 | Client-Request der zu diesem Broadcast führt |
| `EntityUpdate` | 1404 | Vollständiges Entity-Update inkl. Stats |
| `PlayerJoinedZone` | 103 | Initiale Position wenn Spieler spawnt |

### Beispiel Payload
```csharp
var posBroadcast = new PositionBroadcast
{
    Type = MessageType.PositionBroadcast,
    EntityId = 50001,
    Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
    X = validatedPos.x,
    Y = validatedPos.y,
    Z = validatedPos.z,
    VelocityX = vel.x,
    VelocityY = vel.y,
    VelocityZ = vel.z,
    Yaw = rotation
};
```

### Notizen
- **Range**: Nur an Clients in ~50m Range gesendet (Area-of-Interest / AoI)
- **Interpolation**: Client interpoliert über ~100ms Buffer für smooth Movement
- **Rate-Limiting**: Server kann auf 10 Hz throttlen wenn >50 Spieler in Zone
- **Batching**: Server kann mehrere Broadcasts batchen (Optimization Phase 2)
- **Priority**: High-Priority Message - wird vor Low-Priority Messages gesendet
- **Packet-Loss**: Bei Verlust: Client extrapoliert kurzzeitig mit letzter Velocity

---

## MovementCorrection (202)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten (nur bei Desync)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Server korrigiert Client-Position bei Desync (Speed-Hack, Collision-Fehler, Lag-Spike). Client muss zur Server-Position snappen und alle pending Inputs neu anwenden (Input-Reconciliation).

### Im Scope ✅
- Korrekte Server-Position
- Sequence Number (welcher Input war inkorrekt)
- Grund für Correction (collision, speed, bounds)
- Neue Velocity

### Nicht im Scope ❌
- Normale Position-Updates → verwende `PositionBroadcast` (201)
- Admin-Teleport → verwende `ForcePosition` (213) oder `TeleportExecute` (204)

### Response Payload
| Feld           | Typ    | Beschreibung                                     | Pflicht |
| -------------- | ------ | ------------------------------------------------ | ------- |
| SequenceNumber | uint   | Input-Sequence die korrigiert wird               | Ja      |
| Reason         | string | "collision", "speed_too_high", "bounds", "stuck" | Ja      |
| X              | float  | Korrekte Position X                              | Ja      |
| Y              | float  | Korrekte Position Y                              | Ja      |
| Z              | float  | Korrekte Position Z                              | Ja      |
| VelocityX      | float  | Korrekte Velocity X                              | Ja      |
| VelocityY      | float  | Korrekte Velocity Y                              | Ja      |
| VelocityZ      | float  | Korrekte Velocity Z                              | Ja      |

### Erwartete Response
- **Client:** Snap zur Position, re-apply alle Inputs > SequenceNumber

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `PositionUpdate` | 200 | Client-Input der korrigiert wird |
| `ForcePosition` | 213 | Hard-Teleport ohne Reconciliation (Admin/Cutscene) |
| `TeleportExecute` | 204 | Legitimer Teleport (Portal, Spell) |

### Beispiel Payload
```csharp
var correction = new MovementCorrection
{
    Type = MessageType.MovementCorrection,
    SequenceNumber = invalidSequence,
    Reason = "collision",
    X = serverPos.x,
    Y = serverPos.y,
    Z = serverPos.z,
    VelocityX = 0,
    VelocityY = 0,
    VelocityZ = 0
};
```

### Notizen
- **Client-Reconciliation**: Client hat pending Inputs gespeichert ab SequenceNumber
- **Re-Apply**: Nach Snap werden alle Inputs > SequenceNumber neu simuliert
- **Smooth Correction**: Optional kann Client über 100-200ms interpolieren statt hart snappen
- **Häufigkeit**: Sollte selten sein (<1% der Position-Updates), sonst Problem
- **Debug-Mode**: Im Debug kann Client Corrections visualisieren (rote Line)
- **Anti-Cheat**: Viele Corrections (>10/min) = Verdacht auf Speed-Hack oder schlechte Connection

---

## TeleportRequest (203)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Client bittet um Teleport zu einer Position (Hearthstone, Portal, Spell, Fast-Travel). Server validiert Berechtigung und führt Teleport aus.

### Im Scope ✅
- Teleport-Ziel (ZoneId + Position)
- Teleport-Typ (Hearthstone, Portal, Spell, FastTravel)
- Cooldown-Check

### Nicht im Scope ❌
- Admin-Teleport → verwende `AdminTeleport` (2302)
- Zone-Transfer ohne Teleport → verwende `ZoneTransferRequest` (105)

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| TargetZoneId | int | Ziel-Zone-ID | Ja |
| TargetX | float | Ziel-Position X | Ja |
| TargetY | float | Ziel-Position Y | Ja |
| TeleportType | string | "hearthstone", "portal", "spell", "fast_travel" | Ja |
| SourceObjectId | int | Portal/NPC-ID falls relevant | Nein |

### Erwartete Response
- `TeleportResponse` (220)

### Folge-Messages bei Erfolg
- `TeleportExecute` (204) für Position-Set
- `JoinZone` (100) falls Ziel in anderer Zone
- `ZoneState` (102) für Zone-Informationen bei Zone-Wechsel

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `TeleportExecute` | 204 | Server führt Teleport aus |
| `ZoneTransferRequest` | 105 | Zone-Wechsel ohne Teleport |
| `HearthstoneUse` | 4220 | Spezifischer Hearthstone-Use |

### Beispiel Payload
```csharp
var teleportReq = new TeleportRequest
{
    Type = MessageType.TeleportRequest,
    TargetZoneId = 1002,
    TargetX = 150.0f,
    TargetY = 200.0f,
    TeleportType = "hearthstone"
};
```

### Error Codes
| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `COOLDOWN_ACTIVE` | Hearthstone auf Cooldown | Warten (Zeit in Message) |
| `INVALID_TARGET` | Ziel nicht erreichbar | Anderen Ort wählen |
| `IN_COMBAT` | Im Kampf | Kampf beenden |
| `INSUFFICIENT_MANA` | Nicht genug Mana (Spell-Teleport) | Warten bis Mana regen |

### Notizen
- **Hearthstone-Cooldown**: 30 Minuten (Standard)
- **Combat-Lock**: Teleport nur möglich wenn nicht im Kampf (außer Admin)
- **Loading-Screen**: Client zeigt Loading während Teleport
- **Fade-Out**: 2 Sekunden Casting-Zeit für Hearthstone

---

## TeleportExecute (204)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Server führt Teleport aus. Client setzt Position sofort (kein Interpolation). Kann innerhalb Zone oder zu anderer Zone sein.

### Im Scope ✅
- Neue Position (X, Y, Z)
- Neue Zone (ZoneId) falls Zone-Wechsel
- Sofortiges Position-Set (kein Movement)

### Nicht im Scope ❌
- Zone-Load → bei Zone-Wechsel folgt `JoinZone` (100)
- Correction → verwende `MovementCorrection` (202)

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| ZoneId | int | Ziel-Zone-ID | Ja |
| X | float | Position X | Ja |
| Y | float | Position Y | Ja |
| Z | float | Position Z | Ja |
| Yaw | float | Neue Rotation | Nein |

### Erwartete Response
- **Client:** Sofortiges Position-Set, bei Zone-Wechsel folgt `JoinZone` (100)

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `TeleportRequest` | 203 | Client-Request |
| `JoinZone` | 100 | Bei Zone-Wechsel |
| `ForcePosition` | 213 | Ähnlich aber ohne Request |

### Beispiel Payload
```csharp
var teleportExec = new TeleportExecute
{
    Type = MessageType.TeleportExecute,
    ZoneId = 1002,
    X = 150.0f,
    Y = 200.0f,
    Z = 10.0f,
    Yaw = 90.0f
};
```

### Notizen
- **Fade-Effect**: Client kann Fade-Out/In animieren
- **Sound**: Teleport-Sound abspielen
- **No Collision**: Position wird nicht validiert (Server-Trust)

---


## MovementSpeedUpdate (205)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Server informiert Client über geänderte Movement-Speed (durch Buffs, Debuffs, Mounts, Slowdown-Effects). Client muss Speed sofort anpassen für korrekte Prediction.

### Im Scope ✅
- Neue Movement-Speed (Multiplikator)
- Grund für Speed-Change (buff, debuff, mount, environmental)
- Duration (falls temporär)
- Stack-Count (falls mehrere Speed-Modifiers aktiv)

### Nicht im Scope ❌
- Aura-Details → verwende `BuffApplied` (1500) oder `DebuffApplied` (1504)
- Mount-Informationen → verwende `MountSummon` (2000)
- Position-Update → weiterhin `PositionUpdate` (200) verwenden

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| SpeedMultiplier | float | Neue Speed (1.0 = Normal, 1.5 = +50%, 0.5 = -50%) | Ja |
| Reason | string | "buff", "debuff", "mount", "environmental", "root", "stun" | Ja |
| Duration | int | Dauer in Millisekunden (0 = permanent bis manuell entfernt) | Nein |
| StackCount | int | Anzahl aktiver Speed-Modifiers | Nein |
| AuraId | int | ID des Buffs/Debuffs falls relevant | Nein |

### Erwartete Response
- **Keine** - Client passt Speed an

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `BuffApplied` | 1500 | Buff der Speed ändert |
| `DebuffApplied` | 1504 | Debuff der Speed ändert |
| `MountSummon` | 2000 | Mount erhöht Speed |
| `RootEvent` | 218 | Speed = 0 (verwurzelt) |

### Beispiel Payload
```csharp
// Mount: +100% Speed
var speedUpdate = new MovementSpeedUpdate
{
    Type = MessageType.MovementSpeedUpdate,
    SpeedMultiplier = 2.0f, // +100%
    Reason = "mount",
    Duration = 0 // Permanent bis Dismount
};

// Slow-Debuff: -30% Speed für 5 Sekunden
var slowDebuff = new MovementSpeedUpdate
{
    Type = MessageType.MovementSpeedUpdate,
    SpeedMultiplier = 0.7f, // -30%
    Reason = "debuff",
    Duration = 5000, // 5 Sekunden
    AuraId = 1234
};
```

### Notizen
- **Base Speed**: 5.0 m/s (Walking), 7.0 m/s (Sprint)
- **Client Prediction**: Client muss MAX_SPEED in Prediction anpassen
- **Stacking**: Mehrere Speed-Modifiers multiplizieren sich (1.5 * 0.8 = 1.2)
- **Root/Stun**: SpeedMultiplier = 0.0
- **Cap**: Max Speed = 3.0x Base Speed (Anti-Cheat)

---

## JumpRequest (206)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Client initiiert Sprung. Server validiert ob Sprung erlaubt ist (nicht in Air, nicht verwurzelt, Stamina verfügbar) und broadcastet.

### Im Scope ✅
- Jump-Initiierung
- Current Position und Velocity
- Jump-Power/Height Modifikator

### Nicht im Scope ❌
- Falling → automatisch durch Physics
- Double-Jump → geplant Feature
- Jump-Ability (z.B. Leap) → verwende `ActionRequest` (300)

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| SequenceNumber | uint | Input-Sequence | Ja |
| X | float | Position beim Jump | Ja |
| Y | float | Position beim Jump | Ja |
| Z | float | Position beim Jump | Ja |
| VelocityX | float | Horizontal Velocity beim Jump | Ja |
| VelocityY | float | Horizontal Velocity beim Jump | Ja |
| JumpPower | float | Jump-Kraft (1.0 = Normal) | Nein |

### Erwartete Response
- `JumpResponse` (221)

### Folge-Messages bei Erfolg
- `JumpBroadcast` (207) an alle Spieler in Range

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `JumpBroadcast` | 207 | Server broadcastet Jump |
| `FallDamage` | 208 | Falls Sprung zu Fall-Schaden führt |

### Beispiel Payload
```csharp
var jumpReq = new JumpRequest
{
    Type = MessageType.JumpRequest,
    SequenceNumber = currentInputSequence++,
    X = position.x,
    Y = position.y,
    Z = position.z,
    VelocityX = velocity.x,
    VelocityY = velocity.y,
    JumpPower = 1.0f
};
```

### Error Codes
| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `ALREADY_JUMPING` | In der Luft | Ignorieren |
| `ROOTED` | Verwurzelt | Warten bis Root endet |
| `STUNNED` | Stunned | Warten bis Stun endet |
| `NO_STAMINA` | Keine Stamina | Warten bis Regen |

### Notizen
- **Jump-Height**: ~2.5m (Standard)
- **Stamina-Cost**: 10 Stamina pro Jump
- **Cooldown**: 0.5s zwischen Jumps (Spam-Prevention)
- **Anti-Cheat**: Max 5 Jumps/Sekunde = Kick

---

## JumpBroadcast (207)

**Richtung:** 📡 Broadcast  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Server broadcastet Jump eines Spielers an alle in Range. Client spielt Jump-Animation ab.

### Im Scope ✅
- Entity-ID des springenden Spielers
- Position beim Jump
- Jump-Power

### Nicht im Scope ❌
- Velocity → Client kann extrapolieren
- Landing → kein separates Event

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| EntityId | int | Entity-ID | Ja |
| X | float | Position beim Jump | Ja |
| Y | float | Position beim Jump | Ja |
| Z | float | Position beim Jump | Ja |
| JumpPower | float | Jump-Kraft | Nein |

### Erwartete Response
- **Keine** - Client spielt Animation

### Beispiel Payload
```csharp
var jumpBcast = new JumpBroadcast
{
    Type = MessageType.JumpBroadcast,
    EntityId = 50001,
    X = pos.x,
    Y = pos.y,
    Z = pos.z,
    JumpPower = 1.0f
};
```

### Notizen
- **Animation**: Jump-Animation ~0.5s
- **Sound**: Jump-Sound abspielen
- **Particles**: Optional Jump-Dust-Effect

---

## FallDamage (208)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Server berechnet Fall-Schaden basierend auf Fall-Höhe und informiert Client. Client zeigt Damage-Number und Updated Health.

### Im Scope ✅
- Damage-Amount
- Fall-Höhe
- Current Health nach Damage

### Nicht im Scope ❌
- Death → falls Health = 0, folgt `DeathEvent` (303)
- Combat-Damage → verwende `DamageEvent` (302)

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Damage | int | Fall-Schaden | Ja |
| FallHeight | float | Höhe des Falls in Metern | Ja |
| CurrentHealth | int | Health nach Damage | Ja |
| MaxHealth | int | Max Health | Ja |

### Erwartete Response
- **Bei Health > 0:** Client zeigt Damage
- **Bei Health = 0:** Folgt `DeathEvent` (303)

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `DamageEvent` | 302 | Combat-Damage |
| `DeathEvent` | 303 | Falls lethal |

### Beispiel Payload
```csharp
var fallDmg = new FallDamage
{
    Type = MessageType.FallDamage,
    Damage = 250,
    FallHeight = 15.5f,
    CurrentHealth = 750,
    MaxHealth = 1000
};
```

### Notizen
- **Formula**: Damage = max(0, (FallHeight - 4.0) * 50)
- **Safe Height**: Bis 4m kein Damage
- **Lethal Height**: >20m = instant death
- **Immunität**: Flying, Levitate, Slow-Fall

---

## StuckRequest (209)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Spieler meldet "stuck" (in Wand, unter Terrain, in Objekt gefangen). Server teleportiert zu letzter sicherer Position oder nächstem Safe-Spot.

### Im Scope ✅
- Stuck-Meldung
- Current Position (wo stuck)
- Stuck-Reason

### Nicht im Scope ❌
- Hearthstone → verwende `TeleportRequest` (203)
- Admin-Teleport → verwende `AdminTeleport` (2302)

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Reason | string | "in_wall", "under_terrain", "in_object", "cant_move" | Ja |
| X | float | Current Position X | Ja |
| Y | float | Current Position Y | Ja |
| Z | float | Current Position Z | Ja |

### Erwartete Response
- **Immer:** `StuckResponse` (210) mit neuer Position

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `StuckResponse` | 210 | Teleport zu Safe-Spot |
| `TeleportExecute` | 204 | Ähnlich aber ohne Stuck-Context |

### Beispiel Payload
```csharp
var stuckReq = new StuckRequest
{
    Type = MessageType.StuckRequest,
    Reason = "in_wall",
    X = currentPos.x,
    Y = currentPos.y,
    Z = currentPos.z
};
```

### Notizen
- **Cooldown**: 5 Minuten pro Stuck-Request (Anti-Abuse)
- **Safe-Spot**: Letzter Safe-Spot oder Zone-Spawn
- **Logging**: Stuck-Requests werden geloggt (Level-Design-Bug-Detection)

---

## StuckResponse (210)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Server teleportiert Spieler zu sicherer Position nach Stuck-Request.

### Im Scope ✅
- Neue sichere Position
- Erfolgs-Status

### Nicht im Scope ❌
- Zone-Wechsel → bleibt in gleicher Zone

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Success | bool | Teleport erfolgreich? | Ja |
| X | float | Neue Position X | Bei Success |
| Y | float | Neue Position Y | Bei Success |
| Z | float | Neue Position Z | Bei Success |
| Message | string | Info-Text für Client | Nein |

### Erwartete Response
- **Keine** - Client setzt Position

### Beispiel Payload
```csharp
var stuckResp = new StuckResponse
{
    Type = MessageType.StuckResponse,
    Success = true,
    X = safePos.x,
    Y = safePos.y,
    Z = safePos.z,
    Message = "Teleported to safe location"
};
```

### Notizen
- **Fade-Effect**: 2 Sekunden Fade-Out/In
- **Invulnerability**: 5 Sekunden unverwundbar nach Teleport

---

## PathfindingRequest (211)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
**Feature** - Client bittet um Pathfinding-Calculation für Auto-Travel (z.B. zu NPC, Quest-Marker, Party-Member).

### Im Scope ✅
- Ziel-Position oder Ziel-Entity
- Path-Type (walking, flying)

### Nicht im Scope ❌
- Direct Movement → Client steuert selbst

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| TargetX | float | Ziel X | Ja* |
| TargetY | float | Ziel Y | Ja* |
| TargetEntityId | int | Ziel-Entity | Ja* |
| PathType | string | "walking", "flying" | Ja |

\* Entweder X/Y oder EntityId

### Erwartete Response
- **Bei Erfolg:** `PathfindingResponse` (212) mit Waypoints
- **Bei Fehler:** `ErrorMessage` (910)

### Notizen
- **Hinweis**: Nicht im Prototyp implementiert
- **Use-Case**: Auto-Travel, Follow

---

## PathfindingResponse (212)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
**Feature** - Server sendet berechneten Path als Waypoint-Liste.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Success | bool | Path gefunden? | Ja |
| Waypoints | List<Vector2> | Path-Punkte | Bei Success |

### Notizen
- **Hinweis**: Nicht im Prototyp

---

## ForcePosition (213)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Admin/System

### Beschreibung
Server forciert Position ohne Client-Input (Admin-Teleport, Cutscene, Anti-Cheat-Correction). Keine Input-Reconciliation.

### Im Scope ✅
- Hard Position-Set
- Grund (admin, cutscene, anti_cheat)
- Keine Reconciliation

### Nicht im Scope ❌
- Normale Correction → verwende `MovementCorrection` (202)
- Teleport mit Request → verwende `TeleportRequest/Execute` (203/204)

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| X | float | Position X | Ja |
| Y | float | Position Y | Ja |
| Z | float | Position Z | Ja |
| Reason | string | "admin", "cutscene", "anti_cheat", "system" | Ja |

### Erwartete Response
- **Keine** - Client setzt Position sofort, cleared Input-Queue

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `MovementCorrection` | 202 | Normale Correction mit Reconciliation |
| `TeleportExecute` | 204 | Legitimer Teleport |
| `AdminTeleport` | 2302 | Admin-Command |

### Beispiel Payload
```csharp
var forcePos = new ForcePosition
{
    Type = MessageType.ForcePosition,
    X = newPos.x,
    Y = newPos.y,
    Z = newPos.z,
    Reason = "admin"
};
```

### Notizen
- **No Reconciliation**: Client cleared alle pending Inputs
- **Admin-Only**: Normale Spieler können diese Message nicht auslösen
- **Logging**: Alle Force-Positions werden geloggt (Audit-Trail)

---

## MovementModeChange (214)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Movement-Mode ändert sich (Walking → Swimming, Ground → Flying, etc.). Ändert Physics und erlaubte Actions.

### Im Scope ✅
- Neuer Movement-Mode
- Speed-Änderung für den Mode
- Erlaubte Actions im Mode

### Nicht im Scope ❌
- Speed-Buff separat → verwende `MovementSpeedUpdate` (205)
- Mount → verwende `MountSummon` (2000)

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Mode | string | "walking", "swimming", "flying", "falling", "mounted" | Ja |
| SpeedMultiplier | float | Speed im neuen Mode | Ja |
| CanJump | bool | Jump erlaubt? | Ja |
| CanCast | bool | Casting erlaubt? | Ja |

### Erwartete Response
- **Keine** - Client passt Physics an

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `MovementSpeedUpdate` | 205 | Speed-Änderung |
| `MountSummon` | 2000 | Mount-Mode |

### Beispiel Payload
```csharp
// Enter Water
var swimMode = new MovementModeChange
{
    Type = MessageType.MovementModeChange,
    Mode = "swimming",
    SpeedMultiplier = 0.6f, // 60% Speed in Water
    CanJump = false,
    CanCast = false
};
```

### Notizen
- **Swimming**: 60% Speed, kein Jump, kein Cast
- **Flying**: 150% Speed (geplant)
- **Falling**: Kontrollierter Fall mit Air-Control
- **Mounted**: Siehe MountSummon

---

## CollisionEvent (215)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Server informiert über Collision mit Environment (Wand, Objekt, Barrier). Client kann Sound/Effect abspielen.

### Im Scope ✅
- Collision-Position
- Collision-Type (wall, object, barrier)
- Impact-Strength

### Nicht im Scope ❌
- Damage → verwende `DamageEvent` (302) falls Collision Schaden verursacht
- Knockback → verwende `KnockbackEvent` (216)

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| X | float | Collision Position X | Ja |
| Y | float | Collision Position Y | Ja |
| Type | string | "wall", "object", "barrier", "player" | Ja |
| ImpactStrength | float | 0.0-1.0 (für Sound/Effect) | Ja |

### Erwartete Response
- **Keine** - Client spielt Effect

### Beispiel Payload
```csharp
var collision = new CollisionEvent
{
    Type = MessageType.CollisionEvent,
    X = collisionPos.x,
    Y = collisionPos.y,
    Type = "wall",
    ImpactStrength = 0.8f
};
```

### Notizen
- **Sound**: Impact-Sound basierend auf Strength
- **Particles**: Dust/Spark-Effect
- **Frequency**: Nur bei signifikanten Collisions (Speed > 2m/s)

---

## KnockbackEvent (216)

**Richtung:** 📡 Broadcast  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Entity wird zurückgestoßen (Spell, Explosion, Shield-Bash). Server berechnet Knockback-Vector und Dauer.

### Im Scope ✅
- Knockback-Direction und Distance
- Dauer
- Source (welche Entity/Spell verursachte Knockback)

### Nicht im Scope ❌
- Damage → separates `DamageEvent` (302)
- Stun → verwende `StunMovement` (219)

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| EntityId | int | Betroffene Entity | Ja |
| SourceEntityId | int | Verursacher | Nein |
| DirectionX | float | Normalized Direction X | Ja |
| DirectionY | float | Normalized Direction Y | Ja |
| Distance | float | Knockback-Distanz in Metern | Ja |
| Duration | int | Dauer in Millisekunden | Ja |

### Erwartete Response
- **Keine** - Client animiert Knockback

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `DamageEvent` | 302 | Oft zusammen mit Knockback |
| `PullEvent` | 217 | Gegenteil (Pull) |
| `StunMovement` | 219 | Nach Knockback oft Stun |

### Beispiel Payload
```csharp
var knockback = new KnockbackEvent
{
    Type = MessageType.KnockbackEvent,
    EntityId = 50001,
    SourceEntityId = 50002,
    DirectionX = 0.707f, // 45° Northeast
    DirectionY = 0.707f,
    Distance = 5.0f, // 5 Meter
    Duration = 500 // 0.5 Sekunden
};
```

### Notizen
- **Collision-Check**: Server validiert Knockback gegen Walls
- **Control-Loss**: Während Knockback kein Player-Input
- **Animation**: Knockback-Animation ~0.5s

---

## PullEvent (217)

**Richtung:** 📡 Broadcast  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Entity wird herangezogen (Hook, Chain, Pull-Spell). Gegenteil von Knockback.

### Im Scope ✅
- Pull-Richtung und Distance
- Target-Position (wohin gezogen)
- Dauer

### Nicht im Scope ❌
- Stun nach Pull → verwende `StunMovement` (219)

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| EntityId | int | Betroffene Entity | Ja |
| SourceEntityId | int | Verursacher | Ja |
| TargetX | float | Ziel-Position X | Ja |
| TargetY | float | Ziel-Position Y | Ja |
| Duration | int | Dauer in ms | Ja |

### Erwartete Response
- **Keine** - Client animiert Pull

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `KnockbackEvent` | 216 | Gegenteil |

### Beispiel Payload
```csharp
var pull = new PullEvent
{
    Type = MessageType.PullEvent,
    EntityId = 50001,
    SourceEntityId = 50002,
    TargetX = sourcePos.x,
    TargetY = sourcePos.y,
    Duration = 800
};
```

### Notizen
- **Use-Case**: Hook-Abilities, Boss-Mechanics
- **Control-Loss**: Während Pull kein Player-Input
- **Collision**: Pull stoppt bei Walls

---

## RootEvent (218)

**Richtung:** 📡 Broadcast  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Entity wird verwurzelt (Root-Spell, Trap). Movement = 0, aber Abilities nutzbar.

### Im Scope ✅
- Root-Start
- Dauer
- Source

### Nicht im Scope ❌
- Root-Ende → automatisch nach Duration oder durch `BuffRemoved` (1501)
- Stun → verwende `StunMovement` (219)

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| EntityId | int | Verwurzelte Entity | Ja |
| SourceEntityId | int | Verursacher | Nein |
| Duration | int | Dauer in ms | Ja |
| AuraId | int | Aura-ID | Nein |

### Erwartete Response
- **Keine** - Client zeigt Root-Effect

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `BuffRemoved` | 1501 | Root wurde gebrochen/dispelled |
| `StunMovement` | 219 | Movement + Actions disabled |
| `MovementSpeedUpdate` | 205 | Speed = 0 |

### Beispiel Payload
```csharp
var root = new RootEvent
{
    Type = MessageType.RootEvent,
    EntityId = 50001,
    SourceEntityId = 50002,
    Duration = 3000, // 3 Sekunden
    AuraId = 5678
};
```

### Notizen
- **Movement**: Speed = 0, kein Movement möglich
- **Abilities**: Casting/Actions weiterhin möglich
- **Visual**: Root-Chains oder Vines-Effect
- **Break**: Kann durch Damage gebrochen werden (Implementation-Detail)

---

## StunMovement (219)

**Richtung:** 📡 Broadcast  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Entity ist stunned (Stun-Spell, Bash). Movement UND Actions sind disabled.

### Im Scope ✅
- Stun-Start
- Dauer
- Source
- Full Control-Loss

### Nicht im Scope ❌
- Stun-Ende → automatisch nach Duration
- Root (nur Movement) → verwende `RootEvent` (218)

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| EntityId | int | Gestunnte Entity | Ja |
| SourceEntityId | int | Verursacher | Nein |
| Duration | int | Dauer in ms | Ja |
| AuraId | int | Aura-ID | Nein |

### Erwartete Response
- **Keine** - Client zeigt Stun-Effect

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `RootEvent` | 218 | Nur Movement disabled |
| `BuffRemoved` | 1501 | Stun wurde gebrochen |
| `MovementSpeedUpdate` | 205 | Speed = 0 |

### Beispiel Payload
```csharp
var stun = new StunMovement
{
    Type = MessageType.StunMovement,
    EntityId = 50001,
    SourceEntityId = 50002,
    Duration = 2000, // 2 Sekunden
    AuraId = 5679
};
```

### Notizen
- **Full CC**: Weder Movement noch Actions möglich
- **Visual**: Stars/Birds-Effect über Kopf
- **Sound**: "Bonk" Sound
- **PvP**: Diminishing Returns apply (Stun-Duration reduziert bei wiederholtem Stun)
- **Immunity**: Boss-Mechanics können Stun-Immunity haben

---

## TeleportResponse (220)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung
Antwort auf TeleportRequest. Bestätigt erfolgreichen Teleport oder gibt Fehler zurück.

### Im Scope ✅
- Erfolgs-Status (Success/Failure)
- Ziel-Zone und Position bei Erfolg
- Error-Code bei Fehler

### Nicht im Scope ❌
- Tatsächlicher Teleport → erfolgt via `TeleportExecute` (204)

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Success | bool | Teleport erlaubt? | Ja |
| ErrorCode | string | Fehlercode falls Success=false | Nein |
| ErrorMessage | string | Menschenlesbare Fehlermeldung | Nein |
| TargetZoneId | int | Ziel-Zone-ID | Bei Erfolg |
| TargetX | float | Ziel-Position X | Bei Erfolg |
| TargetY | float | Ziel-Position Y | Bei Erfolg |

### Erwartete Response
- Keine (ist selbst Response)

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `TeleportRequest` | 203 | Request zu dieser Response |
| `TeleportExecute` | 204 | Folgt bei Erfolg |
| `JoinZone` | 100 | Folgt bei Zone-Wechsel |

### Beispiel Payload
```csharp
// Erfolg
var successResponse = new TeleportResponse
{
    Type = MessageType.TeleportResponse,
    Success = true,
    TargetZoneId = 1002,
    TargetX = 150.0f,
    TargetY = 200.0f
};

// Fehler
var errorResponse = new TeleportResponse
{
    Type = MessageType.TeleportResponse,
    Success = false,
    ErrorCode = "COOLDOWN_ACTIVE",
    ErrorMessage = "Hearthstone is on cooldown. Available in 1500 seconds."
};
```

### Error Codes
| Code | Bedeutung |
|------|-----------|
| `COOLDOWN_ACTIVE` | Hearthstone auf Cooldown |
| `INVALID_TARGET` | Ziel nicht erreichbar |
| `IN_COMBAT` | Im Kampf |
| `INSUFFICIENT_MANA` | Nicht genug Mana (Spell-Teleport) |

### Notizen
- Nach erfolgreicher Response folgt `TeleportExecute` (204)
- Bei Zone-Wechsel folgt zusätzlich `JoinZone` (100)
- Loading-Screen wird zwischen Response und Execute angezeigt

---

## JumpResponse (221)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung
Antwort auf JumpRequest. Bestätigt erfolgreichen Sprung oder gibt Fehler zurück.

### Im Scope ✅
- Erfolgs-Status (Success/Failure)
- Error-Code bei Fehler
- Stamina-Cost Bestätigung

### Nicht im Scope ❌
- Broadcast an andere Spieler → erfolgt via `JumpBroadcast` (207)

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Success | bool | Sprung erfolgreich? | Ja |
| ErrorCode | string | Fehlercode falls Success=false | Nein |
| ErrorMessage | string | Menschenlesbare Fehlermeldung | Nein |
| SequenceNumber | uint | Matching Client Sequence | Ja |
| StaminaCost | int | Verbrauchte Stamina | Bei Erfolg |

### Erwartete Response
- Keine (ist selbst Response)

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `JumpRequest` | 206 | Request zu dieser Response |
| `JumpBroadcast` | 207 | Folgt bei Erfolg an andere Spieler |

### Beispiel Payload
```csharp
// Erfolg
var successResponse = new JumpResponse
{
    Type = MessageType.JumpResponse,
    Success = true,
    SequenceNumber = 12345,
    StaminaCost = 10
};

// Fehler
var errorResponse = new JumpResponse
{
    Type = MessageType.JumpResponse,
    Success = false,
    ErrorCode = "ROOTED",
    ErrorMessage = "Cannot jump while rooted",
    SequenceNumber = 12345
};
```

### Error Codes
| Code | Bedeutung |
|------|-----------|
| `ALREADY_JUMPING` | In der Luft |
| `ROOTED` | Verwurzelt |
| `STUNNED` | Stunned |
| `NO_STAMINA` | Keine Stamina |

### Notizen
- Nach erfolgreicher Response folgt `JumpBroadcast` (207) an andere Spieler
- SequenceNumber ermöglicht Client, Prediction zu korrigieren
- Stamina-Cost wird vom Server bestimmt (Anti-Cheat)

---

## 🗑️ Obsolete Messages

> Aktuell keine obsoleten Messages in dieser Kategorie.

---

## 📎 Anhang

### MessageType Enum Updates

```csharp
// MOVEMENT / POSITION (0200-0299)
// Reihenfolge exakt wie in MessageType.cs
PositionUpdate = 200,
PositionBroadcast = 201,
MovementCorrection = 202,
TeleportRequest = 203,
TeleportExecute = 204,
MovementSpeedUpdate = 205,
JumpRequest = 206,
JumpBroadcast = 207,
FallDamage = 208,
StuckRequest = 209,
StuckResponse = 210,
PathfindingRequest = 211,
PathfindingResponse = 212,
ForcePosition = 213,
MovementModeChange = 214,
CollisionEvent = 215,
KnockbackEvent = 216,
PullEvent = 217,
RootEvent = 218,
StunMovement = 219,
TeleportResponse = 220,
JumpResponse = 221
```

### Request/Response Paare

| Request | ID | Response | ID | Bemerkung |
|---------|----|---------|----|-----------|
| `PositionUpdate` | 200 | `PositionBroadcast` / `MovementCorrection` | 201 / 202 | High-frequency, fire-and-forget mit Broadcast/Correction |
| `TeleportRequest` | 203 | `TeleportResponse` | 220 | + `TeleportExecute` (204) bei Erfolg |
| `JumpRequest` | 206 | `JumpResponse` | 221 | + `JumpBroadcast` (207) bei Erfolg |
| `StuckRequest` | 209 | `StuckResponse` | 210 | Direct response |
| `PathfindingRequest` | 211 | `PathfindingResponse` | 212 | Direct response |

### Datei-Struktur

```
shared/Mmo.Shared/Messaging/
├── Enums/
│   └── MessageType.cs          # Movement: 200-221
├── Messages/Movement/
│   ├── PositionUpdate.cs
│   ├── PositionBroadcast.cs
│   ├── MovementCorrection.cs
│   ├── TeleportRequest.cs
│   ├── TeleportResponse.cs
│   ├── TeleportExecute.cs
│   ├── MovementSpeedUpdate.cs
│   ├── JumpRequest.cs
│   ├── JumpResponse.cs
│   ├── JumpBroadcast.cs
│   ├── FallDamage.cs
│   ├── StuckRequest.cs
│   ├── StuckResponse.cs
│   ├── PathfindingRequest.cs
│   ├── PathfindingResponse.cs
│   ├── ForcePosition.cs
│   ├── MovementModeChange.cs
│   ├── CollisionEvent.cs
│   ├── KnockbackEvent.cs
│   ├── PullEvent.cs
│   ├── RootEvent.cs
│   └── StunMovement.cs
└── DTOs/
    ├── PositionDto.cs
    └── VelocityDto.cs
```

### Validation Constants

```csharp
public static class MovementConstants
{
    // Speed limits
    public const float WalkSpeed = 5.0f;      // m/s
    public const float SprintSpeed = 7.0f;    // m/s
    public const float SpeedTolerance = 1.1f; // 10% tolerance for latency
    
    // Jump parameters
    public const float JumpHeight = 2.5f;     // meters
    public const int JumpStaminaCost = 10;
    public const int JumpCooldownMs = 500;
    
    // Fall damage
    public const float SafeFallHeight = 4.0f; // meters
    public const float DamagePerMeter = 50f;  // damage per meter above safe
    public const float LethalHeight = 20.0f;  // instant death
    
    // Networking
    public const int TickRateHz = 25;         // 40ms per tick
    public const int MaxPositionUpdatesHz = 20;
    public const int InterpolationBufferMs = 100;
    public const float AoIRange = 50.0f;      // meters
    
    // Anti-cheat
    public const int MaxSpeedViolations = 3;  // before kick
    public const int MaxCorrectionsPerMin = 10;
}
```

---

**Letzte Aktualisierung**: 2026-01-02  
**Version**: 2.0.0

[← Zurück zur Übersicht](Message-Reference.md)

Source: docs/03-messages/02-movement.md
