# 👥 Party Messages (0700-0799)

**Kategorie:** 7  
**Range:** 0700-0799  
**Phase:** Phase 2  
**Status:** ✅ Comprehensive Documentation

[← Zurück zur Übersicht](README.md)

---

## 📋 Übersicht

Diese Kategorie umfasst alle Messages für das **Party/Group-System** im 2DMMO, einschließlich:

- Party-Einladungen und -Verwaltung
- Leadership und Loot-Verteilung  
- Ready-Checks und Synchronisation
- Member-Updates (Health, Position, Buffs, Targets)
- Role-Assignments (Tank/Healer/DPS)
- Erweiterte Features (Raid-Konvertierung, Summoning, Marker, Schwierigkeitsgrad)

**Maximale Gruppengröße**: 5 Spieler (Party), 40 Spieler (Raid nach Konvertierung)

**🔄 DTO-System:**  
In Phase 2 wird ein `PartyMemberDto` eingeführt für Party-Listen und Member-Updates. Dies ermöglicht minimale Spieler-Infos ohne sensible Daten. Siehe [DTO_ARCHITECTURE.md](DTO_ARCHITECTURE.md) für Details.

---

## PartyInvite (700)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

### Beschreibung

Der Client sendet eine Party-Einladung an einen anderen Spieler. Der Server validiert, dass beide Spieler existieren, online sind, sich in kompatiblen Zonen befinden und dass der einladende Spieler berechtigt ist (Party-Leader oder noch keine Party). Die Einladung hat eine Gültigkeitsdauer von 60 Sekunden.

Wenn der eingeladene Spieler bereits in einer Party ist oder beschäftigt (in Dungeon, PvP, etc.), wird die Einladung abgelehnt. Der Server tracked alle ausstehenden Einladungen pro Spieler (Max 5 gleichzeitig).

### Im Scope ✅

- Einladung eines Spielers per PlayerId oder Username
- Validierung der Spieler-Verfügbarkeit
- Auto-Decline nach 60 Sekunden
- Limit von 5 ausstehenden Einladungen pro Spieler
- Cross-Zone-Einladungen (wenn beide in normalen Zonen sind)

### Nicht im Scope ❌

- Raid-Einladungen → verwende stattdessen `PartyConvertToRaid` (718)
- Guild-Einladungen → verwende stattdessen `GuildInvite` (800)
- Friend-Einladungen → verwende stattdessen Social-Messages (21xx)

### Request Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| TargetPlayerId | int | ID des einzuladenden Spielers | Nein* |
| TargetUsername | string | Username des einzuladenden Spielers | Nein* |

*Eines der beiden Felder muss angegeben werden

### Erwartete Response

- **Bei Erfolg:** `PartyUpdate` (704) an alle Party-Member + Notification an Target
- **Bei Fehler:** `ErrorMessage` (910) mit Code

### Verwandte Messages

| Message | ID | Beziehung |
|---------|-----|-----------|
| `PartyInviteResponse` | 701 | Target-Spieler antwortet auf Einladung |
| `PartyUpdate` | 704 | Party-State ändert sich nach Accept |
| `PartyLeave` | 702 | Spieler verlässt Party |

### Flow-Diagramm

```
Player A (Inviter)         Server              Player B (Target)
      │                       │                        │
      │  PartyInvite(B)       │                        │
      │──────────────────────►│                        │
      │                       │  Validate              │
      │                       │  - A not in party?     │
      │                       │  - B online & valid?   │
      │                       │                        │
      │                       │  Party Invite Notif    │
      │                       │───────────────────────►│
      │                       │                        │
      │                       │◄─ Accept (701) ────────│
      │                       │                        │
      │  PartyUpdate (704)    │                        │
      │◄──────────────────────│                        │
      │                       │  PartyUpdate (704)     │
      │                       │───────────────────────►│
```

### Beispiel Payload

```csharp
var invite = new PartyInviteRequest
{
    Type = MessageType.PartyInvite,
    TargetPlayerId = 12345
};
```

### Error Codes

| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `PLAYER_NOT_FOUND` | Target existiert nicht | Username prüfen |
| `PLAYER_OFFLINE` | Target ist offline | Später erneut versuchen |
| `PLAYER_BUSY` | Target in Dungeon/PvP/etc. | Warten bis verfügbar |
| `ALREADY_IN_PARTY` | Target bereits in Party | Target muss erst leave |
| `PARTY_FULL` | Party bereits voll (5/5) | Raid konvertieren oder kicken |
| `TOO_MANY_INVITES` | Zu viele ausstehende Invites | Warten bis Invites expired |
| `CROSS_FACTION` | Verschiedene Fraktionen | Nur gleiche Fraktion |
| `INVALID_ZONE` | Incompatible Zonen (Instanced) | Zones müssen kompatibel sein |

### Notizen

- **Rate Limiting**: 10 Einladungen pro Minute
- **Auto-Decline**: Nach 60s automatisch abgelehnt
- **Notification**: Target erhält UI-Popup mit Accept/Decline Buttons
- **Anti-Spam**: Gleicher Target kann nur alle 5 Minuten erneut eingeladen werden (nach Decline)
- **Cross-Zone**: Funktioniert zwischen normalen Zonen, aber nicht in/aus Instanzen

---

## PartyInviteResponse (701)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

### Beschreibung

Der eingeladene Spieler antwortet auf eine Party-Einladung mit Accept oder Decline. Bei Accept erstellt der Server entweder eine neue Party (wenn Inviter alleine ist) oder fügt den neuen Spieler zur bestehenden Party hinzu. Der neue Spieler erhält sofort alle Party-Member-Informationen.

Bei Decline wird einfach die Einladung gelöscht und beide Spieler erhalten eine Notification. Der Inviter kann denselben Spieler erst nach 5 Minuten erneut einladen.

### Im Scope ✅

- Accept oder Decline einer ausstehenden Einladung
- Automatische Party-Erstellung bei Accept (wenn nötig)
- Vollständiger Party-State-Sync an neuen Member
- Notification an alle beteiligten Spieler
- Cleanup bei Decline

### Nicht im Scope ❌

- Auto-Accept → muss manuell bestätigt werden
- Counter-Offers → nur Accept/Decline möglich

### Request Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| InviterPlayerId | int | ID des Einladers | Ja |
| Accepted | bool | true = Accept, false = Decline | Ja |

### Erwartete Response

- **Bei Accept:** `PartyUpdate` (704) an alle Party-Member
- **Bei Decline:** `ErrorMessage` (910) mit Info (kein Fehler, nur Notification)
- **Bei Fehler:** `ErrorMessage` (910) mit Code

### Verwandte Messages

| Message | ID | Beziehung |
|---------|-----|-----------|
| `PartyInvite` | 700 | Ursprüngliche Einladung |
| `PartyUpdate` | 704 | Party-State nach Accept |
| `PartySync` | 719 | Vollständiger State-Sync |

### Flow-Diagramm

```
Player B (Target)         Server             Player A (Inviter)
      │                      │                       │
      │  Response(Accept)    │                       │
      │─────────────────────►│                       │
      │                      │  Create/Join Party    │
      │                      │                       │
      │  PartyUpdate(704)    │                       │
      │◄─────────────────────│                       │
      │                      │  PartyUpdate(704)     │
      │                      │──────────────────────►│
      │                      │                       │
      │  PartySync(719)      │                       │
      │◄─────────────────────│                       │
```

### Beispiel Payload

```csharp
// Accept
var response = new PartyInviteResponse
{
    Type = MessageType.PartyInviteResponse,
    InviterPlayerId = 12345,
    Accepted = true
};

// Decline
var decline = new PartyInviteResponse
{
    Type = MessageType.PartyInviteResponse,
    InviterPlayerId = 12345,
    Accepted = false
};
```

### Error Codes

| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `INVITE_EXPIRED` | Einladung abgelaufen (>60s) | Neues Invite anfordern |
| `INVITE_NOT_FOUND` | Keine Einladung gefunden | Bereits beantwortet oder expired |
| `ALREADY_IN_PARTY` | Bereits in einer Party | Party erst verlassen |
| `PARTY_FULL` | Party inzwischen voll | Decline ist automatisch |
| `INVITER_OFFLINE` | Inviter offline gegangen | Auto-Decline |

### Notizen

- **Timeout**: Einladung automatisch declined nach 60s
- **UI**: Accept/Decline UI-Buttons verschwinden nach Response
- **Notifications**: Beide Spieler erhalten Chat-Notification
- **Party Creation**: Server erstellt Party automatisch bei erstem Accept
- **Full Sync**: Neuer Member erhält sofort alle Member-Infos (HP, Position, etc.)

---

## PartyLeave (702)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

### Beschreibung

Ein Spieler verlässt freiwillig die Party. Der Server entfernt den Spieler, broadcastet die Änderung an alle verbleibenden Member und reassigned bei Bedarf den Leader. Wenn nur noch 1 Spieler übrig bleibt, wird die Party automatisch aufgelöst.

Loot-Rights, Dungeon-Progress und Quest-Shares werden entsprechend angepasst. Der verlassende Spieler behält keine Quest-Shares, aber Loot-Rights bleiben für 2 Minuten bestehen (für bereits getötete Mobs).

### Im Scope ✅

- Freiwilliges Verlassen der Party
- Leader-Reassignment (falls nötig)
- Party-Auflösung bei <2 Membern
- Cleanup von Loot-Rights und Quest-Shares
- Notification an alle Member

