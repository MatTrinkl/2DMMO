# ✨ Aura / Buff / Debuff Messages (1500-1516)

**Kategorie:** 15  
**Range:** 1500-1516 (AKTIV)  
**Phase:** Prototyp  
**Status:** 🟢 In Entwicklung

[← Zurück zur Übersicht](README.md)

---

## 📋 Inhaltsverzeichnis

- [🔄 Aura Flow](#-aura-flow)
  - [Server-Authoritative Architecture](#server-authoritative-architecture)
  - [Buff/Debuff Application Flow](#buffdebuff-application-flow)
  - [Dispel Flow](#dispel-flow)
- [🧱 DTOs / Enums / Interfaces](#-dtos--enums--interfaces)
  - [AuraType enum](#auratype-enum)
  - [RemovalReason enum](#removalreason-enum)
  - [ActiveAuraDto](#activeauradto)
  - [Wichtige Konstanten](#wichtige-konstanten)
- [📩 Aktive Messages (1500-1516)](#-aktive-messages-1500-1516)
  - [BuffApplied (1500)](#buffapplied-1500)
  - [BuffRemoved (1501)](#buffremoved-1501)
  - [BuffRefreshed (1502)](#buffrefreshed-1502)
  - [BuffStackUpdate (1503)](#buffstackupdate-1503)
  - [DebuffApplied (1504)](#debuffapplied-1504)
  - [DebuffRemoved (1505)](#debuffremoved-1505)
  - [AuraListSync (1506)](#auralistsync-1506)
  - [AuraUpdate (1507)](#auraupdate-1507)
  - [DispelRequest (1508)](#dispelrequest-1508)
  - [DispelResult (1509)](#dispelresult-1509)
  - [StealRequest (1510)](#stealrequest-1510)
  - [StealResult (1511)](#stealresult-1511)
  - [PurgeRequest (1512)](#purgerequest-1512)
  - [PurgeResult (1513)](#purgeresult-1513)
  - [AuraImmune (1514)](#auraimmune-1514)
  - [AuraResist (1515)](#auraresist-1515)
  - [BuffCategoryUpdate (1516)](#buffcategoryupdate-1516)
- [🗑️ Obsolete Messages](#️-obsolete-messages)
- [📎 Anhang](#-anhang)
  - [MessageType Enum](#messagetype-enum)
  - [Request/Response Paare](#requestresponse-paare)
  - [Datei-Struktur](#datei-struktur)

---

## 🔄 Aura Flow

### Server-Authoritative Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                    SERVER-AUTHORITY                              │
│                                                                  │
│  ┌──────────────┐    ┌──────────────┐    ┌──────────────┐       │
│  │ Aura-Manager │────│ Combat-Calc  │────│ Timer-System │       │
│  └──────────────┘    └──────────────┘    └──────────────┘       │
│         │                                        │               │
│         ▼                                        ▼               │
│  ┌────────────────────────────────────────────────┐             │
│  │              Aura-State (Server-Owned)         │             │
│  │  - EntityId → List<ActiveAura>                 │             │
│  │  - Duration-Tracking (ms)                      │             │
│  │  - Stack-Management                            │             │
│  │  - Immunity-Lists                              │             │
│  └────────────────────────────────────────────────┘             │
└─────────────────────────────────────────────────────────────────┘
                           │
                           ▼ Broadcast
            ┌──────────────┴──────────────┐
            │                             │
       ┌────▼────┐                   ┌────▼────┐
       │ Client1 │                   │ Client2 │
       │ (UI)    │                   │ (UI)    │
       └─────────┘                   └─────────┘
```

### Buff/Debuff Application Flow

```
Client                          Server                        Other Clients
   │                               │                               │
   │ [Skill casts Buff/Debuff]     │                               │
   │  CombatActionRequest(300)     │                               │
   │──────────────────────────────►│                               │
   │                               │                               │
   │                               │ Server validates:             │
   │                               │ - Caster has ability          │
   │                               │ - Target valid                │
   │                               │ - Not immune                  │
   │                               │ - Cooldown available          │
   │                               │                               │
   │                               │ ┌─────────────────────────┐   │
   │                               │ │ RESIST ROLL:            │   │
   │                               │ │ - Base resist chance    │   │
   │                               │ │ - Level difference      │   │
   │                               │ │ - Resistance stats      │   │
   │                               │ └─────────────────────────┘   │
   │                               │                               │
   │ [ON RESIST]                   │                               │
   │◄─────────────────────────────[│]─────────────────────────────►│
   │       AuraResist(1515)        │       AuraResist(1515)        │
   │                               │                               │
   │ [ON SUCCESS - Buff]           │                               │
   │◄─────────────────────────────[│]─────────────────────────────►│
   │       BuffApplied(1500)       │       BuffApplied(1500)       │
   │                               │                               │
   │ [ON SUCCESS - Debuff]         │                               │
   │◄─────────────────────────────[│]─────────────────────────────►│
   │       DebuffApplied(1504)     │       DebuffApplied(1504)     │
   │                               │                               │
   │ [ON IMMUNE]                   │                               │
   │◄─────────────────────────────[│]─────────────────────────────►│
   │       AuraImmune(1514)        │       AuraImmune(1514)        │
   │                               │                               │
```

### Dispel Flow

```
Healer                          Server                        Target/Others
   │                               │                               │
   │  DispelRequest(1508)          │                               │
   │  {TargetEntityId, DispelType} │                               │
   │──────────────────────────────►│                               │
   │                               │                               │
   │                               │ Server validates:             │
   │                               │ - In range                    │
   │                               │ - Has dispellable aura        │
   │                               │ - Dispel type matches         │
   │                               │ - Cooldown available          │
   │                               │                               │
   │ [ON SUCCESS]                  │                               │
   │◄──────────────────────────────│                               │
   │       DispelResult(1509)      │                               │
   │       {Success=true}          │                               │
   │                               │                               │
   │◄─────────────────────────────[│]─────────────────────────────►│
   │       DebuffRemoved(1505)     │       DebuffRemoved(1505)     │
   │       {Reason="dispelled"}    │       {Reason="dispelled"}    │
   │                               │                               │
   │ [ON NO AURAS]                 │                               │
   │◄──────────────────────────────│                               │
   │       DispelResult(1509)      │                               │
   │       {ErrorCode=             │                               │
   │        "NO_DISPELLABLE_AURAS"}│                               │
   │                               │                               │
```

---

## 🧱 DTOs / Enums / Interfaces

### AuraType enum

```csharp
public enum AuraType : byte
{
    Magic = 0,      // Magische Buffs/Debuffs (dispellable)
    Physical = 1,   // Physische Effekte (nicht dispellable)
    Curse = 2,      // Flüche (dispellable mit Remove Curse)
    Disease = 3,    // Krankheiten (dispellable mit Cleanse Disease)
    Poison = 4,     // Gifte (dispellable mit Cleanse Poison)
    Bleed = 5       // Blutungen (nicht dispellable)
}
```

### RemovalReason enum

```csharp
public enum RemovalReason : byte
{
    Expired = 0,    // Duration abgelaufen
    Dispelled = 1,  // Durch Dispel entfernt
    Cancelled = 2,  // Manuell abgebrochen
    Death = 3,      // Entfernt durch Tod
    Stolen = 4,     // Gestohlen (Spellsteal)
    Replaced = 5    // Ersetzt durch stärkeren Buff
}
```

### ActiveAuraDto

```csharp
[MessagePackObject]
public class ActiveAuraDto
{
    [Key(0)]
    public uint AuraId { get; set; }
    
    [Key(1)]
    public int CasterEntityId { get; set; }
    
    [Key(2)]
    public int RemainingDurationMs { get; set; }
    
    [Key(3)]
    public int Stacks { get; set; }
    
    [Key(4)]
    public bool IsBuff { get; set; }
    
    [Key(5)]
    public AuraType AuraType { get; set; }
    
    [Key(6)]
    public bool Dispellable { get; set; }
}
```

### Wichtige Konstanten

| Konstante | Wert | Beschreibung |
|-----------|------|--------------|
| `MAX_BUFFS_PER_ENTITY` | 40 | Max. Buffs pro Entity |
| `MAX_DEBUFFS_PER_ENTITY` | 16 | Max. Debuffs pro Entity |
| `DEFAULT_BUFF_DURATION_MS` | 1800000 | 30 Minuten |
| `MAX_STACKS` | 99 | Maximale Stack-Anzahl |
| `DISPEL_RANGE` | 40f | Reichweite für Dispel |
| `PURGE_COOLDOWN_MS` | 120000 | 2 Minuten Cooldown |

---

## 📩 Aktive Messages (1500-1516)

### BuffApplied (1500)

**Richtung:** 📡 Broadcast (Server → All in Range)  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Server informiert dass Buff auf Entity applied wurde. Kann selbst-cast (Self-Buff) oder von anderem Spieler sein.

### Im Scope ✅
- Buff-Application
- Duration und Stacks
- Caster-Information
- Visual-Effects

### Nicht im Scope ❌
- Debuffs → verwende `DebuffApplied` (1504)
- Buff-Removal → verwende `BuffRemoved` (1501)
- Buff-Refresh → verwende `BuffRefreshed` (1502)

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| TargetEntityId | int | Entity die Buff erhält | Ja |
| CasterEntityId | int | Entity die Buff castet | Ja |
| AuraId | uint | Aura/Buff-ID | Ja |
| Duration | int | Duration in Millisekunden (0 = permanent) | Ja |
| Stacks | int | Anzahl Stacks (1-N) | Ja |
| AuraType | string | "magic", "physical" | Ja |

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `BuffRemoved` | 1501 | Buff wird entfernt |
| `BuffRefreshed` | 1502 | Buff-Duration wird resettet |
| `BuffStackUpdate` | 1503 | Stack-Count ändert sich |
| `DebuffApplied` | 1504 | Hostile-Version |

### Beispiel Payload
```csharp
var buffApplied = new BuffApplied
{
    Type = MessageType.BuffApplied,
    TargetEntityId = 50001,
    CasterEntityId = 50002, // Heiler
    AuraId = 1001, // "Blessing of Might"
    Duration = 300000, // 5 Minuten
    Stacks = 1,
    AuraType = "magic"
};
```

### Notizen
- **Visual**: Client zeigt Buff-Icon in UI
- **Sound**: Buff-Application Sound
- **Particles**: Buff-Visual-Effect
- **Stacking**: Manche Buffs stacken (bis Max-Stacks)
- **Duration**: 0 = permanent bis manuell removed

---

### BuffRemoved (1501)

**Richtung:** 📡 Broadcast  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Buff wurde entfernt (Expired, Dispelled, Manually Cancelled).

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| TargetEntityId | int | Entity | Ja |
| AuraId | uint | Entfernter Buff | Ja |
| Reason | string | "expired", "dispelled", "cancelled", "death" | Ja |

### Beispiel Payload
```csharp
var buffRemoved = new BuffRemoved
{
    Type = MessageType.BuffRemoved,
    TargetEntityId = 50001,
    AuraId = 1001,
    Reason = "expired"
};
```

### Notizen
- **Visual**: Buff-Icon verschwindet
- **Death**: Alle Buffs werden removed bei Death
- **Zone-Change**: Buffs bleiben bei Zone-Change (außer Zone-Specific)

---

### BuffRefreshed (1502)

**Richtung:** 📡 Broadcast  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Buff-Duration wird resettet (Re-Application ohne Remove/Apply).

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| TargetEntityId | int | Entity | Ja |
| AuraId | uint | Refreshed Buff | Ja |
| NewDuration | int | Neue Duration (ms) | Ja |

### Beispiel Payload
```csharp
var buffRefreshed = new BuffRefreshed
{
    Type = MessageType.BuffRefreshed,
    TargetEntityId = 50001,
    AuraId = 1001,
    NewDuration = 300000
};
```

### Notizen
- **Visual**: Buff-Icon "blinkt"
- **Stacks**: Stacks bleiben erhalten bei Refresh
- **Efficiency**: Verhindert Remove→Apply Spam

---

### BuffStackUpdate (1503)

**Richtung:** 📡 Broadcast  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Stack-Count eines Buffs ändert sich (Increase/Decrease).

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| TargetEntityId | int | Entity | Ja |
| AuraId | uint | Stackable Buff | Ja |
| Stacks | int | Neue Stack-Count | Ja |
| MaxStacks | int | Max mögliche Stacks | Ja |

### Beispiel Payload
```csharp
var stackUpdate = new BuffStackUpdate
{
    Type = MessageType.BuffStackUpdate,
    TargetEntityId = 50001,
    AuraId = 1002, // "Sundering Armor" (-Armor Stack)
    Stacks = 3,
    MaxStacks = 5
};
```

### Notizen
- **Visual**: Stack-Number im Buff-Icon
- **Max-Stacks**: Bei Max → weitere Applications refreshen nur
- **Decrease**: Stacks können auch fallen (Consume-Effekte)

---

### DebuffApplied (1504)

**Richtung:** 📡 Broadcast  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Debuff (negative Effect) wurde applied. Analog zu BuffApplied aber hostile.

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| TargetEntityId | int | Entity die Debuff erhält | Ja |
| CasterEntityId | int | Entity die Debuff castet | Ja |
| AuraId | uint | Debuff-ID | Ja |
| Duration | int | Duration (ms) | Ja |
| Stacks | int | Anzahl Stacks | Ja |
| AuraType | string | "magic", "curse", "disease", "poison", "bleed" | Ja |
| Dispellable | bool | Kann dispelled werden? | Ja |

### Beispiel Payload
```csharp
var debuffApplied = new DebuffApplied
{
    Type = MessageType.DebuffApplied,
    TargetEntityId = 50001,
    CasterEntityId = 60001, // Boss
    AuraId = 2001, // "Shadow Curse"
    Duration = 10000, // 10 Sekunden
    Stacks = 1,
    AuraType = "curse",
    Dispellable = true
};
```

### Notizen
- **Visual**: Debuff-Icon mit rotem Border
- **Sound**: Debuff-Sound (negative)
- **Dispel-Types**: Magic, Curse, Disease, Poison können dispelled werden
- **Bleed**: Physical Debuff (nicht dispellable)

---

### DebuffRemoved (1505)

**Richtung:** 📡 Broadcast  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Debuff wurde entfernt.

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| TargetEntityId | int | Entity | Ja |
| AuraId | uint | Entfernter Debuff | Ja |
| Reason | string | "expired", "dispelled", "death" | Ja |

### Notizen
- **Death**: Alle Debuffs werden removed
- **Dispel**: Wird durch Dispel-Spells removed

---

### AuraListSync (1506)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten (Login, Reconnect)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Komplette Liste aller aktiven Auras (Buffs + Debuffs) auf Entity. Für Initial-Sync nach Login/Reconnect.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| EntityId | int | Entity-ID | Ja |
| Auras | List<ActiveAura> | Alle aktiven Auras | Ja |

**ActiveAura**:
| Feld | Typ | Beschreibung |
|------|-----|--------------|
| AuraId | uint | Aura-ID |
| CasterEntityId | int | Caster |
| Duration | int | Verbleibende Duration (ms) |
| Stacks | int | Stack-Count |
| IsBuff | bool | Buff (true) oder Debuff (false) |
| AuraType | string | Aura-Type |

### Beispiel Payload
```csharp
var auraSync = new AuraListSync
{
    Type = MessageType.AuraListSync,
    EntityId = 50001,
    Auras = new List<ActiveAura>
    {
        new ActiveAura
        {
            AuraId = 1001,
            CasterEntityId = 50002,
            Duration = 180000, // 3 Min remaining
            Stacks = 1,
            IsBuff = true,
            AuraType = "magic"
        },
        new ActiveAura
        {
            AuraId = 2001,
            CasterEntityId = 60001,
            Duration = 5000, // 5 Sek remaining
            Stacks = 2,
            IsBuff = false,
            AuraType = "curse"
        }
    }
};
```

### Notizen
- **Login**: Automatisch nach Login gesendet
- **Reconnect**: Bei Reconnect alle Auras resyncen
- **UI**: Client rendert Buff/Debuff-Frame

---

### AuraUpdate (1507)

**Richtung:** 📡 Broadcast  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Generische Aura-Update Message (Alternative zu spezifischen Messages).

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| EntityId | int | Entity | Ja |
| AuraId | uint | Updated Aura | Ja |
| UpdateType | string | "applied", "removed", "refreshed", "stack_change" | Ja |
| NewStacks | int | Bei stack_change | Nein |
| NewDuration | int | Bei refreshed | Nein |

### Notizen
- **Alternativ**: Kann statt spezifischer Messages verwendet werden
- **Batching**: Mehrere Updates können gebatched werden

---

### DispelRequest (1508)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Client castet Dispel-Spell um Debuff von Friendly oder Buff von Enemy zu entfernen.

### Im Scope ✅
- Debuff von Friendly dispellen
- Buff von Enemy dispellen
- Type-Specific Dispel (Magic, Curse, Disease, Poison)

### Nicht im Scope ❌
- Mass-Dispel → verwende `PurgeRequest` (1512)
- Buff-Steal → verwende `StealRequest` (1510)

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| TargetEntityId | int | Entity zum dispellen | Ja |
| DispelType | string | "magic", "curse", "disease", "poison" | Ja |

### Erwartete Response
- **Bei Erfolg:** `DispelResult` (1509) + `BuffRemoved/DebuffRemoved`
- **Bei Fehler:** `DispelResult` (1509) mit ErrorCode

### Beispiel Payload
```csharp
var dispelRequest = new DispelRequest
{
    Type = MessageType.DispelRequest,
    TargetEntityId = 50001,
    DispelType = "curse"
};
```

### Error Codes
| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `NO_DISPELLABLE_AURAS` | Keine dispellable Auras vom Type | Ignorieren |
| `OUT_OF_RANGE` | Target zu weit weg | Näher kommen |
| `COOLDOWN_ACTIVE` | Dispel auf Cooldown | Warten |

### Notizen
- **Priority**: Dispelt höchste Duration/Stack Aura first
- **Cooldown**: Dispels haben meist Cooldown
- **Cost**: Meist Mana-Cost

---

### DispelResult (1509)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung
Ergebnis des Dispel-Versuchs.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Success | bool | Dispel erfolgreich? | Ja |
| TargetEntityId | int | Target-Entity | Ja |
| RemovedAuraId | uint | Entfernte Aura-ID | Bei Success |
| ErrorCode | string | Error-Code | Bei Fehler |

### Beispiel Payload
```csharp
var dispelResult = new DispelResult
{
    Type = MessageType.DispelResult,
    Success = true,
    TargetEntityId = 50001,
    RemovedAuraId = 2001
};
```

### Notizen
- **Visual**: Dispel-Effect Animation
- **Sound**: Dispel-Sound
- **Resist**: Kann resistiert werden (dann Success=false)

---

### StealRequest (1510)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Buff-Steal Spell. Entfernt Buff von Enemy und applied auf Self.

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| TargetEntityId | int | Enemy mit Buff | Ja |

### Erwartete Response
- **Bei Erfolg:** `StealResult` (1511) + `BuffRemoved` (Enemy) + `BuffApplied` (Self)
- **Bei Fehler:** `StealResult` (1511) mit ErrorCode

### Notizen
- **Use-Case**: Mage-Ability "Spellsteal"
- **Stealable**: Nur Magic-Buffs
- **Duration**: Gestohlener Buff hat reduzierte Duration

---

### StealResult (1511)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung
Ergebnis des Buff-Steal-Versuchs.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Success | bool | Steal erfolgreich? | Ja |
| StolenAuraId | uint | Gestohlener Buff | Bei Success |
| ErrorCode | string | Error | Bei Fehler |

---

### PurgeRequest (1512)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Mass-Dispel. Entfernt ALLE Buffs von Enemy oder alle Debuffs von Friendly.

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| TargetEntityId | int | Entity zum purgen | Ja |
| PurgeType | string | "buffs" (von Enemy), "debuffs" (von Friendly) | Ja |

### Erwartete Response
- **Bei Erfolg:** `PurgeResult` (1513) + multiple `BuffRemoved/DebuffRemoved`

### Notizen
- **Cost**: Sehr hoher Mana-Cost
- **Cooldown**: Lange Cooldown (2-3 Minuten)
- **Use-Case**: Boss-Mechanic Counter, Emergency Heal

---

### PurgeResult (1513)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung
Ergebnis des Purge-Versuchs.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Success | bool | Purge erfolgreich? | Ja |
| RemovedCount | int | Anzahl entfernter Auras | Bei Success |

---

### AuraImmune (1514)

**Richtung:** 📡 Broadcast  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Entity ist immune gegen Aura-Type. Wird gesendet wenn Aura-Application fehlschlägt wegen Immunity.

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| EntityId | int | Immune Entity | Ja |
| AuraType | string | Immunity-Type | Ja |

### Beispiel Payload
```csharp
var auraImmune = new AuraImmune
{
    Type = MessageType.AuraImmune,
    EntityId = 60001, // Boss
    AuraType = "curse"
};
```

### Notizen
- **Visual**: "IMMUNE" Text
- **Use-Case**: Boss-Mechanics, Racial-Passives

---

### AuraResist (1515)

**Richtung:** 📡 Broadcast  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Entity resistiert Aura (Chance-Based). Aura wird nicht applied.

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| EntityId | int | Resisting Entity | Ja |
| AuraId | uint | Resistierte Aura | Ja |

### Beispiel Payload
```csharp
var auraResist = new AuraResist
{
    Type = MessageType.AuraResist,
    EntityId = 50001,
    AuraId = 2001
};
```

### Notizen
- **Visual**: "RESIST" Text
- **Chance**: Basiert auf Stats (Resistance, Level-Difference)

---

### BuffCategoryUpdate (1516)

**Richtung:** 📡 Broadcast  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Kategorie-basiertes Buff-Update. Für Buff-Gruppen (z.B. "Stat-Buffs", "Defensive-Buffs").

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| EntityId | int | Entity | Ja |
| Category | string | Buff-Category | Ja |
| ActiveBuffs | List<uint> | Aktive Buffs in Category | Ja |

### Notizen
- **Use-Case**: Buff-Stacking Rules
- **Categories**: "stats", "defensive", "offensive", "utility"

---

## 🗑️ Obsolete Messages

*Keine obsoleten Messages in dieser Kategorie.*

---

## 📎 Anhang

### MessageType Enum

```csharp
// BUFFS / DEBUFFS / AURAS (1500-1599)

BuffApplied = 1500,
BuffRemoved = 1501,
BuffRefreshed = 1502,
BuffStackUpdate = 1503,
DebuffApplied = 1504,
DebuffRemoved = 1505,
AuraListSync = 1506,
AuraUpdate = 1507,
DispelRequest = 1508,
DispelResult = 1509,
StealRequest = 1510,
StealResult = 1511,
PurgeRequest = 1512,
PurgeResult = 1513,
AuraImmune = 1514,
AuraResist = 1515,
BuffCategoryUpdate = 1516,
```

### Request/Response Paare

| Request | ID | Response | ID | Beschreibung |
|---------|-----|----------|-----|--------------|
| `DispelRequest` | 1508 | `DispelResult` | 1509 | Dispel Debuff/Buff |
| `StealRequest` | 1510 | `StealResult` | 1511 | Buff-Steal (Spellsteal) |
| `PurgeRequest` | 1512 | `PurgeResult` | 1513 | Mass-Dispel |

### Datei-Struktur

```
shared/Mmo.Shared/
├── Messaging/
│   ├── Enums/
│   │   └── MessageType.cs          # Aura Messages 1500-1516
│   └── Messages/
│       └── Aura/
│           ├── BuffApplied.cs
│           ├── BuffRemoved.cs
│           ├── BuffRefreshed.cs
│           ├── BuffStackUpdate.cs
│           ├── DebuffApplied.cs
│           ├── DebuffRemoved.cs
│           ├── AuraListSync.cs
│           ├── AuraUpdate.cs
│           ├── DispelRequest.cs
│           ├── DispelResult.cs
│           ├── StealRequest.cs
│           ├── StealResult.cs
│           ├── PurgeRequest.cs
│           ├── PurgeResult.cs
│           ├── AuraImmune.cs
│           ├── AuraResist.cs
│           └── BuffCategoryUpdate.cs
└── Entities/
    └── Aura/
        ├── ActiveAuraDto.cs
        ├── AuraType.cs
        └── RemovalReason.cs
```

---

**Letzte Aktualisierung**: 2026-01-02  
**Version**: 3.0.0

[← Zurück zur Übersicht](README.md)
