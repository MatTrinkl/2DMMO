# Response Pattern Standardization - Implementation Guide

**Status:** In Progress  
**Erstellt:** 2025-12-25  
**Ziel:** Vereinheitlichung aller Request/Response-Patterns auf dedizierte Response-Messages

---

## 📊 Übersicht

Dieses Dokument dokumentiert die Standardisierung von Request/Response-Patterns in der Message-Dokumentation. Ziel ist es, **alle** Request-Messages mit dedizierten Response-Messages auszustatten, die ein einheitliches Success/ErrorCode/ErrorMessage-Pattern verwenden.

### Problem

**Inkonsistent** (❌):
```markdown
### Erwartete Response
- **Bei Erfolg:** `JoinZone` (100) für Character-Spawn
- **Bei Fehler:** `ErrorMessage` (910)
```

**Konsistent** (✅):
```markdown
### Erwartete Response
- `CharacterSelectResponse` (21)

### Folge-Messages bei Erfolg
- `JoinZone` (100) für Character-Spawn in Zone
- `ZoneState` (102) für vollständige Zone-Informationen
```

---

## ✅ Abgeschlossene Dateien

### 00-connection.md (IDs 21-24) ✅

| Request | Neue Response | ID | Status |
|---------|---------------|-----|--------|
| CharacterSelect | CharacterSelectResponse | 21 | ✅ |
| CharacterCreate | CharacterCreateResponse | 22 | ✅ |
| CharacterDelete | CharacterDeleteResponse | 23 | ✅ |
| ServerSelect | ServerSelectResponse | 24 | ✅ |

### 02-movement.md (IDs 220-221) ✅

| Request | Neue Response | ID | Status |
|---------|---------------|-----|--------|
| TeleportRequest | TeleportResponse | 220 | ✅ |
| JumpRequest | JumpResponse | 221 | ✅ |

### 04-chat.md (IDs 440-445) ✅

| Request | Neue Response | ID | Status |
|---------|---------------|-----|--------|
| ChatMessage | ChatMessageResponse | 440 | ✅ |
| ChatChannelJoin | ChatChannelJoinResponse | 441 | ✅ |
| ChatChannelCreate | ChatChannelCreateResponse | 442 | ✅ |
| ChatChannelDelete | ChatChannelDeleteResponse | 443 | ✅ |
| ChatChannelPassword | ChatChannelPasswordResponse | 444 | ✅ |
| ChatChannelMute | ChatChannelMuteResponse | 445 | ✅ |

### 06-character.md (IDs 650-655) ✅

| Request | Neue Response | ID | Status |
|---------|---------------|-----|--------|
| AttributeIncrease | AttributeIncreaseResponse | 650 | ✅ |
| CharacterCustomize | CharacterCustomizeResponse | 651 | ✅ |
| TalentLearn | TalentLearnResponse | 652 | ✅ |
| TalentReset | TalentResetResponse | 653 | ✅ |
| SpecializationChange | SpecializationChangeResponse | 654 | ✅ |
| TitleChange | TitleChangeResponse | 655 | ✅ |

### 07-party.md (IDs 740-747) ✅

| Request | Neue Response | ID | Status |
|---------|---------------|-----|--------|
| PartyInvite | PartyInviteResponse | 740 | ✅ |
| PartyAccept | PartyAcceptResponse | 741 | ✅ |
| PartyLeave | PartyLeaveResponse | 742 | ✅ |
| PartyKick | PartyKickResponse | 743 | ✅ |
| PartyPromote | PartyPromoteResponse | 744 | ✅ |
| PartyDisband | PartyDisbandResponse | 745 | ✅ |
| PartyLootMode | PartyLootModeResponse | 746 | ✅ |
| PartyReadyCheck | PartyReadyCheckStartResponse | 747 | ✅ |

**Fortschritt:** 26 neue Response-Messages definiert

---

## 📝 Verbleibende Dateien

### 03-combat.md (IDs 320-325)

| Request | Neue Response | ID | Vorgeschlagene Änderung |
|---------|---------------|-----|-------------------------|
| ActionRequest | ActionResponse | 320 | Ersetzt `ActionResult` (301) → Umbenennen zu ActionResponse mit Success-Pattern |
| ThreatListRequest | ThreatListResponse | 321 | Neu, ersetzt direkte `ThreatUpdate` Antwort |
| ResurrectionRequest | ResurrectionResponse | 322 | Neu, ersetzt Broadcast-only Pattern |

