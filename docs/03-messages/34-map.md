# 🗺️ Map Messages (3400-3499)

**Kategorie:** 34  
**Range:** 3400-3499  
**Phase:** Phase 2

[← Zurück zur Übersicht](README.md)

---

## 📋 Übersicht

Map-System mit Minimap, Worldmap, und POIs.

---

## MapReveal (3400)

**Richtung:** 📡 Broadcast  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| ZoneId | uint | Zone-ID | Ja |
| AreaId | uint | Area-ID | Ja |

---

## MapPing (3401)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| X | float | Map X | Ja |
| Y | float | Map Y | Ja |
| PingType | string | "alert", "assist", "defend" | Ja |

---

## MapPOIAdd (3410)

**Richtung:** 📡 Broadcast  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| POIId | uint | POI-ID | Ja |
| X | float | Position X | Ja |
| Y | float | Position Y | Ja |
| Type | string | "quest", "vendor", "dungeon" | Ja |

---

## WaypointSet (3420)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| X | float | Waypoint X | Ja |
| Y | float | Waypoint Y | Ja |

---

**Letzte Aktualisierung**: 2025-12-25  
**Version**: 1.0.0

[← Zurück zur Übersicht](README.md)
