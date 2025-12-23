# 👤 Character Messages (0600-0699)

**Kategorie:** 6  
**Range:** 0600-0699  
**Phase:** Phase 2  
**Status:** ✅ Fully Documented

[← Zurück zur Übersicht](README.md)

---

## 📋 Übersicht

Diese Kategorie umfasst alle Messages für **Character-Progression und -Management** im 2DMMO.

**Hauptbereiche:**
- **Leveling**: XP-Gain, Level-Ups, Skill Points
- **Stats**: HP, Mana, Stamina, Attributes (Strength, Agility, Intelligence, etc.)
- **Reputation**: Faction standings, reputation gains/losses
- **Titles**: Title unlocks, selection, display
- **Appearance**: Customization, race/class/name/gender changes
- **Rest System**: Rest XP bonus, inn resting

**Phase 2 Features**: Vollständige character-progression mit Reputation-System, Titles, und Appearance-Customization.

---

## 📝 Message-Liste

### Leveling & XP (600-605)
- `LevelUp` (600) - Player erreicht neues Level
- `XpGain` (601) - XP aus Quests/Kills/Exploration
- `SkillPointGain` (608) - Skill Points aus Levels
- `TalentPointGain` (609) - Talent Points aus Levels (Phase 2)

### Stats & Resources (602-607)
- `StatUpdate` (602) - Einzelner Stat-Update (HP, Mana, etc.)
- `StatFullSync` (603) - Vollständiger Stat-Sync (Login, Reconnect)
- `ResourceUpdate` (604) - Resource-Change (HP/Mana/Energy/Rage)
- `ResourceRegen` (605) - Regeneration Tick
- `CharacterInfo` (606) - Vollständige Character-Info
- `CharacterInfoRequest` (607) - Anfrage für Character-Info (Inspect)

### Reputation (610-612)
- `ReputationChange` (610) - Reputation Gain/Loss
- `ReputationListRequest` (611) - Anfrage aller Reputationen
- `ReputationListResponse` (612) - Alle Faction-Standings

### Titles (613-616)
- `TitleUnlocked` (613) - Neuer Title freigeschaltet
- `TitleSelect` (614) - Title auswählen
- `TitleListRequest` (615) - Anfrage aller Titles
- `TitleListResponse` (616) - Alle Titles mit Status

### Appearance (617-622)
- `AppearanceChange` (617) - Appearance ändern
- `AppearancePreview` (618) - Preview vor Kauf
- `RaceChange` (619) - Rassen-Wechsel (Phase 2)
- `ClassChange` (620) - Klassen-Wechsel (Phase 2)
- `NameChange` (621) - Namen-Wechsel
- `GenderChange` (622) - Geschlechts-Wechsel (Phase 2)

### Rest System (623-624)
- `RestXpUpdate` (623) - Rest XP Bonus Update
- `RestStateChange` (624) - Rested/Normal State

---

## 📖 Detaillierte Message-Dokumentation

## LevelUp (600)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Der Server benachrichtigt den Client, dass der Spieler ein neues Level erreicht hat. Diese Message wird gesendet wenn genügend XP für den nächsten Level gesammelt wurde. Sie enthält das neue Level sowie Belohnungen (Stat-Erhöhungen, Skill Points, Talent Points).

Der Level-Up triggert oft zusätzliche Events: Vollheilung, neue Fähigkeiten freigeschaltet, neue Ausrüstung verfügbar. Der Client sollte eine prominente UI-Benachrichtigung und Sound-Effekte abspielen.

Level-Cap wird in ServerConfig definiert (aktuell 60 für Prototyp). Erfahrungspunkte über dem Cap werden ignoriert oder in ein alternatives Progression-System (Paragon, etc.) umgeleitet.

### Im Scope ✅
- Benachrichtigung über neues Level
- Stat-Belohnungen (HP, Mana, Attribute)
- Skill Point Rewards
- Talent Point Rewards (Phase 2)
- Vollheilung des Characters
- Client UI-Trigger

### Nicht im Scope ❌
- XP-Gain selbst → verwende `XpGain` (601)
- Stat-Updates → werden automatisch via `StatUpdate` (602) gesendet
- Skill/Talent Allocation → separate Messages in Skill-Kategorie
- Quest-Completion → verwende Quest-Messages

### Request/Response Payload

**LevelUp Broadcast:**
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| PlayerId | int | ID des level-up Spielers | Ja |
| NewLevel | int | Das neue Level (1-60) | Ja |
| StatIncreases | Dictionary<StatType, int> | Erhöhte Stats (HP+100, Mana+50, etc.) | Ja |
| SkillPointsGained | int | Anzahl Skill Points | Ja |
| TalentPointsGained | int | Anzahl Talent Points (0 für Prototyp) | Ja |
| FullHeal | bool | Character wurde vollgeheilt | Ja (true) |

### Erwartete Response
- **Bei Erfolg:** Keine Response erwartet (Broadcast)
- **Keine Fehler möglich** (Server-initiiert)

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `XpGain` | 601 | Verursacht Level-Up wenn genug XP |
| `StatUpdate` | 602 | Folgt auf LevelUp für Stat-Sync |
| `SkillPointGain` | 608 | Alternative für Skill Point Rewards |
| `TalentPointGain` | 609 | Alternative für Talent Point Rewards |

### Flow-Diagramm
```
Client                    Server
  │                          │
  │      (XP reaches threshold)
  │                          │
  │      LevelUp (600)       │
  │◄─────────────────────────│
  │                          │
  │   StatUpdate (602) x N   │
  │◄─────────────────────────│
  │                          │
  │  (Play Level-Up VFX)     │
```

### Beispiel Payload
```csharp
var levelUp = new LevelUpMessage
{
    Type = MessageType.LevelUp,
    PlayerId = 12345,
    NewLevel = 10,
    StatIncreases = new Dictionary<StatType, int>
    {
        { StatType.MaxHP, 100 },
        { StatType.MaxMana, 50 },
        { StatType.Strength, 2 },
        { StatType.Agility, 2 },
        { StatType.Intelligence, 2 }
    },
    SkillPointsGained = 1,
    TalentPointsGained = 0,  // Phase 2
    FullHeal = true
};
```

### Notizen
- **Level-Curve**: XP required = BaseXP * (Level ^ 1.8)
  - Level 1→2: 100 XP
  - Level 9→10: ~1,500 XP
  - Level 59→60: ~500,000 XP
- **Stat-Gains per Level**:
  - HP: +100 per level
  - Mana: +50 per level  
  - Stamina: +10 per level
  - Primary Stat (Strength/Agi/Int): +2 per level
  - Secondary Stats: +1 per level
- **Skill/Talent Points**:
  - 1 Skill Point every level
  - 1 Talent Point every 5 levels (Phase 2)
- **Full Heal**: Setzt HP/Mana auf 100%, entfernt Debuffs
- **UI**: Prominente Benachrichtigung, Sound, Visual Effects
- **Achievement**: "Ding!" Achievement für Level 60

---

## XpGain (601)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Der Server informiert den Client über gewonnene Erfahrungspunkte (XP). XP kann aus verschiedenen Quellen stammen: Monster-Kills, Quest-Completion, Exploration, Crafting, PvP, etc.

Die Message enthält die Menge an XP, die Quelle, und ob Rest-Bonus angewendet wurde. Der Client zeigt dies in der XP-Bar und als Floating-Text an. Wenn genügend XP für einen Level-Up erreicht wird, folgt eine `LevelUp` Message.

XP-Gains werden für Party-Members geteilt (mit Distance-Check). Rest-Bonus (150% XP) wird für Spieler angewendet die in Inns ausgeloggt haben.

### Im Scope ✅
- XP-Gain Benachrichtigung
- XP-Quelle (Kill, Quest, Exploration, etc.)
- Rest-Bonus Indikator (1.5x)
- Aktuelle Total-XP
- XP bis nächstes Level
- Party XP-Sharing Info

### Nicht im Scope ❌
- Level-Up selbst → verwende `LevelUp` (600)
- XP-Loss bei Tod → separate Death-Message
- XP-Debt System → Phase 3 Feature
- Paragon XP → Phase 3 End-Game System

### Request/Response Payload

**XpGain Broadcast:**
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| PlayerId | int | Empfänger | Ja |
| XpAmount | int | Gewonnene XP (vor Bonus) | Ja |
| XpSource | XpSourceType | Quelle (Kill, Quest, Exploration, etc.) | Ja |
| SourceEntityId | int | Entity ID (bei Kill) oder Quest ID | Nein |
| RestBonus | bool | Rest-Bonus aktiv (1.5x) | Ja |
| TotalXp | long | Neue Total-XP | Ja |
| XpToNextLevel | int | Verbleibende XP bis Level-Up | Ja |
| PartyShare | bool | XP war party-shared | Ja |

**XpSourceType:**
- Kill = 0
- QuestComplete = 1
- Exploration = 2
- Crafting = 3
- PvP = 4
- Event = 5

### Erwartete Response
- **Bei Erfolg:** Keine Response erwartet (Broadcast)
- **Keine Fehler möglich** (Server-initiiert)

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `LevelUp` | 600 | Folgt wenn genug XP |
| `RestXpUpdate` | 623 | Rest-Bonus Management |
| `PartyMemberUpdate` | 710 | Party XP-Sharing |
| `DeathEvent` | 303 | XP-Penalty bei Tod |