### Nicht im Scope ❌

- Kick → verwende stattdessen `PartyKick` (703)
- Party-Disband → verwende stattdessen `PartyDisband` (705)

### Request Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| - | - | Keine zusätzlichen Felder | - |

### Erwartete Response

- **Bei Erfolg:** `PartyUpdate` (704) an alle verbleibenden Member
- **Bei Fehler:** `ErrorMessage` (910) mit Code

### Verwandte Messages

| Message | ID | Beziehung |
|---------|-----|-----------|
| `PartyKick` | 703 | Erzwungenes Entfernen |
| `PartyUpdate` | 704 | Party-State nach Leave |
| `PartyDisband` | 705 | Komplette Auflösung |
| `PartyLeaderChange` | 706 | Leader-Reassignment |

### Flow-Diagramm

```
Player B (Leaving)        Server         Party Members A,C,D
      │                      │                    │
      │  PartyLeave()        │                    │
      │─────────────────────►│                    │
      │                      │  Remove B          │
      │                      │  Reassign Leader?  │
      │                      │                    │
      │  Success             │                    │
      │◄─────────────────────│                    │
      │                      │  PartyUpdate(704)  │
      │                      │───────────────────►│
      │                      │  "B left party"    │
```

### Beispiel Payload

```csharp
var leave = new PartyLeaveRequest
{
    Type = MessageType.PartyLeave
};
```

### Error Codes

| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `NOT_IN_PARTY` | Nicht in einer Party | Fehler sollte nicht auftreten |
| `CANNOT_LEAVE_IN_COMBAT` | Im Kampf | Aus Kampf gehen, dann leave |
| `DUNGEON_ACTIVE` | Dungeon-Run aktiv | Dungeon erst abschließen/verlassen |

### Notizen

- **Combat**: Kann im Kampf nicht verlassen werden (Anti-Exploit)
- **Loot-Rights**: Bleiben 2 Minuten für bereits getötete Mobs
- **Quest-Shares**: Werden sofort entfernt, kein weiterer Progress
- **Leader**: Wird automatisch an ältestes Member weitergegeben
- **Auto-Disband**: Party wird aufgelöst bei <2 Membern
- **Dungeon**: Wird aus Dungeon teleportiert (zu letztem Hearthstone-Point)

---

## PartyKick (703)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

### Beschreibung

Der Party-Leader entfernt einen Spieler aus der Party. Funktioniert nur für den Leader und kann nicht im Kampf ausgeführt werden. Der gekickte Spieler erhält eine Notification und wird sofort aus der Party entfernt.

Anders als bei Leave gibt es einen "Kick-Cooldown" von 5 Minuten: Der gekickte Spieler kann in dieser Zeit nicht erneut von demselben Leader eingeladen werden (Anti-Griefing).

### Im Scope ✅

- Leader kickt Party-Member
- Kick-Cooldown (5 Min) für Re-Invite
- Combat-Protection (kein Kick im Kampf)
- Notification an gekickten Spieler und alle Member
- Cleanup wie bei Leave

### Nicht im Scope ❌

- Nicht-Leader können nicht kicken → verwende Vote-Kick (nicht implementiert)
- Leave → verwende stattdessen `PartyLeave` (702)

### Request Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| TargetPlayerId | int | ID des zu kickenden Spielers | Ja |
| Reason | string | Optional: Kick-Grund (Max 100 Zeichen) | Nein |

### Erwartete Response

- **Bei Erfolg:** `PartyUpdate` (704) an alle verbleibenden Member
- **Bei Fehler:** `ErrorMessage` (910) mit Code

### Verwandte Messages

| Message | ID | Beziehung |
|---------|-----|-----------|
| `PartyLeave` | 702 | Freiwilliges Verlassen |
| `PartyUpdate` | 704 | Party-State nach Kick |
| `PartyLeaderChange` | 706 | Leader kann nicht gekickt werden |

### Flow-Diagramm

```
Leader (A)               Server            Member B (Kicked)
   │                        │                      │
   │  PartyKick(B)          │                      │
   │───────────────────────►│                      │
   │                        │  Validate: A=Leader  │
   │                        │  Remove B            │
   │                        │                      │
   │  Success               │  Kick Notification   │
   │◄───────────────────────│─────────────────────►│
   │                        │  "Kicked from party" │
   │  PartyUpdate(704)      │                      │
   │◄───────────────────────│                      │
```

### Beispiel Payload

```csharp
var kick = new PartyKickRequest
{
    Type = MessageType.PartyKick,
    TargetPlayerId = 54321,
    Reason = "AFK too long"
};
```

### Error Codes

| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `NOT_PARTY_LEADER` | Nur Leader kann kicken | Kein Kick erlaubt |
| `PLAYER_NOT_IN_PARTY` | Target nicht in Party | Falsche ID |
| `CANNOT_KICK_IN_COMBAT` | Party im Kampf | Aus Kampf gehen |
| `CANNOT_KICK_SELF` | Leader kann sich nicht selbst kicken | Disband oder Leader übergeben |

### Notizen

- **Leader-Only**: Nur Party-Leader kann kicken
- **Combat-Protection**: Kein Kick während Kampf (Anti-Griefing)
- **Cooldown**: 5-Minuten Cooldown für Re-Invite desselben Spielers
- **Reason**: Optional, wird in Kick-Notification angezeigt
- **Auto-Disband**: Wie bei Leave, wenn <2 Member übrig
- **Loot**: Wie bei Leave, Loot-Rights für 2 Min

---

## PartyUpdate (704)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** Nein (Server→Client)

### Beschreibung

Der Server broadcastet alle allgemeinen Party-State-Änderungen an alle Member: Member-Join, Member-Leave, Loot-Settings, Leader-Change, etc. Dies ist die zentrale "Party hat sich verändert"-Message.

Dies ist NICHT für hochfrequente Updates wie Position/HP (dafür gibt es spezialisierte Messages). PartyUpdate wird nur bei strukturellen Änderungen gesendet.

### Im Scope ✅

- Member Join/Leave-Events
- Leader-Änderungen
- Loot-Setting-Änderungen
- Party-zu-Raid-Konvertierung
- Member-Count-Updates

### Nicht im Scope ❌

- HP/Mana-Updates → verwende `PartyHealthUpdate` (712) / `PartyResourceUpdate` (713)
- Position-Updates → verwende `PartyPositionUpdate` (711)
- Buff-Updates → verwende `PartyBuffUpdate` (714)

### Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| EventType | enum | Join, Leave, LeaderChange, LootChange, etc. | Ja |
| PlayerId | int | Betroffener Spieler (bei Join/Leave) | Nein |
| NewLeaderId | int | Neuer Leader (bei LeaderChange) | Nein |
| LootSetting | enum | Neue Loot-Einstellung | Nein |
| MemberCount | int | Aktuelle Anzahl Member | Ja |
| Members | List<PartyMember> | Vollständige Member-Liste (bei Join) | Nein |

### Verwandte Messages

| Message | ID | Beziehung |
|---------|-----|-----------|
| `PartySync` | 719 | Vollständiger Party-State |
| `PartyMemberUpdate` | 710 | Member-spezifische Updates |
| `PartyLeaderChange` | 706 | Spezielle Leader-Change-Message |

### Beispiel Payload

```csharp
// Member joined
var update = new PartyUpdateBroadcast
{
    Type = MessageType.PartyUpdate,
    EventType = PartyEventType.MemberJoined,
    PlayerId = 12345,
    MemberCount = 3,
    Members = new List<PartyMember> { ... }
};

// Loot changed
var lootUpdate = new PartyUpdateBroadcast
{
    Type = MessageType.PartyUpdate,
    EventType = PartyEventType.LootSettingChanged,
    LootSetting = LootSetting.NeedBeforeGreed,
    MemberCount = 3
};
```

### Notizen

- **Broadcast**: Geht an ALLE Party-Member
- **Frequency**: Nur bei strukturellen Änderungen (Low-Frequency)
- **Full Sync**: Bei Member-Join enthält vollständige Member-Liste
- **Incremental**: Bei anderen Events nur die Änderung
- **UI**: Client aktualisiert Party-Frame basierend auf diesem Event

---

## PartyDisband (705)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

### Beschreibung

Der Party-Leader löst die gesamte Party auf. Alle Member werden entfernt und erhalten eine Notification. Alle Party-spezifischen Daten (Loot-Settings, Quest-Shares, etc.) werden gelöscht.

Dies ist eine "atomare" Operation - entweder wird die gesamte Party aufgelöst oder garnichts passiert (wenn Fehler auftritt).

### Im Scope ✅

- Komplette Party-Auflösung durch Leader
- Notification an alle Member
- Cleanup aller Party-Daten
- Combat-Protection

### Nicht im Scope ❌

- Einzelne Member entfernen → verwende `PartyKick` (703)
- Auto-Disband (bei <2 Member) → geschieht automatisch bei Leave

### Request Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| - | - | Keine zusätzlichen Felder | - |

### Erwartete Response

- **Bei Erfolg:** `PartyUpdate` (704) an alle Member mit EventType=Disbanded
- **Bei Fehler:** `ErrorMessage` (910) mit Code

### Verwandte Messages

| Message | ID | Beziehung |
|---------|-----|-----------|
| `PartyLeave` | 702 | Einzelner Member verlässt |
| `PartyUpdate` | 704 | Disband-Notification |

