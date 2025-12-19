namespace Mmo.Shared.Enums;

/// <summary>
///     Represents the type of resource used by character classes.
/// </summary>
public enum ResourceType : byte
{
    /// <summary>
    ///     No resource (e.g., warriors without rage).
    /// </summary>
    None = 0,

    /// <summary>
    ///     Mana resource (used by mages, priests).
    /// </summary>
    Mana = 1,

    /// <summary>
    ///     Energy resource (used by rogues).
    /// </summary>
    Energy = 2,

    /// <summary>
    ///     Rage resource (generated through combat).
    /// </summary>
    Rage = 3,

    /// <summary>
    ///     Focus resource (used by hunters).
    /// </summary>
    Focus = 4,

    /// <summary>
    ///     Runic power resource (used by death knights).
    /// </summary>
    RunicPower = 5
}
