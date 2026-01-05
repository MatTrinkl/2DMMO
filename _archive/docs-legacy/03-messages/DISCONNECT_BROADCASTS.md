# 📡 Disconnect Broadcast Messages

**Version:** 1.0.0  
**Letzte Aktualisierung:** 2025-12-24  

**Status:** 🟡 Geplant

[← Zurück zur Übersicht](README.md)

---

## 📋 Übersicht

Diese Dokumentation beschreibt das Verhalten und den Ablauf aller Broadcast-Messages, die bei einem Spieler-Disconnect ausgelöst werden. Diese Messages informieren andere Spieler über den Offline-Status in verschiedenen sozialen Kontexten (Party, Guild, Friends).

---

## 🔄 Flow-Diagramm

```
Player A Disconnects
    │
    ├──► Disconnect (5) → Client A
    │
    ├──► PlayerLeftZone (104) → Alle Spieler in Zone
    │
    ├──► PartyMemberOffline (7XX) → Party-Mitglieder (andere Zonen)
    │
    ├──► GuildMemberOffline (8XX) → Guild-Mitglieder (Online)
    │
    └──► FriendOffline (2108) → Freunde (Online)
```

### Sequenz-Details

1. **Disconnect (5)** wird an den betroffenen Client gesendet
2. **PlayerLeftZone (104)** wird an alle Spieler in der gleichen Zone gebroadcastet
3. **Phase 2 Broadcasts** (nur wenn entsprechende Features aktiv):
   - **PartyMemberOffline**: An alle Party-Mitglieder (auch in anderen Zonen)
   - **GuildMemberOffline**: An alle Online-Mitglieder der Guild
   - **FriendOffline (2108)**: An alle Online-Freunde

---

## 📊 MessageType-Tabelle

| Message | ID | Empfänger | Wann gesendet | Phase |
|---------|-----|-----------|---------------|-------|
| **Disconnect** | 5 | Betroffener Client | Immer | Prototyp ✅ |
| **PlayerLeftZone** | 104 | Alle in Zone | Immer | Prototyp ✅ |
| **PartyMemberOffline** | 7XX | Party-Mitglieder | Wenn in Party | Phase 2 🟡 |
| **GuildMemberOffline** | 8XX | Guild-Mitglieder (online) | Wenn in Guild | Phase 2 🟡 |
| **FriendOffline** | 2108 | Freunde (online) | Wenn Freunde online | Phase 2 🟡 |

### Notizen zu IDs

