using MessagePack;
using Mmo.Shared.Connection.Enums;

namespace Mmo.Shared.Connection.Records;

/// <summary>
///     Contains the core information of a realm.
/// </summary>
[MessagePackObject]
public record RealmInfo
{
    /// <summary>
    ///     The ID of a realm.
    /// </summary>
    [property: Key(0)]
    private int RealmId { get; set; }

    /// <summary>
    ///     The Name of the realm
    /// </summary>
    [property: Key(1)]
    private string? Name { get; set; }

    /// <summary>
    ///     Type of the realm
    /// </summary>
    [property: Key(2)]
    private RealmType Type { get; set; }

    /// <summary>
    ///     The population of the realm.
    /// </summary>
    [property: Key(3)]
    private RealmPopulationStatus Population { get; set; }

    /// <summary>
    ///     Is the realm online?
    /// </summary>
    [property: Key(4)]
    private bool Online { get; set; }
}
