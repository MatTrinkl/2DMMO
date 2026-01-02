# 🏡 Housing / Player Buildings Messages (4500-4599)

**Kategorie:** 45  
**Range:** 4500-4599  

**Status:** ✅ Dokumentiert

[← Zurück zur Übersicht](README.md)

---

## 📋 Inhaltsverzeichnis

1. [HousingEnter (4500)](#housingenter-4500) - House betreten
2. [HousingLeave (4501)](#housingleave-4501) - House verlassen
3. [HousingEdit (4502)](#housingedit-4502) - Edit-Modus aktivieren
4. [HousingPlace (4503)](#housingplace-4503) - Möbel platzieren
5. [HousingRemove (4504)](#housingremove-4504) - Möbel entfernen
6. [HousingSave (4505)](#housingsave-4505) - Layout speichern

---

## HousingEnter (4500)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

Betritt ein Player-House (eigenes oder fremdes House).

### Im Scope ✅
- House-Teleport
- Permission-Check (Public, Friends-Only, Private)
- House-Instance laden

### Nicht im Scope ❌
- Möbel-Platzierung → use `HousingPlace` (4503)
- Guild-Hall → separate System

### Request Payload
| Field | Type | Description | Required |
|-------|------|-------------|----------|
| HouseId | ulong | House-Identifier (Player-ID) | Yes |

### Error Codes
| Code | Meaning | Action |
|------|---------|--------|
| HOUSE_PRIVATE | House ist privat | Permission-Request |
| HOUSE_NOT_FOUND | House existiert nicht | - |

---

## HousingLeave (4501)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

Verlässt das Player-House (zurück zur Welt).

---

## HousingEdit (4502)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

Aktiviert den Edit-Modus (nur im eigenen House).

---

## HousingPlace (4503)

**Richtung:** 📤 Client → Server  
**Frequenz:** Gelegentlich  
**Authentifizierung:** 🔒 Ja

Platziert ein Möbel-Objekt im House.

### Request Payload
| Field | Type | Description | Required |
|-------|------|-------------|----------|
| ItemId | uint | Möbel-Item aus Inventory | Yes |
| X | float | Position X | Yes |
| Y | float | Position Y | Yes |
| Z | float | Position Z | Yes |
| Rotation | float | Rotation in Grad | Yes |

---

## HousingRemove (4504)

**Richtung:** 📤 Client → Server  
**Frequenz:** Gelegentlich  
**Authentifizierung:** 🔒 Ja

Entfernt ein Möbel-Objekt (zurück ins Inventory).

---

## HousingSave (4505)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

Speichert das House-Layout.

---

**Letzte Aktualisierung**: 2025-12-25  
**Version**: 2.0.0

[← Zurück zur Übersicht](README.md)
