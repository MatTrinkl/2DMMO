# 👾 entity Messages (1400-1499)

**Kategorie:** 14  
**Range:** 1400-1499  
**Phase:** Phase 2  
**Status:** 🟡

[← Zurück zur Übersicht](README.md)

---

## 📋 Übersicht

Diese Kategorie umfasst alle Messages für **Entity** Funktionalität im 2DMMO.

Entity-Spawning und Synchronisation für Spieler, NPCs und Objekte.

**🔄 DTO-System:**  
Entity Messages wie `EntitySpawn` (1400) und `EntityUpdate` (1404) werden in Phase 2 mit DTOs arbeiten:
- `EntitySpawn` sollte `IEntityDto` verwenden (polymorphes Union-Interface)
- `EntityUpdate` sollte Delta-DTOs verwenden (nur geänderte Properties)
- Ermöglicht type-sichere Entity-Updates ohne sensible Server-Daten

Siehe [DTO_ARCHITECTURE.md](DTO_ARCHITECTURE.md) für Details zu `IEntityDto`, `PlayerEntityDto` und `NpcEntityDto`.

---

## 📝 Message-Liste

Siehe [MESSAGES.md](../02-architecture/MESSAGES.md) für die vollständige Liste aller MessageTypes in dieser Kategorie.

### Implementierungs-Status

- **Prototyp**: Grundlegende Funktionalität implementiert
- **Phase 2**: Erweiterte Features geplant  
- **Phase 3**: Zukünftige Erweiterungen

---

## 🎯 Wichtige Messages

Die wichtigsten Messages in dieser Kategorie werden im Laufe der Entwicklung hier detailliert dokumentiert.

Für die aktuelle MessageType-Definition siehe:
- [MessageType.cs](../../../shared/Mmo.Shared/Enums/MessageType.cs)
- [MESSAGES.md](../02-architecture/MESSAGES.md)

---

## 🔗 Verwandte Kategorien

Siehe [Message-Referenz Übersicht](README.md) für Links zu verwandten Message-Kategorien.

---

**Letzte Aktualisierung**: 2025-12-17  
**Version**: 1.0.0

[← Zurück zur Übersicht](README.md)
