# Issue-Aktualisierungs-Leitfaden

**Version:** 3.0.0  
**Letzte Aktualisierung:** 2025-12-09  
**Status:** Prozess-Guide

---

## 📋 Übersicht

Dieser Leitfaden beschreibt **wie** Issues aktualisiert werden sollten, wenn:
1. Ein großes Issue in Sub-Issues aufgeteilt wird (Epik-Issues)
2. Architektur-Änderungen existierende Issues betreffen
3. Issues auf neue Konzepte migriert werden müssen

> **📌 Hinweis:** Für spezifische Architektur-Details siehe [02-architecture/](../02-architecture/).

---

## 🎯 Wann Issues aktualisieren?

### 1. Epik-Issues aufteilen
Ein Issue ist zu groß, wenn:
- Mehr als 10 Aufgaben enthalten sind
- Mehrere Wochen Arbeit erforderlich sind
- Verschiedene Komponenten/Bereiche betroffen sind
- Mehrere Entwickler parallel arbeiten könnten

**Aktion:** Issue in Sub-Issues aufteilen

### 2. Architektur-Änderungen
Wenn Architektur-Dokumente aktualisiert werden:
- Prüfen welche Issues betroffen sind
- Issue-Beschreibungen aktualisieren
- Neue Anforderungen ergänzen
- Obsolete Aufgaben entfernen oder als "veraltet" markieren

**Aktion:** Issue-Body aktualisieren mit Verweis auf neue Architektur

### 3. Konzept-Migration
Wenn grundlegende Konzepte sich ändern (z.B. World → Zone-basiert):
- Alle betroffenen Issues identifizieren
- Aufgaben an neue Konzepte anpassen
- DTOs/Klassen entsprechend umbenennen
- Akzeptanzkriterien aktualisieren

**Aktion:** Systematische Updates mit Hinweis auf Migration

---

## 📝 Wie Issues aktualisieren

### Template für Epik-Issue mit Sub-Issues

```markdown
[Original Issue Titel]

[Kurze Beschreibung des Gesamtziels]

> **📌 Dies ist ein Epik-Issue.** Die Arbeit wurde in folgende Sub-Issues aufgeteilt:

**Sub-Issues:**
- [ ] #XXX [Sub-Issue Titel 1]
- [ ] #XXX [Sub-Issue Titel 2]
- [ ] #XXX [Sub-Issue Titel 3]

**Dieses Issue kann geschlossen werden, wenn alle Sub-Issues erledigt sind.**

---

<details>
<summary>Ursprüngliche Aufgaben (zur Referenz)</summary>

[Original Aufgabenliste]

</details>

---

**Verwandte Dokumentation:**
- [Link zu Architektur-Doku]
- [Link zu Technical Design]
```

### Template für Architektur-Update

```markdown
[Original Issue Titel]

> **📌 Architektur-Update:** [Kurze Beschreibung der Änderung]
> Siehe [Link zur Architektur-Doku] für Details.

**Aktualisierte Anforderungen:**
- [ ] [Neue/geänderte Aufgabe 1]
- [ ] [Neue/geänderte Aufgabe 2]

**Akzeptanzkriterien:**
- [ ] [Kriterium basierend auf neuer Architektur]
- [ ] [Alle bestehenden Tests funktionieren weiterhin]

---

<details>
<summary>Ursprüngliche Aufgaben (zur Referenz)</summary>

[Original Aufgabenliste]

</details>
```

### Template für Sub-Issue

```markdown
[Sub-Issue Titel]

**Teil von:** #XXX (Eltern-Issue)

**Beschreibung:**
[1-2 Sätze was dieses Sub-Issue macht]

**Aufgaben:**
- [ ] [Konkrete Aufgabe 1]
- [ ] [Konkrete Aufgabe 2]
- [ ] [Konkrete Aufgabe 3]

**Akzeptanzkriterien:**
- [ ] [Messbares Kriterium 1]
- [ ] [Messbares Kriterium 2]

**Verwandte Dokumentation:**
- [Link zu relevanter Doku]

**Labels:** `type:feature`, `area:[component]`, `priority:pX`
```

---

## 🔄 Prozess für große Updates

### Schritt 1: Analyse
1. Lies die Architektur-Änderung durch
2. Identifiziere betroffene Issues (nutze Labels, Suche)
3. Erstelle Liste der Updates

### Schritt 2: Planung
1. Entscheide: Sub-Issue-Aufteilung nötig?
2. Priorisiere Issues nach Abhängigkeiten
3. Erstelle Update-Plan (welches Issue zuerst)

