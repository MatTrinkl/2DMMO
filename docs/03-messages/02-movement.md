# 🏃 Movement / Position Messages (0200-0299)

**Kategorie:** 02  
**Range:** 0200-0299  
**Phase:** Prototyp  
**Status:** 🟢 In Entwicklung

[← Zurück zur Übersicht](README.md)

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
Client sendet aktuelle Position, Velocity und Input. Server validiert und broadcastet.

### Im Scope ✅
- [Zu dokumentieren basierend auf konkreter Implementierung]

### Nicht im Scope ❌
- [Zu dokumentieren basierend auf konkreter Implementierung]

### Request/Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| [Felder basierend auf Implementation] | type | Beschreibung | Ja/Nein |

### Erwartete Response
- **Bei Erfolg:** [Response Message]
- **Bei Fehler:** `ErrorMessage` (910)

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| [Related] | ID | Beschreibung |

### Beispiel Payload
```csharp
var message = new PositionUpdate
{
    Type = MessageType.PositionUpdate,
    // Felder hier
};
```

### Notizen
- [Implementierungs-Hinweise]
- [Edge Cases]
- [Performance-Überlegungen]

---

## PositionBroadcast (201)

**Richtung:** 📡 Broadcast  
**Frequenz:** ⚡ High-Frequency  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Server broadcastet validierte Position eines Spielers an alle in Range.

### Im Scope ✅
- [Zu dokumentieren basierend auf konkreter Implementierung]

### Nicht im Scope ❌
- [Zu dokumentieren basierend auf konkreter Implementierung]

### Request/Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| [Felder basierend auf Implementation] | type | Beschreibung | Ja/Nein |

### Erwartete Response
- **Bei Erfolg:** [Response Message]
- **Bei Fehler:** `ErrorMessage` (910)

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| [Related] | ID | Beschreibung |

### Beispiel Payload
```csharp
var message = new PositionBroadcast
{
    Type = MessageType.PositionBroadcast,
    // Felder hier
};
```

### Notizen
- [Implementierungs-Hinweise]
- [Edge Cases]
- [Performance-Überlegungen]

---

## MovementCorrection (202)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Server korrigiert Client-Position bei Desync (Speed-Hack, Collision-Error).

### Im Scope ✅
- [Zu dokumentieren basierend auf konkreter Implementierung]

### Nicht im Scope ❌
- [Zu dokumentieren basierend auf konkreter Implementierung]

### Request/Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| [Felder basierend auf Implementation] | type | Beschreibung | Ja/Nein |

### Erwartete Response
- **Bei Erfolg:** [Response Message]
- **Bei Fehler:** `ErrorMessage` (910)

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| [Related] | ID | Beschreibung |

### Beispiel Payload
```csharp
var message = new MovementCorrection
{
    Type = MessageType.MovementCorrection,
    // Felder hier
};
```

### Notizen
- [Implementierungs-Hinweise]
- [Edge Cases]
- [Performance-Überlegungen]

---

## TeleportRequest (203)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Client bittet um Teleport (Hearthstone, Portal, Spell).

### Im Scope ✅
- [Zu dokumentieren basierend auf konkreter Implementierung]

### Nicht im Scope ❌
- [Zu dokumentieren basierend auf konkreter Implementierung]

### Request/Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| [Felder basierend auf Implementation] | type | Beschreibung | Ja/Nein |

### Erwartete Response
- **Bei Erfolg:** [Response Message]
- **Bei Fehler:** `ErrorMessage` (910)

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| [Related] | ID | Beschreibung |

### Beispiel Payload
```csharp
var message = new TeleportRequest
{
    Type = MessageType.TeleportRequest,
    // Felder hier
};
```

### Notizen
- [Implementierungs-Hinweise]
- [Edge Cases]
- [Performance-Überlegungen]

---

## TeleportExecute (204)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Server führt Teleport aus. Client setzt Position sofort.

### Im Scope ✅
- [Zu dokumentieren basierend auf konkreter Implementierung]

### Nicht im Scope ❌
- [Zu dokumentieren basierend auf konkreter Implementierung]

### Request/Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| [Felder basierend auf Implementation] | type | Beschreibung | Ja/Nein |

### Erwartete Response
- **Bei Erfolg:** [Response Message]
- **Bei Fehler:** `ErrorMessage` (910)

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| [Related] | ID | Beschreibung |

