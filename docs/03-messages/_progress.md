# 📊 MessageType SOT Alignment - Progress Tracker

**Created:** 2026-01-02  
**Last Updated:** 2026-01-02  
**Goal:** Align `MessageType` enum as Source of Truth with all documentation in `03-messages/`

> **Wichtig:** Messages werden phasenlos dokumentiert. Phasen beziehen sich nur auf Feature-Implementierung, nicht auf Message-Kategorisierung.

---

## 📋 Legend

- ✅ **Done** - Category fully aligned (enum + docs match)
- 🔄 **In Progress** - Currently being worked on
- ⏳ **Todo** - Not yet started
- ⚠️ **Issues Found** - Discrepancies identified

---

## 📈 Overall Progress

| Range | Status | Categories |
|-------|--------|------------|
| Core (0-9) | ✅ | 10 categories |
| Extended (10-29) | ⏳ | ~20 categories |
| Advanced (30-49) | ⏳ | ~20 categories |
| Reserved/Debug (47-50) | ⏳ | 4 categories |

---

## 📂 Category Status

### Connection & Core (0000-0999)

| Cat | Name | Range | Enum Count | Doc Status | Notes |
|-----|------|-------|------------|------------|-------|
| 00 | Connection | 0-99 | 24 | ✅ | Fixed naming discrepancies, added [Obsolete] |
| 01 | Zone | 100-199 | 19 | ✅ | Fixed ID mismatches, added [Obsolete] markers |
| 02 | Movement | 200-299 | 22 | ✅ | Added TeleportResponse (220), JumpResponse (221) |
| 03 | Combat | 300-399 | 35 | ✅ | Added ThreatListResponse (333), ResurrectionResponse (334) |
| 04 | Chat | 400-499 | 37 | ✅ | Added response messages (440-445) |
| 05 | Inventory | 500-599 | 44 | ✅ | Fixed message names, added responses (536-543) |
| 06 | Character | 600-699 | 30 | ✅ | Fixed names, added responses (650-655) |
| 07 | Party | 700-799 | 33 | ✅ | Fixed names, added responses (740-746) |
| 08 | Guild | 800-899 | 50 | ✅ | Fixed names, added responses (840-850) |
| 09 | System | 900-999 | 26 | ✅ | Verified - aligned with enum |

### Gameplay Features (1000-1999)

| Cat | Name | Range | Enum Count | Doc Status | Notes |
|-----|------|-------|------------|------------|-------|
| 10 | Quest | 1000-1099 | 24 | ⏳ | |
| 11 | Trading | 1100-1199 | 14 | ⏳ | |
| 12 | Targeting | 1200-1299 | 17 | ⏳ | |
| 13 | NPC | 1300-1399 | 38 | ⏳ | |
| 14 | Entity | 1400-1499 | 35 | ⏳ | |
| 15 | Aura | 1500-1599 | 17 | ⏳ | |
| 16 | Crafting | 1600-1699 | 25 | ⏳ | |
| 17 | Auction | 1700-1799 | 21 | ⏳ | |
| 18 | Mail | 1800-1899 | 17 | ⏳ | |
| 19 | Achievement | 1900-1999 | 15 | ⏳ | |

### Social & PvP (2000-2999)

| Cat | Name | Range | Enum Count | Doc Status | Notes |
|-----|------|-------|------------|------------|-------|
| 20 | Mount | 2000-2099 | 25 | ⏳ | |
| 21 | Social | 2100-2199 | 13 | ⏳ | |
| 22 | Emote | 2200-2299 | 23 | ⏳ | |
| 23 | Admin | 2300-2399 | 37 | ⏳ | |
| 24 | Instance | 2400-2499 | 33 | ⏳ | |
| 25 | PvP | 2500-2599 | 32 | ⏳ | |
| 26 | World | 2600-2699 | 22 | ⏳ | |
| 27 | Matchmaking | 2700-2799 | 13 | ⏳ | |
| 28 | Leaderboard | 2800-2899 | 14 | ⏳ | |
| 29 | Tutorial | 2900-2999 | 13 | ⏳ | |

### Advanced Systems (3000-3999)