### Flow-Diagramm
```
Client                    Server
  │                          │
  │    (Kill Monster)        │
  │                          │
  │      XpGain (601)        │
  │◄─────────────────────────│
  │  (+100 XP from Kill)     │
  │                          │
  │  (Update XP Bar)         │
  │  (Floating +100 XP)      │
```

### Beispiel Payload
```csharp
var xpGain = new XpGainMessage
{
    Type = MessageType.XpGain,
    PlayerId = 12345,
    XpAmount = 100,
    XpSource = XpSourceType.Kill,
    SourceEntityId = 98765,  // Monster ID
    RestBonus = true,        // +50% = 150 XP total
    TotalXp = 45150,         // Nach Gain
    XpToNextLevel = 850,
    PartyShare = false
};
```

### Notizen
- **XP-Amounts**:
  - Normal Monster: Level * 10 XP
  - Elite Monster: Level * 25 XP
  - Boss Monster: Level * 100 XP
  - Quest: 100-5000 XP je nach Schwierigkeit
  - Exploration: 50 XP pro neues Gebiet
- **Rest-Bonus**:
  - 150% XP wenn Rest-Status aktiv
  - Rest-Bar = 1.5 Levels worth of XP
  - Verbraucht sich bei XP-Gain
  - Aufgeladen in Inns/Cities
- **Party XP-Sharing**:
  - Geteilt wenn <100m Distanz
  - XP / PartySize (kein Penalty für große Gruppen im Prototyp)
  - Bonus +5% per Partymitglied (Phase 2)
- **XP-Penalty bei Tod**: -10% current Level XP (Phase 2)
- **UI**: Floating Combat Text, XP-Bar Update, Sound
- **Anti-Cheat**: XP-Gain wird nur server-side berechnet

---

## StatUpdate (602)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Der Server sendet einen inkrementellen Stat-Update für einen einzelnen Stat (z.B. HP, Mana, Strength, Agility). Diese Message wird verwendet für einzelne Änderungen um Bandbreite zu sparen. Bei größeren Updates (Login, Reconnect, Level-Up) wird `StatFullSync` (603) verwendet.

Stats umfassen: HP, Mana, Stamina, Attributes (Strength, Agility, Intelligence, Vitality, Spirit), Secondary Stats (Armor, Resist, Crit, Haste, etc.). Jeder Stat hat einen Current und Max-Wert.

Updates können durch viele Ereignisse ausgelöst werden: Equipment-Change, Buff/Debuff, Level-Up, Talent-Allocation, Food-Buffs, etc.

### Im Scope ✅
- Einzelner Stat-Update
- Current und Max-Werte
- Stat-Änderungs-Grund (Buff, Equipment, etc.)
- Delta-Änderung für UI
- Temporary vs Permanent Change

### Nicht im Scope ❌
- Vollständiger Stat-Sync → verwende `StatFullSync` (603)
- Resource-Regeneration → verwende `ResourceRegen` (605)
- Combat Damage → verwende `DamageEvent` (302)
- Healing → verwende `HealEvent` (304)

### Request/Response Payload

**StatUpdate Message:**
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| PlayerId | int | Betroffener Spieler | Ja |
| StatType | StatType | Welcher Stat | Ja |
| CurrentValue | int | Neuer Current-Wert | Ja |
| MaxValue | int | Neuer Max-Wert | Ja |
| Delta | int | Änderungs-Betrag (+/-) | Ja |
| Reason | StatChangeReason | Grund für Change | Ja |
| Temporary | bool | Temporär (Buff) oder Permanent (Equipment) | Ja |

**StatType Enum:**
- HP = 0, Mana = 1, Stamina = 2
- Strength = 10, Agility = 11, Intelligence = 12, Vitality = 13, Spirit = 14
- Armor = 20, MagicResist = 21, FireResist = 22, etc.
- CritChance = 30, CritDamage = 31, Haste = 32, etc.

**StatChangeReason:**
- Equipment = 0, Buff = 1, Debuff = 2, LevelUp = 3, Talent = 4, Food = 5

### Erwartete Response
- **Bei Erfolg:** Keine Response erwartet (Broadcast)
- **Keine Fehler möglich** (Server-initiiert)

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `StatFullSync` | 603 | Vollständiger Stat-Sync |
| `ResourceUpdate` | 604 | Ähnlich aber für Resources |
| `LevelUp` | 600 | Triggert Multiple Stat-Updates |
| `ItemUseResult` | 506 | Equipment-Change triggert Stats |

### Flow-Diagramm
```
Client                    Server
  │                          │
  │   (Equip +10 STR Sword)  │
  │                          │
  │   StatUpdate (602)       │
  │◄─────────────────────────│
  │  (Strength: 50 → 60)     │
  │                          │
  │   StatUpdate (602)       │
  │◄─────────────────────────│
  │  (Attack Power: 100→110) │
```

### Beispiel Payload
```csharp
var statUpdate = new StatUpdateMessage
{
    Type = MessageType.StatUpdate,
    PlayerId = 12345,
    StatType = StatType.Strength,
    CurrentValue = 60,
    MaxValue = 60,
    Delta = +10,
    Reason = StatChangeReason.Equipment,
    Temporary = false
};
```

### Notizen
- **Stat-Calculation**:
  - Base Stats (aus Level)
  - Equipment Stats (Additive)
  - Buff Stats (Multiplicative)
  - Final = (Base + Equipment) * (1 + BuffMult)
- **Secondary Stats berechnet aus Primary**:
  - 1 Strength = +2 Attack Power
  - 1 Agility = +0.1% Crit, +1 Armor
  - 1 Intelligence = +15 Mana, +1.5 Spell Power
  - 1 Vitality = +10 HP
  - 1 Spirit = +1 Mana Regen/5s
- **Stat-Caps** (Phase 2):
  - Crit Chance: Max 50%
  - Haste: Max 50%
  - Resists: Max 75%
- **UI**: Update Character-Sheet, Update Stat-Bars
- **Performance**: Batch Updates wenn möglich (max 1 pro Frame)

---

## StatFullSync (603)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Der Server sendet eine vollständige Synchronisation aller Character-Stats. Diese Message wird bei Login, Reconnect, Zone-Transfer, oder nach größeren Änderungen (z.B. nach Level-Up mit vielen Stat-Changes) verwendet.

Enthält alle Stats: HP, Mana, Stamina, alle Attributes, alle Secondary Stats. Dies ist eine große Message (~500 bytes) und sollte sparsam verwendet werden. Für einzelne Stat-Changes verwende `StatUpdate` (602).

Der Client ersetzt seinen lokalen Stat-Cache komplett mit den Server-Daten. Dies verhindert Client-Server Desyncs bei Stats.

### Im Scope ✅
- Vollständiger Stat-Snapshot
- Alle Primary Stats
- Alle Secondary Stats
- Alle Resource Pools (HP, Mana, Stamina)
- Equipment-Based Stats
- Buff-Based Stats
- Talent-Based Stats

### Nicht im Scope ❌
- Inkrementelle Updates → verwende `StatUpdate` (602)
- Nur Resources → verwende `ResourceUpdate` (604)
- Combat Events → separate Combat-Messages

### Request/Response Payload

**StatFullSync Message:**
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| PlayerId | int | Betroffener Spieler | Ja |
| Stats | Dictionary<StatType, StatValue> | Alle Stats | Ja |
| Timestamp | long | Server-Timestamp | Ja |

**StatValue:**
| Feld | Typ | Beschreibung |
|------|-----|--------------|
| Current | int | Current Value |
| Max | int | Max Value |
| Base | int | Base (ohne Equipment/Buffs) |
| FromEquipment | int | Bonus von Equipment |
| FromBuffs | int | Bonus von Buffs |

### Erwartete Response
- **Bei Erfolg:** Keine Response erwartet (Server-Push)
- **Keine Fehler möglich** (Server-initiiert)

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `StatUpdate` | 602 | Inkrementelle Alternative |
| `CharacterInfo` | 606 | Enthält auch Stats (für Inspect) |
| `LevelUp` | 600 | Triggert oft StatFullSync |
| `JoinZone` | 100 | Login/Zone-Transfer nutzt FullSync |

### Flow-Diagramm
```
Client                    Server
  │                          │
  │   LoginRequest (1)       │
  │─────────────────────────►│
  │                          │
  │  LoginResponse (2)       │
  │◄─────────────────────────│
  │                          │
  │  StatFullSync (603)      │
  │◄─────────────────────────│
  │  (All Stats)             │
  │                          │
  │  (Initialize UI)         │
```

### Beispiel Payload
```csharp
var fullSync = new StatFullSyncMessage
{
    Type = MessageType.StatFullSync,
    PlayerId = 12345,
    Stats = new Dictionary<StatType, StatValue>
    {
        { StatType.HP, new StatValue { Current = 1500, Max = 1500, Base = 1000, FromEquipment = 300, FromBuffs = 200 } },
        { StatType.Mana, new StatValue { Current = 800, Max = 1000, Base = 500, FromEquipment = 200, FromBuffs = 300 } },
        { StatType.Strength, new StatValue { Current = 60, Max = 60, Base = 40, FromEquipment = 15, FromBuffs = 5 } },
        { StatType.Agility, new StatValue { Current = 45, Max = 45, Base = 30, FromEquipment = 10, FromBuffs = 5 } },
        // ... alle anderen Stats
    },
    Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
};
```

