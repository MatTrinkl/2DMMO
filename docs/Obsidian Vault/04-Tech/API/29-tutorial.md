# 🎓 Tutorial Messages (2900-2999)

**Kategorie:** 29  
**Range:** 2900-2999  
**Phase:** Prototyp  
**Status:** 🟢 In Entwicklung

[← Zurück zur Übersicht](Message-Reference.md)

---

## 📋 Inhaltsverzeichnis

- [📋 Überblick](#-überblick)
- [🧠 Datenmodell](#-datenmodell)
- [⚡ Trigger Engine](#-trigger-engine)
- [📈 Progression & Persistence](#-progression--persistence)
- [⏭️ Skip/Replay Regeln](#️-skipreplay-regeln)
- [🧱 DTOs / Interfaces](#-dtos--interfaces)
- [🧩 Enums / ErrorCodes / Flags](#-enums--errorcodes--flags)
- [⚙️ Regeln & Validierung](#️-regeln--validierung)
- [📩 Aktive Messages 2900–2999](#-aktive-messages-29002999)
- [��️ Obsolete Messages](#️-obsolete-messages)
- [🧨 Edge Cases & Fehlerfälle](#-edge-cases--fehlerfälle)
- [📎 Anhang](#-anhang)

---

## 📋 Überblick

Das Tutorial-System im 2DMMO ist ein **server-autoritatives Onboarding-System**, das neue Spieler durch grundlegende Spielmechaniken führt.

### Ziele

- **Spieler-Onboarding**: Neue Spieler lernen Basismechaniken (Movement, Combat, Inventory, Trading)
- **Server Authority**: Gesamter Tutorial-Progress ist server-seitig persistent
- **Request→Response Pattern**: Jede Client-Aktion hat eine definierte Server-Response
- **Idempotenz**: Wiederholte Requests führen zum gleichen Ergebnis ohne Seiteneffekte
- **Resync-Fähigkeit**: Client kann jederzeit vollständigen State vom Server anfordern

### Abgrenzung

| Feature | Tutorial-System | Andere Systeme |
|---------|-----------------|----------------|
| Basic Onboarding | ✅ Tutorial | - |
| Quest-Tutorial | ❌ | Quest-System (1000-1099) |
| Ability-Learning | ❌ | Skill-System (3800-3899) |
| Achievement-Fortschritt | ❌ | Achievement-System (1900-1999) |
| Loading-Tips | ✅ TipOfTheDay | - |
| Patch-Notes | ✅ NewFeatureHighlight | - |

### Onboarding-Philosophie

1. **Non-Intrusive**: Tutorials können übersprungen werden (außer kritische)
2. **Contextual**: Hints erscheinen nur bei relevanten Aktionen
3. **Progressive**: Von einfach zu komplex, step-by-step
4. **Persistent**: Fortschritt wird pro Character gespeichert (nicht Account)
5. **Throttled**: Hint/Step Events werden debounced um Spam zu vermeiden

---

## 🧠 Datenmodell

### Tutorial-Persistenz: Pro Character

Tutorial-State wird **pro Character** (nicht pro Account) persistiert.

### TutorialDefinition (Server-seitig, statisch)

```csharp
public class TutorialDefinition
{
    public ushort TutorialId { get; init; }
    public string DisplayName { get; init; } = "";
    public TutorialCategory Category { get; init; }
    public List<TutorialStepDefinition> Steps { get; init; } = new();
    public bool CanSkip { get; init; } = true;
    public bool IsMandatory { get; init; } = false;
    public TutorialTriggerType TriggerType { get; init; }
    public int XpReward { get; init; }
    public int GoldReward { get; init; }
}
```

### PlayerTutorialState (Server-seitig, dynamisch)

```csharp
public class PlayerTutorialState
{
    public Guid CharacterId { get; init; }
    public uint Revision { get; set; } = 1;
    public Dictionary<ushort, TutorialProgress> Tutorials { get; init; } = new();
    public HashSet<uint> DismissedHintIds { get; init; } = new();
    public HashSet<string> DisabledHintCategories { get; init; } = new();
    public uint LastSeenTipId { get; set; }
    public string? LastSeenPatchVersion { get; set; }
}

public class TutorialProgress
{
    public ushort TutorialId { get; init; }
    public TutorialState State { get; set; } = TutorialState.NotStarted;
    public byte CurrentStep { get; set; } = 0;
    public bool WasSkipped { get; set; } = false;
}
```

### State Machine: TutorialState

```
NotStarted ──TutorialStart──► Active
                                │
         ┌──────────────────────┼──────────────────────┐
         │                      │                      │
    TutorialComplete       TutorialSkip           TutorialReset
         │                      │                      │
         ▼                      ▼                      │
    Completed               Skipped                    │
         │                      │                      │
         └──────────────────────┴──────────────────────┘
                                │
                                ▼
                          NotStarted (nach Reset)
```

---

## ⚡ Trigger Engine

### Server-seitige Trigger (Push)

| Trigger | Event | Tutorial/Hint |
|---------|-------|---------------|
| `ZoneEnter` | Spieler betritt Zone erstmals | TutorialStart (Basic Movement) |
| `FirstCombat` | Erstes Combat-Event | TutorialStart (Combat Basics) |
| `FirstLoot` | Erstes Item gelooted | HintShow (Inventory) |
| `QuestAccepted` | Erste Quest angenommen | TutorialStart (Quest System) |
| `LevelUp` | Level 5/10/20 erreicht | TutorialStart (Advanced Features) |

### Debounce & Rate Limiting

| Event Type | Cooldown | Max pro Minute |
|------------|----------|----------------|
| TutorialStep | 500ms | 10 |
| HintShow | 5000ms | 6 |
| TipOfTheDay | 24h | 1 |

---

## 📈 Progression & Persistence

### Revision System

Jede Änderung am Tutorial-State erhöht die `Revision`. Client und Server können so Desync erkennen.

- **Initial Load**: Vollständiger `TutorialStateSyncResponse` bei CharacterSelect
- **Laufende Updates**: Einzelne Messages (`TutorialStep`, `TutorialComplete`, etc.)
- **Resync**: Client kann jederzeit `TutorialStateSyncRequest` senden

---

## ⏭️ Skip/Replay Regeln

### Skip-Regeln

| Tutorial-Typ | Skippable | Begründung |
|--------------|-----------|------------|
| Basic Movement | ✅ Ja | Optional für erfahrene Spieler |
| Combat Basics | ✅ Ja | Optional |
| Critical Safety | ❌ Nein | z.B. Account-Sicherheit |

### Replay-Regeln

- **Reset einzelnes Tutorial**: `TutorialResetRequest` mit spezifischer `TutorialId`
- **Reset alle Tutorials**: `TutorialResetRequest` mit `TutorialId = 0`
- Belohnungen werden **nicht** erneut vergeben bei Replay

---

## 🧱 DTOs / Interfaces

### TutorialStateDto

```csharp
[MessagePackObject]
public class TutorialStateDto
{
    [Key(0)] public uint Revision { get; set; }
    [Key(1)] public List<TutorialProgressDto> Tutorials { get; set; } = new();
    [Key(2)] public List<uint> DismissedHintIds { get; set; } = new();
    [Key(3)] public List<string> DisabledHintCategories { get; set; } = new();
    [Key(4)] public uint LastSeenTipId { get; set; }
    [Key(5)] public string? LastSeenPatchVersion { get; set; }
}
```

### TutorialProgressDto

```csharp
[MessagePackObject]
public class TutorialProgressDto
{
    [Key(0)] public ushort TutorialId { get; set; }
    [Key(1)] public TutorialState State { get; set; }
    [Key(2)] public byte CurrentStep { get; set; }
    [Key(3)] public bool WasSkipped { get; set; }
}
```

### TutorialStepDto

```csharp
[MessagePackObject]
public class TutorialStepDto
{
    [Key(0)] public byte StepIndex { get; set; }
    [Key(1)] public string Title { get; set; } = "";
    [Key(2)] public string Description { get; set; } = "";
    [Key(3)] public TutorialActionType RequiredAction { get; set; }
    [Key(4)] public string? ActionParameter { get; set; }
    [Key(5)] public string? HighlightUiElement { get; set; }
    [Key(6)] public int TimeoutMs { get; set; }
}
```

### HintDto

```csharp
[MessagePackObject]
public class HintDto
{
    [Key(0)] public uint HintId { get; set; }
    [Key(1)] public string Category { get; set; } = "";
    [Key(2)] public string Title { get; set; } = "";
    [Key(3)] public string Message { get; set; } = "";
    [Key(4)] public int DurationMs { get; set; }
    [Key(5)] public string? HighlightUiElement { get; set; }
}
```

### GuideEntryDto

```csharp
[MessagePackObject]
public class GuideEntryDto
{
    [Key(0)] public uint EntryId { get; set; }
    [Key(1)] public string Category { get; set; } = "";
    [Key(2)] public string Title { get; set; } = "";
    [Key(3)] public string Content { get; set; } = "";
    [Key(4)] public int SortOrder { get; set; }
    [Key(5)] public bool IsNew { get; set; }
}
```

---

## 🧩 Enums / ErrorCodes / Flags

### TutorialState Enum

```csharp
public enum TutorialState : byte
{
    NotStarted = 0,
    Active = 1,
    Completed = 2,
    Skipped = 3
}
```

### TutorialCategory Enum

```csharp
public enum TutorialCategory : byte
{
    Movement = 1,
    Combat = 2,
    Inventory = 3,
    Quest = 4,
    Social = 5,
    Trading = 6,
    Guild = 7,
    PvP = 8,
    Crafting = 9,
    Advanced = 10
}
```

### TutorialActionType Enum

```csharp
public enum TutorialActionType : byte
{
    None = 0,
    Move = 1,
    Attack = 2,
    UseAbility = 3,
    OpenInventory = 4,
    EquipItem = 5,
    UseItem = 6,
    AcceptQuest = 7,
    CompleteQuest = 8,
    OpenChat = 9,
    SendMessage = 10,
    TargetEntity = 11,
    InteractNpc = 12,
    OpenMap = 13,
    Custom = 255
}
```

### TutorialTriggerType Enum

```csharp
public enum TutorialTriggerType : byte
{
    Manual = 0,
    ZoneEnter = 1,
    LevelUp = 2,
    FirstCombat = 3,
    FirstLoot = 4,
    QuestAccepted = 5,
    GuildJoined = 6,
    FirstTrade = 7,
    Login = 8,
    CharacterCreate = 9
}
```

### TutorialErrorCode Enum

```csharp
public enum TutorialErrorCode : byte
{
    None = 0,
    TutorialNotFound = 1,
    TutorialAlreadyCompleted = 2,
    TutorialNotActive = 3,
    CannotSkipMandatory = 4,
    InvalidStep = 5,
    CooldownActive = 6,
    HintNotFound = 7,
    HintAlreadyDismissed = 8,
    CategoryAlreadyDisabled = 9,
    GuideEntryNotFound = 10,
    RateLimited = 11,
    InvalidState = 12
}
```

---

## ⚙️ Regeln & Validierung

### Server-Validierung

| Regel | Beschreibung |
|-------|--------------|
| **Step-Validierung** | Client kann nur aktuellen Step abschließen |
| **State-Validierung** | Tutorial muss `Active` sein für StepAck |
| **Skip-Validierung** | Nur wenn `CanSkip = true` und nicht `IsMandatory` |
| **Reset-Validierung** | Nur `Completed` oder `Skipped` Tutorials können resettet werden |

### Throttling / Anti-Spam

```csharp
public const int MaxStepAcksPerMinute = 10;
public const int MaxHintDismissesPerMinute = 20;
public const int MaxResetRequestsPerHour = 5;
```

---

## 📩 Aktive Messages 2900–2999

Messages in Enum-Reihenfolge aus `MessageType.cs`.

---

## TutorialStart (2900)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Server startet ein neues Tutorial für den Spieler. Wird automatisch gesendet bei Trigger-Event.

### Im Scope ✅

- Tutorial-Initiierung durch Server-Event
- Tutorial-Metadaten (ID, Name, Steps)
- Erster Step wird mitgeliefert

### Nicht im Scope ❌

- Quest-Tutorials → Quest-System (1000-1099)
- Manuelle Tutorial-Auswahl durch Client

### Payload

| Feld | Typ | Key | Beschreibung | Pflicht |
|------|-----|-----|--------------|---------|
| Type | MessageType | 0 | `MessageType.TutorialStart` | Ja |
| TutorialId | ushort | 1 | Eindeutige Tutorial-ID | Ja |
| DisplayName | string | 2 | Lokalisierter Anzeigename | Ja |
| Category | TutorialCategory | 3 | Tutorial-Kategorie | Ja |
| TotalSteps | byte | 4 | Gesamtanzahl Steps | Ja |
| CanSkip | bool | 5 | Kann übersprungen werden? | Ja |
| FirstStep | TutorialStepDto | 6 | Erster Tutorial-Step | Ja |
| Revision | uint | 7 | State-Revision | Ja |

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.TutorialStart)]
public class TutorialStart : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.TutorialStart;
    [Key(1)] public ushort TutorialId { get; init; }
    [Key(2)] public string DisplayName { get; init; } = "";
    [Key(3)] public TutorialCategory Category { get; init; }
    [Key(4)] public byte TotalSteps { get; init; }
    [Key(5)] public bool CanSkip { get; init; }
    [Key(6)] public required TutorialStepDto FirstStep { get; init; }
    [Key(7)] public uint Revision { get; init; }
}
```

### Server-Verhalten

1. Trigger-Event tritt ein
2. Prüfen ob Tutorial bereits `Completed` oder `Skipped`
3. Wenn `NotStarted` → auf `Active` setzen
4. `TutorialStart` an Client senden

### Client-Verhalten

1. Tutorial-UI öffnen
2. Step-Informationen anzeigen
3. UI-Element highlighten (falls angegeben)
4. Auf Spieler-Aktion warten

### Flow-Diagramm

```
Server                             Client
  │                                   │
  │  [Trigger: Zone Enter]            │
  │                                   │
  │  TutorialStart (2900)             │
  │──────────────────────────────────►│
  │                                   │  [Show Tutorial UI]
  │                                   │
  │  TutorialStepAck (2907)           │
  │◄──────────────────────────────────│
  │                                   │
  │  TutorialStep (2901)              │
  │──────────────────────────────────►│
```

### Beispiel Payload

```csharp
var tutorialStart = new TutorialStart
{
    TutorialId = 1,
    DisplayName = "Basic Movement",
    Category = TutorialCategory.Movement,
    TotalSteps = 5,
    CanSkip = true,
    FirstStep = new TutorialStepDto
    {
        StepIndex = 0,
        Title = "Welcome!",
        Description = "Use WASD to move around.",
        RequiredAction = TutorialActionType.Move
    },
    Revision = 42
};
```

### Verwandte Messages

| Message | ID | Beziehung |
|---------|-----|-----------|
| `TutorialStep` | 2901 | Folge-Step |
| `TutorialStepAck` | 2907 | Client-Bestätigung |
| `TutorialSkipRequest` | 2903 | Überspringen |
| `TutorialComplete` | 2902 | Abschluss |

### Notizen

- `FirstStep` ist immer enthalten um Round-Trip zu sparen
- `Revision` ermöglicht Desync-Detection

---

## TutorialStep (2901)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Server sendet nächsten Tutorial-Step nach Client-Acknowledgment.

### Im Scope ✅

- Nächster Step im Tutorial
- Neue Instruktionen und Highlight

### Nicht im Scope ❌

- Rückwärts-Navigation
- Step-Überspringen

### Payload

| Feld | Typ | Key | Beschreibung | Pflicht |
|------|-----|-----|--------------|---------|
| Type | MessageType | 0 | `MessageType.TutorialStep` | Ja |
| TutorialId | ushort | 1 | Tutorial-ID | Ja |
| Step | TutorialStepDto | 2 | Step-Details | Ja |
| Revision | uint | 3 | State-Revision | Ja |

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.TutorialStep)]
public class TutorialStep : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.TutorialStep;
    [Key(1)] public ushort TutorialId { get; init; }
    [Key(2)] public required TutorialStepDto Step { get; init; }
    [Key(3)] public uint Revision { get; init; }
}
```

### Verwandte Messages

| Message | ID | Beziehung |
|---------|-----|-----------|
| `TutorialStart` | 2900 | Enthält ersten Step |
| `TutorialStepAck` | 2907 | Bestätigung vom Client |
| `TutorialComplete` | 2902 | Nach letztem Step |

---

## TutorialComplete (2902)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Server bestätigt erfolgreichen Abschluss eines Tutorials mit optionalen Belohnungen.

### Im Scope ✅

- Tutorial-Abschluss Bestätigung
- XP- und Gold-Belohnungen

### Nicht im Scope ❌

- Item-Belohnungen → separate Message
- Achievement-Unlock → Achievement-System

### Payload

| Feld | Typ | Key | Beschreibung | Pflicht |
|------|-----|-----|--------------|---------|
| Type | MessageType | 0 | `MessageType.TutorialComplete` | Ja |
| TutorialId | ushort | 1 | Abgeschlossenes Tutorial | Ja |
| XpReward | int | 2 | Bonus-XP (0 wenn keine) | Ja |
| GoldReward | int | 3 | Bonus-Gold (0 wenn keine) | Ja |
| Revision | uint | 4 | State-Revision | Ja |

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.TutorialComplete)]
public class TutorialComplete : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.TutorialComplete;
    [Key(1)] public ushort TutorialId { get; init; }
    [Key(2)] public int XpReward { get; init; }
    [Key(3)] public int GoldReward { get; init; }
    [Key(4)] public uint Revision { get; init; }
}
```

### Beispiel Payload

```csharp
var tutorialComplete = new TutorialComplete
{
    TutorialId = 1,
    XpReward = 100,
    GoldReward = 50,
    Revision = 50
};
```

---

## TutorialSkipRequest (2903)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Client möchte aktives Tutorial überspringen.

### Im Scope ✅

- Tutorial überspringen
- Confirmation vom Client

### Nicht im Scope ❌

- Mandatory Tutorials überspringen

### Payload

| Feld | Typ | Key | Beschreibung | Pflicht |
|------|-----|-----|--------------|---------|
| Type | MessageType | 0 | `MessageType.TutorialSkipRequest` | Ja |
| TutorialId | ushort | 1 | Zu überspringendes Tutorial | Ja |

### Erwartete Response

- `TutorialSkipResponse` (2904)

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.TutorialSkipRequest)]
public class TutorialSkipRequest : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.TutorialSkipRequest;
    [Key(1)] public ushort TutorialId { get; init; }
}
```

### Error Codes

| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `TutorialNotFound` | Tutorial existiert nicht | Ignorieren |
| `TutorialNotActive` | Tutorial nicht aktiv | UI aktualisieren |
| `CannotSkipMandatory` | Pflicht-Tutorial | UI-Hinweis |

### Verwandte Messages

| Message | ID | Beziehung |
|---------|-----|-----------|
| `TutorialSkipResponse` | 2904 | Response |
| `TutorialStart` | 2900 | Aktuelles Tutorial |

---

## TutorialSkipResponse (2904)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Server-Antwort auf Skip-Request.

### Im Scope ✅

- Erfolgs-/Fehlerstatus
- Error-Code bei Fehler

### Payload

| Feld | Typ | Key | Beschreibung | Pflicht |
|------|-----|-----|--------------|---------|
| Type | MessageType | 0 | `MessageType.TutorialSkipResponse` | Ja |
| Success | bool | 1 | Skip erfolgreich? | Ja |
| GlobalError | GlobalErrorCode? | 2 | Globaler Fehlercode | Nein |
| ErrorMessage | string? | 3 | Fehlermeldung | Nein |
| ErrorCode | TutorialErrorCode? | 4 | Spezifischer Fehlercode | Nein |
| TutorialId | ushort | 5 | Tutorial-ID | Ja |
| Revision | uint | 6 | State-Revision | Ja |

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.TutorialSkipResponse)]
public class TutorialSkipResponse : IResponseMessage<TutorialErrorCode>
{
    [Key(0)] public MessageType Type => MessageType.TutorialSkipResponse;
    [Key(1)] public bool Success { get; init; }
    [Key(2)] public GlobalErrorCode? GlobalError { get; init; }
    [Key(3)] public string? ErrorMessage { get; init; }
    [Key(4)] public TutorialErrorCode? ErrorCode { get; init; }
    [Key(5)] public ushort TutorialId { get; init; }
    [Key(6)] public uint Revision { get; init; }
}
```

---

## TutorialResetRequest (2905)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Client möchte Tutorial zurücksetzen (für erneutes Durchspielen).

### Payload

| Feld | Typ | Key | Beschreibung | Pflicht |
|------|-----|-----|--------------|---------|
| Type | MessageType | 0 | `MessageType.TutorialResetRequest` | Ja |
| TutorialId | ushort | 1 | Tutorial-ID (0 = alle) | Ja |

### Erwartete Response

- `TutorialResetResponse` (2906)

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.TutorialResetRequest)]
public class TutorialResetRequest : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.TutorialResetRequest;
    [Key(1)] public ushort TutorialId { get; init; }
}
```

---

## TutorialResetResponse (2906)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** �� Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Server-Antwort auf Reset-Request.

### Payload

| Feld | Typ | Key | Beschreibung | Pflicht |
|------|-----|-----|--------------|---------|
| Type | MessageType | 0 | `MessageType.TutorialResetResponse` | Ja |
| Success | bool | 1 | Reset erfolgreich? | Ja |
| GlobalError | GlobalErrorCode? | 2 | Globaler Fehlercode | Nein |
| ErrorMessage | string? | 3 | Fehlermeldung | Nein |
| ErrorCode | TutorialErrorCode? | 4 | Spezifischer Fehlercode | Nein |
| TutorialId | ushort | 5 | Tutorial-ID | Ja |
| Revision | uint | 6 | State-Revision | Ja |

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.TutorialResetResponse)]
public class TutorialResetResponse : IResponseMessage<TutorialErrorCode>
{
    [Key(0)] public MessageType Type => MessageType.TutorialResetResponse;
    [Key(1)] public bool Success { get; init; }
    [Key(2)] public GlobalErrorCode? GlobalError { get; init; }
    [Key(3)] public string? ErrorMessage { get; init; }
    [Key(4)] public TutorialErrorCode? ErrorCode { get; init; }
    [Key(5)] public ushort TutorialId { get; init; }
    [Key(6)] public uint Revision { get; init; }
}
```

---

## TutorialStepAck (2907)

**Richtung:** 📤 Client → Server  
**Frequenz:** Mittel  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Client bestätigt Abschluss einer Tutorial-Aktion. Server validiert und sendet nächsten Step.

### Im Scope ✅

- Bestätigung einer Aktion
- Step-Progression

### Nicht im Scope ❌

- Direktes Step-Überspringen

### Payload

| Feld | Typ | Key | Beschreibung | Pflicht |
|------|-----|-----|--------------|---------|
| Type | MessageType | 0 | `MessageType.TutorialStepAck` | Ja |
| TutorialId | ushort | 1 | Tutorial-ID | Ja |
| StepIndex | byte | 2 | Abgeschlossener Step | Ja |

### Erwartete Response

- `TutorialStep` (2901) bei weiterem Step
- `TutorialComplete` (2902) bei letztem Step

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.TutorialStepAck)]
public class TutorialStepAck : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.TutorialStepAck;
    [Key(1)] public ushort TutorialId { get; init; }
    [Key(2)] public byte StepIndex { get; init; }
}
```

### Server-Verhalten

1. Validiere dass `StepIndex` == `CurrentStep`
2. Validiere dass Tutorial `Active` ist
3. Inkrementiere `CurrentStep`
4. Wenn letzter Step → `TutorialComplete` senden
5. Sonst → `TutorialStep` mit nächstem Step

---

## TutorialStateSyncRequest (2908)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Client fordert vollständigen Tutorial-State an (Resync).

### Payload

| Feld | Typ | Key | Beschreibung | Pflicht |
|------|-----|-----|--------------|---------|
| Type | MessageType | 0 | `MessageType.TutorialStateSyncRequest` | Ja |
| ClientRevision | uint | 1 | Lokale Revision des Clients | Ja |

### Erwartete Response

- `TutorialStateSyncResponse` (2909)

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.TutorialStateSyncRequest)]
public class TutorialStateSyncRequest : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.TutorialStateSyncRequest;
    [Key(1)] public uint ClientRevision { get; init; }
}
```

---

## TutorialStateSyncResponse (2909)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Server sendet vollständigen Tutorial-State.

### Payload

| Feld | Typ | Key | Beschreibung | Pflicht |
|------|-----|-----|--------------|---------|
| Type | MessageType | 0 | `MessageType.TutorialStateSyncResponse` | Ja |
| State | TutorialStateDto | 1 | Vollständiger State | Ja |

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.TutorialStateSyncResponse)]
public class TutorialStateSyncResponse : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.TutorialStateSyncResponse;
    [Key(1)] public required TutorialStateDto State { get; init; }
}
```

