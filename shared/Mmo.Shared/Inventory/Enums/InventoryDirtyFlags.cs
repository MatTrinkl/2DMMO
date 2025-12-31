namespace Mmo.Shared.Inventory.Enums;

/// <summary>
///     Flags indicating which inventory slots or properties have changed.
///     These flags are used by the dirty-tracking system to optimize inventory updates.
///     Use bitwise OR to combine multiple flags.
/// </summary>
/// <remarks>
///     This enum uses ulong (64 bits) to support tracking up to 60 individual inventory slots
///     plus special properties (gold, bank, bag slots).
///     Slots 0-59 use bits 0-59, special properties use bits 60-63.
/// </remarks>
[Flags]
public enum InventoryDirtyFlags : ulong
{
    /// <summary>
    ///     No properties have changed.
    /// </summary>
    None = 0,
    
    // ═══════════════════════════════════════════════════════════════
    // INVENTORY SLOTS (Bits 0-31) - Main Bag
    // ═══════════════════════════════════════════════════════════════
    
    Slot0  = 1UL << 0,
    Slot1  = 1UL << 1,
    Slot2  = 1UL << 2,
    Slot3  = 1UL << 3,
    Slot4  = 1UL << 4,
    Slot5  = 1UL << 5,
    Slot6  = 1UL << 6,
    Slot7  = 1UL << 7,
    Slot8  = 1UL << 8,
    Slot9  = 1UL << 9,
    Slot10 = 1UL << 10,
    Slot11 = 1UL << 11,
    Slot12 = 1UL << 12,
    Slot13 = 1UL << 13,
    Slot14 = 1UL << 14,
    Slot15 = 1UL << 15,
    Slot16 = 1UL << 16,
    Slot17 = 1UL << 17,
    Slot18 = 1UL << 18,
    Slot19 = 1UL << 19,
    Slot20 = 1UL << 20,
    Slot21 = 1UL << 21,
    Slot22 = 1UL << 22,
    Slot23 = 1UL << 23,
    Slot24 = 1UL << 24,
    Slot25 = 1UL << 25,
    Slot26 = 1UL << 26,
    Slot27 = 1UL << 27,
    Slot28 = 1UL << 28,
    Slot29 = 1UL << 29,
    Slot30 = 1UL << 30,
    Slot31 = 1UL << 31,
    
    // ═══════════════════════════════════════════════════════════════
    // ADDITIONAL SLOTS (Bits 32-59) - Additional Bags
    // ═══════════════════════════════════════════════════════════════
    
    Slot32 = 1UL << 32,
    Slot33 = 1UL << 33,
    Slot34 = 1UL << 34,
    Slot35 = 1UL << 35,
    Slot36 = 1UL << 36,
    Slot37 = 1UL << 37,
    Slot38 = 1UL << 38,
    Slot39 = 1UL << 39,
    Slot40 = 1UL << 40,
    Slot41 = 1UL << 41,
    Slot42 = 1UL << 42,
    Slot43 = 1UL << 43,
    Slot44 = 1UL << 44,
    Slot45 = 1UL << 45,
    Slot46 = 1UL << 46,
    Slot47 = 1UL << 47,
    Slot48 = 1UL << 48,
    Slot49 = 1UL << 49,
    Slot50 = 1UL << 50,
    Slot51 = 1UL << 51,
    Slot52 = 1UL << 52,
    Slot53 = 1UL << 53,
    Slot54 = 1UL << 54,
    Slot55 = 1UL << 55,
    Slot56 = 1UL << 56,
    Slot57 = 1UL << 57,
    Slot58 = 1UL << 58,
    Slot59 = 1UL << 59,
    
    // ═══════════════════════════════════════════════════════════════
    // SPECIAL PROPERTIES (Bits 60-63)
    // ═══════════════════════════════════════════════════════════════
    
    /// <summary>
    ///     Gold/currency amount has changed.
    /// </summary>
    Gold = 1UL << 60,
    
    /// <summary>
    ///     Bank contents have been updated.
    /// </summary>
    BankUpdated = 1UL << 61,
    
    /// <summary>
    ///     Number of bag slots has changed (bag equipped/unequipped).
    /// </summary>
    BagSlotsChanged = 1UL << 62,
    
    // ═══════════════════════════════════════════════════════════════
    // CONVENIENCE COMBINATIONS
    // ═══════════════════════════════════════════════════════════════
    
    /// <summary>
    ///     All slot flags (bits 0-59).
    /// </summary>
    AllSlots = 0x0FFFFFFFFFFFFFFFUL,
    
    /// <summary>
    ///     All special property flags (Gold, Bank, BagSlots).
    /// </summary>
    AllSpecial = Gold | BankUpdated | BagSlotsChanged,
    
    /// <summary>
    ///     All flags set.
    /// </summary>
    All = ~None
}
