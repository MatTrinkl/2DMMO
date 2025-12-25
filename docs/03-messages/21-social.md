# 👥 Social Messages (Friends, Block, Ignore) (2100-2199)

**Kategorie:** 21  
**Range:** 2100-2199  
**Phase:** Phase 2  
**Status:** 🟡 Phase 2

[← Zurück zur Übersicht](README.md)

---

## 📋 Inhaltsverzeichnis

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

---

## 📋 Übersicht

Diese Kategorie umfasst alle Messages für **Social Features** im 2DMMO.

Das Social-System implementiert:
- Friend-System (Add, Remove, Online-Status)
- Friend-Notes (Private Notizen über Freunde)
- Block-System (Kommunikation blockieren)
- Ignore-List (Chat ignorieren)
- Online-Status Benachrichtigungen

**Server Authority**: Friend-Lists werden server-seitig gespeichert. Online-Status wird automatisch getrackt.

---

## FriendRequest (2100)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Sendet Friend-Request an anderen Spieler. Server validiert und sendet Prompt an Target.

### Im Scope ✅
- Friend-Request senden
- Character-Name oder Entity-ID
- Request-Validation (Block-List Check)

### Nicht im Scope ❌
- Guild-Invite → verwende `GuildInvite` (800)
- Party-Invite → verwende `PartyInvite` (700)

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| TargetCharacterName | string | Name des Spielers | Ja* |
| TargetEntityId | int | Entity-ID (wenn online) | Ja* |

\* Entweder Name oder EntityId

### Erwartete Response
- **Bei Erfolg:** `FriendRequestResult` (2101) + Prompt an Target
- **Bei Fehler:** `FriendRequestResult` (2101) mit ErrorCode

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `FriendRequestResult` | 2101 | Response |
| `FriendAccept` | 2102 | Target akzeptiert |
| `FriendDecline` | 2103 | Target lehnt ab |

### Beispiel Payload
```csharp
var friendRequest = new FriendRequest
{
    Type = MessageType.FriendRequest,
    TargetCharacterName = "Legolas"
};
```

### Error Codes
| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `PLAYER_NOT_FOUND` | Character existiert nicht | Namen prüfen |
| `ALREADY_FRIENDS` | Bereits in Friend-List | Ignorieren |
| `BLOCKED_BY_TARGET` | Target hat dich geblockt | Ignorieren |
| `FRIEND_LIST_FULL` | Friend-List voll (max 50) | Platz schaffen |
| `TARGET_FRIEND_LIST_FULL` | Target Friend-List voll | Später versuchen |

### Notizen
- **Max Friends**: 50 Freunde
- **Timeout**: Request expires nach 5 Minuten
- **Offline**: Requests an Offline-Spieler möglich (bei Login gesendet)

---

## FriendRequestResult (2101)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung
Result des Friend-Requests. Bestätigung oder Fehler.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Success | bool | Request erfolgreich? | Ja |
| TargetName | string | Name des Targets | Ja |
| ErrorCode | string | Error bei Fehler | Nein |
| Pending | bool | Request ist pending (offline Target) | Nein |

### Beispiel Payload
```csharp
var requestResult = new FriendRequestResult
{
    Type = MessageType.FriendRequestResult,
    Success = true,
    TargetName = "Legolas",
    Pending = true // Offline
};
```

### Notizen
- **Pending**: Request wird bei Target-Login zugestellt
- **UI**: Client zeigt "Request sent" Notification

---

## FriendAccept (2102)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Akzeptiert Friend-Request.

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| RequesterCharacterId | long | Character-ID des Requesters | Ja |

### Erwartete Response
- **Immer:** `FriendListResponse` (2106) mit neuem Friend

### Beispiel Payload
```csharp
var accept = new FriendAccept
{
    Type = MessageType.FriendAccept,
    RequesterCharacterId = 98765
};
```

### Notizen
- **Beide Richtungen**: Beide Spieler fügen sich gegenseitig hinzu
- **Notification**: Beide Spieler erhalten Update

---

## FriendDecline (2103)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Lehnt Friend-Request ab.

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| RequesterCharacterId | long | Character-ID des Requesters | Ja |

### Erwartete Response
- **Keine** - Requester erhält keine Notification (Privacy)

### Notizen
- **Silent**: Requester sieht nicht dass declined wurde
- **Privacy**: Target-Privacy wird gewahrt

---

## FriendRemove (2104)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Entfernt Friend aus Friend-List.

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| FriendCharacterId | long | Character-ID zum Entfernen | Ja |

### Erwartete Response
- **Immer:** `FriendListResponse` (2106) ohne entfernten Friend

### Notizen
- **Beide Richtungen**: Friend wird aus beiden Listen entfernt
- **Silent**: Andere Spieler erhält keine Notification (Privacy)
- **Confirmation**: Client sollte Confirmation-Dialog zeigen

---

## FriendListRequest (2105)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Fordert Friend-List an.

### Request Payload
Keine zusätzlichen Felder

### Erwartete Response
- **Immer:** `FriendListResponse` (2106)

### Notizen
- **Login**: Automatisch nach Login gesendet
- **Refresh**: Manuell refresh möglich

---

## FriendListResponse (2106)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung
Komplette Friend-List mit Online-Status.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Friends | List<FriendInfo> | Alle Freunde | Ja |