---

## HintShow (2910)

**Richtung:** 📥 Server → Client  
**Frequenz:** Mittel  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Server zeigt kontextuellen Hint an.

### Im Scope ✅

- Kontextuelle Tipps
- UI-Highlight
- Auto-Dismiss nach Duration

### Nicht im Scope ❌

- Persistente Tutorial-Steps

### Payload

| Feld | Typ | Key | Beschreibung | Pflicht |
|------|-----|-----|--------------|---------|
| Type | MessageType | 0 | `MessageType.HintShow` | Ja |
| Hint | HintDto | 1 | Hint-Details | Ja |

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.HintShow)]
public class HintShow : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.HintShow;
    [Key(1)] public required HintDto Hint { get; init; }
}
```

### Beispiel Payload

```csharp
var hintShow = new HintShow
{
    Hint = new HintDto
    {
        HintId = 101,
        Category = "inventory",
        Title = "Inventory Full",
        Message = "Your inventory is full. Sell or destroy items.",
        DurationMs = 5000,
        HighlightUiElement = "InventoryButton"
    }
};
```

---

## HintDismissRequest (2911)

**Richtung:** 📤 Client → Server  
**Frequenz:** Mittel  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Client dismisst Hint manuell.

### Payload

| Feld | Typ | Key | Beschreibung | Pflicht |
|------|-----|-----|--------------|---------|
| Type | MessageType | 0 | `MessageType.HintDismissRequest` | Ja |
| HintId | uint | 1 | Hint-ID | Ja |

### Erwartete Response

- `HintDismissResponse` (2912)

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.HintDismissRequest)]
public class HintDismissRequest : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.HintDismissRequest;
    [Key(1)] public uint HintId { get; init; }
}
```

