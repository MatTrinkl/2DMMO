# 🏰 Guild Messages (0800-0899)

**Kategorie:** 8  
**Range:** 0800-0899  
**Phase:** Phase 2  
**Status:** ✅ Vollständig dokumentiert (39 Messages)

[← Zurück zur Übersicht](README.md)

---

## 📋 Übersicht

Diese Kategorie umfasst alle Messages für **Guild**-Funktionalität im 2DMMO - das komplette Gilden-System mit Management, Rängen, Permissions, Bank, Kalender, Rekrutierung und Allianzen.

**Scope**: Gilden-Management, Rank-System mit Permissions, Guild Bank mit Tabs, Guild Events/Kalender, Rekrutierung/Applications, Guild Allianzen

**Verwandte Kategorien**: 
- [Party (07)](07-party.md) - Gruppen-System
- [Chat (04)](04-chat.md) - Guild Chat Channel
- [Character (06)](06-character.md) - Guild-Zugehörigkeit im Charakter-Profil

---

## GuildInvite (800)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 GUILD_INVITE Permission

### Beschreibung

Der Client sendet diese Message, wenn ein Spieler einen anderen Spieler in die Gilde einladen möchte. Der einladende Spieler benötigt die `GUILD_INVITE` Permission in seiner aktuellen Rang-Stufe.

Das System validiert, dass beide Spieler online sind, der eingeladene Spieler nicht bereits in einer Gilde ist, und dass keine cooldown-Periode aktiv ist. Einladungen verfallen nach 60 Sekunden automatisch.

Guild-Einladungen sind persönlich und können nicht über Distanz verschickt werden - beide Spieler müssen in der gleichen Zone sein. Es können maximal 5 gleichzeitige ausstehende Einladungen pro Gilde existieren (Anti-Spam).

### Im Scope ✅

- Einladen eines Spielers per CharacterName oder PlayerId
- Permission-Check (GUILD_INVITE erforderlich)
- Validation: Zielspieler online, in gleicher Zone, gildenfrei
- Anti-Spam: Max 5 ausstehende Einladungen, 60s cooldown nach Ablehnung
- 60 Sekunden Auto-Expire

### Nicht im Scope ❌

- Masseneinladungen → mehrere einzelne `GuildInvite` Messages senden
- Offline-Einladungen → verwende stattdessen `MailSystem` (1800) mit Guild-Info
- Guild-Applications → verwende `GuildApply` (833)

### Request Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `GuildInvite` (800) | Ja |
| TargetPlayerName | string | Character-Name des Ziels (max 32 chars) | Ja (oder PlayerId) |
| TargetPlayerId | int | Alternative: PlayerId des Ziels | Ja (oder Name) |
| PersonalMessage | string | Optional persönliche Nachricht (max 200 chars) | Nein |

### Erwartete Response

- **Bei Erfolg:** `GuildUpdate` (804) mit neuem Member wenn Einladung akzeptiert
- **Bei Ablehnung:** `GuildInviteResponse` (801) mit Declined-Status
- **Bei Fehler:** `ErrorMessage` (910) mit entsprechendem Code

### Verwandte Messages

| Message | ID | Beziehung |
|---------|-----|-----------|
| `GuildInviteResponse` | 801 | Antwort des eingeladenen Spielers |
| `GuildUpdate` | 804 | Benachrichtigung über neues Member |
| `GuildRosterResponse` | 811 | Aktualisiertes Roster nach Beitritt |

### Flow-Diagramm

```
Inviter (Client)                    Server                     Target (Client)
     │                                 │                              │
     │  GuildInvite (800)              │                              │
     │────────────────────────────────►│                              │
     │                                 │  Permission-Check            │
     │                                 │  Zone-Check                  │
     │                                 │  Guild-Status-Check          │
     │                                 │  Spam-Check                  │
     │                                 │                              │
     │                                 │  GuildInviteNotification     │
     │                                 │─────────────────────────────►│
     │                                 │                              │
     │                                 │  (60s Timeout)               │
     │                                 │                              │
     │                                 │  GuildInviteResponse (801)   │
     │                                 │◄─────────────────────────────│
     │                                 │                              │
     │  GuildUpdate (804)              │  GuildUpdate (804)           │
     │◄────────────────────────────────│─────────────────────────────►│
```