### Beispiel Payload
```csharp
var message = new TeleportExecute
{
    Type = MessageType.TeleportExecute,
    // Felder hier
};
```

### Notizen
- [Implementierungs-Hinweise]
- [Edge Cases]
- [Performance-Überlegungen]

---

## MovementSpeedUpdate (205)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Movement-Speed hat sich geändert (Buff, Debuff, Mount).

### Im Scope ✅
- [Zu dokumentieren basierend auf konkreter Implementierung]

### Nicht im Scope ❌
- [Zu dokumentieren basierend auf konkreter Implementierung]

### Request/Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| [Felder basierend auf Implementation] | type | Beschreibung | Ja/Nein |

### Erwartete Response
- **Bei Erfolg:** [Response Message]
- **Bei Fehler:** `ErrorMessage` (910)

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| [Related] | ID | Beschreibung |

### Beispiel Payload
```csharp
var message = new MovementSpeedUpdate
{
    Type = MessageType.MovementSpeedUpdate,
    // Felder hier
};
```

### Notizen
- [Implementierungs-Hinweise]
- [Edge Cases]
- [Performance-Überlegungen]

---

## JumpRequest (206)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Client initiiert Sprung.

### Im Scope ✅
- [Zu dokumentieren basierend auf konkreter Implementierung]

### Nicht im Scope ❌
- [Zu dokumentieren basierend auf konkreter Implementierung]

### Request/Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| [Felder basierend auf Implementation] | type | Beschreibung | Ja/Nein |

### Erwartete Response
- **Bei Erfolg:** [Response Message]
- **Bei Fehler:** `ErrorMessage` (910)

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| [Related] | ID | Beschreibung |

### Beispiel Payload
```csharp
var message = new JumpRequest
{
    Type = MessageType.JumpRequest,
    // Felder hier
};
```

### Notizen
- [Implementierungs-Hinweise]
- [Edge Cases]
- [Performance-Überlegungen]

---

## JumpBroadcast (207)

**Richtung:** 📡 Broadcast  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Server broadcastet Sprung eines Spielers.

### Im Scope ✅
- [Zu dokumentieren basierend auf konkreter Implementierung]

### Nicht im Scope ❌
- [Zu dokumentieren basierend auf konkreter Implementierung]

### Request/Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| [Felder basierend auf Implementation] | type | Beschreibung | Ja/Nein |

### Erwartete Response
- **Bei Erfolg:** [Response Message]
- **Bei Fehler:** `ErrorMessage` (910)

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| [Related] | ID | Beschreibung |

### Beispiel Payload
```csharp
var message = new JumpBroadcast
{
    Type = MessageType.JumpBroadcast,
    // Felder hier
};
```

### Notizen
- [Implementierungs-Hinweise]
- [Edge Cases]
- [Performance-Überlegungen]

---

## FallDamage (208)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Spieler nimmt Fall-Schaden (Server-berechnet).

### Im Scope ✅
- [Zu dokumentieren basierend auf konkreter Implementierung]

### Nicht im Scope ❌
- [Zu dokumentieren basierend auf konkreter Implementierung]

### Request/Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| [Felder basierend auf Implementation] | type | Beschreibung | Ja/Nein |

### Erwartete Response
- **Bei Erfolg:** [Response Message]
- **Bei Fehler:** `ErrorMessage` (910)

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| [Related] | ID | Beschreibung |

### Beispiel Payload
```csharp
var message = new FallDamage
{
    Type = MessageType.FallDamage,
    // Felder hier
};
```

### Notizen
- [Implementierungs-Hinweise]
- [Edge Cases]
- [Performance-Überlegungen]

---

## StuckRequest (209)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Spieler meldet 'stuck' Situation (in Wand, unter Terrain).

### Im Scope ✅
- [Zu dokumentieren basierend auf konkreter Implementierung]

### Nicht im Scope ❌
- [Zu dokumentieren basierend auf konkreter Implementierung]

### Request/Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| [Felder basierend auf Implementation] | type | Beschreibung | Ja/Nein |

### Erwartete Response
- **Bei Erfolg:** [Response Message]
- **Bei Fehler:** `ErrorMessage` (910)

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| [Related] | ID | Beschreibung |

### Beispiel Payload
```csharp
var message = new StuckRequest
{
    Type = MessageType.StuckRequest,
    // Felder hier
};
```