### Flow-Diagramm

```
Leader                  Server          All Party Members
   │                       │                    │
   │  PartyDisband()       │                    │
   │──────────────────────►│                    │
   │                       │  Validate: Leader  │
   │                       │  Delete Party      │
   │                       │                    │
   │  Success              │  PartyUpdate(704)  │
   │◄──────────────────────│───────────────────►│
   │                       │  "Party disbanded" │
```

### Beispiel Payload

```csharp
var disband = new PartyDisbandRequest
{
    Type = MessageType.PartyDisband
};
```

### Error Codes

| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `NOT_PARTY_LEADER` | Nur Leader kann disbanden | Kein Disband erlaubt |
| `NOT_IN_PARTY` | Nicht in Party | Sollte nicht auftreten |
| `CANNOT_DISBAND_IN_COMBAT` | Party im Kampf | Aus Kampf gehen |
| `DUNGEON_ACTIVE` | In aktivem Dungeon | Dungeon erst abschließen |

### Notizen

- **Leader-Only**: Nur Party-Leader kann disbanden
- **Combat**: Kein Disband im Kampf möglich
- **Atomic**: Entweder alle entfernt oder Fehler
- **Cleanup**: Alle Party-Daten werden sofort gelöscht
- **Loot**: Bestehende Loot-Rights bleiben 2 Min

---

## PartyLeaderChange (706)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

### Beschreibung

Der aktuelle Party-Leader überträgt die Leadership an ein anderes Party-Member. Der neue Leader erhält alle Leader-Rechte (Kick, Disband, Loot-Settings, Invites). Der alte Leader wird normales Member.

Der neue Leader muss online und in derselben Zone sein. Im Kampf ist kein Leader-Change erlaubt (Anti-Exploit).

### Im Scope ✅

- Leader-Transfer an anderes Member
- Validierung: Target muss in Party sein
- Validierung: Target muss online sein
- Combat-Protection
- Notification an alle Member

### Nicht im Scope ❌

- Auto-Leader-Assignment (bei Leave) → geschieht automatisch
- Leader-Vote → verwende Vote-System (nicht implementiert)

### Request Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| NewLeaderId | int | ID des neuen Leaders | Ja |

### Erwartete Response

- **Bei Erfolg:** `PartyUpdate` (704) mit EventType=LeaderChanged
- **Bei Fehler:** `ErrorMessage` (910) mit Code

### Verwandte Messages

| Message | ID | Beziehung |
|---------|-----|-----------|
| `PartyUpdate` | 704 | Leader-Change-Notification |
| `PartyKick` | 703 | Leader-Rechte |
| `PartyDisband` | 705 | Leader-Rechte |

### Flow-Diagramm

```
Old Leader (A)          Server          New Leader (B)
      │                    │                   │
      │  LeaderChange(B)   │                   │
      │───────────────────►│                   │
      │                    │  Validate         │
      │                    │  Set B as Leader  │
      │                    │                   │
      │  PartyUpdate(704)  │                   │
      │◄───────────────────│                   │
      │                    │  PartyUpdate(704) │
      │                    │──────────────────►│
      │                    │  "You are Leader" │
```

### Beispiel Payload

```csharp
var change = new PartyLeaderChangeRequest
{
    Type = MessageType.PartyLeaderChange,
    NewLeaderId = 54321
};
```

### Error Codes

| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `NOT_PARTY_LEADER` | Nur aktueller Leader kann übertragen | Kein Change erlaubt |
| `PLAYER_NOT_IN_PARTY` | Target nicht in Party | Falsche ID |
| `PLAYER_OFFLINE` | Target offline | Anderen Spieler wählen |
| `CANNOT_CHANGE_IN_COMBAT` | Party im Kampf | Aus Kampf gehen |
| `INVALID_ZONE` | Target in anderer Instanz | Gleiche Zone erforderlich |

### Notizen

- **Leader-Only**: Nur aktueller Leader kann übertragen
- **Combat**: Kein Leader-Change im Kampf
- **Online**: Target muss online und verbunden sein
- **Zone**: Target muss in derselben oder kompatibler Zone sein
- **UI**: Leader-Icon wechselt im Party-Frame
- **Permissions**: Neue Leader erhält sofort alle Rechte

---

## PartyLootChange (707)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

### Beschreibung

Der Party-Leader ändert die Loot-Distribution-Einstellung für die Party. Vier Modi sind verfügbar: Free-for-All, Round-Robin, Master-Looter und Need-before-Greed.

Die Änderung wird sofort aktiv und gilt für alle neu gespawnten Loot (bereits existierender Loot behält alte Einstellung). Alle Member erhalten eine Notification über die Änderung.

### Im Scope ✅

- Änderung der Loot-Verteilung durch Leader
- Vier Loot-Modi: FFA, RR, ML, NBG
- Sofortige Aktivierung für neuen Loot
- Notification an alle Member

### Nicht im Scope ❌

- Item-spezifische Loot-Regeln → verwende Loot-Messages (31xx)
- Personal-Loot-Mode → noch nicht implementiert (Phase 3)

### Request Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| LootSetting | enum | Neues Loot-Setting | Ja |
| MasterLooterId | int | Bei ML: Der Master-Looter (default: Leader) | Nein |

**LootSetting Enum:**
- `FreeForAll` (0) - Jeder kann alles looten
- `RoundRobin` (1) - Reihum, jeder bekommt ein Item
- `MasterLooter` (2) - Nur Master-Looter verteilt
- `NeedBeforeGreed` (3) - Need/Greed/Pass-System

### Erwartete Response

- **Bei Erfolg:** `PartyUpdate` (704) mit EventType=LootSettingChanged
- **Bei Fehler:** `ErrorMessage` (910) mit Code

### Verwandte Messages

| Message | ID | Beziehung |
|---------|-----|-----------|
| `PartyUpdate` | 704 | Loot-Setting-Notification |
| Loot-Messages | 31xx | Loot-Distribution |

### Flow-Diagramm

```
Leader                  Server          All Party Members
   │                       │                    │
   │  LootChange(NBG)      │                    │
   │──────────────────────►│                    │
   │                       │  Validate: Leader  │
   │                       │  Update Setting    │
   │                       │                    │
   │  PartyUpdate(704)     │                    │
   │◄──────────────────────│───────────────────►│
   │                       │  "Loot: NBG"       │
```

### Beispiel Payload

```csharp
// Need-before-Greed
var change = new PartyLootChangeRequest
{
    Type = MessageType.PartyLootChange,
    LootSetting = LootSetting.NeedBeforeGreed
};

// Master Looter (Leader as ML)
var ml = new PartyLootChangeRequest
{
    Type = MessageType.PartyLootChange,
    LootSetting = LootSetting.MasterLooter,
    MasterLooterId = 12345 // Leader's ID
};
```

### Error Codes

| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `NOT_PARTY_LEADER` | Nur Leader kann ändern | Kein Change erlaubt |
| `INVALID_LOOT_SETTING` | Unbekanntes Setting | Enum-Wert prüfen |
| `MASTER_LOOTER_NOT_IN_PARTY` | ML nicht in Party | Anderen ML wählen |
| `CANNOT_CHANGE_IN_COMBAT` | Party im Kampf | Aus Kampf gehen |

### Notizen

- **Leader-Only**: Nur Party-Leader kann Loot-Setting ändern
- **Immediate**: Gilt sofort für neuen Loot
- **Existing Loot**: Behält alte Einstellung
- **Master-Looter**: Kann auch normales Member sein (nicht nur Leader)
- **NBG**: Need-before-Greed ist empfohlenes Setting für Dungeons
- **FFA**: Nur für Open-World empfohlen

---

## PartyReadyCheck (708)

**Richtung:** 📤 Client → Server  
**Frequenz:** Gelegentlich  
**Authentifizierung:** 🔒 Ja

### Beschreibung

Der Party-Leader initiiert einen Ready-Check: Alle Member müssen innerhalb von 30 Sekunden mit Yes/No/AFK antworten. Dies wird vor Dungeon-Pulls, Boss-Pulls oder wichtigen Aktivitäten verwendet.

Der Server sendet an alle Member eine Ready-Check-Anfrage und sammelt die Antworten. Nach 30s oder wenn alle geantwortet haben, wird das Ergebnis gebroadcastet.

### Im Scope ✅

- Leader startet Ready-Check
- 30-Sekunden-Timeout
- Drei mögliche Antworten: Yes, No, AFK
- Auto-"No" bei Timeout
- Ergebnis-Broadcast an alle

### Nicht im Scope ❌

- Role-Check → verwende `PartyRoleCheck` (717)
- Equipment-Check → verwende Inspection-Messages (33xx)

### Request Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Message | string | Optional: Nachricht (z.B. "Boss Pull") | Nein |

### Erwartete Response

- **Bei Erfolg:** Ready-Check-Request an alle Member + Collection läuft
- **Nach 30s oder alle Responses:** Ready-Check-Result-Broadcast
- **Bei Fehler:** `ErrorMessage` (910) mit Code

### Verwandte Messages

| Message | ID | Beziehung |
|---------|-----|-----------|
| `PartyReadyResponse` | 709 | Member antwortet auf Check |
| `PartyRoleCheck` | 717 | Prüfung der Rollen-Assignments |

### Flow-Diagramm

