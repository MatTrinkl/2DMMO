# 🎒 Inventory Messages (500-599)

**Kategorie:** 05  
**Range:** 500-599  
**Phase:** Phase 2  
**Status:** 🟡 Phase 2

[← Zurück zur Übersicht](README.md)

---

## 📋 Übersicht

Inventory-System mit Item-Management, Stacking, Sorting, und Bag-System.

**Server Authority**: Alle Inventory-Changes sind server-authoritative.

---

## InventorySync (500)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten (Login)  
**Authentifizierung:** 🔒 Ja

### Beschreibung
Synchronisiert komplettes Inventory nach Login.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Items | List<InventoryItem> | Alle Items | Ja |

---

## ItemAdd (501)

**Richtung:** 📡 Broadcast  
**Frequenz:** Sehr häufig  
**Authentifizierung:** 🔒 Ja

### Beschreibung
Item wurde zum Inventory hinzugefügt.

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| SlotIndex | byte | Slot | Ja |
| ItemId | uint | Item-ID | Ja |
| Quantity | int | Anzahl | Ja |

---

## ItemRemove (502)

**Richtung:** 📡 Broadcast  
**Frequenz:** Sehr häufig  
**Authentifizierung:** 🔒 Ja

### Beschreibung
Item wurde entfernt.

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| SlotIndex | byte | Slot | Ja |
| Quantity | int | Entfernte Anzahl | Ja |

---

## ItemMove (503)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja

### Beschreibung
Verschiebt Item zwischen Slots.

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| FromSlot | byte | Source-Slot | Ja |
| ToSlot | byte | Target-Slot | Ja |

---

## ItemSplit (504)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja

### Beschreibung
Splittet Stack in zwei Stacks.

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| FromSlot | byte | Source-Slot | Ja |
| ToSlot | byte | Target-Slot | Ja |
| Quantity | int | Zu bewegende Anzahl | Ja |

---

## ItemUse (505)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja

### Beschreibung
Verwendet Item (Potion, Food, etc.).

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| SlotIndex | byte | Item-Slot | Ja |
| TargetId | int | Target (optional) | Nein |

---

## ItemDelete (506)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

### Beschreibung
Löscht Item permanent.

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| SlotIndex | byte | Slot | Ja |

---

## BagExpand (510)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

### Beschreibung
Erweitert Bag-Größe.

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| BagId | byte | Bag-ID | Ja |

---

**Letzte Aktualisierung**: 2025-12-25  
**Version**: 1.0.0

[← Zurück zur Übersicht](README.md)