### Beispiel Payload

```csharp
var invite = new GuildInvite
{
    Type = MessageType.GuildInvite,
    TargetPlayerName = "Thorgar",
    PersonalMessage = "Join us! We're raiding tonight."
};
```

### Error Codes

| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `GUILD_NO_PERMISSION` | Keine GUILD_INVITE Permission | Permission von Officer anfordern |
| `GUILD_TARGET_NOT_FOUND` | Zielspiel nicht online oder existiert nicht | Namen prüfen, später erneut |
| `GUILD_TARGET_ALREADY_IN_GUILD` | Ziel ist bereits in einer Gilde | Anderen Spieler einladen |
| `GUILD_TARGET_TOO_FAR` | Ziel nicht in gleicher Zone | Näher zum Ziel bewegen |
| `GUILD_INVITE_COOLDOWN` | 60s Cooldown nach letzter Ablehnung | 60 Sekunden warten |
| `GUILD_TOO_MANY_PENDING_INVITES` | Mehr als 5 ausstehende Einladungen | Warten bis Einladungen verfallen |
| `GUILD_FULL` | Gilde hat max. Memberanzahl erreicht (500) | Platz schaffen oder warten |

### Notizen

- **Anti-Spam**: Max 5 gleichzeitige ausstehende Einladungen pro Gilde
- **Range-Check**: Beide Spieler müssen in gleicher Zone sein (nicht cross-zone)
- **Cooldown**: Nach Ablehnung 60s Cooldown für diesen spezifischen Spieler
- **Auto-Expire**: Einladungen verfallen nach 60s automatisch
- **Permission**: `GUILD_INVITE` Permission ist erforderlich (Default: Officer+)
- **Guild-Cap**: Gilden haben max. 500 Members (Standard-MMO-Größe)
- **Personal Message**: Optional, wird dem Ziel angezeigt, kann leer sein
- **Rate Limiting**: Pro Spieler max. 10 Einladungen/Minute

---

## GuildInviteResponse (801)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Der Client sendet diese Message als Antwort auf eine erhaltene Guild-Einladung. Der Spieler kann die Einladung akzeptieren oder ablehnen.

Bei Akzeptierung tritt der Spieler der Gilde mit dem niedrigsten Rang bei (typischerweise "Member" oder "Initiate") und alle Guild-Members werden über das neue Mitglied benachrichtigt.

Bei Ablehnung wird der einladende Spieler informiert und ein 60-Sekunden Cooldown für erneute Einladungen dieses Spielers aktiviert.

### Im Scope ✅

- Einladung akzeptieren oder ablehnen
- Automatischer Beitritt mit niedrigstem Rang bei Akzeptierung
- Benachrichtigung aller Guild-Members bei Beitritt
- 60s Cooldown bei Ablehnung

### Nicht im Scope ❌

- Ignorieren (kein Response) → führt zu Auto-Expire nach 60s
- Mit bestimmtem Rang beitreten → nur niedrigster Rang möglich, später Promotion

### Request Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `GuildInviteResponse` (801) | Ja |
| GuildId | int | ID der Gilde (aus Invitation) | Ja |
| Accepted | bool | `true` = Akzeptiert, `false` = Abgelehnt | Ja |

### Erwartete Response

- **Bei Akzeptierung:** `GuildUpdate` (804) an alle Members mit neuem Member
- **Bei Ablehnung:** `GuildInviteDeclined` Notification an Inviter
- **Bei Fehler:** `ErrorMessage` (910) mit entsprechendem Code

