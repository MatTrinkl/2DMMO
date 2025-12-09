using MessagePack;
using Mmo.Shared.Enums;
using Mmo.Shared.Records;

namespace Mmo.Shared.Entities;

/// <summary>
///     This class represents the player data in the world.
/// </summary>
[MessagePackObject]
public class PlayerEntity : Entity
{
    /// <summary>
    ///     The constructor used bei <see cref="MessagePackSerializer" />.
    /// </summary>
    [SerializationConstructor]
    public PlayerEntity()
    {
    }

    /// <summary>
    ///     Creates a new PlayerState object.
    /// </summary>
    /// <param name="playerId">ID of the Player (Entity).</param>
    /// <param name="displayName">Username of the Player.</param>
    /// <param name="x">Current X Position.</param>
    /// <param name="y">Current Y Position.</param>
    public PlayerEntity(EntityIdentity playerId, string displayName, float x, float y)
    {
        EntityId = playerId;
        DisplayName = displayName;
        Position = new Position(x, y);
    }

    /// <summary>
    ///     Creates a new PlayerState object.
    /// </summary>
    /// <param name="playerId">ID of the Player (Entity).</param>
    /// <param name="displayName">Username of the Player.</param>
    /// <param name="position">Current position of the player.</param>
    public PlayerEntity(EntityIdentity playerId, string displayName, Position position) : base(playerId, position)
    {
        EntityId = playerId;
        DisplayName = displayName;
        Position = position;
    }

    /// <summary>
    ///     The username of the player.
    /// </summary>
    [Key(2)]
    public string DisplayName { get; set; } = "";

    /// <summary>
    ///     The EntityType of the player is Player.
    /// </summary>
    [Key(3)]
    public override EntityType Type => EntityType.Player;

    /// <summary>
    ///     The player has no EntityRole for now.
    /// </summary>
    [Key(4)]
    public override EntityRole Role => EntityRole.None;

    /// <summary>
    ///     WIP: This methode will be called when this player changes the zone.
    /// </summary>
    /// <param name="newZoneId">The ID of the new zone.</param>
    public override void ChangeZone(ushort newZoneId)
    {
    }
}