### Notizen
- [Implementierungs-Hinweise]
- [Edge Cases]
- [Performance-Überlegungen]

---

## StuckResponse (210)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Server teleportiert Spieler zu sicherer Position.

### Im Scope ✅
- [Zu dokumentieren basierend auf konkreter Implementierung]

### Nicht im Scope ❌
- [Zu dokumentieren basierend auf konkreter Implementierung]

### Request/Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| [Felder basierend auf Implementation] | type | Beschreibung | Ja/Nein |

### Erwartete Response
- **Bei Erfolg:** [Response Message]
- **Bei Fehler:** `ErrorMessage` (910)

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| [Related] | ID | Beschreibung |

### Beispiel Payload
```csharp
var message = new StuckResponse
{
    Type = MessageType.StuckResponse,
    // Felder hier
};
```

### Notizen
- [Implementierungs-Hinweise]
- [Edge Cases]
- [Performance-Überlegungen]

---

## PathfindingRequest (211)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung
Phase 2: Request für Auto-Pathing (Travel to NPC, etc.).

### Im Scope ✅
- [Zu dokumentieren basierend auf konkreter Implementierung]

### Nicht im Scope ❌
- [Zu dokumentieren basierend auf konkreter Implementierung]

### Request/Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| [Felder basierend auf Implementation] | type | Beschreibung | Ja/Nein |

### Erwartete Response
- **Bei Erfolg:** [Response Message]
- **Bei Fehler:** `ErrorMessage` (910)

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| [Related] | ID | Beschreibung |

### Beispiel Payload
```csharp
var message = new PathfindingRequest
{
    Type = MessageType.PathfindingRequest,
    // Felder hier
};
```

### Notizen
- [Implementierungs-Hinweise]
- [Edge Cases]
- [Performance-Überlegungen]

---

## PathfindingResponse (212)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung
Phase 2: Server sendet Path-Points.

### Im Scope ✅
- [Zu dokumentieren basierend auf konkreter Implementierung]

### Nicht im Scope ❌
- [Zu dokumentieren basierend auf konkreter Implementierung]

### Request/Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| [Felder basierend auf Implementation] | type | Beschreibung | Ja/Nein |

### Erwartete Response
- **Bei Erfolg:** [Response Message]
- **Bei Fehler:** `ErrorMessage` (910)

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| [Related] | ID | Beschreibung |

### Beispiel Payload
```csharp
var message = new PathfindingResponse
{
    Type = MessageType.PathfindingResponse,
    // Felder hier
};
```

### Notizen
- [Implementierungs-Hinweise]
- [Edge Cases]
- [Performance-Überlegungen]

---

## ForcePosition (213)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Server forciert Position (Admin, Anti-Cheat, Cutscene).

### Im Scope ✅
- [Zu dokumentieren basierend auf konkreter Implementierung]

### Nicht im Scope ❌
- [Zu dokumentieren basierend auf konkreter Implementierung]

### Request/Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| [Felder basierend auf Implementation] | type | Beschreibung | Ja/Nein |

### Erwartete Response
- **Bei Erfolg:** [Response Message]
- **Bei Fehler:** `ErrorMessage` (910)

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| [Related] | ID | Beschreibung |

### Beispiel Payload
```csharp
var message = new ForcePosition
{
    Type = MessageType.ForcePosition,
    // Felder hier
};
```

### Notizen
- [Implementierungs-Hinweise]
- [Edge Cases]
- [Performance-Überlegungen]

---

## MovementModeChange (214)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Movement-Mode ändert sich (Walking, Flying, Swimming).

### Im Scope ✅
- [Zu dokumentieren basierend auf konkreter Implementierung]

### Nicht im Scope ❌
- [Zu dokumentieren basierend auf konkreter Implementierung]

### Request/Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| [Felder basierend auf Implementation] | type | Beschreibung | Ja/Nein |

### Erwartete Response
- **Bei Erfolg:** [Response Message]
- **Bei Fehler:** `ErrorMessage` (910)

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| [Related] | ID | Beschreibung |

### Beispiel Payload
```csharp
var message = new MovementModeChange
{
    Type = MessageType.MovementModeChange,
    // Felder hier
};
```

### Notizen
- [Implementierungs-Hinweise]
- [Edge Cases]
- [Performance-Überlegungen]

---

## CollisionEvent (215)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Collision mit Environment (Wand, Objekt).