### Verwandte Messages

| Message | ID | Beziehung |
|---------|-----|-----------|
| `GuildInvite` | 800 | Ursprüngliche Einladung |
| `GuildUpdate` | 804 | Member-Join Benachrichtigung |
| `GuildRosterResponse` | 811 | Komplettes Roster nach Beitritt |
| `GuildMOTD` | 808 | Message-of-the-Day nach Beitritt |

### Flow-Diagramm

```
Target (Client)                  Server                     Guild Members
     │                              │                              │
     │  GuildInviteResponse (801)   │                              │
     │  Accepted=true               │                              │
     │─────────────────────────────►│                              │
     │                              │  Add Member                  │
     │                              │  Set Lowest Rank             │
     │                              │                              │
     │  GuildUpdate (804)           │  GuildUpdate (804)           │
     │◄─────────────────────────────│─────────────────────────────►│
     │  GuildRosterResponse (811)   │                              │
     │◄─────────────────────────────│                              │
     │  GuildMOTD (808)             │                              │
     │◄─────────────────────────────│                              │
```

### Beispiel Payload

```csharp
// Akzeptieren
var accept = new GuildInviteResponse
{
    Type = MessageType.GuildInviteResponse,
    GuildId = 12345,
    Accepted = true
};

// Ablehnen
var decline = new GuildInviteResponse
{
    Type = MessageType.GuildInviteResponse,
    GuildId = 12345,
    Accepted = false
};
```

### Error Codes

| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `GUILD_INVITE_EXPIRED` | Einladung ist abgelaufen (>60s) | Neue Einladung anfordern |
| `GUILD_ALREADY_IN_GUILD` | Spieler ist bereits in einer Gilde | Aktuelle Gilde verlassen |
| `GUILD_FULL` | Gilde hat max. Members erreicht | Andere Gilde suchen |
| `GUILD_DISBANDED` | Gilde wurde aufgelöst | Andere Gilde suchen |

### Notizen

- **Default Rank**: Neue Members erhalten automatisch den niedrigsten definierten Rang
- **Immediate Effect**: Beitritt ist sofort, keine zusätzliche Bestätigung
- **MOTD**: Nach Beitritt erhält Spieler automatisch die Guild MOTD
- **Roster Sync**: Komplettes Guild-Roster wird nach Beitritt synchronisiert
- **Cross-Zone**: Beitritt funktioniert auch wenn Inviter in anderer Zone ist
- **Cooldown**: Ablehnung aktiviert 60s Cooldown für erneute Einladungen
- **Guild Chat**: Nach Beitritt automatisch dem Guild-Chat-Channel beigetreten

---

## GuildLeave (802)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine (außer Guild Master - siehe Notizen)

### Beschreibung

Der Client sendet diese Message, wenn ein Spieler freiwillig die Gilde verlassen möchte. Dies ist für alle Ranks außer Guild Master möglich.

Der Guild Master kann die Gilde nicht verlassen - er muss entweder die Gilde auflösen (`GuildDisband` 805) oder die Leadership an ein anderes Member übertragen (`GuildLeaderChange` 806) bevor er gehen kann.

Nach dem Verlassen wird der Spieler aus allen Guild-Systemen entfernt (Roster, Bank-Permissions, Events) und alle aktiven Guild-Members werden benachrichtigt.

### Im Scope ✅

- Freiwilliges Verlassen der Gilde
- Entfernung aus Guild-Roster, Bank-Permissions, Event-Signups
- Benachrichtigung aller aktiven Guild-Members
- Sofortiger Effekt, keine Bestätigung erforderlich

### Nicht im Scope ❌

- Guild Master Leave → muss erst `GuildDisband` (805) oder `GuildLeaderChange` (806) verwenden
- Kick anderer Spieler → verwende `GuildKick` (803)
- Guild-Wechsel ohne Leave → muss erst Leave, dann andere Guild joinen

