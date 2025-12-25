# ⚙️ Settings Messages (3000-3099)

**Kategorie:** 30  
**Range:** 3000-3099  
**Phase:** Phase 3

[← Zurück zur Übersicht](README.md)

---

## 📋 Übersicht

Client-Settings synchronisiert mit Server.

---

## SettingsSync (3000)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten (Login)  
**Authentifizierung:** 🔒 Ja

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Settings | Dictionary<string, string> | Key-Value Settings | Ja |

---

## SettingsUpdate (3001)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Key | string | Setting-Key | Ja |
| Value | string | Setting-Value | Ja |

---

**Letzte Aktualisierung**: 2025-12-25  
**Version**: 1.0.0

[← Zurück zur Übersicht](README.md)
