# 🔨 Crafting Messages (1600-1634)

**Kategorie:** 16  
**Range:** 1600-1634 (AKTIV)  
**Phase:** Prototyp  
**Status:** 🟢 In Entwicklung  

[← Zurück zur Übersicht](README.md)

---

## 📋 Inhaltsverzeichnis

- [🔄 Crafting Flow](#-crafting-flow)
  - [Server-Authoritative Architecture](#server-authoritative-architecture)
  - [Crafting Flow](#crafting-flow)
  - [Gathering Flow](#gathering-flow)
- [🧱 DTOs / Enums](#-dtos--enums)
  - [ProfessionType Enum](#professiontype-enum)
  - [CraftingErrorCode Enum](#craftingerrorcode-enum)
  - [RecipeInfoDto](#recipeinfodto)
  - [CraftingQueueItemDto](#craftingqueueitemdto)
  - [Wichtige Konstanten](#wichtige-konstanten)
- [📩 Aktive Messages (1600-1634)](#-aktive-messages-1600-1634)
  - [CraftingOpen (1600)](#craftingopen-1600)
  - [CraftingClose (1601)](#craftingclose-1601)
  - [CraftingRecipeList (1602)](#craftingrecipelist-1602)
  - [CraftingStart (1603)](#craftingstart-1603)
  - [CraftingProgress (1604)](#craftingprogress-1604)
  - [CraftingComplete (1605)](#craftingcomplete-1605)
  - [CraftingFailed (1606)](#craftingfailed-1606)
  - [CraftingCancel (1607)](#craftingcancel-1607)
  - [CraftingQueue (1608)](#craftingqueue-1608)
  - [CraftingQueueAdd (1609)](#craftingqueueadd-1609)
  - [CraftingQueueRemove (1610)](#craftingqueueremove-1610)
  - [RecipeLearn (1611)](#recipelearn-1611)
  - [RecipeUnlearn (1612)](#recipeunlearn-1612)
  - [RecipeDiscovery (1613)](#recipediscovery-1613)
  - [ProfessionInfo (1620)](#professioninfo-1620)
  - [ProfessionLevelUp (1621)](#professionlevelup-1621)
  - [ProfessionSkillUp (1622)](#professionskillup-1622)
  - [GatheringStart (1630)](#gatheringstart-1630)
  - [GatheringProgress (1631)](#gatheringprogress-1631)
  - [GatheringComplete (1632)](#gatheringcomplete-1632)
  - [GatheringFailed (1633)](#gatheringfailed-1633)
  - [GatheringInterrupt (1634)](#gatheringinterrupt-1634)
- [🗑️ Obsolete Messages](#-obsolete-messages)
- [📎 Anhang](#-anhang)
  - [MessageType Enum (Crafting)](#messagetype-enum-crafting)
  - [Request/Response Paare](#requestresponse-paare)

---

## 🔄 Crafting Flow

### Server-Authoritative Architecture

```
┌─────────────────────────────────────────────────────────────────────┐
│                         SERVER (Authoritative)                      │
│  ┌───────────────────────────────────────────────────────────────┐  │
│  │ CraftingService                                               │  │
│  │  - Recipe validation                                          │  │
│  │  - Material check/consume                                     │  │
│  │  - Profession skill requirements                              │  │
│  │  - Crafting time calculation                                  │  │
│  │  - Quality/Success calculation                                │  │
│  └───────────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────────┘
                              │
           ┌──────────────────┼──────────────────┐
           ▼                  ▼                  ▼
    ┌────────────┐    ┌────────────┐    ┌────────────┐
    │  CLIENT A  │    │  CLIENT B  │    │  CLIENT C  │
    │  (Crafter) │    │  (Viewer)  │    │  (Gatherer)│
    └────────────┘    └────────────┘    └────────────┘
```

### Crafting Flow

```
    Client                                Server
       │                                    │
       │─── CraftingOpen (1600) ───────────▶│   C→S: Open UI
       │                                    │
       │◀── CraftingRecipeList (1602) ─────│   S→C: Available recipes
       │                                    │
       │─── CraftingStart (1603) ──────────▶│   C→S: Start craft
       │                                    │
       │◀── CraftingProgress (1604) ────────│   S→C: Progress %
       │◀── CraftingProgress (1604) ────────│   S→C: Progress %
       │◀── CraftingProgress (1604) ────────│   S→C: Progress %
       │                                    │
       │◀── CraftingComplete (1605) ────────│   S→C: Success + item
       │      OR                            │
       │◀── CraftingFailed (1606) ──────────│   S→C: Failed + reason
       │                                    │
       │─── CraftingClose (1601) ───────────▶│   C→S: Close UI
       │                                    │
```

### Gathering Flow

```
    Client                                Server
       │                                    │
       │─── GatheringStart (1630) ─────────▶│   C→S: Start gather
       │                                    │
       │◀── GatheringProgress (1631) ───────│   S→C: Progress %
       │◀── GatheringProgress (1631) ───────│   S→C: Progress %
       │                                    │
       │◀── GatheringComplete (1632) ───────│   S→C: Success + loot
       │      OR                            │
       │◀── GatheringFailed (1633) ─────────│   S→C: Failed
       │      OR                            │
       │◀── GatheringInterrupt (1634) ──────│   S→C: Interrupted
       │                                    │
```

---

## 🧱 DTOs / Enums

### ProfessionType Enum

```csharp
public enum ProfessionType : byte
{
    None = 0,
    Blacksmithing = 1,
    Leatherworking = 2,
    Tailoring = 3,
    Alchemy = 4,
    Enchanting = 5,
    Jewelcrafting = 6,
    Engineering = 7,
    Inscription = 8,
    Cooking = 9,
    FirstAid = 10,
    // Gathering
    Mining = 20,
    Herbalism = 21,
    Skinning = 22,
    Fishing = 23
}
```

### CraftingErrorCode Enum

```csharp
public enum CraftingErrorCode : byte
{
    None = 0,
    RecipeNotKnown = 1,
    MissingMaterials = 2,
    SkillTooLow = 3,
    InventoryFull = 4,
    AlreadyCrafting = 5,
    QueueFull = 6,
    InvalidRecipe = 7,
    CraftingStationRequired = 8,
    Interrupted = 9,
    CooldownActive = 10,
    NodeDepleted = 11,
    NodeNotFound = 12,
    TooFarAway = 13
}
```

### RecipeInfoDto

```csharp
[MessagePackObject]
public class RecipeInfoDto
{
    [Key(0)] public uint RecipeId { get; set; }
    [Key(1)] public string Name { get; set; }
    [Key(2)] public uint OutputItemId { get; set; }
    [Key(3)] public byte OutputQuantity { get; set; }
    [Key(4)] public List<MaterialDto> Materials { get; set; }
    [Key(5)] public ushort RequiredSkill { get; set; }
    [Key(6)] public ushort CraftTimeMs { get; set; }
    [Key(7)] public bool IsKnown { get; set; }
    [Key(8)] public byte DifficultyColor { get; set; } // 0=Grey,1=Green,2=Yellow,3=Orange
}

[MessagePackObject]
public class MaterialDto
{
    [Key(0)] public uint ItemId { get; set; }
    [Key(1)] public byte Quantity { get; set; }
}
```

### CraftingQueueItemDto

```csharp
[MessagePackObject]
public class CraftingQueueItemDto
{
    [Key(0)] public byte QueuePosition { get; set; }
    [Key(1)] public uint RecipeId { get; set; }
    [Key(2)] public byte Quantity { get; set; }
}
```

### Wichtige Konstanten

| Konstante | Wert | Beschreibung |
|-----------|------|-------------|
| MAX_PROFESSION_SKILL | 300 | Maximum skill level per profession |
| MAX_CRAFT_QUEUE | 5 | Maximum items in crafting queue |
| GATHER_RANGE | 5.0f | Maximum distance to gathering node |
| MIN_CRAFT_TIME_MS | 500 | Minimum crafting time |
| PROGRESS_UPDATE_INTERVAL_MS | 250 | How often progress updates are sent |

---

## 📩 Aktive Messages (1600-1634)

### CraftingOpen (1600)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Auth:** 🔒 Required  
**Rechte:** Keine besonderen

#### Beschreibung

Client öffnet das Crafting-Interface für eine bestimmte Profession. Server antwortet mit CraftingRecipeList.

#### Im Scope ✅

- Öffnen des Crafting-UI für eine Profession
- Optional: Mit Crafting-Station (NPC/Object)

#### Nicht im Scope ❌

- Recipe Details abrufen → siehe RecipeLearn (1611)

#### Request Payload

| Feld | Typ | Pflicht | Beschreibung |
|------|-----|---------|-------------|
| Type | MessageType | ✅ | = 1600 |
| ProfessionType | byte | ✅ | Enum ProfessionType |
| StationEntityId | uint | ❌ | Optional: ID der Crafting-Station |

#### Erwartete Response

- **CraftingRecipeList (1602)** – Liste verfügbarer Recipes

#### Verwandte Messages

- CraftingClose (1601)
- CraftingRecipeList (1602)

---

### CraftingClose (1601)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Auth:** 🔒 Required  
**Rechte:** Keine besonderen

#### Beschreibung

Client schließt das Crafting-Interface. Server beendet ggf. laufende Crafting-Session.

#### Im Scope ✅

- Schließen des Crafting-UI
- Cleanup der Server-Session

#### Nicht im Scope ❌

- Laufende Crafts abbrechen → siehe CraftingCancel (1607)

#### Request Payload

| Feld | Typ | Pflicht | Beschreibung |
|------|-----|---------|-------------|
| Type | MessageType | ✅ | = 1601 |

#### Erwartete Response

- Keine direkte Response (Fire-and-Forget)

#### Verwandte Messages

- CraftingOpen (1600)

---

### CraftingRecipeList (1602)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Auth:** 🔒 Required

#### Beschreibung

Server sendet Liste aller verfügbaren Recipes für die geöffnete Profession. Response auf CraftingOpen.

#### Im Scope ✅

- Alle bekannten Recipes
- Recipe-Schwierigkeit basierend auf aktuellem Skill

#### Nicht im Scope ❌

- Unbekannte Recipes (Discovery-System)

#### Response Payload

| Feld | Typ | Pflicht | Beschreibung |
|------|-----|---------|-------------|
| Type | MessageType | ✅ | = 1602 |
| ProfessionType | byte | ✅ | Enum ProfessionType |
| CurrentSkill | ushort | ✅ | Aktueller Skill-Level |
| Recipes | RecipeInfoDto[] | ✅ | Array von Recipes |

#### Verwandte Messages

- CraftingOpen (1600)

---

### CraftingStart (1603)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Auth:** 🔒 Required  
**Rechte:** Keine besonderen

#### Beschreibung

Client startet einen Craft-Vorgang für ein bestimmtes Recipe. Server validiert und beginnt Crafting.

#### Im Scope ✅

- Einzelnen Craft starten
- Material-Validierung
- Skill-Check

#### Nicht im Scope ❌

- Batch-Crafting → siehe CraftingQueueAdd (1609)

#### Request Payload

| Feld | Typ | Pflicht | Beschreibung |
|------|-----|---------|-------------|
| Type | MessageType | ✅ | = 1603 |
| RecipeId | uint | ✅ | ID des Recipes |
| Quantity | byte | ❌ | Anzahl (default: 1) |

#### Erwartete Response

- **CraftingProgress (1604)** – Fortschritt-Updates
- **CraftingComplete (1605)** – Bei Erfolg
- **CraftingFailed (1606)** – Bei Fehler

#### Verwandte Messages

- CraftingProgress (1604)
- CraftingComplete (1605)
- CraftingCancel (1607)

---

### CraftingProgress (1604)

**Richtung:** 📥 Server → Client  
**Frequenz:** ⚡ High-Frequency  
**Auth:** 🔒 Required

#### Beschreibung

Server sendet Crafting-Fortschritt für Progress-Bar-Anzeige.

#### Im Scope ✅

- Prozentualer Fortschritt (0-100)
- Regelmäßige Updates während Craft

#### Nicht im Scope ❌

- Finale Ergebnis-Meldung → siehe CraftingComplete/Failed

#### Response Payload

| Feld | Typ | Pflicht | Beschreibung |
|------|-----|---------|-------------|
| Type | MessageType | ✅ | = 1604 |
| RecipeId | uint | ✅ | ID des Recipes |
| Progress | byte | ✅ | 0-100 Prozent |
| RemainingMs | ushort | ✅ | Verbleibende Zeit in ms |

#### Verwandte Messages

- CraftingStart (1603)
- CraftingComplete (1605)

---

### CraftingComplete (1605)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Auth:** 🔒 Required

#### Beschreibung

Crafting erfolgreich abgeschlossen. Enthält das gecraftete Item und optional Skill-Up.

#### Im Scope ✅

- Gecraftetes Item
- Skill-Up Chance

#### Nicht im Scope ❌

- Item direkt ins Inventar → Inventory-System verarbeitet das

#### Response Payload

| Feld | Typ | Pflicht | Beschreibung |
|------|-----|---------|-------------|
| Type | MessageType | ✅ | = 1605 |
| RecipeId | uint | ✅ | ID des Recipes |
| OutputItemId | uint | ✅ | ID des gecrafteten Items |
| Quantity | byte | ✅ | Gecraftete Menge |
| Quality | byte | ❌ | Item-Qualität falls variabel |
| SkillGained | byte | ❌ | Skill-Punkte erhalten |

#### Verwandte Messages

- CraftingStart (1603)
- ProfessionSkillUp (1622)

---

### CraftingFailed (1606)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Auth:** 🔒 Required

#### Beschreibung

Crafting fehlgeschlagen. Informiert über Grund und ob Materialien verloren wurden.

#### Im Scope ✅

- Fehlergrund
- Material-Verlust Info

#### Nicht im Scope ❌

- Retry-Logik (Client-seitig)

#### Response Payload

| Feld | Typ | Pflicht | Beschreibung |
|------|-----|---------|-------------|
| Type | MessageType | ✅ | = 1606 |
| RecipeId | uint | ✅ | ID des Recipes |
| ErrorCode | CraftingErrorCode | ✅ | Fehlergrund |
| MaterialsLost | bool | ✅ | Wurden Materials verbraucht? |

#### Verwandte Messages

- CraftingStart (1603)

---

### CraftingCancel (1607)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Auth:** 🔒 Required  
**Rechte:** Keine besonderen

#### Beschreibung

Client bricht laufenden Craft-Vorgang ab. Materials werden nicht zurückgegeben.

#### Im Scope ✅

- Laufenden Craft abbrechen
- Queue-Item abbrechen

#### Nicht im Scope ❌

- Material-Rückgabe

#### Request Payload

| Feld | Typ | Pflicht | Beschreibung |
|------|-----|---------|-------------|
| Type | MessageType | ✅ | = 1607 |

#### Erwartete Response

- **CraftingFailed (1606)** – Mit ErrorCode = Interrupted

#### Verwandte Messages

- CraftingStart (1603)
- CraftingFailed (1606)

---

### CraftingQueue (1608)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Auth:** 🔒 Required

#### Beschreibung

Server sendet aktuelle Crafting-Queue des Spielers.

#### Im Scope ✅

- Alle Queue-Einträge
- Position und Status

#### Nicht im Scope ❌

- Queue-Manipulation → siehe CraftingQueueAdd/Remove

#### Response Payload

| Feld | Typ | Pflicht | Beschreibung |
|------|-----|---------|-------------|
| Type | MessageType | ✅ | = 1608 |
| QueueItems | CraftingQueueItemDto[] | ✅ | Array von Queue-Items |

#### Verwandte Messages

- CraftingQueueAdd (1609)
- CraftingQueueRemove (1610)

---

### CraftingQueueAdd (1609)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Auth:** 🔒 Required  
**Rechte:** Keine besonderen

#### Beschreibung

Client fügt Recipe zur Crafting-Queue hinzu.

#### Im Scope ✅

- Recipe zur Queue hinzufügen
- Mehrere Items auf einmal queuen

#### Nicht im Scope ❌

- Queue-Priorität ändern

#### Request Payload

| Feld | Typ | Pflicht | Beschreibung |
|------|-----|---------|-------------|
| Type | MessageType | ✅ | = 1609 |
| RecipeId | uint | ✅ | ID des Recipes |
| Quantity | byte | ✅ | Anzahl zu craften |

#### Erwartete Response

- **CraftingQueue (1608)** – Aktualisierte Queue
- **CraftingFailed (1606)** – Bei Fehler (Queue voll etc.)

#### Verwandte Messages

- CraftingQueue (1608)
- CraftingQueueRemove (1610)

---

### CraftingQueueRemove (1610)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Auth:** 🔒 Required  
**Rechte:** Keine besonderen

#### Beschreibung

Client entfernt Item aus der Crafting-Queue.

#### Im Scope ✅

- Queue-Eintrag entfernen
- Materials werden nicht reserviert bis Craft startet

#### Nicht im Scope ❌

- Laufenden Craft abbrechen → siehe CraftingCancel (1607)

#### Request Payload

| Feld | Typ | Pflicht | Beschreibung |
|------|-----|---------|-------------|
| Type | MessageType | ✅ | = 1610 |
| QueuePosition | byte | ✅ | Position in Queue (0-based) |

#### Erwartete Response

- **CraftingQueue (1608)** – Aktualisierte Queue

#### Verwandte Messages

- CraftingQueue (1608)
- CraftingQueueAdd (1609)

---

### RecipeLearn (1611)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Auth:** 🔒 Required  
**Rechte:** Keine besonderen

#### Beschreibung

Client lernt ein neues Recipe (von Trainer oder Recipe-Item).

#### Im Scope ✅

- Recipe von Trainer lernen
- Recipe-Scroll verwenden

#### Nicht im Scope ❌

- Discovery-Recipes → siehe RecipeDiscovery (1613)

#### Request Payload

| Feld | Typ | Pflicht | Beschreibung |
|------|-----|---------|-------------|
| Type | MessageType | ✅ | = 1611 |
| RecipeId | uint | ✅ | ID des zu lernenden Recipes |
| SourceType | byte | ✅ | 0=Trainer, 1=Item |
| SourceId | uint | ❌ | NpcId oder ItemId |

#### Erwartete Response

- **CraftingRecipeList (1602)** – Aktualisierte Recipe-Liste
- **CraftingFailed (1606)** – Bei Fehler

#### Verwandte Messages

- CraftingRecipeList (1602)

---

### RecipeUnlearn (1612)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Auth:** 🔒 Required  
**Rechte:** Keine besonderen

#### Beschreibung

Client verlernt ein Recipe (falls System dies unterstützt).

#### Im Scope ✅

- Recipe vergessen
- Keine Material-Rückgabe

#### Nicht im Scope ❌

- Automatisches Verlernen bei Profession-Wechsel

#### Request Payload

| Feld | Typ | Pflicht | Beschreibung |
|------|-----|---------|-------------|
| Type | MessageType | ✅ | = 1612 |
| RecipeId | uint | ✅ | ID des zu verlernenden Recipes |

#### Erwartete Response

- **CraftingRecipeList (1602)** – Aktualisierte Recipe-Liste

#### Verwandte Messages

- RecipeLearn (1611)

---

### RecipeDiscovery (1613)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Auth:** 🔒 Required

#### Beschreibung

Spieler entdeckt neues Recipe durch Experimentieren/Crafting.

#### Im Scope ✅

- Discovery-Benachrichtigung
- Recipe-Details

#### Nicht im Scope ❌

- Discovery-Trigger-Logik (Server-intern)

#### Response Payload

| Feld | Typ | Pflicht | Beschreibung |
|------|-----|---------|-------------|
| Type | MessageType | ✅ | = 1613 |
| RecipeId | uint | ✅ | ID des entdeckten Recipes |
| RecipeName | string | ✅ | Name für UI-Anzeige |

#### Verwandte Messages

- CraftingComplete (1605)

---

### ProfessionInfo (1620)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Auth:** 🔒 Required

#### Beschreibung

Server sendet vollständige Profession-Informationen (bei Login oder Request).

#### Im Scope ✅

- Alle Professionen des Charakters
- Skill-Level und XP
- Gelernte Recipes pro Profession

#### Nicht im Scope ❌

- Einzelne Recipe-Details

#### Response Payload

| Feld | Typ | Pflicht | Beschreibung |
|------|-----|---------|-------------|
| Type | MessageType | ✅ | = 1620 |
| Professions | ProfessionDto[] | ✅ | Array aller Professionen |

```csharp
[MessagePackObject]
public class ProfessionDto
{
    [Key(0)] public ProfessionType Type { get; set; }
    [Key(1)] public ushort CurrentSkill { get; set; }
    [Key(2)] public ushort MaxSkill { get; set; }
    [Key(3)] public uint[] KnownRecipeIds { get; set; }
}
```

#### Verwandte Messages

- ProfessionLevelUp (1621)
- ProfessionSkillUp (1622)

---

### ProfessionLevelUp (1621)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Auth:** 🔒 Required

#### Beschreibung

Profession-Level gestiegen (z.B. Apprentice → Journeyman). Neue Recipes freigeschaltet.

#### Im Scope ✅

- Level-Up Benachrichtigung
- Neues Max-Skill
- Freigeschaltete Recipes

#### Nicht im Scope ❌

- Skill-Punkte → siehe ProfessionSkillUp (1622)

#### Response Payload

| Feld | Typ | Pflicht | Beschreibung |
|------|-----|---------|-------------|
| Type | MessageType | ✅ | = 1621 |
| ProfessionType | byte | ✅ | Enum ProfessionType |
| NewLevel | byte | ✅ | Neuer Level (1=Apprentice, etc.) |
| NewMaxSkill | ushort | ✅ | Neues Skill-Maximum |
| UnlockedRecipes | uint[] | ❌ | Neu verfügbare Recipe-IDs |

#### Verwandte Messages

- ProfessionInfo (1620)

---

### ProfessionSkillUp (1622)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Auth:** 🔒 Required

#### Beschreibung

Profession-Skill durch erfolgreichen Craft gestiegen.

#### Im Scope ✅

- Skill-Up Benachrichtigung
- Neuer Skill-Wert

#### Nicht im Scope ❌

- Recipe-Schwierigkeits-Update (Client berechnet lokal)

#### Response Payload

| Feld | Typ | Pflicht | Beschreibung |
|------|-----|---------|-------------|
| Type | MessageType | ✅ | = 1622 |
| ProfessionType | byte | ✅ | Enum ProfessionType |
| OldSkill | ushort | ✅ | Vorheriger Skill-Wert |
| NewSkill | ushort | ✅ | Neuer Skill-Wert |
| PointsGained | byte | ✅ | Gewonnene Punkte |

#### Verwandte Messages

- CraftingComplete (1605)
- ProfessionInfo (1620)

---

### GatheringStart (1630)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Auth:** 🔒 Required  
**Rechte:** Keine besonderen

#### Beschreibung

Client beginnt Gathering an einem Resource-Node (Mining, Herbalism, etc.).

#### Im Scope ✅

- Gathering starten
- Node-Validierung (Existenz, Range, Skill)

#### Nicht im Scope ❌

- Fishing → separate Kategorie

#### Request Payload

| Feld | Typ | Pflicht | Beschreibung |
|------|-----|---------|-------------|
| Type | MessageType | ✅ | = 1630 |
| NodeEntityId | uint | ✅ | Entity-ID des Resource-Nodes |
| GatherType | byte | ❌ | 0=Normal, 1=Prospecting |

#### Erwartete Response

- **GatheringProgress (1631)** – Fortschritt-Updates
- **GatheringComplete (1632)** – Bei Erfolg
- **GatheringFailed (1633)** – Bei Fehler

#### Verwandte Messages

- GatheringProgress (1631)
- GatheringComplete (1632)

---

### GatheringProgress (1631)

**Richtung:** 📥 Server → Client  
**Frequenz:** ⚡ High-Frequency  
**Auth:** 🔒 Required

#### Beschreibung

Server sendet Gathering-Fortschritt für Progress-Bar.

#### Im Scope ✅

- Prozentualer Fortschritt

#### Nicht im Scope ❌

- Finale Ergebnis-Meldung

#### Response Payload

| Feld | Typ | Pflicht | Beschreibung |
|------|-----|---------|-------------|
| Type | MessageType | ✅ | = 1631 |
| NodeEntityId | uint | ✅ | Entity-ID des Nodes |
| Progress | byte | ✅ | 0-100 Prozent |

#### Verwandte Messages

- GatheringStart (1630)
- GatheringComplete (1632)

---

### GatheringComplete (1632)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Auth:** 🔒 Required

#### Beschreibung

Gathering erfolgreich abgeschlossen. Enthält gesammelte Materials.

#### Im Scope ✅

- Gesammelte Items
- Node-Status (erschöpft?)
- Skill-Up Chance

#### Nicht im Scope ❌

- Item direkt ins Inventar → Inventory-System

#### Response Payload

| Feld | Typ | Pflicht | Beschreibung |
|------|-----|---------|-------------|
| Type | MessageType | ✅ | = 1632 |
| NodeEntityId | uint | ✅ | Entity-ID des Nodes |
| Loot | LootItemDto[] | ✅ | Gesammelte Items |
| NodeDepleted | bool | ✅ | Node erschöpft? |
| SkillGained | byte | ❌ | Skill-Punkte erhalten |

```csharp
[MessagePackObject]
public class LootItemDto
{
    [Key(0)] public uint ItemId { get; set; }
    [Key(1)] public byte Quantity { get; set; }
}
```

#### Verwandte Messages

- GatheringStart (1630)
- ProfessionSkillUp (1622)

---

### GatheringFailed (1633)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Auth:** 🔒 Required

#### Beschreibung

Gathering fehlgeschlagen. Node erschöpft oder Skill zu niedrig.

#### Im Scope ✅

- Fehlergrund
- Node-Status

#### Nicht im Scope ❌

- Retry-Logik (Client-seitig)

#### Response Payload

| Feld | Typ | Pflicht | Beschreibung |
|------|-----|---------|-------------|
| Type | MessageType | ✅ | = 1633 |
| NodeEntityId | uint | ✅ | Entity-ID des Nodes |
| ErrorCode | CraftingErrorCode | ✅ | Fehlergrund |

#### Verwandte Messages

- GatheringStart (1630)

---

### GatheringInterrupt (1634)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Auth:** 🔒 Required

#### Beschreibung

Gathering durch Damage, Movement oder andere Aktion unterbrochen.

#### Im Scope ✅

- Interrupt-Benachrichtigung
- Grund für Interrupt

#### Nicht im Scope ❌

- Automatischer Retry

#### Response Payload

| Feld | Typ | Pflicht | Beschreibung |
|------|-----|---------|-------------|
| Type | MessageType | ✅ | = 1634 |
| NodeEntityId | uint | ✅ | Entity-ID des Nodes |
| Reason | byte | ✅ | 0=Damage, 1=Movement, 2=Other |

#### Verwandte Messages

- GatheringStart (1630)
- GatheringFailed (1633)

---

## 🗑️ Obsolete Messages

*Derzeit keine obsoleten Messages in dieser Kategorie.*

---

## 📎 Anhang

### MessageType Enum (Crafting)

```csharp
// ═══════════════════════════════════════════════════════════════
// CRAFTING (1600-1699)
// ═══════════════════════════════════════════════════════════════
CraftingOpen = 1600,
CraftingClose = 1601,
CraftingRecipeList = 1602,
CraftingStart = 1603,
CraftingProgress = 1604,
CraftingComplete = 1605,
CraftingFailed = 1606,
CraftingCancel = 1607,
CraftingQueue = 1608,
CraftingQueueAdd = 1609,
CraftingQueueRemove = 1610,
RecipeLearn = 1611,
RecipeUnlearn = 1612,
RecipeDiscovery = 1613,
ProfessionInfo = 1620,
ProfessionLevelUp = 1621,
ProfessionSkillUp = 1622,
GatheringStart = 1630,
GatheringProgress = 1631,
GatheringComplete = 1632,
GatheringFailed = 1633,
GatheringInterrupt = 1634,
```

### Request/Response Paare

| Request | Response(s) |
|---------|-------------|
| CraftingOpen (1600) | CraftingRecipeList (1602) |
| CraftingClose (1601) | - (Fire-and-Forget) |
| CraftingStart (1603) | CraftingProgress (1604), CraftingComplete (1605), CraftingFailed (1606) |
| CraftingCancel (1607) | CraftingFailed (1606) |
| CraftingQueueAdd (1609) | CraftingQueue (1608), CraftingFailed (1606) |
| CraftingQueueRemove (1610) | CraftingQueue (1608) |
| RecipeLearn (1611) | CraftingRecipeList (1602), CraftingFailed (1606) |
| RecipeUnlearn (1612) | CraftingRecipeList (1602) |
| GatheringStart (1630) | GatheringProgress (1631), GatheringComplete (1632), GatheringFailed (1633) |

### Datei-Struktur

```
shared/Mmo.Shared/Messaging/
├── Enums/
│   ├── MessageType.cs          # Crafting = 1600-1634
│   ├── ProfessionType.cs
│   └── CraftingErrorCode.cs
├── Dtos/
│   ├── RecipeInfoDto.cs
│   ├── MaterialDto.cs
│   ├── CraftingQueueItemDto.cs
│   ├── ProfessionDto.cs
│   └── LootItemDto.cs
└── Messages/
    └── Crafting/
        ├── CraftingOpenMessage.cs
        ├── CraftingStartMessage.cs
        └── ...
```

---

---

**Letzte Aktualisierung**: 2026-01-02  
**Version**: 3.0.0  
**Status**: ✅ Aligned mit MessageType Enum (22 Messages)

[← Zurück zur Übersicht](README.md)