**Notiz:** `ActionResult` (301) sollte umbenannt werden zu `ActionResponse` und Success/ErrorCode/ErrorMessage Pattern erhalten.

### 04-chat.md (IDs 440-450)

| Request | Neue Response | ID | Vorgeschlagene Änderung |
|---------|---------------|-----|-------------------------|
| ChatMessage | ChatMessageResponse | 440 | Neu, bestätigt Acceptance bevor Broadcast |
| ChatChannelJoin | ChatChannelJoinResponse | 441 | Neu, ersetzt direkten Broadcast |
| ChatChannelCreate | ChatChannelCreateResponse | 442 | Neu, ersetzt Auto-Join Pattern |
| ChatChannelDelete | ChatChannelDeleteResponse | 443 | Neu, ersetzt Auto-Leave Pattern |
| ChatChannelPassword | ChatChannelPasswordResponse | 444 | Neu, bestätigt Passwort-Änderung |
| ChatChannelMute | ChatChannelMuteResponse | 445 | Neu, bestätigt Mute-Action |
| ChatChannelUnmute | ChatChannelUnmuteResponse | 446 | Neu, bestätigt Unmute-Action |
| ChatChannelKick | ChatChannelKickResponse | 447 | Neu, bestätigt Kick-Action |
| ChatChannelBan | ChatChannelBanResponse | 448 | Neu, bestätigt Ban-Action |
| ChatChannelOwner | ChatChannelOwnerResponse | 449 | Neu, bestätigt Owner-Transfer |
| ChatChannelModerator | ChatChannelModeratorResponse | 450 | Neu, bestätigt Moderator-Promotion |

**Notiz:** ChatWhisperResponse (403) ist bereits korrekt implementiert!

### 05-inventory.md (IDs 520-530)

| Request | Neue Response | ID | Vorgeschlagene Änderung |
|---------|---------------|-----|-------------------------|
| ItemMove | ItemMoveResponse | 520 | Neu, ersetzt `ItemMoveSuccess` + `ItemRemove`/`ItemAdd` Pattern |
| ItemUse | ItemUseResponse | 521 | Neu, ersetzt `ItemRemove` + Effect Pattern |
| ItemDrop | ItemDropResponse | 522 | Neu, ersetzt direktes `ItemRemove` |
| ItemSplit | ItemSplitResponse | 523 | Neu, ersetzt multiple `ItemRemove`/`ItemAdd` |
| ItemStack | ItemStackResponse | 524 | Neu, ersetzt multiple `ItemMove` Pattern |
| ItemLock | ItemLockResponse | 525 | Neu, ersetzt `ItemLockChanged` Event |
| BagExpand | BagExpandResponse | 526 | Neu, ersetzt `BagExpandSuccess` + `GoldUpdate` |

### 07-party.md (IDs 740-755)

| Request | Neue Response | ID | Vorgeschlagene Änderung |
|---------|---------------|-----|-------------------------|
| PartyInvite | PartyInviteResponse | 740 | Neu, bestätigt bevor `PartyInviteReceived` an Target |
| PartyAccept | PartyAcceptResponse | 741 | Neu, bestätigt bevor `PartyJoin` Broadcast |
| PartyLeave | PartyLeaveResponse | 742 | Neu, bestätigt bevor `PartyLeaveNotification` |
| PartyKick | PartyKickResponse | 743 | Neu, bestätigt bevor `PartyKickNotification` |
| PartyPromote | PartyPromoteResponse | 744 | Neu, bestätigt bevor `PartyPromoteNotification` |
| PartyDisband | PartyDisbandResponse | 745 | Neu, bestätigt bevor `PartyDisbandNotification` |
| PartyLootMode | PartyLootModeResponse | 746 | Neu, bestätigt bevor `PartyLootModeChanged` |
| PartyReadyCheck | PartyReadyCheckResponse | 747 | Neu, bestätigt bevor `PartyReadyCheckStart` |
| PartyReadyCheckResponse | — | — | Bereits korrekt benannt! |

### 08-guild.md (IDs 840-860)

