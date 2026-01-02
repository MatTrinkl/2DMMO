# ⚔️ PvP Messages (2500-2599)

**Kategorie:** 25  
**Range:** 2500-2599  


[← Zurück zur Übersicht](README.md)

---

## 📋 Übersicht

PvP-System mit Arenas und Battlegrounds.

---

## PvPFlagToggle (2500)

**Richtung:** �� Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Enabled | bool | PvP aktivieren? | Ja |

---

## PvPKill (2501)

**Richtung:** 📡 Broadcast  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| KillerId | int | Killer-ID | Ja |
| VictimId | int | Victim-ID | Ja |
| HonorGain | int | Honor-Points | Ja |

---

## ArenaQueue (2510)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| ArenaType | string | "2v2", "3v3", "5v5" | Ja |

---

## ArenaMatch (2511)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| MatchId | string | Match-ID | Ja |
| Team1 | List<int> | Team 1 Player-IDs | Ja |
| Team2 | List<int> | Team 2 Player-IDs | Ja |

---

## BattlegroundQueue (2520)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| BattlegroundId | uint | BG-ID | Ja |

---

**Letzte Aktualisierung**: 2025-12-25  
**Version**: 1.0.0

[← Zurück zur Übersicht](README.md)
