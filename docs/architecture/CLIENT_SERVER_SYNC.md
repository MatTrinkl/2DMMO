# 🔄 Client-Server Synchronisation

## 2DMMO – Prediction, Interpolation & Reconciliation

**Version:** 1.1.0  
**Letzte Aktualisierung:** 2025-12-02  
**Teil von:** [Architektur-Dokumentation](../ARCHITECTURE.md)

---

## 📋 Übersicht

Diese Dokumentation beschreibt die Client-Server Synchronisation, einschließlich Client-Side Prediction, Entity Interpolation und Server Reconciliation.

---

## Client-Side Prediction

```
┌─────────────────────────────────────────────────────────┐
│              CLIENT-SIDE PREDICTION                      │
│                                                          │
│  Problem: Server ist 30 Hz, Client ist 60 FPS           │
│  Lösung:  Client sagt vorher, Server korrigiert         │
│                                                          │
│  ┌─────────────────────────────────────────────────┐    │
│  │                    CLIENT                        │    │
│  │                                                  │    │
│  │  1. Spieler drückt "W" (nach vorne)             │    │
│  │                                                  │    │
│  │  2. Client bewegt Spieler SOFORT lokal          │    │
│  │     Position: (100, 100) → (100, 105)           │    │
│  │                                                  │    │
│  │  3. Client sendet Input an Server               │    │
│  │     { seq: 42, input: "forward", timestamp: T } │    │
│  │                                                  │    │
│  │  4. Client speichert Prediction                 │    │
│  │     predictions[42] = { pos: (100, 105) }       │    │
│  └────────────────────────┬────────────────────────┘    │
│                           │                              │
│                           ▼                              │
│  ┌─────────────────────────────────────────────────┐    │
│  │                    SERVER                        │    │
│  │                                                  │    │
│  │  5. Server empfängt Input                       │    │
│  │                                                  │    │
│  │  6. Server validiert & simuliert                │    │
│  │     (evtl. andere Position wegen Kollision!)    │    │
│  │                                                  │    │
│  │  7. Server sendet authoritative Position        │    │
│  │     { seq: 42, pos: (100, 103), serverTime: T } │    │
│  └────────────────────────┬────────────────────────┘    │
│                           │                              │
│                           ▼                              │
│  ┌─────────────────────────────────────────────────┐    │
│  │              CLIENT RECONCILIATION               │    │
│  │                                                  │    │
│  │  8. Client empfängt Server-Position             │    │
│  │                                                  │    │
│  │  9. Vergleicht mit Prediction[42]               │    │
│  │     Predicted: (100, 105)                       │    │
│  │     Server:    (100, 103)                       │    │
│  │     Differenz: 2 Einheiten!                     │    │
│  │                                                  │    │
│  │  10. Client korrigiert Position                 │    │
│  │      (Interpolation für Smoothness)             │    │
│  │                                                  │    │
│  │  11. Alle Predictions nach seq:42 neu berechnen │    │
│  └─────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────┘
```

---

## Entity Interpolation (für andere Spieler)

```
┌─────────────────────────────────────────────────────────┐
│             ENTITY INTERPOLATION                         │
│                                                          │
│  Problem: Andere Spieler kommen nur mit 30 Hz an        │
│  Lösung:  Zwischen zwei bekannten Positionen            │
│           interpolieren                                  │
│                                                          │
│  Server-Updates:                                        │
│                                                          │
│  T=0ms      T=33ms     T=66ms     T=100ms               │
│    ●──────────●──────────●──────────●                   │
│  (10,10)   (10,15)   (10,20)   (10,25)                 │
│                                                          │
│  Client Rendering (mit 100ms Buffer):                   │
│                                                          │
│  Render-Zeit   Interpolierte Position                   │
│  ──────────────────────────────────────                 │
│  T=100ms       (10,10)  ← Zeigt T=0 Daten              │
│  T=116ms       (10,12.5) ← Interpoliert                │
│  T=133ms       (10,15)  ← Zeigt T=33 Daten             │
│  T=150ms       (10,17.5) ← Interpoliert                │
│                                                          │
│  Buffer sorgt für smooth movement trotz Jitter!         │
└─────────────────────────────────────────────────────────┘
```

---

## Zusammenfassung der Techniken

| Technik | Anwendung | Zweck |
|---------|-----------|-------|
| **Client-Side Prediction** | Eigener Spieler | Sofortige Reaktion auf Input |
| **Server Reconciliation** | Eigener Spieler | Korrektur bei Abweichungen |
| **Entity Interpolation** | Andere Spieler | Smooth Movement bei 30 Hz |
| **Input Buffering** | Server | Deterministische Verarbeitung |

---

## Verwandte Dokumentation

- [Game Loop](GAME_LOOP.md) - Server Tick Timing
- [Netzwerk-Protokoll](NETWORK_PROTOCOL.md) - Message Timing
- [Messages](MESSAGES.md) - PositionUpdate Format

---

## 🔗 Nützliche Links

- [Gabriel Gambetta: Client-Side Prediction](https://www.gabrielgambetta.com/client-side-prediction-server-reconciliation.html)
- [Valve: Source Multiplayer Networking](https://developer.valvesoftware.com/wiki/Source_Multiplayer_Networking)
- [Glenn Fiedler: Networked Physics](https://gafferongames.com/categories/networked-physics/)

---

*Teil der [Architektur-Dokumentation](../ARCHITECTURE.md)*
