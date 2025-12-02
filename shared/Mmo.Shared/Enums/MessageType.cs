namespace Mmo.Shared.Enums;

public enum MessageType : byte
{
    // Connection
    LoginRequest = 1,
    LoginResponse = 2,
    Disconnect = 3,

    // Player Events
    PlayerJoined = 10,
    PlayerLeft = 11,

    // Movement
    PositionUpdate = 20,
    WorldState = 21,

    // Chat
    ChatMessage = 30,

    // System
    Ping = 100,
    Pong = 101
}