```
Leader              Server          Members A,B,C
   │                   │                  │
   │  ReadyCheck()     │                  │
   │──────────────────►│                  │
   │                   │  RC Request      │
   │                   │─────────────────►│
   │                   │                  │
   │                   │◄─ Yes/No/AFK ────│
   │                   │                  │
   │  (30s timeout or all responded)     │
   │                   │                  │
   │  RC Result        │                  │
   │◄──────────────────│─────────────────►│
   │  "3 Yes, 0 No"    │                  │
```

### Beispiel Payload

```csharp
var readyCheck = new PartyReadyCheckRequest
{
    Type = MessageType.PartyReadyCheck,
    Message = "Boss pull in 10 seconds"
};
```

### Error Codes

| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `NOT_PARTY_LEADER` | Nur Leader kann Ready-Check starten | Kein Check erlaubt |
| `READY_CHECK_ACTIVE` | Ready-Check bereits aktiv | Warten bis abgeschlossen |
| `PARTY_TOO_SMALL` | <2 Member | Mehr Member einladen |

### Notizen

- **Leader-Only**: Nur Party-Leader kann Ready-Check starten
- **Timeout**: 30 Sekunden, dann Auto-"No"
- **UI**: Popup mit Yes/No/AFK Buttons
- **Sound**: Akustisches Signal bei Ready-Check
- **Cooldown**: 1 Minute Cooldown zwischen Ready-Checks
- **Result**: Zeigt Anzahl Yes/No/AFK an
- **Common Usage**: Vor Boss-Pulls, Dungeon-Start, PvP-Match

---

## PartyReadyResponse (709)

**Richtung:** 📤 Client → Server  
**Frequenz:** Gelegentlich  
**Authentifizierung:** 🔒 Ja

### Beschreibung

Ein Party-Member antwortet auf einen aktiven Ready-Check mit Yes, No oder AFK. Der Server sammelt alle Responses und broadcastet das Ergebnis, sobald alle geantwortet haben oder der Timeout (30s) abläuft.

### Im Scope ✅

- Response auf aktiven Ready-Check
- Drei Optionen: Yes, No, AFK
- Auto-"No" bei Timeout
- Response-Count für Leader

### Nicht im Scope ❌

- Response ohne aktiven Check → Fehler
- Mehrfache Responses → nur erste zählt

### Request Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Response | enum | Yes, No, AFK | Ja |

**Response Enum:**
- `Yes` (0) - Bereit
- `No` (1) - Nicht bereit
- `AFK` (2) - Away from keyboard

### Erwartete Response

- **Bei Erfolg:** Response akzeptiert, ggf. Result-Broadcast
- **Bei Fehler:** `ErrorMessage` (910) mit Code

### Verwandte Messages

| Message | ID | Beziehung |
|---------|-----|-----------|
| `PartyReadyCheck` | 708 | Ursprünglicher Check |
| `PartyUpdate` | 704 | Result-Broadcast |

### Flow-Diagramm

```
Member                 Server              Leader
   │                      │                   │
   │  ReadyResponse(Yes)  │                   │
   │─────────────────────►│                   │
   │                      │  Collect          │
   │  Response OK         │                   │
   │◄─────────────────────│                   │
   │                      │                   │
   │  (All responded or 30s timeout)         │
   │                      │                   │
   │  RC Result           │  RC Result        │
   │◄─────────────────────│──────────────────►│
```

### Beispiel Payload

```csharp
// Ready
var response = new PartyReadyResponse
{
    Type = MessageType.PartyReadyResponse,
    Response = ReadyResponse.Yes
};

// Not ready
var notReady = new PartyReadyResponse
{
    Type = MessageType.PartyReadyResponse,
    Response = ReadyResponse.No
};

// AFK
var afk = new PartyReadyResponse
{
    Type = MessageType.PartyReadyResponse,
    Response = ReadyResponse.AFK
};
```

### Error Codes

| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `NO_READY_CHECK_ACTIVE` | Kein aktiver Ready-Check | Warten auf neuen Check |
| `ALREADY_RESPONDED` | Bereits geantwortet | Erste Antwort zählt |
| `READY_CHECK_EXPIRED` | Check bereits abgelaufen | Zu spät |

### Notizen

- **Timeout**: Auto-"No" nach 30s
- **First Response Wins**: Kann nicht geändert werden
- **UI**: Buttons verschwinden nach Response
- **Broadcast**: Leader sieht Live-Counter der Responses
- **AFK**: AFK-Status wird auch im Party-Frame angezeigt

---

## PartyMemberUpdate (710)

**Richtung:** 📥 Server → Client (Broadcast zu allen Members)  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

### Beschreibung

Der Server informiert alle Party-Mitglieder über allgemeine Änderungen eines Members (Level, Klasse, Zone, Online-Status). Diese Message wird gesendet, wenn sich fundamentale Charaktereigenschaften ändern, die andere Mitglieder sehen sollten.

Im Gegensatz zu spezifischen Updates (Health, Position, Buffs) enthält diese Message statische oder selten ändernde Informationen. Sie wird typischerweise beim Zonen-Wechsel, Level-Up oder nach Reconnect gesendet.

### Im Scope ✅

- Level-Änderungen (Level-Up)
- Klassen/Spezialisierung-Wechsel
- Zonen-Wechsel
- Online/Offline-Status
- Name-Änderungen

### Nicht im Scope ❌

- Health/Resource-Änderungen → verwende `PartyHealthUpdate` (712) oder `PartyResourceUpdate` (713)
- Position-Updates → verwende `PartyPositionUpdate` (711)
- Buff-Änderungen → verwende `PartyBuffUpdate` (714)

### Request/Response Payload

**Server → Client**:

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| `PlayerId` | uint32 | ID des aktualisierten Members | Ja |
| `Level` | byte | Aktuelles Level (1-60) | Ja |
| `Class` | ClassType | Klasse des Members | Ja |
| `Specialization` | byte | Spezialisierung (0-2) | Nein |
| `ZoneId` | uint16 | Aktuelle Zone-ID | Ja |
| `IsOnline` | bool | Ist Member online? | Ja |
| `Name` | string | Aktueller Name | Ja |

### Erwartete Response

- **Keine direkte Response** - Broadcast-Message

### Verwandte Messages

| Message | ID | Beziehung |
|---------|-----|-----------|
| `PartyHealthUpdate` | 712 | Health/Resource-Updates |
| `PartyPositionUpdate` | 711 | Positions-Updates |
| `PartyUpdate` | 704 | General Party-Updates |

### Flow-Diagramm

```
Server                    All Party Members
  │                              │
  │  Member Level-Up / Zone     │
  │                              │
  │  PartyMemberUpdate (710)    │
  │─────────────────────────────►│
  │                              │
  │  (UI aktualisiert)           │
```

### Beispiel Payload

```csharp
var msg = new PartyMemberUpdate
{
    Type = MessageType.PartyMemberUpdate,
    PlayerId = 12345,
    Level = 25,
    Class = ClassType.Warrior,
    Specialization = 1, // Protection
    ZoneId = 101,
    IsOnline = true,
    Name = "Thorgar"
};
```

### Notizen

- **Throttling**: Max 1 Update pro Member pro 5s
- **Zone-Change**: Immer gesendet bei Zone-Wechsel
- **Offline**: Markiert Member als offline (grau im UI)
- **Level-Up**: Trigger Party-Gratulations-Message

---

## PartyPositionUpdate (711)

**Richtung:** 📥 Server → Client (Broadcast zu allen Members)  
**Frequenz:** ⚡ High-Frequency  
**Authentifizierung:** 🔒 Ja

### Beschreibung

Der Server broadcastet die Position eines Party-Members an alle anderen Members für die Minimap-Darstellung. Diese Message wird mit ~1 Hz gesendet (jede Sekunde), damit Party-Mitglieder auf der Minimap sichtbar bleiben.

Positions-Updates werden nur gesendet, wenn sich Members in der gleichen Zone befinden. Cross-Zone-Members zeigen keinen Marker auf der Minimap (nur Zone-Name im Party-Frame).

### Im Scope ✅

- Real-time Position für Minimap
- 1 Hz Update-Rate (jede Sekunde)
- Nur innerhalb gleicher Zone
- Höheninformation für 3D-Zonen

### Nicht im Scope ❌

- Exakte Position für Movement-Sync → verwende `PositionBroadcast` (201)
- Cross-Zone-Tracking → nicht implementiert (Privacy/Anti-Stalking)

### Request/Response Payload

**Server → Client**:

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| `PlayerId` | uint32 | ID des Members | Ja |
| `X` | float | X-Koordinate | Ja |
| `Y` | float | Y-Koordinate | Ja |
| `Z` | float | Höhe (für 3D-Minimap) | Nein |
| `ZoneId` | uint16 | Zone-ID zur Validierung | Ja |

### Erwartete Response

- **Keine direkte Response** - Broadcast-Message

### Verwandte Messages

| Message | ID | Beziehung |
|---------|-----|-----------|
| `PositionBroadcast` | 201 | Exakte Entity-Position |
| `PartyMemberUpdate` | 710 | Zone-Change-Info |

### Flow-Diagramm

```
Server (1 Hz)             All Party Members (Same Zone)
  │                              │
  │  Member bewegt sich          │
  │                              │
  │  PartyPositionUpdate (711)  │
  │─────────────────────────────►│
  │                              │
  │  (Minimap-Marker update)     │
```

