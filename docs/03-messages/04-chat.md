# 💬 Chat Messages (0400-0499)

**Kategorie:** 04  
**Range:** 0400-0499  
**Phase:** Prototyp  
**Status:** 🟢 In Entwicklung

[← Zurück zur Übersicht](README.md)

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

---

## ChatMessage (400)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Client sendet Chat-Nachricht (verschiedene Channels).

### Im Scope ✅
- [Zu dokumentieren basierend auf konkreter Implementierung]

### Nicht im Scope ❌
- [Zu dokumentieren basierend auf konkreter Implementierung]

### Request/Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| [Felder basierend auf Implementation] | type | Beschreibung | Ja/Nein |

### Erwartete Response
- **Bei Erfolg:** [Response Message]
- **Bei Fehler:** `ErrorMessage` (910)

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| [Related] | ID | Beschreibung |

### Beispiel Payload
```csharp
var message = new ChatMessage
{
    Type = MessageType.ChatMessage,
    // Felder hier
};
```

### Notizen
- [Implementierungs-Hinweise]
- [Edge Cases]
- [Performance-Überlegungen]

---

## ChatBroadcast (401)

**Richtung:** 📡 Broadcast  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Server broadcastet Chat-Nachricht an Channel-Teilnehmer.

### Im Scope ✅
- [Zu dokumentieren basierend auf konkreter Implementierung]

### Nicht im Scope ❌
- [Zu dokumentieren basierend auf konkreter Implementierung]

### Request/Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| [Felder basierend auf Implementation] | type | Beschreibung | Ja/Nein |

### Erwartete Response
- **Bei Erfolg:** [Response Message]
- **Bei Fehler:** `ErrorMessage` (910)

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| [Related] | ID | Beschreibung |

### Beispiel Payload
```csharp
var message = new ChatBroadcast
{
    Type = MessageType.ChatBroadcast,
    // Felder hier
};
```

### Notizen
- [Implementierungs-Hinweise]
- [Edge Cases]
- [Performance-Überlegungen]

---

## ChatWhisper (402)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Private Nachricht an anderen Spieler.

### Im Scope ✅
- [Zu dokumentieren basierend auf konkreter Implementierung]

### Nicht im Scope ❌
- [Zu dokumentieren basierend auf konkreter Implementierung]

### Request/Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| [Felder basierend auf Implementation] | type | Beschreibung | Ja/Nein |

### Erwartete Response
- **Bei Erfolg:** [Response Message]
- **Bei Fehler:** `ErrorMessage` (910)

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| [Related] | ID | Beschreibung |

### Beispiel Payload
```csharp
var message = new ChatWhisper
{
    Type = MessageType.ChatWhisper,
    // Felder hier
};
```

### Notizen
- [Implementierungs-Hinweise]
- [Edge Cases]
- [Performance-Überlegungen]

---

## ChatWhisperResponse (403)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Bestätigung oder Fehler für Whisper.

### Im Scope ✅
- [Zu dokumentieren basierend auf konkreter Implementierung]

### Nicht im Scope ❌
- [Zu dokumentieren basierend auf konkreter Implementierung]

### Request/Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| [Felder basierend auf Implementation] | type | Beschreibung | Ja/Nein |

### Erwartete Response
- **Bei Erfolg:** [Response Message]
- **Bei Fehler:** `ErrorMessage` (910)

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| [Related] | ID | Beschreibung |

### Beispiel Payload
```csharp
var message = new ChatWhisperResponse
{
    Type = MessageType.ChatWhisperResponse,
    // Felder hier
};
```

### Notizen
- [Implementierungs-Hinweise]
- [Edge Cases]
- [Performance-Überlegungen]

---

## ChatParty (404)

**Richtung:** 🔄 Bidirektional  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Party-Chat Nachricht.

### Im Scope ✅
- [Zu dokumentieren basierend auf konkreter Implementierung]

### Nicht im Scope ❌
- [Zu dokumentieren basierend auf konkreter Implementierung]

### Request/Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| [Felder basierend auf Implementation] | type | Beschreibung | Ja/Nein |

### Erwartete Response
- **Bei Erfolg:** [Response Message]
- **Bei Fehler:** `ErrorMessage` (910)

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| [Related] | ID | Beschreibung |

### Beispiel Payload
```csharp
var message = new ChatParty
{
    Type = MessageType.ChatParty,
    // Felder hier
};
```

### Notizen
- [Implementierungs-Hinweise]
- [Edge Cases]
- [Performance-Überlegungen]

---

## ChatGuild (405)

**Richtung:** 🔄 Bidirektional  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Guild-Chat Nachricht.

