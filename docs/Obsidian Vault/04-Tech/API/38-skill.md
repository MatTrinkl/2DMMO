# 🧠 Skill / Talent / Ability Messages (3800-3899)

**Kategorie:** 38  
**Range:** 3800-3899  
**Phase:** Prototyp  
**Status:** 🟢 In Entwicklung

[← Zurück zur Übersicht](Message-Reference.md)

---

## 📋 Inhaltsverzeichnis

- [Überblick](#-überblick)
- [Datenmodell](#-datenmodell)
  - [SkillDefinition](#skilldefinition)
  - [SkillState](#skillstate)
  - [TalentDefinition](#talentdefinition)
  - [TalentState](#talentstate)
  - [AbilityBarSlot](#abilitybarslot)
- [Learn/Upgrade/Respec Regeln](#-learnupgraderespec-regeln)
- [Sync, Deltas & Revisioning](#-sync-deltas--revisioning)
- [DTOs / Interfaces](#-dtos--interfaces)
- [Enums / ErrorCodes / Flags](#-enums--errorcodes--flags)
- [Regeln & Sicherheit](#-regeln--sicherheit)
- [Aktive Messages 3800-3899](#-aktive-messages-3800-3899)
  - [SkillListRequest (3800)](#skilllistrequest-3800)
  - [SkillListResponse (3801)](#skilllistresponse-3801)
  - [SkillLearn (3802)](#skilllearn-3802)
  - [SkillLearnResult (3803)](#skilllearnresult-3803)
  - [SkillUnlearn (3804)](#skillunlearn-3804)
  - [SkillUnlearnResult (3807)](#skillunlearnresult-3807)
  - [SkillUpgrade (3805)](#skillupgrade-3805)
  - [SkillUpgradeResult (3806)](#skillupgraderesult-3806)
  - [TalentListRequest (3810)](#talentlistrequest-3810)
  - [TalentListResponse (3811)](#talentlistresponse-3811)
  - [TalentLearn (3812)](#talentlearn-3812)
  - [TalentLearnResult (3813)](#talentlearnresult-3813)
  - [TalentReset (3814)](#talentreset-3814)
  - [TalentResetResult (3815)](#talentresetresult-3815)
  - [TalentPreview (3816)](#talentpreview-3816)
  - [TalentPreviewResult (3817)](#talentpreviewresult-3817)
  - [SpecializationList (3820)](#specializationlist-3820)
  - [SpecializationChange (3821)](#specializationchange-3821)
  - [SpecializationChangeResult (3822)](#specializationchangeresult-3822)
  - [AbilityBarUpdate (3830)](#abilitybarupdate-3830)
  - [AbilityBarSlotSet (3831)](#abilitybarslotset-3831)
  - [AbilityBarSlotSetResult (3834)](#abilitybarslotsetresult-3834)
  - [AbilityBarSlotClear (3832)](#abilitybarslotclear-3832)
  - [AbilityBarSlotClearResult (3835)](#abilitybarslotclearresult-3835)
  - [AbilityBarSwap (3833)](#abilitybarswap-3833)
  - [AbilityBarSwapResult (3836)](#abilitybarswapresult-3836)
  - [PassiveListRequest (3840)](#passivelistrequest-3840)
  - [PassiveListResponse (3841)](#passivelistresponse-3841)
  - [PassiveUpdate (3842)](#passiveupdate-3842)
  - [GlyphApply (3850)](#glyphapply-3850)
  - [GlyphApplyResult (3854)](#glyphapplyresult-3854)
  - [GlyphRemove (3851)](#glyphremove-3851)
  - [GlyphRemoveResult (3855)](#glyphremoveresult-3855)
  - [GlyphListRequest (3852)](#glyphlistrequest-3852)
  - [GlyphListResponse (3853)](#glyphlistresponse-3853)
- [Obsolete Messages](#-obsolete-messages)
- [Edge Cases & Fehlerfälle](#-edge-cases--fehlerfälle)
- [Anhang](#-anhang)

---

## 📋 Überblick

Das Skill-System verwaltet alle Fähigkeiten, Talente, Spezialisierungen und die Ability-Bar des Characters. **Der Server ist authoritative** – der Client darf keine Skill-Level, Talentpunkte oder Ability-Bar-Bindings direkt setzen, sondern sendet nur Requests. 

### Scope

| Feature | Beschreibung |
|---------|--------------|
| **Skills** | Aktive und passive Fähigkeiten, die erlernt und aufgewertet werden können |
| **Talents** | Talent-Bäume mit Punkteverteilung und Reset-Mechanik |
| **Specializations** | Klassenspezifische Spezialisierungen (z.B. Tank, Healer, DPS) |
| **Ability Bar** | Hotbar-Bindings für schnellen Zugriff auf Skills |
| **Passives** | Automatisch aktive Fähigkeiten ohne Cooldown |
| **Glyphs** | Skill-Modifikatoren, die bestehende Skills verändern |

### Architektur

```
┌─────────────────────────────────────────────────────────────────┐
│  Client                                                         │
│  ├── SkillUI (Anzeige, keine Logik)                            │
│  ├── AbilityBar (Bindings anzeigen, Requests senden)           │
│  └── TalentTree (Preview, keine direkte Manipulation)          │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼ Requests (Learn, Upgrade, Respec)
┌─────────────────────────────────────────────────────────────────┐
│  Server (Authoritative)                                         │
│  ├── SkillService (Validation, Costs, Prerequisites)           │
│  ├── TalentService (Points, Trees, Reset)                      │
│  ├── SpecializationService (Class-specific logic)              │
│  └── AbilityBarService (Slot management, Sync)                 │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼ Responses (Results, Sync)
┌─────────────────────────────────────────────────────────────────┐
│  Client (Update UI)                                             │
│  ├── Skill gelernt → UI aktualisieren                          │
│  ├── Talent vergeben → Baum aktualisieren                      │
│  └── Bar geändert → Hotbar neu rendern                         │
└─────────────────────────────────────────────────────────────────┘
```

---

## 🧠 Datenmodell

### SkillDefinition

Statische Skill-Definition aus dem GameData-System. 

| Feld | Typ | Beschreibung |
|------|-----|--------------|
| SkillId | ushort | Eindeutige Skill-ID |
| InternalName | string | Interner Name (z.B. "fireball_rank1") |
| DisplayName | string | Anzeigename (lokalisiert) |
| Description | string | Beschreibungstext |
| IconId | string | Asset-ID für Icon |
| SkillType | SkillType | Active, Passive, Toggle |
| MaxRank | byte | Maximale Ausbaustufe (1-10) |
| RequiredLevel | int | Mindest-Character-Level |
| RequiredSkills | List<SkillPrerequisite> | Voraussetzungs-Skills |
| BaseCooldown | float | Basis-Cooldown in Sekunden |
| ResourceCost | int | Ressourcenkosten (Mana, Energy, etc.) |
| ResourceType | ResourceType | Art der Ressource |
| CastTime | float | Zauberzeit in Sekunden (0 = instant) |
| Range | float | Reichweite in Units |
| ClassRestriction | ClassType?  | Klassenbeschränkung (null = alle) |

### SkillState

Character-spezifischer Skill-Zustand (Server-authoritative).

| Feld | Typ | Beschreibung |
|------|-----|--------------|
| SkillId | ushort | Referenz auf SkillDefinition |
| CurrentRank | byte | Aktuelle Stufe (1 bis MaxRank) |
| PointsSpent | int | Investierte Skillpunkte |
| UnlockedAt | long | Unix Timestamp des Erlernens |
| Source | SkillSource | Wie erlernt (Trainer, Quest, Book, Class) |
| IsActive | bool | Skill aktiviert (für Toggles) |

### TalentDefinition

Statische Talent-Definition. 

| Feld | Typ | Beschreibung |
|------|-----|--------------|
| TalentId | ushort | Eindeutige Talent-ID |
| TreeId | byte | Zugehöriger Talent-Baum |
| TierIndex | byte | Position im Baum (0-6) |
| ColumnIndex | byte | Spalte im Baum (0-3) |
| DisplayName | string | Anzeigename |
| Description | string | Beschreibung pro Rang |
| IconId | string | Asset-ID |
| MaxRanks | byte | Maximale Ränge (1-5) |
| RequiredPoints | int | Benötigte Punkte im Baum |
| RequiredTalents | List<TalentPrerequisite> | Voraussetzungs-Talents |
| SpecializationId | byte?  | Spezialisierungs-Beschränkung |

### TalentState

Character-spezifischer Talent-Zustand. 

| Feld | Typ | Beschreibung |
|------|-----|--------------|
| TalentId | ushort | Referenz auf TalentDefinition |
| CurrentRanks | byte | Aktuelle Ränge (0 bis MaxRanks) |
| TreeId | byte | Baum-ID |

### AbilityBarSlot

Einzelner Slot in der Ability-Bar. 

| Feld | Typ | Beschreibung |
|------|-----|--------------|
| SlotIndex | byte | Position (0-11 für Hauptbar, 12-23 für zweite Bar, etc.) |
| ContentType | SlotContentType | Skill, Item, Macro, Empty |
| ContentId | uint | SkillId, ItemId, oder MacroId |
| BarIndex | byte | Bar-Nummer (0-3) |

---

## 📚 Learn/Upgrade/Respec Regeln

### Skill Learning

```
┌─────────────────────────────────────────────────────────────────┐
│  Skill Learn Validation (Server)                                │
├─────────────────────────────────────────────────────────────────┤
│  1. Character Level >= RequiredLevel                            │
│  2. Alle RequiredSkills erfüllt (mit MinRank)                  │
│  3. ClassRestriction == Character.Class (wenn gesetzt)          │
│  4. Genug Currency (Gold/SkillPoints)                          │
│  5. Skill nicht bereits auf MaxRank                             │
│  6. Trainer in Range (wenn Source = Trainer)                   │
└─────────────────────────────────────────────────────────────────┘
```

### Skill Costs

| Rank | Skillpoint Cost | Gold Cost |
|------|-----------------|-----------|
| 1 | 1 | 10s |
| 2 | 1 | 50s |
| 3 | 2 | 1g |
| 4 | 2 | 2g |
| 5 | 3 | 5g |
| 6+ | 3 | 10g * (Rank-5) |

### Talent Points

| Level | Talent Points Gained | Total Points |
|-------|---------------------|--------------|
| 10 | 1 | 1 |
| 11-59 | 1 per Level | 50 |
| 60 | 1 | 51 |

### Talent Reset (Respec)

| Reset Count | Cost | Cooldown |
|-------------|------|----------|
| 1 | 1g | 0 |
| 2 | 5g | 0 |
| 3 | 10g | 0 |
| 4 | 25g | 24h |
| 5+ | 50g | 24h |

**Refund-Regel:** Bei Talent-Reset werden 100% der Punkte zurückerstattet.  Gold wird NICHT zurückerstattet. 

---

## 🔄 Sync, Deltas & Revisioning

### Full Sync

Full Sync wird gesendet bei:
- Login / Character Select
- Öffnen des Skill-UI
- Reconnect

```csharp
// Server sendet SkillListResponse mit allen Skills
var response = new SkillListResponse
{
    Skills = character.LearnedSkills. ToList(),
    AvailableSkillPoints = character.SkillPoints,
    Revision = character. SkillRevision
};
```

### Delta Updates

Nach Learn/Upgrade/Unlearn sendet Server nur den geänderten Skill: 

```csharp
// In SkillLearnResult
var result = new SkillLearnResult
{
    Success = true,
    LearnedSkill = newSkillState,
    RemainingSkillPoints = character.SkillPoints,
    NewRevision = ++character.SkillRevision
};
```

### Revision Tracking

| Feld | Typ | Beschreibung |
|------|-----|--------------|
| SkillRevision | uint | Monoton steigend bei jeder Skill-Änderung |
| TalentRevision | uint | Monoton steigend bei jeder Talent-Änderung |
| AbilityBarRevision | uint | Monoton steigend bei Bar-Änderungen |

**Client-Desync-Detection:**
```csharp
if (response. Revision != localRevision + 1)
{
    // Desync detected - Request full sync
    SendMessage(new SkillListRequest { ForceFullSync = true });
}
```

---

## 🧱 DTOs / Interfaces

### SkillStateDto

```csharp
[MessagePackObject]
public class SkillStateDto
{
    [Key(0)] public ushort SkillId { get; set; }
    [Key(1)] public byte CurrentRank { get; set; }
    [Key(2)] public int PointsSpent { get; set; }
    [Key(3)] public long UnlockedAt { get; set; }
    [Key(4)] public SkillSource Source { get; set; }
    [Key(5)] public bool IsActive { get; set; }
}
```

### TalentStateDto

```csharp
[MessagePackObject]
public class TalentStateDto
{
    [Key(0)] public ushort TalentId { get; set; }
    [Key(1)] public byte CurrentRanks { get; set; }
    [Key(2)] public byte TreeId { get; set; }
}
```

### AbilityBarDto

```csharp
[MessagePackObject]
public class AbilityBarDto
{
    [Key(0)] public byte BarIndex { get; set; }
    [Key(1)] public List<AbilityBarSlotDto> Slots { get; set; } = new();
}

[MessagePackObject]
public class AbilityBarSlotDto
{
    [Key(0)] public byte SlotIndex { get; set; }
    [Key(1)] public SlotContentType ContentType { get; set; }
    [Key(2)] public uint ContentId { get; set; }
}
```

### SkillPrerequisiteDto

```csharp
[MessagePackObject]
public class SkillPrerequisiteDto
{
    [Key(0)] public ushort RequiredSkillId { get; set; }
    [Key(1)] public byte MinRank { get; set; }
}
```

---

## 🧩 Enums / ErrorCodes / Flags

### SkillType

```csharp
public enum SkillType : byte
{
    Active = 0,      // Muss aktiviert werden (Cooldown)
    Passive = 1,     // Immer aktiv
    Toggle = 2,      // An/Aus-Schalter
    Channeled = 3,   // Kanalisiert
    Charged = 4      // Aufladbar
}
```

### SkillSource

```csharp
public enum SkillSource : byte
{
    Class = 0,       // Automatisch durch Klasse
    Trainer = 1,     // Bei Trainer gelernt
    Quest = 2,       // Quest-Belohnung
    Book = 3,        // Skillbuch Item
    Achievement = 4, // Achievement-Belohnung
    Specialization = 5 // Durch Spezialisierung
}
```

### SlotContentType

```csharp
public enum SlotContentType : byte
{
    Empty = 0,
    Skill = 1,
    Item = 2,
    Macro = 3,
    Mount = 4,
    Pet = 5,
    Companion = 6
}
```

### SkillErrorCode

```csharp
public enum SkillErrorCode : byte
{
    None = 0,
    AlreadyKnown = 1,
    LevelTooLow = 2,
    MissingPrerequisite = 3,
    NotEnoughSkillPoints = 4,
    NotEnoughGold = 5,
    MaxRankReached = 6,
    ClassRestricted = 7,
    TrainerNotInRange = 8,
    SkillNotKnown = 9,
    CannotUnlearn = 10,
    InvalidSkillId = 11,
    OnCooldown = 12,
    Disabled = 13
}
```

### TalentErrorCode

```csharp
public enum TalentErrorCode : byte
{
    None = 0,
    NotEnoughTalentPoints = 1,
    MissingPrerequisite = 2,
    MaxRanksReached = 3,
    TierLocked = 4,
    WrongSpecialization = 5,
    InvalidTalentId = 6,
    ResetOnCooldown = 7,
    NotEnoughGold = 8,
    TreeLocked = 9
}
```

---

## ⚙️ Regeln & Sicherheit

### Server Authoritative Rules

| Regel | Beschreibung |
|-------|--------------|
| **No Client Trust** | Client sendet nur Requests, Server validiert alles |
| **Atomic Operations** | Skill-Changes sind atomar (all-or-nothing) |
| **Revision Monotonic** | Revision steigt immer, nie Rollback |
| **Idempotency** | Gleicher Request mit gleicher Revision = gleiche Response |

### Anti-Cheat

| Prüfung | Beschreibung |
|---------|--------------|
| **Trainer Distance** | Max. 10 Units Distanz zum Trainer |
| **Cooldown Validation** | Server tracked alle Cooldowns |
| **Point Validation** | Server berechnet verfügbare Punkte |
| **Prerequisite Check** | Server prüft alle Voraussetzungen |

### Rate Limits

| Operation | Limit | Cooldown |
|-----------|-------|----------|
| SkillLearn | 10/min | - |
| SkillUpgrade | 20/min | - |
| TalentLearn | 30/min | - |
| TalentReset | 1/min | 24h nach 4.  Reset |
| AbilityBarChange | 60/min | - |

---

## 📩 Aktive Messages 3800-3899

---

## SkillListRequest (3800)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten (UI öffnen, Login)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Client fordert die vollständige Liste aller erlernten Skills an.  Wird beim Öffnen des Skill-UI oder nach Reconnect gesendet.

### Im Scope ✅
- Vollständige Skill-Liste anfordern
- Force Full Sync nach Desync

### Nicht im Scope ❌
- Einzelne Skill-Details → Skill-Definition ist Client-seitig gecached

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.SkillListRequest` | Ja |
| ForceFullSync | bool | True = ignoriere Client-Cache | Nein |
| ClientRevision | uint | Aktuelle Client-Revision | Nein |

### Erwartete Response
- `SkillListResponse` (3801)

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.SkillListRequest)]
public class SkillListRequest : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.SkillListRequest;
    [Key(1)] public bool ForceFullSync { get; set; }
    [Key(2)] public uint ClientRevision { get; set; }
}
```

### Server-Verhalten

1.  Prüfe `ClientRevision` gegen Server-Revision
2. Wenn gleich und `ForceFullSync=false`: Sende leere Response (no changes)
3. Sonst: Sende vollständige Skill-Liste

### Client-Verhalten

1. Beim Öffnen des Skill-UI
2. Nach Reconnect mit `ForceFullSync=true`
3. Bei Desync-Detection

### Flow-Diagramm

```
Client                         Server
  │                              │
  │  SkillListRequest (3800)     │
  │  ClientRevision:  5           │
  │─────────────────────────────►│
  │                              │  Check Revision
  │                              │  Server: 7 > Client: 5
  │                              │
  │  SkillListResponse (3801)    │
  │  Skills: [... ], Revision: 7  │
  │◄─────────────────────────────│
  │                              │
  │  [Update UI]                 │
```

### Verwandte Messages

| Message | ID | Beziehung |
|---------|-----|-----------|
| `SkillListResponse` | 3801 | Response |
| `SkillLearn` | 3802 | Einzelnen Skill lernen |

---

## SkillListResponse (3801)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Server sendet die vollständige Liste aller erlernten Skills des Characters.

### Im Scope ✅
- Alle erlernten Skills mit aktuellem Rank
- Verfügbare Skillpunkte
- Aktuelle Revision

### Nicht im Scope ❌
- Skill-Definitionen → Client-seitig gecached
- Talent-Daten → Separate Message

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.SkillListResponse` | Ja |
| Skills | List\<SkillStateDto\> | Alle erlernten Skills | Ja |
| AvailableSkillPoints | int | Verfügbare Punkte | Ja |
| Revision | uint | Server-Revision | Ja |
| TotalSkillPoints | int | Gesamt erhaltene Punkte | Ja |

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType. SkillListResponse)]
public class SkillListResponse : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.SkillListResponse;
    [Key(1)] public List<SkillStateDto> Skills { get; set; } = new();
    [Key(2)] public int AvailableSkillPoints { get; set; }
    [Key(3)] public uint Revision { get; set; }
    [Key(4)] public int TotalSkillPoints { get; set; }
}
```

### Beispiel Payload

```csharp
var response = new SkillListResponse
{
    Skills = new List<SkillStateDto>
    {
        new() { SkillId = 101, CurrentRank = 3, PointsSpent = 4, Source = SkillSource. Trainer },
        new() { SkillId = 102, CurrentRank = 1, PointsSpent = 1, Source = SkillSource. Class },
        new() { SkillId = 105, CurrentRank = 2, PointsSpent = 2, Source = SkillSource. Quest }
    },
    AvailableSkillPoints = 12,
    TotalSkillPoints = 19,
    Revision = 7
};
```

---

## SkillLearn (3802)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Client möchte einen neuen Skill erlernen. Server validiert alle Voraussetzungen und zieht Kosten ab.

### Im Scope ✅
- Neuen Skill erlernen (Rank 1)
- Trainer-basiertes Lernen
- Quest/Book-basiertes Lernen

### Nicht im Scope ❌
- Skill upgraden → `SkillUpgrade` (3805)
- Skill verlernen → `SkillUnlearn` (3804)

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.SkillLearn` | Ja |
| SkillId | ushort | Zu erlernender Skill | Ja |
| Source | SkillSource | Wie wird gelernt | Ja |
| TrainerEntityId | Guid?  | Trainer-Entity (wenn Source=Trainer) | Nein |

### Erwartete Response
- `SkillLearnResult` (3803)

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType. SkillLearn)]
public class SkillLearn : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.SkillLearn;
    [Key(1)] public ushort SkillId { get; set; }
    [Key(2)] public SkillSource Source { get; set; }
    [Key(3)] public Guid?  TrainerEntityId { get; set; }
}
```

### Server-Verhalten

```csharp
public SkillLearnResult HandleSkillLearn(SkillLearn request, Character character)
{
    var definition = _skillDefinitions.Get(request.SkillId);
    
    // 1. Validation
    if (definition == null)
        return Fail(SkillErrorCode. InvalidSkillId);
    
    if (character.HasSkill(request.SkillId))
        return Fail(SkillErrorCode.AlreadyKnown);
    
    if (character.Level < definition.RequiredLevel)
        return Fail(SkillErrorCode.LevelTooLow);
    
    if (! CheckPrerequisites(character, definition))
        return Fail(SkillErrorCode.MissingPrerequisite);
    
    if (definition.ClassRestriction. HasValue && 
        definition.ClassRestriction != character.Class)
        return Fail(SkillErrorCode.ClassRestricted);
    
    // 2. Cost Check
    var cost = GetLearnCost(definition, rank: 1);
    if (character. SkillPoints < cost. SkillPoints)
        return Fail(SkillErrorCode.NotEnoughSkillPoints);
    if (character.Gold < cost.Gold)
        return Fail(SkillErrorCode.NotEnoughGold);
    
    // 3. Trainer Check (if applicable)
    if (request.Source == SkillSource.Trainer)
    {
        if (! ValidateTrainerDistance(character, request.TrainerEntityId))
            return Fail(SkillErrorCode.TrainerNotInRange);
    }
    
    // 4. Apply
    character.SkillPoints -= cost.SkillPoints;
    character.Gold -= cost.Gold;
    
    var newSkill = new SkillState
    {
        SkillId = request.SkillId,
        CurrentRank = 1,
        PointsSpent = cost.SkillPoints,
        UnlockedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
        Source = request.Source
    };
    character.Skills.Add(newSkill);
    character.SkillRevision++;
    
    return new SkillLearnResult
    {
        Success = true,
        LearnedSkill = newSkill. ToDto(),
        RemainingSkillPoints = character.SkillPoints,
        RemainingGold = character.Gold,
        NewRevision = character.SkillRevision
    };
}
```

### Flow-Diagramm

```
Client                         Server                    Trainer NPC
  │                              │                           │
  │  [Click "Learn Fireball"]    │                           │
  │                              │                           │
  │  SkillLearn (3802)           │                           │
  │  SkillId: 101                │                           │
  │  Source: Trainer             │                           │
  │  TrainerEntityId: xyz        │                           │
  │─────────────────────────────►│                           │
  │                              │  Validate Distance        │
  │                              │──────────────────────────►│
  │                              │  Distance: 3 units ✓      │
  │                              │◄──────────────────────────│
  │                              │                           │
  │                              │  Check Prerequisites ✓    │
  │                              │  Deduct Costs             │
  │                              │  Add Skill                │
  │                              │                           │
  │  SkillLearnResult (3803)     │                           │
  │  Success: true               │                           │
  │  LearnedSkill: {...}         │                           │
  │◄─────────────────────────────│                           │
  │                              │                           │
  │  [Play Learn Animation]      │                           │
  │  [Update Skill UI]           │                           │
```

### Error Codes

| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `AlreadyKnown` | Skill bereits erlernt | UI sollte Button disabled haben |
| `LevelTooLow` | Level zu niedrig | Zeige benötigtes Level |
| `MissingPrerequisite` | Voraussetzung fehlt | Zeige fehlende Skills |
| `NotEnoughSkillPoints` | Zu wenig Skillpunkte | Zeige Kosten |
| `NotEnoughGold` | Zu wenig Gold | Zeige Kosten |
| `TrainerNotInRange` | Zu weit vom Trainer | "Näher an Trainer herantreten" |

---

## SkillLearnResult (3803)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Antwort auf `SkillLearn`. Enthält Erfolg/Fehler und den neuen Skill-State.

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.SkillLearnResult` | Ja |
| Success | bool | Erfolgreich?  | Ja |
| ErrorCode | SkillErrorCode | Fehlercode | Bei Fehler |
| ErrorMessage | string?  | Menschenlesbare Nachricht | Nein |
| LearnedSkill | SkillStateDto?  | Der neue Skill | Bei Erfolg |
| RemainingSkillPoints | int | Verbleibende Punkte | Bei Erfolg |
| RemainingGold | long | Verbleibendes Gold | Bei Erfolg |
| NewRevision | uint | Neue Revision | Bei Erfolg |

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.SkillLearnResult)]
public class SkillLearnResult : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType. SkillLearnResult;
    [Key(1)] public bool Success { get; set; }
    [Key(2)] public SkillErrorCode ErrorCode { get; set; }
    [Key(3)] public string? ErrorMessage { get; set; }
    [Key(4)] public SkillStateDto? LearnedSkill { get; set; }
    [Key(5)] public int RemainingSkillPoints { get; set; }
    [Key(6)] public long RemainingGold { get; set; }
    [Key(7)] public uint NewRevision { get; set; }
}
```

---

## SkillUnlearn (3804)

**Richtung:** 📤 Client → Server  
**Frequenz:** Sehr selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Client möchte einen Skill verlernen.  Nur bei bestimmten Trainern oder mit speziellen Items möglich.

### Im Scope ✅
- Skill komplett verlernen
- Teilweise Skillpunkt-Rückerstattung

### Nicht im Scope ❌
- Downgrade einzelner Ränge → nicht unterstützt

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.SkillUnlearn` | Ja |
| SkillId | ushort | Zu verlernender Skill | Ja |
| TrainerEntityId | Guid? | Unlearn-Trainer | Nein |

### Erwartete Response
- `SkillUnlearnResult` (3807)

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.SkillUnlearn)]
public class SkillUnlearn : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.SkillUnlearn;
    [Key(1)] public ushort SkillId { get; set; }
    [Key(2)] public Guid? TrainerEntityId { get; set; }
}
```

### Server-Verhalten

1. Prüfe ob Skill bekannt ist
2. Prüfe ob Skill verlernbar ist (nicht Class-inherent)
3. Prüfe ob andere Skills diesen als Prerequisite haben
4. Berechne Refund (50% der Skillpunkte)
5. Entferne Skill, erstate Punkte

### Error Codes

| Code | Bedeutung |
|------|-----------|
| `SkillNotKnown` | Skill nicht erlernt |
| `CannotUnlearn` | Skill ist Klassen-inherent |
| `MissingPrerequisite` | Andere Skills benötigen diesen |

---

## SkillUnlearnResult (3807)

**Richtung:** 📥 Server → Client  
**Frequenz:** Sehr selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Antwort auf `SkillUnlearn`.

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.SkillUnlearnResult` | Ja |
| Success | bool | Erfolgreich? | Ja |
| ErrorCode | SkillErrorCode | Fehlercode | Bei Fehler |
| UnlearnedSkillId | ushort | Verlernter Skill | Bei Erfolg |
| RefundedSkillPoints | int | Erstattete Punkte | Bei Erfolg |
| RemainingSkillPoints | int | Neue Punktzahl | Bei Erfolg |
| NewRevision | uint | Neue Revision | Bei Erfolg |

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.SkillUnlearnResult)]
public class SkillUnlearnResult : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType. SkillUnlearnResult;
    [Key(1)] public bool Success { get; set; }
    [Key(2)] public SkillErrorCode ErrorCode { get; set; }
    [Key(3)] public ushort UnlearnedSkillId { get; set; }
    [Key(4)] public int RefundedSkillPoints { get; set; }
    [Key(5)] public int RemainingSkillPoints { get; set; }
    [Key(6)] public uint NewRevision { get; set; }
}
```

---

## SkillUpgrade (3805)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Client möchte einen bereits erlernten Skill auf den nächsten Rang upgraden.

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.SkillUpgrade` | Ja |
| SkillId | ushort | Zu upgradender Skill | Ja |
| TargetRank | byte | Ziel-Rang (optional, sonst +1) | Nein |
| TrainerEntityId | Guid? | Trainer (wenn benötigt) | Nein |

### Erwartete Response
- `SkillUpgradeResult` (3806)

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType. SkillUpgrade)]
public class SkillUpgrade : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.SkillUpgrade;
    [Key(1)] public ushort SkillId { get; set; }
    [Key(2)] public byte TargetRank { get; set; }
    [Key(3)] public Guid? TrainerEntityId { get; set; }
}
```

### Server-Verhalten

1. Prüfe ob Skill bekannt
2. Prüfe ob nicht bereits MaxRank
3. Prüfe Level-Requirement für nächsten Rank
4. Prüfe Kosten
5. Upgrade durchführen

---

## SkillUpgradeResult (3806)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Antwort auf `SkillUpgrade`.

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.SkillUpgradeResult` | Ja |
| Success | bool | Erfolgreich? | Ja |
| ErrorCode | SkillErrorCode | Fehlercode | Bei Fehler |
| UpgradedSkill | SkillStateDto?  | Aktualisierter Skill | Bei Erfolg |
| OldRank | byte | Vorheriger Rang | Bei Erfolg |
| NewRank | byte | Neuer Rang | Bei Erfolg |
| RemainingSkillPoints | int | Verbleibende Punkte | Bei Erfolg |
| RemainingGold | long | Verbleibendes Gold | Bei Erfolg |
| NewRevision | uint | Neue Revision | Bei Erfolg |

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.SkillUpgradeResult)]
public class SkillUpgradeResult : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.SkillUpgradeResult;
    [Key(1)] public bool Success { get; set; }
    [Key(2)] public SkillErrorCode ErrorCode { get; set; }
    [Key(3)] public SkillStateDto?  UpgradedSkill { get; set; }
    [Key(4)] public byte OldRank { get; set; }
    [Key(5)] public byte NewRank { get; set; }
    [Key(6)] public int RemainingSkillPoints { get; set; }
    [Key(7)] public long RemainingGold { get; set; }
    [Key(8)] public uint NewRevision { get; set; }
}
```

---

## TalentListRequest (3810)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Client fordert die vollständige Talent-Konfiguration an.

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.TalentListRequest` | Ja |
| ForceFullSync | bool | Full Sync erzwingen | Nein |
| ClientRevision | uint | Client-Revision | Nein |

### Erwartete Response
- `TalentListResponse` (3811)

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.TalentListRequest)]
public class TalentListRequest : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.TalentListRequest;
    [Key(1)] public bool ForceFullSync { get; set; }
    [Key(2)] public uint ClientRevision { get; set; }
}
```

---

## TalentListResponse (3811)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Server sendet alle Talent-Daten des Characters.

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.TalentListResponse` | Ja |
| Talents | List\<TalentStateDto\> | Alle vergebenen Talente | Ja |
| AvailableTalentPoints | int | Verfügbare Punkte | Ja |
| TotalTalentPoints | int | Gesamt erhaltene Punkte | Ja |
| PointsPerTree | Dictionary\<byte, int\> | Punkte pro Baum | Ja |
| ActiveSpecialization | byte | Aktive Spezialisierung | Ja |
| Revision | uint | Server-Revision | Ja |

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.TalentListResponse)]
public class TalentListResponse :  IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.TalentListResponse;
    [Key(1)] public List<TalentStateDto> Talents { get; set; } = new();
    [Key(2)] public int AvailableTalentPoints { get; set; }
    [Key(3)] public int TotalTalentPoints { get; set; }
    [Key(4)] public Dictionary<byte, int> PointsPerTree { get; set; } = new();
    [Key(5)] public byte ActiveSpecialization { get; set; }
    [Key(6)] public uint Revision { get; set; }
}
```

---

## TalentLearn (3812)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Client möchte einen Talentpunkt in ein Talent investieren.

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.TalentLearn` | Ja |
| TalentId | ushort | Talent-ID | Ja |
| RanksToAdd | byte | Anzahl Ränge (default: 1) | Nein |

### Erwartete Response
- `TalentLearnResult` (3813)

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.TalentLearn)]
public class TalentLearn : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.TalentLearn;
    [Key(1)] public ushort TalentId { get; set; }
    [Key(2)] public byte RanksToAdd { get; set; } = 1;
}
```

### Server-Verhalten

```csharp
public TalentLearnResult HandleTalentLearn(TalentLearn request, Character character)
{
    var definition = _talentDefinitions.Get(request.TalentId);
    
    if (definition == null)
        return Fail(TalentErrorCode.InvalidTalentId);
    
    var currentState = character. Talents
        .FirstOrDefault(t => t.TalentId == request.TalentId);
    var currentRanks = currentState?.CurrentRanks ?? 0;
    
    // Check max ranks
    if (currentRanks + request.RanksToAdd > definition.MaxRanks)
        return Fail(TalentErrorCode.MaxRanksReached);
    
    // Check available points
    if (character.AvailableTalentPoints < request.RanksToAdd)
        return Fail(TalentErrorCode.NotEnoughTalentPoints);
    
    // Check tree points requirement
    var treePoints = character.GetPointsInTree(definition.TreeId);
    if (treePoints < definition.RequiredPoints)
        return Fail(TalentErrorCode.TierLocked);
    
    // Check prerequisites
    foreach (var prereq in definition.RequiredTalents)
    {
        var prereqState = character.Talents
            .FirstOrDefault(t => t.TalentId == prereq. TalentId);
        if (prereqState == null || prereqState.CurrentRanks < prereq.MinRanks)
            return Fail(TalentErrorCode. MissingPrerequisite);
    }
    
    // Apply
    if (currentState == null)
    {
        character.Talents. Add(new TalentState
        {
            TalentId = request.TalentId,
            CurrentRanks = request.RanksToAdd,
            TreeId = definition.TreeId
        });
    }
    else
    {
        currentState.CurrentRanks += request.RanksToAdd;
    }
    
    character.AvailableTalentPoints -= request.RanksToAdd;
    character.TalentRevision++;
    
    return Success(character);
}
```

### Flow-Diagramm

```
Client                         Server
  │                              │
  │  TalentLearn (3812)          │
  │  TalentId:  201               │
  │  RanksToAdd: 1               │
  │─────────────────────────────►│
  │                              │
  │                              │  Validate: 
  │                              │  - Points available? ✓
  │                              │  - Tree points met? ✓
  │                              │  - Prerequisites?  ✓
  │                              │  - Max ranks?  ✓
  │                              │
  │                              │  Apply changes
  │                              │
  │  TalentLearnResult (3813)    │
  │  Success:  true               │
  │  UpdatedTalent:  {... }        │
  │◄─────────────────────────────│
  │                              │
  │  [Update Talent Tree UI]     │
```

---

## TalentLearnResult (3813)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Antwort auf `TalentLearn`.

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.TalentLearnResult` | Ja |
| Success | bool | Erfolgreich? | Ja |
| ErrorCode | TalentErrorCode | Fehlercode | Bei Fehler |
| UpdatedTalent | TalentStateDto?  | Aktualisiertes Talent | Bei Erfolg |
| RemainingTalentPoints | int | Verbleibende Punkte | Bei Erfolg |
| PointsInTree | int | Punkte im Baum | Bei Erfolg |
| NewRevision | uint | Neue Revision | Bei Erfolg |

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.TalentLearnResult)]
public class TalentLearnResult : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.TalentLearnResult;
    [Key(1)] public bool Success { get; set; }
    [Key(2)] public TalentErrorCode ErrorCode { get; set; }
    [Key(3)] public TalentStateDto?  UpdatedTalent { get; set; }
    [Key(4)] public int RemainingTalentPoints { get; set; }
    [Key(5)] public int PointsInTree { get; set; }
    [Key(6)] public uint NewRevision { get; set; }
}
```

---

## TalentReset (3814)

**Richtung:** 📤 Client → Server  
**Frequenz:** Sehr selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Client möchte alle Talente zurücksetzen (Respec).

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.TalentReset` | Ja |
| TreeId | byte?  | Nur bestimmten Baum reset (null = alle) | Nein |
| TrainerEntityId | Guid? | Respec-Trainer | Nein |

### Erwartete Response
- `TalentResetResult` (3815)

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType. TalentReset)]
public class TalentReset : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.TalentReset;
    [Key(1)] public byte?  TreeId { get; set; }
    [Key(2)] public Guid? TrainerEntityId { get; set; }
}
```

### Server-Verhalten

1. Prüfe Cooldown (24h nach 4.  Reset)
2. Berechne Kosten basierend auf Reset-Count
3. Prüfe Gold
4. Entferne alle Talente (oder nur TreeId)
5. Erstate 100% der Punkte
6. Inkrementiere Reset-Counter

---

## TalentResetResult (3815)

**Richtung:** 📥 Server → Client  
**Frequenz:** Sehr selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Antwort auf `TalentReset`.

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.TalentResetResult` | Ja |
| Success | bool | Erfolgreich? | Ja |
| ErrorCode | TalentErrorCode | Fehlercode | Bei Fehler |
| RefundedPoints | int | Erstattete Punkte | Bei Erfolg |
| AvailableTalentPoints | int | Neue verfügbare Punkte | Bei Erfolg |
| GoldCost | long | Bezahlte Goldkosten | Bei Erfolg |
| RemainingGold | long | Verbleibendes Gold | Bei Erfolg |
| NextResetCooldown | long?  | Unix Timestamp bis nächster Reset | Nein |
| NewRevision | uint | Neue Revision | Bei Erfolg |

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.TalentResetResult)]
public class TalentResetResult : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType. TalentResetResult;
    [Key(1)] public bool Success { get; set; }
    [Key(2)] public TalentErrorCode ErrorCode { get; set; }
    [Key(3)] public int RefundedPoints { get; set; }
    [Key(4)] public int AvailableTalentPoints { get; set; }
    [Key(5)] public long GoldCost { get; set; }
    [Key(6)] public long RemainingGold { get; set; }
    [Key(7)] public long? NextResetCooldown { get; set; }
    [Key(8)] public uint NewRevision { get; set; }
}
```

---

## TalentPreview (3816)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig (beim Hovern)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Client fordert eine Vorschau der Talent-Auswirkungen an.

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.TalentPreview` | Ja |
| TalentId | ushort | Talent-ID | Ja |
| PreviewRank | byte | Vorschau für welchen Rang | Ja |

### Erwartete Response
- `TalentPreviewResult` (3817)

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.TalentPreview)]
public class TalentPreview : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.TalentPreview;
    [Key(1)] public ushort TalentId { get; set; }
    [Key(2)] public byte PreviewRank { get; set; }
}
```

---

## TalentPreviewResult (3817)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Server sendet Talent-Preview-Daten.

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.TalentPreviewResult` | Ja |
| TalentId | ushort | Talent-ID | Ja |
| PreviewRank | byte | Vorschau-Rang | Ja |
| Description | string | Beschreibung für diesen Rang | Ja |
| StatChanges | List\<StatChangeDto\> | Stat-Änderungen | Ja |
| CanLearn | bool | Kann gelernt werden?  | Ja |
| BlockReason | TalentErrorCode?  | Warum nicht lernbar | Nein |

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.TalentPreviewResult)]
public class TalentPreviewResult : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.TalentPreviewResult;
    [Key(1)] public ushort TalentId { get; set; }
    [Key(2)] public byte PreviewRank { get; set; }
    [Key(3)] public string Description { get; set; } = "";
    [Key(4)] public List<StatChangeDto> StatChanges { get; set; } = new();
    [Key(5)] public bool CanLearn { get; set; }
    [Key(6)] public TalentErrorCode?  BlockReason { get; set; }
}
```

---

## SpecializationList (3820)

**Richtung:** 📥 Server → Client (Event/Broadcast)  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Server sendet die Liste verfügbarer Spezialisierungen für die Klasse.

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.SpecializationList` | Ja |
| Specializations | List\<SpecializationDto\> | Verfügbare Specs | Ja |
| ActiveSpecId | byte | Aktive Spezialisierung | Ja |
| CanChange | bool | Kann gewechselt werden? | Ja |
| ChangeCooldown | long?  | Cooldown bis Wechsel | Nein |

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType. SpecializationList)]
public class SpecializationList : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.SpecializationList;
    [Key(1)] public List<SpecializationDto> Specializations { get; set; } = new();
    [Key(2)] public byte ActiveSpecId { get; set; }
    [Key(3)] public bool CanChange { get; set; }
    [Key(4)] public long? ChangeCooldown { get; set; }
}
```

---

## SpecializationChange (3821)

**Richtung:** 📤 Client → Server  
**Frequenz:** Sehr selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Client möchte die Spezialisierung wechseln.

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.SpecializationChange` | Ja |
| TargetSpecId | byte | Ziel-Spezialisierung | Ja |

### Erwartete Response
- `SpecializationChangeResult` (3822)

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.SpecializationChange)]
public class SpecializationChange : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.SpecializationChange;
    [Key(1)] public byte TargetSpecId { get; set; }
}
```

---

## SpecializationChangeResult (3822)

**Richtung:** 📥 Server → Client  
**Frequenz:** Sehr selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Antwort auf `SpecializationChange`.

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.SpecializationChangeResult` | Ja |
| Success | bool | Erfolgreich? | Ja |
| Error

Source: docs/03-messages/38-skill.md