### Beispiel Payload

```csharp
var msg = new PartyPositionUpdate
{
    Type = MessageType.PartyPositionUpdate,
    PlayerId = 12345,
    X = 1523.45f,
    Y = 892.12f,
    Z = 105.5f,
    ZoneId = 101
};
```

### Notizen

- **Update-Rate**: 1 Hz (jede Sekunde)
- **Same-Zone-Only**: Keine Cross-Zone-Updates
- **Minimap-Only**: Nicht für exakte Position-Sync
- **3D-Support**: Z-Koordinate für Höhenstufen
- **Bandwidth**: ~20 bytes × 4 Members × 1 Hz = 80 bytes/s

---

## PartyHealthUpdate (712)

**Richtung:** 📥 Server → Client (Broadcast)  
**Frequenz:** Häufig (bei Änderung, ~1s in Combat)  
**Authentifizierung:** 🔒 Ja

### Beschreibung

Der Server broadcastet Health und Resource-Prozentwerte eines Party-Members an alle anderen Members. Diese Message ermöglicht Healern und Tanks, den Zustand ihrer Gruppe zu überwachen. Updates werden on-change gesendet mit Throttling von ~1s im Kampf.

Out-of-Combat werden Updates seltener gesendet (nur bei signifikanten Änderungen >5%). In-Combat erfolgen Updates häufiger für besseres Healing-Timing.

### Im Scope ✅

- Health-Prozentsatz (0-100%)
- Primary-Resource-Prozentsatz (Mana, Energy, Rage, etc.)
- Max-Health für absolute Werte
- Shield/Absorb-Menge

### Nicht im Scope ❌

- Detaillierte Stat-Info → verwende `CharacterInfo` (606)
- Regen-Ticks → implizit durch Änderungen

### Request/Response Payload

**Server → Client**:

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| `PlayerId` | uint32 | ID des Members | Ja |
| `HealthPercent` | byte | Health 0-100% | Ja |
| `ResourcePercent` | byte | Resource 0-100% | Ja |
| `MaxHealth` | uint32 | Aktuelles MaxHealth | Ja |
| `MaxResource` | uint32 | Aktuelles MaxResource | Ja |
| `AbsorbAmount` | uint32 | Shield/Absorb-Wert | Nein |

### Erwartete Response

- **Keine direkte Response** - Broadcast-Message

### Verwandte Messages

| Message | ID | Beziehung |
|---------|-----|-----------|
| `PartyResourceUpdate` | 713 | Separate Resource-Updates |
| `PartyBuffUpdate` | 714 | Buff-Info (Shields, HoTs) |
| `ResourceUpdate` | 604 | Eigene Resource-Updates |

### Flow-Diagramm

```
Server                    All Party Members
  │                              │
  │  Member nimmt Damage         │
  │                              │
  │  PartyHealthUpdate (712)     │
  │─────────────────────────────►│
  │                              │
  │  (Health-Bar aktualisiert)   │
```

### Beispiel Payload

```csharp
var msg = new PartyHealthUpdate
{
    Type = MessageType.PartyHealthUpdate,
    PlayerId = 12345,
    HealthPercent = 45,
    ResourcePercent = 80,
    MaxHealth = 5000,
    MaxResource = 2000,
    AbsorbAmount = 500
};
```

### Notizen

- **Throttling**: ~1s in Combat, ~5s out-of-Combat
- **Change-Threshold**: >5% für Out-of-Combat-Updates
- **Absorb-Shields**: Separate Anzeige in UI
- **Color-Coding**: <25% rot, <50% gelb, >=50% grün
- **Healer-Prio**: Sortierung nach niedrigstem Health%

---

## PartyResourceUpdate (713)

**Richtung:** 📥 Server → Client (Broadcast)  
**Frequenz:** Häufig (bei signifikanten Änderungen)  
**Authentifizierung:** 🔒 Ja

### Beschreibung

Separate Resource-Updates für Power-Types die schnell wechseln (Energy, Rage). Während Mana in `PartyHealthUpdate` (712) enthalten ist, benötigen Energy-Klassen (Rogues) und Rage-Klassen (Warriors) häufigere Updates.

Diese Message wird nur für Energy/Rage gesendet, da Mana-Änderungen langsamer sind. Update-Rate ist resource-type-abhängig.

### Im Scope ✅

- Energy-Updates (Rogues, Druids-Cat)
- Rage-Updates (Warriors, Druids-Bear)
- Runic Power (Death Knights, Phase 2)
- Fast-Regenerating Resources

### Nicht im Scope ❌

- Mana-Updates → enthalten in `PartyHealthUpdate` (712)
- Health-Updates → verwende `PartyHealthUpdate` (712)

### Request/Response Payload

**Server → Client**:

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| `PlayerId` | uint32 | ID des Members | Ja |
| `ResourceType` | ResourceType | Energy/Rage/RunicPower | Ja |
| `Amount` | uint16 | Aktueller Wert | Ja |
| `MaxAmount` | uint16 | Maximum | Ja |

### Erwartete Response

- **Keine direkte Response** - Broadcast-Message

### Beispiel Payload

```csharp
var msg = new PartyResourceUpdate
{
    Type = MessageType.PartyResourceUpdate,
    PlayerId = 12345,
    ResourceType = ResourceType.Energy,
    Amount = 85,
    MaxAmount = 100
};
```

### Notizen

- **Energy**: 10 Energie/s Regen → Update bei ±10
- **Rage**: Gain on Damage → Update on change
- **Mana**: Enthalten in PartyHealthUpdate
- **Bandwidth**: Nur für Energy/Rage-Klassen

---

## PartyBuffUpdate (714)

**Richtung:** 📥 Server → Client (Broadcast)  
**Frequenz:** Selten (bei Buff-Änderungen)  
**Authentifizierung:** 🔒 Ja

### Beschreibung

Der Server broadcastet aktive Buffs und Debuffs eines Party-Members. Diese Information ist wichtig für Raid-Koordination (z.B. "Wer hat welchen Buff?") und für Healer (Dispellable Debuffs).

Nur wichtige Buffs werden übertragen (max 10 pro Member), priorisiert nach: Party-Buffs > Raid-Buffs > Long-Duration-Buffs > Debuffs.

### Im Scope ✅

- Top 10 aktive Buffs pro Member
- Buff-Icon-ID und Verbleibende Zeit
- Dispellable-Flag für Debuffs
- Stack-Count für stapelbare Buffs

### Nicht im Scope ❌

- Alle Buffs (nur Top 10) → Vollständige Liste via `CharacterInfo` (606)
- Buff-Tooltips → Client hat lokale Daten

### Request/Response Payload

**Server → Client**:

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| `PlayerId` | uint32 | ID des Members | Ja |
| `Buffs` | BuffInfo[] | Array von bis zu 10 Buffs | Ja |

**BuffInfo**:

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| `BuffId` | uint16 | Buff-ID | Ja |
| `Duration` | uint32 | Verbleibende Zeit in ms | Ja |
| `Stacks` | byte | Stack-Count (1-255) | Nein |
| `IsDispellable` | bool | Kann dispelled werden? | Ja |
| `DispelType` | DispelType | Magic/Poison/Disease/Curse | Nein |

### Erwartete Response

- **Keine direkte Response** - Broadcast-Message

### Beispiel Payload

```csharp
var msg = new PartyBuffUpdate
{
    Type = MessageType.PartyBuffUpdate,
    PlayerId = 12345,
    Buffs = new BuffInfo[]
    {
        new BuffInfo { BuffId = 501, Duration = 300000, Stacks = 1 },
        new BuffInfo { BuffId = 702, Duration = 15000, Stacks = 5 },
        new BuffInfo { BuffId = 920, Duration = 8000, Stacks = 1, IsDispellable = true, DispelType = DispelType.Magic }
    }
};
```

### Notizen

- **Max 10 Buffs**: Priorisiert nach Wichtigkeit
- **Update on Change**: Buff applied/removed/expired
- **Dispel-Highlighting**: Debuffs mit IsDispellable=true hervorheben
- **Tooltip**: Client hat lokale Buff-Datenbank

---

## PartyTargetUpdate (715)

**Richtung:** 📥 Server → Client (Broadcast)  
**Frequenz:** Häufig (bei Target-Wechsel)  
**Authentifizierung:** 🔒 Ja

### Beschreibung

Broadcastet das aktuelle Target eines Party-Members an alle anderen Members. Ermöglicht Focus-Fire-Koordination und zeigt "Wer greift was an?" im Party-Frame.

Target-Updates werden nur gesendet wenn sich das Target ändert (nicht kontinuierlich). NPCs und Spieler können Targets sein.

### Im Scope ✅

- Target-EntityId (NPC oder Player)
- Target-Name für UI-Anzeige
- Target-Health-Percent
- Target-Type (Friendly/Hostile/Neutral)

### Nicht im Scope ❌

- Detaillierte Target-Info → verwende `EntityInfo` separate Request
- Target-of-Target → nicht implementiert

### Request/Response Payload

**Server → Client**:

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| `PlayerId` | uint32 | Member der target hat | Ja |
| `TargetId` | uint32 | EntityId des Targets (0=kein Target) | Ja |
| `TargetName` | string | Name des Targets | Nein |
| `TargetHealthPercent` | byte | Health 0-100% | Nein |
| `TargetType` | TargetType | Friendly/Hostile/Neutral | Ja |

