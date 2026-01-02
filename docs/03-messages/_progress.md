# 📊 MessageType SOT Alignment - Progress Tracker

**Created:** 2026-01-02  
**Last Updated:** 2026-01-02  
**Goal:** Align `MessageType` enum as Source of Truth with all documentation in `03-messages/`

> **Wichtig:** Messages werden phasenlos dokumentiert. Phasen beziehen sich nur auf Feature-Implementierung, nicht auf Message-Kategorisierung.

---

## 📋 Legend

### Alignment Phase (Complete)
- ✅ **Aligned** - Category enum + docs IDs match

### Detail Pass Phase (Current)
- 🔵 **Detail Done** - All messages have full template sections
- 🔄 **In Detail Pass** - Currently being detailed
- ⏳ **Detail Todo** - Needs full template elaboration

---

## 📈 Overall Progress

**Phase 1: Alignment** - ✅ COMPLETE (All 51 categories aligned)

**Phase 2: Detail Pass** - 🔄 IN PROGRESS (Batch 4 Complete)

| Range | Alignment | Detail Pass | Categories |
|-------|-----------|-------------|------------|
| Core (0-9) | ✅ | 🔵 Detail Done | 10 categories |
| Extended (10-19) | ✅ | 🔵 Detail Done | 10 categories |
| Social (20-29) | ✅ | 🔵 Detail Done | 10 categories |
| Advanced (30-39) | ✅ | 🔵 Detail Done | 10 categories |
| Extended (40-49) | ✅ | ⏳ Batch 5 | 10 categories |
| S2S (50) | ✅ | 📋 Geplant | 1 category |

---

## 📂 Category Status

### Connection & Core (0000-0999)

| Cat | Name | Range | Enum Count | Doc Status | Detail Status | Notes |
|-----|------|-------|------------|------------|---------------|-------|
| 00 | Connection | 0-99 | 24 | ✅ | 🔵 Done | Full template for all messages |
| 01 | Zone | 100-199 | 19 | ✅ | 🔵 Done | Comprehensive with DTOs and flows |
| 02 | Movement | 200-299 | 22 | ✅ | 🔵 Done | Complete with examples |
| 03 | Combat | 300-399 | 35 | ✅ | 🔵 Done | Complete combat system docs |
| 04 | Chat | 400-499 | 37 | ✅ | 🔵 Done | Chat channel system complete |
| 05 | Inventory | 500-599 | 44 | ✅ | 🔵 Done | Item management complete |
| 06 | Character | 600-699 | 30 | ✅ | 🔵 Done | Stats and progression complete |
| 07 | Party | 700-799 | 33 | ✅ | 🔵 Done | Group mechanics complete |
| 08 | Guild | 800-899 | 50 | ✅ | 🔵 Done | Guild system complete |
| 09 | System | 900-999 | 26 | ✅ | 🔵 Done | System/Error handling complete |

### Gameplay Features (1000-1999)

| Cat | Name | Range | Enum Count | Doc Status | Detail Status | Notes |
|-----|------|-------|------------|------------|---------------|-------|
| 10 | Quest | 1000-1099 | 24 | ✅ | 🔵 Done | Full template with ErrorCodes, Examples |
| 11 | Trading | 1100-1199 | 14 | ✅ | 🔵 Done | Full template with Flow diagrams |
| 12 | Targeting | 1200-1299 | 23 | ✅ | 🔵 Done | Full template with Response messages |
| 13 | NPC | 1300-1399 | 38 | ✅ | 🔵 Done | Full template for all NPC types |
| 14 | Entity | 1400-1499 | 27 | ✅ | 🔵 Done | Aligned with Zone, deprecated duplicates |
| 15 | Aura | 1500-1599 | 17 | ✅ | 🔵 Done | Full template for Buff/Debuff system |
| 16 | Crafting | 1600-1699 | 22 | ✅ | 🔵 Done | Full template for Crafting/Gathering |
| 17 | Auction | 1700-1799 | 21 | ✅ | 🔵 Done | Full template for Auction House |
| 18 | Mail | 1800-1899 | 17 | ✅ | 🔵 Done | Full template for Mail system |
| 19 | Achievement | 1900-1999 | 15 | ✅ | 🔵 Done | Full template for Achievements/Titles |

### Social & PvP (2000-2999)

