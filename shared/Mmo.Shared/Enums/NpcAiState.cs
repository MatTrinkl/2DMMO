namespace Mmo.Shared.Enums;

public enum NpcAiState : byte
{
    Idle = 0,
    Patrol = 1,
    Combat = 2,
    Evade = 3, // Läuft zurück zum Spawn
    Flee = 4,
    Dead = 5,
    Scripted = 6 // In einem Script/Event
}