### Im Scope ✅
- [Zu dokumentieren basierend auf konkreter Implementierung]

### Nicht im Scope ❌
- [Zu dokumentieren basierend auf konkreter Implementierung]

### Request/Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| [Felder basierend auf Implementation] | type | Beschreibung | Ja/Nein |

### Erwartete Response
- **Bei Erfolg:** [Response Message]
- **Bei Fehler:** `ErrorMessage` (910)

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| [Related] | ID | Beschreibung |

### Beispiel Payload
```csharp
var message = new ChatGuild
{
    Type = MessageType.ChatGuild,
    // Felder hier
};
```

### Notizen
- [Implementierungs-Hinweise]
- [Edge Cases]
- [Performance-Überlegungen]

---

## ChatRaid (406)

**Richtung:** 🔄 Bidirektional  
**Frequenz:** Häufig  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung
Phase 2: Raid-Chat Nachricht.

### Im Scope ✅
- [Zu dokumentieren basierend auf konkreter Implementierung]

### Nicht im Scope ❌
- [Zu dokumentieren basierend auf konkreter Implementierung]

### Request/Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| [Felder basierend auf Implementation] | type | Beschreibung | Ja/Nein |

### Erwartete Response
- **Bei Erfolg:** [Response Message]
- **Bei Fehler:** `ErrorMessage` (910)

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| [Related] | ID | Beschreibung |

### Beispiel Payload
```csharp
var message = new ChatRaid
{
    Type = MessageType.ChatRaid,
    // Felder hier
};
```

### Notizen
- [Implementierungs-Hinweise]
- [Edge Cases]
- [Performance-Überlegungen]

---

## ChatZone (407)

**Richtung:** 📡 Broadcast  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Zone-weiter Chat (Local).

### Im Scope ✅
- [Zu dokumentieren basierend auf konkreter Implementierung]

### Nicht im Scope ❌
- [Zu dokumentieren basierend auf konkreter Implementierung]

### Request/Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| [Felder basierend auf Implementation] | type | Beschreibung | Ja/Nein |

### Erwartete Response
- **Bei Erfolg:** [Response Message]
- **Bei Fehler:** `ErrorMessage` (910)

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| [Related] | ID | Beschreibung |

### Beispiel Payload
```csharp
var message = new ChatZone
{
    Type = MessageType.ChatZone,
    // Felder hier
};
```

### Notizen
- [Implementierungs-Hinweise]
- [Edge Cases]
- [Performance-Überlegungen]

---

## ChatTrade (408)

**Richtung:** 📡 Broadcast  
**Frequenz:** Häufig  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung
Phase 2: Trade-Channel.

### Im Scope ✅
- [Zu dokumentieren basierend auf konkreter Implementierung]

### Nicht im Scope ❌
- [Zu dokumentieren basierend auf konkreter Implementierung]

### Request/Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| [Felder basierend auf Implementation] | type | Beschreibung | Ja/Nein |

### Erwartete Response
- **Bei Erfolg:** [Response Message]
- **Bei Fehler:** `ErrorMessage` (910)

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| [Related] | ID | Beschreibung |

### Beispiel Payload
```csharp
var message = new ChatTrade
{
    Type = MessageType.ChatTrade,
    // Felder hier
};
```

### Notizen
- [Implementierungs-Hinweise]
- [Edge Cases]
- [Performance-Überlegungen]

---

## ChatLFG (409)

**Richtung:** 📡 Broadcast  
**Frequenz:** Häufig  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung
Phase 2: Looking for Group Channel.

### Im Scope ✅
- [Zu dokumentieren basierend auf konkreter Implementierung]

### Nicht im Scope ❌
- [Zu dokumentieren basierend auf konkreter Implementierung]

### Request/Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| [Felder basierend auf Implementation] | type | Beschreibung | Ja/Nein |

### Erwartete Response
- **Bei Erfolg:** [Response Message]
- **Bei Fehler:** `ErrorMessage` (910)

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| [Related] | ID | Beschreibung |

### Beispiel Payload
```csharp
var message = new ChatLFG
{
    Type = MessageType.ChatLFG,
    // Felder hier
};
```

### Notizen
- [Implementierungs-Hinweise]
- [Edge Cases]
- [Performance-Überlegungen]

---

## ChatSystem (410)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
System-Nachricht (Server-Announcement, Game-Event).

### Im Scope ✅
- [Zu dokumentieren basierend auf konkreter Implementierung]

### Nicht im Scope ❌
- [Zu dokumentieren basierend auf konkreter Implementierung]

### Request/Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| [Felder basierend auf Implementation] | type | Beschreibung | Ja/Nein |

