namespace Mmo.Shared.Enums.Messages;

/// <summary>
///     Each Category represents up to 100 (x00-x99) <see cref="MessageType" />s.
///     The Category can be access when dividing the Type by 100 and the mod 100 is the subindex.
///     The <see cref="MessageType" /> need to start with the Category e.g.: ConnectionLoginRequest, ZoneEventPlayerJoined
///     etc.
///     All Categories are in singular therefore.
/// </summary>
public enum MessageCategory : byte
{
    /// <summary>
    ///     Range: 0000-0099
    ///     Includes: Connection/Authentication
    /// </summary>
    Connection = 0,

    /// <summary>
    ///     Range: 0100-0199
    ///     Includes: Zone Events (Player joning the Zone, etc.)
    /// </summary>
    ZoneEvent = 1,

    /// <summary>
    ///     Range: 0200-0299
    ///     Includes: Movement/Position of Entities
    /// </summary>
    Movement = 2,

    /// <summary>
    ///     Range: 0300-0399
    ///     Includes: Combat
    /// </summary>
    Combat = 3,

    /// <summary>
    ///     Range: 0400-0499
    ///     Includes: Chat (1-all, 1-1, 1-zone, 1-guild, Server-all, NPC-1 etc.)
    /// </summary>
    Chat = 4,

    /// <summary>
    ///     Range: 0500-0599
    ///     Includes: Inventory/Items/Potions
    /// </summary>
    Inventory = 5,

    /// <summary>
    ///     Range: 0600-0699
    ///     Includes: Character/Stats/Progression (EXP, Level-Up, Health/Mana)
    /// </summary>
    Character = 6,

    /// <summary>
    ///     Range: 0700-0799
    ///     Includes: Group/Party (Invention, Interaction, etc.)
    /// </summary>
    Group = 7,

    /// <summary>
    ///     Range: 0800-0899
    ///     Includes: Guild (Join, Leave, Infos etc.)
    /// </summary>
    Guild = 8,

    /// <summary>
    ///     Range: 0900-0999
    ///     Includes: Ping/Latency/System
    /// </summary>
    Ping = 9,

    /// <summary>
    ///     Range: 1000-1099
    ///     Includes: Quest (Show, Accept, Finish etc.)
    /// </summary>
    Quest = 10,

    /// <summary>
    ///     Range: 1100-1199
    ///     Includes: Trading (Between Players, to NPCs etc.)
    /// </summary>
    Trading = 11,

    /// <summary>
    ///     Range: 1200-1299
    ///     Includes: Targeting of other Entities
    /// </summary>
    Targeting = 12,

    /// <summary>
    ///     Range: 1300-1399
    ///     Includes: NPC Interactions (Talk, Asking for Quests (this will be handled by <see cref="Quest" />) etc.)
    /// </summary>
    NpcInteraction = 13,

    /// <summary>
    ///     Range: 1400-1499
    ///     Includes: Entity Spawning, Sync, etc.
    /// </summary>
    EntitySpawning = 14,

    /// <summary>
    ///     Range: 1500-1599
    ///     Includes: Buffs / Debuffs / Auras etc.
    /// </summary>
    Buff = 15,

    /// <summary>
    ///     Range: 1600-1699
    ///     Includes: Crafting/Professions/Gathering/Recipes
    /// </summary>
    Crafting = 16,

    /// <summary>
    ///     Range: 1700-1799
    ///     Includes: Auction House
    /// </summary>
    AuctionHouse = 17,

    /// <summary>
    ///     Range: 1800-1899
    ///     Includes: Mail System
    /// </summary>
    Mail = 18,

    /// <summary>
    ///     Range: 1900-1999
    ///     Includes: Achievements and Titles
    /// </summary>
    Achievement = 19,

    /// <summary>
    ///     Range: 2000-2099
    ///     Includes: Mounts, Pets, Companies
    /// </summary>
    Mount = 20,

    /// <summary>
    ///     Range: 2100-2199
    ///     Includes: Friend System, Blocking
    /// </summary>
    Social = 21,

