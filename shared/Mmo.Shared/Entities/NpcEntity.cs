using MessagePack;
using Mmo.Shared.Enums;
using Mmo.Shared.Enums.Entities;
using Mmo.Shared.Interfaces;
using Mmo.Shared.Records;

namespace Mmo.Shared.Entities;

/// <summary>
///     NPC/Monster Entity.
/// </summary>
[MessagePackObject]
public class NpcEntity : CombatEntity, INpcEntity
{
    private readonly Dictionary<Guid, int> _threatTable = new();

    // ═══════════════════════════════════════════════════════════════
    // CONSTRUCTORS
    // ═══════════════════════════════════════════════════════════════

    [SerializationConstructor]
    public NpcEntity()
    {
    }

    public NpcEntity(
        int npcTemplateId,
        int spawnId,
        string displayName,
        Position spawnPosition,
        ushort prefabId)
        : base(Guid.NewGuid(), spawnPosition, prefabId)
    {
        NpcTemplateId = npcTemplateId;
        SpawnId = spawnId;
        DisplayName = displayName;
        SpawnPosition = spawnPosition;
    }

    [Key(25)] public bool CanFlee { get; set; } = false;

    [Key(26)] public float FleeHealthPercent { get; set; } = 0.15f;

    [Key(27)] public bool CanCallForHelp { get; set; } = true;

    [Key(28)] public float CallForHelpRadius { get; set; } = 15f;

    [Key(31)] public DateTime? LastDeathTime { get; set; }

    [IgnoreMember]
    public bool IsReadyToRespawn => LastDeathTime != null &&
                                    DateTime.UtcNow >= LastDeathTime.Value.AddSeconds(RespawnTimeSeconds);

    // ═══════════════════════════════════════════════════════════════
    // INpcEntity - Patrol
    // ═══════════════════════════════════════════════════════════════

    [Key(32)] public bool HasPatrolPath { get; set; } = false;

    [Key(33)] public int? PatrolPathId { get; set; }

    [Key(34)] public int CurrentWaypointIndex { get; set; }

    [Key(35)] public bool IsWaitingAtWaypoint { get; set; } = false;

    [Key(37)] public int GoldDrop { get; set; } = 0;

    // ═══════════════════════════════════════════════════════════════
    // INpcEntity - Interaction
    // ═══════════════════════════════════════════════════════════════

    [Key(39)] public bool IsInteractable { get; set; } = false;

    [Key(40)] public int? GossipId { get; set; }

    [Key(41)] public bool IsVendor { get; set; } = false;

    [Key(42)] public int? VendorListId { get; set; }

    [Key(43)] public bool IsQuestGiver { get; set; } = false;

    [Key(44)] public bool IsTrainer { get; set; } = false;

    [Key(45)] public TrainerType? TrainerType { get; set; }

    // ═══════════════════════════════════════════════════════════════
    // IEntity Overrides
    // ═══════════════════════════════════════════════════════════════

    [IgnoreMember] public override EntityType Type => EntityType.Npc;

    [IgnoreMember] public override bool IsTrulyPersistent => false; // NPCs werden nicht in DB gespeichert

    // ═══════════════════════════════════════════════════════════════
    // INpcEntity - Identity
    // ═══════════════════════════════════════════════════════════════

    [Key(18)] public int NpcTemplateId { get; }

    [Key(19)] public int SpawnId { get; }

    [Key(20)] public CombatRank CombatRank { get; set; } = CombatRank.Normal;

    [Key(21)] public NpcFunction Function { get; set; } = NpcFunction.Hostile;

    // CreatureType bleibt (Humanoid, Beast, Undead, etc.)
    [Key(22)] public CreatureType CreatureType { get; set; } = CreatureType.Humanoid;

    // ═══════════════════════════════════════════════════════════════
    // INpcEntity - AI
    // ═══════════════════════════════════════════════════════════════

    [Key(22)] public NpcAiState AiState { get; set; } = NpcAiState.Idle;

    [Key(23)] public float AggroRadius { get; set; } = 10f;

    [Key(24)] public float LeashRadius { get; set; } = 40f;

    // ═══════════════════════════════════════════════════════════════
    // INpcEntity - Spawn
    // ═══════════════════════════════════════════════════════════════

    [Key(29)] public Position SpawnPosition { get; }

    [Key(30)] public int RespawnTimeSeconds { get; set; } = 60;

    // ═══════════════════════════════════════════════════════════════
    // INpcEntity - Loot
    // ═══════════════════════════════════════════════════════════════

    [Key(36)] public int? LootTableId { get; set; }

    [Key(38)] public int ExperienceReward { get; set; } = 10;

    // ═══════════════════════════════════════════════════════════════
    // INpcEntity - Threat
    // ═══════════════════════════════════════════════════════════════

    [IgnoreMember]
    public Guid? HighestThreatEntityId
    {
        get
        {
            if (_threatTable.Count == 0) return null;
            return _threatTable.MaxBy(kvp => kvp.Value).Key;
        }
    }

    public void AddThreat(Guid entityId, int amount)
    {
        if (!_threatTable.TryAdd(entityId, amount))
            _threatTable[entityId] += amount;
    }

    public void ClearThreat() => _threatTable.Clear();

    // ═══════════════════════════════════════════════════════════════
    // OVERRIDES
    // ═══════════════════════════════════════════════════════════════

    public override bool IsHostileTo(ICombatEntity other)
    {
        // Monster sind immer feindlich zu Spielern
        if (Faction == Faction.Monster && other is ICharacterEntity)
            return true;

        return base.IsHostileTo(other);
    }

    // ═══════════════════════════════════════════════════════════════
    // INpcEntity METHODS
    // ═══════════════════════════════════════════════════════════════

    public void Reset()
    {
        // Zurück zum Spawn
        Position = SpawnPosition;
        CurrentHealth = MaxHealth;
        CurrentResource = MaxResource;
        AiState = HasPatrolPath ? NpcAiState.Patrol : NpcAiState.Idle;
        IsInCombat = false;
        TargetEntityId = null;
        ClearThreat();
        CurrentWaypointIndex = 0;
    }

    public void Respawn()
    {
        Reset();
        AiState = HasPatrolPath ? NpcAiState.Patrol : NpcAiState.Idle;
        LastDeathTime = null;
    }

    public void Evade()
    {
        AiState = NpcAiState.Evade;
        TargetEntityId = null;
        ClearThreat();

        // Movement zurück zum Spawn wird vom AI-System behandelt
    }

    public void RemoveThreat(Guid entityId) => _threatTable.Remove(entityId);

    public int GetThreat(Guid entityId) => _threatTable.GetValueOrDefault(entityId, 0);

    protected override void OnDeath(ICombatEntity? killer)
    {
        AiState = NpcAiState.Dead;
        LastDeathTime = DateTime.UtcNow;
        ClearThreat();
    }

    protected override void OnEnterCombat(ICombatEntity? enemy)
    {
        AiState = NpcAiState.Combat;

        if (enemy != null)
        {
            AddThreat(enemy.PersistentId, 1);
            TargetEntityId = enemy.PersistentId;
        }
    }

    protected override void OnLeaveCombat()
    {
        // Evade statt einfach aus Combat gehen
        if (!IsDead) Evade();
    }

    public void CallForHelp()
    {
        // Wird vom AI-System implementiert - signalisiert nur den Intent
    }
}