### Beispiel Payload

```csharp
var msg = new PartyTargetUpdate
{
    Type = MessageType.PartyTargetUpdate,
    PlayerId = 12345,
    TargetId = 99887,
    TargetName = "Gnoll Berserker",
    TargetHealthPercent = 75,
    TargetType = TargetType.Hostile
};
```

### Notizen

- **Update on Change**: Nur bei Target-Wechsel
- **Clear Target**: TargetId=0 für "kein Target"
- **Focus-Fire**: UI kann gleiche Targets highlighten
- **Combat-Only**: Meist nur in Combat relevant

---

## PartyRoleSet (716)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

### Beschreibung

Ein Spieler setzt seine bevorzugte Rolle für Dungeon-Finder und Party-Organisation. Rollen sind: Tank, Healer, DPS. Ein Spieler kann mehrere Rollen wählen (z.B. "Tank oder DPS").

Die Rolle beeinflusst Dungeon-Finder-Matchmaking und Ready-Checks. Role-Changes erfordern Out-of-Combat-Status.

### Im Scope ✅

- Setzen einer oder mehrerer Rollen
- Tank/Healer/DPS-Flags
- Validierung gegen Klasse/Spec
- Broadcast an Party

### Nicht im Scope ❌

- Erzwingen einer Rolle → Leader kann nur Empfehlungen geben
- Auto-Role-Detection → Spieler wählt selbst

### Request/Response Payload

**Client → Server**:

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| `Roles` | RoleFlags | Tank/Healer/DPS-Flags | Ja |

**RoleFlags** (Bitflags):
- `Tank = 0x01`
- `Healer = 0x02`
- `DPS = 0x04`

**Server → Client**:

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| `Success` | bool | Role-Set erfolgreich? | Ja |

**Broadcast (PartyUpdate 704)**:

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| `PlayerId` | uint32 | Member der Role geändert hat | Ja |
| `Roles` | RoleFlags | Neue Rollen | Ja |

### Erwartete Response

- **Bei Erfolg**: `PartyRoleSet` Response + `PartyUpdate` (704) Broadcast
- **Bei Fehler**: `ErrorMessage` (910) mit Code `IN_COMBAT` oder `INVALID_ROLE_FOR_CLASS`

### Beispiel Payload

```csharp
// Client setzt Rolle "Tank oder DPS"
var request = new PartyRoleSet
{
    Type = MessageType.PartyRoleSet,
    Roles = RoleFlags.Tank | RoleFlags.DPS
};
```

### Error Codes

| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `IN_COMBAT` | Im Kampf | Warten bis Out-of-Combat |
| `INVALID_ROLE_FOR_CLASS` | Klasse kann Rolle nicht | Spec wechseln |
| `NOT_IN_PARTY` | Nicht in Party | Party beitreten |

### Notizen

- **Multi-Role**: Spieler kann mehrere wählen (schnellere Queue)
- **Dungeon-Finder**: Verwendet diese Info für Matchmaking
- **Validation**: Priest kann nicht Tank, Warrior kann nicht Healer
- **Hybrid-Klassen**: Druids, Paladins können alle 3

---

## PartyRoleCheck (717)

**Richtung:** 📤 Client → Server (Leader initiiert)  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Party-Leader

### Beschreibung

Der Party-Leader initiiert einen Role-Check um sicherzustellen, dass alle Members ihre Rollen gesetzt haben bevor man in einen Dungeon geht. Jeder Member muss seine Rolle bestätigen oder ändern.

Role-Check hat 30s Timeout. Members die nicht antworten werden als "Not Ready" markiert. Dungeon-Finder verweigert Eintritt bis alle Rollen bestätigt sind.

### Im Scope ✅

- Leader initiiert Check
- Alle Members müssen Rolle bestätigen
- 30s Timeout
- UI zeigt offene Bestätigungen

### Nicht im Scope ❌

- Auto-Role-Assignment → Leader muss manuell zuweisen
- Erzwingen von Rollen → nur Empfehlungen

### Request/Response Payload

**Client (Leader) → Server**:

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| _(leer)_ | - | Leader initiiert | - |

**Server → All Members**:

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| `TimeoutSeconds` | byte | Countdown (30) | Ja |

**Member Response (via PartyRoleSet 716)**:

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| `Roles` | RoleFlags | Bestätigte Rollen | Ja |

### Erwartete Response

- **Bei Erfolg**: Broadcast `PartyRoleCheck` zu allen Members
- **Bei Fehler**: `ErrorMessage` (910) mit Code `NOT_LEADER` oder `ALREADY_IN_PROGRESS`

### Flow-Diagramm

```
Leader                    Server                    All Members
  │                          │                            │
  │  PartyRoleCheck (717)    │                            │
  │─────────────────────────►│                            │
  │                          │                            │
  │                          │  PartyRoleCheck (Broadcast)│
  │                          │───────────────────────────►│
  │                          │                            │
  │                          │  PartyRoleSet (716)        │
  │                          │◄───────────────────────────│
  │                          │                            │
  │  Role-Check-Summary      │                            │
  │◄─────────────────────────│                            │
```

### Beispiel Payload

```csharp
var request = new PartyRoleCheck
{
    Type = MessageType.PartyRoleCheck
};
```

### Error Codes

| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `NOT_LEADER` | Nur Leader kann Check starten | - |
| `ALREADY_IN_PROGRESS` | Check läuft bereits | Warten |
| `IN_DUNGEON` | Bereits im Dungeon | Zu spät |

### Notizen

- **Dungeon-Requirement**: Viele Dungeons erfordern Role-Check
- **Timeout**: Auto-Fail nach 30s
- **UI**: Großes Pop-up für jeden Member
- **Summary**: Leader sieht Live-Status aller Antworten

---

## PartyConvertToRaid (718)

**Richtung:** 📤 Client → Server  
**Frequenz:** Einmalig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Party-Leader

### Beschreibung

Konvertiert eine Party (max 5 Spieler) zu einem Raid (max 40 Spieler). Dies ist notwendig wenn mehr als 5 Spieler zusammen spielen wollen. Raid-Conversion ist permanent - eine Raid kann nicht zurück zu Party konvertiert werden.

Nach Conversion ändert sich das UI (Raid-Frames), Loot-Regeln werden komplexer, und der Leader kann Assistant-Leader ernennen.

### Im Scope ✅

- Conversion Party → Raid
- Erhaltung aller Members
- Erhaltung des Leaders
- UI-Umschaltung auf Raid-Frames

### Nicht im Scope ❌

- Raid → Party Conversion → nicht möglich
- Raid-Merging → Phase 3 Feature
- Cross-Server-Raids → Phase 3 Feature

### Request/Response Payload

**Client (Leader) → Server**:

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| _(leer)_ | - | Leader konvertiert | - |

**Server → Client**:

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| `Success` | bool | Conversion erfolgreich? | Ja |
| `RaidId` | uint32 | Neue Raid-ID | Ja |

**Broadcast (PartyUpdate 704)**:

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| `ConvertedToRaid` | bool | Party ist jetzt Raid | Ja |
| `RaidId` | uint32 | Raid-ID | Ja |

### Erwartete Response

- **Bei Erfolg**: `PartyConvertToRaid` Response + `PartyUpdate` (704) Broadcast
- **Bei Fehler**: `ErrorMessage` (910) mit Code `NOT_LEADER`, `LESS_THAN_6_MEMBERS`, `ALREADY_RAID`

### Flow-Diagramm

```
Leader                    Server                    All Members
  │                          │                            │
  │  6. Member joins         │                            │
  │                          │                            │
  │  PartyConvertToRaid(718) │                            │
  │─────────────────────────►│                            │
  │                          │                            │
  │                          │  PartyUpdate (704)         │
  │                          │  ConvertedToRaid=true      │
  │◄─────────────────────────│───────────────────────────►│
  │                          │                            │
  │  (UI switches to Raid)   │                            │
```

### Beispiel Payload

```csharp
var request = new PartyConvertToRaid
{
    Type = MessageType.PartyConvertToRaid
};
```

### Error Codes

| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `NOT_LEADER` | Nur Leader kann konvertieren | - |
| `LESS_THAN_6_MEMBERS` | Mindestens 6 Members nötig | Mehr einladen |
| `ALREADY_RAID` | Bereits ein Raid | - |
| `IN_COMBAT` | Im Kampf | Warten |

### Notizen

- **Permanent**: Kann nicht rückgängig gemacht werden
- **Min 6 Members**: Wird oft bei 6. Member-Join auto-vorgeschlagen
- **UI-Change**: Raid-Frames statt Party-Frames
- **Assistant-Leader**: Leader kann nach Conversion Assistants ernennen
- **Loot-Master**: Komplexere Loot-Optionen in Raids

---

## PartySync (719)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten (nach Reconnect)  
**Authentifizierung:** 🔒 Ja

### Beschreibung

Request für vollständige Party-State-Synchronisation nach Disconnect/Reconnect. Der Server sendet alle Party-Informationen: Members, Rollen, Loot-Settings, Leader, etc.

Diese Message wird automatisch nach Reconnect gesendet wenn der Client erkennt dass er in einer Party war. Der Server validiert die Session und sendet entweder Full-Sync oder "NOT_IN_PARTY".

### Im Scope ✅

