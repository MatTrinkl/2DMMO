#!/bin/bash
#
# 2DMMO Issue Creator Script
# ===========================
# Dieses Script erstellt alle Issues aus docs/ISSUES_ROADMAP.md
# inklusive Sub-Issues und Verlinkungen.
#
# Voraussetzungen:
# - GitHub CLI (gh) installiert und authentifiziert
# - Ausführung im Root-Verzeichnis des Repositories
#
# Verwendung:
#   chmod +x scripts/create-issues.sh
#   ./scripts/create-issues.sh
#
# Das Script erstellt zuerst alle Labels, dann die Issues in der
# richtigen Reihenfolge, um Verlinkungen zu ermöglichen.

set -e

# Farben für Output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Repository Info
REPO="MatTrinkl/2DMMO"

# Tracking für erstellte Issues
declare -A ISSUE_MAP

# ============================================
# Helper Functions
# ============================================

log_info() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

log_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

log_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

log_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Erstellt ein Label falls es nicht existiert
create_label() {
    local name="$1"
    local color="$2"
    local description="$3"
    
    if gh label view "$name" --repo "$REPO" &>/dev/null; then
        log_warning "Label '$name' existiert bereits"
    else
        gh label create "$name" --color "$color" --description "$description" --repo "$REPO"
        log_success "Label '$name' erstellt"
    fi
}

# Erstellt ein Issue und speichert die Nummer
create_issue() {
    local key="$1"
    local title="$2"
    local body="$3"
    local labels="$4"
    
    log_info "Erstelle Issue: $title"
    
    local issue_number
    issue_number=$(gh issue create \
        --repo "$REPO" \
        --title "$title" \
        --body "$body" \
        --label "$labels" 2>&1 | grep -oP 'issues/\K\d+')
    
    if [[ -n "$issue_number" ]]; then
        ISSUE_MAP["$key"]="$issue_number"
        log_success "Issue #$issue_number erstellt: $title"
    else
        log_error "Fehler beim Erstellen von: $title"
    fi
}

# Aktualisiert ein bestehendes Issue
update_issue() {
    local issue_number="$1"
    local body="$2"
    
    gh issue edit "$issue_number" --repo "$REPO" --body "$body"
    log_success "Issue #$issue_number aktualisiert"
}

# ============================================
# Label Erstellung
# ============================================

create_all_labels() {
    log_info "=== Erstelle Labels ==="
    
    # Typ-Labels
    create_label "type:feature" "1D76DB" "Neue Funktionalität"
    create_label "type:chore" "FEF2C0" "Aufräumen, Refactoring"
    create_label "type:test" "BFD4F2" "Tests"
    create_label "type:documentation" "0075CA" "Dokumentation"
    create_label "type:bug" "D73A4A" "Fehlerbehebung"
    create_label "type:epic" "5319E7" "Epik-Issue mit Sub-Issues"
    
    # Bereichs-Labels
    create_label "area:server" "C2E0C6" "Server-seitig"
    create_label "area:client" "E99695" "Client-seitig (Godot)"
    create_label "area:network" "F9D0C4" "Netzwerk-Code"
    create_label "area:persistenz" "D4C5F9" "Datenpersistenz"
    create_label "area:chat" "FBCA04" "Chat-System"
    create_label "area:gameplay" "B60205" "Spielmechaniken"
    
    # Prioritäts-Labels
    create_label "priority:p1" "B60205" "Kritisch, muss erledigt werden"
    create_label "priority:p2" "FBCA04" "Wichtig, aber nicht blockierend"
    create_label "priority:p3" "0E8A16" "Nice-to-have"
    
    # Status-Labels
    create_label "status:epic" "5319E7" "Sammlung von Sub-Issues"
    create_label "status:blocked" "D93F0B" "Wartet auf anderes Issue"
    create_label "status:ready" "0E8A16" "Bereit zur Bearbeitung"
    
    log_success "=== Labels erstellt ==="
}

# ============================================
# Sub-Issues für bestehende Epik-Issues
# ============================================

