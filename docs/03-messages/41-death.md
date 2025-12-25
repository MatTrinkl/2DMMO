# 💀 Death / Respawn / Ghost Messages (4100-4199)

**Kategorie:** 41  
**Range:** 4100-4199  
**Phase:** Phase 2  
**Status:** 🟡 Phase 2

[← Zurück zur Übersicht](README.md)

---

## 📋 Inhaltsverzeichnis

- [DeathNotification (4100)](#deathnotification-4100)
- [DeathRecap (4101)](#deathrecap-4101)
- [GhostModeStart (4110)](#ghostmodestart-4110)
- [RespawnRequest (4120)](#respawnrequest-4120)
- [RespawnComplete (4124)](#respawncomplete-4124)
- [ResurrectOffer (4130)](#resurrectoffer-4130)
- [ResurrectAccept (4131)](#resurrectaccept-4131)
- [ReleaseSpirit (4140)](#releasespirit-4140)

---

## 📋 Übersicht

Diese Kategorie umfasst alle Messages für **Death-, Respawn- und Resurrection-Systeme** im 2DMMO.

Das Death-System implementiert:
- Death-Notification und Recap
- Ghost-Mode (Geist-Form)
- Respawn-Mechanics (Graveyard, Checkpoint)
- Resurrection durch andere Spieler
- Spirit-Healer Mechanics
- Corpse-Retrieval

**Server Authority**: Alle Death/Respawn-Mechanics sind server-authoritative.

---

## DeathNotification (4100)

**Richtung:** 📡 Broadcast  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Spieler ist gestorben.

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| VictimId | int | Gestorbener Entity-ID | Ja |
| KillerId | int | Killer Entity-ID (0=environment) | Ja |
| KillerName | string | Killer-Name | Nein |
| DeathType | string | "pvp", "pve", "fall", "drown" | Ja |

### Notizen
- **UI**: Client zeigt Death-Screen
- **Durability**: Equipment-Durability-Loss
- **Corpse**: Corpse wird am Death-Location platziert

---

## DeathRecap (4101)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Detaillierter Death-Recap (letzte 10 Sekunden Damage).

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| DamageEvents | List<DamageRecapEvent> | Damage-Events | Ja |
| TotalDamage | int | Total Damage | Ja |

---

## GhostModeStart (4110)

**Richtung:** 📡 Broadcast  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Spieler ist im Ghost-Mode (Geist-Form).

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| PlayerId | int | Spieler-ID | Ja |

### Notizen
- **Visual**: Spieler ist transparent
- **No Combat**: Kann nicht angreifen/angegriffen werden
- **Speed**: 50% schneller

---

## RespawnRequest (4120)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Spieler möchte respawnen.

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| RespawnType | string | "graveyard", "checkpoint", "corpse" | Ja |

---

## RespawnComplete (4124)

**Richtung:** 📡 Broadcast  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Respawn abgeschlossen.

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| PlayerId | int | Spieler-ID | Ja |
| X | float | Respawn-Position X | Ja |
| Y | float | Respawn-Position Y | Ja |

---

## ResurrectOffer (4130)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Resurrection-Angebot von anderem Spieler.

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| ResurrectorId | int | Resurrector-ID | Ja |
| ResurrectorName | string | Name | Ja |
| Timeout | int | Sekunden zum Accept | Ja |

---

## ResurrectAccept (4131)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Akzeptiert Resurrection.

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| ResurrectorId | int | Resurrector-ID | Ja |

---

## ReleaseSpirit (4140)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Gibt Geist frei (startet Ghost-Mode).

### Request Payload
Keine zusätzlichen Felder

---

**Letzte Aktualisierung**: 2025-12-25  
**Version**: 1.0.0

[← Zurück zur Übersicht](README.md)
