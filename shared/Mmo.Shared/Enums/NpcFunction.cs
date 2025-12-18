namespace Mmo.Shared.Enums;

/// <summary>
///     Funktion/Interaktionsart eines NPCs.
///     Was kann der Spieler mit diesem NPC tun?
/// </summary>
[Flags]
public enum NpcFunction : ushort
{
    None = 0,

    // Combat Behavior
    Hostile = 1 << 0, // Greift Spieler an
    Neutral = 1 << 1, // Greift nur zurück
    Friendly = 1 << 2, // Nicht angreifbar

    // Services (kombinierbar!)
    Vendor = 1 << 3, // Verkauft Items
    QuestGiver = 1 << 4, // Gibt/Beendet Quests
    Trainer = 1 << 5, // Lehrt Skills
    FlightMaster = 1 << 6, // Flugpunkte
    Innkeeper = 1 << 7, // Hearthstone binden
    Banker = 1 << 8, // Bank-Zugang
    Auctioneer = 1 << 9, // Auktionshaus
    Repairer = 1 << 10, // Repariert Ausrüstung
    SpiritHealer = 1 << 11 // Respawn-Punkt
}