### Im Scope ✅
- [Zu dokumentieren basierend auf konkreter Implementierung]

### Nicht im Scope ❌
- [Zu dokumentieren basierend auf konkreter Implementierung]

### Request/Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| [Felder basierend auf Implementation] | type | Beschreibung | Ja/Nein |

### Erwartete Response
- **Bei Erfolg:** [Response Message]
- **Bei Fehler:** `ErrorMessage` (910)

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| [Related] | ID | Beschreibung |

### Beispiel Payload
```csharp
var message = new CollisionEvent
{
    Type = MessageType.CollisionEvent,
    // Felder hier
};
```

### Notizen
- [Implementierungs-Hinweise]
- [Edge Cases]
- [Performance-Überlegungen]

---

## KnockbackEvent (216)

**Richtung:** 📡 Broadcast  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Entity wird zurückgestoßen (Spell, Explosion).

### Im Scope ✅
- [Zu dokumentieren basierend auf konkreter Implementierung]

### Nicht im Scope ❌
- [Zu dokumentieren basierend auf konkreter Implementierung]

### Request/Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| [Felder basierend auf Implementation] | type | Beschreibung | Ja/Nein |

### Erwartete Response
- **Bei Erfolg:** [Response Message]
- **Bei Fehler:** `ErrorMessage` (910)

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| [Related] | ID | Beschreibung |

### Beispiel Payload
```csharp
var message = new KnockbackEvent
{
    Type = MessageType.KnockbackEvent,
    // Felder hier
};
```

### Notizen
- [Implementierungs-Hinweise]
- [Edge Cases]
- [Performance-Überlegungen]

---

## PullEvent (217)

**Richtung:** 📡 Broadcast  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Entity wird herangezogen (Spell, Hook).

### Im Scope ✅
- [Zu dokumentieren basierend auf konkreter Implementierung]

### Nicht im Scope ❌
- [Zu dokumentieren basierend auf konkreter Implementierung]

### Request/Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| [Felder basierend auf Implementation] | type | Beschreibung | Ja/Nein |

### Erwartete Response
- **Bei Erfolg:** [Response Message]
- **Bei Fehler:** `ErrorMessage` (910)

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| [Related] | ID | Beschreibung |

### Beispiel Payload
```csharp
var message = new PullEvent
{
    Type = MessageType.PullEvent,
    // Felder hier
};
```

### Notizen
- [Implementierungs-Hinweise]
- [Edge Cases]
- [Performance-Überlegungen]

---

## RootEvent (218)

**Richtung:** 📡 Broadcast  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Entity wird verwurzelt (Movement disabled).

### Im Scope ✅
- [Zu dokumentieren basierend auf konkreter Implementierung]

### Nicht im Scope ❌
- [Zu dokumentieren basierend auf konkreter Implementierung]

### Request/Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| [Felder basierend auf Implementation] | type | Beschreibung | Ja/Nein |

### Erwartete Response
- **Bei Erfolg:** [Response Message]
- **Bei Fehler:** `ErrorMessage` (910)

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| [Related] | ID | Beschreibung |

### Beispiel Payload
```csharp
var message = new RootEvent
{
    Type = MessageType.RootEvent,
    // Felder hier
};
```

### Notizen
- [Implementierungs-Hinweise]
- [Edge Cases]
- [Performance-Überlegungen]

---

## StunMovement (219)

**Richtung:** 📡 Broadcast  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Entity ist stunned (Movement + Actions disabled).

### Im Scope ✅
- [Zu dokumentieren basierend auf konkreter Implementierung]

### Nicht im Scope ❌
- [Zu dokumentieren basierend auf konkreter Implementierung]

### Request/Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| [Felder basierend auf Implementation] | type | Beschreibung | Ja/Nein |

### Erwartete Response
- **Bei Erfolg:** [Response Message]
- **Bei Fehler:** `ErrorMessage` (910)

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| [Related] | ID | Beschreibung |

### Beispiel Payload
```csharp
var message = new StunMovement
{
    Type = MessageType.StunMovement,
    // Felder hier
};
```

### Notizen
- [Implementierungs-Hinweise]
- [Edge Cases]
- [Performance-Überlegungen]

---


**Letzte Aktualisierung**: 2025-12-17  
**Version**: 1.0.0

[← Zurück zur Übersicht](README.md)