### Request Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `GuildLeave` (802) | Ja |

### Erwartete Response

- **Bei Erfolg:** `GuildUpdate` (804) an alle Members über departed Member
- **Bei Fehler:** `ErrorMessage` (910) mit entsprechendem Code

### Verwandte Messages

| Message | ID | Beziehung |
|---------|-----|-----------|
| `GuildKick` | 803 | Erzwungenes Entfernen durch Officer |
| `GuildDisband` | 805 | Guild Master löst Gilde auf |
| `GuildUpdate` | 804 | Benachrichtigung über departed Member |
| `GuildLeaderChange` | 806 | Guild Master gibt Leadership ab |

### Flow-Diagramm

```
Client                          Server                     Guild Members
  │                                │                              │
  │  GuildLeave (802)              │                              │
  │───────────────────────────────►│                              │
  │                                │  Not Guild Master?           │
  │                                │  Remove from Roster          │
  │                                │  Remove Bank Permissions     │
  │                                │  Remove Event Signups        │
  │                                │                              │
  │  Confirmation                  │  GuildUpdate (804)           │
  │◄───────────────────────────────│─────────────────────────────►│
  │                                │  (Member Left)               │
```

### Beispiel Payload

```csharp
var leave = new GuildLeave
{
    Type = MessageType.GuildLeave
};
```

### Error Codes

| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `GUILD_NOT_IN_GUILD` | Spieler ist nicht in einer Gilde | Keine Aktion nötig |
| `GUILD_LEADER_CANNOT_LEAVE` | Guild Master muss erst Leadership abgeben | `GuildLeaderChange` oder `GuildDisband` |

### Notizen

- **Guild Master**: Kann nicht direkt leaven, muss erst Leadership übergeben oder Gilde auflösen
- **Immediate Effect**: Leave ist sofort wirksam, keine Wartezeit
- **Bank Items**: Items im Guild Bank von diesem Spieler bleiben erhalten
- **Event Signups**: Alle Event-Signups werden automatisch storniert
- **Bank Logs**: Transaktionen des Spielers bleiben in Bank-Logs sichtbar
- **Rejoin**: Spieler kann derselben Gilde erneut beitreten (mit neuer Einladung)
- **Achievements**: Guild-Achievements bleiben beim Spieler (nicht mehr progress-able)
- **No Cooldown**: Kein Cooldown für Rejoin oder Join anderer Gilden

---

## GuildKick (803)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 GUILD_KICK Permission

### Beschreibung

Der Client sendet diese Message, wenn ein Spieler mit `GUILD_KICK` Permission ein anderes Guild-Member entfernen möchte. Dies ist eine erzwungene Entfernung (im Gegensatz zum freiwilligen `GuildLeave`).

Der kickende Spieler muss einen höheren Rang haben als der gekickte Spieler. Guild Master kann alle kicken, aber niemand kann den Guild Master kicken.

Nach dem Kick wird der Spieler aus allen Guild-Systemen entfernt und erhält eine Benachrichtigung mit optionalem Grund. Alle aktiven Guild-Members werden ebenfalls informiert. Es gibt eine 5-Minuten Cooldown nach jedem Kick (Anti-Abuse).

### Im Scope ✅

- Erzwungenes Entfernen eines Guild-Members
- Permission-Check: GUILD_KICK erforderlich
- Rank-Hierarchie: Kann nur niedrigere Ranks kicken
- Optionaler Kick-Grund (max 200 chars)
- 5-Minuten Cooldown zwischen Kicks
- Benachrichtigung des gekickten Spielers und aller Members

### Nicht im Scope ❌

- Guild Master kicken → unmöglich
- Gleichrangige kicken → nur niedrigere Ranks
- Ban-System → Kick entfernt nur, kein permanentes Ban
- Freiwilliges Leave → verwende `GuildLeave` (802)