### Notizen
- **Message-Size**: ~500-1000 bytes (alle Stats)
- **Frequency**: Nur bei großen Events (Login, Reconnect, Level-Up)
- **Performance**: Client cached Stats lokal, Updates nur via `StatUpdate`
- **Anti-Cheat**: Server ist immer authoritative
- **Stat-Count**: ~50-70 Stats insgesamt
- **Compression**: Consider MessagePack Compression für große Payloads
- **Delta-Sync**: Future optimization: nur geänderte Stats senden

---

## ResourceUpdate (604)

**Richtung:** 📥 Server → Client  
**Frequenz:** ⚡ High-Frequency  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Der Server sendet einen Resource-Update (HP, Mana, Stamina, Energy, Rage, etc.). Diese Message wird häufig gesendet bei Combat, Casting, Movement. Sie ist optimiert für Bandbreite mit nur dem Resource-Type und neuen Wert.

Resources haben Current/Max Werte und regenerieren über Zeit. HP regeneriert langsam out-of-combat, Mana regeneriert via Spirit, Stamina regeneriert schnell, Energy/Rage haben eigene Mechaniken.

Diese Message ist für High-Frequency Updates optimiert (~20 bytes). Für vollständige Stat-Syncs verwende `StatFullSync` (603).

### Im Scope ✅
- Resource-Wert Update
- HP, Mana, Stamina, Energy, Rage, etc.
- Current und Max-Werte
- Delta-Change für UI
- Change-Reason (Damage, Heal, Regen, Cost)

### Nicht im Scope ❌
- Damage-Details → verwende `DamageEvent` (302)
- Healing-Details → verwende `HealEvent` (304)
- Regeneration-Ticks → verwende `ResourceRegen` (605)
- Vollständiger Stat-Sync → verwende `StatFullSync` (603)

### Request/Response Payload

**ResourceUpdate Message:**
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| PlayerId | int | Betroffener Spieler | Ja |
| ResourceType | ResourceType | Welche Resource | Ja |
| CurrentValue | int | Neuer Current-Wert | Ja |
| MaxValue | int | Neuer Max-Wert (falls geändert) | Ja |
| Delta | int | Änderungs-Betrag (+/-) | Ja |
| Reason | ResourceChangeReason | Grund | Ja |

**ResourceType:**
- HP = 0, Mana = 1, Stamina = 2, Energy = 3, Rage = 4

**ResourceChangeReason:**
- Damage = 0, Heal = 1, Cost = 2, Regen = 3, Buff = 4, Death = 5

### Erwartete Response
- **Bei Erfolg:** Keine Response erwartet (Broadcast)
- **Keine Fehler möglich** (Server-initiiert)

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `ResourceRegen` | 605 | Regeneration-Ticks |
| `DamageEvent` | 302 | Verursacht HP-Loss |
| `HealEvent` | 304 | Verursacht HP-Gain |
| `ActionResult` | 301 | Verursacht Resource-Cost |

### Flow-Diagramm
```
Client                    Server
  │                          │
  │   (Cast Fireball)        │
  │   ActionRequest (300)    │
  │─────────────────────────►│
  │                          │
  │  ResourceUpdate (604)    │
  │◄─────────────────────────│
  │  (Mana: 800 → 700)       │
  │                          │
  │  ActionResult (301)      │
  │◄─────────────────────────│
```

### Beispiel Payload
```csharp
var resourceUpdate = new ResourceUpdateMessage
{
    Type = MessageType.ResourceUpdate,
    PlayerId = 12345,
    ResourceType = ResourceType.Mana,
    CurrentValue = 700,
    MaxValue = 1000,
    Delta = -100,
    Reason = ResourceChangeReason.Cost
};
```

### Notizen
- **Resource Types**:
  - **HP**: Health, 0 = Death
  - **Mana**: Spell-Casting Resource
  - **Stamina**: Physical Abilities Resource
  - **Energy**: Rogue/Ninja Resource (100 max, regens fast)
  - **Rage**: Warrior Resource (0-100, gains on taking/dealing damage)
- **Regeneration Rates**:
  - HP: 1% max HP / 5s out-of-combat, 0.1% in-combat
  - Mana: (Spirit * 0.5) / 5s
  - Stamina: 10% max / 1s
  - Energy: 10 / 1s
  - Rage: Decays 1 / 3s out-of-combat
- **Update Frequency**: Max 10/second per player
- **Batching**: Multiple resource-changes in same frame → single message
- **UI**: Update Resource-Bars, Floating Combat Text

---

## ResourceRegen (605)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Der Server sendet Regeneration-Ticks für Resources (HP, Mana, Stamina). Diese Message wird alle 2-5 Sekunden gesendet je nach Resource-Type. Sie ist ähnlich zu `ResourceUpdate` (604) aber speziell für Regeneration optimiert.

Regeneration hängt von verschiedenen Faktoren ab: Combat-State (in/out-of-combat), Spirit-Stat, Buffs, Food, Rest-State. Out-of-combat Regeneration ist deutlich höher als in-combat.

Der Client kann Regeneration lokal predictieren, aber Server ist authoritative. Diese Message dient zur Korrektur und Synchronisation.

### Im Scope ✅
- Regeneration-Tick
- HP, Mana, Stamina Regeneration
- Regen-Amount
- Regen-Reason (Spirit, Food, Buff, Rest)
- In/Out-of-Combat Modifier

### Nicht im Scope ❌
- Damage/Healing → verwende Combat-Messages
- Resource-Cost → verwende `ResourceUpdate` (604)
- Stat-Changes → verwende `StatUpdate` (602)

### Request/Response Payload

**ResourceRegen Message:**
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| PlayerId | int | Regenerierender Spieler | Ja |
| ResourceType | ResourceType | HP, Mana, Stamina | Ja |
| RegenAmount | int | Regenerierte Menge | Ja |
| CurrentValue | int | Neuer Current-Wert | Ja |
| RegenSource | RegenSource | Quelle der Regeneration | Ja |
| InCombat | bool | Combat-State | Ja |

**RegenSource:**
- Natural = 0, Spirit = 1, Food = 2, Buff = 3, Rest = 4

### Erwartete Response
- **Bei Erfolg:** Keine Response erwartet (Broadcast)
- **Keine Fehler möglich** (Server-initiiert)

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `ResourceUpdate` | 604 | Alternative für Resource-Changes |
| `CombatStart` | 310 | Ändert Regen-Rate |
| `CombatEnd` | 311 | Erhöht Regen-Rate |
| `RestStateChange` | 624 | Rest-Bonus aktiviert |

### Flow-Diagramm
```
Client                    Server
  │                          │
  │   (Out-of-Combat)        │
  │   (Every 5s)             │
  │                          │
  │  ResourceRegen (605)     │
  │◄─────────────────────────│
  │  (+50 HP, +100 Mana)     │
  │                          │
  │  (Update Bars)           │
```

### Beispiel Payload
```csharp
var regen = new ResourceRegenMessage
{
    Type = MessageType.ResourceRegen,
    PlayerId = 12345,
    ResourceType = ResourceType.Mana,
    RegenAmount = 100,
    CurrentValue = 800,
    RegenSource = RegenSource.Spirit,
    InCombat = false
};
```

### Notizen
- **Regen-Rates**:
  - **HP**: 1% MaxHP / 5s (out-of-combat), 0.1% MaxHP / 5s (in-combat)
  - **Mana**: (Spirit * 0.5) / 5s
  - **Stamina**: 10% MaxStamina / 1s (very fast)
- **Combat-State**: Drastisch reduziert HP-Regen
- **Food-Buffs**: +50 HP/5s, +100 Mana/5s für 20 minutes
- **Rest-State**: +100% Regen-Rate in Inns/Cities
- **Tick-Frequency**: 
  - HP/Mana: Every 5s
  - Stamina: Every 1s
- **Client-Prediction**: Client kann Regen lokal simulieren, Server korrigiert
- **UI**: Smooth interpolation between ticks

---

## CharacterInfo (606)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Der Server sendet vollständige Character-Informationen als Response auf `CharacterInfoRequest` (607). Diese Message wird verwendet für Character-Inspect (andere Spieler anschauen), Character-Select Screen, oder Tooltip-Hover.

Enthält alle öffentlich sichtbaren Informationen: Name, Level, Class, Race, Equipment, Stats, Titles, Guild, Achievements, etc. Private Informationen (Gold, Inventory) werden nicht inkludiert.

Die Datenmenge kann groß sein (~1-2KB), daher nur auf explizite Anfrage und mit Rate-Limiting (max 5/minute pro Spieler).

### Im Scope ✅
- Vollständige öffentliche Character-Info
- Name, Level, Class, Race, Gender
- Equipment (visible items)
- Stats (public stats nur)
- Title, Guild
- Achievements (public ones)
- Reputation (major factions)
- Appearance-Customization

### Nicht im Scope ❌
- Private Info (Gold, Inventory) → nur eigener Character via API
- Full Stat-Details → verwende `StatFullSync` (603)
- Equipment-Stats → nur Item-IDs, kein Full-Tooltip
- Bank/Mail → nicht public