| Cat | Name | Range | Enum Count | Doc Status | Notes |
|-----|------|-------|------------|------------|-------|
| 30 | Settings | 3000-3099 | 12 | ⏳ | |
| 31 | Loot | 3100-3199 | 17 | ⏳ | |
| 32 | Cooldown | 3200-3299 | 20 | ⏳ | |
| 33 | Inspection | 3300-3399 | 21 | ⏳ | |
| 34 | Map | 3400-3499 | 24 | ⏳ | |
| 35 | Voice | 3500-3599 | 14 | ⏳ | |
| 36 | Reporting | 3600-3699 | 16 | ⏳ | |
| 37 | Economy | 3700-3799 | 14 | ⏳ | |
| 38 | Skill | 3800-3899 | 24 | ⏳ | |
| 39 | Equipment | 3900-3999 | 24 | ⏳ | |

### Extended Features (4000-4999)

| Cat | Name | Range | Enum Count | Doc Status | Notes |
|-----|------|-------|------------|------------|-------|
| 40 | Bank | 4000-4099 | 19 | ⏳ | |
| 41 | Death | 4100-4199 | 24 | ⏳ | |
| 42 | Transportation | 4200-4299 | 28 | ⏳ | |
| 43 | Notification | 4300-4399 | 24 | ⏳ | |
| 44 | Cutscene | 4400-4499 | 6 | ⏳ | |
| 45 | Housing | 4500-4599 | 6 | ⏳ | |
| 46 | Event | 4600-4699 | 5 | ⏳ | |
| 47 | Reserved | 4700-4799 | 0 | ⏳ | |
| 48 | Reserved | 4800-4899 | 0 | ⏳ | |
| 49 | Debug | 4900-4999 | 5 | ⏳ | |

### Server-to-Server (5000-5999)

| Cat | Name | Range | Enum Count | Doc Status | Notes |
|-----|------|-------|------------|------------|-------|
| 50 | S2S | 5000-5999 | TBD | ⏳ | Server-to-Server |

---

## 📝 Decisions Log

| Date | Decision | Reason |
|------|----------|--------|
| 2026-01-02 | MessageType enum is SOT | Defined in problem statement |
| | | |

---

## ⚠️ Issues Found

_(Issues are tracked and resolved during processing)_

---

## 🔄 Change Summary

_(Updated after each category completion)_

### Category 00: Connection (2026-01-02)
- ✅ Fixed section headings to match enum names:
  - `CharacterSelect (9)` → `CharacterSelectRequest (9)`
  - `CharacterCreate (10)` → `CharacterCreateRequest (10)`
  - `CharacterDelete (11)` → `CharacterDeleteRequest (11)`
  - `ServerSelect (14)` → `ServerSelectRequest (14)`
- ✅ Fixed code examples to use correct class/type names
- ✅ Fixed cross-references in "Verwandte Messages" tables
- ✅ Added `[Obsolete]` attribute to `SessionValidate` in enum

### Category 01: Zone (2026-01-02)
- ✅ Fixed ID mismatches in section headings:
  - `ZoneDiscovered (108)` → `ZoneDiscovered (109)`
  - `ZoneListRequest (109)` → `ZoneListRequest (110)`
  - `ZoneListResponse (110)` → `ZoneListResponse (111)`
  - `ShardTransfer (111)` → `ShardTransfer (112)`
  - `ShardListRequest (112)` → `ShardListRequest (113)`
  - `ShardListResponse (113)` → `ShardListResponse (114)`
  - `SubZoneEnter (114)` → `SubZoneEnter (115)`
  - `SubZoneLeave (115)` → `SubZoneLeave (116)`
  - `ZoneLoadedAck (119)` → `ZoneLoadedAck (118)`
- ✅ Added `[Obsolete]` attributes to enum:
  - `JoinZone` (100) - replaced by ZoneState.MyCharacter
  - `ZoneLoadingProgress` (108) - no longer needed
- ✅ Fixed obsolete section IDs in docs
- ✅ Updated `ZonePhaseChange` to note it has no ID yet (planned feature)

---

**Maintainer:** GitHub Copilot Agent  
**Source of Truth:** `shared/Mmo.Shared/Messaging/Enums/MessageType.cs`