- Full Party-State nach Reconnect
- Alle Member-Infos
- Leader, Loot-Settings, Rollen
- Pending Invites/Ready-Checks

### Nicht im Scope ❌

- Kontinuierliche Sync → verwende incremental Updates
- History → nur aktueller State

### Request/Response Payload

**Client → Server**:

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| _(leer)_ | - | Request Full-Sync | - |

**Server → Client**:

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| `PartyId` | uint32 | Party-ID | Ja |
| `LeaderId` | uint32 | Leader PlayerId | Ja |
| `LootMode` | LootMode | Loot-Verteilung | Ja |
| `Members` | PartyMember[] | Alle Members | Ja |
| `IsRaid` | bool | Ist Raid? | Ja |

**PartyMember**:

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| `PlayerId` | uint32 | Member-ID | Ja |
| `Name` | string | Name | Ja |
| `Level` | byte | Level | Ja |
| `Class` | ClassType | Klasse | Ja |
| `Roles` | RoleFlags | Rollen | Ja |
| `IsOnline` | bool | Online? | Ja |
| `ZoneId` | uint16 | Aktuelle Zone | Ja |

### Erwartete Response

- **Bei Erfolg**: `PartySync` Response mit Full-State
- **Bei Fehler**: `ErrorMessage` (910) mit Code `NOT_IN_PARTY` oder `PARTY_DISBANDED`

### Beispiel Payload

```csharp
var request = new PartySync
{
    Type = MessageType.PartySync
};

// Response
var response = new PartySyncResponse
{
    Type = MessageType.PartySync,
    PartyId = 78901,
    LeaderId = 12345,
    LootMode = LootMode.RoundRobin,
    Members = new PartyMember[]
    {
        new PartyMember { PlayerId = 12345, Name = "Thorgar", Level = 30, Class = ClassType.Warrior, Roles = RoleFlags.Tank, IsOnline = true, ZoneId = 101 },
        new PartyMember { PlayerId = 67890, Name = "Lyria", Level = 28, Class = ClassType.Priest, Roles = RoleFlags.Healer, IsOnline = true, ZoneId = 101 }
    },
    IsRaid = false
};
```

### Error Codes

| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `NOT_IN_PARTY` | Spieler nicht in Party | Party-UI ausblenden |
| `PARTY_DISBANDED` | Party wurde aufgelöst | Party-UI ausblenden |
| `SESSION_INVALID` | Session ungültig | Re-Login |

### Notizen

- **Auto-Request**: Client sendet auto nach Reconnect
- **Full-Sync**: Enthält alle Party-Daten
- **Fallback**: Bei Fehler Party-UI verstecken
- **Pending-Actions**: Laufende Ready-Checks/Summons werden NICHT erneut gesendet

---

## PartySummon (720)

**Richtung:** 📤 Client → Server (initiiert von 3 Members)  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

### Beschreibung

Drei Party-Members können gemeinsam einen 4. oder 5. Member zu ihrer Position beschwören. Alle 3 müssen 10 Sekunden channeln ohne sich zu bewegen oder Damage zu nehmen. Bei Unterbrechung bricht der Summon ab.

Der beschworene Spieler erhält eine `PartySummonResponse` (721) und kann Accept/Decline. Bei Accept wird er zur durchschnittlichen Position der 3 Summoner teleportiert.

### Im Scope ✅

- 3 Members channeln 10s
- Unterbrechung bei Movement/Damage
- Target-Member erhält Prompt
- Teleport bei Accept

### Nicht im Scope ❌

- Solo-Summon → benötigt 3 Spieler
- Raid-Summon → nur 5-Player-Parties
- Cross-Zone-Summon → Target muss in kompatibler Zone sein

### Request/Response Payload

**Client (Initiator) → Server**:

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| `TargetPlayerId` | uint32 | Zu beschw örender Member | Ja |

**Server → Initiator**:

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| `Success` | bool | Summon gestartet? | Ja |
| `ChannelTime` | byte | Channel-Duration (10s) | Ja |

**Server → All 3 Summoners (Broadcast)**:

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| `TargetPlayerId` | uint32 | Wer wird beschworen | Ja |
| `ChannelProgress` | byte | 0-100% | Ja |
| `InterruptReason` | InterruptReason | Grund bei Abbruch | Nein |

### Erwartete Response

- **Bei Erfolg**: Channel startet, nach 10s → `PartySummonResponse` (721) an Target
- **Bei Fehler**: `ErrorMessage` (910) mit Code `NOT_ENOUGH_MEMBERS`, `TARGET_IN_COMBAT`, `INVALID_ZONE`

### Flow-Diagramm

```
3 Summoners               Server                    Target
  │                          │                          │
  │  PartySummon (720)       │                          │
  │─────────────────────────►│                          │
  │                          │                          │
  │  Channel 10s...          │                          │
  │                          │                          │
  │  (Complete)              │                          │
  │                          │                          │
  │                          │  PartySummonResponse(721)│
  │                          │─────────────────────────►│
  │                          │                          │
  │                          │  Accept                  │
  │                          │◄─────────────────────────│
  │                          │                          │
  │                          │  TeleportExecute (204)   │
  │                          │─────────────────────────►│
```

### Beispiel Payload

```csharp
var request = new PartySummon
{
    Type = MessageType.PartySummon,
    TargetPlayerId = 99888
};
```

### Error Codes

| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `NOT_ENOUGH_MEMBERS` | <3 verfügbare Summoners | Mehr Members brauchen |
| `TARGET_IN_COMBAT` | Ziel im Kampf | Warten |
| `TARGET_IN_INSTANCE` | Ziel in Dungeon | Kann nicht summon |
| `INVALID_ZONE` | Inkompatible Zonen | Zone wechseln |
| `CHANNEL_INTERRUPTED` | Movement/Damage | Erneut versuchen |

### Notizen

- **3-Player-Requirement**: Exakt 3 müssen channeln
- **10s Channel**: Kann nicht verkürzt werden
- **Interrupt**: Movement (>1m), Damage, oder Stun/CC
- **Cooldown**: 30 Min Cooldown pro erfolgreichem Summon
- **Visual**: Casting-Bar + Ground-Effect an Summoner-Location

---

## PartySummonResponse (721)

**Richtung:** 📤 Client → Server  
**Frequenz:** Einmalig (nach Summon-Request)  
**Authentifizierung:** 🔒 Ja

### Beschreibung

Antwort auf eine Party-Summon-Anfrage. Der beschworene Spieler kann Accept oder Decline wählen. Bei Accept wird er zur Party-Position teleportiert. Bei Decline oder Timeout (60s) wird der Summon abgebrochen.

Accept führt sofort zu `TeleportExecute` (204). Der Spieler muss Out-of-Combat sein um zu accepten.

### Im Scope ✅

- Accept/Decline Summon
- 60s Timeout
- Out-of-Combat-Check
- Teleport bei Accept

### Nicht im Scope ❌

- In-Combat-Summon → nicht erlaubt
- Partial-Accept → alles-oder-nichts

### Request/Response Payload

**Client → Server**:

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| `Accept` | bool | Accept=true, Decline=false | Ja |

**Server → Client (bei Accept)**:

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| `Success` | bool | Teleport erfolgreich? | Ja |

**Broadcast an Summoners**:

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| `TargetPlayerId` | uint32 | Wer summoned wurde | Ja |
| `Accepted` | bool | Accepted oder Declined | Ja |

### Erwartete Response

- **Bei Accept**: `TeleportExecute` (204) + Broadcast an Summoners
- **Bei Decline**: Broadcast an Summoners
- **Bei Fehler**: `ErrorMessage` (910) mit Code `IN_COMBAT`, `TIMEOUT`

### Beispiel Payload

```csharp
var response = new PartySummonResponse
{
    Type = MessageType.PartySummonResponse,
    Accept = true
};
```

### Error Codes

| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `IN_COMBAT` | Im Kampf | Combat beenden |
| `TIMEOUT` | 60s abgelaufen | Erneut summon |
| `SUMMON_EXPIRED` | Summoners haben Zone verlassen | - |

### Notizen

- **Timeout**: 60s Auto-Decline
- **In-Combat**: Kann nicht accepten
- **UI**: Accept/Decline Buttons mit Countdown
- **Teleport-Location**: Durchschnitt der 3 Summoner-Positionen

---

## PartyMarkerSet (722)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Party-Leader oder Raid-Assistants

### Beschreibung

Setzt Raid-Marker (Icons) auf der Welt-Map oder auf Entities für Party-/Raid-Koordination. 8 Marker verfügbar: Skull, Cross, Square, Moon, Triangle, Diamond, Circle, Star. Marker sind für alle Party/Raid-Members sichtbar.

Marker können auf Position (X, Y) oder auf Entity (EntityId) gesetzt werden. Position-Marker sind statisch, Entity-Marker folgen der Entity.

### Im Scope ✅

- 8 unterschiedliche Marker-Icons
- Position-basiert oder Entity-basiert
- Nur für Party/Raid-Members sichtbar
- Leader/Assistant-Only

### Nicht im Scope ❌

- Personal-Marker → nicht implementiert
- Mehr als 8 gleichzeitig → Limit = 8
- Cross-Zone-Marker → nur in gleicher Zone sichtbar

### Request/Response Payload