---

## HintDismissResponse (2912)

**Richtung:** 📥 Server → Client  
**Frequenz:** Mittel  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Server bestätigt Hint-Dismiss.

### Payload

| Feld | Typ | Key | Beschreibung | Pflicht |
|------|-----|-----|--------------|---------|
| Type | MessageType | 0 | `MessageType.HintDismissResponse` | Ja |
| Success | bool | 1 | Dismiss erfolgreich? | Ja |
| GlobalError | GlobalErrorCode? | 2 | Globaler Fehlercode | Nein |
| ErrorMessage | string? | 3 | Fehlermeldung | Nein |
| ErrorCode | TutorialErrorCode? | 4 | Spezifischer Fehlercode | Nein |
| HintId | uint | 5 | Hint-ID | Ja |

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.HintDismissResponse)]
public class HintDismissResponse : IResponseMessage<TutorialErrorCode>
{
    [Key(0)] public MessageType Type => MessageType.HintDismissResponse;
    [Key(1)] public bool Success { get; init; }
    [Key(2)] public GlobalErrorCode? GlobalError { get; init; }
    [Key(3)] public string? ErrorMessage { get; init; }
    [Key(4)] public TutorialErrorCode? ErrorCode { get; init; }
    [Key(5)] public uint HintId { get; init; }
}
```

---

## HintDisableRequest (2913)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Client deaktiviert Hint-Category permanent.

### Payload

| Feld | Typ | Key | Beschreibung | Pflicht |
|------|-----|-----|--------------|---------|
| Type | MessageType | 0 | `MessageType.HintDisableRequest` | Ja |
| Category | string | 1 | Hint-Category | Ja |

### Erwartete Response

- `HintDisableResponse` (2914)

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.HintDisableRequest)]
public class HintDisableRequest : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.HintDisableRequest;
    [Key(1)] public string Category { get; init; } = "";
}
```

