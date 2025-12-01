# 2DMMO Issue-Hierarchie und Abhängigkeiten

Dieses Dokument zeigt die Beziehungen zwischen den Issues und die empfohlene Bearbeitungsreihenfolge.

---

## Issue-Hierarchie

### Phase 1: Grundlagen & Projektsetup

```
#1 Projektstruktur für 2DMMO anlegen
 └── Basis-Ordnerstruktur, .gitignore, README

#2 .NET-Solution mit Server- und Shared-Projekt anlegen
 ├── Abhängig von: #1
 └── Erstellt: Mmo.sln, Mmo.Server, Mmo.Shared

#3 Godot-Client-Projekt mit C# erstellen
 ├── Abhängig von: #1
 └── Erstellt: client/GodotProject, Main.tscn, Main.cs
```

### Phase 2: Basis-Netzwerk & Auth-Skelett

```
#4 Basis-Nachrichtentypen und Interfaces definieren (Shared)
 ├── Abhängig von: #2
 └── Erstellt: INetworkMessage, MessageType, DTOs

#5 Einfache JSON-Serialisierung für Netzwerk-Nachrichten
 ├── Abhängig von: #4
 └── Erstellt: MessageSerializer

#6 GameServer Entry Point und Game-Loop implementieren
 ├── Abhängig von: #2
 └── Erstellt: GameServer, Tick-Loop

#7 World- und Player-Domänenklassen erstellen
 ├── Abhängig von: #6
 └── Erstellt: World, Player

#8 NetworkServer und ClientConnection-Skelett [EPIK]
 ├── Abhängig von: #5, #6
 ├── Sub-Issue 8a: NetworkServer TCP-Listener
 ├── Sub-Issue 8b: ClientConnection-Klasse
 └── Sub-Issue 8c: Message-Lese-Loop und Events

#9 MessageRouter implementieren und Login-Flow [EPIK]
 ├── Abhängig von: #7, #8
 ├── Sub-Issue 9a: MessageRouter-Basisklasse
 └── Sub-Issue 9b: Login-Handler (In-Memory)

#10 NetworkClient Singleton (Godot C#) implementieren
 ├── Abhängig von: #3, #5
 └── Erstellt: NetworkClient als Autoload

#11 Einfache Login-UI und LoginRequest-Senden im Client [EPIK]
 ├── Abhängig von: #10
 ├── Sub-Issue 11a: LoginPanel-UI erstellen
 ├── Sub-Issue 11b: Login-Button-Handler und Netzwerk
 └── Sub-Issue 11c: LoginResponse verarbeiten

[NEU] Verbindungsaufbau-Flow mit Retry und Fehlerbehandlung
 └── Abhängig von: #10, #11

[NEU] Konsistente Verwendung der Shared DTOs
 └── Abhängig von: #4, #10

[NEU] Grundlegendes Logging im Network-Code
 └── Abhängig von: #8, #10
```

### Phase 3: Welt & Movement synchronisieren

```
#12 MoveRequest verarbeiten und Player-Position updaten [EPIK]
 ├── Abhängig von: #9, #11
 ├── Sub-Issue 12a: LocalPlayerController im Client
 └── Sub-Issue 12b: HandleMove im Server

#13 PlayerStateUpdate vom Server broadcasten
 ├── Abhängig von: #12
 └── Erstellt: Broadcast-Loop, PlayerStateUpdate

#14 Remote-Player-Instanzen im Client anzeigen [EPIK]
 ├── Abhängig von: #13
 ├── Sub-Issue 14a: GameManager für Spieler-Verwaltung
 ├── Sub-Issue 14b: PlayerNode-Szene und Sprite
 └── Sub-Issue 14c: Positions-Interpolation (optional)

#15 Movement-Prototyp stabilisieren und Tests [EPIK]
 ├── Abhängig von: #12, #13, #14
 ├── Sub-Issue 15a: Tickrate und Geschwindigkeit tunen
 ├── Sub-Issue 15b: Unit-Tests für World und Movement
 └── Sub-Issue 15c: Manuelle Integrationstests

[NEU] 2D-Tilemap für Prototyp-Welt erstellen
 └── Abhängig von: #3

[NEU] Serverseitige Startposition und Spawn-Logik
 └── Abhängig von: #7

[NEU] Mapping von Server-Positionen auf Godot-Koordinaten
 └── Abhängig von: #14

[NEU] Weltgrenzen im Server implementieren
 └── Abhängig von: #7, #12
```

### Phase 4: Minimaler MMO-Layer

#### Persistenz
```
[NEU] Datenmodell für Charakterpersistenz definieren
 └── Abhängig von: #4

[NEU] Persistenzschicht implementieren (File/JSON oder SQLite)
 └── Abhängig von: Datenmodell

[NEU] Charakterdaten beim Login laden
 └── Abhängig von: #9, Persistenzschicht

[NEU] Charakterdaten beim Logout/Disconnect speichern
 └── Abhängig von: #8, Persistenzschicht

[NEU] Fehlerbehandlung für Persistenz
 └── Abhängig von: Persistenzschicht
```

#### Chat-System
```
[NEU] Chat-Nachrichtentypen im Shared-Projekt
 └── Abhängig von: #4

[NEU] Serverseitiges Chat-Handling implementieren
 └── Abhängig von: #9, Chat-Nachrichtentypen

[NEU] Einfaches Rate-Limiting für Chat
 └── Abhängig von: Chat-Handling

[NEU] Chat-UI im Godot-Client erstellen
 └── Abhängig von: #3

[NEU] Chat-Nachrichten senden und empfangen im Client
 └── Abhängig von: #10, Chat-UI, Chat-Handling
```

