# 🔔 Notifications / Alerts Messages (4300-4399)

**Kategorie:** 43  
**Range:** 4300-4399  
**Status:** ✅ Dokumentiert

[← Zurück zur Übersicht](README.md)

---

## 📋 Inhaltsverzeichnis

### Notifications (4300-4302)
- [NotificationShow (4300)](#notificationshow-4300)
- [NotificationDismiss (4301)](#notificationdismiss-4301)
- [NotificationQueue (4302)](#notificationqueue-4302)

### Alerts (4310-4312)
- [AlertPopup (4310)](#alertpopup-4310)
- [AlertConfirm (4311)](#alertconfirm-4311)
- [AlertDismiss (4312)](#alertdismiss-4312)

### Toast Messages (4320-4323)
- [ToastMessage (4320)](#toastmessage-4320)
- [ToastAchievement (4321)](#toastachievement-4321)
- [ToastLevelUp (4322)](#toastlevelup-4322)
- [ToastLoot (4323)](#toastloot-4323)

### Boss Mechanics (4330-4332)
- [BossWarning (4330)](#bosswarning-4330)
- [BossAbility (4331)](#bossability-4331)
- [BossPhase (4332)](#bossphase-4332)

### Countdown (4340-4342)
- [CountdownStart (4340)](#countdownstart-4340)
- [CountdownUpdate (4341)](#countdownupdate-4341)
- [CountdownCancel (4342)](#countdowncancel-4342)

### Screen Effects (4350-4353)
- [ScreenEffect (4350)](#screeneffect-4350)
- [ScreenShake (4351)](#screenshake-4351)
- [ScreenFlash (4352)](#screenflash-4352)
- [ScreenFade (4353)](#screenfade-4353)

---

## 📋 Übersicht

Diese Kategorie umfasst alle Messages für **Notification-, Alert- und UI-Effect-Systeme** im 2DMMO.

Das Notification-System implementiert:
- Persistente Notifications (Notification-Center)
- Alert-Popups (modal Dialoge)
- Toast-Messages (kurze Einblendungen)
- Boss-Warnings und Mechanics
- Countdown-Timer
- Screen-Effects (Shake, Flash, Fade)

**Server Authority**: Server sendet Notifications/Alerts. Client zeigt UI.

---

## NotificationShow (4300)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Zeigt Notification im Notification-Center.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| NotificationId | string | UUID | Ja |
| Title | string | Titel | Ja |
| Message | string | Notification-Text | Ja |
| Type | string | "info", "warning", "error", "success" | Ja |
| Priority | byte | 0=Low, 1=Normal, 2=High | Ja |
| Persistent | bool | Im Notification-Center speichern? | Ja |

---

## NotificationDismiss (4301)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Dismissed Notification.

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| NotificationId | string | Zu dismissende Notification | Ja |

---

## NotificationQueue (4302)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Sendet Queue von pending Notifications (bei Login).

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Notifications | List<NotificationDto> | Pending Notifications | Ja |

---

## AlertPopup (4310)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Zeigt modalen Alert-Dialog.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| AlertId | string | UUID | Ja |
| Title | string | Dialog-Titel | Ja |
| Message | string | Alert-Text | Ja |
| Buttons | List<string> | Button-Labels (z.B. ["OK", "Cancel"]) | Ja |
| DefaultButton | int | Default-Button-Index | Ja |

---

## AlertConfirm (4311)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Alert-Button wurde geklickt.

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| AlertId | string | Alert-UUID | Ja |
| ButtonIndex | int | Geklickter Button-Index | Ja |

---

## AlertDismiss (4312)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Alert wurde geschlossen (ohne Button-Klick).

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| AlertId | string | Alert-UUID | Ja |

---

## ToastMessage (4320)

**Richtung:** 📥 Server → Client  
**Frequenz:** Sehr häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Kurze Toast-Message (Auto-Dismiss).

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Message | string | Toast-Text | Ja |
| Duration | int | Anzeigedauer (ms) | Ja |
| Type | string | "info", "success", "warning", "error" | Ja |

---

## ToastAchievement (4321)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Achievement-Unlock Toast (special styling).

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| AchievementId | uint | Achievement-ID | Ja |
| Title | string | Achievement-Name | Ja |
| IconUrl | string | Icon-URL | Ja |
| Points | int | Achievement-Points | Ja |

---

## ToastLevelUp (4322)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Level-Up Toast.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| NewLevel | int | Neues Level | Ja |
| NewAbilities | List<uint> | Freigeschaltete Abilities | Nein |

---

## ToastLoot (4323)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Loot-Toast (Item erhalten).

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| ItemId | uint | Item-ID | Ja |
| ItemName | string | Item-Name | Ja |
| Quantity | int | Anzahl | Ja |
| Quality | byte | Item-Quality (0=Common, 4=Epic) | Ja |

---

## BossWarning (4330)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig (in Boss-Fights)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Boss-Mechanic Warning.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| BossId | uint | Boss-ID | Ja |
| WarningType | string | "ability", "phase", "enrage" | Ja |
| Message | string | Warning-Text | Ja |
| Countdown | int | Sekunden bis Event | Nein |

---

## BossAbility (4331)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Boss castet spezielle Ability.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| BossId | uint | Boss-ID | Ja |
| AbilityId | uint | Ability-ID | Ja |
| AbilityName | string | Ability-Name | Ja |
| TargetId | int | Target-Entity (0=all) | Nein |

---

## BossPhase (4332)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Boss wechselt Phase.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| BossId | uint | Boss-ID | Ja |
| Phase | int | Neue Phase-Nummer | Ja |
| PhaseName | string | Phase-Name | Ja |

---

## CountdownStart (4340)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Startet Countdown-Timer.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| CountdownId | string | UUID | Ja |
| Duration | int | Duration (Sekunden) | Ja |
| Message | string | Countdown-Text | Ja |

---

## CountdownUpdate (4341)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Countdown-Update (Sekunden verbleibend).

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| CountdownId | string | UUID | Ja |
| SecondsRemaining | int | Verbleibende Sekunden | Ja |

---

## CountdownCancel (4342)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Countdown wurde abgebrochen.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| CountdownId | string | UUID | Ja |

---

## ScreenEffect (4350)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Generic Screen-Effect.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| EffectType | string | "shake", "flash", "fade", "blur" | Ja |
| Intensity | float | Stärke (0.0-1.0) | Ja |
| Duration | int | Duration (ms) | Ja |

---

## ScreenShake (4351)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Screen-Shake Effect.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Intensity | float | Shake-Stärke | Ja |
| Duration | int | Duration (ms) | Ja |

---

## ScreenFlash (4352)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Screen-Flash Effect (kurzer Blitz).

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Color | string | Flash-Farbe (hex) | Ja |
| Duration | int | Duration (ms) | Ja |

---

## ScreenFade (4353)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Screen-Fade Effect (Ein-/Ausblenden).

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| FadeIn | bool | true=FadeIn, false=FadeOut | Ja |
| Duration | int | Duration (ms) | Ja |
| Color | string | Fade-Farbe (hex) | Ja |

---

**Letzte Aktualisierung**: 2026-01-02  
**Version**: 2.0.0

[← Zurück zur Übersicht](README.md)
