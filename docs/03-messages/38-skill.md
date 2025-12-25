# 🎯 Skills / Talents / Abilities Messages (3800-3899)

**Kategorie:** 38  
**Range:** 3800-3899  
**Phase:** Phase 2  
**Status:** 🟡 Phase 2

[← Zurück zur Übersicht](README.md)

---

## 📋 Inhaltsverzeichnis

- [SkillListRequest (3800)](#skilllistrequest-3800)
- [SkillListResponse (3801)](#skilllistresponse-3801)
- [SkillLearn (3802)](#skilllearn-3802)
- [SkillUpgrade (3805)](#skillupgrade-3805)
- [TalentListRequest (3810)](#talentlistrequest-3810)
- [TalentLearn (3812)](#talentlearn-3812)
- [TalentReset (3814)](#talentreset-3814)
- [SpecializationChange (3821)](#specializationchange-3821)
- [AbilityBarUpdate (3830)](#abilitybarupdate-3830)
- [GlyphApply (3850)](#glyphapply-3850)

---

## 📋 Übersicht

Diese Kategorie umfasst alle Messages für **Skill-, Talent- und Ability-Systeme** im 2DMMO.

Das Skill-System implementiert:
- Skill-Learning (aus Trainer, Levelup, Quests)
- Skill-Upgrades (Rank 1 → Rank 2)
- Talent-Trees (Specializationen)
- Specialization-Swapping
- Ability-Bar Management
- Glyph-System (Ability-Modifiers)

**Server Authority**: Alle Skill/Talent-Changes sind server-authoritative.

---

## SkillListRequest (3800)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Fordert Liste aller gelernten Skills an.

### Request Payload
Keine zusätzlichen Felder

### Erwartete Response
- **Immer:** `SkillListResponse` (3801)

---

## SkillListResponse (3801)

**Richtung:** 📥 Server → Client  
**Frequenz:** Selten  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung
Liste aller gelernten Skills mit Ranks.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Skills | List<SkillInfo> | Gelernte Skills | Ja |

**SkillInfo**:
| Feld | Typ | Beschreibung |
|------|-----|--------------|
| SkillId | uint | Skill-ID |
| Rank | int | Current Rank |
| MaxRank | int | Max Rank |

---

## SkillLearn (3802)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Lernt neuen Skill.

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| SkillId | uint | Zu lernender Skill | Ja |
| TrainerId | int | Trainer-NPC-ID | Nein |

### Erwartete Response
- **Bei Erfolg:** `SkillLearnResult` (3803)
- **Bei Fehler:** `SkillLearnResult` (3803) mit ErrorCode

### Error Codes
| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `LEVEL_TOO_LOW` | Level zu niedrig | Leveln |
| `INSUFFICIENT_GOLD` | Nicht genug Gold | Gold farmen |
| `PREREQUISITE_NOT_MET` | Vorskill nicht gelernt | Vorskill lernen |

---

## SkillUpgrade (3805)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Upgraded Skill zu höherem Rank.

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| SkillId | uint | Skill zum upgraden | Ja |

### Notizen
- **Cost**: Gold-Cost steigt mit Rank
- **Auto**: Bei Levelup oft auto-upgrade verfügbar

---

## TalentListRequest (3810)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Fordert Talent-Tree an.

### Request Payload
Keine zusätzlichen Felder

### Erwartete Response
- **Immer:** `TalentListResponse` (3811)

---

## TalentLearn (3812)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Lernt Talent-Point.

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| TalentId | uint | Talent-ID | Ja |

### Error Codes
| Code | Bedeutung | Aktion |
|------|-----------|--------|
| `NO_TALENT_POINTS` | Keine Talent-Points | Leveln |
| `PREREQUISITE_NOT_MET` | Vortalent fehlt | Vortalent lernen |

---

## TalentReset (3814)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Resettet alle Talents (gegen Gold).

### Request Payload
Keine zusätzlichen Felder

### Erwartete Response
- **Bei Erfolg:** Alle Talent-Points werden returned
- **Bei Fehler:** `TalentResetResult` (3815)

### Notizen
- **Cost**: Steigt mit jedem Reset
- **Cap**: Max Reset-Cost

---

## SpecializationChange (3821)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Wechselt Specialization (z.B. Tank → Healer).

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| SpecId | uint | Spec-ID | Ja |

### Notizen
- **Dual-Spec**: Meist 2-3 Specs parallel möglich
- **Cost**: Erste Spec-Änderung kostenlos, danach Gold

---

## AbilityBarUpdate (3830)

**Richtung:** 📡 Broadcast  
**Frequenz:** Häufig  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Ability-Bar hat sich geändert.

### Broadcast Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Slots | List<AbilitySlot> | Alle Slots | Ja |

**AbilitySlot**:
| Feld | Typ | Beschreibung |
|------|-----|--------------|
| SlotIndex | byte | Slot (1-12) |
| SkillId | uint | Skill-ID (0=empty) |

---

## GlyphApply (3850)

**Richtung:** 📤 Client → Server  
**Frequenz:** Selten  
**Authentifizierung:** 🔒 Ja  
**Spezielle Rechte:** Keine

### Beschreibung
Applied Glyph auf Skill (modifiziert Skill-Behavior).

### Request Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| GlyphId | uint | Glyph-ID | Ja |
| SkillId | uint | Target-Skill | Ja |

### Notizen
- **Use-Case**: Skill-Modifiers (mehr Damage, weniger Cooldown, etc.)
- **Limit**: Max 3 Glyphs pro Skill

---

**Letzte Aktualisierung**: 2025-12-25  
**Version**: 1.0.0

[← Zurück zur Übersicht](README.md)
