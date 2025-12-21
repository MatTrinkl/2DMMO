namespace Mmo.Shared.Combat.Enums;

/// <summary>
///     Represents the type of damage dealt or received.
/// </summary>
public enum DamageType : byte
{
    /// <summary>
    ///     Physical damage (affected by armor).
    /// </summary>
    Physical = 0,

    /// <summary>
    ///     Fire elemental damage.
    /// </summary>
    Fire = 1,

    /// <summary>
    ///     Ice elemental damage.
    /// </summary>
    Ice = 2,

    /// <summary>
    ///     Lightning elemental damage.
    /// </summary>
    Lightning = 3,

    /// <summary>
    ///     Arcane magical damage.
    /// </summary>
    Arcane = 4,

    /// <summary>
    ///     Holy light damage.
    /// </summary>
    Holy = 5,

    /// <summary>
    ///     Shadow dark damage.
    /// </summary>
    Shadow = 6,

    /// <summary>
    ///     Nature damage.
    /// </summary>
    Nature = 7,

    /// <summary>
    ///     True damage (ignores armor and resistances).
    /// </summary>
    True = 8
}
