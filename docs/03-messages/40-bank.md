# 🏦 Bank / Storage Messages (4000-4099)

**Kategorie:** 40  
**Range:** 4000-4099  
**Status:** ✅ Dokumentiert

[← Zurück zur Übersicht](README.md)

---

## 📋 Inhaltsverzeichnis

### Personal Bank (4000-4009)
- [BankOpen (4000)](#bankopen-4000)
- [BankClose (4001)](#bankclose-4001)
- [BankDeposit (4002)](#bankdeposit-4002)
- [BankDepositResult (4003)](#bankdepositresult-4003)
- [BankWithdraw (4004)](#bankwithdraw-4004)
- [BankWithdrawResult (4005)](#bankwithdrawresult-4005)
- [BankSlotPurchase (4006)](#bankslotpurchase-4006)
- [BankSlotPurchaseResult (4007)](#bankslotpurchaseresult-4007)
- [BankTabPurchase (4008)](#banktabpurchase-4008)
- [BankSync (4009)](#banksync-4009)

### Guild Bank (4020-4026)
- [GuildBankOpenMsg (4020)](#guildbankopenmsg-4020)
- [GuildBankCloseMsg (4021)](#guildbankclosemsg-4021)
- [GuildBankDepositMsg (4022)](#guildbankdepositmsg-4022)
- [GuildBankWithdrawMsg (4023)](#guildbankwithdrawmsg-4023)
- [GuildBankLogMsg (4024)](#guildbanklogmsg-4024)
- [GuildBankTabInfo (4025)](#guildbanktabinfo-4025)
- [GuildBankSyncMsg (4026)](#guildbanksyncmsg-4026)

### Void Storage (4040-4044)
- [VoidStorageOpen (4040)](#voidstorageopen-4040)
- [VoidStorageClose (4041)](#voidstorageclose-4041)
- [VoidStorageDeposit (4042)](#voidstoragedeposit-4042)
- [VoidStorageWithdraw (4043)](#voidstoragewithdraw-4043)
- [VoidStorageSync (4044)](#voidstoragesync-4044)

### Reagent Bank (4050-4052)
- [ReagentBankOpen (4050)](#reagentbankopen-4050)
- [ReagentBankDeposit (4051)](#reagentbankdeposit-4051)
- [ReagentBankSync (4052)](#reagentbanksync-4052)

---

## 📋 Übersicht

Diese Kategorie umfasst alle Messages für **Bank- und Storage-Systeme** im 2DMMO.

Das Bank-System implementiert:
- Personal Bank (pro Charakter)
- Guild Bank (geteilter Guild-Storage)
- Void Storage (Langzeit-Storage für Transmog-Items)
- Reagent Bank (Crafting-Material-Storage)

**Server Authority**: Alle Bank-Operationen sind server-authoritative.

---

## BankOpen (4000)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Öffnet die persönliche Bank beim Banker-NPC.

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| NpcId | int | Banker-NPC-ID | Ja |

### Erwartete Response
- **Bei Erfolg:** `BankSync` (4009)
- **Bei Fehler:** `ErrorMessage` (910)

---

## BankClose (4001)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Schließt die Bank-UI.

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| — | — | Keine Felder | — |

---

## BankDeposit (4002)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Legt ein Item aus dem Inventar in die Bank.

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| InventorySlot | byte | Quell-Slot im Inventar | Ja |
| BankSlot | byte | Ziel-Slot in der Bank | Ja |
| Quantity | int | Anzahl (für Stacks) | Ja |

### Erwartete Response
- **Bei Erfolg:** `BankDepositResult` (4003)
- **Bei Fehler:** `ErrorMessage` (910)

---

## BankDepositResult (4003)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Bestätigt das erfolgreiche Einlagern eines Items.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Success | bool | Erfolgreich? | Ja |
| BankSlot | byte | Bank-Slot | Ja |
| ItemId | uint | Item-ID | Ja |
| Quantity | int | Eingelagerte Menge | Ja |

---

## BankWithdraw (4004)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Entnimmt ein Item aus der Bank ins Inventar.

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| BankSlot | byte | Quell-Slot in der Bank | Ja |
| InventorySlot | byte | Ziel-Slot im Inventar | Ja |
| Quantity | int | Anzahl (für Stacks) | Ja |

### Erwartete Response
- **Bei Erfolg:** `BankWithdrawResult` (4005)
- **Bei Fehler:** `ErrorMessage` (910)

---

## BankWithdrawResult (4005)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Bestätigt das erfolgreiche Entnehmen eines Items.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Success | bool | Erfolgreich? | Ja |
| InventorySlot | byte | Inventar-Slot | Ja |
| ItemId | uint | Item-ID | Ja |
| Quantity | int | Entnommene Menge | Ja |

---

## BankSlotPurchase (4006)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Kauft zusätzliche Bank-Slots.

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| SlotCount | int | Anzahl zu kaufender Slots | Ja |

### Erwartete Response
- **Bei Erfolg:** `BankSlotPurchaseResult` (4007)
- **Bei Fehler:** `ErrorMessage` (910)

---

## BankSlotPurchaseResult (4007)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Bestätigt den Kauf zusätzlicher Bank-Slots.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Success | bool | Erfolgreich? | Ja |
| NewSlotCount | int | Neue Gesamtzahl Slots | Ja |
| GoldSpent | int | Ausgegebenes Gold | Ja |

---

## BankTabPurchase (4008)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Kauft einen neuen Bank-Tab (zusätzliche Seite mit Slots).

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| TabIndex | byte | Index des zu kaufenden Tabs | Ja |

---

## BankSync (4009)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Synchronisiert den kompletten Bank-Inhalt.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Items | List<BankItemDto> | Bank-Items | Ja |
| TotalSlots | int | Verfügbare Slots | Ja |
| UnlockedTabs | int | Freigeschaltete Tabs | Ja |

---

## GuildBankOpenMsg (4020)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Öffnet die Guild-Bank.

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| NpcId | int | Guild-Banker-NPC-ID | Ja |

### Erwartete Response
- **Bei Erfolg:** `GuildBankSyncMsg` (4026)
- **Bei Fehler:** `ErrorMessage` (910)

---

## GuildBankCloseMsg (4021)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Schließt die Guild-Bank-UI.

---

## GuildBankDepositMsg (4022)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Guild-Deposit-Permission

### Beschreibung
Legt ein Item in die Guild-Bank ein.

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| InventorySlot | byte | Quell-Slot im Inventar | Ja |
| TabIndex | byte | Guild-Bank-Tab | Ja |
| BankSlot | byte | Ziel-Slot | Ja |
| Quantity | int | Anzahl | Ja |

---

## GuildBankWithdrawMsg (4023)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Guild-Withdraw-Permission

### Beschreibung
Entnimmt ein Item aus der Guild-Bank.

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| TabIndex | byte | Guild-Bank-Tab | Ja |
| BankSlot | byte | Quell-Slot | Ja |
| InventorySlot | byte | Ziel-Slot im Inventar | Ja |
| Quantity | int | Anzahl | Ja |

---

## GuildBankLogMsg (4024)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Fordert das Guild-Bank-Log an (Transaktionshistorie).

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| TabIndex | byte | Tab-Index (oder alle) | Ja |

---

## GuildBankTabInfo (4025)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Informationen zu einem Guild-Bank-Tab.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| TabIndex | byte | Tab-Index | Ja |
| TabName | string | Tab-Name | Ja |
| TabIcon | string | Icon-ID | Ja |
| Permissions | GuildBankPermissionDto | Berechtigungen | Ja |

---

## GuildBankSyncMsg (4026)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Synchronisiert den Guild-Bank-Inhalt.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Tabs | List<GuildBankTabDto> | Tab-Daten | Ja |
| Items | List<GuildBankItemDto> | Items | Ja |

---

## VoidStorageOpen (4040)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Öffnet den Void Storage (Langzeit-Storage für Transmog-Items).

---

## VoidStorageClose (4041)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Schließt den Void Storage.

---

## VoidStorageDeposit (4042)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Legt ein Item in den Void Storage (Item wird zum Transmog-Appearance konvertiert).

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| InventorySlot | byte | Quell-Slot | Ja |
| VoidSlot | byte | Ziel-Slot | Ja |

---

## VoidStorageWithdraw (4043)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Entnimmt ein Item aus dem Void Storage.

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| VoidSlot | byte | Quell-Slot | Ja |
| InventorySlot | byte | Ziel-Slot | Ja |

---

## VoidStorageSync (4044)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Synchronisiert den Void Storage Inhalt.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Items | List<VoidStorageItemDto> | Items im Void Storage | Ja |

---

## ReagentBankOpen (4050)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Öffnet die Reagent Bank (spezielle Bank für Crafting-Materialien).

---

## ReagentBankDeposit (4051)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Legt alle Crafting-Reagenzien aus dem Inventar in die Reagent Bank.

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| DepositAll | bool | Alle Reagenzien einlagern | Ja |

---

## ReagentBankSync (4052)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Synchronisiert den Reagent Bank Inhalt.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Reagents | List<ReagentBankItemDto> | Reagenzien | Ja |

---

**Letzte Aktualisierung**: 2026-01-02  
**Version**: 2.0.0

[← Zurück zur Übersicht](README.md)
