# ⏱️ Cooldown / Timer Messages (3200-3299)

**Kategorie:** 32  
**Range:** 3200-3299  
**Phase:** Prototyp  
**Status:** 🟢 In Entwicklung

[← Zurück zur Übersicht](README.md)

---

## 📋 Inhaltsverzeichnis

- [📋 Überblick](#-überblick)
- [🧠 Datenmodell](#-datenmodell)
- [🗂️ Cooldown-Kategorien](#️-cooldown-kategorien)
- [🔄 Sync & Timing](#-sync--timing)
- [🧱 DTOs / Interfaces](#-dtos--interfaces)
- [🧩 Enums / ErrorCodes](#-enums--errorcodes)
- [⚙️ Regeln & Validierung](#️-regeln--validierung)
- [📩 Aktive Messages 3200-3299](#-aktive-messages-3200-3299)
- [🧨 Edge Cases & Fehlerfälle](#-edge-cases--fehlerfälle)
- [📎 Anhang](#-anhang)

---

## 📋 Überblick

Diese Kategorie behandelt alle **Cooldown-, Cast- und Timer-Messages** im 2DMMO-System.

### Ziele

- **Ability Cooldowns**: Verwalten von Abklingzeiten für Skills und Items
- **Global Cooldown (GCD)**: Systemweite Abklingzeit nach Ability-Nutzung
- **Cast-System**: Zauber mit Wirkzeit (Cast Time)
- **Channel-System**: Kanalisierte Fähigkeiten mit periodischen Ticks
- **Charge-System**: Fähigkeiten mit aufladbaren Ladungen

### Server-Autorität

Der Server ist **IMMER** autoritativ für:
- Cooldown-Start und -Ende
- Cast-Validierung und -Unterbrechung
- Channel-Ticks und -Abschluss
- Charge-Regeneration

---

## 🧠 Datenmodell

### CooldownState

```csharp
public class CooldownState
{
    public uint AbilityId { get; set; }
    public long StartTimestamp { get; set; }
    public int DurationMs { get; set; }
    public int RemainingMs { get; set; }
    public CooldownCategory Category { get; set; }
}
```

### CastState

```csharp
public class CastState
{
    public uint AbilityId { get; set; }
    public Guid CasterId { get; set; }
    public Guid? TargetId { get; set; }
    public Position? TargetPosition { get; set; }
    public long StartTimestamp { get; set; }
    public int CastTimeMs { get; set; }
    public int ProgressMs { get; set; }
    public bool IsChanneled { get; set; }
}
```

### ChargeState

```csharp
public class ChargeState
{
    public uint AbilityId { get; set; }
    public int CurrentCharges { get; set; }
    public int MaxCharges { get; set; }
    public int RechargeTimeMs { get; set; }
    public long NextChargeTimestamp { get; set; }
}
```

---

## 🗂️ Cooldown-Kategorien

| Kategorie | Beschreibung | Beispiel |
|-----------|--------------|----------|
| `Ability` | Standard-Skill-Cooldown | Fireball: 3s CD |
| `Item` | Gegenstand-Cooldown | Health Potion: 60s CD |
| `Global` | Global Cooldown (GCD) | 1.5s nach jeder Ability |
| `Shared` | Geteilter Cooldown (mehrere Abilities) | Defensiv-CDs |
| `Charge` | Ladungs-basiert | Dash: 2 Charges, 10s Recharge |

---

## 🔄 Sync & Timing

### Timing-Konstanten

| Konstante | Wert | Beschreibung |
|-----------|------|--------------|
| `DEFAULT_GCD_MS` | 1500 | Standard Global Cooldown |
| `MIN_GCD_MS` | 750 | Minimaler GCD (mit Haste) |
| `TICK_RATE_MS` | 40 | Server-Tick-Rate |
| `CAST_TOLERANCE_MS` | 100 | Toleranz für Latenz |

### Sync-Strategie

1. **Initial Sync**: `CooldownSync` bei Login/Zone-Wechsel
2. **Event-basiert**: Individuelle Start/End Messages bei Änderungen
3. **Latenz-Kompensation**: Client zeigt vorhergesagte Zeiten, Server korrigiert

---

## 🧱 DTOs / Interfaces

### CooldownDto

```csharp
[MessagePackObject]
public class CooldownDto
{
    [Key(0)] public uint AbilityId { get; set; }
    [Key(1)] public int DurationMs { get; set; }
    [Key(2)] public int RemainingMs { get; set; }
    [Key(3)] public CooldownCategory Category { get; set; }
}
```

### CastProgressDto

```csharp
[MessagePackObject]
public class CastProgressDto
{
    [Key(0)] public uint AbilityId { get; set; }
    [Key(1)] public Guid CasterId { get; set; }
    [Key(2)] public int CastTimeMs { get; set; }
    [Key(3)] public int ProgressMs { get; set; }
    [Key(4)] public Guid? TargetId { get; set; }
}
```

---

## 🧩 Enums / ErrorCodes

### CooldownCategory

```csharp
public enum CooldownCategory : byte
{
    Ability = 1,
    Item = 2,
    Global = 3,
    Shared = 4,
    Charge = 5
}
```

### CastFailReason

```csharp
public enum CastFailReason : byte
{
    None = 0,
    Interrupted = 1,
    OutOfRange = 2,
    LineOfSight = 3,
    NotEnoughResource = 4,
    InvalidTarget = 5,
    OnCooldown = 6,
    Silenced = 7,
    Stunned = 8,
    Moving = 9,
    Dead = 10
}
```

### InterruptSource

```csharp
public enum InterruptSource : byte
{
    Self = 1,
    Enemy = 2,
    Environment = 3,
    Movement = 4,
    Stun = 5,
    Silence = 6,
    Knockback = 7
}
```

---

## ⚙️ Regeln & Validierung

### Cooldown-Regeln

1. **Cooldown-Start**: Server sendet `CooldownStart` wenn Ability verwendet wird
2. **Cooldown-Modifikation**: Haste/CDR wirkt multiplikativ auf Cooldown
3. **Cooldown-Reset**: Bestimmte Effekte können CDs zurücksetzen
4. **Shared Cooldowns**: Abilities in derselben Gruppe teilen CD

### Cast-Regeln

1. **Cast-Start**: Client sendet `ActionRequest`, Server validiert und broadcastet `CastStart`
2. **Cast-Unterbrechung**: Bei Bewegung, Schaden (wenn pushback), Stun, etc.
3. **Cast-Abschluss**: Server sendet `CastComplete` und führt Effekt aus

### Channel-Regeln

1. **Channel-Start**: Wie Cast, aber mit periodischen Ticks
2. **Channel-Ticks**: Server sendet `ChannelTick` für jeden Effekt-Tick
3. **Channel-Dauer**: Kann vorzeitig abgebrochen werden

---

## 📩 Aktive Messages 3200-3299

---

## CooldownStart (3200)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Server informiert Client, dass ein Cooldown für eine Ability beginnt.

### Im Scope ✅

- Cooldown für Abilities
- Cooldown für Items
- Shared Cooldowns (mehrere Abilities gleichzeitig)

### Nicht im Scope ❌

- Global Cooldown → verwende `GlobalCooldownStart` (3210)

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.CooldownStart` | Ja |
| AbilityId | uint | ID der Ability | Ja |
| DurationMs | int | Cooldown-Dauer in Millisekunden | Ja |
| Category | CooldownCategory | Kategorie des Cooldowns | Ja |
| SharedAbilityIds | List\<uint\>? | IDs von Abilities mit Shared CD | Nein |

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.CooldownStart)]
public class CooldownStart : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.CooldownStart;
    [Key(1)] public uint AbilityId { get; set; }
    [Key(2)] public int DurationMs { get; set; }
    [Key(3)] public CooldownCategory Category { get; set; }
    [Key(4)] public List<uint>? SharedAbilityIds { get; set; }
}
```

### Server-Verhalten

```csharp
public void StartCooldown(PlayerEntity player, uint abilityId, int durationMs)
{
    var ability = _abilityService.GetAbility(abilityId);
    var modifiedDuration = ApplyCooldownReduction(player, durationMs);
    
    player.Cooldowns[abilityId] = new CooldownState
    {
        AbilityId = abilityId,
        StartTimestamp = NetworkTime.Now,
        DurationMs = modifiedDuration,
        Category = ability.CooldownCategory
    };
    
    Send(player.ConnectionId, new CooldownStart
    {
        AbilityId = abilityId,
        DurationMs = modifiedDuration,
        Category = ability.CooldownCategory,
        SharedAbilityIds = ability.SharedCooldownGroup
    });
}
```

### Client-Verhalten

```csharp
public void OnCooldownStart(CooldownStart msg)
{
    _cooldownManager.StartCooldown(msg.AbilityId, msg.DurationMs);
    
    if (msg.SharedAbilityIds != null)
    {
        foreach (var sharedId in msg.SharedAbilityIds)
        {
            _cooldownManager.StartCooldown(sharedId, msg.DurationMs);
        }
    }
    
    _actionBar.UpdateCooldownDisplay(msg.AbilityId);
}
```

### Beispiel Payloads

```csharp
// Standard Ability Cooldown
var fireball = new CooldownStart
{
    AbilityId = 1001,
    DurationMs = 3000,
    Category = CooldownCategory.Ability
};

// Shared Cooldown (Defensiv-Abilities)
var defensiveStance = new CooldownStart
{
    AbilityId = 2001,
    DurationMs = 30000,
    Category = CooldownCategory.Shared,
    SharedAbilityIds = new List<uint> { 2002, 2003 }
};
```

### Verwandte Messages

| Message | ID | Beziehung |
|---------|-----|-----------|
| `CooldownEnd` | 3201 | Wenn Cooldown abläuft |
| `CooldownReset` | 3203 | Wenn CD vorzeitig zurückgesetzt |
| `ActionRequest` | 300 | Löst Cooldown aus |

---

## CooldownEnd (3201)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Server informiert Client, dass ein Cooldown abgelaufen ist. Wird nur gesendet wenn Client nicht korrekt predicten kann (z.B. bei Server-Korrektur).

### Im Scope ✅

- Explizite Cooldown-Ende Benachrichtigung
- Korrektur bei Client-Desync

### Nicht im Scope ❌

- Normale Cooldown-Enden (Client predictet lokal)

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.CooldownEnd` | Ja |
| AbilityId | uint | ID der Ability | Ja |

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.CooldownEnd)]
public class CooldownEnd : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.CooldownEnd;
    [Key(1)] public uint AbilityId { get; set; }
}
```

### Verwandte Messages

| Message | ID | Beziehung |
|---------|-----|-----------|
| `CooldownStart` | 3200 | Startet Cooldown |

---

## CooldownUpdate (3202)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Server korrigiert einen laufenden Cooldown (z.B. durch CDR-Buff oder Debuff).

### Im Scope ✅

- Cooldown-Modifikation durch Buffs/Debuffs
- Cooldown-Korrektur bei Desync

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.CooldownUpdate` | Ja |
| AbilityId | uint | ID der Ability | Ja |
| NewDurationMs | int | Neue Gesamtdauer | Ja |
| RemainingMs | int | Verbleibende Zeit | Ja |

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.CooldownUpdate)]
public class CooldownUpdate : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.CooldownUpdate;
    [Key(1)] public uint AbilityId { get; set; }
    [Key(2)] public int NewDurationMs { get; set; }
    [Key(3)] public int RemainingMs { get; set; }
}
```

---

## CooldownReset (3203)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Server setzt einen Cooldown komplett zurück (z.B. durch bestimmte Abilities oder Kills).

### Im Scope ✅

- Reset durch spezielle Abilities
- Reset durch Kills (z.B. "Reset on Kill" Passive)
- Reset durch Items

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.CooldownReset` | Ja |
| AbilityId | uint | ID der Ability | Ja |
| ResetSource | CooldownResetSource | Grund des Resets | Ja |

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.CooldownReset)]
public class CooldownReset : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.CooldownReset;
    [Key(1)] public uint AbilityId { get; set; }
    [Key(2)] public CooldownResetSource ResetSource { get; set; }
}

public enum CooldownResetSource : byte
{
    Ability = 1,
    Kill = 2,
    Item = 3,
    Buff = 4,
    Admin = 5
}
```

---

## CooldownSync (3204)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten (Login, Zone-Wechsel, Reconnect)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Server sendet vollständige Liste aller aktiven Cooldowns. Wird bei Login, Zone-Wechsel oder Reconnect gesendet.

### Im Scope ✅

- Vollständiger Cooldown-State
- Initial-Sync nach Login
- Resync nach Reconnect

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.CooldownSync` | Ja |
| Cooldowns | List\<CooldownDto\> | Alle aktiven Cooldowns | Ja |
| ServerTimestamp | long | Server-Zeit für Sync | Ja |

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.CooldownSync)]
public class CooldownSync : IServerMessage, ITimestampedMessage
{
    [Key(0)] public MessageType Type => MessageType.CooldownSync;
    [Key(1)] public List<CooldownDto> Cooldowns { get; set; } = new();
    [Key(2)] public long Timestamp { get; set; }
}
```

### Beispiel Payload

```csharp
var sync = new CooldownSync
{
    Cooldowns = new List<CooldownDto>
    {
        new CooldownDto
        {
            AbilityId = 1001,
            DurationMs = 3000,
            RemainingMs = 1500,
            Category = CooldownCategory.Ability
        },
        new CooldownDto
        {
            AbilityId = 5001,
            DurationMs = 60000,
            RemainingMs = 45000,
            Category = CooldownCategory.Item
        }
    },
    Timestamp = NetworkTime.Now
};
```

---

## GlobalCooldownStart (3210)

**Richtung:** 📥 Server → Client  
**Frequenz:** Sehr häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Server startet Global Cooldown (GCD). Verhindert Nutzung aller GCD-gebundenen Abilities für kurze Zeit.

### Im Scope ✅

- GCD nach Ability-Nutzung
- Haste-modifizierter GCD

### Nicht im Scope ❌

- Off-GCD Abilities (haben eigene Cooldowns)

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.GlobalCooldownStart` | Ja |
| DurationMs | int | GCD-Dauer (1500ms Standard, min 750ms) | Ja |
| TriggerAbilityId | uint | Ability die GCD ausgelöst hat | Ja |

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.GlobalCooldownStart)]
public class GlobalCooldownStart : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.GlobalCooldownStart;
    [Key(1)] public int DurationMs { get; set; }
    [Key(2)] public uint TriggerAbilityId { get; set; }
}
```

### Server-Verhalten

```csharp
public int CalculateGCD(PlayerEntity player)
{
    var baseGcd = 1500; // ms
    var hastePercent = player.Stats.Haste;
    var modifiedGcd = (int)(baseGcd / (1 + hastePercent / 100f));
    return Math.Max(750, modifiedGcd); // Min 750ms
}
```

---

## GlobalCooldownEnd (3211)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten (nur bei Korrektur)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Server beendet GCD explizit (nur bei Desync-Korrektur, normalerweise predictet Client).

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.GlobalCooldownEnd` | Ja |

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.GlobalCooldownEnd)]
public class GlobalCooldownEnd : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.GlobalCooldownEnd;
}
```

---

## CastStart (3220)

**Richtung:** 📡 Broadcast (Server → All Clients in Range)  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Server broadcastet, dass ein Charakter einen Cast beginnt. Andere Spieler sehen Castbar.

### Im Scope ✅

- Cast-Start Notification
- Castbar-Anzeige für alle in Range
- Target-Info für gezielte Spells

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.CastStart` | Ja |
| CasterId | Guid | ID des Casters | Ja |
| AbilityId | uint | ID der Ability | Ja |
| CastTimeMs | int | Gesamte Cast-Zeit | Ja |
| TargetId | Guid? | Ziel-Entity (falls targeted) | Nein |
| TargetPosition | Position? | Ziel-Position (falls AoE) | Nein |

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.CastStart)]
public class CastStart : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.CastStart;
    [Key(1)] public Guid CasterId { get; set; }
    [Key(2)] public uint AbilityId { get; set; }
    [Key(3)] public int CastTimeMs { get; set; }
    [Key(4)] public Guid? TargetId { get; set; }
    [Key(5)] public Position? TargetPosition { get; set; }
}
```

### Flow-Diagramm

```
Client                         Server                      Other Clients
  │                              │                              │
  │  ActionRequest (300)         │                              │
  │  AbilityId: 1001             │                              │
  │─────────────────────────────►│                              │
  │                              │                              │
  │                              │  [Validate]                  │
  │                              │  - Not on CD                 │
  │                              │  - Has Resources             │
  │                              │  - In Range                  │
  │                              │                              │
  │  CastStart (3220)            │  CastStart (3220)            │
  │◄─────────────────────────────│─────────────────────────────►│
  │                              │                              │
  │  [Show Castbar]              │                              │  [Show Enemy Castbar]
```

### Verwandte Messages

| Message | ID | Beziehung |
|---------|-----|-----------|
| `ActionRequest` | 300 | Client-Request der Cast auslöst |
| `CastUpdate` | 3221 | Progress-Update |
| `CastComplete` | 3223 | Cast abgeschlossen |
| `CastInterrupt` | 3222 | Cast unterbrochen |
| `CastFailed` | 3224 | Cast fehlgeschlagen |

---

## CastUpdate (3221)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten (nur bei Haste-Änderung während Cast)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Server korrigiert Cast-Progress (z.B. bei Haste-Buff während Cast).

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.CastUpdate` | Ja |
| AbilityId | uint | ID der Ability | Ja |
| NewCastTimeMs | int | Neue Gesamtdauer | Ja |
| ProgressMs | int | Aktueller Progress | Ja |

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.CastUpdate)]
public class CastUpdate : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.CastUpdate;
    [Key(1)] public uint AbilityId { get; set; }
    [Key(2)] public int NewCastTimeMs { get; set; }
    [Key(3)] public int ProgressMs { get; set; }
}
```

---

## CastInterrupt (3222)

**Richtung:** 📡 Broadcast  
**Frequenz:** Mittel  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Server broadcastet, dass ein Cast unterbrochen wurde.

### Im Scope ✅

- Unterbrechung durch Schaden (Pushback)
- Unterbrechung durch Stun/Silence
- Unterbrechung durch Bewegung
- Unterbrechung durch eigenen Abbruch

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.CastInterrupt` | Ja |
| CasterId | Guid | ID des Casters | Ja |
| AbilityId | uint | ID der unterbrochenen Ability | Ja |
| InterruptSource | InterruptSource | Grund der Unterbrechung | Ja |
| InterrupterId | Guid? | ID des Unterbrechers (falls Enemy) | Nein |

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.CastInterrupt)]
public class CastInterrupt : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.CastInterrupt;
    [Key(1)] public Guid CasterId { get; set; }
    [Key(2)] public uint AbilityId { get; set; }
    [Key(3)] public InterruptSource InterruptSource { get; set; }
    [Key(4)] public Guid? InterrupterId { get; set; }
}
```

### Beispiel Payloads

```csharp
// Interrupt durch Spieler
var playerInterrupt = new CastInterrupt
{
    CasterId = targetId,
    AbilityId = 1001,
    InterruptSource = InterruptSource.Enemy,
    InterrupterId = attackerId
};

// Interrupt durch Bewegung
var moveInterrupt = new CastInterrupt
{
    CasterId = casterId,
    AbilityId = 1001,
    InterruptSource = InterruptSource.Movement
};
```

---

## CastComplete (3223)

**Richtung:** 📡 Broadcast  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Server broadcastet, dass ein Cast erfolgreich abgeschlossen wurde. Effekt wird ausgeführt.

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.CastComplete` | Ja |
| CasterId | Guid | ID des Casters | Ja |
| AbilityId | uint | ID der Ability | Ja |

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.CastComplete)]
public class CastComplete : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.CastComplete;
    [Key(1)] public Guid CasterId { get; set; }
    [Key(2)] public uint AbilityId { get; set; }
}
```

---

## CastFailed (3224)

**Richtung:** 📥 Server → Client (nur Caster)  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Server informiert Client, dass Cast fehlgeschlagen ist (vor Start oder Validierungsfehler).

### Im Scope ✅

- Nicht genug Ressourcen
- Ziel außer Reichweite
- Ziel nicht sichtbar (LoS)
- Ability auf Cooldown
- Spieler gestunned/silenced

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.CastFailed` | Ja |
| AbilityId | uint | ID der Ability | Ja |
| FailReason | CastFailReason | Grund des Fehlschlags | Ja |
| ErrorMessage | string? | Optionale Fehlermeldung | Nein |

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.CastFailed)]
public class CastFailed : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.CastFailed;
    [Key(1)] public uint AbilityId { get; set; }
    [Key(2)] public CastFailReason FailReason { get; set; }
    [Key(3)] public string? ErrorMessage { get; set; }
}
```

### Error Codes

| FailReason | Bedeutung | UI-Nachricht |
|------------|-----------|--------------|
| `OutOfRange` | Ziel zu weit entfernt | "Out of range" |
| `LineOfSight` | Kein Sichtkontakt | "Target not in line of sight" |
| `NotEnoughResource` | Nicht genug Mana/Energy | "Not enough mana" |
| `InvalidTarget` | Ungültiges Ziel | "Invalid target" |
| `OnCooldown` | Ability auf CD | "Ability not ready" |
| `Silenced` | Spieler ist gesilenced | "Can't do that while silenced" |
| `Stunned` | Spieler ist gestunned | "Can't do that while stunned" |
| `Moving` | Bewegung verhindert Cast | "Can't cast while moving" |

---

## ChannelStart (3230)

**Richtung:** 📡 Broadcast  
**Frequenz:** Mittel  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Server broadcastet Start eines Channels (kanalisierte Fähigkeit mit periodischen Effekten).

### Im Scope ✅

- Kanalisierte Abilities (z.B. Drain Life)
- Beam-Spells
- AoE-Channels

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.ChannelStart` | Ja |
| CasterId | Guid | ID des Casters | Ja |
| AbilityId | uint | ID der Ability | Ja |
| DurationMs | int | Channel-Dauer | Ja |
| TickIntervalMs | int | Intervall zwischen Ticks | Ja |
| TargetId | Guid? | Ziel-Entity | Nein |

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.ChannelStart)]
public class ChannelStart : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.ChannelStart;
    [Key(1)] public Guid CasterId { get; set; }
    [Key(2)] public uint AbilityId { get; set; }
    [Key(3)] public int DurationMs { get; set; }
    [Key(4)] public int TickIntervalMs { get; set; }
    [Key(5)] public Guid? TargetId { get; set; }
}
```

---

## ChannelTick (3231)

**Richtung:** 📡 Broadcast  
**Frequenz:** Häufig (während Channel)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Server broadcastet einen Channel-Tick (periodischer Effekt).

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.ChannelTick` | Ja |
| CasterId | Guid | ID des Casters | Ja |
| AbilityId | uint | ID der Ability | Ja |
| TickNumber | int | Aktueller Tick (1, 2, 3, ...) | Ja |
| RemainingMs | int | Verbleibende Channel-Zeit | Ja |

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.ChannelTick)]
public class ChannelTick : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.ChannelTick;
    [Key(1)] public Guid CasterId { get; set; }
    [Key(2)] public uint AbilityId { get; set; }
    [Key(3)] public int TickNumber { get; set; }
    [Key(4)] public int RemainingMs { get; set; }
}
```

---

## ChannelInterrupt (3232)

**Richtung:** 📡 Broadcast  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Server broadcastet, dass ein Channel unterbrochen wurde.

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.ChannelInterrupt` | Ja |
| CasterId | Guid | ID des Casters | Ja |
| AbilityId | uint | ID der Ability | Ja |
| InterruptSource | InterruptSource | Grund der Unterbrechung | Ja |
| TicksCompleted | int | Anzahl abgeschlossener Ticks | Ja |

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.ChannelInterrupt)]
public class ChannelInterrupt : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.ChannelInterrupt;
    [Key(1)] public Guid CasterId { get; set; }
    [Key(2)] public uint AbilityId { get; set; }
    [Key(3)] public InterruptSource InterruptSource { get; set; }
    [Key(4)] public int TicksCompleted { get; set; }
}
```

---

## ChannelComplete (3233)

**Richtung:** 📡 Broadcast  
**Frequenz:** Mittel  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Server broadcastet, dass ein Channel vollständig abgeschlossen wurde.

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.ChannelComplete` | Ja |
| CasterId | Guid | ID des Casters | Ja |
| AbilityId | uint | ID der Ability | Ja |
| TotalTicks | int | Gesamtzahl Ticks | Ja |

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.ChannelComplete)]
public class ChannelComplete : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.ChannelComplete;
    [Key(1)] public Guid CasterId { get; set; }
    [Key(2)] public uint AbilityId { get; set; }
    [Key(3)] public int TotalTicks { get; set; }
}
```

---

## ChargeUpdate (3240)

**Richtung:** 📥 Server → Client  
**Frequenz:** Mittel  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Server aktualisiert Charge-Status einer Ability (z.B. nach Nutzung oder Aufladen).

### Im Scope ✅

- Charge-Verbrauch bei Nutzung
- Charge-Status Update
- Multi-Charge Abilities

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.ChargeUpdate` | Ja |
| AbilityId | uint | ID der Ability | Ja |
| CurrentCharges | int | Aktuelle Ladungen | Ja |
| MaxCharges | int | Maximale Ladungen | Ja |
| RechargeTimeMs | int | Zeit bis nächste Ladung | Ja |
| NextChargeTimestamp | long | Timestamp für nächste Ladung | Ja |

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.ChargeUpdate)]
public class ChargeUpdate : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.ChargeUpdate;
    [Key(1)] public uint AbilityId { get; set; }
    [Key(2)] public int CurrentCharges { get; set; }
    [Key(3)] public int MaxCharges { get; set; }
    [Key(4)] public int RechargeTimeMs { get; set; }
    [Key(5)] public long NextChargeTimestamp { get; set; }
}
```

### Beispiel Payload

```csharp
// Dash-Ability: 1 von 2 Ladungen verbraucht
var dashCharge = new ChargeUpdate
{
    AbilityId = 3001,
    CurrentCharges = 1,
    MaxCharges = 2,
    RechargeTimeMs = 10000,
    NextChargeTimestamp = NetworkTime.Now + 10000
};
```

---

## ChargeRestore (3241)

**Richtung:** 📥 Server → Client  
**Frequenz:** Mittel  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Server informiert über Wiederherstellung einer Charge.

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.ChargeRestore` | Ja |
| AbilityId | uint | ID der Ability | Ja |
| NewChargeCount | int | Neue Anzahl Ladungen | Ja |
| MaxCharges | int | Maximale Ladungen | Ja |
| NextRechargeMs | int? | Zeit bis nächste (falls nicht max) | Nein |

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.ChargeRestore)]
public class ChargeRestore : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.ChargeRestore;
    [Key(1)] public uint AbilityId { get; set; }
    [Key(2)] public int NewChargeCount { get; set; }
    [Key(3)] public int MaxCharges { get; set; }
    [Key(4)] public int? NextRechargeMs { get; set; }
}
```

---

## 🧨 Edge Cases & Fehlerfälle

### Cast während Bewegung

```
Szenario: Spieler bewegt sich während Cast
Erwartung: CastInterrupt mit InterruptSource.Movement
Ausnahme: Instant-Casts und bestimmte Abilities erlauben Bewegung
```

### Cooldown während Disconnect

```
Szenario: Spieler disconnected mit laufendem Cooldown
Erwartung: Bei Reconnect wird CooldownSync gesendet mit korrekter verbleibender Zeit
Server-Verhalten: Cooldowns laufen auch offline weiter
```

### GCD-Stacking

```
Szenario: Ability wird während GCD verwendet
Erwartung: CastFailed mit FailReason.OnCooldown
Client-Verhalten: Ability-Button ist ausgegraut während GCD
```

### Channel-Unterbrechung bei Ziel-Tod

```
Szenario: Channel-Ziel stirbt während Channel
Erwartung: ChannelInterrupt mit InterruptSource.InvalidTarget (Annahme: neuer Enum-Wert)
Alternative: Channel endet automatisch, ChannelComplete mit reduzierter Tick-Anzahl
```

### Haste-Änderung während Cast

```
Szenario: Buff erhöht Haste während Cast
Erwartung: CastUpdate mit neuer CastTime, Progress wird proportional angepasst
Berechnung: newProgress = (oldProgress / oldCastTime) * newCastTime
```

---

## 📎 Anhang

### MessageType Enum (Kategorie 32)

```csharp
// ═══════════════════════════════════════════════════════════════
// COOLDOWNS / TIMERS (3200-3299)
// ═══════════════════════════════════════════════════════════════
CooldownStart = 3200,
CooldownEnd = 3201,
CooldownUpdate = 3202,
CooldownReset = 3203,
CooldownSync = 3204,
GlobalCooldownStart = 3210,
GlobalCooldownEnd = 3211,
CastStart = 3220,
CastUpdate = 3221,
CastInterrupt = 3222,
CastComplete = 3223,
CastFailed = 3224,
ChannelStart = 3230,
ChannelTick = 3231,
ChannelInterrupt = 3232,
ChannelComplete = 3233,
ChargeUpdate = 3240,
ChargeRestore = 3241,
```

### Neue Enums

```csharp
// Mmo.Shared/Cooldowns/Enums/CooldownCategory.cs
public enum CooldownCategory : byte
{
    Ability = 1,
    Item = 2,
    Global = 3,
    Shared = 4,
    Charge = 5
}

// Mmo.Shared/Cooldowns/Enums/CastFailReason.cs
public enum CastFailReason : byte
{
    None = 0,
    Interrupted = 1,
    OutOfRange = 2,
    LineOfSight = 3,
    NotEnoughResource = 4,
    InvalidTarget = 5,
    OnCooldown = 6,
    Silenced = 7,
    Stunned = 8,
    Moving = 9,
    Dead = 10
}

// Mmo.Shared/Cooldowns/Enums/InterruptSource.cs
public enum InterruptSource : byte
{
    Self = 1,
    Enemy = 2,
    Environment = 3,
    Movement = 4,
    Stun = 5,
    Silence = 6,
    Knockback = 7
}

// Mmo.Shared/Cooldowns/Enums/CooldownResetSource.cs
public enum CooldownResetSource : byte
{
    Ability = 1,
    Kill = 2,
    Item = 3,
    Buff = 4,
    Admin = 5
}
```

### Datei-Struktur

```
Mmo.Shared/
├── Cooldowns/
│   ├── Enums/
│   │   ├── CooldownCategory.cs
│   │   ├── CastFailReason.cs
│   │   ├── InterruptSource.cs
│   │   └── CooldownResetSource.cs
│   ├── Dtos/
│   │   ├── CooldownDto.cs
│   │   └── CastProgressDto.cs
│   └── Messages/
│       ├── Server→Client/
│       │   ├── CooldownStart.cs
│       │   ├── CooldownEnd.cs
│       │   ├── CooldownUpdate.cs
│       │   ├── CooldownReset.cs
│       │   ├── CooldownSync.cs
│       │   ├── GlobalCooldownStart.cs
│       │   ├── GlobalCooldownEnd.cs
│       │   ├── CastUpdate.cs
│       │   ├── CastFailed.cs
│       │   ├── ChargeUpdate.cs
│       │   └── ChargeRestore.cs
│       └── Server→Broadcast/
│           ├── CastStart.cs
│           ├── CastInterrupt.cs
│           ├── CastComplete.cs
│           ├── ChannelStart.cs
│           ├── ChannelTick.cs
│           ├── ChannelInterrupt.cs
│           └── ChannelComplete.cs
```

---

**Letzte Aktualisierung:** 2026-01-03  
**Version:** 3.0.0

[← Zurück zur Übersicht](README.md)