**FriendInfo**:
| Feld | Typ | Beschreibung |
|------|-----|--------------|
| CharacterId | long | Character-ID |
| CharacterName | string | Character-Name |
| IsOnline | bool | Online-Status |
| Level | int | Aktuelles Level |
| ZoneId | int | Aktuelle Zone (wenn online) |
| ZoneName | string | Zone-Name (wenn online) |
| Note | string | Private Notiz |
| LastSeen | long | Unix Timestamp (letztes Online) |

### Beispiel Payload
```csharp
var friendList = new FriendListResponse
{
    Type = MessageType.FriendListResponse,
    Friends = new List<FriendInfo>
    {
        new FriendInfo
        {
            CharacterId = 98765,
            CharacterName = "Legolas",
            IsOnline = true,
            Level = 12,
            ZoneId = 1002,
            ZoneName = "Mirkwood Forest",
            Note = "Great archer",
            LastSeen = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        },
        new FriendInfo
        {
            CharacterId = 98766,
            CharacterName = "Gimli",
            IsOnline = false,
            Level = 11,
            Note = "",
            LastSeen = DateTimeOffset.UtcNow.AddDays(-2).ToUnixTimeSeconds()
        }
    }
};
```

### Notizen
- **Max**: 50 Friends
- **Sorting**: Client sortiert nach Online-Status dann Name
- **Privacy**: Zone-Info nur wenn online

---

## FriendOnline (2107)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Friend ist online gekommen. Automatische Notification.

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| CharacterId | long | Friend Character-ID | Ja |
| CharacterName | string | Character-Name | Ja |
| Level | int | Aktuelles Level | Ja |

### Beispiel Payload
```csharp
var friendOnline = new FriendOnline
{
    Type = MessageType.FriendOnline,
    CharacterId = 98765,
    CharacterName = "Legolas",
    Level = 12
};
```

### Notizen
- **Notification**: Client zeigt "Friend Online" Popup
- **Sound**: Optional Sound abspielen
- **Auto**: Automatisch bei Login

---

## FriendOffline (2108)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Friend ist offline gegangen.

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| CharacterId | long | Friend Character-ID | Ja |

### Beispiel Payload
```csharp
var friendOffline = new FriendOffline
{
    Type = MessageType.FriendOffline,
    CharacterId = 98765
};
```

### Notizen
- **Auto**: Automatisch bei Logout/Disconnect
- **UI**: Friend-List updated

---

## FriendUpdate (2109)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Friend-Info hat sich geändert (Level-Up, Zone-Change).

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| CharacterId | long | Friend Character-ID | Ja |
| Level | int | Neues Level | Nein |
| ZoneId | int | Neue Zone-ID | Nein |
| ZoneName | string | Neue Zone-Name | Nein |

### Beispiel Payload
```csharp
var friendUpdate = new FriendUpdate
{
    Type = MessageType.FriendUpdate,
    CharacterId = 98765,
    Level = 13, // Level-Up
    ZoneId = 1003,
    ZoneName = "Rivendell"
};
```

### Notizen
- **Throttle**: Max 1 Update pro Friend pro Minute
- **Level-Up**: Optional Notification "Friend leveled up"

---

## FriendNote (2110)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Setzt private Notiz für Friend. Nur für eigenen Client sichtbar.

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| FriendCharacterId | long | Friend Character-ID | Ja |
| Note | string | Notiz-Text (max 100 Zeichen) | Ja |

### Erwartete Response
- **Immer:** Server speichert, keine Response

### Beispiel Payload
```csharp
var friendNote = new FriendNote
{
    Type = MessageType.FriendNote,
    FriendCharacterId = 98765,
    Note = "Best DPS in guild"
};
```

### Notizen
- **Private**: Nur eigener Client sieht Notiz
- **Max Length**: 100 Zeichen
- **Use-Case**: Charakteristiken merken, Beziehung notieren

---

## BlockPlayer (2120)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Blockiert Spieler komplett. Keine Kommunikation, Friend-Requests, Party-Invites, etc.

### Im Scope ✅
- Komplette Kommunikations-Blockade
- Verhindert Friend-Requests
- Verhindert Party-Invites
- Verhindert Trade-Requests
- Chat-Messages werden gefiltert

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| TargetCharacterName | string | Zu blockierender Spieler | Ja |

### Erwartete Response
- **Bei Erfolg:** `BlockPlayerResult` (2121)
- **Bei Fehler:** `BlockPlayerResult` (2121) mit ErrorCode

### Beispiel Payload
```csharp
var blockPlayer = new BlockPlayer
{
    Type = MessageType.BlockPlayer,
    TargetCharacterName = "Spammer123"
};
```

### Error Codes
| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `PLAYER_NOT_FOUND` | Character existiert nicht | Namen prüfen |
| `ALREADY_BLOCKED` | Bereits geblockt | Ignorieren |
| `BLOCK_LIST_FULL` | Block-List voll (max 100) | Platz schaffen |

### Notizen
- **Max**: 100 Blocked Players
- **Scope**: Account-wide (nicht character-specific)
- **Permanent**: Bleibt bis manuell unblocked
- **Privacy**: Blocked Player sieht nicht dass geblockt

---

## BlockPlayerResult (2121)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung
Result des Block-Requests.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Success | bool | Block erfolgreich? | Ja |
| BlockedName | string | Blocked Player-Name | Ja |
| ErrorCode | string | Error bei Fehler | Nein |

### Notizen
- **UI**: Client zeigt "Player blocked" Notification
- **Effect**: Sofort aktiv

---

**Letzte Aktualisierung**: 2025-12-25  
**Version**: 1.0.0

[← Zurück zur Übersicht](README.md)
