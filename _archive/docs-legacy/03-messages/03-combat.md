# ⚔️ Combat Messages (0300-0334)

**Kategorie:** 3  
**Range:** 0300-0334 (AKTIV)  
**Phase:** Prototyp  
**Status:** 🟢 In Entwicklung

[← Zurück zur Übersicht](README.md)

---

## 📋 Inhaltsverzeichnis

- [🔄 Combat Flow (Übersicht)](#-combat-flow-übersicht)
  - [Action-Request Flow](#action-request-flow)
  - [Damage/Heal Flow](#damageheal-flow)
  - [Threat/Aggro Flow](#threataggro-flow)
  - [Death/Resurrection Flow](#deathresurrection-flow)
- [🧱 DTOs / Enums / Interfaces](#-dtos--enums--interfaces)
  - [DamageType](#damagetype)
  - [DeathReason](#deathreason)
  - [RezType](#reztype)
  - [CombatResultType](#combatresulttype)
  - [ThreatEntry](#threatentry)
  - [AoETarget](#aoetarget)
- [📩 Aktive Messages (0300-0334)](#-aktive-messages-0300-0334)
  - [ActionRequest (300)](#actionrequest-300)
  - [ActionResult (301)](#actionresult-301)
  - [DamageEvent (302)](#damageevent-302)
  - [DeathEvent (303)](#deathevent-303)
  - [HealEvent (304)](#healevent-304)
  - [MissEvent (305)](#missevent-305)
  - [DodgeEvent (306)](#dodgeevent-306)
  - [ParryEvent (307)](#parryevent-307)
  - [BlockEvent (308)](#blockevent-308)
  - [CriticalHitEvent (309)](#criticalhitevent-309)
  - [CombatStart (310)](#combatstart-310)
  - [CombatEnd (311)](#combatend-311)
  - [ThreatUpdate (312)](#threatupdate-312)
  - [ThreatListRequest (313)](#threatlistrequest-313)
  - [InterruptEvent (314)](#interruptevent-314)
  - [ReflectEvent (315)](#reflectevent-315)
  - [AbsorbEvent (316)](#absorbevent-316)
  - [LifestealEvent (317)](#lifestealevent-317)
  - [ExecutePhase (318)](#executephase-318)
  - [EnrageEvent (319)](#enrageevent-319)
  - [CombatLogEntry (320)](#combatlogentry-320)
  - [AggroTransfer (321)](#aggrotransfer-321)
  - [TauntEvent (322)](#tauntevent-322)
  - [FeintEvent (323)](#feintevent-323)
  - [CounterAttack (324)](#counterattack-324)
  - [ComboFinisher (325)](#combofinisher-325)
  - [AreaDamage (326)](#areadamage-326)
  - [DamageOverTime (327)](#damageovertime-327)
  - [HealOverTime (328)](#healovertime-328)
  - [ShieldApplied (329)](#shieldapplied-329)
  - [ShieldBroken (330)](#shieldbroken-330)
  - [Resurrection (331)](#resurrection-331)
  - [CombatStateSync (332)](#combatstatesc-332)
  - [ThreatListResponse (333)](#threatlistresponse-333)
  - [ResurrectionResponse (334)](#resurrectionresponse-334)
- [🗑️ Obsolete Messages](#️-obsolete-messages)
- [📎 Anhang](#-anhang)
  - [MessageType Enum Updates](#messagetype-enum-updates)
  - [Neue Enums](#neue-enums)

---

## 🔄 Combat Flow (Übersicht)

Das Combat-System implementiert **server-authoritative** Kampfmechaniken. Der Server ist die einzige Autorität für alle Berechnungen. Der Client sendet Action-Requests, der Server validiert und broadcasted die Events an alle relevanten Clients.

### Kernprinzipien

```
┌─────────────────────────────────────────────────────────────────┐
│  SERVER-AUTHORITATIVE COMBAT                                    │
│  ├── Client: Sendet nur Requests (ActionRequest)                │
│  ├── Server: Validiert, berechnet, entscheidet                  │
│  ├── Server: Broadcasted Results an alle in Range               │
│  └── Client: Rendert Results (Damage Numbers, VFX, Sounds)      │
└─────────────────────────────────────────────────────────────────┘
```

### Combat-System Features

| Feature | Beschreibung | Messages |
|---------|--------------|----------|
| **Action System** | Abilities, Spells, Basic Attacks | 300, 301 |
| **Damage/Heal** | Damage Types, Critical Hits, Overkill | 302, 304, 309 |
| **Avoidance** | Miss, Dodge, Parry, Block | 305-308 |
| **Combat State** | In/Out Combat, Regen, Mount Restrictions | 310, 311 |
| **Threat/Aggro** | Threat Table, Taunt, Feint | 312, 313, 321-323 |
| **Advanced** | Reflect, Absorb, Lifesteal, Execute | 315-318 |
| **DoT/HoT** | Damage/Heal Over Time | 327, 328 |
| **Shields** | Absorption Shields | 316, 329, 330 |
| **Boss Mechanics** | Enrage Timer | 319 |
| **AoE** | Area Damage | 326 |
| **Combo** | Finisher-System | 325 |
| **Death/Rez** | Death, Resurrection | 303, 331, 334 |
| **Logging** | Combat Log für DPS-Meters | 320 |

### Action-Request Flow

```
Client                         Server                      All Clients
  │                              │                              │
  │  ActionRequest (300)         │                              │
  │  ├── ActionId: 123           │                              │
  │  ├── TargetId: 456           │                              │
  │  └── SequenceNumber: 789     │                              │
  │─────────────────────────────►│                              │
  │                              │                              │
  │                              │  ┌─ Validate Cooldown        │
  │                              │  ├─ Validate Range/LoS       │
  │                              │  ├─ Validate Resources       │
  │                              │  ├─ Validate Target          │
  │                              │  └─ Calculate Damage/Heal    │
  │                              │                              │
  │  ActionResult (301)          │                              │
  │◄─────────────────────────────│                              │
  │                              │                              │
  │                              │  DamageEvent (302)           │
  │◄─────────────────────────────│─────────────────────────────►│
  │                              │  (Broadcast to all in range) │
```

### Damage/Heal Flow

```
Server calculates combat outcome:

   ┌───────────────────────────────────────────────────────────┐
   │                    COMBAT CALCULATION                     │
   └───────────────────────────────────────────────────────────┘
                              │
                              ▼
                    ┌─────────────────┐
                    │  Hit-Check      │
                    │  Base: 95%      │
                    └────────┬────────┘
                             │
            ┌────────────────┼────────────────┐
            │                │                │
            ▼                ▼                ▼
       ┌────────┐       ┌────────┐       ┌────────┐
       │ MISS   │       │ DODGE  │       │ HIT    │
       │ (305)  │       │ (306)  │       │        │
       └────────┘       └────────┘       └───┬────┘
                                             │
                              ┌──────────────┼──────────────┐
                              │              │              │
                              ▼              ▼              ▼
                         ┌────────┐    ┌────────┐    ┌────────┐
                         │ PARRY  │    │ BLOCK  │    │ DAMAGE │
                         │ (307)  │    │ (308)  │    │ (302)  │
                         └────────┘    └────────┘    └───┬────┘
                                                        │
                                              ┌─────────┼─────────┐
                                              │         │         │
                                              ▼         ▼         ▼
                                         ┌────────┐ ┌────────┐ ┌────────┐
                                         │ NORMAL │ │ CRIT   │ │ KILL   │
                                         │        │ │ (309)  │ │ (303)  │
                                         └────────┘ └────────┘ └────────┘
```

### Threat/Aggro Flow

```
Party Fight:

Tank                Server                  Boss NPC
  │                    │                       │
  │  ActionRequest     │                       │
  │  (Attack Boss)     │                       │
  │───────────────────►│  +1000 Threat         │
  │                    │──────────────────────►│
  │                    │                       │
  │                    │  ThreatUpdate (312)   │
  │◄───────────────────│───────────────────────│
  │                    │                       │
  │                    │                       │
DPS                    │                       │
  │  ActionRequest     │                       │
  │  (Big Damage)      │                       │
  │───────────────────►│  +800 Threat          │
  │                    │──────────────────────►│
  │                    │                       │
  │                    │  ThreatUpdate (312)   │
  │◄───────────────────│───────────────────────│
  │                    │                       │
  │                    │                       │
                       │  If DPS > Tank:       │
                       │  AggroTransfer (321)  │
                       │◄──────────────────────│
                       │                       │
                       │  Boss attacks DPS!    │
```

### Death/Resurrection Flow

```
Player                  Server               All Clients
  │                        │                      │
  │  (HP reaches 0)        │                      │
  │                        │                      │
  │                        │  DeathEvent (303)    │
  │◄───────────────────────│─────────────────────►│
  │                        │                      │
  │  (Show Death UI)       │                      │
  │                        │                      │
  │  Resurrection (331)    │                      │
  │  RezType: Graveyard    │                      │
  │───────────────────────►│                      │
  │                        │                      │
  │  ResurrectionResponse  │                      │
  │  (334)                 │                      │
  │◄───────────────────────│                      │
  │                        │                      │
  │                        │  Resurrection Event  │
  │◄───────────────────────│─────────────────────►│
  │                        │  (Broadcast)         │
```

### Message-Übersicht

| Message | ID | Richtung | Frequenz | Zweck |
|---------|-----|----------|----------|-------|
| `ActionRequest` | 300 | C→S | Häufig | Initiiert Combat-Action |
| `ActionResult` | 301 | S→C | Häufig | Bestätigt Action-Ausführung |
| `DamageEvent` | 302 | Broadcast | ⚡ High | Damage-Notification |
| `DeathEvent` | 303 | Broadcast | Selten | Entity ist gestorben |
| `HealEvent` | 304 | Broadcast | Häufig | Healing-Notification |
| `MissEvent` | 305 | Broadcast | Häufig | Angriff verfehlt |
| `DodgeEvent` | 306 | Broadcast | Häufig | Ausweichen |
| `ParryEvent` | 307 | Broadcast | Häufig | Parieren |
| `BlockEvent` | 308 | Broadcast | Häufig | Blocken mit Shield |
| `CriticalHitEvent` | 309 | Broadcast | Häufig | Kritischer Treffer |
| `CombatStart` | 310 | S→C | Selten | Combat-State Enter |
| `CombatEnd` | 311 | S→C | Selten | Combat-State Exit |
| `ThreatUpdate` | 312 | S→Party | Häufig | Threat-Table Update |
| `ThreatListRequest` | 313 | C→S | Selten | Request Threat-Liste |
| `InterruptEvent` | 314 | Broadcast | Häufig | Cast interrupted |
| `ReflectEvent` | 315 | Broadcast | Selten | Damage reflektiert |
| `AbsorbEvent` | 316 | Broadcast | Häufig | Damage absorbiert |
| `LifestealEvent` | 317 | Broadcast | Häufig | HP durch Damage |
| `ExecutePhase` | 318 | Broadcast | Selten | Target <20% HP |
| `EnrageEvent` | 319 | Broadcast | Selten | Boss enraged |
| `CombatLogEntry` | 320 | S→C | ⚡ High | DPS-Meter Daten |
| `AggroTransfer` | 321 | Broadcast | Selten | Aggro wechselt |
| `TauntEvent` | 322 | Broadcast | Häufig | Tank tauntet |
| `FeintEvent` | 323 | Broadcast | Selten | Threat-Reduction |
| `CounterAttack` | 324 | Broadcast | Selten | Counter nach Parry |
| `ComboFinisher` | 325 | Broadcast | Häufig | Finisher-Move |
| `AreaDamage` | 326 | Broadcast | Häufig | AoE-Damage |
| `DamageOverTime` | 327 | Broadcast | ⚡ High | DoT-Tick |
| `HealOverTime` | 328 | Broadcast | ⚡ High | HoT-Tick |
| `ShieldApplied` | 329 | Broadcast | Häufig | Absorb-Shield |
| `ShieldBroken` | 330 | Broadcast | Häufig | Shield aufgebraucht |
| `Resurrection` | 331 | C→S/Broadcast | Selten | Respawn/Rez |
| `CombatStateSync` | 332 | S→C | Selten | Full-State nach Reconnect |
| `ThreatListResponse` | 333 | S→C | Selten | Threat-Liste Response |
| `ResurrectionResponse` | 334 | S→C | Selten | Rez-Request Response |

---

## 🧱 DTOs / Enums / Interfaces

### DamageType

**Zweck:** Typ des Schadens für Mitigation-Berechnung.

```csharp
public enum DamageType : byte
{
    Physical = 1,   // Reduziert durch Armor
    Magical = 2,    // Reduziert durch Magic Resistance
    True = 3        // Ignoriert alle Mitigation
}
```

**Verwendung:**
- Physical: Melee-Attacks, Ranged Physical
- Magical: Spells, Elemental Damage
- True: Percentage-Based, Execute-Damage

### DeathReason

**Zweck:** Grund für den Tod einer Entity.

```csharp
public enum DeathReason : byte
{
    Combat = 1,         // Getötet durch Entity
    FallDamage = 2,     // Zu tief gefallen
    Drowning = 3,       // Ertrunken
    Environment = 4,    // Lava, Gift, etc.
    Suicide = 5,        // /kill Command
    Disconnected = 6    // Disconnect während Combat
}
```

### RezType

**Zweck:** Art der Wiederbelebung.

```csharp
public enum RezType : byte
{
    GraveyardRespawn = 0,  // Normale Respawn am Graveyard
    AcceptBattleRez = 1,   // Battle-Rez akzeptieren
    SpiritHealer = 2       // Spirit Healer (mit Rez-Sickness)
}
```

**Eigenschaften:**

| RezType | HP% | Mana% | Cooldown | Debuff |
|---------|-----|-------|----------|--------|
| GraveyardRespawn | 50% | 50% | Keiner | Keiner |
| AcceptBattleRez | 30% | 20% | 10 min | Keiner |
| SpiritHealer | 100% | 100% | Keiner | 10 min Rez-Sickness |

### CombatResultType

**Zweck:** Ergebnis eines Combat-Checks.

```csharp
public enum CombatResultType : byte
{
    Hit = 1,
    Miss = 2,
    Dodge = 3,
    Parry = 4,
    Block = 5,
    Critical = 6,
    Absorb = 7,
    Reflect = 8
}
```

### ThreatEntry

**Zweck:** Eintrag in der Threat-Table.

```csharp
[MessagePackObject]
public class ThreatEntry
{
    [Key(0)] public Guid EntityId { get; set; }
    [Key(1)] public int ThreatAmount { get; set; }
    [Key(2)] public float Percentage { get; set; }  // % von Top-Threat
}
```

### AoETarget

**Zweck:** Target-Information für Area-Damage.

```csharp
[MessagePackObject]
public class AoETarget
{
    [Key(0)] public Guid TargetId { get; set; }
    [Key(1)] public int Damage { get; set; }
    [Key(2)] public bool IsCritical { get; set; }
}
```

---

## 📩 Aktive Messages (0300-0334)

---

## ActionRequest (300)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Client initiiert eine Combat-Action (Angriff, Spell, Ability). Der Server validiert die Action (Cooldown, Range, Line-of-Sight, Resource-Kosten, Target-Validity) und führt sie aus falls valide.

Diese Message ist das Herzstück des combat-Systems. Der Client sendet die gewünschte Action mit Target-Information, und der Server entscheidet authoritat, ob die Action ausgeführt wird.

### Im Scope ✅
- Initiierung einer Combat-Action vom Client
- Target-Angabe (Entity, Self, Ground-Position)
- Sequence Number für Client-Side Prediction
- Resource-Kosten werden vom Server geprüft

### Nicht im Scope ❌
- Damage-Berechnung → wird server-seitig durchgeführt, siehe `ActionResult` (301)
- Target-Auswahl UI → siehe `Targeting` Messages (1200-1299)
- Ability-Bar Management → Client-seitig

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| ActionId | uint | ID der Ability/Spell/Attack | Ja |
| TargetId | int | Entity-ID des Ziels (0 = kein Ziel) | Nein |
| TargetX | float | Ground-Target X-Position | Nein |
| TargetY | float | Ground-Target Y-Position | Nein |
| SequenceNumber | uint | Client Sequence für Prediction | Ja |

### Erwartete Response
- `ActionResult` (301) - bereits korrekt mit Success-Pattern!

### Folge-Messages bei Erfolg
- `DamageEvent` (302), `HealEvent` (304), etc. je nach Action

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `ActionResult` | 301 | Server bestätigt Action-Ausführung |
| `DamageEvent` | 302 | Follow-up bei Damage-Action |
| `HealEvent` | 304 | Follow-up bei Heal-Action |
| `MissEvent` | 305 | Follow-up bei Fehlschlag |

### Flow-Diagramm
```
Client                    Server
  │                          │
  │  ActionRequest (300)     │
  │  ActionId=123            │
  │  TargetId=456            │
  │  SequenceNumber=789      │
  │─────────────────────────►│
  │                          │  ┌─ Validate Cooldown
  │                          │  ├─ Validate Range/LoS
  │                          │  ├─ Validate Resources
  │                          │  ├─ Validate Target
  │                          │  └─ Execute Action
  │                          │
  │  ActionResult (301)      │
  │◄─────────────────────────│
  │                          │
  │  DamageEvent (302)       │  (Broadcast zu allen in Range)
  │◄─────────────────────────│
```

### Beispiel Payload
```csharp
var request = new ActionRequest
{
    Type = MessageType.ActionRequest,
    ActionId = 123, // Fireball Spell
    TargetId = 456, // Enemy Entity
    TargetX = 0,
    TargetY = 0,
    SequenceNumber = clientSequence++
};
```

### Error Codes
| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `ACTION_ON_COOLDOWN` | Ability ist noch auf Cooldown | Warte bis Cooldown abgelaufen |
| `OUT_OF_RANGE` | Ziel ist außerhalb der Reichweite | Näher herangehen |
| `NO_LINE_OF_SIGHT` | Keine Sichtlinie zum Ziel | Position ändern |
| `INSUFFICIENT_RESOURCES` | Nicht genug Mana/Energy/Rage | Warten auf Resource-Regeneration |
| `INVALID_TARGET` | Ziel ist ungültig (tot, friendly, etc.) | Anderes Ziel wählen |
| `NOT_IN_COMBAT` | Action erfordert Combat-State | Combat starten |
| `STUNNED` | Spieler ist gestunned | Warten bis Stun abläuft |

### Notizen
- **Client-Side Prediction**: Client kann Action sofort darstellen, muss aber auf Server-Bestätigung warten
- **Sequence Numbers**: Ermöglichen Reconciliation bei Prediction-Fehlern
- **Anti-Cheat**: Server validiert ALLE Parameter (Cooldown, Range, LoS, Resources)
- **Performance**: Rate-Limiting auf ~30 Actions/Sekunde pro Spieler
- **Range-Check**: Verwendet 2D-Distanz + Tolerance (5%)
- **LoS-Check**: Raycast gegen Collision-Layer
- **Cooldown**: Server-seitig getrackt, Client zeigt nur UI

---

## ActionResult (301)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Server bestätigt die Ausführung einer Action und sendet das Ergebnis zurück. Enthält Information ob die Action erfolgreich war, und triggert Cooldown/Resource-Consumption auf dem Client.

Diese Message schließt den Action-Request Loop ab und gibt dem Client Feedback für seine Prediction. Der Client kann nun seine lokale Simulation mit dem Server-State abgleichen.

### Im Scope ✅
- Bestätigung der Action-Ausführung
- Success/Failure Status
- Cooldown-Start auf Client
- Resource-Consumption Feedback
- Sequence Number Matching für Prediction

### Nicht im Scope ❌
- Damage-Werte → siehe `DamageEvent` (302)
- Healing-Werte → siehe `HealEvent` (304)
- Combat-Log Details → siehe `CombatLogEntry` (320)

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| ActionId | uint | ID der ausgeführten Action | Ja |
| SequenceNumber | uint | Matching Client Sequence | Ja |
| Success | bool | Ob Action erfolgreich war | Ja |
| CooldownMs | uint | Cooldown in Millisekunden | Ja |
| ResourceCost | int | Tatsächliche Resource-Kosten | Ja |

### Erwartete Response
- Keine direkte Response, aber Follow-up Events folgen (Damage, Heal, Miss, etc.)

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `ActionRequest` | 300 | Dieser Request wurde bestätigt |
| `DamageEvent` | 302 | Folgt bei Damage-Action |
| `HealEvent` | 304 | Folgt bei Heal-Action |
| `MissEvent` | 305 | Folgt bei Fehlschlag |

### Beispiel Payload
```csharp
var result = new ActionResult
{
    Type = MessageType.ActionResult,
    ActionId = 123,
    SequenceNumber = 789,
    Success = true,
    CooldownMs = 1500, // 1.5s Cooldown
    ResourceCost = 50 // 50 Mana
};
```

### Error Codes
Keine - Fehler werden via Success=false + ErrorMessage (910) kommuniziert

### Notizen
- **Reconciliation**: SequenceNumber ermöglicht Client, Prediction zu korrigieren
- **Cooldown**: Client startet Cooldown-Timer sofort nach Erhalt
- **Resource-Display**: Client aktualisiert Resource-Bar (Mana/Energy/Rage)
- **Latency**: Typische RTT für Action: 20-100ms
- **Animation**: Client kann Animation bereits bei ActionRequest starten

---

## DamageEvent (302)

**Richtung:** 📡 Broadcast (Server → Alle in Range)  
**Frequenz:** ⚡ High-Frequency  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Server broadcasted Damage an alle Spieler in Sichtweite. Enthält Source, Target, Damage-Amount und Damage-Type. Wird von Clients für Combat-Feedback (Floating Combat Text, Health-Bars) und Combat-Logs verwendet.

Diese Message ist das Resultat erfolgreicher Damage-Actions. Sie wird an alle Entities in Render-Distance gesendet, damit sie die Animation/VFX darstellen können.

### Im Scope ✅
- Damage-Amount und Type (Physical, Magical, True)
- Source und Target Entity-IDs
- Critical Hit Flag
- Overkill-Amount (für Kill-Events)
- Action-ID für Animation-Trigger

### Nicht im Scope ❌
- Miss/Dodge/Parry/Block → siehe entsprechende Events (305-308)
- DoT-Ticks → siehe `DamageOverTime` (327)
- AoE-Damage → siehe `AreaDamage` (326)
- Detailed Calculation → nur Result, nicht Formula

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| SourceId | int | Angreifer Entity-ID | Ja |
| TargetId | int | Opfer Entity-ID | Ja |
| Damage | int | Damage-Amount (nach Mitigation) | Ja |
| DamageType | enum | Physical/Magical/True | Ja |
| IsCritical | bool | Ob kritischer Treffer | Ja |
| Overkill | int | Damage über Tod hinaus (für Kills) | Nein |
| ActionId | uint | ID der Action für Animation | Ja |

### Erwartete Response
- Keine Response erforderlich
- Client stellt Damage dar (FCT, HP-Bar Update)

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `ActionRequest` | 300 | Action die zum Damage führte |
| `CriticalHitEvent` | 309 | Spezielle Crit-Notification |
| `DeathEvent` | 303 | Folgt wenn HP auf 0 |
| `DamageOverTime` | 327 | Für DoT-Ticks |
| `AreaDamage` | 326 | Für AoE-Damage |

### Beispiel Payload
```csharp
var dmgEvent = new DamageEvent
{
    Type = MessageType.DamageEvent,
    SourceId = 123, // Player
    TargetId = 456, // Enemy
    Damage = 250,
    DamageType = DamageType.Physical,
    IsCritical = false,
    Overkill = 0,
    ActionId = 789 // Sword Slash
};
```

### Notizen
- **Broadcast Range**: 50m Radius um Source und Target
- **Floating Combat Text**: Client zeigt Damage als FCT
- **Health Bar**: Client aktualisiert HP-Bar von Target
- **Critical**: IsCritical=true triggert spezielle VFX/SFX
- **Overkill**: Wird für "Killing Blow" Statistiken verwendet
- **Damage Types**:
  - **Physical**: Reduziert durch Armor
  - **Magical**: Reduziert durch Magic Resistance
  - **True**: Ignoriert alle Mitigation
- **Animation**: ActionId triggert Hit-Animation auf Client

**🔄 Zukünftiger DTO-Einsatz:**  
In Phase 2 wird ein `CombatEntityDto` eingeführt um Target-Informationen zu übertragen. Siehe [DTO_ARCHITECTURE.md](DTO_ARCHITECTURE.md) für Details.

---

## DeathEvent (303)

**Richtung:** 📡 Broadcast (Server → Alle in Range)  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Server broadcasted den Tod einer Entity. Enthält Killer, Victim, und potentielle Rewards (XP, Loot). Triggert Death-Animation, Corpse-Spawning und Respawn-Timer.

Diese Message markiert den Übergang von Living → Dead State. Sie enthält alle Information die Clients brauchen um Death-Sequence darzustellen und Rewards zu vergeben.

### Im Scope ✅
- Victim und Killer Entity-IDs
- Death-Reason (Combat, Fall, Drowning, etc.)
- XP-Reward (für Killer)
- Loot-Information (für Loot-System)
- Respawn-Timer (für Spieler)

### Nicht im Scope ❌
- Respawn-Handling → siehe `Resurrection` (331)
- Loot-Window → siehe `Loot` Messages (3100-3199)
- Death-Penalties → siehe `Character` Messages (600-699)
- Corpse-Interaction → siehe `Entity` Messages (1400-1499)

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| VictimId | int | Entity die starb | Ja |
| KillerId | int | Entity die tötete (0 = Environment) | Nein |
| DeathReason | enum | Combat/Fall/Drown/etc. | Ja |
| XpReward | int | XP für Killer (0 für Spieler-Tod) | Nein |
| HasLoot | bool | Ob Entity Loot dropped | Ja |
| RespawnTimeSec | int | Respawn-Timer (nur für Spieler) | Nein |

### Erwartete Response
- Keine Response erforderlich
- Spieler sendet später `Resurrection` Request

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `DamageEvent` | 302 | Letzter Damage vor Tod |
| `Resurrection` | 331 | Spieler möchte respawnen |
| `CombatEnd` | 311 | Combat endet für Participants |
| `LootRequest` | 3100 | Loot vom Corpse |

### Flow-Diagramm
```
Server                   All Clients in Range
  │                            │
  │  DeathEvent (303)          │
  │  VictimId=456              │
  │  KillerId=123              │
  │  DeathReason=Combat        │
  │  XpReward=100              │
  │  HasLoot=true              │
  │  RespawnTimeSec=30         │
  │───────────────────────────►│
  │                            │  ┌─ Play Death Animation
  │                            │  ├─ Spawn Corpse
  │                            │  ├─ Show XP Gain (Killer)
  │                            │  ├─ Start Respawn Timer (Victim)
  │                            │  └─ Remove from Threat Tables
```

### Beispiel Payload
```csharp
var death = new DeathEvent
{
    Type = MessageType.DeathEvent,
    VictimId = 456, // Enemy NPC
    KillerId = 123, // Player
    DeathReason = DeathReason.Combat,
    XpReward = 100,
    HasLoot = true,
    RespawnTimeSec = 0 // NPC respawnt automatisch
};
```

### Error Codes
Keine - Death ist immer erfolgreich (server-authoritative)

### Notizen
- **Death Animation**: Client spielt Death-Animation auf Victim
- **Corpse**: Victim Entity wird zu Corpse (für Looting)
- **XP Gain**: Killer erhält XP (nur bei NPC-Tod)
- **Combat End**: Alle Participants bekommen `CombatEnd` (311)
- **Threat Reset**: Victim wird aus allen Threat-Tables entfernt
- **Respawn Timer**: Für Spieler: 30s, für NPCs: variable
- **Death Reasons**:
  - **Combat**: Getötet durch Entity
  - **Fall**: Fall-Damage (siehe `FallDamage` 208)
  - **Drown**: Ertrunken
  - **Environment**: Lava, Gift, etc.
  - **Suicide**: /kill Command
- **Loot**: HasLoot=true → Corpse kann geplündert werden

---

## HealEvent (304)

**Richtung:** 📡 Broadcast (Server → Alle in Range)  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Server broadcasted Healing an alle Spieler in Sichtweite. Funktioniert analog zu `DamageEvent`, aber für Heilung. Enthält Source, Target und Heal-Amount.

### Im Scope ✅
- Heal-Amount (nach Modifiers)
- Source und Target Entity-IDs
- Critical Heal Flag
- Overheal-Amount (für Statistiken)
- Action-ID für Animation

### Nicht im Scope ❌
- HoT-Ticks → siehe `HealOverTime` (328)
- Absorption Shields → siehe `ShieldApplied` (329)
- Resurrection → siehe `Resurrection` (331)

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| SourceId | int | Heiler Entity-ID | Ja |
| TargetId | int | Geheilte Entity-ID | Ja |
| HealAmount | int | Heilung (nach Modifiers) | Ja |
| IsCritical | bool | Ob kritische Heilung | Ja |
| Overheal | int | Healing über Max-HP | Nein |
| ActionId | uint | ID der Heal-Action | Ja |

### Erwartete Response
- Keine Response erforderlich

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `ActionRequest` | 300 | Heal-Action Request |
| `HealOverTime` | 328 | Für HoT-Ticks |
| `ShieldApplied` | 329 | Für Absorption Shields |

### Beispiel Payload
```csharp
var heal = new HealEvent
{
    Type = MessageType.HealEvent,
    SourceId = 789, // Healer
    TargetId = 123, // Injured Player
    HealAmount = 150,
    IsCritical = true, // Crit Heal = 1.5x
    Overheal = 20, // 20 HP über Max
    ActionId = 456 // Holy Light Spell
};
```

### Notizen
- **Critical Heals**: ~30% Chance, 1.5x Healing
- **Overheal**: Wird getrackt für Healer-Statistiken
- **Broadcast Range**: 50m Radius
- **VFX**: Grüne/Goldene Heilungs-Effekte

---

## MissEvent (305)

**Richtung:** 📡 Broadcast  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja

### Beschreibung

Action hat das Ziel verfehlt (Miss). Base-Miss-Chance: 5%, erhöht gegen höher-levelige Targets.

### Im Scope ✅
- Miss-Notification für Source und Target
- Animation-Trigger (Miss-Animation)

### Nicht im Scope ❌
- Dodge/Parry/Block → separate Events (306-308)

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| SourceId | int | Angreifer | Ja |
| TargetId | int | Ziel | Ja |
| ActionId | uint | Action die misste | Ja |

### Beispiel Payload
```csharp
var miss = new MissEvent
{
    Type = MessageType.MissEvent,
    SourceId = 123,
    TargetId = 456,
    ActionId = 789
};
```

### Notizen
- **Miss Chance**: 5% Base + Level-Difference Modifier
- **No Damage**: Kein Damage bei Miss
- **Combat Log**: "Your attack missed!"

---

## DodgeEvent (306)

**Richtung:** 📡 Broadcast  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja

### Beschreibung

Target hat den Angriff ausgewichen (Dodge). Dodged Attacks verursachen keinen Damage.

### Im Scope ✅
- Dodge-Notification
- Dodge-Animation Trigger

### Nicht im Scope ❌
- Miss → separate Event (305)
- Parry/Block → separate Events (307-308)

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| SourceId | int | Angreifer | Ja |
| TargetId | int | Ausweichender | Ja |
| ActionId | uint | Ausgewichene Action | Ja |

### Beispiel Payload
```csharp
var dodge = new DodgeEvent
{
    Type = MessageType.DodgeEvent,
    SourceId = 123,
    TargetId = 456,
    ActionId = 789
};
```

### Notizen
- **Dodge Chance**: Basiert auf Agility-Stat + Gear
- **Typical Range**: 10-30% Dodge-Chance
- **PvE**: Tanks haben höhere Dodge-Chance

---

## ParryEvent (307)

**Richtung:** 📡 Broadcast  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja

### Beschreibung

Target hat den Angriff pariert (Parry). Erfordert Weapon in Main-Hand.

### Im Scope ✅
- Parry-Notification
- Parry-Animation (Weapon-Block)

### Nicht im Scope ❌
- Shield-Block → siehe `BlockEvent` (308)

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| SourceId | int | Angreifer | Ja |
| TargetId | int | Parierender | Ja |
| ActionId | uint | Parierte Action | Ja |

### Beispiel Payload
```csharp
var parry = new ParryEvent
{
    Type = MessageType.ParryEvent,
    SourceId = 123,
    TargetId = 456,
    ActionId = 789
};
```

### Notizen
- **Parry Chance**: Erfordert Melee Weapon equipped
- **Typical Range**: 5-15% Parry-Chance
- **Counter**: Kann `CounterAttack` (324) triggern

---

## BlockEvent (308)

**Richtung:** 📡 Broadcast  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja

### Beschreibung

Target hat Damage mit Shield geblockt. Blocktes Damage wird reduziert (z.B. 30-50%).

### Im Scope ✅
- Block-Notification
- Reduced Damage Amount
- Shield-Block Animation

### Nicht im Scope ❌
- Absorption Shields → siehe `ShieldApplied` (329)
- Parry → siehe `ParryEvent` (307)

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| SourceId | int | Angreifer | Ja |
| TargetId | int | Blockender | Ja |
| ActionId | uint | Geblockte Action | Ja |
| BlockedDamage | int | Reduzierter Damage | Ja |
| ActualDamage | int | Verbleibender Damage | Ja |

### Beispiel Payload
```csharp
var block = new BlockEvent
{
    Type = MessageType.BlockEvent,
    SourceId = 123,
    TargetId = 456,
    ActionId = 789,
    BlockedDamage = 100, // 100 Damage geblockt
    ActualDamage = 50 // 50 Damage kam durch
};
```

### Notizen
- **Block Chance**: Erfordert Shield equipped
- **Block Amount**: 30-50% Damage Reduction
- **Typical Chance**: 20-40% für Tanks

---

## CriticalHitEvent (309)

**Richtung:** 📡 Broadcast  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja

### Beschreibung

Kritischer Treffer! Damage/Healing wurde erhöht (typisch 2x Multiplier).

### Im Scope ✅
- Crit-Notification
- Multiplier-Information
- Special VFX/SFX Trigger

### Nicht im Scope ❌
- Reguläres Damage → siehe `DamageEvent` (302)

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| SourceId | int | Angreifer/Heiler | Ja |
| TargetId | int | Ziel | Ja |
| ActionId | uint | Crit Action | Ja |
| BaseDamage | int | Base-Wert vor Crit | Ja |
| CritDamage | int | Finaler Wert nach Crit | Ja |
| Multiplier | float | Crit-Multiplier (z.B. 2.0) | Ja |

### Beispiel Payload
```csharp
var crit = new CriticalHitEvent
{
    Type = MessageType.CriticalHitEvent,
    SourceId = 123,
    TargetId = 456,
    ActionId = 789,
    BaseDamage = 100,
    CritDamage = 200,
    Multiplier = 2.0f
};
```

### Notizen
- **Crit Chance**: Basiert auf Stats + Gear
- **Typical Range**: 10-40% Crit-Chance
- **Multiplier**: 2x Standard, kann durch Talents erhöht werden

---

## CombatStart (310)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

### Beschreibung

Spieler ist in Combat-State eingetreten. Triggert Combat-UI, verhindert Mounting, aktiviert Combat-Regeneration.

### Im Scope ✅
- Enter Combat-State
- Combat-UI Activation
- Disable Mounting/Resting

### Nicht im Scope ❌
- Combat-Actions → siehe `ActionRequest` (300)
- Combat-End → siehe `CombatEnd` (311)

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| PlayerId | int | Spieler der in Combat geht | Ja |
| InitiatorId | int | Entity die Combat startete | Nein |

### Beispiel Payload
```csharp
var combatStart = new CombatStart
{
    Type = MessageType.CombatStart,
    PlayerId = 123,
    InitiatorId = 456 // Enemy
};
```

### Notizen
- **Auto-Flag**: Wird auto-gesetzt bei Damage/Healing
- **5s Timeout**: Combat endet 5s nach letzter Action
- **No Mount**: Mounting während Combat nicht möglich
- **Regen**: In-Combat-Regeneration ist langsamer

---

## CombatEnd (311)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

### Beschreibung

Combat-State wurde beendet. Spieler kann wieder mounten, normale Regeneration startet.

### Im Scope ✅
- Exit Combat-State
- Enable Mounting/Resting
- Start Out-of-Combat-Regen

### Nicht im Scope ❌
- Combat-Summary → Client-seitig

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| PlayerId | int | Spieler der Combat verlässt | Ja |

### Beispiel Payload
```csharp
var combatEnd = new CombatEnd
{
    Type = MessageType.CombatEnd,
    PlayerId = 123
};
```

### Notizen
- **Auto-Trigger**: 5s nach letzter Combat-Action
- **Death**: Combat endet automatisch bei Tod
- **Zone-Change**: Combat endet beim Zone-Wechsel

---

## ThreatUpdate (312)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja

### Beschreibung

Update der Threat-Table (Aggro). Wird an alle Party-Members gesendet für Threat-Meter.

### Im Scope ✅
- Threat-Amount pro Entity
- Current Target des NPC
- Threat-Percentage (relativ zu Top-Threat)

### Nicht im Scope ❌
- Aggro-Transfer → siehe `AggroTransfer` (321)
- Taunt → siehe `TauntEvent` (322)

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| SourceId | int | NPC der Threat hat | Ja |
| ThreatList | ThreatEntry[] | Threat-Table | Ja |

**ThreatEntry:**
| Feld | Typ | Beschreibung |
|------|-----|--------------|
| EntityId | int | Entity mit Threat |
| ThreatAmount | int | Absoluter Threat-Wert |
| Percentage | float | % von Top-Threat |

### Beispiel Payload
```csharp
var update = new ThreatUpdate
{
    Type = MessageType.ThreatUpdate,
    SourceId = 999, // Boss NPC
    ThreatList = new[]
    {
        new ThreatEntry { EntityId = 123, ThreatAmount = 10000, Percentage = 100 }, // Tank (Top)
        new ThreatEntry { EntityId = 456, ThreatAmount = 8000, Percentage = 80 }, // DPS
        new ThreatEntry { EntityId = 789, ThreatAmount = 5000, Percentage = 50 } // Healer
    }
};
```

### Notizen
- **Update Frequency**: Jede 1s während Combat
- **Threat Meter**: Clients können Threat-UI darstellen
- **Tank Priority**: Tanks müssen Top-Threat halten

---

## ThreatListRequest (313)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

### Beschreibung

Client fragt Threat-Table eines NPC ab (für Threat-Meter).

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| TargetId | int | NPC-ID | Ja |

### Erwartete Response
- `ThreatListResponse` (321)

### Folge-Messages bei Erfolg
- `ThreatUpdate` (312) mit Threat-Liste

---

## InterruptEvent (314)

**Richtung:** 📡 Broadcast  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja

### Beschreibung

Spell/Action wurde interrupted (z.B. durch Kick, Stun, Silence).

### Im Scope ✅
- Interrupt-Notification
- Interrupted Action-ID
- Interrupter Entity-ID

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| SourceId | int | Interrupter | Ja |
| TargetId | int | Interrupted Entity | Ja |
| InterruptedActionId | uint | Unterbrochene Action | Ja |

### Beispiel Payload
```csharp
var interrupt = new InterruptEvent
{
    Type = MessageType.InterruptEvent,
    SourceId = 123, // Player mit Kick
    TargetId = 456, // Casting Enemy
    InterruptedActionId = 789 // Fireball Cast
};
```

### Notizen
- **Interrupt Lockout**: Target kann 4s lang keine Spells vom gleichen Typ casten

---

## ReflectEvent (315)

**Richtung:** 📡 Broadcast  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

### Beschreibung

Damage wurde reflektiert (zurück an Attacker).

### Im Scope ✅
- Reflect-Notification
- Reflected Damage Amount
- Original Damage Amount

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| SourceId | int | Original Attacker (erhält Reflect) | Ja |
| TargetId | int | Reflector | Ja |
| ReflectedDamage | int | Damage zurück an Source | Ja |
| OriginalDamage | int | Original Damage | Ja |

### Beispiel Payload
```csharp
var reflect = new ReflectEvent
{
    Type = MessageType.ReflectEvent,
    SourceId = 123, // Attacker erhält Reflect
    TargetId = 456, // Mage mit Reflect Shield
    ReflectedDamage = 50, // 50% reflected
    OriginalDamage = 100
};
```

---

## AbsorbEvent (316)

**Richtung:** 📡 Broadcast  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja

### Beschreibung

Damage wurde von Absorption Shield absorbiert.

### Im Scope ✅
- Absorbed Damage Amount
- Remaining Shield Amount

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| TargetId | int | Geschützte Entity | Ja |
| AbsorbedDamage | int | Absorbierter Damage | Ja |
| RemainingShield | int | Verbleibender Shield | Ja |

### Beispiel Payload
```csharp
var absorb = new AbsorbEvent
{
    Type = MessageType.AbsorbEvent,
    TargetId = 123,
    AbsorbedDamage = 200,
    RemainingShield = 300 // 300 HP Shield remaining
};
```

---

## LifestealEvent (317)

**Richtung:** 📡 Broadcast  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja

### Beschreibung

Attacker hat HP durch Lifesteal zurückgewonnen.

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| SourceId | int | Lifesteal Entity | Ja |
| TargetId | int | Opfer | Ja |
| DamageDeal | int | Damage dealt | Ja |
| HealAmount | int | HP zurückgewonnen | Ja |
| LifestealPercent | float | Lifesteal-% (z.B. 0.2 = 20%) | Ja |

---

## ExecutePhase (318)

**Richtung:** 📡 Broadcast  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

### Beschreibung

Target ist in Execute-Phase (<20% HP). Execute-Abilities machen Bonus-Damage.

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| TargetId | int | Entity in Execute-Phase | Ja |

---

## EnrageEvent (319)

**Richtung:** 📡 Broadcast  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

### Beschreibung

Boss ist enraged (erhöhter Damage, Attack-Speed). Tritt nach Timer oder HP-Threshold auf.

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| BossId | int | Enraged Boss | Ja |
| DamageMultiplier | float | Damage-Boost (z.B. 2.0) | Ja |
| SpeedMultiplier | float | Attack-Speed Boost (z.B. 1.5) | Ja |

---

## CombatLogEntry (320)

**Richtung:** 📥 Server → Client  
**Frequenz:** ⚡ High-Frequency  
**Authentifizierung:** 🔒 Ja

### Beschreibung

Detaillierter Combat-Log Entry für DPS-Meters und Combat-Log-Parser.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Timestamp | long | Unix-Timestamp | Ja |
| SourceId | int | Source Entity | Ja |
| TargetId | int | Target Entity | Ja |
| EventType | string | DAMAGE/HEAL/MISS/etc. | Ja |
| Amount | int | Damage/Heal Amount | Ja |
| ActionId | uint | Action-ID | Ja |

---

## AggroTransfer (321)

**Richtung:** 📡 Broadcast  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

### Beschreibung

Aggro wurde von einer Entity zu einer anderen transferiert (z.B. durch Threat-Wipe).

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| SourceId | int | NPC | Ja |
| OldTargetId | int | Vorheriges Target | Ja |
| NewTargetId | int | Neues Target | Ja |

---

## TauntEvent (322)

**Richtung:** 📡 Broadcast  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja

### Beschreibung

Tank hat Taunt verwendet → erzwingt Aggro für kurze Zeit.

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| TankId | int | Tank der tauntet | Ja |
| TargetId | int | Getauntetes NPC | Ja |
| DurationSec | int | Taunt-Duration | Ja |

---

## FeintEvent (323)

**Richtung:** 📡 Broadcast  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

### Beschreibung

DPS hat Feint verwendet → reduziert Threat temporär.

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| PlayerId | int | Player der feintet | Ja |
| ThreatReduction | float | Threat-Reduction (z.B. 0.5 = 50%) | Ja |

---

## CounterAttack (324)

**Richtung:** 📡 Broadcast  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

### Beschreibung

Nach Parry/Dodge erfolgt automatischer Counter-Attack.

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| SourceId | int | Counter-Attacker | Ja |
| TargetId | int | Original Attacker | Ja |
| Damage | int | Counter-Damage | Ja |

---

## ComboFinisher (325)

**Richtung:** 📡 Broadcast  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja

### Beschreibung

Finisher-Ability wurde mit Combo-Points ausgeführt.

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| SourceId | int | Angreifer | Ja |
| TargetId | int | Ziel | Ja |
| ComboPoints | int | Verwendete Combo-Points (1-5) | Ja |
| Damage | int | Finisher-Damage | Ja |

---

## AreaDamage (326)

**Richtung:** 📡 Broadcast  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja

### Beschreibung

AoE-Damage Event. Einzelne Message für alle getroffenen Entities.

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| SourceId | int | AoE-Source | Ja |
| Targets | AoETarget[] | Liste aller Targets | Ja |
| ActionId | uint | AoE-Action | Ja |

**AoETarget:**
| Feld | Typ | Beschreibung |
|------|-----|--------------|
| TargetId | int | Entity-ID |
| Damage | int | Damage (kann variieren) |

---

## DamageOverTime (327)

**Richtung:** 📡 Broadcast  
**Frequenz:** ⚡ High-Frequency  
**Authentifizierung:** 🔒 Ja

### Beschreibung

DoT-Tick (z.B. Bleed, Poison, Burn).

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| SourceId | int | DoT-Caster | Ja |
| TargetId | int | DoT-Opfer | Ja |
| Damage | int | Tick-Damage | Ja |
| AuraId | uint | DoT-Aura-ID | Ja |
| TickNumber | int | Wievielter Tick | Ja |

---

## HealOverTime (328)

**Richtung:** 📡 Broadcast  
**Frequenz:** ⚡ High-Frequency  
**Authentifizierung:** 🔒 Ja

### Beschreibung

HoT-Tick (z.B. Renew, Rejuvenation).

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| SourceId | int | HoT-Caster | Ja |
| TargetId | int | HoT-Target | Ja |
| HealAmount | int | Tick-Healing | Ja |
| AuraId | uint | HoT-Aura-ID | Ja |
| TickNumber | int | Wievielter Tick | Ja |

---

## ShieldApplied (329)

**Richtung:** 📡 Broadcast  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja

### Beschreibung

Absorption-Shield wurde auf Target angewendet.

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| SourceId | int | Shield-Caster | Ja |
| TargetId | int | Geschützte Entity | Ja |
| ShieldAmount | int | Absorption-Amount | Ja |
| AuraId | uint | Shield-Aura-ID | Ja |

---

## ShieldBroken (330)

**Richtung:** 📡 Broadcast  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja

### Beschreibung

Absorption-Shield wurde vollständig aufgebraucht.

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| TargetId | int | Entity deren Shield brach | Ja |
| AuraId | uint | Shield-Aura-ID | Ja |

---

## Resurrection (331) - Request

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Spieler möchte respawnen (an Graveyard) oder akzeptiert eine Battle-Resurrection von einem anderen Spieler. Server validiert Request und führt Resurrection durch.

### Im Scope ✅
- Graveyard-Respawn (nach Tod)
- Battle-Rez akzeptieren (während Combat)
- Spirit-Healer Resurrection

### Nicht im Scope ❌
- Selbst-Rez → nur bestimmte Klassen (geplant)
- Auto-Rez → verwende `ResurrectionSickness` (4143)

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| RezType | enum | GraveyardRespawn, AcceptBattleRez, SpiritHealer | Ja |
| SourceId | int | ID des Rezzing-Spielers (nur bei AcceptBattleRez) | Nein |

### RezType Enum
```csharp
public enum RezType : byte
{
    GraveyardRespawn = 0,  // Normale Respawn am Graveyard
    AcceptBattleRez = 1,    // Battle-Rez akzeptieren
    SpiritHealer = 2        // Beim Spirit Healer wiederbeleben (Resurrection Sickness)
}
```

### Erwartete Response
- `ResurrectionResponse` (322)

### Folge-Messages bei Erfolg
- `Resurrection` Event (331) Broadcast an nahestehende Spieler

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `Resurrection` Event | 331 | Server broadcastet Rez |
| `DeathNotification` | 4100 | Vorangegangen vor Rez |
| `ResurrectionOffer` | 4130 | Spieler bietet Battle-Rez an |
| `ResurrectionSickness` | 4143 | Debuff nach Spirit-Healer-Rez |

### Beispiel Payload
```csharp
// Graveyard Respawn
var graveyardRez = new Resurrection
{
    Type = MessageType.Resurrection,
    RezType = RezType.GraveyardRespawn
};

// Battle-Rez akzeptieren
var battleRez = new Resurrection
{
    Type = MessageType.Resurrection,
    RezType = RezType.AcceptBattleRez,
    SourceId = 98765  // Spieler der Battle-Rez castet
};
```

### Error Codes
| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `NOT_DEAD` | Spieler ist nicht tot | - |
| `IN_COMBAT` | Kann nicht am Graveyard respawnen während Combat | Battle-Rez verwenden |
| `REZ_EXPIRED` | Battle-Rez ist abgelaufen | Neuen Battle-Rez anfordern |
| `INVALID_REZ_SOURCE` | Ungültige SourceId | - |

### Notizen
- **Graveyard-Respawn**: Teleportiert zum nächsten Graveyard, 50% HP/Mana
- **Battle-Rez**: Während Combat, sofort, HP/Mana vom Caster abhängig
- **Spirit-Healer**: Sofort, aber mit Resurrection Sickness (10 Minuten Debuff)
- **Cooldown**: Battle-Rez hat Cooldown (variiert nach Klasse)

---

## Resurrection (331) - Event

**Richtung:** 📡 Broadcast (Server → Nearby Players)  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Server broadcastet Resurrection-Event an alle Spieler in der Nähe. Zeigt an dass ein Spieler wiederbelebt wurde. Client spielt Resurrection-Animation und Sound.

### Im Scope ✅
- Resurrection-Notification an nahestehende Spieler
- Resurrecter-Information (wer hat revived)
- HP/Mana nach Resurrection
- Animation-Trigger

### Nicht im Scope ❌
- Ressource-Update → verwende `ResourceUpdate` (604)
- Combat-State-Update → verwende `CombatEnd` (311)

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| TargetId | int | Character-ID des Wiederbelebten | Ja |
| SourceId | int | Character-ID des Resurrecter (0 = Graveyard/Spirit-Healer) | Ja |
| RezType | enum | Typ der Resurrection | Ja |
| HealthPercent | float | HP nach Rez (0.0-1.0, z.B. 0.5 = 50%) | Ja |
| ManaPercent | float | Mana nach Rez (0.0-1.0) | Ja |
| X | float | Position X (Respawn-Punkt) | Ja |
| Y | float | Position Y | Ja |

### Beispiel Payload
```csharp
// Graveyard Respawn
var graveyardRezEvent = new Resurrection
{
    Type = MessageType.Resurrection,
    TargetId = 12345,
    SourceId = 0,  // Graveyard
    RezType = RezType.GraveyardRespawn,
    HealthPercent = 0.5f,  // 50% HP
    ManaPercent = 0.5f,    // 50% Mana
    X = 100.0f,
    Y = 200.0f
};

// Battle-Rez
var battleRezEvent = new Resurrection
{
    Type = MessageType.Resurrection,
    TargetId = 12345,
    SourceId = 98765,  // Priest der rezzt
    RezType = RezType.AcceptBattleRez,
    HealthPercent = 0.3f,  // 30% HP
    ManaPercent = 0.2f,    // 20% Mana
    X = 150.0f,
    Y = 250.0f
};
```

### Notizen
- **Animation**: Client spielt Resurrection-Effekt an Position (X, Y)
- **Sound**: Heiliger Sound-Effekt beim Rez
- **Visual-Range**: Nur Spieler in Sichtweite erhalten Event
- **Combat-State**: Resurrecteter Player ist nach Battle-Rez wieder in Combat
- **Resurrection-Sickness**: Bei Spirit-Healer-Rez wird zusätzlich `DebuffApplied` (1504) gesendet

---

## CombatStateSync (332)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

### Beschreibung

Full Combat-State Synchronisation (nach Reconnect oder Zone-Transfer).

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| InCombat | bool | Ob Player in Combat | Ja |
| CombatStartTime | long | Wann Combat startete | Nein |
| ActiveEnemies | int[] | List of Enemy IDs | Nein |

---

## ThreatListResponse (333)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung
Antwort auf ThreatListRequest. Liefert die Threat-Liste einer Entity.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Success | bool | Request erfolgreich? | Ja |
| ErrorCode | string | Fehlercode falls Success=false | Nein |
| ErrorMessage | string | Menschenlesbare Fehlermeldung | Nein |
| EntityId | int | Entity deren Threat-Liste | Bei Erfolg |

### Error Codes
| Code | Bedeutung |
|------|-----------|
| `ENTITY_NOT_FOUND` | Entity existiert nicht |
| `NO_THREAT_DATA` | Keine Threat-Daten verfügbar |

---

## ResurrectionResponse (334)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung
Antwort auf Resurrection Request. Bestätigt erfolgreiche Wiederbelebung oder gibt Fehler zurück.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Success | bool | Resurrection erfolgreich? | Ja |
| ErrorCode | string | Fehlercode falls Success=false | Nein |
| ErrorMessage | string | Menschenlesbare Fehlermeldung | Nein |
| TargetId | int | Wiederbelebter Spieler | Bei Erfolg |

### Error Codes
| Code | Bedeutung |
|------|-----------|
| `TARGET_NOT_DEAD` | Target ist nicht tot |
| `OUT_OF_RANGE` | Zu weit entfernt |
| `INSUFFICIENT_MANA` | Nicht genug Mana |
| `IN_COMBAT` | Im Kampf |

---

## 🗑️ Obsolete Messages

Derzeit keine obsoleten Messages in der Combat-Kategorie.

---

## 📎 Anhang

### MessageType Enum (Combat Range)

```csharp
// ═══════════════════════════════════════════════════════════════
// COMBAT (0300-0399)
// ═══════════════════════════════════════════════════════════════
ActionRequest = 300,
ActionResult = 301,
DamageEvent = 302,
DeathEvent = 303,
HealEvent = 304,
MissEvent = 305,
DodgeEvent = 306,
ParryEvent = 307,
BlockEvent = 308,
CriticalHitEvent = 309,
CombatStart = 310,
CombatEnd = 311,
ThreatUpdate = 312,
ThreatListRequest = 313,
InterruptEvent = 314,
ReflectEvent = 315,
AbsorbEvent = 316,
LifestealEvent = 317,
ExecutePhase = 318,
EnrageEvent = 319,
CombatLogEntry = 320,
AggroTransfer = 321,
TauntEvent = 322,
FeintEvent = 323,
CounterAttack = 324,
ComboFinisher = 325,
AreaDamage = 326,
DamageOverTime = 327,
HealOverTime = 328,
ShieldApplied = 329,
ShieldBroken = 330,
Resurrection = 331,
CombatStateSync = 332,
ThreatListResponse = 333,
ResurrectionResponse = 334,
```

### Neue Enums (Combat-spezifisch)

Die folgenden Enums sind für das Combat-System erforderlich:

```csharp
// Mmo.Shared/Combat/Enums/DamageType.cs
public enum DamageType : byte
{
    Physical = 1,   // Reduziert durch Armor
    Magical = 2,    // Reduziert durch Magic Resistance
    True = 3        // Ignoriert alle Mitigation
}

// Mmo.Shared/Combat/Enums/DeathReason.cs
public enum DeathReason : byte
{
    Combat = 1,
    FallDamage = 2,
    Drowning = 3,
    Environment = 4,
    Suicide = 5,
    Disconnected = 6
}

// Mmo.Shared/Combat/Enums/RezType.cs
public enum RezType : byte
{
    GraveyardRespawn = 0,
    AcceptBattleRez = 1,
    SpiritHealer = 2
}

// Mmo.Shared/Combat/Enums/CombatResultType.cs
public enum CombatResultType : byte
{
    Hit = 1,
    Miss = 2,
    Dodge = 3,
    Parry = 4,
    Block = 5,
    Critical = 6,
    Absorb = 7,
    Reflect = 8
}
```

### Datei-Struktur (Combat)

```
Mmo.Shared/
├── Combat/
│   ├── Enums/
│   │   ├── DamageType.cs
│   │   ├── DeathReason.cs
│   │   ├── RezType.cs
│   │   └── CombatResultType.cs
│   ├── Dtos/
│   │   ├── ThreatEntry.cs
│   │   └── AoETarget.cs
│   └── Messages/
│       ├── ActionRequest.cs
│       ├── ActionResult.cs
│       ├── DamageEvent.cs
│       ├── DeathEvent.cs
│       ├── HealEvent.cs
│       ├── MissEvent.cs
│       ├── DodgeEvent.cs
│       ├── ParryEvent.cs
│       ├── BlockEvent.cs
│       ├── CriticalHitEvent.cs
│       ├── CombatStart.cs
│       ├── CombatEnd.cs
│       ├── ThreatUpdate.cs
│       ├── ThreatListRequest.cs
│       ├── ThreatListResponse.cs
│       ├── InterruptEvent.cs
│       ├── ReflectEvent.cs
│       ├── AbsorbEvent.cs
│       ├── LifestealEvent.cs
│       ├── ExecutePhase.cs
│       ├── EnrageEvent.cs
│       ├── CombatLogEntry.cs
│       ├── AggroTransfer.cs
│       ├── TauntEvent.cs
│       ├── FeintEvent.cs
│       ├── CounterAttack.cs
│       ├── ComboFinisher.cs
│       ├── AreaDamage.cs
│       ├── DamageOverTime.cs
│       ├── HealOverTime.cs
│       ├── ShieldApplied.cs
│       ├── ShieldBroken.cs
│       ├── Resurrection.cs
│       ├── ResurrectionResponse.cs
│       └── CombatStateSync.cs
```

### Verwandte Kategorien

| Kategorie | Beschreibung | Message-Range |
|-----------|--------------|---------------|
| Movement (02) | Crowd Control (Root, Stun) | 0200-0299 |
| Aura (15) | Buffs/Debuffs | 1500-1599 |
| Targeting (12) | Target-Selection | 1200-1299 |
| Character (06) | HP/Mana/Resources | 0600-0699 |
| Loot (31) | Rewards nach Combat | 3100-3199 |
| Death (41) | Death-System (erweitert) | 4100-4199 |

---

**Letzte Aktualisierung:** 2026-01-02  
**Version:** 3.0.0  
**Status:** ✅ Vollständig dokumentiert (35/35 Messages)

[← Zurück zur Übersicht](README.md)
