# 📨 Message Reference Documentation

**Version:** 2.1.0  
**Last Updated:** 2026-01-01  
**Part of:** [Documentation](../README.md) | [Architecture](../02-architecture/README.md)  
**See also:** [Message Specification](../02-architecture/MESSAGES.md) | [Issue #140](https://github.com/MatTrinkl/2DMMO/issues/140)

---

## 📋 Overview

This documentation provides a complete reference for **all MessageTypes** in the 2DMMO project. Each message is documented in detail with:

- ✅ What the message does (In Scope)
- ❌ What it does NOT do (Not in Scope)
- 📤 Direction (Client→Server, Server→Client, Broadcast)
- 🔒 Authentication requirements
- 📊 Request/Response payload structure
- 🔗 Related messages
- 💡 Example code and error handling

---

## 🗂️ Message Categories

2DMMO uses a **100-block system** for O(1) message routing:
- `Category = MessageType / 100`
- Each category has 100 message IDs (e.g., Connection: 0-99, Zone: 100-199)
- Enables fast dispatching without hash lookups

### All Categories

> **Hinweis:** Die Kategorisierung erfolgt ausschließlich nach **Funktionsbereich** (Category Range), nicht nach Phasen. Features werden separat im Projekt geplant.

| Category | Range | File | Messages |
|----------|-------|------|----------|
| **Connection** | 0000-0099 | [00-connection.md](00-connection.md) | 24 |
| **Zone** | 0100-0199 | [01-zone.md](01-zone.md) | 19 |
| **Movement** | 0200-0299 | [02-movement.md](02-movement.md) | 22 |
| **Combat** | 0300-0399 | [03-combat.md](03-combat.md) | 35 |
| **Chat** | 0400-0499 | [04-chat.md](04-chat.md) | 37 |
| **Inventory** | 0500-0599 | [05-inventory.md](05-inventory.md) | 44 |
| **Character** | 0600-0699 | [06-character.md](06-character.md) | 30 |
| **Party** | 0700-0799 | [07-party.md](07-party.md) | 33 |
| **Guild** | 0800-0899 | [08-guild.md](08-guild.md) | 50 |
| **System** | 0900-0999 | [09-system.md](09-system.md) | 26 |
| **Quest** | 1000-1099 | [10-quest.md](10-quest.md) | 24 |
| **Trading** | 1100-1199 | [11-trading.md](11-trading.md) | 14 |
| **Targeting** | 1200-1299 | [12-targeting.md](12-targeting.md) | 17 |
| **NPC** | 1300-1399 | [13-npc.md](13-npc.md) | 38 |
| **Entity** | 1400-1499 | [14-entity.md](14-entity.md) | 35 |
| **Aura** | 1500-1599 | [15-aura.md](15-aura.md) | 17 |
| **Crafting** | 1600-1699 | [16-crafting.md](16-crafting.md) | 25 |
| **Auction** | 1700-1799 | [17-auction.md](17-auction.md) | 21 |
| **Mail** | 1800-1899 | [18-mail.md](18-mail.md) | 17 |
| **Achievement** | 1900-1999 | [19-achievement.md](19-achievement.md) | 15 |
| **Mount** | 2000-2099 | [20-mount.md](20-mount.md) | 25 |
| **Social** | 2100-2199 | [21-social.md](21-social.md) | 13 |
| **Emote** | 2200-2299 | [22-emote.md](22-emote.md) | 23 |
| **Admin** | 2300-2399 | [23-admin.md](23-admin.md) | 37 |
| **Instance** | 2400-2499 | [24-instance.md](24-instance.md) | 33 |
| **PvP** | 2500-2599 | [25-pvp.md](25-pvp.md) | 32 |
| **World** | 2600-2699 | [26-world.md](26-world.md) | 22 |
| **Matchmaking** | 2700-2799 | [27-matchmaking.md](27-matchmaking.md) | 13 |
| **Leaderboard** | 2800-2899 | [28-leaderboard.md](28-leaderboard.md) | 14 |
| **Tutorial** | 2900-2999 | [29-tutorial.md](29-tutorial.md) | 13 |
| **Settings** | 3000-3099 | [30-settings.md](30-settings.md) | 12 |
| **Loot** | 3100-3199 | [31-loot.md](31-loot.md) | 17 |
| **Cooldown** | 3200-3299 | [32-cooldown.md](32-cooldown.md) | 20 |
| **Inspection** | 3300-3399 | [33-inspection.md](33-inspection.md) | 21 |
| **Map** | 3400-3499 | [34-map.md](34-map.md) | 24 |
| **Voice** | 3500-3599 | [35-voice.md](35-voice.md) | 14 |
| **Reporting** | 3600-3699 | [36-reporting.md](36-reporting.md) | 16 |
| **Economy** | 3700-3799 | [37-economy.md](37-economy.md) | 14 |
| **Skill** | 3800-3899 | [38-skill.md](38-skill.md) | 24 |
| **Equipment** | 3900-3999 | [39-equipment.md](39-equipment.md) | 24 |
| **Bank** | 4000-4099 | [40-bank.md](40-bank.md) | 19 |
| **Death** | 4100-4199 | [41-death.md](41-death.md) | 24 |
| **Transportation** | 4200-4299 | [42-transportation.md](42-transportation.md) | 28 |
| **Notification** | 4300-4399 | [43-notification.md](43-notification.md) | 24 |
| **Cutscene** | 4400-4499 | [44-cutscene.md](44-cutscene.md) | 6 |
| **Housing** | 4500-4599 | [45-housing.md](45-housing.md) | 6 |
| **Event** | 4600-4699 | [46-event.md](46-event.md) | 5 |

### Reserved & Development

| Category | Range | File | Messages | Status |
|----------|-------|------|----------|--------|
| **Reserved** | 4700-4799 | [47-reserved.md](47-reserved.md) | - | ⚪ Reserved |
| **Reserved** | 4800-4899 | [48-reserved.md](48-reserved.md) | - | ⚪ Reserved |
| **Debug** | 4900-4999 | [49-debug.md](49-debug.md) | 5 | 🟣 Dev |

### Server-to-Server (Internal)

⚠️ **Important**: Diese Messages sind **ausschließlich für Server-zu-Server-Kommunikation**. Clients senden oder empfangen diese Messages **niemals**.

| Category | Range | File | Messages |
|----------|-------|------|----------|
| **S2S Core** | 5000-5099 | [50-server-to-server.md](50-server-to-server.md) | 7 |
| **S2S Transfer** | 5100-5199 | [50-server-to-server.md](50-server-to-server.md) | 9 |
| **S2S Cross-Zone** | 5200-5299 | [50-server-to-server.md](50-server-to-server.md) | 9 |
| **S2S Matchmaking** | 5300-5399 | [50-server-to-server.md](50-server-to-server.md) | 10 |
| **S2S Economy** | 5400-5499 | [50-server-to-server.md](50-server-to-server.md) | 7 |
| **S2S Admin** | 5500-5599 | [50-server-to-server.md](50-server-to-server.md) | 12 |

**See also:** [SERVER_TO_SERVER.md](../02-architecture/SERVER_TO_SERVER.md) for full S2S architecture documentation

---

## ➕ Adding New Messages

### Automatic Registration

Since version 1.2.0, the `MessageSerializer` uses an **attribute-based auto-registration system**. New message types require **NO** manual changes to the MessageSerializer anymore.

### Step-by-Step Guide

1. **Define message type in Enum** (`shared/Mmo.Shared/Messaging/Enums/MessageType.cs`):
   ```csharp
   public enum MessageType : byte
   {
       // ... existing types ...
       NewMessageType = 123,  // Choose free ID in appropriate category
   }
   ```

2. **Choose correct Interface** - **IMPORTANT for Security**:
   - **Client → Server** (Input, Requests): Use `IClientMessage` or `ITimestampedClientMessage`
   - **Server → Client** (Responses, State): Use `IServerMessage` or `ITimestampedServerMessage`
   - **NEVER** bidirectional - each message has exactly ONE direction

3. **Create message class** with `[NetworkMessage]` attribute:
   ```csharp
   using MessagePack;
   using Mmo.Shared.Messaging.Attributes;
   using Mmo.Shared.Messaging.Enums;
   using Mmo.Shared.Messaging.Interfaces;
   
   namespace Mmo.Shared.YourCategory.Messages;
   
   [MessagePackObject]
   [NetworkMessage(MessageType.NewMessageType)]  // ← Auto-registration
   public class NewMessageType : IClientMessage  // ← Correct interface choice!
   {
       [Key(0)]
       public MessageType Type => MessageType.NewMessageType;
       
       [Key(1)]
       public string SomeField { get; set; }
       
       [Key(2)]
       public int AnotherField { get; set; }
   }
   ```

4. **Done!** The message will be automatically registered at program startup.

### Important Requirements

✅ **MUST be present:**
- `[MessagePackObject]` attribute on the class
- `[NetworkMessage(MessageType.XXX)]` attribute on the class
- **Correct Interface**: `IClientMessage` (Client→Server) OR `IServerMessage` (Server→Client)
- For timestamped messages: `ITimestampedClientMessage` or `ITimestampedServerMessage`
- `Type` property with `[Key(0)]` attribute
- All properties with ascending `[Key(n)]` attributes

❌ **NO LONGER needed:**
- ~~Extend MessageSerializer.Deserialize()~~
- ~~Update switch-case statement~~
- ~~Manual registration~~

### Performance

- **O(1) Lookup** via Dictionary
- **Compiled Expression Delegates** for near-native performance
- **Validation at startup**: Duplicates and missing attributes are detected

### Troubleshooting

**Error: "Type XXX has [NetworkMessage] but does not implement INetworkMessage"**
→ Add `INetworkMessage` interface

**Error: "Following types implement INetworkMessage but are missing [NetworkMessage] attribute"**
→ Add `[NetworkMessage(MessageType.XXX)]` attribute

**Error: "Duplicate MessageType registration detected"**
→ Two classes use the same MessageType - choose a different ID

---

## 🔄 DTO System

For Server→Client messages, DTOs (Data Transfer Objects) are used to:
- Protect server-only data (`[ServerOnly]` attributes)
- Guarantee consistent mappings
- Ensure compile-time safety

See [DTO_ARCHITECTURE.md](DTO_ARCHITECTURE.md) for details.

### Available DTOs

| DTO | Entity | Used in |
|-----|--------|---------|
| `CharacterEntityDto` | `CharacterEntity` | ZoneState (102), CharacterJoinedZone |
| `NpcEntityDto` | `NpcEntity` | ZoneState (102), EntitySpawn (1400) - 🟡 Planned |
| `EntityDtoUnion` | Union interface | ZoneState polymorphic list |

**Future DTOs (planned):**
- `CombatEntityDto` - for Damage/Heal Events (Combat Messages)
- `PartyMemberDto` - for Party lists (Party Messages)
- `TargetEntityDto` - for Target Frame (Targeting Messages)
- `CharacterListItemDto` - for Character Selection (Character Messages)

---

## 🔄 Zone Loading Flow

The zone loading process has been simplified:

| Step | Message | Description |
|------|---------|-------------|
| 1 | `CharacterSelectResponse` | Server communicates SpawnZoneId |
| 2 | `GetZoneRequest` (117) | Client requests zone |
| 3 | `ZoneState` (102) | Server sends EVERYTHING (Zone + MyPlayer + Entities) |
| 4 | `ZoneLoadedAck` (119) | Client confirms readiness |
| 5 | Updates start | PositionBroadcast, EntityUpdates, etc. |

See [01-zone.md](01-zone.md) for details.

### Obsolete Messages

| Message | ID | Replaced by |
|---------|-----|-------------|
| `GetZoneResponse` | 118 | `ZoneState` (102) |
| `JoinZone` | 100 | `ZoneState.MyPlayer` |

---

## 🔍 Quick Search

### By Function

- **Connection & Login**: [Connection](00-connection.md) | [System](09-system.md)
- **World & Movement**: [Zone](01-zone.md) | [Movement](02-movement.md) | [World](26-world.md)
- **Combat & Damage**: [Combat](03-combat.md) | [Targeting](12-targeting.md) | [Death](41-death.md)
- **Communication**: [Chat](04-chat.md) | [Social](21-social.md) | [Voice](35-voice.md)
- **Player Progression**: [Character](06-character.md) | [Quest](10-quest.md) | [Achievement](19-achievement.md)
- **Items & Economy**: [Inventory](05-inventory.md) | [Trading](11-trading.md) | [Auction](17-auction.md) | [Economy](37-economy.md)
- **Group Activities**: [Party](07-party.md) | [Guild](08-guild.md) | [Instance](24-instance.md)
- **NPC Interaction**: [NPC](13-npc.md) | [Entity](14-entity.md)

### By Development Phase

- **✅ Implemented (Prototype)**: Connection, Zone, Movement, Chat, System
- **🔨 In Progress (Phase 2)**: Combat, Inventory, Character, Party, Guild, Quest, Trading, Targeting, NPC, Entity, Aura, Social, Admin, Instance, PvP, World, Tutorial, Loot, Cooldown, Map, Reporting, Economy, Skill, Equipment, Death, Notification
- **📋 Planned (Phase 3)**: Crafting, Auction, Mail, Achievement, Mount, Emote, Matchmaking, Leaderboard, Settings, Inspection, Voice, Bank, Transportation, Cutscene, Housing, Event

---

## 📖 Using This Documentation

### For Developers

When implementing a message:
1. Open the appropriate category file (e.g., `03-combat.md` for combat messages)
2. Find the message by ID (e.g., `DamageEvent (302)`)
3. Read the **In Scope** and **Not in Scope** sections
4. Implement according to the payload schema
5. Use the examples as a template

### For Designers

When you have a new feature requirement:
1. Search for the appropriate category
2. Check if a message already exists that covers your feature
3. If not, check **Not in Scope** sections - perhaps it's intentionally excluded
4. Create an issue with reference to the message category

### For Testers

When you find a bug:
1. Identify the affected message(s)
2. Check the **Expected Response** and **Error Codes** sections
3. Determine if the behavior matches the documentation
4. If not, create a bug report with message reference

---

## 🎨 Documentation Format

Each message follows this template:

```markdown
## MessageName (ID)

**Direction:** 📤 Client → Server | 📥 Server → Client | 📡 Broadcast
**Frequency:** Once | Rare | Frequent | ⚡ High-Frequency
**Authentication:** 🔒 Yes | No
**Special Permissions:** 👑 [Which] | None

### Description
[2-3 sentences about what the message does]

### In Scope ✅
- Feature A
- Feature B

### Not in Scope ❌
- Other feature → use `OtherMessage` (ID)

### Request/Response Payload
| Field | Type | Description | Required |
|-------|------|-------------|----------|

### Expected Response
- **On Success:** `SuccessMessage` (ID)
- **On Error:** `ErrorMessage` (910)

### Related Messages
| Message | ID | Relationship |

### Example Payload
```csharp
var message = new MessageName { ... };
```
```

**Note on Directions:**
- **📤 Client → Server**: Client sends request/input to server (uses `IClientMessage`)
- **📥 Server → Client**: Server sends response/state to client (uses `IServerMessage`)
- **📡 Broadcast**: Server sends to multiple clients simultaneously (uses `IServerMessage`)
- **🚫 NO bidirectional messages** - each message has exactly ONE direction!

---

## 🔗 Related Documentation

- **[Message Specification](../02-architecture/MESSAGES.md)** - Technical details of the message system
- **[Network Protocol](../02-architecture/NETWORK_PROTOCOL.md)** - Transport and framing
- **[Client-Server Sync](../02-architecture/CLIENT_SERVER_SYNC.md)** - Message processing
- **[Security](../02-architecture/SECURITY.md)** - Input validation for messages
- **[Game Loop](../02-architecture/GAME_LOOP.md)** - When messages are processed
- **[Disconnect Broadcasts](DISCONNECT_BROADCASTS.md)** - Broadcast messages on player disconnects
- **[Server-to-Server Communication](../02-architecture/SERVER_TO_SERVER.md)** - S2S architecture & load balancing

---

## 📊 Statistics

- **Total Messages**: ~1170
- **Prototype (Implemented)**: 114 Messages
- **Phase 2 (In Progress)**: 674 Messages (incl. 54 S2S)
- **Phase 3 (Planned)**: 332 Messages (incl. 12 S2S)
- **Server-to-Server (S2S)**: 70 Messages (5000-5999)
- **Reserved**: 200 IDs
- **Debug**: 5 Messages

---

**Last Updated**: 2026-01-01  
**Version**: 2.1.0  
**Maintainer**: 2DMMO Team
