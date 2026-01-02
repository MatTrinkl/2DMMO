# 🏅 Leaderboard Messages (2800-2899)

**Kategorie:** 28  
**Range:** 2800-2899  


[← Zurück zur Übersicht](README.md)

---

## 📋 Übersicht

Leaderboards und Rankings.

---

## LeaderboardRequest (2800)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| LeaderboardType | string | "pvp", "pve", "arena" | Ja |
| Bracket | string | "2v2", "3v3", "5v5" | Nein |

---

## LeaderboardResponse (2801)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** Nein

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Entries | List<LeaderboardEntry> | Top-Entries | Ja |

**LeaderboardEntry**:
| Feld | Typ | Beschreibung |
|------|-----|--------------|
| Rank | int | Rank |
| PlayerName | string | Name |
| Rating | int | Rating/Score |

---

**Letzte Aktualisierung**: 2025-12-25  
**Version**: 1.0.0

[← Zurück zur Übersicht](README.md)
