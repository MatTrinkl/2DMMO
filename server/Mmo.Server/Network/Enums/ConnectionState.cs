namespace Mmo.Server.Network.Enums;

/// <summary>
///     Verbindungsstatus.
/// </summary>
public enum ConnectionState
{
    /// <summary>Verbunden, aber nicht authentifiziert.</summary>
    Connected = 0,

    /// <summary>Authentifiziert, wartet auf Character-Auswahl.</summary>
    Authenticated = 1,

    /// <summary>Im Spiel (Charakter ausgewählt und gespawnt).</summary>
    InGame = 2
}
