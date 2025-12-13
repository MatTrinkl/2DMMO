namespace Mmo.Shared.Entities;

/// <summary>
///     Central registry for all Prefab IDs in the game.
///     PrefabIds define the type/template of an entity and never change.
/// </summary>
public static class PrefabIds
{
    // ═══════════════════════════════════════════════════
    // PLAYERS (1-99)
    // ═══════════════════════════════════════════════════
    
    /// <summary>
    ///     Default player prefab.
    /// </summary>
    public const ushort PlayerDefault = 1;
    
    // ═══════════════════════════════════════════════════
    // NPCs - QUESTGIVERS (100-199)
    // ═══════════════════════════════════════════════════
    
    /// <summary>
    ///     Old man questgiver NPC.
    /// </summary>
    public const ushort QuestgiverOldMan = 100;
    
    /// <summary>
    ///     Elf questgiver NPC.
    /// </summary>
    public const ushort QuestgiverElf = 101;
    
    // ═══════════════════════════════════════════════════
    // NPCs - VENDORS (200-299)
    // ═══════════════════════════════════════════════════
    
    /// <summary>
    ///     Blacksmith vendor NPC.
    /// </summary>
    public const ushort VendorBlacksmith = 200;
    
    // ═══════════════════════════════════════════════════
    // MOBS (1000-1999)
    // ═══════════════════════════════════════════════════
    
    /// <summary>
    ///     Goblin mob enemy.
    /// </summary>
    public const ushort MobGoblin = 1000;
    
    // ═══════════════════════════════════════════════════
    // INTERACTABLE OBJECTS (2000-2999)
    // ═══════════════════════════════════════════════════
    
    /// <summary>
    ///     Wooden chest that can be opened for loot.
    /// </summary>
    public const ushort ObjectChestWooden = 2000;
    
    /// <summary>
    ///     Red herb that can be harvested.
    /// </summary>
    public const ushort ObjectHerbRed = 2100;
    
    // ═══════════════════════════════════════════════════
    // STATIC OBJECTS (3000-3999)
    // ═══════════════════════════════════════════════════
    
    /// <summary>
    ///     Door that can be opened/closed.
    /// </summary>
    public const ushort ObjectDoor = 3000;
    
    /// <summary>
    ///     Sign with text information.
    /// </summary>
    public const ushort ObjectSign = 3001;
}