| Cat | Name | Range | Enum Count | Doc Status | Detail Status | Notes |
|-----|------|-------|------------|------------|---------------|-------|
| 20 | Mount | 2000-2099 | 27 | ✅ | 🔵 Done | Full template for Mount/Pet/Companion system |
| 21 | Social | 2100-2199 | 21 | ✅ | 🔵 Done | Full template for Friends/Block/Who system |
| 22 | Emote | 2200-2299 | 21 | ✅ | 🔵 Done | Full template for Emotes/Cosmetics/Transmog |
| 23 | Admin | 2300-2399 | 37 | ✅ | 🔵 Done | Full template for GM Tools |
| 24 | Instance | 2400-2499 | 35 | ✅ | 🔵 Done | Full template for Dungeons/Raids/Finder |
| 25 | PvP | 2500-2599 | 35 | ✅ | 🔵 Done | Full template for Arena/BG/World PvP |
| 26 | World | 2600-2699 | 21 | ✅ | 🔵 Done | Full template for Weather/Events/Bosses |
| 27 | Matchmaking | 2700-2799 | 17 | ✅ | 🔵 Done | Full template for Queues/Roles |
| 28 | Leaderboard | 2800-2899 | 14 | ✅ | 🔵 Done | Full template for Rankings |
| 29 | Tutorial | 2900-2999 | 13 | ✅ | 🔵 Done | Full template for Guide/Tutorial system |

### Advanced Systems (3000-3999)

| Cat | Name | Range | Enum Count | Doc Status | Detail Status | Notes |
|-----|------|-------|------------|------------|---------------|-------|
| 30 | Settings | 3000-3099 | 17 | ✅ | 🔵 Done | Full template for User/Game settings |
| 31 | Loot | 3100-3199 | 21 | ✅ | 🔵 Done | Full template for Loot/Roll/Distribution |
| 32 | Cooldown | 3200-3299 | 18 | ✅ | 🔵 Done | Full template for Cooldown management |
| 33 | Inspection | 3300-3399 | 17 | ✅ | 🔵 Done | Full template for Player inspection |
| 34 | Map | 3400-3499 | 20 | ✅ | 🔵 Done | Full template for Map/Waypoint system |
| 35 | Voice | 3500-3599 | 15 | ✅ | 🔵 Done | Full template for Voice chat |
| 36 | Reporting | 3600-3699 | 18 | ✅ | 🔵 Done | Full template for Reporting/Moderation |
| 37 | Economy | 3700-3799 | 17 | ✅ | 🔵 Done | Full template for Currency/Gold system |
| 38 | Skill | 3800-3899 | 28 | ✅ | 🔵 Done | Full template for Skill/Talent system |
| 39 | Equipment | 3900-3999 | 28 | ✅ | 🔵 Done | Full template for Equipment/Enchant system |

### Extended Features (4000-4999)

| Cat | Name | Range | Enum Count | Doc Status | Notes |
|-----|------|-------|------------|------------|-------|
| 40 | Bank | 4000-4099 | 25 | ✅ | Completely rewritten to match enum |
| 41 | Death | 4100-4199 | 24 | ✅ | Completely rewritten to match enum |
| 42 | Transportation | 4200-4299 | 28 | ✅ | Completely rewritten to match enum |
| 43 | Notification | 4300-4399 | 20 | ✅ | Completely rewritten to match enum |
| 44 | Cutscene | 4400-4499 | 6 | ✅ | Verified - aligned with enum |
| 45 | Housing | 4500-4599 | 6 | ✅ | Verified - aligned with enum |
| 46 | Event | 4600-4699 | 5 | ✅ | Verified - aligned with enum |
| 47 | Reserved | 4700-4799 | 0 | ✅ | Reserved for future use |
| 48 | Reserved | 4800-4899 | 0 | ✅ | Reserved for future use |
| 49 | Debug | 4900-4999 | 5 | ✅ | Verified - aligned with enum |

### Server-to-Server (5000-5999)

| Cat | Name | Range | Enum Count | Doc Status | Notes |
|-----|------|-------|------------|------------|-------|
| 50 | S2S | 5000-5999 | 0 (planned) | ✅ | Server-to-Server - not in enum yet, docs aligned |

---

## 📝 Decisions Log

| Date | Decision | Reason |
|------|----------|--------|
| 2026-01-02 | MessageType enum is SOT | Defined in problem statement |
| 2026-01-02 | Categories 40-49 aligned | Extended features complete |

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
