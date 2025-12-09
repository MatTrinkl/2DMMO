# 🎯 Prototyp-Scope

## 2DMMO – Phase 1: Prototyp

**Version:** 1.1.0  
**Letzte Aktualisierung:** 2025-12-02  
**Status:** In Entwicklung

---

## 📋 Übersicht

Dieses Dokument definiert den **exakten Scope** des Prototyps. Es dient als klare Abgrenzung, was implementiert werden muss und was explizit **nicht** Teil des Prototyps ist.

### Prototyp-Ziel

> **Zwei Spieler können sich über das Netzwerk verbinden, sehen sich gegenseitig in einer einfachen 2D-Welt und können sich bewegen.**

### Entwicklungsumgebung

| Aspekt | Prototyp |
|--------|----------|
| **Deployment** | Nur lokal (localhost) |
| **Authentication** | Nur Username (kein Passwort) |
| **Datenbank** | Keine (nur In-Memory) |
| **Redis** | Optional (kann ohne laufen) |
| **TLS** | Optional/Self-signed |

---

## ✅ Muss implementiert werden

### 1. Server-Grundgerüst

- [ ] TCP Server startet und akzeptiert Verbindungen
- [ ] MessagePack Serialisierung funktioniert
- [ ] Game Loop läuft mit 25 Hz (Tick-Rate)
- [ ] Verbindungs-Management (Connect/Disconnect)
- [ ] Logging (Console-Output reicht)