- **Disconnect (5)**: Siehe [00-connection.md](00-connection.md#disconnect-5)
- **PlayerLeftZone (104)**: Siehe [01-zone.md](01-zone.md)
- **FriendOffline (2108)**: Siehe [21-social.md](21-social.md)
- **PartyMemberOffline** und **GuildMemberOffline**: Genaue IDs werden später definiert

---

## 🔧 Payload-Spezifikationen

### Disconnect (5) - Betroffener Client

```csharp
[MessagePackObject]
public class Disconnect : IServerMessage
{
    [Key(0)]
    public MessageType Type => MessageType.Disconnect;
    
    [Key(1)]
    public string Reason { get; set; }          // "TIMEOUT", "KICKED", "BANNED", etc.
    
    [Key(2)]
    public string Message { get; set; }         // Menschenlesbare Nachricht
    
    [Key(3)]
    public bool CanReconnect { get; set; }      // Darf Client reconnecten?
    
    [Key(4)]
    public int ReconnectDelay { get; set; }     // Sekunden bis Reconnect erlaubt
}
```

### PlayerLeftZone (104) - Zone-Broadcast

```csharp
[MessagePackObject]
public class PlayerLeftZone : IServerMessage
{
    [Key(0)]
    public MessageType Type => MessageType.PlayerLeftZone;
    
    [Key(1)]
    public Guid PlayerId { get; set; }          // ID des Spielers
    
    [Key(2)]
    public string PlayerName { get; set; }      // Name des Spielers
}
```

### PartyMemberOffline (7XX) - Party-Broadcast

**Feature**

```csharp
[MessagePackObject]
public class PartyMemberOffline : IServerMessage
{
    [Key(0)]
    public MessageType Type => MessageType.PartyMemberOffline;
    
    [Key(1)]
    public Guid PlayerId { get; set; }                      // ID des Spielers
    
    [Key(2)]
    public string PlayerName { get; set; }                  // Name des Spielers
    
    [Key(3)]
    public DisconnectBroadcastReason Reason { get; set; }   // Grund (Logout/Timeout/Kicked)
}
```

### GuildMemberOffline (8XX) - Guild-Broadcast

**Feature**

```csharp
[MessagePackObject]
public class GuildMemberOffline : IServerMessage
{
    [Key(0)]
    public MessageType Type => MessageType.GuildMemberOffline;
    
    [Key(1)]
    public Guid PlayerId { get; set; }                      // ID des Spielers
    
    [Key(2)]
    public string PlayerName { get; set; }                  // Name des Spielers
    
    [Key(3)]
    public DisconnectBroadcastReason Reason { get; set; }   // Grund (Logout/Timeout/Kicked)
}
```

### FriendOffline (2108) - Friend-Broadcast

**Feature**

```csharp
[MessagePackObject]
public class FriendOffline : IServerMessage
{
    [Key(0)]
    public MessageType Type => MessageType.FriendOffline;
    
    [Key(1)]
    public Guid PlayerId { get; set; }                      // ID des Freundes
    
    [Key(2)]
    public string PlayerName { get; set; }                  // Name des Freundes
    
    [Key(3)]
    public DisconnectBroadcastReason Reason { get; set; }   // Grund (Logout/Timeout/Kicked)
}
```

### DisconnectBroadcastReason Enum

```csharp
/// <summary>
/// Grund für Disconnect-Broadcast an social features.
/// </summary>
public enum DisconnectBroadcastReason : byte
{
    /// <summary>
    /// Normaler Logout durch Client
    /// </summary>
    Logout = 1,
    
    /// <summary>
    /// Connection Timeout (keine Heartbeats)
    /// </summary>
    Timeout = 2,
    
    /// <summary>
    /// Vom Admin gekicked
    /// </summary>
    Kicked = 3
    
    // WICHTIG: Ban wird NICHT gebroadcastet (Privacy)
    // Bei Ban erhalten andere Spieler KEINE Benachrichtigung
}
```

---

## 🔒 Privacy-Regeln

### Ban - KEIN Broadcast

Bei einem **Ban** werden **KEINE** Social-Broadcasts gesendet:
- ❌ Kein `PartyMemberOffline`
- ❌ Kein `GuildMemberOffline`
- ❌ Kein `FriendOffline`

**Begründung:**
- Privacy-Schutz für gebannte Spieler
- Verhindert Stigmatisierung
- Gebannter Spieler erscheint einfach als "Offline"

**Implementierung:**
```csharp
// Bei Ban: Nur Disconnect an Client, keine Social-Broadcasts
if (disconnectReason == DisconnectReason.BANNED)
{
    await SendDisconnectAsync(client, DisconnectReason.BANNED);
    // KEINE Social-Broadcasts!
}
else
{
    await SendDisconnectAsync(client, disconnectReason);
    await BroadcastSocialOfflineAsync(player, MapToReason(disconnectReason));
}
```

### Kick - Reduzierte Information

Bei einem **Admin-Kick**:
- ✅ Social-Broadcasts werden gesendet
- ⚠️ Grund ist nur "Kicked" ohne Details
- ℹ️ Keine Informationen über den Kick-Grund

**Beispiel:**
```csharp
// Party sieht nur: "PlayerName ist offline (Kicked)"
// NICHT: "PlayerName wurde wegen Beleidigung gekicked"
```

### Logout/Timeout - Vollständige Information

Bei **normalen Disconnects**:
- ✅ Alle Social-Broadcasts werden gesendet
- ✅ Vollständiger Grund (Logout oder Timeout)
- ✅ Maximale Transparenz für Party/Guild/Friends

---

## 📝 Beispiel-Szenarien

### Szenario 1: Normaler Logout

**Situation:** Spieler "Alice" loggt sich normal aus

**Ablauf:**
1. Alice sendet `LogoutRequest (3)`
2. Server sendet `Disconnect (5)` an Alice mit Reason="LOGOUT"
3. Server sendet `PlayerLeftZone (104)` an alle in Zone
4. Server sendet `PartyMemberOffline` an Party mit Reason=`Logout`
5. Server sendet `GuildMemberOffline` an Guild mit Reason=`Logout`
6. Server sendet `FriendOffline (2108)` an Freunde mit Reason=`Logout`

**Client-Anzeige:**
- Zone-Spieler: "Alice hat die Zone verlassen"
- Party: "Alice ist offline"
- Guild: "Alice ist offline"
- Freunde: "Alice ist offline"

### Szenario 2: Connection Timeout

**Situation:** Spieler "Bob" verliert Internetverbindung

**Ablauf:**
1. Server erkennt nach 15s fehlende Heartbeats
2. Server sendet `Disconnect (5)` an Bob mit Reason="TIMEOUT" (wird nicht ankommen)
3. Server sendet `PlayerLeftZone (104)` an alle in Zone
4. Server sendet `PartyMemberOffline` an Party mit Reason=`Timeout`
5. Server sendet `GuildMemberOffline` an Guild mit Reason=`Timeout`
6. Server sendet `FriendOffline (2108)` an Freunde mit Reason=`Timeout`

**Client-Anzeige:**
- Zone-Spieler: "Bob hat die Zone verlassen"
- Party: "Bob ist offline (Verbindung unterbrochen)"
- Guild: "Bob ist offline (Verbindung unterbrochen)"
- Freunde: "Bob ist offline (Verbindung unterbrochen)"

### Szenario 3: Admin Kick

**Situation:** Admin kicked Spieler "Charlie"

**Ablauf:**
1. Admin sendet `AdminKick (2304)` an Server
2. Server sendet `Disconnect (5)` an Charlie mit Reason="KICKED"
3. Server sendet `PlayerLeftZone (104)` an alle in Zone
4. Server sendet `PartyMemberOffline` an Party mit Reason=`Kicked`
5. Server sendet `GuildMemberOffline` an Guild mit Reason=`Kicked`
6. Server sendet `FriendOffline (2108)` an Freunde mit Reason=`Kicked`

**Client-Anzeige:**
- Zone-Spieler: "Charlie hat die Zone verlassen"
- Party: "Charlie ist offline"
- Guild: "Charlie ist offline"
- Freunde: "Charlie ist offline"
- ⚠️ **KEIN** Hinweis auf Kick (nur für Charlie sichtbar)

### Szenario 4: Ban (KEIN Broadcast)

**Situation:** Admin bannt Spieler "Dave"

**Ablauf:**
1. Admin sendet `AdminBan (2305)` an Server
2. Server sendet `Disconnect (5)` an Dave mit Reason="BANNED"
3. Server sendet `PlayerLeftZone (104)` an alle in Zone
4. ❌ **KEIN** `PartyMemberOffline` (Privacy)
5. ❌ **KEIN** `GuildMemberOffline` (Privacy)
6. ❌ **KEIN** `FriendOffline` (Privacy)

**Client-Anzeige:**
- Zone-Spieler: "Dave hat die Zone verlassen"
- Party: Dave erscheint einfach als "Offline" (ohne Benachrichtigung)
- Guild: Dave erscheint einfach als "Offline" (ohne Benachrichtigung)
- Freunde: Dave erscheint einfach als "Offline" (ohne Benachrichtigung)

**Privacy-Schutz:**
- Andere Spieler erfahren NICHT, dass Dave gebannt wurde
- Dave wird in Listen als "Offline" angezeigt
- Kein "toast notification" oder andere Hinweise

---

## 🔗 Verwandte Dokumentation

- **[Connection Messages (00-connection.md)](00-connection.md)** - Disconnect (5) Details
- **[Zone Messages (01-zone.md)](01-zone.md)** - PlayerLeftZone (104) Details
- **[Social Messages (21-social.md)](21-social.md)** - FriendOffline (2108) Details
- **[Party Messages (07-party.md)](07-party.md)** - PartyMemberOffline (geplant)
- **[Guild Messages (08-guild.md)](08-guild.md)** - GuildMemberOffline (geplant)
- **[Admin Messages (23-admin.md)](23-admin.md)** - AdminKick, AdminBan

---

## 📌 Implementierungs-Hinweise

### Broadcast-Reihenfolge

Die Broadcasts sollten in folgender Reihenfolge gesendet werden:

1. **Disconnect** an Client (garantiert erste Message)
2. **PlayerLeftZone** an Zone (sofort sichtbar)
3. **Social-Broadcasts** (Party/Guild/Friends) parallel

### Performance-Überlegungen

- Social-Broadcasts können **parallel** gesendet werden (keine Abhängigkeit)
- Bei großen Guilds (100+ Mitglieder): Batch-Processing erwägen
- Redis Pub/Sub für cross-server broadcasts (geplant)

### Error-Handling

**Was passiert wenn...**

| Situation | Verhalten |
|-----------|-----------|
| Client ist bereits disconnected | Ignore `Disconnect` (5), sende trotzdem Broadcasts |
| Party ist leer geworden | Löse Party automatisch auf |
| Guild-Member-Offline fehlschlägt | Logge Fehler, aber stoppe Disconnect NICHT |
| Friend ist bereits offline | Ignore, kein Broadcast nötig |

### Testing-Checkliste

Für Tests der Disconnect-Broadcasts:

- [ ] Normaler Logout sendet alle Broadcasts
- [ ] Timeout sendet alle Broadcasts mit Reason=`Timeout`
- [ ] Admin-Kick sendet Broadcasts mit Reason=`Kicked`
- [ ] Ban sendet **KEINE** Social-Broadcasts
- [ ] PlayerLeftZone wird immer gesendet (auch bei Ban)
- [ ] Broadcasts erreichen nur Online-Spieler
- [ ] Cross-Zone Broadcasts funktionieren (Party in anderer Zone)

---

## 🔗 Issues & Pull Requests

- **Related Issue:** [#182](https://github.com/MatTrinkl/2DMMO/issues/182) - Implementierung Disconnect Broadcasts
- **Related Issue:** [#183](https://github.com/MatTrinkl/2DMMO/issues/183) - Dokumentation Disconnect Broadcasts

---

**Letzte Aktualisierung**: 2025-12-24  
**Version**: 1.0.0  
**Phase**: Phase 2

[← Zurück zur Übersicht](README.md)
