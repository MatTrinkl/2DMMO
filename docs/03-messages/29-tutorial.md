# 📚 Tutorial / Guide System Messages (2900-2999)

**Kategorie:** 29  
**Range:** 2900-2999  
**Phase:** Phase 2  
**Status:** 🟡 Phase 2

[← Zurück zur Übersicht](README.md)

---

## 📋 Inhaltsverzeichnis

- [TutorialStart (2900)](#tutorialstart-2900)
- [TutorialStep (2901)](#tutorialstep-2901)
- [TutorialComplete (2902)](#tutorialcomplete-2902)
- [TutorialSkip (2903)](#tutorialskip-2903)
- [TutorialReset (2904)](#tutorialreset-2904)
- [TutorialFlag (2905)](#tutorialflag-2905)
- [HintShow (2910)](#hintshow-2910)
- [HintDismiss (2911)](#hintdismiss-2911)
- [HintDisable (2912)](#hintdisable-2912)
- [TipOfTheDay (2920)](#tipoftheday-2920)
- [NewFeatureHighlight (2921)](#newfeaturehighlight-2921)
- [GuideOpen (2930)](#guideopen-2930)
- [GuideClose (2931)](#guideclose-2931)

---

## 📋 Übersicht

Diese Kategorie umfasst alle Messages für das **Tutorial- und Guide-System** im 2DMMO.

Das Tutorial-System implementiert:
- Schritt-für-Schritt Tutorial für neue Spieler
- Kontextuelle Hints und Tipps
- Feature-Highlights bei neuen Updates
- In-Game Guide/Help-System
- Tutorial-Skip Funktion
- Persistente Tutorial-Flags

**Server Authority**: Tutorial-Progress wird server-seitig gespeichert. Client kann Skip/Disable Requests senden.

---

## TutorialStart (2900)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Startet Tutorial-Sequence. Wird automatisch bei neuem Account/Character gesendet.

### Im Scope ✅
- Tutorial-Initiierung
- Tutorial-ID und Steps
- Kann übersprungen werden

### Nicht im Scope ❌
- Quest-Tutorial → verwende `QuestAccept` (1000)
- Ability-Tutorial → separate Tutorial-ID

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| TutorialId | uint | Tutorial-ID (z.B. 1=Basic, 2=Combat, 3=Trading) | Ja |
| TotalSteps | int | Anzahl Tutorial-Steps | Ja |
| CanSkip | bool | Kann übersprungen werden? | Ja |

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `TutorialStep` | 2901 | Nächster Step |
| `TutorialComplete` | 2902 | Tutorial beendet |
| `TutorialSkip` | 2903 | Tutorial überspringen |

### Beispiel Payload
```csharp
var tutorialStart = new TutorialStart
{
    Type = MessageType.TutorialStart,
    TutorialId = 1, // Basic Tutorial
    TotalSteps = 10,
    CanSkip = true
};
```

### Notizen
- **First Login**: Automatisch bei neuem Character
- **UI**: Client zeigt Tutorial-UI/Overlay
- **Skip**: Spieler kann Tutorial überspringen

---

## TutorialStep (2901)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Nächster Tutorial-Step. Zeigt Instructions und wartet auf Player-Action.

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| TutorialId | uint | Tutorial-ID | Ja |
| StepNumber | int | Aktueller Step (1-N) | Ja |
| Title | string | Step-Titel | Ja |
| Description | string | Instruction-Text | Ja |
| TargetAction | string | Erwartete Action (z.B. "move", "attack", "open_inventory") | Ja |
| HighlightUI | string | UI-Element zum highlighten | Nein |

### Beispiel Payload
```csharp
var tutorialStep = new TutorialStep
{
    Type = MessageType.TutorialStep,
    TutorialId = 1,
    StepNumber = 3,
    Title = "Open Inventory",
    Description = "Press 'I' to open your inventory",
    TargetAction = "open_inventory",
    HighlightUI = "InventoryButton"
};
```

### Notizen
- **Wait**: Client wartet auf Player-Action
- **Highlight**: UI-Element wird highlighted
- **Auto-Progress**: Bei korrekter Action → nächster Step

---

## TutorialComplete (2902)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Tutorial erfolgreich abgeschlossen.

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| TutorialId | uint | Abgeschlossenes Tutorial | Ja |
| XpReward | int | Bonus-XP | Nein |
| GoldReward | int | Bonus-Gold | Nein |

### Notizen
- **UI**: Completion-Screen
- **Reward**: Optional Belohnung

---

## TutorialSkip (2903)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Spieler überspringt Tutorial.

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| TutorialId | uint | Zu überspringendes Tutorial | Ja |

### Notizen
- **Confirmation**: Client sollte Confirmation-Dialog zeigen
- **No Reward**: Kein Reward bei Skip

---

## TutorialReset (2904)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Resettet Tutorial (für erneutes Durchspielen).

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| TutorialId | uint | Zu resettendes Tutorial (0 = alle) | Ja |

---

## TutorialFlag (2905)

**Richtung:** 📡 Broadcast  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Setzt persistenten Tutorial-Flag.

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| FlagName | string | Flag-Name | Ja |
| Value | bool | Flag-Value | Ja |

---

## HintShow (2910)

**Richtung:** 📥 Server → Client  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Zeigt kontextuellen Hint/Tip.

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| HintId | uint | Hint-ID | Ja |
| Title | string | Hint-Titel | Ja |
| Message | string | Hint-Text | Ja |
| Duration | int | Anzeigedauer (ms) | Ja |

---

## HintDismiss (2911)

**Richtung:** 📤 Client → Server  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Spieler dismisst Hint manuell.

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| HintId | uint | Dismissed Hint | Ja |

---

## HintDisable (2912)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Deaktiviert Hint-Category permanent.

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| HintCategory | string | Category | Ja |

---

## TipOfTheDay (2920)

**Richtung:** 📥 Server → Client  
**Frequenz:** Täglich  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Zeigt "Tip of the Day" bei Login.

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| TipId | uint | Tip-ID | Ja |
| Title | string | Tip-Titel | Ja |
| Message | string | Tip-Text | Ja |

---

## NewFeatureHighlight (2921)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Highlighted neue Features nach Patch.

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| FeatureId | uint | Feature-ID | Ja |
| Title | string | Feature-Name | Ja |
| Description | string | Beschreibung | Ja |
| PatchVersion | string | Patch-Version | Ja |

---

## GuideOpen (2930)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Öffnet In-Game Guide/Help-System.

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| GuideCategory | string | Category | Nein |

---

## GuideClose (2931)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Schließt Guide-UI.

### Request Payload
Keine zusätzlichen Felder

---

**Letzte Aktualisierung**: 2025-12-25  
**Version**: 1.0.0

[← Zurück zur Übersicht](README.md)