### Request/Response Payload

**CharacterInfo Message:**
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| PlayerId | int | Inspizierter Character | Ja |
| Name | string | Character-Name | Ja |
| Level | int | Character-Level | Ja |
| Class | ClassType | Klasse | Ja |
| Race | RaceType | Rasse | Ja |
| Gender | Gender | Geschlecht | Ja |
| Equipment | Dictionary<EquipSlot, int> | Equipped Item-IDs | Ja |
| Stats | Dictionary<StatType, int> | Public Stats | Ja |
| CurrentTitle | int | Selected Title-ID | Nein |
| GuildName | string | Guild-Name | Nein |
| GuildRank | string | Rang in Guild | Nein |
| PublicAchievements | int[] | Achievement-IDs | Ja |
| ReputationHighlights | Dictionary<FactionId, int> | Major Faction Standings | Ja |
| AppearanceOptions | Dictionary<string, int> | Customization (Skin, Hair, etc.) | Ja |

### Erwartete Response
- **Bei Erfolg:** Keine weitere Response (ist bereits Response)
- **Bei Fehler:** `ErrorMessage` (910) mit Code
  - `PLAYER_NOT_FOUND`: Character existiert nicht
  - `RATE_LIMIT`: Zu viele Requests

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `CharacterInfoRequest` | 607 | Anfrage für diese Message |
| `StatFullSync` | 603 | Eigener Character Stats |
| `InventoryUpdate` | 500 | Private Inventory-Info |
| `GuildRosterResponse` | 811 | Guild-Member Info |

### Flow-Diagramm
```
Client                    Server
  │                          │
  │  CharacterInfoRequest    │
  │  (607)                   │
  │─────────────────────────►│
  │  (TargetPlayerId: 999)   │
  │                          │
  │  CharacterInfo (606)     │
  │◄─────────────────────────│
  │  (Full Public Info)      │
  │                          │
  │  (Display Inspect UI)    │
```

### Beispiel Payload
```csharp
var charInfo = new CharacterInfoMessage
{
    Type = MessageType.CharacterInfo,
    PlayerId = 999,
    Name = "Thorgar",
    Level = 45,
    Class = ClassType.Warrior,
    Race = RaceType.Orc,
    Gender = Gender.Male,
    Equipment = new Dictionary<EquipSlot, int>
    {
        { EquipSlot.Head, 12345 },
        { EquipSlot.Chest, 12346 },
        { EquipSlot.MainHand, 12347 },
        // ... other slots
    },
    Stats = new Dictionary<StatType, int>
    {
        { StatType.Strength, 120 },
        { StatType.Agility, 60 },
        { StatType.HP, 5000 },
        // ... public stats
    },
    CurrentTitle = 42,  // "The Unyielding"
    GuildName = "Warriors of Light",
    GuildRank = "Officer",
    PublicAchievements = new int[] { 1, 5, 10, 25, 100 },
    ReputationHighlights = new Dictionary<FactionId, int>
    {
        { FactionId.Stormwind, 21000 },  // Exalted
        { FactionId.Ironforge, 18000 }   // Revered
    },
    AppearanceOptions = new Dictionary<string, int>
    {
        { "SkinColor", 3 },
        { "HairStyle", 7 },
        { "HairColor", 2 }
    }
};
```

### Notizen
- **Rate-Limiting**: Max 5 CharacterInfoRequests / minute
- **Cache**: Client sollte Ergebnisse 5 minutes cachen
- **Privacy**: Nur public info, kein Gold/Inventory/Mail
- **Size**: ~1-2KB per Message
- **Use-Cases**:
  - Character-Inspect (Rechtsklick → Inspect)
  - Character-Select Screen
  - Party/Guild Member Tooltip
  - Achievement-Comparison
- **Performance**: Expensive Query → Cache server-side
- **UI**: Zeigt Equipment, Stats, Achievements, Guild-Info

---

## CharacterInfoRequest (607)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Der Client fordert vollständige Character-Informationen für einen anderen Spieler an (Inspect-Feature). Der Server antwortet mit `CharacterInfo` (606) Message.

Diese Anfrage ist rate-limited (max 5/minute) um Server-Load zu reduzieren. Clients sollten Ergebnisse cachen um wiederholte Anfragen für denselben Character zu vermeiden.

Nur Characters in der gleichen Zone können inspiziert werden (Anti-Stalking Maßnahme). Der Target-Character wird optional benachrichtigt dass er inspiziert wurde.

### Im Scope ✅
- Anfrage für Character-Info
- Target-PlayerId
- Anfrage-Typ (Full Inspect, Quick-Tooltip, etc.)
- Client-Side Cache-Check

### Nicht im Scope ❌
- Character-Info selbst → kommt via `CharacterInfo` (606)
- Eigener Character → nutze lokale Daten
- Offline Characters → nur Online-Characters

### Request/Response Payload

**CharacterInfoRequest:**
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| TargetPlayerId | int | Zu inspizierender Character | Ja |
| RequestType | InspectType | Full oder Quick | Ja |

**InspectType:**
- Full = 0  // Komplette Inspect-UI
- QuickTooltip = 1  // Nur für Tooltip (weniger Daten)

### Erwartete Response
- **Bei Erfolg:** `CharacterInfo` (606)
- **Bei Fehler:** `ErrorMessage` (910) mit Code
  - `PLAYER_NOT_FOUND`: Character nicht online oder nicht in gleicher Zone
  - `RATE_LIMIT_EXCEEDED`: Zu viele Requests
  - `PERMISSION_DENIED`: Character hat Inspect disabled

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `CharacterInfo` | 606 | Response Message |
| `PlayerJoinedZone` | 103 | Nur Characters in gleicher Zone |

### Flow-Diagramm
```
Client                    Server
  │                          │
  │  (Rechtsklick → Inspect) │
  │                          │
  │CharacterInfoRequest (607)│
  │─────────────────────────►│
  │  (TargetPlayerId: 999)   │
  │                          │
  │   [Rate-Limit Check]     │
  │   [Same-Zone Check]      │
  │                          │
  │  CharacterInfo (606)     │
  │◄─────────────────────────│
  │                          │
  │  (Open Inspect UI)       │
```

### Beispiel Payload
```csharp
var request = new CharacterInfoRequestMessage
{
    Type = MessageType.CharacterInfoRequest,
    TargetPlayerId = 999,
    RequestType = InspectType.Full
};
```

### Error Codes
| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `PLAYER_NOT_FOUND` | Character nicht online oder nicht in gleicher Zone | Zeige "Character nicht verfügbar" |
| `RATE_LIMIT_EXCEEDED` | Zu viele Anfragen (>5/min) | Zeige Cooldown-Timer |
| `PERMISSION_DENIED` | Target hat Inspect disabled | Zeige "Inspect nicht erlaubt" |

### Notizen
- **Rate-Limiting**: Max 5 Requests / minute pro Spieler
- **Same-Zone Requirement**: Anti-Stalking Maßnahme
- **Cache**: Client sollte Responses 5 min cachen
- **Privacy-Settings** (Phase 2): Spieler können Inspect disablen
- **Notification** (Optional): Target wird benachrichtigt ("Thorgar inspects you")
- **Use-Cases**:
  - Inspect Equipment/Achievements
  - Party-Vetting (Gear-Check)
  - Duell-Vorbereitung (See Enemy Stats)
- **UI**: Rechtsklick → "Inspect" Option

---

## SkillPointGain (608)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Der Server benachrichtigt den Client über gewonnene Skill Points. Skill Points werden primär durch Level-Ups vergeben (1 pro Level), können aber auch durch Quests, Achievements, oder Events vergeben werden.

Skill Points werden verwendet um Skills zu lernen oder zu verbessern (Phase 2). Der Client zeigt eine Benachrichtigung und updated die Skill-UI.

### Im Scope ✅
- Skill Point Gain Benachrichtigung
- Anzahl gewonnener Points
- Quelle (Level-Up, Quest, Achievement)
- Total Skill Points
- Unspent Skill Points

### Nicht im Scope ❌
- Skill-Allocation → separate Skill-Messages (Phase 2)
- Talent Points → verwende `TalentPointGain` (609)
- Level-Up Details → verwende `LevelUp` (600)

### Request/Response Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| PlayerId | int | Empfänger | Ja |
| PointsGained | int | Gewonnene Skill Points | Ja |
| Source | PointSource | Quelle (LevelUp, Quest, etc.) | Ja |
| TotalPoints | int | Total Skill Points ever earned | Ja |
| UnspentPoints | int | Verfügbare Skill Points | Ja |

### Erwartete Response
- **Bei Erfolg:** Keine Response erwartet
- **Keine Fehler möglich**

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `LevelUp` | 600 | Hauptquelle für Skill Points |
| `TalentPointGain` | 609 | Ähnlich für Talent Points |

### Beispiel Payload
```csharp
var skillPoints = new SkillPointGainMessage
{
    Type = MessageType.SkillPointGain,
    PlayerId = 12345,
    PointsGained = 1,
    Source = PointSource.LevelUp,
    TotalPoints = 45,
    UnspentPoints = 3
};
```

