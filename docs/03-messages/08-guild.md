# 🏰 Guild Messages (800-899)

**Kategorie:** 08  
**Range:** 800-899  



[← Zurück zur Übersicht](README.md)

---

## 📋 Inhaltsverzeichnis

- [GuildCreate (800)](#guildcreate-800)
- [GuildInvite (801)](#guildinvite-801)
- [GuildInviteReceived (802)](#guildinvitereceived-802)
- [GuildJoin (803)](#guildjoin-803)
- [GuildLeave (804)](#guildleave-804)
- [GuildKick (805)](#guildkick-805)
- [GuildDisband (806)](#guilddisband-806)
- [GuildPromote (810)](#guildpromote-810)
- [GuildDemote (811)](#guilddemote-811)
- [GuildRankEdit (812)](#guildrankedit-812)
- [GuildMOTD (813)](#guildmotd-813)
- [GuildInfo (820)](#guildinfo-820)
- [GuildRoster (821)](#guildroster-821)
- [GuildMessage (822)](#guildmessage-822)
- [GuildBankDeposit (830)](#guildbankdeposit-830)
- [GuildBankWithdraw (831)](#guildbankwithdraw-831)
- [GuildBankLog (832)](#guildbanklog-832)
- [GuildCreateResponse (840)](#guildcreateresponse-840)
- [GuildInviteResponse (841)](#guildinviteresponse-841)
- [GuildLeaveResponse (842)](#guildleaveresponse-842)
- [GuildKickResponse (843)](#guildkickresponse-843)
- [GuildDisbandResponse (844)](#guilddisbandresponse-844)
- [GuildPromoteResponse (845)](#guildpromoteresponse-845)
- [GuildDemoteResponse (846)](#guilddemoteresponse-846)
- [GuildRankEditResponse (847)](#guildrankeditresponse-847)
- [GuildMOTDResponse (848)](#guildmotdresponse-848)
- [GuildMessageResponse (849)](#guildmessageresponse-849)
- [GuildBankDepositResponse (850)](#guildbankdepositresponse-850)
- [GuildBankWithdrawResponse (851)](#guildbankwithdrawresponse-851)

---

## 📋 Übersicht

Diese Kategorie umfasst alle Messages für das **Guild-System** im 2DMMO.

Das Guild-System implementiert:
- **Guild-Creation**: Charter-System mit Signatures (Hinweis: vereinfacht)
- **Rank-System**: 10 Ranks (0=GM, 1=Officer, 2-9=Members) mit Permissions
- **Guild-Chat**: Dedizierter Chat-Channel → siehe `ChatGuild` (405)
- **Guild-Bank**: Shared Storage mit Tabs und Permissions
- **MOTD**: Message-of-the-Day für Announcements
- **Guild-Roster**: Member-Liste mit Online-Status, Rank, Join-Date
- **Permissions**: Granulare Rechte pro Rank (Invite, Promote, Bank-Access, etc.)

**Server Authority**: Alle Guild-Changes sind server-authoritative.

**Guild-Size**: Max 100 Members (Standard), 500 (mit Perks später)

**Guild-Bank**: 8 Tabs, je 98 Slots

**Rank-System**:
- **Rank 0**: Guild Master (GM) - volle Kontrolle
- **Rank 1**: Officer - kann inviten, promoten (bis Rank 2), kicken
- **Rank 2-9**: Members - konfigurierbare Permissions

---

## GuildInvite (800)

**Richtung:** 📤 Client → Server  
**Frequenz:** Sehr selten (einmalig)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine (aber Kosten: 100 Gold)

### Beschreibung
Client erstellt neue Guild. Erfordert eindeutigen Guild-Namen und Tag. Creator wird automatisch Guild Master (Rank 0).

Im Prototyp wird die Guild sofort erstellt. In Phase 2 ist ein Charter-System geplant wo 9 weitere Spieler signieren müssen.

### Im Scope ✅
- Guild-Name (3-24 Zeichen, unique server-weit)
- Guild-Tag (2-4 Zeichen, unique, displayed in brackets [TAG])
- Auto-Promotion zu Guild Master
- Gold-Cost (100 Gold)
- Name-Validation (Profanity-Filter, Uniqueness)

### Nicht im Scope ❌
- Charter-System → geplant (9 Signatures erforderlich)
- Guild-Tabard Design → geplant
- Guild-Level → geplant

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| GuildName | string | Guild-Name (3-24 Zeichen) | Ja |
| Tag | string | Guild-Tag (2-4 Zeichen, uppercase) | Ja |

### Erwartete Response
- `GuildCreateResponse` (840)

### Folge-Messages bei Erfolg
- `GuildJoin` (803) Auto-Join des Creators
- `GoldUpdate` (3703) mit neuem Gold-Betrag

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `GuildJoin` | 803 | Auto-Join nach Creation |
| `GuildInfo` | 820 | Guild-Details |
| `GoldUpdate` | 3703 | Gold-Cost |

### Flow-Diagramm
```
Client                    Server                    Database
  │                          │                          │
  │  GuildCreate (800)       │                          │
  │  Name="Knights"          │                          │
  │  Tag="KNT"               │                          │
  │─────────────────────────►│                          │
  │                          │  ┌─ Validate: 100 Gold?
  │                          │  ├─ Validate: Name unique?
  │                          │  ├─ Validate: Tag unique?
  │                          │  ├─ Validate: Profanity?
  │                          │  ├─ Create Guild
  │                          │  └─ Set as GM (Rank 0)
  │                          │                          │
  │                          │  Insert Guild            │
  │                          │─────────────────────────►│
  │                          │                          │
  │  GuildCreateSuccess      │                          │
  │◄─────────────────────────│                          │
  │                          │                          │
  │  GuildJoin (803)         │                          │
  │  (Self as GM)            │                          │
  │◄─────────────────────────│                          │
  │                          │                          │
  │  GoldUpdate (3703)       │                          │
  │  (-100 Gold)             │                          │
  │◄─────────────────────────│                          │
```

### Beispiel Payload
```csharp
var guildCreate = new GuildCreate
{
    Type = MessageType.GuildCreate,
    GuildName = "Knights of Valor",
    Tag = "KNT"
};
```

### Error Codes
| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `GUILD_NAME_TAKEN` | Name bereits vergeben | Anderen Namen wählen |
| `GUILD_TAG_TAKEN` | Tag bereits vergeben | Anderen Tag wählen |
| `INVALID_GUILD_NAME` | Name verstößt gegen Regeln | Gültigen Namen wählen |
| `INVALID_GUILD_TAG` | Tag ungültig (Länge, Format) | 2-4 Zeichen, uppercase |
| `INSUFFICIENT_GOLD` | Nicht genug Gold (100) | Gold farmen |
| `ALREADY_IN_GUILD` | Bereits in Guild | Guild verlassen erst |
| `NAME_PROFANITY` | Name enthält Profanity | Anderen Namen wählen |

### Notizen
- **Cost**: 100 Gold (nicht rückerstattbar)
- **Name-Regeln**: 3-24 Zeichen, alphanumerisch + Leerzeichen, keine Sonderzeichen
- **Tag-Regeln**: 2-4 Zeichen, nur Großbuchstaben, keine Zahlen
- **Uniqueness**: Server-weit (nicht nur Realm)
- **Profanity-Filter**: Aktiv für Name und Tag
- **Default-Ranks**: 10 Ranks werden automatisch erstellt mit Standard-Permissions
- **Auto-MOTD**: "Welcome to [GuildName]!"

---

## GuildInviteResponse (801)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Permission: "Invite Members" (Rank 0-1 default)

### Beschreibung
Guild-Member mit Invite-Permission sendet Guild-Einladung an Spieler. Target erhält `GuildInviteReceived` (802).

### Im Scope ✅
- Guild-Invite an Online-Spieler
- Permission-Check (Rank muss Invite-Recht haben)
- Cross-Zone Invites
- Guild-Info im Invite (Name, Tag, Member-Count)

### Nicht im Scope ❌
- Offline-Invites → nicht möglich
- Mass-Invite → geplant

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| TargetName | string | Einzuladender Spieler-Name | Ja |

### Erwartete Response
- `GuildInviteResponse` (841)

### Folge-Messages bei Erfolg
- `GuildInviteReceived` (802) an Target-Spieler

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `GuildInviteReceived` | 802 | Target erhält Invite |
| `GuildJoin` | 803 | Nach Accept |

### Beispiel Payload
```csharp
var guildInvite = new GuildInvite
{
    Type = MessageType.GuildInvite,
    TargetName = "Legolas"
};
```

### Error Codes
| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `PLAYER_NOT_FOUND` | Target nicht online | Namen prüfen |
| `ALREADY_IN_GUILD` | Target ist bereits in Guild | - |
| `GUILD_FULL` | Guild ist voll (100/100) | Members kicken oder Perk kaufen |
| `NO_INVITE_PERMISSION` | Rank hat keine Invite-Permission | Permission vom GM anfordern |
| `NOT_IN_GUILD` | Inviter nicht in Guild | - |
| `PLAYER_DECLINED_INVITES` | Target hat Guild-Invites deaktiviert | - |
| `INVITE_ALREADY_PENDING` | Invite bereits gesendet | Auf Antwort warten |

### Notizen
- **Invite-Timeout**: 60 Sekunden
- **Permission**: Default für Rank 0-1, konfigurierbar für andere
- **Guild-Full**: Max 100 Members (Standard)
- **UI-Notification**: "You invited Legolas to join [KNT] Knights of Valor."

---

## GuildLeave (802)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Server informiert Spieler über eingehende Guild-Einladung. Client zeigt UI-Popup mit Guild-Info und Accept/Decline-Buttons.

### Im Scope ✅
- Invite-Notification mit Guild-Info
- Inviter-Info
- Timeout (60s)

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| InviterId | long | Inviter Character-ID | Ja |
| InviterName | string | Inviter-Name | Ja |
| GuildId | long | Guild-ID | Ja |
| GuildName | string | Guild-Name | Ja |
| GuildTag | string | Guild-Tag | Ja |
| MemberCount | int | Aktuelle Member-Anzahl | Ja |
| TimeoutSeconds | int | Sekunden bis Auto-Decline (60) | Ja |

### Erwartete Response
- Client sendet `GuildInviteAccept` oder `GuildInviteDecline`

### Beispiel Payload
```csharp
var inviteReceived = new GuildInviteReceived
{
    Type = MessageType.GuildInviteReceived,
    InviterId = 98765,
    InviterName = "Aragorn",
    GuildId = 12345,
    GuildName = "Knights of Valor",
    GuildTag = "KNT",
    MemberCount = 45,
    TimeoutSeconds = 60
};
```

### Notizen
- **UI-Popup**: "Aragorn has invited you to join [KNT] Knights of Valor (45/100 members)."
- **Sound**: Guild-Invite Sound
- **Timeout**: 60s → dann Auto-Decline

---

## GuildKick (803)

**Richtung:** 📡 Broadcast (Server → All Guild Members)  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Server broadcastet an alle Guild-Members dass neuer Spieler joined. Enthält Member-Info und Initial-Rank.

### Im Scope ✅
- Join-Notification an alle Guild-Members
- Neue Member-Info
- Initial-Rank (default: Rank 9 = niedrigster)

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| PlayerId | long | Neues Member | Ja |
| PlayerName | string | Name | Ja |
| PlayerLevel | int | Level | Ja |
| PlayerClass | byte | Class | Ja |
| Rank | byte | Initial-Rank (9) | Ja |
| JoinTimestamp | long | Unix Timestamp | Ja |

### Erwartete Response
- Keine Response erforderlich

### Beispiel Payload
```csharp
var guildJoin = new GuildJoin
{
    Type = MessageType.GuildJoin,
    PlayerId = 54321,
    PlayerName = "Legolas",
    PlayerLevel = 9,
    PlayerClass = 3, // Rogue
    Rank = 9, // Initiate (lowest rank)
    JoinTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
};
```

### Notizen
- **Broadcast**: An alle Online-Guild-Members
- **Initial-Rank**: Rank 9 (konfigurierbar per Guild-Setting)
- **Guild-Chat**: Auto-Join zu Guild-Chat
- **Chat-Notification**: "Legolas has joined the guild. Welcome!"
- **MOTD**: Neues Member erhält Guild-MOTD

---

## GuildUpdate (804)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Member verlässt Guild freiwillig. GM kann Guild nur via `GuildDisband` (806) verlassen.

### Im Scope ✅
- Voluntary Leave
- Broadcast an verbleibende Members
- GM-Restriction (GM kann nicht Leave, nur Disband)

### Request Payload
Keine zusätzlichen Felder

### Erwartete Response
- `GuildLeaveResponse` (842)

### Folge-Messages bei Erfolg
- `GuildLeaveNotification` Broadcast an verbleibende Members

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `GuildKick` | 805 | Forced Leave |
| `GuildDisband` | 806 | GM löst Guild auf |

### Beispiel Payload
```csharp
var guildLeave = new GuildLeave
{
    Type = MessageType.GuildLeave
};
```

### Error Codes
| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `NOT_IN_GUILD` | Nicht in Guild | - |
| `GUILD_MASTER_CANNOT_LEAVE` | GM kann nicht leave | Disband oder Promote anderen zu GM |

### Notizen
- **GM-Restriction**: GM muss erst anderen zu GM promoten oder Guild disband
- **Chat-Notification**: "Legolas has left the guild."
- **Guild-Chat**: Auto-Leave aus Chat
- **Bank-Items**: Können nicht mitgenommen werden

---

## GuildDisband (805)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Permission: "Remove Members" (Rank 0-1 default)

### Beschreibung
Member mit Remove-Permission kicked anderen Member. Kann nur Members mit niedrigerem Rank kicken.

### Im Scope ✅
- Forced Removal
- Rank-Check (kann nur niedrigere Ranks kicken)
- Kick-Reason (optional)

### Nicht im Scope ❌
- GM-Kick → GM kann nicht gekickt werden
- Self-Kick → verwende `GuildLeave` (804)

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| PlayerName | string | Zu kickender Spieler | Ja |
| Reason | string | Optional Kick-Grund | Nein |

### Erwartete Response
- `GuildKickResponse` (843)

### Folge-Messages bei Erfolg
- `GuildKickNotification` an alle Members (inkl. Kicked Player)

### Beispiel Payload
```csharp
var guildKick = new GuildKick
{
    Type = MessageType.GuildKick,
    PlayerName = "Troublemaker",
    Reason = "Violation of guild rules"
};
```

### Error Codes
| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `NO_REMOVE_PERMISSION` | Keine Remove-Permission | - |
| `CANNOT_KICK_HIGHER_RANK` | Target hat gleichen/höheren Rank | - |
| `CANNOT_KICK_GUILD_MASTER` | GM kann nicht gekickt werden | - |
| `PLAYER_NOT_IN_GUILD` | Spieler nicht in Guild | - |

### Notizen
- **Rank-Hierarchy**: Kann nur niedrigere Ranks kicken
- **Notification**: "You have been removed from [KNT] Knights of Valor. Reason: Violation of guild rules"
- **No Cooldown**: Kein Cooldown fürs Kicken

---

## GuildPromote (806)

**Richtung:** 📤 Client → Server  
**Frequenz:** Sehr selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Guild Master (Rank 0)

### Beschreibung
Guild Master löst Guild komplett auf. Alle Members werden entfernt, Guild-Bank-Items verschwinden (oder verteilt später).

### Im Scope ✅
- Vollständige Guild-Auflösung
- Nur GM darf disband
- Broadcast an alle Members
- Confirmation erforderlich

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Confirmation | string | Muss Guild-Name sein | Ja |

### Erwartete Response
- `GuildDisbandResponse` (844)

### Folge-Messages bei Erfolg
- `GuildDisbandNotification` an alle Members

### Beispiel Payload
```csharp
var guildDisband = new GuildDisband
{
    Type = MessageType.GuildDisband,
    Confirmation = "Knights of Valor" // Muss exakter Guild-Name sein
};
```

### Error Codes
| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `NOT_GUILD_MASTER` | Nur GM darf disband | - |
| `INVALID_CONFIRMATION` | Confirmation falsch | Exakten Guild-Namen eingeben |

### Notizen
- **GM-Only**: Nur Guild Master
- **Confirmation**: Muss exakten Guild-Namen eintippen
- **Bank-Items**: Verschwinden (Hinweis: per Mail an Members verteilt)
- **Chat-Notification**: "The guild has been disbanded by the Guild Master."
- **No Refund**: Keine Rückerstattung der 100 Gold Creation-Cost

---

## GuildRosterRequest (810)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Permission: "Promote Members"

### Beschreibung
Member mit Promote-Permission erhöht Rank eines Members. Kann nur zu niedrigerem Rank als eigener Rank promoten.

### Im Scope ✅
- Rank-Increase
- Rank-Check (kann nur zu Ranks promoten die niedriger als eigener)
- GM-Transfer (Rank 0 → 0 ist special case)

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| PlayerName | string | Zu promotender Spieler | Ja |
| NewRank | byte | Neuer Rank (0-9) | Ja |

### Erwartete Response
- `GuildPromoteResponse` (845)

### Folge-Messages bei Erfolg
- `GuildPromoteNotification` an alle Members

### Beispiel Payload
```csharp
// Officer promoted zu GM (GM-Transfer)
var promoteToGM = new GuildPromote
{
    Type = MessageType.GuildPromote,
    PlayerName = "TrustedOfficer",
    NewRank = 0 // GM
};

// Member promoted zu Officer
var promoteToOfficer = new GuildPromote
{
    Type = MessageType.GuildPromote,
    PlayerName = "ReliableMember",
    NewRank = 1 // Officer
};
```

### Error Codes
| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `NO_PROMOTE_PERMISSION` | Keine Promote-Permission | - |
| `CANNOT_PROMOTE_TO_HIGHER_RANK` | Ziel-Rank höher als eigener | - |
| `INVALID_RANK` | Rank 0-9 ungültig | Validen Rank wählen |
| `PLAYER_NOT_IN_GUILD` | Spieler nicht in Guild | - |
| `ALREADY_THAT_RANK` | Spieler hat bereits diesen Rank | - |

### Notizen
- **GM-Transfer**: Wenn zu Rank 0 promoted → automatisch Demote von aktuellem GM zu Rank 1
- **Permissions**: Defaultmäßig nur Rank 0 (GM)
- **Chat-Notification**: "TrustedOfficer has been promoted to Guild Master."

---

## GuildRosterResponse (811)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Permission: "Demote Members"

### Beschreibung
Member mit Demote-Permission verringert Rank eines Members. Kann nur Members mit niedrigerem Rank demoten.

### Im Scope ✅
- Rank-Decrease
- Rank-Check (kann nur niedrigere Ranks demoten)

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| PlayerName | string | Zu demotender Spieler | Ja |
| NewRank | byte | Neuer Rank (0-9) | Ja |

### Erwartete Response
- `GuildDemoteResponse` (846)

### Folge-Messages bei Erfolg
- `GuildDemoteNotification` an alle Members

### Beispiel Payload
```csharp
var guildDemote = new GuildDemote
{
    Type = MessageType.GuildDemote,
    PlayerName = "ProblematicOfficer",
    NewRank = 5 // Demoted von Rank 1 zu Rank 5
};
```

### Error Codes
| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `NO_DEMOTE_PERMISSION` | Keine Demote-Permission | - |
| `CANNOT_DEMOTE_HIGHER_RANK` | Target hat gleichen/höheren Rank | - |
| `CANNOT_DEMOTE_GUILD_MASTER` | GM kann nicht demoted werden | - |
| `INVALID_RANK` | Rank ungültig | Validen Rank wählen |

### Notizen
- **Permissions**: Defaultmäßig nur Rank 0 (GM)
- **Chat-Notification**: "ProblematicOfficer has been demoted to Member."

---

## GuildRankCreate (812)

**Richtung:** 📤 Client → Server  
**Frequenz:** Sehr selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Guild Master (Rank 0)

### Beschreibung
GM editiert Rank-Konfiguration (Name, Permissions). Permissions bestimmen was Member mit diesem Rank dürfen.

### Im Scope ✅
- Rank-Name ändern
- Permissions setzen
- Nur GM darf editieren

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| RankId | byte | Rank (0-9) | Ja |
| RankName | string | Neuer Rank-Name | Nein |
| Permissions | uint | Bitfield von Permissions | Nein |

**Permissions (Bitfield)**:
- `0x01`: Invite Members
- `0x02`: Remove Members
- `0x04`: Promote Members
- `0x08`: Demote Members
- `0x10`: Edit Guild Info (MOTD)
- `0x20`: Bank Tab 1 Access
- `0x40`: Bank Tab 2 Access
- `0x80`: Bank Tab 3 Access
- ... etc.

### Erwartete Response
- `GuildRankEditResponse` (847)

### Folge-Messages bei Erfolg
- Broadcast an alle Members mit neuen Rank-Permissions

### Beispiel Payload
```csharp
var rankEdit = new GuildRankEdit
{
    Type = MessageType.GuildRankEdit,
    RankId = 2,
    RankName = "Veteran",
    Permissions = 0x01 | 0x20 // Invite + Bank Tab 1
};
```

### Error Codes
| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `NOT_GUILD_MASTER` | Nur GM darf editieren | - |
| `INVALID_RANK` | Rank ungültig | - |
| `RANK_NAME_TOO_LONG` | Name >20 Zeichen | Kürzen |

### Notizen
- **GM-Only**: Nur Guild Master
- **Default-Names**: "Guild Master", "Officer", "Veteran", "Member", "Initiate", etc.
- **Max-Name-Length**: 20 Zeichen

---

## GuildRankDelete (813)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Permission: "Edit Guild Info"

### Beschreibung
Member mit Edit-Guild-Info-Permission setzt Message-of-the-Day. Wird allen Members beim Login angezeigt.

### Im Scope ✅
- MOTD setzen
- Broadcast an alle Online-Members
- Max 256 Zeichen

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| MOTD | string | Message-of-the-Day (max 256) | Ja |

### Erwartete Response
- `GuildMOTDResponse` (848)

### Folge-Messages bei Erfolg
- `GuildMOTDUpdate` Broadcast an alle Members

### Beispiel Payload
```csharp
var setMOTD = new GuildMOTD
{
    Type = MessageType.GuildMOTD,
    MOTD = "Guild raid tonight at 20:00 server time! All welcome!"
};
```

### Error Codes
| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `NO_EDIT_GUILD_INFO_PERMISSION` | Keine Permission | - |
| `MOTD_TOO_LONG` | >256 Zeichen | Kürzen |
| `PROFANITY_DETECTED` | Profanity-Filter | Anpassen |

### Notizen
- **Permissions**: Default Rank 0-1
- **Display**: Bei Login + via `/guild motd` Command
- **Profanity-Filter**: Aktiv
- **Max-Length**: 256 Zeichen

---

## GuildBankDeposit (820)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten (Login, Guild-Join)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Vollständige Guild-Informationen nach Login oder Guild-Join.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| GuildId | long | Guild-ID | Ja |
| GuildName | string | Guild-Name | Ja |
| GuildTag | string | Guild-Tag | Ja |
| MemberCount | int | Anzahl Members | Ja |
| CreatedAt | long | Creation-Timestamp | Ja |
| MOTD | string | Message-of-the-Day | Ja |
| YourRank | byte | Eigener Rank | Ja |
| Ranks | List<RankInfo> | Alle Ranks | Ja |

**RankInfo:**
| Feld | Typ | Beschreibung |
|------|-----|--------------|
| RankId | byte | Rank (0-9) |
| RankName | string | Rank-Name |
| Permissions | uint | Permissions-Bitfield |

### Beispiel Payload
```csharp
var guildInfo = new GuildInfo
{
    Type = MessageType.GuildInfo,
    GuildId = 12345,
    GuildName = "Knights of Valor",
    GuildTag = "KNT",
    MemberCount = 45,
    CreatedAt = DateTimeOffset.UtcNow.AddMonths(-6).ToUnixTimeSeconds(),
    MOTD = "Guild raid tonight at 20:00!",
    YourRank = 5,
    Ranks = new List<RankInfo>
    {
        new RankInfo { RankId = 0, RankName = "Guild Master", Permissions = 0xFFFFFFFF },
        new RankInfo { RankId = 1, RankName = "Officer", Permissions = 0x1F },
        // ... etc
    }
};
```

### Notizen
- **Frequency**: Nur bei Login/Join, dann cached
- **Updates**: Separate Messages für MOTD-Change, Rank-Edit, etc.

---

## GuildBankWithdraw (821)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten (Request)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Liste aller Guild-Members mit Details (Level, Class, Rank, Online-Status, Last-Online).

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Members | List<GuildMemberInfo> | Alle Members | Ja |

**GuildMemberInfo:**
| Feld | Typ | Beschreibung |
|------|-----|--------------|
| CharacterId | long | Character-ID |
| Name | string | Character-Name |
| Level | int | Level |
| Class | byte | Class |
| Rank | byte | Guild-Rank |
| IsOnline | bool | Online? |
| LastOnline | long | Last-Online Timestamp |
| JoinedAt | long | Join-Timestamp |
| Note | string | Public-Note (geplant) |

### Beispiel Payload
```csharp
var roster = new GuildRoster
{
    Type = MessageType.GuildRoster,
    Members = new List<GuildMemberInfo>
    {
        new GuildMemberInfo
        {
            CharacterId = 98765,
            Name = "Aragorn",
            Level = 60,
            Class = 1, // Warrior
            Rank = 0, // GM
            IsOnline = true,
            LastOnline = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            JoinedAt = DateTimeOffset.UtcNow.AddMonths(-6).ToUnixTimeSeconds()
        },
        // ... more members
    }
};
```

### Notizen
- **Request**: Via `/guild roster` Command oder UI
- **Sorting**: Client-side (nach Rank, Name, Level, Online-Status)
- **Max-Size**: 100 Members (Standard), kann groß sein

---

## GuildBankLog (822)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Permission: "Use Guild Chat" (alle Ranks default)

### Beschreibung
Guild-Chat-Message. Alternative zu `ChatGuild` (405). Server broadcastet an alle Online-Guild-Members.

**Note**: Identisch zu `ChatGuild` (405), kann deprecated werden.

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Message | string | Text-Message (max 500) | Ja |

### Erwartete Response
- `GuildMessageResponse` (849)

### Folge-Messages bei Erfolg
- Broadcast an alle Online-Guild-Members

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `ChatGuild` | 405 | Alternative (empfohlen) |

### Notizen
- **Empfehlung**: Verwende `ChatGuild` (405) stattdessen
- **Rate-Limit**: 30 Messages/Minute

---

## GuildEventDelete (830)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Permission: Bank-Tab-Access

### Beschreibung
Member deposited Item in Guild-Bank. Erfordert Permission für spezifischen Bank-Tab.

### Im Scope ✅
- Item von Inventory zu Bank
- Tab-Selection
- Permission-Check pro Tab
- Daily-Limit (geplant)

### Nicht im Scope ❌
- Gold-Deposit → geplant

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| InventorySlot | byte | Item-Slot in Inventory | Ja |
| BagId | byte | Bag-ID (-1=Backpack) | Ja |
| BankTab | byte | Bank-Tab (0-7) | Ja |
| BankSlot | byte | Slot in Bank (0-97) | Ja |
| Quantity | int | Anzahl (für Stacks) | Ja |

### Erwartete Response
- `GuildBankDepositResponse` (850)

### Folge-Messages bei Erfolg
- `ItemRemove` (502) aus Player-Inventory
- `GuildBankUpdate` mit aktualisierten Bank-Daten

### Beispiel Payload
```csharp
var bankDeposit = new GuildBankDeposit
{
    Type = MessageType.GuildBankDeposit,
    InventorySlot = 5,
    BagId = -1, // Backpack
    BankTab = 0, // Tab 1
    BankSlot = 10,
    Quantity = 20 // 20 Items
};
```

### Error Codes
| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `NO_BANK_ACCESS` | Keine Permission für diesen Tab | - |
| `BANK_SLOT_OCCUPIED` | Slot bereits belegt | Anderen Slot wählen |
| `INVALID_BANK_TAB` | Tab 0-7 ungültig | - |
| `ITEM_NOT_IN_INVENTORY` | Item nicht gefunden | - |
| `DAILY_LIMIT_REACHED` | Daily-Deposit-Limit erreicht | Morgen wiederkommen |

### Notizen
- **Bank-Tabs**: 8 Tabs, je 98 Slots (7×14 Grid)
- **Permissions**: Pro Tab konfigurierbar
- **Daily-Limit**: Phase 2 (z.B. 50 Items/Tag)
- **Bank-Log**: Alle Deposits werden geloggt
- **Soulbound**: Soulbound-Items können nicht deposited werden

---

## GuildEventSignup (831)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Permission: Bank-Tab-Withdraw

### Beschreibung
Member withdraws Item aus Guild-Bank. Erfordert Withdraw-Permission für Tab.

### Im Scope ✅
- Item von Bank zu Inventory
- Tab-Selection
- Permission-Check
- Daily-Limit

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| BankTab | byte | Bank-Tab (0-7) | Ja |
| BankSlot | byte | Slot in Bank (0-97) | Ja |
| Quantity | int | Anzahl | Ja |

### Erwartete Response
- `GuildBankWithdrawResponse` (851)

### Folge-Messages bei Erfolg
- `ItemAdd` (501) zu Player-Inventory
- `GuildBankUpdate` mit aktualisierten Bank-Daten

### Beispiel Payload
```csharp
var bankWithdraw = new GuildBankWithdraw
{
    Type = MessageType.GuildBankWithdraw,
    BankTab = 0,
    BankSlot = 10,
    Quantity = 5
};
```

### Error Codes
| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `NO_WITHDRAW_PERMISSION` | Keine Permission | - |
| `INVENTORY_FULL` | Inventory voll | Platz machen |
| `DAILY_LIMIT_REACHED` | Daily-Withdraw-Limit erreicht | - |
| `ITEM_NOT_IN_BANK` | Item nicht gefunden | - |

### Notizen
- **Permissions**: Separate Withdraw-Permission pro Tab
- **Daily-Limit**: Konfigurierbar per Rank (z.B. Rank 9: 10/Tag, Rank 1: unlimited)
- **Bank-Log**: Alle Withdrawals werden geloggt

---

## GuildSearch (832)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten (Request)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Permission: "View Bank Log" (Rank 0-1 default)

### Beschreibung
Audit-Log für Guild-Bank. Zeigt Deposits, Withdrawals, und Bank-Item-Movements.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Entries | List<BankLogEntry> | Log-Einträge (letzte 100) | Ja |

**BankLogEntry:**
| Feld | Typ | Beschreibung |
|------|-----|--------------|
| Timestamp | long | Unix Timestamp |
| CharacterName | string | Wer |
| Action | string | "deposit", "withdraw", "move" |
| ItemId | uint | Item-ID |
| Quantity | int | Anzahl |
| TabFrom | byte | Von Tab |
| TabTo | byte | Zu Tab |

### Beispiel Payload
```csharp
var bankLog = new GuildBankLog
{
    Type = MessageType.GuildBankLog,
    Entries = new List<BankLogEntry>
    {
        new BankLogEntry
        {
            Timestamp = DateTimeOffset.UtcNow.AddHours(-2).ToUnixTimeSeconds(),
            CharacterName = "Aragorn",
            Action = "deposit",
            ItemId = 2001, // Health Potion
            Quantity = 20,
            TabFrom = 0, // Not used for deposit
            TabTo = 0 // Tab 1
        },
        new BankLogEntry
        {
            Timestamp = DateTimeOffset.UtcNow.AddHours(-1).ToUnixTimeSeconds(),
            CharacterName = "Legolas",
            Action = "withdraw",
            ItemId = 2001,
            Quantity = 5,
            TabFrom = 0,
            TabTo = 0
        }
    }
};
```

### Notizen
- **Permissions**: Default Rank 0-1 (Officers+)
- **Retention**: Letzte 100 Einträge oder 7 Tage
- **Purpose**: Audit-Trail für Bank-Activity

---

## GuildCreateResponse (840)

**Richtung:** 📥 Server → Client  
**Frequenz:** Sehr selten  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung
Antwort auf GuildCreate Request. Bestätigt erfolgreiche Guild-Erstellung oder gibt Fehler zurück.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Success | bool | Erstellung erfolgreich? | Ja |
| ErrorCode | string | Fehlercode falls Success=false | Nein |
| ErrorMessage | string | Menschenlesbare Fehlermeldung | Nein |
| GuildId | long | ID der neuen Guild | Bei Erfolg |
| GuildName | string | Guild-Name | Bei Erfolg |
| GoldCost | int | Kosten in Gold | Bei Erfolg |

### Error Codes
| Code | Bedeutung |
|------|-----------|
| `NAME_TAKEN` | Guild-Name bereits vergeben |
| `TAG_TAKEN` | Guild-Tag bereits vergeben |
| `INSUFFICIENT_GOLD` | Nicht genug Gold (100 Gold benötigt) |
| `ALREADY_IN_GUILD` | Spieler ist bereits in Guild |
| `INVALID_NAME` | Name verstößt gegen Regeln |
| `INVALID_TAG` | Tag nicht 2-4 Zeichen oder ungültig |

---

## GuildLeaveResponse (841)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung
Antwort auf GuildInvite Request. Bestätigt erfolgreiche Einladungs-Versendung oder gibt Fehler zurück.

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
| `ALREADY_IN_GUILD` | Target ist bereits in Guild |
| `GUILD_FULL` | Guild ist voll (100/100) |
| `NO_PERMISSION` | Keine Invite-Permission |
| `PLAYER_BLOCKED_YOU` | Target hat Inviter blockiert |
| `INVITE_ALREADY_PENDING` | Invite bereits gesendet |

---

## GuildKickResponse (842)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung
Antwort auf GuildLeave Request. Bestätigt erfolgreichen Guild-Austritt oder gibt Fehler zurück.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Success | bool | Leave erfolgreich? | Ja |
| ErrorCode | string | Fehlercode falls Success=false | Nein |
| ErrorMessage | string | Menschenlesbare Fehlermeldung | Nein |

### Error Codes
| Code | Bedeutung |
|------|-----------|
| `IS_GUILD_MASTER` | GM muss Guild erst übergeben oder disband |

---

## GuildDisbandResponse (843)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung
Antwort auf GuildKick Request. Bestätigt erfolgreichen Kick oder gibt Fehler zurück.

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
| `NO_PERMISSION` | Keine Kick-Permission |
| `PLAYER_NOT_IN_GUILD` | Target nicht in Guild |
| `CANNOT_KICK_GM` | Guild Master kann nicht gekickt werden |
| `INSUFFICIENT_RANK` | Kann höhere Ranks nicht kicken |

---

## GuildPromoteResponse (844)

**Richtung:** 📥 Server → Client  
**Frequenz:** Sehr selten  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung
Antwort auf GuildDisband Request. Bestätigt erfolgreiche Guild-Auflösung oder gibt Fehler zurück.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Success | bool | Disband erfolgreich? | Ja |
| ErrorCode | string | Fehlercode falls Success=false | Nein |
| ErrorMessage | string | Menschenlesbare Fehlermeldung | Nein |

### Error Codes
| Code | Bedeutung |
|------|-----------|
| `NOT_GUILD_MASTER` | Nur GM darf disband |

---

## GuildDemoteResponse (845)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung
Antwort auf GuildPromote Request. Bestätigt erfolgreiche Beförderung oder gibt Fehler zurück.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Success | bool | Promote erfolgreich? | Ja |
| ErrorCode | string | Fehlercode falls Success=false | Nein |
| ErrorMessage | string | Menschenlesbare Fehlermeldung | Nein |
| PlayerName | string | Beförderter Spieler | Bei Erfolg |
| NewRank | int | Neuer Rank | Bei Erfolg |

### Error Codes
| Code | Bedeutung |
|------|-----------|
| `NO_PERMISSION` | Keine Promote-Permission |
| `PLAYER_NOT_IN_GUILD` | Target nicht in Guild |
| `ALREADY_MAX_RANK` | Target bereits Rank 0 |
| `INSUFFICIENT_RANK` | Kann nicht über eigenen Rank promoten |

---

## GuildRankEditResponse (846)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung
Antwort auf GuildDemote Request. Bestätigt erfolgreiche Degradierung oder gibt Fehler zurück.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Success | bool | Demote erfolgreich? | Ja |
| ErrorCode | string | Fehlercode falls Success=false | Nein |
| ErrorMessage | string | Menschenlesbare Fehlermeldung | Nein |
| PlayerName | string | Degradierter Spieler | Bei Erfolg |
| NewRank | int | Neuer Rank | Bei Erfolg |

### Error Codes
| Code | Bedeutung |
|------|-----------|
| `NO_PERMISSION` | Keine Demote-Permission |
| `PLAYER_NOT_IN_GUILD` | Target nicht in Guild |
| `ALREADY_MIN_RANK` | Target bereits Rank 9 |
| `CANNOT_DEMOTE_GM` | Guild Master kann nicht demoted werden |

---

## GuildMOTDResponse (847)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung
Antwort auf GuildRankEdit Request. Bestätigt erfolgreiche Rank-Änderung oder gibt Fehler zurück.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Success | bool | Änderung erfolgreich? | Ja |
| ErrorCode | string | Fehlercode falls Success=false | Nein |
| ErrorMessage | string | Menschenlesbare Fehlermeldung | Nein |
| RankId | int | Geänderter Rank | Bei Erfolg |

### Error Codes
| Code | Bedeutung |
|------|-----------|
| `NO_PERMISSION` | Nur GM darf Ranks editieren |
| `INVALID_RANK` | Rank 0 (GM) kann nicht editiert werden |

---

## GuildMessageResponse (848)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung
Antwort auf GuildMOTD Request. Bestätigt erfolgreiche MOTD-Änderung oder gibt Fehler zurück.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Success | bool | Änderung erfolgreich? | Ja |
| ErrorCode | string | Fehlercode falls Success=false | Nein |
| ErrorMessage | string | Menschenlesbare Fehlermeldung | Nein |

### Error Codes
| Code | Bedeutung |
|------|-----------|
| `NO_PERMISSION` | Keine MOTD-Permission |
| `MESSAGE_TOO_LONG` | MOTD >500 Zeichen |

---

## GuildBankDepositResponse (849)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung
Antwort auf GuildMessage Request. Bestätigt erfolgreiche Message-Übermittlung oder gibt Fehler zurück.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Success | bool | Message übermittelt? | Ja |
| ErrorCode | string | Fehlercode falls Success=false | Nein |
| ErrorMessage | string | Menschenlesbare Fehlermeldung | Nein |

### Error Codes
| Code | Bedeutung |
|------|-----------|
| `NOT_IN_GUILD` | Nicht in Guild |
| `RATE_LIMITED` | Zu viele Messages |

---

## GuildBankWithdrawResponse (850)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung
Antwort auf GuildBankDeposit Request. Bestätigt erfolgreiche Einzahlung oder gibt Fehler zurück.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Success | bool | Deposit erfolgreich? | Ja |
| ErrorCode | string | Fehlercode falls Success=false | Nein |
| ErrorMessage | string | Menschenlesbare Fehlermeldung | Nein |
| TabIndex | int | Bank-Tab | Bei Erfolg |
| SlotIndex | int | Slot im Tab | Bei Erfolg |

### Error Codes
| Code | Bedeutung |
|------|-----------|
| `NO_PERMISSION` | Keine Bank-Deposit-Permission für Tab |
| `TAB_FULL` | Bank-Tab ist voll |
| `ITEM_NOT_FOUND` | Item nicht in Inventory |
| `ITEM_SOULBOUND` | Soulbound-Items können nicht deposited werden |

---

## GuildBankWithdrawResponse (851)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung
Antwort auf GuildBankWithdraw Request. Bestätigt erfolgreiche Abhebung oder gibt Fehler zurück.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Success | bool | Withdraw erfolgreich? | Ja |
| ErrorCode | string | Fehlercode falls Success=false | Nein |
| ErrorMessage | string | Menschenlesbare Fehlermeldung | Nein |
| TabIndex | int | Bank-Tab | Bei Erfolg |
| SlotIndex | int | Slot im Tab | Bei Erfolg |

### Error Codes
| Code | Bedeutung |
|------|-----------|
| `NO_PERMISSION` | Keine Bank-Withdraw-Permission für Tab |
| `INVENTORY_FULL` | Player-Inventory ist voll |
| `ITEM_NOT_FOUND` | Item nicht in Bank |

---

## 🔗 Verwandte Kategorien

- **Chat (04)**: Guild-Chat → `ChatGuild` (405)
- **Inventory (05)**: Bank-Items → `ItemAdd` (501), `ItemRemove` (502)
- **Character (06)**: Guild-Member-Info → `CharacterInfo` (600)
- **Social (21)**: Friends-System → `FriendAdd` (2100)

---

**Letzte Aktualisierung**: 2025-12-25  
**Version**: 2.1.0  
**Status**: ✅ Vollständig dokumentiert (29/29 Messages)

[← Zurück zur Übersicht](README.md)
