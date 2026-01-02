# 🚩 Reporting Messages (3600-3699)

**Kategorie:** 36  
**Range:** 3600-3699  


[← Zurück zur Übersicht](README.md)

---

## 📋 Übersicht

Reporting-System für Moderation.

---

## PlayerReport (3600)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| TargetName | string | Reported Player | Ja |
| Reason | string | "spam", "harassment", "cheating", "inappropriate_name" | Ja |
| Description | string | Details | Ja |

---

## ReportConfirmation (3601)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** Nein

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| ReportId | string | Report-ID | Ja |
| Timestamp | long | Unix Timestamp | Ja |

---

**Letzte Aktualisierung**: 2025-12-25  
**Version**: 1.0.0

[← Zurück zur Übersicht](README.md)