| Request | Neue Response | ID | Vorgeschlagene Änderung |
|---------|---------------|-----|-------------------------|
| GuildCreate | GuildCreateResponse | 840 | Umbenennen von `GuildCreateSuccess` + Success-Pattern |
| GuildInvite | GuildInviteResponse | 841 | Neu, bestätigt bevor `GuildInviteReceived` |
| GuildLeave | GuildLeaveResponse | 842 | Neu, bestätigt bevor `GuildLeaveNotification` |
| GuildKick | GuildKickResponse | 843 | Neu, bestätigt bevor `GuildKickNotification` |
| GuildDisband | GuildDisbandResponse | 844 | Neu, bestätigt bevor `GuildDisbandNotification` |
| GuildPromote | GuildPromoteResponse | 845 | Neu, bestätigt bevor `GuildPromoteNotification` |
| GuildDemote | GuildDemoteResponse | 846 | Neu, bestätigt bevor `GuildDemoteNotification` |
| GuildRankEdit | GuildRankEditResponse | 847 | Umbenennen von `GuildRankEditSuccess` |
| GuildMOTD | GuildMOTDResponse | 848 | Neu, bestätigt bevor `GuildMOTDUpdate` |
| GuildMessage | GuildMessageResponse | 849 | Neu, bestätigt bevor Broadcast |
| GuildBankDeposit | GuildBankDepositResponse | 850 | Neu, ersetzt `ItemRemove` + `GuildBankUpdate` |
| GuildBankWithdraw | GuildBankWithdrawResponse | 851 | Neu, ersetzt `ItemAdd` + `GuildBankUpdate` |

### 10-quest.md

**Notiz:** `QuestAcceptResult` (1001) und `QuestCompleteResult` (1005) sind bereits korrekt mit Success-Pattern!

### 11-trading.md

**Notiz:** `TradeRequestResponse` (1101) ist bereits korrekt!

### 12-targeting.md (IDs 1220-1225)

| Request | Neue Response | ID | Vorgeschlagene Änderung |
|---------|---------------|-----|-------------------------|
| TargetSelect | TargetSelectResponse | 1220 | Neu, bestätigt bevor `TargetUpdate` + `TargetInfoResponse` |
| AssistTarget | AssistTargetResponse | 1221 | Neu, bestätigt Target-Assist |
| MarkTarget | MarkTargetResponse | 1222 | Neu, bestätigt bevor Broadcast |
| TabTarget | TabTargetResponse | 1223 | Neu, bestätigt bevor `TargetUpdate` |
| NearestEnemyTarget | NearestEnemyTargetResponse | 1224 | Neu, bestätigt Target-Selection |
| NearestFriendTarget | NearestFriendTargetResponse | 1225 | Neu, bestätigt Target-Selection |

### 13-npc.md (IDs 1340-1350)

| Request | Neue Response | ID | Vorgeschlagene Änderung |
|---------|---------------|-----|-------------------------|
| VendorBuy | VendorBuyResponse | 1340 | Neu, ersetzt `ItemAdd` + `GoldUpdate` |
| VendorSell | VendorSellResponse | 1341 | Neu, ersetzt `ItemRemove` + `GoldUpdate` |
| VendorBuyback | VendorBuybackResponse | 1342 | Neu, ersetzt `ItemAdd` + `GoldUpdate` |
| TrainerLearn | TrainerLearnResponse | 1343 | Neu, ersetzt `SkillLearned` + `GoldUpdate` |
| RepairAll | RepairAllResponse | 1344 | Neu, ersetzt `GoldUpdate` + Durability-Updates |

### 14-entity.md (IDs 1440-1445)

| Request | Neue Response | ID | Vorgeschlagene Änderung |
|---------|---------------|-----|-------------------------|
| EntityTarget | EntityTargetResponse | 1440 | Neu, bestätigt bevor `EntityTargetUpdate` |

### 15-aura.md

**Notiz:** `DispelResult` (1509), `StealResult` (1511), `PurgeResult` (1513) sind bereits korrekt mit Success-Pattern!

### 21-social.md

**Notiz:** `FriendRequestResult` (2101) und `BlockPlayerResult` (2121) sind bereits korrekt!

### 31-loot.md

