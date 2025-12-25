# 📬 Mail Messages (1800-1899)

**Kategorie:** 18  
**Range:** 1800-1899  
**Phase:** Phase 3

[← Zurück zur Übersicht](README.md)

---

## 📋 Übersicht

Mail-System mit Attachments und Gold-Transfer.

---

## MailSend (1800)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Recipient | string | Empfänger-Name | Ja |
| Subject | string | Betreff | Ja |
| Body | string | Mail-Text | Ja |
| Gold | long | Attached Gold | Nein |
| Items | List<uint> | Attached Item-IDs | Nein |

---

## MailReceive (1801)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| MailId | string | Mail-ID | Ja |
| Sender | string | Sender-Name | Ja |
| Subject | string | Betreff | Ja |
| HasAttachment | bool | Hat Attachments? | Ja |

---

## MailOpen (1802)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| MailId | string | Mail-ID | Ja |

---

## MailTakeAttachment (1806)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| MailId | string | Mail-ID | Ja |

---

**Letzte Aktualisierung**: 2025-12-25  
**Version**: 1.0.0

[← Zurück zur Übersicht](README.md)