### Request Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `GuildKick` (803) | Ja |
| TargetPlayerId | int | PlayerId des zu kickenden Members | Ja |
| Reason | string | Grund für Kick (max 200 chars) | Nein |

### Erwartete Response

- **Bei Erfolg:** `GuildUpdate` (804) an alle Members über kicked Member
- **Bei Fehler:** `ErrorMessage` (910) mit entsprechendem Code

### Verwandte Messages

| Message | ID | Beziehung |
|---------|-----|-----------|
| `GuildLeave` | 802 | Freiwilliges Verlassen |
| `GuildUpdate` | 804 | Benachrichtigung über kicked Member |
| `GuildDemote` | 807 | Alternative: Demote statt Kick |

### Flow-Diagramm

```
Kicker (Client)                 Server                     Target/Members
     │                             │                              │
     │  GuildKick (803)            │                              │
     │────────────────────────────►│                              │
     │                             │  Permission-Check            │
     │                             │  Rank-Hierarchy-Check        │
     │                             │  Cooldown-Check              │
     │                             │  Remove from Guild           │
     │                             │                              │
     │  Confirmation               │  KickNotification            │
     │◄────────────────────────────│─────────────────────────────►│
     │                             │  (with Reason)               │
     │                             │                              │
     │                             │  GuildUpdate (804)           │
     │                             │─────────────────────────────►│
     │                             │  (to all other members)      │
```

### Beispiel Payload

```csharp
var kick = new GuildKick
{
    Type = MessageType.GuildKick,
    TargetPlayerId = 789,
    Reason = "Violation of guild rules - harassment"
};
```

### Error Codes

| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `GUILD_NO_PERMISSION` | Keine GUILD_KICK Permission | Permission anfordern |
| `GUILD_TARGET_NOT_FOUND` | Ziel-Spieler nicht in Gilde | PlayerId prüfen |
| `GUILD_RANK_TOO_LOW` | Kann diesen Rank nicht kicken (gleich/höher) | Höheren Rank um Kick bitten |
| `GUILD_CANNOT_KICK_LEADER` | Guild Master kann nicht gekickt werden | Leadership-Change vorher |
| `GUILD_KICK_COOLDOWN` | 5-Minuten Cooldown aktiv | Warten |

### Notizen

- **Rank Hierarchy**: Kann nur Spieler mit niedrigerem Rank kicken
- **Guild Master**: Kann alle kicken außer sich selbst
- **Cooldown**: 5 Minuten zwischen Kicks (gilt pro kickendem Spieler)
- **Reason**: Optional, wird dem gekickten Spieler angezeigt
- **Bank Items**: Items vom gekickten Spieler im Bank bleiben erhalten
- **Rejoin**: Gekickte Spieler können mit neuer Einladung wieder beitreten
- **Log**: Kick wird in Guild-Log aufgezeichnet (wer, wen, wann, warum)
- **Offline Kick**: Kann auch offline Spieler kicken
- **Combat**: Kick auch während Combat möglich

---

## GuildUpdate (804)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung

Der Server sendet diese Message an Guild-Members, wenn sich ein relevanter Guild-State ändert. Dies ist die zentrale Broadcast-Message für Guild-Events.

Typische Anlässe: Member Join/Leave/Kick, Rank-Änderungen, Permission-Updates, MOTD-Änderungen, Info-Updates. Die Message enthält nur die ge Änderung, nicht den kompletten Guild-State (für Full-Sync verwende `GuildRosterRequest`/`GuildRosterResponse`).

Dies ermöglicht inkrementelle Updates und reduziert Bandbreite. Clients sollten ihren lokalen Guild-State basierend auf diesen Updates aktualisieren.

### Im Scope ✅

- Benachrichtigung über Guild-State-Änderungen
- Inkrementelle Updates (nur Änderungen, nicht Full-State)
- Member Join/Leave/Kick Events
- Rank/Permission Änderungen
- MOTD/Info Updates
- Broadcast an alle relevanten Members

