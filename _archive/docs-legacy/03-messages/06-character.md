# 👤 Character Messages (0600-0655)

**Kategorie:** 6  
**Range:** 0600-0655 (AKTIV)  
**Phase:** Prototyp  
**Status:** 🟢 In Entwicklung

[← Zurück zur Übersicht](README.md)

---

## 📋 Inhaltsverzeichnis

- [🔄 Character Flow (Übersicht)](#-character-flow-übersicht)
- [🧱 DTOs / Enums / Interfaces](#-dtos--enums--interfaces)
  - [CharacterProgressionDto](#characterprogressiondto)
  - [CharacterStatsDto](#characterstatsdto)
  - [ResourceDto](#resourcedto)
  - [ReputationDto](#reputationdto)
  - [AppearanceDto](#appearancedto)
- [📩 Aktive Messages](#-aktive-messages)
  - [LevelUp (600)](#levelup-600)
  - [XpGain (601)](#xpgain-601)
  - [StatUpdate (602)](#statupdate-602)
  - [StatFullSync (603)](#statfullsync-603)
  - [ResourceUpdate (604)](#resourceupdate-604)
  - [ResourceRegen (605)](#resourceregen-605)
  - [CharacterInfo (606)](#characterinfo-606)
  - [CharacterInfoRequest (607)](#characterinforequest-607)
  - [SkillPointGain (608)](#skillpointgain-608)
  - [TalentPointGain (609)](#talentpointgain-609)
  - [ReputationChange (610)](#reputationchange-610)
  - [ReputationListRequest (611)](#reputationlistrequest-611)
  - [ReputationListResponse (612)](#reputationlistresponse-612)
  - [TitleUnlocked (613)](#titleunlocked-613)
  - [TitleSelect (614)](#titleselect-614)
  - [AppearanceChange (617)](#appearancechange-617)
  - [AppearancePreview (618)](#appearancepreview-618)
  - [RaceChange (619)](#racechange-619)
  - [ClassChange (620)](#classchange-620)
  - [NameChange (621)](#namechange-621)
  - [GenderChange (622)](#genderchange-622)
  - [RestXpUpdate (623)](#restxpupdate-623)
  - [RestStateChange (624)](#reststatechange-624)
  - [AttributeIncreaseResponse (650)](#attributeincreaseresponse-650)
  - [CharacterCustomizeResponse (651)](#charactercustomizeresponse-651)
  - [TalentLearnResponse (652)](#talentlearnresponse-652)
  - [TalentResetResponse (653)](#talentresetresponse-653)
  - [SpecializationChangeResponse (654)](#specializationchangeresponse-654)
  - [TitleChangeResponse (655)](#titlechangeresponse-655)
- [🗑️ Obsolete Messages](#️-obsolete-messages)
- [📎 Anhang](#-anhang)

---

## 🔄 Character Flow (Übersicht)

Diese Kategorie umfasst alle Messages für **Character-Management, Progression, und Customization** im 2DMMO. Der Server ist **immer autoritativ** für alle Character-Daten.

### Architektur: Server-Authoritative Character System

```
┌─────────────────────────────────────────────────────────────────┐
│  CharacterProgressionDto (Progression)                          │
│  ├── Level, CurrentXp, XpToNextLevel                            │
│  ├── UnspentStatPoints, UnspentTalentPoints                     │
│  └── RestXp, RestBonusPercent                                   │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│  CharacterStatsDto (Attribute + Derived Stats)                  │
│  ├── Strength, Dexterity, Intelligence, Stamina, Spirit         │
│  ├── AttackPower, SpellPower, Armor, CritChance                 │
│  └── DodgeChance, BlockChance, HastePercent                     │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│  ResourceDto (Health, Mana, Energy, Rage)                       │
│  ├── Current, Max                                               │
│  └── RegenPerSecond                                             │
└─────────────────────────────────────────────────────────────────┘
```

### XP-Gain + Level-Up Flow

```
Client                         Server                    DB
  │                              │                        │
  │  (Monster Kill)              │                        │
  │                              │                        │
  │                              │  Calculate XP          │
  │                              │  (Base * Bonuses)      │
  │                              │                        │
  │  XpGain (601)                │                        │
  │◄─────────────────────────────│                        │
  │  Amount: 150                 │                        │
  │  Source: "Goblin"            │                        │
  │                              │                        │
  │                              │  Check LevelUp         │
  │                              │  (CurrentXP >= Req)    │
  │                              │                        │
  │  LevelUp (600) [if reached]  │                        │
  │◄─────────────────────────────│                        │
  │  NewLevel: 11                │                        │
  │  StatPoints: +5              │                        │
  │                              │                        │
  │                              │  Persist to DB         │
  │                              │───────────────────────►│
```

### Resource Update Flow (HP/Mana/Energy)

```
Client                         Server
  │                              │
  │  (Damage/Heal/Regen Event)   │
  │                              │
  │  ResourceUpdate (604)        │
  │◄─────────────────────────────│
  │  Type: Health                │
  │  Current: 850                │
  │  Max: 1200                   │
  │                              │
  │  (UI: Update Health Bar)     │
  │                              │
```

### Character Customization Flow

```
Client                         Server                    DB
  │                              │                        │
  │  AppearanceChange (617)      │                        │
  │  HairStyle: 5                │                        │
  │  HairColor: #FF0000          │                        │
  │───────────────────────────────►                       │
  │                              │  Validate Gold         │
  │                              │  Validate Options      │
  │                              │                        │
  │  CharacterCustomizeResponse  │                        │
  │  (651), Success=true         │                        │
  │◄─────────────────────────────│                        │
  │                              │  Persist              │
  │                              │───────────────────────►│
```

---

## 🧱 DTOs / Enums / Interfaces

### CharacterProgressionDto

```csharp
[MessagePackObject]
public class CharacterProgressionDto
{
    [Key(0)] public int Level { get; set; }
    [Key(1)] public long CurrentXp { get; set; }
    [Key(2)] public long XpToNextLevel { get; set; }
    [Key(3)] public int UnspentStatPoints { get; set; }
    [Key(4)] public int UnspentTalentPoints { get; set; }
    [Key(5)] public long RestXp { get; set; }
    [Key(6)] public float RestBonusPercent { get; set; } // 0.5 = +50%
}
```

### CharacterStatsDto

```csharp
[MessagePackObject]
public class CharacterStatsDto
{
    // Primary Attributes
    [Key(0)] public int Strength { get; set; }
    [Key(1)] public int Dexterity { get; set; }
    [Key(2)] public int Intelligence { get; set; }
    [Key(3)] public int Stamina { get; set; }
    [Key(4)] public int Spirit { get; set; }
    
    // Derived Stats
    [Key(5)] public int AttackPower { get; set; }
    [Key(6)] public int SpellPower { get; set; }
    [Key(7)] public int Armor { get; set; }
    [Key(8)] public float CritChance { get; set; }      // 0.0-1.0
    [Key(9)] public float DodgeChance { get; set; }     // 0.0-1.0
    [Key(10)] public float BlockChance { get; set; }    // 0.0-1.0
    [Key(11)] public float HastePercent { get; set; }   // 0.0-1.0
}
```

### ResourceDto

```csharp
[MessagePackObject]
public class ResourceDto
{
    [Key(0)] public ResourceType Type { get; set; }
    [Key(1)] public int Current { get; set; }
    [Key(2)] public int Max { get; set; }
    [Key(3)] public float RegenPerSecond { get; set; }
}

public enum ResourceType : byte
{
    Health = 0,
    Mana = 1,
    Energy = 2,
    Rage = 3,
    Focus = 4
}
```

### ReputationDto

```csharp
[MessagePackObject]
public class ReputationDto
{
    [Key(0)] public int FactionId { get; set; }
    [Key(1)] public string FactionName { get; set; }
    [Key(2)] public int CurrentValue { get; set; }      // -42000 to +42000
    [Key(3)] public ReputationStanding Standing { get; set; }
    [Key(4)] public int ValueToNextStanding { get; set; }
}

public enum ReputationStanding : byte
{
    Hated = 0,       // -42000 to -6000
    Hostile = 1,     // -6000 to -3000
    Unfriendly = 2,  // -3000 to 0
    Neutral = 3,     // 0 to 3000
    Friendly = 4,    // 3000 to 9000
    Honored = 5,     // 9000 to 21000
    Revered = 6,     // 21000 to 42000
    Exalted = 7      // 42000
}
```

### AppearanceDto

```csharp
[MessagePackObject]
public class AppearanceDto
{
    [Key(0)] public byte HairStyle { get; set; }
    [Key(1)] public uint HairColor { get; set; }     // RGB
    [Key(2)] public byte FaceType { get; set; }
    [Key(3)] public uint SkinColor { get; set; }     // RGB
    [Key(4)] public byte Gender { get; set; }        // 0=Male, 1=Female
}
```

---

## 📩 Aktive Messages

---

## LevelUp (600)

**Richtung:** 📡 Broadcast (Server → Client + Nearby)  
**Frequenz:** Selten (nur bei Level-Aufstieg)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Character hat ein Level aufgestiegen. Server broadcastet Level-Up Event an alle Spieler in Sichtweite mit Visual-Effects und Sound. HP/Mana werden auf 100% restored.

### Im Scope ✅
- Level-Up Notification an Self und Nearby
- Stat-Points Reward (5 pro Level)
- Talent-Points Reward (1 ab Level 10)
- HP/Mana Full-Restore
- Visual-Effect an Character-Position

### Nicht im Scope ❌
- XP-Gain selbst → verwende `XpGain` (601)
- Stat-Distribution → verwende Skill-System

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| CharacterId | long | Character-ID | Ja |
| Name | string | Character-Name | Ja |
| Race | byte | Race-ID (1=Human, 2=Elf, 3=Dwarf, 4=Orc) | Ja |
| Class | byte | Class-ID (1=Warrior, 2=Mage, 3=Rogue, 4=Priest) | Ja |
| Level | int | Aktuelles Level (1-60) | Ja |
| CurrentXP | long | Aktuelle XP | Ja |
| RequiredXP | long | XP für nächstes Level | Ja |
| UnspentStatPoints | int | Verfügbare Stat-Points | Ja |
| UnspentTalentPoints | int | Verfügbare Talent-Points | Ja |
| PrimaryAttributes | AttributeBlock | Primäre Attribute | Ja |
| DerivedStats | DerivedStatsBlock | Berechnete Stats | Ja |
| EquippedTitle | uint | Title-ID (0=kein Title) | Nein |
| Appearance | AppearanceData | Appearance-Daten | Ja |
| Specialization | byte | Spec-ID (0=keine) | Nein |

**AttributeBlock:**
| Feld | Typ | Beschreibung |
|------|-----|--------------|
| Strength | int | Stärke (Base + Bonus) |
| Dexterity | int | Geschicklichkeit |
| Intelligence | int | Intelligenz |
| Stamina | int | Ausdauer |
| Spirit | int | Willenskraft |

**DerivedStatsBlock:**
| Feld | Typ | Beschreibung |
|------|-----|--------------|
| MaxHP | int | Maximale HP |
| MaxMana | int | Maximale Mana |
| Armor | int | Rüstung |
| AttackPower | int | Angriffskraft |
| SpellPower | int | Zaubermacht |
| CritChance | float | Krit-Chance (0.0-1.0) |
| DodgeChance | float | Ausweich-Chance |
| BlockChance | float | Block-Chance |

**AppearanceData:**
| Feld | Typ | Beschreibung |
|------|-----|--------------|
| HairStyle | byte | Hairstyle-ID |
| HairColor | uint | RGB-Color |
| SkinColor | uint | RGB-Color |
| FaceType | byte | Face-ID |

### Erwartete Response
- Keine Response erforderlich (ist selbst Response)

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `StatsUpdate` | 601 | Für laufende Stat-Updates |
| `LevelUp` | 602 | Bei Level-Aufstieg |
| `ExperienceGain` | 603 | Bei XP-Gain |
| `CharacterListResponse` | 13 | Zeigt Characters vor Auswahl |

### Beispiel Payload
```csharp
var characterInfo = new CharacterInfo
{
    Type = MessageType.CharacterInfo,
    CharacterId = 98765,
    Name = "Aragorn",
    Race = 1, // Human
    Class = 1, // Warrior
    Level = 10,
    CurrentXP = 25000,
    RequiredXP = 50000,
    UnspentStatPoints = 5,
    UnspentTalentPoints = 2,
    PrimaryAttributes = new AttributeBlock
    {
        Strength = 25,  // 15 Base + 10 Bonus
        Dexterity = 18,
        Intelligence = 12,
        Stamina = 22,
        Spirit = 15
    },
    DerivedStats = new DerivedStatsBlock
    {
        MaxHP = 1200,
        MaxMana = 500,
        Armor = 350,
        AttackPower = 180,
        SpellPower = 60,
        CritChance = 0.15f, // 15%
        DodgeChance = 0.08f,
        BlockChance = 0.12f
    },
    EquippedTitle = 101, // "Knight"
    Appearance = new AppearanceData
    {
        HairStyle = 3,
        HairColor = 0x8B4513, // Brown
        SkinColor = 0xFFDBBE,
        FaceType = 1
    },
    Specialization = 1 // Arms Warrior
};
```

### Notizen
- **Loading-Time**: 100-300ms zum Laden aller Daten
- **Caching**: Client cached Character-Info lokal
- **Updates**: Laufende Updates via dedizierte Messages
- **Stat-Calculation**: Derived Stats werden server-seitig berechnet

---

## XpGain (601)

**Richtung:** 📡 Broadcast (Server → Client + Nearby Players)  
**Frequenz:** ⚡ Sehr häufig (Combat, Buffs, Regen)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Update der aktuellen Character-Resources (HP, Mana). Wird bei Damage, Healing, Mana-Verbrauch, oder Regeneration gesendet.

Für Nearby-Players wird nur HP gesendet (nicht Mana), außer sie sind in Party/Raid.

### Im Scope ✅
- Current HP/Mana Updates
- Max HP/Mana Changes (durch Buffs/Equipment)
- Resource-Regeneration
- Broadcast an Party-Members (volle Daten)
- Broadcast an Nearby-Players (nur HP)

### Nicht im Scope ❌
- Attribute-Changes → verwende `AttributeUpdate` (dedizierte Message)
- Level-Up → verwende `LevelUp` (602)

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| EntityId | int | Character-ID | Ja |
| CurrentHP | int | Aktuelle HP | Ja |
| MaxHP | int | Maximale HP | Ja |
| CurrentMana | int | Aktuelles Mana (nur für Self/Party) | Nein |
| MaxMana | int | Maximales Mana (nur für Self/Party) | Nein |
| CurrentEnergy | int | Energy (Rogue-Resource, nur Self) | Nein |
| CurrentRage | int | Rage (Warrior-Resource, nur Self) | Nein |

### Erwartete Response
- Keine Response erforderlich

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `DamageEvent` | 302 | Reduziert HP |
| `HealEvent` | 304 | Erhöht HP |
| `ResourceUpdate` | 604 | Alternative für Resource-Updates |

### Beispiel Payload
```csharp
// Für Self/Party (volle Daten)
var statsUpdate = new StatsUpdate
{
    Type = MessageType.StatsUpdate,
    EntityId = 98765,
    CurrentHP = 950,
    MaxHP = 1200,
    CurrentMana = 380,
    MaxMana = 500,
    CurrentEnergy = 0, // Not a Rogue
    CurrentRage = 45 // Warrior
};

// Für Nearby-Players (nur HP)
var nearbyUpdate = new StatsUpdate
{
    Type = MessageType.StatsUpdate,
    EntityId = 98765,
    CurrentHP = 950,
    MaxHP = 1200
    // Kein Mana/Energy/Rage für Fremde
};
```

### Notizen
- **Update-Frequency**: Max 10/Sekunde (gebatched)
- **Party-Sharing**: Party-Members sehen HP+Mana
- **Privacy**: Nur Self sieht Energy/Rage
- **Regen**: Out-of-Combat: 5% HP/Mana pro 5s, In-Combat: 1% pro 5s

---

## StatUpdate (602)

**Richtung:** 📡 Broadcast (Server → Client + Nearby Players)  
**Frequenz:** Selten (nur bei Level-Up)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Character hat ein Level aufgestiegen! Server broadcastet Level-Up Event an alle Spieler in Sichtweite mit Visual-Effects und Sound.

### Im Scope ✅
- Level-Up Notification
- Stat-Points Reward
- Talent-Points Reward (ab Level 10)
- HP/Mana Full-Restore
- Visual-Effect an Character-Position

### Nicht im Scope ❌
- XP-Gain selbst → verwende `ExperienceGain` (603)
- Stat-Distribution → verwende `AttributeIncrease` (605)

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| EntityId | int | Character-ID | Ja |
| CharacterName | string | Character-Name (für UI) | Ja |
| OldLevel | int | Vorheriges Level | Ja |
| NewLevel | int | Neues Level | Ja |
| StatPointsGained | int | Erhaltene Stat-Points (5 pro Level) | Ja |
| TalentPointsGained | int | Erhaltene Talent-Points (1 ab Level 10) | Ja |
| NewMaxHP | int | Neue Max-HP | Ja |
| NewMaxMana | int | Neue Max-Mana | Ja |

### Erwartete Response
- Keine Response erforderlich

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `ExperienceGain` | 603 | XP-Gain vor Level-Up |
| `AttributeIncrease` | 605 | Stat-Points verwenden |
| `TalentLearn` | 620 | Talent-Points verwenden |
| `ChatSystem` | 410 | "Congratulations! You reached Level X!" |

### Flow-Diagramm
```
Client                    Server                 All Nearby Players
  │                          │                          │
  │  Kill Enemy              │                          │
  │                          │  Calculate XP            │
  │                          │  Check Level-Up          │
  │                          │                          │
  │  ExperienceGain (603)    │                          │
  │◄─────────────────────────│                          │
  │                          │                          │
  │  LevelUp (602)           │   LevelUp (602)          │
  │◄─────────────────────────│─────────────────────────►│
  │                          │                          │
  │  (Visual Effect)         │         (Visual Effect)  │
  │  (Sound)                 │         (Sound)          │
  │  (HP/Mana Restore)       │                          │
  │                          │                          │
  │  ChatSystem (410)        │                          │
  │  "Congrats Level 11!"    │                          │
  │◄─────────────────────────│                          │
```

### Beispiel Payload
```csharp
var levelUp = new LevelUp
{
    Type = MessageType.LevelUp,
    EntityId = 98765,
    CharacterName = "Aragorn",
    OldLevel = 10,
    NewLevel = 11,
    StatPointsGained = 5,
    TalentPointsGained = 1,
    NewMaxHP = 1300, // War 1200
    NewMaxMana = 550  // War 500
};
```

### Notizen
- **Visual-Effect**: Goldener Strahl vom Himmel + Partikel-Effekt
- **Sound**: Epic Level-Up Sound
- **HP/Mana**: Automatisch auf 100% restored
- **Stat-Points**: 5 pro Level
- **Talent-Points**: 1 pro Level (ab Level 10)
- **Max-Level**: 60 (Prototyp), 100 (geplant)
- **Broadcast-Range**: 50m Radius

---

## StatFullSync (603)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig (nach Kill, Quest-Complete, etc.)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Character hat XP gewonnen. Server sendet XP-Gain nach Mob-Kill, Quest-Completion, oder anderen XP-Quellen. Inkludiert Rested-XP Bonus falls aktiv.

### Im Scope ✅
- XP-Amount
- XP-Source (Kill, Quest, Discovery, etc.)
- Rested-XP Bonus-Indicator
- Progress-zu-nächstem-Level

### Nicht im Scope ❌
- Level-Up selbst → verwende `LevelUp` (602)

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Amount | int | Gewonnene XP (inkl. Bonus) | Ja |
| BaseAmount | int | Base-XP (ohne Bonus) | Ja |
| Source | string | "kill", "quest", "discovery", "event" | Ja |
| SourceName | string | z.B. "Goblin Warrior" oder "Quest: Save the Village" | Ja |
| CurrentXP | long | Aktuelle Gesamt-XP | Ja |
| RequiredXP | long | XP für nächstes Level | Ja |
| RestedXPBonus | bool | Rested-XP Bonus aktiv? | Ja |
| BonusPercent | float | Bonus-% (z.B. 1.5 für +50%) | Nein |

### Erwartete Response
- Keine Response erforderlich

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `LevelUp` | 602 | Folgt falls XP >= RequiredXP |
| `DeathEvent` | 303 | XP-Source via Kill |
| `QuestComplete` | 1002 | XP-Source via Quest |

### Beispiel Payload
```csharp
// Mob-Kill
var xpGain = new ExperienceGain
{
    Type = MessageType.ExperienceGain,
    Amount = 150, // 100 Base + 50 Bonus
    BaseAmount = 100,
    Source = "kill",
    SourceName = "Goblin Warrior",
    CurrentXP = 25150,
    RequiredXP = 50000,
    RestedXPBonus = true,
    BonusPercent = 1.5f // +50% Rested
};

// Quest-Completion
var questXP = new ExperienceGain
{
    Type = MessageType.ExperienceGain,
    Amount = 5000,
    BaseAmount = 5000,
    Source = "quest",
    SourceName = "Quest: Save the Village",
    CurrentXP = 30150,
    RequiredXP = 50000,
    RestedXPBonus = false
};
```

### Notizen
- **UI-Display**: "+150 XP (Goblin Warrior)" als Floating-Text
- **Rested-XP**: +50% Bonus für Offline-Zeit (geplant)
- **Party-Sharing**: XP wird geteilt in Party (mit Bonus für Gruppe)
- **Level-Cap**: Bei Max-Level wird XP nicht mehr angezeigt

---

## ResourceUpdate (604)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Alternative zu `StatsUpdate` (601) speziell für Resource-Updates (HP, Mana, Energy, Rage). Kann mehr Details enthalten.

### Im Scope ✅
- Detaillierte Resource-Updates
- Regeneration-Rate
- Resource-Type (Health, Mana, Energy, Rage)

### Nicht im Scope ❌
- Max-Werte → verwende `StatsUpdate` (601)

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| ResourceType | string | "health", "mana", "energy", "rage" | Ja |
| Current | int | Aktueller Wert | Ja |
| Max | int | Maximaler Wert | Ja |
| RegenRate | float | Regen pro Sekunde | Nein |

### Beispiel Payload
```csharp
var resourceUpdate = new ResourceUpdate
{
    Type = MessageType.ResourceUpdate,
    ResourceType = "mana",
    Current = 380,
    Max = 500,
    RegenRate = 10.5f // 10.5 Mana/s
};
```

### Notizen
- **Alternative**: Kann statt `StatsUpdate` (601) verwendet werden
- **Detail-Level**: Mehr Info als StatsUpdate

---

## ResourceRegen (605)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten (nur bei Stat-Point-Spending)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Client möchte verfügbare Stat-Points in Attribute investieren. Server validiert ob Points vorhanden und führt Increase durch.

### Im Scope ✅
- Single-Point oder Multi-Point Investment
- Alle 5 Primary Attributes (Strength, Dexterity, Intelligence, Stamina, Spirit)
- Auto-Recalculation von Derived Stats

### Nicht im Scope ❌
- Attribute-Decrease → nur via Respec (geplant)
- Talent-Points → verwende `TalentLearn` (620)

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Attribute | string | "strength", "dexterity", "intelligence", "stamina", "spirit" | Ja |
| Points | int | Anzahl Points zu investieren (1-10) | Ja |

### Erwartete Response
- `AttributeIncreaseResponse` (650)

### Folge-Messages bei Erfolg
- `StatsUpdate` (601) mit neuen Derived Stats

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `StatsUpdate` | 601 | Derived Stats werden updated |
| `LevelUp` | 602 | Source von Stat-Points |

### Flow-Diagramm
```
Client                    Server
  │                          │
  │  (Player clicks +STR)    │
  │                          │
  │  AttributeIncrease (605) │
  │  Attribute="strength"    │
  │  Points=5                │
  │─────────────────────────►│
  │                          │  ┌─ Validate: 5 Points available?
  │                          │  ├─ Strength += 5
  │                          │  ├─ Recalculate Derived Stats
  │                          │  │  (HP, Armor, AttackPower)
  │                          │  └─ UnspentPoints -= 5
  │                          │
  │  AttributeIncreaseSuccess│
  │◄─────────────────────────│
  │                          │
  │  StatsUpdate (601)       │
  │  (New Derived Stats)     │
  │◄─────────────────────────│
```

### Beispiel Payload
```csharp
var attributeIncrease = new AttributeIncrease
{
    Type = MessageType.AttributeIncrease,
    Attribute = "strength",
    Points = 5
};
```

### Error Codes
| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `INSUFFICIENT_POINTS` | Nicht genug Stat-Points | Erst leveln |
| `INVALID_ATTRIBUTE` | Ungültiger Attribute-Name | Korrekten Namen verwenden |
| `MAX_ATTRIBUTE_REACHED` | Attribute-Cap erreicht (999) | - |
| `INVALID_POINTS` | Points < 1 oder > 10 | Valide Anzahl |

### Notizen
- **Stat-Point-Source**: 5 pro Level-Up
- **Derived-Stats Calculation**:
  - **Strength**: +2 Attack Power, +0.1 Armor pro Point
  - **Dexterity**: +0.5% Crit, +0.3% Dodge pro Point
  - **Intelligence**: +3 Spell Power, +10 Mana pro Point
  - **Stamina**: +10 HP pro Point
  - **Spirit**: +5 Mana Regen/5s pro Point
- **Cap**: Max 999 pro Attribute (praktisch unmöglich zu erreichen)
- **No Refund**: Keine Respec im Prototyp (Hinweis: Respec für Gold)

---

## ReputationChange (610)

**Richtung:** 📤 Client → Server  
**Frequenz:** Sehr selten (nur via Barber-Shop)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Client ändert Character-Appearance (Hairstyle, Hair-Color, etc.). Erfordert Barber-Shop NPC und kostet Gold.

### Im Scope ✅
- Hairstyle-Change
- Hair-Color-Change
- Face-Type-Change (geplant)
- Skin-Color-Change (Phase 2, race-restricted)
- Cost-Validation

### Nicht im Scope ❌
- Race-Change → geplant Feature
- Class-Change → nicht möglich
- Name-Change → geplant via Support

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| HairStyle | byte | Neue Hairstyle-ID | Nein |
| HairColor | uint | Neue RGB-Color | Nein |
| FaceType | byte | Neue Face-ID | Nein |
| SkinColor | uint | Neue RGB-Color (race-restricted) | Nein |

### Erwartete Response
- `CharacterCustomizeResponse` (651)

### Folge-Messages bei Erfolg
- `GoldUpdate` (3703) mit neuem Gold-Betrag
- `AppearanceUpdate` (631) Broadcast an nahe Spieler

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `NPCInteract` | 1300 | Barber-Shop NPC-Interaction |
| `GoldUpdate` | 3703 | Gold-Cost |

### Beispiel Payload
```csharp
var customize = new CharacterCustomize
{
    Type = MessageType.CharacterCustomize,
    HairStyle = 5, // New Style
    HairColor = 0xFF0000, // Red
    // FaceType und SkinColor nicht geändert
};
```

### Error Codes
| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `INSUFFICIENT_GOLD` | Nicht genug Gold | Gold farmen |
| `INVALID_HAIRSTYLE` | Hairstyle für Race nicht verfügbar | Anderen wählen |
| `INVALID_SKINCOLOR` | Skin-Color für Race nicht erlaubt | Race-spezifische Color |
| `NOT_AT_BARBER` | Nicht bei Barber-NPC | Zu Barber gehen |

### Notizen
- **Cost**: 10 Gold für Hairstyle/Color-Change
- **Prototyp**: Nur Hairstyle + HairColor verfügbar
- **Hinweis**: Face-Type, Skin-Color
- **Barber-Shop**: Nur in Hauptstädten
- **Preview**: Client zeigt Preview vor Bestätigung

---

## ClassChange (620)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
**Feature** - Client möchte Talent lernen. Server validiert ob Talent-Points vorhanden, Voraussetzungen erfüllt, und aktiviert Talent.

### Im Scope ✅
- Talent-Learning
- Talent-Tree-Navigation
- Prerequisite-Check
- Talent-Point-Spending

### Nicht im Scope ❌
- Talent-Unlearn → verwende `TalentReset` (621)

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| TalentId | uint | Talent-ID aus Talent-Tree | Ja |
| Rank | byte | Talent-Rank (1-5) | Ja |

### Erwartete Response
- `TalentLearnResponse` (652)

### Folge-Messages bei Erfolg
- Talent-Tree UI wird aktualisiert

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `TalentReset` | 621 | Talents zurücksetzen |
| `LevelUp` | 602 | Source von Talent-Points |

### Beispiel Payload
```csharp
var talentLearn = new TalentLearn
{
    Type = MessageType.TalentLearn,
    TalentId = 1001, // "Improved Strike"
    Rank = 3 // Rank 3/5
};
```

### Error Codes
| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `INSUFFICIENT_POINTS` | Nicht genug Talent-Points | Erst leveln |
| `TALENT_LOCKED` | Voraussetzungen nicht erfüllt | Prerequisite-Talents lernen |
| `MAX_RANK_REACHED` | Talent bereits max Rank | - |
| `INVALID_SPECIALIZATION` | Talent für andere Spec | Spec wechseln |

### Notizen
- **Hinweis**: Nicht im Prototyp
- **Talent-Points**: 1 pro Level ab Level 10
- **Talent-Trees**: 3 Trees pro Class
- **Max-Rank**: Meist 5 Ranks pro Talent

---

## NameChange (621)

**Richtung:** 📤 Client → Server  
**Frequenz:** Sehr selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
**Feature** - Client möchte alle Talents zurücksetzen (Respec). Kostet Gold (Preis steigt mit jeder Respec).

### Im Scope ✅
- Vollständiger Talent-Reset
- Cost-Validation
- Refund aller Talent-Points

### Request Payload
Keine zusätzlichen Felder

### Erwartete Response
- `TalentResetResponse` (653)

### Folge-Messages bei Erfolg
- `GoldUpdate` (3703) mit neuem Gold-Betrag

### Beispiel Payload
```csharp
var talentReset = new TalentReset
{
    Type = MessageType.TalentReset
};
```

### Error Codes
| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `INSUFFICIENT_GOLD` | Nicht genug Gold | Gold farmen |
| `NO_TALENTS_LEARNED` | Keine Talents gelernt | - |

### Notizen
- **Hinweis**: Nicht im Prototyp
- **Cost**: 1 Gold (1. Respec), 5 Gold (2.), 10 Gold (3.), max 50 Gold
- **Cooldown**: 24h zwischen Respecs

---

## GenderChange (622)

**Richtung:** 📤 Client → Server  
**Frequenz:** Sehr selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
**Feature** - Client wechselt Specialization (z.B. Warrior: Arms → Protection). Resettet alle Talents.

### Im Scope ✅
- Spec-Change
- Auto-Talent-Reset
- Cost-Validation

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| NewSpecializationId | byte | Neue Spec-ID | Ja |

### Erwartete Response
- `SpecializationChangeResponse` (654)

### Folge-Messages bei Erfolg
- Talents werden automatisch zurückgesetzt

### Beispiel Payload
```csharp
var specChange = new SpecializationChange
{
    Type = MessageType.SpecializationChange,
    NewSpecializationId = 2 // Arms → Protection
};
```

### Error Codes
| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `INVALID_SPECIALIZATION` | Spec für Class nicht verfügbar | - |
| `INSUFFICIENT_GOLD` | Nicht genug Gold | Gold farmen |

### Notizen
- **Hinweis**: Nicht im Prototyp
- **Cost**: 50 Gold
- **Cooldown**: 7 Tage

---

## TitleChange (630)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
**Feature** - Client wählt einen Achievement-basierten Title. Title wird über/unter Character-Name angezeigt.

### Im Scope ✅
- Title-Equip
- Title-Unequip
- Achievement-Validation

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| TitleId | uint | Title-ID (0=kein Title) | Ja |

### Erwartete Response
- `TitleChangeResponse` (655)

### Beispiel Payload
```csharp
var titleChange = new TitleChange
{
    Type = MessageType.TitleChange,
    TitleId = 101 // "Knight"
};
```

### Error Codes
| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `TITLE_NOT_UNLOCKED` | Title nicht freigeschaltet | Achievement erlangen |
| `INVALID_TITLE` | Title existiert nicht | - |

### Notizen
- **Hinweis**: Nicht im Prototyp
- **Titles**: Via Achievements freigeschaltet
- **Display**: Über/Unter Character-Name

---

## AppearanceUpdate (631)

**Richtung:** 📡 Broadcast  
**Frequenz:** Sehr selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Server broadcastet Appearance-Change an alle Spieler in Sichtweite (nach `CharacterCustomize` 610).

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| EntityId | int | Character-ID | Ja |
| NewAppearance | AppearanceData | Neue Appearance | Ja |

### Beispiel Payload
```csharp
var appearanceUpdate = new AppearanceUpdate
{
    Type = MessageType.AppearanceUpdate,
    EntityId = 98765,
    NewAppearance = new AppearanceData
    {
        HairStyle = 5,
        HairColor = 0xFF0000, // Red
        SkinColor = 0xFFDBBE,
        FaceType = 1
    }
};
```

### Notizen
- **Broadcast-Range**: 50m Radius
- **Client-Update**: Client updated Character-Model

---

## RestedXPUpdate (640)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten (Login, Rested-XP-Change)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
**Feature** - Update des Rested-XP Bonus. Spieler erhält +50% XP für Offline-Zeit (max 1.5 Level).

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| RestedXP | long | Verfügbare Rested-XP | Ja |
| MaxRestedXP | long | Max Rested-XP (1.5 Level) | Ja |
| BonusActive | bool | Rested-XP Bonus aktiv? | Ja |

### Beispiel Payload
```csharp
var restedXPUpdate = new RestedXPUpdate
{
    Type = MessageType.RestedXPUpdate,
    RestedXP = 25000,
    MaxRestedXP = 75000, // 1.5 Level
    BonusActive = true
};
```

### Notizen
- **Hinweis**: Nicht im Prototyp
- **Accrual**: 5% eines Levels pro 8h Offline (in Inn/City)
- **Max**: 1.5 Level
- **Bonus**: +50% XP

---

## AttributeIncreaseResponse (650)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung
Antwort auf AttributeIncrease Request. Bestätigt erfolgreiche Stat-Point-Investition oder gibt Fehler zurück.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Success | bool | Investition erfolgreich? | Ja |
| ErrorCode | string | Fehlercode falls Success=false | Nein |
| ErrorMessage | string | Menschenlesbare Fehlermeldung | Nein |
| Attribute | string | Erhöhtes Attribut | Bei Erfolg |
| PointsSpent | int | Investierte Points | Bei Erfolg |
| NewValue | int | Neuer Attribut-Wert | Bei Erfolg |
| RemainingPoints | int | Verbleibende Stat-Points | Bei Erfolg |

### Erwartete Response
- Keine (ist selbst Response)

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `AttributeIncrease` | 605 | Request zu dieser Response |
| `StatsUpdate` | 601 | Folgt mit neuen Derived Stats |

### Beispiel Payload
```csharp
// Erfolg
var successResponse = new AttributeIncreaseResponse
{
    Type = MessageType.AttributeIncreaseResponse,
    Success = true,
    Attribute = "strength",
    PointsSpent = 5,
    NewValue = 30,
    RemainingPoints = 15
};

// Fehler
var errorResponse = new AttributeIncreaseResponse
{
    Type = MessageType.AttributeIncreaseResponse,
    Success = false,
    ErrorCode = "INSUFFICIENT_POINTS",
    ErrorMessage = "Not enough stat points available"
};
```

### Error Codes
| Code | Bedeutung |
|------|-----------|
| `INSUFFICIENT_POINTS` | Nicht genug Stat-Points |
| `INVALID_ATTRIBUTE` | Ungültiger Attribute-Name |
| `MAX_ATTRIBUTE_REACHED` | Attribute-Cap erreicht (999) |
| `INVALID_POINTS` | Points < 1 oder > 10 |

---

## CharacterCustomizeResponse (651)

**Richtung:** 📥 Server → Client  
**Frequenz:** Sehr selten  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung
Antwort auf CharacterCustomize Request. Bestätigt erfolgreiche Appearance-Änderung oder gibt Fehler zurück.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Success | bool | Customization erfolgreich? | Ja |
| ErrorCode | string | Fehlercode falls Success=false | Nein |
| ErrorMessage | string | Menschenlesbare Fehlermeldung | Nein |
| GoldCost | int | Kosten in Gold | Bei Erfolg |

### Erwartete Response
- Keine (ist selbst Response)

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `CharacterCustomize` | 610 | Request zu dieser Response |
| `GoldUpdate` | 3703 | Folgt mit neuem Gold-Betrag |
| `AppearanceUpdate` | 631 | Broadcast an nahe Spieler |

### Beispiel Payload
```csharp
// Erfolg
var successResponse = new CharacterCustomizeResponse
{
    Type = MessageType.CharacterCustomizeResponse,
    Success = true,
    GoldCost = 10
};

// Fehler
var errorResponse = new CharacterCustomizeResponse
{
    Type = MessageType.CharacterCustomizeResponse,
    Success = false,
    ErrorCode = "INSUFFICIENT_GOLD",
    ErrorMessage = "Not enough gold. Required: 10 gold"
};
```

### Error Codes
| Code | Bedeutung |
|------|-----------|
| `INSUFFICIENT_GOLD` | Nicht genug Gold |
| `INVALID_HAIRSTYLE` | Hairstyle für Race nicht verfügbar |
| `INVALID_SKINCOLOR` | Skin-Color für Race nicht erlaubt |
| `NOT_AT_BARBER` | Nicht bei Barber-NPC |

---

## TalentLearnResponse (652)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung
**Feature** - Antwort auf TalentLearn Request. Bestätigt erfolgreiche Talent-Aktivierung oder gibt Fehler zurück.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Success | bool | Talent gelernt? | Ja |
| ErrorCode | string | Fehlercode falls Success=false | Nein |
| ErrorMessage | string | Menschenlesbare Fehlermeldung | Nein |
| TalentId | uint | Gelernte Talent-ID | Bei Erfolg |
| Rank | byte | Neue Rank | Bei Erfolg |
| RemainingPoints | int | Verbleibende Talent-Points | Bei Erfolg |

### Error Codes
| Code | Bedeutung |
|------|-----------|
| `INSUFFICIENT_POINTS` | Nicht genug Talent-Points |
| `TALENT_LOCKED` | Voraussetzungen nicht erfüllt |
| `MAX_RANK_REACHED` | Talent bereits max Rank |
| `INVALID_SPECIALIZATION` | Talent für andere Spec |

---

## TalentResetResponse (653)

**Richtung:** 📥 Server → Client  
**Frequenz:** Sehr selten  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung
**Feature** - Antwort auf TalentReset Request. Bestätigt erfolgreichen Reset oder gibt Fehler zurück.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Success | bool | Reset erfolgreich? | Ja |
| ErrorCode | string | Fehlercode falls Success=false | Nein |
| ErrorMessage | string | Menschenlesbare Fehlermeldung | Nein |
| GoldCost | int | Kosten in Gold | Bei Erfolg |
| RefundedPoints | int | Zurückgegebene Talent-Points | Bei Erfolg |

### Error Codes
| Code | Bedeutung |
|------|-----------|
| `INSUFFICIENT_GOLD` | Nicht genug Gold |
| `NO_TALENTS_LEARNED` | Keine Talents gelernt |

---

## SpecializationChangeResponse (654)

**Richtung:** 📥 Server → Client  
**Frequenz:** Sehr selten  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung
**Feature** - Antwort auf SpecializationChange Request. Bestätigt erfolgreichen Spec-Wechsel oder gibt Fehler zurück.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Success | bool | Spec-Wechsel erfolgreich? | Ja |
| ErrorCode | string | Fehlercode falls Success=false | Nein |
| ErrorMessage | string | Menschenlesbare Fehlermeldung | Nein |
| NewSpecializationId | byte | Neue Spec-ID | Bei Erfolg |
| GoldCost | int | Kosten in Gold | Bei Erfolg |

### Error Codes
| Code | Bedeutung |
|------|-----------|
| `INVALID_SPECIALIZATION` | Spec für Class nicht verfügbar |
| `INSUFFICIENT_GOLD` | Nicht genug Gold |

---

## TitleChangeResponse (655)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung
**Feature** - Antwort auf TitleChange Request. Bestätigt erfolgreichen Title-Wechsel oder gibt Fehler zurück.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Success | bool | Title-Wechsel erfolgreich? | Ja |
| ErrorCode | string | Fehlercode falls Success=false | Nein |
| ErrorMessage | string | Menschenlesbare Fehlermeldung | Nein |
| TitleId | uint | Neue Title-ID (0=kein Title) | Bei Erfolg |

### Error Codes
| Code | Bedeutung |
|------|-----------|
| `TITLE_NOT_UNLOCKED` | Title nicht freigeschaltet |
| `INVALID_TITLE` | Title existiert nicht |

---

## 🔗 Verwandte Kategorien

- **Combat (03)**: Combat-Stats → `DamageEvent` (302), `CriticalHitEvent` (309)
- **Aura (15)**: Buffs/Debuffs auf Stats → `BuffApplied` (1500)
- **Equipment (39)**: Equipment-Stats → `EquipItem` (3901)
- **Quest (10)**: XP-Rewards → `QuestComplete` (1002)
- **Achievement (19)**: Titles → `AchievementUnlocked` (1900)

---

**Letzte Aktualisierung**: 2025-12-25  
**Version**: 2.0.0  
**Status**: ✅ Vollständig dokumentiert (13/13 Messages)

[← Zurück zur Übersicht](README.md)
