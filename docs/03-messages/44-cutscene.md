# 🎬 Cutscenes / Cinematics Messages (4400-4499)

**Kategorie:** 44  
**Range:** 4400-4499  
**Phase:** Phase 3  
**Status:** ✅ Dokumentiert

[← Zurück zur Übersicht](README.md)

---

## 📋 Inhaltsverzeichnis

1. [CutsceneStart (4400)](#cutscenestart-4400) - Cutscene starten
2. [CutsceneEnd (4401)](#cutsceneend-4401) - Cutscene beenden
3. [CutsceneSkip (4402)](#cutsceneskip-4402) - Cutscene überspringen
4. [CutscenePause (4403)](#cutscenepause-4403) - Cutscene pausieren
5. [CutsceneResume (4404)](#cutsceneresume-4404) - Cutscene fortsetzen
6. [CutsceneProgress (4405)](#cutsceneprogress-4405) - Cutscene Fortschritt

---

## CutsceneStart (4400)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

Startet eine Cutscene / Cinematic (Quest-Intro, Boss-Intro, Story-Events).

### Im Scope ✅
- Cutscene-ID und Dauer
- Kamera-Kontrolle übernehmen
- Eingaben blockieren
- Skippable vs Non-Skippable

### Nicht im Scope ❌
- Dialog-Text → use `NPCDialog` (13xx)
- Subtitle-Rendering → Client-Side

### Request Payload
| Field | Type | Description | Required |
|-------|------|-------------|----------|
| CutsceneId | uint | Cutscene-Identifier | Yes |
| Duration | float | Länge in Sekunden | Yes |
| Skippable | bool | Kann übersprungen werden | Yes |

---

## CutsceneEnd (4401)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

Beendet die Cutscene, gibt Kontrolle zurück an den Spieler.

---

## CutsceneSkip (4402)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

Überspringt die laufende Cutscene (wenn Skippable = true).

---

## CutscenePause (4403)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

Pausiert die Cutscene.

---

## CutsceneResume (4404)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja

Setzt die Cutscene fort.

---

## CutsceneProgress (4405)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja

Update über Cutscene-Fortschritt (für Synchronisation bei Group-Cutscenes).

---

**Letzte Aktualisierung**: 2025-12-25  
**Version**: 2.0.0

[← Zurück zur Übersicht](README.md)
