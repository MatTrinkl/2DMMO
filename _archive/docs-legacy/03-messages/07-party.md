# 👥 Party Messages (0700-0746)

**Kategorie:** 7  
**Range:** 0700-0746 (AKTIV)  
**Status:** 🟢 In Entwicklung  
**Version:** 3.0.0

[← Zurück zur Übersicht](README.md)

---

## 📋 Inhaltsverzeichnis

- [🔄 Party Flow](#-party-flow)
- [🧱 DTOs / Enums / Interfaces](#-dtos--enums--interfaces)
- [📩 Aktive Messages (0700-0746)](#-aktive-messages-0700-0746)
  - [PartyInvite (700)](#partyinvite-700)
  - [PartyInviteResponse (701)](#partyinviteresponse-701)
  - [PartyLeave (702)](#partyleave-702)
  - [PartyKick (703)](#partykick-703)
  - [PartyUpdate (704)](#partyupdate-704)
  - [PartyDisband (705)](#partydisband-705)
  - [PartyLeaderChange (706)](#partyleaderchange-706)
  - [PartyLootChange (707)](#partylootchange-707)
  - [PartyReadyCheck (708)](#partyreadycheck-708)
  - [PartyReadyResponse (709)](#partyreadyresponse-709)
  - [PartyMemberUpdate (710)](#partymemberupdate-710)
  - [PartyPositionUpdate (711)](#partypositionupdate-711)
  - [PartyHealthUpdate (712)](#partyhealthupdate-712)
  - [PartyResourceUpdate (713)](#partyresourceupdate-713)
  - [PartyBuffUpdate (714)](#partybuffupdate-714)
  - [PartyTargetUpdate (715)](#partytargetupdate-715)
  - [PartyRoleSet (716)](#partyroleset-716)
  - [PartyRoleCheck (717)](#partyrolecheck-717)
  - [PartyConvertToRaid (718)](#partyconverttoraid-718)
  - [PartySync (719)](#partysync-719)
  - [PartySummon (720)](#partysummon-720)
  - [PartySummonResponse (721)](#partysummonresponse-721)
  - [PartyMarkerSet (722)](#partymarkerset-722)
  - [PartyMarkerClear (723)](#partymarkerclear-723)
  - [PartyDifficultyVote (724)](#partydifficultyvote-724)
  - [PartyDifficultySet (725)](#partydifficultyset-725)
  - [PartyAcceptResponse (740)](#partyacceptresponse-740)
  - [PartyLeaveResponse (741)](#partyleaveresponse-741)
  - [PartyKickResponse (742)](#partykickresponse-742)
  - [PartyPromoteResponse (743)](#partypromoteresponse-743)
  - [PartyDisbandResponse (744)](#partydisbandresponse-744)
  - [PartyLootModeResponse (745)](#partylootmoderesponse-745)
  - [PartyReadyCheckStartResponse (746)](#partyreadycheckstartresponse-746)
- [🗑️ Obsolete Messages](#️-obsolete-messages)
- [📎 Anhang](#-anhang)

---

## 🔄 Party Flow

### Server-Authoritative Architektur

```
┌─────────────────────────────────────────────────────────────────┐
│                     PARTY-ARCHITEKTUR                          │
├─────────────────────────────────────────────────────────────────┤
│  CLIENT A          SERVER              CLIENT B                │
│     │                 │                     │                  │
│     │  PartyInvite    │                     │                  │
│     │  (700)          │                     │                  │
│     │────────────────►│                     │                  │
│     │                 │  ┌──────────────────┤                  │
│     │                 │  │ Validate:        │                  │
│     │                 │  │ - Target online? │                  │
│     │                 │  │ - Not in party?  │                  │
│     │                 │  │ - Not blocked?   │                  │
│     │                 │  │ - Party not full?│                  │
│     │                 │  └──────────────────┤                  │
│     │                 │                     │                  │
│     │ PartyInvite     │ PartyInviteResponse │                  │
│     │ Response(701)   │ (701)               │                  │
│     │◄────────────────│────────────────────►│                  │
│     │                 │                     │                  │
│     │                 │    PartyLeave(702)  │                  │
│     │                 │◄────────────────────│ (Accept)         │
│     │                 │                     │                  │
│     │ PartyUpdate     │ PartyUpdate(704)    │                  │
│     │ (704)           │                     │                  │
│     │◄────────────────│────────────────────►│                  │
│     │                 │                     │                  │
└─────────────────────────────────────────────────────────────────┘
```

### Ready-Check Flow

```
Leader                    Server              All Members
  │                          │                     │
  │  PartyReadyCheck (708)   │                     │
  │─────────────────────────►│                     │
  │                          │  PartyReadyCheck    │
  │                          │  Broadcast          │
  │                          │────────────────────►│
  │                          │                     │
  │                          │  PartyReadyResponse │
  │                          │  (709) from each    │
  │                          │◄────────────────────│
  │                          │                     │
  │  PartyReadyCheckStart    │                     │
  │  Response (746)          │                     │
  │◄─────────────────────────│                     │
```

---

## 🧱 DTOs / Enums / Interfaces

### PartyMemberDto

```csharp
[MessagePackObject]
public class PartyMemberDto
{
    [Key(0)] public long CharacterId { get; set; }
    [Key(1)] public string Name { get; set; }
    [Key(2)] public byte Level { get; set; }
    [Key(3)] public byte ClassId { get; set; }
    [Key(4)] public int CurrentHp { get; set; }
    [Key(5)] public int MaxHp { get; set; }
    [Key(6)] public int CurrentMana { get; set; }
    [Key(7)] public int MaxMana { get; set; }
    [Key(8)] public float X { get; set; }
    [Key(9)] public float Y { get; set; }
    [Key(10)] public uint ZoneId { get; set; }
    [Key(11)] public bool IsOnline { get; set; }
    [Key(12)] public bool IsLeader { get; set; }
    [Key(13)] public PartyRole Role { get; set; }
}
```

### PartyRole Enum

```csharp
public enum PartyRole : byte
{
    None = 0,
    Tank = 1,
    Healer = 2,
    DamageDealer = 3
}
```

### LootMode Enum

```csharp
public enum LootMode : byte
{
    FreeForAll = 0,
    RoundRobin = 1,
    MasterLooter = 2,
    GroupLoot = 3,
    NeedBeforeGreed = 4
}
```

### PartyMarkerType Enum

```csharp
public enum PartyMarkerType : byte
{
    Skull = 1,
    Cross = 2,
    Square = 3,
    Moon = 4,
    Triangle = 5,
    Diamond = 6,
    Circle = 7,
    Star = 8
}
```

---

## 📩 Aktive Messages (0700-0746)

---

## 📋 Übersicht

Diese Kategorie umfasst alle Messages für das **Party-System** im 2DMMO.

Das Party-System implementiert:
- **Group-Formation**: Invites, Accept, Decline, Auto-Join
- **Party-Management**: Leader-Promotion, Kick, Disband
- **Member-Tracking**: HP/Mana, Position, Status (Online/Offline/Dead)
- **Loot-System**: Group-Loot, Round-Robin, Master-Looter, Need-Before-Greed
- **XP-Sharing**: XP-Bonus für Gruppe (10-20% je nach Größe)
- **Ready-Check**: Für Dungeon/Boss-Pulls
- **Party-Chat**: Dedizierter Chat-Channel → siehe `ChatParty` (404)

**Server Authority**: Alle Party-Changes sind server-authoritative.

**Party-Size**: Max 5 Spieler (Raid = 40 Spieler)

**XP-Range**: Max 100m zwischen Party-Members für XP-Share

**Level-Range**: Max 10 Level-Differenz für XP-Share (flexible basierend auf höchstem Level)

---

## PartyInvite (700)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine (oder Party-Leader falls Party bereits existiert)

### Beschreibung
Client sendet Party-Einladung an anderen Spieler. Falls Client noch keine Party hat, wird automatisch eine neue Party erstellt mit ihm als Leader.

### Im Scope ✅
- Party-Einladung an Online-Spieler
- Auto-Party-Creation (falls Client noch keine Party)
- Invite-Queue (mehrere Invites möglich)
- Cross-Zone Invites

### Nicht im Scope ❌
- Raid-Invites → verwende `PartyConvertToRaid` (718)
- Offline-Invites → nicht möglich

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| TargetName | string | Einzuladender Spieler-Name | Ja |

### Erwartete Response
- `PartyInviteResponse` (701)

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `PartyInviteResponse` | 701 | Response |
| `PartyLeave` | 702 | Nach Accept |
| `PartyUpdate` | 704 | Roster-Update |

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
| `INVITE_ALREADY_PENDING` | Invite bereits gesendet | Auf Antwort warten |
| `NOT_PARTY_LEADER` | Nur Leader darf inviten | - |

### Notizen
- **Auto-Party-Creation**: Falls Inviter keine Party hat, wird eine erstellt
- **Invite-Timeout**: 60 Sekunden für Accept/Decline
- **Cross-Zone**: Invites funktionieren Zone-übergreifend

---

## PartyInviteResponse (701)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Server bestätigt das Ergebnis eines PartyInvite Requests. Enthält Success-Status und bei Fehlern den ErrorCode. Bei Erfolg wird parallel dem Target ein Invite-Event gesendet.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Success | bool | Invite erfolgreich versendet? | Ja |
| ErrorCode | string | Fehlercode falls Success=false | Nein |
| ErrorMessage | string | Menschenlesbare Fehlermeldung | Nein |
| TargetName | string | Eingeladener Spieler | Bei Erfolg |

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `PartyInvite` | 700 | Request |
| `PartyUpdate` | 704 | Bei Accept |

### Beispiel Payload
```csharp
var response = new PartyInviteResponse
{
    Type = MessageType.PartyInviteResponse,
    Success = true,
    TargetName = "Legolas"
};
```

### Error Codes
| Code | Bedeutung |
|------|-----------|
| `PLAYER_NOT_FOUND` | Target nicht online |
| `ALREADY_IN_PARTY` | Target ist bereits in Party |
| `PARTY_FULL` | Party ist voll (5/5) |
| `NOT_PARTY_LEADER` | Nur Leader darf inviten |

---

## PartyLeave (702)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Client verlässt Party freiwillig. Server broadcastet Leave-Event an verbleibende Members. Falls Leader leaved, wird automatisch neuer Leader gewählt.

### Request Payload
Keine zusätzlichen Felder (nur MessageType)

### Erwartete Response
- `PartyLeaveResponse` (741)

### Folge-Messages bei Erfolg
- `PartyUpdate` (704) Broadcast an verbleibende Members
- `PartyLeaderChange` (706) falls Leader leaved

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

---

## PartyKick (703)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Party-Leader

### Beschreibung
Party-Leader kickt Member aus der Party. Server validiert Leader-Status und führt Kick durch.

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| TargetCharacterId | long | Zu kickender Spieler | Ja |
| Reason | string | Optional Kick-Grund | Nein |

### Erwartete Response
- `PartyKickResponse` (742)

### Folge-Messages bei Erfolg
- `PartyUpdate` (704) an alle Members

### Beispiel Payload
```csharp
var partyKick = new PartyKick
{
    Type = MessageType.PartyKick,
    TargetCharacterId = 54321,
    Reason = "AFK too long"
};
```

### Error Codes
| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `NOT_PARTY_LEADER` | Nur Leader darf kicken | - |
| `PLAYER_NOT_IN_PARTY` | Spieler nicht in Party | - |
| `CANNOT_KICK_SELF` | Leader kann sich nicht selbst kicken | Verwende PartyLeave |

---

## PartyUpdate (704)

**Richtung:** 📡 Broadcast (Server → All Party Members)  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Server broadcastet Party-Roster-Update an alle Members. Wird bei Join, Leave, Kick oder Leadership-Change gesendet.

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| UpdateType | byte | 1=Join, 2=Leave, 3=Kick, 4=LeaderChange | Ja |
| Members | PartyMemberDto[] | Aktuelle Member-Liste | Ja |
| LeaderId | long | Character-ID des Leaders | Ja |

### Beispiel Payload
```csharp
var partyUpdate = new PartyUpdate
{
    Type = MessageType.PartyUpdate,
    UpdateType = 1, // Join
    Members = new[] { ... },
    LeaderId = 98765
};
```

### Notizen
- **Broadcast**: An alle Party-Members (inkl. Joiner selbst)
- **UI-Update**: Client aktualisiert Party-Frames

---

## PartyDisband (705)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Party-Leader

### Beschreibung
Party-Leader löst Party komplett auf. Alle Members werden aus der Party entfernt.

### Request Payload
Keine zusätzlichen Felder (nur MessageType)

### Erwartete Response
- `PartyDisbandResponse` (744)

### Folge-Messages bei Erfolg
- `PartyUpdate` (704) Broadcast an alle Members mit leerer Liste

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

---

## PartyLeaderChange (706)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Party-Leader

### Beschreibung
Party-Leader übergibt Leadership an anderen Member.

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| NewLeaderId | long | Character-ID des neuen Leaders | Ja |

### Erwartete Response
- `PartyPromoteResponse` (743)

### Folge-Messages bei Erfolg
- `PartyUpdate` (704) Broadcast an alle Members

### Beispiel Payload
```csharp
var partyLeaderChange = new PartyLeaderChange
{
    Type = MessageType.PartyLeaderChange,
    NewLeaderId = 54321
};
```

### Error Codes
| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `NOT_PARTY_LEADER` | Nur Leader darf promoten | - |
| `PLAYER_NOT_IN_PARTY` | Spieler nicht in Party | - |

---

## PartyLootChange (707)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Party-Leader

### Beschreibung
Party-Leader ändert Loot-Einstellungen der Party.

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| LootMode | byte | LootMode enum | Ja |
| LootThreshold | byte | Item-Qualität ab der Regeln greifen | Nein |
| MasterLooterId | long | Master-Looter (nur bei LootMode=2) | Nein |

### Erwartete Response
- `PartyLootModeResponse` (745)

### Beispiel Payload
```csharp
var partyLootChange = new PartyLootChange
{
    Type = MessageType.PartyLootChange,
    LootMode = 1, // RoundRobin
    LootThreshold = 2 // Green+
};
```

---

## PartyReadyCheck (708)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Party-Leader

### Beschreibung
Party-Leader startet Ready-Check. Alle Members erhalten Popup.

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Question | string | Optional Question (z.B. "Ready for boss?") | Nein |

### Erwartete Response
- `PartyReadyCheckStartResponse` (746)

### Beispiel Payload
```csharp
var readyCheck = new PartyReadyCheck
{
    Type = MessageType.PartyReadyCheck,
    Question = "Ready for boss pull?"
};
```

---

## PartyReadyResponse (709)

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

### Beispiel Payload
```csharp
var readyResponse = new PartyReadyResponse
{
    Type = MessageType.PartyReadyResponse,
    IsReady = true
};
```

---

## PartyMemberUpdate (710)

**Richtung:** 📡 Broadcast (Server → All Party Members)  
**Frequenz:** ⚡ Sehr häufig (Combat)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Server broadcastet Member-Status-Update (Online/Offline/Dead).

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| CharacterId | long | Member Character-ID | Ja |
| IsOnline | bool | Online-Status | Ja |
| IsDead | bool | Tot? | Ja |
| IsAFK | bool | AFK? | Ja |

---

## PartyPositionUpdate (711)

**Richtung:** 📡 Broadcast (Server → All Party Members)  
**Frequenz:** ⚡ Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Server broadcastet Member-Position für Mini-Map.

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| CharacterId | long | Member Character-ID | Ja |
| X | float | Position X | Ja |
| Y | float | Position Y | Ja |
| ZoneId | uint | Zone-ID | Ja |

---

## PartyHealthUpdate (712)

**Richtung:** 📡 Broadcast (Server → All Party Members)  
**Frequenz:** ⚡ Sehr häufig (Combat)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Server broadcastet Member-HP für Party-Frames.

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| CharacterId | long | Member Character-ID | Ja |
| CurrentHp | int | Aktuelle HP | Ja |
| MaxHp | int | Max HP | Ja |

---

## PartyResourceUpdate (713)

**Richtung:** 📡 Broadcast (Server → All Party Members)  
**Frequenz:** ⚡ Häufig (Combat)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Server broadcastet Member-Mana/Resource für Party-Frames.

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| CharacterId | long | Member Character-ID | Ja |
| CurrentResource | int | Aktueller Wert | Ja |
| MaxResource | int | Max Wert | Ja |
| ResourceType | byte | 0=Mana, 1=Energy, 2=Rage | Ja |

---

## PartyBuffUpdate (714)

**Richtung:** 📡 Broadcast (Server → All Party Members)  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Server broadcastet Member-Buffs/Debuffs für Party-Frames.

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| CharacterId | long | Member Character-ID | Ja |
| BuffId | uint | Buff/Debuff ID | Ja |
| RemainingDuration | float | Verbleibende Zeit (Sekunden) | Ja |
| Stacks | byte | Anzahl Stacks | Ja |
| IsDebuff | bool | Ist Debuff? | Ja |

---

## PartyTargetUpdate (715)

**Richtung:** 📡 Broadcast (Server → All Party Members)  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Server broadcastet Member-Target für Assist-Feature.

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| CharacterId | long | Member Character-ID | Ja |
| TargetId | long | Target Entity-ID (0=kein Target) | Ja |
| TargetType | byte | 0=None, 1=Player, 2=NPC, 3=Object | Ja |

---

## PartyRoleSet (716)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Party-Leader

### Beschreibung
Party-Leader setzt Rolle für Member (Tank/Healer/DPS).

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| CharacterId | long | Member Character-ID | Ja |
| Role | byte | PartyRole enum | Ja |

---

## PartyRoleCheck (717)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Party-Leader

### Beschreibung
Party-Leader startet Role-Check (alle wählen Rolle).

### Request Payload
Keine zusätzlichen Felder

---

## PartyConvertToRaid (718)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Party-Leader

### Beschreibung
Party-Leader konvertiert Party zu Raid (max 40 Members).

### Request Payload
Keine zusätzlichen Felder

---

## PartySync (719)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Server sendet vollständige Party-Daten bei Login/Reconnect.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Members | PartyMemberDto[] | Alle Members | Ja |
| LeaderId | long | Leader Character-ID | Ja |
| LootMode | byte | LootMode enum | Ja |
| IsRaid | bool | Ist Raid? | Ja |

---

## PartySummon (720)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Warlock oder Meeting Stone

### Beschreibung
Spieler startet Summon für Party-Member.

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| TargetCharacterId | long | Zu summonendes Member | Ja |

### Erwartete Response
- `PartySummonResponse` (721)

---

## PartySummonResponse (721)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Server bestätigt Summon-Start oder Fehler.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Success | bool | Summon gestartet? | Ja |
| ErrorCode | string | Fehlercode | Nein |
| RequiredClicks | int | Benötigte Klicks (2 für Summon) | Bei Erfolg |

---

## PartyMarkerSet (722)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Party-Leader oder Assist

### Beschreibung
Setzt Raid-Marker auf Target.

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| TargetId | long | Target Entity-ID | Ja |
| MarkerType | byte | PartyMarkerType enum | Ja |

---

## PartyMarkerClear (723)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Party-Leader oder Assist

### Beschreibung
Entfernt Raid-Marker von Target.

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| TargetId | long | Target Entity-ID | Ja |

---

## PartyDifficultyVote (724)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Member stimmt für Dungeon-Schwierigkeit.

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Difficulty | byte | 0=Normal, 1=Heroic, 2=Mythic | Ja |

---

## PartyDifficultySet (725)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Party-Leader

### Beschreibung
Party-Leader setzt Dungeon-Schwierigkeit.

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Difficulty | byte | 0=Normal, 1=Heroic, 2=Mythic | Ja |

---

## PartyAcceptResponse (740)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Antwort wenn ein Target einen Invite annimmt. Wird an beide Parteien gesendet.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Success | bool | Join erfolgreich? | Ja |
| ErrorCode | string | Fehlercode falls Success=false | Nein |
| PartySize | int | Aktuelle Party-Größe | Bei Erfolg |

---

## PartyLeaveResponse (741)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Antwort auf PartyLeave Request. Bestätigt erfolgreichen Party-Austritt.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Success | bool | Leave erfolgreich? | Ja |
| ErrorCode | string | Fehlercode falls Success=false | Nein |

---

## PartyKickResponse (742)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Antwort auf PartyKick Request. Bestätigt erfolgreichen Kick.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Success | bool | Kick erfolgreich? | Ja |
| ErrorCode | string | Fehlercode falls Success=false | Nein |
| KickedPlayerName | string | Name des gekickten Spielers | Bei Erfolg |

---

## PartyPromoteResponse (743)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Antwort auf PartyLeaderChange Request.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Success | bool | Promote erfolgreich? | Ja |
| ErrorCode | string | Fehlercode falls Success=false | Nein |
| NewLeaderName | string | Name des neuen Leaders | Bei Erfolg |

---

## PartyDisbandResponse (744)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Antwort auf PartyDisband Request.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Success | bool | Disband erfolgreich? | Ja |
| ErrorCode | string | Fehlercode falls Success=false | Nein |

---

## PartyLootModeResponse (745)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Antwort auf PartyLootChange Request.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Success | bool | Änderung erfolgreich? | Ja |
| ErrorCode | string | Fehlercode falls Success=false | Nein |
| LootMode | byte | Neuer LootMode | Bei Erfolg |

---

## PartyReadyCheckStartResponse (746)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Antwort auf PartyReadyCheck Request.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Success | bool | Start erfolgreich? | Ja |
| ErrorCode | string | Fehlercode falls Success=false | Nein |
| ReadyCheckId | uint | ID des Ready-Checks | Bei Erfolg |

---

## 🗑️ Obsolete Messages

*Derzeit keine obsoleten Messages in dieser Kategorie.*

---

## 📎 Anhang

### MessageType Enum (Party-Bereich)

```csharp
// GROUP / PARTY (0700-0799)
PartyInvite = 700,
PartyInviteResponse = 701,
PartyLeave = 702,
PartyKick = 703,
PartyUpdate = 704,
PartyDisband = 705,
PartyLeaderChange = 706,
PartyLootChange = 707,
PartyReadyCheck = 708,
PartyReadyResponse = 709,
PartyMemberUpdate = 710,
PartyPositionUpdate = 711,
PartyHealthUpdate = 712,
PartyResourceUpdate = 713,
PartyBuffUpdate = 714,
PartyTargetUpdate = 715,
PartyRoleSet = 716,
PartyRoleCheck = 717,
PartyConvertToRaid = 718,
PartySync = 719,
PartySummon = 720,
PartySummonResponse = 721,
PartyMarkerSet = 722,
PartyMarkerClear = 723,
PartyDifficultyVote = 724,
PartyDifficultySet = 725,
PartyAcceptResponse = 740,
PartyLeaveResponse = 741,
PartyKickResponse = 742,
PartyPromoteResponse = 743,
PartyDisbandResponse = 744,
PartyLootModeResponse = 745,
PartyReadyCheckStartResponse = 746,
```

### Request/Response Paare

| Request | ID | Response | ID |
|---------|-----|----------|-----|
| PartyInvite | 700 | PartyInviteResponse | 701 |
| PartyLeave | 702 | PartyLeaveResponse | 741 |
| PartyKick | 703 | PartyKickResponse | 742 |
| PartyDisband | 705 | PartyDisbandResponse | 744 |
| PartyLeaderChange | 706 | PartyPromoteResponse | 743 |
| PartyLootChange | 707 | PartyLootModeResponse | 745 |
| PartyReadyCheck | 708 | PartyReadyCheckStartResponse | 746 |
| PartySummon | 720 | PartySummonResponse | 721 |

### Datei-Struktur

```
shared/Mmo.Shared/Messaging/
├── Enums/
│   └── MessageType.cs
├── Contracts/
│   └── IPartyMessage.cs
└── Dtos/
    ├── PartyInviteDto.cs
    ├── PartyMemberDto.cs
    └── PartyUpdateDto.cs
```

---

## 🔗 Verwandte Kategorien

- **Chat (04)**: Party-Chat → `ChatParty` (404)
- **Combat (03)**: Party-Combat → XP-Sharing
- **Loot (31)**: Party-Loot → `LootRequest` (3100)
- **Quest (10)**: Party-Quests → `QuestShare` (1010)

---

**Letzte Aktualisierung**: 2026-01-02  
**Version**: 3.0.0  
**Status**: ✅ Vollständig dokumentiert (33/33 Messages)

[← Zurück zur Übersicht](README.md)