**Client (Leader/Assistant) → Server**:

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| `MarkerId` | byte | 0-7 (Skull, Cross, Square, Moon, Triangle, Diamond, Circle, Star) | Ja |
| `TargetType` | MarkerTargetType | Position oder Entity | Ja |
| `X` | float | X-Koordinate (wenn Position) | Nein |
| `Y` | float | Y-Koordinate (wenn Position) | Nein |
| `EntityId` | uint32 | EntityId (wenn Entity) | Nein |

**Server → All Party Members (Broadcast)**:

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| `MarkerId` | byte | 0-7 | Ja |
| `TargetType` | MarkerTargetType | Position oder Entity | Ja |
| `X` | float | Position | Nein |
| `Y` | float | Position | Nein |
| `EntityId` | uint32 | Entity | Nein |

### Erwartete Response

- **Bei Erfolg**: Broadcast `PartyMarkerSet` an alle Members
- **Bei Fehler**: `ErrorMessage` (910) mit Code `NOT_LEADER`, `MARKER_LIMIT_REACHED`, `INVALID_ENTITY`

### Beispiel Payload

```csharp
// Entity-Marker (Skull auf Boss)
var markerEntity = new PartyMarkerSet
{
    Type = MessageType.PartyMarkerSet,
    MarkerId = 0, // Skull
    TargetType = MarkerTargetType.Entity,
    EntityId = 88776
};

// Position-Marker (Moon auf Sammel-Punkt)
var markerPosition = new PartyMarkerSet
{
    Type = MessageType.PartyMarkerSet,
    MarkerId = 3, // Moon
    TargetType = MarkerTargetType.Position,
    X = 1500.5f,
    Y = 800.2f
};
```

### Error Codes

| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `NOT_LEADER` | Nur Leader/Assistant | - |
| `MARKER_LIMIT_REACHED` | 8 Marker-Limit | Alten entfernen |
| `INVALID_ENTITY` | Entity existiert nicht | Andere wählen |
| `NOT_IN_ZONE` | Entity nicht in Zone | Zone wechseln |

### Notizen

- **8 Marker**: Skull, Cross, Square, Moon, Triangle, Diamond, Circle, Star
- **Entity-Follow**: Marker auf Entity folgt dieser
- **Persistence**: Marker bleiben bis entfernt oder Zone-Leave
- **Visibility**: Nur für Party/Raid-Members
- **Use-Cases**: Kill-Order, Gather-Points, CC-Targets

---

## PartyMarkerClear (723)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Party-Leader oder Raid-Assistants

### Beschreibung

Entfernt einen spezifischen Raid-Marker oder alle Marker gleichzeitig. Leader/Assistants können jederzeit Marker entfernen um die Map aufzuräumen.

Clear-All ist nützlich nach Raid-Wipes oder Boss-Kills um alte Marker zu entfernen.

### Im Scope ✅

- Einzelnen Marker entfernen
- Alle Marker entfernen (Clear-All)
- Leader/Assistant-Only
- Broadcast an alle Members

### Nicht im Scope ❌

- Auto-Clear → muss manuell erfolgen
- Marker-Edit → nur Remove + Re-Add

### Request/Response Payload

**Client (Leader/Assistant) → Server**:

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| `MarkerId` | byte | 0-7 oder 255=All | Ja |

**Server → All Party Members (Broadcast)**:

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| `MarkerId` | byte | Entfernter Marker oder 255=All | Ja |

### Erwartete Response

- **Bei Erfolg**: Broadcast `PartyMarkerClear` an alle Members
- **Bei Fehler**: `ErrorMessage` (910) mit Code `NOT_LEADER`, `MARKER_NOT_FOUND`

### Beispiel Payload

```csharp
// Einzelnen Marker entfernen
var clearSingle = new PartyMarkerClear
{
    Type = MessageType.PartyMarkerClear,
    MarkerId = 0 // Skull entfernen
};

// Alle Marker entfernen
var clearAll = new PartyMarkerClear
{
    Type = MessageType.PartyMarkerClear,
    MarkerId = 255 // Alle entfernen
};
```

### Error Codes

| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `NOT_LEADER` | Nur Leader/Assistant | - |
| `MARKER_NOT_FOUND` | Marker existiert nicht | - |

### Notizen

- **MarkerId=255**: Entfernt ALLE Marker
- **Instant**: Keine Verzögerung
- **Common**: Oft nach Wipes/Boss-Kills
- **Keyboard-Shortcut**: Viele MMOs binden auf Ctrl+Shift+0-7

---

## PartyDifficultyVote (724)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

### Beschreibung

Initiiert eine Abstimmung über die Dungeon-Schwierigkeit (Normal, Heroic, Mythic). Jedes Party-Member kann voten, Mehrheitsentscheidung bestimmt die Schwierigkeit. Vote hat 60s Timeout.

Schwierigkeit muss VOR Dungeon-Eintritt gesetzt werden. Nach Zone-In kann Schwierigkeit nicht mehr geändert werden. Vote kann nur Out-of-Instance erfolgen.

### Im Scope ✅

- Vote-Initiation durch beliebiges Member
- Normal/Heroic/Mythic-Optionen
- Mehrheitsentscheidung
- 60s Timeout

### Nicht im Scope ❌

- In-Instance-Change → nur vor Eintritt
- Erzwingen → Leader kann nicht überstimmen
- Custom-Difficulty → nur 3 feste Level

### Request/Response Payload

**Client → Server**:

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| `Difficulty` | DifficultyLevel | Normal/Heroic/Mythic | Ja |

**DifficultyLevel**:
- `Normal = 0`
- `Heroic = 1`
- `Mythic = 2`

**Server → All Members (Broadcast)**:

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| `InitiatorId` | uint32 | Wer voted hat | Ja |
| `Difficulty` | DifficultyLevel | Gewünschte Schwierigkeit | Ja |
| `TimeoutSeconds` | byte | Countdown (60s) | Ja |

### Erwartete Response

- **Bei Erfolg**: Broadcast `PartyDifficultyVote` an alle Members
- **Nach Timeout**: `PartyDifficultySet` (725) mit Mehrheitsergebnis
- **Bei Fehler**: `ErrorMessage` (910) mit Code `IN_INSTANCE`, `VOTE_IN_PROGRESS`

### Flow-Diagramm

```
Any Member                Server                    All Members
  │                          │                            │
  │  PartyDifficultyVote(724)│                            │
  │─────────────────────────►│                            │
  │                          │                            │
  │                          │  Vote Broadcast            │
  │                          │───────────────────────────►│
  │                          │                            │
  │                          │  Votes (60s)               │
  │                          │◄───────────────────────────│
  │                          │                            │
  │                          │  PartyDifficultySet (725)  │
  │                          │───────────────────────────►│
```

### Beispiel Payload

```csharp
var vote = new PartyDifficultyVote
{
    Type = MessageType.PartyDifficultyVote,
    Difficulty = DifficultyLevel.Heroic
};
```

### Error Codes

| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `IN_INSTANCE` | Bereits in Dungeon | Vor Eintritt voten |
| `VOTE_IN_PROGRESS` | Vote läuft bereits | Warten oder canceln |
| `LEVEL_TOO_LOW` | Level-Requirement | Höheres Level |

### Notizen

- **Majority-Wins**: >50% für Difficulty
- **Default**: Kein Vote = Normal
- **Level-Gates**: Heroic ab Level 80, Mythic ab Level 85
- **Rewards**: Höhere Difficulty = besseres Loot

---

## PartyDifficultySet (725)

**Richtung:** 📥 Server → Client (Broadcast nach Vote)  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

### Beschreibung

Server-seitige Broadcast nachdem eine Difficulty-Vote abgeschlossen ist. Informiert alle Members über die gesetzte Schwierigkeit. Schwierigkeit wird beim Dungeon-Eintritt angewendet.

Diese Message kommt entweder nach erfolgreichem Vote oder nach Timeout (dann Mehrheitsergebnis). Zone-Eintritt verwendet dann diese Difficulty.

### Im Scope ✅

- Broadcast nach Vote-Complete
- Setzt Difficulty für nächsten Dungeon-Eintritt
- Kann mehrfach geändert werden (vor Eintritt)

### Nicht im Scope ❌

- In-Instance-Change → nur vor Eintritt
- Force-Set ohne Vote → nur nach Vote

### Request/Response Payload

**Server → All Members (Broadcast)**:

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| `Difficulty` | DifficultyLevel | Gesetzte Schwierigkeit | Ja |
| `VoteResult` | VoteResult | Unanimous/Majority/Default | Ja |

**VoteResult**:
- `Unanimous = 0` - Alle stimmten zu
- `Majority = 1` - Mehrheit gewann
- `Default = 2` - Timeout/Tie → Default Normal

### Erwartete Response

- **Keine direkte Response** - Broadcast-Message

### Beispiel Payload

```csharp
var set = new PartyDifficultySet
{
    Type = MessageType.PartyDifficultySet,
    Difficulty = DifficultyLevel.Heroic,
    VoteResult = VoteResult.Majority
};
```

### Notizen

- **Apply-Time**: Beim nächsten Zone-Eintritt
- **UI-Update**: Difficulty im Dungeon-Finder anzeigen
- **Re-Vote**: Kann jederzeit neu gevoted werden (vor Eintritt)
- **Tie**: Bei Gleichstand → Default Normal

---

**Letzte Aktualisierung**: 2025-12-17  
**Version**: 2.0.0

[← Zurück zur Übersicht](README.md)
