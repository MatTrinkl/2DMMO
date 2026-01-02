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

---

_(Entries will be added as categories are processed)_
