# 🎮 Matchmaking Messages (2700-2799)

**Kategorie:** 27  
**Range:** 2700-2799  


[← Zurück zur Übersicht](README.md)

---

## 📋 Übersicht

Matchmaking für Dungeons, PvP, und Raids.

---

## MatchmakingQueue (2700)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| QueueType | string | "dungeon", "pvp", "raid" | Ja |
| Role | string | "tank", "healer", "dps" | Ja |

---

## MatchFound (2701)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| MatchId | string | Match-ID | Ja |
| AcceptTimeout | int | Sekunden zum Accept | Ja |

---

## MatchAccept (2702)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| MatchId | string | Match-ID | Ja |

---

**Letzte Aktualisierung**: 2025-12-25  
**Version**: 1.0.0

[← Zurück zur Übersicht](README.md)
