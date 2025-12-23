namespace Mmo.Server.Connections.Enums;

/// <summary>
///     Connection status.
/// </summary>
public enum ConnectionState
{
    /// <summary>Connected, but not authenticated.</summary>
    Connected = 0,

    /// <summary>Authenticated, waiting for character selection.</summary>
    Authenticated = 1,

    /// <summary>In game (character selected and spawned).</summary>
    InGame = 2
}
