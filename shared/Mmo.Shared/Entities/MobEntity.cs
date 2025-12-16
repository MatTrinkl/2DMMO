using MessagePack;
using Mmo.Shared.Enums;
using Mmo.Shared.Records;

namespace Mmo.Shared.Entities;

/// <summary>
///     Represents a spawned mob (runtime-persistent entity).
/// </summary>
[MessagePackObject]
public class MobEntity : Entity
{
    /// <summary>
    ///     The constructor used by <see cref="MessagePackSerializer" />.
    /// </summary>
    [SerializationConstructor]
    public MobEntity()
    {
    }

    /// <summary>
    ///     Creates a new spawned mob with auto-generated PersistentId.
    ///     Uses default Goblin prefab if not specified.
    /// </summary>
    /// <param name="mobName">The name/type of the mob.</param>
    /// <param name="position">The spawn position.</param>
    /// <param name="maxHealth">Maximum health of the mob (default: 100).</param>
    /// <param name="prefabId">The prefab ID of the mob (default: Goblin from PrefabIds).</param>
    public MobEntity(string mobName, Position position, int maxHealth = 100, ushort prefabId = 1000)
        : base(position, prefabId) // Runtime-only (generates new PersistentId)
    {
        MobName = mobName;
        MaxHealth = maxHealth;
        CurrentHealth = maxHealth;
    }

    /// <summary>
    ///     Creates a mob with specific PersistentId (for syncing/respawn).
    ///     Uses default Goblin prefab if not specified.
    /// </summary>
    /// <param name="persistentId">The persistent ID.</param>
    /// <param name="mobName">The name/type of the mob.</param>
    /// <param name="position">The spawn position.</param>
    /// <param name="maxHealth">Maximum health of the mob (default: 100).</param>
    /// <param name="prefabId">The prefab ID of the mob (default: Goblin from PrefabIds).</param>
    public MobEntity(Guid persistentId, string mobName, Position position, int maxHealth = 100, ushort prefabId = 1000)
        : base(persistentId, position, prefabId, false)
    {
        MobName = mobName;
        MaxHealth = maxHealth;
        CurrentHealth = maxHealth;
    }

    /// <summary>
    ///     The name/type of the mob (e.g., "Goblin", "Wolf").
    /// </summary>
    [Key(4)]
    public string MobName { get; set; } = "";

    /// <summary>
    ///     Maximum health of the mob.
    /// </summary>
    [Key(5)]
    public int MaxHealth { get; set; } = 100;

    /// <summary>
    ///     Current health of the mob.
    /// </summary>
    [Key(6)]
    public int CurrentHealth { get; set; } = 100;

    /// <summary>
    ///     The EntityType of the mob is Mob.
    /// </summary>
    [Key(7)]
    public override EntityType Type => EntityType.Mob;

    /// <summary>
    ///     The mob is hostile by default.
    /// </summary>
    [Key(8)]
    public override EntityRole Role => EntityRole.Hostile;

    /// <summary>
    ///     Whether the mob is dead.
    /// </summary>
    [IgnoreMember]
    public bool IsDead => CurrentHealth <= 0;

    /// <summary>
    ///     Health as a percentage (0.0 to 1.0).
    /// </summary>
    [IgnoreMember]
    public float HealthPercentage => MaxHealth > 0 ? (float)CurrentHealth / MaxHealth : 0;

    /// <summary>
    ///     Called when the mob changes zones.
    /// </summary>
    public override void ChangeZone(ushort newZoneId)
    {
        // Mobs typically don't persist across zone changes
    }
}
