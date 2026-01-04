# 🐴 Mount / Pet / Companion Messages (2000-2054)

**Kategorie:** 20 – Mounts / Pets / Companions  
**Range:** 2000–2054  
**Phase:** ✅ Aktiv  
**Version:** 3.0.0

[← Zurück zur Übersicht](Message-Reference.md)

---

## 📋 Inhaltsverzeichnis

1. [Übersicht](#übersicht)
2. [Mount/Pet/Companion Flow](#mountpetcompanion-flow)
   - [Server-Authoritative Architektur](#server-authoritative-architektur)
   - [Mount-Summon Flow](#mount-summon-flow)
   - [Pet-Command Flow](#pet-command-flow)
3. [DTOs und Enums](#dtos-und-enums)
   - [MountRarity](#mountrarity)
   - [MountType](#mounttype)
   - [PetCommand](#petcommand-enum)
   - [MountErrorCode](#mounterrorcode)
   - [MountDto](#mountdto)
   - [PetDto](#petdto)
   - [CompanionDto](#companiondto)
   - [Wichtige Konstanten](#wichtige-konstanten)
4. [Messages](#messages)
   - [MountSummon (2000)](#mountsummon-2000)
   - [MountSummonResult (2001)](#mountsummonresult-2001)
   - [MountDismount (2002)](#mountdismount-2002)
   - [MountListRequest (2003)](#mountlistrequest-2003)
   - [MountListResponse (2004)](#mountlistresponse-2004)
   - [MountFavorite (2005)](#mountfavorite-2005)
   - [MountUnfavorite (2006)](#mountunfavorite-2006)
   - [MountRandomFavorite (2007)](#mountrandomfavorite-2007)
   - [PetSummon (2020)](#petsummon-2020)
   - [PetSummonResult (2021)](#petsummonresult-2021)
   - [PetDismiss (2022)](#petdismiss-2022)
   - [PetRename (2023)](#petrename-2023)
   - [PetCommand (2024)](#petcommand-2024)
   - [PetCommandResult (2025)](#petcommandresult-2025)
   - [PetUpdate (2026)](#petupdate-2026)
   - [PetFeed (2027)](#petfeed-2027)
   - [PetTrain (2028)](#pettrain-2028)
   - [PetAbandon (2029)](#petabandon-2029)
   - [PetStable (2030)](#petstable-2030)
   - [PetUnstable (2031)](#petunstable-2031)
   - [PetListRequest (2032)](#petlistrequest-2032)
   - [PetListResponse (2033)](#petlistresponse-2033)
   - [CompanionSummon (2050)](#companionsummon-2050)
   - [CompanionDismiss (2051)](#companiondismiss-2051)
   - [CompanionInteract (2052)](#companioninteract-2052)
   - [CompanionListRequest (2053)](#companionlistrequest-2053)
   - [CompanionListResponse (2054)](#companionlistresponse-2054)
5. [Anhang](#anhang)
   - [MessageType Enum](#messagetype-enum)
   - [Request/Response Paare](#requestresponse-paare)
   - [Datei-Struktur](#datei-struktur)

---

## Übersicht

Diese Kategorie umfasst alle Messages für das **Mount-, Pet- und Companion-System** im 2DMMO.

**Drei Subsysteme:**
- **Mounts (2000-2007):** Reittiere für schnellere Fortbewegung
- **Pets (2020-2033):** Kampfbegleiter mit Befehlen und Training
- **Companions (2050-2054):** Kosmetische Begleiter ohne Kampffunktion

**Wichtig:** Server ist autoritativ für alle Mount/Pet/Companion-Aktionen. Client sendet Requests, Server validiert und bestätigt.

---

## Mount/Pet/Companion Flow

### Server-Authoritative Architektur

```
┌─────────────────────────────────────────────────────────────────┐
│                    SERVER (Authoritative)                       │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────────────────┐  │
│  │ Mount-      │  │ Pet-        │  │ Companion-              │  │
│  │ Manager     │  │ Manager     │  │ Manager                 │  │
│  │             │  │             │  │                         │  │
│  │ • Validate  │  │ • Validate  │  │ • Validate              │  │
│  │ • Summon    │  │ • Commands  │  │ • Summon                │  │
│  │ • Speed     │  │ • Training  │  │ • Interactions          │  │
│  │ • Dismount  │  │ • Stable    │  │ • Collection            │  │
│  └─────────────┘  └─────────────┘  └─────────────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
         │                  │                      │
         ▼                  ▼                      ▼
┌─────────────────────────────────────────────────────────────────┐
│                    CLIENT (Präsentation)                        │
│  • Mount-UI + Sammlung                                          │
│  • Pet-UI + Befehle                                             │
│  • Companion-UI + Interaktion                                   │
│  • Lokale Vorschau (nicht-autoritativ)                          │
└─────────────────────────────────────────────────────────────────┘
```

### Mount-Summon Flow

```
Client                          Server                          Broadcast
   │                               │                               │
   │ MountSummon(MountId)          │                               │
   │ ────────────────────────────► │                               │
   │                               │ Validate:                     │
   │                               │  • Mount owned?               │
   │                               │  • Not in combat?             │
   │                               │  • Zone allows mount?         │
   │                               │  • Cooldown expired?          │
   │                               │                               │
   │ MountSummonResult(Success)    │                               │
   │ ◄──────────────────────────── │                               │
   │                               │                               │
   │                               │ EntityUpdate (Mount spawned)  │
   │                               │ ─────────────────────────────►│
   │                               │                               │
   │ MountDismount()               │                               │
   │ ────────────────────────────► │                               │
   │                               │ Validate + Dismount           │
   │ MountSummonResult(Dismounted) │                               │
   │ ◄──────────────────────────── │                               │
   │                               │                               │
```

### Pet-Command Flow

```
Client                          Server                          
   │                               │                               
   │ PetCommand(Attack, TargetId)  │                               
   │ ────────────────────────────► │                               
   │                               │ Validate:                     
   │                               │  • Pet active?                
   │                               │  • Target valid?              
   │                               │  • In range?                  
   │                               │                               
   │ PetCommandResult(Success)     │                               
   │ ◄──────────────────────────── │                               
   │                               │                               
   │ PetUpdate(State=Attacking)    │                               
   │ ◄──────────────────────────── │ (continuous updates)          
   │                               │                               
```

---

## DTOs und Enums

### MountRarity

```csharp
public enum MountRarity : byte
{
    Common    = 0,   // Standard-Mounts
    Uncommon  = 1,   // Verbesserte Mounts
    Rare      = 2,   // Seltene Mounts
    Epic      = 3,   // Epische Mounts
    Legendary = 4,   // Legendäre Mounts
    Unique    = 5    // Einzigartige Event-Mounts
}
```

### MountType

```csharp
public enum MountType : byte
{
    Ground    = 0,   // Nur Boden
    Flying    = 1,   // Flug + Boden
    Aquatic   = 2,   // Wasser
    Amphibian = 3,   // Wasser + Boden
    Special   = 4    // Spezialfähigkeiten
}
```

### PetCommand (Enum)

```csharp
public enum PetCommandType : byte
{
    Follow     = 0,   // Dem Spieler folgen
    Stay       = 1,   // An Position bleiben
    Attack     = 2,   // Ziel angreifen
    Defensive  = 3,   // Nur bei Angriff reagieren
    Passive    = 4,   // Nicht angreifen
    Assist     = 5    // Spieler-Target angreifen
}
```

### MountErrorCode

```csharp
public enum MountErrorCode : byte
{
    Success           = 0,
    MountNotOwned     = 1,   // Mount nicht im Besitz
    InCombat          = 2,   // Spieler im Kampf
    ZoneRestricted    = 3,   // Mount hier nicht erlaubt
    OnCooldown        = 4,   // Noch auf Cooldown
    AlreadyMounted    = 5,   // Bereits auf Mount
    NotMounted        = 6,   // Nicht auf Mount (für Dismount)
    PetNotOwned       = 7,   // Pet nicht im Besitz
    PetNotActive      = 8,   // Kein aktives Pet
    InvalidTarget     = 9,   // Ungültiges Ziel
    NameInvalid       = 10,  // Ungültiger Pet-Name
    StableFull        = 11,  // Stall voll
    NotEnoughFood     = 12,  // Nicht genug Futter
    CompanionNotOwned = 13,  // Companion nicht im Besitz
    MaxFavorites      = 14   // Max. Favoriten erreicht
}
```

### MountDto

```csharp
[MessagePackObject]
public class MountDto
{
    [Key(0)] public MessageType Type => MessageType.MountListResponse;
    [Key(1)] public int MountId { get; set; }
    [Key(2)] public string Name { get; set; }          // "Swift Horse"
    [Key(3)] public MountRarity Rarity { get; set; }
    [Key(4)] public MountType MountType { get; set; }
    [Key(5)] public float SpeedBonus { get; set; }     // 1.6 = 60% faster
    [Key(6)] public int SpriteId { get; set; }
    [Key(7)] public bool IsFavorite { get; set; }
    [Key(8)] public long ObtainedAt { get; set; }      // Unix timestamp
}
```

### PetDto

```csharp
[MessagePackObject]
public class PetDto
{
    [Key(0)] public MessageType Type => MessageType.PetListResponse;
    [Key(1)] public int PetId { get; set; }
    [Key(2)] public string Name { get; set; }          // Custom name
    [Key(3)] public int TemplateId { get; set; }       // Pet-Typ-ID
    [Key(4)] public byte Level { get; set; }
    [Key(5)] public int Experience { get; set; }
    [Key(6)] public int Health { get; set; }
    [Key(7)] public int MaxHealth { get; set; }
    [Key(8)] public int Happiness { get; set; }        // 0-100
    [Key(9)] public PetCommandType CurrentCommand { get; set; }
    [Key(10)] public bool IsActive { get; set; }
    [Key(11)] public bool IsStabled { get; set; }
    [Key(12)] public int[] AbilityIds { get; set; }
}
```

### CompanionDto

```csharp
[MessagePackObject]
public class CompanionDto
{
    [Key(0)] public MessageType Type => MessageType.CompanionListResponse;
    [Key(1)] public int CompanionId { get; set; }
    [Key(2)] public string Name { get; set; }
    [Key(3)] public int SpriteId { get; set; }
    [Key(4)] public string[] InteractionEmotes { get; set; }
    [Key(5)] public bool IsActive { get; set; }
    [Key(6)] public long ObtainedAt { get; set; }
}
```

### Wichtige Konstanten

| Konstante | Wert | Beschreibung |
|-----------|------|--------------|
| `MOUNT_SUMMON_CAST_TIME` | 3000 ms | Cast-Zeit zum Beschwören |
| `MOUNT_SUMMON_COOLDOWN` | 0 ms | Cooldown nach Dismount |
| `MAX_MOUNT_FAVORITES` | 25 | Maximale Favoriten |
| `MAX_ACTIVE_PETS` | 1 | Gleichzeitig aktive Pets |
| `MAX_STABLE_SLOTS` | 50 | Pet-Stallplätze |
| `PET_HAPPINESS_DECAY` | 1/h | Happiness-Verlust pro Stunde |
| `PET_FEED_HAPPINESS` | +20 | Happiness pro Fütterung |
| `MAX_ACTIVE_COMPANIONS` | 1 | Gleichzeitig aktive Companions |
| `PET_NAME_MIN_LENGTH` | 2 | Minimale Namenslänge |
| `PET_NAME_MAX_LENGTH` | 16 | Maximale Namenslänge |

---

## Messages

### MountSummon (2000)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Response:** [MountSummonResult (2001)](#mountsummonresult-2001)

#### Beschreibung
Client beschwört ein Mount aus seiner Sammlung.

#### Payload

| Feld | Typ | Beschreibung |
|------|-----|--------------|
| `Type` | MessageType | `MountSummon` (2000) |
| `RequestId` | uint | Request-Korrelation |
| `MountId` | int | ID des zu beschwörenden Mounts |

#### Validierung (Server)
- Mount muss im Besitz des Spielers sein
- Spieler darf nicht im Kampf sein
- Zone muss Mounts erlauben
- Spieler darf nicht bereits gemounted sein
- 3 Sekunden Cast-Zeit

---

### MountSummonResult (2001)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja

#### Beschreibung
Server bestätigt oder verweigert Mount-Beschwörung.

#### Payload

| Feld | Typ | Beschreibung |
|------|-----|--------------|
| `Type` | MessageType | `MountSummonResult` (2001) |
| `RequestId` | uint | Korrelation zur Anfrage |
| `Success` | bool | Erfolg der Aktion |
| `ErrorCode` | MountErrorCode | Fehlergrund (wenn !Success) |
| `MountId` | int | ID des gemounteten Mounts |
| `SpeedBonus` | float | Aktiver Speed-Bonus |

---

### MountDismount (2002)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Response:** [MountSummonResult (2001)](#mountsummonresult-2001)

#### Beschreibung
Client steigt vom Mount ab.

#### Payload

| Feld | Typ | Beschreibung |
|------|-----|--------------|
| `Type` | MessageType | `MountDismount` (2002) |
| `RequestId` | uint | Request-Korrelation |

#### Validierung (Server)
- Spieler muss gemounted sein

#### Response
Verwendet `MountSummonResult` mit `ErrorCode = Success` oder `NotMounted`.

---

### MountListRequest (2003)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Response:** [MountListResponse (2004)](#mountlistresponse-2004)

#### Beschreibung
Client fordert seine Mount-Sammlung an.

#### Payload

| Feld | Typ | Beschreibung |
|------|-----|--------------|
| `Type` | MessageType | `MountListRequest` (2003) |
| `RequestId` | uint | Request-Korrelation |
| `IncludeFavoritesOnly` | bool | Nur Favoriten zurückgeben |

---

### MountListResponse (2004)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

#### Beschreibung
Server sendet Mount-Sammlung.

#### Payload

| Feld | Typ | Beschreibung |
|------|-----|--------------|
| `Type` | MessageType | `MountListResponse` (2004) |
| `RequestId` | uint | Korrelation zur Anfrage |
| `Mounts` | MountDto[] | Liste aller Mounts |
| `ActiveMountId` | int? | Aktuell aktives Mount (null = keins) |

---

### MountFavorite (2005)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Response:** [MountSummonResult (2001)](#mountsummonresult-2001)

#### Beschreibung
Client markiert ein Mount als Favorit.

#### Payload

| Feld | Typ | Beschreibung |
|------|-----|--------------|
| `Type` | MessageType | `MountFavorite` (2005) |
| `RequestId` | uint | Request-Korrelation |
| `MountId` | int | ID des Mounts |

#### Validierung (Server)
- Mount muss im Besitz sein
- Max 25 Favoriten

---

### MountUnfavorite (2006)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Response:** [MountSummonResult (2001)](#mountsummonresult-2001)

#### Beschreibung
Client entfernt Favorit-Markierung.

#### Payload

| Feld | Typ | Beschreibung |
|------|-----|--------------|
| `Type` | MessageType | `MountUnfavorite` (2006) |
| `RequestId` | uint | Request-Korrelation |
| `MountId` | int | ID des Mounts |

---

### MountRandomFavorite (2007)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Response:** [MountSummonResult (2001)](#mountsummonresult-2001)

#### Beschreibung
Client beschwört ein zufälliges Mount aus seinen Favoriten.

#### Payload

| Feld | Typ | Beschreibung |
|------|-----|--------------|
| `Type` | MessageType | `MountRandomFavorite` (2007) |
| `RequestId` | uint | Request-Korrelation |

#### Validierung (Server)
- Mindestens ein Favorit muss existieren
- Alle Standard-Mount-Validierungen

---

### PetSummon (2020)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Response:** [PetSummonResult (2021)](#petsummonresult-2021)

#### Beschreibung
Client beschwört ein Kampf-Pet.

#### Payload

| Feld | Typ | Beschreibung |
|------|-----|--------------|
| `Type` | MessageType | `PetSummon` (2020) |
| `RequestId` | uint | Request-Korrelation |
| `PetId` | int | ID des Pets |

#### Validierung (Server)
- Pet muss im Besitz sein
- Nicht bereits aktiv
- Max 1 aktives Pet

---

### PetSummonResult (2021)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja

#### Beschreibung
Server bestätigt Pet-Beschwörung.

#### Payload

| Feld | Typ | Beschreibung |
|------|-----|--------------|
| `Type` | MessageType | `PetSummonResult` (2021) |
| `RequestId` | uint | Korrelation zur Anfrage |
| `Success` | bool | Erfolg |
| `ErrorCode` | MountErrorCode | Fehlergrund |
| `Pet` | PetDto | Pet-Daten (wenn Success) |

---

### PetDismiss (2022)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Response:** [PetSummonResult (2021)](#petsummonresult-2021)

#### Beschreibung
Client schickt aktives Pet weg.

#### Payload

| Feld | Typ | Beschreibung |
|------|-----|--------------|
| `Type` | MessageType | `PetDismiss` (2022) |
| `RequestId` | uint | Request-Korrelation |

---

### PetRename (2023)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Response:** [PetSummonResult (2021)](#petsummonresult-2021)

#### Beschreibung
Client benennt Pet um.

#### Payload

| Feld | Typ | Beschreibung |
|------|-----|--------------|
| `Type` | MessageType | `PetRename` (2023) |
| `RequestId` | uint | Request-Korrelation |
| `PetId` | int | ID des Pets |
| `NewName` | string | Neuer Name (2-16 Zeichen) |

#### Validierung (Server)
- Name: 2-16 Zeichen
- Keine verbotenen Wörter
- Nur alphanumerisch + Leerzeichen

---

### PetCommand (2024)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Response:** [PetCommandResult (2025)](#petcommandresult-2025)

#### Beschreibung
Client gibt aktivem Pet einen Befehl.

#### Payload

| Feld | Typ | Beschreibung |
|------|-----|--------------|
| `Type` | MessageType | `PetCommand` (2024) |
| `RequestId` | uint | Request-Korrelation |
| `Command` | PetCommandType | Befehlstyp |
| `TargetEntityId` | int? | Ziel-Entity (für Attack/Assist) |

---

### PetCommandResult (2025)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja

#### Beschreibung
Server bestätigt Pet-Befehl.

#### Payload

| Feld | Typ | Beschreibung |
|------|-----|--------------|
| `Type` | MessageType | `PetCommandResult` (2025) |
| `RequestId` | uint | Korrelation zur Anfrage |
| `Success` | bool | Erfolg |
| `ErrorCode` | MountErrorCode | Fehlergrund |
| `CurrentCommand` | PetCommandType | Aktiver Befehl |

---

### PetUpdate (2026)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja

#### Beschreibung
Server sendet Pet-Status-Update (HP, Position, State).

#### Payload

| Feld | Typ | Beschreibung |
|------|-----|--------------|
| `Type` | MessageType | `PetUpdate` (2026) |
| `PetId` | int | Pet-ID |
| `Health` | int | Aktuelle HP |
| `Happiness` | int | Aktuelles Happiness |
| `X` | float | Position X |
| `Y` | float | Position Y |
| `State` | byte | Aktueller Zustand |

---

### PetFeed (2027)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Response:** [PetSummonResult (2021)](#petsummonresult-2021)

#### Beschreibung
Client füttert Pet (erhöht Happiness).

#### Payload

| Feld | Typ | Beschreibung |
|------|-----|--------------|
| `Type` | MessageType | `PetFeed` (2027) |
| `RequestId` | uint | Request-Korrelation |
| `PetId` | int | Pet-ID |
| `FoodItemId` | int | Item-ID des Futters |

#### Validierung (Server)
- Item muss vorhanden sein
- Item muss Futter sein
- Pet muss existieren

---

### PetTrain (2028)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Response:** [PetSummonResult (2021)](#petsummonresult-2021)

#### Beschreibung
Client trainiert Pet (erhöht XP/Level).

#### Payload

| Feld | Typ | Beschreibung |
|------|-----|--------------|
| `Type` | MessageType | `PetTrain` (2028) |
| `RequestId` | uint | Request-Korrelation |
| `PetId` | int | Pet-ID |
| `AbilityId` | int | Zu trainierende Fähigkeit |

---

### PetAbandon (2029)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Response:** [PetSummonResult (2021)](#petsummonresult-2021)

#### Beschreibung
Client gibt Pet permanent frei.

#### Payload

| Feld | Typ | Beschreibung |
|------|-----|--------------|
| `Type` | MessageType | `PetAbandon` (2029) |
| `RequestId` | uint | Request-Korrelation |
| `PetId` | int | Pet-ID |
| `Confirm` | bool | Bestätigung (muss `true` sein) |

#### Validierung (Server)
- Confirm muss `true` sein
- Pet darf nicht aktiv sein

---

### PetStable (2030)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Response:** [PetSummonResult (2021)](#petsummonresult-2021)

#### Beschreibung
Client stellt Pet in den Stall (Stablemaster-NPC).

#### Payload

| Feld | Typ | Beschreibung |
|------|-----|--------------|
| `Type` | MessageType | `PetStable` (2030) |
| `RequestId` | uint | Request-Korrelation |
| `PetId` | int | Pet-ID |

#### Validierung (Server)
- Spieler muss bei Stablemaster sein
- Stall darf nicht voll sein

---

### PetUnstable (2031)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Response:** [PetSummonResult (2021)](#petsummonresult-2021)

#### Beschreibung
Client holt Pet aus dem Stall.

#### Payload

| Feld | Typ | Beschreibung |
|------|-----|--------------|
| `Type` | MessageType | `PetUnstable` (2031) |
| `RequestId` | uint | Request-Korrelation |
| `PetId` | int | Pet-ID |

#### Validierung (Server)
- Spieler muss bei Stablemaster sein
- Pet muss im Stall sein

---

### PetListRequest (2032)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Response:** [PetListResponse (2033)](#petlistresponse-2033)

#### Beschreibung
Client fordert Pet-Liste an.

#### Payload

| Feld | Typ | Beschreibung |
|------|-----|--------------|
| `Type` | MessageType | `PetListRequest` (2032) |
| `RequestId` | uint | Request-Korrelation |
| `IncludeStabled` | bool | Stall-Pets mit einschließen |

---

### PetListResponse (2033)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

#### Beschreibung
Server sendet Pet-Liste.

#### Payload

| Feld | Typ | Beschreibung |
|------|-----|--------------|
| `Type` | MessageType | `PetListResponse` (2033) |
| `RequestId` | uint | Korrelation zur Anfrage |
| `Pets` | PetDto[] | Liste aller Pets |
| `StableSlotsFree` | int | Freie Stallplätze |
| `StableSlotsTotal` | int | Gesamte Stallplätze |

---

### CompanionSummon (2050)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Response:** [CompanionListResponse (2054)](#companionlistresponse-2054)

#### Beschreibung
Client beschwört kosmetischen Companion.

#### Payload

| Feld | Typ | Beschreibung |
|------|-----|--------------|
| `Type` | MessageType | `CompanionSummon` (2050) |
| `RequestId` | uint | Request-Korrelation |
| `CompanionId` | int | Companion-ID |

---

### CompanionDismiss (2051)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Response:** [CompanionListResponse (2054)](#companionlistresponse-2054)

#### Beschreibung
Client schickt Companion weg.

#### Payload

| Feld | Typ | Beschreibung |
|------|-----|--------------|
| `Type` | MessageType | `CompanionDismiss` (2051) |
| `RequestId` | uint | Request-Korrelation |

---

### CompanionInteract (2052)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Response:** [CompanionListResponse (2054)](#companionlistresponse-2054)

#### Beschreibung
Client interagiert mit aktivem Companion (Emote/Animation).

#### Payload

| Feld | Typ | Beschreibung |
|------|-----|--------------|
| `Type` | MessageType | `CompanionInteract` (2052) |
| `RequestId` | uint | Request-Korrelation |
| `InteractionType` | byte | Art der Interaktion (0=Pet, 1=Play, 2=Dance) |

---

### CompanionListRequest (2053)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Response:** [CompanionListResponse (2054)](#companionlistresponse-2054)

#### Beschreibung
Client fordert Companion-Sammlung an.

#### Payload

| Feld | Typ | Beschreibung |
|------|-----|--------------|
| `Type` | MessageType | `CompanionListRequest` (2053) |
| `RequestId` | uint | Request-Korrelation |

---

### CompanionListResponse (2054)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

#### Beschreibung
Server sendet Companion-Sammlung.

#### Payload

| Feld | Typ | Beschreibung |
|------|-----|--------------|
| `Type` | MessageType | `CompanionListResponse` (2054) |
| `RequestId` | uint | Korrelation zur Anfrage |
| `Companions` | CompanionDto[] | Liste aller Companions |
| `ActiveCompanionId` | int? | Aktuell aktiver Companion |

---

## Anhang

### MessageType Enum

Exakte Reihenfolge aus `MessageType.cs`:

```csharp
// MOUNTS / PETS / COMPANIONS (2000-2099)
MountSummon = 2000,
MountSummonResult = 2001,
MountDismount = 2002,
MountListRequest = 2003,
MountListResponse = 2004,
MountFavorite = 2005,
MountUnfavorite = 2006,
MountRandomFavorite = 2007,
PetSummon = 2020,
PetSummonResult = 2021,
PetDismiss = 2022,
PetRename = 2023,
PetCommand = 2024,
PetCommandResult = 2025,
PetUpdate = 2026,
PetFeed = 2027,
PetTrain = 2028,
PetAbandon = 2029,
PetStable = 2030,
PetUnstable = 2031,
PetListRequest = 2032,
PetListResponse = 2033,
CompanionSummon = 2050,
CompanionDismiss = 2051,
CompanionInteract = 2052,
CompanionListRequest = 2053,
CompanionListResponse = 2054,
```

### Request/Response Paare

| # | Request | Response |
|---|---------|----------|
| 1 | MountSummon (2000) | MountSummonResult (2001) |
| 2 | MountDismount (2002) | MountSummonResult (2001) |
| 3 | MountListRequest (2003) | MountListResponse (2004) |
| 4 | MountFavorite (2005) | MountSummonResult (2001) |
| 5 | MountUnfavorite (2006) | MountSummonResult (2001) |
| 6 | MountRandomFavorite (2007) | MountSummonResult (2001) |
| 7 | PetSummon (2020) | PetSummonResult (2021) |
| 8 | PetDismiss (2022) | PetSummonResult (2021) |
| 9 | PetRename (2023) | PetSummonResult (2021) |
| 10 | PetCommand (2024) | PetCommandResult (2025) |
| 11 | PetFeed (2027) | PetSummonResult (2021) |
| 12 | PetTrain (2028) | PetSummonResult (2021) |
| 13 | PetAbandon (2029) | PetSummonResult (2021) |
| 14 | PetStable (2030) | PetSummonResult (2021) |
| 15 | PetUnstable (2031) | PetSummonResult (2021) |
| 16 | PetListRequest (2032) | PetListResponse (2033) |
| 17 | CompanionSummon (2050) | CompanionListResponse (2054) |
| 18 | CompanionDismiss (2051) | CompanionListResponse (2054) |
| 19 | CompanionInteract (2052) | CompanionListResponse (2054) |
| 20 | CompanionListRequest (2053) | CompanionListResponse (2054) |

**Server-initiated Messages (Fire-and-Forget):**
- PetUpdate (2026)

### Datei-Struktur

```
shared/Mmo.Shared/
├── Messaging/
│   ├── Enums/
│   │   └── MessageType.cs          # MountSummon=2000 ... CompanionListResponse=2054
│   └── DTOs/
│       └── Mount/
│           ├── MountSummonMessage.cs
│           ├── MountSummonResultMessage.cs
│           ├── MountListRequestMessage.cs
│           ├── MountListResponseMessage.cs
│           ├── PetSummonMessage.cs
│           ├── PetCommandMessage.cs
│           └── ...
└── Enums/
    ├── MountRarity.cs
    ├── MountType.cs
    ├── PetCommandType.cs
    └── MountErrorCode.cs
```

---

**Letzte Aktualisierung**: 2026-01-02  
**Version**: 3.0.0  
**Status**: ✅ Aligned mit MessageType Enum (27 Messages)

[← Zurück zur Übersicht](Message-Reference.md)

Source: docs/03-messages/20-mount.md
