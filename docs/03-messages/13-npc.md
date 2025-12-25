# 🤖 NPC Messages (1300-1399)

**Kategorie:** 13  
**Range:** 1300-1399  
**Phase:** Phase 2

[← Zurück zur Übersicht](README.md)

---

## 📋 Übersicht

NPC-Interaction und Dialog-System.

---

## NPCInteract (1300)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| NPCId | int | NPC Entity-ID | Ja |

---

## NPCDialogOpen (1301)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| NPCId | int | NPC-ID | Ja |
| DialogText | string | Dialog-Text | Ja |
| Options | List<DialogOption> | Dialog-Optionen | Ja |

---

## NPCDialogSelect (1302)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| NPCId | int | NPC-ID | Ja |
| OptionId | uint | Gewählte Option | Ja |

---

## VendorOpen (1310)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| NPCId | int | Vendor-NPC-ID | Ja |
| Items | List<VendorItem> | Verkaufte Items | Ja |

---

## VendorBuy (1311)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| NPCId | int | Vendor-ID | Ja |
| ItemId | uint | Item-ID | Ja |
| Quantity | int | Anzahl | Ja |

---

## VendorSell (1312)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| InventorySlot | byte | Zu verkaufendes Item | Ja |
| Quantity | int | Anzahl | Ja |

---

**Letzte Aktualisierung**: 2025-12-25  
**Version**: 1.0.0

[← Zurück zur Übersicht](README.md)
