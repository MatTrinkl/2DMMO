using Mmo.Shared.Entities;

namespace Mmo.Server.Entities;

/// <summary>
///     Server-side wrapper for PlayerEntity with connection info.
/// </summary>
public class ServerPlayer(PlayerEntity entity, Guid connectionId)
{
    /// <summary>
    ///     The shared PlayerEntity (position, stats, etc.).
    /// </summary>
    public PlayerEntity Entity { get; } = entity;

    /// <summary>
    ///     The ClientConnection ID for this player.
    /// </summary>
    public Guid ConnectionId { get; } = connectionId;

    /// <summary>
    ///     When this player connected.
    /// </summary>
    public DateTimeOffset ConnectedAt { get; } = DateTimeOffset.UtcNow;

    /// <summary>
    ///     Last received input timestamp (for timeout detection).
    /// </summary>
    public DateTimeOffset LastActivity { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// The ID of the player. This includes the ID in the Zone, the ZoneId and the ShardID. All combined are the <see cref="EntityIdentity.GlobalKey"/>.
    /// </summary>
    public EntityIdentity EntityId => Entity.EntityId;

    /// <summary>
    /// The name of the player.
    /// </summary>
    public string Name => Entity.DisplayName;
}
