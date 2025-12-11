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
    ///     The constructor used by <see cref="MessagePackSerializer" />.
    /// </summary>
    [SerializationConstructor]
    public PlayerEntity()
    {
    }

    /// <summary>
    ///     Creates a new PlayerEntity.
    /// </summary>
    /// <param name="characterId">Persistent ID from database - NEVER changes.</param>
    /// <param name="displayName">Username of the player.</param>
    /// <param name="position">Starting position.</param>
    public PlayerEntity(Guid characterId, string displayName, Position position)
        : base(characterId, position, true)
    {
        DisplayName = displayName;
    }

    /// <summary>
    ///     The username of the player.
    /// </summary>
    [Key(4)]
    public string DisplayName { get; set; } = "";

    /// <summary>
    ///     The EntityType of the player is Player.
    /// </summary>
    [Key(5)]
    public override EntityType Type => EntityType.Player;

    /// <summary>
    ///     The player has no EntityRole for now.
    /// </summary>
    [Key(6)]
    public override EntityRole Role => EntityRole.None;

    /// <summary>
    ///     WIP: This methode will be called when this player changes the zone.
    /// </summary>
    /// <param name="newZoneId">The ID of the new zone.</param>
    public override void ChangeZone(ushort newZoneId)
    {
    }
}
