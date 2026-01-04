# ⚙️ Settings Messages (3000-3099)

**Kategorie:** 30  
**Range:** 3000-3099  
**Phase:** Prototyp  
**Status:** 🟢 In Entwicklung

[← Zurück zur Übersicht](Message-Reference.md)

---

## 📋 Inhaltsverzeichnis

- [📋 Überblick](#-überblick)
- [🧠 Datenmodell](#-datenmodell)
- [🗂️ Settings Kategorien](#️-settings-kategorien)
- [🔄 Sync & Persistence](#-sync--persistence)
- [🧱 DTOs / Interfaces](#-dtos--interfaces)
- [🧩 Enums / ErrorCodes](#-enums--errorcodes)
- [⚙️ Regeln & Validierung](#️-regeln--validierung)
- [📩 Aktive Messages 3000-3099](#-aktive-messages-3000-3099)
- [🧨 Edge Cases & Fehlerfälle](#-edge-cases--fehlerfälle)
- [📎 Anhang](#-anhang)

---

## 📋 Überblick

Diese Kategorie behandelt alle **Settings-, Keybinding-, UI-Layout-, Macro- und Addon-Daten-Messages** im 2DMMO-System.

### Ziele

- **Client Preferences**: Speichern und Laden von Spielereinstellungen
- **Account vs Character Scope**: Unterscheidung zwischen Account-weiten und Character-spezifischen Settings
- **Keybindings**: Tastenbelegungen verwalten
- **UI-Layouts**: Benutzerdefinierte UI-Anordnungen
- **Macros**: Spieler-erstellte Makros
- **Addon-Daten**: Daten für Addon-Erweiterungen

### Server-Autorität

Der Server ist autoritativ für:
- Validierung von Settings-Werten
- Speicherung und Persistenz
- Rate-Limiting von Save-Requests
- Schema-Versionierung und Migration

---

## 🧠 Datenmodell

### SettingsScope

```csharp
public enum SettingsScope : byte
{
    Account = 1,    // Account-weit (alle Characters)
    Character = 2   // Character-spezifisch
}
```

### PlayerSettingsState

```csharp
public class PlayerSettingsState
{
    public Guid AccountId { get; set; }
    public Guid? CharacterId { get; set; }
    public SettingsScope Scope { get; set; }
    public int SchemaVersion { get; set; }
    public long Revision { get; set; }
    public long LastModified { get; set; }
    public Dictionary<string, SettingValue> Settings { get; set; }
}
```

### SettingValue

```csharp
public class SettingValue
{
    public SettingValueType Type { get; set; }
    public object Value { get; set; }
}

public enum SettingValueType : byte
{
    Bool = 1,
    Int = 2,
    Float = 3,
    String = 4,
    Enum = 5
}
```

### KeybindingData

```csharp
public class KeybindingData
{
    public string ActionId { get; set; }
    public KeyCode PrimaryKey { get; set; }
    public KeyModifiers PrimaryModifiers { get; set; }
    public KeyCode? SecondaryKey { get; set; }
    public KeyModifiers? SecondaryModifiers { get; set; }
}
```

### UiLayoutData

```csharp
public class UiLayoutData
{
    public string LayoutName { get; set; }
    public Dictionary<string, UiElementPosition> Elements { get; set; }
    public bool IsDefault { get; set; }
}

public class UiElementPosition
{
    public float X { get; set; }
    public float Y { get; set; }
    public float Width { get; set; }
    public float Height { get; set; }
    public float Scale { get; set; }
    public bool Visible { get; set; }
}
```

### MacroData

```csharp
public class MacroData
{
    public Guid MacroId { get; set; }
    public string Name { get; set; }
    public string Icon { get; set; }
    public string Body { get; set; }
    public MacroScope Scope { get; set; }
    public int SlotIndex { get; set; }
}

public enum MacroScope : byte
{
    Account = 1,
    Character = 2
}
```

---

## 🗂️ Settings Kategorien

| Kategorie | Scope | Beispiele |
|-----------|-------|-----------|
| Audio | Account | MasterVolume, MusicVolume, SfxVolume, VoiceVolume |
| Video | Account | Resolution, Fullscreen, VSync, QualityPreset |
| UI | Account/Character | UIScale, ShowDamageNumbers, ShowHealthBars |
| Controls | Account | MouseSensitivity, InvertY, ClickToMove |
| Accessibility | Account | Colorblind Mode, ScreenReader, SubtitleSize |
| Gameplay | Character | AutoLoot, ShowTutorials, ActionBarLock |

---

## 🔄 Sync & Persistence

### Sync-Strategie

1. **Full Snapshot on Login**: `SettingsLoad` → `SettingsLoadResult` mit allen Settings
2. **Delta Updates on Change**: `SettingsSave` sendet nur geänderte Werte
3. **Revision-basiert**: Jede Änderung erhöht `Revision` für Conflict-Detection

### Merge-Regeln bei Multi-Session

- **Last-Write-Wins**: Neuester `ServerTimestamp` gewinnt
- **Monotonic Revision**: Server erhöht Revision bei jedem Save
- **Conflict-Detection**: Client sendet `ExpectedRevision`, Server lehnt bei Mismatch ab

### Persistence

- Settings werden in DB gespeichert (nicht Redis)
- Backup bei Schema-Migration
- Max 30 Tage Inaktivität → Settings bleiben erhalten

---

## 🧱 DTOs / Interfaces

### SettingsDto

```csharp
[MessagePackObject]
public class SettingsDto
{
    [Key(0)] public SettingsScope Scope { get; set; }
    [Key(1)] public int SchemaVersion { get; set; }
    [Key(2)] public long Revision { get; set; }
    [Key(3)] public Dictionary<string, SettingValueDto> Settings { get; set; }
}
```

### SettingValueDto

```csharp
[MessagePackObject]
public class SettingValueDto
{
    [Key(0)] public SettingValueType Type { get; set; }
    [Key(1)] public byte[] SerializedValue { get; set; }
}
```

### KeybindingDto

```csharp
[MessagePackObject]
public class KeybindingDto
{
    [Key(0)] public string ActionId { get; set; }
    [Key(1)] public int PrimaryKey { get; set; }
    [Key(2)] public int PrimaryModifiers { get; set; }
    [Key(3)] public int? SecondaryKey { get; set; }
    [Key(4)] public int? SecondaryModifiers { get; set; }
}
```

### MacroDto

```csharp
[MessagePackObject]
public class MacroDto
{
    [Key(0)] public Guid MacroId { get; set; }
    [Key(1)] public string Name { get; set; }
    [Key(2)] public string Icon { get; set; }
    [Key(3)] public string Body { get; set; }
    [Key(4)] public MacroScope Scope { get; set; }
    [Key(5)] public int SlotIndex { get; set; }
}
```

---

## 🧩 Enums / ErrorCodes

### SettingsErrorCode

```csharp
public enum SettingsErrorCode : byte
{
    None = 0,
    InvalidKey = 1,
    InvalidValue = 2,
    ValueOutOfRange = 3,
    RevisionMismatch = 4,
    SchemaMismatch = 5,
    RateLimited = 6,
    StorageError = 7,
    MaxSettingsExceeded = 8
}
```

### KeybindingsErrorCode

```csharp
public enum KeybindingsErrorCode : byte
{
    None = 0,
    InvalidAction = 1,
    InvalidKey = 2,
    ConflictingBinding = 3,
    ReservedKey = 4,
    StorageError = 5
}
```

### UiLayoutErrorCode

```csharp
public enum UiLayoutErrorCode : byte
{
    None = 0,
    InvalidLayout = 1,
    InvalidElement = 2,
    MaxLayoutsExceeded = 3,
    StorageError = 4
}
```

### MacroErrorCode

```csharp
public enum MacroErrorCode : byte
{
    None = 0,
    InvalidName = 1,
    InvalidBody = 2,
    BodyTooLong = 3,
    MaxMacrosExceeded = 4,
    MacroNotFound = 5,
    ForbiddenCommand = 6,
    StorageError = 7
}
```

---

## ⚙️ Regeln & Validierung

### Settings-Limits

| Limit | Wert | Beschreibung |
|-------|------|--------------|
| Max Keys | 500 | Maximale Anzahl Settings-Keys |
| Max Key Length | 64 | Maximale Key-Länge |
| Max String Value | 1024 | Maximale String-Wert-Länge |
| Rate Limit | 10/min | Max Save-Requests pro Minute |

### Keybinding-Regeln

- Reservierte Keys: Escape, PrintScreen, etc.
- Max 2 Bindings pro Action (Primary + Secondary)
- Conflict-Check: Warnung bei doppelter Belegung

### Macro-Limits

| Limit | Wert |
|-------|------|
| Max Account Macros | 120 |
| Max Character Macros | 18 |
| Max Body Length | 255 |
| Max Name Length | 16 |

### Idempotency

- Duplicate `SettingsSave` mit gleichen Values → No-Op (keine Revision-Erhöhung)
- Server vergleicht eingehende Werte mit gespeicherten

---

## 📩 Aktive Messages 3000-3099

---

## SettingsLoad (3000)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten (Login, Settings-UI öffnen)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Client fordert Settings vom Server an.

### Im Scope ✅

- Account-Settings laden
- Character-Settings laden
- Beide Scopes gleichzeitig

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.SettingsLoad` | Ja |
| Scope | SettingsScope | Account, Character oder Both | Ja |
| CharacterId | Guid? | Character-ID (falls Scope Character) | Nein |

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.SettingsLoad)]
public class SettingsLoad : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.SettingsLoad;
    [Key(1)] public SettingsScope Scope { get; set; }
    [Key(2)] public Guid? CharacterId { get; set; }
}
```

### Erwartete Response

- `SettingsLoadResult` (3001)

---

## SettingsLoadResult (3001)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Server sendet geladene Settings an Client.

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.SettingsLoadResult` | Ja |
| Success | bool | Erfolgreich? | Ja |
| ErrorCode | SettingsErrorCode | Fehlercode | Nein |
| AccountSettings | SettingsDto? | Account-Settings | Nein |
| CharacterSettings | SettingsDto? | Character-Settings | Nein |

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.SettingsLoadResult)]
public class SettingsLoadResult : IServerMessage, IResponseMessage<SettingsErrorCode>
{
    [Key(0)] public MessageType Type => MessageType.SettingsLoadResult;
    [Key(1)] public bool Success { get; set; }
    [Key(2)] public GlobalErrorCode GlobalError { get; set; }
    [Key(3)] public SettingsErrorCode? ErrorCode { get; set; }
    [Key(4)] public string? ErrorMessage { get; set; }
    [Key(5)] public SettingsDto? AccountSettings { get; set; }
    [Key(6)] public SettingsDto? CharacterSettings { get; set; }
}
```

---

## SettingsSave (3002)

**Richtung:** 📤 Client → Server  
**Frequenz:** Mittel (bei Settings-Änderung)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Client speichert geänderte Settings.

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.SettingsSave` | Ja |
| Scope | SettingsScope | Account oder Character | Ja |
| CharacterId | Guid? | Character-ID (falls Scope Character) | Nein |
| ExpectedRevision | long | Erwartete Revision (Conflict-Check) | Ja |
| Changes | Dictionary\<string, SettingValueDto\> | Geänderte Settings | Ja |

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.SettingsSave)]
public class SettingsSave : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.SettingsSave;
    [Key(1)] public SettingsScope Scope { get; set; }
    [Key(2)] public Guid? CharacterId { get; set; }
    [Key(3)] public long ExpectedRevision { get; set; }
    [Key(4)] public Dictionary<string, SettingValueDto> Changes { get; set; }
}
```

### Erwartete Response

- `SettingsSaveResult` (3003)

---

## SettingsSaveResult (3003)

**Richtung:** 📥 Server → Client  
**Frequenz:** Mittel  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Server bestätigt Settings-Speicherung.

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.SettingsSaveResult` | Ja |
| Success | bool | Erfolgreich? | Ja |
| ErrorCode | SettingsErrorCode | Fehlercode | Nein |
| NewRevision | long | Neue Revision nach Save | Bei Erfolg |

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.SettingsSaveResult)]
public class SettingsSaveResult : IServerMessage, IResponseMessage<SettingsErrorCode>
{
    [Key(0)] public MessageType Type => MessageType.SettingsSaveResult;
    [Key(1)] public bool Success { get; set; }
    [Key(2)] public GlobalErrorCode GlobalError { get; set; }
    [Key(3)] public SettingsErrorCode? ErrorCode { get; set; }
    [Key(4)] public string? ErrorMessage { get; set; }
    [Key(5)] public long NewRevision { get; set; }
}
```

---

## SettingsReset (3004)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Client setzt Settings auf Standardwerte zurück.

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.SettingsReset` | Ja |
| Scope | SettingsScope | Account oder Character | Ja |
| CharacterId | Guid? | Character-ID | Nein |
| Category | string? | Nur bestimmte Kategorie (null = alle) | Nein |

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.SettingsReset)]
public class SettingsReset : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.SettingsReset;
    [Key(1)] public SettingsScope Scope { get; set; }
    [Key(2)] public Guid? CharacterId { get; set; }
    [Key(3)] public string? Category { get; set; }
}
```

### Erwartete Response

- `SettingsResetResult` (3005)

---

## SettingsResetResult (3005)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Server bestätigt Settings-Reset.

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.SettingsResetResult` | Ja |
| Success | bool | Erfolgreich? | Ja |
| ErrorCode | SettingsErrorCode | Fehlercode | Nein |
| NewSettings | SettingsDto | Neue Default-Settings | Bei Erfolg |

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.SettingsResetResult)]
public class SettingsResetResult : IServerMessage, IResponseMessage<SettingsErrorCode>
{
    [Key(0)] public MessageType Type => MessageType.SettingsResetResult;
    [Key(1)] public bool Success { get; set; }
    [Key(2)] public GlobalErrorCode GlobalError { get; set; }
    [Key(3)] public SettingsErrorCode? ErrorCode { get; set; }
    [Key(4)] public string? ErrorMessage { get; set; }
    [Key(5)] public SettingsDto? NewSettings { get; set; }
}
```

---

## KeybindingsLoad (3010)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Client fordert Keybindings vom Server an.

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.KeybindingsLoad` | Ja |

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.KeybindingsLoad)]
public class KeybindingsLoad : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.KeybindingsLoad;
}
```

### Erwartete Response

- `KeybindingsLoadResult` (3011)

---

## KeybindingsLoadResult (3011)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Server sendet Keybindings an Client.

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.KeybindingsLoadResult` | Ja |
| Success | bool | Erfolgreich? | Ja |
| ErrorCode | KeybindingsErrorCode | Fehlercode | Nein |
| Keybindings | List\<KeybindingDto\> | Alle Keybindings | Bei Erfolg |

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.KeybindingsLoadResult)]
public class KeybindingsLoadResult : IServerMessage, IResponseMessage<KeybindingsErrorCode>
{
    [Key(0)] public MessageType Type => MessageType.KeybindingsLoadResult;
    [Key(1)] public bool Success { get; set; }
    [Key(2)] public GlobalErrorCode GlobalError { get; set; }
    [Key(3)] public KeybindingsErrorCode? ErrorCode { get; set; }
    [Key(4)] public string? ErrorMessage { get; set; }
    [Key(5)] public List<KeybindingDto>? Keybindings { get; set; }
}
```

---

## KeybindingsSave (3012)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Client speichert geänderte Keybindings.

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.KeybindingsSave` | Ja |
| Keybindings | List\<KeybindingDto\> | Geänderte Keybindings | Ja |

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.KeybindingsSave)]
public class KeybindingsSave : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.KeybindingsSave;
    [Key(1)] public List<KeybindingDto> Keybindings { get; set; }
}
```

### Erwartete Response

- `KeybindingsSaveResult` (3013)

---

## KeybindingsSaveResult (3013)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Server bestätigt Keybindings-Speicherung.

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.KeybindingsSaveResult` | Ja |
| Success | bool | Erfolgreich? | Ja |
| ErrorCode | KeybindingsErrorCode | Fehlercode | Nein |

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.KeybindingsSaveResult)]
public class KeybindingsSaveResult : IServerMessage, IResponseMessage<KeybindingsErrorCode>
{
    [Key(0)] public MessageType Type => MessageType.KeybindingsSaveResult;
    [Key(1)] public bool Success { get; set; }
    [Key(2)] public GlobalErrorCode GlobalError { get; set; }
    [Key(3)] public KeybindingsErrorCode? ErrorCode { get; set; }
    [Key(4)] public string? ErrorMessage { get; set; }
}
```

---

## KeybindingsReset (3014)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Client setzt Keybindings auf Standardwerte zurück.

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.KeybindingsReset` | Ja |

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.KeybindingsReset)]
public class KeybindingsReset : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.KeybindingsReset;
}
```

### Erwartete Response

- `KeybindingsResetResult` (3015)

---

## KeybindingsResetResult (3015)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Server bestätigt Keybindings-Reset und sendet Defaults.

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.KeybindingsResetResult` | Ja |
| Success | bool | Erfolgreich? | Ja |
| ErrorCode | KeybindingsErrorCode | Fehlercode | Nein |
| DefaultKeybindings | List\<KeybindingDto\> | Default-Keybindings | Bei Erfolg |

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.KeybindingsResetResult)]
public class KeybindingsResetResult : IServerMessage, IResponseMessage<KeybindingsErrorCode>
{
    [Key(0)] public MessageType Type => MessageType.KeybindingsResetResult;
    [Key(1)] public bool Success { get; set; }
    [Key(2)] public GlobalErrorCode GlobalError { get; set; }
    [Key(3)] public KeybindingsErrorCode? ErrorCode { get; set; }
    [Key(4)] public string? ErrorMessage { get; set; }
    [Key(5)] public List<KeybindingDto>? DefaultKeybindings { get; set; }
}
```

---

## UiLayoutLoad (3020)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Client fordert UI-Layouts vom Server an.

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.UiLayoutLoad` | Ja |

### Erwartete Response

- `UiLayoutLoadResult` (3021)

---

## UiLayoutLoadResult (3021)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Server sendet UI-Layouts an Client.

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.UiLayoutLoadResult` | Ja |
| Success | bool | Erfolgreich? | Ja |
| ErrorCode | UiLayoutErrorCode | Fehlercode | Nein |
| Layouts | List\<UiLayoutDto\> | Alle Layouts | Bei Erfolg |
| ActiveLayoutName | string | Aktuell aktives Layout | Bei Erfolg |

---

## UiLayoutSave (3022)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Client speichert UI-Layout.

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.UiLayoutSave` | Ja |
| Layout | UiLayoutDto | Layout-Daten | Ja |

### Erwartete Response

- `UiLayoutSaveResult` (3023)

---

## UiLayoutSaveResult (3023)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Server bestätigt UI-Layout-Speicherung.

---

## UiLayoutReset (3024)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Client setzt UI-Layout auf Standard zurück.

### Erwartete Response

- `UiLayoutResetResult` (3025)

---

## UiLayoutResetResult (3025)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Server bestätigt UI-Layout-Reset und sendet Default-Layout.

---

## MacroCreate (3030)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Client erstellt ein neues Makro.

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.MacroCreate` | Ja |
| Name | string | Makro-Name (max 16 Zeichen) | Ja |
| Icon | string | Icon-ID | Ja |
| Body | string | Makro-Inhalt (max 255 Zeichen) | Ja |
| Scope | MacroScope | Account oder Character | Ja |
| SlotIndex | int | Slot-Position | Ja |

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.MacroCreate)]
public class MacroCreate : IClientMessage
{
    [Key(0)] public MessageType Type => MessageType.MacroCreate;
    [Key(1)] public string Name { get; set; }
    [Key(2)] public string Icon { get; set; }
    [Key(3)] public string Body { get; set; }
    [Key(4)] public MacroScope Scope { get; set; }
    [Key(5)] public int SlotIndex { get; set; }
}
```

### Erwartete Response

- `MacroCreateResult` (3031)

---

## MacroCreateResult (3031)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Server bestätigt Makro-Erstellung.

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.MacroCreateResult` | Ja |
| Success | bool | Erfolgreich? | Ja |
| ErrorCode | MacroErrorCode | Fehlercode | Nein |
| MacroId | Guid | ID des erstellten Makros | Bei Erfolg |

---

## MacroEdit (3032)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Client bearbeitet ein bestehendes Makro.

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.MacroEdit` | Ja |
| MacroId | Guid | ID des Makros | Ja |
| Name | string? | Neuer Name | Nein |
| Icon | string? | Neues Icon | Nein |
| Body | string? | Neuer Inhalt | Nein |

### Erwartete Response

- `MacroEditResult` (3033)

---

## MacroEditResult (3033)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Server bestätigt Makro-Bearbeitung.

---

## MacroDelete (3034)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Client löscht ein Makro.

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.MacroDelete` | Ja |
| MacroId | Guid | ID des zu löschenden Makros | Ja |

### Erwartete Response

- `MacroDeleteResult` (3035)

---

## MacroDeleteResult (3035)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Server bestätigt Makro-Löschung.

---

## MacroSync (3036)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten (Login, Character-Wechsel)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Server synchronisiert alle Makros mit Client.

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.MacroSync` | Ja |
| AccountMacros | List\<MacroDto\> | Account-weite Makros | Ja |
| CharacterMacros | List\<MacroDto\> | Character-Makros | Ja |

### Code-Beispiel

```csharp
[MessagePackObject]
[NetworkMessage(MessageType.MacroSync)]
public class MacroSync : IServerMessage
{
    [Key(0)] public MessageType Type => MessageType.MacroSync;
    [Key(1)] public List<MacroDto> AccountMacros { get; set; }
    [Key(2)] public List<MacroDto> CharacterMacros { get; set; }
}
```

---

## AddonDataLoad (3040)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Client fordert Addon-Daten an.

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.AddonDataLoad` | Ja |
| AddonName | string | Name des Addons | Ja |

### Erwartete Response

- `AddonDataLoadResult` (3041)

---

## AddonDataLoadResult (3041)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Server sendet Addon-Daten.

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.AddonDataLoadResult` | Ja |
| Success | bool | Erfolgreich? | Ja |
| AddonName | string | Name des Addons | Ja |
| Data | byte[]? | Serialisierte Addon-Daten | Bei Erfolg |

---

## AddonDataSave (3042)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Client speichert Addon-Daten.

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `MessageType.AddonDataSave` | Ja |
| AddonName | string | Name des Addons | Ja |
| Data | byte[] | Serialisierte Addon-Daten | Ja |

### Erwartete Response

- `AddonDataSaveResult` (3043)

---

## AddonDataSaveResult (3043)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Server bestätigt Addon-Daten-Speicherung.

---

## 🧨 Edge Cases & Fehlerfälle

### Revision Conflict

```
Szenario: Client A und B ändern gleichzeitig Settings
Client A: Save mit Revision 5 → Erfolg, neue Revision 6
Client B: Save mit Revision 5 → Fehler RevisionMismatch
Lösung: Client B muss reload und erneut ändern
```

### Schema Migration

```
Szenario: Server-Update ändert Settings-Schema
Erwartung: Alte Keys werden migriert oder ignoriert
Server-Verhalten: Unbekannte Keys werden beibehalten aber nicht validiert
```

### Rate Limiting

```
Szenario: Client sendet >10 SettingsSave pro Minute
Erwartung: SettingsSaveResult mit ErrorCode.RateLimited
Client-Verhalten: Batching von Änderungen, Debouncing
```

### Max Macros Exceeded

```
Szenario: Client versucht mehr als 120 Account-Macros zu erstellen
Erwartung: MacroCreateResult mit ErrorCode.MaxMacrosExceeded
```

---

## 📎 Anhang

### MessageType Enum (Kategorie 30)

```csharp
// ═══════════════════════════════════════════════════════════════
// SETTINGS / PREFERENCES SYNC (3000-3099)
// ═══════════════════════════════════════════════════════════════
SettingsLoad = 3000,
SettingsLoadResult = 3001,
SettingsSave = 3002,
SettingsSaveResult = 3003,
SettingsReset = 3004,
SettingsResetResult = 3005,
KeybindingsLoad = 3010,
KeybindingsLoadResult = 3011,
KeybindingsSave = 3012,
KeybindingsSaveResult = 3013,
KeybindingsReset = 3014,
KeybindingsResetResult = 3015,
UiLayoutLoad = 3020,
UiLayoutLoadResult = 3021,
UiLayoutSave = 3022,
UiLayoutSaveResult = 3023,
UiLayoutReset = 3024,
UiLayoutResetResult = 3025,
MacroCreate = 3030,
MacroCreateResult = 3031,
MacroEdit = 3032,
MacroEditResult = 3033,
MacroDelete = 3034,
MacroDeleteResult = 3035,
MacroSync = 3036,
AddonDataLoad = 3040,
AddonDataLoadResult = 3041,
AddonDataSave = 3042,
AddonDataSaveResult = 3043,
```

### Datei-Struktur

```
Mmo.Shared/
├── Settings/
│   ├── Enums/
│   │   ├── SettingsScope.cs
│   │   ├── SettingValueType.cs
│   │   ├── SettingsErrorCode.cs
│   │   ├── KeybindingsErrorCode.cs
│   │   ├── UiLayoutErrorCode.cs
│   │   ├── MacroErrorCode.cs
│   │   └── MacroScope.cs
│   ├── Dtos/
│   │   ├── SettingsDto.cs
│   │   ├── SettingValueDto.cs
│   │   ├── KeybindingDto.cs
│   │   ├── UiLayoutDto.cs
│   │   └── MacroDto.cs
│   └── Messages/
│       ├── Client→Server/
│       │   ├── SettingsLoad.cs
│       │   ├── SettingsSave.cs
│       │   ├── SettingsReset.cs
│       │   ├── KeybindingsLoad.cs
│       │   ├── KeybindingsSave.cs
│       │   ├── KeybindingsReset.cs
│       │   ├── UiLayoutLoad.cs
│       │   ├── UiLayoutSave.cs
│       │   ├── UiLayoutReset.cs
│       │   ├── MacroCreate.cs
│       │   ├── MacroEdit.cs
│       │   ├── MacroDelete.cs
│       │   ├── AddonDataLoad.cs
│       │   └── AddonDataSave.cs
│       └── Server→Client/
│           ├── SettingsLoadResult.cs
│           ├── SettingsSaveResult.cs
│           ├── SettingsResetResult.cs
│           ├── KeybindingsLoadResult.cs
│           ├── KeybindingsSaveResult.cs
│           ├── KeybindingsResetResult.cs
│           ├── UiLayoutLoadResult.cs
│           ├── UiLayoutSaveResult.cs
│           ├── UiLayoutResetResult.cs
│           ├── MacroCreateResult.cs
│           ├── MacroEditResult.cs
│           ├── MacroDeleteResult.cs
│           ├── MacroSync.cs
│           ├── AddonDataLoadResult.cs
│           └── AddonDataSaveResult.cs
```

---

**Letzte Aktualisierung:** 2026-01-03  
**Version:** 3.0.0

[← Zurück zur Übersicht](Message-Reference.md)

Source: docs/03-messages/30-settings.md
