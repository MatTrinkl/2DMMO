# 📨 Message Reference Documentation

**Version:** 1.0.0  
**Letzte Aktualisierung:** 2025-12-17  
**Teil von:** [Dokumentation](../README.md) | [Architektur](../02-architecture/README.md)  
**Siehe auch:** [Message-Spezifikation](../02-architecture/MESSAGES.md) | [Issue #140](https://github.com/MatTrinkl/2DMMO/issues/140)

---

## 📋 Übersicht

Diese Dokumentation bietet eine vollständige Referenz für **alle MessageTypes** im 2DMMO-Projekt. Jede Message ist detailliert dokumentiert mit:

- ✅ Was die Message tut (Im Scope)
- ❌ Was sie NICHT tut (Nicht im Scope)
- 📤 Richtung (Client→Server, Server→Client, Broadcast)
- 🔒 Authentifizierungs-Anforderungen
- 📊 Request/Response Payload-Struktur
- 🔗 Verwandte Messages
- 💡 Beispiel-Code und Error-Handling

---

## 🗂️ Message-Kategorien

Das 2DMMO verwendet ein **100-Block-System** für O(1) Message-Routing:
- `Category = MessageType / 100`
- Jede Kategorie hat 100 Message-IDs (z.B. Connection: 0-99, Zone: 100-199)
- Ermöglicht schnelles Dispatching ohne Hash-Lookups

### Prototyp-Phase (In Entwicklung)

| Kategorie | Range | Datei | Messages | Status |
|-----------|-------|-------|----------|--------|
| **Connection** | 0000-0099 | [00-connection.md](00-connection.md) | 20 | 🟢 Prototyp |
| **Zone** | 0100-0199 | [01-zone.md](01-zone.md) | 17 | 🟢 Prototyp |
| **Movement** | 0200-0299 | [02-movement.md](02-movement.md) | 20 | 🟢 Prototyp |
| **Chat** | 0400-0499 | [04-chat.md](04-chat.md) | 31 | 🟢 Prototyp |
| **System** | 0900-0999 | [09-system.md](09-system.md) | 26 | 🟢 Prototyp |

### Phase 2 (Geplant)

| Kategorie | Range | Datei | Messages | Status |
|-----------|-------|-------|----------|--------|
| **Combat** | 0300-0399 | [03-combat.md](03-combat.md) | 33 | 🟡 Phase 2 |
| **Inventory** | 0500-0599 | [05-inventory.md](05-inventory.md) | 36 | 🟡 Phase 2 |
| **Character** | 0600-0699 | [06-character.md](06-character.md) | 25 | 🟡 Phase 2 |
| **Party** | 0700-0799 | [07-party.md](07-party.md) | 26 | 🟡 Phase 2 |
| **Guild** | 0800-0899 | [08-guild.md](08-guild.md) | 39 | 🟡 Phase 2 |
| **Quest** | 1000-1099 | [10-quest.md](10-quest.md) | 24 | 🟡 Phase 2 |
| **Trading** | 1100-1199 | [11-trading.md](11-trading.md) | 14 | 🟡 Phase 2 |
| **Targeting** | 1200-1299 | [12-targeting.md](12-targeting.md) | 17 | 🟡 Phase 2 |
| **NPC** | 1300-1399 | [13-npc.md](13-npc.md) | 38 | 🟡 Phase 2 |
| **Entity** | 1400-1499 | [14-entity.md](14-entity.md) | 35 | 🟡 Phase 2 |
| **Aura** | 1500-1599 | [15-aura.md](15-aura.md) | 17 | 🟡 Phase 2 |
| **Social** | 2100-2199 | [21-social.md](21-social.md) | 13 | 🟡 Phase 2 |
| **Admin** | 2300-2399 | [23-admin.md](23-admin.md) | 37 | 🟡 Phase 2 |
| **Instance** | 2400-2499 | [24-instance.md](24-instance.md) | 33 | 🟡 Phase 2 |
| **PvP** | 2500-2599 | [25-pvp.md](25-pvp.md) | 32 | 🟡 Phase 2 |
| **World** | 2600-2699 | [26-world.md](26-world.md) | 22 | 🟡 Phase 2 |
| **Tutorial** | 2900-2999 | [29-tutorial.md](29-tutorial.md) | 13 | 🟡 Phase 2 |
| **Loot** | 3100-3199 | [31-loot.md](31-loot.md) | 17 | 🟡 Phase 2 |
| **Cooldown** | 3200-3299 | [32-cooldown.md](32-cooldown.md) | 20 | 🟡 Phase 2 |
| **Map** | 3400-3499 | [34-map.md](34-map.md) | 24 | 🟡 Phase 2 |
| **Reporting** | 3600-3699 | [36-reporting.md](36-reporting.md) | 16 | 🟡 Phase 2 |
| **Economy** | 3700-3799 | [37-economy.md](37-economy.md) | 14 | 🟡 Phase 2 |
| **Skill** | 3800-3899 | [38-skill.md](38-skill.md) | 24 | 🟡 Phase 2 |
| **Equipment** | 3900-3999 | [39-equipment.md](39-equipment.md) | 24 | 🟡 Phase 2 |
| **Death** | 4100-4199 | [41-death.md](41-death.md) | 24 | 🟡 Phase 2 |
| **Notification** | 4300-4399 | [43-notification.md](43-notification.md) | 24 | 🟡 Phase 2 |

### Phase 3 (Zukünftig)

| Kategorie | Range | Datei | Messages | Status |
|-----------|-------|-------|----------|--------|
| **Crafting** | 1600-1699 | [16-crafting.md](16-crafting.md) | 25 | 🔵 Phase 3 |
| **Auction** | 1700-1799 | [17-auction.md](17-auction.md) | 21 | 🔵 Phase 3 |
| **Mail** | 1800-1899 | [18-mail.md](18-mail.md) | 17 | 🔵 Phase 3 |
| **Achievement** | 1900-1999 | [19-achievement.md](19-achievement.md) | 15 | 🔵 Phase 3 |
| **Mount** | 2000-2099 | [20-mount.md](20-mount.md) | 25 | 🔵 Phase 3 |
| **Emote** | 2200-2299 | [22-emote.md](22-emote.md) | 23 | 🔵 Phase 3 |
| **Matchmaking** | 2700-2799 | [27-matchmaking.md](27-matchmaking.md) | 13 | 🔵 Phase 3 |
| **Leaderboard** | 2800-2899 | [28-leaderboard.md](28-leaderboard.md) | 14 | 🔵 Phase 3 |
| **Settings** | 3000-3099 | [30-settings.md](30-settings.md) | 12 | 🔵 Phase 3 |
| **Inspection** | 3300-3399 | [33-inspection.md](33-inspection.md) | 21 | 🔵 Phase 3 |
| **Voice** | 3500-3599 | [35-voice.md](35-voice.md) | 14 | 🔵 Phase 3 |
| **Bank** | 4000-4099 | [40-bank.md](40-bank.md) | 19 | 🔵 Phase 3 |
| **Transportation** | 4200-4299 | [42-transportation.md](42-transportation.md) | 28 | 🔵 Phase 3 |
| **Cutscene** | 4400-4499 | [44-cutscene.md](44-cutscene.md) | 6 | 🔵 Phase 3 |
| **Housing** | 4500-4599 | [45-housing.md](45-housing.md) | 6 | 🔵 Phase 3 |
| **Event** | 4600-4699 | [46-event.md](46-event.md) | 5 | 🔵 Phase 3 |

### Reserviert & Development

| Kategorie | Range | Datei | Messages | Status |
|-----------|-------|-------|----------|--------|
| **Reserved** | 4700-4799 | [47-reserved.md](47-reserved.md) | - | ⚪ Reserviert |
| **Reserved** | 4800-4899 | [48-reserved.md](48-reserved.md) | - | ⚪ Reserviert |
| **Debug** | 4900-4999 | [49-debug.md](49-debug.md) | 5 | 🟣 Dev |

### Server-to-Server (Internal)

⚠️ **Wichtig**: Diese Messages sind **AUSSCHLIESSLICH für Server-zu-Server Kommunikation**. Clients senden oder empfangen diese Messages **NIEMALS**.

| Kategorie | Range | Datei | Messages | Status |
|-----------|-------|-------|----------|--------|
| **S2S Core** | 5000-5099 | [50-server-to-server.md](50-server-to-server.md) | 7 | 🟡 Phase 2 |
| **S2S Transfer** | 5100-5199 | [50-server-to-server.md](50-server-to-server.md) | 9 | 🟡 Phase 2 |
| **S2S Cross-Zone** | 5200-5299 | [50-server-to-server.md](50-server-to-server.md) | 9 | 🟡 Phase 2 |
| **S2S Matchmaking** | 5300-5399 | [50-server-to-server.md](50-server-to-server.md) | 10 | 🟡 Phase 2 |
| **S2S Economy** | 5400-5499 | [50-server-to-server.md](50-server-to-server.md) | 7 | 🔵 Phase 3 |
| **S2S Admin** | 5500-5599 | [50-server-to-server.md](50-server-to-server.md) | 12 | 🟡 Phase 2 |

**Siehe auch:** [SERVER_TO_SERVER.md](../02-architecture/SERVER_TO_SERVER.md) für vollständige S2S-Architektur-Dokumentation

---

## ➕ Neue Messages Hinzufügen

### Automatische Registrierung

Seit Version 1.2.0 verwendet der `MessageSerializer` ein **attribute-basiertes Auto-Registrierungs-System**. Neue Message-Types benötigen **KEINE** manuellen Änderungen am MessageSerializer mehr.

### Schritt-für-Schritt Anleitung

1. **Message-Type in Enum definieren** (`shared/Mmo.Shared/Messaging/Enums/MessageType.cs`):
   ```csharp
   public enum MessageType : byte
   {
       // ... existing types ...
       NewMessageType = 123,  // Wähle freie ID in passender Kategorie
   }
   ```

2. **Korrekte Interface-Wahl** - **WICHTIG für Security**:
   - **Client → Server** (Input, Requests): Verwende `IClientMessage` oder `ITimestampedClientMessage`
   - **Server → Client** (Responses, State): Verwende `IServerMessage` oder `ITimestampedServerMessage`
   - **NIE** bidirektional - jede Message hat exakt EINE Richtung

3. **Message-Klasse erstellen** mit `[NetworkMessage]` Attribut:
   ```csharp
   using MessagePack;
   using Mmo.Shared.Messaging.Attributes;
   using Mmo.Shared.Messaging.Enums;
   using Mmo.Shared.Messaging.Interfaces;
   
   namespace Mmo.Shared.YourCategory.Messages;
   
   [MessagePackObject]
   [NetworkMessage(MessageType.NewMessageType)]  // ← Auto-Registrierung
   public class NewMessageType : IClientMessage  // ← Korrekte Interface-Wahl!
   {
       [Key(0)]
       public MessageType Type => MessageType.NewMessageType;
       
       [Key(1)]
       public string SomeField { get; set; }
       
       [Key(2)]
       public int AnotherField { get; set; }
   }
   ```

4. **Fertig!** Die Message wird beim Programmstart automatisch registriert.

### Wichtige Anforderungen

✅ **MUSS vorhanden sein:**
- `[MessagePackObject]` Attribut auf der Klasse
- `[NetworkMessage(MessageType.XXX)]` Attribut auf der Klasse
- **Korrektes Interface**: `IClientMessage` (Client→Server) ODER `IServerMessage` (Server→Client)
- Für timestamped Messages: `ITimestampedClientMessage` oder `ITimestampedServerMessage`
- `Type` Property mit `[Key(0)]` Attribut
- Alle Properties mit aufsteigenden `[Key(n)]` Attributen

❌ **NICHT MEHR nötig:**
- ~~MessageSerializer.Deserialize() erweitern~~
- ~~Switch-Case Statement updaten~~
- ~~Manuelle Registrierung~~

### Performance

- **O(1) Lookup** per Dictionary
- **Compiled Expression Delegates** für Near-Native Performance
- **Validation beim Start**: Duplikate und fehlende Attribute werden erkannt

### Fehlerbehebung

**Fehler: "Type XXX has [NetworkMessage] but does not implement INetworkMessage"**
→ Füge `INetworkMessage` Interface hinzu

**Fehler: "Following types implement INetworkMessage but are missing [NetworkMessage] attribute"**
→ Füge `[NetworkMessage(MessageType.XXX)]` Attribut hinzu

**Fehler: "Duplicate MessageType registration detected"**
→ Zwei Klassen verwenden den gleichen MessageType - wähle eine andere ID

---

## 🔍 Schnellsuche

### Nach Funktion

- **Verbindung & Login**: [Connection](00-connection.md) | [System](09-system.md)
- **Welt & Bewegung**: [Zone](01-zone.md) | [Movement](02-movement.md) | [World](26-world.md)
- **Kampf & Schaden**: [Combat](03-combat.md) | [Targeting](12-targeting.md) | [Death](41-death.md)
- **Kommunikation**: [Chat](04-chat.md) | [Social](21-social.md) | [Voice](35-voice.md)
- **Spieler-Progression**: [Character](06-character.md) | [Quest](10-quest.md) | [Achievement](19-achievement.md)
- **Items & Wirtschaft**: [Inventory](05-inventory.md) | [Trading](11-trading.md) | [Auction](17-auction.md) | [Economy](37-economy.md)
- **Gruppen-Aktivitäten**: [Party](07-party.md) | [Guild](08-guild.md) | [Instance](24-instance.md)
- **NPC-Interaktion**: [NPC](13-npc.md) | [Entity](14-entity.md)

### Nach Entwicklungsphase

- **✅ Implementiert (Prototyp)**: Connection, Zone, Movement, Chat, System
- **🔨 In Arbeit (Phase 2)**: Combat, Inventory, Character, Party, Guild, Quest, Trading, Targeting, NPC, Entity, Aura, Social, Admin, Instance, PvP, World, Tutorial, Loot, Cooldown, Map, Reporting, Economy, Skill, Equipment, Death, Notification
- **📋 Geplant (Phase 3)**: Crafting, Auction, Mail, Achievement, Mount, Emote, Matchmaking, Leaderboard, Settings, Inspection, Voice, Bank, Transportation, Cutscene, Housing, Event

---

## 📖 Verwendung dieser Dokumentation

### Für Entwickler

Wenn du eine Message implementierst:
1. Öffne die passende Kategorie-Datei (z.B. `03-combat.md` für Kampf-Messages)
2. Finde die Message anhand der ID (z.B. `DamageEvent (302)`)
3. Lies die **Im Scope** und **Nicht im Scope** Abschnitte
4. Implementiere entsprechend dem Payload-Schema
5. Verwende die Beispiele als Vorlage

### Für Designer

Wenn du eine neue Feature-Anforderung hast:
1. Suche nach der passenden Kategorie
2. Prüfe ob eine Message bereits existiert, die dein Feature abdeckt
3. Wenn nicht, prüfe **Nicht im Scope** Abschnitte - vielleicht ist es absichtlich ausgeschlossen
4. Erstelle ein Issue mit Referenz zur Message-Kategorie

### Für Tester

Wenn du einen Bug findest:
1. Identifiziere die betroffene Message(s)
2. Prüfe die **Expected Response** und **Error Codes** Abschnitte
3. Stelle fest ob das Verhalten der Dokumentation entspricht
4. Wenn nicht, erstelle ein Bug-Report mit Message-Referenz

---

## 🎨 Dokumentations-Format

Jede Message folgt diesem Template:

```markdown
## MessageName (ID)

**Richtung:** 📤 Client → Server | 📥 Server → Client | 📡 Broadcast
**Frequenz:** Einmalig | Selten | Häufig | ⚡ High-Frequency
**Authentifizierung:** 🔒 Ja | Nein
**Spezielle Rechte:** 👑 [Welche] | Keine

### Beschreibung
[2-3 Sätze was die Message macht]

### Im Scope ✅
- Feature A
- Feature B

### Nicht im Scope ❌
- Anderes Feature → verwende `OtherMessage` (ID)

### Request/Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|

### Erwartete Response
- **Bei Erfolg:** `SuccessMessage` (ID)
- **Bei Fehler:** `ErrorMessage` (910)

### Verwandte Messages
| Message | ID | Beziehung |

### Beispiel Payload
```csharp
var message = new MessageName { ... };
```
```

**Hinweis zu Richtungen:**
- **📤 Client → Server**: Client sendet Request/Input an Server (verwendet `IClientMessage`)
- **📥 Server → Client**: Server sendet Response/State an Client (verwendet `IServerMessage`)
- **📡 Broadcast**: Server sendet an mehrere Clients gleichzeitig (verwendet `IServerMessage`)
- **🚫 KEINE bidirektionalen Messages** - jede Message hat exakt EINE Richtung!

---

## 🔗 Verwandte Dokumentation

- **[Message-Spezifikation](../02-architecture/MESSAGES.md)** - Technische Details zum Message-System
- **[Netzwerk-Protokoll](../02-architecture/NETWORK_PROTOCOL.md)** - Transport und Framing
- **[Client-Server Sync](../02-architecture/CLIENT_SERVER_SYNC.md)** - Message Processing
- **[Sicherheit](../02-architecture/SECURITY.md)** - Input Validation für Messages
- **[Game Loop](../02-architecture/GAME_LOOP.md)** - Wann Messages verarbeitet werden
- **[Disconnect Broadcasts](DISCONNECT_BROADCASTS.md)** - Broadcast-Messages bei Spieler-Disconnects
- **[Server-zu-Server Kommunikation](../02-architecture/SERVER_TO_SERVER.md)** - S2S-Architektur & Load-Balancing

---

## 📊 Statistiken

- **Gesamt Messages**: ~1170
- **Prototyp (Implementiert)**: 114 Messages
- **Phase 2 (In Arbeit)**: 674 Messages (inkl. 54 S2S)
- **Phase 3 (Geplant)**: 332 Messages (inkl. 12 S2S)
- **Server-to-Server (S2S)**: 70 Messages (5000-5999)
- **Reserviert**: 200 IDs
- **Debug**: 5 Messages

---

**Letzte Aktualisierung**: 2025-12-25  
**Version**: 2.0.0  
**Maintainer**: 2DMMO Team