---

## HintDisableResponse (2914)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Server bestätigt Category-Deaktivierung.

### Payload

| Feld | Typ | Key | Beschreibung | Pflicht |
|------|-----|-----|--------------|---------|
| Type | MessageType | 0 | `MessageType.HintDisableResponse` | Ja |
| Success | bool | 1 | Erfolgreich? | Ja |
| GlobalError | GlobalErrorCode? | 2 | Globaler Fehlercode | Nein |
| ErrorMessage | string? | 3 | Fehlermeldung | Nein |
| ErrorCode | TutorialErrorCode? | 4 | Spezifischer Fehlercode | Nein |
| Category | string | 5 | Category | Ja |

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.HintDisableResponse)]
public class HintDisableResponse : IResponseMessage<TutorialErrorCode>
{
    [Key(0)] public MessageType Type => MessageType.HintDisableResponse;
    [Key(1)] public bool Success { get; init; }
    [Key(2)] public GlobalErrorCode? GlobalError { get; init; }
    [Key(3)] public string? ErrorMessage { get; init; }
    [Key(4)] public TutorialErrorCode? ErrorCode { get; init; }
    [Key(5)] public string Category { get; init; } = "";
}
```

---

## TipOfTheDay (2920)

**Richtung:** 📥 Server → Client  
**Frequenz:** Täglich (einmal pro Login)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Server sendet "Tip of the Day" bei Login.

### Payload

| Feld | Typ | Key | Beschreibung | Pflicht |
|------|-----|-----|--------------|---------|
| Type | MessageType | 0 | `MessageType.TipOfTheDay` | Ja |
| TipId | uint | 1 | Tip-ID | Ja |
| Title | string | 2 | Tip-Titel | Ja |
| Message | string | 3 | Tip-Text | Ja |

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.TipOfTheDay)]
public class TipOfTheDay : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.TipOfTheDay;
    [Key(1)] public uint TipId { get; init; }
    [Key(2)] public string Title { get; init; } = "";
    [Key(3)] public string Message { get; init; } = "";
}
```

