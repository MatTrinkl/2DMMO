# 🔔 Notifications / Alerts Messages (4300-4399)

**Kategorie:** 43  
**Range:** 4300-4399  



[← Zurück zur Übersicht](README.md)

---

## 📋 Inhaltsverzeichnis

- [NotificationShow (4300)](#notificationshow-4300)
- [NotificationDismiss (4301)](#notificationdismiss-4301)
- [AlertPopup (4310)](#alertpopup-4310)
- [ToastMessage (4320)](#toastmessage-4320)
- [ToastAchievement (4321)](#toastachievement-4321)
- [ToastLevelUp (4322)](#toastlevelup-4322)
- [BossWarning (4330)](#bosswarning-4330)
- [BossAbility (4331)](#bossability-4331)
- [CountdownStart (4340)](#countdownstart-4340)
- [ScreenEffect (4350)](#screeneffect-4350)
- [ScreenShake (4351)](#screenshake-4351)

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

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| NotificationId | string | UUID | Ja |
| Title | string | Titel | Ja |
| Message | string | Notification-Text | Ja |
| Type | string | "info", "warning", "error", "success" | Ja |
| Priority | byte | 0=Low, 1=Normal, 2=High | Ja |
| Persistent | bool | Im Notification-Center speichern? | Ja |

### Notizen
- **UI**: Client zeigt Icon-Badge mit Count
- **Click**: Öffnet Notification-Center
- **Auto-Dismiss**: Non-persistent nach 10s

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
| NotificationId | string | Zu dismissed Notification | Ja |

---

## AlertPopup (4310)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Zeigt modalen Alert-Dialog.

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| AlertId | string | UUID | Ja |
| Title | string | Dialog-Titel | Ja |
| Message | string | Alert-Text | Ja |
| Buttons | List<string> | Button-Labels (z.B. ["OK", "Cancel"]) | Ja |
| DefaultButton | int | Default-Button-Index | Ja |

### Notizen
- **Modal**: Blockiert andere UI-Interaktionen
- **Use-Case**: Wichtige Warnings, Confirmations

---

## ToastMessage (4320)

**Richtung:** 📥 Server → Client  
**Frequenz:** Sehr häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Kurze Toast-Message (Auto-Dismiss).

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Message | string | Toast-Text | Ja |
| Duration | int | Anzeigedauer (ms) | Ja |
| Type | string | "info", "success", "warning", "error" | Ja |

### Notizen
- **UI**: Kleines Popup bottom-right
- **Auto-Dismiss**: Nach Duration
- **Stack**: Multiple Toasts stacken

---

## ToastAchievement (4321)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Achievement-Unlock Toast (special styling).

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| AchievementId | uint | Achievement-ID | Ja |
| Title | string | Achievement-Name | Ja |
| IconUrl | string | Icon-URL | Ja |
| Points | int | Achievement-Points | Ja |

### Notizen
- **UI**: Special Achievement-Toast mit Sound
- **Animation**: Slide-in mit Celebration-Effect

---

## ToastLevelUp (4322)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Level-Up Toast.

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| NewLevel | int | Neues Level | Ja |
| NewAbilities | List<uint> | Freigeschaltete Abilities | Nein |

### Notizen
- **Sound**: Level-Up Sound
- **Effect**: Screen-Flash

---

## BossWarning (4330)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig (in Boss-Fights)  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Boss-Mechanic Warning.

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| BossId | uint | Boss-ID | Ja |
| WarningType | string | "ability", "phase", "enrage" | Ja |
| Message | string | Warning-Text | Ja |
| Countdown | int | Sekunden bis Event | Nein |

### Notizen
- **UI**: Großer Text center-screen
- **Sound**: Warning-Sound
- **DBM/BigWigs**: Ähnlich zu WoW-Addons

---

## BossAbility (4331)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Boss castet spezielle Ability.

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| BossId | uint | Boss-ID | Ja |
| AbilityId | uint | Ability-ID | Ja |
| AbilityName | string | Ability-Name | Ja |
| TargetId | int | Target-Entity (0=all) | Nein |

---

## CountdownStart (4340)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Startet Countdown-Timer.

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| CountdownId | string | UUID | Ja |
| Duration | int | Duration (Sekunden) | Ja |
| Message | string | Countdown-Text | Ja |

### Notizen
- **Use-Case**: Dungeon-Start, Event-Start
- **UI**: Großer Countdown center-screen

---

## ScreenEffect (4350)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Generic Screen-Effect.

### Broadcast Payload
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

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Intensity | float | Shake-Stärke | Ja |
| Duration | int | Duration (ms) | Ja |

### Notizen
- **Use-Case**: Explosionen, Boss-Stomps
- **Accessibility**: Kann in Settings disabled werden

---

**Letzte Aktualisierung**: 2025-12-25  
**Version**: 1.0.0

[← Zurück zur Übersicht](README.md)