### Notizen
- **Gain Rate**: 1 Skill Point per Level (1-60 = 60 total)
- **Bonus Sources**: Special Quests (+1), Achievements (+1)
- **Respec**: Phase 2 feature (costs Gold)
- **UI**: Skill-Tree UI, Glow-Animation für verfügbare Points

---

## TalentPointGain (609)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Der Server benachrichtigt über gewonnene Talent Points (Phase 2 Feature). Talent Points werden alle 5 Levels vergeben und erlauben tiefere Spec-Customization als Skills.

Talent Trees sind class-spezifisch mit 3 Branches pro Class. Talent Points sind schwerer zu respecen als Skill Points (höhere Kosten).

### Im Scope ✅
- Talent Point Gain Benachrichtigung
- Anzahl gewonnener Points
- Total/Unspent Talent Points
- Verfügbare Talent-Tiers

### Nicht im Scope ❌
- Talent-Allocation → separate Talent-Messages
- Skill Points → verwende `SkillPointGain` (608)
- Talent-Trees → separate Documentation

### Request/Response Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| PlayerId | int | Empfänger | Ja |
| PointsGained | int | Gewonnene Talent Points | Ja |
| TotalPoints | int | Total Talent Points | Ja |
| UnspentPoints | int | Verfügbare Talent Points | Ja |
| UnlockedTier | int | Neuer Tier freigeschaltet (0-5) | Nein |

### Erwartete Response
- **Bei Erfolg:** Keine Response erwartet
- **Keine Fehler möglich**

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `LevelUp` | 600 | Talent Points bei Level 5, 10, 15, etc. |
| `SkillPointGain` | 608 | Ähnlich für Skill Points |

### Beispiel Payload
```csharp
var talentPoints = new TalentPointGainMessage
{
    Type = MessageType.TalentPointGain,
    PlayerId = 12345,
    PointsGained = 1,
    TotalPoints = 9,
    UnspentPoints = 2,
    UnlockedTier = 2  // Tier 2 unlocked at 25 points
};
```

### Notizen
- **Gain Rate**: 1 Talent Point every 5 Levels (Level 60 = 12 Talent Points)
- **Talent-Tiers**: Unlock at 5, 10, 15, 20, 25 points invested
- **Respec-Cost**: Increases each time (50g, 100g, 250g, 500g, 1000g cap)
- **Phase**: Phase 2 Feature
- **UI**: Talent-Tree UI mit Glow-Animation

---

## ReputationChange (610)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Der Server benachrichtigt über Reputation-Änderungen bei einer Faction. Reputation wird durch Quests, Kills, Donations, oder PvP gewonnen/verloren.

Reputation hat mehrere Levels: Hated, Hostile, Unfriendly, Neutral, Friendly, Honored, Revered, Exalted. Jedes Level benötigt 3000-21000 Reputation Points.

Höhere Reputation schaltet Rewards frei: Discounts, Special Items, Mounts, Titles, Quests.

### Im Scope ✅
- Reputation Gain/Loss
- Faction-ID
- Change-Amount
- Neuer Reputation-Wert
- Reputation-Level (Neutral, Friendly, etc.)
- Change-Reason (Quest, Kill, Donation)

### Nicht im Scope ❌
- Alle Reputationen → verwende `ReputationListResponse` (612)
- Reputation-Rewards → separate Item/Quest Messages
- Faction-Info → separate Faction-Database

### Request/Response Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| PlayerId | int | Betroffener Spieler | Ja |
| FactionId | int | Faction-ID | Ja |
| Change | int | Reputation-Änderung (+/-) | Ja |
| NewValue | int | Neuer Reputation-Wert (0-42000) | Ja |
| NewLevel | ReputationLevel | Neues Level | Ja |
| LeveledUp | bool | Level-Up erreicht? | Ja |
| Reason | RepChangeReason | Grund für Change | Ja |

**ReputationLevel:**
- Hated = 0 (0-2999)
- Hostile = 1 (3000-5999)
- Unfriendly = 2 (6000-8999)
- Neutral = 3 (9000-11999)
- Friendly = 4 (12000-14999)
- Honored = 5 (15000-17999)
- Revered = 6 (18000-20999)
- Exalted = 7 (21000+)

### Erwartete Response
- **Bei Erfolg:** Keine Response erwartet
- **Keine Fehler möglich**

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `ReputationListRequest` | 611 | Anfrage aller Reputationen |
| `ReputationListResponse` | 612 | Alle Faction-Standings |

### Beispiel Payload
```csharp
var repChange = new ReputationChangeMessage
{
    Type = MessageType.ReputationChange,
    PlayerId = 12345,
    FactionId = 1,  // Stormwind
    Change = 250,
    NewValue = 15250,
    NewLevel = ReputationLevel.Honored,
    LeveledUp = true,  // Just reached Honored!
    Reason = RepChangeReason.QuestComplete
};
```

### Notizen
- **Reputation-Gains**:
  - Normal Quest: 250 Rep
  - Daily Quest: 150 Rep
  - Elite Monster Kill: 10 Rep
  - Boss Kill: 100 Rep
  - Donation (10g): 50 Rep
- **Reputation-Loss**:
  - Kill Friendly NPC: -500 Rep (allied factions)
  - PvP in faction territory: -50 Rep per kill
- **Level-Requirements**:
  - Hated → Hostile: 3000 Rep
  - Neutral → Friendly: 3000 Rep
  - Honored → Revered: 3000 Rep
  - Revered → Exalted: 3000 Rep (hardest grind)
- **Rewards**:
  - Friendly: 10% Discount
  - Honored: Tabard, Special Items
  - Revered: Epic Mount
  - Exalted: Title, Best-in-Slot Items
- **UI**: Floating "+250 Stormwind Reputation", Reputation-Bar Update

---

## ReputationListRequest (611)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Der Client fordert eine vollständige Liste aller Faction-Reputationen an. Dies wird verwendet für die Reputation-UI oder Character-Info Screen.

Der Server antwortet mit `ReputationListResponse` (612) enthaltend alle Factions und ihre Standings.

### Im Scope ✅
- Anfrage für alle Reputationen
- Optional Filter (nur aktive Factions, nur high-level, etc.)

### Nicht im Scope ❌
- Reputation-Daten selbst → kommt via `ReputationListResponse` (612)
- Single Reputation → via local cache

### Request/Response Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Filter | RepFilter | Optional Filter | Nein |

**RepFilter:**
- All = 0
- ActiveOnly = 1  // Nur Factions mit Rep > 0
- MajorOnly = 2  // Nur Haupt-Factions

### Erwartete Response
- **Bei Erfolg:** `ReputationListResponse` (612)
- **Keine Fehler möglich**

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `ReputationListResponse` | 612 | Response Message |
| `ReputationChange` | 610 | Incremental Updates |

### Beispiel Payload
```csharp
var request = new ReputationListRequestMessage
{
    Type = MessageType.ReputationListRequest,
    Filter = RepFilter.MajorOnly
};
```

### Notizen
- **Frequency**: Nur bei UI-Open (nicht jedes Mal cachen)
- **Size**: ~500 bytes für 20-30 Factions
- **Cache**: Client sollte lokal cachen, Updates via `ReputationChange`

---

## ReputationListResponse (612)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Der Server sendet eine vollständige Liste aller Faction-Reputationen als Response auf `ReputationListRequest` (611).

Enthält Faction-ID, Reputation-Wert, Level, und ob die Faction "at war" ist (PvP Flag).

### Im Scope ✅
- Vollständige Reputation-Liste
- Alle Factions mit Standings
- At-War Flags
- Faction-Details (Name, Description)

### Nicht im Scope ❌
- Reputation-History → nicht tracked
- Reputation-Gains-Detail → via `ReputationChange` (610)

### Request/Response Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Reputations | List<FactionStanding> | Alle Factions | Ja |

**FactionStanding:**
| Feld | Typ | Beschreibung |
|------|-----|--------------|
| FactionId | int | Faction-ID |
| Value | int | Reputation-Wert (0-42000) |
| Level | ReputationLevel | Level (Hated-Exalted) |
| AtWar | bool | PvP Flag |
| FactionName | string | Faction-Name |

### Erwartete Response
- **Bei Erfolg:** Keine weitere Response
- **Keine Fehler möglich**

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `ReputationListRequest` | 611 | Request Message |
| `ReputationChange` | 610 | Incremental Updates |

### Beispiel Payload
```csharp
var response = new ReputationListResponseMessage
{
    Type = MessageType.ReputationListResponse,
    Reputations = new List<FactionStanding>
    {
        new FactionStanding
        {
            FactionId = 1,
            Value = 21000,
            Level = ReputationLevel.Exalted,
            AtWar = false,
            FactionName = "Stormwind"
        },
        new FactionStanding
        {
            FactionId = 2,
            Value = 6000,
            Level = ReputationLevel.Unfriendly,
            AtWar = true,
            FactionName = "Defias Brotherhood"
        }
        // ... more factions
    }
};
```

### Notizen
- **Faction-Count**: ~20-30 Major Factions, ~50-100 Minor Factions
- **At-War**: PvP Flag, Guards attack on sight bei Hostile+AtWar
- **UI**: Reputation-Tab im Character-Sheet

---

## TitleUnlocked (613)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Der Server benachrichtigt den Client dass ein neuer Title freigeschaltet wurde. Titles werden durch Achievements, Reputation, PvP-Ranks, Raid-Clears, oder Events freigeschaltet.

