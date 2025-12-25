# 👥 Party Messages (700-799)

**Kategorie:** 07  
**Range:** 700-799  
**Phase:** Phase 2  
**Status:** 🟡 Phase 2

[← Zurück zur Übersicht](README.md)

---

## 📋 Inhaltsverzeichnis

- [PartyInvite (700)](#partyinvite-700)
- [PartyInviteReceived (701)](#partyinvitereceived-701)
- [PartyAccept (702)](#partyaccept-702)
- [PartyDecline (703)](#partydecline-703)
- [PartyJoin (704)](#partyjoin-704)
- [PartyLeave (705)](#partyleave-705)
- [PartyKick (706)](#partykick-706)
- [PartyPromote (707)](#partyromote-707)
- [PartyDisband (708)](#partydisband-708)
- [PartyMemberUpdate (710)](#partymemberupdate-710)
- [PartyMemberOffline (711)](#partymemberoffline-711)
- [PartyLootMode (720)](#partylootmode-720)
- [PartyReadyCheck (730)](#partyreadycheck-730)
- [PartyReadyCheckResponse (731)](#partyreadycheckresponse-731)
- [PartyInviteResponse (740)](#partyinviteresponse-740)
- [PartyAcceptResponse (741)](#partyacceptresponse-741)
- [PartyLeaveResponse (742)](#partyleaveresponse-742)
- [PartyKickResponse (743)](#partykickresponse-743)
- [PartyPromoteResponse (744)](#partypromoteresponse-744)
- [PartyDisbandResponse (745)](#partydisbandresponse-745)
- [PartyLootModeResponse (746)](#partylootmoderesponse-746)
- [PartyReadyCheckStartResponse (747)](#partyreadycheckstartresponse-747)

---

## 📋 Übersicht

Diese Kategorie umfasst alle Messages für das **Party-System** im 2DMMO.

Das Party-System implementiert:
- **Group-Formation**: Invites, Accept, Decline, Auto-Join
- **Party-Management**: Leader-Promotion, Kick, Disband
- **Member-Tracking**: HP/Mana, Position, Status (Online/Offline/Dead)
- **Loot-System**: Group-Loot, Round-Robin, Master-Looter, Need-Before-Greed (Phase 2)
- **XP-Sharing**: XP-Bonus für Gruppe (10-20% je nach Größe)
- **Ready-Check**: Für Dungeon/Boss-Pulls
- **Party-Chat**: Dedizierter Chat-Channel → siehe `ChatParty` (404)

**Server Authority**: Alle Party-Changes sind server-authoritative.

**Party-Size**: Max 5 Spieler (Raid = 40 Spieler in Phase 2)

**XP-Range**: Max 100m zwischen Party-Members für XP-Share

**Level-Range**: Max 10 Level-Differenz für XP-Share (flexible basierend auf höchstem Level)

---

## PartyInvite (700)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine (oder Party-Leader falls Party bereits existiert)

### Beschreibung
Client sendet Party-Einladung an anderen Spieler. Falls Client noch keine Party hat, wird automatisch eine neue Party erstellt mit ihm als Leader. Target erhält `PartyInviteReceived` (701).

### Im Scope ✅
- Party-Einladung an Online-Spieler
- Auto-Party-Creation (falls Client noch keine Party)
- Invite-Queue (mehrere Invites möglich)
- Cross-Zone Invites

### Nicht im Scope ❌
- Raid-Invites → Phase 2 (RaidInvite Message)
- Offline-Invites → nicht möglich
- Guild-Mass-Invite → Phase 3

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| TargetName | string | Einzuladender Spieler-Name | Ja |

### Erwartete Response
- `PartyInviteResponse` (740)

### Folge-Messages bei Erfolg
- `PartyInviteReceived` (701) an Target-Spieler

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `PartyInviteReceived` | 701 | Target erhält Invite |
| `PartyAccept` | 702 | Target akzeptiert |
| `PartyDecline` | 703 | Target lehnt ab |
| `PartyJoin` | 704 | Nach Accept |

### Flow-Diagramm
```
Inviter                   Server                    Target
  │                          │                          │
  │  PartyInvite (700)       │                          │
  │  TargetName="Legolas"    │                          │
  │─────────────────────────►│                          │
  │                          │  ┌─ Validate: Target online?
  │                          │  ├─ Validate: Not in Party?
  │                          │  ├─ Validate: Not blocked?
  │                          │  ├─ Create Party (if needed)
  │                          │  └─ Add to Invite-Queue
  │                          │                          │
  │  PartyInviteSent (ack)   │  PartyInviteReceived (701)│
  │◄─────────────────────────│─────────────────────────►│
  │                          │                          │
  │                          │                          │  (UI shows Invite)
  │                          │                          │  (60s Timer)
```

### Beispiel Payload
```csharp
var partyInvite = new PartyInvite
{
    Type = MessageType.PartyInvite,
    TargetName = "Legolas"
};
```

### Error Codes
| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `PLAYER_NOT_FOUND` | Target nicht online | Namen prüfen |
| `ALREADY_IN_PARTY` | Target ist bereits in Party | Target muss erst Party verlassen |
| `PARTY_FULL` | Party ist voll (5/5) | Member kicken oder Raid konvertieren |
| `PLAYER_BLOCKED_YOU` | Target hat Inviter blockiert | - |
| `PLAYER_DECLINED_INVITES` | Target hat Invites deaktiviert | - |
| `INVITE_ALREADY_PENDING` | Invite bereits gesendet | Auf Antwort warten |
| `NOT_PARTY_LEADER` | Nur Leader darf inviten | - |

### Notizen
- **Auto-Party-Creation**: Falls Inviter keine Party hat, wird eine erstellt
- **Invite-Timeout**: 60 Sekunden für Accept/Decline
- **Invite-Queue**: Max 5 gleichzeitige Invites pro Party
- **Cross-Zone**: Invites funktionieren Zone-übergreifend
- **UI-Notification**: "You invited Legolas to your party."

---

## PartyInviteReceived (701)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Server informiert Spieler über eingehende Party-Einladung. Client zeigt UI-Popup mit Accept/Decline-Buttons.

### Im Scope ✅
- Invite-Notification
- Inviter-Info (Name, Level, Class)
- Timeout-Information
- Auto-Decline bei Timeout

### Nicht im Scope ❌
- Auto-Accept → muss manuell sein
- Invite-Forwarding → nicht möglich

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| InviterId | long | Character-ID des Inviters | Ja |
| InviterName | string | Inviter-Name | Ja |
| InviterLevel | int | Inviter-Level | Ja |
| InviterClass | byte | Inviter-Class | Ja |
| TimeoutSeconds | int | Sekunden bis Auto-Decline (60) | Ja |
| PartySize | int | Aktuelle Party-Größe (1-5) | Ja |

### Erwartete Response
- Client sendet `PartyAccept` (702) oder `PartyDecline` (703)
- Bei Timeout: Auto-`PartyDecline`

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `PartyInvite` | 700 | Source von dieser Message |
| `PartyAccept` | 702 | Invite annehmen |
| `PartyDecline` | 703 | Invite ablehnen |

### Beispiel Payload
```csharp
var inviteReceived = new PartyInviteReceived
{
    Type = MessageType.PartyInviteReceived,
    InviterId = 98765,
    InviterName = "Aragorn",
    InviterLevel = 10,
    InviterClass = 1, // Warrior
    TimeoutSeconds = 60,
    PartySize = 2 // 2/5 Members already
};
```

### Notizen
- **UI-Popup**: "Aragorn (Warrior, Level 10) has invited you to join their party. (2/5)"
- **Sound**: Party-Invite Sound
- **Timeout**: 60 Sekunden → dann Auto-Decline
- **Multi-Invite**: Kann mehrere gleichzeitig haben (zeigt Liste)

---

## PartyAccept (702)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Client akzeptiert Party-Invite. Server validiert ob Invite noch gültig, Party nicht voll, und added Spieler zur Party.

### Im Scope ✅
- Invite-Accept
- Auto-Join zur Party
- Party-Member-Broadcast an alle Members

### Nicht im Scope ❌
- Accept für bereits volle Party → Error
- Accept nach Timeout → Error

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| InviterId | long | Inviter Character-ID | Ja |

### Erwartete Response
- `PartyAcceptResponse` (741)

### Folge-Messages bei Erfolg
- `PartyJoin` (704) Broadcast an alle Party-Members (inkl. Self)

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `PartyInviteReceived` | 701 | Invite das akzeptiert wird |
| `PartyJoin` | 704 | Join-Notification |

### Beispiel Payload
```csharp
var partyAccept = new PartyAccept
{
    Type = MessageType.PartyAccept,
    InviterId = 98765
};
```

### Error Codes
| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `INVITE_EXPIRED` | Invite abgelaufen (>60s) | Neuen Invite anfordern |
| `PARTY_FULL` | Party ist voll | - |
| `ALREADY_IN_PARTY` | Spieler ist bereits in Party | Party verlassen erst |
| `INVITE_NOT_FOUND` | Invite existiert nicht | - |

### Notizen
- **Auto-Join**: Sofortiger Join nach Accept
- **XP-Sharing**: Ab jetzt XP-Share aktiv
- **Party-Chat**: Auto-Join zu Party-Chat-Channel
- **UI-Update**: Party-Frames werden angezeigt

---

## PartyDecline (703)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Client lehnt Party-Invite ab. Server informiert Inviter.

### Im Scope ✅
- Invite-Decline
- Inviter-Notification

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| InviterId | long | Inviter Character-ID | Ja |

### Erwartete Response
- Server sendet Notification an Inviter: "Legolas declined your party invitation."

### Beispiel Payload
```csharp
var partyDecline = new PartyDecline
{
    Type = MessageType.PartyDecline,
    InviterId = 98765
};
```

### Notizen
- **No Penalty**: Kein Penalty fürs Decline
- **Inviter-Notification**: "Legolas declined your party invitation."
- **Auto-Decline**: Bei Timeout wird automatisch declined

---

## PartyJoin (704)

**Richtung:** 📡 Broadcast (Server → All Party Members)  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Server broadcastet an alle Party-Members dass neuer Spieler joined. Enthält vollständige Member-Info.

### Im Scope ✅
- Join-Notification an alle Members
- Neue Member-Info (Name, Level, Class, HP, Mana, Position)
- Party-Roster-Update

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| PlayerId | long | Neues Member Character-ID | Ja |
| PlayerName | string | Name | Ja |
| PlayerLevel | int | Level | Ja |
| PlayerClass | byte | Class | Ja |
| CurrentHP | int | Aktuelle HP | Ja |
| MaxHP | int | Max HP | Ja |
| CurrentMana | int | Aktuelles Mana | Ja |
| MaxMana | int | Max Mana | Ja |
| ZoneId | uint | Zone-ID | Ja |
| X | float | Position X | Ja |
| Y | float | Position Y | Ja |
| IsLeader | bool | Ist Party-Leader? | Ja |

### Erwartete Response
- Keine Response erforderlich

### Beispiel Payload
```csharp
var partyJoin = new PartyJoin
{
    Type = MessageType.PartyJoin,
    PlayerId = 54321,
    PlayerName = "Legolas",
    PlayerLevel = 9,
    PlayerClass = 3, // Rogue
    CurrentHP = 800,
    MaxHP = 900,
    CurrentMana = 350,
    MaxMana = 400,
    ZoneId = 1001,
    X = 150.5f,
    Y = 200.3f,
    IsLeader = false
};
```

### Notizen
- **Broadcast**: An alle Party-Members (inkl. Joiner selbst)
- **UI-Update**: Client added Party-Frame für neuen Member
- **Chat-Notification**: "Legolas has joined the party."
- **Party-Size**: Jetzt 3/5 Members

---

## PartyLeave (705)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Client verlässt Party freiwillig. Server broadcastet Leave-Event an verbleibende Members.

### Im Scope ✅
- Voluntary Leave
- Broadcast an verbleibende Members
- Leader-Reassignment (falls Leader leaved)
- Auto-Disband (falls letzter Member)

### Request Payload
Keine zusätzlichen Felder (nur MessageType)

### Erwartete Response
- `PartyLeaveResponse` (742)

### Folge-Messages bei Erfolg
- `PartyLeaveNotification` Broadcast an verbleibende Members
- `PartyPromote` (707) falls Leader leaved (neuer Leader wird gewählt)

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `PartyKick` | 706 | Forced Leave |
| `PartyDisband` | 708 | Party wird aufgelöst |

### Flow-Diagramm
```
Leaving Player            Server              Remaining Members
  │                          │                          │
  │  PartyLeave (705)        │                          │
  │─────────────────────────►│                          │
  │                          │  Remove from Party       │
  │                          │  Reassign Leader?        │
  │                          │                          │
  │  PartyLeaveSuccess       │  PartyLeaveNotification  │
  │◄─────────────────────────│─────────────────────────►│
  │                          │                          │
  │                          │  (if Leader)             │
  │                          │  PartyPromote (707)      │
  │                          │  (New Leader)            │
  │                          │─────────────────────────►│
```

### Beispiel Payload
```csharp
var partyLeave = new PartyLeave
{
    Type = MessageType.PartyLeave
};
```

### Notizen
- **Leader-Reassignment**: Nächster Member wird Leader (by Join-Order)
- **Auto-Disband**: Falls letzter Member leaved
- **XP-Sharing**: Sofort deaktiviert
- **Party-Chat**: Auto-Leave aus Chat-Channel
- **UI-Update**: Party-Frames werden entfernt
- **Chat-Notification**: "Legolas has left the party."

---

## PartyKick (706)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Party-Leader

### Beschreibung
Party-Leader kicked Member aus der Party. Server validiert Leader-Status und führt Kick durch.

### Im Scope ✅
- Forced Removal von Party-Member
- Nur Leader darf kicken
- Kick-Reason (optional)

### Nicht im Scope ❌
- Self-Kick → verwende `PartyLeave` (705)
- Kick von Leader → Leader kann sich nur selbst via Leave entfernen

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| PlayerId | long | Zu kickender Spieler | Ja |
| Reason | string | Optional Kick-Grund | Nein |

### Erwartete Response
- `PartyKickResponse` (743)

### Folge-Messages bei Erfolg
- `PartyKickNotification` an alle Members (inkl. Kicked Player)

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `PartyLeave` | 705 | Voluntary Leave |
| `PartyPromote` | 707 | Leader-Change |

### Beispiel Payload
```csharp
var partyKick = new PartyKick
{
    Type = MessageType.PartyKick,
    PlayerId = 54321,
    Reason = "AFK too long"
};
```

### Error Codes
| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `NOT_PARTY_LEADER` | Nur Leader darf kicken | - |
| `PLAYER_NOT_IN_PARTY` | Spieler nicht in Party | - |
| `CANNOT_KICK_SELF` | Leader kann sich nicht selbst kicken | Verwende PartyLeave |

### Notizen
- **Leader-Only**: Nur Leader darf kicken
- **Notification**: "You have been removed from the party. Reason: AFK too long"
- **Chat-Notification**: "Legolas has been removed from the party."
- **No Cooldown**: Kein Cooldown fürs Kicken

---

## PartyPromote (707)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Party-Leader

### Beschreibung
Party-Leader übergibt Leadership an anderen Member. Server validiert und führt Promotion durch.

### Im Scope ✅
- Leadership-Transfer
- Nur Leader darf promoten
- Broadcast an alle Members

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| PlayerId | long | Neuer Leader | Ja |

### Erwartete Response
- `PartyPromoteResponse` (744)

### Folge-Messages bei Erfolg
- `PartyPromoteNotification` an alle Members

### Beispiel Payload
```csharp
var partyPromote = new PartyPromote
{
    Type = MessageType.PartyPromote,
    PlayerId = 54321 // Legolas wird Leader
};
```

### Error Codes
| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `NOT_PARTY_LEADER` | Nur Leader darf promoten | - |
| `PLAYER_NOT_IN_PARTY` | Spieler nicht in Party | - |
| `CANNOT_PROMOTE_SELF` | Bereits Leader | - |

### Notizen
- **Leader-Powers**: Neuer Leader kann nun inviten, kicken, promoten, disband
- **Chat-Notification**: "Legolas is now the party leader."
- **UI-Update**: Crown-Icon wechselt zum neuen Leader

---

## PartyDisband (708)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Party-Leader

### Beschreibung
Party-Leader löst Party komplett auf. Alle Members werden gekickt.

### Im Scope ✅
- Vollständige Party-Auflösung
- Nur Leader darf disband
- Broadcast an alle Members

### Request Payload
Keine zusätzlichen Felder

### Erwartete Response
- `PartyDisbandResponse` (745)

### Folge-Messages bei Erfolg
- `PartyDisbandNotification` an alle Members

### Beispiel Payload
```csharp
var partyDisband = new PartyDisband
{
    Type = MessageType.PartyDisband
};
```

### Error Codes
| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `NOT_PARTY_LEADER` | Nur Leader darf disband | - |
| `NOT_IN_PARTY` | Nicht in Party | - |

### Notizen
- **Leader-Only**: Nur Leader darf disband
- **Chat-Notification**: "The party has been disbanded."
- **Alternative**: Leader kann auch einfach Leave (dann auto-promote)

---

## PartyMemberUpdate (710)

**Richtung:** 📡 Broadcast (Server → All Party Members)  
**Frequenz:** ⚡ Sehr häufig (Combat)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Server broadcastet Member-Stats-Update an alle Party-Members. Wird bei HP/Mana-Changes, Position-Changes, oder Status-Changes gesendet.

### Im Scope ✅
- HP/Mana Updates
- Position Updates
- Status Updates (Dead, Ghost, AFK)
- Zone-Change

### Nicht im Scope ❌
- Full Member-Info → nur Deltas
- Non-Party-Members → kein Broadcast

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| PlayerId | long | Member Character-ID | Ja |
| CurrentHP | int | Aktuelle HP | Nein |
| MaxHP | int | Max HP | Nein |
| CurrentMana | int | Aktuelles Mana | Nein |
| MaxMana | int | Max Mana | Nein |
| X | float | Position X | Nein |
| Y | float | Position Y | Nein |
| ZoneId | uint | Zone-ID | Nein |
| IsDead | bool | Ist tot? | Nein |
| IsGhost | bool | Ist Ghost? | Nein |
| IsAFK | bool | Ist AFK? | Nein |

### Erwartete Response
- Keine Response erforderlich

### Beispiel Payload
```csharp
// HP-Update (Combat)
var memberUpdate = new PartyMemberUpdate
{
    Type = MessageType.PartyMemberUpdate,
    PlayerId = 54321,
    CurrentHP = 650,
    MaxHP = 900
    // Andere Felder nicht gesendet (keine Changes)
};

// Position-Update
var posUpdate = new PartyMemberUpdate
{
    Type = MessageType.PartyMemberUpdate,
    PlayerId = 54321,
    X = 155.2f,
    Y = 205.8f
};

// Zone-Change
var zoneChange = new PartyMemberUpdate
{
    Type = MessageType.PartyMemberUpdate,
    PlayerId = 54321,
    ZoneId = 1002 // New Zone
};
```

### Notizen
- **Update-Frequency**: Max 5/Sekunde per Member (gebatched)
- **Delta-Only**: Nur geänderte Felder werden gesendet
- **UI-Update**: Client aktualisiert Party-Frames
- **Party-Frames**: Zeigen HP-Bars, Mana-Bars, Position auf Map

---

## PartyMemberOffline (711)

**Richtung:** 📡 Broadcast (Server → All Party Members)  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Server broadcastet dass Party-Member offline gegangen ist (Disconnect, Logout). Member bleibt in Party für 5 Minuten (Reconnect-Window).

### Im Scope ✅
- Offline-Notification
- Reconnect-Window (5 Minuten)
- Auto-Kick nach Timeout

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| PlayerId | long | Offline Member | Ja |
| ReconnectWindowSeconds | int | Sekunden bis Auto-Kick (300) | Ja |

### Beispiel Payload
```csharp
var memberOffline = new PartyMemberOffline
{
    Type = MessageType.PartyMemberOffline,
    PlayerId = 54321,
    ReconnectWindowSeconds = 300 // 5 min
};
```

### Notizen
- **Reconnect-Window**: 5 Minuten bevor Auto-Kick
- **XP-Sharing**: Deaktiviert während Offline
- **UI-Update**: Member-Frame grayed out
- **Chat-Notification**: "Legolas has gone offline."
- **Reconnect**: Bei Reconnect → `PartyMemberOnline` Event

---

## PartyLootMode (720)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Party-Leader

### Beschreibung
**Phase 2 Feature** - Party-Leader ändert Loot-Mode. Bestimmt wie Loot verteilt wird.

### Im Scope ✅
- Loot-Mode-Change
- Modes: Group-Loot, Round-Robin, Master-Looter, Need-Before-Greed
- Master-Looter-Assignment

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| LootMode | string | "group", "round_robin", "master", "need_greed" | Ja |
| MasterLooterId | long | Master-Looter (nur bei mode=master) | Nein |

### Erwartete Response
- `PartyLootModeResponse` (746)

### Folge-Messages bei Erfolg
- `PartyLootModeChanged` Broadcast an alle Members

### Beispiel Payload
```csharp
var lootMode = new PartyLootMode
{
    Type = MessageType.PartyLootMode,
    LootMode = "master",
    MasterLooterId = 98765 // Leader ist Master-Looter
};
```

### Error Codes
| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `NOT_PARTY_LEADER` | Nur Leader darf ändern | - |
| `INVALID_LOOT_MODE` | Ungültiger Mode | - |
| `PLAYER_NOT_IN_PARTY` | Master-Looter nicht in Party | - |

### Notizen
- **Phase 2**: Nicht im Prototyp
- **Default**: Group-Loot (alle können looten)
- **Loot-Modes**:
  - **Group-Loot**: Jeder kann looten
  - **Round-Robin**: Abwechselnd
  - **Master-Looter**: Nur Master-Looter verteilt
  - **Need-Before-Greed**: Roll-System

---

## PartyReadyCheck (730)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Party-Leader

### Beschreibung
Party-Leader startet Ready-Check. Alle Members müssen "Ready" klicken.

### Im Scope ✅
- Ready-Check-Initiation
- Broadcast an alle Members
- Timeout (30s)

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Question | string | Optional Question (z.B. "Ready for boss?") | Nein |

### Erwartete Response
- `PartyReadyCheckStartResponse` (747)

### Folge-Messages bei Erfolg
- `PartyReadyCheckStart` Broadcast an alle Members

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `PartyReadyCheckResponse` | 731 | Members antworten |

### Beispiel Payload
```csharp
var readyCheck = new PartyReadyCheck
{
    Type = MessageType.PartyReadyCheck,
    Question = "Ready for boss pull?"
};
```

### Notizen
- **Leader-Only**: Nur Leader kann starten
- **Timeout**: 30 Sekunden
- **UI**: Popup mit Ready/Not Ready Buttons
- **Result**: Nach Timeout oder alle geantwortet → Result-Broadcast

---

## PartyReadyCheckResponse (731)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Party-Member antwortet auf Ready-Check.

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| IsReady | bool | Ready? | Ja |

### Erwartete Response
- **Bei Erfolg:** `PartyReadyCheckUpdate` Broadcast (Fortschritt)

### Beispiel Payload
```csharp
var readyResponse = new PartyReadyCheckResponse
{
    Type = MessageType.PartyReadyCheckResponse,
    IsReady = true
};
```

### Notizen
- **Broadcast**: Fortschritt wird an alle gesendet (3/5 Ready)
- **Result**: Alle Ready → "Party is ready!"
- **Not Ready**: Zeigt wer nicht ready ist

---

## PartyInviteResponse (740)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung
Antwort auf PartyInvite Request. Bestätigt erfolgreiche Einladungs-Versendung oder gibt Fehler zurück.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Success | bool | Invite versendet? | Ja |
| ErrorCode | string | Fehlercode falls Success=false | Nein |
| ErrorMessage | string | Menschenlesbare Fehlermeldung | Nein |
| TargetName | string | Eingeladener Spieler | Bei Erfolg |

### Error Codes
| Code | Bedeutung |
|------|-----------|
| `PLAYER_NOT_FOUND` | Target nicht online |
| `ALREADY_IN_PARTY` | Target ist bereits in Party |
| `PARTY_FULL` | Party ist voll (5/5) |
| `PLAYER_BLOCKED_YOU` | Target hat Inviter blockiert |
| `PLAYER_DECLINED_INVITES` | Target hat Invites deaktiviert |
| `INVITE_ALREADY_PENDING` | Invite bereits gesendet |
| `NOT_PARTY_LEADER` | Nur Leader darf inviten |

---

## PartyAcceptResponse (741)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung
Antwort auf PartyAccept Request. Bestätigt erfolgreichen Party-Beitritt oder gibt Fehler zurück.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Success | bool | Join erfolgreich? | Ja |
| ErrorCode | string | Fehlercode falls Success=false | Nein |
| ErrorMessage | string | Menschenlesbare Fehlermeldung | Nein |
| PartySize | int | Aktuelle Party-Größe | Bei Erfolg |

### Error Codes
| Code | Bedeutung |
|------|-----------|
| `INVITE_EXPIRED` | Invite ist abgelaufen (60s Timeout) |
| `PARTY_FULL` | Party wurde voll während Accept |
| `INVITE_CANCELLED` | Inviter hat Invite zurückgezogen |

---

## PartyLeaveResponse (742)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung
Antwort auf PartyLeave Request. Bestätigt erfolgreichen Party-Austritt.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Success | bool | Leave erfolgreich? | Ja |
| ErrorCode | string | Fehlercode falls Success=false | Nein |
| ErrorMessage | string | Menschenlesbare Fehlermeldung | Nein |

---

## PartyKickResponse (743)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung
Antwort auf PartyKick Request. Bestätigt erfolgreichen Kick oder gibt Fehler zurück.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Success | bool | Kick erfolgreich? | Ja |
| ErrorCode | string | Fehlercode falls Success=false | Nein |
| ErrorMessage | string | Menschenlesbare Fehlermeldung | Nein |
| KickedPlayerName | string | Name des gekickten Spielers | Bei Erfolg |

### Error Codes
| Code | Bedeutung |
|------|-----------|
| `NOT_PARTY_LEADER` | Nur Leader darf kicken |
| `PLAYER_NOT_IN_PARTY` | Target nicht in Party |
| `CANNOT_KICK_SELF` | Leader kann sich nicht selbst kicken (use Leave) |

---

## PartyPromoteResponse (744)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung
Antwort auf PartyPromote Request. Bestätigt erfolgreiche Leader-Übergabe oder gibt Fehler zurück.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Success | bool | Promote erfolgreich? | Ja |
| ErrorCode | string | Fehlercode falls Success=false | Nein |
| ErrorMessage | string | Menschenlesbare Fehlermeldung | Nein |
| NewLeaderName | string | Name des neuen Leaders | Bei Erfolg |

### Error Codes
| Code | Bedeutung |
|------|-----------|
| `NOT_PARTY_LEADER` | Nur Leader darf promoten |
| `PLAYER_NOT_IN_PARTY` | Target nicht in Party |

---

## PartyDisbandResponse (745)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung
Antwort auf PartyDisband Request. Bestätigt erfolgreiche Party-Auflösung oder gibt Fehler zurück.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Success | bool | Disband erfolgreich? | Ja |
| ErrorCode | string | Fehlercode falls Success=false | Nein |
| ErrorMessage | string | Menschenlesbare Fehlermeldung | Nein |

### Error Codes
| Code | Bedeutung |
|------|-----------|
| `NOT_PARTY_LEADER` | Nur Leader darf disband |

---

## PartyLootModeResponse (746)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung
**Phase 2 Feature** - Antwort auf PartyLootMode Request. Bestätigt erfolgreiche Loot-Mode-Änderung oder gibt Fehler zurück.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Success | bool | Änderung erfolgreich? | Ja |
| ErrorCode | string | Fehlercode falls Success=false | Nein |
| ErrorMessage | string | Menschenlesbare Fehlermeldung | Nein |
| LootMode | string | Neuer Loot-Mode | Bei Erfolg |

### Error Codes
| Code | Bedeutung |
|------|-----------|
| `NOT_PARTY_LEADER` | Nur Leader darf ändern |
| `INVALID_LOOT_MODE` | Ungültiger Mode |
| `PLAYER_NOT_IN_PARTY` | Master-Looter nicht in Party |

---

## PartyReadyCheckStartResponse (747)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung
Antwort auf PartyReadyCheck Request. Bestätigt erfolgreichen Ready-Check-Start oder gibt Fehler zurück.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Success | bool | Start erfolgreich? | Ja |
| ErrorCode | string | Fehlercode falls Success=false | Nein |
| ErrorMessage | string | Menschenlesbare Fehlermeldung | Nein |
| ReadyCheckId | uint | ID des Ready-Checks | Bei Erfolg |

### Error Codes
| Code | Bedeutung |
|------|-----------|
| `NOT_PARTY_LEADER` | Nur Leader kann starten |
| `READY_CHECK_ACTIVE` | Ein Ready-Check läuft bereits |

---

## 🔗 Verwandte Kategorien

- **Chat (04)**: Party-Chat → `ChatParty` (404)
- **Combat (03)**: Party-Combat → XP-Sharing
- **Loot (31)**: Party-Loot → `LootRequest` (3100)
- **Quest (10)**: Party-Quests → `QuestShare` (1010)

---

**Letzte Aktualisierung**: 2025-12-25  
**Version**: 2.1.0  
**Status**: ✅ Vollständig dokumentiert (22/22 Messages)

[← Zurück zur Übersicht](README.md)