### Schritt 3: Updates durchführen
1. Füge Hinweis am Anfang des Issue-Body hinzu
2. Nutze `<details>` für alte Aufgaben (Referenz)
3. Ergänze Links zur aktualisierten Dokumentation
4. Markiere obsolete Aufgaben als durchgestrichen ~~wie hier~~

### Schritt 4: Kommunikation
1. Kommentiere im Issue über die Änderung
2. Verlinke relevante PRs oder Docs
3. Erwähne betroffene Entwickler (@mentions)
4. Update Issue-Hierarchie-Dokument wenn nötig

---

## 🏗️ Best Practices

### ✅ DO

- **Bewahre Original-Aufgaben** in `<details>` Tags für Referenz
- **Verwende klare Hinweise** mit 📌 für Updates
- **Verlinke Dokumentation** statt Details zu duplizieren
- **Nutze Labels** konsistent (type:, area:, priority:)
- **Schließe Epik-Issues** erst wenn alle Sub-Issues erledigt
- **Update Dependencies** in Issue-Hierarchie-Dokument

### ❌ DON'T

- ~~Original-Aufgaben löschen~~ → In `<details>` verschieben
- ~~Code-Details ins Issue kopieren~~ → Auf Doku verlinken
- ~~Issue ohne Sub-Issues schließen~~ → Warten bis alle done
- ~~Kommentare nutzen für permanente Updates~~ → Issue-Body editieren
- ~~Zu viele Sub-Issues erstellen~~ → Max 5-7 pro Epik

---

## 📊 Beispiel-Workflow: Zone-Konzept Integration

### Situation
Architektur wurde von monolithischer `World`-Klasse auf Zone-basiertes System umgestellt.

### Betroffene Issues identifiziert
- #7: World/Player/Entity Implementation
- #74: WorldState Broadcast
- #75: Client WorldState Rendering
- #76: Movement Validation

### Update-Prozess

1. **Issue #7 aufteilen** in Sub-Issues:
   - Zone-Klasse implementieren
   - ZoneManager implementieren
   - Zone-Konfiguration laden
   - Player Zone-Zugehörigkeit

2. **Issues #74, #75, #76 aktualisieren**:
   - Hinweis hinzufügen: WorldState → ZoneState
   - Aufgaben anpassen (Zone-spezifisch)
   - Link zu ID-System-Doku (für ZoneId Details)

3. **Dokumentation**:
   - Update ISSUE_HIERARCHY.md
   - Vermerke in diesem Guide (Beispiel-Sektion)

---

## 🔗 Verwandte Dokumentation

### Architektur
- [ID-System](../02-architecture/ID_SYSTEM.md) - Entity Identity, ZoneId Ranges
- [Architektur-Übersicht](../02-architecture/README.md) - System-Design
- [Server-Komponenten](../02-architecture/SERVER_COMPONENTS.md) - Komponenten-Details

### Projekt-Management
- [Issue-Hierarchie](ISSUE_HIERARCHY.md) - Issue-Beziehungen und Abhängigkeiten
- [Sub-Issues](SUB_ISSUES.md) - Konkrete Sub-Issue-Vorschläge
- [Feature-Roadmap](FEATURE_ROADMAP.md) - Geplante Features

---

## 📝 Verwendung dieses Guides

1. **Bei Issue-Erstellung**: Nutze Templates für konsistente Struktur
2. **Bei Architektur-Änderung**: Folge dem Update-Prozess
3. **Bei Unsicherheit**: Referenziere Best Practices
4. **Für Reviews**: Prüfe ob Updates diesem Guide folgen

---

## 🆘 Häufige Fragen

**Q: Wann Sub-Issues vs. einfach Aufgaben im Issue?**  
A: Sub-Issues wenn >5 Aufgaben ODER verschiedene Entwickler ODER verschiedene Komponenten.

**Q: Alte Aufgaben löschen oder behalten?**  
A: In `<details>` verschieben für Kontext, aber nicht löschen.

**Q: Wie mit veralteten Issues umgehen?**  
A: Label `status:outdated` + Kommentar warum + Link zu Ersatz-Issue.

**Q: Issue-Beschreibung vs. Kommentar für Updates?**  
A: Issue-Body für permanente Updates, Kommentare für Diskussion/Kontext.

---

**Version History:**
- v3.0.0 (2025-12-09): Umstrukturiert als Prozess-Guide, spezifische Details entfernt
- v2.0.0 (2025-12-09): Konsolidiert aus ISSUE_UPDATES.md + ZONE_CONCEPT_UPDATES.md
- v1.0.0: Original ISSUE_UPDATES.md

---

**Navigation**: [← Zurück zur Projekt-Management-Übersicht](README.md)
