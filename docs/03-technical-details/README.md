# 🛠️ Technische Details

Dieser Bereich enthält **detaillierte technische Spezifikationen** und Implementierungs-Entscheidungen.

---

## 📚 Dokumente in diesem Bereich

### [Technical Design Document](TECHNICAL_DESIGN.md)
Ausführliche technische Entscheidungen für die Implementierung:
- Server Game Loop (Tick-Rate, Phasen)
- Client Szenen-Struktur
- Thread-Modell
- Collision & Prediction
- Entity-System
- Zone-Definition & Statische Entities
- Netzwerk-Flow
- Logging & Monitoring
- Testing-Strategie
- Entscheidungslog

**Zielgruppe**: Entwickler (fortgeschritten)

---

## 🔗 Verwandte Dokumentation

- **System-Architektur**: Siehe [02-architecture/README.md](../02-architecture/README.md)
- **Gameplay-Design**: Siehe [01-overview/GAME_DESIGN_DOCUMENT.md](../01-overview/GAME_DESIGN_DOCUMENT.md)
- **Netzwerk-Protokoll**: Siehe [02-architecture/NETWORK_PROTOCOL.md](../02-architecture/NETWORK_PROTOCOL.md)

---

## 📝 Unterschied zu Architecture

| Architecture (02) | Technical Details (03) |
|------------------|----------------------|
| **WAS** wird gebaut | **WIE** wird es gebaut |
| System-Design | Implementierungs-Entscheidungen |
| Komponenten-Übersicht | Detaillierte Spezifikationen |
| High-Level Entscheidungen | Low-Level Entscheidungen |

**Beispiel**:
- **Architecture**: "Wir verwenden einen Game Loop mit 25 Hz"
- **Technical Details**: "Der Game Loop hat 3 Phasen (Input → Update → Output), verwendet Fixed Timestep, und hat ein Budget von 33ms pro Tick mit Overrun-Logging"

---

**Navigation**: [← Zurück zur Hauptdokumentation](../README.md)