#### Erste Gameplay-Aktion
```
[NEU] Action-Nachrichtentypen definieren
 └── Abhängig von: #4

[NEU] Action-Handler im Server implementieren
 └── Abhängig von: #9, Action-Nachrichtentypen

[NEU] Dummy-Mob für Attack-Ziel erstellen
 └── Abhängig von: #7, Action-Handler

[NEU] Action-Input im Client implementieren
 └── Abhängig von: #10, Action-Nachrichtentypen

[NEU] Action-Visualisierung im Client
 └── Abhängig von: #14, Action-Handler
```

#### Abschluss
```
[NEU] Code-Cleanup und Naming-Konventionen
 └── Abhängig von: Alle vorherigen Issues

[NEU] README und Dokumentation aktualisieren
 └── Abhängig von: Alle vorherigen Issues

[NEU] Manueller Testplan für Phase 4
 └── Abhängig von: Alle Phase-4-Features
```

---

## Empfohlene Bearbeitungsreihenfolge (Meilensteine)

### Meilenstein 1: Projektbasis (Phase 1)
1. #1 Projektstruktur
2. #2 .NET-Solution
3. #3 Godot-Client

### Meilenstein 2: Shared Code & Serialisierung
4. #4 Basis-Nachrichtentypen
5. #5 JSON-Serialisierung

### Meilenstein 3: Server-Grundlagen
6. #6 GameServer Entry Point
7. #7 World- und Player-Klassen
8. Sub-Issue 8a: TCP-Listener
9. Sub-Issue 8b: ClientConnection
10. Sub-Issue 8c: Message-Loop

### Meilenstein 4: Login-Flow (Server)
11. Sub-Issue 9a: MessageRouter
12. Sub-Issue 9b: Login-Handler

### Meilenstein 5: Client-Netzwerk
13. #10 NetworkClient Singleton
14. Sub-Issue 11a: LoginPanel-UI
15. Sub-Issue 11b: Login-Integration
16. Sub-Issue 11c: Response-Handling

### Meilenstein 6: Movement
17. Sub-Issue 12a: LocalPlayerController
18. Sub-Issue 12b: HandleMove
19. #13 PlayerStateUpdate Broadcast
20. Sub-Issue 14a: GameManager
21. Sub-Issue 14b: PlayerNode-Szene
22. [NEU] Weltgrenzen

### Meilenstein 7: Stabilisierung
23. [NEU] 2D-Tilemap
24. [NEU] Spawn-Logik
25. [NEU] Koordinaten-Mapping
26. Sub-Issue 14c: Interpolation
27. Sub-Issue 15a: Tuning
28. Sub-Issue 15b: Unit-Tests

### Meilenstein 8: Persistenz
29. [NEU] Datenmodell
30. [NEU] Persistenzschicht
31. [NEU] Login laden
32. [NEU] Logout speichern
33. [NEU] Fehlerbehandlung Persistenz

### Meilenstein 9: Chat
34. [NEU] Chat-Nachrichtentypen
35. [NEU] Chat-UI
36. [NEU] Chat-Handling Server
37. [NEU] Chat Client-Integration
38. [NEU] Rate-Limiting

### Meilenstein 10: Gameplay-Aktion
39. [NEU] Action-Nachrichtentypen
40. [NEU] Action-Handler
41. [NEU] Dummy-Mob
42. [NEU] Action-Input
43. [NEU] Action-Visualisierung

### Meilenstein 11: Abschluss Phase 4
44. [NEU] Code-Cleanup
45. [NEU] Dokumentation
46. [NEU] Manueller Testplan
47. Sub-Issue 15c: Manuelle Tests

---

## Parallele Bearbeitung

Folgende Issues können parallel bearbeitet werden:

### Parallel-Gruppe A (nach Meilenstein 1)
- #2 .NET-Solution
- #3 Godot-Client

### Parallel-Gruppe B (nach Meilenstein 2)
- #6 GameServer (Server-Track)
- #10 NetworkClient (Client-Track)

### Parallel-Gruppe C (nach Meilenstein 5)
- Sub-Issue 12a: LocalPlayerController (Client)
- Sub-Issue 12b: HandleMove (Server)

### Parallel-Gruppe D (nach Meilenstein 7)
- Persistenz-Issues (Server)
- Chat-UI (Client)
- 2D-Tilemap (Client)

---

## Labels-Übersicht

### Typ-Labels
- `type:feature` - Neue Funktionalität
- `type:chore` - Aufräumen, Refactoring
- `type:test` - Tests
- `type:documentation` - Dokumentation
- `type:bug` - Fehlerbehebung

### Bereichs-Labels
- `area:server` - Server-seitig
- `area:client` - Client-seitig (Godot)
- `area:network` - Netzwerk-Code
- `area:persistenz` - Datenpersistenz
- `area:chat` - Chat-System
- `area:gameplay` - Spielmechaniken

### Prioritäts-Labels
- `priority:p1` - Kritisch, muss erledigt werden
- `priority:p2` - Wichtig, aber nicht blockierend
- `priority:p3` - Nice-to-have

### Status-Labels
- `status:epic` - Sammlung von Sub-Issues
- `status:blocked` - Wartet auf anderes Issue
- `status:ready` - Bereit zur Bearbeitung
