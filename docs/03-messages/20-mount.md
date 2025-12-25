# 🐴 Mount Messages (2000-2099)

**Kategorie:** 20  
**Range:** 2000-2099  
**Phase:** Phase 3

[← Zurück zur Übersicht](README.md)

---

## 📋 Übersicht

Mount-System für schnelleres Reisen.

---

## MountSummon (2000)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| MountId | uint | Mount-ID | Ja |

---

## MountDismount (2001)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja

---

## MountSpeed (2002)

**Richtung:** 📡 Broadcast  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| PlayerId | int | Player-ID | Ja |
| SpeedMultiplier | float | Speed (z.B. 1.6 = 60% faster) | Ja |

---

## MountLearn (2010)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| MountId | uint | Zu lernendes Mount | Ja |

---

**Letzte Aktualisierung**: 2025-12-25  
**Version**: 1.0.0

[← Zurück zur Übersicht](README.md)