### Erwartete Response
- **Bei Erfolg:** [Response Message]
- **Bei Fehler:** `ErrorMessage` (910)

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| [Related] | ID | Beschreibung |

### Beispiel Payload
```csharp
var message = new ChatSystem
{
    Type = MessageType.ChatSystem,
    // Felder hier
};
```

### Notizen
- [Implementierungs-Hinweise]
- [Edge Cases]
- [Performance-Überlegungen]

---

## ChatYell (411)

**Richtung:** 📡 Broadcast  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Yell (größere Range als Say).

### Im Scope ✅
- [Zu dokumentieren basierend auf konkreter Implementierung]

### Nicht im Scope ❌
- [Zu dokumentieren basierend auf konkreter Implementierung]

### Request/Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| [Felder basierend auf Implementation] | type | Beschreibung | Ja/Nein |

### Erwartete Response
- **Bei Erfolg:** [Response Message]
- **Bei Fehler:** `ErrorMessage` (910)

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| [Related] | ID | Beschreibung |

### Beispiel Payload
```csharp
var message = new ChatYell
{
    Type = MessageType.ChatYell,
    // Felder hier
};
```

### Notizen
- [Implementierungs-Hinweise]
- [Edge Cases]
- [Performance-Überlegungen]

---

## ChatSay (412)

**Richtung:** 📡 Broadcast  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Say (kleine Range, lokaler Chat).

### Im Scope ✅
- [Zu dokumentieren basierend auf konkreter Implementierung]

### Nicht im Scope ❌
- [Zu dokumentieren basierend auf konkreter Implementierung]

### Request/Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| [Felder basierend auf Implementation] | type | Beschreibung | Ja/Nein |

### Erwartete Response
- **Bei Erfolg:** [Response Message]
- **Bei Fehler:** `ErrorMessage` (910)

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| [Related] | ID | Beschreibung |

### Beispiel Payload
```csharp
var message = new ChatSay
{
    Type = MessageType.ChatSay,
    // Felder hier
};
```

### Notizen
- [Implementierungs-Hinweise]
- [Edge Cases]
- [Performance-Überlegungen]

---

## ChatEmote (413)

**Richtung:** 📡 Broadcast  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Emote-Text (roleplaying).

### Im Scope ✅
- [Zu dokumentieren basierend auf konkreter Implementierung]

### Nicht im Scope ❌
- [Zu dokumentieren basierend auf konkreter Implementierung]

### Request/Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| [Felder basierend auf Implementation] | type | Beschreibung | Ja/Nein |

### Erwartete Response
- **Bei Erfolg:** [Response Message]
- **Bei Fehler:** `ErrorMessage` (910)

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| [Related] | ID | Beschreibung |

### Beispiel Payload
```csharp
var message = new ChatEmote
{
    Type = MessageType.ChatEmote,
    // Felder hier
};
```

### Notizen
- [Implementierungs-Hinweise]
- [Edge Cases]
- [Performance-Überlegungen]

---

## ChatAFK (414)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Spieler ist AFK.

### Im Scope ✅
- [Zu dokumentieren basierend auf konkreter Implementierung]

### Nicht im Scope ❌
- [Zu dokumentieren basierend auf konkreter Implementierung]

### Request/Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| [Felder basierend auf Implementation] | type | Beschreibung | Ja/Nein |

### Erwartete Response
- **Bei Erfolg:** [Response Message]
- **Bei Fehler:** `ErrorMessage` (910)

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| [Related] | ID | Beschreibung |

### Beispiel Payload
```csharp
var message = new ChatAFK
{
    Type = MessageType.ChatAFK,
    // Felder hier
};
```

### Notizen
- [Implementierungs-Hinweise]
- [Edge Cases]
- [Performance-Überlegungen]

---

## ChatDND (415)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Spieler ist DND (Do Not Disturb).

### Im Scope ✅
- [Zu dokumentieren basierend auf konkreter Implementierung]

### Nicht im Scope ❌
- [Zu dokumentieren basierend auf konkreter Implementierung]

### Request/Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| [Felder basierend auf Implementation] | type | Beschreibung | Ja/Nein |

### Erwartete Response
- **Bei Erfolg:** [Response Message]
- **Bei Fehler:** `ErrorMessage` (910)

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| [Related] | ID | Beschreibung |

### Beispiel Payload
```csharp
var message = new ChatDND
{
    Type = MessageType.ChatDND,
    // Felder hier
};
```

### Notizen
- [Implementierungs-Hinweise]
- [Edge Cases]
- [Performance-Überlegungen]

---


**Letzte Aktualisierung**: 2025-12-17  
**Version**: 1.0.0

[← Zurück zur Übersicht](README.md)
