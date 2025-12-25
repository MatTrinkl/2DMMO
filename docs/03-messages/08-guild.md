# 🏰 Guild Messages (800-899)

**Kategorie:** 08  
**Range:** 800-899  
**Phase:** Phase 2

[← Zurück zur Übersicht](README.md)

---

## 📋 Übersicht

Guild-System mit Ranks, Perks, und Guild-Banks.

---

## GuildCreate (800)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| GuildName | string | Guild-Name | Ja |
| Tag | string | Guild-Tag (2-4 Zeichen) | Ja |

---

## GuildInvite (801)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| TargetName | string | Spieler-Name | Ja |

---

## GuildJoin (803)

**Richtung:** 📡 Broadcast  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| PlayerName | string | Neues Member | Ja |

---

## GuildLeave (804)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

---

## GuildPromote (810)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Officer+

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| PlayerName | string | Zu promoten | Ja |
| NewRank | byte | Neuer Rank | Ja |

---

## GuildMessage (820)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Message | string | Guild-Message | Ja |

---

## GuildBankDeposit (830)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| ItemSlot | byte | Item-Slot | Ja |
| Quantity | int | Anzahl | Ja |

---

**Letzte Aktualisierung**: 2025-12-25  
**Version**: 1.0.0

[← Zurück zur Übersicht](README.md)