create_subissues_for_issue_8() {
    log_info "=== Sub-Issues für #8 NetworkServer ==="
    
    # Sub-Issue 8a
    create_issue "8a" "Sub-Issue 8a: NetworkServer TCP-Listener implementieren" \
"**Parent-Issue:** #8

Den TCP-Listener des NetworkServers implementieren, der auf eingehende Verbindungen wartet und diese an die ClientConnection-Verwaltung weitergibt.

**Aufgaben:**
- [ ] \`NetworkServer\`-Klasse anlegen mit Port-Parameter im Konstruktor
- [ ] \`RunAsync(CancellationToken)\` Methode implementieren, die den Listener startet
- [ ] \`TcpListener\` auf dem konfigurierten Port starten
- [ ] Accept-Loop implementieren, der auf neue Verbindungen wartet
- [ ] Logging bei Start, Stop und Fehlern

**Akzeptanzkriterien:**
- [ ] Server startet und lauscht auf dem konfigurierten Port
- [ ] Server kann per CancellationToken sauber beendet werden
- [ ] Fehler beim Starten (z.B. Port belegt) werden geloggt

**Ressourcen:**
- [TcpListener-Klasse](https://learn.microsoft.com/en-us/dotnet/api/system.net.sockets.tcplistener)
- [Async/Await Pattern in .NET](https://learn.microsoft.com/en-us/dotnet/csharp/asynchronous-programming/)" \
"type:feature,area:server,area:network,priority:p1"

    # Sub-Issue 8b
    create_issue "8b" "Sub-Issue 8b: ClientConnection-Klasse implementieren" \
"**Parent-Issue:** #8

Die ClientConnection-Klasse implementieren, die eine einzelne Client-Verbindung repräsentiert und Nachrichten senden/empfangen kann.

**Aufgaben:**
- [ ] \`ClientConnection\`-Klasse mit eindeutiger \`Id\` (Guid) anlegen
- [ ] Konstruktor mit \`TcpClient\` Parameter
- [ ] Property für den Verbindungsstatus
- [ ] \`Task SendAsync(INetworkMessage msg)\` implementieren
- [ ] \`Task DisconnectAsync()\` implementieren
- [ ] Logging bei Verbindungsaktionen

**Akzeptanzkriterien:**
- [ ] Jede ClientConnection hat eine eindeutige ID
- [ ] Nachrichten können asynchron gesendet werden
- [ ] Disconnect räumt Ressourcen sauber auf

**Ressourcen:**
- [TcpClient-Klasse](https://learn.microsoft.com/en-us/dotnet/api/system.net.sockets.tcpclient)
- [NetworkStream-Klasse](https://learn.microsoft.com/en-us/dotnet/api/system.net.sockets.networkstream)" \
"type:feature,area:server,area:network,priority:p1"

    # Sub-Issue 8c
    create_issue "8c" "Sub-Issue 8c: Message-Lese-Loop und Events implementieren" \
"**Parent-Issue:** #8

Den internen Lese-Loop für eingehende Nachrichten implementieren und Events für Verbindungs-/Nachrichtenereignisse bereitstellen.

**Aufgaben:**
- [ ] Interner Lese-Loop in ClientConnection, der kontinuierlich Nachrichten liest
- [ ] Event \`ClientConnected\` im NetworkServer
- [ ] Event \`ClientDisconnected\` im NetworkServer
- [ ] Event \`MessageReceived\` im NetworkServer
- [ ] Fehlerbehandlung bei Verbindungsabbruch

**Akzeptanzkriterien:**
- [ ] Events werden korrekt ausgelöst bei Connect/Disconnect/Message
- [ ] Verbindungsabbruch führt zu sauberem Disconnect-Event
- [ ] Keine Exceptions bei normalem Verbindungsende

**Ressourcen:**
- [Ereignisse in C#](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/events/)
- [Exception Handling Best Practices](https://learn.microsoft.com/en-us/dotnet/standard/exceptions/best-practices-for-exceptions)" \
"type:feature,area:server,area:network,priority:p1"
}

create_subissues_for_issue_9() {
    log_info "=== Sub-Issues für #9 MessageRouter ==="
    
    # Sub-Issue 9a
    create_issue "9a" "Sub-Issue 9a: MessageRouter-Basisklasse implementieren" \
"**Parent-Issue:** #9

Die MessageRouter-Klasse implementieren, die eingehende Nachrichten anhand des MessageType an die passenden Handler weiterleitet.

**Aufgaben:**
- [ ] \`MessageRouter\`-Klasse anlegen
- [ ] \`Route(ClientConnection, INetworkMessage)\` Methode implementieren
- [ ] Switch/Dictionary-basiertes Routing nach MessageType
- [ ] Logging für unbekannte Nachrichtentypen
- [ ] Stub-Handler für zukünftige Message-Typen anlegen

**Akzeptanzkriterien:**
- [ ] Nachrichten werden korrekt an Handler geroutet
- [ ] Unbekannte Nachrichtentypen werden geloggt, crashen aber nicht
- [ ] Router ist erweiterbar für neue Nachrichtentypen

**Ressourcen:**
- [Pattern Matching in C#](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/functional/pattern-matching)
- [Dependency Injection in .NET](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection)" \
"type:feature,area:server,priority:p1"

    # Sub-Issue 9b
    create_issue "9b" "Sub-Issue 9b: Login-Handler (In-Memory) implementieren" \
"**Parent-Issue:** #9

Den Login-Handler implementieren, der LoginRequests verarbeitet und Spieler in der World erstellt.

**Aufgaben:**
- [ ] \`HandleLogin(ClientConnection, LoginRequest)\` Methode implementieren
- [ ] Spielernamen validieren (nicht leer, passende Länge)
- [ ] Player über \`World.CreatePlayer()\` erstellen
- [ ] \`LoginResponse\` mit \`Success = true\` und \`PlayerId\` senden
- [ ] Bei Fehler: \`LoginResponse\` mit \`Success = false\` und Fehlermeldung

**Akzeptanzkriterien:**
- [ ] Gültiger LoginRequest erzeugt einen Player in der World
- [ ] Client erhält korrekte LoginResponse
- [ ] Ungültige Anfragen werden mit Fehlermeldung beantwortet

**Ressourcen:**
- [Validierung in .NET](https://learn.microsoft.com/en-us/dotnet/api/system.componentmodel.dataannotations)
- [System.Text.Json](https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/overview)" \
"type:feature,area:server,priority:p1"
}

create_subissues_for_issue_11() {
    log_info "=== Sub-Issues für #11 Login-UI ==="
    
    # Sub-Issue 11a
    create_issue "11a" "Sub-Issue 11a: LoginPanel-UI in Godot erstellen" \
"**Parent-Issue:** #11

Die Login-UI mit Eingabefeld und Button in Godot erstellen.

**Aufgaben:**
- [ ] \`LoginPanel\`-Szene erstellen (Control Node)
- [ ] LineEdit für Spielernamen-Eingabe
- [ ] Button zum Einloggen
- [ ] Label für Fehlermeldungen (initial versteckt)
- [ ] Einfaches, sauberes Layout

**Akzeptanzkriterien:**
- [ ] UI ist sichtbar und bedienbar
- [ ] Textfeld akzeptiert Eingaben
- [ ] Button ist klickbar

**Ressourcen:**
- [Control Nodes in Godot](https://docs.godotengine.org/en/stable/tutorials/ui/control_node_gallery.html)
- [GUI-Design in Godot](https://docs.godotengine.org/en/stable/tutorials/ui/index.html)" \
"type:feature,area:client,priority:p1"

    # Sub-Issue 11b
    create_issue "11b" "Sub-Issue 11b: Login-Button-Handler und Netzwerk-Integration" \
"**Parent-Issue:** #11

Den Button-Handler implementieren, der die Verbindung herstellt und LoginRequest sendet.

**Aufgaben:**
- [ ] Signal-Verbindung für Button-Click
- [ ] Bei Klick: Verbindung zu \`localhost:7777\` über NetworkClient
- [ ] \`LoginRequest\` mit eingegebenem Namen senden
- [ ] UI während des Vorgangs deaktivieren (Button, Textfeld)

**Akzeptanzkriterien:**
- [ ] Klick auf Button initiiert Verbindung
- [ ] LoginRequest wird gesendet
- [ ] UI reagiert auf den Vorgang

**Ressourcen:**
- [Signals in Godot C#](https://docs.godotengine.org/en/stable/tutorials/scripting/c_sharp/c_sharp_signals.html)
- [Godot Networking Basics](https://docs.godotengine.org/en/stable/tutorials/networking/index.html)" \
"type:feature,area:client,area:network,priority:p1"

    # Sub-Issue 11c
    create_issue "11c" "Sub-Issue 11c: LoginResponse verarbeiten und Fehlerbehandlung" \
"**Parent-Issue:** #11

Die LoginResponse vom Server verarbeiten und entsprechend in der UI reagieren.

**Aufgaben:**
- [ ] Handler für LoginResponse im NetworkClient registrieren
- [ ] Bei Erfolg: Login-UI ausblenden, zur Spielszene wechseln
- [ ] Bei Fehler: Fehlermeldung im Label anzeigen
- [ ] UI nach Fehler wieder aktivieren

**Akzeptanzkriterien:**
- [ ] Erfolgreicher Login führt zum Szenenwechsel
- [ ] Fehler werden in der UI angezeigt
- [ ] Nach Fehler kann erneut versucht werden

**Ressourcen:**
- [SceneTree in Godot](https://docs.godotengine.org/en/stable/classes/class_scenetree.html)
- [Godot C# Basics](https://docs.godotengine.org/en/stable/tutorials/scripting/c_sharp/c_sharp_basics.html)" \
"type:feature,area:client,priority:p1"
}

create_subissues_for_issue_12() {
    log_info "=== Sub-Issues für #12 MoveRequest ==="
    
    # Sub-Issue 12a
    create_issue "12a" "Sub-Issue 12a: LocalPlayerController im Client implementieren" \
"**Parent-Issue:** #12

Den LocalPlayerController implementieren, der Tastatureingaben liest und in MoveRequests umwandelt.

**Aufgaben:**
- [ ] \`LocalPlayerController\`-Script erstellen
- [ ] Input-Handling für WASD und Pfeiltasten
- [ ] Richtungsvektor aus Eingaben berechnen
- [ ] Normalisierung des Richtungsvektors
- [ ] \`MoveRequest { DirX, DirY }\` an NetworkClient senden

**Akzeptanzkriterien:**
- [ ] Tastatureingaben werden erkannt
- [ ] MoveRequests werden kontinuierlich gesendet bei Bewegung
- [ ] Keine Bewegung → kein MoveRequest oder MoveRequest mit (0,0)

**Ressourcen:**
- [Input Handling in Godot](https://docs.godotengine.org/en/stable/tutorials/inputs/input_examples.html)
- [InputMap in Godot](https://docs.godotengine.org/en/stable/tutorials/inputs/inputevent.html)" \
"type:feature,area:client,priority:p1"

    # Sub-Issue 12b
    create_issue "12b" "Sub-Issue 12b: HandleMove im Server implementieren" \
"**Parent-Issue:** #12

Die serverseitige Verarbeitung von MoveRequests implementieren.

**Aufgaben:**
- [ ] \`HandleMove(ClientConnection, MoveRequest)\` im MessageRouter
- [ ] Zugehörigen Player über Connection finden
- [ ] Richtung validieren (Länge ≤ 1)
- [ ] Position mit Geschwindigkeit × deltaTime aktualisieren
- [ ] Weltgrenzen berücksichtigen

**Akzeptanzkriterien:**
- [ ] Serverposition ändert sich konsistent mit Eingaben
- [ ] Keine Teleportation durch ungültige Richtungswerte
- [ ] Spieler bleibt innerhalb der Weltgrenzen

**Ressourcen:**
- [Vector-Mathematik in C#](https://learn.microsoft.com/en-us/dotnet/api/system.numerics.vector2)
- [Game Loop Patterns](https://learn.microsoft.com/en-us/archive/msdn-magazine/2015/march/c-game-programming-building-a-basic-game-engine)" \
"type:feature,area:server,priority:p1"
}

create_subissues_for_issue_14() {
    log_info "=== Sub-Issues für #14 Remote-Player ==="
    
    # Sub-Issue 14a
    create_issue "14a" "Sub-Issue 14a: GameManager für Spieler-Verwaltung erstellen" \
"**Parent-Issue:** #14

Den GameManager implementieren, der Remote-Spieler-Instanzen verwaltet.

**Aufgaben:**
- [ ] \`GameManager\`-Script als Autoload oder Szenen-Root
- [ ] Dictionary \`PlayerId → PlayerNode\` für Spielerverwaltung
- [ ] Methode \`UpdatePlayers(PlayerStateUpdate)\`
- [ ] Neue Spieler instanziieren, wenn sie erstmalig auftauchen
- [ ] Spieler entfernen, die nicht mehr in Updates vorkommen

**Akzeptanzkriterien:**
- [ ] Spieler werden bei erstem Auftauchen erstellt
- [ ] Spieler werden entfernt, wenn sie verschwinden
- [ ] Eigener Spieler wird nicht als Remote erstellt

**Ressourcen:**
- [Instanziieren von Szenen](https://docs.godotengine.org/en/stable/tutorials/scripting/instancing.html)
- [Node-Verwaltung in Godot](https://docs.godotengine.org/en/stable/classes/class_node.html)" \
"type:feature,area:client,priority:p1"

    # Sub-Issue 14b
    create_issue "14b" "Sub-Issue 14b: PlayerNode-Szene und Sprite erstellen" \
"**Parent-Issue:** #14

Die PlayerNode-Szene mit Platzhalter-Sprite für Remote-Spieler erstellen.

**Aufgaben:**
- [ ] \`PlayerNode.tscn\` Szene erstellen
- [ ] Platzhalter-Sprite (z.B. farbiges Rechteck oder Icon)
- [ ] Label für Spielernamen über dem Sprite
- [ ] Script mit Property für PlayerId und Name
- [ ] Methode \`UpdatePosition(float x, float y)\`

**Akzeptanzkriterien:**
- [ ] PlayerNode ist sichtbar in der Szene
- [ ] Name wird korrekt angezeigt
- [ ] Position kann aktualisiert werden

**Ressourcen:**
- [Sprite2D in Godot](https://docs.godotengine.org/en/stable/classes/class_sprite2d.html)
- [Label in Godot](https://docs.godotengine.org/en/stable/classes/class_label.html)" \
"type:feature,area:client,priority:p1"

    # Sub-Issue 14c
    create_issue "14c" "Sub-Issue 14c: Positions-Interpolation für Remote-Spieler" \
"**Parent-Issue:** #14

Optionale Interpolation implementieren, um Bewegungen von Remote-Spielern zu glätten.

**Aufgaben:**
- [ ] Zielposition und aktuelle Position speichern
- [ ] Lineare Interpolation in \`_Process()\`
- [ ] Interpolationsgeschwindigkeit konfigurierbar
- [ ] Fallback bei großen Positionssprüngen (Teleport)

**Akzeptanzkriterien:**
- [ ] Bewegungen wirken flüssiger
- [ ] Keine Verzögerung bei Teleports
- [ ] Performance bleibt akzeptabel

**Ressourcen:**
- [Lerp-Funktion in Godot](https://docs.godotengine.org/en/stable/classes/class_@globalscope.html#class-globalscope-method-lerp)
- [Smooth Movement Tutorial](https://docs.godotengine.org/en/stable/tutorials/physics/using_kinematic_body_2d.html)" \
"type:feature,area:client,priority:p2"
}

create_subissues_for_issue_15() {
    log_info "=== Sub-Issues für #15 Movement-Tests ==="
    
    # Sub-Issue 15a
    create_issue "15a" "Sub-Issue 15a: Tickrate und Geschwindigkeit tunen" \
"**Parent-Issue:** #15

Tickrate und Bewegungsgeschwindigkeit so anpassen, dass sich der Prototyp flüssig anfühlt.

**Aufgaben:**
- [ ] Server-Tickrate evaluieren (20-60 Hz)
- [ ] Client-Update-Rate für Broadcasts evaluieren (10-20/s)
- [ ] Bewegungsgeschwindigkeit anpassen
- [ ] Inputrate vom Client evaluieren
- [ ] Werte als Konstanten/Config auslagern

**Akzeptanzkriterien:**
- [ ] Bewegung fühlt sich responsiv an
- [ ] Keine sichtbare Verzögerung bei Eingaben
- [ ] CPU-Last bleibt niedrig

**Ressourcen:**
- [Game Loop Timing](https://learn.microsoft.com/en-us/archive/msdn-magazine/2014/november/windows-with-c-high-performance-game-timing)
- [Godot _Process vs _PhysicsProcess](https://docs.godotengine.org/en/stable/tutorials/scripting/idle_and_physics_processing.html)" \
"type:chore,area:server,area:client,priority:p1"

    # Sub-Issue 15b
    create_issue "15b" "Sub-Issue 15b: Unit-Tests für World und Movement" \
"**Parent-Issue:** #15

Unit-Tests für die Kernlogik von World und Movement implementieren.

**Aufgaben:**
- [ ] Testprojekt \`Mmo.Server.Tests\` erstellen
- [ ] Tests für \`World.CreatePlayer\`
- [ ] Tests für \`World.RemovePlayerByConnection\`
- [ ] Tests für Movement-Berechnung
- [ ] Edge-Cases: 0-Werte, Grenzen, ungültige Eingaben

**Akzeptanzkriterien:**
- [ ] Alle Tests laufen durch
- [ ] Kritische Pfade sind getestet
- [ ] Tests sind in CI integrierbar

**Ressourcen:**
- [xUnit Testing](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-with-dotnet-test)
- [Best Practices für Unit-Tests](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices)" \
"type:test,area:server,priority:p1"

    # Sub-Issue 15c
    create_issue "15c" "Sub-Issue 15c: Manuelle Integrationstests dokumentieren" \
"**Parent-Issue:** #15

Manuelle Testfälle dokumentieren und durchführen.

**Aufgaben:**
- [ ] Testplan-Dokument erstellen
- [ ] Testfall: Ein-/Ausloggen mit mehreren Clients
- [ ] Testfall: Dauerhaftes Movement über mehrere Minuten
- [ ] Testfall: Server-Stop während aktiver Verbindungen
- [ ] Ergebnisse dokumentieren

**Akzeptanzkriterien:**
- [ ] Prototyp läuft 5+ Minuten stabil mit 2-3 Clients
- [ ] Keine Crashes oder Memory-Leaks beobachtet
- [ ] Testplan ist dokumentiert

**Ressourcen:**
- [Manuelles Testen](https://learn.microsoft.com/en-us/azure/devops/test/create-test-cases)
- [Exploratory Testing](https://learn.microsoft.com/en-us/azure/devops/test/exploratory-testing-with-cuit)" \
"type:chore,area:server,area:client,priority:p2"
}

# ============================================
# Neue Epik-Issues Phase 2-4
# ============================================

create_phase2_issues() {
    log_info "=== Phase 2: Feinschliff ==="
    
    # Epik: Verbindungsaufbau-Flow
    create_issue "epic_connection" "Epik: Verbindungsaufbau-Flow mit Retry und Fehlerbehandlung" \
"Den Verbindungsaufbau im \`NetworkClient\` robuster gestalten mit automatischem Retry-Mechanismus, konfigurierbarem Timeout und Benutzer-Feedback bei Fehlern.

> **📌 Dies ist ein Epik-Issue.** Die Arbeit wurde in folgende Sub-Issues aufgeteilt.

**Sub-Issues werden nach Erstellung dieses Issues verlinkt.**

**Übersicht:**
- [ ] ConnectionConfig und ConnectionState implementieren
- [ ] Retry-Logik im NetworkClient
- [ ] UI-Feedback für Verbindungsstatus" \
"type:epic,area:client,area:network,priority:p1"

    # Sub-Issues für Verbindungsaufbau
    create_issue "connection_config" "ConnectionConfig und ConnectionState implementieren" \
"**Parent-Issue:** Epik: Verbindungsaufbau-Flow

Konfigurationsklasse und State-Enum für den Verbindungsaufbau erstellen.

**Aufgaben:**
- [ ] \`ConnectionConfig\`-Klasse erstellen mit TimeoutMs, MaxRetries, RetryDelayMs
- [ ] \`ConnectionState\`-Enum definieren (Disconnected, Connecting, Connected, Reconnecting, Failed)
- [ ] Signal \`ConnectionStateChanged\` im NetworkClient definieren

**Akzeptanzkriterien:**
- [ ] ConnectionConfig-Klasse existiert mit allen Properties
- [ ] ConnectionState-Enum ist im Shared-Projekt
- [ ] Signal ist definiert und dokumentiert" \
"type:feature,area:client,area:network,priority:p1"

    create_issue "connection_retry" "Retry-Logik im NetworkClient implementieren" \
"**Parent-Issue:** Epik: Verbindungsaufbau-Flow

Die Retry-Logik im NetworkClient implementieren mit Timeout und exponential Backoff.

**Aufgaben:**
- [ ] \`NetworkClient.ConnectAsync()\` erweitern mit Retry-Loop
- [ ] CancellationToken korrekt disposen
- [ ] State-Änderungen über Signal kommunizieren
- [ ] Exponential Backoff implementieren

**Akzeptanzkriterien:**
- [ ] Bei nicht erreichbarem Server: 3 Versuche mit je 5s Timeout
- [ ] State-Signal wird bei jeder Änderung gefeuert
- [ ] Keine Memory-Leaks (CancellationTokenSource disposed)" \
"type:feature,area:client,area:network,priority:p1"

    create_issue "connection_ui" "UI-Feedback für Verbindungsstatus" \
"**Parent-Issue:** Epik: Verbindungsaufbau-Flow

Das LoginPanel mit visuellem Feedback für den Verbindungsstatus erweitern.

**Aufgaben:**
- [ ] Im LoginPanel auf ConnectionStateChanged reagieren
- [ ] Loading-Spinner zur LoginPanel-Szene hinzufügen
- [ ] StatusLabel für Fehlermeldungen hinzufügen
- [ ] Button deaktivieren während Verbindungsaufbau

**Akzeptanzkriterien:**
- [ ] UI zeigt aktuellen Status (Connecting, Retry, Failed)
- [ ] Spinner dreht während Verbindungsaufbau
- [ ] Nach Fehler kann erneut geklickt werden" \
"type:feature,area:client,priority:p1"

    # Epik: Shared DTOs
    create_issue "epic_shared_dto" "Epik: Konsistente Verwendung der Shared DTOs im Client" \
"Sicherstellen, dass der Godot-Client exakt dieselben DTO-Klassen aus \`Mmo.Shared\` verwendet wie der Server.

> **📌 Dies ist ein Epik-Issue.**

**Sub-Issues:**
- [ ] Projektverweise konfigurieren
- [ ] Duplizierte Klassen entfernen
- [ ] Shared-Projekt dokumentieren" \
"type:epic,area:server,area:client,priority:p1"

    # Epik: Logging
    create_issue "epic_logging" "Epik: Grundlegendes Logging im Network-Code" \
"Strukturiertes Logging für alle Netzwerk-Operationen einführen. Server: Microsoft.Extensions.Logging, Client: Wrapper um GD.Print.

> **📌 Dies ist ein Epik-Issue.**

**Sub-Issues:**
- [ ] Server-Logging einrichten
- [ ] Client-Logger-Klasse erstellen
- [ ] Log-Statements hinzufügen" \
"type:epic,area:server,area:client,priority:p2"
}

create_phase3_issues() {
    log_info "=== Phase 3: Welt & Movement ==="
    
    # Epik: Tilemap
    create_issue "epic_tilemap" "Epik: 2D-Tilemap für Prototyp-Welt erstellen" \
"Eine einfache 2D-Tilemap in Godot erstellen als visuelle Grundlage für die Spielwelt.

> **📌 Dies ist ein Epik-Issue.**

**Sub-Issues:**
- [ ] TileSet erstellen
- [ ] TileMap-Node einrichten
- [ ] Test-Map gestalten" \
"type:epic,area:client,priority:p1"

    # Epik: Spawn-Logik
    create_issue "epic_spawn" "Epik: Serverseitige Startposition und Spawn-Logik" \
"Die Startposition für neue Spieler serverseitig definieren und Spawn-Logik implementieren.

> **📌 Dies ist ein Epik-Issue.**

**Sub-Issues:**
- [ ] SpawnConfig-Klasse erstellen
- [ ] Spawn-Position-Berechnung implementieren
- [ ] LoginResponse erweitern" \
"type:epic,area:server,priority:p1"

    # Epik: Koordinaten-Mapping
    create_issue "epic_coordinates" "Epik: Mapping von Server-Positionen auf Godot-Koordinaten" \
"Die Server-Koordinaten auf Godot-Weltkoordinaten mappen.

> **📌 Dies ist ein Epik-Issue.**

**Sub-Issues:**
- [ ] CoordinateMapper-Singleton erstellen
- [ ] Koordinaten-Umrechnung integrieren" \
"type:epic,area:client,priority:p1"

    # Epik: Weltgrenzen
    create_issue "epic_worldbounds" "Epik: Weltgrenzen im Server implementieren" \
"Weltgrenzen auf dem Server implementieren, damit Spieler nicht außerhalb laufen können.

> **📌 Dies ist ein Epik-Issue.**

**Sub-Issues:**
- [ ] WorldBounds-Konfiguration erstellen
- [ ] Position-Clamping implementieren" \
"type:epic,area:server,priority:p1"
}

create_phase4_persistenz_issues() {
    log_info "=== Phase 4: Persistenz ==="
    
    create_issue "epic_datamodel" "Epik: Datenmodell für Charakterpersistenz definieren" \
"Das Datenmodell für die Speicherung von Charakterdaten definieren.

> **📌 Dies ist ein Epik-Issue.**

**Sub-Issues:**
- [ ] CharacterData-Record erstellen
- [ ] Factory-Methoden und Hilfsmethoden" \
"type:epic,area:persistenz,priority:p1"

    create_issue "epic_persistence" "Epik: Persistenzschicht implementieren (File/JSON)" \
"Eine einfache Persistenzschicht mit JSON-Dateien implementieren.

> **📌 Dies ist ein Epik-Issue.**

**Sub-Issues:**
- [ ] ICharacterRepository-Interface erstellen
- [ ] JsonCharacterRepository implementieren
- [ ] Repository in DI registrieren" \
"type:epic,area:persistenz,priority:p1"

    create_issue "epic_login_load" "Epik: Charakterdaten beim Login laden" \
"Beim Login vorhandene Charakterdaten laden oder neuen Charakter erstellen.

> **📌 Dies ist ein Epik-Issue.**

**Sub-Issues:**
- [ ] Login-Handler erweitern
- [ ] Spawn-Position aus gespeicherten Daten" \
"type:epic,area:persistenz,area:server,priority:p1"

    create_issue "epic_logout_save" "Epik: Charakterdaten beim Logout/Disconnect speichern" \
"Bei Disconnect oder Server-Shutdown die Spielerdaten speichern.

> **📌 Dies ist ein Epik-Issue.**

**Sub-Issues:**
- [ ] Disconnect-Handler erweitern
- [ ] Server-Shutdown-Handler" \
"type:epic,area:persistenz,area:server,priority:p1"

    create_issue "epic_persistence_error" "Epik: Fehlerbehandlung für Persistenz" \
"Robuste Fehlerbehandlung für Persistenz-Operationen.

> **📌 Dies ist ein Epik-Issue.**

**Sub-Issues:**
- [ ] Retry-Logik implementieren
- [ ] Korrupte Daten behandeln" \
"type:epic,area:persistenz,priority:p2"
}

create_phase4_chat_issues() {
    log_info "=== Phase 4: Chat ==="
    
    create_issue "epic_chat_messages" "Epik: Chat-Nachrichtentypen im Shared-Projekt" \
"Die Nachrichtentypen für das Chat-System definieren.

> **📌 Dies ist ein Epik-Issue.**

**Sub-Issues:**
- [ ] ChatChannel-Enum erstellen
- [ ] Chat-DTOs erstellen
- [ ] MessageType erweitern" \
"type:epic,area:chat,priority:p1"

    create_issue "epic_chat_server" "Epik: Serverseitiges Chat-Handling" \
"Chat-Handling auf dem Server implementieren.

> **📌 Dies ist ein Epik-Issue.**

**Sub-Issues:**
- [ ] ChatHandler-Klasse erstellen
- [ ] Validierung und Sanitization
- [ ] Broadcast-Mechanismus" \
"type:epic,area:chat,area:server,priority:p1"

    create_issue "epic_chat_ratelimit" "Epik: Rate-Limiting für Chat" \
"Spam-Schutz für Chat implementieren.

> **📌 Dies ist ein Epik-Issue.**

**Sub-Issues:**
- [ ] ChatRateLimiter-Klasse
- [ ] Integration in ChatHandler" \
"type:epic,area:chat,area:server,priority:p2"

    create_issue "epic_chat_ui" "Epik: Chat-UI im Godot-Client" \
"Chat-UI im Client erstellen.

> **📌 Dies ist ein Epik-Issue.**

**Sub-Issues:**
- [ ] ChatPanel-Szene erstellen
- [ ] ChatPanel-Script implementieren" \
"type:epic,area:chat,area:client,priority:p1"

    create_issue "epic_chat_client" "Epik: Chat-Integration im Client" \
"Chat mit NetworkClient verbinden.

> **📌 Dies ist ein Epik-Issue.**

**Sub-Issues:**
- [ ] ChatManager-Singleton erstellen
- [ ] Nachrichten senden und empfangen" \
"type:epic,area:chat,area:client,priority:p1"
}

create_phase4_gameplay_issues() {
    log_info "=== Phase 4: Gameplay ==="
    
    create_issue "epic_action_messages" "Epik: Action-Nachrichtentypen definieren" \
"Die Nachrichtentypen für Gameplay-Aktionen definieren.

> **📌 Dies ist ein Epik-Issue.**

**Sub-Issues:**
- [ ] ActionType und EmoteType Enums
- [ ] ActionRequest und ActionEvent DTOs" \
"type:epic,area:gameplay,priority:p1"

    create_issue "epic_action_handler" "Epik: Action-Handler im Server" \
"Server-seitige Verarbeitung von Aktionen.

> **📌 Dies ist ein Epik-Issue.**

**Sub-Issues:**
- [ ] ActionHandler-Klasse erstellen
- [ ] Attack-Handling implementieren
- [ ] Emote-Handling implementieren" \
"type:epic,area:gameplay,area:server,priority:p1"

    create_issue "epic_dummy_mob" "Epik: Dummy-Mob für Attack-Ziel" \
"Dummy-Mob als Angriffsziel erstellen.

> **📌 Dies ist ein Epik-Issue.**

**Sub-Issues:**
- [ ] Entity-Basisklasse erstellen
- [ ] Mob-Klasse erstellen" \
"type:epic,area:gameplay,area:server,priority:p2"

    create_issue "epic_action_input" "Epik: Action-Input im Client" \
"Eingabebehandlung für Aktionen.

> **📌 Dies ist ein Epik-Issue.**

**Sub-Issues:**
- [ ] InputMap konfigurieren
- [ ] ActionController erstellen" \
"type:epic,area:gameplay,area:client,priority:p1"

    create_issue "epic_action_visual" "Epik: Action-Visualisierung" \
"Visuelle Darstellung von Aktionen.

> **📌 Dies ist ein Epik-Issue.**

**Sub-Issues:**
- [ ] Attack-Effekt erstellen
- [ ] Emote-Popup erstellen" \
"type:epic,area:gameplay,area:client,priority:p1"
}

create_phase4_cleanup_issues() {
    log_info "=== Phase 4: Abschluss ==="
    
    create_issue "epic_cleanup" "Epik: Code-Cleanup und Naming-Konventionen" \
"Code aufräumen und Konventionen vereinheitlichen.

> **📌 Dies ist ein Epik-Issue.**

**Sub-Issues:**
- [ ] Naming-Konventionen prüfen
- [ ] Code-Formatierung durchführen
- [ ] Unused Code und TODOs bereinigen" \
"type:epic,type:chore,priority:p2"

    create_issue "epic_docs" "Epik: README und Dokumentation aktualisieren" \
"Dokumentation aktualisieren.

> **📌 Dies ist ein Epik-Issue.**

**Sub-Issues:**
- [ ] README aktualisieren
- [ ] Setup-Anleitung schreiben" \
"type:epic,type:documentation,priority:p1"

    create_issue "epic_testplan" "Epik: Manueller Testplan für Phase 4" \
"Testplan für Phase 4 erstellen und durchführen.

> **📌 Dies ist ein Epik-Issue.**

**Sub-Issues:**
- [ ] Testplan erstellen
- [ ] Tests durchführen" \
"type:epic,type:test,priority:p1"
}

# ============================================
# Parent-Issues aktualisieren
# ============================================

update_parent_issues() {
    log_info "=== Aktualisiere Parent-Issues mit Sub-Issue-Links ==="
    
    # Issue #8 aktualisieren
    if [[ -n "${ISSUE_MAP[8a]}" ]] && [[ -n "${ISSUE_MAP[8b]}" ]] && [[ -n "${ISSUE_MAP[8c]}" ]]; then
        local body_8="Einen einfachen Netzwerkserver (TCP oder WebSocket) und eine \`ClientConnection\`-Abstraktion implementieren, der Verbindungen verwalten kann.

> **📌 Dies ist ein Epik-Issue.** Die Arbeit wurde in folgende Sub-Issues aufgeteilt:

**Sub-Issues:**
- [ ] #${ISSUE_MAP[8a]} NetworkServer TCP-Listener implementieren
- [ ] #${ISSUE_MAP[8b]} ClientConnection-Klasse implementieren
- [ ] #${ISSUE_MAP[8c]} Message-Lese-Loop und Events implementieren

**Dieses Issue kann geschlossen werden, wenn alle Sub-Issues erledigt sind.**

---

<details>
<summary>Ursprüngliche Aufgaben (zur Referenz)</summary>

**Aufgaben:**
- [ ] \`NetworkServer\`-Klasse
- [ ] \`ClientConnection\`-Klasse
- [ ] \`GameServer\` registriert sich auf diese Events

</details>

**Akzeptanzkriterien:**
- [ ] Ein einfacher Test-Client kann eine Verbindung herstellen und wieder trennen.
- [ ] Server loggt Connect- und Disconnect-Ereignisse ohne Absturz.

## Ressourcen
- [TCP Sockets in .NET](https://learn.microsoft.com/en-us/dotnet/fundamentals/networking/sockets/socket-services)
- [TcpListener und TcpClient](https://learn.microsoft.com/en-us/dotnet/fundamentals/networking/sockets/tcp-classes)"
        
        update_issue 8 "$body_8"
    fi

    # Issue #9 aktualisieren
    if [[ -n "${ISSUE_MAP[9a]}" ]] && [[ -n "${ISSUE_MAP[9b]}" ]]; then
        local body_9="Einen Message-Router einführen, der eingehende Nachrichten anhand des Typs an Handler verteilt, sowie einen simplen Login-Fluss in Memory implementieren.

> **📌 Dies ist ein Epik-Issue.** Die Arbeit wurde in folgende Sub-Issues aufgeteilt:

**Sub-Issues:**
- [ ] #${ISSUE_MAP[9a]} MessageRouter-Basisklasse implementieren
- [ ] #${ISSUE_MAP[9b]} Login-Handler (In-Memory) implementieren

**Dieses Issue kann geschlossen werden, wenn alle Sub-Issues erledigt sind.**

---

<details>
<summary>Ursprüngliche Aufgaben (zur Referenz)</summary>

**Aufgaben:**
- [ ] \`MessageRouter\`-Klasse mit \`Route(ClientConnection, INetworkMessage)\`
- [ ] \`HandleLogin(ClientConnection, LoginRequest)\` implementieren
- [ ] Spätere Handler als Stub anlegen

</details>

**Akzeptanzkriterien:**
- [ ] Ein \`LoginRequest\` erzeugt einen Player in der \`World\`.
- [ ] Der Client erhält eine korrekte \`LoginResponse\`.

## Ressourcen
- [C# Delegates & Events](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/delegates/)
- [DTOs in System.Text.Json](https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/overview)"
        
        update_issue 9 "$body_9"
    fi

    # Issue #11 aktualisieren
    if [[ -n "${ISSUE_MAP[11a]}" ]] && [[ -n "${ISSUE_MAP[11b]}" ]] && [[ -n "${ISSUE_MAP[11c]}" ]]; then
        local body_11="Eine simple Login-Oberfläche implementieren, über die der Spieler seinen Namen eingibt und ein \`LoginRequest\` an den Server gesendet wird.

> **📌 Dies ist ein Epik-Issue.** Die Arbeit wurde in folgende Sub-Issues aufgeteilt:

**Sub-Issues:**
- [ ] #${ISSUE_MAP[11a]} LoginPanel-UI in Godot erstellen
- [ ] #${ISSUE_MAP[11b]} Login-Button-Handler und Netzwerk-Integration
- [ ] #${ISSUE_MAP[11c]} LoginResponse verarbeiten und Fehlerbehandlung

**Dieses Issue kann geschlossen werden, wenn alle Sub-Issues erledigt sind.**

---

<details>
<summary>Ursprüngliche Aufgaben (zur Referenz)</summary>

**Aufgaben:**
- [ ] \`LoginPanel\`-UI mit Textfeld und Button erstellen
- [ ] Button-Handler
- [ ] \`LoginResponse\` im Client auswerten

</details>

**Akzeptanzkriterien:**
- [ ] Ein Spieler kann sich per UI mit Namen einloggen.
- [ ] Der Server erhält und verarbeitet \`LoginRequest\`.
- [ ] Erfolg/Misserfolg wird in der UI sichtbar.

## Ressourcen
- [UI Controls in Godot](https://docs.godotengine.org/en/4.4/tutorials/ui/control_node_gallery.html)
- [Godot C#/.NET für UI-Events](https://docs.godotengine.org/en/stable/tutorials/scripting/c_sharp/index.html)"
        
        update_issue 11 "$body_11"
    fi

    # Issue #12 aktualisieren
    if [[ -n "${ISSUE_MAP[12a]}" ]] && [[ -n "${ISSUE_MAP[12b]}" ]]; then
        local body_12="Die serverseitige Verarbeitung von Bewegungsbefehlen implementieren, sodass der Server Positionsänderungen verwaltet.

> **📌 Dies ist ein Epik-Issue.** Die Arbeit wurde in folgende Sub-Issues aufgeteilt:

**Sub-Issues:**
- [ ] #${ISSUE_MAP[12a]} LocalPlayerController im Client implementieren
- [ ] #${ISSUE_MAP[12b]} HandleMove im Server implementieren

**Dieses Issue kann geschlossen werden, wenn alle Sub-Issues erledigt sind.**

---

<details>
<summary>Ursprüngliche Aufgaben (zur Referenz)</summary>

**Aufgaben:**
- [ ] Client: \`LocalPlayerController\` implementieren
- [ ] Server: \`HandleMove(ClientConnection, MoveRequest)\` implementieren

</details>

**Akzeptanzkriterien:**
- [ ] Serverposition des Players ändert sich konsistent mit den Eingaben.
- [ ] Keine Exceptions bei schnellen oder häufigen Eingaben.

## Ressourcen
- [Input-Beispiele Godot 4](https://docs.godotengine.org/en/4.4/tutorials/inputs/input_examples.html)
- [Delegates/Eventhandler in C#](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/delegates/)"
        
        update_issue 12 "$body_12"
    fi

    # Issue #14 aktualisieren
    if [[ -n "${ISSUE_MAP[14a]}" ]] && [[ -n "${ISSUE_MAP[14b]}" ]] && [[ -n "${ISSUE_MAP[14c]}" ]]; then
        local body_14="Die vom Server gesendeten \`PlayerStateUpdate\`s im Client nutzen, um andere Spieler als Sprites in der Welt darzustellen.

> **📌 Dies ist ein Epik-Issue.** Die Arbeit wurde in folgende Sub-Issues aufgeteilt:

**Sub-Issues:**
- [ ] #${ISSUE_MAP[14a]} GameManager für Spieler-Verwaltung erstellen
- [ ] #${ISSUE_MAP[14b]} PlayerNode-Szene und Sprite erstellen
- [ ] #${ISSUE_MAP[14c]} Positions-Interpolation für Remote-Spieler (optional)

**Dieses Issue kann geschlossen werden, wenn alle Sub-Issues erledigt sind.**

---

<details>
<summary>Ursprüngliche Aufgaben (zur Referenz)</summary>

**Aufgaben:**
- [ ] \`GameManager\`-Script im Client
- [ ] Einfache Platzhalter-Sprites für Spieler
- [ ] Optional: Interpolation

</details>

**Akzeptanzkriterien:**
- [ ] Zwei Clients sehen jeweils die Avatare der anderen Spieler.
- [ ] Positionsänderungen sind sichtbar und nachvollziehbar.

## Ressourcen
- [Godot C#/.NET Scripting](https://docs.godotengine.org/en/stable/tutorials/scripting/c_sharp/index.html)
- [Godot Input und Szene-Updates](https://docs.godotengine.org/en/stable/tutorials/inputs/input_examples.html)"
        
        update_issue 14 "$body_14"
    fi

    # Issue #15 aktualisieren
    if [[ -n "${ISSUE_MAP[15a]}" ]] && [[ -n "${ISSUE_MAP[15b]}" ]] && [[ -n "${ISSUE_MAP[15c]}" ]]; then
        local body_15="Movement-Prototyp verfeinern, Basis-Tests hinzufügen und das Zusammenspiel von Server und Client kurz harttesten.

> **📌 Dies ist ein Epik-Issue.** Die Arbeit wurde in folgende Sub-Issues aufgeteilt:

**Sub-Issues:**
- [ ] #${ISSUE_MAP[15a]} Tickrate und Geschwindigkeit tunen
- [ ] #${ISSUE_MAP[15b]} Unit-Tests für World und Movement
- [ ] #${ISSUE_MAP[15c]} Manuelle Integrationstests dokumentieren

**Dieses Issue kann geschlossen werden, wenn alle Sub-Issues erledigt sind.**

---

<details>
<summary>Ursprüngliche Aufgaben (zur Referenz)</summary>

**Aufgaben:**
- [ ] Geschwindigkeit und Tickraten anpassen
- [ ] Einfache Unit-Tests
- [ ] Manuelle Tests mit mehreren Clients

</details>

**Akzeptanzkriterien:**
- [ ] Der Prototyp läuft mehrere Minuten stabil mit 2–3 Clients.
- [ ] Movement wirkt ausreichend responsiv für einen ersten Online-Prototyp.

## Ressourcen
- [Unit Testing mit xUnit](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-csharp-with-xunit)
- [Best Practices für Unit-Tests](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices)"
        
        update_issue 15 "$body_15"
    fi
}

# ============================================
# Main
# ============================================

main() {
    echo ""
    echo "╔══════════════════════════════════════════════════════════════╗"
    echo "║           2DMMO Issue Creator Script                        ║"
    echo "║  Erstellt alle Issues aus docs/ISSUES_ROADMAP.md            ║"
    echo "╚══════════════════════════════════════════════════════════════╝"
    echo ""
    
    # Prüfe ob gh CLI verfügbar ist
    if ! command -v gh &> /dev/null; then
        log_error "GitHub CLI (gh) ist nicht installiert!"
        log_info "Installation: https://cli.github.com/"
        exit 1
    fi
    
    # Prüfe ob authentifiziert
    if ! gh auth status &> /dev/null; then
        log_error "GitHub CLI ist nicht authentifiziert!"
        log_info "Führe 'gh auth login' aus"
        exit 1
    fi
    
    log_success "GitHub CLI ist konfiguriert"
    echo ""
    
    # Bestätigung einholen
    echo -e "${YELLOW}Dieses Script wird ca. 50+ Issues erstellen.${NC}"
    echo -e "${YELLOW}Repository: $REPO${NC}"
    echo ""
    read -p "Fortfahren? (y/N) " -n 1 -r
    echo ""
    
    if [[ ! $REPLY =~ ^[Yy]$ ]]; then
        log_info "Abgebrochen."
        exit 0
    fi
    
    echo ""
    
    # Labels erstellen
    create_all_labels
    echo ""
    
    # Sub-Issues für bestehende Epik-Issues erstellen
    create_subissues_for_issue_8
    create_subissues_for_issue_9
    create_subissues_for_issue_11
    create_subissues_for_issue_12
    create_subissues_for_issue_14
    create_subissues_for_issue_15
    echo ""
    
    # Neue Epik-Issues Phase 2-4
    create_phase2_issues
    create_phase3_issues
    create_phase4_persistenz_issues
    create_phase4_chat_issues
    create_phase4_gameplay_issues
    create_phase4_cleanup_issues
    echo ""
    
    # Parent-Issues aktualisieren
    update_parent_issues
    echo ""
    
    # Zusammenfassung
    echo ""
    echo "╔══════════════════════════════════════════════════════════════╗"
    echo "║                     Zusammenfassung                         ║"
    echo "╚══════════════════════════════════════════════════════════════╝"
    echo ""
    log_success "Erstellte Issues:"
    for key in "${!ISSUE_MAP[@]}"; do
        echo "  - $key: #${ISSUE_MAP[$key]}"
    done
    echo ""
    log_success "Script abgeschlossen!"
    log_info "Überprüfe die Issues unter: https://github.com/$REPO/issues"
}

main "$@"
