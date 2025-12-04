using System.Net.NetworkInformation;
using MessagePack;
using Mmo.Shared.Enums;
using Mmo.Shared.Exceptions;
using Mmo.Shared.Helper;
using Mmo.Shared.Messages;
using Mmo.Shared.Messages.Chat;
using Mmo.Shared.Messages.Combat;
using Mmo.Shared.Messages.Connection;
using Mmo.Shared.Messages.Interfaces;
using Mmo.Shared.Messages.Movement;
using Mmo.Shared.Messages.ZoneEvents;
using Ping = Mmo.Shared.Messages.Ping;

namespace Mmo.Shared.Serialization;

/// <summary>
/// This class Serializes and deserializes a massage based on its <see cref="MessageType"/>.
/// </summary>
public static class MessageSerializer
{
    /// <summary>
    /// Serializes a message to bytes with type prefix
    /// {MessageTye} needs to be Key(0).
    /// </summary>
    public static byte[] Serialize<T>(T message) where T : INetworkMessage
    {
        return MessagePackSerializer.Serialize(message);
    }

    /// <summary>
    /// Deserializes a message based on its type
    /// </summary>
    public static INetworkMessage Deserialize(ReadOnlyMemory<byte> data)
    {
        // 1. Erst nur den Type lesen (schnell)
        var header = MessagePackSerializer.Deserialize<MessageHeader>(data);

        // 2.  Dann vollständig deserialisieren
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

            MessageType.Ping => MessagePackSerializer.Deserialize<Ping.Ping>(data),
            MessageType.Pong => MessagePackSerializer.Deserialize<Ping.Pong>(data),
            _ => throw new UnknownMessageTypeException(header.Type)
        };
    }
}
