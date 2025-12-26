# ⚔️ Combat Messages (0300-0399)

**Kategorie:** 3  
**Range:** 0300-0399  
**Phase:** Phase 2  
**Status:** ✅ Vollständig dokumentiert

[← Zurück zur Übersicht](README.md)

---

## 📋 Übersicht

Diese Kategorie umfasst alle Messages für das **Combat-System** im 2DMMO.

Das Combat-System implementiert server-authoritative Kampfmechaniken mit:
- Action-basiertes Kampfsystem (Abilities, Spells, Attacks)
- Damage/Healing Calculation mit verschiedenen Ergebnissen (Hit, Miss, Dodge, Parry, Block, Crit)
- Threat/Aggro-System für PvE-Tanking
- Combat State Management (In/Out of Combat)
- Advanced Mechanics (Reflect, Absorb, Lifesteal, Execute)
- Boss Mechanics (Enrage Timer)
- Combat Logging für DPS-Meters
- Combo-Systeme mit Finishers
- DoT/HoT (Damage/Heal over Time)
- Absorption Shields
- Resurrection-System

**Server Authority**: Alle Combat-Berechnungen erfolgen server-seitig. Client sendet Action-Requests, Server validiert und broadcasted die Events.

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
- **Bei Erfolg:** `ActionResult` (301) + Follow-up Events (`DamageEvent` 302, `HealEvent` 304, etc.)
- **Bei Fehler:** `ErrorMessage` (910) mit Code

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
- **Bei Erfolg:** `ThreatUpdate` (312)

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

## Resurrection (331)

**Richtung:** 🔄 Bidirektional  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

### Beschreibung

**Request (Client → Server)**: Spieler möchte respawnen oder akzeptiert Battle-Rez.  
**Event (Server → All)**: Spieler wurde wiederbelebt.

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| RezType | enum | GraveyardRespawn/AcceptBattleRez | Ja |

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| TargetId | int | Wiederbelebte Entity | Ja |
| SourceId | int | Resurrecter (0 = Graveyard) | Nein |
| HealthPercent | float | HP nach Rez (z.B. 0.5 = 50%) | Ja |
| ManaPercent | float | Mana nach Rez | Ja |

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

## 🔗 Verwandte Kategorien

- **Movement (02)**: Crowd Control Effects (Root, Stun) → `RootEvent` (218), `StunMovement` (219)
- **Aura (15)**: Buffs/Debuffs die Combat beeinflussen → `BuffApplied` (1500), `DebuffApplied` (1501)
- **Targeting (12)**: Target-Selection für Combat → `TargetEntity` (1200)
- **Character (06)**: HP/Mana/Resources → `ResourceUpdate` (604)
- **Loot (31)**: Rewards nach Combat → `LootGenerated` (3100)

---

**Letzte Aktualisierung**: 2025-12-17  
**Version**: 2.0.0  
**Status**: ✅ Vollständig dokumentiert (33/33 Messages)

[← Zurück zur Übersicht](README.md)
