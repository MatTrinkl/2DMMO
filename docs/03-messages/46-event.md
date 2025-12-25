# 🎉 Events / Seasonal Content Messages (4600-4699)

**Kategorie:** 46  
**Range:** 4600-4699  
**Phase:** Phase 3  
**Status:** ✅ Dokumentiert

[← Zurück zur Übersicht](README.md)

---

## 📋 Inhaltsverzeichnis

1. [EventStart (4600)](#eventstart-4600) - Event startet
2. [EventEnd (4601)](#eventend-4601) - Event endet
3. [EventProgress (4602)](#eventprogress-4602) - Event-Fortschritt
4. [SeasonalStart (4603)](#seasonalstart-4603) - Seasonal-Event startet
5. [SeasonalEnd (4604)](#seasonalend-4604) - Seasonal-Event endet

---

## EventStart (4600)

**Richtung:** 📡 Broadcast  
**Frequenz:** Selten  
**Authentifizierung:** Nein

Ein In-Game-Event startet (World-Boss, PvP-Event, Timed-Event, Holiday-Event).

### Im Scope ✅
- Event-Notification an alle Online-Spieler
- Event-ID, Name, Dauer
- Event-Typ (World-Boss, PvP, Seasonal, etc.)

### Nicht im Scope ❌
- World-Boss-Mechanics → use `WorldEventStart` (26xx)
- PvP-Events → use `PvP` Messages (25xx)

### Broadcast Payload
| Field | Type | Description | Required |
|-------|------|-------------|----------|
| EventId | uint | Event-Identifier | Yes |
| EventName | string | Anzeigename des Events | Yes |
| Duration | int | Dauer in Minuten | Yes |
| EventType | EventType | Type (WorldBoss, PvP, etc.) | Yes |

---

## EventEnd (4601)

**Richtung:** 📡 Broadcast  
**Frequenz:** Selten  
**Authentifizierung:** Nein

Event ist beendet.

---

## EventProgress (4602)

**Richtung:** 📡 Broadcast  
**Frequenz:** Gelegentlich  
**Authentifizierung:** Nein

Event-Fortschritt Update (z.B. "Boss bei 50% HP", "Team Red führt mit 1500:1200").

---

## SeasonalStart (4603)

**Richtung:** 📡 Broadcast  
**Frequenz:** Sehr selten  
**Authentifizierung:** Nein

Seasonal-Event startet (Halloween, Christmas, Summer-Event, Anniversary).

### Im Scope ✅
- Seasonal-Notification
- Seasonal-Questline verfügbar
- Seasonal-Vendors spawnen
- Seasonal-Cosmetics verfügbar

---

## SeasonalEnd (4604)

**Richtung:** 📡 Broadcast  
**Frequenz:** Sehr selten  
**Authentifizierung:** Nein

Seasonal-Event endet (Seasonal-Quests, Vendors, Cosmetics entfernt).

---

**Letzte Aktualisierung**: 2025-12-25  
**Version**: 2.0.0

[← Zurück zur Übersicht](README.md)
