# 👥 Social Messages (Friends, Block, Ignore) (2100-2199)

**Kategorie:** 21  
**Range:** 2100-2132  
**Phase:** Prototyp  
**Status:** 🟢 In Entwicklung

[← Zurück zur Übersicht](README.md)

---

## 📋 Inhaltsverzeichnis

- [🔄 Social Flow](#-social-flow)
  - [Server-Authoritative Architecture](#server-authoritative-architecture)
  - [Friend-Request Flow](#friend-request-flow)
  - [Block-System Flow](#block-system-flow)
- [🧱 DTOs / Enums](#-dtos--enums)
  - [FriendStatus](#friendstatus)
  - [SocialErrorCode](#socialerrorcode)
  - [FriendInfoDto](#friendinfodto)
  - [BlockedPlayerDto](#blockedplayerdto)
  - [WhoResultDto](#whoresultdto)
  - [Wichtige Konstanten](#wichtige-konstanten)
- [📩 Messages](#-messages)
  - [FriendRequest (2100)](#friendrequest-2100)
  - [FriendRequestResult (2101)](#friendrequestresult-2101)
  - [FriendAccept (2102)](#friendaccept-2102)
  - [FriendDecline (2103)](#frienddecline-2103)
  - [FriendRemove (2104)](#friendremove-2104)
  - [FriendListRequest (2105)](#friendlistrequest-2105)
  - [FriendListResponse (2106)](#friendlistresponse-2106)
  - [FriendOnline (2107)](#friendonline-2107)
  - [FriendOffline (2108)](#friendoffline-2108)
  - [FriendUpdate (2109)](#friendupdate-2109)
  - [FriendNote (2110)](#friendnote-2110)
  - [BlockPlayer (2120)](#blockplayer-2120)
  - [BlockPlayerResult (2121)](#blockplayerresult-2121)
  - [UnblockPlayer (2122)](#unblockplayer-2122)
  - [BlockListRequest (2123)](#blocklistrequest-2123)
  - [BlockListResponse (2124)](#blocklistresponse-2124)
  - [IgnorePlayer (2125)](#ignoreplayer-2125)
  - [UnignorePlayer (2126)](#unignoreplayer-2126)
  - [WhoRequest (2130)](#whorequest-2130)
  - [WhoResponse (2131)](#whoresponse-2131)
  - [PlayerLocation (2132)](#playerlocation-2132)
- [📎 Anhang](#-anhang)
  - [MessageType Enum](#messagetype-enum)
  - [Request/Response Paare](#requestresponse-paare)

---

## 🔄 Social Flow

### Server-Authoritative Architecture

Das Social-System ist vollständig Server-Authoritative:

```
┌─────────────────────────────────────────────────────────────────────┐
│                    SERVER (Social-Authority)                        │
│  ┌───────────────────────────────────────────────────────────────┐  │
│  │                     Social-Service                            │  │
│  │  ┌─────────────┐  ┌─────────────┐  ┌─────────────────────┐   │  │
│  │  │ Friend-List │  │ Block-List  │  │ Online-Status-Cache │   │  │
│  │  │   Manager   │  │   Manager   │  │       (Redis)       │   │  │
│  │  └─────────────┘  └─────────────┘  └─────────────────────┘   │  │
│  │         │                │                   │                │  │
│  │         ▼                ▼                   ▼                │  │
│  │  ┌───────────────────────────────────────────────────────┐   │  │
│  │  │                   Validation-Layer                    │   │  │
│  │  │   • Block-Check    • Max-Friends    • Privacy-Rules   │   │  │
│  │  └───────────────────────────────────────────────────────┘   │  │
│  └───────────────────────────────────────────────────────────────┘  │
│         │                                                            │
│         ▼                                                            │
│  ┌───────────────┐                         ┌───────────────┐        │
│  │ Database (SQL)│                         │ Redis Pub/Sub │        │
│  │ • Friends     │                         │ Online-Events │        │
│  │ • Blocked     │                         │ Cross-Server  │        │
│  │ • Ignored     │                         │               │        │
│  └───────────────┘                         └───────────────┘        │
└─────────────────────────────────────────────────────────────────────┘
                              ▲
                              │
                    ┌─────────┴─────────┐
                    │                   │
              ┌─────┴─────┐       ┌─────┴─────┐
              │  Client A │       │  Client B │
              │ (Request) │       │ (Target)  │
              └───────────┘       └───────────┘
```

### Friend-Request Flow

```
Client A (Requester)           Server                    Client B (Target)
       │                          │                             │
       │ FriendRequest (2100)     │                             │
       │ TargetName="PlayerB"     │                             │
       │─────────────────────────►│                             │
       │                          │                             │
       │                          │─── Validate: ──────────────►│
       │                          │    • A not blocked by B     │
       │                          │    • B not blocked by A     │
       │                          │    • A friend-list not full │
       │                          │    • B friend-list not full │
       │                          │    • Not already friends    │
       │                          │                             │
       │ FriendRequestResult      │                             │
       │ (2101) Success=true      │                             │
       │◄─────────────────────────│                             │
       │                          │  FriendRequestResult (2101) │
       │                          │  IncomingRequest=true       │
       │                          │────────────────────────────►│
       │                          │                             │
       │                          │        [ TARGET DECIDES ]   │
       │                          │                             │
       │                          │     FriendAccept (2102)     │
       │                          │◄────────────────────────────│
       │                          │                             │
       │                          │─── Add to Both Lists ──────►│
       │                          │                             │
       │  FriendListResponse      │  FriendListResponse (2106)  │
       │  (2106) + new friend     │  + new friend               │
       │◄─────────────────────────│────────────────────────────►│
       │                          │                             │
```

### Block-System Flow

```
Client A (Blocker)              Server                    Client B (Blocked)
       │                          │                             │
       │ BlockPlayer (2120)       │                             │
       │ TargetName="SpamPlayer"  │                             │
       │─────────────────────────►│                             │
       │                          │                             │
       │                          │─── Validate: ──────────────►│
       │                          │    • Player exists          │
       │                          │    • Not already blocked    │
       │                          │    • Block-list not full    │
       │                          │                             │
       │                          │─── Effects: ───────────────►│
       │                          │    • Remove from friends    │
       │                          │    • Cancel pending req.    │
       │                          │    • Filter future msgs     │
       │                          │                             │
       │ BlockPlayerResult (2121) │                             │
       │ Success=true             │                             │
       │◄─────────────────────────│                             │
       │                          │                             │
       │                          │   [B receives NO notification -
       │                          │    Privacy protection]       │
       │                          │                             │
```

---

## 🧱 DTOs / Enums

### FriendStatus

```csharp
public enum FriendStatus : byte
{
    Pending = 0,        // Request sent, waiting for response
    Accepted = 1,       // Friends
    Declined = 2,       // Request was declined (internal)
    Removed = 3         // Was friend, now removed (internal)
}
```

### SocialErrorCode

```csharp
public enum SocialErrorCode : byte
{
    None = 0,
    PlayerNotFound = 1,
    AlreadyFriends = 2,
    AlreadyBlocked = 3,
    BlockedByTarget = 4,
    FriendListFull = 5,
    TargetFriendListFull = 6,
    BlockListFull = 7,
    IgnoreListFull = 8,
    CannotTargetSelf = 9,
    RequestPending = 10,
    RequestExpired = 11,
    NotFriends = 12,
    NotBlocked = 13,
    NotIgnored = 14,
    TooManyResults = 15
}
```

### FriendInfoDto

```csharp
[MessagePackObject]
public class FriendInfoDto
{
    [Key(0)] public long CharacterId { get; set; }
    [Key(1)] public string CharacterName { get; set; }
    [Key(2)] public bool IsOnline { get; set; }
    [Key(3)] public int Level { get; set; }
    [Key(4)] public int ZoneId { get; set; }          // 0 if offline/hidden
    [Key(5)] public string ZoneName { get; set; }     // "" if offline/hidden
    [Key(6)] public string Note { get; set; }         // Private note (max 100)
    [Key(7)] public long LastSeen { get; set; }       // Unix timestamp
    [Key(8)] public FriendStatus Status { get; set; } // Pending/Accepted
}
```

### BlockedPlayerDto

```csharp
[MessagePackObject]
public class BlockedPlayerDto
{
    [Key(0)] public long CharacterId { get; set; }
    [Key(1)] public string CharacterName { get; set; }
    [Key(2)] public long BlockedAt { get; set; }      // Unix timestamp
}
```

### WhoResultDto

```csharp
[MessagePackObject]
public class WhoResultDto
{
    [Key(0)] public long CharacterId { get; set; }
    [Key(1)] public string CharacterName { get; set; }
    [Key(2)] public int Level { get; set; }
    [Key(3)] public string ClassName { get; set; }
    [Key(4)] public int ZoneId { get; set; }
    [Key(5)] public string ZoneName { get; set; }
    [Key(6)] public string GuildName { get; set; }    // "" if no guild
}
```

### Wichtige Konstanten

| Konstante | Wert | Beschreibung |
|-----------|------|--------------|
| MAX_FRIENDS | 50 | Maximale Anzahl Freunde |
| MAX_BLOCKED | 100 | Maximale Anzahl geblockte Spieler |
| MAX_IGNORED | 100 | Maximale Anzahl ignorierte Spieler |
| FRIEND_REQUEST_TIMEOUT_SEC | 300 | 5 Minuten Request-Timeout |
| FRIEND_NOTE_MAX_LENGTH | 100 | Maximale Notiz-Länge |
| WHO_MAX_RESULTS | 50 | Maximale Who-Ergebnisse |
| ONLINE_STATUS_UPDATE_INTERVAL_MS | 60000 | 1 Minute Update-Interval |

---

## 📩 Messages

### FriendRequest (2100)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

#### Beschreibung
Sendet Friend-Request an anderen Spieler. Server validiert und sendet Prompt an Target.

#### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `FriendRequest` (2100) | Ja |
| RequestId | uint | Client-generierte Request-ID | Ja |
| TargetCharacterName | string | Name des Spielers (3-16 Zeichen) | Ja* |
| TargetEntityId | int | Entity-ID (wenn online + sichtbar) | Ja* |

\* Entweder Name oder EntityId angeben

#### Erwartete Response
- **Bei Erfolg:** `FriendRequestResult` (2101) mit Success=true
- **Bei Fehler:** `FriendRequestResult` (2101) mit ErrorCode

#### Beispiel Payload
```csharp
var request = new FriendRequestMessage
{
    Type = MessageType.FriendRequest,
    RequestId = 12345,
    TargetCharacterName = "Legolas"
};
```

---

### FriendRequestResult (2101)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

#### Beschreibung
Result des Friend-Requests. Wird sowohl an Requester (Bestätigung) als auch an Target (Incoming-Request) gesendet.

#### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `FriendRequestResult` (2101) | Ja |
| RequestId | uint | Echo der Request-ID | Ja |
| Success | bool | Request erfolgreich? | Ja |
| ErrorCode | SocialErrorCode | Fehlercode bei Fehler | Ja |
| TargetName | string | Name des Targets | Ja |
| IsIncoming | bool | true = Du bist das Target | Ja |
| RequesterCharacterId | long | Für Target: Requester-ID | Nein |
| RequesterName | string | Für Target: Requester-Name | Nein |
| IsPending | bool | Request wartet (Target offline) | Ja |

#### Beispiel Payload (für Requester)
```csharp
var result = new FriendRequestResultMessage
{
    Type = MessageType.FriendRequestResult,
    RequestId = 12345,
    Success = true,
    ErrorCode = SocialErrorCode.None,
    TargetName = "Legolas",
    IsIncoming = false,
    IsPending = true  // Target ist offline
};
```

#### Beispiel Payload (für Target)
```csharp
var incoming = new FriendRequestResultMessage
{
    Type = MessageType.FriendRequestResult,
    RequestId = 0,  // Keine RequestId für Target
    Success = true,
    ErrorCode = SocialErrorCode.None,
    TargetName = "",
    IsIncoming = true,
    RequesterCharacterId = 98765,
    RequesterName = "Aragorn",
    IsPending = false
};
```

---

### FriendAccept (2102)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

#### Beschreibung
Akzeptiert einen eingehenden Friend-Request.

#### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `FriendAccept` (2102) | Ja |
| RequestId | uint | Client-generierte Request-ID | Ja |
| RequesterCharacterId | long | Character-ID des Requesters | Ja |

#### Erwartete Response
- **Immer:** `FriendListResponse` (2106) mit aktualisierter Liste

#### Beispiel Payload
```csharp
var accept = new FriendAcceptMessage
{
    Type = MessageType.FriendAccept,
    RequestId = 12346,
    RequesterCharacterId = 98765
};
```

---

### FriendDecline (2103)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

#### Beschreibung
Lehnt einen eingehenden Friend-Request ab. Der Requester wird NICHT benachrichtigt (Privacy).

#### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `FriendDecline` (2103) | Ja |
| RequestId | uint | Client-generierte Request-ID | Ja |
| RequesterCharacterId | long | Character-ID des Requesters | Ja |

#### Erwartete Response
- **Keine direkte Response** - Server speichert nur den Decline

#### Beispiel Payload
```csharp
var decline = new FriendDeclineMessage
{
    Type = MessageType.FriendDecline,
    RequestId = 12347,
    RequesterCharacterId = 98765
};
```

---

### FriendRemove (2104)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

#### Beschreibung
Entfernt einen Freund aus der Friend-List. Beide Spieler werden aus der jeweils anderen Liste entfernt. Der andere Spieler wird NICHT benachrichtigt (Privacy).

#### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `FriendRemove` (2104) | Ja |
| RequestId | uint | Client-generierte Request-ID | Ja |
| FriendCharacterId | long | Character-ID zum Entfernen | Ja |

#### Erwartete Response
- **Immer:** `FriendListResponse` (2106) mit aktualisierter Liste

#### Beispiel Payload
```csharp
var remove = new FriendRemoveMessage
{
    Type = MessageType.FriendRemove,
    RequestId = 12348,
    FriendCharacterId = 98765
};
```

---

### FriendListRequest (2105)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

#### Beschreibung
Fordert die komplette Friend-List an. Wird automatisch nach Login gesendet.

#### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `FriendListRequest` (2105) | Ja |
| RequestId | uint | Client-generierte Request-ID | Ja |

#### Erwartete Response
- **Immer:** `FriendListResponse` (2106)

#### Beispiel Payload
```csharp
var request = new FriendListRequestMessage
{
    Type = MessageType.FriendListRequest,
    RequestId = 12349
};
```

---

### FriendListResponse (2106)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

#### Beschreibung
Komplette Friend-List mit Online-Status, Pending-Requests und Notizen.

#### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `FriendListResponse` (2106) | Ja |
| RequestId | uint | Echo der Request-ID | Ja |
| Friends | List\<FriendInfoDto\> | Alle Freunde | Ja |
| PendingRequests | List\<FriendInfoDto\> | Eingehende Requests | Ja |

#### Beispiel Payload
```csharp
var response = new FriendListResponseMessage
{
    Type = MessageType.FriendListResponse,
    RequestId = 12349,
    Friends = new List<FriendInfoDto>
    {
        new FriendInfoDto
        {
            CharacterId = 98765,
            CharacterName = "Legolas",
            IsOnline = true,
            Level = 12,
            ZoneId = 1002,
            ZoneName = "Mirkwood Forest",
            Note = "Great archer",
            LastSeen = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            Status = FriendStatus.Accepted
        }
    },
    PendingRequests = new List<FriendInfoDto>()
};
```

---

### FriendOnline (2107)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

#### Beschreibung
Benachrichtigung dass ein Freund online gekommen ist.

#### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `FriendOnline` (2107) | Ja |
| CharacterId | long | Friend Character-ID | Ja |
| CharacterName | string | Character-Name | Ja |
| Level | int | Aktuelles Level | Ja |

#### Beispiel Payload
```csharp
var online = new FriendOnlineMessage
{
    Type = MessageType.FriendOnline,
    CharacterId = 98765,
    CharacterName = "Legolas",
    Level = 12
};
```

---

### FriendOffline (2108)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

#### Beschreibung
Benachrichtigung dass ein Freund offline gegangen ist.

#### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `FriendOffline` (2108) | Ja |
| CharacterId | long | Friend Character-ID | Ja |

#### Beispiel Payload
```csharp
var offline = new FriendOfflineMessage
{
    Type = MessageType.FriendOffline,
    CharacterId = 98765
};
```

---

### FriendUpdate (2109)

**Richtung:** 📥 Server → Client  
**Frequenz:** Gelegentlich  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

#### Beschreibung
Update für einen Online-Freund (Level-Up, Zone-Wechsel).

#### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `FriendUpdate` (2109) | Ja |
| CharacterId | long | Friend Character-ID | Ja |
| Level | int | Neues Level (0 = keine Änderung) | Ja |
| ZoneId | int | Neue Zone-ID (0 = keine Änderung) | Ja |
| ZoneName | string | Neue Zone-Name | Ja |

#### Beispiel Payload
```csharp
var update = new FriendUpdateMessage
{
    Type = MessageType.FriendUpdate,
    CharacterId = 98765,
    Level = 13,  // Level-Up!
    ZoneId = 1003,
    ZoneName = "Rivendell"
};
```

---

### FriendNote (2110)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

#### Beschreibung
Setzt oder aktualisiert eine private Notiz für einen Freund. Nur für den eigenen Client sichtbar.

#### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `FriendNote` (2110) | Ja |
| RequestId | uint | Client-generierte Request-ID | Ja |
| FriendCharacterId | long | Friend Character-ID | Ja |
| Note | string | Notiz-Text (max 100 Zeichen) | Ja |

#### Erwartete Response
- **Keine direkte Response** - Server speichert die Notiz

#### Beispiel Payload
```csharp
var note = new FriendNoteMessage
{
    Type = MessageType.FriendNote,
    RequestId = 12350,
    FriendCharacterId = 98765,
    Note = "Best DPS in guild"
};
```

---

### BlockPlayer (2120)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

#### Beschreibung
Blockiert einen Spieler komplett. Alle Kommunikation wird unterbunden:
- Chat-Messages werden gefiltert
- Friend-Requests werden automatisch abgelehnt
- Party-Invites werden automatisch abgelehnt
- Trade-Requests werden automatisch abgelehnt
- Duel-Requests werden automatisch abgelehnt

#### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `BlockPlayer` (2120) | Ja |
| RequestId | uint | Client-generierte Request-ID | Ja |
| TargetCharacterName | string | Zu blockierender Spieler | Ja |

#### Erwartete Response
- **Bei Erfolg:** `BlockPlayerResult` (2121) mit Success=true
- **Bei Fehler:** `BlockPlayerResult` (2121) mit ErrorCode

#### Beispiel Payload
```csharp
var block = new BlockPlayerMessage
{
    Type = MessageType.BlockPlayer,
    RequestId = 12351,
    TargetCharacterName = "Spammer123"
};
```

---

### BlockPlayerResult (2121)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

#### Beschreibung
Result des Block-Requests.

#### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `BlockPlayerResult` (2121) | Ja |
| RequestId | uint | Echo der Request-ID | Ja |
| Success | bool | Block erfolgreich? | Ja |
| ErrorCode | SocialErrorCode | Fehlercode bei Fehler | Ja |
| BlockedName | string | Geblockter Spieler-Name | Ja |

#### Beispiel Payload
```csharp
var result = new BlockPlayerResultMessage
{
    Type = MessageType.BlockPlayerResult,
    RequestId = 12351,
    Success = true,
    ErrorCode = SocialErrorCode.None,
    BlockedName = "Spammer123"
};
```

---

### UnblockPlayer (2122)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

#### Beschreibung
Entfernt einen Spieler von der Block-Liste.

#### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `UnblockPlayer` (2122) | Ja |
| RequestId | uint | Client-generierte Request-ID | Ja |
| TargetCharacterId | long | Character-ID zum Entblocken | Ja |

#### Erwartete Response
- **Bei Erfolg:** `BlockListResponse` (2124) mit aktualisierter Liste
- **Bei Fehler:** `BlockListResponse` (2124) mit ErrorCode

#### Beispiel Payload
```csharp
var unblock = new UnblockPlayerMessage
{
    Type = MessageType.UnblockPlayer,
    RequestId = 12352,
    TargetCharacterId = 99999
};
```

---

### BlockListRequest (2123)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

#### Beschreibung
Fordert die Block-Liste an.

#### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `BlockListRequest` (2123) | Ja |
| RequestId | uint | Client-generierte Request-ID | Ja |

#### Erwartete Response
- **Immer:** `BlockListResponse` (2124)

#### Beispiel Payload
```csharp
var request = new BlockListRequestMessage
{
    Type = MessageType.BlockListRequest,
    RequestId = 12353
};
```

---

### BlockListResponse (2124)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

#### Beschreibung
Komplette Block-Liste.

#### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `BlockListResponse` (2124) | Ja |
| RequestId | uint | Echo der Request-ID | Ja |
| BlockedPlayers | List\<BlockedPlayerDto\> | Alle geblockten Spieler | Ja |
| ErrorCode | SocialErrorCode | Fehlercode (falls applicable) | Ja |

#### Beispiel Payload
```csharp
var response = new BlockListResponseMessage
{
    Type = MessageType.BlockListResponse,
    RequestId = 12353,
    BlockedPlayers = new List<BlockedPlayerDto>
    {
        new BlockedPlayerDto
        {
            CharacterId = 99999,
            CharacterName = "Spammer123",
            BlockedAt = DateTimeOffset.UtcNow.AddDays(-7).ToUnixTimeSeconds()
        }
    },
    ErrorCode = SocialErrorCode.None
};
```

---

### IgnorePlayer (2125)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

#### Beschreibung
Ignoriert einen Spieler (nur Chat). Weniger restriktiv als Block:
- Chat-Messages werden gefiltert
- Andere Interaktionen (Trade, Party, etc.) bleiben möglich

#### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `IgnorePlayer` (2125) | Ja |
| RequestId | uint | Client-generierte Request-ID | Ja |
| TargetCharacterName | string | Zu ignorierender Spieler | Ja |

#### Erwartete Response
- **Keine direkte Response** - Server speichert und bestätigt implizit

#### Beispiel Payload
```csharp
var ignore = new IgnorePlayerMessage
{
    Type = MessageType.IgnorePlayer,
    RequestId = 12354,
    TargetCharacterName = "AnnoyingPlayer"
};
```

---

### UnignorePlayer (2126)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

#### Beschreibung
Entfernt einen Spieler von der Ignore-Liste.

#### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `UnignorePlayer` (2126) | Ja |
| RequestId | uint | Client-generierte Request-ID | Ja |
| TargetCharacterId | long | Character-ID zum Ent-Ignorieren | Ja |

#### Erwartete Response
- **Keine direkte Response** - Server speichert und bestätigt implizit

#### Beispiel Payload
```csharp
var unignore = new UnignorePlayerMessage
{
    Type = MessageType.UnignorePlayer,
    RequestId = 12355,
    TargetCharacterId = 88888
};
```

---

### WhoRequest (2130)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

#### Beschreibung
Sucht nach Online-Spielern basierend auf verschiedenen Kriterien.

#### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `WhoRequest` (2130) | Ja |
| RequestId | uint | Client-generierte Request-ID | Ja |
| SearchName | string | Name-Filter (Prefix-Match) | Nein |
| MinLevel | int | Minimales Level (0 = ignorieren) | Ja |
| MaxLevel | int | Maximales Level (0 = ignorieren) | Ja |
| ZoneId | int | Zone-Filter (0 = alle) | Ja |
| GuildName | string | Guild-Filter | Nein |
| ClassName | string | Klassen-Filter | Nein |

#### Erwartete Response
- **Immer:** `WhoResponse` (2131)

#### Beispiel Payload
```csharp
var who = new WhoRequestMessage
{
    Type = MessageType.WhoRequest,
    RequestId = 12356,
    SearchName = "Leg",  // Prefix-Match
    MinLevel = 10,
    MaxLevel = 20,
    ZoneId = 0,  // Alle Zonen
    GuildName = "",
    ClassName = ""
};
```

---

### WhoResponse (2131)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

#### Beschreibung
Ergebnisse der Who-Suche.

#### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `WhoResponse` (2131) | Ja |
| RequestId | uint | Echo der Request-ID | Ja |
| Results | List\<WhoResultDto\> | Gefundene Spieler | Ja |
| TotalOnline | int | Gesamtzahl Online (für Stats) | Ja |
| TruncatedResults | bool | Mehr als MAX_RESULTS gefunden | Ja |

#### Beispiel Payload
```csharp
var response = new WhoResponseMessage
{
    Type = MessageType.WhoResponse,
    RequestId = 12356,
    Results = new List<WhoResultDto>
    {
        new WhoResultDto
        {
            CharacterId = 98765,
            CharacterName = "Legolas",
            Level = 12,
            ClassName = "Ranger",
            ZoneId = 1002,
            ZoneName = "Mirkwood Forest",
            GuildName = "Fellowship"
        }
    },
    TotalOnline = 347,
    TruncatedResults = false
};
```

---

### PlayerLocation (2132)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

#### Beschreibung
Sendet die Location eines bestimmten Spielers (für Friends-Tracking oder "/locate"-Befehl).

#### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `PlayerLocation` (2132) | Ja |
| CharacterId | long | Gesuchter Character | Ja |
| CharacterName | string | Character-Name | Ja |
| ZoneId | int | Aktuelle Zone-ID | Ja |
| ZoneName | string | Zone-Name | Ja |
| IsOnline | bool | Spieler online? | Ja |

#### Beispiel Payload
```csharp
var location = new PlayerLocationMessage
{
    Type = MessageType.PlayerLocation,
    CharacterId = 98765,
    CharacterName = "Legolas",
    ZoneId = 1002,
    ZoneName = "Mirkwood Forest",
    IsOnline = true
};
```

---

## 📎 Anhang

### MessageType Enum

```csharp
// Aus MessageType.cs - Reihenfolge exakt wie im Code
// Kategorie 21: Social (2100-2132)

FriendRequest = 2100,
FriendRequestResult = 2101,
FriendAccept = 2102,
FriendDecline = 2103,
FriendRemove = 2104,
FriendListRequest = 2105,
FriendListResponse = 2106,
FriendOnline = 2107,
FriendOffline = 2108,
FriendUpdate = 2109,
FriendNote = 2110,
BlockPlayer = 2120,
BlockPlayerResult = 2121,
UnblockPlayer = 2122,
BlockListRequest = 2123,
BlockListResponse = 2124,
IgnorePlayer = 2125,
UnignorePlayer = 2126,
WhoRequest = 2130,
WhoResponse = 2131,
PlayerLocation = 2132,
```

### Request/Response Paare

| Request | ID | Response | ID | Korrelation |
|---------|-----|----------|-----|-------------|
| FriendRequest | 2100 | FriendRequestResult | 2101 | RequestId |
| FriendAccept | 2102 | FriendListResponse | 2106 | RequestId |
| FriendRemove | 2104 | FriendListResponse | 2106 | RequestId |
| FriendListRequest | 2105 | FriendListResponse | 2106 | RequestId |
| BlockPlayer | 2120 | BlockPlayerResult | 2121 | RequestId |
| UnblockPlayer | 2122 | BlockListResponse | 2124 | RequestId |
| BlockListRequest | 2123 | BlockListResponse | 2124 | RequestId |
| WhoRequest | 2130 | WhoResponse | 2131 | RequestId |

### Fire-and-Forget Messages (Client → Server)

| Message | ID | Beschreibung |
|---------|----|--------------|
| FriendDecline | 2103 | Silent decline |
| FriendNote | 2110 | Notiz speichern |
| IgnorePlayer | 2125 | Ignore ohne Response |
| UnignorePlayer | 2126 | Unignore ohne Response |

### Server-initiierte Messages (Server → Client)

| Message | ID | Trigger |
|---------|----|---------|
| FriendOnline | 2107 | Friend logged in |
| FriendOffline | 2108 | Friend logged out |
| FriendUpdate | 2109 | Friend level/zone change |
| PlayerLocation | 2132 | Location tracking |

### Datei-Struktur

```
shared/Mmo.Shared/Messaging/
├── Enums/
│   └── MessageType.cs          # Social Messages 2100-2132
├── Messages/Social/
│   ├── FriendRequestMessage.cs
│   ├── FriendRequestResultMessage.cs
│   ├── FriendAcceptMessage.cs
│   ├── FriendDeclineMessage.cs
│   ├── FriendRemoveMessage.cs
│   ├── FriendListRequestMessage.cs
│   ├── FriendListResponseMessage.cs
│   ├── FriendOnlineMessage.cs
│   ├── FriendOfflineMessage.cs
│   ├── FriendUpdateMessage.cs
│   ├── FriendNoteMessage.cs
│   ├── BlockPlayerMessage.cs
│   ├── BlockPlayerResultMessage.cs
│   ├── UnblockPlayerMessage.cs
│   ├── BlockListRequestMessage.cs
│   ├── BlockListResponseMessage.cs
│   ├── IgnorePlayerMessage.cs
│   ├── UnignorePlayerMessage.cs
│   ├── WhoRequestMessage.cs
│   ├── WhoResponseMessage.cs
│   └── PlayerLocationMessage.cs
└── DTOs/
    ├── FriendInfoDto.cs
    ├── BlockedPlayerDto.cs
    └── WhoResultDto.cs
```

---

**Letzte Aktualisierung**: 2026-01-02  
**Version**: 3.0.0  
**Status**: ✅ Aligned mit MessageType Enum (21 Messages)

[← Zurück zur Übersicht](README.md)
