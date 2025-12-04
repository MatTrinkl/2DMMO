namespace Mmo.Shared.Enums;

/// <summary>
///     This enum represents the Type which message is currently send between client and server.
/// </summary>
public enum MessageType : byte
{
    // Connection 1-9
    LoginRequest = 1,
    LoginResponse = 2,
    LogoutRequest = 3,
    Heartbeat = 4,
    Disconnect = 5,

    // Zone Events 10-19
    JoinZone = 10,
    LeaveZone = 11,
    ZoneState = 12,
    PlayerJoinedZone = 13,
    PlayerLeftZone = 14,

    // Movement Events 20-29
    PositionUpdate = 20,
    PositionBroadcast = 21,

    //Combat Events (extend later) 30-49
    ActionRequest = 30,
    ActionResult = 31,
    DamageEvent = 32,
    DeathEvent = 33,

    // Chat 50-51
    ChatMessage = 50,
    ChatBroadcast = 51,
    ChatWhisper = 52,

    // Ping 100-109
    Ping = 100,
    Pong = 101
}
