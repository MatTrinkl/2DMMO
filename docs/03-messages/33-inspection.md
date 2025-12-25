# 🔍 Inspection Messages (3300-3399)

**Kategorie:** 33  
**Range:** 3300-3399  
**Phase:** Phase 2

[← Zurück zur Übersicht](README.md)

---

## 📋 Übersicht

Character-Inspection (Equipment, Stats).

---

## InspectRequest (3300)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| TargetId | int | Zu inspizierender Spieler | Ja |

---

## InspectResponse (3301)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** Nein

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| PlayerId | int | Player-ID | Ja |
| Equipment | List<EquipSlotInfo> | Equipment | Ja |
| Stats | CharacterStats | Stats | Ja |

---

**Letzte Aktualisierung**: 2025-12-25  
**Version**: 1.0.0

[← Zurück zur Übersicht](README.md)
