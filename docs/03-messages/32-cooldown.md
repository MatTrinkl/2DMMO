# ⏱️ Cooldowns / Timers Messages (3200-3299)

**Kategorie:** 32  
**Range:** 3200-3299  



[← Zurück zur Übersicht](README.md)

---

## 📋 Inhaltsverzeichnis

- [CooldownStart (3200)](#cooldownstart-3200)
- [CooldownEnd (3201)](#cooldownend-3201)
- [CooldownUpdate (3202)](#cooldownupdate-3202)
- [CooldownReset (3203)](#cooldownreset-3203)
- [CooldownSync (3204)](#cooldownsync-3204)
- [GlobalCooldownStart (3210)](#globalcooldownstart-3210)
- [GlobalCooldownEnd (3211)](#globalcooldownend-3211)
- [CastStart (3220)](#caststart-3220)
- [CastUpdate (3221)](#castupdate-3221)
- [CastInterrupt (3222)](#castinterrupt-3222)
- [CastComplete (3223)](#castcomplete-3223)
- [CastFailed (3224)](#castfailed-3224)
- [ChannelStart (3230)](#channelstart-3230)
- [ChannelTick (3231)](#channeltick-3231)
- [ChannelInterrupt (3232)](#channelinterrupt-3232)
- [ChannelComplete (3233)](#channelcomplete-3233)
- [ChargeUpdate (3240)](#chargeupdate-3240)
- [ChargeRestore (3241)](#chargerestore-3241)

---

## 📋 Übersicht

Diese Kategorie umfasst alle Messages für **Cooldown-, Cast- und Channel-Systeme** im 2DMMO.

Das Cooldown-System implementiert:
- Ability-Cooldowns (individuelle pro Spell)
- Global Cooldown (GCD) für alle Spells
- Cast-Time Tracking
- Channel-Spells (kontinuierliche Casts)
- Charge-basierte Abilities
- Cooldown-Reset Mechanics

**Server Authority**: Alle Cooldown-Timings sind server-authoritative. Client zeigt Timers, Server validiert.

---

## CooldownStart (3200)

**Richtung:** 📡 Broadcast  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Startet Cooldown für Ability/Item. Server sendet nach Ability-Use.

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| CooldownId | uint | Ability/Item-ID | Ja |
| Duration | int | Cooldown-Duration (ms) | Ja |
| CooldownCategory | string | Category (z.B. "spell", "item", "trinket") | Ja |

### Beispiel Payload
```csharp
var cooldownStart = new CooldownStart
{
    Type = MessageType.CooldownStart,
    CooldownId = 1001,
    Duration = 10000, // 10 Sekunden
    CooldownCategory = "spell"
};
```

### Notizen
- **UI**: Client zeigt Cooldown-Spiral auf Icon
- **Categories**: Spells, Items, Trinkets haben separate Cooldowns
- **Shared**: Manche Spells sharen Cooldown-Category

---

## CooldownEnd (3201)

**Richtung:** 📡 Broadcast  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Cooldown ist abgelaufen. Ability ist wieder verwendbar.

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| CooldownId | uint | Ability/Item-ID | Ja |

### Notizen
- **Sound**: Client spielt optional "Ready" Sound
- **Visual**: Cooldown-Spiral verschwindet

---

## CooldownUpdate (3202)

**Richtung:** 📡 Broadcast  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Cooldown-Duration ändert sich (z.B. durch Buff, Haste-Rating).

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| CooldownId | uint | Ability-ID | Ja |
| NewDuration | int | Neue verbleibende Duration (ms) | Ja |

### Notizen
- **Haste**: Haste-Rating reduziert Cooldowns
- **Buffs**: Cooldown-Reduction Buffs

---

## CooldownReset (3203)

**Richtung:** 📡 Broadcast  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Cooldown wird resettet (sofort ready). Durch spezielle Abilities/Procs.

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| CooldownId | uint | Ability-ID | Ja |
| ResetReason | string | "proc", "ability", "boss_phase" | Ja |

### Notizen
- **Procs**: Chance-basierte Resets
- **Abilities**: Cooldown-Reset-Abilities
- **Boss**: Boss-Phase-Transitions können Cooldowns resetten

---

## CooldownSync (3204)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten (Login/Reconnect)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Sync aller aktiven Cooldowns nach Login/Reconnect.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Cooldowns | List<CooldownInfo> | Alle aktiven Cooldowns | Ja |

**CooldownInfo**:
| Feld | Typ | Beschreibung |
|------|-----|--------------|
| CooldownId | uint | Ability-ID |
| RemainingTime | int | Verbleibende Zeit (ms) |
| Category | string | Cooldown-Category |

---

## GlobalCooldownStart (3210)

**Richtung:** 📡 Broadcast  
**Frequenz:** ⚡ Sehr häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Startet Global Cooldown (GCD). Nach fast jeder Ability-Use.

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Duration | int | GCD-Duration (ms, standard 1500ms) | Ja |

### Notizen
- **Standard**: 1.5 Sekunden
- **Haste**: Reduziert durch Haste-Rating (min 1.0s)
- **All Abilities**: Blockiert alle Abilities während GCD
- **Exceptions**: Instant Off-GCD Abilities existieren

---

## GlobalCooldownEnd (3211)

**Richtung:** 📡 Broadcast  
**Frequenz:** ⚡ Sehr häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
GCD ist abgelaufen. Abilities sind wieder verwendbar.

### Broadcast Payload
Keine zusätzlichen Felder

---

## CastStart (3220)

**Richtung:** 📡 Broadcast  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Startet Spell-Cast mit Cast-Time. Spieler muss Channeln bis Complete.

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| CasterId | int | Caster Entity-ID | Ja |
| SpellId | uint | Spell-ID | Ja |
| CastTime | int | Cast-Duration (ms) | Ja |
| TargetId | int | Target-ID (0=self) | Nein |

### Beispiel Payload
```csharp
var castStart = new CastStart
{
    Type = MessageType.CastStart,
    CasterId = 50001,
    SpellId = 2001,
    CastTime = 3000, // 3 Sekunden
    TargetId = 60001
};
```

### Notizen
- **UI**: Client zeigt Cast-Bar
- **Movement**: Movement interruptet Cast (default)
- **Damage**: Damage kann Cast interrupten (Pushback)
- **Haste**: Haste reduziert Cast-Time

---

## CastUpdate (3221)

**Richtung:** 📡 Broadcast  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Cast-Progress-Update (z.B. bei Pushback durch Damage).

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| CasterId | int | Caster-ID | Ja |
| RemainingTime | int | Verbleibende Cast-Time (ms) | Ja |

---

## CastInterrupt (3222)

**Richtung:** 📡 Broadcast  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Cast wurde interruptet (Movement, Damage, Silence, Stun).

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| CasterId | int | Caster-ID | Ja |
| InterruptReason | string | "movement", "damage", "silence", "stun", "death" | Ja |
| InterrupterId | int | Interrupter-ID (bei interrupt-Spell) | Nein |

### Notizen
- **Lockout**: Interrupt kann School-Lockout verursachen
- **Cooldown**: Spell geht auf Cooldown auch bei Interrupt
- **UI**: Cast-Bar verschwindet mit "interrupted" Text

---

## CastComplete (3223)

**Richtung:** 📡 Broadcast  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Cast erfolgreich abgeschlossen. Spell wird executed.

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| CasterId | int | Caster-ID | Ja |
| SpellId | uint | Completed Spell | Ja |

### Notizen
- **Effect**: Spell-Effect wird applied (separate Message)
- **GCD**: Löst Global Cooldown aus

---

## CastFailed (3224)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung
Cast-Start fehlgeschlagen (kein Mana, kein Target, etc.).

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| SpellId | uint | Fehlgeschlagener Spell | Ja |
| FailReason | string | "no_mana", "no_target", "out_of_range", "not_ready" | Ja |

---

## ChannelStart (3230)

**Richtung:** 📡 Broadcast  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Startet Channel-Spell (kontinuierlicher Cast mit Ticks).

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| CasterId | int | Channeler-ID | Ja |
| SpellId | uint | Channel-Spell-ID | Ja |
| Duration | int | Total Duration (ms) | Ja |
| TickInterval | int | Interval zwischen Ticks (ms) | Ja |
| TargetId | int | Target-ID | Nein |

### Notizen
- **Examples**: Mind Flay, Drain Life, Blizzard
- **Ticks**: Effect wird bei jedem Tick applied
- **Movement**: Meist nicht möglich während Channel

---

## ChannelTick (3231)

**Richtung:** 📡 Broadcast  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Channel-Tick (periodischer Effect-Trigger).

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| CasterId | int | Channeler-ID | Ja |
| SpellId | uint | Channel-Spell | Ja |
| TickNumber | int | Tick-Nummer (1-N) | Ja |

---

## ChannelInterrupt (3232)

**Richtung:** 📡 Broadcast  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Channel wurde interruptet.

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| CasterId | int | Channeler-ID | Ja |
| InterruptReason | string | Reason | Ja |

---

## ChannelComplete (3233)

**Richtung:** 📡 Broadcast  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Channel erfolgreich completed (alle Ticks durch).

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| CasterId | int | Channeler-ID | Ja |
| SpellId | uint | Completed Channel | Ja |

---

## ChargeUpdate (3240)

**Richtung:** 📡 Broadcast  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Update für Charge-basierte Abilities (Abilities mit mehreren Charges).

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| SpellId | uint | Spell-ID | Ja |
| CurrentCharges | int | Aktuelle Charges | Ja |
| MaxCharges | int | Max Charges | Ja |
| NextChargeTime | long | Unix Timestamp (nächste Charge) | Ja |

### Beispiel Payload
```csharp
var chargeUpdate = new ChargeUpdate
{
    Type = MessageType.ChargeUpdate,
    SpellId = 3001,
    CurrentCharges = 1,
    MaxCharges = 3,
    NextChargeTime = DateTimeOffset.UtcNow.AddSeconds(20).ToUnixTimeSeconds()
};
```

### Notizen
- **Examples**: Rogue Combo Points, Warlock Soul Shards
- **Recharge**: Charges regenerieren über Zeit
- **UI**: Client zeigt Charge-Count

---

## ChargeRestore (3241)

**Richtung:** 📡 Broadcast  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Charge wurde restored (Time-basiert oder durch Ability).

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| SpellId | uint | Spell-ID | Ja |
| NewCharges | int | Neue Charge-Count | Ja |

---

**Letzte Aktualisierung**: 2025-12-25  
**Version**: 1.0.0

[← Zurück zur Übersicht](README.md)
