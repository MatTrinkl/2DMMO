# 😄 Emote Messages (2200-2299)

**Kategorie:** 22  
**Range:** 2200-2299  


[← Zurück zur Übersicht](README.md)

---

## 📋 Übersicht

Emote-System für Charakter-Animationen.

---

## EmotePlay (2200)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| EmoteId | uint | Emote-ID | Ja |
| TargetId | int | Target (optional) | Nein |

---

## EmoteBroadcast (2201)

**Richtung:** 📡 Broadcast  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| PlayerId | int | Player-ID | Ja |
| EmoteId | uint | Emote-ID | Ja |

---

**Letzte Aktualisierung**: 2025-12-25  
**Version**: 1.0.0

[← Zurück zur Übersicht](README.md)
