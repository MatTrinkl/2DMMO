# 🌍 World Messages (2600-2699)

**Kategorie:** 26  
**Range:** 2600-2699  


[← Zurück zur Übersicht](README.md)

---

## 📋 Übersicht

World-Events und Dynamic-Content.

---

## WorldEventStart (2600)

**Richtung:** 📡 Broadcast  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| EventId | uint | Event-ID | Ja |
| Title | string | Event-Name | Ja |
| Duration | int | Duration (Sekunden) | Ja |

---

## WorldBossSpawn (2610)

**Richtung:** 📡 Broadcast  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| BossId | uint | Boss-ID | Ja |
| X | float | Position X | Ja |
| Y | float | Position Y | Ja |
| ZoneId | uint | Zone-ID | Ja |

---

## WorldStateUpdate (2620)

**Richtung:** 📡 Broadcast  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| StateId | uint | State-ID | Ja |
| Value | int | Neuer Wert | Ja |

---

**Letzte Aktualisierung**: 2025-12-25  
**Version**: 1.0.0

[← Zurück zur Übersicht](README.md)