Titles erscheinen vor oder nach dem Character-Namen und sind Prestige-Symbole. Der Client zeigt eine prominente Benachrichtigung.

### Im Scope ✅
- Title Unlock Benachrichtigung
- Title-ID
- Title-Name
- Title-Format (Prefix/Suffix)
- Unlock-Reason (Achievement, Reputation, etc.)

### Nicht im Scope ❌
- Title-Selection → verwende `TitleSelect` (614)
- Alle Titles → verwende `TitleListResponse` (616)

### Request/Response Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| PlayerId | int | Empfänger | Ja |
| TitleId | int | Freigeschaltener Title-ID | Ja |
| TitleName | string | Title-Text | Ja |
| TitleFormat | TitleFormat | Prefix oder Suffix | Ja |
| UnlockReason | UnlockReason | Quelle | Ja |

**TitleFormat:**
- Prefix = 0  // "Champion Thorgar"
- Suffix = 1  // "Thorgar the Unyielding"

### Erwartete Response
- **Bei Erfolg:** Keine Response erwartet
- **Keine Fehler möglich**

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `TitleSelect` | 614 | Title auswählen |
| `TitleListRequest` | 615 | Alle Titles anfordern |
| `TitleListResponse` | 616 | Alle Titles mit Status |

### Beispiel Payload
```csharp
var titleUnlock = new TitleUnlockedMessage
{
    Type = MessageType.TitleUnlocked,
    PlayerId = 12345,
    TitleId = 42,
    TitleName = "the Unyielding",
    TitleFormat = TitleFormat.Suffix,
    UnlockReason = UnlockReason.Achievement
};
```

### Notizen
- **Title-Sources**:
  - Achievements: "the Explorer", "Loremaster"
  - Reputation: "of Stormwind" (Exalted)
  - PvP: "Gladiator", "Warlord"
  - Raids: "Slayer of Dragons"
  - Events: "the Love Fool" (Valentine Event)
- **Title-Count**: ~100-200 Titles total
- **Rarity**: Common (Achievement), Rare (PvP), Epic (Raid), Legendary (World-First)
- **UI**: Large Achievement-Style Popup, Sound

---

## TitleSelect (614)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Der Client wählt einen Title zur Anzeige aus. Nur freigeschaltete Titles können ausgewählt werden. Der Server validiert und broadcastet die Änderung an nahestehende Spieler.

Ein Title kann auch deselected werden (TitleId = 0) um keinen Title anzuzeigen.

### Im Scope ✅
- Title-Auswahl
- Title-ID (oder 0 für kein Title)
- Validation (Title ist unlocked)
- Broadcast an andere Spieler

### Nicht im Scope ❌
- Title-Unlock → verwende `TitleUnlocked` (613)
- Title-Liste → verwende `TitleListResponse` (616)

### Request/Response Payload

**TitleSelect Request:**
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| TitleId | int | Title-ID (0 = kein Title) | Ja |

### Erwartete Response
- **Bei Erfolg:** Broadcast an Zone (Character-Name Update)
- **Bei Fehler:** `ErrorMessage` (910) mit Code
  - `TITLE_NOT_UNLOCKED`: Title nicht freigeschaltet
  - `INVALID_TITLE_ID`: Title existiert nicht

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `TitleUnlocked` | 613 | Title freischalten |
| `CharacterInfo` | 606 | Zeigt selected Title |

### Beispiel Payload
```csharp
var selectTitle = new TitleSelectMessage
{
    Type = MessageType.TitleSelect,
    TitleId = 42  // "the Unyielding"
};
```

### Error Codes
| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `TITLE_NOT_UNLOCKED` | Title nicht freigeschaltet | Zeige "Title nicht verfügbar" |
| `INVALID_TITLE_ID` | Title existiert nicht | Client-Bug, sollte nicht passieren |

### Notizen
- **UI**: Title-Dropdown im Character-Sheet
- **Broadcast**: Andere Spieler sehen neuen Title sofort
- **None-Option**: TitleId = 0 für kein Title
- **Validation**: Server prüft ob Title unlocked ist

---

## TitleListRequest (615)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Der Client fordert eine Liste aller Titles (unlocked und locked) an. Dies wird für die Title-UI verwendet um alle verfügbaren Titles und deren Unlock-Bedingungen anzuzeigen.

### Im Scope ✅
- Anfrage für alle Titles
- Unlocked und Locked Status
- Optional Filter (nur unlocked, nur prefix, etc.)

### Nicht im Scope ❌
- Title-Daten selbst → kommt via `TitleListResponse` (616)

### Request/Response Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Filter | TitleFilter | Optional Filter | Nein |

**TitleFilter:**
- All = 0
- UnlockedOnly = 1
- PrefixOnly = 2
- SuffixOnly = 3

### Erwartete Response
- **Bei Erfolg:** `TitleListResponse` (616)
- **Keine Fehler möglich**

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `TitleListResponse` | 616 | Response Message |

### Beispiel Payload
```csharp
var request = new TitleListRequestMessage
{
    Type = MessageType.TitleListRequest,
    Filter = TitleFilter.All
};
```

---

## TitleListResponse (616)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Der Server sendet eine vollständige Liste aller Titles mit Unlock-Status als Response auf `TitleListRequest` (615).

### Im Scope ✅
- Alle Titles mit Status
- Title-Name, Format, Unlock-Bedingung
- Unlocked/Locked Status
- Selected Title

### Nicht im Scope ❌
- Title-Selection → verwende `TitleSelect` (614)

### Request/Response Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Titles | List<TitleInfo> | Alle Titles | Ja |
| SelectedTitleId | int | Aktuell gewählter Title | Ja |

**TitleInfo:**
| Feld | Typ | Beschreibung |
|------|-----|--------------|
| TitleId | int | Title-ID |
| Name | string | Title-Text |
| Format | TitleFormat | Prefix/Suffix |
| Unlocked | bool | Freigeschaltet? |
| UnlockCondition | string | Beschreibung wie freizuschalten |

### Erwartete Response
- **Bei Erfolg:** Keine weitere Response
- **Keine Fehler möglich**

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `TitleListRequest` | 615 | Request Message |
| `TitleSelect` | 614 | Title auswählen |

### Beispiel Payload
```csharp
var response = new TitleListResponseMessage
{
    Type = MessageType.TitleListResponse,
    Titles = new List<TitleInfo>
    {
        new TitleInfo
        {
            TitleId = 1,
            Name = "the Explorer",
            Format = TitleFormat.Suffix,
            Unlocked = true,
            UnlockCondition = "Explore all zones"
        },
        new TitleInfo
        {
            TitleId = 42,
            Name = "the Unyielding",
            Format = TitleFormat.Suffix,
            Unlocked = true,
            UnlockCondition = "Complete 'Trials of the Unyielding' quest chain"
        },
        new TitleInfo
        {
            TitleId = 100,
            Name = "Gladiator",
            Format = TitleFormat.Prefix,
            Unlocked = false,
            UnlockCondition = "Reach 2400 PvP Rating in Arena Season"
        }
    },
    SelectedTitleId = 42
};
```

### Notizen
- **Size**: ~2-5KB für 100-200 Titles
- **Cache**: Client sollte cachen
- **UI**: Title-Collection UI mit Unlock-Progress

---

## AppearanceChange (617)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Der Client fordert eine Appearance-Änderung an (Hairstyle, Hair Color, Skin Color, Face, etc.). Dies kostet Gold und ist nur in bestimmten Locations möglich (Barbershop).

Der Server validiert die Änderung, charged Gold, und broadcastet die neue Appearance an andere Spieler.

### Im Scope ✅
- Appearance-Optionen ändern
- Hair Style/Color
- Skin Color
- Face Features
- Markings/Tattoos
- Gold-Cost Validation
- Location Validation (Barbershop)

### Nicht im Scope ❌
- Race/Class Change → separate Messages (619, 620)
- Gender Change → separate Message (622)
- Name Change → separate Message (621)
- Equipment → verwende Inventory-Messages

### Request/Response Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| AppearanceOptions | Dictionary<string, int> | Option → Value | Ja |

**Possible Options:**
- "HairStyle": 0-20
- "HairColor": 0-15
- "SkinColor": 0-10
- "FaceType": 0-15
- "FacialHair": 0-10 (Males only)
- "Markings": 0-5
- "Tattoos": 0-5

### Erwartete Response
- **Bei Erfolg:** Broadcast (Character-Appearance Update)
- **Bei Fehler:** `ErrorMessage` (910) mit Code
  - `INSUFFICIENT_GOLD`: Nicht genug Gold
  - `INVALID_LOCATION`: Nicht in Barbershop
  - `INVALID_OPTIONS`: Option nicht für Race/Gender verfügbar

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `AppearancePreview` | 618 | Preview vor Kauf |
| `RaceChange` | 619 | Vollständiger Race-Change |
| `CharacterInfo` | 606 | Zeigt Appearance |

### Beispiel Payload
```csharp
var appearanceChange = new AppearanceChangeMessage
{
    Type = MessageType.AppearanceChange,
    AppearanceOptions = new Dictionary<string, int>
    {
        { "HairStyle", 7 },
        { "HairColor", 3 },
        { "FacialHair", 2 }
    }
};
```

