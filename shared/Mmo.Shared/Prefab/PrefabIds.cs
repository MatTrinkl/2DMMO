namespace Mmo.Shared.Prefab;

/// <summary>
///     Provides convenient access to Prefab IDs.
///     Can be loaded from prefabs.json via PrefabRegistry or use default constants.
///     PrefabIds define the type/template of an entity and never change.
/// </summary>
public static class PrefabIds
{
    // ═══════════════════════════════════════════════════
    // PLAYERS (1-99)
    // ═══════════════════════════════════════════════════

    /// <summary>
    ///     Default player prefab.
    ///     Can be loaded from prefabs.json: player:playerDefault
    /// </summary>
    public static ushort PlayerDefault =>
        PrefabRegistry.Instance.TryGetId("player", "playerDefault", out ushort id) ? id : (ushort)1;

    // ═══════════════════════════════════════════════════
    // NPCs - QUESTGIVERS (100-199)
    // ═══════════════════════════════════════════════════

    /// <summary>
    ///     Old man questgiver NPC.
    ///     Can be loaded from prefabs.json: npc:questgiverOldMan
    /// </summary>
    public static ushort QuestgiverOldMan =>
        PrefabRegistry.Instance.TryGetId("npc", "questgiverOldMan", out ushort id) ? id : (ushort)100;

    /// <summary>
    ///     Elf questgiver NPC.
    ///     Can be loaded from prefabs.json: npc:questgiverElf
    /// </summary>
    public static ushort QuestgiverElf =>
        PrefabRegistry.Instance.TryGetId("npc", "questgiverElf", out ushort id) ? id : (ushort)101;

    // ═══════════════════════════════════════════════════
    // NPCs - VENDORS (200-299)
    // ═══════════════════════════════════════════════════

    /// <summary>
    ///     Blacksmith vendor NPC.
    ///     Can be loaded from prefabs.json: npc:vendorBlacksmith
    /// </summary>
    public static ushort VendorBlacksmith =>
        PrefabRegistry.Instance.TryGetId("npc", "vendorBlacksmith", out ushort id) ? id : (ushort)200;

    // ═══════════════════════════════════════════════════
    // MOBS (1000-1999)
    // ═══════════════════════════════════════════════════

    /// <summary>
    ///     Goblin mob enemy.
    ///     Can be loaded from prefabs.json: mob:goblin
    /// </summary>
    public static ushort MobGoblin =>
        PrefabRegistry.Instance.TryGetId("mob", "goblin", out ushort id) ? id : (ushort)1000;

    // ═══════════════════════════════════════════════════
    // INTERACTABLE OBJECTS (2000-2999)
    // ═══════════════════════════════════════════════════

    /// <summary>
    ///     Wooden chest that can be opened for loot.
    ///     Can be loaded from prefabs.json: object:chestWooden
    /// </summary>
    public static ushort ObjectChestWooden =>
        PrefabRegistry.Instance.TryGetId("object", "chestWooden", out ushort id) ? id : (ushort)2000;

    /// <summary>
    ///     Red herb that can be harvested.
    ///     Can be loaded from prefabs.json: object:herbRed
    /// </summary>
    public static ushort ObjectHerbRed =>
        PrefabRegistry.Instance.TryGetId("object", "herbRed", out ushort id) ? id : (ushort)2100;

    // ═══════════════════════════════════════════════════
    // STATIC OBJECTS (3000-3999)
    // ═══════════════════════════════════════════════════

    /// <summary>
    ///     Door that can be opened/closed.
    ///     Can be loaded from prefabs.json: object:door
    /// </summary>
    public static ushort ObjectDoor =>
        PrefabRegistry.Instance.TryGetId("object", "door", out ushort id) ? id : (ushort)3000;

    /// <summary>
    ///     Sign with text information.
    ///     Can be loaded from prefabs.json: object:sign
    /// </summary>
    public static ushort ObjectSign =>
        PrefabRegistry.Instance.TryGetId("object", "sign", out ushort id) ? id : (ushort)3001;
}
