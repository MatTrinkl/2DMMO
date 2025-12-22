namespace Mmo.Shared.Zones.Enums;

/// <summary>
///     Arten von Zone-Transfers.
/// </summary>
public enum ZoneTransferType
{
    Portal, // Durch ein Portal
    Teleport, // Teleport-Spell/Item
    DungeonEntrance, // Dungeon betreten
    Hearthstone, // Hearthstone
    GmCommand, // GM Teleport
    Death // Geist zum Friedhof
}