### Error Codes
| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `INSUFFICIENT_GOLD` | Nicht genug Gold (100g) | Zeige Gold-Amount needed |
| `INVALID_LOCATION` | Nicht in Barbershop | Zeige "Find a Barbershop" |
| `INVALID_OPTIONS` | Option nicht verfügbar | Client-Bug |

### Notizen
- **Cost**: 100 Gold
- **Location**: Nur in Barbershops (Major Cities)
- **Restrictions**: Manche Options nur für bestimmte Races/Genders
- **Preview**: Client hat Preview vor Confirm
- **UI**: Barbershop UI mit 3D Character-Preview

---

## AppearancePreview (618)

**Richtung:** 📤 Client → Server (Request) | 📥 Server → Client (Response)  
**Frequenz:** Häufig (während Customization)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Client sendet `AppearancePreviewRequest` an Server mit gewünschten Änderungen. Server validiert und sendet `AppearancePreviewResponse` zurück. Client zeigt Preview ohne zu committen.

Dies erlaubt dem Spieler verschiedene Optionen auszuprobieren bevor er Gold ausgibt.

**Wichtig**: Verwendet zwei separate Message-Typen - `AppearancePreviewRequest` (Client→Server) und `AppearancePreviewResponse` (Server→Client).

### Im Scope ✅
- Preview-Modus aktivieren
- Preview-Changes senden
- Validation ohne Cost
- Client-Side Rendering

### Nicht im Scope ❌
- Actual Change → verwende `AppearanceChange` (617)
- Gold-Charge → nur bei final Confirm

### Request/Response Payload

**Preview Request (Client → Server):**
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| AppearanceOptions | Dictionary<string, int> | Preview-Options | Ja |

**Preview Validation (Server → Client):**
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Valid | bool | Options sind valid | Ja |
| InvalidOptions | List<string> | Invalid Option-Names | Nein |

### Erwartete Response
- **Bei Success:** Preview Validation
- **Bei Fehler:** `ErrorMessage` mit invalid options

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `AppearanceChange` | 617 | Confirm Changes |

### Beispiel Payload
```csharp
// Client → Server
var preview = new AppearancePreviewMessage
{
    Type = MessageType.AppearancePreview,
    AppearanceOptions = new Dictionary<string, int>
    {
        { "HairStyle", 12 },
        { "HairColor", 7 }
    }
};

// Server → Client
var validation = new AppearancePreviewValidationMessage
{
    Type = MessageType.AppearancePreview,
    Valid = true
};
```

### Notizen
- **No Cost**: Preview ist kostenlos
- **Validation**: Server validiert Options aber charged nicht
- **UI**: Real-time 3D Preview
- **Performance**: Throttle Preview-Updates (max 10/second)

---

## RaceChange (619)

**Richtung:** 📤 Client → Server  
**Frequenz:** Sehr Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Der Client fordert einen vollständigen Race-Change an (Phase 2 Feature). Dies ist eine kostenpflichtige Dienstleistung (1000g oder Real-Money).

Race-Change erfordert vollständige Character-Recreation: Appearance-Reset, Faction-Check (manche Races können Faction wechseln), Reputation-Adjustments.

### Im Scope ✅
- Race-Wechsel
- Neue Race-ID
- Appearance-Reset
- Faction-Wechsel (falls nötig)
- Reputation-Adjustments
- Cost Validation (Gold oder Token)

### Nicht im Scope ❌
- Class-Change → verwende `ClassChange` (620)
- Simple Appearance → verwende `AppearanceChange` (617)
- Name-Change → automatisch included falls nötig

### Request/Response Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| NewRace | RaceType | Neue Race | Ja |
| NewAppearance | Dictionary<string, int> | Neue Appearance-Options | Ja |
| NewName | string | Neuer Name (falls Faction-Change) | Nein |
| PaymentMethod | PaymentMethod | Gold oder Token | Ja |

**RaceType:** Orc, Human, Elf, Dwarf, Undead, Tauren, Troll, Gnome, Night Elf

### Erwartete Response
- **Bei Erfolg:** Character-Reload, Zone-Reload
- **Bei Fehler:** `ErrorMessage` (910) mit Code
  - `INSUFFICIENT_FUNDS`: Nicht genug Gold/Tokens
  - `INVALID_RACE`: Race nicht verfügbar
  - `NAME_TAKEN`: Name bereits vergeben (bei Faction-Change)
  - `COOLDOWN_ACTIVE`: Race-Change Cooldown (30 Tage)

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `ClassChange` | 620 | Ähnlicher Service |
| `AppearanceChange` | 617 | Inkludiert in Race-Change |
| `NameChange` | 621 | Optional bei Faction-Change |

### Beispiel Payload
```csharp
var raceChange = new RaceChangeMessage
{
    Type = MessageType.RaceChange,
    NewRace = RaceType.Human,
    NewAppearance = new Dictionary<string, int>
    {
        { "SkinColor", 2 },
        { "HairStyle", 5 },
        { "HairColor", 1 },
        { "FaceType", 3 }
    },
    NewName = "Thorgar",  // Faction-Change: Horde → Alliance
    PaymentMethod = PaymentMethod.Gold
};
```

### Error Codes
| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `INSUFFICIENT_FUNDS` | Nicht genug Gold (1000g) | Zeige benötigten Betrag |
| `INVALID_RACE` | Race nicht verfügbar | Client-Bug |
| `NAME_TAKEN` | Name schon vergeben | Wähle anderen Namen |
| `COOLDOWN_ACTIVE` | 30-Tage Cooldown | Zeige Cooldown-End |

### Notizen
- **Cost**: 1000 Gold ODER Race-Change Token (Real-Money)
- **Cooldown**: 30 Tage zwischen Race-Changes
- **Faction-Change**: Manche Races wechseln Faction (Orc/Tauren → Horde, Human/Dwarf → Alliance)
- **Reputation**: Faction-Rep wird konvertiert (Stormwind → Orgrimmar)
- **Name-Change**: Erzwungen bei Faction-Change (Name-Collision)
- **Restrictions**: Level 10+ required
- **Phase**: Phase 2 Feature
- **UI**: Confirmation-Dialog mit Warnings

---

## ClassChange (620)

**Richtung:** 📤 Client → Server  
**Frequenz:** Sehr Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Der Client fordert einen Class-Wechsel an (Phase 2 Feature). Dies ist sehr teuer (2000g oder Real-Money Token) und hat strikte Einschränkungen.

Class-Change resettet Skills, Talents, Equipment (class-specific items unequipped), Quests (class-specific quests reset).

### Im Scope ✅
- Class-Wechsel
- Neue Class-ID
- Skill/Talent Reset
- Equipment Adjustments
- Quest Reset (class-specific)
- Cost Validation

### Nicht im Scope ❌
- Race-Change → verwende `RaceChange` (619)
- Name-Change → separate Message (621)
- Level-Reset → Level bleibt gleich

### Request/Response Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| NewClass | ClassType | Neue Class | Ja |
| PaymentMethod | PaymentMethod | Gold oder Token | Ja |

**ClassType:** Warrior, Mage, Rogue, Priest, Hunter, Warlock, Paladin, Druid, Shaman

### Erwartete Response
- **Bei Erfolg:** Character-Reload mit neuer Class
- **Bei Fehler:** `ErrorMessage` (910) mit Code
  - `INSUFFICIENT_FUNDS`: Nicht genug Gold (2000g)
  - `INVALID_CLASS_FOR_RACE`: Class nicht verfügbar für Race
  - `COOLDOWN_ACTIVE`: 60-Tage Cooldown
  - `MIN_LEVEL_REQUIRED`: Level 20+ required

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `RaceChange` | 619 | Ähnlicher Service |
| `SkillPointGain` | 608 | Skills werden resettet |

### Beispiel Payload
```csharp
var classChange = new ClassChangeMessage
{
    Type = MessageType.ClassChange,
    NewClass = ClassType.Paladin,
    PaymentMethod = PaymentMethod.Token
};
```

### Error Codes
| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `INSUFFICIENT_FUNDS` | Nicht genug Gold (2000g) | Zeige benötigten Betrag |
| `INVALID_CLASS_FOR_RACE` | Class nicht für Race verfügbar | Zeige verfügbare Classes |
| `COOLDOWN_ACTIVE` | 60-Tage Cooldown | Zeige Cooldown-End |
| `MIN_LEVEL_REQUIRED` | Level 20+ required | Zeige Level-Requirement |

### Notizen
- **Cost**: 2000 Gold ODER Class-Change Token
- **Cooldown**: 60 Tage
- **Level-Requirement**: Level 20+
- **Restrictions**: Race/Class Combinations (z.B. Tauren können nicht Mage sein)
- **Reset**: Skills, Talents, Class-Quests, Equipment unequipped
- **Kept**: Level, Reputation, Gold, Non-Class Items
- **Phase**: Phase 2 Feature
- **UI**: Major Warning-Dialog mit all Changes listed

---

## NameChange (621)

**Richtung:** 📤 Client → Server  
**Frequenz:** Sehr Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Der Client fordert einen Name-Change an. Dies kostet Gold (500g) und unterliegt Name-Validation (Profanity-Filter, Length, Characters).

Name muss unique sein auf dem Server. Nach Name-Change gibt es einen 30-Tage Cooldown.

