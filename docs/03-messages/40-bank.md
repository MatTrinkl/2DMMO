# 🏦 Bank Messages (4000-4099)

**Kategorie:** 40  
**Range:** 4000-4099  


[← Zurück zur Übersicht](README.md)

---

## 📋 Übersicht

Bank-System für zusätzlichen Storage.

---

## BankOpen (4000)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| NPCId | int | Banker-NPC-ID | Ja |

---

## BankSync (4001)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Items | List<BankItem> | Bank-Items | Ja |
| TotalSlots | int | Total Slots | Ja |

---

## BankDeposit (4010)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| InventorySlot | byte | Inventory-Slot | Ja |
| Quantity | int | Anzahl | Ja |

---

## BankWithdraw (4011)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| BankSlot | byte | Bank-Slot | Ja |
| Quantity | int | Anzahl | Ja |

---

## BankExpand (4020)

**Richtung:** �� Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| TabIndex | byte | Bank-Tab | Ja |

---

**Letzte Aktualisierung**: 2025-12-25  
**Version**: 1.0.0

[← Zurück zur Übersicht](README.md)
