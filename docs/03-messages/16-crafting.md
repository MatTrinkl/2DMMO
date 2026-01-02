# 🔨 Crafting Messages (1600-1699)

**Kategorie:** 16  
**Range:** 1600-1699  


[← Zurück zur Übersicht](README.md)

---

## 📋 Übersicht

Crafting-System mit Recipes und Professions.

---

## CraftRecipe (1600)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| RecipeId | uint | Recipe-ID | Ja |
| Quantity | int | Anzahl zu craften | Ja |

---

## CraftResult (1601)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** Nein

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Success | bool | Craft erfolgreich? | Ja |
| ItemId | uint | Gecraftetes Item | Bei Success |
| ErrorCode | string | Error | Bei Fehler |

---

## RecipeLearn (1610)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| RecipeId | uint | Zu lernendes Recipe | Ja |

---

## ProfessionSkillUp (1620)

**Richtung:** 📡 Broadcast  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Profession | string | "blacksmith", "enchanter", "alchemist" | Ja |
| NewSkill | int | Neuer Skill-Level | Ja |

---

**Letzte Aktualisierung**: 2025-12-25  
**Version**: 1.0.0

[← Zurück zur Übersicht](README.md)
