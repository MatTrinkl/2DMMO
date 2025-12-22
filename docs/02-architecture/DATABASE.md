# 🗄️ Datenbank-Strategie

## 2DMMO – PostgreSQL Persistence

**Version:** 1.1.0  
**Letzte Aktualisierung:** 2025-12-02  
**Teil von:** [Architektur-Dokumentation](README.md)

---

## 📋 Übersicht

Diese Dokumentation beschreibt die Datenbank-Strategie für das 2DMMO, einschließlich Write-Strategien, Connection Pooling und Persistence Patterns.

---

## Write-Strategien

```
┌─────────────────────────────────────────────────────────┐
│               PERSISTENCE STRATEGIE                      │
│                                                          │
│  ┌─────────────────────────────────────────────────┐    │
│  │  IMMEDIATE WRITE (Sofort in DB)                 │    │
│  │                                                  │    │
│  │  • Charakter-Erstellung                         │    │
│  │  • Wichtige Item-Transaktionen                  │    │
│  │  • Gold-Transfers                               │    │
│  │  • Gilden-Änderungen                            │    │
│  │  • Level-Up                                     │    │
│  └─────────────────────────────────────────────────┘    │
│                                                          │
│  ┌─────────────────────────────────────────────────┐    │
│  │  BATCHED WRITE (Alle 10-30 Sekunden)            │    │
│  │                                                  │    │
│  │  • Spieler-Positionen                           │    │
│  │  • HP/Mana Zustand                              │    │
│  │  • Spielzeit-Tracking                           │    │
│  │  • Quest-Fortschritt                            │    │
│  └─────────────────────────────────────────────────┘    │
│                                                          │
│  ┌─────────────────────────────────────────────────┐    │
│  │  LOGOUT WRITE (Bei Disconnect)                  │    │
│  │                                                  │    │
│  │  • Kompletter Charakter-State                   │    │
│  │  • Alle Pending Changes                         │    │
│  └─────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────┘
```

---

## Connection Pooling

```csharp
// appsettings.json
{
  "ConnectionStrings": {
    "PostgreSQL": "Host=...;Database=2dmmo;Username=...;Password=...;Pooling=true;MinPoolSize=5;MaxPoolSize=100"
  }
}
```

### Pool-Konfiguration

| Parameter | Wert | Beschreibung |
|-----------|------|--------------|
| `Pooling` | `true` | Connection Pooling aktiviert |
| `MinPoolSize` | `5` | Minimum aktive Connections |
| `MaxPoolSize` | `100` | Maximum Connections |

---

## Write-Strategie Details

### Immediate Write

Daten, die sofort persistiert werden müssen:

- **Charakter-Erstellung** - Neuer Charakter muss sofort in DB
- **Item-Transaktionen** - Käufe, Verkäufe, Trades
- **Gold-Transfers** - Zwischen Spielern oder NPC
- **Gilden-Änderungen** - Beitritt, Austritt, Rang-Änderungen
- **Level-Up** - Wichtiger Fortschritt

### Batched Write

Daten, die periodisch persistiert werden:

- **Positionen** - Alle 10 Sekunden
- **HP/Mana** - Alle 30 Sekunden
- **Spielzeit** - Alle 60 Sekunden
- **Quest-Fortschritt** - Bei Änderung, max alle 30 Sekunden

### Logout Write

Bei Disconnect eines Spielers:

- Kompletter Charakter-State wird gespeichert
- Alle ausstehenden Batched Changes
- Session wird aus Redis entfernt

---

## Verwandte Dokumentation

- [ID-System](ID_SYSTEM.md) - AccountId, CharacterId (persistente IDs)
- [Redis-Strategie](REDIS.md) - Cache Layer
- [Game Loop](GAME_LOOP.md) - Persistence Phase
- [Sicherheit](SECURITY.md) - Datenbank-Security

---

## 🔗 Nützliche Links

- [PostgreSQL Documentation](https://www.postgresql.org/docs/)
- [Npgsql - .NET PostgreSQL Provider](https://www.npgsql.org/)
- [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/)
- [Connection Pooling Best Practices](https://www.npgsql.org/doc/connection-string-parameters.html)

---

*Teil der [Architektur-Dokumentation](README.md)*