**Relevante Docs:**
- [.NET TCP Server](https://learn.microsoft.com/en-us/dotnet/fundamentals/networking/sockets/socket-services)
- [MessagePack-CSharp](https://github.com/MessagePack-CSharp/MessagePack-CSharp)

### 2. Client-Grundgerüst

- [ ] TCP Client verbindet sich zum Server
- [ ] MessagePack Serialisierung funktioniert
- [ ] Godot-Szene lädt und zeigt Spielwelt
- [ ] Input-Handling (WASD oder Pfeiltasten)
- [ ] Verbindungsstatus-Anzeige (Connected/Disconnected)

**Relevante Docs:**
- [Godot StreamPeerTCP](https://docs.godotengine.org/en/stable/classes/class_streampeertcp.html)
- [Godot Input Handling](https://docs.godotengine.org/en/stable/tutorials/inputs/index.html)

### 3. Shared Library

- [ ] Message-Typen definiert (Enum)
- [ ] DTOs für alle Nachrichten (MessagePack-Attribute)
- [ ] Gemeinsame Konstanten (Port, Tick-Rate, etc.)

### 4. Netzwerk-Nachrichten

| Message | Richtung | Beschreibung |
|---------|----------|--------------|
| `LoginRequest` | Client → Server | Username senden |
| `LoginResponse` | Server → Client | Erfolg + PlayerId |
| `PlayerJoined` | Server → Clients | Neuer Spieler ist beigetreten |
| `PlayerLeft` | Server → Clients | Spieler hat verlassen |
| `PositionUpdate` | Client → Server | Eigene Position senden |
| `WorldState` | Server → Clients | Alle Spieler-Positionen |
| `ChatMessage` | Bidirektional | Chat-Nachricht |

### 5. Spieler-Bewegung

- [ ] Client sendet Input an Server
- [ ] Server validiert Bewegung (Basis-Check)
- [ ] Server broadcastet neue Positionen
- [ ] Client zeigt andere Spieler an korrekter Position
- [ ] Client-Prediction für flüssige Bewegung (optional aber empfohlen)

**Relevante Docs:**
- [Gabriel Gambetta - Client-Side Prediction](https://www.gabrielgambetta.com/client-side-prediction-server-reconciliation.html)
- [Valve Multiplayer Networking](https://developer.valvesoftware.com/wiki/Source_Multiplayer_Networking)

### 6. Spielwelt (Minimal)

- [ ] Einfache Tilemap (Gras, vielleicht Wasser/Hindernisse)
- [ ] Feste Größe (z.B. 50x50 Tiles)
- [ ] Keine Zonen, keine Übergänge
- [ ] Spieler-Sprite (Placeholder OK)

**Relevante Docs:**
- [Godot TileMap](https://docs.godotengine.org/en/stable/classes/class_tilemap.html)
- [Godot 2D Tutorial](https://docs.godotengine.org/en/stable/getting_started/first_2d_game/index.html)

### 7. Chat (Basis)

- [ ] Texteingabe im Client
- [ ] Nachricht an Server senden
- [ ] Server broadcastet an alle
- [ ] Chat-Anzeige im Client (einfache Liste)

---

## ❌ Explizit NICHT im Prototyp

### Gameplay
- ❌ Kampfsystem
- ❌ NPCs / Monster
- ❌ Quests
- ❌ Inventar / Items
- ❌ Rassen- oder Klassenwahl
- ❌ Leveling / XP
- ❌ Tod / Respawn

### Technisch
- ❌ Echte Authentication (Email, Passwort, OAuth)
- ❌ Datenbank-Persistenz (PostgreSQL)
- ❌ Redis-Integration
- ❌ TLS-Zertifikate (echte)
- ❌ Cloud-Deployment (Azure)
- ❌ Zone-Sharding (Code kann vorbereitet sein, aber nicht aktiv)
- ❌ Load Balancing
- ❌ Rate Limiting
- ❌ Grafana Cloud Integration (vorbereitet, aber nicht aktiv)

### Assets
- ❌ Finale Grafiken
- ❌ Animationen (außer Basis-Bewegung)
- ❌ Sound / Musik
- ❌ UI-Design (funktional reicht)

---

## 🎨 Placeholder-Assets (Prototyp)

Für den Prototyp verwenden wir einfache Placeholder:

| Asset | Beschreibung | Quelle |
|-------|--------------|--------|
| **Spieler-Sprite** | Einfaches farbiges Rechteck oder Basic-Sprite | Selbst erstellt oder kostenlos |
| **Tileset** | Einfaches Gras-Tile, evtl. Wasser | Kostenlose Assets |
| **Font** | Godot Default oder einfache Pixel-Font | Godot Built-in |

**Mögliche Asset-Quellen:**
- [OpenGameArt.org](https://opengameart.org/)
- [Kenney.nl](https://kenney.nl/assets) ⭐ Empfohlen (CC0)
- [itch.io Free Assets](https://itch.io/game-assets/free)
- [Ninja Adventure Pack](https://pixel-boy.itch.io/ninja-adventure-asset-pack) - Komplett-Paket

---

## 📐 Technische Spezifikationen

### Netzwerk

| Parameter | Wert |
|-----------|------|
| **Protokoll** | TCP |
| **Port** | 7777 |
| **Serialisierung** | MessagePack |
| **Tick-Rate** | 25 Hz |
| **Max Spieler (Test)** | 2-10 |

### Message Frame Format

```
┌──────────┬──────────┬─────────────────────────────┐
│  1 Byte  │  4 Bytes │       N Bytes               │
│   Type   │  Length  │       Payload               │
│          │ (uint32) │   (MessagePack Data)        │
└──────────┴──────────┴─────────────────────────────┘
```

### Client-Server Flow

```
┌────────────────────────────────────────────────────────┐
│                    PROTOTYP FLOW                        │
│                                                         │
│  1. Client startet                                     │
│     └─► Zeigt "Username eingeben" Dialog               │
│                                                         │
│  2. Client verbindet sich                              │
│     └─► TCP Connect zu localhost:7777                  │
│     └─► Sendet LoginRequest { Username }               │
│                                                         │
│  3. Server akzeptiert                                  │
│     └─► Erstellt PlayerId                              │
│     └─► Sendet LoginResponse { Success, PlayerId }     │
│     └─► Broadcastet PlayerJoined an andere             │
│                                                         │
│  4. Gameplay Loop                                      │
│     └─► Client: Input → PositionUpdate an Server       │
│     └─► Server: Validiert, updated State               │
│     └─► Server: WorldState an alle Clients (25 Hz)     │
│     └─► Client: Rendert alle Spieler                   │
│                                                         │
│  5. Chat                                               │
│     └─► Client: ChatMessage an Server                  │
│     └─► Server: Broadcastet an alle                    │
│                                                         │
│  6. Disconnect                                         │
│     └─► Server: Entfernt Spieler                       │
│     └─► Server: Broadcastet PlayerLeft                 │
└────────────────────────────────────────────────────────┘
```

---

## 🧪 Akzeptanzkriterien

Der Prototyp gilt als **abgeschlossen**, wenn:

### Muss (Must Have)
1. ✅ Server startet ohne Fehler
2. ✅ Client verbindet sich erfolgreich
3. ✅ Spieler sieht seine eigene Figur
4. ✅ Zweiter Spieler verbindet sich
5. ✅ Beide Spieler sehen sich gegenseitig
6. ✅ Bewegung eines Spielers ist für den anderen sichtbar
7. ✅ Chat-Nachricht kommt beim anderen an

### Sollte (Should Have)
8. ⬜ Bewegung fühlt sich flüssig an (kein starkes Ruckeln)
9. ⬜ Disconnect wird sauber gehandhabt
10. ⬜ Mehrere Clients (3+) funktionieren gleichzeitig

### Kann (Nice to Have)
11. ⬜ Einfache Tilemap statt leerer Fläche
12. ⬜ Spielername über dem Charakter
13. ⬜ Ping/Latenz-Anzeige

---

## 📅 Geschätzte Tasks

| Task | Geschätzte Zeit | Abhängigkeiten |
|------|-----------------|----------------|
| Shared Library (Messages, DTOs) | 2-3h | - |
| TCP Server Grundgerüst | 3-4h | Shared |
| TCP Client Grundgerüst | 3-4h | Shared |
| Login-Flow | 2h | Server, Client |
| Position-Sync | 4-5h | Login-Flow |
| Godot-Szene (Basis) | 2-3h | - |
| Spieler-Rendering | 2-3h | Position-Sync, Szene |
| Chat | 2h | Login-Flow |
| Testing & Bugfixing | 3-4h | Alles |
| **Gesamt** | **~25-30h** | - |

---

## 🚦 Definition of Done

Ein Feature ist **fertig**, wenn:

- [ ] Code ist geschrieben und kompiliert
- [ ] Keine offensichtlichen Bugs
- [ ] Funktioniert mit 2 Clients gleichzeitig
- [ ] Code ist im `main` Branch

---

## 🔗 Nützliche Links

### Dokumentation
- [.NET Docs](https://learn.microsoft.com/en-us/dotnet/)
- [Godot 4 Docs](https://docs.godotengine.org/en/stable/)
- [MessagePack-CSharp](https://github.com/MessagePack-CSharp/MessagePack-CSharp)

### Netzwerk
- [Gabriel Gambetta - Fast-Paced Multiplayer](https://www.gabrielgambetta.com/client-server-game-architecture.html)
- [Valve Source Multiplayer Networking](https://developer.valvesoftware.com/wiki/Source_Multiplayer_Networking)

### Testing
- [xUnit](https://xunit.net/)
- [GdUnit4](https://mikeschulze.github.io/gdUnit4/)

### Assets
- [Kenney.nl](https://kenney.nl/assets)
- [OpenGameArt](https://opengameart.org/)
- [itch.io Free](https://itch.io/game-assets/free)

---

## 📝 Notizen

- **Fokus auf Funktionalität, nicht Perfektion**
- Placeholder-Assets sind OK
- Code-Qualität ist wichtig, aber Over-Engineering vermeiden
- Dokumentation kann minimal sein (Code-Kommentare reichen)

---

*Dieses Dokument definiert den Scope. Alles außerhalb ist Phase 2+.*