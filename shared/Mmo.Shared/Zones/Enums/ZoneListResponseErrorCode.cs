using Mmo.Shared.Zones.Messages.Client_Server;

namespace Mmo.Shared.Zones.Enums;

/// <summary>
/// Error code when <see cref="ZoneListRequest"/> was not successful.
/// </summary>
public enum ZoneListResponseErrorCode : byte
{
    /// <summary>
    /// The given entity id was not found.
    /// </summary>
    EntityNotFound = 0,
    /// <summary>
    /// The given entity was invalid for Zone Transfer operations.
    /// </summary>
    InvalidEntityType = 1
}
