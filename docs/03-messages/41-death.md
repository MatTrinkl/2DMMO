# 💀 Death / Respawn / Ghost Messages (4100-4199)

**Kategorie:** 41  
**Range:** 4100-4199  
**Status:** ✅ Dokumentiert

[← Zurück zur Übersicht](README.md)

---

## 📋 Inhaltsverzeichnis

### Death (4100-4101)
- [DeathNotification (4100)](#deathnotification-4100)
- [DeathRecap (4101)](#deathrecap-4101)

### Ghost Mode (4110-4114)
- [GhostModeStart (4110)](#ghostmodestart-4110)
- [GhostModeEnd (4111)](#ghostmodeend-4111)
- [GhostPosition (4112)](#ghostposition-4112)
- [CorpseLocation (4113)](#corpselocation-4113)
- [CorpseRevive (4114)](#corpserevive-4114)

### Respawn (4120-4124)
- [RespawnRequest (4120)](#respawnrequest-4120)
- [RespawnAtGraveyard (4121)](#respawnatgraveyard-4121)
- [RespawnAtCheckpoint (4122)](#respawnatcheckpoint-4122)
- [RespawnTimer (4123)](#respawntimer-4123)
- [RespawnComplete (4124)](#respawncomplete-4124)

### Resurrection (4130-4135)
- [ResurrectOffer (4130)](#resurrectoffer-4130)
- [ResurrectAccept (4131)](#resurrectaccept-4131)
- [ResurrectDecline (4132)](#resurrectdecline-4132)
- [ResurrectComplete (4133)](#resurrectcomplete-4133)
- [SoulstoneResurrect (4134)](#soulstoneresurrect-4134)
- [BattleResurrect (4135)](#battleresurrect-4135)

### Spirit Healer (4140-4143)
- [ReleaseSpirit (4140)](#releasespirit-4140)
- [RetrieveCorpse (4141)](#retrievecorpse-4141)
- [SpiritHealerRevive (4142)](#spirithealerrevive-4142)
- [ResurrectionSickness (4143)](#resurrectionsickness-4143)

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
| DamageEvents | List<DamageRecapEventDto> | Damage-Events | Ja |
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

## GhostModeEnd (4111)

**Richtung:** 📡 Broadcast  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Spieler verlässt Ghost-Mode (nach Resurrection/Respawn).

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| PlayerId | int | Spieler-ID | Ja |

---

## GhostPosition (4112)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Position-Update für Ghost (zeigt Richtung zum Corpse/Graveyard).

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| CorpseX | float | Corpse-Position X | Ja |
| CorpseY | float | Corpse-Position Y | Ja |
| GraveyardX | float | Nächster Graveyard X | Ja |
| GraveyardY | float | Nächster Graveyard Y | Ja |

---

## CorpseLocation (4113)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Sendet die Corpse-Location an den Client.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| X | float | Corpse-Position X | Ja |
| Y | float | Corpse-Position Y | Ja |
| ZoneId | uint | Zone-ID | Ja |

---

## CorpseRevive (4114)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Revive am Corpse (wenn Ghost nahe genug ist).

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| — | — | Keine Felder | — |

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

## RespawnAtGraveyard (4121)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Respawn am nächsten Graveyard.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| X | float | Graveyard-Position X | Ja |
| Y | float | Graveyard-Position Y | Ja |
| HasSickness | bool | Resurrection Sickness aktiv? | Ja |

---

## RespawnAtCheckpoint (4122)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Respawn am letzten Checkpoint.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| X | float | Checkpoint-Position X | Ja |
| Y | float | Checkpoint-Position Y | Ja |

---

## RespawnTimer (4123)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Respawn-Timer Update.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| SecondsRemaining | int | Sekunden bis Respawn möglich | Ja |

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

### Response Payload
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

## ResurrectDecline (4132)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Lehnt Resurrection ab.

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| ResurrectorId | int | Resurrector-ID | Ja |

---

## ResurrectComplete (4133)

**Richtung:** 📡 Broadcast  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Resurrection abgeschlossen.

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| PlayerId | int | Resurrecteter Spieler | Ja |
| ResurrectorId | int | Resurrector | Ja |
| X | float | Resurrect-Position X | Ja |
| Y | float | Resurrect-Position Y | Ja |

---

## SoulstoneResurrect (4134)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Resurrection via Soulstone (selbst-resurrect).

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| — | — | Keine Felder | — |

---

## BattleResurrect (4135)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Battle-Resurrection (In-Combat Resurrection).

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| TargetId | int | Zu resurrectender Spieler | Ja |

---

## ReleaseSpirit (4140)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Gibt Geist frei (startet Ghost-Mode).

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| — | — | Keine Felder | — |

---

## RetrieveCorpse (4141)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Holt Corpse ab (wenn Ghost am Corpse ist).

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| — | — | Keine Felder | — |

---

## SpiritHealerRevive (4142)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Resurrection via Spirit Healer (mit Sickness-Debuff).

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| SpiritHealerId | int | Spirit Healer NPC-ID | Ja |

---

## ResurrectionSickness (4143)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Resurrection Sickness Debuff angewendet.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Duration | int | Debuff-Dauer in Sekunden | Ja |
| StatReduction | int | Stat-Reduktion in % | Ja |

---

**Letzte Aktualisierung**: 2026-01-02  
**Version**: 2.0.0

[← Zurück zur Übersicht](README.md)
