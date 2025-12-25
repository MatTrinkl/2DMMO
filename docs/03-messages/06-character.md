# 👤 Character Messages (600-699)

**Kategorie:** 06  
**Range:** 600-699  
**Phase:** Phase 2

[← Zurück zur Übersicht](README.md)

---

## 📋 Übersicht

Character-Management, Stats, Attributes, und Character-Customization.

---

## CharacterInfo (600)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten (Login)  
**Authentifizierung:** 🔒 Ja

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Name | string | Character-Name | Ja |
| Level | int | Level | Ja |
| Class | string | Class | Ja |
| Stats | CharacterStats | Stats | Ja |

---

## StatsUpdate (601)

**Richtung:** 📡 Broadcast  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| HP | int | Health | Ja |
| MaxHP | int | Max Health | Ja |
| MP | int | Mana | Ja |
| MaxMP | int | Max Mana | Ja |

---

## LevelUp (602)

**Richtung:** 📡 Broadcast  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| NewLevel | int | Neues Level | Ja |
| StatPoints | int | Neue Stat-Points | Ja |

---

## ExperienceGain (603)

**Richtung:** 📡 Broadcast  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Amount | int | XP-Gain | Ja |
| CurrentXP | long | Current XP | Ja |
| RequiredXP | long | XP für nächstes Level | Ja |

---

## AttributeIncrease (604)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Attribute | string | "strength", "dexterity", "intelligence" | Ja |
| Points | int | Zu addierende Points | Ja |

---

## CharacterCustomize (610)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| CustomizationData | string | JSON Customization | Ja |

---

**Letzte Aktualisierung**: 2025-12-25  
**Version**: 1.0.0

[← Zurück zur Übersicht](README.md)
