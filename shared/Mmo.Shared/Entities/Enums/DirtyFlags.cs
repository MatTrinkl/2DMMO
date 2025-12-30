namespace Mmo.Shared.Entities.Enums;

/// <summary>
///     Flags indicating which properties of an entity have changed.
///     These flags are used by the dirty-tracking system to optimize network updates.
///     Use bitwise OR to combine multiple flags.
/// </summary>
/// <remarks>
///     This enum is extensible - additional flags can be added as needed.
///     The uint type provides 32 possible flags (bits 0-31).
///     For entity-specific flags, consider creating separate enums (e.g., NpcDirtyFlags, QuestDirtyFlags).
/// </remarks>
[Flags]
public enum DirtyFlags : uint
{
    /// <summary>
    ///     No properties have changed.
    /// </summary>
    None = 0,
    
    // ═══════════════════════════════════════════════════════════════
    // MOVEMENT FLAGS (Bits 0-2)
    // ═══════════════════════════════════════════════════════════════
    
    /// <summary>
    ///     Position (X, Y) has changed.
    /// </summary>
    Position = 1 << 0,
    
    /// <summary>
    ///     Velocity (VelocityX, VelocityY) has changed.
    /// </summary>
    Velocity = 1 << 1,
    
    /// <summary>
    ///     Rotation has changed.
    /// </summary>
    Rotation = 1 << 2,
    
    // ═══════════════════════════════════════════════════════════════
    // COMBAT/STATE FLAGS (Bits 3-7)
    // ═══════════════════════════════════════════════════════════════
    
    /// <summary>
    ///     Current health points have changed.
    /// </summary>
    Health = 1 << 3,
    
    /// <summary>
    ///     Maximum health points have changed.
    /// </summary>
    MaxHealth = 1 << 4,
    
    /// <summary>
    ///     Current resource (mana, energy, rage, etc.) has changed.
    /// </summary>
    Resource = 1 << 5,
    
    /// <summary>
    ///     Maximum resource has changed.
    /// </summary>
    MaxResource = 1 << 6,
    
    /// <summary>
    ///     Entity state (Idle, Combat, Dead, etc.) has changed.
    /// </summary>
    State = 1 << 7,
    
    // ═══════════════════════════════════════════════════════════════
    // VISUAL FLAGS (Bits 8-9)
    // ═══════════════════════════════════════════════════════════════
    
    /// <summary>
    ///     Visual model/appearance has changed (polymorph, disguise, etc.).
    /// </summary>
    Model = 1 << 8,
    
    /// <summary>
    ///     Level has changed.
    /// </summary>
    Level = 1 << 9,
    
    // ═══════════════════════════════════════════════════════════════
    // LIFECYCLE FLAGS (Bits 30-31)
    // ═══════════════════════════════════════════════════════════════
    
    /// <summary>
    ///     Entity has been spawned (new entity in zone).
    /// </summary>
    Spawned = 1u << 30,
    
    /// <summary>
    ///     Entity has been despawned (removed from zone).
    /// </summary>
    Despawned = 1u << 31,
    
    // ═══════════════════════════════════════════════════════════════
    // CONVENIENCE COMBINATIONS
    // ═══════════════════════════════════════════════════════════════
    
    /// <summary>
    ///     All movement-related properties (Position, Velocity, Rotation).
    /// </summary>
    Movement = Position | Velocity | Rotation,
    
    /// <summary>
    ///     All combat-related properties (Health, MaxHealth, State).
    /// </summary>
    Combat = Health | MaxHealth | State,
    
    /// <summary>
    ///     All stat-related properties (Health, MaxHealth, Resource, MaxResource, Level).
    /// </summary>
    AllStats = Health | MaxHealth | Resource | MaxResource | Level
}
