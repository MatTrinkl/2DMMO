# 🏰 Guild Messages (0800-0850)

**Kategorie:** 08  
**Range:** 0800-0850 (AKTIV)  
**Phase:** Prototyp  
**Status:** 🟢 In Entwicklung  
**Version:** 3.0.0  
**Letzte Aktualisierung:** 2026-01-02

[← Zurück zur Übersicht](README.md)

---

## 📋 Inhaltsverzeichnis

- [🔄 Guild Flow (Übersicht)](#-guild-flow-übersicht)
- [🧱 DTOs / Enums / Interfaces](#-dtos--enums--interfaces)
- [📩 Aktive Messages (0800-0850)](#-aktive-messages-0800-0850)
- [🗑️ Obsolete Messages](#️-obsolete-messages)
- [📎 Anhang](#-anhang)

---

## 🔄 Guild Flow (Übersicht)

### Server-Authoritative Architektur

Alle Guild-Operationen sind **server-authoritative**. Der Server validiert alle Anfragen, prüft Permissions und broadcasted Changes an alle Online-Guild-Members.

```
┌─────────────────────────────────────────────────────────────────┐
│                    GUILD SYSTEM ARCHITEKTUR                     │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  Client A              Server                    Client B       │
│     │                    │                          │           │
│     │  GuildInvite       │                          │           │
│     │  TargetName="B"    │                          │           │
│     │───────────────────►│                          │           │
│     │                    │  ┌─ Permission-Check     │           │
│     │                    │  ├─ Guild-Full-Check     │           │
│     │                    │  └─ Target-Available?    │           │
│     │                    │                          │           │
│     │  GuildInviteResp   │  GuildUpdate             │           │
│     │  Success=true      │  (Invite-Pending)        │           │
│     │◄───────────────────│─────────────────────────►│           │
└─────────────────────────────────────────────────────────────────┘
```

### Guild-Invite + Join Flow

```
Client (Inviter)           Server                Client (Target)
      │                      │                        │
      │ GuildInvite (800)    │                        │
      │ TargetName="Legolas" │                        │
      │─────────────────────►│                        │
      │                      │ ┌─ Validate:           │
      │                      │ ├─ Has Invite-Perm?    │
      │                      │ ├─ Guild not full?     │
      │                      │ ├─ Target online?      │
      │                      │ └─ Target not in Guild?│
      │                      │                        │
      │                      │ GuildUpdate (804)      │
      │                      │ (Invite notification)  │
      │                      │───────────────────────►│
      │                      │                        │
      │ GuildInviteResp(801) │                        │
      │ Success=true         │                        │
      │◄─────────────────────│                        │
```

### Guild Rank Management Flow

```
Client (GM/Officer)        Server                All Guild Members
      │                      │                        │
      │ GuildPromote (806)   │                        │
      │ Target="Member1"     │                        │
      │ NewRank=2            │                        │
      │─────────────────────►│                        │
      │                      │ ┌─ Validate:           │
      │                      │ ├─ Has Promote-Perm?   │
      │                      │ ├─ Can promote to rank?│
      │                      │ └─ Target in guild?    │
      │                      │                        │
      │ GuildPromoteResp     │ GuildUpdate (804)      │
      │ (844)                │ (Rank-Changed)         │
      │◄─────────────────────│───────────────────────►│
```

---

## 🧱 DTOs / Enums / Interfaces

### GuildMemberDto

```csharp
[MessagePackObject]
public class GuildMemberDto
{
    [Key(0)] public long CharacterId { get; set; }
    [Key(1)] public string Name { get; set; }
    [Key(2)] public int Level { get; set; }
    [Key(3)] public byte ClassId { get; set; }
    [Key(4)] public byte RankId { get; set; }
    [Key(5)] public bool IsOnline { get; set; }
    [Key(6)] public long LastOnline { get; set; }
    [Key(7)] public long JoinedAt { get; set; }
}
```

### GuildRankDto

```csharp
[MessagePackObject]
public class GuildRankDto
{
    [Key(0)] public byte RankId { get; set; }
    [Key(1)] public string RankName { get; set; }
    [Key(2)] public uint Permissions { get; set; }
    [Key(3)] public int BankGoldLimit { get; set; }
}
```

### GuildPermission (Enum)

```csharp
[Flags]
public enum GuildPermission : uint
{
    None = 0,
    InviteMembers = 1 << 0,
    RemoveMembers = 1 << 1,
    PromoteMembers = 1 << 2,
    DemoteMembers = 1 << 3,
    EditGuildInfo = 1 << 4,
    EditMOTD = 1 << 5,
    ViewBankTab1 = 1 << 6,
    DepositBankTab1 = 1 << 7,
    WithdrawBankTab1 = 1 << 8,
    ManageRanks = 1 << 20,
    ManageEvents = 1 << 21,
    GuildMaster = 0xFFFFFFFF
}
```

---

## 📩 Aktive Messages (0800-0850)

---

### GuildInvite (800)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Auth:** 🔒 Ja  
**Permission:** InviteMembers

Guild-Member mit Invite-Permission sendet Einladung an Online-Spieler.

| Feld | Typ | Pflicht | Beschreibung |
|------|-----|---------|--------------|
| TargetName | string | ✅ | Name des einzuladenden Spielers |

**Response:** `GuildInviteResponse (801)`

---

### GuildInviteResponse (801)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Auth:** 🔒 Ja

Antwort auf GuildInvite mit Erfolg/Fehler-Status.

| Feld | Typ | Pflicht | Beschreibung |
|------|-----|---------|--------------|
| Success | bool | ✅ | Invite erfolgreich? |
| ErrorCode | string | ❌ | Fehlercode bei Failure |
| InvitedName | string | ✅ | Name des Eingeladenen |

---

### GuildLeave (802)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Auth:** 🔒 Ja

Member verlässt Guild freiwillig. GM kann nicht leave (muss disband).

| Feld | Typ | Pflicht | Beschreibung |
|------|-----|---------|--------------|
| (keine) | - | - | Keine zusätzlichen Felder |

**Response:** `GuildLeaveResponse (841)`

---

### GuildKick (803)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Auth:** 🔒 Ja  
**Permission:** RemoveMembers

Member mit Remove-Permission kickt anderen Member (nur niedrigere Ranks).

| Feld | Typ | Pflicht | Beschreibung |
|------|-----|---------|--------------|
| TargetName | string | ✅ | Name des zu kickenden Members |
| Reason | string | ❌ | Optionaler Kick-Grund |

**Response:** `GuildKickResponse (842)`

---

### GuildUpdate (804)

**Richtung:** 📡 Server → All Guild Members  
**Frequenz:** Häufig  
**Auth:** 🔒 Ja

Broadcast bei allen Guild-Änderungen (Join, Leave, Rank-Change, etc.).

| Feld | Typ | Pflicht | Beschreibung |
|------|-----|---------|--------------|
| UpdateType | byte | ✅ | Art der Änderung (GuildUpdateType enum) |
| Data | byte[] | ✅ | Serialisierte Änderungsdaten |
| Timestamp | long | ✅ | Server-Zeitstempel |

---

### GuildDisband (805)

**Richtung:** 📤 Client → Server  
**Frequenz:** Sehr selten  
**Auth:** 🔒 Ja  
**Permission:** GuildMaster only

Guild Master löst Guild komplett auf.

| Feld | Typ | Pflicht | Beschreibung |
|------|-----|---------|--------------|
| Confirmation | string | ✅ | Muss exakten Guild-Namen enthalten |

**Response:** `GuildDisbandResponse (843)`

---

### GuildPromote (806)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Auth:** 🔒 Ja  
**Permission:** PromoteMembers

Erhöht den Rank eines Members. Kann nur zu niedrigerem Rank als eigener promoten.

| Feld | Typ | Pflicht | Beschreibung |
|------|-----|---------|--------------|
| TargetName | string | ✅ | Name des zu promotenden Members |
| NewRank | byte | ✅ | Neuer Rank (0-9) |

**Response:** `GuildPromoteResponse (844)`

---

### GuildDemote (807)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Auth:** 🔒 Ja  
**Permission:** DemoteMembers

Verringert den Rank eines Members. Kann nur niedrigere Ranks demoten.

| Feld | Typ | Pflicht | Beschreibung |
|------|-----|---------|--------------|
| TargetName | string | ✅ | Name des zu demotenden Members |
| NewRank | byte | ✅ | Neuer Rank (0-9) |

**Response:** `GuildDemoteResponse (845)`

---

### GuildMotd (808)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten (Login)  
**Auth:** 🔒 Ja

Server sendet aktuelle Guild-MOTD bei Login/Join.

| Feld | Typ | Pflicht | Beschreibung |
|------|-----|---------|--------------|
| MOTD | string | ✅ | Message of the Day (max 500 Zeichen) |
| SetBy | string | ✅ | Name des Setzers |
| SetAt | long | ✅ | Unix-Timestamp |

---

### GuildMotdSet (809)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Auth:** 🔒 Ja  
**Permission:** EditMOTD

Setzt neue Guild-MOTD.

| Feld | Typ | Pflicht | Beschreibung |
|------|-----|---------|--------------|
| MOTD | string | ✅ | Neue MOTD (max 500 Zeichen) |

**Response:** `GuildMOTDResponse (847)`

---

### GuildRosterRequest (810)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Auth:** 🔒 Ja

Client fordert vollständige Member-Liste an.

| Feld | Typ | Pflicht | Beschreibung |
|------|-----|---------|--------------|
| (keine) | - | - | Keine zusätzlichen Felder |

**Response:** `GuildRosterResponse (811)`

---

### GuildRosterResponse (811)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Auth:** 🔒 Ja

Vollständige Guild-Member-Liste.

| Feld | Typ | Pflicht | Beschreibung |
|------|-----|---------|--------------|
| Members | GuildMemberDto[] | ✅ | Array aller Guild-Members |
| TotalCount | int | ✅ | Gesamtanzahl |

---

### GuildRankCreate (812)

**Richtung:** 📤 Client → Server  
**Frequenz:** Sehr selten  
**Auth:** 🔒 Ja  
**Permission:** ManageRanks

Erstellt neuen Rank (max 10 Ranks).

| Feld | Typ | Pflicht | Beschreibung |
|------|-----|---------|--------------|
| RankName | string | ✅ | Name des neuen Ranks |
| Permissions | uint | ✅ | Permission-Bitfield |
| InsertAfterRank | byte | ✅ | Position (0-9) |

**Response:** `GuildRankEditResponse (846)`

---

### GuildRankDelete (813)

**Richtung:** 📤 Client → Server  
**Frequenz:** Sehr selten  
**Auth:** 🔒 Ja  
**Permission:** ManageRanks

Löscht einen Rank (außer GM-Rank 0). Members werden zu nächsthöherem Rank verschoben.

| Feld | Typ | Pflicht | Beschreibung |
|------|-----|---------|--------------|
| RankId | byte | ✅ | Zu löschender Rank (1-9) |

**Response:** `GuildRankEditResponse (846)`

---

### GuildRankEdit (814)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Auth:** 🔒 Ja  
**Permission:** ManageRanks

Editiert Rank-Name und/oder Permissions.

| Feld | Typ | Pflicht | Beschreibung |
|------|-----|---------|--------------|
| RankId | byte | ✅ | Zu editierender Rank |
| RankName | string | ❌ | Neuer Name |
| Permissions | uint | ❌ | Neue Permissions |

**Response:** `GuildRankEditResponse (846)`

---

### GuildRankReorder (815)

**Richtung:** 📤 Client → Server  
**Frequenz:** Sehr selten  
**Auth:** 🔒 Ja  
**Permission:** ManageRanks

Ändert die Reihenfolge der Ranks.

| Feld | Typ | Pflicht | Beschreibung |
|------|-----|---------|--------------|
| RankOrder | byte[] | ✅ | Neue Rank-Reihenfolge |

**Response:** `GuildRankEditResponse (846)`

---

### GuildPermissionSet (816)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Auth:** 🔒 Ja  
**Permission:** ManageRanks

Setzt spezifische Permission für einen Rank.

| Feld | Typ | Pflicht | Beschreibung |
|------|-----|---------|--------------|
| RankId | byte | ✅ | Rank-ID |
| Permission | uint | ✅ | Permission-Bit |
| Enabled | bool | ✅ | Ein/Aus |

**Response:** `GuildRankEditResponse (846)`

---

### GuildInfoEdit (817)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Auth:** 🔒 Ja  
**Permission:** EditGuildInfo

Editiert Guild-Informationen (Name, Tag, Description).

| Feld | Typ | Pflicht | Beschreibung |
|------|-----|---------|--------------|
| Field | string | ✅ | Zu editierendes Feld |
| Value | string | ✅ | Neuer Wert |

**Response:** Via `GuildUpdate (804)`

---

### GuildTabardChange (818)

**Richtung:** 📤 Client → Server  
**Frequenz:** Sehr selten  
**Auth:** 🔒 Ja  
**Permission:** EditGuildInfo

Ändert das Guild-Tabard-Design.

| Feld | Typ | Pflicht | Beschreibung |
|------|-----|---------|--------------|
| IconId | int | ✅ | Icon-ID |
| BorderId | int | ✅ | Rand-ID |
| BackgroundId | int | ✅ | Hintergrund-ID |
| IconColor | int | ✅ | Icon-Farbe (RGB) |
| BorderColor | int | ✅ | Rand-Farbe (RGB) |
| BackgroundColor | int | ✅ | Hintergrund-Farbe (RGB) |

**Response:** Via `GuildUpdate (804)`

---

### GuildBankOpen (819)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Auth:** 🔒 Ja

Öffnet Guild-Bank-UI. Server sendet Bank-Contents.

| Feld | Typ | Pflicht | Beschreibung |
|------|-----|---------|--------------|
| (keine) | - | - | Keine zusätzlichen Felder |

**Response:** Via `GuildUpdate (804)` mit Bank-Data

---

### GuildBankDeposit (820)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Auth:** 🔒 Ja  
**Permission:** DepositBankTab[N]

Legt Item in Guild-Bank.

| Feld | Typ | Pflicht | Beschreibung |
|------|-----|---------|--------------|
| SourceBag | byte | ✅ | Quell-Bag |
| SourceSlot | byte | ✅ | Quell-Slot |
| TabIndex | byte | ✅ | Ziel-Bank-Tab |
| TargetSlot | byte | ✅ | Ziel-Slot |
| Quantity | int | ✅ | Anzahl |

**Response:** `GuildBankDepositResponse (849)`

---

### GuildBankWithdraw (821)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Auth:** 🔒 Ja  
**Permission:** WithdrawBankTab[N]

Nimmt Item aus Guild-Bank.

| Feld | Typ | Pflicht | Beschreibung |
|------|-----|---------|--------------|
| TabIndex | byte | ✅ | Quell-Bank-Tab |
| SourceSlot | byte | ✅ | Quell-Slot |
| Quantity | int | ✅ | Anzahl |

**Response:** `GuildBankWithdrawResponse (850)`

---

### GuildBankLog (822)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Auth:** 🔒 Ja

Fordert Bank-Transaction-Log an.

| Feld | Typ | Pflicht | Beschreibung |
|------|-----|---------|--------------|
| TabIndex | byte | ✅ | Bank-Tab |
| PageIndex | int | ❌ | Seite (Pagination) |

**Response:** Via `GuildUpdate (804)` mit Log-Data

---

### GuildBankTabCreate (823)

**Richtung:** 📤 Client → Server  
**Frequenz:** Sehr selten  
**Auth:** 🔒 Ja  
**Permission:** ManageRanks

Erstellt neuen Bank-Tab (max 8).

| Feld | Typ | Pflicht | Beschreibung |
|------|-----|---------|--------------|
| TabName | string | ✅ | Name des Tabs |
| TabIcon | int | ✅ | Icon-ID |

**Response:** Via `GuildUpdate (804)`

---

### GuildBankTabEdit (824)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Auth:** 🔒 Ja  
**Permission:** ManageRanks

Editiert Bank-Tab (Name, Icon).

| Feld | Typ | Pflicht | Beschreibung |
|------|-----|---------|--------------|
| TabIndex | byte | ✅ | Tab-Index |
| TabName | string | ❌ | Neuer Name |
| TabIcon | int | ❌ | Neues Icon |

**Response:** Via `GuildUpdate (804)`

---

### GuildBankPermission (825)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Auth:** 🔒 Ja  
**Permission:** ManageRanks

Setzt Bank-Tab-Permissions für einen Rank.

| Feld | Typ | Pflicht | Beschreibung |
|------|-----|---------|--------------|
| TabIndex | byte | ✅ | Tab-Index |
| RankId | byte | ✅ | Rank-ID |
| CanView | bool | ✅ | Darf Tab sehen |
| CanDeposit | bool | ✅ | Darf einzahlen |
| CanWithdraw | bool | ✅ | Darf abheben |
| WithdrawLimit | int | ✅ | Tägliches Limit |

**Response:** Via `GuildUpdate (804)`

---

### GuildAchievement (826)

**Richtung:** 📡 Server → All Guild Members  
**Frequenz:** Selten  
**Auth:** 🔒 Ja

Broadcast bei Guild-Achievement-Unlock.

| Feld | Typ | Pflicht | Beschreibung |
|------|-----|---------|--------------|
| AchievementId | int | ✅ | Achievement-ID |
| UnlockedBy | string | ✅ | Member der Achievement freigeschaltet hat |
| Timestamp | long | ✅ | Unix-Timestamp |

---

### GuildNews (827)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Auth:** 🔒 Ja

Guild-News-Feed-Eintrag (Kills, Achievements, Loot, etc.).

| Feld | Typ | Pflicht | Beschreibung |
|------|-----|---------|--------------|
| NewsType | byte | ✅ | Art der News |
| MemberName | string | ✅ | Betroffener Member |
| Data | byte[] | ✅ | News-spezifische Daten |
| Timestamp | long | ✅ | Unix-Timestamp |

---

### GuildEventCreate (828)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Auth:** 🔒 Ja  
**Permission:** ManageEvents

Erstellt Guild-Event (Raid, Meeting, etc.).

| Feld | Typ | Pflicht | Beschreibung |
|------|-----|---------|--------------|
| Title | string | ✅ | Event-Titel |
| Description | string | ❌ | Beschreibung |
| EventType | byte | ✅ | Art (Raid, Dungeon, Meeting) |
| StartTime | long | ✅ | Startzeit (Unix) |
| Duration | int | ✅ | Dauer in Minuten |

**Response:** Via `GuildUpdate (804)`

---

### GuildEventEdit (829)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Auth:** 🔒 Ja  
**Permission:** ManageEvents

Editiert bestehendes Guild-Event.

| Feld | Typ | Pflicht | Beschreibung |
|------|-----|---------|--------------|
| EventId | long | ✅ | Event-ID |
| Title | string | ❌ | Neuer Titel |
| Description | string | ❌ | Neue Beschreibung |
| StartTime | long | ❌ | Neue Startzeit |
| Duration | int | ❌ | Neue Dauer |

**Response:** Via `GuildUpdate (804)`

---

### GuildEventDelete (830)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Auth:** 🔒 Ja  
**Permission:** ManageEvents

Löscht Guild-Event.

| Feld | Typ | Pflicht | Beschreibung |
|------|-----|---------|--------------|
| EventId | long | ✅ | Event-ID |

**Response:** Via `GuildUpdate (804)`

---

### GuildEventSignup (831)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Auth:** 🔒 Ja

Member meldet sich für Event an/ab.

| Feld | Typ | Pflicht | Beschreibung |
|------|-----|---------|--------------|
| EventId | long | ✅ | Event-ID |
| Status | byte | ✅ | 0=Abmelden, 1=Anmelden, 2=Tentative |
| Role | byte | ❌ | Gewünschte Rolle (Tank/Healer/DPS) |

**Response:** Via `GuildUpdate (804)`

---

### GuildSearch (832)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Auth:** 🔒 Ja

Sucht öffentliche Guilds.

| Feld | Typ | Pflicht | Beschreibung |
|------|-----|---------|--------------|
| SearchQuery | string | ❌ | Suchbegriff |
| MinLevel | int | ❌ | Min Guild-Level |
| MaxMembers | int | ❌ | Max Member-Anzahl |
| PlayStyle | byte | ❌ | PvE/PvP/Casual |

**Response:** Via `GuildUpdate (804)` mit Search-Results

---

### GuildApply (833)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Auth:** 🔒 Ja

Bewirbt sich bei einer Guild.

| Feld | Typ | Pflicht | Beschreibung |
|------|-----|---------|--------------|
| GuildId | long | ✅ | Ziel-Guild-ID |
| Message | string | ❌ | Bewerbungstext |

**Response:** `GuildApplicationResponse (835)`

---

### GuildApplicationList (834)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Auth:** 🔒 Ja  
**Permission:** InviteMembers

Fordert Liste offener Bewerbungen an.

| Feld | Typ | Pflicht | Beschreibung |
|------|-----|---------|--------------|
| (keine) | - | - | Keine zusätzlichen Felder |

**Response:** Via `GuildUpdate (804)` mit Application-List

---

### GuildApplicationResponse (835)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Auth:** 🔒 Ja  
**Permission:** InviteMembers

Akzeptiert/Lehnt Bewerbung ab.

| Feld | Typ | Pflicht | Beschreibung |
|------|-----|---------|--------------|
| ApplicationId | long | ✅ | Bewerbungs-ID |
| Accept | bool | ✅ | true=Annehmen, false=Ablehnen |
| Message | string | ❌ | Optionale Nachricht |

**Response:** Via `GuildUpdate (804)`

---

### GuildAllianceInvite (836)

**Richtung:** 📤 Client → Server  
**Frequenz:** Sehr selten  
**Auth:** 🔒 Ja  
**Permission:** ManageAlliance

Sendet Allianz-Einladung an andere Guild.

| Feld | Typ | Pflicht | Beschreibung |
|------|-----|---------|--------------|
| TargetGuildId | long | ✅ | Ziel-Guild-ID |

**Response:** `GuildAllianceResponse (837)`

---

### GuildAllianceResponse (837)

**Richtung:** 📤 Client → Server  
**Frequenz:** Sehr selten  
**Auth:** 🔒 Ja  
**Permission:** ManageAlliance

Akzeptiert/Lehnt Allianz-Einladung ab.

| Feld | Typ | Pflicht | Beschreibung |
|------|-----|---------|--------------|
| InvitingGuildId | long | ✅ | Einladende Guild-ID |
| Accept | bool | ✅ | true=Annehmen, false=Ablehnen |

**Response:** Via `GuildUpdate (804)`

---

### GuildAllianceLeave (838)

**Richtung:** 📤 Client → Server  
**Frequenz:** Sehr selten  
**Auth:** 🔒 Ja  
**Permission:** ManageAlliance

Verlässt bestehende Allianz.

| Feld | Typ | Pflicht | Beschreibung |
|------|-----|---------|--------------|
| AllianceId | long | ✅ | Allianz-ID |

**Response:** Via `GuildUpdate (804)`

---

### GuildCreateResponse (840)

**Richtung:** 📥 Server → Client  
**Frequenz:** Sehr selten  
**Auth:** 🔒 Ja

Antwort auf implizite Guild-Creation (via erste Invite bei guildless Char).

| Feld | Typ | Pflicht | Beschreibung |
|------|-----|---------|--------------|
| Success | bool | ✅ | Erfolgreich? |
| ErrorCode | string | ❌ | Fehlercode |
| GuildId | long | ✅ | Neue Guild-ID |
| GuildName | string | ✅ | Guild-Name |

---

### GuildLeaveResponse (841)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Auth:** 🔒 Ja

Antwort auf GuildLeave (802).

| Feld | Typ | Pflicht | Beschreibung |
|------|-----|---------|--------------|
| Success | bool | ✅ | Erfolgreich? |
| ErrorCode | string | ❌ | Fehlercode (z.B. IS_GUILD_MASTER) |

---

### GuildKickResponse (842)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Auth:** 🔒 Ja

Antwort auf GuildKick (803).

| Feld | Typ | Pflicht | Beschreibung |
|------|-----|---------|--------------|
| Success | bool | ✅ | Erfolgreich? |
| ErrorCode | string | ❌ | Fehlercode |
| KickedName | string | ✅ | Name des Gekickten |

---

### GuildDisbandResponse (843)

**Richtung:** 📥 Server → Client  
**Frequenz:** Sehr selten  
**Auth:** 🔒 Ja

Antwort auf GuildDisband (805).

| Feld | Typ | Pflicht | Beschreibung |
|------|-----|---------|--------------|
| Success | bool | ✅ | Erfolgreich? |
| ErrorCode | string | ❌ | Fehlercode |

---

### GuildPromoteResponse (844)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Auth:** 🔒 Ja

Antwort auf GuildPromote (806).

| Feld | Typ | Pflicht | Beschreibung |
|------|-----|---------|--------------|
| Success | bool | ✅ | Erfolgreich? |
| ErrorCode | string | ❌ | Fehlercode |
| TargetName | string | ✅ | Promoted Member |
| NewRank | byte | ✅ | Neuer Rank |

---

### GuildDemoteResponse (845)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Auth:** 🔒 Ja

Antwort auf GuildDemote (807).

| Feld | Typ | Pflicht | Beschreibung |
|------|-----|---------|--------------|
| Success | bool | ✅ | Erfolgreich? |
| ErrorCode | string | ❌ | Fehlercode |
| TargetName | string | ✅ | Demoted Member |
| NewRank | byte | ✅ | Neuer Rank |

---

### GuildRankEditResponse (846)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Auth:** 🔒 Ja

Antwort auf GuildRankCreate/Delete/Edit/Reorder (812-815).

| Feld | Typ | Pflicht | Beschreibung |
|------|-----|---------|--------------|
| Success | bool | ✅ | Erfolgreich? |
| ErrorCode | string | ❌ | Fehlercode |
| AffectedRankId | byte | ✅ | Betroffener Rank |

---

### GuildMOTDResponse (847)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Auth:** 🔒 Ja

Antwort auf GuildMotdSet (809).

| Feld | Typ | Pflicht | Beschreibung |
|------|-----|---------|--------------|
| Success | bool | ✅ | Erfolgreich? |
| ErrorCode | string | ❌ | Fehlercode |

---

### GuildMessageResponse (848)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Auth:** 🔒 Ja

Generische Antwort auf Guild-Operations.

| Feld | Typ | Pflicht | Beschreibung |
|------|-----|---------|--------------|
| Success | bool | ✅ | Erfolgreich? |
| ErrorCode | string | ❌ | Fehlercode |
| Message | string | ❌ | Server-Nachricht |

---

### GuildBankDepositResponse (849)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Auth:** 🔒 Ja

Antwort auf GuildBankDeposit (820).

| Feld | Typ | Pflicht | Beschreibung |
|------|-----|---------|--------------|
| Success | bool | ✅ | Erfolgreich? |
| ErrorCode | string | ❌ | Fehlercode (NO_PERMISSION, TAB_FULL) |
| TabIndex | byte | ✅ | Betroffener Tab |
| SlotIndex | byte | ✅ | Betroffener Slot |

---

### GuildBankWithdrawResponse (850)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Auth:** 🔒 Ja

Antwort auf GuildBankWithdraw (821).

| Feld | Typ | Pflicht | Beschreibung |
|------|-----|---------|--------------|
| Success | bool | ✅ | Erfolgreich? |
| ErrorCode | string | ❌ | Fehlercode (NO_PERMISSION, LIMIT_REACHED) |
| TabIndex | byte | ✅ | Betroffener Tab |
| SlotIndex | byte | ✅ | Betroffener Slot |

---

## 🗑️ Obsolete Messages

*Derzeit keine obsoleten Messages in dieser Kategorie.*

---

## 📎 Anhang

### MessageType Enum (Guild-Range)

```csharp
// GUILD (0800-0899)
GuildInvite = 800,
GuildInviteResponse = 801,
GuildLeave = 802,
GuildKick = 803,
GuildUpdate = 804,
GuildDisband = 805,
GuildPromote = 806,
GuildDemote = 807,
GuildMotd = 808,
GuildMotdSet = 809,
GuildRosterRequest = 810,
GuildRosterResponse = 811,
GuildRankCreate = 812,
GuildRankDelete = 813,
GuildRankEdit = 814,
GuildRankReorder = 815,
GuildPermissionSet = 816,
GuildInfoEdit = 817,
GuildTabardChange = 818,
GuildBankOpen = 819,
GuildBankDeposit = 820,
GuildBankWithdraw = 821,
GuildBankLog = 822,
GuildBankTabCreate = 823,
GuildBankTabEdit = 824,
GuildBankPermission = 825,
GuildAchievement = 826,
GuildNews = 827,
GuildEventCreate = 828,
GuildEventEdit = 829,
GuildEventDelete = 830,
GuildEventSignup = 831,
GuildSearch = 832,
GuildApply = 833,
GuildApplicationList = 834,
GuildApplicationResponse = 835,
GuildAllianceInvite = 836,
GuildAllianceResponse = 837,
GuildAllianceLeave = 838,
GuildCreateResponse = 840,
GuildLeaveResponse = 841,
GuildKickResponse = 842,
GuildDisbandResponse = 843,
GuildPromoteResponse = 844,
GuildDemoteResponse = 845,
GuildRankEditResponse = 846,
GuildMOTDResponse = 847,
GuildMessageResponse = 848,
GuildBankDepositResponse = 849,
GuildBankWithdrawResponse = 850,
```

### Request/Response Paare

| Request | ID | Response | ID |
|---------|-----|----------|-----|
| GuildInvite | 800 | GuildInviteResponse | 801 |
| GuildLeave | 802 | GuildLeaveResponse | 841 |
| GuildKick | 803 | GuildKickResponse | 842 |
| GuildDisband | 805 | GuildDisbandResponse | 843 |
| GuildPromote | 806 | GuildPromoteResponse | 844 |
| GuildDemote | 807 | GuildDemoteResponse | 845 |
| GuildMotdSet | 809 | GuildMOTDResponse | 847 |
| GuildRosterRequest | 810 | GuildRosterResponse | 811 |
| GuildRankCreate | 812 | GuildRankEditResponse | 846 |
| GuildRankDelete | 813 | GuildRankEditResponse | 846 |
| GuildRankEdit | 814 | GuildRankEditResponse | 846 |
| GuildRankReorder | 815 | GuildRankEditResponse | 846 |
| GuildBankDeposit | 820 | GuildBankDepositResponse | 849 |
| GuildBankWithdraw | 821 | GuildBankWithdrawResponse | 850 |
| GuildApply | 833 | GuildApplicationResponse | 835 |
| GuildAllianceInvite | 836 | GuildAllianceResponse | 837 |

### Datei-Struktur

```
shared/Mmo.Shared/Messaging/
├── Enums/
│   └── MessageType.cs          # Guild Messages 800-850
├── DTOs/
│   └── Guild/
│       ├── GuildMemberDto.cs
│       ├── GuildRankDto.cs
│       └── GuildBankItemDto.cs
└── Messages/
    └── Guild/
        ├── GuildInvite.cs
        ├── GuildInviteResponse.cs
        └── ... (50 Messages)
```

---

[← Zurück zur Übersicht](README.md)
