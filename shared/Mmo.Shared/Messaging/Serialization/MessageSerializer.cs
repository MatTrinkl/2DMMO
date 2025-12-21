using MessagePack;
using Mmo.Shared.Chat.Messages;
using Mmo.Shared.Combat.Messages;
using Mmo.Shared.Connection.Messages;
using Mmo.Shared.Messaging.Enums;
using Mmo.Shared.Messaging.Exceptions;
using Mmo.Shared.Messaging.Helper;
using Mmo.Shared.Messaging.Interfaces;
using Mmo.Shared.Movement;
using Mmo.Shared.System.Messages;
using Mmo.Shared.Zones.Messages;
using Messages_Ping = Mmo.Shared.System.Messages.Ping;

namespace Mmo.Shared.Messaging.Serialization;

/// <summary>
///     This class Serializes and deserializes a message based on its <see cref="MessageType" />.
/// </summary>
public static class MessageSerializer
{
    /// <summary>
    ///     Serializes a message to bytes with type prefix
    ///     {MessageTye} needs to be Key(0).
    /// </summary>
    public static byte[] Serialize<T>(T message) where T : INetworkMessage => MessagePackSerializer.Serialize(message);

    /// <summary>
    ///     Deserializes a message based on its type
    /// </summary>
    public static INetworkMessage Deserialize(ReadOnlyMemory<byte> data)
    {
        MessageHeader header = MessagePackSerializer.Deserialize<MessageHeader>(data);

        return header.Type switch
        {
            MessageType.LoginRequest => MessagePackSerializer.Deserialize<LoginRequest>(data),
            MessageType.LoginResponse => MessagePackSerializer.Deserialize<LoginResponse>(data),
            MessageType.LogoutRequest => MessagePackSerializer.Deserialize<LogoutRequest>(data),
            MessageType.Heartbeat => MessagePackSerializer.Deserialize<Heartbeat>(data),
            MessageType.Disconnect => MessagePackSerializer.Deserialize<Disconnect>(data),

            MessageType.JoinZone => MessagePackSerializer.Deserialize<JoinZone>(data),
            MessageType.LeaveZone => MessagePackSerializer.Deserialize<LeaveZone>(data),
            MessageType.ZoneState => MessagePackSerializer.Deserialize<ZoneState>(data),
            MessageType.PlayerJoinedZone => MessagePackSerializer.Deserialize<PlayerJoinedZone>(data),
            MessageType.PlayerLeftZone => MessagePackSerializer.Deserialize<PlayerLeftZone>(data),

            MessageType.PositionUpdate => MessagePackSerializer.Deserialize<PositionUpdate>(data),
            MessageType.PositionBroadcast => MessagePackSerializer.Deserialize<PositionBroadcast>(data),

            MessageType.ActionRequest => MessagePackSerializer.Deserialize<ActionRequest>(data),
            MessageType.ActionResult => MessagePackSerializer.Deserialize<ActionResult>(data),
            MessageType.DamageEvent => MessagePackSerializer.Deserialize<DamageEvent>(data),
            MessageType.DeathEvent => MessagePackSerializer.Deserialize<DeathEvent>(data),

            MessageType.ChatMessage => MessagePackSerializer.Deserialize<ChatMessage>(data),
            MessageType.ChatBroadcast => MessagePackSerializer.Deserialize<ChatBroadcast>(data),
            MessageType.ChatWhisper => MessagePackSerializer.Deserialize<ChatWhisper>(data),

            MessageType.Ping => MessagePackSerializer.Deserialize<Messages_Ping>(data),
            MessageType.Pong => MessagePackSerializer.Deserialize<Pong>(data),
            _ => throw new UnknownMessageTypeException(header.Type)
        };
    }
}
