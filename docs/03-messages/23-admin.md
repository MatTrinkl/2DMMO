# 👑 Admin Messages (2300-2399)

**Kategorie:** 23  
**Range:** 2300-2399  


[← Zurück zur Übersicht](README.md)

---

## 📋 Übersicht

Admin-Commands für GM-Tools.

---

## AdminCommand (2300)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Admin

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Command | string | Admin-Command | Ja |
| Args | List<string> | Command-Arguments | Nein |

---

## AdminTeleport (2301)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Admin

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| X | float | Target X | Ja |
| Y | float | Target Y | Ja |
| ZoneId | uint | Zone-ID | Nein |

---

## AdminKick (2310)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 GM

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| PlayerName | string | Zu kickender Spieler | Ja |
| Reason | string | Kick-Reason | Ja |

---

## AdminBan (2311)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 Admin

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| PlayerName | string | Zu bannender Spieler | Ja |
| Duration | int | Duration (Stunden, 0=permanent) | Ja |
| Reason | string | Ban-Reason | Ja |

---

## AdminMute (2312)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** 👑 GM

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| PlayerName | string | Zu mutender Spieler | Ja |
| Duration | int | Duration (Minuten) | Ja |

---

**Letzte Aktualisierung**: 2025-12-25  
**Version**: 1.0.0

[← Zurück zur Übersicht](README.md)
