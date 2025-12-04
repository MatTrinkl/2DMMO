using MessagePack;
using Mmo.Shared.Records;

namespace Mmo.Shared.Entities;

/// <summary>
///     This class represents the player data in the world.
/// </summary>
[MessagePackObject]
public class PlayerState : EntityState
{
    /// <summary>
    ///     The constructor used bei <see cref="MessagePackSerializer" />.
    /// </summary>
    [SerializationConstructor]
    public PlayerState()
    {
    }


    /// <summary>
    ///     Creates a new PlayerState object.
    /// </summary>
    /// <param name="playerId">ID of the Player (Entity).</param>
    /// <param name="username">Username of the Player.</param>
    /// <param name="x">Current X Position.</param>
    /// <param name="y">Current Y Position.</param>
    public PlayerState(Guid playerId, string username, float x, float y)
    {
        EntityId = playerId;
        Username = username;
        Position = new Position(x, y);
    }

    /// <summary>
    ///     Creates a new PlayerState object.
    /// </summary>
    /// <param name="playerId">ID of the Player (Entity).</param>
    /// <param name="username">Username of the Player.</param>
    /// <param name="position">Current position of the player.</param>
    public PlayerState(Guid playerId, string username, Position position) : base(playerId, position)
    {
        EntityId = playerId;
        Username = username;
        Position = position;
    }

    /// <summary>
    ///     The username of the player.
    /// </summary>
    [Key(3)]
    public string Username { get; set; }

    /// <summary>
    ///     Calls the EntityId.
    /// </summary>
    [IgnoreMember]
    public Guid PlayerId => EntityId;
}
