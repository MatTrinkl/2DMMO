# 🏛️ Instance Messages (2400-2499)

**Kategorie:** 24  
**Range:** 2400-2499  


[← Zurück zur Übersicht](README.md)

---

## 📋 Übersicht

Instanced-Dungeons und Raids.

---

## InstanceEnter (2400)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| InstanceId | uint | Instance-ID | Ja |
| Difficulty | string | "normal", "heroic", "mythic" | Ja |

---

## InstanceLeave (2401)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja

---

## InstanceLockout (2410)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| InstanceId | uint | Instance-ID | Ja |
| ResetTime | long | Unix Timestamp | Ja |

---

## InstanceProgress (2411)

**Richtung:** 📡 Broadcast  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| BossesKilled | int | Getötete Bosse | Ja |
| TotalBosses | int | Total Bosse | Ja |

---

**Letzte Aktualisierung**: 2025-12-25  
**Version**: 1.0.0

[← Zurück zur Übersicht](README.md)