---

## NewFeatureHighlight (2921)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten (nach Patch)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Server highlighted neue Features nach Patch.

### Payload

| Feld | Typ | Key | Beschreibung | Pflicht |
|------|-----|-----|--------------|---------|
| Type | MessageType | 0 | `MessageType.NewFeatureHighlight` | Ja |
| FeatureId | uint | 1 | Feature-ID | Ja |
| Title | string | 2 | Feature-Name | Ja |
| Description | string | 3 | Beschreibung | Ja |
| PatchVersion | string | 4 | Patch-Version | Ja |

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.NewFeatureHighlight)]
public class NewFeatureHighlight : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.NewFeatureHighlight;
    [Key(1)] public uint FeatureId { get; init; }
    [Key(2)] public string Title { get; init; } = "";
    [Key(3)] public string Description { get; init; } = "";
    [Key(4)] public string PatchVersion { get; init; } = "";
}
```

---

## GuideOpenRequest (2930)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Client öffnet In-Game Guide/Help-System.

### Payload

| Feld | Typ | Key | Beschreibung | Pflicht |
|------|-----|-----|--------------|---------|
| Type | MessageType | 0 | `MessageType.GuideOpenRequest` | Ja |
| Category | string | 1 | Optional: Kategorie-Filter | Nein |

### Erwartete Response

- `GuideOpenResponse` (2931)

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.GuideOpenRequest)]
public class GuideOpenRequest : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.GuideOpenRequest;
    [Key(1)] public string? Category { get; init; }
}
```