### Nicht im Scope ❌

- Full Guild State → verwende `GuildRosterResponse` (811)
- Bank-Änderungen → verwende `GuildBankLog` (822)
- Event-Änderungen → verwende Event-spezifische Messages

### Broadcast Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `GuildUpdate` (804) | Ja |
| UpdateType | GuildUpdateType | Art der Änderung | Ja |
| PlayerId | int | Betroffener Spieler (falls relevant) | Conditional |
| PlayerName | string | Name des betroffenen Spielers | Conditional |
| RankId | int | Neuer Rank (bei Rank-Änderungen) | Conditional |
| MOTD | string | Neues MOTD (bei MOTD-Update) | Conditional |
| Info | string | Neue Guild-Info (bei Info-Update) | Conditional |
| Timestamp | long | Unix-Timestamp der Änderung | Ja |

### GuildUpdateType Enum

```csharp
public enum GuildUpdateType
{
    MemberJoined = 1,
    MemberLeft = 2,
    MemberKicked = 3,
    MemberPromoted = 4,
    MemberDemoted = 5,
    RankCreated = 6,
    RankDeleted = 7,
    RankEdited = 8,
    MOTDChanged = 9,
    InfoChanged = 10,
    PermissionChanged = 11
}
```

### Verwandte Messages

| Message | ID | Beziehung |
|---------|-----|-----------|
| `GuildInviteResponse` | 801 | Löst MemberJoined aus |
| `GuildLeave` | 802 | Löst MemberLeft aus |
| `GuildKick` | 803 | Löst MemberKicked aus |
| `GuildPromote` | 806 | Löst MemberPromoted aus |
| `GuildDemote` | 807 | Löst MemberDemoted aus |
| `GuildRosterResponse` | 811 | Full-State Alternative |

### Flow-Diagramm

```
Server                          All Guild Members
  │                                    │
  │  (Guild State Change)              │
  │                                    │
  │  GuildUpdate (804)                 │
  │  UpdateType=MemberJoined           │
  │───────────────────────────────────►│
  │                                    │  Update Local State
  │                                    │  Display Notification
```

### Beispiel Payload

```csharp
// Member Joined
var joinUpdate = new GuildUpdate
{
    Type = MessageType.GuildUpdate,
    UpdateType = GuildUpdateType.MemberJoined,
    PlayerId = 123,
    PlayerName = "NewMember",
    RankId = 1, // Lowest rank
    Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
};

// MOTD Changed
var motdUpdate = new GuildUpdate
{
    Type = MessageType.GuildUpdate,
    UpdateType = GuildUpdateType.MOTDChanged,
    MOTD = "Raid tonight at 8pm!",
    Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
};

// Member Kicked
var kickUpdate = new GuildUpdate
{
    Type = MessageType.GuildUpdate,
    UpdateType = GuildUpdateType.MemberKicked,
    PlayerId = 789,
    PlayerName = "KickedPlayer",
    Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
};
```

### Notizen

- **Incremental**: Nur Änderungen, nicht kompletter State
- **Broadcast**: Gesendet an alle ONLINE Guild-Members
- **Offline Members**: Erhalten Update beim nächsten Login via `GuildRosterResponse`
- **Client-State**: Client muss lokalen Guild-State basierend auf Updates pflegen
- **Throttling**: Keine Throttling - jede Änderung löst sofort Update aus
- **Order**: Updates werden in chronologischer Reihenfolge gesendet
- **Persistence**: Server speichert alle Updates für Offline-Member-Sync
- **Display**: UI sollte Toast-Notifications für wichtige Updates zeigen

---

## GuildDisband (805)

**Richtung:** 📤 Client → Server  
**Frequenz:** Sehr selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Guild Master Only

### Beschreibung

