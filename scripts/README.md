# 2DMMO Scripts

## create-issues.sh

Dieses Script erstellt alle Issues aus `docs/ISSUES_ROADMAP.md` automatisch in GitHub, inklusive:

- **Labels** für Typ, Bereich, Priorität und Status
- **Sub-Issues** für bestehende Epik-Issues (#8, #9, #11, #12, #14, #15)
- **Neue Epik-Issues** für Phase 2-4 (Verbindung, Shared DTOs, Logging, Tilemap, Persistenz, Chat, Gameplay, etc.)
- **Verlinkungen** zwischen Parent-Issues und Sub-Issues

### Voraussetzungen

1. **GitHub CLI (gh)** muss installiert sein:
   ```bash
   # macOS
   brew install gh
   
   # Ubuntu/Debian
   sudo apt install gh
   
   # Windows
   winget install GitHub.cli
   ```

2. **Authentifizierung** bei GitHub:
   ```bash
   gh auth login
   ```

### Verwendung

```bash
# Im Repository-Root ausführen
cd /pfad/zu/2DMMO
./scripts/create-issues.sh
```

Das Script wird:
1. Alle benötigten Labels erstellen (falls nicht vorhanden)
2. Sub-Issues für die 6 bestehenden Epik-Issues erstellen
3. ~25 neue Epik-Issues mit Sub-Issues erstellen
4. Die Parent-Issues (#8, #9, #11, #12, #14, #15) mit Links zu den Sub-Issues aktualisieren

### Erstellte Issue-Struktur

```
Phase 2 (Feinschliff)
├── Epik: Verbindungsaufbau-Flow
│   ├── ConnectionConfig und ConnectionState
│   ├── Retry-Logik im NetworkClient
│   └── UI-Feedback für Verbindungsstatus
├── Epik: Shared DTOs
└── Epik: Logging

Phase 3 (Welt & Movement)
├── Epik: 2D-Tilemap
├── Epik: Spawn-Logik
├── Epik: Koordinaten-Mapping
└── Epik: Weltgrenzen

Phase 4 (Persistenz, Chat, Gameplay)
├── Epik: Datenmodell Persistenz
├── Epik: Persistenzschicht
├── Epik: Login laden
├── Epik: Logout speichern
├── Epik: Chat-Nachrichtentypen
├── Epik: Chat-Server
├── Epik: Chat-UI
├── Epik: Action-Nachrichtentypen
├── Epik: Action-Handler
├── Epik: Action-Visualisierung
├── Epik: Code-Cleanup
├── Epik: Dokumentation
└── Epik: Testplan
```

### Labels

| Kategorie | Labels |
|-----------|--------|
| **Typ** | `type:feature`, `type:chore`, `type:test`, `type:documentation`, `type:bug`, `type:epic` |
| **Bereich** | `area:server`, `area:client`, `area:network`, `area:persistenz`, `area:chat`, `area:gameplay` |
| **Priorität** | `priority:p1`, `priority:p2`, `priority:p3` |
| **Status** | `status:epic`, `status:blocked`, `status:ready` |

### Hinweise

- Das Script fragt vor dem Erstellen nach Bestätigung
- Bereits existierende Labels werden übersprungen
- Die Issue-Nummern werden automatisch erfasst und für Verlinkungen verwendet
- Nach dem Ausführen werden alle erstellten Issues mit ihren Nummern aufgelistet
