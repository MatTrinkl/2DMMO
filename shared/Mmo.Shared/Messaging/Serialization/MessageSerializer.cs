using System.Reflection;
using MessagePack;
using Mmo.Shared.Messaging.Attributes;
using Mmo.Shared.Messaging.Enums;
using Mmo.Shared.Messaging.Exceptions;
using Mmo.Shared.Messaging.Helper;
using Mmo.Shared.Messaging.Interfaces;

namespace Mmo.Shared.Messaging.Serialization;

/// <summary>
///     This class Serializes and deserializes a message based on its <see cref="MessageType" />.
///     Uses an attribute-based registry system for automatic message type registration.
/// </summary>
public static class MessageSerializer
{
    /// <summary>
    ///     Registry mapping MessageType to deserialization function.
    ///     O(1) lookup for message deserialization with pre-compiled delegates.
    /// </summary>
    private static readonly Dictionary<MessageType, Func<ReadOnlyMemory<byte>, INetworkMessage>> MessageRegistry = new();

    /// <summary>
    ///     Static constructor that automatically registers all message types with the [NetworkMessage] attribute.
    /// </summary>
    static MessageSerializer()
    {
        // Find all types in the assembly that have the NetworkMessageAttribute
        var assembly = typeof(INetworkMessage).Assembly;
        var messageTypes = assembly.GetTypes()
            .Where(t => t.GetCustomAttribute<NetworkMessageAttribute>() != null)
            .ToList();

        // Find the MessagePackSerializer.Deserialize<T> method once
        var deserializeMethod = typeof(MessagePackSerializer)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(m => m.Name == nameof(MessagePackSerializer.Deserialize))
            .Where(m => m.IsGenericMethodDefinition)
            .Where(m => m.GetParameters().Length == 3)
            .Where(m => m.GetParameters()[0].ParameterType == typeof(ReadOnlyMemory<byte>))
            .FirstOrDefault();

        if (deserializeMethod == null)
        {
            throw new InvalidOperationException(
                "Could not find MessagePackSerializer.Deserialize<T>(ReadOnlyMemory<byte>, MessagePackSerializerOptions, CancellationToken) method.");
        }

        // Register each message type with a pre-compiled delegate
        foreach (var messageType in messageTypes)
        {
            var attribute = messageType.GetCustomAttribute<NetworkMessageAttribute>()!;
            
            // Create the generic method for this specific message type
            var genericMethod = deserializeMethod.MakeGenericMethod(messageType);
            
            // Create a cached delegate that calls MessagePackSerializer.Deserialize<T>(data, null, default)
            Func<ReadOnlyMemory<byte>, INetworkMessage> deserializer = data =>
                (INetworkMessage)genericMethod.Invoke(null, new object?[] { data, null, default(CancellationToken) })!;

            MessageRegistry[attribute.Type] = deserializer;
        }
    }

    /// <summary>
    ///     Serializes a message to bytes with type prefix
    ///     {MessageTye} needs to be Key(0).
    /// </summary>
    public static byte[] Serialize<T>(T message) where T : INetworkMessage => MessagePackSerializer.Serialize(message);

    /// <summary>
    ///     Deserializes a message based on its type using the registered message types.
    /// </summary>
    public static INetworkMessage Deserialize(ReadOnlyMemory<byte> data)
    {
        MessageHeader header = MessagePackSerializer.Deserialize<MessageHeader>(data);

        if (!MessageRegistry.TryGetValue(header.Type, out var deserializer))
        {
            throw new UnknownMessageTypeException(header.Type);
        }

        return deserializer(data);
    }
}
