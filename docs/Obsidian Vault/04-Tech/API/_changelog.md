# 📜 MessageType SOT Alignment - Changelog

**Created:** 2026-01-02  
**Purpose:** Track all changes made during the MessageType SOT alignment task

---

## Format

```
### [Category Name] (Date)
- **Added:** New messages or documentation
- **Changed:** Modifications to existing items
- **Deprecated:** Items marked as deprecated
- **Fixed:** Corrections and bug fixes
```

---

## Changelog Entries

### Initial Setup (2026-01-02)

- **Added:** `_progress.md` for tracking alignment progress
- **Added:** `_changelog.md` for tracking changes

### Category 00: Connection (2026-01-02)

- **Fixed:** Section headings now match enum names exactly
  - `CharacterSelect` → `CharacterSelectRequest`
  - `CharacterCreate` → `CharacterCreateRequest`
  - `CharacterDelete` → `CharacterDeleteRequest`
  - `ServerSelect` → `ServerSelectRequest`
- **Fixed:** Code examples updated to use correct class names and MessageType values
- **Fixed:** Cross-references in "Verwandte Messages" tables updated
- **Deprecated:** Added `[Obsolete]` attribute to `SessionValidate` (8) in MessageType enum
  - Replacement: `S2S_SessionValidate` (5000) for server-to-server communication

### Category 01: Zone (2026-01-02)

- **Fixed:** ID mismatches in section headings corrected to match enum
  - `ZoneDiscovered` 108→109, `ZoneListRequest` 109→110, `ZoneListResponse` 110→111
  - `ShardTransfer` 111→112, `ShardListRequest` 112→113, `ShardListResponse` 113→114
  - `SubZoneEnter` 114→115, `SubZoneLeave` 115→116, `ZoneLoadedAck` 119→118
- **Deprecated:** Added `[Obsolete]` attributes in MessageType enum:
  - `JoinZone` (100) - Replaced by `ZoneState.MyCharacter`
  - `ZoneLoadingProgress` (108) - Client loads locally, sends `ZoneLoadedAck` (118)
- **Changed:** `ZonePhaseChange` marked as planned feature (no ID assigned yet)

### Category 02: Movement (2026-01-02)

- **Added:** Missing response messages to enum:
  - `TeleportResponse` (220)
  - `JumpResponse` (221)

### Category 03: Combat (2026-01-02)

- **Added:** Missing response messages to enum:
  - `ThreatListResponse` (333)
  - `ResurrectionResponse` (334)
- **Fixed:** Docs had wrong IDs (321, 322 were already used by AggroTransfer and TauntEvent)

### Category 04: Chat (2026-01-02)

- **Added:** Missing response messages to enum (440-445):
  - `ChatMessageResponse` (440)
  - `ChatChannelJoinResponse` (441)
  - `ChatChannelCreateResponse` (442)
  - `ChatChannelDeleteResponse` (443)
  - `ChatChannelPasswordResponse` (444)
  - `ChatChannelMuteResponse` (445)

### Category 05: Inventory (2026-01-02)

- **Fixed:** Message names in docs to match enum (significant discrepancies):
  - `InventorySync` → `InventoryUpdate`
  - `ItemAdd` → `InventorySlotUpdate`
  - `ItemRemove` → `ItemPickup`
  - And many more...
- **Added:** Missing response messages to enum (536-543):
  - `ItemMoveResponse`, `ItemSplitResponse`, `ItemUseResponse`
  - `ItemDeleteResponse`, `ItemStackResponse`, `ItemSortResponse`
  - `ItemLockResponse`, `BagExpandResponse`
- **Fixed:** Response IDs in docs (were 520-527, conflicted with existing entries)

### Categories 06-09: Character, Party, Guild, System (2026-01-02)

- **Fixed:** Message names and IDs aligned with enum
- **Added:** Response messages where missing
- **Verified:** All Request/Response pairs documented

---

## Detail Pass Phase (2026-01-02)

### Batch 1: Categories 00-09 (Core) - COMPLETE

- **Verified:** All 10 core categories have complete template sections
- **Status:** All messages follow the standard template with:
  - Richtung, Frequenz, Auth, Rechte headers
  - Beschreibung (2-3 sentences)
  - Im Scope ✅ / Nicht im Scope ❌
  - Payload tables
  - Erwartete Response
  - Verwandte Messages
  - Code examples

Categories verified:
- 00 Connection: 24 messages fully detailed
- 01 Zone: 19 messages + comprehensive DTOs
- 02 Movement: 22 messages with prediction/correction docs
- 03 Combat: 35 messages with combat system docs
- 04 Chat: 37 messages with channel system
- 05 Inventory: 44 messages with item management
- 06 Character: 30 messages with progression
- 07 Party: 33 messages with group mechanics
- 08 Guild: 50 messages with guild system
- 09 System: 26 messages with error handling

---

_(Entries will be added as categories are processed)_

Source: docs/03-messages/_changelog.md