---

## GuideOpenResponse (2931)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Server sendet Guide-Einträge.

### Payload

| Feld | Typ | Key | Beschreibung | Pflicht |
|------|-----|-----|--------------|---------|
| Type | MessageType | 0 | `MessageType.GuideOpenResponse` | Ja |
| Entries | List&lt;GuideEntryDto&gt; | 1 | Guide-Einträge | Ja |
| Categories | List&lt;string&gt; | 2 | Verfügbare Kategorien | Ja |

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.GuideOpenResponse)]
public class GuideOpenResponse : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.GuideOpenResponse;
    [Key(1)] public List<GuideEntryDto> Entries { get; init; } = new();
    [Key(2)] public List<string> Categories { get; init; } = new();
}
```

---

## GuideCloseRequest (2932)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Client schließt Guide-UI. Für Analytics/Tracking.

### Payload

| Feld | Typ | Key | Beschreibung | Pflicht |
|------|-----|-----|--------------|---------|
| Type | MessageType | 0 | `MessageType.GuideCloseRequest` | Ja |

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.GuideCloseRequest)]
public class GuideCloseRequest : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.GuideCloseRequest;
}
```

### Notizen

- Keine explizite Response erforderlich (Fire-and-Forget für Analytics)

---

## GuideProgress (2933)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Server sendet Guide-Progress Update (z.B. neue Einträge verfügbar).

