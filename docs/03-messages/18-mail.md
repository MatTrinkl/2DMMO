# 📬 Mail Messages (1800-1899)

**Kategorie:** 18  
**Range:** 1800-1899  

[← Zurück zur Übersicht](README.md)

---

## 📋 Inhaltsverzeichnis

- [MailInboxRequest (1800)](#mailinboxrequest-1800)
- [MailInboxResponse (1801)](#mailinboxresponse-1801)
- [MailSend (1802)](#mailsend-1802)
- [MailSendResult (1803)](#mailsendresult-1803)
- [MailRead (1804)](#mailread-1804)
- [MailMarkRead (1805)](#mailmarkread-1805)
- [MailTakeAttachment (1806)](#mailtakeattachment-1806)
- [MailTakeAttachmentResult (1807)](#mailtakeattachmentresult-1807)
- [MailTakeGold (1808)](#mailtakegold-1808)
- [MailTakeGoldResult (1809)](#mailtakegoldresult-1809)
- [MailTakeAll (1810)](#mailtakeall-1810)
- [MailDelete (1811)](#maildelete-1811)
- [MailReturn (1812)](#mailreturn-1812)
- [MailNotification (1813)](#mailnotification-1813)
- [MailCashOnDelivery (1814)](#mailcashondelivery-1814)
- [MailCashOnDeliveryPay (1815)](#mailcashondeliverypay-1815)
- [MailExpired (1816)](#mailexpired-1816)

---

## 📋 Übersicht

Diese Kategorie umfasst alle Messages für das **Mail-System** im 2DMMO.

---

## MailInboxRequest (1800)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

### Beschreibung
Client fordert Mail-Inbox an.

---

## MailInboxResponse (1801)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

### Beschreibung
Server sendet Mail-Liste.

---

## MailSend (1802)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja

### Beschreibung
Client sendet Mail mit optionalen Attachments/Gold.

---

## MailSendResult (1803)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja

### Beschreibung
Server bestätigt Mail-Versand.

---

## MailRead (1804)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja

### Beschreibung
Client öffnet und liest Mail.

---

## MailMarkRead (1805)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja

### Beschreibung
Client markiert Mail als gelesen.

---

## MailTakeAttachment (1806)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja

### Beschreibung
Client nimmt einzelnes Attachment aus Mail.

---

## MailTakeAttachmentResult (1807)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja

### Beschreibung
Server bestätigt Attachment-Entnahme.

---

## MailTakeGold (1808)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja

### Beschreibung
Client nimmt Gold aus Mail.

---

## MailTakeGoldResult (1809)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja

### Beschreibung
Server bestätigt Gold-Entnahme.

---

## MailTakeAll (1810)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja

### Beschreibung
Client nimmt alle Attachments und Gold.

---

## MailDelete (1811)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja

### Beschreibung
Client löscht Mail.

---

## MailReturn (1812)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

### Beschreibung
Client sendet Mail zurück an Absender.

---

## MailNotification (1813)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

### Beschreibung
Server informiert über neue Mail.

---

## MailCashOnDelivery (1814)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

### Beschreibung
Mail mit Cash-on-Delivery Anforderung.

---

## MailCashOnDeliveryPay (1815)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

### Beschreibung
Client zahlt Cash-on-Delivery.

---

## MailExpired (1816)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

### Beschreibung
Mail ist abgelaufen und wurde zurückgesendet oder gelöscht.

---

**Letzte Aktualisierung**: 2026-01-02  
**Version**: 2.0.0  
**Status**: ✅ Aligned mit MessageType Enum (17 Messages)

[← Zurück zur Übersicht](README.md)
