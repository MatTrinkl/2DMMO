# 📋 Projekt-Management

Dieser Bereich enthält **Issue-Tracking, Roadmaps und Planungsdokumente**.

---

## 📚 Dokumente in diesem Bereich

### [Issue-Hierarchie](ISSUE_HIERARCHY.md)
Zeigt die Beziehungen zwischen Issues und die empfohlene Bearbeitungsreihenfolge:
- Phase 1: Grundlagen & Projektsetup
- Phase 2: Basis-Netzwerk & Auth-Skelett
- Phase 3: Gameplay-Loop
- Phase 4: Testing & Optimierung
- Abhängigkeiten zwischen Issues

**Zielgruppe**: Entwickler, Projektmanagement

---

### [Issue-Aktualisierungs-Leitfaden](ISSUE_UPDATES_GUIDE.md)
**Konsolidiert:** ISSUE_UPDATES.md + ZONE_CONCEPT_UPDATES.md

Beschreibt wie bestehende Issues aktualisiert werden:
- Aktualisierungen für Epik-Issues (Sub-Issues)
- Integration des Zone-Konzepts in bestehende Issues
- Zone-Struktur (ZoneManager, Zone-Klasse)
- Templates für Issue-Updates

**Zielgruppe**: Projektmanagement, Entwickler (Server)

---

### [Sub-Issues](SUB_ISSUES.md)
**Aufgeteilt aus:** ISSUES_ROADMAP.md (Teil 1)

Detaillierte Sub-Issue-Vorschläge für bestehende große Issues:
- Issue #8: NetworkServer → 3 Sub-Issues
- Issue #10: Login-Flow → 3 Sub-Issues
- Issue #11: MessagePack → 3 Sub-Issues
- Weitere Aufspaltungen für bessere Handhabung

**Zielgruppe**: Entwickler, Projektmanagement

---

### [Feature-Roadmap](FEATURE_ROADMAP.md)
**Aufgeteilt aus:** ISSUES_ROADMAP.md (Teil 2)

Neue Feature-Issues für Funktionalität bis Ende Phase 4:
- Phase 2: Verbindungsaufbau, Disconnect-Handling
- Phase 3: Movement, Collision, Chat
- Phase 4: Zone-Transfer, NPCs, Performance
- Alle als Epik-Issues strukturiert

**Zielgruppe**: Projektmanagement, Product Owner

---

## 🔗 Verwandte Dokumentation

- **Architektur (Zone-System)**: Siehe [ID-System](../02-architecture/ID_SYSTEM.md)
- **Prototyp-Scope**: Siehe [Prototype Scope](../01-overview/PROTOTYPE_SCOPE.md)

---

## 📊 Workflow

```
1. Neue Features identifizieren
   ↓
2. Issue erstellen (siehe FEATURE_ROADMAP.md)
   ↓
3. Hierarchie prüfen (siehe ISSUE_HIERARCHY.md)
   ↓
4. Große Issues in Sub-Issues aufteilen (siehe SUB_ISSUES.md)
   ↓
5. Bei Architektur-Änderungen: ISSUE_UPDATES_GUIDE.md nutzen
   ↓
6. Issues bearbeiten (nach Priorisierung)
```

---

## 📝 Änderungslog

### Version 2.0.0 (2025-12-09)
- ✅ **Konsolidiert**: ISSUE_UPDATES.md + ZONE_CONCEPT_UPDATES.md → ISSUE_UPDATES_GUIDE.md
- ✅ **Aufgeteilt**: ISSUES_ROADMAP.md → SUB_ISSUES.md + FEATURE_ROADMAP.md
- ✨ Verbesserte Struktur für bessere Navigierbarkeit
- 📖 Klarere Kategorisierung der Dokumente

---

**Navigation**: [← Zurück zur Hauptdokumentation](../README.md)
