using System.Buffers.Binary;
using MessagePack;
using Mmo.Shared.Enums;
using Mmo.Shared.Messages;

namespace Mmo.Shared.Serialization;

public static class MessageSerializer
{
    /// <summary>
    /// Serializes a message to bytes with type prefix
    /// Format: [1 byte Type][4 bytes Length][N bytes Payload]
    /// </summary>
    public static byte[] Serialize<T>(T message) where T : INetworkMessage
    {
        var payload = MessagePackSerializer.Serialize(message);
        var result = new byte[1 + 4 + payload.Length];

        result[0] = (byte)message.Type;
        BinaryPrimitives.WriteInt32LittleEndian(result.AsSpan(1, 4), payload.Length);
        payload.CopyTo(result, 5);

        return result;
    }

    /// <summary>
    /// Deserializes a message from bytes (without type prefix)
    /// </summary>
    public static T Deserialize<T>(byte[] data) where T : INetworkMessage
    {
        return MessagePackSerializer.Deserialize<T>(data);
    }

    /// <summary>
    /// Deserializes a message based on its type
    /// </summary>
    public static INetworkMessage Deserialize(MessageType type, byte[] payload)
    {
        return type switch
        {
            MessageType.LoginRequest => MessagePackSerializer.Deserialize<LoginRequest>(payload),
            MessageType.LoginResponse => MessagePackSerializer.Deserialize<LoginResponse>(payload),
            MessageType.PlayerJoined => MessagePackSerializer.Deserialize<PlayerJoined>(payload),
            MessageType.PlayerLeft => MessagePackSerializer.Deserialize<PlayerLeft>(payload),
            MessageType.PositionUpdate => MessagePackSerializer.Deserialize<PositionUpdate>(payload),
            MessageType.WorldState => MessagePackSerializer.Deserialize<WorldState>(payload),
            MessageType.ChatMessage => MessagePackSerializer.Deserialize<ChatMessage>(payload),
            _ => throw new ArgumentException($"Unknown message type: {type}")
        };
    }
}