**Notiz:** `LootItemResult` (3103) ist bereits korrekt!

### 38-skill.md

**Notiz:** `SkillLearnResult` (3803) ist bereits korrekt!

### 39-equipment.md

**Notiz:** Viele sind bereits korrekt: `EquipItemResult` (3901), `GemSocketResult` (3921), `EnchantApplyResult` (3931), `WeaponSwapResult` (3961)!

---

## 📋 Standard Response-Pattern

Jede neue Response-Message sollte folgendes Pattern verwenden:

```markdown
## [MessageName]Response ([ID])

**Richtung:** 📥 Server → Client  
**Frequenz:** [Entsprechend dem Request]  
**Authentifizierung:** Nein  
**Spezielle Rechte:** Keine

### Beschreibung
Antwort auf [MessageName] Request. Bestätigt erfolgreiche [Action] oder gibt Fehler zurück.

### Response Payload
| Feld | Typ | Beschreibung | Pflicht |
|------|-----|--------------|---------|
| Success | bool | [Action] erfolgreich? | Ja |
| ErrorCode | string | Fehlercode falls Success=false | Nein |
| ErrorMessage | string | Menschenlesbare Fehlermeldung | Nein |
| [weitere erfolgs-spezifische Felder] | [type] | [description] | Bei Erfolg |

### Erwartete Response
- Keine (ist selbst Response)

### Verwandte Messages
| Message | ID | Beziehung |
|---------|-----|-----------|
| `[MessageName]` | [ID] | Request zu dieser Response |
| [Folge-Messages] | [IDs] | [Beschreibung] |

### Beispiel Payload
```csharp
// Erfolg
var successResponse = new [MessageName]Response
{
    Type = MessageType.[MessageName]Response,
    Success = true,
    [weitere Felder]
};

// Fehler
var errorResponse = new [MessageName]Response
{
    Type = MessageType.[MessageName]Response,
    Success = false,
    ErrorCode = "[ERROR_CODE]",
    ErrorMessage = "[Human readable message]"
};
```

### Error Codes
| Code | Bedeutung |
|------|-----------|
| [codes from original request] |
```

---

## 🎯 Vorgeschlagene Message-ID-Ranges

| Kategorie | Range | Verwendung |
|-----------|-------|------------|
| Connection (0) | 21-24 | ✅ Verwendet für Character/Server-Responses |
| Movement (2) | 220-221 | ✅ Verwendet für Teleport/Jump-Responses |
| Combat (3) | 320-325 | Verfügbar für Combat-Responses |
| Chat (4) | 440-450 | Verfügbar für Chat-Channel-Responses |
| Inventory (5) | 520-530 | Verfügbar für Item-Action-Responses |
| Character (6) | 650-655 | ✅ Verwendet für Character-Progression-Responses |
| Party (7) | 740-755 | Verfügbar für Party-Management-Responses |
| Guild (8) | 840-860 | Verfügbar für Guild-Management-Responses |
| Targeting (12) | 1220-1225 | Verfügbar für Target-Selection-Responses |
| NPC (13) | 1340-1350 | Verfügbar für NPC-Interaction-Responses |
| Entity (14) | 1440-1445 | Verfügbar für Entity-Action-Responses |

---

## 📊 Fortschritt

- **Abgeschlossene Dateien:** 5/20+ (25%)
- **Neue Response-Messages definiert:** 26
- **Verbleibende Response-Messages:** ~25-30 (geschätzt)

**Status:** 🟢 In Bearbeitung - Fortschritt läuft gut!

---

## 🔄 Nächste Schritte

1. Verbleibende Dateien in dieser Reihenfolge abarbeiten:
   - 04-chat.md (viele Channel-Management Messages)
   - 05-inventory.md (Item-Actions)
   - 07-party.md (Party-Management)
   - 08-guild.md (Guild-Management)
   - 03-combat.md (Combat-Actions)
   - 12-targeting.md, 13-npc.md, 14-entity.md

2. Nach Abschluss:
   - README.md aktualisieren mit neuen Message-Counts
   - Prüfen ob alle Message-IDs konsistent dokumentiert sind
   - Final Review aller geänderten Dateien

---

**Letzte Aktualisierung**: 2025-12-25  
**Bearbeiter**: GitHub Copilot Agent
