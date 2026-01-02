# 🏆 Achievement Messages (1900-1999)

**Kategorie:** 19  
**Range:** 1900-1999  


[← Zurück zur Übersicht](README.md)

---

## 📋 Übersicht

Achievement-System mit Progress-Tracking und Rewards.

---

## AchievementUnlock (1900)

**Richtung:** 📡 Broadcast  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| AchievementId | uint | Achievement-ID | Ja |
| Title | string | Achievement-Name | Ja |
| Points | int | Achievement-Points | Ja |

---

## AchievementProgress (1901)

**Richtung:** 📡 Broadcast  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| AchievementId | uint | Achievement-ID | Ja |
| Progress | int | Current Progress | Ja |
| Required | int | Required Progress | Ja |

---

## AchievementList (1902)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Achievements | List<AchievementInfo> | Achievements | Ja |

---

**Letzte Aktualisierung**: 2025-12-25  
**Version**: 1.0.0

[← Zurück zur Übersicht](README.md)
