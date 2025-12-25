# 👾 Entity Messages (1400-1499)

**Kategorie:** 14  
**Range:** 1400-1499  
**Phase:** Phase 2

[← Zurück zur Übersicht](README.md)

---

## 📋 Übersicht

Generic Entity-System für alle Game-Objects.

---

## EntitySpawn (1400)

**Richtung:** 📡 Broadcast  
**Frequenz:** Sehr häufig  
**Authentifizierung:** 🔒 Ja

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| EntityId | int | Entity-ID | Ja |
| EntityType | string | "player", "npc", "monster", "object" | Ja |
| X | float | Position X | Ja |
| Y | float | Position Y | Ja |
| Name | string | Entity-Name | Nein |

---

## EntityDespawn (1401)

**Richtung:** 📡 Broadcast  
**Frequenz:** Sehr häufig  
**Authentifizierung:** 🔒 Ja

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| EntityId | int | Entity-ID | Ja |

---

## EntityMove (1402)

**Richtung:** 📡 Broadcast  
**Frequenz:** ⚡ Extrem häufig  
**Authentifizierung:** 🔒 Ja

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| EntityId | int | Entity-ID | Ja |
| X | float | Neue Position X | Ja |
| Y | float | Neue Position Y | Ja |
| VelocityX | float | Velocity X | Ja |
| VelocityY | float | Velocity Y | Ja |

---

## EntityUpdate (1403)

**Richtung:** 📡 Broadcast  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| EntityId | int | Entity-ID | Ja |
| HP | int | Health | Nein |
| MaxHP | int | Max Health | Nein |

---

**Letzte Aktualisierung**: 2025-12-25  
**Version**: 1.0.0

[← Zurück zur Übersicht](README.md)
