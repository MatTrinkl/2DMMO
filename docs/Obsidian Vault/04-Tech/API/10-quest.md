# 📜 Quest Messages (1000-1023)

**Kategorie:** 10  
**Range:** 1000-1023 (AKTIV)  
**Phase:** Prototyp  
**Status:** 🟢 In Entwicklung

[← Zurück zur Übersicht](Message-Reference.md)

---

## 📋 Inhaltsverzeichnis

- [🔄 Quest Flow](#-quest-flow)
- [🧱 DTOs / Enums / Interfaces](#-dtos--enums--interfaces)
- [📩 Aktive Messages (1000-1023)](#-aktive-messages-1000-1023)
  - [QuestAccept (1000)](#questaccept-1000)
  - [QuestAcceptResult (1001)](#questacceptresult-1001)
  - [QuestAbandon (1002)](#questabandon-1002)
  - [QuestProgress (1003)](#questprogress-1003)
  - [QuestComplete (1004)](#questcomplete-1004)
  - [QuestCompleteResult (1005)](#questcompleteresult-1005)
  - [QuestRewardChoose (1006)](#questrewardchoose-1006)
  - [QuestRewardReceive (1007)](#questrewardreceive-1007)
  - [QuestListRequest (1008)](#questlistrequest-1008)
  - [QuestListResponse (1009)](#questlistresponse-1009)
  - [QuestLogUpdate (1010)](#questlogupdate-1010)
  - [QuestShare (1011)](#questshare-1011)
  - [QuestShareResponse (1012)](#questshareresponse-1012)
  - [QuestTrack (1013)](#questtrack-1013)
  - [QuestUntrack (1014)](#questuntrack-1014)
  - [QuestObjectiveUpdate (1015)](#questobjectiveupdate-1015)
  - [QuestPoiRequest (1016)](#questpoirequest-1016)
  - [QuestPoiResponse (1017)](#questpoiresponse-1017)
  - [QuestGiverStatus (1018)](#questgiverstatus-1018)
  - [QuestGiverList (1019)](#questgiverlist-1019)
  - [DailyQuestReset (1020)](#dailyquestreset-1020)
  - [WeeklyQuestReset (1021)](#weeklyquestreset-1021)
  - [QuestChainUpdate (1022)](#questchainupdate-1022)
  - [QuestRepeatableReset (1023)](#questrepeatblereset-1023)
- [🗑️ Obsolete Messages](#️-obsolete-messages)
- [📎 Anhang](#-anhang)

---

## 🔄 Quest Flow

### Server-Authoritative Architecture

Das Quest-System ist vollständig server-authoritativ. Der Server:
- Validiert alle Quest-Aktionen (Accept, Abandon, Complete)
- Tracked Progress serverseitig (Kills, Items, etc.)
- Verwaltet Prerequisites und Quest-Chains
- Kontrolliert Rewards und verhindert Exploits

### Quest Accept + Progress Flow

```
Client                    Zone Server              Quest DB
  │                            │                        │
  │  QuestAccept (1000)        │                        │
  │  {QuestId, QuestGiverId}   │                        │
  │───────────────────────────►│                        │
  │                            │  Check Prerequisites   │
  │                            │───────────────────────►│
  │                            │                        │
  │                            │  Quest Data + OK       │
  │                            │◄───────────────────────│
  │                            │                        │
  │  QuestAcceptResult (1001)  │                        │
  │  {Success: true}           │                        │
  │◄───────────────────────────│                        │
  │                            │                        │
  │  QuestLogUpdate (1010)     │                        │
  │  {UpdateType: "added"}     │                        │
  │◄───────────────────────────│                        │
  │                            │                        │
  │        ... Gameplay (Kills, Items) ...              │
  │                            │                        │
  │  QuestProgress (1003)      │                        │
  │  {Objectives Updated}      │                        │
  │◄───────────────────────────│                        │
```

### Quest Complete + Reward Flow

```
Client                    Zone Server              Quest DB
  │                            │                        │
  │  QuestComplete (1004)      │                        │
  │  {QuestId, QuestGiverId}   │                        │
  │───────────────────────────►│                        │
  │                            │  Validate Completion   │
  │                            │───────────────────────►│
  │                            │                        │
  │                            │  OK + Rewards          │
  │                            │◄───────────────────────│
  │                            │                        │
  │  QuestCompleteResult       │                        │
  │  (1005) {Success: true}    │                        │
  │◄───────────────────────────│                        │
  │                            │                        │
  │  QuestRewardReceive (1007) │                        │
  │  {XP, Gold, Items}         │                        │
  │◄───────────────────────────│                        │
  │                            │                        │
  │  QuestLogUpdate (1010)     │                        │
  │  {UpdateType: "removed"}   │                        │
  │◄───────────────────────────│                        │
```

### Quest Sharing Flow

```
Client A                  Zone Server              Client B
  │                            │                        │
  │  QuestShare (1011)         │                        │
  │  {QuestId}                 │                        │
  │───────────────────────────►│                        │
  │                            │  Check Party + Quest   │
  │                            │  Check B Prerequisites │
  │                            │                        │
  │                            │  QuestShareResponse    │
  │                            │  (1012)                │
  │                            │───────────────────────►│
  │                            │                        │
  │                            │  QuestAccept (1000)    │
  │                            │  {from Share}          │
  │                            │◄───────────────────────│
```

---

## 🧱 DTOs / Enums / Interfaces

### QuestEntry DTO

```csharp
[MessagePackObject]
public class QuestEntry
{
    [Key(0)] public uint QuestId { get; set; }
    [Key(1)] public string Title { get; set; }
    [Key(2)] public int Level { get; set; }
    [Key(3)] public bool IsTracked { get; set; }
    [Key(4)] public List<ObjectiveProgress> Objectives { get; set; }
    [Key(5)] public long TimeLimit { get; set; } // 0 = no limit
    [Key(6)] public QuestType Type { get; set; }
}
```

### ObjectiveProgress DTO

```csharp
[MessagePackObject]
public class ObjectiveProgress
{
    [Key(0)] public byte ObjectiveIndex { get; set; }
    [Key(1)] public int Current { get; set; }
    [Key(2)] public int Required { get; set; }
    [Key(3)] public bool Completed { get; set; }
    [Key(4)] public string Description { get; set; }
}
```

### QuestPoi DTO

```csharp
[MessagePackObject]
public class QuestPoi
{
    [Key(0)] public byte ObjectiveIndex { get; set; }
    [Key(1)] public int ZoneId { get; set; }
    [Key(2)] public float X { get; set; }
    [Key(3)] public float Y { get; set; }
    [Key(4)] public PoiType Type { get; set; }
}
```

### ItemReward DTO

```csharp
[MessagePackObject]
public class ItemReward
{
    [Key(0)] public uint ItemId { get; set; }
    [Key(1)] public int Quantity { get; set; }
}
```

### ReputationReward DTO

```csharp
[MessagePackObject]
public class ReputationReward
{
    [Key(0)] public uint FactionId { get; set; }
    [Key(1)] public int Amount { get; set; }
}
```

### QuestSummary DTO

```csharp
[MessagePackObject]
public class QuestSummary
{
    [Key(0)] public uint QuestId { get; set; }
    [Key(1)] public string Title { get; set; }
    [Key(2)] public int Level { get; set; }
    [Key(3)] public bool IsDaily { get; set; }
    [Key(4)] public bool IsRepeatable { get; set; }
}
```

### QuestType Enum

```csharp
public enum QuestType : byte
{
    Normal = 0,      // Standard Quest
    Daily = 1,       // Täglich wiederholbar
    Weekly = 2,      // Wöchentlich wiederholbar
    Repeatable = 3,  // Nach Cooldown wiederholbar
    Story = 4,       // Main Story Quest
    Side = 5,        // Side Quest
    Dungeon = 6,     // Dungeon Quest
    Raid = 7         // Raid Quest
}
```

### PoiType Enum

```csharp
public enum PoiType : byte
{
    Objective = 0,   // Quest-Objective Location
    TurnIn = 1,      // Quest Turn-in NPC
    QuestGiver = 2,  // Quest Giver NPC
    Area = 3         // Quest Area (Circle on Map)
}
```

### QuestGiverStatusType Enum

```csharp
public enum QuestGiverStatusType : byte
{
    None = 0,        // Keine Quests (.)
    Available = 1,   // Quest verfügbar (!)
    Completable = 2, // Quest abschließbar (?)
    InProgress = 3   // Quest aktiv (...)
}
```

### Quest-Konstanten

| Konstante | Wert | Beschreibung |
|-----------|------|--------------|
| `MAX_ACTIVE_QUESTS` | 25 | Maximale aktive Quests |
| `SHARE_TIMEOUT_SEC` | 120 | Quest-Share Prompt Timeout |
| `QUEST_GIVER_RANGE` | 30.0f | Range für Quest-Giver Status Update |
| `MAX_OBJECTIVES` | 8 | Max Objectives pro Quest |

---

## 📩 Aktive Messages (1000-1023)

---

## QuestAccept (1000)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Client nimmt eine Quest von einem Quest-Giver (NPC, Objekt, Item) an. Server validiert ob Quest verfügbar ist, ob Prerequisites erfüllt sind, und ob Questlog-Platz vorhanden ist.

### Im Scope ✅
- Quest von NPC annehmen
- Quest von Objekt/Item annehmen
- Prerequisite-Prüfung (Level, Vorquests, Reputation)
- Questlog-Slot-Check

### Nicht im Scope ❌
- Quest-Progress-Updates → verwende `QuestProgress` (1003)
- Quest abschließen → verwende `QuestComplete` (1004)
- Quest teilen → verwende `QuestShare` (1011)

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| QuestId | uint | ID der Quest | Ja |
| QuestGiverId | int | Entity-ID des Quest-Gebers | Ja |

### Erwartete Response
- **Bei Erfolg:** `QuestAcceptResult` (1001) mit Success=true + `QuestLogUpdate` (1010)
- **Bei Fehler:** `QuestAcceptResult` (1001) mit ErrorCode

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `QuestAcceptResult` | 1001 | Response zu diesem Request |
| `QuestLogUpdate` | 1010 | Quest wird zum Questlog hinzugefügt |
| `QuestGiverStatus` | 1018 | Zeigt verfügbare Quests an NPC |

### Beispiel Payload
```csharp
var questAccept = new QuestAccept
{
    Type = MessageType.QuestAccept,
    QuestId = 1234,
    QuestGiverId = 50001 // NPC Entity-ID
};
```

### Error Codes
| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `QUEST_LOG_FULL` | Questlog ist voll (max. 25 Quests) | Quest ablehnen oder andere Quest abandonen |
| `LEVEL_TOO_LOW` | Character-Level zu niedrig | Leveln |
| `PREREQUISITE_NOT_MET` | Vorquest nicht abgeschlossen | Vorquest abschließen |
| `REPUTATION_TOO_LOW` | Reputation zu niedrig | Reputation erhöhen |
| `QUEST_ALREADY_ACCEPTED` | Quest bereits im Questlog | Ignorieren |
| `QUEST_ALREADY_COMPLETED` | Quest bereits abgeschlossen (non-repeatable) | Ignorieren |
| `DAILY_LIMIT_REACHED` | Max. Daily Quests erreicht | Auf Reset warten |

### Notizen
- **Questlog-Limit**: 25 aktive Quests
- **Sharing**: Wenn in Party, automatisch Share-Prompt an Party-Mitglieder
- **Quest-Ketten**: Server prüft automatisch Prerequisite-Quests
- **Daily/Weekly**: Count wird server-seitig getrackt

---

## QuestAcceptResult (1001)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung
Antwort auf QuestAccept. Bestätigt erfolgreiche Annahme oder gibt Fehlergrund zurück.

### Im Scope ✅
- Erfolgs-Status
- Error-Code bei Fehler
- Quest-Details bei Erfolg

### Nicht im Scope ❌
- Questlog-Update → separate `QuestLogUpdate` (1010) Message
- Objective-Updates → verwende `QuestObjectiveUpdate` (1015)

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Success | bool | Quest erfolgreich angenommen? | Ja |
| QuestId | uint | ID der Quest | Ja |
| ErrorCode | string | Fehlercode falls Success=false | Nein |
| ErrorMessage | string | Menschenlesbare Fehlermeldung | Nein |

### Beispiel Payload
```csharp
// Erfolg
var successResult = new QuestAcceptResult
{
    Type = MessageType.QuestAcceptResult,
    Success = true,
    QuestId = 1234
};

// Fehler
var errorResult = new QuestAcceptResult
{
    Type = MessageType.QuestAcceptResult,
    Success = false,
    QuestId = 1234,
    ErrorCode = "LEVEL_TOO_LOW",
    ErrorMessage = "You must be level 10 to accept this quest"
};
```

### Notizen
- Bei Success folgt `QuestLogUpdate` (1010) mit vollständigen Quest-Details
- Client zeigt Accept-Animation im UI

---

## QuestAbandon (1002)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Client gibt eine aktive Quest auf. Server entfernt Quest aus Questlog und löscht Progress.

### Im Scope ✅
- Quest aufgeben
- Progress wird gelöscht
- Quest-Items werden entfernt

### Nicht im Scope ❌
- Quest abschließen → verwende `QuestComplete` (1004)
- Quest teilen → verwende `QuestShare` (1011)

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| QuestId | uint | ID der Quest die aufgegeben wird | Ja |

### Erwartete Response
- **Immer:** `QuestLogUpdate` (1010) mit removed Quest

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `QuestLogUpdate` | 1010 | Quest wird aus Questlog entfernt |
| `QuestAccept` | 1000 | Quest erneut annehmen |

### Beispiel Payload
```csharp
var questAbandon = new QuestAbandon
{
    Type = MessageType.QuestAbandon,
    QuestId = 1234
};
```

### Notizen
- **Quest-Items**: Quest-spezifische Items werden aus Inventory entfernt
- **Cooldown**: Keine Cooldown zum erneuten Accept (außer bei Dailies/Weeklies)
- **Shared Quests**: Wenn Quest geshared war, wird Share-Status entfernt
- **Confirmation**: Client sollte Confirmation-Dialog zeigen

---

## QuestProgress (1003)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Server informiert über Quest-Fortschritt. Wird gesendet wenn Objectives sich ändern (Kill-Count, Item-Sammlung, etc.).

### Im Scope ✅
- Objective-Progress-Update
- Multiple Objectives pro Quest
- Progress-Prozentsatz

### Nicht im Scope ❌
- Quest abschließen → verwende `QuestComplete` (1004)
- Neue Quest hinzufügen → verwende `QuestLogUpdate` (1010)

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| QuestId | uint | ID der Quest | Ja |
| Objectives | List<ObjectiveProgress> | Progress aller Objectives | Ja |

**ObjectiveProgress**:
| Feld | Typ | Beschreibung |
|------|-----|--------------|
| ObjectiveIndex | byte | Index des Objectives (0-based) |
| Current | int | Aktueller Fortschritt |
| Required | int | Benötigter Fortschritt |
| Completed | bool | Objective abgeschlossen? |

### Erwartete Response
- **Keine** - Client updated UI

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `QuestObjectiveUpdate` | 1015 | Einzelnes Objective-Update |
| `QuestComplete` | 1004 | Wenn alle Objectives completed |

### Beispiel Payload
```csharp
var questProgress = new QuestProgress
{
    Type = MessageType.QuestProgress,
    QuestId = 1234,
    Objectives = new List<ObjectiveProgress>
    {
        new ObjectiveProgress
        {
            ObjectiveIndex = 0,
            Current = 5,
            Required = 10,
            Completed = false
        },
        new ObjectiveProgress
        {
            ObjectiveIndex = 1,
            Current = 3,
            Required = 3,
            Completed = true
        }
    }
};
```

### Notizen
- **Trigger**: Wird gesendet bei Kill, Item-Pickup, NPC-Interact, etc.
- **Sound**: Client spielt Quest-Progress-Sound
- **UI**: Client zeigt Progress-Notification
- **Shared Quest**: Wenn Quest geshared, sehen alle Party-Member Progress

---

## QuestComplete (1004)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Client schließt eine Quest ab (Turn-in bei NPC). Server validiert ob alle Objectives completed sind und gibt Rewards.

### Im Scope ✅
- Quest turn-in
- Reward-Auswahl (falls Multiple Rewards)
- XP/Gold/Item-Rewards
- Reputation-Gain

### Nicht im Scope ❌
- Quest annehmen → verwende `QuestAccept` (1000)
- Quest aufgeben → verwende `QuestAbandon` (1002)

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| QuestId | uint | ID der Quest | Ja |
| QuestGiverId | int | Entity-ID des Turn-in NPCs | Ja |
| RewardChoiceIndex | int | Index der gewählten Reward (falls Multiple) | Nein |

### Erwartete Response
- **Bei Erfolg:** `QuestCompleteResult` (1005) + `QuestRewardReceive` (1007)
- **Bei Fehler:** `QuestCompleteResult` (1005) mit ErrorCode

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `QuestCompleteResult` | 1005 | Response zu diesem Request |
| `QuestRewardReceive` | 1007 | Rewards werden gegeben |
| `QuestChainUpdate` | 1022 | Follow-up Quest wird freigeschaltet |

### Beispiel Payload
```csharp
var questComplete = new QuestComplete
{
    Type = MessageType.QuestComplete,
    QuestId = 1234,
    QuestGiverId = 50001,
    RewardChoiceIndex = 1 // Wähle 2. Reward-Option
};
```

### Error Codes
| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `OBJECTIVES_NOT_COMPLETED` | Nicht alle Objectives erfüllt | Objectives abschließen |
| `WRONG_QUEST_GIVER` | Falscher NPC für Turn-in | Richtigen NPC finden |
| `INVENTORY_FULL` | Kein Platz für Rewards | Platz schaffen |
| `QUEST_NOT_IN_LOG` | Quest nicht im Questlog | Quest erneut annehmen |

### Notizen
- **Reward-Auswahl**: Bei Multiple Rewards muss Client vorher wählen
- **Chain-Quests**: Nächste Quest in Kette wird automatisch verfügbar
- **Achievement**: Completion kann Achievement freischalten
- **XP-Scaling**: XP-Reward skaliert mit Level-Unterschied

---

## QuestCompleteResult (1005)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung
Antwort auf QuestComplete. Bestätigt erfolgreiche Completion oder gibt Fehlergrund zurück.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Success | bool | Quest erfolgreich abgeschlossen? | Ja |
| QuestId | uint | ID der Quest | Ja |
| ErrorCode | string | Fehlercode falls Failed | Nein |
| ErrorMessage | string | Fehlermeldung | Nein |

### Beispiel Payload
```csharp
var completeResult = new QuestCompleteResult
{
    Type = MessageType.QuestCompleteResult,
    Success = true,
    QuestId = 1234
};
```

### Notizen
- Bei Success folgt `QuestRewardReceive` (1007)
- Quest wird aus Questlog entfernt
- Client spielt Quest-Complete-Sound und Animation

---

## QuestRewardChoose (1006)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
**Alternative zu RewardChoiceIndex in QuestComplete** - Separates Message wenn Reward-Auswahl vor Turn-in erfolgt.

### Im Scope ✅
- Reward vormerken
- UI-State speichern

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| QuestId | uint | ID der Quest | Ja |
| RewardIndex | int | Index der gewählten Reward | Ja |

### Notizen
- **Optional**: Kann auch direkt in `QuestComplete` (1004) mitgesendet werden
- Server speichert Auswahl temporär

---

## QuestRewardReceive (1007)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Server gibt Quest-Rewards (XP, Gold, Items, Reputation). Wird nach erfolgreicher `QuestComplete` gesendet.

### Im Scope ✅
- XP-Reward
- Gold-Reward
- Item-Rewards (fixe + gewählte)
- Reputation-Gain
- Titel-Unlock (falls Quest-Reward)

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| QuestId | uint | ID der Quest | Ja |
| XpGained | int | Erhaltene XP | Ja |
| GoldGained | int | Erhaltenes Gold (in Copper) | Ja |
| Items | List<ItemReward> | Item-Rewards | Nein |
| ReputationGains | List<ReputationReward> | Reputation-Gains | Nein |
| TitleUnlocked | uint | Titel-ID falls freigeschaltet | Nein |

**ItemReward**:
| Feld | Typ | Beschreibung |
|------|-----|--------------|
| ItemId | uint | Item-ID |
| Quantity | int | Anzahl |

**ReputationReward**:
| Feld | Typ | Beschreibung |
|------|-----|--------------|
| FactionId | uint | Fraktions-ID |
| Amount | int | Reputation-Gain |

### Beispiel Payload
```csharp
var rewardReceive = new QuestRewardReceive
{
    Type = MessageType.QuestRewardReceive,
    QuestId = 1234,
    XpGained = 1500,
    GoldGained = 50000, // 5 Gold
    Items = new List<ItemReward>
    {
        new ItemReward { ItemId = 5001, Quantity = 1 }, // Chosen Reward
        new ItemReward { ItemId = 5002, Quantity = 5 }  // Fixed Reward
    },
    ReputationGains = new List<ReputationReward>
    {
        new ReputationReward { FactionId = 10, Amount = 250 }
    }
};
```

### Notizen
- **XP-Scaling**: XP wird angepasst wenn Quest gray (zu niedriges Level)
- **Inventory**: Server prüft vorher ob Platz vorhanden
- **Mail**: Falls Inventory voll, werden Items per Mail gesendet
- **UI**: Client zeigt Reward-Screen

---

## QuestListRequest (1008)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Fordert Liste aller aktiven Quests im Questlog an. Wird bei Login/Reconnect gesendet.

### Request Payload
Keine zusätzlichen Felder

### Erwartete Response
- **Immer:** `QuestListResponse` (1009)

### Beispiel Payload
```csharp
var listRequest = new QuestListRequest
{
    Type = MessageType.QuestListRequest
};
```

---

## QuestListResponse (1009)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung
Liste aller aktiven Quests im Questlog mit Details und Progress.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Quests | List<QuestEntry> | Alle aktiven Quests | Ja |

**QuestEntry**:
| Feld | Typ | Beschreibung |
|------|-----|--------------|
| QuestId | uint | Quest-ID |
| Title | string | Quest-Titel |
| Level | int | Quest-Level |
| IsTracked | bool | Wird getrackt? |
| Objectives | List<ObjectiveProgress> | Progress aller Objectives |
| TimeLimit | long | Unix Timestamp (Deadline) falls Timed Quest |

### Beispiel Payload
```csharp
var listResponse = new QuestListResponse
{
    Type = MessageType.QuestListResponse,
    Quests = new List<QuestEntry>
    {
        new QuestEntry
        {
            QuestId = 1234,
            Title = "Defeat the Goblins",
            Level = 10,
            IsTracked = true,
            Objectives = new List<ObjectiveProgress>
            {
                new ObjectiveProgress { ObjectiveIndex = 0, Current = 5, Required = 10, Completed = false }
            }
        }
    }
};
```

### Notizen
- **Login**: Wird automatisch nach Login gesendet
- **Max**: 25 aktive Quests
- **Sorting**: Client sortiert nach Level/Zone

---

## QuestLogUpdate (1010)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Update zum Questlog (Quest added, removed, oder updated). Wird nach Accept, Abandon, Complete gesendet.

### Im Scope ✅
- Quest hinzugefügt
- Quest entfernt
- Quest-Details geändert

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| UpdateType | string | "added", "removed", "updated" | Ja |
| Quest | QuestEntry | Quest-Details (bei added/updated) | Nein |
| QuestId | uint | Quest-ID (bei removed) | Nein |

### Beispiel Payload
```csharp
// Quest added
var logUpdate = new QuestLogUpdate
{
    Type = MessageType.QuestLogUpdate,
    UpdateType = "added",
    Quest = new QuestEntry { QuestId = 1234, Title = "New Quest", ... }
};

// Quest removed
var logUpdate = new QuestLogUpdate
{
    Type = MessageType.QuestLogUpdate,
    UpdateType = "removed",
    QuestId = 1234
};
```

---

## QuestShare (1011)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Teilt eine Quest mit der Party. Alle Party-Member erhalten Share-Prompt.

### Im Scope ✅
- Quest mit Party teilen
- Automatische Share-Prompts

### Nicht im Scope ❌
- Solo-Quest sharing
- Cross-Faction sharing

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| QuestId | uint | ID der zu teilenden Quest | Ja |

### Erwartete Response
- **Bei Erfolg:** `QuestShareResponse` (1012) an alle Party-Member

### Beispiel Payload
```csharp
var questShare = new QuestShare
{
    Type = MessageType.QuestShare,
    QuestId = 1234
};
```

### Error Codes
| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `NOT_IN_PARTY` | Nicht in Party | Party joinen |
| `QUEST_NOT_SHARABLE` | Quest ist nicht teilbar | Ignorieren |
| `PARTY_MEMBER_HAS_QUEST` | Alle haben Quest bereits | Ignorieren |

---

## QuestShareResponse (1012)

**Richtung:** 📥 Server → Client (Party-Member)  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Prompt an Party-Member dass Quest geshared wurde. Party-Member können Accept/Decline.

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| QuestId | uint | Geteilte Quest-ID | Ja |
| SharingPlayerName | string | Name des teilenden Spielers | Ja |
| QuestTitle | string | Quest-Titel | Ja |
| QuestLevel | int | Quest-Level | Ja |
| ExpiryTime | long | Unix Timestamp (Ablauf des Prompts) | Ja |

### Beispiel Payload
```csharp
var shareResponse = new QuestShareResponse
{
    Type = MessageType.QuestShareResponse,
    QuestId = 1234,
    SharingPlayerName = "Aragorn",
    QuestTitle = "Defeat the Goblins",
    QuestLevel = 10,
    ExpiryTime = DateTimeOffset.UtcNow.AddMinutes(2).ToUnixTimeSeconds()
};
```

### Notizen
- **Prompt**: Client zeigt Accept/Decline Dialog
- **Timeout**: 2 Minuten zum Accept
- **Auto-Decline**: Bei Questlog full oder Prerequisites not met

---

## QuestTrack (1013)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Markiert eine Quest als "tracked" (im HUD angezeigt).

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| QuestId | uint | Quest-ID zum tracken | Ja |

### Notizen
- **Client-Side**: Tracking-Status ist client-seitig (kein Server-Sync nötig)
- **Alternativ**: Kann rein client-seitig implementiert werden

---

## QuestUntrack (1014)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Entfernt "tracked" Status von Quest.

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| QuestId | uint | Quest-ID zum untracken | Ja |

---

## QuestObjectiveUpdate (1015)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Update für einzelnes Objective (Alternative zu vollständigem `QuestProgress`).

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| QuestId | uint | Quest-ID | Ja |
| ObjectiveIndex | byte | Index des Objectives | Ja |
| Current | int | Aktueller Fortschritt | Ja |
| Required | int | Benötigt | Ja |
| Completed | bool | Completed? | Ja |

### Beispiel Payload
```csharp
var objectiveUpdate = new QuestObjectiveUpdate
{
    Type = MessageType.QuestObjectiveUpdate,
    QuestId = 1234,
    ObjectiveIndex = 0,
    Current = 6,
    Required = 10,
    Completed = false
};
```

---

## QuestPoiRequest (1016)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Fordert POI (Point of Interest) Marker für Quest-Objectives an (Map-Marker).

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| QuestId | uint | Quest-ID | Ja |

### Erwartete Response
- **Immer:** `QuestPoiResponse` (1017)

---

## QuestPoiResponse (1017)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung
POI-Marker für Quest-Objectives (Map-Positionen).

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| QuestId | uint | Quest-ID | Ja |
| Pois | List<QuestPoi> | POI-Marker | Ja |

**QuestPoi**:
| Feld | Typ | Beschreibung |
|------|-----|--------------|
| ObjectiveIndex | byte | Zugehöriges Objective |
| ZoneId | int | Zone-ID |
| X | float | Position X |
| Y | float | Position Y |
| Type | string | "objective", "turn_in", "quest_giver" |

### Beispiel Payload
```csharp
var poiResponse = new QuestPoiResponse
{
    Type = MessageType.QuestPoiResponse,
    QuestId = 1234,
    Pois = new List<QuestPoi>
    {
        new QuestPoi
        {
            ObjectiveIndex = 0,
            ZoneId = 1001,
            X = 150.0f,
            Y = 200.0f,
            Type = "objective"
        },
        new QuestPoi
        {
            ObjectiveIndex = 0,
            ZoneId = 1001,
            X = 180.0f,
            Y = 220.0f,
            Type = "turn_in"
        }
    }
};
```

---

## QuestGiverStatus (1018)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Status eines Quest-Givers (verfügbare/abschließbare Quests). Wird gesendet wenn Spieler NPC nahe kommt.

### Im Scope ✅
- Quest verfügbar (!)
- Quest abschließbar (?)
- Keine Quests (.)

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| QuestGiverId | int | Entity-ID des NPCs | Ja |
| Status | string | "available", "completable", "none" | Ja |
| AvailableQuests | List<uint> | Quest-IDs (bei available) | Nein |
| CompletableQuests | List<uint> | Quest-IDs (bei completable) | Nein |

### Beispiel Payload
```csharp
var giverStatus = new QuestGiverStatus
{
    Type = MessageType.QuestGiverStatus,
    QuestGiverId = 50001,
    Status = "available",
    AvailableQuests = new List<uint> { 1234, 1235 }
};
```

### Notizen
- **Visual**: Client zeigt ! oder ? über NPC-Kopf
- **Range**: Wird gesendet wenn Spieler in 30m Range

---

## QuestGiverList (1019)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Liste aller Quests die ein NPC anbietet. Wird gesendet nach NPC-Interact.

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| QuestGiverId | int | Entity-ID des NPCs | Ja |
| AvailableQuests | List<QuestSummary> | Verfügbare Quests | Ja |
| CompletableQuests | List<uint> | Abschließbare Quest-IDs | Ja |

**QuestSummary**:
| Feld | Typ | Beschreibung |
|------|-----|--------------|
| QuestId | uint | Quest-ID |
| Title | string | Quest-Titel |
| Level | int | Quest-Level |
| IsDaily | bool | Daily Quest? |
| IsRepeatable | bool | Repeatable? |

### Beispiel Payload
```csharp
var giverList = new QuestGiverList
{
    Type = MessageType.QuestGiverList,
    QuestGiverId = 50001,
    AvailableQuests = new List<QuestSummary>
    {
        new QuestSummary
        {
            QuestId = 1234,
            Title = "Defeat the Goblins",
            Level = 10,
            IsDaily = false,
            IsRepeatable = false
        }
    },
    CompletableQuests = new List<uint> { 1235 }
};
```

---

## DailyQuestReset (1020)

**Richtung:** 📥 Server → Client  
**Frequenz:** Täglich  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Server informiert dass Daily Quests resettet wurden. Client kann Daily Quests neu annehmen.

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| ResetTime | long | Unix Timestamp des Resets | Ja |
| NextResetTime | long | Unix Timestamp des nächsten Resets | Ja |

### Beispiel Payload
```csharp
var dailyReset = new DailyQuestReset
{
    Type = MessageType.DailyQuestReset,
    ResetTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
    NextResetTime = DateTimeOffset.UtcNow.AddDays(1).ToUnixTimeSeconds()
};
```

### Notizen
- **Reset-Zeit**: 00:00 Server-Zeit
- **UI**: Client zeigt Notification
- **Quest-Giver**: Status wird updated

---

## WeeklyQuestReset (1021)

**Richtung:** 📥 Server → Client  
**Frequenz:** Wöchentlich  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Server informiert dass Weekly Quests resettet wurden.

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| ResetTime | long | Unix Timestamp des Resets | Ja |
| NextResetTime | long | Unix Timestamp des nächsten Resets | Ja |

### Notizen
- **Reset-Zeit**: Dienstag 00:00 Server-Zeit (Standard)

---

## QuestChainUpdate (1022)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Informiert über Änderungen in einer Quest-Kette (neue Quest freigeschaltet, etc.).

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| ChainId | uint | Quest-Chain-ID | Ja |
| UnlockedQuestId | uint | Freigeschaltete Quest-ID | Ja |
| ChainProgress | int | Fortschritt in Kette (z.B. 3/10) | Ja |
| ChainTotal | int | Totale Quests in Kette | Ja |

### Beispiel Payload
```csharp
var chainUpdate = new QuestChainUpdate
{
    Type = MessageType.QuestChainUpdate,
    ChainId = 100,
    UnlockedQuestId = 1236,
    ChainProgress = 3,
    ChainTotal = 10
};
```

---

## QuestRepeatableReset (1023)

**Richtung:** 📥 Server → Client  
**Frequenz:** Variabel  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Repeatable Quest kann erneut angenommen werden (nach Cooldown).

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| QuestId | uint | Quest-ID | Ja |
| ResetTime | long | Unix Timestamp des Resets | Ja |

### Notizen
- **Cooldown**: Variiert pro Quest (z.B. 1 Stunde)
- **UI**: Client zeigt dass Quest wieder verfügbar ist

---

## 🗑️ Obsolete Messages

*Derzeit keine obsoleten Messages in dieser Kategorie.*

---

## 📎 Anhang

### MessageType Enum (Quest-Range)

```csharp
// QUEST (1000-1099) - Category 10
QuestAccept = 1000,
QuestAcceptResult = 1001,
QuestAbandon = 1002,
QuestProgress = 1003,
QuestComplete = 1004,
QuestCompleteResult = 1005,
QuestRewardChoose = 1006,
QuestRewardReceive = 1007,
QuestListRequest = 1008,
QuestListResponse = 1009,
QuestLogUpdate = 1010,
QuestShare = 1011,
QuestShareResponse = 1012,
QuestTrack = 1013,
QuestUntrack = 1014,
QuestObjectiveUpdate = 1015,
QuestPoiRequest = 1016,
QuestPoiResponse = 1017,
QuestGiverStatus = 1018,
QuestGiverList = 1019,
DailyQuestReset = 1020,
WeeklyQuestReset = 1021,
QuestChainUpdate = 1022,
QuestRepeatableReset = 1023,
```

### Request/Response Paare

| Request | ID | Response | ID | Beschreibung |
|---------|-----|----------|-----|--------------|
| QuestAccept | 1000 | QuestAcceptResult | 1001 | Quest annehmen |
| QuestAbandon | 1002 | QuestLogUpdate | 1010 | Quest aufgeben (Update als Response) |
| QuestComplete | 1004 | QuestCompleteResult | 1005 | Quest abschließen |
| QuestRewardChoose | 1006 | QuestRewardReceive | 1007 | Belohnung wählen |
| QuestListRequest | 1008 | QuestListResponse | 1009 | Questlog anfordern |
| QuestShare | 1011 | QuestShareResponse | 1012 | Quest teilen |
| QuestTrack | 1013 | - | - | Tracking (rein client-seitig) |
| QuestUntrack | 1014 | - | - | Untracking (rein client-seitig) |
| QuestPoiRequest | 1016 | QuestPoiResponse | 1017 | POI-Marker anfordern |

### Datei-Struktur

```
shared/Mmo.Shared/Messaging/
├── Enums/
│   └── MessageType.cs          # Quest = 1000-1023
├── Messages/Quest/
│   ├── QuestAccept.cs
│   ├── QuestAcceptResult.cs
│   ├── QuestAbandon.cs
│   ├── QuestProgress.cs
│   ├── QuestComplete.cs
│   ├── QuestCompleteResult.cs
│   ├── QuestRewardChoose.cs
│   ├── QuestRewardReceive.cs
│   ├── QuestListRequest.cs
│   ├── QuestListResponse.cs
│   ├── QuestLogUpdate.cs
│   ├── QuestShare.cs
│   ├── QuestShareResponse.cs
│   ├── QuestTrack.cs
│   ├── QuestUntrack.cs
│   ├── QuestObjectiveUpdate.cs
│   ├── QuestPoiRequest.cs
│   ├── QuestPoiResponse.cs
│   ├── QuestGiverStatus.cs
│   ├── QuestGiverList.cs
│   ├── DailyQuestReset.cs
│   ├── WeeklyQuestReset.cs
│   ├── QuestChainUpdate.cs
│   └── QuestRepeatableReset.cs
└── DTOs/Quest/
    ├── QuestEntry.cs
    ├── ObjectiveProgress.cs
    ├── QuestPoi.cs
    ├── ItemReward.cs
    ├── ReputationReward.cs
    └── QuestSummary.cs
```

---

**Letzte Aktualisierung**: 2026-01-02  
**Version**: 3.0.0

[← Zurück zur Übersicht](Message-Reference.md)

Source: docs/03-messages/10-quest.md
