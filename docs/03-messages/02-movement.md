# 🏃 Movement / Position Messages (0200-0299)

**Kategorie:** 2  
**Range:** 0200-0299  
**Phase:** Prototyp  
**Status:** 🟢 In Entwicklung

[← Zurück zur Übersicht](README.md)

---

## 📋 Übersicht

Diese Kategorie umfasst alle Messages für **Bewegung und Positionierung** im 2DMMO.

Das Movement-System basiert auf **Client-Side Prediction** mit **Server-Authority**:
- Client sendet Input und predicted Position
- Server validiert gegen Speed/Collision
- Server broadcastet authoritative Position
- Bei Desync: Server sendet Correction

**Tick-Rate**: 25 Hz (40ms pro Tick)  
**Position-Update-Rate**: Bis zu 20 Hz (optimiert für Bandwidth)  
**Interpolation-Buffer**: ~100ms für smooth Movement anderer Spieler

---

## 📋 Inhaltsverzeichnis

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

---

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
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| SequenceNumber | uint | Aufsteigende Input-Sequence | Ja |
| Timestamp | long | Client Unix Timestamp (ms) | Ja |
| X | float | Position X-Koordinate | Ja |
| Y | float | Position Y-Koordinate | Ja |
| Z | float | Position Z-Koordinate (Höhe) | Ja |
| VelocityX | float | Velocity X | Ja |
| VelocityY | float | Velocity Y | Ja |
| VelocityZ | float | Velocity Z (Fallgeschwindigkeit) | Ja |
| Yaw | float | Rotation (0-360 Grad) | Ja |
| InputFlags | byte | Bit-Flags: Forward=1, Back=2, Left=4, Right=8, Jump=16, Sprint=32 | Ja |

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
| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `SPEED_TOO_HIGH` | Velocity > MAX_SPEED * TOLERANCE | Disconnect bei 3+ Verstößen |
| `POSITION_OUT_OF_BOUNDS` | Position außerhalb der Zone | Teleport zur letzten gültigen Position |
| `COLLISION_INVALID` | Position in Wand/Objekt | Correction zur gültigen Position |
| `SEQUENCE_TOO_OLD` | SequenceNumber < erwartete | Ignorieren (Packet-Loss) |

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
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| SequenceNumber | uint | Input-Sequence die korrigiert wird | Ja |
| Reason | string | "collision", "speed_too_high", "bounds", "stuck" | Ja |
| X | float | Korrekte Position X | Ja |
| Y | float | Korrekte Position Y | Ja |
| Z | float | Korrekte Position Z | Ja |
| VelocityX | float | Korrekte Velocity X | Ja |
| VelocityY | float | Korrekte Velocity Y | Ja |
| VelocityZ | float | Korrekte Velocity Z | Ja |

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
- **Bei Erfolg:** `TeleportExecute` (204) → dann `JoinZone` (100) falls andere Zone
- **Bei Fehler:** `ErrorMessage` (910) mit Code

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

**Letzte Aktualisierung**: 2025-12-17  
**Version**: 1.0.0

[← Zurück zur Übersicht](README.md)
