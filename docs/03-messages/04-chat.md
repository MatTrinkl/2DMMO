# 💬 Chat Messages (0400-0499)

**Kategorie:** 4  
**Range:** 0400-0499  
**Phase:** Prototyp  
**Status:** 🟢 In Entwicklung

[← Zurück zur Übersicht](README.md)

---

## 📋 Übersicht

Diese Kategorie umfasst alle Messages für **Chat-Kommunikation** im 2DMMO.

Das Chat-System unterstützt:
- **Multiple Channels**: Say, Yell, Party, Guild, Raid, Zone, Trade, LFG, System
- **Private Messages**: Whisper (Direct Messages)
- **Channel Management**: Create, Join, Leave, Password-Protection
- **Moderation**: Mute, Kick, Ban, Owner/Moderator Roles
- **Spam Protection**: Rate-Limiting, Profanity-Filter
- **Status Messages**: AFK, DND

**Rate-Limits**:
- **Say/Yell**: 10 Messages/Minute
- **Whisper**: 20 Messages/Minute
- **Channel**: 30 Messages/Minute
- **Spam-Detection**: 5+ identical Messages = Mute

---

## 📋 Inhaltsverzeichnis

- [ChatMessage (400)](#chatmessage-400)
- [ChatBroadcast (401)](#chatbroadcast-401)
- [ChatWhisper (402)](#chatwhisper-402)
- [ChatWhisperResponse (403)](#chatwhisperresponse-403)
- [ChatParty (404)](#chatparty-404)
- [ChatGuild (405)](#chatguild-405)
- [ChatRaid (406)](#chatraid-406)
- [ChatZone (407)](#chatzone-407)
- [ChatTrade (408)](#chattrade-408)
- [ChatLFG (409)](#chatlfg-409)
- [ChatSystem (410)](#chatsystem-410)
- [ChatYell (411)](#chatyell-411)
- [ChatSay (412)](#chatsay-412)
- [ChatEmote (413)](#chatemote-413)
- [ChatAFK (414)](#chatafk-414)
- [ChatDND (415)](#chatdnd-415)
- [ChatChannelJoin (416)](#chatchanneljoin-416)
- [ChatChannelLeave (417)](#chatchannelleave-417)
- [ChatChannelList (418)](#chatchannellist-418)
- [ChatChannelCreate (419)](#chatchannelcreate-419)
- [ChatChannelDelete (420)](#chatchanneldelete-420)
- [ChatChannelPassword (421)](#chatchannelpassword-421)
- [ChatChannelMute (422)](#chatchannelmute-422)
- [ChatChannelUnmute (423)](#chatchannelunmute-423)
- [ChatChannelKick (424)](#chatchannelkick-424)
- [ChatChannelBan (425)](#chatchannelban-425)
- [ChatChannelOwner (426)](#chatchannelowner-426)
- [ChatChannelModerator (427)](#chatchannelmoderator-427)
- [ChatMOTD (428)](#chatmotd-428)
- [ChatFilter (429)](#chatfilter-429)
- [ChatSpamWarning (430)](#chatspamwarning-430)
- [ChatMessageResponse (440)](#chatmessageresponse-440)
- [ChatChannelJoinResponse (441)](#chatchanneljoinresponse-441)
- [ChatChannelCreateResponse (442)](#chatchannelcreateresponse-442)
- [ChatChannelDeleteResponse (443)](#chatchanneldeleteresponse-443)
- [ChatChannelPasswordResponse (444)](#chatchannelpasswordresponse-444)
- [ChatChannelMuteResponse (445)](#chatchannelmuteresponse-445)

---

## ChatMessage (400)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Universelle Chat-Message für alle Channel-Types. Client sendet Message, Server validiert (Rate-Limit, Profanity-Filter, Permissions) und broadcastet an Channel-Teilnehmer.

### Im Scope ✅
- Text-Message (max 500 Zeichen)
- Channel-Type (say, yell, party, guild, zone, trade, lfg, custom)
- Optional: Channel-Name für Custom-Channels
- Item-Links, Achievement-Links (geplant)

### Nicht im Scope ❌
- Private Messages → verwende `ChatWhisper` (402)
- System-Messages → Server sendet `ChatSystem` (410)
- Emotes (Text) → verwende `ChatEmote` (413)

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Message | string | Text-Message (max 500 Zeichen) | Ja |
| ChannelType | string | "say", "yell", "party", "guild", "raid", "zone", "trade", "lfg", "custom" | Ja |
| ChannelName | string | Name für Custom-Channels | Nein |

### Erwartete Response
- `ChatMessageResponse` (440)

### Folge-Messages bei Erfolg
- `ChatBroadcast` (401) an Channel-Teilnehmer

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `ChatBroadcast` | 401 | Server broadcastet Message |
| `ChatWhisper` | 402 | Private Message |
| `ChatSpamWarning` | 430 | Bei Rate-Limit-Violation |

### Beispiel Payload
```csharp
// Party-Chat
var chatMsg = new ChatMessage
{
    Type = MessageType.ChatMessage,
    Message = "Ready for boss pull!",
    ChannelType = "party"
};

// Custom-Channel
var customChat = new ChatMessage
{
    Type = MessageType.ChatMessage,
    Message = "Anyone selling Iron Ore?",
    ChannelType = "custom",
    ChannelName = "Trade"
};
```

### Error Codes
| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `RATE_LIMITED` | Zu viele Messages | Warten (Zeit in Response) |
| `PROFANITY_DETECTED` | Profanity-Filter | Message anpassen |
| `NOT_IN_CHANNEL` | Nicht im Channel | Channel joinen |
| `MUTED` | Spieler ist gemuted | Warten bis Unmute |
| `MESSAGE_TOO_LONG` | >500 Zeichen | Kürzen |

### Notizen
- **Profanity-Filter**: Server replaced Schimpfwörter mit "***"
- **Rate-Limiting**: 10/min (Say/Yell), 30/min (Channels)
- **Spam-Detection**: 5+ identische Messages = Auto-Mute (5 Minuten)
- **Item-Links**: Phase 2 - Format: `[item:12345:ItemName]`

---

## ChatBroadcast (401)

**Richtung:** 📡 Broadcast  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Server broadcastet Chat-Message an alle Teilnehmer eines Channels. Enthält Sender-Info, Message-Text, Timestamp.

### Im Scope ✅
- Sender-Name und Character-ID
- Message-Text (gefiltert)
- Channel-Type
- Timestamp
- Sender-Level, Klasse (für UI)

### Nicht im Scope ❌
- Private Messages → verwende `ChatWhisperResponse` (403)

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| SenderId | long | Character-ID des Senders | Ja |
| SenderName | string | Character-Name | Ja |
| SenderLevel | int | Level (für UI-Anzeige) | Ja |
| SenderClass | int | Klassen-ID | Ja |
| Message | string | Text-Message (gefiltert) | Ja |
| ChannelType | string | "say", "yell", "party", etc. | Ja |
| ChannelName | string | Custom-Channel-Name | Nein |
| Timestamp | long | Server Unix Timestamp | Ja |

### Erwartete Response
- **Keine** - Client zeigt Message an

### Beispiel Payload
```csharp
var chatBcast = new ChatBroadcast
{
    Type = MessageType.ChatBroadcast,
    SenderId = 98765,
    SenderName = "Aragorn",
    SenderLevel = 10,
    SenderClass = 2, // Warrior
    Message = "Ready for boss pull!",
    ChannelType = "party",
    Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
};
```

### Notizen
- **UI-Formatting**: Client kann Level/Klasse-Color verwenden
- **Ignore-List**: Client filtert Messages von ignorierten Spielern lokal
- **History**: Client speichert letzten 100 Messages pro Channel

---

## ChatWhisper (402)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Private Nachricht an einen anderen Spieler (Direct Message). Server validiert ob Empfänger online und nicht blockiert.

### Im Scope ✅
- Private 1-to-1 Message
- Target-Spieler-Name oder Character-ID
- Text-Message

### Nicht im Scope ❌
- Group-Messages → kein Feature
- Offline-Messages → geplant mit Mail-System (1800)

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| TargetName | string | Empfänger-Name | Ja* |
| TargetId | long | Empfänger Character-ID | Ja* |
| Message | string | Text-Message (max 500 Zeichen) | Ja |

\* Entweder Name oder ID

### Erwartete Response
- **Bei Erfolg:** `ChatWhisperResponse` (403) mit Success=true, Message wird an Target gesendet
- **Bei Fehler:** `ChatWhisperResponse` (403) mit ErrorCode

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `ChatWhisperResponse` | 403 | Response + Delivery an Target |
| `ChatDND` | 415 | Target ist DND |
| `BlockPlayer` | 2120 | Target hat Sender blockiert |

### Beispiel Payload
```csharp
var whisper = new ChatWhisper
{
    Type = MessageType.ChatWhisper,
    TargetName = "Legolas",
    Message = "Want to join our party for dungeon?"
};
```

### Error Codes
| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `PLAYER_NOT_FOUND` | Target offline oder nicht existent | Namen prüfen |
| `PLAYER_BLOCKED_YOU` | Target hat Sender blockiert | Keine Action möglich |
| `PLAYER_DND` | Target ist Do-Not-Disturb | Später versuchen |
| `RATE_LIMITED` | Zu viele Whispers | Warten |

### Notizen
- **Rate-Limit**: 20 Whispers/Minute
- **Reply-Command**: `/r` für Reply an letzten Whisper-Sender
- **History**: Client speichert Whisper-History pro Kontakt

---

## ChatWhisperResponse (403)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Bestätigung für gesendeten Whisper (an Sender) und Delivery der Whisper-Message (an Empfänger).

### Im Scope ✅
- Erfolgs-Status (für Sender)
- Message-Delivery (für Empfänger)
- Sender-Info (für Empfänger)

### Nicht im Scope ❌
- Read-Receipts → nicht implementiert

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Success | bool | Whisper delivered? | Ja |
| ErrorCode | string | Fehlercode falls Failed | Nein |
| SenderId | long | Sender Character-ID | Bei Delivery |
| SenderName | string | Sender-Name | Bei Delivery |
| Message | string | Text-Message | Bei Delivery |
| Timestamp | long | Server Timestamp | Ja |

### Beispiel Payload
```csharp
// An Sender (Confirmation)
var whisperConf = new ChatWhisperResponse
{
    Type = MessageType.ChatWhisperResponse,
    Success = true,
    Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
};

// An Empfänger (Delivery)
var whisperDeliver = new ChatWhisperResponse
{
    Type = MessageType.ChatWhisperResponse,
    Success = true,
    SenderId = 98765,
    SenderName = "Aragorn",
    Message = "Want to join our party for dungeon?",
    Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
};
```

### Notizen
- **Sound**: Client spielt Whisper-Sound bei Delivery
- **UI-Notification**: Blinken im UI falls minimiert

---

## ChatParty (404) - Request

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine (muss in Party sein)

### Beschreibung
Client sendet Party-Chat-Message. Server validiert Party-Membership und broadcastet an alle Party-Members.

### Im Scope ✅
- Party-Chat für alle Members
- Auto-Membership-Check
- Rate-Limiting und Profanity-Filter

### Nicht im Scope ❌
- Raid-Chat → verwende `ChatRaid` (406)
- Whisper innerhalb Party → verwende `ChatWhisper` (402)

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Message | string | Text-Message (max 500 Zeichen) | Ja |

### Erwartete Response
- **Bei Erfolg:** `ChatParty` Broadcast (404) an alle Party-Members
- **Bei Fehler:** `ErrorMessage` (910)

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `ChatParty` Broadcast | 404 | Server broadcastet an Party |
| `ChatMessage` | 400 | Alternative mit ChannelType="party" |
| `PartyUpdate` | 704 | Party-Membership-Info |

### Beispiel Payload
```csharp
var partyChat = new ChatParty
{
    Type = MessageType.ChatParty,
    Message = "Ready for boss pull!"
};
```

### Error Codes
| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `NOT_IN_PARTY` | Spieler ist nicht in einer Party | Party joinen |
| `RATE_LIMITED` | Zu viele Messages | Warten |
| `PROFANITY_DETECTED` | Profanity-Filter | Message anpassen |
| `MESSAGE_TOO_LONG` | >500 Zeichen | Kürzen |

### Notizen
- **Auto-Join**: Automatisch beim Party-Join verfügbar
- **Auto-Leave**: Channel wird bei Party-Leave automatisch verlassen
- **Rate-Limit**: 30 Messages/Minute
- **Alternative**: Kann auch via `ChatMessage` (400) mit ChannelType="party" gesendet werden

---

## ChatParty (404) - Broadcast

**Richtung:** 📡 Broadcast (Server → Party Members)  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Server broadcastet Party-Chat-Message an alle Members der Party. Enthält Sender-Info und Message-Text.

### Im Scope ✅
- Broadcast an alle Party-Members
- Sender-Informationen (Name, Level, Klasse)
- Gefilterte Message
- Timestamp

### Nicht im Scope ❌
- Delivery-Confirmation → keine Read-Receipts

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| SenderId | long | Character-ID des Senders | Ja |
| SenderName | string | Character-Name | Ja |
| SenderLevel | int | Level (für UI-Anzeige) | Ja |
| SenderClass | int | Klassen-ID | Ja |
| Message | string | Text-Message (gefiltert) | Ja |
| Timestamp | long | Server Unix Timestamp | Ja |

### Beispiel Payload
```csharp
var partyBcast = new ChatParty
{
    Type = MessageType.ChatParty,
    SenderId = 98765,
    SenderName = "Aragorn",
    SenderLevel = 10,
    SenderClass = 2, // Warrior
    Message = "Ready for boss pull!",
    Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
};
```

### Notizen
- **UI-Formatting**: Spezielle Farbe für Party-Chat (z.B. Blau)
- **History**: Client speichert letzte 100 Party-Messages
- **Offline-Members**: Erhalten Message nicht (kein Queueing)

---

## ChatGuild (405) - Request

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine (muss in Guild sein)

### Beschreibung
Client sendet Guild-Chat-Message. Server validiert Guild-Membership und Permissions, dann broadcastet an alle Online-Guild-Members.

### Im Scope ✅
- Guild-weite Kommunikation
- Permissions-Check (Guild-Ranks können Chat deaktiviert haben)
- Rate-Limiting und Profanity-Filter
- Persistent Channel (bleibt auch bei Logout aktiv)

### Nicht im Scope ❌
- Officer-Chat → geplant Feature (separater Channel)
- Guild-Announcements → verwende `GuildMOTD` (808)

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Message | string | Text-Message (max 500 Zeichen) | Ja |

### Erwartete Response
- **Bei Erfolg:** `ChatGuild` Broadcast (405) an alle Online-Guild-Members
- **Bei Fehler:** `ErrorMessage` (910)

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `ChatGuild` Broadcast | 405 | Server broadcastet an Guild |
| `ChatMessage` | 400 | Alternative mit ChannelType="guild" |
| `GuildUpdate` | 804 | Guild-Membership-Info |
| `GuildMOTD` | 808 | Guild Message-of-the-Day |

### Beispiel Payload
```csharp
var guildChat = new ChatGuild
{
    Type = MessageType.ChatGuild,
    Message = "Anyone wants to join raid tonight at 20:00?"
};
```

### Error Codes
| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `NOT_IN_GUILD` | Spieler ist nicht in einer Guild | Guild joinen |
| `NO_GUILD_CHAT_PERMISSION` | Rank hat keine Chat-Permission | Permission vom GM anfordern |
| `RATE_LIMITED` | Zu viele Messages | Warten |
| `PROFANITY_DETECTED` | Profanity-Filter | Message anpassen |
| `MESSAGE_TOO_LONG` | >500 Zeichen | Kürzen |
| `GUILD_CHAT_MUTED` | Von Officer gemuted | Auf Unmute warten |

### Notizen
- **Permissions**: Guild-Ranks können "Use Guild Chat" Permission haben
- **History**: Server speichert letzte 100 Messages (geplant)
- **Auto-Join**: Automatisch bei Guild-Membership
- **Rate-Limit**: 30 Messages/Minute
- **Alternative**: Kann auch via `ChatMessage` (400) mit ChannelType="guild" gesendet werden

---

## ChatGuild (405) - Broadcast

**Richtung:** 📡 Broadcast (Server → Guild Members)  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Server broadcastet Guild-Chat-Message an alle Online-Members der Guild. Enthält Sender-Info, Rank und Message-Text.

### Im Scope ✅
- Broadcast an alle Online-Guild-Members
- Sender-Informationen (Name, Level, Klasse, Rank)
- Gefilterte Message
- Timestamp

### Nicht im Scope ❌
- Offline-Message-Queue → geplant Feature
- Officer-Chat → geplant (separater Channel)

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| SenderId | long | Character-ID des Senders | Ja |
| SenderName | string | Character-Name | Ja |
| SenderLevel | int | Level (für UI-Anzeige) | Ja |
| SenderClass | int | Klassen-ID | Ja |
| SenderGuildRank | int | Guild-Rank (0=GM, 1=Officer, etc.) | Ja |
| Message | string | Text-Message (gefiltert) | Ja |
| Timestamp | long | Server Unix Timestamp | Ja |

### Beispiel Payload
```csharp
var guildBcast = new ChatGuild
{
    Type = MessageType.ChatGuild,
    SenderId = 98765,
    SenderName = "Aragorn",
    SenderLevel = 10,
    SenderClass = 2, // Warrior
    SenderGuildRank = 1, // Officer
    Message = "Anyone wants to join raid tonight at 20:00?",
    Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
};
```

### Notizen
- **UI-Formatting**: Spezielle Farbe für Guild-Chat (z.B. Grün)
- **Rank-Display**: Client zeigt Rank-Badge neben Name
- **History**: Client speichert letzte 100 Guild-Messages
- **Offline-Members**: Erhalten Message nicht später (Hinweis: Message-Queue)
- **Cross-Zone**: Funktioniert Zone-übergreifend (über Redis Pub/Sub)

---

## ChatRaid (406) - Request

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine (muss in Raid sein)

### Beschreibung
**Feature** - Client sendet Raid-Chat-Message. Server validiert Raid-Membership und broadcastet an alle Raid-Members. Für große Gruppen (>5 Spieler).

### Im Scope ✅
- Raid-weite Kommunikation (bis 40 Spieler)
- Multiple Raid-Groups
- Rate-Limiting und Profanity-Filter

### Nicht im Scope ❌
- Party-Chat → verwende `ChatParty` (404) für kleine Gruppen
- Raid-Warning (Leader-Only) → geplant Feature

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Message | string | Text-Message (max 500 Zeichen) | Ja |

### Erwartete Response
- **Bei Erfolg:** `ChatRaid` Broadcast (406) an alle Raid-Members
- **Bei Fehler:** `ErrorMessage` (910)

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `ChatRaid` Broadcast | 406 | Server broadcastet an Raid |
| `ChatMessage` | 400 | Alternative mit ChannelType="raid" |
| `RaidConvert` | 2430 | Party zu Raid konvertieren |

### Beispiel Payload
```csharp
var raidChat = new ChatRaid
{
    Type = MessageType.ChatRaid,
    Message = "Group 2, prepare for next boss pull!"
};
```

### Error Codes
| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `NOT_IN_RAID` | Spieler ist nicht in einem Raid | Raid joinen oder erstellen |
| `RATE_LIMITED` | Zu viele Messages | Warten |
| `PROFANITY_DETECTED` | Profanity-Filter | Message anpassen |
| `MESSAGE_TOO_LONG` | >500 Zeichen | Kürzen |

### Notizen
- **Hinweis**: Nicht im Prototyp verfügbar
- **Auto-Join**: Automatisch bei Raid-Join verfügbar
- **Auto-Leave**: Channel wird bei Raid-Leave automatisch verlassen
- **Rate-Limit**: 30 Messages/Minute
- **Raid-Size**: Bis zu 40 Spieler (8 Gruppen à 5 Spieler)
- **Alternative**: Kann auch via `ChatMessage` (400) mit ChannelType="raid" gesendet werden

---

## ChatRaid (406) - Broadcast

**Richtung:** 📡 Broadcast (Server → Raid Members)  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
**Feature** - Server broadcastet Raid-Chat-Message an alle Members des Raids. Enthält Sender-Info, Group-Nummer und Message-Text.

### Im Scope ✅
- Broadcast an alle Raid-Members (bis 40 Spieler)
- Sender-Informationen (Name, Level, Klasse, Group)
- Gefilterte Message
- Timestamp

### Nicht im Scope ❌
- Raid-Warning → geplant (Leader-Only Broadcast)
- Group-Specific Chat → verwende Party-Chat

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| SenderId | long | Character-ID des Senders | Ja |
| SenderName | string | Character-Name | Ja |
| SenderLevel | int | Level (für UI-Anzeige) | Ja |
| SenderClass | int | Klassen-ID | Ja |
| SenderGroupNumber | int | Raid-Group-Nummer (1-8) | Ja |
| Message | string | Text-Message (gefiltert) | Ja |
| Timestamp | long | Server Unix Timestamp | Ja |

### Beispiel Payload
```csharp
var raidBcast = new ChatRaid
{
    Type = MessageType.ChatRaid,
    SenderId = 98765,
    SenderName = "Aragorn",
    SenderLevel = 10,
    SenderClass = 2, // Warrior
    SenderGroupNumber = 2,
    Message = "Group 2, prepare for next boss pull!",
    Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
};
```

### Notizen
- **Hinweis**: Nicht im Prototyp verfügbar
- **UI-Formatting**: Spezielle Farbe für Raid-Chat (z.B. Orange)
- **Group-Display**: Client zeigt Group-Nummer in Klammern, z.B. "[G2] Aragorn: ..."
- **History**: Client speichert letzte 100 Raid-Messages
- **Offline-Members**: Erhalten Message nicht (kein Queueing)
- **Performance**: Optimiert für 40 Spieler (broadcast via Raid-Server)

---

## ChatZone (407)

**Richtung:** 📡 Broadcast  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Zone-weiter Chat. Alle Spieler in der Zone empfangen (Local/General Chat).

### Im Scope ✅
- Zone-weite Kommunikation
- Auto-Join bei Zone-Enter
- Auto-Leave bei Zone-Leave

### Nicht im Scope ❌
- Cross-Zone Chat → nicht möglich

### Payload
Identisch zu `ChatMessage` (400) und `ChatBroadcast` (401), aber ChannelType="zone"

### Notizen
- **Range**: Gesamte Zone (keine Range-Limit)
- **Busy**: Kann sehr voll sein in Städten

---

## ChatTrade (408)

**Richtung:** 📡 Broadcast  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
**Feature** - Trade-Channel für Verkauf/Kauf-Angebote.

### Payload
Identisch zu `ChatMessage` (400), aber ChannelType="trade"

### Notizen
- **Hinweis**: Für Trade-Economy

---

## ChatLFG (409)

**Richtung:** 📡 Broadcast  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
**Feature** - Looking-for-Group Channel für Party/Raid-Suche.

### Payload
Identisch zu `ChatMessage` (400), aber ChannelType="lfg"

### Notizen
- **Hinweis**: Für Group-Finding

---

## ChatSystem (410)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung
System-generierte Messages (Level-Up, Achievement, Server-Events, etc.). Nur Server kann diese senden.

### Im Scope ✅
- Server-Announcements
- Achievement-Unlocks
- Level-Up Messages
- Quest-Completion
- Important Game-Events

### Nicht im Scope ❌
- Player-Messages → verwende andere Chat-Messages

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Message | string | System-Message | Ja |
| MessageType | string | "info", "warning", "error", "achievement", "levelup" | Ja |
| Timestamp | long | Server Timestamp | Ja |

### Beispiel Payload
```csharp
// Level-Up
var systemMsg = new ChatSystem
{
    Type = MessageType.ChatSystem,
    Message = "Congratulations! You reached Level 11!",
    MessageType = "levelup",
    Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
};

// Server-Announcement
var announcement = new ChatSystem
{
    Type = MessageType.ChatSystem,
    Message = "Server restart in 10 minutes!",
    MessageType = "warning",
    Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
};
```

### Notizen
- **UI-Color**: MessageType bestimmt Farbe (info=white, warning=yellow, error=red)
- **Sound**: Unterschiedliche Sounds für Types
- **Cannot be disabled**: System-Messages immer sichtbar

---

## ChatYell (411)

**Richtung:** 📡 Broadcast  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Yell-Chat mit größerer Range als Say (~50m vs ~20m). Für wichtige Announcements.

### Im Scope ✅
- Range: ~50m Radius
- Höhere Sichtbarkeit im UI (größere Schrift, andere Farbe)

### Nicht im Scope ❌
- Zone-weit → verwende `ChatZone` (407)

### Payload
Identisch zu `ChatMessage` (400) und `ChatBroadcast` (401), aber ChannelType="yell"

### Beispiel Payload
```csharp
var yell = new ChatMessage
{
    Type = MessageType.ChatMessage,
    Message = "LFG for Deadmines!",
    ChannelType = "yell"
};
```

### Notizen
- **Range**: 50m Radius
- **Rate-Limit**: 10/min (gleich wie Say)
- **UI**: Größere Schrift, auffälligere Farbe

---

## ChatSay (412)

**Richtung:** 📡 Broadcast  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Say-Chat mit kleiner Range (~20m). Lokaler Chat für Roleplaying und nahe Spieler.

### Im Scope ✅
- Range: ~20m Radius
- Standard local Chat

### Nicht im Scope ❌
- Größere Range → verwende `ChatYell` (411)

### Payload
Identisch zu `ChatMessage` (400) und `ChatBroadcast` (401), aber ChannelType="say"

### Beispiel Payload
```csharp
var say = new ChatMessage
{
    Type = MessageType.ChatMessage,
    Message = "Hello, how are you?",
    ChannelType = "say"
};
```

### Notizen
- **Range**: 20m Radius
- **Default**: Standard-Chat für Roleplaying
- **UI**: Normale Schrift

---

## ChatEmote (413)

**Richtung:** 📡 Broadcast  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Text-Emote (z.B. "/dance", "/wave", "/bow"). Erscheint als Third-Person-Text im Chat.

### Im Scope ✅
- Text-based Emotes
- Optional: Target (z.B. "/wave at Legolas")
- Format: "*Aragorn waves at Legolas.*"

### Nicht im Scope ❌
- Visual Emotes (Animation) → verwende `EmoteRequest` (2200)

### Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| EmoteText | string | Emote-Text | Ja |
| TargetName | string | Optional Target | Nein |

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| SenderId | long | Sender Character-ID | Ja |
| SenderName | string | Sender-Name | Ja |
| EmoteText | string | Formatted Emote (z.B. "*Aragorn waves.*") | Ja |
| TargetName | string | Optional Target | Nein |
| Timestamp | long | Server Timestamp | Ja |

### Beispiel Payload
```csharp
// Ohne Target
var emote = new ChatEmote
{
    Type = MessageType.ChatEmote,
    EmoteText = "waves"
};
// Broadcast: "*Aragorn waves.*"

// Mit Target
var emoteTarget = new ChatEmote
{
    Type = MessageType.ChatEmote,
    EmoteText = "bows to",
    TargetName = "Legolas"
};
// Broadcast: "*Aragorn bows to Legolas.*"
```

### Notizen
- **Format**: Server formatiert als "*{Name} {emote}.*"
- **Range**: Gleich wie Say (~20m)
- **Predefined**: Liste von ~50 Emotes (wave, bow, dance, laugh, cry, etc.)

---

## ChatAFK (414)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Server informiert dass ein Spieler AFK (Away-From-Keyboard) ist. Wird gesendet als Auto-Response auf Whisper.

### Im Scope ✅
- AFK-Status des Spielers
- Optional: AFK-Message

### Nicht im Scope ❌
- AFK-Setting → Client setzt lokal, Server trackt

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| PlayerId | long | AFK-Spieler Character-ID | Ja |
| PlayerName | string | Player-Name | Ja |
| IsAFK | bool | AFK-Status | Ja |
| AFKMessage | string | Optional AFK-Message | Nein |

### Beispiel Payload
```csharp
var afk = new ChatAFK
{
    Type = MessageType.ChatAFK,
    PlayerId = 98765,
    PlayerName = "Aragorn",
    IsAFK = true,
    AFKMessage = "Bathroom break, back in 5 min"
};
```

### Notizen
- **Auto-Response**: Bei Whisper an AFK-Spieler
- **UI-Indicator**: Name wird grayed out oder mit (AFK) markiert
- **Timeout**: Nach 30 min AFK = Auto-Logout

---

## ChatDND (415)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Server informiert dass ein Spieler DND (Do-Not-Disturb) ist. Blocks Whispers automatisch.

### Im Scope ✅
- DND-Status
- Optional: DND-Reason

### Nicht im Scope ❌
- Enforcement → Server blocks Whispers automatisch

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| PlayerId | long | DND-Spieler Character-ID | Ja |
| PlayerName | string | Player-Name | Ja |
| IsDND | bool | DND-Status | Ja |
| DNDMessage | string | Optional Reason | Nein |

### Beispiel Payload
```csharp
var dnd = new ChatDND
{
    Type = MessageType.ChatDND,
    PlayerId = 98765,
    PlayerName = "Aragorn",
    IsDND = true,
    DNDMessage = "In raid, please no whispers"
};
```

### Notizen
- **Blocks Whispers**: Server rejected Whispers an DND-Spieler
- **UI-Indicator**: Name mit (DND) markiert
- **Guild/Party**: DND blocks nur Whispers, nicht Guild/Party-Chat

---

## ChatChannelJoin (416)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Client joined Custom-Channel (z.B. "Trade", "LFG-Dungeon", "Roleplaying").

### Im Scope ✅
- Channel-Name
- Optional: Password (falls Protected)

### Nicht im Scope ❌
- Built-in Channels (Party, Guild) → Auto-Join

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| ChannelName | string | Channel-Name | Ja |
| Password | string | Password falls Protected | Nein |

### Erwartete Response
- `ChatChannelJoinResponse` (441)

### Folge-Messages bei Erfolg
- Client empfängt `ChatBroadcast` für diesen Channel

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `ChatChannelLeave` | 417 | Channel verlassen |
| `ChatChannelCreate` | 419 | Neuen Channel erstellen |

### Beispiel Payload
```csharp
var joinChannel = new ChatChannelJoin
{
    Type = MessageType.ChatChannelJoin,
    ChannelName = "Trade"
};

// Protected Channel
var joinProtected = new ChatChannelJoin
{
    Type = MessageType.ChatChannelJoin,
    ChannelName = "OfficerChat",
    Password = "secret123"
};
```

### Error Codes
| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `CHANNEL_NOT_FOUND` | Channel existiert nicht | Erstellen mit Create (419) |
| `WRONG_PASSWORD` | Falsches Password | Korrektes Password eingeben |
| `CHANNEL_FULL` | Max Members erreicht | Anderen Channel wählen |
| `BANNED` | Vom Channel gebannt | Cannot join |

### Notizen
- **Max Channels**: Spieler kann max 10 Custom-Channels joinen
- **Auto-Rejoin**: Bei Reconnect automatisch wieder joinen
- **Persistence**: Channel-Membership persistent gespeichert

---

## ChatChannelLeave (417)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Client verlässt Custom-Channel.

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| ChannelName | string | Channel-Name | Ja |

### Erwartete Response
- **Erfolg**: Client empfängt keine Messages mehr von diesem Channel

### Beispiel Payload
```csharp
var leaveChannel = new ChatChannelLeave
{
    Type = MessageType.ChatChannelLeave,
    ChannelName = "Trade"
};
```

### Notizen
- **Auto-Leave**: Bei Logout wird automatisch geleaved
- **Owner-Leave**: Falls Owner leaved, wird neuer Owner bestimmt

---

## ChatChannelList (418)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Liste aller verfügbaren Channels (für UI).

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Channels | List<ChannelInfo> | Channel-Liste | Ja |

**ChannelInfo**:
| Feld | Typ | Beschreibung |
|------|-----|--------------|
| Name | string | Channel-Name |
| MemberCount | int | Anzahl Members |
| HasPassword | bool | Password-Protected? |
| IsJoined | bool | Client ist Member? |

### Beispiel Payload
```csharp
var channelList = new ChatChannelList
{
    Type = MessageType.ChatChannelList,
    Channels = new List<ChannelInfo>
    {
        new ChannelInfo { Name = "Trade", MemberCount = 150, HasPassword = false, IsJoined = true },
        new ChannelInfo { Name = "LFG", MemberCount = 75, HasPassword = false, IsJoined = false },
        new ChannelInfo { Name = "RP-Haven", MemberCount = 20, HasPassword = true, IsJoined = false }
    }
};
```

---

## ChatChannelCreate (419)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Erstellt neuen Custom-Channel. Creator wird automatisch Owner.

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| ChannelName | string | Channel-Name (unique) | Ja |
| Password | string | Optional Password | Nein |

### Erwartete Response
- `ChatChannelCreateResponse` (442)

### Folge-Messages bei Erfolg
- Client wird automatisch als Owner dem Channel hinzugefügt

### Beispiel Payload
```csharp
var createChannel = new ChatChannelCreate
{
    Type = MessageType.ChatChannelCreate,
    ChannelName = "MyGuildAllies",
    Password = "secret"
};
```

### Error Codes
| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `NAME_TAKEN` | Channel existiert bereits | Anderen Namen wählen |
| `INVALID_NAME` | Name ungültig (Profanity, zu kurz) | Gültigen Namen wählen |
| `MAX_CHANNELS_CREATED` | Max Channels erreicht (3 pro Spieler) | Alten Channel löschen |

### Notizen
- **Max per Player**: 3 Channels pro Spieler
- **Persistence**: Channels bleiben bis Delete oder 30 Tage inaktiv
- **Owner-Powers**: Owner kann Password ändern, Members kicken/bannen

---

## ChatChannelDelete (420)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Channel Owner

### Beschreibung
Owner deleted Channel. Alle Members werden gekickt.

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| ChannelName | string | Channel-Name | Ja |

### Erwartete Response
- `ChatChannelDeleteResponse` (443)

### Folge-Messages bei Erfolg
- Alle Members werden informiert und automatisch aus dem Channel entfernt

### Beispiel Payload
```csharp
var deleteChannel = new ChatChannelDelete
{
    Type = MessageType.ChatChannelDelete,
    ChannelName = "MyOldChannel"
};
```

### Error Codes
| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `NOT_OWNER` | Nur Owner kann deleten | Keine Action |
| `CHANNEL_NOT_FOUND` | Channel existiert nicht | - |

---

## ChatChannelPassword (421)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Channel Owner

### Beschreibung
Owner ändert oder setzt Channel-Password.

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| ChannelName | string | Channel-Name | Ja |
| NewPassword | string | Neues Password (leer = remove Password) | Ja |

### Erwartete Response
- `ChatChannelPasswordResponse` (444)

### Beispiel Payload
```csharp
// Set Password
var setPassword = new ChatChannelPassword
{
    Type = MessageType.ChatChannelPassword,
    ChannelName = "MyChannel",
    NewPassword = "newSecret123"
};

// Remove Password
var removePassword = new ChatChannelPassword
{
    Type = MessageType.ChatChannelPassword,
    ChannelName = "MyChannel",
    NewPassword = "" // Empty = remove
};
```

---

## ChatChannelMute (422)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Channel Owner/Moderator

### Beschreibung
Owner/Moderator muted Member im Channel. Member kann lesen aber nicht schreiben.

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| ChannelName | string | Channel-Name | Ja |
| TargetName | string | Zu mutender Spieler | Ja |
| Duration | int | Dauer in Minuten (0 = permanent) | Ja |
| Reason | string | Optional Grund | Nein |

### Erwartete Response
- `ChatChannelMuteResponse` (445)

### Folge-Messages bei Erfolg
- Target wird über Mute informiert

### Beispiel Payload
```csharp
var mutePlayer = new ChatChannelMute
{
    Type = MessageType.ChatChannelMute,
    ChannelName = "Trade",
    TargetName = "Spammer123",
    Duration = 30, // 30 Minuten
    Reason = "Spam"
};
```

### Error Codes
| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `NO_PERMISSION` | Nicht Owner/Moderator | Keine Action |
| `PLAYER_NOT_IN_CHANNEL` | Target nicht im Channel | - |
| `CANNOT_MUTE_OWNER` | Owner kann nicht gemuted werden | - |

---

## ChatChannelUnmute (423)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Channel Owner/Moderator

### Beschreibung
Owner/Moderator unmuted gemuteten Member.

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| ChannelName | string | Channel-Name | Ja |
| TargetName | string | Zu unmutender Spieler | Ja |

### Beispiel Payload
```csharp
var unmutePlayer = new ChatChannelUnmute
{
    Type = MessageType.ChatChannelUnmute,
    ChannelName = "Trade",
    TargetName = "Spammer123"
};
```

---

## ChatChannelKick (424)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Channel Owner/Moderator

### Beschreibung
Owner/Moderator kicked Member aus Channel. Member kann wieder joinen.

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| ChannelName | string | Channel-Name | Ja |
| TargetName | string | Zu kickender Spieler | Ja |
| Reason | string | Optional Grund | Nein |

### Beispiel Payload
```csharp
var kickPlayer = new ChatChannelKick
{
    Type = MessageType.ChatChannelKick,
    ChannelName = "Trade",
    TargetName = "TrollUser",
    Reason = "Inappropriate behavior"
};
```

### Error Codes
| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `NO_PERMISSION` | Nicht Owner/Moderator | Keine Action |
| `CANNOT_KICK_OWNER` | Owner kann nicht gekickt werden | - |
| `CANNOT_KICK_MODERATOR` | Nur Owner kann Moderator kicken | - |

---

## ChatChannelBan (425)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Channel Owner/Moderator

### Beschreibung
Owner/Moderator banned Member aus Channel. Member kann nicht wieder joinen.

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| ChannelName | string | Channel-Name | Ja |
| TargetName | string | Zu bannender Spieler | Ja |
| Reason | string | Optional Grund | Nein |

### Beispiel Payload
```csharp
var banPlayer = new ChatChannelBan
{
    Type = MessageType.ChatChannelBan,
    ChannelName = "Trade",
    TargetName = "PersistentTroll",
    Reason = "Repeated violations"
};
```

### Notizen
- **Permanent**: Ban bleibt bis Unban
- **Channel-Specific**: Ban gilt nur für diesen Channel

---

## ChatChannelOwner (426)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Channel Owner

### Beschreibung
Owner übergibt Ownership an anderen Member.

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| ChannelName | string | Channel-Name | Ja |
| NewOwnerName | string | Neuer Owner | Ja |

### Beispiel Payload
```csharp
var transferOwner = new ChatChannelOwner
{
    Type = MessageType.ChatChannelOwner,
    ChannelName = "MyChannel",
    NewOwnerName = "TrustedFriend"
};
```

### Error Codes
| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `PLAYER_NOT_IN_CHANNEL` | Target nicht im Channel | Target muss erst joinen |

---

## ChatChannelModerator (427)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Channel Owner

### Beschreibung
Owner promoted/demoted Moderator im Channel.

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| ChannelName | string | Channel-Name | Ja |
| TargetName | string | Spieler | Ja |
| IsModerator | bool | true=promote, false=demote | Ja |

### Beispiel Payload
```csharp
// Promote zu Moderator
var promoteMod = new ChatChannelModerator
{
    Type = MessageType.ChatChannelModerator,
    ChannelName = "Trade",
    TargetName = "HelpfulPlayer",
    IsModerator = true
};
```

### Notizen
- **Moderator-Powers**: Mute, Kick, Ban (außer Owner)
- **Max Moderators**: 5 pro Channel

---

## ChatMOTD (428)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung
Message-of-the-Day beim Login oder Channel-Join.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| MOTD | string | Message-of-the-Day | Ja |
| Source | string | "server", "guild", "channel" | Ja |
| ChannelName | string | Falls Source=channel | Nein |

### Beispiel Payload
```csharp
// Server-MOTD
var serverMotd = new ChatMOTD
{
    Type = MessageType.ChatMOTD,
    MOTD = "Welcome to 2DMMO! New patch 0.2.0 released!",
    Source = "server"
};

// Channel-MOTD
var channelMotd = new ChatMOTD
{
    Type = MessageType.ChatMOTD,
    MOTD = "Welcome to Trade channel! No spam please.",
    Source = "channel",
    ChannelName = "Trade"
};
```

### Notizen
- **Server-MOTD**: Bei Login
- **Guild-MOTD**: Bei Login (falls in Guild)
- **Channel-MOTD**: Bei Channel-Join

---

## ChatFilter (429)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Server informiert dass Message gefiltert wurde (Profanity-Filter).

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| OriginalMessage | string | Original-Message | Ja |
| FilteredMessage | string | Gefilterte Message | Ja |
| Reason | string | "profanity", "spam", "advertising" | Ja |

### Beispiel Payload
```csharp
var filterNotif = new ChatFilter
{
    Type = MessageType.ChatFilter,
    OriginalMessage = "This is bad word here",
    FilteredMessage = "This is *** here",
    Reason = "profanity"
};
```

### Notizen
- **Auto-Filter**: Server replaced Profanity mit "***"
- **Warning**: 3+ Violations = Temp-Mute

---

## ChatSpamWarning (430)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Server warnt vor Spam-Behavior (zu viele Messages, identische Messages).

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Reason | string | "rate_limit", "duplicate_messages", "flood" | Ja |
| WaitTime | int | Sekunden bis nächste Message erlaubt | Ja |
| ViolationCount | int | Anzahl Violations | Ja |

### Beispiel Payload
```csharp
var spamWarning = new ChatSpamWarning
{
    Type = MessageType.ChatSpamWarning,
    Reason = "rate_limit",
    WaitTime = 30, // 30 Sekunden warten
    ViolationCount = 2 // 2. Violation
};
```

### Error Actions
| Violation Count | Action |
|-----------------|--------|
| 1-2 | Warning |
| 3 | 5-Minuten Mute |
| 4+ | 30-Minuten Mute |

### Notizen
- **Auto-Mute**: Bei 3+ Violations
- **Reset**: Violations resetten nach 1 Stunde
- **UI-Notification**: Client zeigt Warning im Chat

---

## ChatMessageResponse (440)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung
Antwort auf ChatMessage Request. Bestätigt erfolgreiche Message-Übermittlung oder gibt Fehler zurück.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Success | bool | Message übermittelt? | Ja |
| ErrorCode | string | Fehlercode falls Success=false | Nein |
| ErrorMessage | string | Menschenlesbare Fehlermeldung | Nein |
| ChannelType | string | Channel-Type | Bei Erfolg |

### Error Codes
| Code | Bedeutung |
|------|-----------|
| `RATE_LIMITED` | Zu viele Messages |
| `PROFANITY_DETECTED` | Profanity-Filter |
| `NOT_IN_CHANNEL` | Nicht im Channel |
| `MUTED` | Spieler ist gemuted |
| `MESSAGE_TOO_LONG` | >500 Zeichen |

---

## ChatChannelJoinResponse (441)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung
Antwort auf ChatChannelJoin Request. Bestätigt erfolgreichen Channel-Beitritt oder gibt Fehler zurück.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Success | bool | Join erfolgreich? | Ja |
| ErrorCode | string | Fehlercode falls Success=false | Nein |
| ErrorMessage | string | Menschenlesbare Fehlermeldung | Nein |
| ChannelName | string | Channel-Name | Bei Erfolg |

### Error Codes
| Code | Bedeutung |
|------|-----------|
| `CHANNEL_NOT_FOUND` | Channel existiert nicht |
| `WRONG_PASSWORD` | Falsches Password |
| `CHANNEL_FULL` | Max Members erreicht |
| `BANNED` | Vom Channel gebannt |

---

## ChatChannelCreateResponse (442)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung
Antwort auf ChatChannelCreate Request. Bestätigt erfolgreiche Channel-Erstellung oder gibt Fehler zurück.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Success | bool | Erstellung erfolgreich? | Ja |
| ErrorCode | string | Fehlercode falls Success=false | Nein |
| ErrorMessage | string | Menschenlesbare Fehlermeldung | Nein |
| ChannelName | string | Channel-Name | Bei Erfolg |

### Error Codes
| Code | Bedeutung |
|------|-----------|
| `NAME_TAKEN` | Channel existiert bereits |
| `INVALID_NAME` | Name ungültig |
| `MAX_CHANNELS_CREATED` | Max Channels erreicht (3 pro Spieler) |

---

## ChatChannelDeleteResponse (443)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung
Antwort auf ChatChannelDelete Request. Bestätigt erfolgreiche Channel-Löschung oder gibt Fehler zurück.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Success | bool | Löschung erfolgreich? | Ja |
| ErrorCode | string | Fehlercode falls Success=false | Nein |
| ErrorMessage | string | Menschenlesbare Fehlermeldung | Nein |
| ChannelName | string | Channel-Name | Bei Erfolg |

### Error Codes
| Code | Bedeutung |
|------|-----------|
| `NOT_OWNER` | Nur Owner kann deleten |
| `CHANNEL_NOT_FOUND` | Channel existiert nicht |

---

## ChatChannelPasswordResponse (444)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung
Antwort auf ChatChannelPassword Request. Bestätigt erfolgreiche Password-Änderung oder gibt Fehler zurück.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Success | bool | Änderung erfolgreich? | Ja |
| ErrorCode | string | Fehlercode falls Success=false | Nein |
| ErrorMessage | string | Menschenlesbare Fehlermeldung | Nein |
| ChannelName | string | Channel-Name | Bei Erfolg |
| PasswordSet | bool | Password gesetzt (true) oder entfernt (false) | Bei Erfolg |

### Error Codes
| Code | Bedeutung |
|------|-----------|
| `NOT_OWNER` | Nur Owner kann Password ändern |
| `CHANNEL_NOT_FOUND` | Channel existiert nicht |

---

## ChatChannelMuteResponse (445)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung
Antwort auf ChatChannelMute Request. Bestätigt erfolgreiche Mute-Action oder gibt Fehler zurück.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Success | bool | Mute erfolgreich? | Ja |
| ErrorCode | string | Fehlercode falls Success=false | Nein |
| ErrorMessage | string | Menschenlesbare Fehlermeldung | Nein |
| ChannelName | string | Channel-Name | Bei Erfolg |
| TargetName | string | Gemuteter Spieler | Bei Erfolg |
| Duration | int | Dauer in Minuten | Bei Erfolg |

### Error Codes
| Code | Bedeutung |
|------|-----------|
| `NO_PERMISSION` | Nicht Owner/Moderator |
| `PLAYER_NOT_IN_CHANNEL` | Target nicht im Channel |
| `CANNOT_MUTE_OWNER` | Owner kann nicht gemuted werden |

---

**Letzte Aktualisierung**: 2025-12-25  
**Version**: 1.1.0

[← Zurück zur Übersicht](README.md)
