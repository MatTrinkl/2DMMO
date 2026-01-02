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

---

_(Entries will be added as categories are processed)_
