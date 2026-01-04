# 🎯 Targeting Messages (1200-1225)

**Kategorie:** 12  
**Range:** 1200-1225  
**Phase:** Prototyp  
**Status:** 🟢 In Entwicklung

[← Zurück zur Übersicht](Message-Reference.md)

---

## 📋 Inhaltsverzeichnis

-   [TargetSelect (1200)](#targetselect-1200)
-   [TargetClear (1201)](#targetclear-1201)
-   [TargetUpdate (1202)](#targetupdate-1202)
-   [TargetInfoRequest (1203)](#targetinforequest-1203)
-   [TargetInfoResponse (1204)](#targetinforesponse-1204)
-   [TargetOfTarget (1205)](#targetoftarget-1205)
-   [TargetOfTargetUpdate (1206)](#targetoftargetupdate-1206)
-   [FocusTarget (1207)](#focustarget-1207)
-   [FocusClear (1208)](#focusclear-1208)
-   [AssistTarget (1209)](#assisttarget-1209)
-   [MarkTarget (1210)](#marktarget-1210)
-   [MarkClear (1211)](#markclear-1211)
-   [MarkClearAll (1212)](#markclearall-1212)
-   [MouseoverTarget (1213)](#mouseovertarget-1213)
-   [TabTarget (1214)](#tabtarget-1214)
-   [NearestEnemyTarget (1215)](#nearestenemytarget-1215)
-   [NearestFriendTarget (1216)](#nearestfriendtarget-1216)
-   [TargetSelectResponse (1220)](#targetselectresponse-1220)
-   [AssistTargetResponse (1221)](#assisttargetresponse-1221)
-   [MarkTargetResponse (1222)](#marktargetresponse-1222)
-   [TabTargetResponse (1223)](#tabtargetresponse-1223)
-   [NearestEnemyTargetResponse (1224)](#nearestenemytargetresponse-1224)
-   [NearestFriendTargetResponse (1225)](#nearestfriendtargetresponse-1225)

---

## 🔄 Targeting Flow

### Server-Authority Architecture

```
┌─────────────────────────────────────────────────────────────────────┐
│                     TARGETING SYSTEM                                 │
│                                                                     │
│  Client-Side                                Server-Side              │
│  ┌────────────────┐                        ┌────────────────┐       │
│  │ Target-Select  │──TargetSelect(1200)───►│ Validate/Track │       │
│  │ (Click/Tab/Key)│                        │ Target for     │       │
│  │                │◄─TargetSelectResponse──│ Abilities      │       │
│  └────────────────┘                        └────────────────┘       │
│                                                                     │
│  ┌────────────────┐                        ┌────────────────┐       │
│  │ Target-Frame   │◄──TargetInfoResponse───│ Entity-Info    │       │
│  │ UI Update      │   (Health, Name, etc)  │ Provider       │       │
│  └────────────────┘                        └────────────────┘       │
│                                                                     │
│  ┌────────────────┐                        ┌────────────────┐       │
│  │ ToT-Frame      │◄─TargetOfTargetUpdate──│ ToT-Tracker    │       │
│  │ UI Update      │                        │                │       │
│  └────────────────┘                        └────────────────┘       │
└─────────────────────────────────────────────────────────────────────┘
```

### Target Select + Info Flow

```
Client                        Zone Server                  Entity
  │                               │                          │
  │  TargetSelect (1200)          │                          │
  │  { EntityId: 50001 }          │                          │
  │──────────────────────────────►│                          │
  │                               │  Validate Entity exists  │
  │                               │─────────────────────────►│
  │                               │                          │
  │                               │  Entity Data             │
  │                               │◄─────────────────────────│
  │                               │                          │
  │  TargetSelectResponse (1220)  │  Track Target for Player │
  │  { Success: true }            │                          │
  │◄──────────────────────────────│                          │
  │                               │                          │
  │  TargetUpdate (1202)          │                          │
  │  { EntityId: 50001 }          │                          │
  │◄──────────────────────────────│                          │
  │                               │                          │
  │  TargetInfoResponse (1204)    │                          │
  │  { Name, Level, Health... }   │                          │
  │◄──────────────────────────────│                          │
```

### Assist Flow (Group Coordination)

```
Player A (Tank)              Zone Server              Player B (DPS)
  │                               │                          │
  │  TargetSelect { Boss }        │                          │
  │──────────────────────────────►│                          │
  │                               │                          │
  │◄── TargetUpdate ──────────────│                          │
  │                               │                          │
  │                               │  AssistTarget (1209)     │
  │                               │  { AssistEntityId: A }   │
  │                               │◄─────────────────────────│
  │                               │                          │
  │                               │  AssistTargetResponse    │
  │                               │  { AssistTargetId: Boss }│
  │                               │─────────────────────────►│
  │                               │                          │
  │                               │  TargetUpdate            │
  │                               │  { EntityId: Boss }      │
  │                               │─────────────────────────►│
```

---

## 🧱 DTOs / Enums / Interfaces

### TargetEntityDto

```csharp
/// <summary>
/// Target entity information for Target-Frame UI
/// </summary>
[MessagePackObject]
public class TargetEntityDto
{
    [Key(0)] public int EntityId { get; set; }
    [Key(1)] public string Name { get; set; } = string.Empty;
    [Key(2)] public int Level { get; set; }
    [Key(3)] public EntityType EntityType { get; set; }
    [Key(4)] public int Health { get; set; }
    [Key(5)] public int MaxHealth { get; set; }
    [Key(6)] public float HealthPercent { get; set; }
    [Key(7)] public string Faction { get; set; } = string.Empty;
    [Key(8)] public bool IsHostile { get; set; }
    [Key(9)] public bool IsDead { get; set; }
    [Key(10)] public string? Title { get; set; }
    [Key(11)] public string? GuildName { get; set; }
}
```

### MarkType (Raid Markers)

```csharp
/// <summary>
/// Raid marker icons for target coordination
/// </summary>
public enum MarkType : byte
{
    None = 0,
    Skull = 1,      // Kill first
    Cross = 2,      // Kill second
    Square = 3,     // CC / Ice Trap
    Moon = 4,       // CC / Polymorph
    Triangle = 5,   // CC / Sap
    Diamond = 6,    // Tank position
    Circle = 7,     // Healer position
    Star = 8        // Special
}
```

### TargetErrorCode

```csharp
/// <summary>
/// Error codes for targeting operations
/// </summary>
public enum TargetErrorCode
{
    None = 0,
    EntityNotFound = 1,
    OutOfRange = 2,
    NoTarget = 3,
    NoTargetsInRange = 4,
    NoEnemiesInRange = 5,
    NoFriendsInRange = 6,
    NotInParty = 7,
    NotPartyLeader = 8
}
```

### Targeting Constants

| Konstante | Wert | Beschreibung |
|-----------|------|--------------|
| `MAX_TARGET_RANGE` | 100f | Max. Targeting-Distanz (Units) |
| `TAB_TARGET_ANGLE` | 90° | Winkel für Tab-Target-Suche |
| `MOUSEOVER_RATE_LIMIT` | 20/s | Max. Mouseover-Updates |
| `TARGET_INFO_CACHE_TTL` | 1s | Cache-Dauer für Target-Info |
| `MARK_ICONS_COUNT` | 8 | Anzahl verfügbarer Marker |

---

## 📋 Übersicht

Diese Kategorie umfasst alle Messages für **Targeting** Funktionalität im 2DMMO.

Das Targeting-System implementiert:

-   Target-Selektion (Click, Tab, Nearest, Mouseover)
-   Target-Information Abfrage
-   Focus-Target (sekundäres Target)
-   Target-of-Target (ToT) Mechanik
-   Raid-Marker und Target-Marking
-   Assist-Functionality für Gruppen

**Server Authority**: Target-Selection ist client-initiiert, Server validiert und trackt Target für Abilities.

---

## TargetSelect (1200)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Client teilt Server mit dass Entity targetiert wurde. Server sendet Target-Info zurück und trackt Target für Abilities.

### Im Scope ✅

-   Entity targetieren (Spieler, NPC, Object)
-   Target-Info-Request
-   Server-seitige Target-Tracking

### Nicht im Scope ❌

-   Auto-Targeting → verwende `TabTarget` (1214) oder `NearestEnemyTarget` (1215)
-   Focus-Target → verwende `FocusTarget` (1207)
-   Target-Info ohne Targeting → verwende `TargetInfoRequest` (1203)

### Request Payload

| Feld     | Typ | Beschreibung          | Pflicht |
| -------- | --- | --------------------- | ------- |
| EntityId | int | Entity-ID des Targets | Ja      |

### Erwartete Response

-   `TargetSelectResponse` (1220)

### Folge-Messages bei Erfolg

-   `TargetUpdate` (1202) mit neuer Target-ID
-   `TargetInfoResponse` (1204) mit Target-Details

### Verwandte Messages

| Message              | ID   | Beziehung                      |
| -------------------- | ---- | ------------------------------ |
| `TargetUpdate`       | 1202 | Server bestätigt Target-Change |
| `TargetInfoResponse` | 1204 | Target-Details                 |
| `TargetClear`        | 1201 | Target aufheben                |

### Beispiel Payload

```csharp
var targetSelect = new TargetSelect
{
    Type = MessageType.TargetSelect,
    EntityId = 50001
};
```

### Notizen

-   **Range**: Keine Range-Limit für Targeting (nur für Actions)
-   **Dead Entities**: Tote Entities können targetiert werden (für Rez, Loot)
-   **Friendly Fire**: Eigene Faction kann targetiert werden
-   **UI**: Client zeigt Target-Frame

---

## TargetClear (1201)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Client löscht aktuelles Target. Server cleared Target-Tracking.

### Request Payload

Keine zusätzlichen Felder

### Erwartete Response

-   **Immer:** `TargetUpdate` (1202) mit EntityId=0

### Beispiel Payload

```csharp
var targetClear = new TargetClear
{
    Type = MessageType.TargetClear
};
```

### Notizen

-   **Hotkey**: Standard Keybind: ESC
-   **Auto-Clear**: Bei Target-Death oder Despawn
-   **Combat**: Clearen im Combat ist erlaubt

---

## TargetUpdate (1202)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Server bestätigt Target-Change. Client updated UI.

### Broadcast Payload

| Feld     | Typ | Beschreibung                            | Pflicht |
| -------- | --- | --------------------------------------- | ------- |
| EntityId | int | Neue Target Entity-ID (0 = kein Target) | Ja      |

### Beispiel Payload

```csharp
var targetUpdate = new TargetUpdate
{
    Type = MessageType.TargetUpdate,
    EntityId = 50001 // 0 für Target Clear
};
```

### Notizen

-   **EntityId=0**: Bedeutet kein Target
-   **UI**: Client zeigt/versteckt Target-Frame

---

## TargetInfoRequest (1203)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Fordert detaillierte Info über Entity an ohne zu targetieren. Für Mouseover-Tooltips.

### Request Payload

| Feld     | Typ | Beschreibung | Pflicht |
| -------- | --- | ------------ | ------- |
| EntityId | int | Entity-ID    | Ja      |

### Erwartete Response

-   **Immer:** `TargetInfoResponse` (1204)

### Beispiel Payload

```csharp
var infoRequest = new TargetInfoRequest
{
    Type = MessageType.TargetInfoRequest,
    EntityId = 50002
};
```

### Notizen

-   **Mouseover**: Für Tooltip-Info ohne Target-Change
-   **Rate-Limit**: Max 10 Requests/Sekunde

---

## TargetInfoResponse (1204)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung

Detaillierte Entity-Informationen. Enthält Name, Level, Health, Faction, etc.

### Response Payload

| Feld          | Typ    | Beschreibung              | Pflicht |
| ------------- | ------ | ------------------------- | ------- |
| EntityId      | int    | Entity-ID                 | Ja      |
| Name          | string | Entity-Name               | Ja      |
| Level         | int    | Level                     | Ja      |
| EntityType    | string | "player", "npc", "object" | Ja      |
| Health        | int    | Current Health            | Ja      |
| MaxHealth     | int    | Max Health                | Ja      |
| HealthPercent | float  | Health % (0.0-1.0)        | Ja      |
| Faction       | string | Fraktions-Name            | Ja      |
| IsHostile     | bool   | Ist feindlich?            | Ja      |
| IsDead        | bool   | Ist tot?                  | Ja      |
| Title         | string | Titel (bei Spielern)      | Nein    |
| GuildName     | string | Guild-Name (bei Spielern) | Nein    |

### Beispiel Payload

```csharp
var infoResponse = new TargetInfoResponse
{
    Type = MessageType.TargetInfoResponse,
    EntityId = 50001,
    Name = "Aragorn",
    Level = 10,
    EntityType = "player",
    Health = 850,
    MaxHealth = 1000,
    HealthPercent = 0.85f,
    Faction = "Alliance",
    IsHostile = false,
    IsDead = false,
    Title = "Ranger of the North",
    GuildName = "Fellowship"
};
```

### Notizen

-   **Privacy**: Gewisse Infos nur bei Friendly/Guild
-   **Health**: Boss-Health als Prozent (nicht absolute Werte)
-   **Caching**: Client kann cachen für Performance

---

## TargetOfTarget (1205)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Client fragt nach Target-of-Target (ToT). Was targetiert mein Target?

### Request Payload

| Feld     | Typ | Beschreibung | Pflicht |
| -------- | --- | ------------ | ------- |
| EntityId | int | Entity-ID    | Ja      |

### Erwartete Response

-   **Immer:** `TargetOfTargetUpdate` (1206)

### Beispiel Payload

```csharp
var totRequest = new TargetOfTarget
{
    Type = MessageType.TargetOfTarget,
    EntityId = 50001
};
```

### Notizen

-   **Use-Case**: Tank sieht wen Boss targetiert
-   **UI**: ToT-Frame im Target-Frame

---

## TargetOfTargetUpdate (1206)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Target-of-Target Information. Server sendet automatisch bei Target-Changes.

### Broadcast Payload

| Feld           | Typ    | Beschreibung                            | Pflicht |
| -------------- | ------ | --------------------------------------- | ------- |
| SourceEntityId | int    | Original Entity                         | Ja      |
| TargetEntityId | int    | Was Source targetiert (0 = kein Target) | Ja      |
| TargetName     | string | Name des ToT                            | Nein    |

### Beispiel Payload

```csharp
var totUpdate = new TargetOfTargetUpdate
{
    Type = MessageType.TargetOfTargetUpdate,
    SourceEntityId = 50001,
    TargetEntityId = 50002,
    TargetName = "Legolas"
};
```

### Notizen

-   **Auto-Update**: Server sendet bei ToT-Change
-   **TargetEntityId=0**: Bedeutet kein Target

---

## FocusTarget (1207)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Setzt Focus-Target (sekundäres Target). Bleibt auch wenn Main-Target wechselt.

### Im Scope ✅

-   Focus-Target setzen
-   Persistent Target (bleibt bei Main-Target-Change)
-   Abilities auf Focus casten

### Request Payload

| Feld     | Typ | Beschreibung        | Pflicht |
| -------- | --- | ------------------- | ------- |
| EntityId | int | Entity-ID für Focus | Ja      |

### Erwartete Response

-   **Immer:** Server trackt Focus, keine explizite Response

### Verwandte Messages

| Message        | ID   | Beziehung      |
| -------------- | ---- | -------------- |
| `FocusClear`   | 1208 | Focus aufheben |
| `TargetSelect` | 1200 | Main-Target    |

### Beispiel Payload

```csharp
var focusTarget = new FocusTarget
{
    Type = MessageType.FocusTarget,
    EntityId = 60001 // NPC/Boss
};
```

### Notizen

-   **Use-Case**: Boss-Mechanics tracken während Add-Targeting
-   **Abilities**: Können mit @focus Modifier auf Focus gecastet werden
-   **UI**: Separates Focus-Frame

---

## FocusClear (1208)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Löscht Focus-Target.

### Request Payload

Keine zusätzlichen Felder

### Notizen

-   **Hotkey**: Standard Keybind: SHIFT+F
-   **Auto-Clear**: Bei Focus-Death oder Despawn

---

## AssistTarget (1209)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Targetiert das Target von anderem Spieler (Assist). Für koordinierte Angriffe in Gruppen.

### Request Payload

| Feld           | Typ | Beschreibung                             | Pflicht |
| -------------- | --- | ---------------------------------------- | ------- |
| AssistEntityId | int | Entity-ID des zu assistierenden Spielers | Ja      |

### Erwartete Response

-   `AssistTargetResponse` (1221)

### Folge-Messages bei Erfolg

-   `TargetUpdate` (1202) mit Assist-Target
-   `TargetInfoResponse` (1204) mit Target-Details

### Beispiel Payload

```csharp
var assist = new AssistTarget
{
    Type = MessageType.AssistTarget,
    AssistEntityId = 50001 // Assist Aragorn
};
```

### Notizen

-   **Use-Case**: Tank ruft Target, DPS assisten
-   **Hotkey**: Standard Keybind: F (Assist Party-Leader)
-   **Macro**: Oft in Macros verwendet

---

## MarkTarget (1210)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Party-Leader / Raid-Leader

### Beschreibung

Markiert Target mit Raid-Marker (Skull, Cross, Square, etc.). Sichtbar für ganze Party/Raid.

### Im Scope ✅

-   Raid-Marker setzen (8 Symbole)
-   Kill-Order markieren
-   CC-Targets markieren

### Request Payload

| Feld     | Typ  | Beschreibung                                                                  | Pflicht |
| -------- | ---- | ----------------------------------------------------------------------------- | ------- |
| EntityId | int  | Zu markierende Entity                                                         | Ja      |
| MarkType | byte | Marker-Typ (0-7: Skull, Cross, Square, Moon, Triangle, Diamond, Circle, Star) | Ja      |

### Erwartete Response

-   `MarkTargetResponse` (1222)

### Folge-Messages bei Erfolg

-   Broadcast an Party/Raid Members

### Verwandte Messages

| Message        | ID   | Beziehung             |
| -------------- | ---- | --------------------- |
| `MarkClear`    | 1211 | Marker entfernen      |
| `MarkClearAll` | 1212 | Alle Marker entfernen |

### Beispiel Payload

```csharp
var mark = new MarkTarget
{
    Type = MessageType.MarkTarget,
    EntityId = 60001,
    MarkType = 0 // Skull (Kill-Target)
};
```

### Error Codes

| Code           | Bedeutung               | Aktion     |
| -------------- | ----------------------- | ---------- |
| `NOT_LEADER`   | Nicht Party/Raid-Leader | Ignorieren |
| `NOT_IN_PARTY` | Nicht in Party/Raid     | Ignorieren |

### Notizen

-   **Permission**: Nur Leader können markieren
-   **Visual**: Icon über Entity-Kopf
-   **Use-Case**: Kill-Order, CC-Assignment, Tank-Swap

---

## MarkClear (1211)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Party-Leader / Raid-Leader

### Beschreibung

Entfernt Raid-Marker von Entity.

### Request Payload

| Feld     | Typ | Beschreibung                      | Pflicht |
| -------- | --- | --------------------------------- | ------- |
| EntityId | int | Entity mit zu entfernendem Marker | Ja      |

### Notizen

-   **Permission**: Nur Leader
-   **Auto-Clear**: Bei Entity-Death

---

## MarkClearAll (1212)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Party-Leader / Raid-Leader

### Beschreibung

Entfernt alle Raid-Marker.

### Request Payload

Keine zusätzlichen Felder

### Notizen

-   **Use-Case**: Nach Boss-Kill, vor neuem Pull
-   **Hotkey**: Oft auf Macro gebunden

---

## MouseoverTarget (1213)

**Richtung:** 📤 Client → Server  
**Frequenz:** ⚡ High-Frequency  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Client informiert über Mouseover-Entity. Für Mouseover-Macros und @mouseover Abilities.

### Request Payload

| Feld     | Typ | Beschreibung      | Pflicht |
| -------- | --- | ----------------- | ------- |
| EntityId | int | Entity unter Maus | Ja      |

### Notizen

-   **High-Frequency**: Nur bei Mouseover-Change senden
-   **Rate-Limit**: Max 20/Sekunde
-   **Use-Case**: @mouseover Healing-Macros

---

## TabTarget (1214)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Targetiert nächste Entity in Tab-Order (links nach rechts, nah nach fern).

### Request Payload

| Feld    | Typ  | Beschreibung           | Pflicht |
| ------- | ---- | ---------------------- | ------- |
| Reverse | bool | Rückwärts (SHIFT+TAB)? | Nein    |

### Erwartete Response

-   `TabTargetResponse` (1223)

### Folge-Messages bei Erfolg

-   `TargetUpdate` (1202) mit nächstem Target
-   `TargetInfoResponse` (1204) mit Target-Details

### Beispiel Payload

```csharp
var tabTarget = new TabTarget
{
    Type = MessageType.TabTarget,
    Reverse = false
};
```

### Notizen

-   **Hotkey**: TAB (forward), SHIFT+TAB (reverse)
-   **Filter**: Nur lebende Enemies
-   **Sort**: Nach Angle dann Distance

---

## NearestEnemyTarget (1215)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Targetiert nächsten feindlichen Entity (nur Distance, kein Angle).

### Request Payload

Keine zusätzlichen Felder

### Erwartete Response

-   `NearestEnemyTargetResponse` (1224)

### Folge-Messages bei Erfolg

-   `TargetUpdate` (1202) mit nächstem Enemy
-   `TargetInfoResponse` (1204) mit Target-Details

### Beispiel Payload

```csharp
var nearestEnemy = new NearestEnemyTarget
{
    Type = MessageType.NearestEnemyTarget
};
```

### Notizen

-   **Use-Case**: Aggro-Übernahme, Quick-Target
-   **Filter**: Nur hostile, lebende Entities
-   **Sort**: Nur nach Distance (nicht Angle)

---

## NearestFriendTarget (1216)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Targetiert nächsten freundlichen Entity (für Heals/Buffs).

### Request Payload

Keine zusätzlichen Felder

### Erwartete Response

-   `NearestFriendTargetResponse` (1225)

### Folge-Messages bei Erfolg

-   `TargetUpdate` (1202) mit nächstem Friend
-   `TargetInfoResponse` (1204) mit Target-Details

### Notizen

-   **Use-Case**: Emergency-Heals, Quick-Rez
-   **Filter**: Nur friendly, lebende Entities
-   **Sort**: Nach Distance

---

## TargetSelectResponse (1220)

**Richtung:** 📥 Server → Client  
**Frequenz:** Sehr häufig  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung

Antwort auf TargetSelect Request. Bestätigt erfolgreiche Target-Selektion oder gibt Fehler zurück.

### Response Payload

| Feld         | Typ    | Beschreibung                   | Pflicht    |
| ------------ | ------ | ------------------------------ | ---------- |
| Success      | bool   | Target-Selektion erfolgreich?  | Ja         |
| ErrorCode    | string | Fehlercode falls Success=false | Nein       |
| ErrorMessage | string | Menschenlesbare Fehlermeldung  | Nein       |
| TargetId     | int    | Selektierte Entity-ID          | Bei Erfolg |

### Error Codes

| Code               | Bedeutung               |
| ------------------ | ----------------------- |
| `ENTITY_NOT_FOUND` | Entity existiert nicht  |
| `OUT_OF_RANGE`     | Entity zu weit entfernt |

---

## AssistTargetResponse (1221)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung

Antwort auf AssistTarget Request. Bestätigt erfolgreiche Assist-Target-Selektion oder gibt Fehler zurück.

### Response Payload

| Feld           | Typ    | Beschreibung                   | Pflicht    |
| -------------- | ------ | ------------------------------ | ---------- |
| Success        | bool   | Assist erfolgreich?            | Ja         |
| ErrorCode      | string | Fehlercode falls Success=false | Nein       |
| ErrorMessage   | string | Menschenlesbare Fehlermeldung  | Nein       |
| AssistTargetId | int    | Target des Assist-Targets      | Bei Erfolg |

### Error Codes

| Code        | Bedeutung                     |
| ----------- | ----------------------------- |
| `NO_TARGET` | Assist-Target hat kein Target |

---

## MarkTargetResponse (1222)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung

Antwort auf MarkTarget Request. Bestätigt erfolgreiche Target-Markierung oder gibt Fehler zurück.

### Response Payload

| Feld         | Typ    | Beschreibung                   | Pflicht    |
| ------------ | ------ | ------------------------------ | ---------- |
| Success      | bool   | Mark erfolgreich?              | Ja         |
| ErrorCode    | string | Fehlercode falls Success=false | Nein       |
| ErrorMessage | string | Menschenlesbare Fehlermeldung  | Nein       |
| MarkIcon     | int    | Mark-Icon (0-8)                | Bei Erfolg |

### Error Codes

| Code               | Bedeutung              |
| ------------------ | ---------------------- |
| `NOT_IN_PARTY`     | Nicht in Party/Raid    |
| `NOT_PARTY_LEADER` | Nur Leader darf marken |

---

## TabTargetResponse (1223)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung

Antwort auf TabTarget Request. Bestätigt erfolgreiche Tab-Target-Selektion oder gibt Fehler zurück.

### Response Payload

| Feld         | Typ    | Beschreibung                   | Pflicht    |
| ------------ | ------ | ------------------------------ | ---------- |
| Success      | bool   | Tab-Target erfolgreich?        | Ja         |
| ErrorCode    | string | Fehlercode falls Success=false | Nein       |
| ErrorMessage | string | Menschenlesbare Fehlermeldung  | Nein       |
| TargetId     | int    | Neues Target                   | Bei Erfolg |

### Error Codes

| Code                  | Bedeutung               |
| --------------------- | ----------------------- |
| `NO_TARGETS_IN_RANGE` | Keine Targets verfügbar |

---

## NearestEnemyTargetResponse (1224)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung

Antwort auf NearestEnemyTarget Request. Bestätigt erfolgreiche Enemy-Target-Selektion oder gibt Fehler zurück.

### Response Payload

| Feld         | Typ    | Beschreibung                   | Pflicht    |
| ------------ | ------ | ------------------------------ | ---------- |
| Success      | bool   | Target-Selektion erfolgreich?  | Ja         |
| ErrorCode    | string | Fehlercode falls Success=false | Nein       |
| ErrorMessage | string | Menschenlesbare Fehlermeldung  | Nein       |
| TargetId     | int    | Nächster Enemy                 | Bei Erfolg |
| Distance     | float  | Distanz zum Enemy              | Bei Erfolg |

### Error Codes

| Code                  | Bedeutung                   |
| --------------------- | --------------------------- |
| `NO_ENEMIES_IN_RANGE` | Keine Enemies in Reichweite |

---

## NearestFriendTargetResponse (1225)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung

Antwort auf NearestFriendTarget Request. Bestätigt erfolgreiche Friend-Target-Selektion oder gibt Fehler zurück.

### Response Payload

| Feld         | Typ    | Beschreibung                   | Pflicht    |
| ------------ | ------ | ------------------------------ | ---------- |
| Success      | bool   | Target-Selektion erfolgreich?  | Ja         |
| ErrorCode    | string | Fehlercode falls Success=false | Nein       |
| ErrorMessage | string | Menschenlesbare Fehlermeldung  | Nein       |
| TargetId     | int    | Nächster Friend                | Bei Erfolg |
| Distance     | float  | Distanz zum Friend             | Bei Erfolg |

### Error Codes

| Code                  | Bedeutung                   |
| --------------------- | --------------------------- |
| `NO_FRIENDS_IN_RANGE` | Keine Friends in Reichweite |

---

## 🗑️ Obsolete Messages

Derzeit keine obsoleten Messages in dieser Kategorie.

---

## 📎 Anhang

### MessageType Enum (Reihenfolge aus Code)

```csharp
// TARGETING (1200-1225)
TargetSelect = 1200,
TargetClear = 1201,
TargetUpdate = 1202,
TargetInfoRequest = 1203,
TargetInfoResponse = 1204,
TargetOfTarget = 1205,
TargetOfTargetUpdate = 1206,
FocusTarget = 1207,
FocusClear = 1208,
AssistTarget = 1209,
MarkTarget = 1210,
MarkClear = 1211,
MarkClearAll = 1212,
MouseoverTarget = 1213,
TabTarget = 1214,
NearestEnemyTarget = 1215,
NearestFriendTarget = 1216,
TargetSelectResponse = 1220,
AssistTargetResponse = 1221,
MarkTargetResponse = 1222,
TabTargetResponse = 1223,
NearestEnemyTargetResponse = 1224,
NearestFriendTargetResponse = 1225,
```

### Request/Response Paare

| Request | ID | Response | ID |
|---------|-----|----------|-----|
| `TargetSelect` | 1200 | `TargetSelectResponse` | 1220 |
| `TargetClear` | 1201 | `TargetUpdate` | 1202 |
| `TargetInfoRequest` | 1203 | `TargetInfoResponse` | 1204 |
| `TargetOfTarget` | 1205 | `TargetOfTargetUpdate` | 1206 |
| `AssistTarget` | 1209 | `AssistTargetResponse` | 1221 |
| `MarkTarget` | 1210 | `MarkTargetResponse` | 1222 |
| `TabTarget` | 1214 | `TabTargetResponse` | 1223 |
| `NearestEnemyTarget` | 1215 | `NearestEnemyTargetResponse` | 1224 |
| `NearestFriendTarget` | 1216 | `NearestFriendTargetResponse` | 1225 |

### Datei-Struktur

```
shared/Mmo.Shared/Messaging/
├── Enums/
│   └── MessageType.cs           # Targeting: 1200-1225
├── DTOs/
│   └── TargetEntityDto.cs       # Target-Frame Daten
└── Messages/
    └── Targeting/
        ├── TargetSelect.cs
        ├── TargetClear.cs
        ├── TargetUpdate.cs
        ├── TargetInfoRequest.cs
        ├── TargetInfoResponse.cs
        ├── TargetOfTarget.cs
        ├── TargetOfTargetUpdate.cs
        ├── FocusTarget.cs
        ├── FocusClear.cs
        ├── AssistTarget.cs
        ├── AssistTargetResponse.cs
        ├── MarkTarget.cs
        ├── MarkClear.cs
        ├── MarkClearAll.cs
        ├── MarkTargetResponse.cs
        ├── MouseoverTarget.cs
        ├── TabTarget.cs
        ├── TabTargetResponse.cs
        ├── NearestEnemyTarget.cs
        ├── NearestEnemyTargetResponse.cs
        ├── NearestFriendTarget.cs
        └── NearestFriendTargetResponse.cs
```

---

**Letzte Aktualisierung**: 2026-01-02  
**Version**: 3.0.0

[← Zurück zur Übersicht](Message-Reference.md)

Source: docs/03-messages/12-targeting.md
