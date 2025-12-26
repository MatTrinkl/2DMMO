# 🎯 targeting Messages (1200-1299)

**Kategorie:** 12  
**Range:** 1200-1299  
**Phase:** Phase 2  
**Status:** 🟡

[← Zurück zur Übersicht](README.md)

---

## 📋 Übersicht

Diese Kategorie umfasst alle Messages für **Targeting** Funktionalität im 2DMMO.

Target-Selection, Focus-Target, Marking und Assist-Funktionalität.

**🔄 DTO-System:**  
Targeting Messages wie `TargetChanged` (1200) werden in Phase 2 ein `TargetEntityDto` verwenden:
- Minimale Target-Informationen für UI (Name, Level, Health, Buffs)
- Keine sensiblen Server-Daten wie AccountId oder Gold
- Optimiert für Target-Frame UI-Updates

Siehe [DTO_ARCHITECTURE.md](DTO_ARCHITECTURE.md) für geplante `TargetEntityDto` Struktur.

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