    /// <summary>
    ///     Range: 2200-2299
    ///     Includes: Emotes, Animations, Skins, Cosmetics, Transmog
    /// </summary>
    Emote = 22,

    /// <summary>
    ///     Range: 2300-2399
    ///     Includes: Admin, GM
    /// </summary>
    Admin = 23,

    /// <summary>
    ///     Range: 2400-2499
    ///     Includes: Instancing, Dungeons, Raids etc.
    /// </summary>
    Instancing = 24,

    /// <summary>
    ///     Range: 2500-2599
    ///     Includes: PVP, Arena, Battlegrounds
    /// </summary>
    PvP = 25,

    /// <summary>
    ///     Range: 2600-2699
    ///     Includes: World State (Weather, Time, Events etc.)
    /// </summary>
    WorldState = 26,

    /// <summary>
    ///     Range: 2700-2799
    ///     Includes: Matchmaking, Queue
    /// </summary>
    Matchmaking = 27,

    /// <summary>
    ///     Range: 2800-2899
    ///     Includes: Leaderboards, Rankings
    /// </summary>
    Leaderboard = 28,

    /// <summary>
    ///     Range: 2900-2999
    ///     Includes: Tutorial, Guide System
    /// </summary>
    Tutorial = 29,

    /// <summary>
    ///     Range: 3000-3099
    ///     Includes: Settings and Preferences Sync
    /// </summary>
    Setting = 30,

    /// <summary>
    ///     Range: 3100-3199
    ///     Includes: Loot, Rewards
    /// </summary>
    Loot = 31,

    /// <summary>
    ///     Range: 3200-3299
    ///     Includes: Cooldowns / Timers
    /// </summary>
    Cooldown = 32,

    /// <summary>
    ///     Range: 3300-3399
    ///     Includes: Inspection, Character Info,
    /// </summary>
    Inspection = 33,

    /// <summary>
    ///     Range: 3400-3499
    ///     Includes: Map, Minimap, Waypoints
    /// </summary>
    Map = 34,

    /// <summary>
    ///     Range: 3500-3599
    ///     Includes: Voicechat, Audio, Music
    /// </summary>
    VoiceChat = 35,

    /// <summary>
    ///     Range: 3600-3699
    ///     Includes: Reporting, Moderation, Feedback, Bugreport
    /// </summary>
    Reporting = 36,

    /// <summary>
    ///     Range: 3700-3799
    ///     Includes: Economy, Currency
    /// </summary>
    Economy = 37,

    /// <summary>
    ///     Range: 3800-3899
    ///     Includes: Skills, Talents, Abilities, Glyphes etc.
    /// </summary>
    Skill = 38,

    /// <summary>
    ///     Range: 3900-3999
    ///     Includes: Equipment, Gear, Swapping etc.
    /// </summary>
    Equipment = 39,

    /// <summary>
    ///     Range: 4000-4099
    ///     Includes: Bank, Storage
    /// </summary>
    Bank = 40,

    /// <summary>
    ///     Range: 4100-4199
    ///     Includes: Death, Respawn, Ghost
    /// </summary>
    Death = 41,

    /// <summary>
    ///     Range: 4200-4299
    ///     Includes: Transportation, Portals
    /// </summary>
    Transportation = 42,

    /// <summary>
    ///     Range: 4300-4399
    ///     Includes: Notification, Alerts
    /// </summary>
    Notification = 43,

    /// <summary>
    ///     Range: 4400-4499
    ///     Includes: Cutscences, Cinematics
    /// </summary>
    Cutscene = 44,

    /// <summary>
    ///     Range: 4500-4599
    ///     Includes: Housing, Player Buildings
    /// </summary>
    Housing = 45,

    /// <summary>
    ///     Range: 4600-4699
    ///     Includes: Events, Seasonal Content
    /// </summary>
    GameEvent = 46,

    //47-98 reserved for later

    /// <summary>
    ///     Range: 9900-9999
    ///     Includes: Debug, Development
    /// </summary>
    Debug = 99
}
