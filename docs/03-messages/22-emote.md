# 😄 Emote Messages (2200-2242)

**Kategorie:** 22  
**Range:** 2200-2242  
**Phase:** Prototyp  
**Status:** 🟢 In Entwicklung

[← Zurück zur Übersicht](README.md)

---

## 📋 Inhaltsverzeichnis

1. [Übersicht](#übersicht)
2. [Emote Flow](#emote-flow)
   - [Server-Authoritative Architektur](#server-authoritative-architektur)
   - [Emote-Execution Flow](#emote-execution-flow)
   - [Cosmetic/Transmog Flow](#cosmetic-transmog-flow)
3. [DTOs und Enums](#dtos-und-enums)
   - [EmoteType Enum](#emotetype-enum)
   - [PoseType Enum](#posetype-enum)
   - [EmoteErrorCode Enum](#emoteerrorcode-enum)
   - [CosmeticSlot Enum](#cosmeticslot-enum)
   - [EmoteInfoDto](#emoteinfodto)
   - [CosmeticItemDto](#cosmeticitemdto)
   - [TransmogSetDto](#transmogsetdto)
   - [ToyInfoDto](#toyinfodto)
   - [Wichtige Konstanten](#wichtige-konstanten)
4. [Messages](#messages)
   - [EmoteRequest (2200)](#emoterequest-2200)
   - [EmoteBroadcast (2201)](#emotebroadcast-2201)
   - [EmoteTargeted (2202)](#emotetargeted-2202)
   - [AnimationTrigger (2203)](#animationtrigger-2203)
   - [AnimationCancel (2204)](#animationcancel-2204)
   - [DanceStart (2210)](#dancestart-2210)
   - [DanceStop (2211)](#dancestop-2211)
   - [SitRequest (2212)](#sitrequest-2212)
   - [StandRequest (2213)](#standrequest-2213)
   - [SleepRequest (2214)](#sleeprequest-2214)
   - [KneelRequest (2215)](#kneelrequest-2215)
   - [CosmeticEquip (2230)](#cosmeticequip-2230)
   - [CosmeticUnequip (2231)](#cosmeticunequip-2231)
   - [CosmeticPreview (2232)](#cosmeticpreview-2232)
   - [TransmogApply (2233)](#transmogapply-2233)
   - [TransmogRemove (2234)](#transmogremove-2234)
   - [TransmogSave (2235)](#transmogsave-2235)
   - [TransmogLoad (2236)](#transmogload-2236)
   - [ToyUse (2240)](#toyuse-2240)
   - [ToyListRequest (2241)](#toylistrequest-2241)
   - [ToyListResponse (2242)](#toylistresponse-2242)
5. [Anhang](#anhang)
   - [MessageType Enum (Exact Order)](#messagetype-enum-exact-order)
   - [Request/Response Paare](#request-response-paare)
   - [Datei-Struktur](#datei-struktur)

---

## Übersicht

Diese Kategorie umfasst alle Messages für **Emotes, Animationen, Posen und Cosmetics** im 2D Pixelart-MMORPG.

**Kern-Features:**
- Emotes (Gestiken wie /wave, /bow, /laugh)
- Posen (sit, stand, sleep, kneel)
- Dauer-Animationen (Dance)
- Cosmetic-System (Appearance-Override)
- Transmog-System (Equipment-Appearance ändern)
- Toy-Collection (Fun-Items mit Effekten)

**Server-Authority:**
- Server validiert alle Emote-Requests
- Server entscheidet über Animation-Abbruch
- Server verwaltet Cosmetic/Transmog-Daten
- Client zeigt nur was Server broadcastet

---

## Emote Flow

### Server-Authoritative Architektur

```
┌─────────────────────────────────────────────────────────────────┐
│                    EMOTE SYSTEM                                 │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│   ┌─────────────┐      ┌─────────────┐      ┌─────────────┐   │
│   │   CLIENT    │      │   SERVER    │      │   OTHER     │   │
│   │  (Request)  │─────►│ (Validate)  │─────►│  CLIENTS    │   │
│   └─────────────┘      └─────────────┘      └─────────────┘   │
│                              │                                 │
│                              ▼                                 │
│                    ┌─────────────────┐                        │
│                    │ VALIDATION:     │                        │
│                    │ - Cooldown      │                        │
│                    │ - State Check   │                        │
│                    │ - Permission    │                        │
│                    │ - Distance      │                        │
│                    └─────────────────┘                        │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

### Emote-Execution Flow

```
Client                     Zone Server                Other Clients
   │                            │                           │
   │  EmoteRequest (2200)       │                           │
   │  {EmoteId, TargetId?}      │                           │
   │───────────────────────────►│                           │
   │                            │                           │
   │                            │ Validate:                 │
   │                            │ - Not in combat?          │
   │                            │ - Cooldown passed?        │
   │                            │ - Emote unlocked?         │
   │                            │                           │
   │                            │ If TargetId set:          │
   │                            │ - Target exists?          │
   │                            │ - Target in range?        │
   │                            │                           │
   │  EmoteBroadcast (2201)     │  EmoteBroadcast (2201)   │
   │  OR EmoteTargeted (2202)   │  OR EmoteTargeted (2202) │
   │◄───────────────────────────│──────────────────────────►│
   │                            │                           │
```

### Cosmetic-Transmog Flow

```
Client                     Zone Server                  Database
   │                            │                           │
   │  TransmogApply (2233)      │                           │
   │  {SlotId, AppearanceId}    │                           │
   │───────────────────────────►│                           │
   │                            │                           │
   │                            │ Validate:                 │
   │                            │ - Appearance unlocked?    │
   │                            │ - At Transmog NPC?        │
   │                            │ - Enough gold?            │
   │                            │                           │
   │                            │  Save Transmog            │
   │                            │─────────────────────────► │
   │                            │                           │
   │  AnimationTrigger (2203)   │                           │
   │  {Type=TransmogApplied}    │                           │
   │◄───────────────────────────│                           │
   │                            │                           │
   │  (Appearance Update via    │                           │
   │   EntityAppearance 1409)   │                           │
   │◄───────────────────────────│                           │
```

---

## DTOs und Enums

### EmoteType Enum

```csharp
public enum EmoteType : byte
{
    Wave = 0,        // /wave - Winken
    Bow = 1,         // /bow - Verbeugung
    Laugh = 2,       // /laugh - Lachen
    Cry = 3,         // /cry - Weinen
    Cheer = 4,       // /cheer - Jubeln
    Point = 5,       // /point - Zeigen
    Salute = 6,      // /salute - Salutieren
    Shrug = 7,       // /shrug - Achselzucken
    Clap = 8,        // /clap - Klatschen
    Angry = 9,       // /angry - Wütend
    Kiss = 10,       // /kiss - Kuss
    Hug = 11,        // /hug - Umarmung
    ThumbsUp = 12,   // /thumbsup - Daumen hoch
    ThumbsDown = 13, // /thumbsdown - Daumen runter
    Flex = 14,       // /flex - Muskeln zeigen
    Dance = 15,      // /dance - Tanzen (Dauer)
    Custom = 255     // Custom emote
}
```

### PoseType Enum

```csharp
public enum PoseType : byte
{
    Standing = 0,  // Default
    Sitting = 1,   // /sit
    Sleeping = 2,  // /sleep
    Kneeling = 3,  // /kneel
    Dancing = 4    // /dance (looping)
}
```

### EmoteErrorCode Enum

```csharp
public enum EmoteErrorCode : byte
{
    None = 0,                    // Erfolg
    InCombat = 1,               // Nicht im Kampf erlaubt
    Cooldown = 2,               // Emote auf Cooldown
    NotUnlocked = 3,            // Emote nicht freigeschaltet
    TargetNotFound = 4,         // Ziel nicht gefunden
    TargetTooFar = 5,           // Ziel zu weit entfernt
    InvalidEmote = 6,           // Ungültige Emote-ID
    MovementBlocked = 7,        // Bewegung blockiert Emote
    Silenced = 8,               // Spieler ist stumm geschaltet
    InvalidPose = 9,            // Ungültige Pose
    AlreadyInPose = 10,         // Bereits in dieser Pose
    CosmeticNotOwned = 11,      // Cosmetic nicht im Besitz
    TransmogLocked = 12,        // Transmog nicht freigeschaltet
    NotAtNPC = 13,              // Nicht bei Transmog-NPC
    InsufficientGold = 14,      // Nicht genug Gold
    ToyOnCooldown = 15,         // Toy auf Cooldown
    ToyNotOwned = 16            // Toy nicht im Besitz
}
```

### CosmeticSlot Enum

```csharp
public enum CosmeticSlot : byte
{
    Head = 0,       // Helm-Cosmetic
    Shoulders = 1,  // Schulter-Cosmetic
    Back = 2,       // Umhang/Rücken
    Chest = 3,      // Brustpanzer
    Shirt = 4,      // Unterhemd
    Tabard = 5,     // Wappenrock
    Hands = 6,      // Handschuhe
    Waist = 7,      // Gürtel
    Legs = 8,       // Beinrüstung
    Feet = 9,       // Schuhe
    MainHand = 10,  // Haupthand-Waffe
    OffHand = 11,   // Nebenhand
    Pet = 12        // Pet-Appearance
}
```

### EmoteInfoDto

```csharp
[MessagePackObject]
public class EmoteInfoDto
{
    [Key(0)] public MessageType Type => MessageType.EmoteBroadcast;
    [Key(1)] public ushort EmoteId { get; set; }       // EmoteType oder Custom-ID
    [Key(2)] public string EmoteName { get; set; }     // Display-Name
    [Key(3)] public ushort AnimationId { get; set; }   // Sprite-Animation ID
    [Key(4)] public ushort DurationMs { get; set; }    // Animation Dauer
    [Key(5)] public bool IsLooping { get; set; }       // Dauer-Animation?
    [Key(6)] public bool RequiresTarget { get; set; }  // Braucht Ziel?
}
```

### CosmeticItemDto

```csharp
[MessagePackObject]
public class CosmeticItemDto
{
    [Key(0)] public MessageType Type => MessageType.CosmeticEquip;
    [Key(1)] public uint CosmeticId { get; set; }      // Eindeutige ID
    [Key(2)] public CosmeticSlot Slot { get; set; }    // Slot
    [Key(3)] public ushort AppearanceId { get; set; }  // Sprite-ID
    [Key(4)] public string Name { get; set; }          // Display-Name
    [Key(5)] public byte Rarity { get; set; }          // 0=Common..5=Legendary
}
```

### TransmogSetDto

```csharp
[MessagePackObject]
public class TransmogSetDto
{
    [Key(0)] public MessageType Type => MessageType.TransmogLoad;
    [Key(1)] public byte SetId { get; set; }                    // 0-9
    [Key(2)] public string SetName { get; set; }                // Custom name
    [Key(3)] public Dictionary<byte, ushort> SlotAppearances { get; set; } // Slot→AppearanceId
}
```

### ToyInfoDto

```csharp
[MessagePackObject]
public class ToyInfoDto
{
    [Key(0)] public MessageType Type => MessageType.ToyListResponse;
    [Key(1)] public uint ToyId { get; set; }           // Eindeutige ID
    [Key(2)] public string Name { get; set; }          // Display-Name
    [Key(3)] public ushort CooldownSec { get; set; }   // Cooldown in Sekunden
    [Key(4)] public ushort EffectId { get; set; }      // Visual-Effect ID
    [Key(5)] public bool IsCollected { get; set; }     // Im Besitz?
}
```

### Wichtige Konstanten

| Konstante | Wert | Beschreibung |
|-----------|------|--------------|
| EMOTE_COOLDOWN_MS | 1500 | Min. Zeit zwischen Emotes |
| EMOTE_TARGET_RANGE | 40f | Max. Distanz für targeted Emotes |
| MAX_TRANSMOG_SETS | 10 | Max. speicherbare Transmog-Sets |
| TRANSMOG_COST_GOLD | 100 | Gold pro Transmog-Anwendung |
| TOY_MIN_COOLDOWN_SEC | 30 | Minimum Toy-Cooldown |
| MAX_COSMETICS_EQUIPPED | 13 | Alle Slots |
| DANCE_LOOP_DURATION_MS | 5000 | Dance-Animation Loop |

---

## Messages

### EmoteRequest (2200)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Erforderlich

#### Beschreibung
Client möchte ein Emote ausführen. Server validiert und broadcastet bei Erfolg.

#### Request Payload

| Feld | Typ | Key | Beschreibung | Pflicht |
|------|-----|-----|--------------|---------|
| Type | MessageType | 0 | = EmoteRequest (2200) | Ja |
| RequestId | uint | 1 | Eindeutige Request-ID | Ja |
| EmoteId | ushort | 2 | EmoteType oder Custom-ID | Ja |
| TargetEntityId | uint? | 3 | Optional: Ziel-Entity | Nein |

#### Erwartete Response
- **Bei Erfolg:** `EmoteBroadcast` (2201) oder `EmoteTargeted` (2202)
- **Bei Fehler:** `ErrorMessage` (910) mit EmoteErrorCode

#### Validierung
- Spieler nicht im Kampf
- Cooldown eingehalten (1.5s)
- Emote freigeschaltet (für Custom-Emotes)
- Falls TargetEntityId: Ziel in Reichweite (40 Units)

---

### EmoteBroadcast (2201)

**Richtung:** 📡 Server → Clients (Broadcast)  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Server-Authoritative

#### Beschreibung
Server broadcastet ein ausgeführtes Emote an alle Spieler in der Zone.

#### Payload

| Feld | Typ | Key | Beschreibung | Pflicht |
|------|-----|-----|--------------|---------|
| Type | MessageType | 0 | = EmoteBroadcast (2201) | Ja |
| EntityId | uint | 1 | Entity die Emote ausführt | Ja |
| EmoteId | ushort | 2 | EmoteType oder Custom-ID | Ja |
| Timestamp | long | 3 | Server-Timestamp | Ja |

#### Client-Verhalten
- Animation für EntityId abspielen
- Chat-Message anzeigen: "[Player] waves."

---

### EmoteTargeted (2202)

**Richtung:** 📡 Server → Clients (Broadcast)  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Server-Authoritative

#### Beschreibung
Server broadcastet ein Emote mit Ziel (z.B. "/bow @Player").

#### Payload

| Feld | Typ | Key | Beschreibung | Pflicht |
|------|-----|-----|--------------|---------|
| Type | MessageType | 0 | = EmoteTargeted (2202) | Ja |
| SourceEntityId | uint | 1 | Ausführende Entity | Ja |
| TargetEntityId | uint | 2 | Ziel-Entity | Ja |
| EmoteId | ushort | 3 | EmoteType | Ja |
| Timestamp | long | 4 | Server-Timestamp | Ja |

#### Client-Verhalten
- Animation für SourceEntityId abspielen
- Source dreht sich zu Target
- Chat-Message: "[Player] bows to [Target]."

---

### AnimationTrigger (2203)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Server-Authoritative

#### Beschreibung
Server triggert eine spezifische Animation auf Client (nicht Emote-basiert).

#### Payload

| Feld | Typ | Key | Beschreibung | Pflicht |
|------|-----|-----|--------------|---------|
| Type | MessageType | 0 | = AnimationTrigger (2203) | Ja |
| EntityId | uint | 1 | Betroffene Entity | Ja |
| AnimationId | ushort | 2 | Animation-ID | Ja |
| DurationMs | ushort | 3 | Dauer in Millisekunden | Ja |
| IsLooping | bool | 4 | Loop-Animation? | Ja |

#### Verwendung
- Transmog-Anwendung visuell
- NPC-Animationen
- Umgebungs-Trigger (Fallen, etc.)

---

### AnimationCancel (2204)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Erforderlich

#### Beschreibung
Client möchte laufende Animation abbrechen (z.B. Bewegung während Dance).

#### Request Payload

| Feld | Typ | Key | Beschreibung | Pflicht |
|------|-----|-----|--------------|---------|
| Type | MessageType | 0 | = AnimationCancel (2204) | Ja |
| RequestId | uint | 1 | Eindeutige Request-ID | Ja |

#### Erwartete Response
- Server broadcastet Pose-Change via `EntityState` (1407)
- Keine explizite Response

#### Anmerkung
Fire-and-Forget Message. Animation wird serverseitig beendet.

---

### DanceStart (2210)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Erforderlich

#### Beschreibung
Client startet Tanz-Animation (looping bis DanceStop oder Bewegung).

#### Request Payload

| Feld | Typ | Key | Beschreibung | Pflicht |
|------|-----|-----|--------------|---------|
| Type | MessageType | 0 | = DanceStart (2210) | Ja |
| RequestId | uint | 1 | Eindeutige Request-ID | Ja |
| DanceId | byte | 2 | Dance-Variante (0-9) | Ja |

#### Erwartete Response
- **Bei Erfolg:** `EmoteBroadcast` (2201) mit EmoteId=Dance + Loop-Flag
- **Bei Fehler:** `ErrorMessage` (910)

#### Validierung
- Nicht im Kampf
- Nicht bereits tanzend
- DanceId gültig (0-9)

---

### DanceStop (2211)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Erforderlich

#### Beschreibung
Client beendet Tanz-Animation explizit.

#### Request Payload

| Feld | Typ | Key | Beschreibung | Pflicht |
|------|-----|-----|--------------|---------|
| Type | MessageType | 0 | = DanceStop (2211) | Ja |
| RequestId | uint | 1 | Eindeutige Request-ID | Ja |

#### Erwartete Response
- Server broadcastet Pose-Change (PoseType=Standing)
- Keine explizite Response (Fire-and-Forget)

---

### SitRequest (2212)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Erforderlich

#### Beschreibung
Client möchte sich hinsetzen.

#### Request Payload

| Feld | Typ | Key | Beschreibung | Pflicht |
|------|-----|-----|--------------|---------|
| Type | MessageType | 0 | = SitRequest (2212) | Ja |
| RequestId | uint | 1 | Eindeutige Request-ID | Ja |
| ChairEntityId | uint? | 2 | Optional: Stuhl-Entity | Nein |

#### Erwartete Response
- **Bei Erfolg:** `EntityState` (1407) Update mit PoseType=Sitting
- **Bei Fehler:** `ErrorMessage` (910)

#### Validierung
- Nicht im Kampf
- Nicht bereits sitzend
- Falls ChairEntityId: Stuhl frei und in Reichweite

---

### StandRequest (2213)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Erforderlich

#### Beschreibung
Client steht von Pose auf (sit, sleep, kneel).

#### Request Payload

| Feld | Typ | Key | Beschreibung | Pflicht |
|------|-----|-----|--------------|---------|
| Type | MessageType | 0 | = StandRequest (2213) | Ja |
| RequestId | uint | 1 | Eindeutige Request-ID | Ja |

#### Erwartete Response
- **Bei Erfolg:** `EntityState` (1407) Update mit PoseType=Standing
- **Bei Fehler:** `ErrorMessage` (910) falls bereits stehend

---

### SleepRequest (2214)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Erforderlich

#### Beschreibung
Client legt sich schlafen (Regen-Boost in Ruhe-Bereichen).

#### Request Payload

| Feld | Typ | Key | Beschreibung | Pflicht |
|------|-----|-----|--------------|---------|
| Type | MessageType | 0 | = SleepRequest (2214) | Ja |
| RequestId | uint | 1 | Eindeutige Request-ID | Ja |
| BedEntityId | uint? | 2 | Optional: Bett-Entity | Nein |

#### Erwartete Response
- **Bei Erfolg:** `EntityState` (1407) Update mit PoseType=Sleeping
- **Bei Fehler:** `ErrorMessage` (910)

#### Validierung
- Nicht im Kampf
- Nicht bereits schlafend
- Falls BedEntityId: Bett frei und in Reichweite

---

### KneelRequest (2215)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Erforderlich

#### Beschreibung
Client kniet nieder (Respekt-Geste, Quest-Interaktion).

#### Request Payload

| Feld | Typ | Key | Beschreibung | Pflicht |
|------|-----|-----|--------------|---------|
| Type | MessageType | 0 | = KneelRequest (2215) | Ja |
| RequestId | uint | 1 | Eindeutige Request-ID | Ja |

#### Erwartete Response
- **Bei Erfolg:** `EntityState` (1407) Update mit PoseType=Kneeling
- **Bei Fehler:** `ErrorMessage` (910)

---

### CosmeticEquip (2230)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Erforderlich

#### Beschreibung
Client rüstet ein kosmetisches Item aus (überschreibt Equipment-Look).

#### Request Payload

| Feld | Typ | Key | Beschreibung | Pflicht |
|------|-----|-----|--------------|---------|
| Type | MessageType | 0 | = CosmeticEquip (2230) | Ja |
| RequestId | uint | 1 | Eindeutige Request-ID | Ja |
| CosmeticId | uint | 2 | Cosmetic-Item ID | Ja |
| Slot | CosmeticSlot | 3 | Ziel-Slot | Ja |

#### Erwartete Response
- **Bei Erfolg:** `EntityAppearance` (1409) Update
- **Bei Fehler:** `ErrorMessage` (910) mit CosmeticNotOwned

#### Validierung
- Cosmetic im Besitz des Spielers
- Cosmetic passt zu Slot

---

### CosmeticUnequip (2231)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Erforderlich

#### Beschreibung
Client entfernt kosmetisches Item (zeigt wieder echtes Equipment).

#### Request Payload

| Feld | Typ | Key | Beschreibung | Pflicht |
|------|-----|-----|--------------|---------|
| Type | MessageType | 0 | = CosmeticUnequip (2231) | Ja |
| RequestId | uint | 1 | Eindeutige Request-ID | Ja |
| Slot | CosmeticSlot | 2 | Slot zum Leeren | Ja |

#### Erwartete Response
- **Bei Erfolg:** `EntityAppearance` (1409) Update
- **Bei Fehler:** `ErrorMessage` (910) falls Slot leer

---

### CosmeticPreview (2232)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Erforderlich

#### Beschreibung
Client previewed ein Cosmetic bevor Kauf/Equip (nur lokal sichtbar).

#### Request Payload

| Feld | Typ | Key | Beschreibung | Pflicht |
|------|-----|-----|--------------|---------|
| Type | MessageType | 0 | = CosmeticPreview (2232) | Ja |
| RequestId | uint | 1 | Eindeutige Request-ID | Ja |
| CosmeticId | uint | 2 | Cosmetic-Item ID | Ja |

#### Erwartete Response
- **Bei Erfolg:** Lokales Update (kein Broadcast)
- **Bei Fehler:** `ErrorMessage` (910) falls ungültige ID

#### Anmerkung
Preview ist nur für den anfragenden Client sichtbar, andere Spieler sehen es nicht.

---

### TransmogApply (2233)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Erforderlich

#### Beschreibung
Client wendet Transmog auf Equipment-Slot an (ändert Aussehen, nicht Stats).

#### Request Payload

| Feld | Typ | Key | Beschreibung | Pflicht |
|------|-----|-----|--------------|---------|
| Type | MessageType | 0 | = TransmogApply (2233) | Ja |
| RequestId | uint | 1 | Eindeutige Request-ID | Ja |
| EquipmentSlot | byte | 2 | Ziel Equipment-Slot | Ja |
| AppearanceId | ushort | 3 | Gewünschte Appearance-ID | Ja |

#### Erwartete Response
- **Bei Erfolg:** `AnimationTrigger` (2203) + `EntityAppearance` (1409)
- **Bei Fehler:** `ErrorMessage` (910)

#### Validierung
- Bei Transmog-NPC (z.B. Ethereal)
- Appearance freigeschaltet (Item einmal besessen)
- Genug Gold (TRANSMOG_COST_GOLD = 100)

---

### TransmogRemove (2234)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Erforderlich

#### Beschreibung
Client entfernt Transmog von Equipment (zeigt echtes Item wieder).

#### Request Payload

| Feld | Typ | Key | Beschreibung | Pflicht |
|------|-----|-----|--------------|---------|
| Type | MessageType | 0 | = TransmogRemove (2234) | Ja |
| RequestId | uint | 1 | Eindeutige Request-ID | Ja |
| EquipmentSlot | byte | 2 | Slot zum Entfernen | Ja |

#### Erwartete Response
- **Bei Erfolg:** `EntityAppearance` (1409) Update
- **Bei Fehler:** `ErrorMessage` (910) falls kein Transmog aktiv

#### Validierung
- Bei Transmog-NPC
- Slot hat aktiven Transmog

---

### TransmogSave (2235)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Erforderlich

#### Beschreibung
Client speichert aktuelles Transmog-Setup als Set (max 10 Sets).

#### Request Payload

| Feld | Typ | Key | Beschreibung | Pflicht |
|------|-----|-----|--------------|---------|
| Type | MessageType | 0 | = TransmogSave (2235) | Ja |
| RequestId | uint | 1 | Eindeutige Request-ID | Ja |
| SetId | byte | 2 | Set-Slot (0-9) | Ja |
| SetName | string | 3 | Custom Name (max 32 Zeichen) | Ja |

#### Erwartete Response
- **Bei Erfolg:** Bestätigung via `SystemMessage` (920)
- **Bei Fehler:** `ErrorMessage` (910) falls SetId ungültig

---

### TransmogLoad (2236)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Erforderlich

#### Beschreibung
Client lädt gespeichertes Transmog-Set und wendet es an.

#### Request Payload

| Feld | Typ | Key | Beschreibung | Pflicht |
|------|-----|-----|--------------|---------|
| Type | MessageType | 0 | = TransmogLoad (2236) | Ja |
| RequestId | uint | 1 | Eindeutige Request-ID | Ja |
| SetId | byte | 2 | Set-Slot (0-9) | Ja |

#### Erwartete Response
- **Bei Erfolg:** `EntityAppearance` (1409) Update für alle Slots
- **Bei Fehler:** `ErrorMessage` (910) falls Set nicht existiert

#### Validierung
- Bei Transmog-NPC
- Set existiert
- Alle Appearances im Set noch freigeschaltet

---

### ToyUse (2240)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Erforderlich

#### Beschreibung
Client benutzt ein Toy aus der Collection (Fun-Effekt).

#### Request Payload

| Feld | Typ | Key | Beschreibung | Pflicht |
|------|-----|-----|--------------|---------|
| Type | MessageType | 0 | = ToyUse (2240) | Ja |
| RequestId | uint | 1 | Eindeutige Request-ID | Ja |
| ToyId | uint | 2 | Toy-ID | Ja |

#### Erwartete Response
- **Bei Erfolg:** `AnimationTrigger` (2203) mit Toy-Effekt
- **Bei Fehler:** `ErrorMessage` (910)

#### Validierung
- Toy im Besitz
- Toy nicht auf Cooldown
- Nicht im Kampf (für bestimmte Toys)

---

### ToyListRequest (2241)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Erforderlich

#### Beschreibung
Client fordert Toy-Collection Liste an.

#### Request Payload

| Feld | Typ | Key | Beschreibung | Pflicht |
|------|-----|-----|--------------|---------|
| Type | MessageType | 0 | = ToyListRequest (2241) | Ja |
| RequestId | uint | 1 | Eindeutige Request-ID | Ja |

#### Erwartete Response
- `ToyListResponse` (2242) mit allen Toys

---

### ToyListResponse (2242)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Server-Authoritative

#### Beschreibung
Server sendet vollständige Toy-Collection (gesammelt + nicht gesammelt).

#### Payload

| Feld | Typ | Key | Beschreibung | Pflicht |
|------|-----|-----|--------------|---------|
| Type | MessageType | 0 | = ToyListResponse (2242) | Ja |
| RequestId | uint | 1 | Korrelation mit Request | Ja |
| Toys | ToyInfoDto[] | 2 | Array aller Toys | Ja |
| TotalCollected | ushort | 3 | Anzahl gesammelter Toys | Ja |
| TotalAvailable | ushort | 4 | Anzahl aller Toys | Ja |

---

## Anhang

### MessageType Enum (Exact Order)

```csharp
// EMOTES / ANIMATIONS / COSMETICS (2200-2299)
EmoteRequest = 2200,
EmoteBroadcast = 2201,
EmoteTargeted = 2202,
AnimationTrigger = 2203,
AnimationCancel = 2204,
DanceStart = 2210,
DanceStop = 2211,
SitRequest = 2212,
StandRequest = 2213,
SleepRequest = 2214,
KneelRequest = 2215,
CosmeticEquip = 2230,
CosmeticUnequip = 2231,
CosmeticPreview = 2232,
TransmogApply = 2233,
TransmogRemove = 2234,
TransmogSave = 2235,
TransmogLoad = 2236,
ToyUse = 2240,
ToyListRequest = 2241,
ToyListResponse = 2242,
```

### Request-Response Paare

| Request | ID | Response | ID | Beschreibung |
|---------|-----|----------|-----|--------------|
| EmoteRequest | 2200 | EmoteBroadcast/EmoteTargeted | 2201/2202 | Emote ausführen |
| DanceStart | 2210 | EmoteBroadcast | 2201 | Tanz starten |
| SitRequest | 2212 | EntityState | 1407 | Hinsetzen |
| StandRequest | 2213 | EntityState | 1407 | Aufstehen |
| SleepRequest | 2214 | EntityState | 1407 | Schlafen |
| KneelRequest | 2215 | EntityState | 1407 | Knien |
| CosmeticEquip | 2230 | EntityAppearance | 1409 | Cosmetic anlegen |
| CosmeticUnequip | 2231 | EntityAppearance | 1409 | Cosmetic ablegen |
| TransmogApply | 2233 | EntityAppearance | 1409 | Transmog anwenden |
| TransmogRemove | 2234 | EntityAppearance | 1409 | Transmog entfernen |
| TransmogLoad | 2236 | EntityAppearance | 1409 | Transmog-Set laden |
| ToyUse | 2240 | AnimationTrigger | 2203 | Toy benutzen |
| ToyListRequest | 2241 | ToyListResponse | 2242 | Toy-Liste abrufen |

### Fire-and-Forget Messages

| Message | ID | Beschreibung |
|---------|-----|--------------|
| AnimationCancel | 2204 | Animation abbrechen |
| DanceStop | 2211 | Tanz beenden |
| CosmeticPreview | 2232 | Cosmetic Preview (lokal) |
| TransmogSave | 2235 | Set speichern |

### Server-initiated Messages

| Message | ID | Beschreibung |
|---------|-----|--------------|
| EmoteBroadcast | 2201 | Emote an alle Clients |
| EmoteTargeted | 2202 | Targeted Emote Broadcast |
| AnimationTrigger | 2203 | Animation triggern |
| ToyListResponse | 2242 | Response auf ToyListRequest |

### Datei-Struktur

```
shared/Mmo.Shared/
├── Messaging/
│   ├── Enums/
│   │   └── MessageType.cs          # 2200-2242
│   └── Messages/
│       └── Emote/
│           ├── EmoteRequest.cs
│           ├── EmoteBroadcast.cs
│           ├── EmoteTargeted.cs
│           ├── AnimationTrigger.cs
│           ├── AnimationCancel.cs
│           ├── DanceStart.cs
│           ├── DanceStop.cs
│           ├── SitRequest.cs
│           ├── StandRequest.cs
│           ├── SleepRequest.cs
│           ├── KneelRequest.cs
│           ├── CosmeticEquip.cs
│           ├── CosmeticUnequip.cs
│           ├── CosmeticPreview.cs
│           ├── TransmogApply.cs
│           ├── TransmogRemove.cs
│           ├── TransmogSave.cs
│           ├── TransmogLoad.cs
│           ├── ToyUse.cs
│           ├── ToyListRequest.cs
│           └── ToyListResponse.cs
└── DTOs/
    └── Emote/
        ├── EmoteInfoDto.cs
        ├── CosmeticItemDto.cs
        ├── TransmogSetDto.cs
        └── ToyInfoDto.cs
```

---

**Letzte Aktualisierung**: 2026-01-02  
**Version**: 3.0.0  
**Status**: ✅ Aligned mit MessageType Enum (21 Messages)

[← Zurück zur Übersicht](README.md)