Der Client sendet diese Message, wenn der Guild Master die Gilde permanent auflösen möchte. Dies ist eine irreversible Aktion, die die gesamte Gilde, alle Ranks, Permissions, Bank-Items und Event-Daten löscht.

Als Sicherheitsmaßnahme muss eine Bestätigung mitgesendet werden (Guild-Name tippen). Nach der Auflösung erhalten alle Members eine Benachrichtigung und werden automatisch aus der Gilde entfernt.

Guild Bank Items werden **nicht** zurückgegeben - sie sind permanent verloren. Spieler sollten vorher alle wertvollen Items aus dem Bank entfernen.

### Im Scope ✅

- Permanentes Auflösen der Gilde
- Nur Guild Master kann ausführen
- Sicherheits-Bestätigung erforderlich (Guild-Name)
- Alle Members werden benachrichtigt und entfernt
- Alle Guild-Daten werden gelöscht (Ranks, Bank, Events)
- Bank-Items gehen verloren

### Nicht im Scope ❌

- Temporäre Inaktivierung → keine Funktion dafür
- Rückgängig machen → irreversibel
- Bank-Items zurückgeben → gehen verloren
- Leadership-Transfer → verwende `GuildLeaderChange` (806) stattdessen

### Request Payload

| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Type | MessageType | `GuildDisband` (805) | Ja |
| GuildNameConfirmation | string | Guild-Name zur Bestätigung (exakte Schreibweise) | Ja |

### Erwartete Response

- **Bei Erfolg:** `GuildDisbandedNotification` an alle Members
- **Bei Fehler:** `ErrorMessage` (910) mit entsprechendem Code

### Verwandte Messages

| Message | ID | Beziehung |
|---------|-----|-----------|
| `GuildLeaderChange` | 806 | Alternative: Leadership abgeben statt auflösen |
| `GuildLeave` | 802 | Alternative: Gilde verlassen (nach Leadership-Transfer) |

### Flow-Diagramm

```
Guild Master (Client)           Server                     All Members
      │                            │                              │
      │  GuildDisband (805)        │                              │
      │  + Confirmation            │                              │
      │───────────────────────────►│                              │
      │                            │  Verify Guild Master         │
      │                            │  Verify Confirmation         │
      │                            │  Delete Guild Data           │
      │                            │  Delete Bank Items           │
      │                            │  Delete Events               │
      │                            │                              │
      │  Confirmation              │  GuildDisbandedNotification  │
      │◄───────────────────────────│─────────────────────────────►│
      │                            │  (All members removed)       │
```

### Beispiel Payload

```csharp
var disband = new GuildDisband
{
    Type = MessageType.GuildDisband,
    GuildNameConfirmation = "Legendary Heroes"  // Exact guild name
};
```

### Error Codes

| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `GUILD_NOT_LEADER` | Nur Guild Master kann Gilde auflösen | Leadership übernehmen |
| `GUILD_CONFIRMATION_MISMATCH` | Guild-Name stimmt nicht überein | Exakten Namen eingeben |
| `GUILD_HAS_PENDING_EVENTS` | Guild hat aktive Events (optional check) | Events absagen |

### Notizen

- **Guild Master Only**: Nur der Guild Master kann diese Aktion ausführen
- **Irreversible**: Kann nicht rückgängig gemacht werden
- **Confirmation**: Guild-Name muss exakt eingegeben werden (case-sensitive)
- **Bank Items Lost**: Alle Items im Guild Bank gehen permanent verloren
- **Warnings**: Client sollte mehrfache Warnungen anzeigen vor Ausführung
- **Alternative**: Guild Master kann Leadership übertragen und dann leaven
- **Name Reuse**: Guild-Name wird sofort zur Wiederverwendung freigegeben
- **Achievements**: Guild-Achievements werden aus allen Member-Profilen entfernt
- **No Cooldown**: Kein Cooldown - sofortige Auflösung
- **Recommendation**: Vor Disband: Bank leeren, Events absagen, Members informieren

---