### Payload

| Feld | Typ | Key | Beschreibung | Pflicht |
|------|-----|-----|--------------|---------|
| Type | MessageType | 0 | `MessageType.GuideProgress` | Ja |
| NewEntryIds | List&lt;uint&gt; | 1 | IDs neuer Einträge | Ja |
| UpdatedCategories | List&lt;string&gt; | 2 | Aktualisierte Kategorien | Ja |

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.GuideProgress)]
public class GuideProgress : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.GuideProgress;
    [Key(1)] public List<uint> NewEntryIds { get; init; } = new();
    [Key(2)] public List<string> UpdatedCategories { get; init; } = new();
}
```

---

## 🗑️ Obsolete Messages

### TutorialSkip (alt: 2903)

> ⚠️ **OBSOLET** - Umbenannt zu `TutorialSkipRequest`

### TutorialReset (alt: 2904)

> ⚠️ **OBSOLET** - Umbenannt zu `TutorialResetRequest`

### TutorialFlag (alt: 2905)

> ⚠️ **OBSOLET** - Entfernt. Funktionalität in `TutorialStateSyncResponse` integriert.

### HintDismiss (alt: 2911)

> ⚠️ **OBSOLET** - Umbenannt zu `HintDismissRequest`

### HintDisable (alt: 2912)

> ⚠️ **OBSOLET** - Umbenannt zu `HintDisableRequest`

### GuideOpen (alt: 2930)

> ⚠️ **OBSOLET** - Umbenannt zu `GuideOpenRequest`

### GuideClose (alt: 2931)

> ⚠️ **OBSOLET** - Umbenannt zu `GuideCloseRequest`

---

## 🧨 Edge Cases & Fehlerfälle

### Tutorial während Disconnect

| Szenario | Verhalten |
|----------|-----------|
| Disconnect während Tutorial | State bleibt persistent, Resume bei Reconnect |
| Reconnect | `TutorialStateSyncResponse` sendet aktuellen State |

### Gleichzeitige Tutorials

| Szenario | Verhalten |
|----------|-----------|
| Zwei Tutorials gleichzeitig | Server queued zweites Tutorial |
| Tutorial während Hint | Beide werden angezeigt |

### Rate Limiting Überschreitung

| Szenario | Verhalten |
|----------|-----------|
| Zu viele StepAcks | `RateLimited` Error, 60s Cooldown |
| Zu viele Resets | `RateLimited` Error, 1h Cooldown |

### Invalid State Transitions

| Szenario | Verhalten |
|----------|-----------|
| Skip auf `Completed` | `TutorialAlreadyCompleted` Error |
| StepAck auf `NotStarted` | `TutorialNotActive` Error |
| Reset auf `Active` | `InvalidState` Error |

---

## 📎 Anhang

### MessageType Enum Updates

Die folgenden Änderungen wurden am `MessageType.cs` Enum vorgenommen:

```csharp
// TUTORIAL / GUIDE SYSTEM (2900-2999)
TutorialStart = 2900,
TutorialStep = 2901,
TutorialComplete = 2902,
TutorialSkipRequest = 2903,      // Umbenannt von TutorialSkip
TutorialSkipResponse = 2904,     // NEU
TutorialResetRequest = 2905,     // Umbenannt von TutorialReset
TutorialResetResponse = 2906,    // NEU
TutorialStepAck = 2907,          // NEU
TutorialStateSyncRequest = 2908, // NEU
TutorialStateSyncResponse = 2909,// NEU
HintShow = 2910,
HintDismissRequest = 2911,       // Umbenannt von HintDismiss
HintDismissResponse = 2912,      // NEU
HintDisableRequest = 2913,       // Umbenannt von HintDisable
HintDisableResponse = 2914,      // NEU
TipOfTheDay = 2920,
NewFeatureHighlight = 2921,
GuideOpenRequest = 2930,         // Umbenannt von GuideOpen
GuideOpenResponse = 2931,        // NEU
GuideCloseRequest = 2932,        // Umbenannt von GuideClose
GuideProgress = 2933,
```

### Integrationshinweise

1. **CharacterSelect**: Nach `CharacterSelectResponse` wird automatisch `TutorialStateSyncResponse` gesendet
2. **Zone Enter**: Server prüft auf ausstehende Tutorials bei Zone-Wechsel
3. **Level Up**: Server prüft auf Level-basierte Tutorials
4. **Quest Accept**: Server prüft auf Quest-Tutorial-Trigger

---

**Letzte Aktualisierung**: 2026-01-02  
**Version**: 3.0.0

[← Zurück zur Übersicht](Message-Reference.md)

Source: docs/03-messages/29-tutorial.md