### Im Scope ✅
- Name-Wechsel
- Neuer Name
- Name-Validation (Profanity, Uniqueness)
- Cost Validation (500g)
- Cooldown-Check (30 Tage)

### Nicht im Scope ❌
- Race/Class-Change → separate Messages
- Guild-Name-Change → separate Guild-Message

### Request/Response Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| NewName | string | Neuer Character-Name | Ja |

**Name-Requirements:**
- 2-12 Characters
- Letters only (a-z, A-Z)
- No special characters
- No profanity
- Unique on server

### Erwartete Response
- **Bei Erfolg:** Broadcast (Name-Update)
- **Bei Fehler:** `ErrorMessage` (910) mit Code
  - `NAME_TAKEN`: Name bereits vergeben
  - `NAME_INVALID`: Name invalid (Profanity, Length, Characters)
  - `INSUFFICIENT_GOLD`: Nicht genug Gold (500g)
  - `COOLDOWN_ACTIVE`: 30-Tage Cooldown

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `CharacterCreate` | 10 | Initial Name-Validation |
| `RaceChange` | 619 | Kann Name-Change erzwingen |

### Beispiel Payload
```csharp
var nameChange = new NameChangeMessage
{
    Type = MessageType.NameChange,
    NewName = "Aragorn"
};
```

### Error Codes
| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `NAME_TAKEN` | Name bereits vergeben | Wähle anderen Namen |
| `NAME_INVALID` | Name invalid | Zeige Validation-Rules |
| `INSUFFICIENT_GOLD` | Nicht genug Gold (500g) | Zeige benötigten Betrag |
| `COOLDOWN_ACTIVE` | 30-Tage Cooldown | Zeige Cooldown-End |

### Notizen
- **Cost**: 500 Gold
- **Cooldown**: 30 Tage
- **Validation**: Profanity-Filter, Uniqueness, Length (2-12), Letters-Only
- **Broadcast**: Name-Change wird in Zone gebroadcastet
- **Guild**: Guild-Roster wird updated
- **Friends**: Friend-List wird updated
- **UI**: Name-Change UI mit Preview

---

## GenderChange (622)

**Richtung:** 📤 Client → Server  
**Frequenz:** Sehr Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Der Client fordert einen Gender-Change an (Phase 2 Feature). Dies kostet Gold (200g) und resettet Appearance-Options (neue Customization required).

Gender-Change ist weniger invasiv als Race/Class-Change, beeinflusst aber manche NPCs-Dialogs und Quest-Texts.

### Im Scope ✅
- Gender-Wechsel (Male ↔ Female)
- Appearance-Reset
- Voice-Change
- Cost Validation (200g)
- Cooldown-Check (30 Tage)

### Nicht im Scope ❌
- Race/Class-Change → separate Messages
- Name-Change → optional, separate Message (621)
- Stats-Change → Stats bleiben gleich

### Request/Response Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| NewGender | Gender | Male oder Female | Ja |
| NewAppearance | Dictionary<string, int> | Neue Appearance-Options | Ja |

### Erwartete Response
- **Bei Erfolg:** Character-Reload mit neuem Gender
- **Bei Fehler:** `ErrorMessage` (910) mit Code
  - `INSUFFICIENT_GOLD`: Nicht genug Gold (200g)
  - `COOLDOWN_ACTIVE`: 30-Tage Cooldown
  - `INVALID_APPEARANCE`: Appearance-Options invalid für Gender

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `AppearanceChange` | 617 | Inkludiert Appearance-Change |
| `RaceChange` | 619 | Kann auch Gender ändern |

### Beispiel Payload
```csharp
var genderChange = new GenderChangeMessage
{
    Type = MessageType.GenderChange,
    NewGender = Gender.Female,
    NewAppearance = new Dictionary<string, int>
    {
        { "FaceType", 2 },
        { "HairStyle", 8 },
        { "HairColor", 4 },
        { "SkinColor", 1 }
    }
};
```

### Error Codes
| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `INSUFFICIENT_GOLD` | Nicht genug Gold (200g) | Zeige benötigten Betrag |
| `COOLDOWN_ACTIVE` | 30-Tage Cooldown | Zeige Cooldown-End |
| `INVALID_APPEARANCE` | Appearance invalid | Client-Bug |

### Notizen
- **Cost**: 200 Gold
- **Cooldown**: 30 Tage
- **Appearance**: Vollständiger Reset mit neuen Options
- **Voice**: Voice-Over ändert sich
- **NPCs**: Manche NPC-Dialogs ändern sich
- **Stats**: Keine Stat-Changes (Gender hat keinen Gameplay-Impact)
- **Equipment**: Equipment bleibt equipped (most items sind unisex)
- **Phase**: Phase 2 Feature
- **UI**: Gender-Change UI mit 3D-Preview

---

## RestXpUpdate (623)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Der Server sendet Updates über Rest-XP (Bonus-XP für ausgeloggte Zeit in Inns/Cities). Rest-XP gibt 150% XP für Kills/Quests bis der Bonus aufgebraucht ist.

Rest-XP akkumuliert mit 5% einer Level-Bar pro 8 Stunden (max 1.5 Levels worth). Dies encouraged casual play patterns.

### Im Scope ✅
- Rest-XP Amount Update
- Rest-XP Bar Status
- Accumulation Rate
- Consumption Rate
- Max Rest-XP

### Nicht im Scope ❌
- XP-Gain selbst → verwende `XpGain` (601)
- Rest-State → verwende `RestStateChange` (624)

### Request/Response Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| PlayerId | int | Betroffener Spieler | Ja |
| RestXp | int | Verbleibende Rest-XP | Ja |
| MaxRestXp | int | Max Rest-XP (1.5 Levels) | Ja |
| IsRested | bool | Currently Rested (150% XP) | Ja |
| AccumulationRate | int | XP/Hour wenn in Inn | Ja |

### Erwartete Response
- **Bei Erfolg:** Keine Response erwartet
- **Keine Fehler möglich**

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `XpGain` | 601 | Verbraucht Rest-XP |
| `RestStateChange` | 624 | Entered/Left Rested Area |

### Beispiel Payload
```csharp
var restXpUpdate = new RestXpUpdateMessage
{
    Type = MessageType.RestXpUpdate,
    PlayerId = 12345,
    RestXp = 5000,
    MaxRestXp = 15000,  // 1.5 levels worth
    IsRested = true,
    AccumulationRate = 625  // Per 8 hours
};
```

### Notizen
- **Accumulation**: 5% Level-Bar / 8 hours in Inn/City
- **Max**: 1.5 Levels worth (150% einer Level-Bar)
- **Bonus**: 150% XP (statt 100%)
- **Consumption**: 1:1 mit earned XP (1 Rest-XP = 0.5 Bonus-XP)
- **Rested Areas**: Inns, Major Cities
- **UI**: Rest-XP Bar (Blue portion of XP-Bar)
- **Encouragement**: Casual-Friendly Mechanic

---

## RestStateChange (624)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Der Server benachrichtigt den Client über Rest-State Änderungen. Der Spieler entered/left eine Rested Area (Inn, City) wo Rest-XP akkumuliert.

Der Client zeigt ein "Zzz" Icon und ändert die XP-Bar Farbe um den Rested-State anzuzeigen.

### Im Scope ✅
- Rest-State Change (Entered/Left)
- Rested Area-Type (Inn, City, Sanctuary)
- Rest-XP Accumulation aktiviert/deaktiviert
- UI-Indicator (Zzz Icon)

### Nicht im Scope ❌
- Rest-XP Amount → verwende `RestXpUpdate` (623)
- XP-Gain → verwende `XpGain` (601)

### Request/Response Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| PlayerId | int | Betroffener Spieler | Ja |
| IsRested | bool | Entered (true) oder Left (false) | Ja |
| RestedAreaType | RestedAreaType | Inn, City, Sanctuary | Ja |
| AccumulationActive | bool | Rest-XP Accumulation aktiv | Ja |

**RestedAreaType:**
- Inn = 0  // Fastest Accumulation
- City = 1  // Normal Accumulation
- Sanctuary = 2  // Slow Accumulation (Phase 2)

### Erwartete Response
- **Bei Erfolg:** Keine Response erwartet
- **Keine Fehler möglich**

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `RestXpUpdate` | 623 | Rest-XP Amount Updates |
| `XpGain` | 601 | Rest-Bonus angewendet |

### Beispiel Payload
```csharp
var restState = new RestStateChangeMessage
{
    Type = MessageType.RestStateChange,
    PlayerId = 12345,
    IsRested = true,  // Entered Inn
    RestedAreaType = RestedAreaType.Inn,
    AccumulationActive = true
};
```

### Notizen
- **Rested Areas**:
  - **Inns**: Fastest (5% / 8h)
  - **Cities**: Normal (3% / 8h)
  - **Sanctuaries**: Slow (1% / 8h) - Phase 2
- **Trigger**: Zone-Change, SubZone-Change
- **Accumulation**: Only when logged out in Rested Area
- **Online**: No Accumulation while online (prevents AFK-Farming)
- **UI**: "Zzz" Icon near portrait, XP-Bar turns blue
- **Sound**: Relaxing ambient sound in Inns

---

**Letzte Aktualisierung**: 2025-12-17  
**Version**: 2.0.0

[← Zurück zur Übersicht](README.md)
